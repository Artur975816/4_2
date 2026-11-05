# Moss & Moon — platformer project

Unity project made for Unity 6.0.3f1. Open this folder in Unity Hub and start at `MainMenu`.

## Controls

- Move: A/D or left/right arrows; gamepad left stick; on-screen stick on touch devices.
- Jump: Space, W, Up, gamepad South, or the on-screen JUMP button. Releasing jump early shortens the jump.
- Fire: X, left mouse button, gamepad West, or the on-screen FIRE button.
- Pause: Escape, gamepad Start, or the on-screen pause button.

## Level flow

`Level1` and `Level2` retain their authored scene objects. Runtime setup adds a longer platform route, two moving platforms, collectibles, a checkpoint, fall recovery, arena edges, and touch controls. Finishing Level 1 unlocks Level 2; finishing Level 2 unlocks the Level 3 boss arena. Defeating the boss ends the run and offers a return to the menu.

Level 3 had no playable arena objects in its scene, so `RuntimeLevelBootstrap` builds that arena from the project's existing pixel-art style at scene load. The player Animator Controller is loaded from `Assets/Resources/Player.controller`; it reuses the existing idle, run, and jump clips.

## Physics and combat

Player movement uses a 2.1-unit jump height, 5.5-unit horizontal speed, 0.08-second coyote time, and 0.1-second input buffer. Ground contact checks three downward rays against the Default and Ground layers, excluding the player's own collider. Falling uses increased gravity. Moving platforms use kinematic Rigidbody2D movement and pass their velocity to a player standing on top.

Player health, hazard damage, boss health, projectile hits, checkpoint respawns, and scene transitions use one gameplay flow. Boss health is clamped and the arena slider updates on every hit. Scene progression is selected by scene name so it does not depend on Build Settings order.

## Verification

Unity batch import and C# compilation were run during implementation. The remaining release check is to open each level in Play mode and verify the touch UI on a phone or Device Simulator; no physical device run was available during implementation. The existing animation assets provide idle, run, and jump clips; separate fall, land, hurt, death, and victory clips are not present.
