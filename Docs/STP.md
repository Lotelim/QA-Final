# Software Test Plan (STP)

## Space Shooter — QA Final Project

Document version 1.0 — prepared as the QA/test-engineering deliverable for the Space Shooter mini project (Unity, built on the "Space Shooter Template FREE" asset). This plan follows the spirit of IEEE 829, scoped to a small single-developer project rather than a large formal test organization.

## 1. Introduction

### 1.1 Purpose

This document defines the scope, approach, resources, and schedule of testing activities for the Space Shooter project. It covers both the pre-existing gameplay code and the features added for this QA pass: Shield defense, a Boss enemy, and a second, harder level with a working level-completion/transition flow.

### 1.2 Scope

Testing covers: player movement and shooting, enemy behavior (including the new Shield component), the new Boss ship, level-completion and scene-transition logic, object pooling, projectile/collision routing, the boundary cleanup system, and the two playable levels (Level_1, Level_2) built for this project. It covers functional correctness (automated + manual), non-functional aspects (performance/pooling, cross-platform behavior), and accessibility.

### 1.3 References

- `Assets/Space Shooter Template FREE/Scripts/*.cs` — system under test
- `Assets/Tests/EditMode`, `Assets/Tests/PlayMode` — automated test suite (79 tests at time of writing)
- `Docs/STD.md` — detailed test case specification (companion to this plan)
- `Docs/BUG_TRACKING.md` — defects found during this test cycle
- `Docs/CROSS_PLATFORM_REPORT.md`, `Docs/ACCESSIBILITY_REPORT.md` — specialized test reports
- `.github/workflows/unity-ci.yml`, `Docs/CI_SETUP.md` — continuous integration pipeline

## 2. Test Items

| Item | Description | Test Type |
|---|---|---|
| Player movement & shooting | PlayerMoving, PlayerShooting, Bonus power-up | Automated |
| Enemy & Shield | Enemy health/damage/shooting, new Shield absorb-damage component | Automated |
| Boss | BossMovement direction pattern, huge-HP Enemy composition, Boss marker | Automated |
| Level completion & flow | LevelCompletionTracker, LevelFlow, LevelController boss spawn | Automated |
| Level_1 / Level_2 content | Generated scenes and prefabs (shielded wave, boss, harder timings) | Automated (scene-load) + Manual |
| Pooling | PoolingController object reuse | Automated |
| Projectile/collision routing | Projectile, Boundary, tag-based hit detection | Automated |
| Background/VFX lifetime | RepeatingBackground, VisualEffect | Automated |
| WebGL build | Full build + in-browser smoke test | Manual (executed once this cycle) |
| Mobile (Android/iOS) | Touch input, performance, screen sizes | Manual (documented, not yet executed) |
| Accessibility | Input modality, color reliance, motion/flashing, difficulty options | Manual (checklist) |

## 3. Features to Be Tested

- All gameplay scripts under `Assets/Space Shooter Template FREE/Scripts` (14 original + Shield, Boss, BossMovement, BossMovementPattern, LevelCompletionTracker, LevelFlow, SplineUtility)
- Level_1 baseline difficulty and its transition into Level_2 on completion
- Level_2's harder timings, shielded-enemy wave, and boss encounter
- CI pipeline correctness (tests + WebGL build both succeed on a clean checkout)

## 4. Features Not to Be Tested

- Third-party asset content shipped with the "Space Shooter Template FREE" package that this project does not modify (sprite art, particle system visual tuning, audio assets) — visual/audio polish is out of scope for a QA-only pass.
- Any UI/menu/HUD system — the project has none; adding one was judged out of scope (see `Docs/ACCESSIBILITY_REPORT.md` for the accessibility implications of that decision).
- Real Unity Editor GUI regression testing (opening the Editor and clicking through Inspectors by hand) — all functional coverage here is via automated EditMode/PlayMode tests and the built player, not manual Editor use.

## 5. Approach

### 5.1 Automated testing

Unity Test Framework (NUnit-based), split into:

