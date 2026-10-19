using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;
using Object = UnityEngine.Object;
using TilemapTile = UnityEngine.Tilemaps.Tile;

/// <summary>
/// Builds the three campaign levels from the room maps below. Each room is one screen wide and
/// about two screens tall, with three floors; the camera follows the player vertically.
///
/// Map legend (one character per 0.8-unit tile, top row first):
///   #  grass ground          M  metal wall (wall-jumpable)     -  one-way platform (S/Down drops)
///   ^  spikes                F  fire trap                      less-than, greater-than, v  arrow trap (fires left, right, down)
///   E  knight facing left    e  knight facing right            H  heart
///   C  checkpoint            P  player start                   S  exit sign
/// Gaps in the outer wall columns are the passages between rooms; neighbouring rooms must share them.
///
/// Rebuilding replaces each scene's "Level" object, so make layout changes here rather than in the scenes.
/// </summary>
public static class CampaignRoomBuilder
{
    private const int Cols = 22, Rows = 23;
    private const float Tile = 0.8f, OriginY = -4f, RoomWidth = Cols * Tile;
    private const float CameraHalfHeight = 5f;

    private const string TileFolder = "Assets/Platformer/Tiles/";
    private const string AtlasPath = "Assets/Platformer/Sprites/Tiles/CampaignTiles.png";
    private const string PrefabFolder = "Assets/Platformer/Prefabs/";

    private static TileBase grassTile, metalTile, platformTile;
    private static Sprite background, exitSign;

    [MenuItem("Tools/Platformer/Build Campaign Rooms")]
    public static void BuildAll()
    {
        if (EditorApplication.isPlaying)
            throw new InvalidOperationException("Stop Play Mode before building levels.");
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        CreateTiles();
        background = LoadSprite("Assets/Platformer/Sprites/Pixel Adventure 1/Assets/Background/Brown.png", true);
        exitSign = AssetDatabase.LoadAllAssetsAtPath("Assets/Platformer/Sprites/Pixel Adventure 1/Assets/Items/Checkpoints/Start/Start (Idle).png")
            .OfType<Sprite>().First();

        BuildLevel(1, "Level 2", Level1);
        BuildLevel(2, "Level 3", Level2);
        BuildLevel(3, "Main Menu", Level3);

        EditorBuildSettings.scenes = new[] { "Main Menu", "Level 1", "Level 2", "Level 3" }
            .Select(n => new EditorBuildSettingsScene("Assets/Scenes/" + n + ".unity", true)).ToArray();
        EditorSceneManager.OpenScene("Assets/Scenes/Level 1.unity");
        Debug.Log("Campaign rooms built: 4 / 4 / 5 rooms.");
    }

    // ------------------------------------------------------------------ Level 1: the grounds
    private static readonly string[][] Level1 =
    {
        new[] // Climb: steps, a ledge zigzag and the first spikes.
        {
            "MMMMMMMMMMMMMMMMMMMMMM",
            "M                    M",
            "M                    M",
            "M                    M",
            "M                    M",
            "M                     ",
            "M                     ",
            "M                     ",
            "M          ^^     S   ",
            "M------##############M",
            "M                    M",
            "M                    M",
            "M ---                M",
            "M                    M",
            "M                    M",
            "M   ############-----M",
            "M                    M",
            "M                    M",
            "M                --- M",
            "M           ##       M",
            "M PC    ^^  ##       M",
            "######################",
            "######################",
        },
        new[] // Descent: drop through the one-way floor, slip past a knight, run the fire corridor.
        {
            "MMMMMMMMMMMMMMMMMMMMMM",
            "M                    M",
            "M                    M",
            "M                 H  M",
            "M                --- M",
            "                     M",
            "                     M",
            "                     M",
            "   C           F     M",
            "##########----#######M",
            "M                    M",
            "M                    M",
            "M                    M",
            "M                    M",
            "M                E   M",
            "M   #################M",
            "M                    M",
            "M                     ",
            "M                     ",
            "M                     ",
            "M     F   ^^^   F  S  ",
            "######################",
            "######################",
        },
        new[] // Shaft: duck the arrows, climb between the metal walls; a guarded hall hides a heart.
        {
            "MMMMMMMMMMMMMMMMMMMMMM",
            "M                    M",
            "M                    M",
            "M                    M",
            "M                    M",
            "M                     ",
            "M                M    ",
            "M                M    ",
            "M             ---M  S ",
            "M                M --M",
            "M                M   M",
            "M          ---   M   M",
            "M                M   M",
            "M                M --M",
            "M H e   ^^^      M   M",
            "M################M   M",
            "M                M   M",
            "                 M --M",
            "                     M",
            "                    <M",
            "  C    F   ^^        M",
            "######################",
            "######################",
        },
        new[] // Ambush: drop past a sentry, then cross the spike pit under fire to the exit.
        {
            "MMMMMMMMMMMMMMMMMMMMMM",
            "M                    M",
            "M                    M",
            "M                    M",
            "M                    M",
            "                     M",
            "                     M",
            "                     M",
            "  C               E  M",
            "#############   #####M",
            "M                    M",
            "M                    M",
            "M                    M",
            "M                    M",
            "M H  F               M",
            "M#########---########M",
            "M                    M",
            "M                     ",
            "M                     ",
            "M                     ",
            "M                E S  ",
            "##############^^^#####",
            "######################",
        },
    };

