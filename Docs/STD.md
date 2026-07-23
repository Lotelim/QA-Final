# Software Test Description (STD)

## Space Shooter — QA Final Project

Document version 1.0 — companion to `STP.md`. Specifies the individual test cases (automated and manual) that implement the approach defined there.

## 1. Purpose and Traceability

This document lists every automated test class in the suite (mapped to the feature it covers) and a curated set of manual test cases for what automation cannot practically exercise in this environment. Every automated row is traceable to a real, currently-passing test file under `Assets/Tests` — this is a description of tests that exist and run, not a plan for tests yet to be written.

## 2. Severity Scale

| Severity | Definition |
|---|---|
| Critical | Crashes, data loss, or a core mechanic (movement, shooting, taking damage) completely broken |
| High | A feature works incorrectly in a way players will notice every session (e.g. level transition never fires) |
| Medium | A feature works incorrectly in an edge case, or a real defect with a workaround |
| Low | Cosmetic, or a non-functional/performance concern with no correctness impact |
| Trivial | Nitpick with no user-facing impact under normal play |

## 3. Automated Test Cases

### 3.1 EditMode (34 tests — pure logic, no MonoBehaviour lifecycle)

| Test Class | Cases | Covers |
|---|---|---|
| SplineUtilityTests | 12 | Catmull-Rom spline math shared by Wave and FollowThePath: endpoint exactness, collinear-path linearity, closed-loop continuity, pad-then-interpolate equivalence, and input validation (throws below 2 points) |
| ShieldTests | 6 | Shield.AbsorbDamage: partial absorption, exact depletion, overflow-through, already-depleted pass-through, zero/negative damage no-ops |
| BossMovementPatternTests | 16 | BossMovementPattern.PickNextDirection/ToVector: every roll-to-direction boundary (5-way split), the roll=1.0 edge case, and every direction's unit vector |

### 3.2 PlayMode (45 tests — MonoBehaviour lifecycle, physics, coroutines, scenes)

| Test Class | Cases | Covers |
|---|---|---|
| PlayerTests | 4 | Awake sets the static instance; any damage amount destroys the player; OnDestroy clears the instance; a second Player correctly takes over after the first is destroyed (the level-transition scenario) |
| PlayerMovingTests | 2 | Border computation from viewport + offsets; position clamping when pushed out of bounds |
| PlayerShootingTests | 4 | Projectile fan-out count at each weapon power level (1/2/3/4 → 1/2/3/5 shots) |
| BonusTests | 2 | Weapon power increments on pickup (and is capped at max); bonus is destroyed on pickup via real 2D trigger collision |
| BoundaryTests | 3 | Projectile- and Bonus-tagged objects are destroyed on exiting the boundary trigger; Enemy-tagged objects are left alone |
| ProjectileTests | 4 | Enemy bullet damages Player and self-destructs if configured; Player bullet damages Enemy; destroyedByCollision=false survives a hit; no error when Player.instance is stale (regression) |
| PoolingControllerTests | 3 | Prewarms the configured count as inactive clones; reuses an existing inactive instance; creates a new one only when none are available |
| RepeatingBackgroundTests | 2 | Repositions upward by 2x verticalSize when below threshold; does not reposition otherwise |
| VisualEffectTests | 1 | Destroys the GameObject once destructionTime elapses |
| WaveTests | 2 | Spawns the configured enemy count over time with the player present; stops spawning once the player is gone (regression) |
| LevelControllerTests | 3 | Zero-delay wave spawns immediately with the player present; does not spawn with no player; delayed wave waits before spawning |
| LevelControllerBossTests | 3 | No boss configured spawns nothing; boss spawns after its delay with the player present; does not spawn with no player at spawn time |
| ShieldIntegrationTests | 3 | Enemy+Shield: shield protects health until depleted, then overflow damages health; no-shield enemy behaves exactly as before; lethal damage through a depleted shield destroys the enemy |
| BossIntegrationTests | 3 | BossMovement stays within configured screen bounds across many frames; Boss (huge HP) survives a normal hit; Boss raises OnDestroyed and is removed when truly defeated |
| LevelCompletionTrackerTests | 4 | OnLevelComplete fires only once all registered enemies are defeated; does not fire again afterward; duplicate registration doesn't double-count; Enemy auto-registers with the active tracker on Start |
| LevelSceneLoadTests | 2 | Level_1 loads with completion tracking wired to Level_2; Level_2 loads with a shielded wave and a huge-HP boss present |

