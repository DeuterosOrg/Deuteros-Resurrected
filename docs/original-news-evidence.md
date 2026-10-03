# News event inventory

Follow-up for **1215685674676231 — News system and screen**, 2026-10-02. This separates original event evidence from the baseline triage's suggested training/research/production examples. The task has no description, comments or subtasks in the retrieved snapshot.

## Original dispatch

Use the disk identity and address mapping in [the ACC trace](original-asteroid-acc-evidence.md). `$393A0` appends the eight-byte scratch record at `$39396` to a twelve-record history by shifting the preceding eleven records. It stamps the date from `$1378E`. The News screen loops twelve times at `$396E4–$39724`, dispatching by the record's first word through `$3941C`.

| Type | Handler | Rendered event |
| ---: | --- | --- |
| 0 | `$3948A` | Empty record |
| 1 | `$3948C` | Named person's new rank |
| 2 | `$394A6` | Rank/name killed |
| 3 | `$394C0` | Named IOS destroyed |
| 4 | `$394E0` | Location's shuttle lost |
| 5 | `$394F2` | Location under attack |
| 6 | `$39520` | Location captured |
| 7 | `$39532` | Location's orbital factory destroyed |
| 8 | `$3954E` | Named starship destroyed |
| 9 | `$3956E` | Named IOS under attack |
| 10 | `$395AA` | Named starship under attack |
| 11 | `$395E6` | Location self-destructing |
| 12 | `$3960E` | IOS scrapped |
| 13 | `$39620` | Starship scrapped |

Strings `$191–$197`, `$17F` and `$19C` supply the event suffixes. Types 3/9 index ten-byte IOS names at `$13128`; types 8/10 index twelve-byte starship names at `$136C8`. Types 12/13 use text IDs 6/7 (`I.O.S.` / `Starship`) plus `Scrapped.`.

Confirmed direct scratch-record producers include ordinary marine promotion (`$22E52`), Warlord promotion (`$3153A`), ship attack (`$31280`, types 9/10), station attack (`$38D62`, type 5), station destruction (`$3616A`, type 7), crew death (`$361EE`, type 2), shuttle/IOS/SCG destruction (`$36292/$36342/$36406`, types 4/3/8), and pirate self-destruct arming (`$39FCA`, type 11). The direct-reference scan does not establish producers for types 6/12/13; their rendering alone is not evidence of a reachable original event.

No training graduation, ordinary research completion or production completion event was identified in this dispatch. Do not add those solely because the initial triage proposed them as examples. This is a static inventory, not an emulator observation or proof that indirect writers cannot exist.

Raw extracts are retained under ignored `artifacts/research/news/`: `news-renderer.txt`, `news-entry-points.txt`, `news-loss-producers.txt`.

## Remake coverage at `642ed00`

The existing News model retains history, the screen displays the latest twelve reports, and saves preserve both history and replay context. Staff promotion and SDM capture/destruction already publish reports. Alien transmissions now replay through the existing News control in the isolated campaign branch; full 435-case Mac validation subsequently passed.

The caller audit finds silent removals in `ShipInterior.UpdateShips` (fuel/hostile-orbit losses), the battle-result path, `EnemyFleet.CapturePlanet`, and successful `ShipBay` dismantling. SDM losses do report. Ship attack and `EnemyFleet.ProcessFleet` station-attack transitions also have no News producer. These are concrete integration gaps; one shared ship-loss formatter can cover all loss callers without replacing the existing history model.

Verify reports at committed state transitions, once per event, with named ships, failed dismantling, repeated updates, save/reload and newest-first screen ordering. Keep station loss/capture and crew loss distinct. Suppress repeated attack notices while an attack remains active. A report must not convert a rejected or cancelled operation into a successful event. Original clock/date formatting remains part of the separate clock integration decision.

## Isolated producer corrections

Branch `codex/news-events` now publishes ship attack/loss, fleet station attack/capture and successful dismantling reports at their existing state transitions. Fuel losses, hostile-orbit losses, battle-result removal and fleet capture share the ship-loss formatter. Existing SDM cause-specific reports remain intact. Failed dismantling and repeated inactive fleet updates publish nothing. This batch does not add crew-death or pirate self-destruct producers, or implement missing campaign mechanics.

