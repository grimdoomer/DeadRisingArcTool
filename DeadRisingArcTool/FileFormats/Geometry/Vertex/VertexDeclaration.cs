using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Format = SharpDX.DXGI.Format;

namespace DeadRisingArcTool.FileFormats.Geometry.Vertex
{
    public abstract class VertexDeclaration
    {
        public T GetComponent<T>(int index)
        {
            // Get a list of all fields that have a InputSemantic attribute.
            FieldInfo[] fields = this.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance).Where(e => e.GetCustomAttribute<InputSemanticAttribute>() != null).ToArray();

            // Return the value of the field at the specified index.
            return (T)fields[index].GetValue(this);
        }
    }

    public class SkinnedRigidVertex : VertexDeclaration
    {
        [InputSemantic("POSITION", 0, Format.R16G16B16A16_SNorm, 0, 0)]
        public Vector4 Position = new Vector4(0.0f);

        [InputSemantic("NORMAL", 0, Format.R16G16B16A16_SNorm, 16, 0)]
        public Vector4 Normal = new Vector4();

        [InputSemantic("TANGENT", 0, Format.R16G16B16A16_SNorm, 0, 1)]
        public Vector4 Tangent = new Vector4();

        [InputSemantic("TEXCOORD", 0, Format.R16G16_SNorm, 24, 0)]
        public Vector2 Texcoord0 = new Vector2();

        [InputSemantic("TEXCOORD", 1, Format.R16G16_SNorm, 8, 1)]
        public Vector2 Texcoord1 = new Vector2();

        [InputSemantic("BLENDWEIGHT", 0, Format.R8G8B8A8_UNorm, 12, 0)]
        public Vector4 BlendWeight = new Vector4();

        [InputSemantic("BLENDINDICES", 0, Format.R8G8B8A8_SInt, 8, 0)]
        public int[] BlendIndices = new int[4];

        [InputSemantic("TEXCOORD", 2, Format.R32G32B32A32_Float, 0, 0)]
        public Vector2 Texcoord2 = new Vector2();

        [InputSemantic("TEXCOORD", 3, Format.R32G32B32A32_Float, 0, 0)]
        public Vector2 Texcoord3 = new Vector2();
    }
}