    // ------------------------------------------------------------------ Level 2: the ramparts
    private static readonly string[][] Level2 =
    {
        new[] // Arrow cascade: zigzag down the ledges between two arrow lanes.
        {
            "MMMMMMMMMMMMMMMMMMMMMM",
            "M                    M",
            "M                    M",
            "M                    M",
            "M                    M",
            "M                    M",
            "M                    M",
            "M                    M",
            "M PC                 M",
            "M#####               M",
            "M                    M",
            "M        ####       <M",
            "M                    M",
            "M              ##### M",
            "M>                   M",
            "M      ######        M",
            "M                    M",
            "M                     ",
            "M                     ",
            "M                     ",
            "MH      ^^^^      E S ",
            "######################",
            "######################",
        },
        new[] // Twin towers: wall-jump up the first tower and leap to the second over a spike floor.
        {
            "MMMMMMMMMMMMMMMMMMMMMM",
            "M                    M",
            "M                    M",
            "M                    M",
            "M            H       M",
            "M           ----     M",
            "M                    M",
            "M                    M",
            "M                    M",
            "M                    M",
            "M>           MM      M",
            "M            MM       ",
            "M    MM      MM       ",
            "M    MM      MM       ",
            "M    MM      MM    S  ",
            "M    MM      MM  ####M",
            "M    MM      MM  ####M",
            "     MM          ####M",
            "     MM        --####M",
            "     MM          ####M",
            "  C  MM^^^^^^^^^^####M",
            "######################",
            "######################",
        },
        new[] // Guard post: climb on the left, then push past the knight using the metal cover.
        {
            "MMMMMMMMMMMMMMMMMMMMMM",
            "M                    M",
            "M                    M",
            "M                    M",
            "M                    M",
            "M                     ",
            "M                     ",
            "M          ##         ",
            "M          ##     E S ",
            "M-----###############M",
            "M                    M",
            "                     M",
            "  ---                M",
            "         ##          M",
            "  C      ##        E M",
            "##############---#####",
            "M                    M",
            "M                    M",
            "M                    M",
            "M                    M",
            "M        ^^^^   F  H M",
            "######################",
            "######################",
        },
        new[] // Fire pillars: hop across burning pillar tops while arrows sweep the jump line.
        {
            "MMMMMMMMMMMMMMMMMMMMMM",
            "M                    M",
            "M                    M",
            "M                    M",
            "M          H         M",
            "          ---        M",
            "                     M",
            "                     M",
            "  C                  M",
            "##--                <M",
            "M     F   ##         M",
            "M     ##  ##  F       ",
            "M --  ##  ##  ##      ",
            "M     ##  ##  ##      ",
            "M     ##  ##  ##   S  ",
            "M     ##  ##  ##  ###M",
            "M --  ##  ##  ##  ###M",
            "M     ##  ##  ##  ###M",
            "M     ##  ##  ##  ###M",
            "M     ##  ##  ##  ###M",
            "M   ^^##^^##^^##^^###M",
            "######################",
            "######################",
        },
    };

