using DeadRisingArcTool.FileFormats.Geometry.DirectX.Shaders;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Device = SharpDX.Direct3D11.Device;

namespace DeadRisingArcTool.FileFormats.Geometry.DirectX.Shaders
{
    /// <summary>
    /// Collection of all the built in shaders
    /// </summary>
    public class BuiltInShaderCollection : IRenderable
    {
        /// <summary>
        /// Dictionary of <see cref="BuiltInShaderType"/> to <see cref="BuiltInShader"/> object instances
        /// </summary>
        public Dictionary<BuiltInShaderType, BuiltInShader> Shaders { get; private set; } = new Dictionary<BuiltInShaderType, BuiltInShader>();

        /// <summary>
        /// Gets the <see cref="BuiltInShader"/> for the specified <see cref="BuiltInShaderType"/>
        /// </summary>
        /// <param name="type">Built in shader type</param>
        /// <returns>An instance of the specified built in shader</returns>
        public BuiltInShader GetShader(BuiltInShaderType type)
        {
            // Check if we have an entry for this shader type.
            if (this.Shaders.ContainsKey(type) == false)
                return null;

            // Return the shader instance.
            return this.Shaders[type];
        }

        #region IRenderable

        public bool InitializeGraphics(IRenderManager manager, Device device)
        {
            // Get a list of all types that have a BuiltInShaderAttribute.
            Type[] types = Assembly.GetEntryAssembly().GetTypes().Where(t => t.GetCustomAttribute<BuiltInShaderAttribute>() != null).ToArray();

            // Loop through all of the types and build the shaders list.
            for (int i = 0; i < types.Length; i++)
            {
                // Create an instance of the type and call the InitializeGraphics routine.
                BuiltInShader shader = (BuiltInShader)Activator.CreateInstance(types[i]);
                if (shader.InitializeGraphics(manager, device) == false)
                {
                    // Failed to initialize the shader, skip for now.
                    continue;
                }

                // Add the shader to the dictionary.
                this.Shaders.Add(shader.ShaderType, shader);
            }

            // Successfully initialized.
            return true;
        }

        public bool DrawFrame(IRenderManager manager, Device device)
        {
            // ShaderCollection is not renderable.
            throw new NotImplementedException();
        }

        public void CleanupGraphics(IRenderManager manager, Device device)
        {
            // Cleanup all shaders in the dictionary.
            BuiltInShaderType[] keys = this.Shaders.Keys.ToArray();
            for (int i = 0; i < keys.Length; i++)
            {
                // Dispose of any resources.
                this.Shaders[keys[i]].CleanupGraphics(manager, device);
                this.Shaders.Remove(keys[i]);
            }
        }

        #endregion
    }
}
