# Deuteros contribution update

Draft for Discord, 5 October 2026. Not posted.

> We’ve made substantial progress across the 48-item backlog: 35 have implementation evidence, with acceptance tracked separately. The work covers gameplay, ship controls, production, save/load, recovered artwork and original-game behavior, plus build documentation and automated validation.
>
> Windows has 595 automated checks and reported desktop passes across seven task areas. Craig has also confirmed the Service button, missing-pod popup, background navigation and blast-door fixes. The latest complete Mac checkpoint passes 630 checks plus 19 tooling tests; the newer 632-case run stopped on an audio-resource shutdown error after case 264’s assertions passed. The remaining 368 checks now pass separately, and a fresh Windows package is audited. The intermittent Mac failure and a separate Windows shutdown crash remain open; matching Windows gameplay is still pending.
>
> Normal campaign testing has now gone from mining and freight through manufacture of 162 battle drones, successful defence of Moon and Earth, and our first hostile station capture at Jupiter. Stock costs, casualties, ownership and save/reload all reconcile. That run caught another real bug: the first self-destruct discovery message could destroy the station while it blocked the controls. We traced the original behavior, reproduced the failure, applied a small correction and completed the capture through the actual controls. Failed attempts are retained separately from the successful replay.
>
> Next are further campaign progression, matching Windows acceptance and the remaining visual/original-behavior gaps. We’ll present the combined work on a separate contribution branch with reproduction steps and test evidence. Nothing will be merged into main as part of that handoff.

The 35 implementation-evidence items are not 35 fully accepted fixes. Four research/internal requirements are locally accepted; Windows scenarios and player confirmations overlap those tasks. See [the backlog](backlog-progress.md), [campaign evidence](native-gameplay-results.md) and [Windows results](windows-validation-results.md).
