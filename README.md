# Mint Tycoon

Mint Tycoon is a portrait mobile casual tycoon prototype about running a medieval mint. The project targets Unity 6 and URP. The first milestone is an Editor-playable prototype, with a later WebGL playable-ad build in mind.

## Project status

**Current milestone: Stage 3 — automatic copper-ore collection at an infinite mine.** Furnace and mint-press production are planned for later milestones.

## Architecture

Project-specific runtime code lives under `Assets/_MintTycoon/Scripts` and uses the `MintTycoon` namespace. Configuration assets live in `Configs` and hold designer-authored data; gameplay behavior will be implemented in focused components in the relevant system folders. Resource, recipe, order, and upgrade definitions are data-only ScriptableObjects. No station references or global gameplay manager are introduced at this stage.

The `MintTycoon.Runtime` assembly contains project runtime code. EditMode and PlayMode test assemblies are separated under `Assets/_MintTycoon/Tests` and reference the runtime assembly.

## Folder map

- `Art`, `Audio`, `Configs`, and `Scenes` — authored content and configuration assets.
- `Scripts/Core` — shared domain identifiers and foundational types.
- `Scripts/Player`, `Inventory`, `Resources`, `Production`, `Interaction`, `Orders`, `Economy`, `Upgrades`, `Camera`, `UI`, and `Tutorial` — focused runtime areas for later milestones.
- `Tests/EditMode` and `Tests/PlayMode` — editor and runtime test suites.

## Stage 2 inventory

The player can hold up to 12 units of one resource type at a time. Inventory additions and removals are atomic, and an event notifies presentation code when contents change. The player prefab and greybox scene include an overhead carried-stack view that displays up to six units; the three resource definitions provide copper ore, ingot, and coin visuals. The player starts empty and the mine adds ore as soon as the player enters its marked collection zone.

## Stage 3 mine

The scene contains a reusable Copper Mine prefab with an automatic collection zone, marked by a copper-colored outline on the ground. While the player remains inside the outline, the mine adds one copper ore every 0.4 seconds. It pauses for a full inventory or one carrying another resource type, and resumes when the inventory can accept ore. Leaving the zone clears the mining timer. The mine has no finite stock; its collection trigger uses a kinematic Rigidbody, while decorative mine visuals have no colliders. Furnace and press processing are not part of this milestone.

To inspect or rebuild the mine in Unity, use **Mint Tycoon → Stage 3 → Set Up Copper Mine**. Run the greybox scene and walk into the marked outline; inventory count should increase every 0.4 seconds and up to six ore models should appear above the player. Leave the outline to stop collection. Collection pauses at 12 units and resumes after inventory space is freed.

## Development order

1. Project foundation and architecture.
2. Greybox map, player movement, and camera.
3. Resources, inventory, and carried-resource visuals.
4. Mine, furnace, and mint press.
5. Wagon orders and gold economy.
6. Transport upgrade, tutorial/UX, presentation, and optimization/WebGL preparation.

Work stops at the end of each milestone for review before the next milestone begins.

## Unity setup

Open the project folder containing `Assets`, `Packages`, and `ProjectSettings` with Unity **6000.6.5f1**. Open `Assets/_MintTycoon/Scenes/MintTycoon_Greybox.unity` and set the Game view aspect to **9:16** for the intended portrait preview. Use WASD or arrow keys in the Editor; use the on-screen joystick on touch devices. The configured Universal Render Pipeline assets are in `Assets/Settings`. The package manifest is the source of truth for package dependencies. No third-party paid assets or dependencies have been added.

Run project tests from **Window → General → Test Runner**, selecting EditMode or PlayMode as appropriate. The test assemblies are ready for later milestones.
