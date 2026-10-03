# Rogue crew, sabotage and prison containment

## Outcome and scope

Complete the missing rogue-crew loop that supplies the Mutiny/Prison bulletins and pirate SDM News, advancing the SCG, News, SDM, saves and event-order tasks in the 48-task goal. A qualifying Warlord steals an SCG, visits hostile stations, raids human stores, sabotages occupied bays, can hijack a docked SCG from a station roster, and can be contained through existing crew/pod controls. Implement the complete loop in an isolated branch before enabling it in the contribution branch. No push, PR, remote merge or Asana writes. Craig authorized autonomous local implementation/testing while AFK; Windows and original-runtime acceptance remain separate gates.

Use Godot 4.2.2 .NET, net6.0, Newtonsoft.Json and the existing regression runner; add no dependencies. Preserve root AGENTS.md, existing saves and the independent Windows 8cdd458 handoff. Only one Godot/game/test process at a time.

## Evidence and adaptations

The aligned instruction traces and hashes in `docs/original-news-evidence.md` are authoritative for controller selection, twenty stages, crew references, resource mask, prison behavior and SDM interactions. Exploratory `*-leads.txt` files are not complete instruction evidence. The remake has body-to-body routes and larger configurable fleets/station allocations: reuse its saved interstellar/local travel and stable allocation ordering. A cross-star rogue route composes star and body travel, as current player/ACC routes do. This preserves actual acceleration, clocks, fuel and destination arrival; it is an explicit route-interface adaptation, not an exact original cycle-count claim. Do not invent instant relocation or fuel-free travel.

The original empty-prison dismantle branch rejects even an empty pod through a likely wrong-register test. Preserve the remake's empty-equipment return contract; occupied prisons must reject removal/replacement/dismantling without losing their crew. Retained UI callbacks and interrupted capture dialogs are modern lifecycle boundaries: reject stale commands and keep an already captured prisoner on exit/reload rather than silently discarding it. Record these adaptations in validation documentation.

## Saved identity and lifecycle

Add one `RogueCrew` state to SaveFile, defaulting inactive for old saves. Persist Occurred, selected Crew reference, controller Stage, pending Mutiny notice, prison-discovery countdown/divider, and any temporarily reduced crew count. Reuse Newtonsoft reference preservation and ShipModule.StaffStored. Persist Pirate rank on Staff independently of ordinary experience/Warlord; GetLevel/GetLevelString expose rank 5. Never use MethanoidOwned for a rogue SCG: consumers cast that flag to EnemyFleet.

Derive ship control from the selected crew actually piloting an SCG; derive containment from the selected crew occupying a fitted prison pod. Validate identity before world activation: at most one live selected reference, exactly one physical location, marine/Warlord eligibility, no duplicate or detached selected crew, stage/countdown ranges and valid temporary-count stages. Old saves contain no rogue event; loading never starts one. Crew loss clears the selected reference and pending controller work while retaining Occurred, preventing a second mutiny. Count restoration on an interrupted sabotage stage must preserve surviving staff, including transfers and save/reload.

## Selection, routing and progression

After the existing ship/refining/enemy/MTX simulation phase and before SDM expiry, resolve the selected crew/controller, then attempt first selection when fewer than five systems have hostile stations. Selection requires nonzero Hyperlight research progress. Choose the first mixed human/hostile system in original star order, then its first eligible SCG in stable allocation order: undocked, fuel exactly 250, Warlord pilot, no occupied cryopod, no Blaser conversion. DFCC and ordinary PTL fittings do not independently exclude it. Mark Pirate, rename BOUNTY, disable ACC activity, preserve unrelated hull state, begin stage 11 and queue Mutiny once.

Controller stages preserve the original decisions: 1 chooses a hostile station; 2 starts travel; 3 docks/refits with fuel250 and six supply pods; 4 waits for launch; 5 chooses the human target; 6 starts travel; 7 waits; 8 docks; 9 redirects MTX; 10 steals/refuels/launches; 11 waits; 12 resets; 13–15 carry the cross-star relocation; 16–19 handle occupied-dock sabotage; 20 resets. Resolve targets each routing cycle. Keep the current system if hostile stations remain, otherwise choose the first with at least two hostile plus a human station, otherwise the first hostile system. Select the first enemy station and the first human station with at least five IOS drones, falling back to the enemy station. Missing targets wait/reselect safely without inventing a station.

