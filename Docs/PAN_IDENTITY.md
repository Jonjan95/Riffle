# Riffle signature pan

Branch: `pan-identity`, based on the approved `ab0f94c` Alder Creek polish.

## Visual direction

The pan is now a saved hero asset with an intentionally shaped cross section, a darker outer skirt, a thicker rolled satin-brass rim, and two broad beveled enamel grips. The rear wall rises slightly above the front working lip. Authored teal color regions across the shoulder, slope, and floor make the bowl read as a cup rather than a flat disk with outline rings.

The front keeps four functional visual riffles, now formed as wider sculpted ribs with quiet staggered ends. Upgraded levels add fine brass crowns. A small enamel maker medallion with an `r` and creek underline sits on the rear shoulder; three understated marks identify the front pouring lip. Pan-level inlays sit on the right grip.

Gold uses a dedicated irregular, folded flake mesh with asymmetric edges and varied facets. The existing small nugget remains thicker through the established rendering scale. The silt mound and charcoal pocket have their own shaded mesh regions, separating fine material from stones and bright gold. Material positions, quantities, readiness, and gold retention still come from the existing simulation.

Work has slightly more visible lateral rocking, using the same agitation and phase. Wash retains its existing forward tilt and outflow; the shallow water band now appears only during washing. Ready gold still settles into the existing front concentrate pocket with the approved short reveal glint.

## Implementation

A hybrid authoring approach: explicit cross sections, beveled grip outlines, color regions and a custom flake shape are baked into `Assets/Creek/Generated/PanIdentity.asset`. They are saved scene geometry, not a new runtime pan generator. `PanIdentity.ApplyAndBuild` replaces only the three pan geometry/detail groups and their own generated subassets. It does not regenerate the camp, reposition the camera, or modify progression.

`Creek/PanSurface` is a pan-specific URP shader based on the existing soft/cel lighting. It reads the authored vertex colors and adds a restrained broad satin highlight. Existing environment shaders and materials are unchanged. Grains without authored colors keep their existing matte rendering.

`PanView` continues to render the same simulation. It selects the new gold mesh, retains the authored silt material, aligns the upgrade crowns/inlays, and adjusts only visual rocking and the water-band visibility.

The input mapper, pan simulation, automation controller, game commands, progression/economy, and save system have no changes. The 0.6-second Auto Collect reveal and instant manual override are intact.

## Verification and captures

- Signature-pan editor checks: saved mesh/reference presence, authored bowl depth/colors, upward-facing floor, upward-facing gold facets, and visible riffle top surfaces.
- Existing saved-scene/material/missing-script checks and editor startup checks passed.
- 83 simulation checks and 102 progression/automation/save checks passed.
- Windows manual smoke at 1366 x 768: 40 checks passed.
- Full earned chapter at 1920 x 1080 and 1280 x 1024: 185 checks passed at each size, including upgrades, assistance, manual priority, save/load, reset, and camp milestones.
- Player logs contain no shader errors or exceptions. Visible captures were reviewed for pan shape, material stages, gold, and upgraded appearance. No hardware performance benchmark is claimed.

Before captures: `Playtest/Pan-Before/Manual/` and `Playtest/Pan-Before/Chapter-1920x1080/`.

After captures: the manual lifecycle PNGs in `Playtest/`, plus `Playtest/Chapter-1920x1080/` and `Playtest/Chapter-1280x1024/`. The clearest completed-pan view is `10-collect-status.png`; `10-black-sand-pocket.png` shows late washing. Motion phases can differ between before/after shots.

Logs: `Logs/pan-identity-build.log`, `Logs/pan-identity-smoke.log`, and `Logs/pan-identity-chapter-*.log`. Dedicated checks: `Playtest/pan-identity-checks.txt`.

Playable deliverables: `Builds/Windows/Riffle.exe` and `Builds/Riffle-Windows.zip`. Verification uses disposable saves and preserves the normal player save.

## Assessment

The bevel grips, teal bowl, maker stamp, and pale front ribs give the pan enough identity to serve as the main visual anchor. The model still fits the surrounding low-detail camp. Next, refine a few individual large-flake silhouettes and consider sparse enamel wear around the grips, guided by hand-play in motion. More ornamental bands or a new control scheme would not help this direction.
