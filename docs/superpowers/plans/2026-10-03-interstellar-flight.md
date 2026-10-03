# Interstellar Flight Implementation Plan

> **For agentic workers:** Use superpowers:executing-plans inline. Obtain one fresh-context whole-branch review at the end; no per-task agents.

**Goal:** Replace instant cross-star SCG arrivals with saved original-backed travel, clock-aware arrival and Hyperlight-specific Warlord promotion.

**Architecture:** Keep existing body-to-body routes, compose the star leg with a local arrival leg, and persist only changing flight state. Derive star clocks from the saved Sol clock and initial offsets; retain the legacy path only for journeys already underway without new flight state.

**Tech Stack:** Existing Godot 4.2.2 .NET, C# net6.0, Newtonsoft.Json and real-engine regression runner; no new dependencies.

**Spec:** `docs/superpowers/specs/2026-10-03-interstellar-flight-design.md`.

## Global Constraints

- Baseline `2f2e131` contains the audited493 runtime `e369433`; preserve root `AGENTS.md` and the independent Windows8cdd458 handoff.
- Work in `codex/interstellar-flight` using the existing isolated `artifacts/worktrees/hyperlight-discovery` checkout. Only one Godot/game/test process at a time.
- No push, PR, remote merge or Asana writes. Preserve saved inventories, crews and in-progress legacy journeys.
- Source and calculated traces are not emulator observations. Do not substitute generic battle promotion or guaranteed safe arrival for the traced rules.

## Review Focus

- New route state must not reinterpret an old flight or promote/destroy a ship during loading.
- Changing time mode, blocking a pending increment or reloading must not desynchronize private and star clocks.
- A fuel-loss/clock-mismatch result must remove/report once and release UI/ACC ownership without crediting destination stores.
- Research completion and drive damage near a phase boundary must affect the correct next update, with no generic double fuel charge.
- Existing local routes, cancelled map selections, complete-cycle semantics and staff experience must survive the new path.

## Task 1: Reproduce and implement saved interstellar progress

**Files:** `Objects/SCG.cs`, new `Objects/InterstellarFlight.cs`, `Objects/GameClock.cs`, `Objects/Ship.cs`, `Platform/Screens/ShipInterior.cs`, `Utility/SaveStorage.cs`, new `Tests/RegressionRunner.InterstellarFlight.cs`, `Tests/RegressionRunner.cs`.

**Interfaces:** `SCG.Flight` is nullable saved `InterstellarFlight`; null with existing transit denotes the legacy journey. The flight owns leg, remaining distance, phase, fraction and signed private-clock offset. `SCG.TravelTimeRemain()` supplies the remaining projected consumed updates; new-departure initialization and shared update paths own mutations, never display getters.

- [x] Add case494 to the real updater: ready SCG at Mercury, destination Atlantic, completed Hyperlight, fuel250. Engage, advance one update, assert it remains in transit at its origin. Run before production edits; expected failure is immediate arrival.
- [x] Add source-table/phase/fuel/clock regressions, each failing against the current path. Pin Mercury→Atlantic interstellar distance4300, phase1 first step2, next phase2 cost2, and no generic extra fuel charge.
- [x] Implement the saved flight and route table using the source formulas in the spec. Add strict save validation and legacy-null fallback together with the model. Test a save/reload after each phase change and malformed values before activation.
- [x] Integrate new departure/update handling once in the shared ship path. Preserve ordinary local/legacy transit, handle underflow and mismatched arrival as committed loss, and defer ACC arrival until the final body leg.
- [x] Run targeted494+ and existing236–247/277–304/314–329/475–493 checks. Expected: all pass with no engine errors; commit the working model/integration together.

## Task 2: Hyperlight arrival, rank and UI integration

**Files:** `Objects/Staff.cs`, `Enums.cs`, `Utility/SaveStorage.cs`, `Platform/Screens/ShipInterior.cs`, `Platform/Screens/MainMenu.cs`, `Objects/News.cs`, the interstellar regression partial and relevant rank/clock fixtures.

**Interfaces:** Consume `SCG.Flight` and its actual arrival result. Promotion occurs only at the Hyperlight arrival boundary, never when research is discovered or a route is selected. `Staff.GetLevel()`/`GetLevelString()` remain the public rank APIs.

- [x] Reproduce missing Warlord through a complete Hyperlight route with an Admiral; pin one saved rank report, retained actions, no promotion for Captain/ordinary arrival and no repeat promotion after reload.
- [x] Persist the rank milestone, validate it only on eligible staff, and audit every rank consumer before updating display/combat behavior. Keep ordinary action thresholds unchanged.
- [x] Reproduce stale/misleading flight ETA/date or route controls through actual scenes. Show the private flight clock and remaining leg correctly without mutating state from getters. Preserve cancellation and ACC complete-cycle behavior.
- [x] Cover exact/insufficient fuel, damage, research completed mid-flight, blocked natural updates, mixed modes, losses while viewing the ship, and saves before Hyperlight arrival. Expected: source-backed outcomes, conserved inventories and no duplicate notifications.
- [x] Run native UI/rank/loss/save cases, inspect screenshots and commit after focused compatibility passes.

## Task 3: Complete-path verification and local integration

**Files:** `docs/{validation-results,backlog-progress,windows-agent-brief,save-files,original-interstellar-travel-evidence,original-warlord-evidence}.md`.

- [ ] Review the entire branch against the spec and the five review-focus conditions. Dispatch the executing-plans skill's one final reviewer; reproduce/fix Important/Critical findings once and record declined judgments.
- [ ] Run nine Python tests and full `scripts/validate.py --godot /tmp/deuteros-godot-isolated --export-windows` with the toolchain sourced. Expected: all discovered fresh-process cases, strict import, source smoke and export pass.
- [ ] Independently audit current manifest/log freshness, package assets and exclusion of tests. Record native/physical evidence separately from staged fixtures and Windows acceptance.
- [ ] Integrate the verified runtime locally, verify runtime-tree identity, update the exact Windows candidate/acceptance brief and preserve failure evidence. Keep total48 acceptance claims tied to task evidence, not test count.
