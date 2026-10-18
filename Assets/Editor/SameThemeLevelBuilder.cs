using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

/// <summary>Expands the original rooms with their existing art and gameplay prefabs.</summary>
public static class SameThemeLevelBuilder
{
    private const string SceneFolder = "Assets/Scenes/";
    private const string PrefabFolder = "Assets/Platformer/Prefabs/";
    private static Sprite ledgeSprite;

    [MenuItem("Tools/Platformer/Expand Original Three Levels")]
    public static void Build()
    {
        if (EditorApplication.isPlaying)
            throw new InvalidOperationException("Stop play mode before authoring levels.");
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(SceneFolder + "Level 3.unity"))
            throw new InvalidOperationException("The campaign is already authored. Edit the saved scenes directly; this builder is intended for the original two-level baseline.");
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        ledgeSprite = AssetDatabase.LoadAllAssetsAtPath(
            "Assets/Platformer/Sprites/Pixel Adventure 1/Assets/Terrain/Terrain (16x16) 1.png")
            .OfType<Sprite>().Single(s => s.name.EndsWith("_3"));

        // Build the new level first, while Level 2 is still an untouched room template.
        BuildLevel(3, 5, "Level 2");
        BuildLevel(1, 4, "Level 1");
        BuildLevel(2, 4, "Level 2");
        EditorBuildSettings.scenes = new[] { "Main Menu", "Level 1", "Level 2", "Level 3" }
            .Select(n => new EditorBuildSettingsScene(SceneFolder + n + ".unity", true)).ToArray();
        AssetDatabase.SaveAssets();
        EditorSceneManager.OpenScene(SceneFolder + "Level 1.unity");
        SceneView.lastActiveSceneView?.LookAt(Vector3.zero, Quaternion.identity, 10, true);
        Debug.Log("Three-level campaign saved: 4 / 4 / 5 rooms. Original UI retained.");
    }

    private static void BuildLevel(int number, int count, string source)
    {
        var scene = EditorSceneManager.OpenScene(SceneFolder + source + ".unity");
        if (number == 3)
            EditorSceneManager.SaveScene(scene, SceneFolder + "Level 3.unity");
        var level = GameObject.Find("Level").transform;
        var rooms = level.Cast<Transform>().Where(t => t.GetComponent<Platformer.Room>())
            .OrderBy(t => t.position.x).ToList();
        // Remove only the old unfinished expedition portal, whose target did not exist.
        foreach (var g in scene.GetRootGameObjects())
            if (g.name.StartsWith("Expedition entrance")) Object.DestroyImmediate(g);
        foreach (var exit in Object.FindObjectsByType<Platformer.LevelExit>(FindObjectsSortMode.None))
            Object.DestroyImmediate(exit);

        int originalCount = rooms.Count;
        while (rooms.Count < count)
        {
            var room = Object.Instantiate(rooms[0].gameObject, level).transform;
            room.name = "Room" + (rooms.Count + 1);
            room.position = new Vector3(rooms.Last().position.x + 16.64f, 0, 0);
            foreach (Transform child in room.Cast<Transform>().ToArray())
                if (!new[] { "Ground", "WallRight", "Ceiling", "Start (Idle)", "Background", "Door" }.Contains(child.name))
                    Object.DestroyImmediate(child.gameObject);
            rooms.Add(room);
        }

        for (int i = 0; i < rooms.Count; i++)
        {
            Transform room = rooms[i];
            room.name = "Room" + (i + 1);
            var door = room.Find("Door").GetComponent<Platformer.Door>();
            SetReference(door, "previousRoom", room);
            SetReference(door, "nextRoom", i + 1 < rooms.Count ? rooms[i + 1] : null);
            SetReference(door, "cam", Camera.main.GetComponent<Platformer.CameraController>());
            if (i == rooms.Count - 1)
            {
                var exit = door.gameObject.AddComponent<Platformer.LevelExit>();
                SetString(exit, "nextScene", number < 3 ? "Level " + (number + 1) : "Main Menu");
            }
            // Keep authored rooms intact; add distinct optional upper routes.
            AddRoute(room, number, i);
            if (!room.GetComponentInChildren<Animator>(true) ||
                !room.GetComponentsInChildren<Collider2D>(true).Any(c => c.CompareTag("Checkpoint")))
                Spawn("Props/Checkpoint.prefab", room, -5.7f, -1.25f, "Checkpoint");

            if (number == 3 || i >= originalCount || (number == 2 && i == 1))
                AddEncounter(room, number, i);

            var enemies = room.GetComponentsInChildren<Platformer.RangedEnemy>(true)
                .Select(e => e.transform.parent.gameObject).Distinct().ToArray();
            var serializedRoom = new SerializedObject(room.GetComponent<Platformer.Room>());
            var list = serializedRoom.FindProperty("enemies");
            list.arraySize = enemies.Length;
            for (int e = 0; e < enemies.Length; e++) list.GetArrayElementAtIndex(e).objectReferenceValue = enemies[e];
            serializedRoom.ApplyModifiedPropertiesWithoutUndo();
        }

        if (number == 3)
        {
            GameObject.Find("Player").transform.position = new Vector3(-6.75f, -1.62f, 0);
            // Reposition the inherited loose pickup into the new level's opening alcove.
            var loose = scene.GetRootGameObjects().FirstOrDefault(g => g.name == "HealthCollectible");
            if (loose) loose.transform.position = new Vector3(4.8f, 2.4f, 0);
        }
        FixScenePixelImports(scene);
        var camera = Camera.main;
        camera.allowHDR = camera.allowMSAA = camera.allowDynamicResolution = false;
        // Preserve the established 10-unit camera framing and mixed sprite scales.
        camera.GetComponent<Platformer.CameraController>().enabled = true;
        GameObject.Find("Player").GetComponent<Rigidbody2D>().interpolation = RigidbodyInterpolation2D.Interpolate;
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, SceneFolder + "Level " + number + ".unity");
    }

    private static void AddRoute(Transform room, int stage, int index)
    {
        var old = room.Find("Exploration route");
        if (old) Object.DestroyImmediate(old.gameObject);
        var route = new GameObject("Exploration route").transform;
        route.SetParent(room, false);
        // 1.6-unit rises are comfortably within the existing 20-unit jump impulse.
        // Routes work in both directions; the original ground corridor stays walkable.
        float direction = (stage + index) % 2 == 0 ? -1 : 1;
        Ledge(route, -4.8f * direction, -1.25f);
        Ledge(route, -1.6f * direction, 0.35f);
        Ledge(route, 1.6f * direction, 1.8f);
        Ledge(route, 4.8f * direction, 1.8f);
        Spawn("Collectibles/HealthCollectible.prefab", room, 4.8f * direction, 2.4f, "Upper route reward");
        if (stage > 1)
        {
            Ledge(route, 4.8f * direction, -1.25f);
            Spawn("Collectibles/HealthCollectible.prefab", room, 5.8f, -2.2f, "Lower alcove reward");
        }
    }

    private static void Ledge(Transform route, float x, float top)
    {
        var go = new GameObject("Terrain ledge");
        go.transform.SetParent(route, false);
        go.transform.localPosition = new Vector3(x, top - 0.125f, 0);
        go.transform.localScale = Vector3.one * 5;
        go.layer = 6;
        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = ledgeSprite;
        renderer.sortingLayerName = "Foreground";
        renderer.sortingOrder = 2;
        var box = go.AddComponent<BoxCollider2D>();
        box.size = new Vector2(0.48f, 0.05f);
        box.usedByEffector = true;
        var effector = go.AddComponent<PlatformEffector2D>();
        effector.useOneWay = true;
        effector.useSideFriction = false;
        effector.useSideBounce = false;
    }

    private static void AddEncounter(Transform room, int stage, int index)
    {
        if (stage == 1)
        {
            Spawn("Traps/Firetrap.prefab", room, 0, -2.33f, "Fire crossing");
            Spawn("Enemy/RangedEnemyHolder.prefab", room, 3.4f, -1.68f, "Exit guard");
        }
        else if (stage == 2)
        {
            Spawn("Traps/Spikes.prefab", room, -0.3f, -2.49f, "Spiked lower route");
            if (index % 2 == 0)
                Spawn("Traps/Firetrap.prefab", room, 3.3f, -2.33f, "Timed flame");
            else
                Spawn("Enemy/RangedEnemyHolder.prefab", room, 3.2f, -1.68f, "Alcove guard");
        }
        else
        {
            Spawn("Traps/Spikes.prefab", room, -0.3f, -2.49f, "Vault spikes");
            if (index > 0)
                Spawn("Traps/Firetrap.prefab", room, -3, -2.33f, "Vault flame");
            if (index % 2 == 0)
                Spawn("Enemy/RangedEnemyHolder.prefab", room, 3.4f, -1.68f, "Vault guard");
            else
            {
                var arrows = Spawn("Traps/ArrowTrap.prefab", room, 0, 3.5f, "Ceiling arrow trap");
                arrows.transform.rotation = Quaternion.Euler(0, 0, 90);
            }
        }
    }

    private static GameObject Spawn(string path, Transform room, float x, float y, string name)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabFolder + path);
        if (!prefab) throw new InvalidOperationException("Missing prefab: " + path);
        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, room);
        go.name = name;
        go.transform.position = room.position + new Vector3(x, y, 0);
        if (path.Contains("HealthCollectible")) go.transform.localScale = Vector3.one * 0.5f;
        return go;
    }

    private static void FixScenePixelImports(Scene scene)
    {
        var textures = new HashSet<string>();
        foreach (var root in scene.GetRootGameObjects())
        {
            foreach (var renderer in root.GetComponentsInChildren<SpriteRenderer>(true))
                if (renderer.sprite) textures.Add(AssetDatabase.GetAssetPath(renderer.sprite));
            foreach (var animator in root.GetComponentsInChildren<Animator>(true))
            {
                if (!animator.runtimeAnimatorController) continue;
                foreach (var clip in animator.runtimeAnimatorController.animationClips)
                    foreach (var binding in AnimationUtility.GetObjectReferenceCurveBindings(clip))
                        foreach (var key in AnimationUtility.GetObjectReferenceCurve(clip, binding))
                            if (key.value is Sprite sprite) textures.Add(AssetDatabase.GetAssetPath(sprite));
            }
        }
        foreach (string path in textures)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (!importer || (importer.filterMode == FilterMode.Point && !importer.mipmapEnabled &&
                importer.textureCompression == TextureImporterCompression.Uncompressed)) continue;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }
    }

    private static void SetReference(Object target, string field, Object value)
    {
        var s = new SerializedObject(target);
        s.FindProperty(field).objectReferenceValue = value;
        s.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetString(Object target, string field, string value)
    {
        var s = new SerializedObject(target);
        s.FindProperty(field).stringValue = value;
        s.ApplyModifiedPropertiesWithoutUndo();
    }
}