Cases **436/437** reproduced missing ship loss and station attack reports. Existing dismantling cases **297/301** reproduced missing successful-scrap reports while retaining their rejected-operation inventory checks. All four now pass. Follow-up screenshot review and a failing short-history assertion exposed the News screen's empty leading rows; both short and full histories now show newest reports first. Long names use native ellipsis inside the panel and retain complete report text in hover help; unused rows clear that text.

Focused 9/124–128/297/301/360/374/435–437 pass. Native 124/436/437 pass and corrected screenshots were inspected; physical hover interaction remains a desktop check. Saved report history and full-width source strings are verified. Evidence is under ignored `artifacts/validation/evidence/news-events/`, including initial missing-producer failures and the short-history failure. Full 437-case Mac validation and audited Windows cross-export pass at `9b245f5`; Windows execution remains pending and no task is counted fully accepted.


## Crew-loss boundary still requiring comparison

Helper `$36188` first tests the low byte of the supplied reference and returns for zero or a negative signed byte. Accepted references are decremented and indexed into the eight-byte roster at `$197C2`; the emitted type-2 report uses `rank & 7` plus text base `$A0`. Allocator `$22D86` uses a bitmap and accepts one-based references below `$AB`, so allocation can reach beyond the helper's signed-byte range. These instructions do not establish that the rejection denotes a crew type, or that every allocated team can emit this report. Do not infer a marine/scientist distinction from the sign test alone. An original-runtime high-reference loss and the later roster-release consumer are still needed. Extracts: `crew-loss.txt` and `staff-allocation.txt` in the same ignored evidence directory.


The additional roster-consumer trace identifies byte `+7 = 5` as a persistent **deceased status**, not a five-update release timer. `$392B0–$39332` branches on 5 and draws the text at `$390A1` (`>DECEASED<`). `$39166–$39190` includes every nonzero roster entry in that display. The loss helper clears a bitmap bit in a register at `$361D6` but does not write it back before reusing that register. Thus this trace does not establish slot release or justify treating the helper's signed-reference test as a staff-type filter. High-reference runtime behavior remains unresolved. Evidence: `artifacts/research/news/crew-status-consumer.txt` and `roster-timer-followup.txt`.

## Crew rank text and report ordering

Initializer `$403E6–$403F2` sets the text-table pointer to `$1C482`; the on-disk value of `$1F97C` itself is zero before initialization and must not be used as the table base. Decoding the initialized table gives `$A0 = Name Ship`, `$A1 = Pilot`, `$A2 = Captain`, `$A3 = Admiral`, `$A4 = Warlord` and `$A5 = Pirate`. These are the indices reached by the loss helper's `rank & 7` calculation. This confirms the marine rank labels but does not establish correct labels for every other roster type or resolve the signed-reference limit.

The ship-loss callers invoke `$36188` for the pilot and qualifying occupied slots before emitting their type-3/4/8 vessel report. Station destruction likewise processes roster references before its type-7 report. Crew and vessel loss are therefore separate reports, ordered crew first in these callers. Exact rank strings are retained in `artifacts/research/news/crew-rank-texts.json`; the earlier read of the uninitialized pointer is retained separately as invalid evidence.

The remake caller review also finds that `EnemyFleet.CapturePlanet` clears ground/orbital staff arrays but retains `Station.Factory.Builder` while turning on AOC. That reference remains serialized and visible to the attrition traversal. The crew-loss follow-up must test actual removals, retained distant crews, duplicate transitions and this stale factory reference; staff transfers and ordinary attrition are separate operations.

Cases 471–472 reproduce missing crew reports through actual ship fuel losses, station destruction and fleet capture. The shared loss helper now reports pilots and cryopod passengers before their vessel; station callers report removed ground/orbital/factory teams. Capture clears the retained factory-team reference. Reports use the remake's existing typed rank formatter, including Engineer/Doctor passengers; this does not claim to reproduce the original's unresolved high-reference behavior. Earth ground crews, distant ships, transfers, scrapping and attrition keep their existing survival/reporting rules. Native cases 436–437 also check the rendered order and saved history. Evidence: `artifacts/validation/evidence/crew-news/` (initial missing-report failures, duplicate-factory-report failure, focused passes and native screenshots). Pirate-warning sourcing and full News acceptance remain open.

## Pirate-warning dependency

The master calls `$39878` only while fewer than five systems retain hostile stations and `$1C372` is zero. The selector requires a nonzero Hyperlight research byte, a qualifying SCG and rank 4. `$39930–$39980` records the selected crew, changes its low rank bits to 5, sets the ship's high flag bit, renames it `BOUNTY` and starts controller stage 11. The 20-entry controller at `$39B3A` dispatches stage 18 to `$39F98`; that handler advances its stage before testing whether `random & 7 == 4`. Only the matching branch writes `$96` to the station's SDM byte and publishes type-11 News.

