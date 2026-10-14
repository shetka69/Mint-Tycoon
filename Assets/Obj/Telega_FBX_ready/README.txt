Готовая модель для Unity: Telega_materials_fixed.fbx.
Перетащи этот FBX из Project в сцену. Материалы назначаются автоматически скриптом Editor/TelegaMaterialImporter.cs.
Дерево использует Telega/Wood UV1: существующий Wood_UV (второй канал). Остальные материалы используют первый UV-канал.
Для переноса в другой проект копируй всю папку Telega_FBX_ready, а не только FBX. Скрипт привязан к пути Assets/Obj/Telega_FBX_ready/Telega_materials_fixed.fbx.
Не включай Generate Lightmap UVs: это перезапишет второй канал дерева.
Исходный Telega_UV_fixed.blend не перезаписан. Исправленная копия: Telega_materials_fixed.blend. Резервная копия: Telega_before_material_fix.blend.
UV-карты не изменялись: координаты, имена, порядок, активные слои и флаги render сравнивались до и после. Проверены UV-координаты внутри FBX.
Unity batchmode: 31 меш, 9 деревянных мешей со вторым UV-каналом, все материалы с текстурами, шейдер без ошибок.
Ограничение: Unity предупреждает о самопересекающихся полигонах на Cube.003, Cube.012, Cube.015, Cube.019 и отбрасывает их. Геометрия не исправлялась.
