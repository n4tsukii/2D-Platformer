using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Platformer;

public static class ExpeditionLevelBuilder
{
    private static Sprite square;
    private static PhysicsMaterial2D movementMaterial;
    private static Color stone, trim, glow, backdrop;
    private static Transform root;
    private static System.Random random;

    [MenuItem("Tools/Platformer/Build Three Expeditions")]
    public static void Build()
    {
        if (EditorApplication.isPlaying) throw new System.InvalidOperationException("Stop play mode before building.");
        EnsureSprite();
        for (int n = 0; n < 3; n++) BuildLevel(n);
        AddMenu();
        var scenes = EditorBuildSettings.scenes.ToList();
        foreach (string name in new[] { "Level 3", "Level 4", "Level 5" })
        {
            string path = "Assets/Scenes/" + name + ".unity";
            scenes.RemoveAll(s => s.path == path);
            scenes.Add(new EditorBuildSettingsScene(path, true));
        }
        EditorBuildSettings.scenes = scenes.ToArray();
        // The existing second scene has no exit. Add a portal without changing its layout.
        var bridge = EditorSceneManager.OpenScene("Assets/Scenes/Level 2.unity");
        var oldEntrance = GameObject.Find("Expedition entrance — Ruined Prison");
        if (oldEntrance) Object.DestroyImmediate(oldEntrance);
        var end = new GameObject("Expedition entrance — Ruined Prison");
        end.transform.position = new Vector3(23, -1.7f, 0);
        end.AddComponent<BoxCollider2D>().isTrigger = true;
        end.GetComponent<BoxCollider2D>().size = new Vector2(1, 2.2f);
        Set(end.AddComponent<Platformer.LevelExit>(), "nextScene", "Level 3");
        root = end.transform;
        Rect("Portal", 23, -1.8f, 0.9f, 2, new Color(0.15f, 0.8f, 0.85f), "Foreground", 4);
        Label("EXPEDITIONS >", 22.5f, 0, 0.11f, Color.white);
        EditorSceneManager.SaveScene(bridge);
        AssetDatabase.SaveAssets();
        EditorSceneManager.OpenScene("Assets/Scenes/Level 3.unity");
        SceneView.lastActiveSceneView?.LookAt(new Vector3(0, 0, 0), Quaternion.identity, 10, true);
        Debug.Log("Built Level 3, Level 4, Level 5; registered build scenes and menu shortcuts.");
    }

