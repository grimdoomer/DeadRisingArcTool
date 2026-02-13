import struct
from typing import Any
import bpy
import mathutils
from mathutils import Vector
import math


class Triangle:
    material_index: int
    vertex_0: int
    vertex_1: int
    vertex_2: int

    def __init__(self, material_index: int, v0: int, v1: int, v2: int):
        self.material_index = material_index
        self.vertex_0= v0
        self.vertex_1 = v1
        self.vertex_2 = v2


class Vertex:
    position: Vector
    normal: Vector
    tangent: Vector
    texcoords: list[Vector]
    bone_weights: Vector
    bone_indices: list[int]

    def __init__(self):
        self.position = Vector()
        self.normal = Vector()
        self.tangent = Vector()
        self.texcoords = [Vector(),
                          Vector(),
                          Vector(),
                          Vector()]
        self.bone_weights = Vector()
        self.bone_indices = [0, 0, 0, 0]


class Material:
    name: str
    base_map: str
    normal_map: str

    def __init__(self):
        self.name = ""
        self.base_map = ""
        self.normal_map = ""


class DeadRisingModel:
    triangles: list[Triangle]
    vertices: list[Vertex]
    materials_dict: dict[str, int]
    materials: list[Material]

    VERSION: int = 1

    def __init__(self):
        self.triangles = []
        self.vertices = []
        self.materials_dict = {}
        self.materials = []

    def write_to_file(self, filename: str):
        """Outputs the class data in binary format."""

        with open(filename, "wb") as f:

            # Write header:
            f.write(struct.pack('<iiii',
                                self.VERSION,
                                len(self.materials),
                                len(self.triangles),
                                len(self.vertices)))

            # Write materials:
            for material in self.materials:
                f.write(material.name.encode())
                f.write(b"\0")
                f.write(material.base_map.encode())
                f.write(b"\0")
                f.write(material.normal_map.encode())
                f.write(b"\0")

            # Write triangles:
            for triangle in self.triangles:
                f.write(struct.pack('i', triangle.material_index))
                f.write(struct.pack('i', triangle.vertex_0))
                f.write(struct.pack('i', triangle.vertex_1))
                f.write(struct.pack('i', triangle.vertex_2))

            # Write vertices:
            for vertex in self.vertices:
                f.write(struct.pack('<fff', vertex.position.x, vertex.position.y, vertex.position.z))
                f.write(struct.pack('<fff', vertex.normal.x, vertex.normal.y, vertex.normal.z))
                f.write(struct.pack('<fff', vertex.tangent.x, vertex.tangent.y, vertex.tangent.z))

                for i in range(0, 4):
                    f.write(struct.pack('<ff', vertex.texcoords[i].x, vertex.texcoords[i].y))

                f.write(struct.pack('<ffff', vertex.bone_weights.x, vertex.bone_weights.y, vertex.bone_weights.z, vertex.bone_weights.w))
                f.write(struct.pack('<iiii', vertex.bone_indices[0], vertex.bone_indices[1], vertex.bone_indices[2], vertex.bone_indices[3]))


def build_mesh_list(context) -> list[Any]:

    scene_objects = list(context.scene.objects)

    # Build a list of scene mesh objects available for export.
    export_objects = []
    for obj in scene_objects:

        # Check if the object is a mesh and visible.
        if obj.type != 'MESH' or obj.visible_get() == False:
            continue

        # Add the object to the export list.
        export_objects.append(obj)

    return export_objects


def change_mesh_up_axis(obj, mesh, to_up_axis='Y', to_forward_axis='-Z'):
    """
    Applies an axis conversion rotation to all selected objects and their mesh data.

    :param to_up_axis: The desired 'up' axis (e.g., 'Y', 'Z', '-X', etc.).
    :param to_forward_axis: The desired 'forward' axis (e.g., 'X', '-Y', 'Z', etc.).
    """

    # Calculate the conversion matrix
    # The 'from' parameters are Blender's default: Z up, -Y forward
    m = axis_conversion(
        from_forward='-Y',
        from_up='Z',
        to_forward=to_forward_axis,
        to_up=to_up_axis
    ).to_4x4()

    # Apply to object's world matrix
    obj.matrix_world = m @ obj.matrix_world

    # Apply the inverse transformation to the mesh data itself
    # to make the local data match the new object orientation
    mesh.transform(m.inverted())


