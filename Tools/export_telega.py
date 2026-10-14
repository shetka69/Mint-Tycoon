"""Export a triangulated game copy of the supplied cart without repacking its UVs."""
import bpy, bmesh, json, hashlib
from pathlib import Path
root=Path(__file__).resolve().parents[1]
source=root/'Assets/Obj/Telega_FBX_ready/Telega_materials_fixed.fbx'
target=root/'Assets/_MintTycoon/Art/Models/Telega'
target.mkdir(parents=True,exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.fbx(filepath=str(source))
meshes=[o for o in bpy.context.scene.objects if o.type=='MESH']
report=[]
for obj in meshes:
    uv_names=[uv.name for uv in obj.data.uv_layers]
    bm=bmesh.new(); bm.from_mesh(obj.data)
    bmesh.ops.triangulate(bm,faces=list(bm.faces),quad_method='BEAUTY',ngon_method='BEAUTY')
    bm.to_mesh(obj.data);bm.free();obj.data.update()
    if uv_names != [uv.name for uv in obj.data.uv_layers]:raise RuntimeError('UV layers changed: '+obj.name)
    report.append({'name':obj.name,'triangles':len(obj.data.polygons),'uvLayers':uv_names,'materials':[m.name for m in obj.data.materials if m]})
bpy.ops.object.select_all(action='DESELECT')
for obj in meshes:obj.select_set(True)
bpy.ops.export_scene.fbx(filepath=str(target/'Telega.fbx'),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,bake_anim=False,add_leaf_bones=False,path_mode='STRIP',embed_textures=False)
(target/'manifest.json').write_text(json.dumps({'source':str(source.relative_to(root)),'sourceSha256':hashlib.sha256(source.read_bytes()).hexdigest(),'meshes':report},indent=2))
print('Exported Telega:',len(meshes),'meshes,',sum(x['triangles'] for x in report),'triangles. UV channels retained.')
