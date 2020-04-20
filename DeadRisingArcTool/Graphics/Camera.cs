using SharpDX;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeadRisingArcTool.Graphics
{
    public class Camera : IDisposable
    {
        public DirectInput directInput;
        public Keyboard Keyboard;
        private Mouse mouse;
        //public Controller XboxControler;

        public float radius = 1.0f;
        public float speed = 1.0f;

        public int oldx = 0;
        public int oldy = 0;


        float moveLeftRight = 0.0f;
        float moveBackForward = 0.0f;

        float camYaw = 0.0f;
        float camPitch = 0.0f;

        // Constant directional vectors:
        private static readonly Vector3 DefaultUp = new Vector3(0.0f, 1.0f, 0.0f);
        private static readonly Vector3 DefaultDown = new Vector3(0.0f, -1.0f, 0.0f);
        private static readonly Vector3 DefaultForward = new Vector3(0.0f, 0.0f, 1.0f);
        private static readonly Vector3 DefaultBackward = new Vector3(0.0f, 0.0f, -1.0f);
        private static readonly Vector3 DefaultRight = new Vector3(1.0f, 0.0f, 0.0f);
        private static readonly Vector3 DefaultLeft = new Vector3(-1.0f, 0.0f, 0.0f);

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
                return Matrix.LookAtLH(this.Position, this.LookAt, this.UpVector);
            }
        }

        public Camera(Control form)
        {
            // Initialize Keyboard
            directInput = new DirectInput();
            Keyboard = new Keyboard(directInput);
            Keyboard.SetCooperativeLevel(form.Handle, CooperativeLevel.Foreground | CooperativeLevel.NonExclusive);

            this.mouse = new Mouse(directInput);
            mouse.SetCooperativeLevel(form.Handle, CooperativeLevel.Foreground | CooperativeLevel.NonExclusive);

            // Setup Camera vectors with default values.
            this.position = new Vector3(0.0f, 0.0f, -5.0f);
            this.lookAt = new Vector3(0.0f, 0.0f, 0.0f);
            this.upVector = new Vector3(0.0f, 1.0f, 0.0f);

            ComputePosition();
        }

        public void move()
        {
            // Aquire Devices
            try
            {
                Keyboard.Acquire();
                this.mouse.Acquire();
            }
            catch
            {
                return;
            }

            // Get Keyboard State
            Keyboard.Poll();
            KeyboardState KState = this.Keyboard.GetCurrentState();
            foreach (Key kk in KState.PressedKeys)
            {
                switch (kk.ToString())
                {
                    case "W":
                        //this.moveBackForward += this.speed;
                        this.position += this.camForward * this.speed;
                        break;
                    case "S":
                        //this.moveBackForward -= this.speed;
                        this.position += this.camBackward * this.speed;
                        break;
                    case "A":
                        this.position += this.camLeft * this.speed;
                        //this.moveLeftRight -= this.speed;
                        break;
                    case "D":
                        this.position += this.camRight * this.speed;
                        //this.moveLeftRight += this.speed;
                        break;
                    case "Z":
                        this.position += DefaultUp * this.speed;
                        //this.position.Z -= this.speed;
                        break;
                    case "X":
                        this.position += DefaultDown * this.speed;
                        //this.position.Z += this.speed;
                        break;
                    case "Equals":
                    case "Add":
                        speed += 0.0001f;
                        if (speed < 0) { speed = 0.002f; }
                        break;
                    case "Minus":
                    case "NumPadMinus":
                        speed -= 0.0001f;
                        if (speed < 0) { speed = 0.002f; }
                        break;
                }
            }

            this.mouse.Poll();
            MouseState mouseState = this.mouse.GetCurrentState();

            // If the left mouse button is held down adjust rotation.
            if (mouseState.Buttons[0] == true)
            {
                change(mouseState.X, mouseState.Y);
            }

            ComputePosition();

        }
        public void change(int x, int y)
        {

            //int tempx = oldx - x;
            //int tempy = oldy - y;

            this.camYaw += x * 0.001f;
            this.camPitch += y * 0.001f;


            //ComputePosition();
            //oldx = x;
            //oldy = y;
        }

        public void Dispose()
        {
            Keyboard = null;
        }
        public void ComputePosition()
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


        public static float DegreesToRadian(float degree)
        {
            return (float)(degree * (Math.PI / 180.0f));
        }
    }
}