    private static void EnsureSprite()
    {
        const string path = "Assets/Platformer/Sprites/ExpeditionBlock.png";
        if (!System.IO.File.Exists(path))
        {
            var t = new Texture2D(2, 2);
            t.SetPixels(new[] { Color.white, Color.white, Color.white, Color.white });
            t.Apply();
            System.IO.File.WriteAllBytes(path, t.EncodeToPNG());
            Object.DestroyImmediate(t);
            AssetDatabase.ImportAsset(path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 2;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }
        square = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        const string materialPath = "Assets/Platformer/ExpeditionMovement.physicsMaterial2D";
        movementMaterial = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(materialPath);
        if (!movementMaterial)
        {
            movementMaterial = new PhysicsMaterial2D("Expedition movement") { friction = 0, bounciness = 0 };
            AssetDatabase.CreateAsset(movementMaterial, materialPath);
        }
    }

    private static void BuildLevel(int n)
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/Level 1.unity");
        foreach (var go in scene.GetRootGameObjects())
            if (go.name != "Core" && go.name != "Player" && go.name != "FireballHolder") Object.DestroyImmediate(go);
        var player = Object.FindFirstObjectByType<Platformer.PlayerMovement>().gameObject;
        Object.DestroyImmediate(player.GetComponent<Platformer.PlayerRespawn>());
        player.AddComponent<ExpeditionPlayer>();
        player.GetComponent<BoxCollider2D>().sharedMaterial = movementMaterial;
        player.transform.position = new Vector3(-5.5f, -1.75f, 0);
        var camera = Camera.main;
        camera.GetComponent<Platformer.CameraController>().enabled = false;
        var follow = camera.gameObject.AddComponent<ExpeditionCamera>();
        follow.target = player.transform; follow.left = -8; follow.right = 72;
        camera.orthographicSize = 5;
        camera.transform.position = new Vector3(0.9f, 0, -10);
        root = new GameObject("Expedition — authored geometry").transform;
        random = new System.Random(440 + n);
        string[] titles = { "RUINED PRISON", "TOXIC AQUEDUCTS", "EMBER FORTRESS" };
        string[] subtitles = { "Broken cells. Open routes. Find the way out.", "Stay above the runoff. The upper route holds supplies.", "Cross the furnace bridges. Reach the last gate." };
        stone = n == 0 ? Hex("35445D") : n == 1 ? Hex("344B47") : Hex("51404F");
        trim = n == 0 ? Hex("71899D") : n == 1 ? Hex("7E9871") : Hex("AA8270");
        glow = n == 0 ? Hex("62DEEC") : n == 1 ? Hex("B4E967") : Hex("FFAA65");
        backdrop = n == 0 ? Hex("111B30") : n == 1 ? Hex("101F24") : Hex("211A2B");
        camera.backgroundColor = backdrop;
        var stage = new GameObject("Stage identity").AddComponent<ExpeditionStage>();
        stage.title = titles[n]; stage.subtitle = subtitles[n]; stage.accent = glow; stage.player = player.transform;
        Background(n);
        if (n == 0)
        {
            Ground(-8, 14); Ground(18, 32); Ground(36, 50); Ground(54, 72);
            Platform(-1, -1.2f, 3); Platform(4, 0.5f, 3.5f); Platform(9, 2, 3);
            Platform(16, -1.5f, 2); Platform(23, -0.5f, 4); Platform(28, 1.2f, 3);
            Platform(34, -1.3f, 2); Platform(40, -0.4f, 4); Platform(45, 1.3f, 3);
            Platform(52, -1.5f, 2); Platform(59, -0.7f, 4); Platform(64, 1, 3);
            Hazard(14, 18, n); Hazard(32, 36, n); Hazard(50, 54, n);
            Enemy(7, -3, 2.8f); Enemy(26, -3, 2.6f); Enemy(43, -3, 2.4f); Enemy(61, -3, 2.2f);
            Reward(9, 2); Reward(28, 1.2f); Reward(45, 1.3f); Reward(64, 1);
        }
        else if (n == 1)
        {
            Ground(-8, 3); Ground(10, 20); Ground(27, 38); Ground(45, 56); Ground(63, 72);
            foreach (float x in new[] { 6.5f, 23.5f, 41.5f, 59.5f }) Platform(x, -1.5f, 2.5f);
            Platform(-1, -1, 3); Platform(4, 1, 3); Platform(10, 2.3f, 3);
            Platform(16, -0.7f, 3); Platform(21, 1.3f, 3); Platform(28, 2.2f, 3);
            Platform(34, -0.5f, 3); Platform(39, 1.3f, 3); Platform(46, 2.2f, 3);
            Platform(52, -0.5f, 3); Platform(57, 1.3f, 3); Platform(64, 2.2f, 3);
            Hazard(3, 10, n); Hazard(20, 27, n); Hazard(38, 45, n); Hazard(56, 63, n);
            Enemy(14, -3, 2.6f); Enemy(30, -3, 2.4f); Enemy(49, -3, 2.3f); Enemy(66, -3, 2.1f);
            Reward(10, 2.3f); Reward(28, 2.2f); Reward(46, 2.2f); Reward(64, 2.2f);
        }
        else
        {
            Ground(-8, 4); Ground(10, 20); Ground(26, 36); Ground(42, 52); Ground(58, 72);
            foreach (float x in new[] { 7f, 23f, 39f, 55f }) Platform(x, -1.3f, 2);
            Platform(-1, -0.9f, 3); Platform(4, 1.1f, 3); Platform(11, 2.2f, 3);
            Platform(16, -0.6f, 3); Platform(21, 1.2f, 3); Platform(28, 2.2f, 3);
            Platform(32, -0.5f, 3); Platform(37, 1.3f, 3); Platform(44, 2.2f, 3);
            Platform(48, -0.6f, 3); Platform(53, 1.2f, 3); Platform(60, 2.2f, 3);
            Hazard(4, 10, n); Hazard(20, 26, n); Hazard(36, 42, n); Hazard(52, 58, n);
            Enemy(1, -3, 2.5f); Enemy(15, -3, 2.3f); Enemy(30, -3, 2.2f); Enemy(46, -3, 2.1f); Enemy(64, -3, 2);
            Reward(11, 2.2f); Reward(28, 2.2f); Reward(44, 2.2f); Reward(60, 2.2f);
        }
        Solid("West boundary", -8.4f, 0, 0.8f, 12);
        Solid("East boundary", 72.4f, 0, 0.8f, 12);
        // Closed upper boundary keeps double-jumps inside the authored space.
        Solid("Vault ceiling", 32, 5.8f, 80, 1);
        foreach (float x in new[] { -5.5f, 12f, 29.5f, 47.5f, 65.5f }) Checkpoint(x);
        Label(titles[n], -1, 3.9f, 0.18f, glow);
        Label("A / D  MOVE    SPACE  JUMP x2    LMB  FIRE", -0.5f, -4.3f, 0.1f, trim);
        for (int r = 1; r < 5; r++) Label("0" + (r + 1) + "   /   " + new[] { "CELLBLOCK", "CONDUIT", "BASTION" }[n], r * 16, 4, 0.12f, trim);
        var portal = Rect("Exit gate", 70, -1.65f, 1.3f, 2.7f, glow, "Foreground", 5);
        Rect("Gate shadow", 70, -1.65f, 0.8f, 2.35f, backdrop, "Foreground", 6);
        portal.AddComponent<BoxCollider2D>().isTrigger = true;
        if (n < 2) Set(portal.AddComponent<Platformer.LevelExit>(), "nextScene", "Level " + (n + 4));
        else portal.AddComponent<ExpeditionFinish>().stage = stage;
        Label(n == 2 ? "FREEDOM >" : "NEXT BIOME >", 68, 0.4f, 0.12f, glow);
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Level " + (n + 3) + ".unity");
    }

