# Independent artist cart

`Tools/export_telega.py` creates this game FBX from the supplied `Assets/Obj/Telega_FBX_ready/Telega_materials_fixed.fbx`. Source files are preserved. The game copy is explicitly triangulated to avoid Unity dropping the source's self-intersecting n-gons, retaining both existing UV layers and all 31 mesh names. The manifest records the source hash, material names, and UV layers.

Wood uses the supplied `Telega/Wood UV1` shader and its second UV channel. Automatic lightmap UV generation is disabled. Shared wood, wheel, and fitting materials reference the three supplied textures, imported at a maximum of 1024 pixels for the game. Source image pixels are unchanged. Byte-identical Blender authoring copies and their small wood texture are versioned under `ArtSource/Telega`, outside Unity's asset database. The local working Blender files under `Assets/Obj`, large fix/backup copies and empty generated `Materials` folders are excluded from Git; the source FBX, shader, textures and game export are included. Opening the committed game does not require Blender.

The reusable visual prefab is `Art/Prefabs/Telega/Telega.prefab`. It has four wheel pivots containing each rim and its hubs, a chassis with subtle distance-driven suspension, and a cargo anchor on the bed. Wheel radius is measured from the model, and rotation follows real travel distance with the correct axle direction. The route turning point is centred between the axles rather than the extended handles.

Use **Mint Tycoon → Art → Set Up Independent Telega** in the open greybox scene. The scene contains two independent roots:

- **Royal Wagon** moves along the existing route and owns its order state, receiver subscription, and active cargo. Previous visual sets remain inactive. Its old prefab is unpacked only in this scene to permit reparenting; the original prefab asset is unchanged.
- **Royal Delivery Station** contains the fixed delivery trigger, pad, border markers, and order sign. The pad, trigger and board retain their world positions; the live text is aligned back onto the relocated board. Existing object references are retained. Completing an order still blocks further deliveries until the cart returns and advances to the next order.

The compact HUD and tutorial still reference the same order and trigger. Presentation setup menus preserve the artist cart and resolve the detached sign/zone through the delivery station. Recipes, order amounts, rewards and route waypoints are unchanged. Existing map edits are preserved when the setup saves the open scene.