    // ------------------------------------------------------------------ Level 3: the keep
    private static readonly string[][] Level3 =
    {
        new[] // Chimney: wall-jump up a narrow metal flue crossed by two arrow lanes.
        {
            "MMMMMMMMMMMMMMMMMMMMMM",
            "M                    M",
            "M                    M",
            "M                    M",
            "M                    M",
            "M                    M",
            "M             M   M  M",
            "M             M   M  M",
            "M             M   M  M",
            "M  H          M>  M  M",
            "M#############M   M  M",
            "M#############M   M   ",
            "M#############M   M   ",
            "M#############M  <M   ",
            "M#############M   M S ",
            "M#############M   M##M",
            "M             M   M##M",
            "M             M   M##M",
            "M             M   M##M",
            "M                 M##M",
            "M PC   F ^^^      M##M",
            "######################",
            "######################",
        },
        new[] // Spike bridge: stepping stones over a spike pit, a falling arrow lane and a sniper perch.
        {
            "MMMMMMMMMMMMMMMMMMMMMM",
            "M            v       M",
            "M                    M",
            "M                    M",
            "M                    M",
            "M                    M",
            "M                    M",
            "M                    M",
            "M                    M",
            "M                    M",
            "M                  E M",
            "                 #### ",
            "           ##         ",
            "                      ",
            "  C   ##        ##  S ",
            "####               ##M",
            "M                  ##M",
            "M                  ##M",
            "M                --##M",
            "M                  ##M",
            "M   ^^^^^^^^^^^^^^ ##M",
            "######################",
            "######################",
        },
        new[] // Tower: zigzag up burning ledges past an arrow lane, then drop on the exit guard.
        {
            "MMMMMMMMMMMMMMMMMMMMMM",
            "M                    M",
            "M                    M",
            "M                    M",
            "M              M     M",
            "M       F      M      ",
            "M      ##      M      ",
            "M>             M      ",
            "M          F   M   ES ",
            "M          ##  M#####M",
            "M              M#####M",
            "        F      M#####M",
            "       ##      M#####M",
            "               M#####M",
            "  C            M#####M",
            "######################",
            "######################",
            "######################",
            "######################",
            "######################",
            "######################",
            "######################",
            "######################",
        },
        new[] // Drop: fall floor by floor through fire, a knight's lane and a ground-level arrow.
        {
            "MMMMMMMMMMMMMMMMMMMMMM",
            "M                    M",
            "M                    M",
            "M                    M",
            "M                    M",
            "                     M",
            "                     M",
            "                     M",
            "  C              E H M",
            "#######---###########M",
            "M                    M",
            "M                    M",
            "M                    M",
            "M                    M",
            "M       F          E M",
            "M#############---####M",
            "M                    M",
            "M                     ",
            "M                     ",
            "M>                    ",
            "M                ^^ S ",
            "######################",
            "######################",
        },
        new[] // Final arena: fight up three floors, one knight per floor, to the last exit.
        {
            "MMMMMMMMMMMMMMMMMMMMMM",
            "M                    M",
            "M                    M",
            "M                    M",
            "M                    M",
            "M                     ",
            "M         ##          ",
            "M>        ##          ",
            "M         ##      E S ",
            "M----################M",
            "M                    M",
            "M                    M",
            "M  ---               M",
            "M                    M",
            "M e     F          H M",
            "M############----####M",
            "M                    M",
            "                     M",
            "              --     M",
            "         ##          M",
            "  C      ##       E  M",
            "######################",
            "######################",
        },
    };

    // ------------------------------------------------------------------ Scene assembly

    private static void BuildLevel(int number, string nextScene, string[][] maps)
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/Level " + number + ".unity");
        foreach (var root in scene.GetRootGameObjects())
            if (root.name == "Level" || root.name == "Spikes" || root.name == "HealthCollectible")
                Object.DestroyImmediate(root);

        var level = new GameObject("Level").transform;
        var player = GameObject.Find("Player");
        var cam = Camera.main.GetComponent<Platformer.CameraController>();
        var rooms = new List<Transform>();