def apply_transforms(obj):

    # Define the context override
    # We specify the 'object' and 'active_object' to be our target object
    # and 'selected_objects' to include only this object.
    override_context = {
        'object': obj,
        'active_object': obj,
        'selected_objects': [obj]
    }

    # Use the context override to call the operator
    # Apply Location, Rotation, and Scale
    with bpy.context.temp_override(**override_context):
        bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)


def find_or_create_material(model_data, context, mesh, material_index) -> int:

    # Make sure the material index is valid.
    if material_index < 0 or material_index >= len(mesh.material_slots):
        return -1

    # Get the material name and see if we already created a descriptor for it.
    material_name = mesh.material_slots[material_index].name
    if material_name in model_data.materials_dict:
        return model_data.materials_dict[material_name]

    # Create a new material and add it to the model data.
    material = Material()
    material.name = material_name

    # TODO

    material_index = len(model_data.materials)
    model_data.materials.append(material)
    model_data.materials_dict[material_name] = material_index

    return material_index


def triangulate_mesh(context, obj) -> None:

    # Check if the mesh already has the triangulate modifier and if not add one.
    if not 'Triangulate' in obj.modifiers:
        triangulate = obj.modifiers.new("Triangulate", type='TRIANGULATE')
        try:
            triangulate.keep_custom_normals = True
        except AttributeError:
            print("Can't set keep custom normals for triangulate modifier. Blender version is newer or something went very wrong.")

    #obj.modifiers['Triangulate']

    context.view_layer.update()


def build_model_data(context, depsgraph) -> DeadRisingModel:
    model_data = DeadRisingModel()

    # Build a list of mesh objects in the scene available for export.
    scene_meshes = build_mesh_list(context)
    for obj in scene_meshes:
        print("Exporting: %s" % obj.name)

        # Ensure mesh is triangulated.
        triangulate_mesh(context, obj)

        # Evaluate the object with modifiers applied.
        obj_eval = obj.evaluated_get(depsgraph)

        # Apply all transforms to the object.
        apply_transforms(obj_eval)

        # Flip axis orientation to Y up -Z forward.
        mesh = obj_eval.to_mesh()
        change_mesh_up_axis(obj_eval, mesh)

        if not mesh.loop_triangles:
            mesh.calc_loop_triangles()

        if obj_eval.matrix_world.determinant() < 0.0:
            mesh.flip_normals()

        # Calculate tangents.
        mesh.uv_layers.active = mesh.uv_layers[0]
        mesh.calc_tangents()

        # Loop through all the triangles.
        vertex_mapping = {}
        for face in mesh.polygons:

            # Get the material index or create a new material for the face.
            material_index = find_or_create_material(model_data, context, obj_eval, face.material_index)

            # calculate vertex indices.
            vert_count = len(model_data.vertices)
            v0 = vert_count
            v1 = vert_count + 1
            v2 = vert_count + 2

            model_data.triangles.append(Triangle(material_index, v0, v1, v2))

            # Make sure the triangle has exactly 3 vertices.
            if len(face.loop_indices) != 3:
                raise Exception("Face is not trianglulated!")

            # Loop and process vertices for the face.
            for loop_index in face.loop_indices:

                vertex = Vertex()

                vertex_index = mesh.loops[loop_index].vertex_index
                vertex.position = Vector(mesh.vertices[vertex_index].co)

                # TODO: normal calculation
                vertex.normal = Vector(mesh.loops[loop_index].normal)
                vertex.tangent = Vector(mesh.loops[loop_index].tangent)
                # print(f"Normal: {vertex.normal} Tangent: {vertex.tangent}")

                # Get up to 4 sets of texcoords from the mesh UV sets.
                texcoord_index = mesh.loops[loop_index].index
                for i in range(0, min(4, len(mesh.uv_layers))):
                    mesh.uv_layers.active = mesh.uv_layers[i]
                    if len(mesh.uv_layers.active.data) == 0:
                        print("bork")
                    else:
                        vertex.texcoords[i] = Vector(mesh.uv_layers.active.data[texcoord_index].uv)

                # TODO: bone weights and indices
                vertex.bone_weights = Vector([1.0, 0.0, 0.0, 0.0])
                vertex.bone_indices = [0, 0, 0, 0]

                model_data.vertices.append(vertex)

    return model_data


