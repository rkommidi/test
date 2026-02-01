# Space Runner - Unity Mobile Game

A space shooter game built with Unity for mobile platforms.

## Setup in Unity

1. Open Unity Hub and create a new 2D project
2. Copy the `Assets` folder into your project
3. Create the following prefabs and assign sprites:
   - Player (with PlayerController, Collider2D, Rigidbody2D)
   - Enemy (with Enemy script, Collider2D)
   - Bullet (with Bullet script, Collider2D)
   - Star (with Star script, Collider2D)

## Scene Setup

1. Create a Canvas with UI elements:
   - Score Text
   - Game Over Panel
   - Pause Panel
   - Main Menu Panel

2. Create GameManager object with:
   - GameManager script
   - EnemySpawner script
   - Reference to UIManager

3. Add BackgroundScroller to an empty object for stars

## Tags Required

- "Player" - on player object
- "Enemy" - on enemy prefabs
- "Bullet" - on bullet prefab

## Controls

**Mobile:**
- Tap left side → Move left
- Tap right side → Move right
- Tap center → Shoot

**Desktop:**
- Arrow keys → Move
- Space → Shoot
- P/Escape → Pause

## Scripts

| Script | Purpose |
|--------|---------|
| GameManager.cs | Game state, score, pause |
| PlayerController.cs | Player movement and shooting |
| Enemy.cs | Enemy behavior and damage |
| Bullet.cs | Bullet movement |
| Star.cs | Collectible stars |
| EnemySpawner.cs | Spawns enemies and stars |
| UIManager.cs | UI panel management |
| BackgroundScroller.cs | Scrolling star background |

## Build Settings

For mobile:
1. File → Build Settings
2. Select Android or iOS
3. Player Settings → Set orientation to Portrait
4. Build and Run