## 4. Manual Test Cases

### 4.1 Cross-platform (see `Docs/CROSS_PLATFORM_REPORT.md` for full detail)

| ID | Title | Steps (summary) | Expected Result |
|---|---|---|---|
| MT-01 | WebGL multi-browser | Open the WebGL build in Chrome, Firefox, and Safari | Loads and plays identically in all three; no console errors |
| MT-02 | WebGL slow connection | Throttle network, load the build | Loading bar progresses smoothly; no apparent hang |
| MT-03 | WebGL window resize | Resize the browser window / test common resolutions mid-game | Player and enemies stay on-screen; borders/boundary re-clamp correctly |
| MT-04 | Level_1 → Level_2 transition | Clear Level_1's waves and boss condition, let LevelFlow fire | Level_2 loads within ~2s with no visual hitch or double-audio |
| MT-05 | Mobile touch input | Deploy to an Android/iOS device or emulator, drag with one finger | Ship tracks the touch smoothly, stays clamped on-screen at device aspect ratio |
| MT-06 | Mobile performance | Play through a wave-heavy moment (multiple waves + power-ups + planets) | No unacceptable frame drop; note actual FPS observed |
| MT-07 | Mobile screen cutouts | Test on a device with a notch/cutout | No gameplay-critical sprite is obscured |
| MT-08 | Device orientation | Rotate the device during play | Game remains playable, or portrait is explicitly locked in Player Settings |

### 4.2 Accessibility (see `Docs/ACCESSIBILITY_REPORT.md` for full detail)

| ID | Title | Steps (summary) | Expected Result |
|---|---|---|---|
| MT-09 | Shape-based distinction | Observe Player/Enemy/Bonus on screen | Distinguishable by silhouette alone, without relying on color |
| MT-10 | Keyboard-only play attempt | Try to play using only a keyboard, no mouse/touch | Currently fails — no keyboard input path exists (logged as backlog, not a regression) |
| MT-11 | Flashing/seizure risk | Watch several enemy destruction VFX bursts in sequence | No sustained strobing/flashing effect |
| MT-12 | Colorblind simulation | View the WebGL build through a deuteranopia/protanopia emulator (e.g. browser devtools vision-deficiency emulation) | Player/Enemy/Bonus/Boss remain distinguishable |

### 4.3 Exploratory charter

| ID | Title | Charter |
|---|---|---|
| MT-13 | Full playthrough, Level_1 through Level_2 | Play from Level_1's first wave through Level_2's boss defeat in one sitting. Explore: does difficulty feel like it escalates as intended, does the shielded wave read as visibly different from a normal wave, does the boss's movement feel distinct from a regular enemy's fixed path, and does anything look or feel unfinished |

## 5. Test Data and Preconditions

Automated tests construct their own isolated GameObjects/components per test (see `Assets/Tests/PlayMode/TestSceneHelpers.cs` for shared setup helpers) rather than depending on the real Level_1/Level_2 scenes, except `LevelSceneLoadTests`, which deliberately loads the real generated scenes to verify their actual content — its teardown swaps back to a freshly created empty scene specifically so it cannot leak a stale Main Camera (or any other real-scene state) into later test fixtures. Manual test cases assume a built WebGL player (Tools > QA Final > Build WebGL) or a deployed mobile build, as noted per case.

## 6. Reporting

Automated results: NUnit XML from `Unity.exe -runTests`, also published as a GitHub Actions check on every push/PR once CI secrets are configured (`Docs/CI_SETUP.md`). Manual results: log pass/fail plus any deviation as a new entry in `Docs/BUG_TRACKING.md`, following the same fixed/open format already used there.
