# Artist model workflow

The original twenty Blender files are retained in `Models/`. They are authoring sources; Unity uses the exported FBX files and visual prefabs under `Assets/_MintTycoon/Art`. Local Blender save-history files and old raw-source Unity metadata remain on disk but are excluded from Git.

## Export and import

1. Run Blender in background mode with `--python Tools/export_game_models.py` from the project root. The script reads `ArtSource/Models` and also accepts fresh `.blend` drop-ins in `Assets/Models` as overrides.
2. In Unity Edit Mode, use **Mint Tycoon → Art → Import Model Assets**. This configures metre units, imported normals, disabled cameras/lights/animation, shared URP materials, bottom pivots, and resource/station scale.
3. Use **Mint Tycoon → Art → Use Imported Models in Scene** to fit the set to the existing gameplay objects and zones. This also rebinds presentation animation and upgrade/cargo states.

Exports omit `Preview_Floor`, `Presentation_Floor`, `Camera_Target`, cameras, lights, and the furnace's example tray resources. The generated manifest records source content hashes, triangle counts, and material mappings. Material variants with different colours retain distinct assets even when their original names match.

The worker's arms are lowered and the pickaxe counter-rotated in the exported copy to prepare a relaxed gameplay pose. Original T-pose sources are preserved. The floor upgrade pad has half-height geometry in the Unity visual prefab so its stone lip stays below the CharacterController's step height. The gameplay scene adds a simple walk surface for it and simple body colliders for the three stations; resource models have no individual physics bodies.

## Model mapping

| Source models | Game use |
| --- | --- |
| CopperMine, Furnace, MintPress | Existing production-station visual children |
| PlayerCharacter_Stickman | Player visual with imported limb walk motion |
| Conveyor | Existing automatic ingot conveyor; rollers move after purchase |
| UpgradePad, UpgradePad off | Unlocked and locked purchase-pad variants |
| CopperOre 01–03 | Alternating single-unit ore visuals; one sample on a mine crate |
| CopperIngot, CopperCoin | Single-unit carried and production-buffer visuals |
| CoinStack 03, 06, 10 | Wagon cargo stages at matching delivery thresholds |
| OrderSign | Royal-wagon display with live order progress |
| WoodenCrate, Barrel | Supplies around the mine, press, and wagon |
| AnvilWorkbench | Workshop decor beside the press |
| WoodenFence Corner | Courtyard corner decorations |

Resource quantities remain in the inventory, station, and order components. Visual variants do not create resources. Carried inventory still displays at most six models. The current ground is reserved for replacement with the forthcoming tile set.
