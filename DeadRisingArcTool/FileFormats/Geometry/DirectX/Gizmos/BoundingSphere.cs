using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeadRisingArcTool.FileFormats.Geometry.DirectX.Shaders;
using DeadRisingArcTool.Utilities;
using SharpDX;
using SharpDX.Direct3D11;

namespace DeadRisingArcTool.FileFormats.Geometry.DirectX.Gizmos
{
    public struct D3DColoredVertex
    {
        public Vector3 Position;
        public Color4 Color;
    }

    public class BoundingSphere : IRenderable
    {
        private const int RingSegments = 32;

        public Vector3 Position { get; set; }
        private Vector4 rotation;
        public Vector4 Rotation
        {
            get { return this.rotation; }
            set { this.rotation = value; this.rotationMatrix = Matrix.RotationQuaternion(new Quaternion(this.rotation)); }
        }
        public float Radius { get; set; }
        public Color4 Color { get; set; }

        private Matrix rotationMatrix;
        
        private D3DColoredVertex[] vertices;
        private SharpDX.Direct3D11.Buffer vertexBuffer;

        private Shader shader;

        public BoundingSphere(Vector3 position, Vector4 rotation, float radius, Color4 color)
        {
            // Initialize fields.
            this.Position = position;
            this.Rotation = rotation;
            this.Radius = radius;
            this.Color = color;
        }

        public bool InitializeGraphics(RenderManager manager)
        {
            // Allocate vertex array.
            this.vertices = new D3DColoredVertex[(RingSegments + 1)];

            // Setup the vertex buffer using the vertex data stream.
            BufferDescription desc = new BufferDescription();
            desc.BindFlags = BindFlags.VertexBuffer;
            desc.CpuAccessFlags = CpuAccessFlags.Read | CpuAccessFlags.Write;
            desc.StructureByteStride = 28;
            desc.Usage = ResourceUsage.Default;
            this.vertexBuffer = SharpDX.Direct3D11.Buffer.Create<D3DColoredVertex>(manager.Device, BindFlags.VertexBuffer, this.vertices);

            // Get the wireframe shader.
            this.shader = manager.ShaderCollection.GetShader(ShaderType.Wireframe);

            return true;
        }

        private void DrawRing(Device device, Vector3 majorAxis, Vector3 minorAxis)
        {
            float angleDelta = MathUtil.TwoPi / (float)RingSegments;
            Vector3 cosDelta = new Vector3((float)Math.Cos(angleDelta));
            Vector3 sinDelta = new Vector3((float)Math.Sin(angleDelta));

            Vector3 incrementalSin = new Vector3(0.0f);
            Vector3 incrementalCos = new Vector3(1.0f);

            for (int i = 0; i < RingSegments; i++)
            {
                Vector3 position = (majorAxis * incrementalCos) + Vector4.Transform(new Vector4(this.Position, 1.0f), this.rotationMatrix).ToVector3();
                position = (minorAxis * incrementalSin) + position;

                this.vertices[i].Position = position;
                this.vertices[i].Color = this.Color;

                Vector3 newSin = incrementalCos * sinDelta + incrementalSin * cosDelta;
                Vector3 newCos = incrementalCos * cosDelta - incrementalSin * sinDelta;
                incrementalSin = newSin;
                incrementalCos = newCos;
            }

            this.vertices[RingSegments] = this.vertices[0];

            // Update the vertex buffer.
            device.ImmediateContext.UpdateSubresource(this.vertices, this.vertexBuffer);

            // Draw the ring to screen.
            device.ImmediateContext.Draw(this.vertices.Length, 0);
        }

        public bool DrawFrame(RenderManager manager)
        {
            // If the radius of this sphere is 0 skip drawing it.
            if (this.Radius == 0.0f)
                return true;

            // Set the primitive type to line strip.
            manager.Device.ImmediateContext.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.LineStrip;

            // Set the vertex buffer.
            manager.Device.ImmediateContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(this.vertexBuffer, 28, 0));

            // Setup the wireframe shader.
            this.shader.DrawFrame(manager);

            Vector3 xaxis = new Vector3(this.Radius, 0.0f, 0.0f);
            Vector3 yaxis = new Vector3(0.0f, this.Radius, 0.0f);
            Vector3 zaxis = new Vector3(0.0f, 0.0f, this.Radius);

            DrawRing(manager.Device, xaxis, zaxis);
            DrawRing(manager.Device, xaxis, yaxis);
            DrawRing(manager.Device, yaxis, zaxis);

            return true;
        }

        public void DrawObjectPropertiesUI(RenderManager manager)
        {

        }

        public void CleanupGraphics(RenderManager manager)
        {
            throw new NotImplementedException();
        }
    }
}
