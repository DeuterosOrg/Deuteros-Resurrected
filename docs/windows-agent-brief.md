# Windows agent brief

## Mission and handoff

Craig's goal is to resolve **all 48 open Asana tasks**, including missing features and original-game research questions. The first contribution supplies build tooling, regression tests and a batch of fixes. It does not resolve the entire backlog.

After Craig pushes `codex/build-tests-and-gameplay-fixes`, work from that branch. First establish the native Windows baseline, verify the existing fixes in the exported game, then continue the backlog in focused, tested changes. Inspect the working tree before switching branches; preserve existing work. Coordinate file ownership if another agent is still working on this branch.

**Windows Codex starting instruction:** “Read `docs/windows-agent-brief.md`, record the checked-out commit, and complete its Windows validation and backlog workflow.” This file supplies the handoff; pushing it does not itself launch an agent. Complete native Windows testing before recommending a team-facing PR.

Read these files first:

- Root `AGENTS.md` — preserve it. Its original testing/export description predates this contribution; use the current commands below and in the README.
- [README](../README.md), [CONTRIBUTING](../CONTRIBUTING.md), and [testing details](testing.md).
- [Validation results](validation-results.md) — verified work and unresolved runtime failures.
- [Asana reconciliation](asana-triage.md) — all 48 task IDs, source locations and acceptance questions.
- [Backlog progress](backlog-progress.md) — current implementation and verification status; coordinate active items before editing.

Keep work local until Craig authorizes publication. Do not push, open/merge PRs, or change/comment on Asana tasks merely because a test passes.

## Start here on Windows

The implementation handoff is commit **`c7b3968`** on **`codex/build-tests-and-gameplay-fixes`**. This documentation may arrive in a later commit; record the actual checked-out SHA. If the branch has advanced, reconcile its changes with the progress ledger before using the counts below.

1. Inspect local changes, fetch the branch from the configured remote, and check out its latest pushed state without discarding local work. Preserve root `AGENTS.md`.
2. Run the pinned setup and full validation below. Investigate case 150 first if it stalls; retain failure evidence before any diagnostic rerun.
3. Complete the source-game and exported-game acceptance passes. Record each scenario as passed, failed or not tested, with the commit and reproduction steps.
4. Write `docs/windows-validation-results.md`, update task evidence in the local ledger, then continue unresolved backlog items. Keep Windows validation and subsequent gameplay changes in separate commits.

The next investigated backlog item is **1215683087492485 — Add MTX module**. Discovery and transfers exist, but the player-built installation path needs investigation. No installation fix is included in this handoff. Trace production, stores and MTX behavior against [original-game evidence](original-behavior-evidence.md) before choosing installation semantics; also check transfers when a destination is captured or loses its station. Coordinate ownership before editing if Mac work has resumed.

Pushing this branch makes the brief available to the Windows agent; it does not start Codex or automatically run the validation workflow. The current CI push trigger targets `develop`; PR and manual workflow triggers are separate.

## Current evidence

The source baseline was `9817216`. The initial macOS arm64 contribution passed compilation, asset import, **26 isolated engine regression cases**, startup smoke and Windows cross-export from a fresh source copy. The suite is growing as backlog work continues; consult the progress ledger and latest validation results for subsequent batches. Five Python validator tests passed. A clean compile still reports 14 pre-existing warnings. Linux/Windows CI is configured but has not run remotely at this handoff.

The last fully passing local run on **2026-10-02 covered 153 isolated cases**, startup smoke and Windows cross-export. The current suite has **162 cases** and **24 Asana-linked fixes with local regression evidence**. Its latest full attempt timed out at case 150 after 180 seconds while Research entered the tree; cases 1–149 passed. A separate continuation passed cases 151–162, startup smoke and a fresh Windows cross-export. Five Python validator tests passed. This is **161/162 across two runs, not a passing aggregate**. Windows gameplay and visual acceptance remain pending.

Commit `4cac288` previously contained 123 cases and 18 Asana-linked fixes with local regression evidence. That batch's full run failed at case 65 on invalid GC-handle errors despite passing assertions; its other 122 cases passed across the attempt and continuation. Case 65 passed in the latest full run, but no proven fix exists for the earlier intermittent failure. Preserve both results when assessing readiness.

### Salvage and bulletin batch — 2026-10-02

