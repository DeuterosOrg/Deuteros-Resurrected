# Autonomous Contribution Goal

## Objective

Resolve all 48 tasks in the [Asana snapshot](asana-triage.md), including bug fixes, missing features and research deliverables, and prepare a substantive, tested contribution for team review. Preserve the full scope: implementation alone does not establish completion.

Use the [backlog ledger](backlog-progress.md) as the task-by-task record. Verify current files, revisions and external results before acting on older notes.

## Independent Work Priorities

After reboot, first investigate the preserved case 264 audio-teardown failure at `artifacts/validation/evidence/full-run-929b4a5-failed/`; do not treat the interrupted 632-case run as passing. Then resume the campaign priorities below.

1. Resume normal campaign from `artifacts/funded-capture-campaign/jupiter-reloaded-slot-4.json` (day 15603, SHA-256 `de673bd73d905d1dc871bed0c5625401ed7f788aa101da5150bf3715b5345528`). Jupiter is captured with SDM disarmed; WAYFARER is docked there with 24 drones, five crew, healthy engine and 232 fuel. Earth/Moon remain friendly after two successful defences. AOC has produced 162 drones and its queue is empty; orbital materials are depleted. Restore mining/freight and fleet supply as needed, repair the captive colony and continue Sol captures toward SCG discovery. Preserve the failed SDM outcomes: `929b4a5` fixes the reproduced mandatory-bulletin countdown defect, and successful capture is a replay of the unchanged post-battle checkpoint. Twenty-seven focused checks and normal capture/reload pass; the full 632-case run failed during case 264 audio teardown; cases 1–263 passed strictly. The last complete aggregate remains 630 at `003c087`.
2. Continue protected-save normal campaign testing through SCG discovery, interstellar travel and later progression. Keep ordinary campaign evidence separate from staged regression fixtures.
3. Resolve remaining original-game fidelity gaps, including staff attrition, News events, construction artwork and timing. Cleared-station/active-SDM danger now shares the original-backed docking exemptions, with reproduced failures and 102 focused passes (cases 631–632 added). Continue normal station-capture acceptance; the latest complete aggregate remains 630 at `003c087`. The ACC lamp now has indexed-pixel and palette-cycle source evidence; original-runtime mode/dimming comparison remains pending before animation acceptance. Keep audio work deferred behind gameplay priorities.
4. Run the complete current regression suite, tooling checks, startup and export audit at meaningful integration checkpoints. Verify relevant pointer interactions and save/reload behavior.
5. Reconcile Windows results and patches, consolidate the contribution branch, and keep the [team update](team-update.md) ready to share.

## Autonomous Working Rules

- Continue authorized investigation, minimal fixes, tests, documentation and local commits without routine confirmation. When one task is blocked, advance another independent task.
- Reuse existing implementations and tests. Reproduce failures before changing behavior; retain failing and passing evidence.
- Coordinate directly with the Windows agent using the established mesh thread and SSH Downloads handoff. Credit existing tests at their actual revisions and request only missing coverage.
- Windows v5 is reconciled at `01c81f9`; the newer `003c087` export is delivered after full Mac validation. Its isolated Windows smoke script was rejected by the Windows execution policy before launch; retain that failure and await an owner-approved test path without bypassing it. Collect the missing matching-export acceptance; preserve the agent's profile and saved day-1157 checkpoint, and do not repeat completed tests. The owner closed PID62688 with native access violation `0xC0000005`; collect its retained logs through builder Downloads and investigate source/export shutdown without assuming the earlier PagedAllocator cause.
- Protect original saves, retain reproducible checkpoints, restore originals exactly, and close only owned processes. Do not change permissions or bypass rejected actions.
- Keep work on `codex/build-tests-and-gameplay-fixes`, using isolated local branches when useful. Do not merge into `main`. Prepare reviewable changes before requesting any still-required approval for pushing, PR creation or external status updates.
- Preserve the existing root `AGENTS.md`. Do not write memory files or send team announcements without authorization.

## Reporting and Completion

While actively working, provide an hourly update with implementation and accepted-task counts, new findings, validation results, blockers and next actions. Update the ledger when evidence changes; do not count overlapping Windows scenarios as additional completed tasks.

Completion requires evidence against every task's actual requirements, relevant Windows/source/export acceptance, and documented resolution of remaining scope questions. A green suite or prepared branch is insufficient by itself.

The Codex goal controller remains authoritative for automatic execution. If it reports a usage limit, preserve this brief and current evidence; resume through Codex rather than creating a replacement goal to bypass the limit.
