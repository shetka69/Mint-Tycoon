# Mint Tycoon

Mint Tycoon is a portrait mobile casual tycoon prototype about running a medieval mint. The project targets Unity 6 and URP. The first milestone is an Editor-playable prototype, with a later WebGL playable-ad build in mind.

## Project status

**Current milestone: Stage 4 — automatic copper smelting with bounded buffers and visible transfer zones.** The mint press and coin economy are planned for later milestones.

## Architecture

Project-specific runtime code lives under `Assets/_MintTycoon/Scripts` and uses the `MintTycoon` namespace. Configuration assets live in `Configs` and hold designer-authored data; gameplay behavior is implemented in focused components in the relevant system folders. Resource, recipe, order, and upgrade definitions are data-only ScriptableObjects. Stations own their interaction zones and bounded buffers; there is no global gameplay manager.

The `MintTycoon.Runtime` assembly contains project runtime code. EditMode and PlayMode test assemblies are separated under `Assets/_MintTycoon/Tests` and reference the runtime assembly.

## Folder map

- `Art`, `Audio`, `Configs`, and `Scenes` — authored content and configuration assets.
- `Scripts/Core` — shared domain identifiers and foundational types.
- `Scripts/Player`, `Inventory`, `Resources`, `Production`, `Interaction`, `Orders`, `Economy`, `Upgrades`, `Camera`, `UI`, and `Tutorial` — focused runtime areas for later milestones.
- `Tests/EditMode` and `Tests/PlayMode` — editor and runtime test suites.

## Stage 2 inventory

The player can hold up to 12 units of one resource type at a time. Inventory additions and removals are atomic, and an event notifies presentation code when contents change. The player prefab and greybox scene include an overhead carried-stack view that displays up to six units; the three resource definitions provide copper ore, ingot, and coin visuals. The player starts empty and the mine adds ore as soon as the player enters its marked collection zone.

## Stage 3 mine

The scene contains a reusable Copper Mine prefab with an automatic collection zone, marked by a bright amber floor pad and raised border. While the player remains inside the marked pad, the mine adds one copper ore every 0.4 seconds. It pauses for a full inventory or one carrying another resource type, and resumes when the inventory can accept ore. Leaving the zone clears the mining timer. The mine has no finite stock; its collection trigger uses a kinematic Rigidbody, while decorative mine visuals have no colliders.

To inspect or rebuild the mine in Unity, use **Mint Tycoon → Stage 3 → Set Up Copper Mine**. Run the greybox scene and the player starts on the marked pad, so inventory count increases every 0.4 seconds and up to six ore models appear above the player. Leave the pad to stop collection. Collection pauses at 12 units and resumes after inventory space is freed.

## Stage 4 furnace

The Copper Furnace prefab sits at the planned map location and has independent, visibly marked input and output zones. Standing on the amber input pad automatically transfers copper ore from the player inventory into a 12-unit input buffer, one unit every 0.12 seconds. The furnace then turns one ore into one ingot every 0.5 seconds without player input. Ingots accumulate in a separate 12-unit output buffer; production waits when that buffer cannot fit a complete recipe output. The teal output pad transfers ingots back to a compatible player inventory at the same 0.12-second interval. Buffer labels and up to six resource models show stored contents.

To inspect or rebuild the furnace in Unity, use **Mint Tycoon → Stage 4 → Set Up Copper Furnace**. The mint press and coin production are not part of Stage 4.

## Development order

1. Project foundation and architecture.
2. Greybox map, player movement, and camera.
3. Resources, inventory, and carried-resource visuals.
4. Mine and furnace.
5. Mint press, wagon orders, and gold economy.
6. Transport upgrade, tutorial/UX, presentation, and optimization/WebGL preparation.

Work stops at the end of 