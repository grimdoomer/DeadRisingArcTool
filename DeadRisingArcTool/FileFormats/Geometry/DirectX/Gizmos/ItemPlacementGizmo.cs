using DeadRisingArcTool.FileFormats.Geometry.DirectX.Gizmos.Polygons;
using DeadRisingArcTool.FileFormats.Geometry.DirectX.Misc;
using DeadRisingArcTool.FileFormats.Geometry.DirectX.Shaders;
using DeadRisingArcTool.Utilities;
using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadRisingArcTool.FileFormats.Geometry.DirectX.Gizmos
{
    public class ItemPlacementGizmo : IRenderable
    {
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }

        private Matrix rotationX;
        private Matrix rotationY;
        private Matrix rotationZ;
        private Quaternion quatRotation;

        public rModel ItemModel { get; private set; }

        private readonly Color4 boxCornerColor = new Color4(0xFF00FF00);
        private readonly Color4 boxCornerHighlightColor = new Color4(0xFF31B0F5);

        private bool isFocused = false;
        public bool IsFocused { get { return this.isFocused; } set { if (this.isFocused != value) { this.isFocused = value; UpdateColors(); } } }

        private bool showRotationCircles = false;
        public bool ShowRotationCircles 
        { 
            get { return this.showRotationCircles; }
            set
            {
                if (this.showRotationCircles != value) 
                { 
                    this.showRotationCircles = value;
                    for (int i = 0; i < 3; i++)
                    {
                        this.rotationCircles[i].Visible = this.showRotationCircles;
                        this.rotationArrows[i].Visible = this.showRotationCircles;
                    }
                }
            }
        }

        // Polygon mesh pieces.
        private PolygonMesh gizmoMesh;
        private Box boundingBox;
        private Circle[] rotationCircles = new Circle[3];
        private Arrow[] rotationArrows = new Arrow[3];

        // Rotational circle data.
        private int ringSegments = 32;
        private float ringRadius;

        // Rotational arrow data.
        private float arrowHeight;
        private float arrowWidth;

        public ItemPlacementGizmo(Vector3 position, Vector3 rotation, rModel itemModel)
        {
            // Initialize fields.
            this.Position = position;
            this.Rotation = rotation;
            this.ItemModel = itemModel;

            // Initialize the rotation matrices.
            this.rotationX = Matrix.RotationX(this.Rotation.X);
            this.rotationY = Matrix.RotationY(this.Rotation.Y);
            this.rotationZ = Matrix.RotationZ(this.Rotation.Z);
            this.quatRotation = Quaternion.RotationMatrix(this.rotationX * this.rotationY * this.rotationZ);

            // If a game resource was specified use its bounding box, else use default size.
            if (this.ItemModel != null)
            {
                // Initialize the bounding box for the game model.
                this.boundingBox = new Box(this.ItemModel.header.BoundingBoxMin.ToVector3(), this.ItemModel.header.BoundingBoxMax.ToVector3(), Vector3.Zero, Quaternion.Identity);
                this.boundingBox.LineStyle = BoxLineStyle.CornersOnly;
            }
            else
            {
                // Use default size.
                this.boundingBox = new Box(5f, 5f, 5f, Vector3.Zero, Quaternion.Identity);
                this.boundingBox.LineStyle = BoxLineStyle.CornersOnly;
            }

            // Get the largest side of the bounding box.
            float largestSideLength = Math.Max(this.boundingBox.Depth, Math.Max(this.boundingBox.Height, this.boundingBox.Width));

            // Setup the rotational ring fields.
            this.ringRadius = largestSideLength + 10.0f;
            this.ringSegments = 32 * (((int)this.ringRadius / 100) + 1);

            // Setup the major rotational axises.
            Vector3 xaxis = new Vector3(this.ringRadius, 0.0f, 0.0f);
            Vector3 yaxis = new Vector3(0.0f, this.ringRadius, 0.0f);
            Vector3 zaxis = new Vector3(0.0f, 0.0f, this.ringRadius);

            // Initialize the rotation circles.
            this.rotationCircles[0] = new Circle(this.ringRadius, zaxis, xaxis, Vector3.Zero, Quaternion.Identity); this.rotationCircles[0].Visible = false; this.rotationCircles[0].Color = new Color4(0xFFFF0000);
            this.rotationCircles[1] = new Circle(this.ringRadius, xaxis, yaxis, Vector3.Zero, Quaternion.Identity); this.rotationCircles[1].Visible = false; this.rotationCircles[1].Color = new Color4(0xFF00FF00);
            this.rotationCircles[2] = new Circle(this.ringRadius, yaxis, zaxis, Vector3.Zero, Quaternion.Identity); this.rotationCircles[2].Visible = false; this.rotationCircles[2].Color = new Color4(0xFF0000FF);

            // Setup the rotation arrow fields.
            this.arrowHeight = this.ringRadius / 2.0f;

            // Initialize the rotation arrows.
            this.rotationArrows[0] = new Arrow(this.arrowHeight, new Vector3(0f, 0f, -(this.ringRadius + (this.arrowHeight / 2f) + 5f)), Quaternion.Identity);
            this.rotationArrows[0].Visible = false;
            this.rotationArrows[0].Color = new Color4(0xFFFF0000);

            this.rotationArrows[1] = new Arrow(this.arrowHeight, new Vector3(this.ringRadius + (this.arrowHeight / 2f) + 5f, 0f, 0f), Quaternion.RotationYawPitchRoll(MathUtil.DegreesToRadians(270), 0f, MathUtil.DegreesToRadians(90)));
            this.rotationArrows[1].Visible = false;
            this.rotationArrows[1].Color = new Color4(0xFF00FF00);

            this.rotationArrows[2] = new Arrow(this.arrowHeight, new Vector3(0f, this.ringRadius + (this.arrowHeight / 2f) + 5f, 0f), Quaternion.RotationYawPitchRoll(MathUtil.DegreesToRadians(90), MathUtil.DegreesToRadians(90), 0f));
            this.rotationArrows[2].Visible = false;
            this.rotationArrows[2].Color = new Color4(0xFF0000FF);

            // Create the polygon stream for our gizmo.
            this.gizmoMesh = new PolygonMesh(this.Position, this.quatRotation, this.boundingBox, 
                this.rotationCircles[0], this.rotationCircles[1], this.rotationCircles[2], 
                this.rotationArrows[0], this.rotationArrows[1], this.rotationArrows[2]);
        }

        private void UpdateColors()
        {
            // Check if the gizmo is focused or not and set the color accordingly.
            if (this.isFocused == true)
                this.boundingBox.BoxLineColor = this.boxCornerHighlightColor;
            else
                this.boundingBox.BoxLineColor = this.boxCornerColor;
        }

        public bool InitializeGraphics(RenderManager manager)
        {
            // Build the polygon mesh from our polygon primitives.
            return this.gizmoMesh.InitializeGraphics(manager);
        }

        public bool DrawFrame(RenderManager manager)
        {
            // Draw the gizmo.
            this.gizmoMesh.DrawFrame(manager);

            return true;
        }

        public void DrawObjectPropertiesUI(RenderManager manager)
        {
            throw new NotImplementedException();
        }

        public void CleanupGraphics(RenderManager manager)
        {
            throw new NotImplementedException();
        }

        public bool DoClippingTest(RenderManager manager, FastBoundingBox viewBox)
        {
            return false;
        }
    }
}
