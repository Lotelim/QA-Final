# CI/CD Setup

This project uses GitHub Actions (`.github/workflows/unity-ci.yml`) with the
[game-ci](https://game.ci/) actions to run automated tests and produce a WebGL build
on every push/PR to `main`/`master`.

## What the pipeline does

1. **test** (matrix: `EditMode`, `PlayMode`) — runs the full automated test suite
   headlessly via `game-ci/unity-test-runner`, publishes NUnit results as a GitHub
   check and as a downloadable artifact.
2. **build-webgl** (runs after `test` passes) — produces a WebGL build via
   `game-ci/unity-builder`, uploaded as a downloadable artifact.

Both jobs cache the `Library/` folder between runs (keyed on `Assets`/`Packages`/
`ProjectSettings` contents) so CI doesn't reimport the whole project every run.

## Required secrets (add these in the GitHub repo's Settings → Secrets and variables → Actions)

game-ci needs a Unity license to run the Editor headlessly. **I did not create or
touch these secrets — you need to add them yourself**, since they involve your own
Unity account credentials, which is not something I should handle:

| Secret | What it is |
|---|---|
| `UNITY_EMAIL` | The email for the Unity account used to activate the license |
| `UNITY_PASSWORD` | That account's password |
| `UNITY_LICENSE` | The contents of a `.ulf` Unity Personal license file |

To get `UNITY_LICENSE` for a free Unity Personal license, game-ci documents a
request/return-activation-file flow that doesn't require ever pasting your real
password into this repo or into me: run their
[`request-activation-file`](https://game.ci/docs/github/getting-started#personal-license)
step once locally (or via a throwaway workflow run), download the resulting
`.alf` file, manually activate it at https://license.unity3d.com/manual, and
commit the returned `.ulf` file's contents as the `UNITY_LICENSE` secret (never
commit the `.ulf` file itself to the repo).

## Editor version note

This CI targets whatever Unity version game-ci's Docker image defaults to for the
project's `ProjectSettings/ProjectVersion.txt` (currently `6000.2.15f1`, bumped
from the originally-pinned `6000.0.43f1` because that exact patch wasn't available
locally to run tests during development — see the `chore:` commit for why). If CI
fails to find a matching image, pin `unityVersion` explicitly on each action.

## Running tests locally without CI

```
Unity.exe -batchmode -nographics -projectPath "<path-to-project>" -runTests -testPlatform EditMode -testResults results-editmode.xml -logFile editmode.log
Unity.exe -batchmode -nographics -projectPath "<path-to-project>" -runTests -testPlatform PlayMode -testResults results-playmode.xml -logFile playmode.log
```

Note: `Assets/Tests/PlayMode/LevelSceneLoadTests.cs` loads the real `Level_1`/
`Level_2` scenes and is intentionally included in the normal PlayMode run (its
`TearDown` swaps back to a fresh empty scene afterward so it doesn't leak into
other fixtures) — no special test filter is required for local or CI runs.
