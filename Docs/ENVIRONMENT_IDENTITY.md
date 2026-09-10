# Alder Creek environment identity

This pass makes the existing Alder Creek scene feel like one inhabited creek bank while keeping the pan as the gameplay focal point.

## Visual changes

- Overlapping moss, damp earth, and gravel shelves soften the former single-cut shoreline and emphasize bends in the creek.
- Broken waterline glints and inside-bend gravel bars give the water edge a readable flow without adding visual noise.
- Low ground-contact shapes settle the tent, tool rack, stepping-stone approach, and sluice trestles into the bank.
- Three rounded alder groupings interrupt the repeated pine silhouette and reinforce the Alder Creek identity.
- Quiet distant bank rises create depth behind the tree line.
- Sparse foreground stones and grass frame the landing from the lower corners while leaving the pan silhouette clear.
- A fallen alder branch and localized understory help the creek read as a place rather than a decorated platform.

The implementation is an isolated authored scene layer backed by `EnvironmentArt.asset`. It can be safely reapplied without regenerating or replacing the approved pan, machinery, camp, progression objects, or gameplay scene. `CreekEnvironmentMotion` adds low-amplitude crown sway, grass movement, and shoreline breathing; gameplay never reads it.

## Light verification

- Unity project opened and compiled with Unity 6000.6.0f1 and URP 17.6.0.
- Targeted saved-scene checks passed for bank layers, camp contact, tree silhouettes, motion references, meshes, and materials.
- A fresh Windows build succeeded without running the simulation, progression, or full chapter suites.
- The environment-only Windows player check passed 6 of 6 checks: authored area active, required visual groups present, camp grounding present, supported shaders, runtime foliage motion, and camp visibility.
- Player and wide environment captures were visually inspected at 1600 x 900. No shader errors, exceptions, or capture failures appeared in the player log.

Local comparison and verification artifacts are in `Playtest/Environment-Before.png` and `Playtest/Environment/`. Builds, captures, logs, and test saves remain intentionally ignored by Git.
