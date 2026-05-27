# Void Arena (Unity 2D) – Beginner Prototype

This project is a small playable “Vampire Survivors”-style prototype:

- One arena map
- One player (WASD movement)
- Two enemy types (melee + ranged)
- Automatic shooting at nearest enemy
- XP gems + leveling (weapon upgrades automatically)
- Increasing enemy spawn rate over time
- Win condition at **3:00**

The code is intentionally simple and easy to expand.

## Project Structure

Scripts live in `Assets/Scripts/`:

- `PlayerController`: WASD movement + HP/death
- `WeaponController`: auto-target + continuous firing + level-up scaling
- `EnemyAI`: melee enemy (chase + contact damage + XP drop)
- `RangedEnemyAI`: ranged enemy (keeps distance + shoots)
- `EnemySpawner`: random spawns around player, ramps up over time
- `XPManager`: XP/level progression + weapon level-ups
- `UIManager`: HP/XP bars, timer, level text, game over/victory screens
- `GameManager`: match timer (3 minutes), win/lose, restart/menu, freeze gameplay

Small helpers:

- `Projectile`: used for both player bullets and enemy projectiles
- `XpGem`: XP collectible
- `CameraFollow2D`: smooth camera follow
- `MainMenuUI`: menu buttons hookup

## Unity Setup (Quick Steps)

### 1) Scenes

Create two scenes in `Assets/Scenes/`:

- `MainMenu`
- `Game` (you can rename `SampleScene` to `Game`)

Then add both to **File → Build Settings… → Scenes In Build** in this order:

1. `MainMenu`
2. `Game`

### 2) Tags (important)

Create tags:

- `Player`
- `Enemy`

Assign:

- Player object tag = `Player`
- Enemy prefabs tag = `Enemy`

### 3) Game Scene Objects

Create these objects in the `Game` scene:

- **GameManager**
  - Create empty object `GameManager`
  - Add `GameManager` component
  - Set `mainMenuSceneName = MainMenu`, `gameSceneName = Game`
- **Player**
  - Create a simple sprite (circle/square) named `Player`
  - Add `Rigidbody2D` (Body Type: Dynamic, Gravity Scale: 0)
  - Add a `CapsuleCollider2D` or `CircleCollider2D`
  - Add `PlayerController`
  - Add `XPManager`
  - Add `WeaponController`
  - Add a child empty object `FirePoint` in front of the player
  - Assign `WeaponController.firePoint = FirePoint`
- **Main Camera**
  - Add `CameraFollow2D`
  - Assign `target = Player`
- **EnemySpawner**
  - Create empty object `EnemySpawner`
  - Add `EnemySpawner` component
  - Assign `player = Player`
  - Assign enemy prefabs (created in step 4)
- **UI**
  - Create a Canvas + EventSystem
  - Add:
    - HP bar (Slider)
    - XP bar (Slider)
    - Timer text (TMP_Text)
    - Level text (TMP_Text)
    - GameOver panel (hidden by default) with Restart + Menu buttons
    - Victory panel (hidden by default) with Menu button
  - Add `UIManager` to a UI root object and wire references.

### 4) Prefabs (simple)

Create these prefabs (place them under `Assets/Prefabs/` if you want):

#### Player Bullet prefab

- Create a small sprite `PlayerBullet`
- Add `Rigidbody2D` (Gravity Scale: 0, Body Type: Kinematic is fine)
- Add `CircleCollider2D` set **Is Trigger = true**
- Add `Projectile`

Assign `WeaponController.projectilePrefab = PlayerBullet`.

#### Enemy Melee prefab

- Create a sprite `EnemyMelee`
- Add `Rigidbody2D` (Gravity Scale: 0)
- Add `Collider2D` (not trigger)
- Add `EnemyAI`
- Assign `xpGemPrefab` (below)

#### Enemy Ranged prefab

- Create a sprite `EnemyRanged`
- Add `Rigidbody2D` (Gravity Scale: 0)
- Add `Collider2D` (not trigger)
- Add `RangedEnemyAI`
- Assign `projectilePrefab` (Enemy projectile below)
- Assign `xpGemPrefab` (below)

#### Enemy Projectile prefab

- Create a small sprite `EnemyProjectile`
- Add `Rigidbody2D` (Gravity Scale: 0)
- Add `CircleCollider2D` set **Is Trigger = true**
- Add `Projectile`

#### XP Gem prefab

- Create a small sprite `XpGem`
- Add `CircleCollider2D` set **Is Trigger = true**
- Add `XpGem`

Important: the **Player** must have a `Collider2D` so triggers fire.

### 5) Main Menu Scene

In `MainMenu`:

- Canvas with title text: **VOID SURVIVORS**
- Start button
- Quit button
- Add `MainMenuUI` to an object and wire the buttons
- Ensure a `GameManager` exists in this scene too (same as Game scene), or place it in one scene and keep `DontDestroyOnLoad` (already in script).

## Notes / Common Gotchas

- If bullets don’t hit: ensure `Projectile` collider is **Is Trigger** and enemies have tag `Enemy`.
- If XP doesn’t collect: ensure XP gem collider is **Is Trigger** and Player has `XPManager`.
- If game “freezes” immediately in editor: `Time.timeScale` may be 0 after GameOver/Win; press Play again or call `Restart()`.

