# Riffle — Alder Creek

A cozy first chapter about turning a manual pan into a small assisted prospecting camp. Built with **Unity 6000.6.0f1 / URP 17.6.0**.

## Play

Run **Play-Riffle.cmd**, or extract **Builds/Riffle-Windows.zip** and run **Riffle.exe** beside its accompanying files.

1. Hold **LMB on the pan** to Work. Keep the pointer still.
2. Hold **Space or RMB** to Wash. When loose dirt stops clearing, Work again.
3. Alternate as needed. When gold glints over the black sand, press **E** to Collect.
4. Collection prepares the next scoop. Spend gold in the small **Tools / Assistance** panel.

There are no gestures, timing windows, missed-gold penalties, or failure states. A few leftover stones never prevent collection. **H / ?** opens the guide, **Esc** pauses and opens save/reset controls, and **M** toggles sound.

## The first chapter

| Improvement | Level 1 | Level 2 | Level 3 |
|---|---:|---:|---:|
| Better pan | 8 gold | 60 gold | 140 gold |
| Better riffles | 10 gold | 50 gold | 120 gold |
| Richer scoop | 12 gold | 90 gold | 200 gold |

The pan accelerates Work and Wash; riffles improve settling; richer scoops add 3 gold per level. Brass pan inlays, raised riffles, and an expanding tool rack make purchases visible.

**Auto Work (80 gold)** requires all three tools at level 1. **Auto Wash (220 gold)** requires Auto Work and all tools at level 2. **Auto Collect (190 gold)** requires Auto Wash and all tools at level 3.

Each helper has a simple **ON/OFF** button in Assistance. Manual holds take priority. With both action helpers on, the pan alternates preparation and washing using the current sediment state. Auto Collect leaves the revealed gold visible for 0.6 seconds before collecting through the same readiness-guarded command.

A first-pan crate, tool rack, sluice screen, crank, water header and catch tray gradually improve the camp. At 40 lifetime gold, the existing sluice begins flowing and pre-screens a little compacted dirt from each new scoop. It produces no passive income.

Common flakes occasionally include a larger flake (every sixth scoop), or a small nugget (every twentieth). Bonuses are only 1–2 gold. Reloading cannot reroll the current scoop's payout.

The recommended path is simulated at approximately **21–29 minutes**, including brief collection and purchase pauses. It is a pacing estimate, not a timer; experienced players can go faster and exploratory players can take longer. See [the progression notes](Docs/PROGRESSION.md).

## Local save

Gold, lifetime gold, tool levels, owned helpers, helper toggles, and completed-pan count save after each collection, purchase, toggle, and on normal exit.

Normal Windows location:

`%USERPROFILE%/AppData/LocalLow/Small River Studio/Riffle - Alder Creek/alder-creek-v1.txt`

The current scoop restarts; there is no offline income. Saves use an atomic replacement with a backup. A corrupt primary falls back to the backup. If neither can be read, the files are preserved and the game displays a save warning. **Esc → Reset local save → Erase and start fresh** clears primary, backup, and temporary data and resets the visible camp.

## Project and checks

Open **Assets/Creek/Scenes/AlderCreek.unity** in Unity 6000.6.0f1. The saved scene and camera are retained. Camp progression objects are small runtime additions; the whole creek is not regenerated.

- **Tools/Check-Code.ps1** compiles the C# sources and runs the original simulation checks plus progression, automation, save/recovery and pacing checks.
- **Tools/Build-Windows.ps1** verifies URP, runs both suites, and builds the saved scene.
- Run the Windows player with **--smoke-test** for the original manual input/runtime checks.
- Run it with **--progression-check** for a full earned chapter, save/load/reset and three subsequent fully assisted scoops.

Verification runs use unique saves under **Playtest/Verification**, never the normal player save. They must use a visible game window for useful screenshots. Logs and captures are local, ignored build artifacts.

Main responsibilities remain: `PanInput` maps held controls, `PanSimulation` processes one pan, `PanAutomation` supplies ordinary intents, `CreekGame` routes actions and collection, `Progression` owns prices/unlocks, `SaveStore` handles local persistence, and `PanView` / `Diorama` / `CampProgressView` render the result.

Presentation notes and before/after capture locations: [Alder Creek polish](Docs/POLISH.md).

The focused pan asset pass is documented in [Signature pan](Docs/PAN_IDENTITY.md).
