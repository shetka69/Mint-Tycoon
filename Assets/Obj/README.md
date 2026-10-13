# Supplied object set

User-supplied originals are kept here separately from the earlier Blender model set. These files are not claimed as original project-authored artwork.

| Source | Gameplay role |
| --- | --- |
| `Mine/peshera2.fbx` | Copper mine |
| `blacksmiths-forge/source/Forge.fbx` | Copper furnace |
| `stylized-coin/source/Coin.fbx` | Carried, produced, and delivered coins |
| `animal-company-ore-copper-l/source/item_ore_copper_l.glb` | Copper ore |
| `wooden-cart-01/source/OPM0021.fbx` | Royal delivery wagon |

Run Blender with `--background --python Tools/export_new_objects.py` from the project root, then use **Mint Tycoon → Art → Import and Place New Objects** in the open greybox scene. Gameplay uses normalized prefabs under `_MintTycoon/Art/Prefabs/NewObjects`, generated FBX and portable textures under `Art/Models/NewObjects`, and shared URP materials under `Art/Materials/NewObjects`.

The mine's export is reduced from 436,518 to 31,234 triangles; its original FBX is unchanged. The coin is laid flat in the game copy for stacks. Ore is converted from GLB to FBX; its embedded albedo is baked to a portable texture while ignoring the source's unused all-zero vertex-colour channel, and receives a copper material tint. Wagon base colour, normal, and occlusion textures are exported at 1024 pixels; metallic and roughness maps are packed into the red and alpha channels of a URP metallic/smoothness texture. The supplied 4K cart texture masters remain locally in their original folder and are excluded from Git; the generated textures and remapped materials make the committed game assets portable without them. Export records source-model SHA256 hashes in `Art/Models/NewObjects/manifest.json`.

The old scene visuals remain inactive and their asset files are retained. Gameplay station roots, triggers, recipes, buffers, orders, and wagon route stay in use. The new wagon keeps its two authored wheels, uses the correct local rotation axis, and fills with the new coin visuals according to real order progress. The old order sign is retained on the new cart. Source cart texture masters are only needed when regenerating its textures at a different resolution.