Thus the warning belongs to the rogue-crew controller, not ordinary player arming or hostile docking. Warlord/Hyperlight travel is implemented at `e1754eb`, but this controller remains absent; adding a message to an unrelated SDM path would invent behavior. The exact selector also tests ship flags, fuel and packed module slots; their complete gameplay eligibility still needs mapping and original-runtime comparison. Trace and decoded dispatch targets: `artifacts/research/news/pirate-controller.txt` and `pirate-controller-facts.json` (trace SHA-256 `aef7cd78368990030a7bea80abf447cecaee8c7ae4c82d9df21b824817307352`).

## Rogue routing follow-up

Additional aligned trace `pirate-routing-followup.txt` covers the previously missing handlers `$39B8A–$39DC2` and star resolver `$35286`; SHA-256 `9bfe128146440b7069783b6f77784ac4e24bea8e5c6308aeb59dcadc6abb7943`. The controller retains the current star when it has hostile stations. Otherwise it prefers a star with at least two hostile stations and a nonzero companion count, then falls back to the first hostile star. Human capture `$35C0C` increments that companion table at `$19703` while decrementing the hostile count; it is the human station count.

The route selects a station with owner byte 9, confirmed by enemy capture `$38EF4`, and separately searches owner 8 with at least five drones; if none qualifies, the second location defaults to the first. A cross-star leg sets destination code `$A0 + star` and controller stage 13. Stage 5 uses the second location; the other selector calls use the first. The shared docking helper accepts states 3/16 and schedules state 4/action 5 with count 1.

On the refit path `$39D18`, fuel becomes 250 and all six module words become `$4000`, before departure and stage advance. This is destructive rogue-controller behavior, not a safe independent player-ship correction. Full ownership/crew transitions, packed module eligibility and original-runtime comparison remain required before porting it. Cross-reference trace SHA-256 `d3231d9b1e0bad3c8991cc6e1a41051de4e5f26f9556a4a0cc92c02d63d5d135`; decoded facts are in `artifacts/research/news/pirate-routing-facts.json`. No new gameplay or acceptance result is claimed.


## Rogue eligibility follow-up

Aligned selector `$39882–$398A6` chooses the first system containing both hostile and human stations before scanning SCGs. A vessel must match that system, be undocked (state 3), have exactly 250 fuel, acceptable nonzero hull flags below 8, and an assigned Warlord. This is narrower than searching every mixed-ownership system for any eligible ship.

The six packed module words at `$398D4–$398F0` reject nonzero low-byte entries of kind `$C000`. Cryopod transfer `$32A86` maps the low 14 bits of that kind to a one-based roster reference: occupied cryopods exclude the ship, empty cryopods do not. The exact low-byte test retains a reference-range caveat. This six-word scan agrees with the six-word refit and the remake’s six existing bay sections. The later `771ceea` correction restores six usable mounts; the older five-mount assumption is superseded.

Cargo selector `$39EA6` uses mask `$C606`, selecting zero-based store indices 1, 2, 9, 10, 14 and 15. Resource names still require an independent original item-order mapping. Full rogue ownership, crew takeover/recovery and scheduling must be understood before integrating the destructive refit. This is static source evidence, not gameplay acceptance. Trace: `artifacts/research/news/pirate-eligibility-crossrefs.txt`, SHA-256 `18b033b1b7e9ecf921a38b8d132172c7e7eb53b266ef6475080880467daa278b`; decoded facts are alongside it.

## Rogue crew lifecycle follow-up

The selected rogue reference is more than a ship flag. `$39A68–$39B18` finds that crew in the four local station slots, swaps it with a docked SCG's pilot, updates both roster locations and resumes controller stage 10. `$32AA6–$32AAC` prevents that selected crew from entering the ordinary cryopod swap. Crew loss `$36192–$3619A` clears the selected reference; this routine does not clear the active-event flag, so repeat eligibility must not be inferred from it.

Aligned traces are in `artifacts/research/news/pirate-crew-lifecycle.txt`, SHA-256 `e80333ec0d9132c55e08a4a01edd1bfdca5f60bf7cb9efb851b923088c040dbe`, with decoded facts alongside. Full controller scheduling and ownership remain implementation work; these static findings do not count as another resolved task.

