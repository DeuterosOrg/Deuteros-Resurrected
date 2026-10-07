# Deuteros contribution update

Draft for Discord, 7 October 2026. Not posted.

> The contribution covers gameplay, ship controls, production, save/load, recovered artwork and original-game behavior. Of the48 backlog tasks,35 have implementation evidence and four research/internal requirements are locally accepted. Full gameplay and matching Windows acceptance remain separate gates.
>
> Normal play has cleared Sol, built the SCG, researched Hyperlight, promoted two crews to Warlord and recovered the first artifact for12% research. The latest day27945 checkpoint has SCG orbitingEarth with200 drones/249fuel/Raphael29 and one Star drone in reserve. WAYFARER defendedUranus and retains142 drones/63fuel/Floyd35; all58 losses await replacement. Existing routes are unchanged, exact reload passes, and original user saves are restored. Earlier failures and natural crew attrition remain preserved. Both Proxima stations are hostile again; the artifact credit persists.
>
> Native progression exposed two corrected defects: fitted tools on friendly DFCC ships opened fleet transfer, and enemy fleets repeatedly attacked already-hostile stations and reset their stocks. Both have reproduced failures, original-source support, regressions and normal campaign checks. Corrected play leaves Proxima idle while allowing a valid Neptune attack and defence.
>
> The current full Mac run passes648/648 regressions, all19 tooling checks, build, strict import/startup and source/log audit with exit0. The first aggregate's unbuilt-station test-fixture failure is retained and corrected. A matching local Windows export passes all1,259 payload hashes and runtime/source checks, but has not run onWindows. Windows' earlier595-case audit and reported desktop coverage keep their original revision limits; earlier shutdown failures remain open.
>
> The main repository's [contribution branch](https://github.com/DeuterosOrg/Deuteros-Resurrected/tree/codex/build-tests-and-gameplay-fixes) is published through `067f1f2`; subsequent work remains local. Next are WAYFARER replenishment, seven remaining artifact recoveries, later progression, original-behavior gaps and matching Windows acceptance.

See [the backlog](backlog-progress.md), [current validation](validation-results.md#enemy-target-ownership--2026-10-07), [campaign evidence](native-gameplay-results.md#uranus-defence-and-full-scg-fleet--2026-10-07) and [Windows results](windows-validation-results.md). Ignored local saves, logs and export packages do not travel with Git.