        for (int i = 0; i < maps.Length; i++)
        {
            string[] map = maps[i];
            Validate(map, number, i);
            var room = new GameObject("Room" + (i + 1)).transform;
            room.SetParent(level, false);
            room.position = new Vector3(i * RoomWidth, 0, 0);
            rooms.Add(room);

            BuildBackground(room);
            BuildTerrain(room, map);
            var enemies = BuildObjects(room, map, player);

            var roomComponent = room.gameObject.AddComponent<Platformer.Room>();
            var so = new SerializedObject(roomComponent);
            var list = so.FindProperty("enemies");
            list.arraySize = enemies.Count;
            for (int e = 0; e < enemies.Count; e++) list.GetArrayElementAtIndex(e).objectReferenceValue = enemies[e];
            so.FindProperty("cameraMinY").floatValue = OriginY + CameraHalfHeight;
            so.FindProperty("cameraMaxY").floatValue = OriginY + Rows * Tile - CameraHalfHeight;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        if (OpenRows(maps[0], 0).Count > 0)
            throw new InvalidOperationException($"Level {number} Room1 has an opening in its outer left wall.");
        for (int i = 0; i < rooms.Count; i++)
        {
            var exitRows = OpenRows(maps[i], Cols - 1);
            if (exitRows.Count == 0)
                throw new InvalidOperationException($"Level {number} Room{i + 1} has no exit.");
            if (i + 1 < rooms.Count)
            {
                if (!exitRows.SequenceEqual(OpenRows(maps[i + 1], 0)))
                    throw new InvalidOperationException($"Level {number}: Room{i + 1} exit does not line up with Room{i + 2} entrance.");
                BuildDoor(rooms[i], rooms[i + 1], exitRows, cam);
            }
            else
                BuildExit(rooms[i], exitRows, nextScene);
        }

        ConfigurePlayer(player);
        var camSo = new SerializedObject(cam);
        camSo.FindProperty("player").objectReferenceValue = player.transform;
        camSo.ApplyModifiedPropertiesWithoutUndo();
        cam.transform.position = new Vector3(0, OriginY + CameraHalfHeight, cam.transform.position.z);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static void Validate(string[] map, int level, int room)
    {
        if (map.Length != Rows)
            throw new InvalidOperationException($"Level {level} Room{room + 1}: expected {Rows} rows, found {map.Length}.");
        for (int r = 0; r < map.Length; r++)
            if (map[r].Length != Cols)
                throw new InvalidOperationException($"Level {level} Room{room + 1}, line {r + 1}: expected {Cols} columns, found {map[r].Length}.");
    }

    // Row index 0 is the bottom of the room; the map lists rows top first.
    private static char At(string[] map, int col, int row) => map[Rows - 1 - row][col];

    private static bool IsTerrain(char c) => c == '#' || c == 'M' || c == '-';

    private static List<int> OpenRows(string[] map, int col)
    {
        var rows = new List<int>();
        for (int r = 0; r < Rows; r++)
            if (!IsTerrain(At(map, col, r))) rows.Add(r);
        return rows;
    }

    private static Vector3 CellCenter(Transform room, int col, int row) =>
        new Vector3(room.position.x - RoomWidth / 2 + (col + 0.5f) * Tile, OriginY + (row + 0.5f) * Tile, 0);

    private static float FloorTop(int row) => OriginY + row * Tile;

    private static void BuildBackground(Transform room)
    {
        var go = new GameObject("Background");
        go.transform.SetParent(room, false);
        go.transform.position = new Vector3(room.position.x, OriginY + Rows * Tile / 2, 0);
        go.transform.localScale = Vector3.one * 5;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = background;
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.size = new Vector2(RoomWidth, Rows * Tile) / 5;
        sr.sortingLayerName = "Background";
    }

    private static void BuildTerrain(Transform room, string[] map)
    {
        var grid = new GameObject("Terrain", typeof(Grid));
        grid.transform.SetParent(room, false);
        grid.transform.position = new Vector3(room.position.x - RoomWidth / 2, OriginY, 0);
        grid.transform.localScale = Vector3.one * 5;
        grid.GetComponent<Grid>().cellSize = new Vector3(Tile / 5, Tile / 5, 0);

        var ground = Layer(grid.transform, "Ground", "Ground", false);
        var walls = Layer(grid.transform, "Walls", "Wall", false);
        var platforms = Layer(grid.transform, "Platforms", "Ground", true);
        ground.gameObject.tag = "Ground";

        for (int r = 0; r < Rows; r++)
            for (int c = 0; c < Cols; c++)
            {
                var cell = new Vector3Int(c, r, 0);
                switch (At(map, c, r))
                {
                    case '#': ground.SetTile(cell, grassTile); break;
                    case 'M': walls.SetTile(cell, metalTile); break;
                    case '-': platforms.SetTile(cell, platformTile); break;
                }
            }

        // The editor builds tilemap colliders lazily; generate them now so the saved scene has
        // its merged geometry, otherwise a level loaded at runtime starts with no collision.
        foreach (var collider in grid.GetComponentsInChildren<TilemapCollider2D>())
        {
            collider.compositeOperation = Collider2D.CompositeOperation.None;
            collider.ProcessTilemapChanges();
            collider.compositeOperation = Collider2D.CompositeOperation.Merge;
            collider.ProcessTilemapChanges();
            collider.GetComponent<CompositeCollider2D>().GenerateGeometry();
        }
    }

    private static Tilemap Layer(Transform grid, string name, string layer, bool oneWay)
    {
        var go = new GameObject(name, typeof(Tilemap), typeof(TilemapRenderer));
        go.transform.SetParent(grid, false);
        go.layer = LayerMask.NameToLayer(layer);
        var renderer = go.GetComponent<TilemapRenderer>();
        renderer.sortingLayerName = "Foreground";
        renderer.sortingOrder = -10;

        var body = go.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Static;
        var composite = go.AddComponent<CompositeCollider2D>();
        composite.geometryType = CompositeCollider2D.GeometryType.Polygons;
        go.AddComponent<TilemapCollider2D>().compositeOperation = Collider2D.CompositeOperation.Merge;
        if (oneWay)
        {
            composite.usedByEffector = true;
            var effector = go.AddComponent<PlatformEffector2D>();
            effector.useOneWay = true;
            effector.useSideFriction = false;
            effector.useSideBounce = false;
        }
        return go.GetComponent<Tilemap>();
    }

    private static List<GameObject> BuildObjects(Transform room, string[] map, GameObject player)
    {
        var enemies = new List<GameObject>();
        for (int r = 0; r < Rows; r++)
        {
            for (int c = 0; c < Cols; c++)
            {
                Vector3 center = CellCenter(room, c, r);
                switch (At(map, c, r))
                {
                    case '^':
                        if (c > 0 && At(map, c - 1, r) == '^') break;
                        int length = 1;
                        while (c + length < Cols && At(map, c + length, r) == '^') length++;
                        BuildSpikes(room, center + Vector3.right * (length - 1) * Tile / 2, FloorTop(r), length);
                        break;
                    case 'F':
                        Spawn("Traps/Firetrap", room, new Vector3(center.x, FloorTop(r) + 0.53f));
                        break;
                    case '>':
                    case '<':
                    case 'v':
                        var trap = Spawn("Traps/ArrowTrap", room, center);
                        char d = At(map, c, r);
                        trap.transform.rotation = d == '>' ? Quaternion.identity
                            : d == '<' ? Quaternion.Euler(0, 180, 0) : Quaternion.Euler(0, 0, -90);
                        trap.name = "ArrowTrap";
                        break;
                    case 'E':
                    case 'e':
                        var holder = Spawn("Enemy/RangedEnemyHolder", room, new Vector3(center.x + 0.25f, FloorTop(r) + 1.18f));
                        holder.name = "Knight";
                        if (At(map, c, r) == 'e')
                        {
                            var knight = holder.transform.Find("RangedEnemy");
                            var s = knight.localScale;
                            knight.localScale = new Vector3(Mathf.Abs(s.x), s.y, s.z);
                        }
                        enemies.Add(holder);
                        break;
                    case 'H':
                        Spawn("Collectibles/HealthCollectible", room, center).name = "Heart";
                        break;
                    case 'C':
                        Spawn("Props/Checkpoint", room, new Vector3(center.x, FloorTop(r) + 1.61f)).name = "Checkpoint";
                        break;
                    case 'S':
                        var sign = new GameObject("Exit sign");
                        sign.transform.SetParent(room, false);
                        sign.transform.position = new Vector3(center.x, FloorTop(r) + 1.6f);
                        sign.transform.localScale = Vector3.one * 5;
                        var sr = sign.AddComponent<SpriteRenderer>();
                        sr.sprite = exitSign;
                        sr.sortingLayerName = "Foreground";
                        sr.sortingOrder = -5;
                        break;
                    case 'P':
                        player.transform.position = new Vector3(center.x, FloorTop(r) + 1.24f, 0);
                        break;
                }
            }
        }
        return enemies;
    }

    private static void BuildSpikes(Transform room, Vector3 center, float floorTop, int length)
    {
        var spikes = Spawn("Traps/Spikes", room, new Vector3(center.x, floorTop + 0.37f));
        PrefabUtility.UnpackPrefabInstance(spikes, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        spikes.name = "Spikes";
        var template = spikes.transform.GetChild(0);
        foreach (Transform child in spikes.transform.Cast<Transform>().Skip(1).ToArray())
            Object.DestroyImmediate(child.gameObject);
        for (int i = 0; i < length; i++)
        {
            var piece = i == 0 ? template : Object.Instantiate(template, spikes.transform);
            piece.name = "Spike";
            piece.localPosition = new Vector3((i - (length - 1) / 2f) * Tile, 0, 0);
        }
        var box = spikes.GetComponent<BoxCollider2D>();
        box.offset = new Vector2(0, box.offset.y);
        box.size = new Vector2(length * Tile - 0.19f, box.size.y);
    }

    private static void BuildDoor(Transform room, Transform next, List<int> rows, Platformer.CameraController cam)
    {
        var door = new GameObject("Door to " + next.name);
        door.transform.SetParent(room, false);
        door.transform.position = new Vector3(room.position.x + RoomWidth / 2, OriginY + (rows.First() + rows.Count / 2f) * Tile, 0);
        var box = door.AddComponent<BoxCollider2D>();
        box.isTrigger = true;
        box.size = new Vector2(2 * Tile, rows.Count * Tile);
        var component = door.AddComponent<Platformer.Door>();
        var so = new SerializedObject(component);
        so.FindProperty("previousRoom").objectReferenceValue = room;
        so.FindProperty("nextRoom").objectReferenceValue = next;
        so.FindProperty("cam").objectReferenceValue = cam;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void BuildExit(Transform room, List<int> rows, string nextScene)
    {
        var exit = new GameObject("Level exit");
        exit.transform.SetParent(room, false);
        exit.transform.position = new Vector3(room.position.x + RoomWidth / 2 - Tile / 2, OriginY + (rows.First() + rows.Count / 2f) * Tile, 0);
        var box = exit.AddComponent<BoxCollider2D>();
        box.isTrigger = true;
        box.size = new Vector2(Tile, rows.Count * Tile);
        var so = new SerializedObject(exit.AddComponent<Platformer.LevelExit>());
        so.FindProperty("nextScene").stringValue = nextScene;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void ConfigurePlayer(GameObject player)
    {
        var so = new SerializedObject(player.GetComponent<Platformer.PlayerMovement>());
        so.FindProperty("wallJumpX").floatValue = 6;
        so.FindProperty("wallJumpY").floatValue = 18;
        so.ApplyModifiedPropertiesWithoutUndo();

        // Without friction the player slides off block sides instead of sticking to them mid-air.
        const string materialPath = "Assets/Platformer/Physics/PlayerNoFriction.physicsMaterial2D";
        var material = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(materialPath);
        if (!material)
        {
            System.IO.Directory.CreateDirectory("Assets/Platformer/Physics");
            material = new PhysicsMaterial2D("PlayerNoFriction") { friction = 0, bounciness = 0 };
            AssetDatabase.CreateAsset(material, materialPath);
        }
        player.GetComponent<BoxCollider2D>().sharedMaterial = material;
        // Long drops onto thin one-way platforms must not tunnel through them.
        player.GetComponent<Rigidbody2D>().collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    private static GameObject Spawn(string prefab, Transform room, Vector3 position)
    {
        var asset = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabFolder + prefab + ".prefab");
        if (!asset) throw new InvalidOperationException("Missing prefab: " + prefab);
        var go = (GameObject)PrefabUtility.InstantiatePrefab(asset, room);
        go.transform.position = position;
        return go;
    }

    // ------------------------------------------------------------------ Tiles

    private static void CreateTiles()
    {
        var importer = (TextureImporter)AssetImporter.GetAtPath(AtlasPath);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = 100;
        importer.filterMode = FilterMode.Point;
        importer.mipmapEnabled = false;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        var settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect;
        settings.spriteGenerateFallbackPhysicsShape = true;
        importer.SetTextureSettings(settings);

        // 64x64 atlas: grass 3x3 set and metal block on top, gold platform pieces along the bottom.
        var names = new[] { "GrassTL", "GrassT", "GrassTR", "GrassL", "GrassM", "GrassR", "GrassBL", "GrassB", "GrassBR" };
        var slices = new List<SpriteMetaData>();
        for (int i = 0; i < 9; i++)
            slices.Add(Slice(names[i], (i % 3) * 16, 48 - (i / 3) * 16));
        slices.Add(Slice("Metal", 48, 48));
        slices.Add(Slice("PlatformL", 0, 0));
        slices.Add(Slice("PlatformM", 16, 0));
        slices.Add(Slice("PlatformR", 32, 0));
#pragma warning disable CS0618
        importer.spritesheet = slices.ToArray();
#pragma warning restore CS0618
        importer.SaveAndReimport();

        var sprites = AssetDatabase.LoadAllAssetsAtPath(AtlasPath).OfType<Sprite>().ToDictionary(s => s.name);
        System.IO.Directory.CreateDirectory(TileFolder);

        const int Empty = RuleTile.TilingRuleOutput.Neighbor.NotThis;
        Vector3Int up = Vector3Int.up, down = Vector3Int.down, left = Vector3Int.left, right = Vector3Int.right;

        var grass = SaveTile<RuleTile>("Grass");
        grass.m_DefaultSprite = sprites["GrassM"];
        grass.m_DefaultColliderType = TilemapTile.ColliderType.Grid;
        grass.m_TilingRules = new List<RuleTile.TilingRule>
        {
            Rule(sprites["GrassTL"], TilemapTile.ColliderType.Grid, (up, Empty), (left, Empty)),
            Rule(sprites["GrassTR"], TilemapTile.ColliderType.Grid, (up, Empty), (right, Empty)),
            Rule(sprites["GrassT"], TilemapTile.ColliderType.Grid, (up, Empty)),
            Rule(sprites["GrassBL"], TilemapTile.ColliderType.Grid, (down, Empty), (left, Empty)),
            Rule(sprites["GrassBR"], TilemapTile.ColliderType.Grid, (down, Empty), (right, Empty)),
            Rule(sprites["GrassB"], TilemapTile.ColliderType.Grid, (down, Empty)),
            Rule(sprites["GrassL"], TilemapTile.ColliderType.Grid, (left, Empty)),
            Rule(sprites["GrassR"], TilemapTile.ColliderType.Grid, (right, Empty)),
        };
        EditorUtility.SetDirty(grass);
        grassTile = grass;

        var metal = SaveTile<TilemapTile>("Metal");
        metal.sprite = sprites["Metal"];
        metal.colliderType = TilemapTile.ColliderType.Grid;
        EditorUtility.SetDirty(metal);
        metalTile = metal;

        var platform = SaveTile<RuleTile>("Platform");
        platform.m_DefaultSprite = sprites["PlatformM"];
        platform.m_DefaultColliderType = TilemapTile.ColliderType.Sprite;
        platform.m_TilingRules = new List<RuleTile.TilingRule>
        {
            Rule(sprites["PlatformL"], TilemapTile.ColliderType.Sprite, (left, Empty)),
            Rule(sprites["PlatformR"], TilemapTile.ColliderType.Sprite, (right, Empty)),
        };
        EditorUtility.SetDirty(platform);
        platformTile = platform;
        AssetDatabase.SaveAssets();
    }

    private static SpriteMetaData Slice(string name, int x, int y) => new SpriteMetaData
    {
        name = name,
        rect = new Rect(x, y, 16, 16),
        alignment = (int)SpriteAlignment.Center,
        pivot = new Vector2(0.5f, 0.5f),
    };

    private static T SaveTile<T>(string name) where T : TileBase
    {
        string path = TileFolder + name + ".asset";
        var tile = AssetDatabase.LoadAssetAtPath<T>(path);
        if (tile) return tile;
        tile = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(tile, path);
        return tile;
    }

    private static RuleTile.TilingRule Rule(Sprite sprite, TilemapTile.ColliderType collider, params (Vector3Int position, int neighbor)[] neighbors)
    {
        return new RuleTile.TilingRule
        {
            m_Sprites = new[] { sprite },
            m_ColliderType = collider,
            m_NeighborPositions = neighbors.Select(n => n.position).ToList(),
            m_Neighbors = neighbors.Select(n => n.neighbor).ToList(),
        };
    }

    private static Sprite LoadSprite(string path, bool fullRect)
    {
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        var settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        if (fullRect && settings.spriteMeshType != SpriteMeshType.FullRect)
        {
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
        }
        return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().First();
    }
}