Hostile refit clears DFCC/PTL conversion and fleet state, removes existing modules and supplies six empty cargo pods; preserve engine damage. Selection rejects occupied cryopods and controller-owned crews cannot be put into ordinary cryopods. No destructive refit may discard another stored crew or manufacture human salvage refunds. Ordinary ACC must not resume while the rogue owns the ship.

The raid mask is Titanium, Aluminium, Paladium, Platinum, MeH and HeD, in that order. Walk supply mounts, replacing their contents with at most250 from the selected stock; continue the same material while stock remains. Preserve non-supply mounts. Stage10 then refuels to250 using remaining HeD at the original one-stock-per-gauge-unit raid rate and launches. Stage19 loads only the first usable supply mount after restoring crew strength.

MTX redirection requires the raided source to have MTX. Search the first eight original global station records (Solar allocation slots0–7) for human ownership and Aluminium>=200, regardless of candidate MTX. Use the existing saved orbital MTX route/send/balance representation for the six-resource mask. Normal MTX eligibility still determines actual subsequent transfer; distinguish route assignment from executed stock movement in tests.

## Docking, sabotage and losses

Rogue ships bypass ordinary hostile-orbit attack accumulation and escape damage rolls. Enemy docking does not arm the ordinary player SDM countdown. An occupied non-shuttle bay returns the rogue to undocked state and controller16 instead of waiting indefinitely. Stage17 stores the original crew count then shifts it right3; stage18 advances first and rolls `random & 7 == 4`, then arms SDM150 and emits one location self-destruct report; stage19 restores count and raids one pod. Failed rolls still advance. Save at every boundary without rerolling or duplicate reports.

Before station SDM casualties, a rogue docked there launches; a docking rogue becomes undocked. Both resume stage11. Ordinary unrelated victims remain subject to existing loss rules. All ship/crew removal callers must clear the selected identity through the shared loss boundary. A frozen prisoner is an ordinary stored crew for loss reporting, but excluded from attrition and autonomous control.

## Crew controls and prison discovery

The original cockpit removal path remains available: moving the rogue pilot into an empty local roster clears control of that hull; swapping another marine does the same. The selected rogue may pilot only an SCG. A free rogue in a station roster may hijack a docked SCG, swapping the old pilot into that same slot and starting stage10. Keep crew inventory/reference counts exact. Ordinary cryopod transfer rejects the selected rogue.

After Mutiny publication, prison discovery starts at252 and decrements every fourth eligible low-priority story visit. Existing higher-priority notices postpone it. Hijack and sabotage can shorten the countdown to2/5 while research remains undiscovered. Discovery unlocks the existing prison research and displays the existing Rogue Ship bulletin once; preserve normal research/manufacture payment.

A fitted prison uses the existing torso staff list. Selecting the rogue transfers it into StaffStored, freezes controller/attrition and starts the nominal0.5-second capture window. Right-click retains/locks the prisoner; expiry releases it to the vacated roster slot. Explain the gesture in the existing panel/hover text. If the roster has become full, retain the prisoner instead of losing it. Releasing a locked prisoner requires a free slot and resumes its Pirate rank and future hijack behavior. Equipment selection must remain reachable for an empty prison. Screen exit, world replacement and repeated input cancel callbacks safely; no timer may mutate a replaced world.

Rogue remote controls reject manual course, ACC, engines, mining, cargo disposal, battle and equipment changes. A docked rogue's intercepted bay command triggers the original stage10 escape; in-flight/stale callbacks reject without changing travel or stealing from an old location. Cockpit crew recovery remains usable. Visual disabled states and model/callback guards must agree.

## Verification

Append real-engine cases after524. Prove missing selection/controller/containment before production fixes. Test mixed-system choice, every eligibility boundary, both route directions, DFCC/PTL/engine damage, full twenty-stage execution, occupied/successful docking, deterministic SDM hit/miss, cargo/refueling quantities, MTX selection, crew swap/hijack, prison right-click/expiry/full-roster/scene-exit, loss and SDM escape. Save/reload at all meaningful stages, frozen/free/aboard locations and malformed identity/state before activation. Exercise actual paid prison research/manufacture/fitting and actual scene controls with retained callbacks.

Run focused headless/native tests, inspect screenshots, perform a staged desktop takeover/recovery/capture/save/reload check, obtain one independent whole-branch review, then full regression/Python/import/smoke/cross-export and log/package audits. Do not count Windows or normal campaign acceptance from Mac fixtures. Keep the 48-task evidence ledger unchanged until the relevant requirement has demonstrable implementation evidence, and keep full acceptance pending its actual platform/gameplay gates.
