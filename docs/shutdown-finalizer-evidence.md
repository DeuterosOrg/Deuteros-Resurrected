# Shutdown finalizer investigation

Observed on macOS arm64 with official Godot **4.2.2 .NET** (`15073afe3`) and SDK **6.0.428**, 2026-10-02. This addresses a reproduced shutdown failure, not an additional Asana task.

## Reproduction and controls

At gameplay commit `600ae24`, cases 183 and 184 passed their MTX assertions but failed strict process validation during teardown. Logs contained leaked unsafe texture/scene references, `FATAL: Condition "!rc_owner" is true`, leaked renderer allocations and resources still in use. Passing assertions did not certify a clean exit.

A fixed diagnostic matrix preserved every attempt:

| Case / shutdown treatment | Strict failures |
| --- | --- |
| Unmodified case 183 | 4/4 |
| Unmodified case 184 | 1/4 |
| Case 183 with logging only | 4/4 |
| Case 183 with finalizer wait only | 4/4 |
| Case 183 with a 100 ms delay only | 3/4 |
| Case 183 with collection followed by finalizer wait | 0/4 |

The delay was diagnostic only; it is absent from production. GC telemetry printed by the probe describes the last collection and must not be read as a live queue-size measurement.

## Mechanism and regression

Godot's [4.2.2 shutdown tracker](https://github.com/godotengine/godot/blob/4.2.2-stable/modules/mono/glue/GodotSharp/GodotSharp/Core/DisposablesTracker.cs) disposes wrappers found through weak references. A cleared target is skipped, including when its finalizer has not yet released the native reference. The [native language teardown](https://github.com/godotengine/godot/blob/4.2.2-stable/modules/mono/csharp_script.cpp#L140-L186) subsequently clears binding data and reports surviving unsafe references.

A standalone project isolates that boundary without gameplay, input, textures or audio. It briefly holds the finalizer thread, creates 64 native `Resource` wrappers, collects them, and verifies all 64 weak targets are cleared. Immediate shutdown leaks all 64; collecting and waiting while the engine is alive exits cleanly. This demonstrates the cleanup gap independently of the MTX screen.

Permanent **case 276** applies that reproduction to the production window-close path. The gate has a bounded release and its worker never calls Godot. On the old code, assertions pass but strict validation catches the 64 leaked native references. On the corrected code, the process exits cleanly. Do not replace its completion wait with a sleep or suppress teardown errors.

## Change and limits

`GameCore.FinishQuit` still frees scenes and waits for stopped audio to drain. Immediately before quitting, it now collects unreachable wrappers and waits for their finalizers while native bindings remain available. This happens only on exit. No engine pin, gameplay ownership, error filter or input behavior changed.

Cases 183/184/276 each passed four focused repetitions after the fix. Native Mac cases 23/150/183/184/205/264/276 passed; see [the current validation record](validation-results.md#shutdown-finalizer-drain--2026-10-02) for the completed stress/full-run results and any remaining failures. Earlier failed logs remain evidence: this does not retroactively prove the cause of every historical crash. Native Windows source/export and physical window-close acceptance are still required.

Raw diagnostic sources, controls and the standalone project are in ignored `artifacts/research/mtx-shutdown/`. Regression red/green/native and aggregate evidence is under ignored `artifacts/validation/evidence/shutdown-finalizers/`. A fresh checkout can reproduce the guarded boundary with `DEUTEROS_TEST_CASE=276` and `res://Tests/Regression.tscn`, after compiling with the pinned toolchain.
