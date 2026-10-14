"""Blender: select ONLY wooden mesh objects, then Run Script.
Creates a new UV map for selected objects and assigns a shared stylized wood material.
Leaves all other objects unchanged, and saves a new .blend copy.
"""
import bpy
import os
from math import radians

TEXTURE_NAME = 'telega_wood_small.png'
UV_NAME = 'Wood_UV'
MAT_NAME = 'Telega_Stylized_Wood'

selected = [ob for ob in bpy.context.selected_objects if ob.type == 'MESH']
if not selected:
    raise RuntimeError('Выдели деревянные Mesh-объекты в Object Mode и запусти скрипт ещё раз.')

base_dir = os.path.dirname(bpy.data.filepath) if bpy.data.filepath else os.path.expanduser('~')
tex_path = os.path.join(base_dir, TEXTURE_NAME)
if not os.path.isfile(tex_path):
    raise RuntimeError('Рядом с .blend должен лежать файл ' + TEXTURE_NAME)

if bpy.context.object and bpy.context.object.mode != 'OBJECT':
    bpy.ops.object.mode_set(mode='OBJECT')

image = bpy.data.images.load(tex_path, check_existing=True)
image.filepath = tex_path
image.filepath_raw = tex_path
image.colorspace_settings.name = 'sRGB'

mat = bpy.data.materials.get(MAT_NAME) or bpy.data.materials.new(MAT_NAME)
mat.use_nodes = True
nodes = mat.node_tree.nodes
links = mat.node_tree.links
bsdf = next((n for n in nodes if n.type == 'BSDF_PRINCIPLED'), None)
if bsdf is None:
    bsdf = nodes.new('ShaderNodeBsdfPrincipled')
    bsdf.location = (50, 0)
output = next((n for n in nodes if n.type == 'OUTPUT_MATERIAL'), None)
if output is None:
    output = nodes.new('ShaderNodeOutputMaterial')
    output.location = (360, 0)
links.new(bsdf.outputs['BSDF'], output.inputs['Surface'])
tex = nodes.get('Wood Texture') or nodes.new('ShaderNodeTexImage')
tex.name = 'Wood Texture'
tex.image = image
tex.interpolation = 'Linear'
tex.extension = 'REPEAT'
uv = nodes.get('Wood UV') or nodes.new('ShaderNodeUVMap')
uv.name = 'Wood UV'
uv.uv_map = UV_NAME
links.new(uv.outputs['UV'], tex.inputs['Vector'])
links.new(tex.outputs['Color'], bsdf.inputs['Base Color'])
bsdf.inputs['Roughness'].default_value = 0.78

# Each object is automatically unwrapped. For a unique hand-painted atlas,
# additional manual UV adjustments may still be required.
view_layer = bpy.context.view_layer
original_active = view_layer.objects.active
for ob in selected:
    bpy.ops.object.select_all(action='DESELECT')
    ob.select_set(True)
    view_layer.objects.active = ob
    # Avoid changing the source mesh when it is shared with another object.
    if ob.data.users > 1:
        ob.data = ob.data.copy()
    uv_layer = ob.data.uv_layers.get(UV_NAME) or ob.data.uv_layers.new(name=UV_NAME)
    ob.data.uv_layers.active = uv_layer
    bpy.ops.object.mode_set(mode='EDIT')
    bpy.ops.mesh.select_all(action='SELECT')
    bpy.ops.uv.smart_project(island_margin=0.02)
    bpy.ops.object.mode_set(mode='OBJECT')
    # Only selected wood objects receive this new material.
    ob.data.materials.clear()
    ob.data.materials.append(mat)

bpy.ops.object.select_all(action='DESELECT')
for ob in selected:
    ob.select_set(True)
if original_active and original_active.name in bpy.data.objects:
    view_layer.objects.active = original_active
elif selected:
    view_layer.objects.active = selected[0]

# Pack texture in .blend for portability.
image.pack()
source_name = os.path.splitext(os.path.basename(bpy.data.filepath))[0] if bpy.data.filepath else 'Telega'
save_path = os.path.join(base_dir, source_name + '_UV_fixed.blend')
bpy.ops.wm.save_as_mainfile(filepath=save_path)
print('Готово! Исправленная копия:', save_path)
