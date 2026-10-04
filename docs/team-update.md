# Deuteros contribution update

Draft for Discord, 4 October 2026. Counts reconciled with the Windows v5 report; integration testing remains in progress.

> We’ve made substantial progress on the 48-item backlog: 35 items now have implementation evidence, with acceptance still being checked against each task. The work covers gameplay, save/load, ship controls, production, missing artwork and original-game behavior, alongside build instructions and automated validation.
>
> Windows already has 595 automated checks and reported desktop passes across seven task areas. Craig has also confirmed the Service button, resized missing-pod popup, background navigation and blast-door fixes. On Mac, the last complete run passed 621 checks plus 19 tooling tests; a further preset fix and the combined Windows changes are undergoing focused verification. Normal campaign testing has reached orbital construction, mining, freight and equipment manufacture, including a 500-platinum delivery preserved through save/load.
>
> The Windows agent is back on testing. We’ve collected and verified its report and patches, and are checking the combined fixes before returning a fresh Windows build. Next are the remaining campaign, visual and integration gaps. We’ll share the combined work on a separate contribution branch for review, with clear reproduction steps and test evidence. Nothing will be merged into main as part of that handoff.

The 35 implementation-evidence items are not 35 fully accepted bug fixes. Four research/internal requirements are locally accepted; the seven Windows task areas and four player confirmations overlap the backlog and must not be added to those counts. Source and packaged-build coverage differ by revision; see [the backlog](backlog-progress.md) and [Windows evidence](windows-validation-results.md).
