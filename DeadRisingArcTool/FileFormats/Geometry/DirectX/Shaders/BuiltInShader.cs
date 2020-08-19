using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadRisingArcTool.FileFormats.Geometry.DirectX.Shaders
{
    public enum BuiltInShaderType
    {
        /// <summary>
        /// Used to display colored wireframe meshes
        /// </summary>
        Wireframe,
        /// <summary>
        /// A skinned rigid mesh with 4 bone weights per vertex
        /// </summary>
        SkinnedRigid4W,         // 0x0550228e
        /// <summary>
        /// A skinned rigid mesh with 8 bone weights per vertex
        /// </summary>
        SkinnedRigid8W,         // 0x87a34e22
        /// <summary>
        /// A mesh that is part of the world or environment
        /// </summary>
        Game_LevelGeometry1,    // 0x7976290a
    }

    public class BuiltInShaderAttribute : Attribute
    {
        /// <summary>
        /// Type of built in shader
        /// </summary>
        public BuiltInShaderType ShaderType { get; private set; }

        /// <summary>
        /// One of <see cref="BuiltInShaderType"/> that this shader maps to
        /// </summary>
        /// <param name="type">Type of built in shader</param>
        public BuiltInShaderAttribute(BuiltInShaderType type)
        {
            // Initialize fields.
            this.ShaderType = type;
        }
    }

    public abstract class BuiltInShader : IRenderable
    {
        /// <summary>
        /// Compiled vertex shader
        /// </summary>
        public VertexShader VertexShader { get; protected set; }
        /// <summary>
        /// Compiled pixel shader
        /// </summary>
        public PixelShader PixelShader { get; protected set; }
        /// <summary>
        /// Vertex declaration the vertex shader is bound to
        /// </summary>
        public InputLayout VertexDeclaration { get; protected set; }
        /// <summary>
        /// Array of sampler states used in the vertex shader
        /// </summary>
        public SamplerState[] VertexSampleStates { get; protected set; }
        /// <summary>
        /// Array of sampler states used in the pixel shader
        /// </summary>
        public SamplerState[] PixelSampleStates { get; protected set; }
        /// <summary>
        /// One of <see cref="BuiltInShaderType"/> that this shader maps to
        /// </summary>
        public BuiltInShaderType ShaderType
        {
            get
            {
                // Get the BuiltInShaderAttribute from the parent class type.
                return ((BuiltInShaderAttribute)this.GetType().GetCustomAttributes(typeof(BuiltInShaderAttribute), true)[0]).ShaderType;
            }
        }

        #region IRenderable

        public abstract bool InitializeGraphics(IRenderManager manager, Device device);

        public virtual bool DrawFrame(IRenderManager manager, Device device)
        {
            // Set the vertex and pixel shaders.
            device.ImmediateContext.VertexShader.Set(this.VertexShader);
            device.ImmediateContext.PixelShader.Set(this.PixelShader);

            // If we have any vertex sampler states set them.
            if (this.VertexSampleStates != null)
            {
                // Loop and set all the shader sampler states.
                for (int i = 0; i < this.VertexSampleStates.Length; i++)
                    device.ImmediateContext.VertexShader.SetSampler(i, this.VertexSampleStates[i]);
            }

            // If we have any pixel shader sampler states set them.
            if (this.PixelSampleStates != null)
            {
                // Loop and set all the shader sampler states.
                for (int i = 0; i < this.PixelSampleStates.Length; i++)
                    device.ImmediateContext.PixelShader.SetSampler(i, this.PixelSampleStates[i]);
            }

            // Set the vertex declaration.
            device.ImmediateContext.InputAssembler.InputLayout = this.VertexDeclaration;

            return true;
        }

        public virtual void CleanupGraphics(IRenderManager manager, Device device)
        {
            // Dispose of all resource.
            if (this.VertexDeclaration != null)
                this.VertexDeclaration.Dispose();

            if (this.VertexSampleStates != null)
                for (int i = 0; i < this.VertexSampleStates.Length; i++)
                    this.VertexSampleStates[i].Dispose();

            if (this.VertexShader != null)
                this.VertexShader.Dispose();

            if (this.PixelShader != null)
                this.PixelShader.Dispose();
        }

        #endregion
    }
}
