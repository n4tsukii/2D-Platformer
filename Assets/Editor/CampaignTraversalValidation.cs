using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Crosses each passage under Rigidbody2D physics instead of placing the player inside its trigger.</summary>
public static class CampaignTraversalValidation
{
    private static readonly List<string> checks = new List<string>();
    private static int stage, doorIndex, phase;
    private static double began;
    private static Platformer.Door[] doors;
    private static float passageX;
    private static string expectedScene;
    private static bool originalRunInBackground;

    public static void Begin()
    {
        if (!EditorApplication.isPlaying || SceneManager.GetActiveScene().name != "Level 1")
            throw new System.InvalidOperationException("Start Play Mode in Level 1 first.");
        EditorApplication.update -= Tick;
        checks.Clear(); stage = 1; phase = 0; doorIndex = 0;
        originalRunInBackground = Application.runInBackground;
        Application.runInBackground = true;
        SessionState.SetString("PhysicalCampaignQA", "Running");
        EditorApplication.update += Tick;
    }

    private static void Tick()
    {
        try
        {
            // Keep Play Mode advancing while Codex, rather than the Game view, has focus.
            EditorApplication.QueuePlayerLoopUpdate();
            if (!EditorApplication.isPlaying) { Finish("STOPPED"); return; }
            if (phase == 5)
            {
                if (SceneManager.GetActiveScene().name == expectedScene)
                {
                    checks.Add("PASS Level " + stage + " exit -> " + expectedScene);
                    if (stage == 3) { Finish("PASS"); return; }
                    stage++; doorIndex = 0; phase = 0;
                }
                else if (EditorApplication.timeSinceStartup - began > 5) Finish("FAIL scene transition");
                return;
            }

            var player = GameObject.Find("Player");
            var body = player.GetComponent<Rigidbody2D>();
            var movement = player.GetComponent<Platformer.PlayerMovement>();
            movement.enabled = false;
            if (phase == 0)
            {
                doors = Object.FindObjectsByType<Platformer.Door>(FindObjectsSortMode.None)
                    .OrderBy(d => d.transform.position.x).ToArray();
                var checkpoint = GameObject.Find("Level/Room1").GetComponentsInChildren<Collider2D>()
                    .First(c => c.CompareTag("Checkpoint"));
                player.transform.position = checkpoint.transform.position;
                body.linearVelocity = Vector2.zero;
                Physics2D.SyncTransforms();
                var previousMode = Physics2D.simulationMode;
                try
                {
                    Physics2D.simulationMode = SimulationMode2D.Script;
                    Physics2D.Simulate(0.02f);
                }
                finally { Physics2D.simulationMode = previousMode; }
                if (checkpoint.enabled) { Finish("FAIL checkpoint trigger"); return; }
                body.position = new Vector2(-6, -6.4f);
                player.GetComponent<Platformer.FallRespawn>().SendMessage("FixedUpdate");
                if (player.transform.position.y < -3 || Time.timeScale == 0) { Finish("FAIL pit respawn"); return; }
                checks.Add("PASS Level " + stage + " fall returns to active checkpoint");
                movement.enabled = false;
                phase = 3;
            }
            if (phase == 3)
            {
                passageX = doors[doorIndex].transform.position.x;
                player.transform.position = new Vector3(passageX - 1.8f, -1.62f, 0);
                body.linearVelocity = new Vector2(4, 0); body.gravityScale = 7;
                player.GetComponent<Platformer.Health>().AddHealth(99);
                if (doorIndex == doors.Length - 1)
                    expectedScene = stage < 3 ? "Level " + (stage + 1) : "Main Menu";
                // Step actual rigidbody physics deterministically: an unfocused Editor can
                // otherwise run editor callbacks while suspending Game-view physics frames.
                Physics2D.SyncTransforms();
                var originalMode = Physics2D.simulationMode;
                try
                {
                    Physics2D.simulationMode = SimulationMode2D.Script;
                    for (int step = 0; step < 45; step++)
                    {
                        body.linearVelocity = new Vector2(4, body.linearVelocity.y);
                        Physics2D.Simulate(0.02f);
                    }
                }
                finally { Physics2D.simulationMode = originalMode; }
                body.linearVelocity = Vector2.zero;
                if (doorIndex == doors.Length - 1)
                {
                    began = EditorApplication.timeSinceStartup; phase = 5; return;
                }
                if (body.position.x > passageX + 0.9f)
                {
                    checks.Add("PASS Level " + stage + " Room " + (doorIndex + 1) + " -> Room " + (doorIndex + 2));
                    doorIndex++; phase = 3; return;
                }
                Finish("FAIL blocked passage: Level " + stage + " Room " + (doorIndex + 1) + " x=" + body.position.x);
            }
        }
        catch (System.Exception e) { Finish("FAIL " + e.Message); }
    }

    private static void Finish(string outcome)
    {
        EditorApplication.update -= Tick;
        Application.runInBackground = originalRunInBackground;
        SessionState.SetString("PhysicalCampaignQA", outcome + "\n" + string.Join("\n", checks));
        Debug.Log("Physical campaign check: " + outcome + "\n" + string.Join("\n", checks));
        var player = GameObject.Find("Player");
        if (player)
        {
            player.GetComponent<Platformer.PlayerMovement>().enabled = true;
            player.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        }
    }
}
