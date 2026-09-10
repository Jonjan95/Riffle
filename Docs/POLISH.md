# Alder Creek presentation polish

Feature branch: `alder-creek-polish`, based on the hand-played and approved `f6cda73`.

## Scope

A presentation pass on the existing chapter. The pan simulation, input mapping, automation controller, collection command, progression prices, upgrade effects, and save format are unchanged. The expected chapter remains 20.7–28.7 minutes; Auto Collect still waits 0.6 seconds and manual controls retain immediate priority.

## Pan and material presentation

- A narrow enamel seam defines the bowl's inner well. A soft rear shoulder accent and satin brass lip make the concavity readable without texture noise.
- Three small front pouring marks and thumb-rest rivets clarify the working edge. Purchased riffle levels add thin satin crests; existing front-only geometry and upgrade effects remain intact.
- The pan eases toward rest when gold is ready, presenting the concentrate more clearly. Work and Wash tilts and their simulation-driven response are preserved.
- Warm brown silt separates from cooler, chunky river stones. Silt lightens as the material loosens, with a slightly irregular mound edge and flatter dirt clods.
- Black sand uses finer, flatter grains over an irregular charcoal pocket. The pocket remains under collect-ready gold; particle locations and gold retention are unchanged.
- Gold reads as broad little flakes, with greater thickness reserved for the existing occasional small nugget. A broad satin highlight improves facet contrast.
- Two brief glints start with readiness instead of depending entirely on a global animation phase. They register during the approved 0.6-second assisted reveal; subsequent glints remain restrained.

## Camp and environment

- Added dock pegs and post collars, canvas entrance hems and a tucked-in entrance mat, a lantern carrying loop, and a few shallow stepping stones connecting the existing landing and tent.
- Existing progression props gain crate slat seams, a folded cloth edge, wooden tool handles, a capped helper housing and maker plate, header bracing, and actual catch-tray rims.
- Helper crank motion eases at transitions; the small water stream settles smoothly. These are visual responses to the existing intents, not extra simulation or automation delays.
- The water shader uses broad flowing bands and sparse, broken crest lines. Shoreline accents follow the existing creek, foam drifts more gently, and active sluice ribbons make flow direction more evident.
- Existing reeds continue to sway. Water glints drift subtly and the lantern pulse is calmer. No additional trees, terrain regions, or gameplay objects were introduced.

## Lighting and UI

- Reduced the heavy yellow tint while keeping a warm stylized palette. Softened cel-shadow contrast and the directional shadow strength; grass, foam, water and small embellishments no longer cast distracting tiny shadows.
- Enamel and brass receive restrained, broad highlights. Most surfaces remain matte. Pan shadows remain subdued and gold receives very little shadow contrast.
- Existing upgrade/helper cards now have quiet inset backgrounds. Owned helper cards explicitly show enabled/paused state and manual priority. The compact activity strip aligns with the scoop panel and has a small activity indicator.
- UI positions, controls, panel layout, and the central pan focus are retained.

## Authoring and verification

The existing saved scene is edited in place, without rerunning the original diorama generator. `PolishCreek.ApplyAndBuild` authors only two finishing groups and their private `PolishArt.asset`, updates the existing palette, verifies references/materials, and builds. Reapplying replaces those two generated finishing groups only. Material editing also causes Unity 6000.6 to upgrade the existing mesh serialization format; the original camp mesh geometry is not redesigned.

Verification reports:

| Check | Result |
|---|---|
| Saved scene presentation references, meshes/materials, missing scripts | Passed |
| Editor startup scene and game/camera/pan/diorama references | 2 passed |
| Simulation suite | 83 passed |
| Progression/automation/save suite | 102 passed |
| Manual Windows smoke, 1366 × 768 | 40 passed |
| Earned chapter, 1920 × 1080 | 185 passed |
| Earned chapter, 1280 × 1024 | 185 passed |

Player runs use isolated verification saves. Full chapter checks earn the upgrades, exercise assistance and manual overrides, reload progression and camp milestones, and reset. Final logs are `Logs/polish-build-final.log`, `Logs/polish-final-smoke.log`, and `Logs/polish-final-chapter-*.log`. No shader errors, missing/pink materials, or player exceptions were observed. These checks and captured frames are not a full-duration human playthrough or a hardware performance benchmark.

## Captures and deliverables

- Before: `Playtest/Polish-Before/Manual/` and `Playtest/Polish-Before/Chapter-{resolution}/`, copied from the approved responsiveness build before editing.
- After: the manual lifecycle captures in `Playtest/`, including the new `10-black-sand-pocket.png`.
- After: ten captures each in `Playtest/Chapter-1920x1080/` and `Playtest/Chapter-1280x1024/`. `10-collect-status.png` shows the early gold glint; `07-complete-camp.png` shows the upgraded setup.
- Updated player: `Builds/Windows/Riffle.exe`; portable archive: `Builds/Riffle-Windows.zip`.

The automated chapter capture pauses at milestones for inspection; screenshots do not measure the player's real wait time. Before/after shots use the same verification routes and sizes, but small motion phases can differ.

## Deliberately deferred

Broad ground surfaces, simple tree silhouettes, and the original blocky sluice remain part of the low-detail style. Water remains opaque and graphic; this pass adds neither refraction nor realistic foam simulation. Taller windows still crop peripheral scenery, while the pan and UI stay readable. More elaborate vegetation, bespoke characterful machinery models, or further scene composition would be separate art work.

Recommendation: this is ready for a short visual hand-check of fresh, washing, and fully upgraded states. If the new material palette and front highlights look right in motion, merge this branch; no further correction pass is currently required by the checks or reviewed captures.
