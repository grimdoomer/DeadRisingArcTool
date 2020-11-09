using DeadRisingArcTool.FileFormats.Geometry.DirectX.Misc;
using DeadRisingArcTool.Utilities;
using ImGuiNET;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadRisingArcTool.FileFormats.Geometry.DirectX
{
    public class RenderableGameResource : IRenderable
    {
        /// <summary>
        /// Game resource to be rendered
        /// </summary>
        public GameResource GameResource { get; private set; }
        /// <summary>
        /// World position of the resource
        /// </summary>
        public Vector3 Position { get; set; } = new Vector3(0.0f);
        /// <summary>
        /// Rotation to be applied to the resource
        /// </summary>
        public Quaternion Rotation { get; set; } = new Quaternion(0.0f);

        protected bool uiVisible = false;
        /// <summary>
        /// Determins if the object properties UI is currently being displayed for the object
        /// </summary>
        public bool UIVisible { get { return this.uiVisible; } set { this.uiVisible = value; } }

        public RenderableGameResource(GameResource resource)
        {
            this.GameResource = resource;
        }

        public RenderableGameResource(GameResource resource, Vector3 position, Quaternion rotation)
        {
            // Initialize fields.
            this.GameResource = resource;
            this.Position = position;
            this.Rotation = rotation;
        }

        #region IRenderable

        public bool InitializeGraphics(RenderManager manager)
        {
            // Initialize graphics for the game resource.
            return this.GameResource.InitializeGraphics(manager);
        }

        public bool DrawFrame(RenderManager manager)
        {
            // Check the resource type and handle accordingly.
            switch (this.GameResource.FileType)
            {
                case ResourceType.rModel:
                    {
                        // Set the model position and rotation.
                        ((rModel)this.GameResource).modelPosition = new Vector4(this.Position, 1.0f);
                        ((rModel)this.GameResource).modelRotation = this.Rotation.ToVector4();
                        break;
                    }
            }

            // Render the game resource.
            return this.GameResource.DrawFrame(manager);
        }

        public void DrawObjectPropertiesUI(RenderManager manager)
        {
            // If the UI is currently active draw it.
            if (this.UIVisible == true)
            {
                // Create the object properties window.
                if (ImGui.Begin("Object Properties - " + this.GameResource.FileName, ref this.uiVisible) == true)
                {
                    // Set the window position and size on first open.
                    ImGui.SetWindowPos(new System.Numerics.Vector2(manager.ViewSize.Width - (manager.ViewSize.Width / 4) - 10, 10), ImGuiCond.Once);
                    ImGui.SetWindowSize(new System.Numerics.Vector2(manager.ViewSize.Width / 4, manager.ViewSize.Height - 20), ImGuiCond.Once);

                    // Render the properties UI.
                    this.GameResource.DrawObjectPropertiesUI(manager);

                    // End the window.
                    ImGui.End();
                }
            }
        }

        public void CleanupGraphics(RenderManager manager)
        {
            // Cleanup resource graphics objects.
            this.GameResource.CleanupGraphics(manager);
        }

        public bool DoClippingTest(RenderManager manager, FastBoundingBox viewBox)
        {
            // TODO: Implement this
            return true;
        }

        #endregion
    }
}
