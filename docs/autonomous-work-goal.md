# Autonomous Contribution Goal

## Objective

Resolve all 48 tasks in the [Asana snapshot](asana-triage.md), including bug fixes, missing features and research deliverables, and prepare a substantive, tested contribution for team review. Preserve the full scope: implementation alone does not establish completion.

Use the [backlog ledger](backlog-progress.md) as the task-by-task record. Verify current files, revisions and external results before acting on older notes.

## Independent Work Priorities

1. Resume normal campaign from `artifacts/prewar-reinforcement-campaign/reloaded-slot-3.json` (day 14508, SHA-256 `054195c8ed899c7fe977faaa57c8792df289eff5e84154e2c926176e8cf6ba49`). Normal mining/freight now funds 160 drones plus DFCC; neither is manufactured yet. Both Earth ships have empty supply pods and ACC stopped, WAYFARER has five crew/healthy engine/232 fuel, Comms and two spare drives are stored, and AOC is installed. Restore Comms in the AMA mount, trigger the normal Jupiter war/prototype encounter, depart promptly, research/build the fleet, then attempt station capture/SDM and SCG discovery. Preserve the separate day 12935 wartime fork (56 stored drones/500-palladium delivery/transmission stage 4) and the earlier successful Moon defence; do not combine their resources or claim capture from defence. Full 630 validation remains at `003c087`; the later danger correction has 102 focused passes at `d0ace3d`.
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