def export_model(context, filepath: str):
    print("running write_some_data: %s..."% filepath)

    # Get the scene dependency graph so we can access mesh data with modifiers applied.
    depsgraph = context.evaluated_depsgraph_get()

    # TODO: do we need to exit edit mode to correctly apply object states?
    if bpy.ops.object.mode_set.poll():
        bpy.ops.object.mode_set(mode='OBJECT')

    # TODO: add support for only exporting the selection
    #   objects = context.selected_objects vs objects = scene.objects

    # Build the model data from scene objects.
    model_data = build_model_data(context, depsgraph)

    # Write the output data to file.
    model_data.write_to_file(filepath)

    return {'FINISHED'}


# ExportHelper is a helper class, defines filename and
# invoke() function which calls the file selector.
from bpy_extras.io_utils import ExportHelper, axis_conversion
from bpy.props import StringProperty, BoolProperty, EnumProperty
from bpy.types import Operator


class DeadRisingExporter(Operator, ExportHelper):
    """This appears in the tooltip of the operator and in the generated docs"""
    bl_idname = "dead_rising_exporter.model"  # important since its how bpy.ops.import_test.some_data is constructed
    bl_label = "Write a DRM file"

    # ExportHelper mixin class uses this
    filename_ext = ".drm"

    filter_glob: StringProperty(
        default="*.drm",
        options={'HIDDEN'},
        maxlen=255,  # Max internal buffer length, longer would be clamped.
    )

    # List of operator properties, the attributes will be assigned
    # to the class instance from the operator settings before calling.
    use_setting: BoolProperty(
        name="Example Boolean",
        description="Example Tooltip",
        default=True,
    )

    type: EnumProperty(
        name="Example Enum",
        description="Choose between two items",
        items=(
            ('OPT_A', "First Option", "Description one"),
            ('OPT_B', "Second Option", "Description two"),
        ),
        default='OPT_A',
    )

    def execute(self, context):
        return export_model(context, self.filepath)


# Only needed if you want to add into a dynamic menu
def menu_func_export(self, context):
    self.layout.operator(DeadRisingExporter.bl_idname, text="Dead Rising Model (.drm)")


# Register and add to the "file selector" menu (required to use F3 search "Text Export Operator" for quick access).
def register():
    bpy.utils.register_class(DeadRisingExporter)
    bpy.types.TOPBAR_MT_file_export.append(menu_func_export)


def unregister():
    bpy.utils.unregister_class(DeadRisingExporter)
    bpy.types.TOPBAR_MT_file_export.remove(menu_func_export)


if __name__ == "__main__":
    register()

    # test call
    #bpy.ops.dead_rising_exporter.model('INVOKE_DEFAULT')
    #export_model(bpy.context, r"X:\Dead Rising\Model Injection\Warthog\Export\warthog.drm")
    export_model(bpy.context, r"X:\Dead Rising\Model Injection\Hotdog Cart\hotdog_cart.drm")
    #export_model(bpy.context, r"X:\Dead Rising\Model Injection\Hotdog\hotdog.drm")
