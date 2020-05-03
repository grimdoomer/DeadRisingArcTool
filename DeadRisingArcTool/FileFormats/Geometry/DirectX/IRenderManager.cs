using DeadRisingArcTool.FileFormats.Geometry.DirectX.Shaders;
using DeadRisingArcTool.Forms;
using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadRisingArcTool.FileFormats.Geometry.DirectX
{
    public enum CannedShader
    {
        ModelMesh,
        LevelMesh
    }

    public interface IRenderManager
    {
        /// <summary>
        /// Gets the game resource with the specified file name if it exists or null otherwise
        /// </summary>
        /// <param name="fileName">File name of the resource to get</param>
        /// <returns></returns>
        GameResource GetResourceFromFileName(string fileName);

        /// <summary>
        /// Gets the specified built in shader instance
        /// </summary>
        /// <param name="type">Type of built in shader to get</param>
        /// <returns>Instance of the built in shader</returns>
        BuiltInShader GetBuiltInShader(BuiltInShaderType type);

        // Hackjob mcjankshack this shit in here.
        void SetMatrixMapFactor(Vector4 vec);

        rMotionList GetMotionList();
    }
}
