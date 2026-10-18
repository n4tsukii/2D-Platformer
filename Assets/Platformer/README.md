# Platformer game assets

The primary levels are `Assets/Scenes/Level 1.unity` and `Assets/Scenes/Level 2.unity`. Play from `Assets/Scenes/Main Menu.unity`. The level flow is Main Menu -> Level 1 -> Level 2 -> Main Menu.

These room layouts and their dependencies originated from https://github.com/nickbota/Unity-Platformer-Episode-15 and were adapted for this Unity 6 project. The gameplay scripts use the `Platformer` namespace. Source GUIDs, layers and compatibility changes are recorded in `ImportManifest.json`.

Level 1 contains three rooms and a checkpoint. Level 2 contains two rooms; it shares the game-over/pause HUD from Level 1. `LevelExit` components connect the final door of each scene to the next scene.

Create additional levels in `Assets/Scenes` using these room hierarchies, camera doors, traps and checkpoint placements as references.
