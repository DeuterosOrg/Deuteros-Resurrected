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

Focused 9/124–128/297/301/360/374/435–437 pass. Native 124/436/437 pass and corrected screenshots were inspected; physical hover interaction remains a desktop check. Saved report history and full-width source strings are verified. Evidence is under ignored `artifacts/validation/evidence/news-events/`, including initial missing-producer failures and the short-history failure. Full aggregate and Windows execution of this additional batch remain pending; no task is counted fully accepted.


## Crew-loss boundary still requiring comparison

Helper `$36188` first tests the low byte of the supplied reference and returns for zero or a negative signed byte. Accepted references are decremented and indexed into the eight-byte roster at `$197C2`; the emitted type-2 report uses `rank & 7` plus text base `$A0`. Allocator `$22D86` uses a bitmap and accepts one-based references below `$AB`, so allocation can reach beyond the helper's signed-byte range. These instructions do not establish that the rejection denotes a crew type, or that every allocated team can emit this report. Do not infer a marine/scientist distinction from the sign test alone. An original-runtime high-reference loss and the later roster-release consumer are still needed. Extracts: `crew-loss.txt` and `staff-allocation.txt` in the same ignored evidence directory.
