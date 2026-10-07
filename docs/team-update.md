# Deuteros contribution update

Draft for Discord, 7 October 2026. Not posted.

> The contribution covers gameplay, ship controls, production, save/load, recovered artwork and original-game behavior. Of the48 backlog tasks,35 have implementation evidence and four research/internal requirements are locally accepted. Full gameplay and matching Windows acceptance remain separate gates.
>
> Normal play has cleared Sol, built the SCG, researched Hyperlight, promoted two crews to Warlord and recovered the first artifact for12% research. The latest Jupiter defence leaves WAYFARER147 with53 further losses retained. Automated asteroid mining delivered500 silver exactly; Earth has22 Star reserves at day28621. The Centauri expedition capturedCercops, disarmed its SDM and verified cross-system drone/fuel resupply, but lost the subsequent mobile defence. SCG survived with10 drones/all29 crew and returnedEarth at day28366; Cercops was recaptured. All61 station-battle and158 defence losses are retained without replay. Exact reload, clean native exit and original-save restoration pass.
>
> Native progression exposed two corrected defects: fitted tools on friendly DFCC ships opened fleet transfer, and enemy fleets repeatedly attacked already-hostile stations and reset their stocks. Both have reproduced failures, original-source support, regressions and normal campaign checks. Corrected play leaves Proxima idle while allowing a valid Neptune attack and defence.
>
> The current full Mac run passes648/648 regressions, all19 tooling checks, build, strict import/startup and source/log audit with exit0. The first aggregate's unbuilt-station test-fixture failure is retained and corrected. A matching local Windows export passes all1,259 payload hashes and runtime/source checks, but has not run onWindows. Windows' earlier595-case audit and reported desktop coverage keep their original revision limits; earlier shutdown failures remain open.
>
> The main repository's [contribution branch](https://github.com/DeuterosOrg/Deuteros-Resurrected/tree/codex/build-tests-and-gameplay-fixes) contains the accumulated contribution; the current publication adds later campaign evidence and the MTX targeting regression. The observed MTX selector mismatch did not reproduce in saved-state native checks; a new all-nine icon/route regression passes without a runtime change. Next are expedition reserve rebuilding, seven remaining artifact recoveries, later progression, original-behavior gaps and matching Windows acceptance.

See [the backlog](backlog-progress.md), [current validation](validation-results.md#enemy-target-ownership--2026-10-07), [campaign evidence](native-gameplay-results.md#automated-silver-resupply-and-jupiter-defence--2026-10-07) and [Windows results](windows-validation-results.md). Ignored local saves, logs and export packages do not travel with Git.
