# Riffle — Alder Creek

The current build uses **separate held Work and Wash actions**, with the actual riffles facing the player. The saved creek, camera, art direction, progression panel, upgrades, and softer audio are preserved.

## Play

Double-click **Play-Riffle.cmd**, or extract **Builds/Riffle-Windows.zip** and run **Riffle.exe** beside its accompanying files. The first scoop is already loaded.

1. Keep the cursor still on the pan and hold **LMB for about 3 seconds**. Watch gentle rocking, dirt loosening, and stones moving toward the front riffles.
2. Release LMB and hold **Space or RMB**. The pan tilts farther forward; water carries the loose dirt out. After that dirt clears, compacted material remains and washing slows to a stop.
3. Return to **LMB**, then wash again. Repeat as needed. Exact timing is unnecessary. A longer initial work hold (about 10–12 seconds) can prepare the whole scoop for one wash.
4. When **Collect gold [E]** appears, release and press **E once**. Gold is collected and the next scoop is prepared. Leftover stones do not block collection.

For a direct comparison, try only Space on a fresh scoop: the initial loose surface dirt clears, but all bulk stones and compacted dirt remain. Try only LMB on the next scoop: stones clear and the heavy material settles, but the fine sediment stays. Switching actions always remains available.

Space works anywhere in the focused game. Mouse holds begin on the pan and remain active if the cursor drifts. No circles, mouse-speed thresholds, gestures, or tapping are needed. Both inputs may be held together, supplying both actions explicitly. **H / ?** opens the guide, **Esc** pauses, **M** toggles sound. Progress remains session-only.

## Current mechanic

- **Front geometry:** The existing four raised riffle arcs are physically rotated 180 degrees in the saved scene. They occupy the lower/front 108-degree sector. The rear is smooth. The bowl and surrounding scene were not regenerated.
- **One working direction:** Local negative Z is the front, toward the player. Working lowers the front about 7 degrees; pouring lowers it about 15 degrees. Stones and waste cross that same front lip, and the water falls downward toward the bottom of the screen.
- **Work:** LMB supplies agitation only. It loosens compacted dirt into a washable pool, rocks and rolls stones toward the front, builds stratification and capture, and settles black sand and gold just inside the front riffles. It creates no outflow and removes no fine sediment by itself.
- **Wash:** Space/RMB supplies pouring only. It removes the available loose sediment, then washes black sand more slowly late in the scoop. It adds no loosening, stratification, capture, or shaking. It can assist a stone already worked onto the front slope, but cannot mobilize bulk gravel.
- **Diminishing returns:** A fresh scoop has 18% loose surface dirt and 82% compacted dirt. Work releases compacted dirt; Wash spends the loose pool without replenishing it. Once that pool is empty, water continues but sediment removal stops. More Work restores washable material. Long Work holds saturate preparation while leaving the sediment for Wash. No timing window, penalty, error state, or gold loss is involved.
- **Heavy material:** Gold and black sand gather in the lower bowl just inside the front riffles. Gold stays safe. Readiness depends on clean, exposed gold, not an empty stone count. Collection remains manual.
- **Automation foundation:** `PanIntent.Work` and `PanIntent.Wash` are independent strengths consumed by `CreekGame.ApplyPanActions()`. `Collect()` is a separate readiness-guarded command. Future upgrades can supply each action independently. No automation upgrades or new progression were added.

## Project and rebuild

Open **Assets/Creek/Scenes/AlderCreek.unity** in **Unity 6000.6.0f1**. The pan geometry is already reoriented in the saved scene. **Tools/Build-Windows.ps1** checks and builds that scene without regenerating it. **Tools/Check-Code.ps1** compiles and checks the standalone simulation against the installed Unity libraries.

**Riffle → Reorient saved pan and build** idempotently sets the saved riffle group's rotation to 180 degrees. **Riffle → Create fresh creek scene** intentionally regenerates the procedural scene and replaces generated art; it is unnecessary for this revision. New procedural pans also place riffles at the front.

`PanInput.cs` maps buttons; `PanSimulation.cs` owns material and separate action effects; `PanView.cs` animates the existing pan and material; `CreekGame.cs` routes actions and collection. `Progression.cs`, the environment, and the restrained audio sources retain their existing behavior.

## Verification

See **Playtest/RESULTS.md**, **Playtest/simulation-checks.txt**, and **Playtest/runtime-checks.txt**. Checks cover isolated actions held for 120 seconds, recovery after either plateau, short alternating holds at 30/60/120 updates per second, front-only riffle mesh vertices and rock exits, retained gold, manual collection with obstructed stones, existing upgrades, input guards, and soft audio.

The Windows `--smoke-test` run uses stationary screen points through the camera/input adapter, captures the rendered stages, writes its report, and exits. It is only a verification mode; normal play never enables it. These checks support the next hand-play assessment of clarity and feel.

Current captures include `01-fresh-scoop.png`, `02-hold-work.png`, `03-hold-pour.png`, `04-concentrate.png`, `05-gold-ready.png`, `06-hold-guide.png`, `07-hold-ready-with-stones.png`, `08-wash-only-plateau.png`, and `09-partial-work-plateau.png`. Other screenshots and `SCOOP-RETUNE.md` are historical.
