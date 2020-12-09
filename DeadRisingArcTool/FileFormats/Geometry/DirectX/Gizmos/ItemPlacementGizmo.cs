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
    public enum RotationAxis
    {
        None = 0,
        Yaw,
        Pitch,
        Roll
    }

    public enum TransformationType
    {
        None = 0,
        Position,
        Rotation,
        Scale
    }

    public class ItemPlacementGizmo : IRenderable
    {
        private Vector3 position;
        public Vector3 Position { get { return this.position; } set { this.position = value; this.gizmoMesh.Position = value; } }
        private Vector3 rotation;
        public Vector3 Rotation { get { return this.rotation; } set { if (this.rotation != value) { this.rotation = value; UpdateRotation(); } } }

        private Matrix rotationX;
        private Matrix rotationY;
        private Matrix rotationZ;
        private Quaternion quatRotation;

        public rModel ItemModel { get; private set; }

        private readonly Color4 boxCornerColor = new Color4(0xFF00FF00);
        private readonly Color4 boxCornerHighlightColor = new Color4(0xFF31B0F5);

        private bool isFocused = false;
        public bool IsFocused { get { return this.isFocused; } set { if (this.isFocused != value) { this.isFocused = value; UpdateColors(); } } }

        public RotationAxis FocusedRotationAxis { get; private set; } = RotationAxis.None;

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

        private Vector3 initialRotationPoint;

        public ItemPlacementGizmo(Vector3 position, Vector3 rotation, rModel itemModel)
        {
            // Initialize fields.
            this.position = position;
            this.rotation = rotation;
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
            this.rotationCircles[0] = new Circle(this.ringRadius, zaxis, xaxis, Vector3.Zero, Quaternion.Identity);
            this.rotationCircles[0].Visible = false;
            this.rotationCircles[0].Color = new Color4(0xFFFF0000); // Yaw Y-axis

            this.rotationCircles[1] = new Circle(this.ringRadius, xaxis, yaxis, Vector3.Zero, Quaternion.Identity);
            this.rotationCircles[1].Visible = false;
            this.rotationCircles[1].Color = new Color4(0xFF00FF00); // Pitch Z-axis

            this.rotationCircles[2] = new Circle(this.ringRadius, yaxis, zaxis, Vector3.Zero, Quaternion.Identity);
            this.rotationCircles[2].Visible = false;
            this.rotationCircles[2].Color = new Color4(0xFF0000FF); // Roll X-axis

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

        private void UpdateRotation()
        {
            // Convert the YPR rotation to a quaternion.
            this.rotationX = Matrix.RotationX(this.Rotation.X);
            this.rotationY = Matrix.RotationY(this.Rotation.Y);
            this.rotationZ = Matrix.RotationZ(this.Rotation.Z);
            this.quatRotation = Quaternion.RotationMatrix(this.rotationX * this.rotationY * this.rotationZ);

            // Update the rotation of the gizmo mesh.
            //if (this.gizmoMesh != null)
                this.gizmoMesh.Rotation = this.quatRotation;
        }

        public bool InitializeGraphics(RenderManager manager)
        {
            // Build the polygon mesh from our polygon primitives.
            return this.gizmoMesh.InitializeGraphics(manager);
        }

        public bool DrawFrame(RenderManager manager)
        {
            // If the gizmo is in focus do a hit test to check for hovering over interactable polygons.
            if (this.isFocused == true)
            {
                // If the active rotation axis is set skip doing the hit test.
                if (this.FocusedRotationAxis == RotationAxis.None)
                {
                    // Invert the gizmo transformation so we can transform the picking ray.
                    Matrix gizmoTransform = this.gizmoMesh.TransformationMatrix;
                    gizmoTransform.Invert();

                    // Transform the picking ray to be in local space for the gizmo.
                    Ray pickingRay = new Ray(Vector3.TransformCoordinate(manager.MouseToWorldRay.Position, gizmoTransform), Vector3.TransformNormal(manager.MouseToWorldRay.Direction, gizmoTransform));
                    pickingRay.Direction.Normalize();

                    // Perform hit tests on interactable polygons.
                    for (int i = 0; i < this.rotationArrows.Length; i++)
                    {
                        // Check for hit detection on the rotation arrow and set the draw style based on the result.
                        this.rotationArrows[i].Style = this.rotationArrows[i].DoPickingTest(manager, pickingRay, out float distance, out object context) == true ? PolygonDrawStyle.Solid : PolygonDrawStyle.Outline;
                    }
                }
                else
                {
                    // Set only the active rotation axis as being solid.
                    for (int i = 0; i < this.rotationArrows.Length; i++)
                        this.rotationArrows[i].Style = (int)this.FocusedRotationAxis - 1 == i ? PolygonDrawStyle.Solid : PolygonDrawStyle.Outline;
                }
            }

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

        public bool HandleInput(RenderManager manager, out Vector3 translation, out Vector3 rotation)
        {
            // Satisfy the compiler.
            translation = Vector3.Zero;
            rotation = Vector3.Zero;

            // If the gizmo is not focused bail out.
            if (this.isFocused == false)
                return false;

            // Check if the left mouse button was just pressed.
            if (manager.InputManager.ButtonPressed(InputAction.LeftClick) == true)
            {
                // Invert the gizmo transformation so we can transform the picking ray.
                Matrix gizmoTransform = this.gizmoMesh.TransformationMatrix;
                gizmoTransform.Invert();

                // Transform the picking ray to be in local space for the gizmo.
                Ray pickingRay = new Ray(Vector3.TransformCoordinate(manager.MouseToWorldRay.Position, gizmoTransform), Vector3.TransformNormal(manager.MouseToWorldRay.Direction, gizmoTransform));
                pickingRay.Direction.Normalize();

                // Perform hit tests on interactable polygons.
                //this.FocusedRotationAxis = RotationAxis.None;
                for (int i = 0; i < this.rotationArrows.Length; i++)
                {
                    // Check for hit detection on the rotation arrow and set the active rotation axis.
                    if (this.rotationArrows[i].DoPickingTest(manager, pickingRay, out float distance, out object context) == true)
                    {
                        // Get the initial point of intersection with the arrow plane for calculating the rotation.
                        this.rotationArrows[i].GetPointOfIntersection(manager, pickingRay, out this.initialRotationPoint);

                        // Set this axis as active.
                        this.FocusedRotationAxis = (RotationAxis)(i + 1);
                        return true;
                    }
                }
            }
            else if (manager.InputManager.ButtonHeld(InputAction.LeftClick) == true)
            {
                // If no focused rotation axis is set bail out.
                if (this.FocusedRotationAxis == RotationAxis.None)
                    return false;

                // If the mouse hasn't moved bail out.
                if (manager.InputManager.MousePositionDelta[0] == 0 && manager.InputManager.MousePositionDelta[1] == 0)
                    return false;

                // Invert the gizmo transformation so we can transform the picking ray.
                Matrix gizmoTransform = this.gizmoMesh.TransformationMatrix;
                gizmoTransform.Invert();

                // Transform the picking ray to be in local space for the gizmo.
                Ray pickingRay = new Ray(Vector3.TransformCoordinate(manager.MouseToWorldRay.Position, gizmoTransform), Vector3.TransformNormal(manager.MouseToWorldRay.Direction, gizmoTransform));
                pickingRay.Direction.Normalize();

                // Get the point of intersection with the arrow plane.
                Vector3 pointofIntersection;// = new Vector3(manager.InputManager.MousePositionDelta[0], manager.InputManager.MousePositionDelta[1], 0.0f);
                this.rotationArrows[(int)this.FocusedRotationAxis - 1].GetPointOfIntersection(manager, pickingRay, out pointofIntersection);
                //if (this.FocusedRotationAxis == RotationAxis.Yaw)
                //    this.initialRotationPoint = Vector3.UnitX;// * this.ringRadius;
                //else if (this.FocusedRotationAxis == RotationAxis.Pitch)
                //    this.initialRotationPoint = Vector3.UnitZ;
                //else
                //    this.initialRotationPoint = Vector3.UnitX;

                //this.initialRotationPoint = new Vector3(manager.InputManager.MousePositionDelta[0], 0.0f, manager.InputManager.MousePositionDelta[1]);

                // Compute the dot product of the two rays.
                float dotProduct = Vector3.Dot(this.initialRotationPoint, pointofIntersection);

                // Get the magnitude of the two lines.
                float lengthA = Math.Abs(this.initialRotationPoint.Length());
                float lengthB = Math.Abs(pointofIntersection.Length());

                // Calculate the angle between the two lines which is how much we rotated the model.
                float angle = dotProduct / (lengthA * lengthB);
                float axisRotation = (float)Math.Acos(angle);

                // If the rotational value is invalid bail out.
                if (float.IsNaN(axisRotation) == true)
                    return true;

                //if (axisRotation < 0.0f)
                //    axisRotation = (float)(2 * Math.PI) + axisRotation;

                // If the mouse delta for the x-axis is negative change the sign of the axis rotation.
                //if (manager.InputManager.MousePositionDelta[0] < 0.0f)
                //    axisRotation *= -1.0f;

                // Create the rotation vector for the axis that is focused.
                Vector3 newRotation = Vector3.Zero;
                if (this.FocusedRotationAxis == RotationAxis.Yaw)
                    newRotation = new Vector3(0.0f, axisRotation, 0.0f);
                else if (this.FocusedRotationAxis == RotationAxis.Pitch)
                    newRotation = new Vector3(0.0f, 0.0f, axisRotation);
                else
                    newRotation = new Vector3(axisRotation, 0.0f, 0.0f);

                // Set the new initial point of rotation.
                this.initialRotationPoint = pointofIntersection;

                if (float.IsNaN(rotation.X) || float.IsNaN(rotation.Y) || float.IsNaN(rotation.Z))
                {

                }

                // Update the position and rotation of the gizmo.
                rotation = newRotation;// - this.Rotation;
                this.Position += translation;
                this.Rotation += newRotation;

                System.Diagnostics.Debug.WriteLine(string.Format("Angle: {0} PoI: {1}", dotProduct, pointofIntersection));

                if (float.IsNaN(this.rotation.X) || float.IsNaN(this.rotation.Y) || float.IsNaN(this.rotation.Z))
                {

                }

                return true;
            }
            else if (manager.InputManager.ButtonReleased(InputAction.LeftClick) == true)
            {
                // Clear the active rotation axis.
                this.FocusedRotationAxis = RotationAxis.None;
            }

            // If we made it here the input was not handled.
            return false;
        }
    }
}
