# Contributing

## Choose and reproduce work

Use [Deuteros Development in Asana](https://app.asana.com/1/507237966097081/project/1214891399253076) as the bug tracker. Link the task from your branch/PR; avoid opening a duplicate GitHub issue for the same report. Check recent commits and [the backlog assessment](docs/asana-triage.md) before implementing a feature already partly present.

Record the build revision, platform, starting game state, exact actions, expected result and actual result. For visual bugs, attach a screenshot or short recording. For original-game fidelity questions, cite a decoded routine/table or reproducible emulator observation. Label assumptions explicitly; behavior in another remake is supporting evidence, not proof.

## Make a focused change

Branch from `develop`. Keep bug fixes, tooling changes and engine upgrades separable. Match nearby C# formatting rather than reformatting unrelated files. Keep scene node names and paths synchronized with their controllers, and preserve asset filename case for Linux.

Write a regression that fails on the original behavior, then make the smallest correct fix. Prefer the real domain model or scene over mocks. Useful assertions cover resource conservation, inventory charges, navigation state and day-update completion. Do not alter research costs or timing merely to make a test pass.

## Validate

Run `python3 scripts/validate.py` with the .NET-enabled Godot executable available as `godot` (or supply `--godot`). Run `python3 -m unittest discover -s scripts -p 'test_*.py'` when changing validation tooling. See [testing.md](docs/testing.md).

Manually exercise changed screens, including pointer interaction and relevant transitions; headless tests do not verify animation, audio or hit areas. Include the commands, results and any untested platforms in the PR.

## Submit for review

Use a short, action-oriented commit summary, such as `Fix MTX balancing to preserve total stock`. Describe the user-visible problem, the fix and the validation evidence. Link the Asana task and include screenshots for visual changes. Explain any deliberate behavior change or remaining uncertainty.

Keep generated `.godot/`, `.tools/`, build output and local logs out of commits. Do not commit personal configuration, credentials or original-game disk/memory dumps. Use generated fixtures or documented local references where possible. Do not mark an Asana task complete solely because code has been written; check its acceptance behavior on the relevant build first.
