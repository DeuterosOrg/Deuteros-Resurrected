# Rogue Crew Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans inline, with one fresh-context whole-branch reviewer after implementation. No per-task agents.

**Goal:** Complete the source-backed rogue SCG takeover, raid, sabotage and recoverable prison loop, with saved identity and actual scene controls.

**Architecture:** Add one saved controller model; reuse Staff, ShipModule.StaffStored, SCG flight, existing inventories, bulletins and SDM/loss paths. Keep all mutations behind shared model methods; wire the simulation only after the controller and containment paths are implemented together.

**Tech Stack:** Godot4.2.2 .NET, C# net6.0, Newtonsoft.Json, current real-engine runner; no new dependencies.

**Spec:** `docs/superpowers/specs/2026-10-03-rogue-crew-design.md`.

## Global Constraints

- Baseline `f801b76` has full523 Mac evidence plus focused524. Preserve root AGENTS.md, existing saves and the independent Windows8cdd458 handoff.
- Reuse clean `artifacts/worktrees/hyperlight-discovery` on isolated `codex/rogue-crew`; one Godot/game/test process at a time.
- No push, PR, remote merge or Asana writes. Keep this branch unintegrated until the complete loop passes review and validation.
- Raw traces establish instructions, not original-runtime acceptance. Preserve explicit composed-route, larger-allocation, empty-prison and interrupted-dialog adaptations from the spec.

## Review Focus

- A detached/duplicated selected crew or mismatched Pirate rank must fail save validation before replacing the active world.
- A command retained from an old view/world must not refit a travelling hull, steal from its old station or reactivate ACC.
- A captured prisoner and temporarily divided crew must survive save/reload, scene exit, full-roster release and casualty paths without duplication/loss.
- Occupied docking must reach sabotage while successful hostile docking must avoid the ordinary player SDM; unrelated victims retain existing behavior.
- Research discovery, competing bulletins and natural/manual time must not skip/repeat Mutiny, Prison discovery, random rolls or resource mutations.

## Task 1: Saved rogue identity and original selection

**Files:** new `Godot/Code/Objects/RogueCrew.cs`; `Objects/GameData/SaveFile.cs`, `Objects/Staff.cs`, `Enums.cs`, `Utility/SaveStorage.cs`, `CoreData.cs`; new `Godot/Tests/RegressionRunner.RogueCrew.cs`, registration in `RegressionRunner.cs`.

**Interfaces:** `SaveFile.RogueCrew` is a nonnull saved model after legacy initialization. Persist `Occurred:bool`, `Crew:Staff`, `Stage:int`, `MutinyPending:bool`, `PrisonCountdown:int`, `PrisonDivider:int`, `OriginalCrewCount:int?`. Add `Staff.Pirate:bool`, enum marine rank5. Model exposes `bool Controls(IShip ship)`, `bool Contained(SaveFile save)`, `void TryStart(SaveFile save)`, `void CrewLost(Staff crew)`, and `void Validate(SaveFile save)` (throws InvalidDataException). No global simulation hook yet.

- [x] Add failing cases for missing rank/selection/save behavior. Use existing `NewInterstellarRoute()`/`Save`, promote an Admiral through the actual Hyperlight route, stage mixed station ownership and call TryStart. Assert `Occurred`, selected reference, BOUNTY, stage11, Pirate5, ACC inactive and unchanged cargo. Pin `TryStart` idempotence.
- [x] Cover each gate independently: five enemy systems versus four, undiscovered research with initial progress1, discovered partial progress, first mixed star without a qualifying ship, exact249/250fuel, occupied/empty cryo, Blaser exclusion, permitted DFCC/PTL, stable ship slot order and old save absence. Example core check:
```csharp
state.TryStart(Save);
Equal(true, state.Occurred, "one qualifying mutiny");
Equal(true, ReferenceEquals(ship.Pilot, state.Crew), "selected identity");
Equal(5, ship.Pilot.GetLevel(), "Pirate rank");
Equal(11, state.Stage, "initial wait stage");
```
- [x] Implement state, selection and rank without enabling gameplay. Default missing legacy fields explicitly through ModelContract/Deserialize, preserve references, and validate phase ranges/location uniqueness. Count physical pilot/module/roster positions, excluding the controller's tracking reference and synthetic enemy crews; reject factory/research/ordinary-cryo locations for the selected rogue.
- [x] Roundtrip with real Serialize/Deserialize, then corrupt Stage, divider, temporary count, Crew reference and rank. Assert failure leaves the active Save reference unchanged. Retain observed compile/API absence separately from semantic failures; run new cases plus existing Warlord/save cases.
- [x] Commit the model/selection and verified tests on the isolated branch; do not wire it or integrate it independently.

