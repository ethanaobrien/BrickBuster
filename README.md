# Overview

A Breakout-style brick-breaking game written in C# for Unity. It handles a moving paddle, a ball with wall/paddle/brick collisions, a score and lives system, and four difficulty levels, all built in code with no image assets.

The purpose of this project is to practice the game loop, collision detection, scene management, and state that survives scene loads while building a complete playable game.

The game is a Breakout clone. A wall of colored bricks sits at the top, the player moves a paddle at the bottom with the arrow keys or A/D, and launches the ball with Space. The ball bounces off the walls, the paddle, and the bricks. Each brick destroyed is worth 10 points. Where the ball hits the paddle changes its bounce angle, so shots can be steered into the corners. Three lives per round, drop the ball and you lose one. Clear the wall to win!

# Development Environment

The game uses C# 10 and Unity 6000.3.17f1, with the Unity Input System and TextMeshPro packages.

- Unity — game engine: 2D rendering, scene management, and UI
- Unity Input System — polling keyboard state (arrow keys, A/D, Space)
- TextMeshPro — rendering the menu text
- Rider — browsing and editing the code

## Run it

Open the project in Unity 6000.3.17f1 and play the **StartScene** to see the difficulty menu, then the **GameScene** to play.

- Left/Right or A/D moves the paddle, Space launches the ball.
- Easy, Medium, Hard, and Expert build larger brick walls.
- Its purpose here is to show the collision, scene transition, and difficulty features.

# Useful Websites

- [Unity — Scene Management](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/SceneManagement.SceneManager.html) - switching between menu and game scenes
- [Unity Input System](https://docs.unity3d.com/Packages/com.unity.inputsystem@latest/) - keyboard input polling
- [Wikipedia — Breakout](https://en.wikipedia.org/wiki/Breakout_(video_game)) - the game design this is based on
- [Wikipedia — Collision detection](https://en.wikipedia.org/wiki/Collision_detection) - the collision approach used in the game

# Future Work

- Add sound effects for paddle hits, brick breaks, and game over.
- Save and load the current score and lives so a round can be resumed.
- Add power-ups (wider paddle, multi-ball) dropped by destroyed bricks.
- Support mouse and gamepad input in addition to the keyboard.
