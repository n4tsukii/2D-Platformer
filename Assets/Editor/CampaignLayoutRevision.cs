using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>Distinct, editable room layouts made from the project's existing terrain.</summary>
public static class CampaignLayoutRevision
{
    private static Sprite ledge, earth;

    [MenuItem("Tools/Platformer/Apply Distinct Campaign Layouts")]
    public static void Apply()
    {
        if (EditorApplication.isPlaying) throw new System.InvalidOperationException("Stop Play Mode first.");
        var sprites = AssetDatabase.LoadAllAssetsAtPath(
            "Assets/Platformer/Sprites/Pixel Adventure 1/Assets/Terrain/Terrain (16x16) 1.png").OfType<Sprite>().ToArray();
        ledge = sprites.Single(s => s.name.EndsWith("_3"));
        earth = sprites.Single(s => s.name.EndsWith("_1"));
        for (int stage = 1; stage <= 3; stage++)
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Level " + stage + ".unity");
            var rooms = GameObject.Find("Level").transform.Cast<Transform>().ToArray();
            for (int i = 0; i < rooms.Length; i++)
            {
                var room = rooms[i];
                foreach (var child in room.Cast<Transform>().ToArray())
                    if (new[] { "Exploration route", "Room terrain", "Upper route reward", "Lower alcove reward" }.Contains(child.name))
                        Object.DestroyImmediate(child.gameObject);
                var route = new GameObject("Room terrain").transform;
                route.SetParent(room, false);
                var door = room.Find("Door").GetComponent<BoxCollider2D>();
                door.isTrigger = true;
                // The player reaches the final exit before running out of floor.
                if (i == rooms.Length - 1)
                {
                    door.offset = new Vector2(-0.55f, 0);
                    door.size = new Vector2(1.4f, 2.8f);
                }
                foreach (var checkpoint in room.GetComponentsInChildren<Collider2D>(true).Where(c => c.CompareTag("Checkpoint")))
                    checkpoint.transform.localPosition = new Vector3(-6f, -1.25f, 0);
                if (stage == 1) OpeningRooms(route, i);
                if (stage == 2) BrokenRooms(route, i);
                if (stage == 3) VaultRooms(route, i);
                var enemies = room.GetComponentsInChildren<Platformer.RangedEnemy>(true);
                // Give the new raised terrain room to breathe around the guards.
                if ((stage == 1 && i == 3) || stage == 3)
                    foreach (var enemy in enemies)
                        enemy.transform.parent.localPosition = new Vector3(5.2f, -1.68f, 0);
            }
            var player = GameObject.Find("Player");
            if (!player.GetComponent<Platformer.FallRespawn>()) player.AddComponent<Platformer.FallRespawn>();
            var loose = scene.GetRootGameObjects().FirstOrDefault(g => g.name == "HealthCollectible");
            if (loose && stage == 3) loose.transform.position = new Vector3(0, 2.05f, 0);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
        AssetDatabase.SaveAssets();
        EditorSceneManager.OpenScene("Assets/Scenes/Level 1.unity");
        SceneView.lastActiveSceneView?.LookAt(Vector3.zero, Quaternion.identity, 10, true);
        Debug.Log("Distinct campaign layouts saved. All room passages are triggers.");
    }

    private static void OpeningRooms(Transform r, int i)
    {
        switch (i)
        {
            case 0: // A short route over the original spikes, with a central lookout.
                L(r, -3, -1.2f); L(r, 0, 0.2f); L(r, 3.8f, -0.8f); Reward(r, 0, 0.8f); break;
            case 1: // A long upper gallery above the existing timed fire and arrow trap.
                L(r, -4.8f, -1.2f); L(r, -1.6f, 0.65f); L(r, 0.8f, 0.65f); L(r, 3.2f, 0.65f);
                L(r, 5.3f, -1.1f); Reward(r, 2, 1.3f); break;
            case 2: // Low broken walkway plus a separate high cache above the guard.
                L(r, -4.8f, -1.05f); L(r, -1.6f, -1.05f); L(r, 1.6f, -1.05f); L(r, 4.8f, -1.05f);
                L(r, 2.8f, 1.35f); Reward(r, 2.8f, 2); break;
            case 3: // Solid central mound: jump over it or climb for the upper reward.
                Block(r, -1.1f, -0.8f); Block(r, 1.1f, -0.8f);
                L(r, -4.5f, -1.2f); L(r, 0, 1.3f); L(r, 4.5f, -1.2f); Reward(r, 0, 1.95f);
                MoveTrap(r.parent, "Fire crossing", 3.4f, -2.33f); break;
        }
    }

