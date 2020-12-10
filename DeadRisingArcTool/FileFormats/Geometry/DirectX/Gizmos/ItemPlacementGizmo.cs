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
        public Vector3 Rotation { get { return this.rotation; } set { if (this.rotation != value) { UpdateRotation(value); } } }

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

        private void UpdateRotation(Vector3 newRotation)
        {
            //System.Diagnostics.Debug.WriteLine("Old: {0} New: {1} Diff: {2}", this.rotation, newRotation, newRotation - this.rotation);

            if (Math.Abs(newRotation.Y - this.rotation.Y) > 1.0f)
            {

            }

            this.rotation = newRotation;

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

        public bool HandleInput(RenderManager manager, out Vector3 translationChange, out Vector3 rotationChange)
        {
            // Satisfy the compiler.
            translationChange = Vector3.Zero;
            rotationChange = Vector3.Zero;

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
                for (int i = 0; i < this.rotationArrows.Length; i++)
                {
                    // Check for hit detection on the rotation arrow and set the active rotation axis.
                    if (this.rotationArrows[i].DoPickingTest(manager, pickingRay, out float distance, out object context) == true)
                    {
                        // Get the initial point of intersection with the arrow plane for calculating the rotation.
                        if (this.rotationArrows[i].GetPointOfIntersection(manager, pickingRay, out this.initialRotationPoint) == false)
                        {
                            return false;
                        }

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
                Matrix gizmoTransform = Matrix.Transformation(Vector3.Zero, Quaternion.Zero, Vector3.One, Vector3.Zero, Quaternion.Identity, this.gizmoMesh.Position);// this.gizmoMesh.TransformationMatrix;
                gizmoTransform.Invert();

                //System.Diagnostics.Debug.WriteLine("Rot: {1}", this.gizmoMesh.Rotation);

                // Transform the picking ray to be in local space for the gizmo.
                Ray pickingRay = new Ray(Vector3.TransformCoordinate(manager.MouseToWorldRay.Position, gizmoTransform), Vector3.TransformNormal(manager.MouseToWorldRay.Direction, gizmoTransform));
                pickingRay.Direction.Normalize();

                // Get the point of intersection with the arrow plane.
                Vector3 pointofIntersection = Vector3.Zero;
                if (this.rotationArrows[(int)this.FocusedRotationAxis - 1].GetPointOfIntersection(manager, pickingRay, out pointofIntersection) == false)
                    return true;

                //System.Diagnostics.Debug.WriteLine("PoI: {0}", pointofIntersection);

                // Transform the point of intersection by the arrow's transform to put it back into the gizmo's local space.
                //Matrix hackTransform = Matrix.Transformation(Vector3.Zero, Quaternion.Identity, Vector3.One, Vector3.Zero, this.quatRotation, this.rotationArrows[(int)this.FocusedRotationAxis - 1].Position);
                //pointofIntersection = Vector3.Transform(pointofIntersection, hackTransform).ToVector3();
                //pointofIntersection = Vector3.Transform(pointofIntersection, this.gizmoMesh.TransformationMatrix).ToVector3();

                //System.Diagnostics.Debug.WriteLine("PoI: {0} Rotation: {1}", pointofIntersection, this.quatRotation);

                // Calculate the unit vector for a rotation of 0 degrees.
                Vector3 zeroRotation;
                if (this.FocusedRotationAxis == RotationAxis.Yaw)
                    zeroRotation = -Vector3.UnitZ;
                else if (this.FocusedRotationAxis == RotationAxis.Pitch)
                    zeroRotation = Vector3.UnitX;
                else
                    zeroRotation = Vector3.UnitY;

                //Matrix hackTransform = Matrix.Transformation(Vector3.Zero, Quaternion.Identity, Vector3.One, Vector3.Zero, Quaternion.Identity, this.Position);
                //zeroRotation = Vector3.Transform(zeroRotation, hackTransform).ToVector3();

                
                //pointofIntersection.Normalize();
                //System.Diagnostics.Debug.WriteLine("PoI: {0}", pointofIntersection);

                // Calculate the angle between the initial rotation point and the current rotation point.
                //float initialAngle = this.initialRotationPoint.AngleBetweenLine(zeroRotation);
                float newAngle = pointofIntersection.AngleBetweenLine(zeroRotation);

                // Make sure both angles are valid, if either one is invalid bail out.
                if (float.IsNaN(newAngle) == true)
                    return true;

                // The range of the angle is [0, pi] (or [0, 180]), check the X component of the point of intersection to see if we need to adjust.
                if (this.FocusedRotationAxis == RotationAxis.Yaw)
                {
                    if (pointofIntersection.X >= 0.0f)
                        newAngle = (2 * (float)Math.PI) - newAngle;
                }
                else if (this.FocusedRotationAxis == RotationAxis.Pitch)
                {
                    if (pointofIntersection.Y <= 0.0f)
                        newAngle = (2 * (float)Math.PI) - newAngle;

                    //newAngle = (2 * (float)Math.PI) - newAngle;
                }
                else if (this.FocusedRotationAxis == RotationAxis.Roll)
                {
                    if (pointofIntersection.Z >= 0.0f)
                        newAngle = (2 * (float)Math.PI) - newAngle;
                }

                // Calculate the change in rotation.
                //float axisRotation = newAngle;
                //if (this.FocusedRotationAxis == RotationAxis.Yaw)
                //    axisRotation = newAngle - this.Rotation.Y;
                //else if (this.FocusedRotationAxis == RotationAxis.Pitch)
                //    axisRotation = newAngle - this.Rotation.Z;
                //else
                //    axisRotation = newAngle - this.Rotation.X;

                // If the rotational value is invalid bail out.
                //if (float.IsNaN(axisRotation) == true)
                //    return true;

                //if (Math.Abs(axisRotation) > 1.0f)
                //{

                //}

                // Create an identity vector we can use to calculate the change in angle.
                Vector3 rotationIdentity;
                if (this.FocusedRotationAxis == RotationAxis.Yaw)
                    rotationIdentity = Vector3.UnitY;
                else if (this.FocusedRotationAxis == RotationAxis.Pitch)
                    rotationIdentity = Vector3.UnitZ;
                else
                    rotationIdentity = Vector3.UnitX;

                System.Diagnostics.Debug.WriteLine("PoI: {0} Angle: {1}", pointofIntersection, newAngle);

                //Quaternion oldQuat = this.quatRotation;
                //Vector3 oldRot = this.Rotation;

                // Calculate the change in rotation.
                rotationChange = (new Vector3(newAngle) * rotationIdentity) - (this.Rotation * rotationIdentity);

                // Update the position and rotation of the gizmo.
                //this.Position += translationChange;
                this.Rotation += rotationChange;

                //System.Diagnostics.Debug.WriteLine("Quat1: {0} Quat2: {1} Angle: {2}", oldQuat, this.quatRotation, newAngle);

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
