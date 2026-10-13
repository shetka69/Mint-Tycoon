"""Export the artist's Blender files to Unity FBX without modifying the sources.

Run with Blender: Blender --background --python Tools/export_game_models.py
Studio floors, cameras, lights, and example tray contents stay out of the game.
"""
import bpy
import hashlib
import json
import math
from pathlib import Path
import re
from mathutils import Vector

PROJECT = Path(__file__).resolve().parents[1]
SOURCE = PROJECT / 'ArtSource/Models'
OUTPUT = PROJECT / 'Assets/_MintTycoon/Art/Models/Imported'
OUTPUT.mkdir(parents=True, exist_ok=True)


def safe_name(value):
    return re.sub(r'[^A-Za-z0-9_]+', '_', value).strip('_')


def material_values(material):
    color = list(material.diffuse_color)
    roughness, metallic = material.roughness, material.metallic
    emission, strength = [0, 0, 0, 1], 0
    if material.use_nodes:
        for node in material.node_tree.nodes:
            if node.type == 'BSDF_PRINCIPLED':
                color = list(node.inputs['Base Color'].default_value)
                roughness = node.inputs['Roughness'].default_value
                metallic = node.inputs['Metallic'].default_value
                if 'Emission Color' in node.inputs:
                    emission = list(node.inputs['Emission Color'].default_value)
                    strength = node.inputs['Emission Strength'].default_value
    return {'color': color, 'roughness': roughness, 'metallic': metallic,
            'emission': emission, 'emissionStrength': strength}


entries = []
shared_materials = {}
sources = {path.name: path for path in SOURCE.glob('*.blend')}
# Accept fresh artist drop-ins in the original Unity folder as overrides.
sources.update({path.name: path for path in (PROJECT / 'Assets/Models').glob('*.blend')})
for source in sorted(sources.values(), key=lambda path: path.name):
    bpy.ops.wm.open_mainfile(filepath=str(source))
    bpy.context.scene.unit_settings.system = 'METRIC'
    bpy.context.scene.unit_settings.scale_length = 1.0
    name = safe_name(source.stem)
    if name == 'PlayerCharacter_Stickman':
        # A relaxed carry/walk pose. The original T-pose and authoring file are retained.
        bpy.data.objects['Arm_L'].rotation_euler.y = math.radians(-65)
        bpy.data.objects['Arm_R'].rotation_euler.y = math.radians(65)
        bpy.data.objects['Pickaxe'].rotation_euler.y = math.radians(-65)
    excluded = {'Preview_Floor', 'Presentation_Floor', 'Camera_Target'}
    selected = [obj for obj in bpy.context.scene.objects
                if obj.type in {'MESH', 'EMPTY'} and obj.name not in excluded
                and not obj.hide_render
                and not obj.name.startswith(('InputTray_CopperOre_', 'OutputTray_Ingot_'))]
    if not any(obj.type == 'MESH' for obj in selected):
        raise RuntimeError('No game meshes in ' + source.name)
    bpy.context.view_layer.update()
    points = [obj.matrix_world @ Vector(corner) for obj in selected
              if obj.type == 'MESH' for corner in obj.bound_box]
    bottom = min(point.z for point in points)
    wrapper = bpy.data.objects.new('ModelRoot', None)
    bpy.context.scene.collection.objects.link(wrapper)
    for obj in selected:
        if obj.parent not in selected:
            world = obj.matrix_world.copy()
            obj.parent = wrapper
            obj.matrix_world = world
    wrapper.location.z = -bottom
    selected.append(wrapper)
    bpy.context.view_layer.update()
    bpy.ops.object.select_all(action='DESELECT')
    for obj in selected:
        obj.hide_set(False)
        obj.select_set(True)
    bpy.context.view_layer.objects.active = wrapper
    mappings = []
    used_materials = {slot.material for obj in selected if obj.type == 'MESH'
                      for slot in obj.material_slots if slot.material is not None}
    for material in sorted(used_materials, key=lambda item: item.name):
        values = material_values(material)
        digest = hashlib.sha256(json.dumps(values, sort_keys=True).encode()).hexdigest()[:10]
        base = re.sub(r'\.\d{3}$', '', material.name)
        key = safe_name(base) + '_' + digest
        shared_materials[key] = {'key': key, **values}
        mappings.append({'sourceName': material.name, 'key': key})
    bpy.ops.export_scene.fbx(
        filepath=str(OUTPUT / (name + '.fbx')), use_selection=True,
        object_types={'MESH', 'EMPTY'}, global_scale=1.0,
        apply_unit_scale=True, apply_scale_options='FBX_SCALE_UNITS',
        use_space_transform=True, bake_space_transform=False,
        axis_forward='-Z', axis_up='Y', use_mesh_modifiers=True,
        mesh_smooth_type='OFF', use_mesh_edges=False,
        add_leaf_bones=False, bake_anim=False,
        use_custom_props=False, path_mode='AUTO', embed_textures=False)
    depsgraph = bpy.context.evaluated_depsgraph_get()
    triangles = 0
    for obj in selected:
        if obj.type != 'MESH':
            continue
        evaluated = obj.evaluated_get(depsgraph)
        mesh = evaluated.to_mesh()
        mesh.calc_loop_triangles()
        triangles += len(mesh.loop_triangles)
        evaluated.to_mesh_clear()
    entries.append({'name': name, 'file': name + '.fbx',
                    'sourceFile': str(source.relative_to(PROJECT)),
                    'sourceSha256': hashlib.sha256(source.read_bytes()).hexdigest(),
                    'triangles': triangles, 'materials': mappings})
    print('EXPORTED', name, triangles, flush=True)
manifest = {'schemaVersion': 1, 'models': entries,
            'materials': list(shared_materials.values())}
(OUTPUT / 'model_manifest.json').write_text(json.dumps(manifest, indent=2) + '\n')
print('MODEL_EXPORT_COMPLETE', len(entries), 'models', len(shared_materials), 'shared materials')
