# Godot 4.2.2 retained-input shutdown

Investigated 2026-10-02 after native Mac ACC case 315 passed its gameplay assertions but failed at shutdown with `!rc_owner`. This is an additional engine-compatibility finding, not another completed Asana task.

## Cause and controlled evidence

Godot 4.2.2 debug builds retain parsed input events in a per-frame cache. `FlushBufferedEvents` drains a different queue. The [pinned input source](https://github.com/godotengine/godot/blob/4.2.2-stable/core/input/input.cpp#L942-L1000) clears the debug cache only when another event arrives in a different process frame. A managed event can therefore survive beyond C# teardown. [Upstream fix #92201](https://github.com/godotengine/godot/pull/92201) clears that cache before language shutdown; it was merged after the pinned 4.2.2 release.

A minimal local project isolates this behavior on the actual pinned runtime:

| Control | Result |
| --- | --- |
| No parsed C# event | Exit 0 |
| Parse a C# mouse-motion event, flush, wait two frames, drain finalizers, quit | `!rc_owner`, 25-second timeout |
| Same, explicitly disposing the event | Same fatal error and timeout |
| Replace it on a later frame with another C# event | Same fatal error and timeout |
| Replace it with an event created entirely in GDScript | Exit 0, headless and native Mac |

The small project and all failed/control logs are preserved under ignored `artifacts/research/input-shutdown/`. This establishes a reproducible input-lifetime failure independently of the existing audio/finalizer investigation. It is consistent with case 315's fatal message and teardown phase; no symbolicated stack identified the specific retained object in that historical crash.

## Compatibility correction

After scene/audio release has advanced past the input frame, debug-build shutdown invokes `Code/Utility/ShutdownInput.gd`. It disables input, shortcut and GUI delivery throughout the viewport, including overlays which process while paused, then parses and flushes a native GDScript mouse-motion event. This releases the old per-frame cache without giving its replacement a C# wrapper or activating UI during exit. The existing finalizer drain still runs afterwards.

The helper must create the event in GDScript: the C# replacement control failed. Disabling delivery is also necessary because paused C# input callbacks could otherwise wrap the replacement. Release builds skip this workaround; the affected cache is guarded by `DEBUG_ENABLED` in the pinned engine.

## Game verification and limits

New cases **348–349** reproduce the fatal shutdown through the game's actual window-close notification, both with and without explicit event disposal. Their gameplay markers print successfully before the old shutdown fails; strict full-log and exit checking is essential. Both pass after correction, along with finalizer case 276 and headless case 315.

Native Mac cases **348/349, 315 three consecutive times, 319/320/327/328/329, and SDM installation 338/339** all pass with clean exits. The native batch stopped on no failure; these were planned checks of the new correction, not retries of unchanged failing code. The original case-315 log and crash report remain untouched.

At this checkpoint, the last full Mac aggregate is 347 cases and the last native Windows aggregate is 337 cases; full validation of this newer correction follows separately. Physical Windows mouse/audio/window-close and normal campaign acceptance remain outstanding. No warning/error filter was relaxed, and this does not assert that every possible engine shutdown failure has been eliminated.
