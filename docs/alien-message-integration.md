# Alien transmission integration

Implementation design for Asana 1215691951441134, based on [the original trace](original-alien-message-evidence.md). This is outstanding work, not implemented behavior. Preserve the existing Windows desktop handoff and complete each change in an isolated checkout while aggregate validation runs.

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
| `Grapple` capture and `ShipBay.GrappleClosed` | Preserve existing single-capture/single-unload safeguards. Credit one delivered segment; eight deliveries reach the original final-instruction transition. |
| `News`, `Bulletins`, alien themes | Reuse the existing screen lifecycle and replay control. Add the department notification/acknowledgement and alien-font presentation; keep readable location insertion separate from cryptographic body text. |
| `SaveFile`, `SaveStorage` | Persist transmission stage, countdown, pending location notices, delivered count and last displayed message context. Validate ranges and referenced locations before switching worlds. |

Keep progression decisions in one small model helper called by these existing paths. No event framework, service interface or second scene-navigation system is needed.

## Saved-state rules

- Stages and delays follow the traced table: 0–2 repeated text, 3 contact, 4 trust, 5–12 location reports, 13 final instructions. Displaying stages 12/13 does not advance beyond them.
- A queued message is not delivered until its presentation starts successfully. Interruption releases audio/input ownership; it must not duplicate segment credit or consume an undisplayed notice.
- Replay records the displayed stage and location independently of the next stage. It neither advances the stage nor resets the countdown.
- A system needs an explicit artifact-assigned marker: `ArtifactLocation == none` currently means both never assigned and already collected.
- Legacy saves lack transmission history. Preserve their existing artifact locations and held cargo; mark those systems assigned so capture cannot duplicate them. Derive credited deliveries from the old research limit only when it is a valid multiple of 11, capped at eight. Preserve unusual/cheated state explicitly rather than inventing a history.
- The legacy ninth artifact may remain as preserved cargo/location, but cannot award a ninth progression credit. New games contain eight. Save conversion must be deterministic, leave the input file untouched and retain the existing backup/atomic-write behavior.

## Verification gates

1. Reproduce both missing war-start paths; prove one start, exact eligible-update boundaries and save/reload at every stage.
2. Exercise competing discoveries and transmissions, active dialogs, leaving/replacing scenes and loading another world. Every undisplayed notice must survive; locks/audio must be released by their owner.
3. Capture a system through real SDM controls. Verify no premature reveal, one assignment, correct location range, no duplicate on repeat capture, and no capture reward for destruction.
4. Scan, collect and unload all eight segments through real controls. Verify locations clear once, credit survives save/load, final instructions occur once, and ordinary non-artifact cargo is unchanged.
5. Check legacy saves with zero through nine credited/held segments and a partly researched artifact. Reject invalid new fields without activating the save.
6. Render all message families with the supplied alien font; verify glyph correspondence, mixed readability, wrapping and ordinary location text. Replay must preserve model state.
7. Run full Mac/Windows validation and Windows desktop acceptance. Final transmitter manufacture/use requires its own original-path trace; displaying instructions alone does not prove campaign completion.
