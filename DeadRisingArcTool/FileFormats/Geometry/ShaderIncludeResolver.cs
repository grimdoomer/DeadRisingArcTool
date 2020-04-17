using SharpDX.D3DCompiler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadRisingArcTool.FileFormats.Geometry
{
    public class ShaderIncludeResolver : Include
    {
        public IDisposable Shadow { get; set; }

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
            string shaderPath = Environment.CurrentDirectory + "\\" + fileName;

            // Open the shader in a new file stream.
            return new FileStream(shaderPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }
    }
}
