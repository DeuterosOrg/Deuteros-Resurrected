# Reference artwork

`AocPanelReference.png` is the unmodified [Asana attachment 1215691800680653](https://app.asana.com/app/asana/-/get_asset?asset_id=1215691800680653), supplied with task 1215691800680642. `Screens/Production.tscn` draws only `Rect2(458, 366, 256, 60)` through an `AtlasTexture`, at 128 × 30 game pixels with nearest filtering. This excludes the screenshot's annotation and retains the complete AOC panel, which differs from the smaller production-item sprite.

The screenshot is retained intact for traceability. Its static indicator is reproduced as shown; this does not establish original indicator animation or timing. Original game credits are in the repository README.


`MethanoidMenuReference.png` is the unmodified [attachment 1215691800680630](https://app.asana.com/app/asana/-/get_asset?asset_id=1215691800680630) from task 1215691800680621. `Screens/Base/MenuBase.tscn` draws `Rect2(74, 334, 96, 32)` at 48 × 16 game pixels, in menu row four. It is a passive station-ownership indicator while a ship is orbiting an occupied station; no new communication action or animation is inferred from the still reference.

`DamagedBaseMenuReference.png` is the unmodified [attachment 1215691800680633](https://app.asana.com/app/asana/-/get_asset?asset_id=1215691800680633) from task 1215691800680623. `Buttons/MainMenu/GroundMaterials_Damaged.tres` uses `Rect2(122, 366, 48, 32)`; `Store_Damaged.tres` uses `Rect2(122, 398, 48, 32)`. Both display at 24 × 16 game pixels. The rectangles retain the crossed-out icons and exclude the surrounding red annotation. Normal button art returns after the existing repair simulation clears `BaseDamaged`.

`ProductionRodReference.png` is an unchanged copy of `SourceMaterials/SpriteSheets/UI.png` (SHA-256 `6a8de89008d53b26df9019344e420a384e029dfcf2f997494f5a68c581bcaf90`). `Screens/Production.tscn` uses three 32×9 atlas regions at x544, y424/456/392, in original resource order 130/129/128. They match every decoded original pixel; see [production rod evidence](../../../docs/original-production-rod-evidence.md). The opaque black pixels are intentional.
