# Bug Tracking

You chose local-only tracking for now (no GitHub repo existed yet in this
session). Every item below is written as a ready-to-paste GitHub Issue
(title, labels, severity, description, repro, fix status) so that once you
create a repo and push, you (or I, in a follow-up session) can file each one
directly with `gh issue create --title "..." --body "..." --label "..."` or
through the GitHub UI. Fixed items reference the commit that fixed them and
the regression test that proves it; open items are real backlog, not
resolved.

---

## Fixed (found via test-writing, each with a regression test)

### 1. Enemy could still fire (or throw) after being destroyed
**Labels:** `bug`, `severity:medium` · **Status:** Fixed & closed

Enemy.Start() schedules `Invoke("ActivateShooting", ...)`. Destruction()
never cancelled that pending Invoke, so an enemy killed before its scheduled
shot time could still have `ActivateShooting` fire on it afterward -
harmless if the shot-chance roll failed, but a `MissingReferenceException`
risk (`gameObject.transform.position` on a destroyed component) if it
didn't.
**Fix:** `Destruction()` now calls `CancelInvoke()` first.
**Regression test:** covered implicitly by `ShieldIntegrationTests` and
`BossIntegrationTests`' lethal-damage cases, which destroy enemies with a
pending Invoke scheduled and assert no error is logged.

### 2. Stale singleton references would break Level 2's transition
**Labels:** `bug`, `severity:high` · **Status:** Fixed & closed

`Player`, `PlayerMoving`, `PlayerShooting`, and `PoolingController` all set
`instance = this` in `Awake()` but never cleared it in `OnDestroy()`. Once a
second level starts (Level 1's `Player` etc. is destroyed when its scene
unloads), the new scene's `Awake()` guard (`if (instance == null)`) would
stay false forever, permanently orphaning `Player.instance` and breaking
every script that reads it for the rest of the game.
**Fix:** added `OnDestroy()` to all four classes, clearing `instance` when
`instance == this`.
**Regression test:** `PlayerTests.OnDestroy_LetsANewPlayerTakeOver_AsHappensOnALevelTransition`
simulates exactly this sequence and asserts the second Player takes over.

### 3. Enemy/Projectile could call GetDamage on a destroyed Player
**Labels:** `bug`, `severity:medium` · **Status:** Fixed & closed

`Enemy.OnTriggerEnter2D` and `Projectile.OnTriggerEnter2D` both called
`Player.instance.GetDamage(...)` with no null check - a
`MissingReferenceException` risk during the same stale-singleton window as
#2.
**Fix:** both now guard with `&& Player.instance != null`.
**Regression test:** `ProjectileTests.EnemyBullet_HittingAPlayerTaggedObjectWithNoLivePlayerInstance_DoesNotErrorOut`.

### 4. Wave kept spawning enemies after the player was gone
**Labels:** `bug`, `severity:low` · **Status:** Fixed & closed

`LevelController.CreateEnemyWave` checks `Player.instance != null` before
spawning; `Wave.CreateEnemyWave` (a different coroutine, same purpose) never
did, so a wave already in progress would keep instantiating enemies into an
empty level.
**Fix:** added the same guard, `yield break`ing once the player is gone.
**Regression test:** `WaveTests.CreateEnemyWave_StopsSpawning_OnceThePlayerIsGone`.

### 5. Enemy/Projectile/Bonus tags used by prefabs but never registered
**Labels:** `bug`, `severity:medium` · **Status:** Fixed & closed

`ProjectSettings/TagManager.asset` had zero custom tags registered, yet
`Enemy_straight_projectile.prefab`, both projectile prefabs, and
`Power Up.prefab` all carry `Enemy`/`Projectile`/`Bonus` in their serialized
`m_TagString`. Existing prefabs still compared correctly by raw string, but
the tags were unusable from the Editor Tag dropdown or from any new
script/prefab (`gameObject.tag = "Enemy"` throws `Tag: Enemy is not defined`
if unregistered) - which would have blocked the Level 2 shielded-enemy/boss
prefab authoring and my own test code.
**Fix:** registered `Enemy`, `Projectile`, `Bonus` in TagManager.

### 6. FollowThePath rebuilt its spline control points every frame
**Labels:** `performance`, `severity:low` · **Status:** Fixed & closed

`FollowThePath.Update()` called `Interpolate(CreatePoints(pathPositions), t)`
- rebuilding the padded Catmull-Rom control-point array from scratch on
*every enemy, every frame*, when the underlying path never changes after
`SetPath()`. Minor but real waste, more relevant on WebGL/mobile.
**Fix:** the padded array is now computed once in `SetPath()` and cached
(`SplineUtility.PadForCatmullRom`), reused every frame.

### 13. Level never actually transitioned from Level_1 to Level_2
**Labels:** `bug`, `severity:critical` · **Status:** Fixed & closed

