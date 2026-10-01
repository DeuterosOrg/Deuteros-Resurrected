# Visual acceptance references

Asana attachments inspected on 2026-10-02. These observations clarify the open tasks; they are not completion claims. Downloaded working copies are ignored under `artifacts/asana/`. Permanent attachment links remain usable from Asana.

| Task | Reference and observed target |
| --- | --- |
| 1215691800680642 — AOC graphic | [Attachment](https://app.asana.com/app/asana/-/get_asset?asset_id=1215691800680653): production screen replaces the lower staff panel with an A.O.C. plaque and indicator. Check idle and active automated production. |
| 1215691800680621 — menu icon | [Attachment](https://app.asana.com/app/asana/-/get_asset?asset_id=1215691800680630): Methanoid face replaces the lower-left menu area while an IOS orbits Jupiter. The screenshot alone does not establish every peace/war visibility rule. |
| 1215691800680623 — moon-base graphic | [Attachment](https://app.asana.com/app/asana/-/get_asset?asset_id=1215691800680633): two lower-right menu slots have crossed-out buttons while at the Moon orbital bay. Determine unavailable/damaged states from code/manual before limiting this to damage. |
| 1215691800680634 — station stats | [Attachment](https://app.asana.com/app/asana/-/get_asset?asset_id=1215691800680637): station overview shows PRODUCT/NONE, SHUTTLE/IDLE and derricks deployed around the station image. Test their updates as production, ship status and ground deployment change. |
| 1215691800680646 — deposit graphic | [Attachment](https://app.asana.com/app/asana/-/get_asset?asset_id=1215691800680651): deposit analysis has a small platform/station graphic below the selected planet name, to the left of its mineral list. This refers to `ResourceMap.tscn`, not necessarily the separate mining-count screen. |
| 1215691800680640 — time animation | [Attachment](https://app.asana.com/app/asana/-/get_asset?asset_id=1215691800680654): annotation identifies the top-left time-control device. A still image establishes location, not frame sequence, cadence or interaction timing. |
| 1215691951441144 — production animation | [Attachment](https://app.asana.com/app/asana/-/get_asset?asset_id=1215691800680620): annotation identifies the rod between the open-book platform and the vertical progress column. This is separate from the product's three construction sprites. A still image does not establish animation timing. |

`SourceMaterials/SpriteSheets/UI.png` and `ui_buttons.png` contain original-style screen/UI artwork, including alternate button states. Inspect these before creating replacement art. Existing task titles can be misleading: compare attachments to the actual scene and record which state was reproduced.

The production-staff colour report (1214891399253092) has a concrete code cause: `BaseData.Blue` passes byte values `(0,34,136,255)` to Godot's float colour constructor. The intended `#002288` is also declared in `Torso.ProductionStaffColor`. Regression cases cover the roster background, cryopod text and the other shared palette values.
