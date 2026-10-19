# 2D Platformer

Unity 6000.3.9f1 project with a three-level campaign, using the original character, Pixel Adventure terrain, UI, audio, enemies, and traps.

Open `Assets/Scenes/Main Menu.unity` and press Play. The PLAY button starts Level 1; finishing Level 3 returns to the menu.

## Levels

Every room is one screen wide and about two screens tall, with three floors. The camera snaps between rooms at the passages, as before, and follows the player up and down inside a room.

| Level | Rooms | Layout |
| --- | --- | --- |
| Level 1 | 4 | Climb, descent through a one-way floor, arrow shaft with a guarded side hall, spike-pit ambush |
| Level 2 | 4 | Arrow cascade, twin metal towers, guard post with cover, burning pillars |
| Level 3 | 5 | Wall-jump chimney, spike bridge, fire tower, floor-by-floor drop, three-floor final arena |

Grass is solid ground, grey metal blocks are walls the player can cling to and wall-jump from, and gold strips are one-way platforms.

## Controls

A/D or arrows to move, Space to jump (again in the air for the double jump, or against a metal wall to wall-jump), S or Down to drop through a gold platform, left mouse to fire while standing still, Escape to pause.

## Editing rooms

Rooms are drawn as text maps in `Assets/Editor/CampaignRoomBuilder.cs`; the legend is at the top of that file. Run **Tools > Platformer > Build Campaign Rooms** to rebuild all three scenes from the maps. Rebuilding replaces each scene's `Level` object, so change layouts in the maps rather than by hand in the scenes.

Terrain uses Tilemaps with the rule tiles in `Assets/Platformer/Tiles` (edges are picked automatically), so the rooms can also be inspected or painted with the Tile Palette.