## Task 2: Full controller, raids and shared simulation boundaries

**Files:** RogueCrew model; `Objects/Ship.cs`, `Objects/InterstellarFlight.cs` only if needed for composed routing, `Platform/Screens/ShipInterior.cs`, `Objects/SdmSystem.cs`, `Objects/News.cs`, `Objects/EnemyFleets.cs`, `Objects/ACC.cs`; rogue regression partial.

**Interfaces:** add `void Advance(SaveFile save, Func<int> random)`, `bool DockingBlocked(SaveFile save, IShip ship)`, `void EscapeStation(SaveFile save, IPlanet planet)`, `bool RejectCommand(SaveFile save, IShip ship)`. Stage handling is a switch over original1–20. Resolve the currently controlled SCG from Crew identity; never store a raw pointer or set MethanoidOwned.

- [x] Add deterministic failing model cases for all stages and transitions described in the spec. Routes consume actual `Ship.EngageEngine`, shared ship updates and saved Flight, then reach docking/refit and a human raid. Verify one transition per controller call, failed travel/absent targets waiting safely, one raid per intended stage and no ACC delivery on rogue arrival.
- [x] Implement route choice by original star/stable station allocation order, composed cross-star/body travel, hostile refit, six-resource cargo loading and HeD refueling. Test a station with600Titanium/200Aluminium/17HeD and mixed supply/tool mounts, checking exact before/after stock and overwritten supply cargo. Refit preserves damaged engine and rejects stored nonselected crew loss.
- [x] Implement original MTX assignment: source MTX required, first eight Solar station slots, human owner/Aluminium>=200, candidate MTX not required. Set existing Target/TargetType and send/balance mask; test assignment separately from the existing transfer eligibility guard.
- [x] Reproduce occupied docking and implement its shared transition to controller16/undocked. Deterministic sabotage test:
```csharp
state.Advance(Save, () => 4); // stage18 advances and hits once
Equal(19, state.Stage, "roll is consumed");
Equal(150, planet.Station.SdmCountdown, "pirate SDM");
state = SaveStorage.Deserialize(SaveStorage.Serialize(Save)).RogueCrew;
```
Also test miss0, saved stage17/18/19, original count restoration, zero crew and repeated calls. Invoke actual docking update rather than only assigning stage16.
- [x] Add rogue exemptions to shared danger/damage and enemy SDM arming; implement SDM escape before casualty enumeration. Keep ordinary casualties/attack behavior green. Clear selected identity through all existing crew loss routes, including occupied prison crew; never clear Occurred.
- [x] Run focused new stage/raid/loss cases and existing SDM, crew-loss, DFCC, interstellar/ACC cases; commit verified controller/lifecycle code without enabling global selection yet.

## Task 3: Crew recovery, prison controls and narrative integration

**Files:** `Platform/Screens/ShipBay.cs`, `Platform/Screens/ShipBayScenes/Torso.cs` as needed, `Platform/StaffList.cs` only if existing interaction cannot distinguish prison selection, `Platform/Screens/ShipInterior.cs`, `Objects/StaffAttrition.cs`, `GameCore.cs`, `Objects/RogueCrew.cs`, save validation and regression partial.

