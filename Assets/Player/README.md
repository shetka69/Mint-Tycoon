# Supplied player character

The two original FBX files are retained without rewriting their geometry:

- `armystickman.fbx` is the unrigged source mesh. It has an offset pivot and is retained for reference.
- `Walking.fbx` contains the same body/outfit meshes, a Mixamo-named humanoid skeleton, and a 0.95-second walking cycle. This rigged version is used by the game.
- `Textures/SoldierAtlas.jpg` is the `1.jpg` outfit atlas referenced by the supplied files, copied from the accompanying local source package. Its image bytes are unchanged.

These are user-supplied character assets, separate from the earlier Blender model set. The project does not claim authorship of the supplied character or animation.

## Unity integration

The FBX importers remap both materials to shared URP Lit materials in `_MintTycoon/Art/Materials/Characters`. The walking model imports as Humanoid, creates its own avatar, and loops. Horizontal motion is extracted from the clip; `CharacterController` drives movement and the Animator does not apply root motion.

`_MintTycoon/Art/Prefabs/Characters/ArmyWorker.prefab` normalizes the standing character to 1.6 metres with its feet at the gameplay root. The Animator controller blends a generated relaxed standing pose with the supplied walking clip. `PlayerAnimatorView` uses actual movement speed, including collision stops, and adjusts the walk playback rate.

Use **Mint Tycoon → Player → Set Up Animated Character and Floating Joystick** while the greybox scene is open in Edit Mode. This rebuilds only the new character's assets and controls. The earlier character objects are retained inactive, and their model assets stay unchanged. Inventory, resource stack anchor, camera, interaction zones, and scene layout remain attached to the existing Player root.

## Floating joystick

Touch or click anywhere on the game screen, then drag to move relative to that first press. The base appears under the pointer with 12% opacity and the handle with 30% opacity. Both disappear on release. The first pointer owns the gesture; other touches cannot steal or end it. Focus loss, suspension, disabling controls, and screen-size changes clear the input. WASD and arrow keys remain available without displaying the joystick.