    private static void BrokenRooms(Transform r, int i)
    {
        switch (i)
        {
            case 0: // A supply loft and grass-covered raised outcrop.
                L(r, -4.4f, -1.1f); L(r, -2, -1.1f); L(r, 1.2f, 0.7f); Block(r, 4.4f, -0.7f);
                Reward(r, 1.2f, 1.35f); break;
            case 1: // Broken ground with a short suspended bridge.
                Pit(r, new[] { 0, 1 }); L(r, -3.8f, -1); L(r, 0, -1); L(r, 3.8f, 0.5f);
                Reward(r, 3.8f, 1.15f); MoveTrap(r.parent, "Spiked lower route", 0.5f, -4.7f); break;
            case 2: // Alternating towers: the route doubles back to a cache on the left.
                Block(r, 0, -1.4f); L(r, -4.9f, -0.9f); L(r, -2.4f, 1.1f);
                L(r, 2.8f, 0.1f); L(r, 5.2f, 1.7f); Reward(r, -2.4f, 1.75f);
                MoveTrap(r.parent, "Spiked lower route", -3.2f, -2.49f); break;
            case 3: // Drop into a sunken alcove or stay on the upper crossing.
                Pit(r, new[] { -1, 0 }); L(r, -5.2f, -1.2f); L(r, -1.1f, -3.6f);
                L(r, -2, 0.1f); L(r, 1.6f, -1.3f); L(r, 4.8f, 0.5f);
                Reward(r, -1.1f, -2.95f); Reward(r, 4.8f, 1.15f);
                MoveTrap(r.parent, "Spiked lower route", 4, -2.49f); break;
        }
    }

    private static void VaultRooms(Transform r, int i)
    {
        switch (i)
        {
            case 0: // Two solid bastions with an optional high crossing.
                Block(r, -2.2f, -1.3f); Block(r, 2.2f, -0.2f);
                L(r, -5.3f, -1.2f); L(r, 0, 1.4f); L(r, 5.2f, 1); Reward(r, 5.2f, 1.65f); break;
            case 1: // Long broken bridge with a lower rescue shelf and falling arrows.
                Pit(r, new[] { -1, 0, 1 }); L(r, -4.8f, -1); L(r, -1.6f, -0.7f);
                L(r, 1.6f, -0.7f); L(r, 4.8f, -1); L(r, 0, -3.7f); Reward(r, 0, -3.05f);
                MoveTrap(r.parent, "Vault flame", 4.5f, -2.33f); MoveTrap(r.parent, "Vault spikes", 0, -4.9f); break;
            case 2: // Central watchtower reached from either side.
                Block(r, 0, -0.3f); L(r, -5.2f, -1.2f); L(r, -2.6f, 0.4f); L(r, 0, 2.1f);
                L(r, 2.6f, 0.4f); L(r, 5.2f, -1.2f); Reward(r, 0, 2.75f);
                MoveTrap(r.parent, "Vault spikes", 3.3f, -2.49f); break;
            case 3: // Upper crossing and a deliberately accessible lower cache.
                Pit(r, new[] { 0 }); L(r, -4.8f, -1.1f); L(r, -2, 0.2f); L(r, 2, 0.2f);
                L(r, 0, -3.6f); L(r, 4.8f, 1.6f); Reward(r, 0, -2.95f); Reward(r, 4.8f, 2.25f);
                MoveTrap(r.parent, "Vault spikes", 4, -2.49f); break;
            case 4: // Raised final arena with a high perch above the last guard.
                Block(r, -1.1f, -1); Block(r, 1.1f, -1);
                L(r, -4.5f, -0.9f); L(r, 0, 1.3f); L(r, 4.5f, -0.9f); Reward(r, 0, 1.95f);
                MoveTrap(r.parent, "Vault flame", -3.5f, -2.33f); MoveTrap(r.parent, "Vault spikes", 3.5f, -2.49f); break;
        }
    }

    private static void L(Transform parent, float x, float top)
    {
        var g = Art(parent, "One-way ledge", ledge, new Vector2(x, top - 0.125f));
        var box = g.AddComponent<BoxCollider2D>(); box.size = new Vector2(0.48f, 0.05f); box.usedByEffector = true;
        var e = g.AddComponent<PlatformEffector2D>(); e.useOneWay = true; e.useSideFriction = false; e.useSideBounce = false;
    }

    private static void Block(Transform parent, float x, float top)
    {
        var g = Art(parent, "Grass outcrop", earth, new Vector2(x, top - 1.2f));
        g.AddComponent<BoxCollider2D>().size = new Vector2(0.44f, 0.48f);
    }

    private static GameObject Art(Transform parent, string name, Sprite sprite, Vector2 p)
    {
        var g = new GameObject(name); g.transform.SetParent(parent, false);
        g.transform.localPosition = p; g.transform.localScale = Vector3.one * 5; g.layer = 6;
        var renderer = g.AddComponent<SpriteRenderer>(); renderer.sprite = sprite;
        renderer.sortingLayerName = "Foreground"; renderer.sortingOrder = 2;
        return g;
    }

    private static void Pit(Transform r, int[] missingTiles)
    {
        // Real gaps in both the ground artwork and collision, with intact seam landings.
        var original = r.parent.Find("Ground");
        if (original) original.gameObject.SetActive(false);
        for (int tile = -4; tile <= 4; tile++)
            if (!missingTiles.Contains(tile)) Block(r, tile * 2.2f, -2.86f);
    }

    private static void Reward(Transform r, float x, float y)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Platformer/Prefabs/Collectibles/HealthCollectible.prefab");
        var g = (GameObject)PrefabUtility.InstantiatePrefab(prefab, r);
        g.name = "Exploration cache"; g.transform.localPosition = new Vector3(x, y, 0);
        g.transform.localScale = Vector3.one * 0.5f;
    }

    private static void MoveTrap(Transform room, string name, float x, float y)
    {
        var t = room.Find(name); if (t) t.localPosition = new Vector3(x, y, 0);
    }
}
