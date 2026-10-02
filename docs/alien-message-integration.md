# Alien transmission integration

Implementation design for Asana 1215691951441134, based on [the original trace](original-alien-message-evidence.md). Direct recovery credit, legacy credit conversion, device manufacture and normal-hull fitting are on the contribution branch. Capture assignment and transmission delivery are implemented in the isolated branch below; device activation and the ending remain outstanding. Preserve the existing Windows desktop handoff and complete each change in an isolated checkout while aggregate validation runs.

## Isolated work in progress

Branch `codex/alien-capture`, in `artifacts/worktrees/scg-names`, now removes startup placement and assigns an artifact only after a system's final hostile station is captured. Sun is excluded. Assignment history survives collection and save/load; old saves preserve all prior assignments and held cargo. Pending location notices are saved in capture order. This branch is **not merged into the contribution branch**: message delivery must be connected before replacing the playable checkpoint.

Focused cases **422–426** pass for initial absence, final-capture gating, duplicate prevention, eight systems, destruction exclusion, legacy saves and invalid saved state. Existing recovery/SDM cases 135/360/374/410/411 also pass. These are focused checks, not a full 426-case or desktop acceptance run. Evidence is under `artifacts/validation/evidence/alien-capture/`.

The capture trace also exposes a missing progression producer. `$35CB0` sets `$1C2D7` even for the Sun; the master consumer at `$23E1A` dispatches `$3771A` after the transmission branch. That handler clears the flag, checks the SCG chassis completion byte `$1A036`, and, only when zero, applies table `$375FE`: item indices **12, 13, 14**, then terminator 0 and bulletin **6**. These map to SCG chassis, star drive, HED fuel and the existing `Sol_Cleared` bulletin. Main-branch definitions have no producer for this discovery. The isolated branch now saves its pending flag and publishes it after model/display updates when existing bulletins, input locks and overlays permit. It exposes the three research projects and the existing galaxy navigation unlock without granting completed research or manufactured stock. `capture-research-followup.txt` retains the instruction extract. The subsequent original transition/clock behavior remains a separate integration concern.

Case **427** reproduces the missing SCG discovery, then passes headless and with native Mac rendering. It captures through `SdmSystem.ApplySwitches`, advances the real simulation, verifies an earlier Drone Ships bulletin retains priority, reloads the pending notice, selects all three projects through Research, and checks recapture preserves progress without repeating the bulletin. Its native screenshot was reviewed. Evidence: `artifacts/validation/evidence/alien-capture/scg-final/`. Six new focused cases pass; no full 427-case run or Windows validation of this isolated branch is claimed.

The isolated branch now starts saved stages/countdowns from both real war paths, presents the research notification and an accessible acknowledgement button, renders the original alien message bodies, and replays through News. Delivery waits for existing scene/input owners; leaving an unacknowledged notice preserves it for retry. An acknowledgement from a replaced save cannot consume either world’s notice. Capture notices retain order, including captures made before the trust message. The eighth real unload supersedes obsolete location notices with the final two-update delay.

Cases **428–435** cover both war triggers, all introductory stage boundaries with save/load, invalid saved fields, interrupted/stale acknowledgements, eight real capture→scan→grapple→bay-unload paths, final-message precedence and replay mask rotation. Existing 424/426 now also cover legacy war/completion migration and draining eight queued notices. Focused checks pass; native Mac 430/433/435 pass. Screenshot review found overlapping alien glyph rows; removing the bulletin’s negative line separation for alien text fixes it, and case 430 now checks each laid-out row is at least eight pixels apart as well as total panel height. The corrected stage screenshots were inspected. Evidence: `artifacts/validation/evidence/alien-transmissions/` (retain the failing `layout-red` and passing `layout-green` logs).

Full **435-case Mac validation passed at `642ed00a61817b3a09a2eece2d0df69ad787337f`**, together with nine Python tests, strict import, startup smoke and Windows cross-export. All individual logs and package contents are audited (1,221 entries, 64 illustration imports, no tests). Windows execution of this isolated revision is still pending. It is not yet a replacement for either the contribution branch or the stable human desktop handoff.

## Intended result

Both existing war declarations start the original transmission sequence. News can replay the last displayed message without changing progression. Capturing the last hostile station in a system makes its artifact location available; collecting eight segments reaches the final instructions. New games follow the original sequence. Existing saves retain ships, cargo, research and already placed artifacts.

Transmission countdowns count eligible simulation updates. Do not describe them as seconds or add a second independent clock. The pending fractional/interstellar-clock decision still governs when updates occur.

## Integration points