    private static void Background(int n)
    {
        Rect("Distant darkness", 32, 0, 82, 12, backdrop, "Background", -20);
        for (int col = 0; col < 54; col++)
        for (int row = 0; row < 8; row++)
        {
            float x = -8 + col * 1.55f + (row % 2) * 0.77f;
            var c = Color.Lerp(backdrop, stone, 0.13f + (float)random.NextDouble() * 0.16f);
            Rect("Masonry", x, -4.7f + row * 1.3f, 1.48f, 1.23f, c, "Background", -15);
        }
        for (int i = 0; i < 10; i++)
        {
            float x = -4 + i * 8;
            Rect("Recess", x, 0.8f, 3.8f, 5.7f, Color.Lerp(backdrop, Color.black, 0.3f), "Background", -10);
            Rect("Recess lintel", x, 3.5f, 4.2f, 0.35f, stone * 0.7f, "Background", -9);
            if (n == 0)
                for (int b = 0; b < 5; b++) Rect("Prison bars", x - 1.5f + b * 0.75f, 0.8f, 0.09f, 5.2f, stone * 0.75f, "Background", -8);
            else if (n == 1)
            {
                Rect("Drain pipe", x + 1, 2.8f, 0.45f, 3.7f, stone, "Background", -8);
                for (int b = 0; b < 4; b++) Rect("Pipe collar", x + 1, 1.5f + b, 0.62f, 0.15f, trim * 0.6f, "Background", -7);
            }
            else
            {
                Rect("Hanging banner", x, 2.2f, 1.15f, 3, Hex("642E48"), "Background", -8);
                Rect("Banner sigil", x, 2.4f, 0.2f, 1.3f, glow * 0.65f, "Background", -7);
            }
            Torch(x + 2.6f, 1.8f);
            for (int c = 0; c < 8; c++) Rect("Suspension chain", x - 2, 4.6f - c * 0.28f, c % 2 == 0 ? 0.12f : 0.06f, 0.22f, trim * 0.55f, "Background", -5);
        }
    }

    private static void Torch(float x, float y)
    {
        Rect("Torch halo", x, y + 0.25f, 0.9f, 1.2f, new Color(glow.r, glow.g, glow.b, 0.07f), "Background", -4);
        Rect("Sconce", x, y - 0.25f, 0.18f, 0.65f, trim, "Background", -3);
        Rect("Flame", x, y + 0.2f, 0.28f, 0.55f, glow, "Background", -2);
        Rect("Flame core", x, y + 0.12f, 0.12f, 0.3f, Color.white, "Background", -1);
    }

