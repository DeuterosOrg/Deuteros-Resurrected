# Self-destruct gameplay implementation

Task **1215685674676229**, 2026-10-02. The control, capture and expiry paths now run in the remake. The task remains partially accepted: original listening and runtime timing/casualty observations still need work. [Original instruction evidence](original-self-destruct-evidence.md) supplies the rules; this page records implementation choices and verification limits.

## Player flow

An installed friendly orbital station exposes **Self-destruct** in the side menu. A hostile station exposes it after a human IOS/SCG occupies its berth. Docking at war starts count 16 on docking completion and retains hostile ownership. Inspecting an undiscovered mechanism opens the existing SDM bulletin and unlocks its research.

Two switches reproduce the traced combinations: switch 1 on/switch 2 off arms at 12; switch 1 off/switch 2 on disarms. Intermediate combinations do neither. Reopening derives the switches from the armed state. At a hostile station, discovered Hyperlight locks switch 1 while switch 2 is on; completed Hyperlight research is not required. Successful defusal captures the station, assigns its ordinal, restores the traced mineral/derrick ranges and leaves a non-Earth captive colony needing repair.

The panel uses ordinary controls and existing fonts. It is a functional remake screen, not recovered original SDM artwork. An audible alarm now uses the recovered original waveform; its calculated stereo timing still needs comparison with an original recording.

## Simulation and persistence

`SdmSystem` owns the mutations. `SpaceStation.SdmCountdown` stores the raw byte range, including the pirate skip bit. The separate saved `SdmTimerRemainder` keeps subsecond progress. Version-1 saves missing these fields start at zero; explicit null, invalid countdowns and invalid timer remainders are rejected. Keep backups: older builds reject the new fields.

The real-time path currently uses one count per second, based on 50 original producer calls and an assumed PAL rate. Godot tree pause also pauses this processing; elapsed frame time is accumulated. Original wall-clock rate, pause behavior and dropped/coalesced producer ticks are not yet measured, so this is a documented provisional mapping.

The simulation path runs after ships, enemy processing and MTX, before display notification. Friendly armed stations expire on that pass. Hostile counts 16/15 survive it; counts below 15 expire. A set high bit is cleared and skips that pass. No pirate event producer was added.

## Loss and recovery

Expiry removes the station, its stocks, installed facilities, production queue and local staff. It removes berthed human hulls with their pilot/cargo/cryo references, while preserving nearby undocked, approaching and in-transit hulls. Non-Earth local shuttles are lost; Earth loses only its orbital docking/docked shuttle. Earth ground stores and services survive.

A non-Earth colony becomes damaged; local ground stocks and derricks are discarded with the lost record. Non-Earth ground crews are removed too: the original uses one local crew roster shared by the ground/shuttle and orbital views. Earth ground crews and crews at other locations survive. [The roster trace](original-self-destruct-evidence.md#local-crew-ownership) records this mapping to the remake's separate lists. Pending enemy attack targets are cleared so a stale attack cannot repopulate the removed station.

Loss news is emitted once, fast-forward stops, and an affected screen exits before the next display event. A surviving ship can retain its interior. Callbacks holding a planet from an obsolete loaded world cannot arm or defuse it.

## Verification

Cases **360–374** cover docking/defusal, manual switches, both expiry paths, grace/skip boundaries, berth versus nearby ships, Earth/non-Earth shuttles, save phase and legacy fields, malformed saves, Hyperlight interlock, discovery, stale callbacks, screen cleanup and one-time capture resources. Fourteen cases reproduced missing behavior before implementation; case 373 verified the cleanup already implemented for the earlier expiry cases. All fifteen pass focused headless checks. Native Mac case 361 passes and its screenshot was reviewed; the clipped warning was shortened and recaptured.

The first full validation attempt stopped after `IMPORT OK` with an editor timer error. Its log is preserved under ignored `artifacts/validation/evidence/self-destruct/import-failure/`. An unchanged strict import probe passed; no filter or code workaround was added. Subsequent Mac and Windows runs stopped at existing preset case 119, then pilot-warning case 129. Focused case 134 reproduced the same issue. Structured Mac save comparisons found only the new real-time timer phase changed across rendered frames. Those tests now freeze GameCore frame processing around the actions, retain their entire-save equality checks and restore processing afterwards. Preset cases 119–122 and pilot-warning cases 129–134 pass focused checks.

Fresh canonical validation at **`862e345dd56931b90781b4ffc04c9a105a425c2e`** passes **374/374** isolated cases on Mac and native Windows, strict import, source startup and Windows release export. Nine Python tests pass on each platform. The native Windows package also passes external headless startup. Both exports have 1,108 pack entries and no test resources; [validation results](validation-results.md#sdm-controls-capture-and-expiry--2026-10-02) record hashes and earlier failed attempts. Existing desktop testing at `8cdd458` remains a separate revision. Automated success does not resolve the audio or original runtime timing/casualty observations above.

Follow-up case **375** reproduced a surviving non-Earth ground roster after loss. The shared destruction handler now clears it using the existing resource helper. Focused Mac cases 375, 362, 363, 366, 367 and 374 pass. Fresh full Mac and Windows validation at **`853986c`** passes **375/375**, strict import, source startup and export; the Windows package also passes startup. Nine Python tests pass on each platform. The same audio and original-runtime acceptance limits remain.

## Recovered alarm

`Sounds/SdmAlarm.wav` contains the original signed-eight-bit sample converted losslessly to stereo 16-bit PCM. The [source trace and conversion](original-self-destruct-evidence.md#alarm-source-and-playback-limits) explain the nominal PAL rate and delayed right channel. One persistent menu player warns at the viewed armed station; global pages, Earth ground and travelling ships are silent. Defusal, expiry and tree exit stop it. Loaded station identity is checked so an obsolete world cannot retain playback.

The alarm uses Master directly; ordinary players use a Game bus feeding Master. While the alarm plays, Game is muted, matching the original channel ownership and normal-sound dispatch lock. Leaving or defusing releases that priority. Master preferences still affect both, and tree pause suspends playback without resetting the waveform.

Cases **376–379** cover waveform bytes, loop bounds, playback rate/volume, arming/defusing, navigation, loaded state, travelling ships, pause, mute, expiry and teardown. The mixer check captures real stereo output while another sound attempts to play, detecting ordinary-sound leakage and checking the right-channel onset. Missing-player and missing-priority failures are retained under ignored `artifacts/validation/evidence/sdm-alarm/`. These checks do not establish listening acceptance or the behavior of sound ID 42's runtime descriptor.

Focused Mac cases 376–379, ambience 200–205 and menu audio 212–219 pass after routing ordinary sounds through Game. Native Mac cases 376–379 also pass with clean exits; the mixer measures a 0.3202-second right onset. Fresh full Mac and native Windows validation at `6ac93012a70d5fa2c4e7c047430500c8a7069e7d` passes **379/379**, strict import, source startup and Windows export, with nine Python tests each. Windows packaged startup also passes; [validation results](validation-results.md#recovered-sdm-alarm--2026-10-02) retain hashes and acceptance limits.
