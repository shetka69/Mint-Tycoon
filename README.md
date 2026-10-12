# Mint Tycoon

Mint Tycoon is a portrait mobile casual tycoon prototype about running a medieval mint. The project targets Unity 6 and URP. The first milestone is an Editor-playable prototype, with a later WebGL playable-ad build in mind.

## Project status

**Current milestone: Stage 8 — contextual onboarding and UX.**

## Architecture

Project-specific runtime code lives under `Assets/_MintTycoon/Scripts` and uses the `MintTycoon` namespace. Configuration assets live in `Configs` and hold designer-authored data; gameplay behavior is implemented in focused components in the relevant system folders. Resource, recipe, order, and upgrade definitions are data-only ScriptableObjects. Buffered production stations share recipe processing, transfer rules, and buffer presentation; each station still owns its interaction zones and bounded buffers. There is no global gameplay manager.

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

To inspect or rebuild the furnace in Unity, use **Mint Tycoon → Stage 4 → Set Up Copper Furnace**. The furnace only produces ingots; the next stage turns those ingots into coins.

## Stage 5 mint press

The Copper Mint Press is placed at the planned map location. Its amber input zone accepts copper ingots into a 12-unit buffer; its teal output zone returns copper coins from a separate 12-unit buffer. The press automatically converts one ingot into two coins every 0.4 seconds. It pauses when fewer than two output slots are available, so it never overfills the buffer. Up to six stored resource models and a live count label are shown at each zone. Resource transfers use the same 0.12-second interval as the furnace.

To inspect or rebuild the press in Unity, use **Mint Tycoon → Stage 5 → Set Up Copper Mint Press**. Orders, wagon delivery, and gold rewards are not part of Stage 5.

## Stage 6 royal wagon and orders

The Royal Wagon starts at its map location with a marked coin-delivery zone. It accepts only the active order's copper coins, transferring one every 0.12 seconds and never accepting surplus. The first order requests 12 coins and awards 50 gold; the second requests 16 coins and awards 70 gold. On completion, the reward is added to the player's separate gold wallet, the wagon travels away along its route and returns with the next order. The two-order sequence repeats. The HUD displays order progress, the reward, and the current gold balance.

To inspect or rebuild this stage in Unity, use **Mint Tycoon → Stage 6 → Set Up Royal Wagon and Orders**. Gold does not occupy the carried-resource inventory.

## Stage 7 automatic conveyor upgrade

The marked upgrade pad costs 40 gold. Entering it with enough gold activates the conveyor between the furnace output and mint press input. It transfers one finished ingot every 0.12 seconds while the press has room. The transfer checks both buffers and compatible recipes before moving anything, so it cannot duplicate or discard ingots when the press is full.

To inspect or rebuild this stage in Unity, use **Mint Tycoon → Stage 7 → Set Up Automatic Conveyor Upgrade**. The belt is shown as locked before purchase and changes appearance when active.

## Stage 8 contextual tutorial

The field guide teaches the full production loop in context: collect ore, load the furnace, move an ingot to the press, deliver coins to the wagon, earn gold, and buy the automatic conveyor. It advances from live inventory, production, order, wallet, and conveyor events, so a hint only changes after the corresponding action succeeds.

To inspect or rebuild this stage in Unity, use **Mint Tycoon → Stage 8 → Set Up Contextual Tutorial**. The guide sits below the order HUD and stays compact in the portrait layout.

## Development order

1. Project foundation and architecture.
2. Greybox map, player movement, and camera.
3. Resources, inventory, and carried-resource visuals.
4. Furnace smelting and visible production buffers.
5. Copper mint press and coin production.
6. Wagon orders and gold economy.
7. Automatic transport and paid upgrades.
8. Contextual onboarding, presentation, and optimization/WebGL preparation.

Work stops at the end of each milestone for review before the next milestone begins.

## Unity setup

Open the project folder containing `Assets`, `Packages`, and `ProjectSettings` with Unity **6000.6.5f1**. Open `Assets/_MintTycoon/Scenes/MintTycoon_Greybox.unity` and set the Game view aspect to **9:16** for the intended portrait preview. Use WASD or arrow keys in the Editor; use the on-screen joystick on touch devices. The configured Universal Render Pipeline assets are in `Assets/Settings`. The package manifest is the source of truth for package dependencies. No third-party paid assets or dependencies have been added.

Run project tests from **Window → General → Test Runner**, selecting EditMode or PlayMode as appropriate. The test assemblies are ready for later milestones.
