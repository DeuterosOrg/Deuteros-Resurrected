# Saved interstellar travel and Warlord

## Intended outcome

Complete the SCG travel path and its Hyperlight-specific Warlord promotion, advancing Asana 1215685674676241 and 1215716464570913. Keep the existing planet-to-planet course and ACC controls. A cross-star route must consume the original interstellar distance, acceleration and fuel rules before reaching its requested body. Ordinary local arrival or accumulated actions must never grant Warlord.

Craig requested autonomous completion while AFK. The route adaptation below is an explicit implementation assumption, not an original UI claim. Work stays isolated until full validation and final review; no push, PR or Asana writes.

## Evidence and route adaptation

Use `original-interstellar-travel-evidence.md`, `original-warlord-evidence.md` and the checked Disk 1 traces. Original star codes are separate destinations. The remake already accepts a remote body through `Ship.CanTravelTo` and ACC. Preserve that interface by composing an interstellar leg with the original destination-star-to-body leg; do not replace the star leg with subtraction of local orbital indices.

The interstellar table is the symmetric 9×9 word matrix at `$34FE8`. The original source body is converted to its star on an accelerated departure (`$30F8E–$30FA0`). At successful star arrival, resolve the destination's local leg with `$3523C–$35256`: distance from that system's first encoded body plus four. The original body grouping places Earth before Mercury/Venus; same-parent distances count the parent's position as zero and its moons from one. Preserve these mappings explicitly rather than assuming enum order or window coordinates.

The first regression uses Mercury → Atlantic: both have remake orbit index zero, so the current route incorrectly arrives on its first update despite crossing stars.

## Saved progress and clock ownership

Keep `CurrentDay` as the consumed-update counter and `GameClock.DateCentidays` as elapsed Sol centidays. All nine original star clocks advance by the same increment, so derive their dates from Sol plus the original initial offsets. Do not persist nine redundant accumulators.

A saved SCG flight records its leg, remaining distance, acceleration phase, fractional speed byte, and private-clock offset from Sol. The offset changes by `distance advanced * 100` during acceleration. This representation automatically follows natural/manual increments, including a produced increment held behind a UI lock. The Hyperlight transition clears the original private clock before its next-update arrival; represent that transient value without unsigned underflow. Successful arrival synchronizes to the target star; ordinary arrival requires exact equality.

A null flight in an already travelling legacy save retains the old countdown to finish that journey without inventing previous acceleration or losses. New departures always create saved flight state. Loading never advances a flight or promotes a crew. Reject invalid legs, phases, fractions, distances, clock ranges and ship/route combinations before activation. Missing legacy fields remain readable; explicit malformed values are not silently repaired.

## Simulation and fuel

Start accelerated interstellar travel at phase 1. Each consumed update uses the source speed words `[256,257,268,295,320,358,426,587,821,1816,5689,10000]`: add the low byte to the saved fraction, carry into the speed word, then subtract `(speed >> 8) + 1`, capped by remaining distance.

At remaining distance at least 47, advance toward phase 11. Below 47, decelerate toward 1. Charge each newly selected phase using `[1,1,2,3,4,6,8,12,15,18,20,20]`. Accelerated state does not also pay the generic one-unit-per-update fuel cost. Source underflow at `$23ABE–$23AC2` selects action `$11`, which dispatches to the ship-loss handler `$3145A`; handle that committed loss once, including crew News and UI cleanup.

At phase 11 with at least 47 remaining and completed Hyperlight, select the one-update Hyperlight transition. Only its arrival synchronizes the clock and promotes an Admiral. Ordinary clock mismatch loses the SCG through the same established loss-reporting boundary. A successful star leg proceeds to the requested body; ACC receives one final arrival, not an intermediate station arrival.

Engine damage, interruption, inadequate fuel, save/reload, route previews and ETA must use the same saved progress. Trace a source branch before assigning it a new behavior; no invented fuel refunds or safe-arrival shortcuts.

## Warlord and display

Persist Warlord separately from ordinary action experience. Existing saves default to ordinary rank; valid promotion retains actions and emits one rank report. Audit all `GetLevel`/rank consumers, including crew display, cryopods, battle, attrition and save validation. Ordinary promotions retain their current thresholds and cap.

The ship interior must show the real remaining journey and private-clock date/projection while travelling. Keep body selection and cancelled-course behavior intact. The composed route is a remake UI convenience, not a claim that the original automatically selected the final body.

## Verification and completion

Reproduce first-update cross-star arrival, missing acceleration, fuel/clock loss, missing Hyperlight promotion and reload discontinuities before their fixes. Check all 81 table entries, asymmetric chronological arrivals, mixed natural/manual time, damaged drives, insufficient/exact fuel, a save in each leg/phase boundary, old in-flight saves, stale metadata, competing News and ACC round trips. Test ordinary and Hyperlight routes independently; completion of research mid-flight must be observed at the original phase check.

Run focused real-engine and native UI checks, then one independent whole-branch review and the complete regression/Python/import/startup/export validation. Audit individual logs and package content. Keep Windows execution and original-runtime comparison explicit. This plan does not close unrelated rogue-crew, ending, or audiovisual acceptance gaps, and does not redefine the 48-task goal.