| Existing path | Required change |
| --- | --- |
| `ShipInterior`'s two war declarations | Call one shared, idempotent start operation after war is committed. Initial countdown 10; repeating a contact must not restart it. |
| `GameCore.TriggerDay` and `Unlocker` producers | Defer competing messages until model updates finish and input is available. Preserve original discovery priority; do not replace an active message or leave a lock behind. |
| `SdmSystem.ApplySwitches` | After a real hostile-to-friendly capture, detect the last hostile station in that system. Assign its artifact once and schedule the location transmission. Station destruction is not this capture event. |
| `CoreData` artifact setup | New games start without preassigned artifacts, including the present Earth artifact. Use the traced per-system location ranges when capture reveals a segment. |
| `Grapple` capture and `ShipBay.GrappleClosed` | Preserve existing single-capture/single-unload safeguards. Directly credit completion 12, 24, 36, 48, 60, 72, 84, 100; the eighth delivery unlocks the device and schedules final instructions without a further scientist gate. |
| Existing item, production and tool controls | Reuse item ID 1. Original data specifies orbital manufacture, production rank 1, zero material cost and mass 2000. Fit one device to an SCG using the original hull eligibility masks; activation requires ordinary arrival state and a Warlord pilot. |
| `News`, `Bulletins`, alien themes | Reuse the existing screen lifecycle and replay control. Add the department notification/acknowledgement and alien-font presentation; keep readable location insertion separate from cryptographic body text. |
| `SaveFile`, `SaveStorage` | Persist transmission stage, countdown, pending location notices, delivered count and last displayed message context. Validate ranges and referenced locations before switching worlds. |

Keep progression decisions in one small model helper called by these existing paths. No event framework, service interface or second scene-navigation system is needed.

`ArtifactRecovery` handles direct credit and legacy conversion using existing persisted research fields. Cases 135/410/411 and full 411-case Mac validation cover recovery. Cases 412–416 verify the zero-material orbital recipe, saved production and both manual/AOC factory gates; 417–421 cover original hull lists, legacy equipment and the manufacture-to-fitting path. Full 421-case runners pass on both hosts; Mac logs/package are audited and Windows packaged smoke/collection remain pending. The isolated branch adds only the transmission state described here; it does not implement the ending.

## Saved-state rules

- Stages and delays follow the traced table: 0–2 repeated text, 3 contact, 4 trust, 5–12 location reports, 13 final instructions. Displaying stages 12/13 does not advance beyond them.
- A queued message is not delivered until its presentation starts successfully. Interruption releases audio/input ownership; it must not duplicate segment credit or consume an undisplayed notice.
- Replay records the displayed stage and location independently of the next stage. It neither advances the stage nor resets the countdown. Like the original, rendering rotates the saved decoding mask again, so replay can change which letters are readable.
- A system needs an explicit artifact-assigned marker: `ArtifactLocation == none` currently means both never assigned and already collected.
- Legacy saves lacking the entire transmission state preserve their existing artifact locations and held cargo; mark all systems assigned so capture cannot duplicate them. At-war saves start the introduction with delay 10; completed recovery takes precedence and schedules final instructions with delay 2. Subsequent loads retain the saved timer. Existing explicit transmission state is not replaced by an invented history. Derive credited deliveries from the old research limit only when it is a valid multiple of 11, capped at eight. Preserve unusual/cheated state explicitly rather than inventing a history.
- The legacy ninth artifact may remain as preserved cargo/location, but cannot award a ninth progression credit. New games contain eight. Save conversion must be deterministic, leave the input file untouched and retain the existing backup/atomic-write behavior.
- Keep delivered progress distinct from ordinary research actions. Derive the original completion value from verified delivery count; completing this item must not award unrelated scientist work. Audit `ResearchOrder`, unlock notifications and canonical item references when converting old partial research.

## Verification gates

1. Reproduce both missing war-start paths; prove one start, exact eligible-update boundaries and save/reload at every stage.
2. Exercise competing discoveries and transmissions, active dialogs, leaving/replacing scenes and loading another world. Every undisplayed notice must survive; locks/audio must be released by their owner.
3. Capture a system through real SDM controls. Verify no premature reveal, one assignment, correct location range, no duplicate on repeat capture, and no capture reward for destruction.
4. Scan, collect and unload all eight segments through real controls. Verify locations clear once, direct completion survives save/load, the eighth delivery unlocks manufacture without scientists, final instructions occur once, and ordinary non-artifact cargo is unchanged. Replace the existing artifact test's erroneous `ResearchLimit + 11` expectation with these source-backed rules.
5. Check legacy saves with zero through nine credited/held segments and a partly researched artifact. Reject invalid new fields without activating the save.
6. Render all message families with the supplied alien font; verify glyph correspondence, mixed readability, wrapping and ordinary location text. Replay must preserve gameplay progression; only the original decoding mask rotates.
7. Exercise zero-cost orbital manufacture through Stores/Production and actual tool fitting; check mass, inventory debit and insufficient rank. Share the original hull filtering between display and selection, keep all eleven SCG choices reachable and allow removal of legacy incompatible equipment. Verify docked/travelling activation rejection and Warlord gating against the traced dispatch. Modified hull behavior and ending presentation still need their original-path trace; displaying instructions alone does not prove campaign completion.
8. Run full Mac/Windows validation and Windows desktop acceptance, including normal campaign progression through Warlord promotion and the ending.

## Save compatibility

The optional `AlienTransmissions` object stores assignment history, queued locations, pending SCG discovery, stage/countdown/readiness and last displayed stage/location/mask. Missing legacy state is migrated without changing the input file. Null objects, impossible stage/delay bounds, unknown or duplicate system references and invalid replay locations are rejected before world activation. The save format number is unchanged; older executables that reject unknown fields cannot load these newer saves. Keep old save backups and use a separate test profile.

Countdown values are verified as eligible update counts, not original elapsed-time fidelity. Warlord promotion, modified hull conversion, activation and ending remain open; receiving final instructions does not establish a playable ending.