The News/bulletin, OF pilot-warning and unknown-object batch expanded the runner to **153 cases** and the ledger to **21 Asana-linked fixes with local regression evidence** at that point. See [batch evidence](validation-results.md#news-of-pilot-warning-and-alien-technology-batch--2026-10-02) for the latest aggregate result and exact limits; none of the 48 tasks is declared fully accepted.

- Cases 124–128 cover News history/replay and bulletin input-lock ownership. Broader News report coverage remains unspecified.
- Cases 129–134 cover OF pilot warnings, state preservation and cleanup. Compare the native layout and pointer behavior with the Asana reference.
- Cases 135–149 and 151 cover unknown-object capture, research discovery, cargo/artifact conservation, crew/weight limits, gift handling and idempotent unlocks.
- Case 150 follows the comms gift through normal research, production, fitting and trade. It passed targeted validation but has also intermittently stalled while Research enters the tree; no proven fix exists for that stall. Removing Research audio did not eliminate it. Case 152 verifies removal of a detached Research placeholder button; this is a separate confirmed leak fix.
- Case 153 follows the actual war-warning Fusion Laser gift through analysis and normal research to drone technology. Prior trades, travel, staff and equipped ships are staged fixtures; narrative text is shortened. Repeat the full gameplay path natively.

Earlier typing-audio leaks were reproduced in a standalone engine probe. The test helper now waits for stopped playback to drain on the mixer before immediate process exit, with a bounded wait and strict error checks. This does not certify native audio/navigation shutdown. Rebuild before investigating any case, never use a stale assembly after compilation fails, and preserve failed logs even when a diagnostic rerun passes.

The initial batch addressed twelve gameplay/UI defects, including AMA equipment duplication, reversed initial IOS ACC endpoints and consecutive grapple unloading. The latter three correspond to open Asana reports. Use the linked results for the full list; do not report all 48 as fixed.

Raw Mac logs and binaries are under ignored `artifacts/` and will not arrive with a Git checkout. Regenerate evidence on Windows.

## Windows setup and automated validation

Use **Godot 4.2.1 .NET**, **.NET SDK 6.0.428 x64**, and **Python 3.9+**. Install the SDK from Microsoft's official .NET 6 download page if it is missing. `global.json` pins the SDK; the standard, non-.NET Godot build is unsuitable.

Run in PowerShell from the repository root:

```powershell
$ErrorActionPreference = "Stop"
git status --short
git branch --show-current
git rev-parse HEAD
dotnet --list-sdks
dotnet --version
python --version
python scripts/install_godot.py --templates
if ($LASTEXITCODE -ne 0) { throw "Godot installation failed" }
$env:GODOT = (Resolve-Path ".tools/godot-4.2.1/Godot_v4.2.1-stable_mono_win64/Godot_v4.2.1-stable_mono_win64_console.exe").Path
& $env:GODOT --version
python -m unittest discover -s scripts -p "test_*.py"
if ($LASTEXITCODE -ne 0) { throw "Validator tests failed" }

# Preserve previous evidence and exports before starting this attempt.
$evidenceDir = Join-Path "artifacts/windows-validation" ([guid]::NewGuid().ToString())
New-Item -ItemType Directory -Path $evidenceDir -Force | Out-Null
if (Test-Path "artifacts/validation") {
    Move-Item "artifacts/validation" (Join-Path $evidenceDir "previous-validation")
}
if (Test-Path "artifacts/windows") {
    Move-Item "artifacts/windows" (Join-Path $evidenceDir "previous-export")
}
python scripts/validate.py --export-windows
if ($LASTEXITCODE -ne 0) { throw "Game validation failed; preserve and inspect logs" }
```

Check each command's exit status and stop to investigate failures. The installer verifies official release checksums. If .NET is installed outside its standard location, set `DOTNET_ROOT` and add that directory to `PATH` before launching Godot.

The validator discovers cases from the C# runner and starts a fresh Godot process for each. Record the discovered count and compare it with the latest validation results; do not hard-code the initial 26-case count. Require a passing aggregate, clean startup and successful export. Read `artifacts/validation/`, including warnings; a process exit of zero alone is insufficient. Test resources are excluded from the exported game.

Archive each attempt before another run: the validator overwrites logs for stages it reaches, but leaves later-stage logs from older runs. An old executable or smoke log must not be attributed to a failed new run. Run interactive export acceptance only after a successful export of the recorded commit.

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

## Additional acceptance for ACC, settings and interiors

- Select the same mineral on both ACC endpoints. Check equal/unequal stocks, multiple pods, odd totals and several round trips. Fill stores near 50,000 and confirm surplus cargo remains aboard without loss. Repeat on shuttle ground/orbit and IOS routes. Activation already passes baseline tests; reproduce the original activation report before closing it.
- Keep the interior open through takeoff, landing, transit and arrival. Check menu destinations, ship-name header and locks. Open an SCG with five occupied pods; all cargo rows must render. Moon/unsupported-colour icons currently use existing neutral artwork.
- Rename each hull through the name label. Check blank/long names, Enter, Cancel, Escape, clipping/full tooltip and save/load. Verify actual pointer hit areas and dialog layout.
- Check time animation while Earth, Overview, News, Save and Store are selected. Exercise toggle, hold/release, pointer exit and automatic stops without resetting the selected-screen indicator.
- Change sound/volume, window scale and fullscreen; reopen settings and restart the executable to check persistence. Restore defaults. Confirm/cancel progression presets and repeat them without duplicate world state; verify the resumed view updates. These shortcuts are not normal progression evidence. Original-bug compatibility toggles are not specified or implemented.
- Use [media inventory](media-gap-inventory.md) for missing sound/animation sourcing and wiring; original audiovisual fidelity remains unverified.

## Additional acceptance for station displays

- Station overview: compare local orbital production with ground production, check completion without reopening, and follow shuttle takeoff/landing/removal. Check zero and nonzero deployed derricks; rigs in stores must not count. Labels describe the current simulation; only idle wording is confirmed by the original screenshot.
- Production: verify the full AOC plaque replaces staff labels and the removal control during idle and active automation, including the day an AOC finishes. Check manual production remains intact. The plaque uses the exact task reference; its static light does not certify original animation timing.
- Deposit analysis: select planets, return to system view, select moons and change systems. A body's own built/incomplete station should appear below the selection, disappear after loss and remain hidden in course selection. Compare placement with the linked [visual references](visual-reference-notes.md).

Cases 154–162 cover these transitions. Native Mac captures passed and corrected one clipped shuttle label. See [batch results](validation-results.md#station-status-and-missing-graphics-batch--2026-10-02) for screenshot reproduction commands and fixture limits. Repeat in the Windows source game and export.

## Known failures to investigate

- Latest full-run case 150 again stalled at Research entry after the gift was analysed. A Mac process sample found the main thread waiting on a native recursive mutex and the .NET finalizer waiting on a managed lock. No root cause or fix is proven. Reproduce the normal comms progression and retain diagnostics; do not disable the case or treat the successful 153-case batch as current aggregate validation.

- On Mac, the audio/navigation path can leak Ogg resources or hang with `!rc_owner`. An isolated Godot 4.3 comparison also failed; an engine upgrade alone is not a demonstrated fix.
- Earlier combined regression runs and an isolated case 65 logged `SwapGCHandleForType: Handle is not initialized` (also `SetGodotObjectPtr`). Case 65 cycles course/ACC/grapple/AMA panels and checks modal destruction. It passed in the latest full attempt without a proven fix; treat the earlier strict failure as unresolved. Fresh-process isolation does not remove this production resource-lifetime risk. Reproduce with `$env:DEUTEROS_TEST_CASE = "65"; & $env:GODOT --headless --path Godot res://Tests/Regression.tscn`, then clear the selector with `Remove-Item Env:DEUTEROS_TEST_CASE`. Preserve failed logs even when a later run passes; add phase/GC-count diagnostics before changing resource ownership.
- Godot 4.2.1's binary scene conversion leaked instances during fresh exports. `export/convert_text_resources_to_binary=false` avoids that path; retain it unless a tested replacement removes the need.
- The validator permits one exact documented `_EDITOR_GET` teardown diagnostic only in editor import/export stages. Do not broaden exclusions, suppress game errors or retry until green without investigating.

For the latest batch, also exercise News replay/cancellation with sound enabled; OF deployment with missing, empty and valid crews; occupied grapples receiving gifts; and the complete comms progression without unlock cheats. Compare the OF warning with the source linked in [visual reference notes](visual-reference-notes.md). If typing-audio leaks recur, preserve them separately from the intermittent Research-entry stall, case 65 and the older navigation shutdown failure; a shared cause has not been established.

## Continue the 48-task goal

For each task: read its description/comments/attachments, establish expected behavior, reproduce the gap, add a failing regression, implement a focused fix, and verify the affected screen or progression. Track **unstarted / investigating / implemented / verified / evidence-blocked** separately. A missing feature or ambiguous research question needs acceptance criteria, not a superficial patch.

Check whether this Windows session has authenticated Asana access to project `1214891399253076` in workspace `507237966097081`; Mac credentials/connectors do not automatically transfer. If unavailable, work from the committed 48-task snapshot and record which live comments or attachments need access. Keep task status updates in the local ledger until Asana writes are authorized.

Use Craig's `WizzoUK2/deuteros-parallel` research where available. Read its `DIVERGENCES.md` and latest `docs/m2-findings.md` addenda before adopting behavior. The Mac's local checkout path is not portable; obtain the repository through existing authorized access. Distinguish decoded facts and emulator observations from provisional formulas. Raise concrete missing evidence while continuing independent tasks.

## Deliverable

Create `docs/windows-validation-results.md` with the tested commit/dirty state, Windows/GPU details, tool versions, commands, case counts, source/export results, manual scenarios and exact unresolved failures. Include Asana IDs and evidence for every newly verified task. Store raw logs/screenshots under `artifacts/windows-validation/`; keep binaries and generated caches out of commits.

Conclude with what is verified, what remains, and whether the contribution is ready for team review. Do not equate an exported executable or a passing headless suite with a completed Windows gameplay test.

Prepare the eventual PR summary around player-visible fixes, linked Asana IDs and Windows evidence. Group changes into reviewable batches and list unresolved items explicitly; documentation and test infrastructure support that contribution. Leave publication to Craig's next instruction.
