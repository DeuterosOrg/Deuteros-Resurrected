# Fractional Clock Implementation Plan

> **For agentic workers:** Use superpowers:executing-plans to implement this plan task-by-task. Preserve the active 48-task goal; this plan is one part of it.

**Goal:** Restore natural hundredth-day advancement with actual simulation consumption, saved timing and correct date-based gates.

**Architecture:** Keep the historical `SaveFile.CurrentDay` field as the consumed-update counter so existing training/travel timestamps remain meaningful. Add a saved `GameClock` containing displayed centidays, normal-mode elapsed seconds and one pending increment. Distinguish per-update consumers from date-gated consumers; neither a label-only change nor dispatching 100 updates for one manual day is correct.

**Tech Stack:** Existing Godot 4.2.2 .NET, C# net6.0, Newtonsoft.Json and regression runner; no new dependencies.

**Spec:** `docs/original-clock-evidence.md`, `docs/original-event-order-evidence.md`, `docs/original-ama-mining-evidence.md`, `docs/original-enemy-production-evidence.md` and Asana 1215716464570923.

## Global Constraints

- Preserve root `AGENTS.md`, existing saves/backups and the separate Windows `8cdd458` handoff. No push, PR or Asana writes.
- Normal-mode source arithmetic: 15,780 VBlanks; nominal PAL interval 315.6 seconds. This is not measured emulator timing.
- One consumed natural increment adds 1 centiday; a manual increment adds 100. Pending producer state prevents catch-up bursts.
- Training, production, research, movement, mining/refining phases and story countdowns consume updates. Attrition and enemy scheduling use dates; AMA uses original clock phase and stable ship slots.
- Preserve saved progress and distinguish legacy migration from new-game initialization. Reject malformed clock/slot data before activation.
- Work in `codex/fractional-clock`, worktree `artifacts/worktrees/hyperlight-discovery`; baseline `b21bfa6` has 474 audited cases. Run one gameplay/test process at a time.

## Review Focus

- Mixed natural/manual time must preserve the natural remainder and consume exactly one update, never 100.
- A stalled or blocked display may retain one pending update; it must not build a catch-up backlog.
- Loading legacy saves must preserve date, countdowns, deadlines, inventories and in-transit ships.
- Date boundaries must age staff/schedule enemies correctly even when the consumed-update counter differs from displayed days.
- Ship removal/reload must not change surviving AMA scan/mining phases; replay/bulletin locks must remain intact.

## Task 1: Saved producer and consumed-update integration

**Files:** Add `Godot/Code/Objects/GameClock.cs`; modify `Objects/GameData/SaveFile.cs`, `CoreData.cs`, `GameCore.cs`, `Utility/SaveStorage.cs`, `Platform/Screens/MainMenu.cs`, `SaveScreen.cs`, `ShipInterior.cs`, `Objects/News.cs`; test `Tests/RegressionRunner.Clock.cs` and register at the end of `RegressionRunner.Run`.

**Interfaces:** `GameClock.DateCentidays` (`ulong`), `NormalElapsed` (`double`), `PendingIncrement` (`int`, 0/1/100); `AdvanceNormal(double)` produces at most one update, `QueueManual()` preserves normal phase, `Consume()` returns bool and records `PreviousCentidays` for synchronous consumers. `PreviousCentidays` is not serialized. Date formatting uses invariant numeric formatting. `SaveFile.Clock` is initialized for new games; a missing legacy clock derives date from `CurrentDay * 100`.

- [x] Add and run case 475 against the real core: `.00` before 315.6s, `.01` at the threshold, then training actually starts. Observed red: date remains `.00`.
- [x] Add timing, pending/stall, mixed-mode, save/reload, legacy and malformed-state cases before their implementations.
- [x] Implement the small clock model and integrate the real process/update/display/save paths; preserve independent SDM wall-clock processing.
- [x] Audit UI date fixtures: setting an update counter is no longer setting a date. Preserve their original assertions with explicit date setup rather than weakening them.
- [x] Run focused clock and save/navigation/training/bulletin checks; inspect native date/News/ETA/slot rendering and commit.

## Task 2: Date gates and stable AMA scheduling

**Files:** `Objects/StaffAttrition.cs`, `Platform/EnemyDroneBuilder.cs`, `Objects/Ship.cs`, `Objects/ShipModule.cs`, `Platform/Screens/ModuleScenes/AMA.cs`, `Objects/ACC.cs`, `Utility/SaveStorage.cs` and relevant existing regression partials.

**Interfaces:** Consume Task 1's displayed centidays and previous date. Keep per-update `CurrentDay` timestamps for travel/repair/training; migrate enemy build deadlines to exact centidays. Stable IOS/SCG allocation must survive save/load/removal and use original phase masks from the linked AMA evidence.

- [x] Reproduce attrition at a true 100-day crossing with a different update count, exact 950-unit enemy intervals, and AMA phase drift/missing natural scheduling.
- [x] Implement original date gates and slot-based scheduling; trace any still-unmapped eligibility before changing it.
- [x] Cover manual/natural crossings, peaceful/hostile transitions, allocation reuse, old saves, full/incompatible cargo and removal. Commit only after focused headless/native checks pass.

## Task 3: Whole-path acceptance and contribution integration

**Files:** Existing validation scripts and `docs/{validation-results,windows-agent-brief,backlog-progress,original-clock-evidence}.md`.

- [x] Review the full diff against the source evidence, migration rules and all five review-focus conditions; obtain the skill's final independent review and reproduce/fix substantive findings.
- [x] Run `python3 -m unittest discover -s scripts -p 'test_*.py'` and `python3 -u scripts/validate.py --godot /tmp/deuteros-godot-isolated --export-windows` from the stable worktree with the toolchain environment sourced.
- [x] Audit every case log and the exported package; run relevant native/physical checks. Keep failed attempts separate from accepted results.
- [x] Integrate locally and update the Windows brief with the exact revision and acceptance steps. Original-runtime comparison and Windows execution remain required for full task acceptance.

## Remaining whole-goal work

Per-star/SCG clocks, Hyperlight routes/Warlord, rogue crews and ending are separate campaign implementation work, not removed from the 48-task goal by this plan. Do not claim this clock task fully accepted merely because its new tests pass. No changes to destructive interstellar-arrival behavior belong in this plan without completing that route integration and migration.
