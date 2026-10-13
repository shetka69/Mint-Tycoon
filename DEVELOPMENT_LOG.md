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

## Stage 3 — Infinite copper mine

**Date:** 2026-10-12

- Added a reusable `InteractionZone` that reports player inventories entering and leaving a trigger, including safe handling of overlapping colliders and zone disable.
- Added `MiningStation`, which produces one copper ore every 0.4 seconds for each compatible inventory inside its collection zone. It stops at capacity or when another resource type is carried, and resumes after space becomes available.
- Replaced the mine blockout with a small timber-and-stone copper mine visual and a scene prefab at the planned map coordinates. The collection trigger has a kinematic Rigidbody; decorative props have no colliders. A bright amber floor pad and raised border mark the collection footprint in Game view.
- Added the idempotent Editor setup action at **Mint Tycoon → Stage 3 → Set Up Copper Mine**.
- No furnace/press recipes, production buffers, wagon, orders, economy, or transport were added.

**Validation:** Unity 6.6 batch mode compiled the runtime and Editor code without C# compiler errors and executed the Stage 3 scene setup in an isolated working copy. Follow-up debugging found that the open Unity Editor had retained a pre-Stage-3 scene in Play Mode, so it did not display the mine or run its collection trigger. The scene was reloaded from disk, the trigger footprint was marked visibly, and the resource stack-prefab references were corrected to resolve as GameObjects. The trigger now seeds overlaps at startup as well as handling trigger-enter events, so an idle player already inside the mine zone begins collecting. Play Mode showed copper ore models appearing above the player. The mine produces ore, not coins; coins are made by the later press stage.

## Stage 4 — Copper furnace

**Date:** 2026-10-12

- Added `FurnaceStation` with bounded 12-unit input and output buffers, independent player transfer zones, and automatic one-ore-to-one-ingot processing every 0.5 seconds.
- Added the Copper Furnace prefab to the greybox scene with visibly marked amber input and teal output pads, buffer count labels, and up to six displayed resource stacks per buffer.
- Added the idempotent Editor setup action at **Mint Tycoon → Stage 4 → Set Up Copper Furnace** and documented the stage in the README.
- Coins, the mint press, orders, and transport remain planned for later stages.

**Validation:** Unity 6.6 reimported the restored full scene, rebuilt the furnace prefab and scene instance, and reported the Stage 4 setup complete. The Inspector shows the furnace recipe, both interaction zones, the 12-unit capacities, and buffer visual references. Unity Console currently shows no compiler errors or scene repair errors.

## Stage 5 — Copper mint press

**Date:** 2026-10-12

- Extracted recipe execution, bounded buffers, zone transfers, and buffer visualization into reusable production-station components while preserving the existing furnace component identity.
- Added the copper minting recipe: one copper ingot becomes two copper coins every 0.4 seconds. Output production requires space for both coins.
- Added a reusable Copper Mint Press prefab and scene instance at the planned map coordinates, with visible amber ingot-input and teal coin-output zones, buffer labels, and stack visuals.
- Added the idempotent Editor setup action at **Mint Tycoon → Stage 5 → Set Up Copper Mint Press** and documented the stage in the README.
- Orders, the wagon, gold rewards, and upgrades remain for later stages.

**Validation:** Unity 6.6 imported and compiled the shared runtime and Stage 5 Editor setup with no compiler errors. The Stage 5 setup action created and saved the recipe, press prefab, and greybox scene instance. The Inspector shows the recipe, both zones, 12-unit input/output capacities, the 0.12-second transfer interval, and the two buffer presentation references. The Unity Console shows no errors or warnings after setup. Play Mode behavior was not exercised in this stage.

## Stage 6 — Royal wagon, orders, and gold

**Date:** 2026-10-12

- Added two ScriptableObject orders: 12 copper coins for 50 gold, followed by 16 coins for 70 gold.
- Added the player's separate `GoldWallet`, an order-progress component that awards each reward once, and an automatic wagon receiver limited to the exact active order quantity.
- Replaced the wagon blockout with a reusable Royal Wagon prefab, a visible delivery zone, and a return route with predefined waypoints. After completing an order, the wagon departs and returns before advancing to the next order; the two-order sequence repeats.
- Added an on-screen order, reward, and gold HUD plus a live order sign over the wagon.
- Added the idempotent Editor setup action at **Mint Tycoon → Stage 6 → Set Up Royal Wagon and Orders** and documented the stage in the README.
- Automatic transport and upgrades remain for Stage 7.