**Interfaces:** add model `bool TryTransferPilot(SaveFile save, IShip ship, Resource resource, Staff selected)`, `bool TryCapture(SaveFile save, ShipModule prison, Resource resource, Staff selected)`, `bool TryRelease(SaveFile save, ShipModule prison, Resource resource)`, `bool PublishMutiny(SaveFile save)`, `bool AdvancePrisonDiscovery(SaveFile save)`. Reuse ordinary crew transfers; validate active ship/module/resource ownership at the shared mutation boundary.

- [x] Reproduce rogue pilot recovery/hijack and ordinary cryopod rejection with real bay roster callbacks. Permit selected rogue assignment only to SCG, preserve full roster counts and the displaced pilot; hijack swaps the same slot. A free rogue resumes stage10, a contained one never dispatches.
- [x] Reuse the existing torso staff list for prison capture/release and leave empty-prison equipment selection reachable. Implement the nominal0.5-second capture window with right-click retain, timeout release and explicit panel/hover guidance. Keep the selected world/module identity in the callback; cancel on scene exit/replacement. A full roster retains the prisoner. Test timeout, right-click, repeated input, scene exit, replacement world, full roster and saved prisoner through actual controls.
- [x] Reject occupied prison equipment removal/replacement/pod change/dismantle before mutation, and exclude stored prisoner from attrition. Report it through the shared crew-loss path. Test exact stock/pod/crew preservation on every rejection and ordinary empty-prison return.
- [x] Add shared rogue manual-command guards and matching disabled states to bay/interior/ACC/cargo controls. Bay interception while docked dispatches stage10 escape; retained in-flight actions only reject. Recovery through cockpit remains available. Test stale button callbacks after takeover and after save replacement, including rename/course/engine/mining/battle/cargo and fitting actions.
- [x] Connect the controller to simulation after enemy/MTX updates, before SDM; add pending story handling after higher-priority existing notices. Mutiny initializes252 countdown; each fourth eligible visit decrements, hijack/sabotage can shorten2/5; unlock prison research and publish Rogue Ship once. Test competing notices, partial research, normal research/manufacture/fitting, no staff sender, save/reload and nonadvancing updates.
- [x] Run headless and native end-to-end cases, inspect scene screenshots, and perform a disposable physical takeover/recovery/prison/save/reload check with original save inventory restored. Commit the complete enabled loop only after these checks.

## Task 4: Whole-branch review, full validation and integration

**Files:** `docs/{original-news-evidence,validation-results,backlog-progress,windows-agent-brief,save-files}.md`, this plan.

- [x] Obtain executing-plans' one final fresh-context whole-branch reviewer against this spec and baseline. Reproduce/fix Important/Critical findings, recording actual failures separately from fixture mistakes. Do not repeatedly request review after the single correction round.
- [x] Source `/Users/craigfletcher/.local/share/deuteros-toolchain/env.sh`; run nine Python tests and `python3 scripts/validate.py --godot /tmp/deuteros-godot-isolated --export-windows` from the isolated checkout. Gate each command on successful build; one engine process at a time. Require all fresh discovered cases, strict import/smoke and export.
- [x] Audit individual case manifest/count/freshness and package64illustrations/zeroTests. Record exact runtime SHA, native screenshots, physical fixture provenance and known adaptations; no Windows claim from a cross-export.
- [x] Update Windows follow-up for takeover, raid/MTX, occupied dock sabotage, containment, full roster, save/loss and story priority. Preserve the8cdd458 handoff. Update48-task counts only against actual requirements; no remote writes.
- [ ] Acquire the coordination merge lock, fast-forward locally, verify Godot/scripts/.github tree identity with tested runtime and unchanged AGENTS.md, release lock, record progress and retain all failure evidence.
