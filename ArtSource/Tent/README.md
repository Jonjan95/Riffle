# Alder Creek tent

Created in Blender 5.2.1 LTS using `build_tent.py`. The editable source is
`AlderTent.blend`; Unity consumes `Assets/Creek/Art/Tent/AlderTent.fbx` and the
`AlderTent.prefab` beside it. Keeping the source outside Assets means importing
the Unity project does not require Blender.

## Asset design

A compact ridge tent with bowed canvas panels, a short reinforced skirt,
tied-back entrance wings, thick pale canvas hems, a dark groundsheet and rear
lining, cedar poles and pegs, and restrained rope ties. The doorway is actual
geometry with interior depth. Warm matte colors use Riffle's existing Creek/Matte
shader in Unity. No textures, animation, colliders, or new shader dependencies.

The Blender source has 63 named mesh parts in three collections: canvas and
bindings, poles and pegs, and guy ropes. Canvas thickness modifiers remain
editable. UVs are present for possible later texture work. The exported copy is
evaluated, joined, and triangulated: 833 vertices and 1,413 triangles, with six
material slots (six submeshes, not a single material draw). Unity may split
vertices at UV, normal, and material boundaries.

Units are metres, with the origin at the ground centre. The footprint including
guy ropes is approximately 3.74 x 2.64 m and height is 1.99 m. The Unity prefab
faces its opening toward local -Z and retains the FBX axis conversion. The camp
instance uses the existing tent site and orientation.

## Editing and export

Open `AlderTent.blend` for direct mesh editing. Save a separate working copy
before regenerating: running `blender --background --python
ArtSource/Tent/build_tent.py` recreates the source and FBX from the script and
overwrites manual edits. Script changes are the reproducible authoring path.

For a manually edited source, export an evaluated duplicate of the three asset
collections: join, set the origin to world zero, apply transforms, recalculate
normals and triangulate. Export only that selected mesh to `AlderTent.fbx`, with
-Z forward, Y up, scale 1, Apply Unit enabled, modifiers enabled, Face smoothing,
and animation disabled. Keep the six material names unchanged: Canvas,
CanvasSun, Binding, Interior, Cedar, Rope. Do not save the joined export copy
over the editable source. The equivalent export settings are at the end of
`build_tent.py`.

In Unity, use **Riffle > Import Blender tent and light build** to update material
remaps, rebuild the prefab and camp instance, verify import, and build Windows.
The FBX importer persistently maps the named materials to the six adjacent
`.mat` assets. The prefab also references those same materials.

## Scene integration and verification

`TentIdentity.cs` replaces the old tent's contribution in affected combined
scenery batches. `TentIntegration.asset` stores those replacement batches;
retained source meshes and the original shared art libraries remain intact.
Only stones and reeds within the tent footprint are cleared to prevent them
protruding through the newly open entrance. Other scenery and the normal camera
are preserved. Reapplying the integration removes unused replacement batches.

Light verification opens the saved scene, checks prefab scale, complexity and
materials, and builds `Builds/Windows/Riffle.exe`. Launching that build with
`--tent-check` uses an isolated save, checks tent visibility and supported
materials, and briefly exercises Work/Wash. It writes four runtime checks and
two screenshots under `Playtest/Tent`. The detail screenshot uses a temporary
inspection camera only in that opt-in mode. No full chapter, regression suite,
or multi-resolution pass is required for this asset change.
