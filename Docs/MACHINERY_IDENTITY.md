# Sluice and helper identity

The camp machinery now shares one small-scale prospecting language: cedar trestles and sledges carry teal enamel working parts, dark blue iron supplies structure, pale end grain clarifies construction, and brass is reserved for hoops, screens, and moving controls.

## Authored silhouettes

- The permanent sluice is a tapered cedar trough with raised rails, a dark riffle mat, pale removable cleats, an intake classifier, splayed trestles, and an open six-paddle wheel.
- Auto Work is a compact gear barrel on a cedar sledge, with an open flywheel and visible linkage.
- Auto Wash is a rounded enamel header tank on a paired cedar stand, with a bent iron neck and a readable water gauge.
- Auto Collect is a low assay bench with a recessed enamel catch, brass rails, gold findings, and a release lever.
- The sluice classifier uses the same cedar, enamel, iron, and brass family as the helpers.

The implementation remains lightweight procedural geometry, but uses authored profiles, proportions, material placement, and construction details. Existing progression gates still control visibility. Existing automation intent still drives the flywheel and water, with only subtle linkage, gauge, and lever motion added for readability.

`ChapterPlaytest` verifies that the authored sluice and all three helper silhouettes are present in the complete camp alongside the existing progression, override, save, and automation checks.

## Verification

- Windows build: succeeded with Unity 6000.6.0f1 and URP.
- Simulation checks: 83 passed.
- Progression, automation, economy, and save checks: 102 passed.
- Earned Windows chapter at 1920 x 1080: 187 passed, 0 failed.
- Earned Windows chapter at 1280 x 1024: 187 passed, 0 failed.
- Both rendered complete-camp views were inspected. The machinery remains readable at the taller aspect ratio and the pan stays the foreground anchor.

The before reference is saved under `Playtest/Machinery-Before/`. Current chapter captures are under `Playtest/Chapter-1920x1080/` and `Playtest/Chapter-1280x1024/`; these playtest artifacts and logs remain intentionally ignored by Git.