The real bug behind "doesn't move from level 1 to 2", found only once you
actually played it: `LevelCompletionTracker.expectedDefeats` (180 for
Level_1) assumed every spawned enemy would eventually be counted, but
`Enemy.OnDestroyed` only fired from the manual `Destruction()` method - i.e.
only enemies the player actually **shot down**. Most enemies in a real
playthrough fly past and self-destruct naturally via `FollowThePath`
reaching the end of its route, which never touched `Destruction()` at all.
Reaching 180 *kills* specifically is essentially impossible in normal play,
so the level's completion count could never reach its target and the scene
transition never fired.
**Fix:** moved the `OnDestroyed` invocation into Unity's own `OnDestroy()`
lifecycle callback on `Enemy`, which fires for *any* destruction path (killed
or naturally despawned), not just the lethal-damage one.
**Regression test:** `LevelCompletionTrackerTests.OnLevelComplete_CountsEnemiesThatDespawnNaturally_NotJustKills`
destroys an enemy directly (bypassing `GetDamage`/`Destruction` entirely) and
asserts it still counts.

### 14. Powerup spawn threw a NullReferenceException after the player died
**Labels:** `bug`, `severity:high` · **Status:** Fixed & closed

`LevelController.PowerupBonusCreation()` referenced
`PlayerMoving.instance.borders.minX/maxX` directly, with no null check. Once
the player dies, `PlayerMoving.instance` is correctly cleared (bug #2's fix),
so the very next powerup timer tick threw.
**Fix:** the coroutine now skips a cycle (`continue`) if `PlayerMoving.instance`
is null instead of dereferencing it.
**Regression test:** `LevelControllerTests.PowerupBonusCreation_AfterThePlayerIsGone_DoesNotErrorOut`.

### 15. Boss movement looked robotic ("enemies move too weird")
**Labels:** `bug`, `severity:low` · **Status:** Fixed & closed

`BossMovement` snapped instantly to a brand-new direction's full-speed
vector the moment `CurrentDirection` changed, every 1-3 seconds - jarring
next to the smoothly path-following regular enemies.
**Fix:** velocity now eases toward the target direction via `Vector2.MoveTowards`
(new `turnAcceleration` field) instead of snapping to it.
**Regression test:** `BossIntegrationTests.BossMovement_RampsUpVelocityGradually_InsteadOfSnappingToFullSpeed`.

---

## Features added in response to first playtest feedback

Once the game was actually playable (Level_1/Level_2 + Shield/Boss all
wired up), direct playtesting surfaced the three bugs above plus requests
for a `GameHUD` (wave-number text, a boss health bar shown only while a
boss is alive, and win/lose overlays with a Restart button) - all delivered
test-first, see `GameHUDTests.cs`. One implementation note worth recording
here rather than as a "bug", since it was caught before ever shipping:
`GameHUD` must subscribe to `LevelController`'s wave/boss events in
**`Awake()`**, not `Start()` - a zero-delay first wave fires its event
synchronously from inside `LevelController.Start()` itself, and Unity does
not guarantee `Start()` ordering across different GameObjects the way it
guarantees all `Awake()` calls finish before any `Start()` begins. (Script
Execution Order was tried first and does **not** help here - it only
orders the Update-family callbacks, not Awake/Start.)

---

## Open backlog (not fixed - flagged for a deliberate scope decision)

### 7. PoolingController is fully implemented, tested, and completely unused
**Labels:** `enhancement`, `tech-debt`, `severity:low` · **Status:** Open

`PoolingController.GetPoolingObject` works correctly (covered by
`PoolingControllerTests`), but nothing in the codebase calls it - Enemy,
Projectile, Bonus, VFX, and Wave all spawn via raw `Instantiate`/`Destroy`.
On mobile/WebGL, this is avoidable GC churn during heavy wave/projectile
traffic. Rewiring the whole spawn pipeline to use pooling is a genuine
architectural change (touches Wave, PlayerShooting, Enemy's shot-firing, and
VFX spawn sites) and was judged out of scope for this pass rather than
rushed in - a good next-sprint ticket.

### 8. No keyboard input alternative to mouse/touch-drag
**Labels:** `accessibility`, `severity:medium` · **Status:** Open

See `ACCESSIBILITY_REPORT.md` #2. `PlayerMoving` only supports drag input;
there's no keyboard alternative on desktop.

### 9. No difficulty/assist option; any hit is instant death
**Labels:** `accessibility`, `enhancement`, `severity:low` · **Status:** Open

See `ACCESSIBILITY_REPORT.md` #6. A real feature addition, not a bug fix -
flagged rather than implemented here.

### 10. PlayerShooting weaponPower 2 plays the VFX on the wrong side
**Labels:** `bug`, `severity:trivial` · **Status:** Open

In `MakeAShot()`'s `case 2`, the shot from `rightGun` triggers
`leftGunVFX.Play()` and the shot from `leftGun` triggers `rightGunVFX.Play()`
- looks like a copy-paste swap. Purely cosmetic (muzzle flash on the visually
wrong gun for that one weapon level); not fixed here since it's a gameplay-feel
tweak I wasn't asked to make, not a functional defect.

### 11. Mobile (Android/iOS) never actually device/emulator tested
**Labels:** `testing`, `severity:medium` · **Status:** Open

See `CROSS_PLATFORM_REPORT.md`. No installed Editor module + no device/
emulator available in this environment; a manual test procedure is
documented for whoever has access to one.

### 12. Seizure-risk and colorblind-simulation checks not empirically run
**Labels:** `accessibility`, `testing`, `severity:low` · **Status:** Open

See `ACCESSIBILITY_REPORT.md` #4 and #8. Needs a manual pass with a
vision-deficiency emulator/tool against the actual WebGL build.
