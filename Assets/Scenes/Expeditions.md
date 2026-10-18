# Three additional expeditions

Open the main menu and click a stage under **New Expeditions**, or open one of these scenes directly:

| Scene | Biome | Layout |
| --- | --- | --- |
| Level 3 | Ruined Prison | Broken cellblocks, three shard pits, gradual platform climbs |
| Level 4 | Toxic Aqueducts | Four runoff channels with bridge islands and high supply routes |
| Level 5 | Ember Fortress | Four furnace crossings, tighter stepping stones, five wardens, final escape |

Each scene is 80 world units wide, with five sections, five checkpoint beacons, four optional healing caches, and a scrolling camera. Checkpoints restore one heart on first activation. Pits cost one heart and return the player to their latest beacon. Death restores the player there with full health. Existing movement, double jump, fireballs, enemy animations, audio, pause screen, and health UI are reused.

Controls: A/D or horizontal arrows to move; Space to jump, then again for the extra jump; left mouse button to fire while standing on a surface; Escape to pause.

The original Level 1 is preserved. Level 2 retains its layout and gains an entrance at its far end. The new chain is Level 2 → Level 3 → Level 4 → Level 5 → completion screen → main menu. All scenes are registered in Build Settings, and each new stage can also be launched directly from the menu.

The palette and layered traversal take inspiration from [Dead Cells](https://deadcells.com/). Geometry and decorations are authored in this project, with the project's existing character sprites.

The new stages use a friction-free player collider material to prevent sticking to platform sides. Their separate camera and respawn components leave the original stage behaviour intact.

For regeneration, use **Tools → Platformer → Build Three Expeditions** in Edit mode. This replaces Levels 3–5 and refreshes the menu shortcuts and Level 2 entrance, so save manual edits elsewhere before regenerating.

Validation: Unity compilation, real Physics2D checks of all 11 complete mandatory pit crossings (including both jumps for each bridge island), spawn settling, audio/shooting, hazard recovery, death respawning, both scene transitions, and the final completion state. Camera previews and the composited menu/completion UI were visually inspected.
