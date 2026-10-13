"""Run from the project root with Blender --background --python Tools/export_new_objects.py."""
import bpy, json, hashlib, math
import numpy as np
from pathlib import Path
from mathutils import Vector

root = Path(__file__).resolve().parents[1]
out = root/'Assets/_MintTycoon/Art/Models/NewObjects'
out.mkdir(parents=True, exist_ok=True)
textures = out/'Textures'; textures.mkdir(exist_ok=True)
sources = {
    'Mine': 'Assets/Obj/Mine/peshera2.fbx',
    'Furnace': 'Assets/Obj/blacksmiths-forge/source/Forge.fbx',
    'Coin': 'Assets/Obj/stylized-coin/source/Coin.fbx',
    'Wagon': 'Assets/Obj/wooden-cart-01/source/OPM0021.fbx',
    'CopperOre': 'Assets/Obj/animal-company-ore-copper-l/source/item_ore_copper_l.glb',
}
texture_sources = {
    'Furnace': ('Assets/Obj/blacksmiths-forge/textures/RGB_Alpha.tga.png',),
    'Coin': ('Assets/Obj/stylized-coin/textures/Coin2_BaseColor.jpg',),
    'Wagon': ('Assets/Obj/wooden-cart-01/textures/OPM0021_4K_BaseColor.png', 'Assets/Obj/wooden-cart-01/textures/OPM0021_4K_Normal.png', 'Assets/Obj/wooden-cart-01/textures/OPM0021_4K_Occlusion.png'),
}
manifest={'models':[]}
for name, relative in sources.items():
    bpy.ops.wm.read_factory_settings(use_empty=True)
    path = root/relative
    if path.suffix == '.glb': bpy.ops.import_scene.gltf(filepath=str(path))
    else: bpy.ops.import_scene.fbx(filepath=str(path))
    meshes=[o for o in bpy.context.scene.objects if o.type == 'MESH']
    before=sum(sum(len(p.vertices)-2 for p in o.data.polygons) for o in meshes)
    if name == 'Mine':
        for obj in meshes:
            if len(obj.data.polygons) < 500: continue
            bpy.context.view_layer.objects.active=obj
            dec=obj.modifiers.new('Game mesh reduction', 'DECIMATE'); dec.ratio=0.06
            bpy.ops.object.modifier_apply(modifier=dec.name)
    pivot=bpy.data.objects.new(name+'_Root', None); bpy.context.collection.objects.link(pivot)
    for obj in meshes:
        world=obj.matrix_world.copy(); obj.parent=pivot; obj.matrix_world=world
    # Source coin is upright; lay the game copy flat for carried piles and cargo.
    if name == 'Coin': pivot.rotation_euler.x=math.pi/2
    bpy.context.view_layer.update()
    points=[o.matrix_world@Vector(c) for o in meshes for c in o.bound_box]
    low=Vector([min(p[i] for p in points) for i in range(3)])
    high=Vector([max(p[i] for p in points) for i in range(3)])
    pivot.location-=Vector(((low.x+high.x)/2,(low.y+high.y)/2,low.z))
    materials=[]
    for mat in {m for o in meshes for m in o.data.materials if m}:
        bsdf=next((n for n in mat.node_tree.nodes if n.type=='BSDF_PRINCIPLED'),None) if mat.use_nodes else None
        materials.append({'name':mat.name,'color':list(bsdf.inputs['Base Color'].default_value) if bsdf else list(mat.diffuse_color),'metallic':float(bsdf.inputs['Metallic'].default_value) if bsdf else 0,'roughness':float(bsdf.inputs['Roughness'].default_value) if bsdf else .7})
    tex=[]
    if name == 'CopperOre':
        # glTF multiplies its embedded albedo by vertex colour. Bake that authored
        # colour into the existing UVs so Unity's URP Lit retains the copper hue.
        obj=meshes[0]; bpy.context.view_layer.objects.active=obj
        bpy.ops.object.select_all(action='DESELECT'); obj.select_set(True)
        mat=obj.data.materials[0]; nodes=mat.node_tree.nodes; links=mat.node_tree.links
        bsdf=next(n for n in nodes if n.type=='BSDF_PRINCIPLED')
        emission=nodes.new('ShaderNodeEmission')
        if bsdf.inputs['Base Color'].is_linked:
            socket=bsdf.inputs['Base Color'].links[0].from_socket
            # This supplied export contains an unused all-zero vertex-colour layer.
            # Its game shader ignores it; use the embedded albedo instead of black.
            attr=obj.data.color_attributes[0] if obj.data.color_attributes else None
            if attr and all(max(d.color[:3])==0 for d in attr.data):
                socket=next(n for n in nodes if n.type=='TEX_IMAGE' and n.image.name=='Image_1').outputs['Color']
            links.new(socket,emission.inputs['Color'])
        else: emission.inputs['Color'].default_value=bsdf.inputs['Base Color'].default_value
        output=next(n for n in nodes if n.type=='OUTPUT_MATERIAL'); links.new(emission.outputs[0],output.inputs['Surface'])
        image=bpy.data.images.new('CopperOreAlbedo',512,512)
        target_node=nodes.new('ShaderNodeTexImage'); target_node.image=image; nodes.active=target_node
        bpy.context.scene.render.engine='CYCLES'; bpy.context.scene.cycles.samples=1
        bpy.ops.object.bake(type='EMIT',margin=4)
        target=textures/'CopperOre_0.png'; image.file_format='PNG'; image.filepath_raw=str(target); image.save()
        tex.append(str(target.relative_to(root)))
        links.new(bsdf.outputs[0],output.inputs['Surface'])
    for i, source in enumerate(texture_sources.get(name, ())):
        image=bpy.data.images.load(str(root/source),check_existing=False)
        if max(image.size)>1024:
            factor=1024/max(image.size); image.scale(round(image.size[0]*factor),round(image.size[1]*factor))
        target=textures/f'{name}_{i}.png'
        image.file_format='PNG'; image.filepath_raw=str(target); image.save()
        tex.append(str(target.relative_to(root)))
    if name == 'Wagon':
        channels=[]
        for suffix in ('Metallic','Roughness'):
            image=bpy.data.images.load(str(root/f'Assets/Obj/wooden-cart-01/textures/OPM0021_4K_{suffix}.png'),check_existing=False)
            image.colorspace_settings.name='Non-Color'; image.scale(1024,1024)
            channels.append(np.asarray(image.pixels[:],dtype=np.float32).reshape(-1,4)[:,0])
        packed=np.zeros((1024*1024,4),dtype=np.float32); packed[:,0]=channels[0]; packed[:,3]=1-channels[1]
        image=bpy.data.images.new('WagonMetallicSmoothness',1024,1024); image.colorspace_settings.name='Non-Color'
        image.pixels.foreach_set(packed.ravel())
        target=textures/'Wagon_3.png'; image.file_format='PNG'; image.filepath_raw=str(target); image.save()
        tex.append(str(target.relative_to(root)))
    bpy.ops.object.select_all(action='DESELECT')
    pivot.select_set(True)
    for o in meshes:o.select_set(True)
    bpy.ops.export_scene.fbx(filepath=str(out/f'{name}.fbx'),use_selection=True,object_types={'MESH','EMPTY'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,bake_anim=False,add_leaf_bones=False)
    after=sum(sum(len(p.vertices)-2 for p in o.data.polygons) for o in meshes)
    manifest['models'].append({'name':name,'source':relative,'sha256':hashlib.sha256(path.read_bytes()).hexdigest(),'file':str((out/f'{name}.fbx').relative_to(root)), 'materials':sorted(materials,key=lambda m:m['name']),'textures':tex,'sourceTriangles':before,'gameTriangles':after})
(out/'manifest.json').write_text(json.dumps(manifest,indent=2))
print(json.dumps([(m['name'],m['sourceTriangles'],m['gameTriangles']) for m in manifest['models']]))
