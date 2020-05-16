using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeadRisingArcTool.FileFormats.Geometry.DirectX
{
    public class Camera : IRenderable
    {
        public float radius = 1.0f;
        public float Speed { get; set; } = 1.0f;
        public float SpeedModifier { get; set; } = 0.002f;


        float moveLeftRight = 0.0f;
        float moveBackForward = 0.0f;

        float camYaw = 0.0f;
        float camPitch = 0.0f;

        // Constant directional vectors:
        private static readonly Vector3 DefaultUp = new Vector3(0.0f, 1.0f, 0.0f);
        private static readonly Vector3 DefaultDown = new Vector3(0.0f, -1.0f, 0.0f);
        private static readonly Vector3 DefaultForward = new Vector3(0.0f, 0.0f, 1.0f);
        private static readonly Vector3 DefaultBackward = new Vector3(0.0f, 0.0f, -1.0f);
        private static readonly Vector3 DefaultRight = new Vector3(-1.0f, 0.0f, 0.0f);
        private static readonly Vector3 DefaultLeft = new Vector3(1.0f, 0.0f, 0.0f);

        // Directional vectors based on the camera's current position and rotation.

        private Vector3 camForward = DefaultForward;
        private Vector3 camBackward = DefaultBackward;
        private Vector3 camRight = DefaultRight;
        private Vector3 camLeft = DefaultLeft;

        private Vector3 position;
        public Vector3 Position { get { return this.position; } set { this.position = value; } }
        private Vector3 lookAt;
        public Vector3 LookAt { get { return this.lookAt; } set { this.lookAt = value; } }
        private Vector3 upVector;
        public Vector3 UpVector { get { return this.upVector; } set { this.upVector = value; } }

        public Matrix ViewMatrix
        {
            get
            {
                return Matrix.LookAtRH(this.Position, this.LookAt, this.UpVector);
            }
        }

        public Camera()
        {
            // Setup Camera vectors with default values.
            this.position = new Vector3(0.0f, 0.0f, -5.0f);
            this.lookAt = new Vector3(0.0f, 0.0f, 0.0f);
            this.upVector = new Vector3(0.0f, 1.0f, 0.0f);

            ComputePosition();
        }

        private void ComputePosition()
        {
            // Update the direction we are looking in.
            Matrix camRotation = Matrix.RotationYawPitchRoll(camYaw, camPitch, 0.0f);
            this.lookAt = Vector3.TransformCoordinate(DefaultForward, camRotation) + this.position;

            // Calculate up direction based on the current rotation.
            this.upVector = Vector3.TransformCoordinate(DefaultUp, camRotation);

            // Update directional vectors based on our new rotation.
            camRotation = Matrix.RotationYawPitchRoll(camYaw, camPitch, 0.0f);
            this.camForward = Vector3.TransformCoordinate(DefaultForward, camRotation);
            this.camBackward = Vector3.TransformCoordinate(DefaultBackward, camRotation);
            this.camRight = Vector3.TransformCoordinate(DefaultRight, camRotation);
            this.camLeft = Vector3.TransformCoordinate(DefaultLeft, camRotation);
        }

        #region IRenderable

        public bool InitializeGraphics(IRenderManager manager, Device device)
        {
            return true;
        }

        public bool DrawFrame(IRenderManager manager, Device device)
        {
            // Get the input manager.
            InputManager input = manager.GetInputManager();

            // Update camera position.
            if (input.ButtonPressed(InputAction.MoveForward) == true || input.ButtonHeld(InputAction.MoveForward) == true)
                this.position += this.camForward * this.Speed;
            if (input.ButtonPressed(InputAction.MoveBackward) == true || input.ButtonHeld(InputAction.MoveBackward) == true)
                this.position += this.camBackward * this.Speed;
            if (input.ButtonPressed(InputAction.StrafeLeft) == true || input.ButtonHeld(InputAction.StrafeLeft) == true)
                this.position += this.camLeft * this.Speed;
            if (input.ButtonPressed(InputAction.StrafeRight) == true || input.ButtonHeld(InputAction.StrafeRight) == true)
                this.position += this.camRight * this.Speed;
            if (input.ButtonPressed(InputAction.MoveUp) == true || input.ButtonHeld(InputAction.MoveUp) == true)
                this.position += DefaultUp * this.Speed;
            if (input.ButtonPressed(InputAction.MoveDown) == true || input.ButtonHeld(InputAction.MoveDown) == true)
                this.position += DefaultDown * this.Speed;

            // Update camera speed.
            if (input.ButtonPressed(InputAction.CamSpeedIncrease) == true || input.ButtonHeld(InputAction.CamSpeedIncrease) == true)
            {
                Speed += SpeedModifier;
                if (Speed < 0) { Speed = 0.002f; }
            }
            if (input.ButtonPressed(InputAction.CamSpeedDecrease) == true || input.ButtonHeld(InputAction.CamSpeedDecrease) == true)
            {
                Speed -= SpeedModifier;
                if (Speed < 0) { Speed = 0.002f; }
            }

            // Check for mouse movement.
            if (input.ButtonPressed(InputAction.LeftClick) == true || input.ButtonHeld(InputAction.LeftClick) == true)
            {
                // Update camera rotation.
                this.camYaw += -input.MousePosition[0] * 0.005f;     // Flip x direction for RH coordinate system
                this.camPitch += input.MousePosition[1] * 0.005f;
            }

            // Update camera vectors.
            ComputePosition();

            return true;
        }

        public void CleanupGraphics(IRenderManager manager, Device device)
        {
        }

        #endregion
    }
}
