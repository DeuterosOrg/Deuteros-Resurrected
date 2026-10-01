# Windows agent brief

## Mission and handoff

Craig's goal is to resolve **all 48 open Asana tasks**, including missing features and original-game research questions. The first contribution supplies build tooling, regression tests and a batch of fixes. It does not resolve the entire backlog.

After Craig pushes `codex/build-tests-and-gameplay-fixes`, work from that branch. First establish the native Windows baseline, verify the existing fixes in the exported game, then continue the backlog in focused, tested changes. Inspect the working tree before switching branches; preserve existing work. Coordinate file ownership if another agent is still working on this branch.

Read these files first:

- Root `AGENTS.md` — preserve it. Its original testing/export description predates this contribution; use the current commands below and in the README.
- [README](../README.md), [CONTRIBUTING](../CONTRIBUTING.md), and [testing details](testing.md).
- [Validation results](validation-results.md) — verified work and unresolved runtime failures.
- [Asana reconciliation](asana-triage.md) — all 48 task IDs, source locations and acceptance questions.
- [Backlog progress](backlog-progress.md) — current implementation and verification status; coordinate active items before editing.

Keep work local until Craig authorizes publication. Do not push, open/merge PRs, or change/comment on Asana tasks merely because a test passes.

## Current evidence

The source baseline was `9817216`. The initial macOS arm64 contribution passed compilation, asset import, **26 isolated engine regression cases**, startup smoke and Windows cross-export from a fresh source copy. The suite is growing as backlog work continues; consult the progress ledger and latest validation results for subsequent batches. Five Python validator tests passed. A clean compile still reports 14 pre-existing warnings. Linux/Windows CI is configured but has not run remotely at this handoff.

The latest full local run on **2026-10-02 passed 75 cases**, startup smoke and Windows export. Thirteen Asana-linked fixes now have local regression evidence; Windows/visual acceptance remains pending.

The initial batch addressed twelve gameplay/UI defects, including AMA equipment duplication, reversed initial IOS ACC endpoints and consecutive grapple unloading. The latter three correspond to open Asana reports. Use the linked results for the full list; do not report all 48 as fixed.

Raw Mac logs and binaries are under ignored `artifacts/` and will not arrive with a Git checkout. Regenerate evidence on Windows.

## Windows setup and automated validation

Use **Godot 4.2.1 .NET**, **.NET SDK 6.0.428 x64**, and **Python 3.9+**. Install the SDK from Microsoft's official .NET 6 download page if it is missing. `global.json` pins the SDK; the standard, non-.NET Godot build is unsuitable.

Run in PowerShell from the repository root:

```powershell
git status --short
git branch --show-current
git rev-parse HEAD
dotnet --list-sdks
dotnet --version
python --version
python scripts/install_godot.py --templates
$env:GODOT = (Resolve-Path ".tools/godot-4.2.1/Godot_v4.2.1-stable_mono_win64/Godot_v4.2.1-stable_mono_win64_console.exe").Path
& $env:GODOT --version
python -m unittest discover -s scripts -p "test_*.py"
python scripts/validate.py --export-windows
```

Check each command's exit status and stop to investigate failures. The installer verifies official release checksums. If .NET is installed outside its standard location, set `DOTNET_ROOT` and add that directory to `PATH` before launching Godot.

The validator discovers cases from the C# runner and starts a fresh Godot process for each. Record the discovered count and compare it with the latest validation results; do not hard-code the initial 26-case count. Require a passing aggregate, clean startup and successful export. Read `artifacts/validation/`, including warnings; a process exit of zero alone is insufficient. Test resources are excluded from the exported game.

## Interactive acceptance pass

Run both the source game and the Windows export:

```powershell
& $env:GODOT --path Godot
& .\artifacts\windows\Deuteros.exe
```