- **EditMode tests**: pure logic with no MonoBehaviour lifecycle dependency (spline math, Shield's damage-absorption math, Boss direction-selection math).
- **PlayMode tests**: anything depending on Awake/Start/Update, coroutines, physics triggers, or scene loading — the majority of the suite, since most of this codebase is MonoBehaviour-driven.

Both suites run headlessly via `Unity.exe -batchmode -runTests` locally and in CI (game-ci/unity-test-runner). At the time of writing: 34 EditMode tests and 45 PlayMode tests, all passing.

### 5.2 Manual testing

Reserved for what automation cannot practically cover in this environment: real multi-browser WebGL compatibility, real mobile device/touch feel, visual/VFX correctness, and accessibility checks that need a human judgment call (see `Docs/CROSS_PLATFORM_REPORT.md` and `Docs/ACCESSIBILITY_REPORT.md`). The STD lists these as explicit manual test cases with steps and expected results.

### 5.3 Test-driven development

Shield, Boss, and level-completion/transition were developed test-first: the contract (e.g. "a shield absorbs damage up to its points, then lets the overflow through") was written as a failing test before the corresponding production code, then implementation followed until green. Where the same Unity batchmode round-trip would have been too slow to run after every single micro-change, verification was batched at logical checkpoints (per-feature, then whole-suite) rather than after each individual edit; the tests themselves remain the executable specification regardless of when they were last run.

## 6. Item Pass/Fail Criteria

- **Automated tests**: pass = the test asserts the documented expected behavior and Unity Test Framework reports it green with no unexpected logged errors/exceptions (an unexpected error/exception logged during a test is itself a failure, per Unity Test Framework's default behavior).
- **Manual tests**: pass = every step in the STD's manual test case produces the documented expected result; any deviation is logged as a defect (see `Docs/BUG_TRACKING.md`) with severity assigned per the scale in Section 2 of the STD.
- **Build/CI**: pass = `Unity.exe -runTests` exits 0 for both EditMode and PlayMode, and the WebGL build (`BuildPipeline.BuildPlayer`) reports `BuildResult.Succeeded` with 0 errors.

## 7. Entry and Exit Criteria

### 7.1 Entry criteria

- Code compiles with no errors in both the runtime and test assemblies.
- Test environment (Section 8) is available.

### 7.2 Exit criteria

- 100% of automated EditMode/PlayMode tests pass.
- No open defect above severity "medium" (per `Docs/BUG_TRACKING.md`'s scale) remains unresolved without an explicit, documented scope decision to leave it as backlog.
- Level_1 and Level_2 both load and are wired correctly (verified by the scene-load smoke tests).
- WebGL build succeeds and passes its in-browser smoke test.
- STP and STD are complete and consistent with the actual test suite.

## 8. Test Environment

| Component | Detail |
|---|---|
| Engine | Unity 6000.2.x (project originally pinned to 6000.0.43f1; bumped to run tests locally — see `Docs/CI_SETUP.md`) |
| Test framework | Unity Test Framework / NUnit, via `Assets/Tests/EditMode` and `Assets/Tests/PlayMode` |
| CI | GitHub Actions, game-ci/unity-test-runner + unity-builder (`.github/workflows/unity-ci.yml`) |
| Automated execution platform | Unity Editor, batchmode, headless (`-batchmode -nographics`) |
| Manual execution platforms | WebGL (in-browser, executed); Android/iOS (documented, needs a device/emulator) |
| Source control | Git (local repository; GitHub Issues planned for bug tracking once a remote exists) |

## 9. Test Deliverables

- This Software Test Plan (`STP.md`)
- Software Test Description (`STD.md`) — full automated + manual test case matrix
- Automated test source (`Assets/Tests/EditMode`, `Assets/Tests/PlayMode`)
- `Docs/BUG_TRACKING.md` — defect log
- `Docs/CROSS_PLATFORM_REPORT.md`, `Docs/ACCESSIBILITY_REPORT.md` — specialized reports
- `.github/workflows/unity-ci.yml`, `Docs/CI_SETUP.md` — CI/CD pipeline and its setup instructions
- The WebGL build artifact (`Builds/WebGL`, gitignored — reproducible via Tools > QA Final > Build WebGL)

## 10. Schedule / Milestones

Given this is a single-cycle QA pass rather than a multi-sprint project, milestones are treated as a dependency-ordered checklist rather than calendar dates:

1. Test infrastructure (assembly definitions) in place
2. Regression coverage for all pre-existing scripts, with any discovered defects fixed
3. Shield, Boss, and level-completion features delivered test-first
4. Level_1/Level_2 content authored and verified via scene-load tests
5. CI/CD pipeline delivered
6. WebGL build produced and smoke-tested; mobile documented for manual execution
7. Accessibility pass completed
8. STP/STD finalized

## 11. Roles and Responsibilities

Single-developer project for a QA course: one person (with AI pair-programming assistance) acted as developer, test engineer, and release engineer for this cycle. In a team setting, the natural split would be: developer (feature code), QA engineer (test design/execution, this STP/STD), and a release engineer (CI/CD, builds) — noted here for completeness since the STP format expects a roles section, not because separate people filled these roles this cycle.

## 12. Risks and Contingencies

| Risk | Impact | Mitigation |
|---|---|---|
| No installed Unity Editor matches the pinned version | Tests/builds could silently behave differently on a different patch version | Documented explicitly (`Docs/CI_SETUP.md`); CI can pin an exact version once decided |
| No WebGL+mobile module combo available locally | Can't produce a real mobile build in this environment | WebGL executed for real; mobile documented as a manual procedure, clearly labeled as not yet executed |
| CI requires a paid/activated Unity license | Pipeline won't actually run until secrets are added | Documented exactly what's needed and how to obtain a free Personal license activation file, without ever handling real credentials |

## 13. Approval

This STP is a working document for a course project; no formal sign-off workflow is in place. Treat it as approved for the purposes of this deliverable once you've reviewed it.
