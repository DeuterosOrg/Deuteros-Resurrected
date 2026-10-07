# Autonomous Contribution Goal

## Objective

Resolve all 48 tasks in the [Asana snapshot](asana-triage.md), including bug fixes, missing features and research deliverables, and prepare a substantive, tested contribution for team review. Preserve the full scope: implementation alone does not establish completion.

Use the [backlog ledger](backlog-progress.md) as the task-by-task record. Verify current files, revisions and external results before acting on older notes.

## Independent Work Priorities

The full current **632-case Mac aggregate**, 19 tooling checks, strict import/startup and Windows package audit now pass at `64df06f` (unchanged runtime `929b4a5`). The export is byte-identical to the package already delivered to Windows. The preserved case-264 audio-teardown failure and separate Windows crash remain unresolved; a new passing run does not establish their cause or resolution. See [complete checkpoint](validation-results.md#632-case-complete-integration-checkpoint--2026-10-07). Continue campaign work and targeted diagnosis without discarding failures.

1. Continue pre-war replacement-reserve funding from `artifacts/fleet-reserve-campaign-20261007/reloaded-slot-1.json` (day 15901, SHA-256 `faee387508e4cd2350b485463af98da01f25758564f98b84138d9b82564f52e2`). Normal freight and another 1,000 platinum now fund **200 IOS drones plus one DFCC**; the fleet is not manufactured. Earth orbit has 24,549 iron, 24,786 titanium, 24,694 aluminium, 7,186 carbon, 13,175 copper, 6,250 palladium, 6,045 platinum, 90 gold and 5,872 MeH. WAYFARER is docked with five crew, healthy engine, 232 fuel, AMA and two empty supply pods. FIRST LIGHT is docked with 37 crew, 55 fuel and an empty supply pod; its current freight selection is iron only. Both ACCs are stopped; the miner retains both rare metals at the asteroid endpoint. Each fleet carries at most 200 drones, so further materials are for replacement reserves or another hull. Build those reserves before replaying war and Sol captures; the prior 162-drone campaign exhausted its reinforcements after Jupiter. Inspect News whenever time stops unexpectedly. The repaired-Jupiter checkpoint and failed resupply branch remain intact; see [new funding audit](native-gameplay-results.md#full-fleet-funding-and-concurrent-freight--2026-10-07) and [previous branches](native-gameplay-results.md#resupply-failure-and-separate-pre-war-funding--2026-10-07). Preserve earlier SDM expiry and case-264 teardown failures; the latest complete aggregate is 632 at `64df06f`.
2. Continue protected-save normal campaign testing through SCG discovery, interstellar travel and later progression. Keep ordinary campaign evidence separate from staged regression fixtures.
3. Resolve remaining original-game fidelity gaps, including staff attrition, News events, construction artwork and timing. Cleared-station/active-SDM danger now shares the original-backed docking exemptions, with reproduced failures and 102 focused passes (cases 631–632 added). Continue normal station-capture acceptance; the latest complete aggregate is 632 at `64df06f`. The ACC lamp now has indexed-pixel/palette-cycle source evidence and a measured 0.4-second pulse in original recorded Engage, Complete Cycle and Disengaged states. Modal dimming, implementation and Windows acceptance remain pending; see [indicator evidence](original-acc-indicator-evidence.md#original-recording-measurement--2026-10-07). Keep audio work deferred behind gameplay priorities.
4. Run the complete current regression suite, tooling checks, startup and export audit at meaningful integration checkpoints. Verify relevant pointer interactions and save/reload behavior.
5. Reconcile Windows results and patches, consolidate the contribution branch, and keep the [team update](team-update.md) ready to share.

## Autonomous Working Rules

- Continue authorized investigation, minimal fixes, tests, documentation and local commits without routine confirmation. When one task is blocked, advance another independent task.
- Reuse existing implementations and tests. Reproduce failures before changing behavior; retain failing and passing evidence.
- Coordinate directly with the Windows agent using the established mesh thread and SSH Downloads handoff. Credit existing tests at their actual revisions and request only missing coverage.
- Windows v5 is reconciled at `01c81f9`. The latest delivered source bundle is `3c58744`, runtime/export `929b4a5`; its continuation and package audit pass with the case-264 failure retained. The latest complete Mac aggregate is 632 at `64df06f`; the byte-identical Windows package has no new native acceptance. The older `003c087` isolated Windows smoke script was rejected by the Windows execution policy before launch; retain that failure and await an owner-approved test path without bypassing it. Collect the missing matching-export acceptance; preserve the agent's profile and saved day-1157 checkpoint, and do not repeat completed tests. The owner closed PID62688 with native access violation `0xC0000005`; its retained log/exit/PID and sanitized callback analysis are now collected. Investigate source/export shutdown without assuming the earlier PagedAllocator cause; no separate engine log was found.
- Protect original saves, retain reproducible checkpoints, restore originals exactly, and close only owned processes. Do not change permissions or bypass rejected actions.
- Keep work on `codex/build-tests-and-gameplay-fixes`, using isolated local branches when useful. On 7 October, Craig authorized committing and pushing this contribution branch to the main repository, now `DeuterosOrg/Deuteros-Resurrected`. Do not merge into the default branch (`develop`) or `main`; PR creation and external status updates remain outside this authorization.
- Preserve the existing root `AGENTS.md`. Do not write memory files or send team announcements without authorization.

## Reporting and Completion

While actively working, provide an hourly update with implementation and accepted-task counts, new findings, validation results, blockers and next actions. Update the ledger when evidence changes; do not count overlapping Windows scenarios as additional completed tasks.

Completion requires evidence against every task's actual requirements, relevant Windows/source/export acceptance, and documented resolution of remaining scope questions. A green suite or prepared branch is insufficient by itself.

The Codex goal controller remains authoritative for automatic execution. If it reports a usage limit, preserve this brief and current evidence; resume through Codex rather than creating a replacement goal to bypass the limit.