1. Check title, Earth, training, research, production, stores, ship bay, overview and Escape/settings screens. Exercise actual pointer hit areas, right-click navigation, font rendering and sound.
2. Exercise the existing fixes: depleted/replenished AOC production; ground/orbit output; MTX stock conservation; new/replacement ship ACC ownership; unavailable menu entries; course cancellation; SCG drone transfers and attacked icon; AMA removal; IOS ACC labels/loading; both grapple pods unloading without reopening the bay.
3. Verify bay entry at the cockpit and when returning to saved pod/engine positions. Dismantle equipped ships with exactly sufficient capacity; verify every returned quantity. Repeat with full staff/store capacity, confirm nothing is lost, clear space and retry. Unload held grapple salvage through its normal discovery path before dismantling.
4. Check roster and cryopod colours at native scale. Exercise station/ship overview hover text, travel/location updates and DFCC counts at zero and a populated fleet; check updates after arrival, transfer and removal.
5. Repeat **start → Earth ground → training → Escape/settings → close** at least three times with sound enabled. Confirm the process exits and inspect the game log, not just the disappearing window. Record hangs, exceptions and retained-resource messages.
6. Development shortcuts can establish a targeted fixture, but do not use them as evidence that normal research/unlock progression works. Record exactly which shortcuts were used.

The startup smoke only exercises the entry scene. It does not cover the above audio/navigation shutdown path. Save/load now has local regression coverage; verify it natively using the steps below and [save-file notes](save-files.md).

## Additional acceptance for the save/navigation batch

- Save a game with active training/research/production, equipped ships, cargo and ACC routes. Advance and change it, then load it and check the original quantities, progress, crew ranks and news. Advance again; old worlds must not keep running. Reopen the exported game and load the same slot.
- Cancel overwrite/load, then confirm each. Check the previous save's `.bak`, corrupt-file feedback, and a write-denied directory without losing the existing slot. Record Windows filesystem results.
- Select a production item on ground and in orbit, then open stores through the menu. Check matching highlight, local recipe capacity and return from equipment/MTX views.
- Right-click actual bay/store controls, then modal windows and timed grapple unloading. Verify dismissal/locks take precedence. Check bay hover labels through the physical mouse, including repeating fuel buttons and roster rows.
- Select HeD fuel in both ACC endpoints, including depleted inventories. Verify cycling finishes, loads available HeD and survives saving/loading.

## Known failures to investigate

- On Mac, the audio/navigation path can leak Ogg resources or hang with `!rc_owner`. An isolated Godot 4.3 comparison also failed; an engine upgrade alone is not a demonstrated fix.
- Combined regression runs intermittently logged `SwapGCHandleForType: Handle is not initialized`. Fresh-process isolation removes retained test worlds and pending deletion between cases; it does not prove a production resource-lifetime fix.
- Godot 4.2.1's binary scene conversion leaked instances during fresh exports. `export/convert_text_resources_to_binary=false` avoids that path; retain it unless a tested replacement removes the need.
- The validator permits one exact documented `_EDITOR_GET` teardown diagnostic only in editor import/export stages. Do not broaden exclusions, suppress game errors or retry until green without investigating.

## Continue the 48-task goal

For each task: read its description/comments/attachments, establish expected behavior, reproduce the gap, add a failing regression, implement a focused fix, and verify the affected screen or progression. Track **unstarted / investigating / implemented / verified / evidence-blocked** separately. A missing feature or ambiguous research question needs acceptance criteria, not a superficial patch.

Use Craig's `WizzoUK2/deuteros-parallel` research where available. Read its `DIVERGENCES.md` and latest `docs/m2-findings.md` addenda before adopting behavior. The Mac's local checkout path is not portable; obtain the repository through existing authorized access. Distinguish decoded facts and emulator observations from provisional formulas. Raise concrete missing evidence while continuing independent tasks.

## Deliverable

Create `docs/windows-validation-results.md` with the tested commit/dirty state, Windows/GPU details, tool versions, commands, case counts, source/export results, manual scenarios and exact unresolved failures. Include Asana IDs and evidence for every newly verified task. Store raw logs/screenshots under `artifacts/windows-validation/`; keep binaries and generated caches out of commits.

Conclude with what is verified, what remains, and whether the contribution is ready for team review. Do not equate an exported executable or a passing headless suite with a completed Windows gameplay test.