    private static void Ground(float left, float right)
    {
        var g = Solid("Foundation", (left + right) / 2, -4.1f, right - left, 2.2f);
        for (float x = left + 0.55f; x < right; x += 1.2f)
            Rect("Foundation face", x, -3.65f, 1.12f, 0.65f, Color.Lerp(stone, backdrop, 0.2f), "Foreground", 2);
        Rect("Walkable edge", (left + right) / 2, -3.04f, right - left, 0.13f, trim, "Foreground", 4);
    }

    private static void Platform(float x, float top, float width)
    {
        Solid("Route platform", x, top - 0.23f, width, 0.46f);
        Rect("Platform cap", x, top - 0.045f, width, 0.09f, trim, "Foreground", 4);
        Rect("Platform support", x, top - 0.58f, 0.28f, 0.35f, stone, "Foreground", 1);
        for (float p = x - width / 2 + 0.3f; p < x + width / 2; p += 0.7f)
            Rect("Platform stone joint", p, top - 0.25f, 0.035f, 0.32f, backdrop, "Foreground", 3);
    }

    private static GameObject Solid(string name, float x, float y, float width, float height)
    {
        var g = Rect(name, x, y, width, height, stone, "Foreground", 1);
        g.layer = 6;
        g.AddComponent<BoxCollider2D>();
        return g;
    }

    private static void Hazard(float left, float right, int n)
    {
        Color c = n == 0 ? Hex("586D9F") : n == 1 ? Hex("80BD39") : Hex("F46B3D");
        var g = Rect(n == 0 ? "Shard pit" : n == 1 ? "Toxic runoff" : "Furnace trench", (left + right) / 2, -4.15f, right - left, 1.6f, c * 0.6f, "Foreground", 3);
        g.AddComponent<BoxCollider2D>().isTrigger = true;
        g.AddComponent<ExpeditionHazard>();
        Rect("Danger surface", (left + right) / 2, -3.4f, right - left, 0.12f, c, "Foreground", 4);
        for (float x = left + 0.2f; x < right; x += 0.43f)
            Rect("Danger glints", x, -3.65f + (float)random.NextDouble() * 0.15f, 0.18f, 0.1f, c, "Foreground", 4);
    }

    private static void Checkpoint(float x)
    {
        var go = new GameObject("Checkpoint beacon"); go.transform.SetParent(root); go.transform.position = new Vector3(x, -2, 0);
        go.AddComponent<BoxCollider2D>().size = new Vector2(1, 2.5f); go.GetComponent<BoxCollider2D>().isTrigger = true;
        var cp = go.AddComponent<ExpeditionCheckpoint>();
        var spawn = new GameObject("Safe spawn").transform; spawn.SetParent(go.transform); spawn.position = new Vector3(x, -1.75f, 0); cp.spawnPoint = spawn;
        Rect("Beacon plinth", x, -2.85f, 0.7f, 0.3f, trim, "Foreground", 5);
        cp.beacon = Rect("Beacon crystal", x, -2.25f, 0.27f, 0.8f, glow * 0.65f, "Foreground", 6).GetComponent<SpriteRenderer>();
        Label("REST", x, -1.5f, 0.075f, glow);
    }

