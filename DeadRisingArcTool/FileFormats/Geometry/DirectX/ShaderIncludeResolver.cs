using SharpDX.D3DCompiler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadRisingArcTool.FileFormats.Geometry.DirectX
{
    public class ShaderIncludeResolver : Include
    {
        /// <summary>
        /// Directory to search for included files in.
        /// </summary>
        public string IncludeDirectory { get; protected set; }

        public IDisposable Shadow { get; set; }

        /// <summary>
        /// Initializes the include resolver using the path provided
        /// </summary>
        /// <param name="includeDirectory">Directory to search for included files in</param>
        public ShaderIncludeResolver(string includeDirectory)
        {
            // Initialize fields.
            this.IncludeDirectory = includeDirectory;
        }

        public void Close(Stream stream)
        {
            // Close the stream.
            stream.Close();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public Stream Open(IncludeType type, string fileName, Stream parentStream)
        {
            // Look for shaders in the startup directory.
            string shaderPath = this.IncludeDirectory + "\\" + fileName;

            // Open the shader in a new file stream.
            return new FileStream(shaderPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }
    }
}