**Validation:** Unity 6.6 compiled the runtime and Editor setup without compiler errors. The setup action saved both order assets, the Royal Wagon prefab, player wallet reference, delivery trigger, four route offsets, HUD, and greybox scene. Play Mode confirmed the initial order and gold HUD render; the Console showed no errors or warnings after the final setup. Two asset-name warnings found during setup were corrected by matching each ScriptableObject name to its asset filename. Coin delivery, reward payout, and the wagon route were not manually exercised in Play Mode.

## Stage 9 — Minimal UI and gameplay presentation

**Date:** 2026-10-12

- Replaced the two large HUD panels with two small rounded counters for the order target and gold. Hints fade after 3.5 seconds; a pulsing world marker identifies the next interaction pad. Shortened station labels, reduced joystick opacity, and added safe-area handling.
- Added original low-poly courtyard decorations and new mine, kiln, timber press, wagon, and worker visuals. Existing gameplay objects, triggers, recipes, orders, and upgrade references remain in use. The saved camera now opens with its isometric pose.
- Added a distance-driven worker walk cycle, press animation tied to real production progress, wagon wheel rotation, furnace smoke, recipe spark bursts, and reward/unlock particles.
- Added five original short mono sound clips for mining, pickup, recipe completion, order reward, and conveyor purchase. The reproducible audio generator uses only Python's standard library. No packages or third-party assets were added.
- Added shared compact HUD setup and **Mint Tycoon → Stage 9 → Set Up Minimal Presentation** to rebuild presentation in Edit Mode. The Stage 8 setup now rebuilds the compact HUD.

**Validation:** Unity compiled the runtime and Editor scripts, executed the presentation setup, and rendered the portrait scene. An automated Play Mode walkthrough moved the CharacterController between the real interaction pads: 12 ore were collected, 12 ingots were produced and picked up, 12 coins were picked up and delivered, the first order awarded 50 gold, and conveyor purchase left 10 gold. It confirmed matching resource types, hint fade, guide movement, press movement, live smoke particles, audio-source playback, and the final conveyor-unlocked hint. The temporary walkthrough tool was removed after recording the result; the saved scene starts with an empty inventory and the first order.

**Remaining:** Device/WebGL performance, browser audio activation, playable-ad packaging, and final sound mixing belong to Stage 10.

## Stage 9 update — Artist model import and placement

**Date:** 2026-10-13

- Inspected twenty supplied Blender models, their hierarchy, geometry, material colours, and dimensions. Preserved original source bytes and moved the authoring folder to `ArtSource/Models` so opening the Unity project does not require Blender. Local `.blend1` history and raw-source metadata remain on disk outside Git.
- Added a reproducible FBX exporter, a source/material manifest, Unity importer setup, 27 shared URP materials, and twenty normalized visual prefabs. Studio floors, preview cameras/lights, and example furnace-tray stock are excluded from game exports.
- Replaced the station, worker, resource, conveyor, upgrade, and order-sign presentation; placed crates, barrels, fences, and the anvil workbench near their corresponding work areas. The original gameplay roots, configurations, and interaction zones stay in use.
- Prepared a relaxed worker pose in the export, preserved imported rest rotations during walking, and adapted press travel to the imported parent axes. Added real-order-driven wagon cargo and locked/unlocked upgrade visuals. Reduced the pad's vertical scale, added its walk surface, and raised the world guide above it.
- Added ore visual variants and resource-specific stack spacing while retaining the six-model inventory display limit. Kept the temporary ground for the forthcoming tile set.

**Validation:** Unity imported all twenty FBX models and compiled the runtime/Editor scripts. All twenty visual prefabs use URP materials and contain no studio objects. A Play Mode walkthrough used CharacterController movement along clear routes around the new station body colliders: collected 12 ore, produced and picked up 12 ingots, picked up and delivered 12 coins, earned 50 gold, and retained 10 after conveyor purchase. It confirmed six carried models, correct resource types, worker walk motion, vertical press motion, furnace smoke, wagon cargo, upgraded pad visibility, standing on the pad, hint fade, and the guide above the raised pad. All original `.blend` hashes matched the export manifest. The temporary review tool was removed after validation.
