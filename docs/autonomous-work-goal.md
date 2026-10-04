# Autonomous Contribution Goal

## Objective

Resolve all 48 tasks in the [Asana snapshot](asana-triage.md), including bug fixes, missing features and research deliverables, and prepare a substantive, tested contribution for team review. Preserve the full scope: implementation alone does not establish completion.

Use the [backlog ledger](backlog-progress.md) as the task-by-task record. Verify current files, revisions and external results before acting on older notes.

## Independent Work Priorities

1. Continue from `artifacts/aoc-fleet-campaign/reloaded-slot-4.json` (day 12714). Normal AOC production paid for 58 drones and one DFCC; successful Moon defence leaves 46 player drones/five crew, 23 fleeing enemy drones and attack threshold 80. Exact stock, battle and reload audits pass. Hostile Sol stations hold 92 defenders each: the shared original-backed docking gate now passes 77 focused checks (cases 628–629 added). Combat-window ownership now passes 13 focused checks and native cases 628–630. Full 630-case Mac integration, 19 tooling checks and the package audit pass at runtime `003c087`. Continue station capture and SCG discovery. Preserve the earlier failed defence and missed-departure outcomes separately; do not stage resources or claim capture acceptance from defensive victory.
2. Continue protected-save normal campaign testing through SCG discovery, interstellar travel and later progression. Keep ordinary campaign evidence separate from staged regression fixtures.
3. Resolve remaining original-game fidelity gaps, including staff attrition, News events, construction artwork and timing. Check cleared-station/active-SDM danger accumulation against the newly traced docking exemptions. The ACC lamp now has indexed-pixel and palette-cycle source evidence; original-runtime mode/dimming comparison remains pending before animation acceptance. Keep audio work deferred behind gameplay priorities.
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
