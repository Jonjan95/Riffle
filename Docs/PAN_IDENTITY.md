# Riffle signature pan

Branch: `pan-identity`, based on the approved `ab0f94c` Alder Creek polish.

## Visual direction

The pan is now a saved hero asset with an intentionally shaped cross section, a darker outer skirt, a plain enamel edge, and an uninterrupted round silhouette. The rear wall rises slightly above the front working lip. Authored teal color regions across the shoulder, slope, and floor make the bowl read as a cup rather than a flat disk with outline rings.

The front keeps four functional visual riffles, now formed as wider sculpted ribs with quiet staggered ends. Upgraded levels add fine brass crowns. The maker medallion, grips, grip-mounted inlays, front decorative marks, and brass rim were removed following visual feedback. The edge now joins the bowl in enamel. Upgrade effects and the existing riffle crowns are retained.

Gold uses a dedicated irregular, folded flake mesh with asymmetric edges and varied facets. The existing small nugget remains thicker through the established rendering scale. The silt mound and charcoal pocket have their own shaded mesh regions, separating fine material from stones and bright gold. Material positions, quantities, readiness, and gold retention still come from the existing simulation.

Work has slightly more visible lateral rocking, using the same agitation and phase. Wash retains its existing forward tilt and outflow; the shallow water band now appears only during washing. Ready gold still settles into the existing front concentrate pocket with the approved short reveal glint.

## Implementation

A hybrid authoring approach: explicit cross sections, color regions and a custom flake shape are baked into `Assets/Creek/Generated/PanIdentity.asset`. They are saved scene geometry, not a new runtime pan generator. `PanIdentity.ApplyAndBuild` replaces only the three pan geometry/detail groups and their own generated subassets. It does not regenerate the camp, reposition the camera, or modify progression.

`Creek/PanSurface` is a pan-specific URP shader based on the existing soft/cel lighting. It reads the authored vertex colors and adds a restrained broad satin highlight. Existing environment shaders and materials are unchanged. Grains without authored colors keep their existing matte rendering.

`PanView` continues to render the same simulation. It selects the new gold mesh, retains the authored silt material, aligns the upgrade crowns, and adjusts only visual rocking and the water-band visibility.

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

The revised design emphasizes the teal bowl's depth, clean circular silhouette, front riffles, and gold presentation. The decorative maker mark, grips, and brass rim did not fit the desired direction and have been removed. Future refinement should concentrate on material motion and individual flake shapes while keeping the shell simple.

Revision verification: the simplified pan is built from `PanIdentity.ApplyAndBuild`. Current revision logs are `Logs/pan-simplify-build.log` and `Logs/pan-simplify-chapter.log`. The chapter capture at `Playtest/Chapter-1920x1080/10-collect-status.png` shows the clean edge and the fully upgraded pan without floating grip inlays.