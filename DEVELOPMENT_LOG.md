# Development Log

## Stage 0 — Project foundation

**Date:** 2026-10-12

- Inspected and retained the existing Unity project and starter scene/settings.
- Confirmed Unity Editor **6000.6.5f1**, Universal Render Pipeline **17.6.0**, Input System **1.20.0**, and Unity Test Framework **1.8.0** from the editor log and package manifest/cache.
- Added the `_MintTycoon` project content structure, runtime assembly, and separate EditMode/PlayMode test assemblies.
- Added data-only definitions for resources, production recipes, orders, and upgrades, plus shared resource identifiers.
- Added the project README and test-folder notes.
- No gameplay systems, levels, external packages, or generated configuration assets were added.

**Validation:** Unity Editor imported the project and compiled `MintTycoon.Runtime` successfully (assembly present in `Library/ScriptAssemblies`; no C# compiler errors in `Logs/Editor.log`). Unity reports the intentionally empty EditMode and PlayMode test assemblies as not compiled because they do not contain test scripts yet.

## Stage 1 — Greybox map, movement, and camera

**Date:** 2026-10-12

- Added a 14 × 18 greybox floor with boundaries and labelled layout blocks at the planned mine, furnace, press, wagon, and upgrade-pad locations.
- Added a capsule player prefab with a `CharacterController`, camera-relative WASD/arrow-key movement, and a touch virtual joystick.
- Added a smooth-follow orthographic camera at a 45° yaw and 50° downward pitch, with orthographic size 11.
- Set project default resolution to 1080 × 1920 and portrait orientation; set WebGL default resolution to 1080 × 1920.
- Created and opened `Assets/_MintTycoon/Scenes/MintTycoon_Greybox.unity`; made it the first enabled build scene while retaining the starter scene.
- No resource, production, order, wagon behavior, or economy systems were implemented.

**Validation:** Unity compiled the runtime and Editor assemblies after adding explicit Input System and UI references to the runtime assembly. The greybox scene was regenerated and opened in the Editor. A Play Mode check confirmed the camera follow, circular joystick graphic, and movement from a WASD input. The Game view was Free Aspect during the check; set it to 9:16 to review the intended portrait composition. No C# compiler errors remain in the current compile.

## Stage 2 — Resources, inventory, and carried stack

**Date:** 2026-10-12

- Added a player inventory with a 12-unit capacity, single-resource-type storage, atomic add/remove operations, and a contents-changed event.
- Added a reusable carried-stack view that renders up to six resource models above the player and refreshes when inventory contents change.
- Created ScriptableObject definitions and simple URP material-backed stack prefabs for copper ore, copper ingots, and copper coins.
- Attached the inventory and stack view to the player prefab and greybox scene. The stack starts empty because resource collection belongs to Stage 3.
- Added an idempotent Editor setup menu at **Mint Tycoon → Stage 2 → Set Up Inventory and Resources** for regenerating the assets and component references.
- No mine, furnace, press, resource gathering, or production transfer behavior was added.

**Validation:** Unity imported the new assets and compiled the runtime and Editor assemblies; no C# compiler errors were found. Confirmed that resource definition assets, stack prefabs, and materials were generated, and that the scene and player prefab were updated. The player starts with an empty inventory; after adding units through `PlayerInventory.TryAdd`, the matching resource stack appears. No Play Mode check of this new behavior was performed in this session.
