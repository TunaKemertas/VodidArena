# Void Survivors (Void Arena)

A small **Vampire Survivors–style** 2D survival prototype built in Unity. Survive enemy waves for **3 minutes**, collect XP, level up your auto-weapon, and try not to die.

---

## Game Description

You control a survivor in a dark arena. Enemies spawn continuously and get more frequent over time. Your weapon **automatically shoots** the nearest enemy at a modest rate. Defeating enemies drops **XP gems**; collecting them fills your XP bar and **levels you up** (more damage + a small fire-rate boost). Win by surviving until the timer hits **0:00**. Lose if your HP reaches zero.

---

## Controls

| Input | Action |
|--------|--------|
| **W / A / S / D** or **Arrow keys** | Move |
| **Esc** or **Pause button** | Pause / resume |
| **Mouse** | UI buttons (Play, Settings, Restart, Menu, etc.) |

---

## Implemented Mechanics

- Top-down **WASD movement** (Rigidbody2D)
- **Automatic targeting** and continuous shooting
- **Two enemy types**: melee (chase + contact damage) and ranged (keeps distance + shoots)
- **Enemy spawner** with increasing spawn rate over 3 minutes
- **XP gems**, leveling, and automatic weapon upgrades
- **3-minute survival timer** with win condition
- **Game Over** on death with restart / main menu
- **Full UI flow**: Main Menu → Game → Win/Lose → Restart or Menu
- **Pause menu** (Resume, Restart, Main Menu, Settings)
- **Settings panel** (Master / Music / SFX volume, saved with PlayerPrefs)
- **Mobile-ready UI**: Canvas Scaler (1080×1920 portrait), anchors, **Safe Area** script
- **DOTween animations** (menu intro, level-up, game over)
- **Juice**: camera shake on hit, level-up punch, XP gem pop, button feedback
- **Audio**: procedural placeholder music + SFX (replace with real clips anytime)

---

## Cut Mechanics (and Why)

| Cut | Reason |
|-----|--------|
| Multiple weapons / inventory | Out of scope; one auto-weapon keeps the prototype focused |
| Procedural map generation | Single arena is enough for a course demo |
| Multiplayer / online | Not required; adds major complexity |
| Advanced RPG stats (crit, armor, perks menu) | Would over-engineer a beginner solo project |
| TextMeshPro dependency | Used built-in UI `Text` for simpler setup |

---

## Features Added During Development

1. Runtime **bootstrap** scenes (playable without manual prefab wiring)
2. **uGUI** panels replacing early IMGUI prototype UI
3. **Pause + Settings** systems with persistent volume
4. **Safe Area** support for notched phones
5. **DOTween** polish on menus and game-over flow
6. **Camera shake**, XP pop, and button juice
7. **Organized script folders** (Core, UI, Combat, Enemies, etc.)

---

## Project Structure

```
Assets/Scripts/
  Bootstrap/     GameBootstrapper, MenuBootstrapper
  Core/          GameManager, SettingsManager
  UI/            UIManager, MainMenuUI, SafeArea, UIAnimations, UICanvasFactory, UIButtonFeedback
  Audio/         AudioManager
  Player/        PlayerController
  Combat/        WeaponController, Projectile
  Enemies/       EnemyAI, RangedEnemyAI, EnemySpawner
  Progression/   XPManager, XpGem
  Utils/         AutoSprite2D, CameraFollow2D
```

---

## How to Run

1. Open the project in **Unity 6** (6000.x).
2. Wait for packages to resolve (**DOTween** + **UGUI** from `Packages/manifest.json`).
3. Open **`Assets/Scenes/MainMenu.unity`**
4. Press **Play** → **Play** → survive 3 minutes.

Build order in **File → Build Settings**:
1. `MainMenu`
2. `Game`

---

## AI Usage (Step by Step)

### Tools used
- **Cursor AI** (Claude-based coding agent in the IDE)

### Example prompts used
- *"Create a simple top-down 2D roguelike survival game in Unity called Void Arena…"*
- *"Make it ready to play with scenes and bootstrap scripts"*
- *"Fix white sprites / missing script errors / restart menu not closing"*
- *"Polish for final submission: uGUI panels, DOTween, pause, settings, safe area, README"*

### How AI helped
| Phase | AI contribution |
|-------|------------------|
| **Coding** | Generated core gameplay scripts (movement, shooting, enemies, XP, spawner) |
| **Debugging** | Fixed inactive bullet prefabs, sprite color timing, scene junk, UI compile errors |
| **Design** | Kept scope small (one map, two enemies, one weapon) suitable for a solo student |
| **Polish** | Added UI panels, DOTween sequences, audio stub, folder organization, documentation |

### What I verified myself
- Play mode flow: menu → game → win/lose → restart
- Pause does not break timer (uses `unscaledDeltaTime`)
- Settings persist after restart

---

## What Changed and Why

| Change | Why |
|--------|-----|
| IMGUI → **uGUI Canvas** | Course requires proper UI panels, sliders, and mobile scaling |
| Added **DOTween** | Required animations with easing and sequences |
| Added **SafeArea** | Mobile notch / inset support for portrait UI |
| Added **Pause + Settings** | Required menu flow and volume controls |
| **Camera shake + juice** | Makes combat feedback readable and more polished |
| **Script folders** | Easier grading and future expansion |
| Kept **bootstrap spawns** | Project stays playable immediately after clone—no manual prefab setup |

---

## DOTween Notes

This project includes a **built-in lightweight tween module** at `Assets/Plugins/VoidSurvivorsTween/` that uses the same **`DG.Tweening` API** as DOTween (sequences, easing, UI tweens). It was added because the OpenUPM DOTween package could not be resolved on all machines.

Your UI animation code (`UIAnimations.cs`, `UIButtonFeedback.cs`, `XpGem.cs`) works without installing anything extra.

**Optional:** You can still import the official **DOTween** from the Unity Asset Store later. Remove the `VoidSurvivorsTween` folder if you switch to the full asset to avoid duplicate `DG.Tweening` types.

---

## Android APK (missing sprites fix)

Unity only ships art into a build if it is:

1. Assigned on a component in a **built scene** (recommended), or  
2. Placed under an **`Assets/Resources/`** folder and loaded with `Resources.Load`.

This project does both:

- **Game scene:** `GameBootstrapper` has all art prefabs wired in `Assets/Scenes/Game.unity`.
- **Fallback:** copies of prefabs also live in `Assets/Resources/Prefabs/` for `RuntimePrefabLoader`.

**If you add a new prefab:**

1. Put the `.prefab` in `Assets/Prefabs/`.
2. Copy it to `Assets/Resources/Prefabs/` (or drag a copy there in Unity).
3. Assign it on **GameBootstrapper → Art Prefabs** in the **Game** scene.

Then rebuild the APK. In the Editor, missing assets can still appear because `RuntimePrefabLoader` can read `Assets/Prefabs/` directly—that path does **not** exist on Android.

---

## License / Credits

Student prototype for university submission. Placeholder audio is generated in code; swap with licensed SFX/music in `AudioManager` when ready.