## Rogue resources, scheduling and containment

Resource rendering `$24604–$24628` begins at text index `$32`. Decoding the initialized `$1C482` table maps mask `$C606` to **Titanium, Aluminium, Paladium, Platinum, MeH Fuel and HeD Fuel**. `$39EA6` walks supply mounts and transfers at most 250 units into each, retaining a resource for the next mount when more stock remains. Other module kinds are skipped. The refit at `$39D18` removes converted hull flags and writes six empty supply pods; it does not clear the separate drive-damage bit. Fitting `$33010/$33096` identifies bit 3 as the Blaser conversion (equipment index 9), not PTL. The eligibility trace therefore does not justify excluding a normally fitted PTL.

The master calls the rogue controller at `$23D1A`, after enemy actions and the station/screen helper, before progression dispatch and SDM expiry. `$21140` is a station/screen helper, not evidence of MTX simulation order. Mutiny notification sets the prison-discovery countdown to `$FC`; after higher-priority events, that countdown decrements every fourth eligible visit. Its completion dispatches recipe index 30 (Prison Pod) and bulletin 11. Controller actions can shorten it, so it is not a fixed elapsed-day deadline.

Prison handler `$32DA8–$32EA4` accepts the selected rogue from a local roster, clears that roster slot and sets frozen bit 5 plus status 23; controller `$39A60` returns for that status. Release restores the crew to the roster with status 3 and clears frozen state, without restoring its old rank. Capture then waits for 100 units of counter `$20284`: that counter advances four per vertical blank, so this is 25 callbacks (nominally half a second at 50 Hz), not 100 frames or a measured duration. The secondary-input bit exits while retaining the prisoner; timeout follows the release path. Reproduce this timing and input behavior in the original before claiming presentation fidelity.

Station-destruction helper `$3A02E–$3A096` checks the selected rogue before ordinary casualties. A rogue docked at the target starts launch; one docking there becomes undocked with zero countdown. Both resume controller stage 11. Implementing the warning alone or applying ordinary docked-ship destruction would omit this behavior.

Aligned extract `artifacts/research/news/pirate-prison-scheduler.txt` has SHA-256 `9e7e6078b2f0c90f886d80f87a8d1e8ccadc126f4c97fb710e4838ad26e063cd`; decoded resource names and facts are alongside it. The earlier `*-leads.txt` files contain incomplete/data ranges and are exploratory only. Controller integration, MTX redirection details, original-runtime and Windows acceptance remain open.

## Rogue command interception and cockpit recovery

Hull access (`$30306`), module/engine navigation (`$32108/$321C6`), bay refresh (`$32488`), dismantling (`$332F4`) and fuel controls (`$36708/$3678A`) intercept the rogue flag. Their `$399D4` path sets controller stage 10, dispatches the controller and refreshes. Some paths also call `$39982`, which can find the selected rogue in the four local roster slots and hijack a docked SCG. A port must cover retained callbacks as well as visible buttons.

Cockpit transfer `$326C8–$327F2` provides a distinct recovery path. Choosing an empty roster slot moves the pilot there, clears the rogue bit in both cached bay and hull records, and assigns status 3. Selecting the rogue from the roster is allowed only for an SCG; the normal marine swap clears the old flag and sets it only when the new pilot is that selected rogue. Blocking all crew transfers would therefore remove an original route to the prison controls. The later bay-refresh/hijack interaction still needs runtime comparison.

Dismantling has an additional fidelity trap: the Prison branch at `$3352E–$3353C` tests register `d0`, which still contains tool-kind 2, rather than the packed prisoner byte in `d2`. These instructions reject even an empty prison; they must not be documented as an occupancy check. Preserving modern empty-prison returns while rejecting occupied-prison removal is a possible adaptation, not implemented or original-verified behavior. The neighboring DFCC dismantle path clears flags and returns drones; it does not establish equivalence with the remake's standalone equipment selector.

Aligned traces: `artifacts/research/news/pirate-control-and-dismantle.txt`, SHA-256 `6806a2e45817f85246628027ba184c7c7e3551d34daaf930978dc4233e15218e`; `pirate-cockpit-transfer.txt`, SHA-256 `f41a57e959fb4390db8de78fd2fa996c9cff38220f53a0ea5f908be5b381cc95`. Decoded facts are alongside them. These are static source findings, not implemented rogue gameplay or a newly resolved Asana task.
