"""Run with Blender --background --python ArtSource/Tent/build_tent.py.
Editable canvas panels, hems, ropes and poles remain separate in the .blend.
Only a joined, triangulated export copy is sent to Unity; no textures required.
"""
import bpy, math, json
from pathlib import Path
from mathutils import Vector

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / "Assets/Creek/Art/Tent"
OUT.mkdir(parents=True, exist_ok=True)
bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.delete(use_global=False)
for c in list(bpy.data.collections):
    if c.name != 'Collection' and not c.objects:
        bpy.data.collections.remove(c)
bpy.context.scene.unit_settings.system='METRIC'
bpy.context.scene.unit_settings.scale_length=1.0
parts=[]
palette={"Canvas":"#D5A15B","CanvasSun":"#E6BE79","Binding":"#EBD4A4",
         "Interior":"#66503B","Cedar":"#936541","Rope":"#A49A76"}
mats={}
for name,h in palette.items():
    srgb=[int(h[i:i+2],16)/255 for i in (1,3,5)]
    rgb=[v/12.92 if v<=.04045 else ((v+.055)/1.055)**2.4 for v in srgb]
    m=bpy.data.materials.new(name);m.diffuse_color=(*rgb,1);m.use_nodes=True
    bs=m.node_tree.nodes.get('Principled BSDF')
    bs.inputs['Base Color'].default_value=(*rgb,1);bs.inputs['Roughness'].default_value=.92
    mats[name]=m

def mesh(name,verts,faces,material,solid=0):
    me=bpy.data.meshes.new(name);me.from_pydata(verts,[],faces);me.update()
    ob=bpy.data.objects.new(name,me);bpy.context.collection.objects.link(ob)
    ob.data.materials.append(mats[material]);parts.append(ob)
    if solid:
        mod=ob.modifiers.new("Canvas thickness","SOLIDIFY");mod.thickness=solid;mod.offset=0
    return ob

def beam(name,a,b,r,material,vertices=8):
    a,b=Vector(a),Vector(b)
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices,radius=r,depth=(b-a).length,location=(a+b)/2)
    ob=bpy.context.object;ob.name=name;ob.rotation_euler=(b-a).to_track_quat('Z','Y').to_euler()
    ob.data.materials.append(mats[material]);parts.append(ob);return ob

def cord(name,points,r,material):
    for i in range(len(points)-1):beam(name+" %02d"%i,points[i],points[i+1],r,material,6)

# Left and right canvas are low-resolution tailored grids, bowed between tension points.
def cloth(side,u,y):
    sag=math.sin(math.pi*(y+1.18)/2.36)
    x=side*(1.35*u + .035*math.sin(math.pi*u)*sag)
    z=1.90*(1-u)+.25*u-.075*sag*(1-u)-.075*math.sin(math.pi*u)
    return (x,y,z)
for side,label in [(-1,"Sunward"),(1,"Creekward")]:
    us=[0,.25,.5,.75,1];ys=[-1.18,-.60,0,.60,1.18]
    vs=[cloth(side,u,y) for y in ys for u in us]
    fs=[]
    for j in range(4):
        for i in range(4):
            a=j*5+i
            f=(a,a+1,a+6,a+5)
            fs.append(f if side==1 else f[::-1])
    ob=mesh(label+" tailored roof",vs,fs,"CanvasSun" if side==-1 else "Canvas",.016)
    for face in ob.data.polygons:face.use_smooth=True
    # Short upright skirt creates a useful, grounded volume.
    mesh(label+" reinforced skirt",[(side*1.35,-1.18,.25),(side*1.35,1.18,.25),
         (side*1.39,1.18,.045),(side*1.39,-1.18,.045)],[(0,1,2,3)],"Canvas",.016)
    cord(label+" eave seam",[(side*1.36,y,.25) for y in ys],.024,"Binding")
    cord(label+" front hem",[cloth(side,u,-1.195) for u in us],.027,"Binding")
    cord(label+" rear hem",[cloth(side,u,1.19) for u in us],.019,"Binding")

# Rear wall and dark groundsheet visibly close the volume.
mesh("Rear canvas wall",[(-1.36,1.165,.045),(1.36,1.165,.045),(1.35,1.165,.25),
    (0,1.165,1.89),(-1.35,1.165,.25)],[(0,1,2,3,4)],"Canvas",.015)
mesh("Groundsheet",[(-1.31,-1.17,.06),(1.31,-1.17,.06),(1.31,1.16,.06),(-1.31,1.16,.06)],[(0,1,2,3)],"Interior",.015)
# Dark interior rear lining makes the opening readable without a flat black decal at its mouth.
mesh("Interior rear lining",[(-1.30,1.14,.07),(1.30,1.14,.07),(0,1.14,1.79)],[(2,1,0)],"Interior")

