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
    public class ShaderCollection : IRenderable
    {
        /// <summary>
        /// Dictionary of <see cref="ShaderType"/> to <see cref="Shader"/> object instances
        /// </summary>
        public Dictionary<ShaderType, Shader> Shaders { get; private set; } = new Dictionary<ShaderType, Shader>();

        /// <summary>
        /// Gets the <see cref="Shader"/> for the specified <see cref="ShaderType"/>
        /// </summary>
        /// <param name="type">Type of shader</param>
        /// <returns>An instance of the specified shader</returns>
        public Shader GetShader(ShaderType type)
        {
            // Check if we have an entry for this shader type.
            if (this.Shaders.ContainsKey(type) == false)
                return null;

            // Return the shader instance.
            return this.Shaders[type];
        }

        #region IRenderable

        public bool InitializeGraphics(RenderManager manager)
        {
            // Get a list of all types that have a ShaderAttribute.
            Type[] types = Assembly.GetEntryAssembly().GetTypes().Where(t => t.GetCustomAttribute<ShaderAttributeAttribute>() != null).ToArray();

            // Loop through all of the types and build the shaders list.
            for (int i = 0; i < types.Length; i++)
            {
                // Create an instance of the type and call the InitializeGraphics routine.
                Shader shader = (Shader)Activator.CreateInstance(types[i]);
                if (shader.InitializeGraphics(manager) == false)
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

        public bool DrawFrame(RenderManager manager)
        {
            // ShaderCollection is not renderable.
            throw new NotImplementedException();
        }

        public void DrawObjectPropertiesUI(RenderManager manager)
        {

        }

        public void CleanupGraphics(RenderManager manager)
        {
            // Cleanup all shaders in the dictionary.
            ShaderType[] keys = this.Shaders.Keys.ToArray();
            for (int i = 0; i < keys.Length; i++)
            {
                // Dispose of any resources.
                this.Shaders[keys[i]].CleanupGraphics(manager);
                this.Shaders.Remove(keys[i]);
            }
        }

        #endregion
    }
}