    private static void Enemy(float x, float top, float cooldown)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Platformer/Prefabs/Enemy/RangedEnemyHolder.prefab");
        var g = (GameObject)PrefabUtility.InstantiatePrefab(prefab); g.transform.SetParent(root);
        var enemy = g.GetComponentInChildren<Platformer.RangedEnemy>();
        var box = enemy.GetComponent<BoxCollider2D>();
        Vector3 feet = box.transform.TransformPoint(box.offset - Vector2.up * box.size.y / 2);
        g.transform.position += new Vector3(x - feet.x, top - feet.y + 0.02f, 0);
        Set(enemy, "attackCooldown", cooldown); Set(enemy, "range", 3.5f);
        g.name = "Warden — ranged encounter";
    }

    private static void Reward(float x, float top)
    {
        var g = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Platformer/Prefabs/Collectibles/HealthCollectible.prefab"));
        g.transform.SetParent(root); g.transform.position = new Vector3(x, top + 0.6f, 0); g.transform.localScale = Vector3.one * 0.65f;
        Label("SUPPLY CACHE", x, top + 1.2f, 0.07f, glow);
    }

    private static GameObject Rect(string name, float x, float y, float w, float h, Color color, string layer, int order)
    {
        var g = new GameObject(name); g.transform.SetParent(root); g.transform.position = new Vector3(x, y, 0); g.transform.localScale = new Vector3(w, h, 1);
        var r = g.AddComponent<SpriteRenderer>(); r.sprite = square; r.color = color; r.sortingLayerName = layer; r.sortingOrder = order;
        return g;
    }

    private static void Label(string value, float x, float y, float scale, Color color)
    {
        var g = new GameObject(value); g.transform.SetParent(root); g.transform.position = new Vector3(x, y, 0); g.transform.localScale = Vector3.one * scale;
        var t = g.AddComponent<TextMesh>(); t.text = value; t.anchor = TextAnchor.MiddleCenter; t.fontSize = 48; t.characterSize = 0.7f; t.color = color;
        g.GetComponent<MeshRenderer>().sortingLayerName = "Player"; g.GetComponent<MeshRenderer>().sortingOrder = 50;
    }

    private static Color Hex(string hex) { ColorUtility.TryParseHtmlString("#" + hex, out var c); return c; }
    private static void Set(Object obj, string property, string value) { var s = new SerializedObject(obj); s.FindProperty(property).stringValue = value; s.ApplyModifiedPropertiesWithoutUndo(); }
    private static void Set(Object obj, string property, float value) { var s = new SerializedObject(obj); s.FindProperty(property).floatValue = value; s.ApplyModifiedPropertiesWithoutUndo(); }

    private static void AddMenu()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/Main Menu.unity");
        var canvas = Object.FindFirstObjectByType<Canvas>();
        var old = canvas.transform.Find("Expedition selection"); if (old) Object.DestroyImmediate(old.gameObject);
        var panel = new GameObject("Expedition selection", typeof(RectTransform), typeof(Image)); panel.transform.SetParent(canvas.transform, false);
        var rt = panel.GetComponent<RectTransform>(); rt.anchorMin = new Vector2(0.70f, 0.18f); rt.anchorMax = new Vector2(0.98f, 0.7f); rt.offsetMin = rt.offsetMax = Vector2.zero;
        panel.GetComponent<Image>().color = new Color(0.035f, 0.06f, 0.1f, 0.95f);
        string[] names = { "3   RUINED PRISON", "4   TOXIC AQUEDUCTS", "5   EMBER FORTRESS" };
        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        for (int i = -1; i < 3; i++)
        {
            var go = new GameObject(i < 0 ? "Expeditions heading" : names[i], typeof(RectTransform), typeof(Text)); go.transform.SetParent(panel.transform, false);
            var r = go.GetComponent<RectTransform>(); r.anchorMin = new Vector2(0.05f, 0.75f - (i + 1) * 0.22f); r.anchorMax = new Vector2(0.95f, 0.95f - (i + 1) * 0.22f); r.offsetMin = r.offsetMax = Vector2.zero;
            var text = go.GetComponent<Text>(); text.font = font; text.text = i < 0 ? "NEW EXPEDITIONS" : names[i]; text.fontSize = i < 0 ? 28 : 23; text.resizeTextForBestFit = true; text.resizeTextMinSize = 12; text.resizeTextMaxSize = text.fontSize; text.alignment = TextAnchor.MiddleCenter; text.color = i < 0 ? Hex("62DEEC") : Color.white;
            if (i >= 0)
            {
                var launch = go.AddComponent<ExpeditionLaunch>(); launch.sceneName = "Level " + (i + 3);
                var button = go.AddComponent<Button>(); button.targetGraphic = text;
                UnityEventTools.AddPersistentListener(button.onClick, launch.Launch);
                var colors = button.colors; colors.highlightedColor = Hex("62DEEC"); button.colors = colors;
            }
        }
        EditorSceneManager.SaveScene(scene);
    }
}