# Door opening widens towards the ground, with folded, tied canvas wings.
for side in [-1,1]:
    verts=[(0,-1.205,1.90),(side*1.35,-1.205,.25),(side*1.38,-1.205,.06),
           (side*.81,-1.225,.07),(side*.85,-1.30,.57),(side*.22,-1.215,1.54)]
    ob=mesh(("Left" if side==-1 else "Right")+" tied-back entrance",verts,
            [(0,1,4,5),(1,2,3,4)],"CanvasSun" if side==-1 else "Canvas",.018)
    cord("Door folded edge",[(side*.22,-1.235,1.54),(side*.86,-1.325,.58),(side*.81,-1.245,.08)],.035,"Binding")
    # Fold ridge is mesh geometry, quiet enough to read as canvas at distance.
    mesh("Folded canvas pleat",[(side*.30,-1.245,1.42),(side*.93,-1.31,.48),
          (side*1.19,-1.215,.27),(side*.85,-1.285,.61)],[(0,1,3),(1,2,3)],"CanvasSun",.01)
    cord("Door rope tie",[(side*.78,-1.34,.58),(side*.96,-1.33,.55),(side*1.04,-1.22,.55)],.018,"Rope")
    # Two ground tie points per side, proportionate to the tent.
    for y in [-.92,.91]:
        cord("Guy line",[(side*1.32,y,.32),(side*1.64,y-.06,.19),(side*1.84,y-.14,.075)],.013,"Rope")
        beam("Cedar tent peg",(side*1.84,y-.14,.0),(side*1.80,y-.12,.19),.031,"Cedar")

cord("Capped ridge seam",[(0,y,cloth(1,0,y)[2]+.013) for y in [-1.18,-.6,0,.6,1.18]],.032,"Binding")
beam("Front ridge support",(0,-1.12,.06),(0,-1.12,1.93),.042,"Cedar")
beam("Back ridge support",(0,1.10,.06),(0,1.10,1.93),.042,"Cedar")
beam("Ridge pole",(0,-1.29,1.94),(0,1.28,1.94),.040,"Cedar")
# Small canvas threshold rolls forward instead of adding camp clutter.
beam("Rolled entrance threshold",(-.72,-1.245,.085),(.72,-1.245,.085),.055,"Canvas")
# Apply scale and unwrap editable mesh parts; modifiers remain editable until export.
bpy.ops.object.select_all(action='DESELECT')
for ob in parts:ob.select_set(True)
bpy.context.view_layer.objects.active=parts[0]
bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
bpy.ops.object.mode_set(mode='EDIT');bpy.ops.mesh.select_all(action='SELECT')
bpy.ops.uv.smart_project(island_margin=.025)
bpy.ops.object.mode_set(mode='OBJECT')
scene=bpy.context.scene
scene['asset_notes']="Alder Creek prospecting tent. Metres; origin at ground centre. Six matte materials, no textures. Roof grids and separate parts remain editable."
# Source file contains only the editable asset.
groups={}
for label in ["01 Canvas and bindings","02 Poles and pegs","03 Guy ropes"]:
    group=bpy.data.collections.new(label);scene.collection.children.link(group);groups[label]=group
for ob in parts:
    material=ob.data.materials[0].name
    label="02 Poles and pegs" if material=="Cedar" else "03 Guy ropes" if material=="Rope" else "01 Canvas and bindings"
    for old in list(ob.users_collection):old.objects.unlink(ob)
    groups[label].objects.link(ob)
bpy.ops.object.select_all(action='DESELECT')
for screen in bpy.data.screens:
    for area in screen.areas:
        if area.type=='VIEW_3D':
            area.spaces.active.shading.color_type='MATERIAL'
            area.spaces.active.region_3d.view_distance=5.5
            area.spaces.active.region_3d.view_location=(0,0,.85)
            area.spaces.active.region_3d.view_rotation=Vector((3,-5,3)).to_track_quat('Z','Y')
scene.render.engine='CYCLES'
scene.render.film_transparent=True
bpy.context.preferences.filepaths.save_version=0
source=ROOT/"ArtSource/Tent/AlderTent.blend"
bpy.ops.wm.save_as_mainfile(filepath=str(source))
# Export evaluated duplicate geometry as a single mesh with material slots.
duplicates=[]
for ob in parts:
    dup=ob.copy();dup.data=ob.data.copy();bpy.context.collection.objects.link(dup);duplicates.append(dup)
bpy.ops.object.select_all(action='DESELECT')
for ob in duplicates:ob.select_set(True)
bpy.context.view_layer.objects.active=duplicates[0]
bpy.ops.object.convert(target='MESH');bpy.ops.object.join()
export=bpy.context.object;export.name="AlderTent"
bpy.context.scene.cursor.location=(0,0,0);bpy.ops.object.origin_set(type='ORIGIN_CURSOR')
bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
# Recalculate consistent outward normals before triangulation.
bpy.ops.object.mode_set(mode='EDIT');bpy.ops.mesh.select_all(action='SELECT')
bpy.ops.mesh.normals_make_consistent(inside=False);bpy.ops.object.mode_set(mode='OBJECT')
tri=export.modifiers.new("Game triangles","TRIANGULATE")
bpy.context.view_layer.objects.active=export;bpy.ops.object.modifier_apply(modifier=tri.name)
bpy.ops.export_scene.fbx(filepath=str(OUT/"AlderTent.fbx"),use_selection=True,object_types={'MESH'},
    axis_forward='-Z',axis_up='Y',apply_unit_scale=True,global_scale=1.0,
    use_mesh_modifiers=True,mesh_smooth_type='FACE',add_leaf_bones=False,bake_anim=False,
    path_mode='AUTO',use_custom_props=False)
stats={"blender":bpy.app.version_string,"vertices":len(export.data.vertices),
       "triangles":len(export.data.polygons),"materials":[m.name for m in export.data.materials],
       "dimensions_m":[round(x,3) for x in export.dimensions],"source_parts":len(parts)}
(ROOT/"ArtSource/Tent/asset-stats.json").write_text(json.dumps(stats,indent=2))
print("TENT_EXPORT "+json.dumps(stats))
