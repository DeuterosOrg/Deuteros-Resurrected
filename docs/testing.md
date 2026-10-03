# Build and regression validation

## Reproducible baseline

Use Godot **4.2.2 .NET** and .NET SDK **6.0.428**. The project targets `net6.0` for desktop; `global.json` pins the SDK. Installers and export templates must match the engine version. The 4.2.2 patch fixes a reproduced C# script creation/finalizer deadlock; see [engine evidence](validation-results.md#godot-422-script-lifetime-fix--2026-10-02). Python 3.9+ runs the repository scripts without third-party Python packages.

```sh
python3 scripts/install_godot.py
python3 scripts/validate.py --godot /path/printed/by/installer
```

On Windows, use `python`. `GODOT` is an alternative to `--godot`. For a custom SDK installation, set `DOTNET_ROOT` and include it in `PATH`.

## What the command proves

1. `dotnet build Godot/Deuteros.csproj` restores dependencies and compiles the actual game and tests.
2. For a fresh cache, an initial editor pass creates imported resources; its diagnostics are retained in `import-bootstrap.log`. Godot may report the global font/theme missing before importing them. A separate strict editor pass then verifies the populated cache and waits for its filesystem scan to finish. The small `validation_import` editor plugin exits only when `DEUTEROS_IMPORT_ONLY=1`; it is inactive during normal editing.
3. The runner discovers cases from `Tests/Regression.tscn`, then launches each in a fresh Godot process with the real `Screens/Master.tscn`. This isolates singleton state, day-event subscriptions and native resources. Assertions exercise the actual simulation and scenes. The suite checks MTX stock conservation, AOC charging/replenishment, factory destination, ship removal, and UI behavior.
4. `Tests/Smoke.gd` starts the configured game scene, advances 120 frames, and explicitly exits successfully.

Gameplay regressions exit through `GameCore.RequestQuit`, the same cleanup used by a normal window-close request. It frees the active scenes, gives the audio mixer a bounded opportunity to release stopped playback, then collects and drains managed finalizers while native bindings are still alive. Case 276 reproduces pending resource finalizers at that boundary; see [the shutdown diagnosis](shutdown-finalizer-evidence.md). Case 205 exercises the close notification after repeated Store/Ship Bay/Training navigation with settings paused. Logs must still be free of engine errors; passing assertions before a shutdown hang are a failed run. The entry-scene smoke uses direct `SceneTree.Quit` and does not establish audio shutdown behavior.

A logged engine error, native crash/fatal header, failed assertion, missing completion marker, nonzero exit, or timeout fails validation. See `artifacts/validation/*.log` for full output. To iterate after imports are current, use `--skip-import`.

Per-case logs are named `regression-NNN.log`; `regression-summary.log` is written only after every discovered case passes. Running the regression scene directly without a selector retains batch mode for diagnostics; it is not the canonical isolated suite.

The test runner invokes subsystem updates directly. It does not establish the original game's ordering of the complete daily event pipeline. Screen tests execute real callbacks and nodes, but do not prove pointer hit-testing, sound quality or animation timing. There is no measured line-coverage percentage or claim of complete game coverage.

## Injected pointer event lifetime

Let Godot retain mouse events passed to `Viewport.PushInput`; do not wrap them in `using` or dispose them when the helper returns. An unhandled event can remain queued until physics picking, after the synchronous input callback and render frames. Premature disposal reproduced the case-61 GC-handle errors. Case 264 repeats real navigation with collection pressure to protect this boundary; see the [diagnosis](validation-results.md#queued-test-input-lifetime--2026-10-02). Direct synchronous callbacks have a different ownership boundary.

## Engine-specific behavior

Godot 4.2.1 predates `--import`; unknown command-line flags are ignored. Its `--quit-after` also returns 1 in our minimal macOS reproduction, so the smoke and regression runners call `quit(0)` explicitly on success.

The .NET editor can emit this diagnostic during shutdown after a completed import:

```text
ERROR: Condition "!EditorSettings::get_singleton() || !EditorSettings::get_singleton()->has_setting(p_setting)" is true. Returning: Variant()
```

The validator reports and permits exactly that line in editor import/export stages only. It remains in the saved log. Other errors in the strict import/export passes, and all game/test errors, fail validation. The bootstrap pass must finish successfully but does not judge resource diagnostics before the cache exists; it can never replace the strict pass. Expected headless mouse/cursor warnings are retained and do not fail tests. The validation script's own tests protect these distinctions:

```sh
python3 -m unittest discover -s scripts -p 'test_*.py'
```

## Export and CI

`python3 scripts/install_godot.py --templates` downloads and verifies the matching .NET templates. `python3 scripts/validate.py --export-windows` adds a release export to `artifacts/windows/`. Keep the complete output directory together. Regression scenes, test C# classes and the import plugin are excluded from release exports. Text scenes/resources remain unconverted: [Godot 4.2.1's exporter](https://github.com/godotengine/godot/blob/4.2.1-stable/editor/export/editor_export_platform.cpp#L754-L788) instantiates scenes during binary conversion without freeing them. Disabling this optional conversion avoids fresh-export leaks. On macOS, missing `rcedit` may produce a Windows metadata/icon warning; it does not prevent compilation or packing.

`.github/workflows/validate.yml` runs on pull requests and pushes to `develop`, with Linux and Windows jobs. Both run compilation, asset import, regressions and startup. Windows also validates release export. The expanded isolated suite has a 45-minute job budget, including toolchain setup; individual command timeouts still apply. The previous 589-case local build-to-export run took 16.54 minutes before clean-runner downloads, leaving little headroom under the old 20-minute cap. This budget change has not been exercised on hosted CI. Logs are uploaded even on failure; the Windows output is a workflow artifact, not an automatically published release. The existing Discord workflow remains separate.

The canonical startup smoke covers the entry scene only. Consult [validation results](validation-results.md) for the latest complete Mac checkpoint and retained failures, and [Windows results](windows-validation-results.md) for the separately audited native checkpoint. Historical case counts do not describe the current discovery manifest. Passing the suite does not certify every scene lifetime, desktop interaction or normal campaign path.

Before release, run the exported Windows build on Windows, exercise new-game progression and affected screens, and record findings. A macOS cross-export cannot certify Windows rendering or input.

The expanded suite also checks active-world day updates, versioned save round trips and screen actions, production/store selection, bay hover, right-click modal precedence and HeD ACC cycling. Pointer hover tests use a SubViewport because native headless mouse-over tracks the OS pointer independently of injected events. The production recipe fixture excludes background audio; audio/navigation shutdown remains a separate manual acceptance scenario, not a suppressed test error.

## Native Windows export dependency

On Windows, `scripts/install_godot.py --templates` also installs pinned RCEdit 2.0.0 beside Godot. `validate.py` adds the Godot directory to the child-process PATH so icon/version resource modification works when using an absolute `--godot` path or `GODOT`. Missing RCEdit failed the first native export despite 309 passing regressions; see [the Windows baseline](windows-validation-results.md). Download/cached-binary integrity checks are included in the Python test suite. Preserve failed exports and verify the resulting executable, including its embedded pack, before reporting Windows readiness.
