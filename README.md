# 2D Platformer

Unity 6000.3.9f1 project with three connected levels, using the original character, Pixel Adventure terrain, UI, audio, enemies, and traps.

| Level | Rooms | Layout |
| --- | --- | --- |
| Level 1 | 4 | Original three rooms plus a fire-and-guard room; optional raised routes |
| Level 2 | 4 | Expanded original rooms, spikes, alternating flame and guard encounters |
| Level 3 | 5 | New gauntlet combining upper routes, spikes, flames, and ceiling arrows |

Open `Assets/Scenes/Main Menu.unity` and press Play. The existing PLAY button starts Level 1. Exit through each room's right-hand passage; the final passage advances to the next level. Finishing Level 3 returns to the menu.

Checkpoints and optional health pickups are available throughout the campaign. The original HUD, pause screen, game-over screen, menu layout, and controls are retained. In-scope gameplay sprites and animation frames use Point filtering without mipmaps or texture compression. Camera framing and sprite scales are retained.

The original project was committed and pushed before level authoring, at commit `8f012e3`. Generated Unity folders are excluded from Git.

## Validation

- Saved-scene checks: 4 / 4 / 5 rooms, connected doors, correct exits, checkpoints, one Canvas per gameplay scene, no missing scripts.
- Unity Play Mode: jumping through a one-way platform and landing, checkpoint respawn, room-camera transition, pause/resume, and physics-triggered Level 1 → Level 2 → Level 3 → Main Menu progression.
- No runtime errors reported during these checks. These checks do not replace a complete manual difficulty/playthrough review.

`Assets/Editor/SameThemeLevelBuilder.cs` documents the authoring process from the original two-level baseline. It refuses to run over the completed campaign. Make further layout edits directly in the saved scenes.
