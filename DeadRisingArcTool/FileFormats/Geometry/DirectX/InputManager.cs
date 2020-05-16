using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Direct3D11;
using SharpDX.DirectInput;
using SharpDX.XInput;
using Device = SharpDX.Direct3D11.Device;

namespace DeadRisingArcTool.FileFormats.Geometry.DirectX
{
    /// <summary>
    /// Enum of actions that get mapped to input devices/keys
    /// </summary>
    public enum InputAction : int
    {
        // Camera movement:
        MoveForward,
        MoveBackward,
        StrafeLeft,
        StrafeRight,
        MoveUp,
        MoveDown,

        // Mouse buttons:
        LeftClick,
        RightClick,

        // Camera misc:
        CamSpeedIncrease, 
        CamSpeedDecrease,

        // Animation controls:
        NextAnimation,
        PreviousAnimation,

        InputAction_Max
    }

    public class InputManager : IRenderable
    {
        /// <summary>
        /// Button state from the previous input poll operation
        /// </summary>
        public bool[] PreviousButtonState { get; protected set; } = new bool[(int)InputAction.InputAction_Max];
        /// <summary>
        /// Input state for the current input poll operation
        /// </summary>
        public bool[] ButtonState { get; protected set; } = new bool[(int)InputAction.InputAction_Max];
        /// <summary>
        /// XY coordinates of the mouse cursor
        /// </summary>
        public int[] MousePosition { get; protected set; } = new int[2];

        /// <summary>
        /// True if the xbox gamepad is connected
        /// </summary>
        public bool IsGamepadConnected { get { return this.gamepadDevice.IsConnected; } }

        // Control handle we bind to for input focus.
        IntPtr formHandle;

        // Direct input HID devices.
        private DirectInput directInput = null;
        private Keyboard keyboardDevice = null;
        private Mouse mouseDevice = null;
        private Controller gamepadDevice = null;

        private State gamepadState;

        /// <summary>
        /// Initializes a new InputManager instance using the form handle specified
        /// </summary>
        /// <param name="formHandle">Handle of a control or form to bind to for input focus</param>
        public InputManager(IntPtr formHandle)
        {
            // Initialize fields.
            this.formHandle = formHandle;
        }

        /// <summary>
        /// Returns true if the specified button was pressed since the last update
        /// </summary>
        /// <param name="action">Input button to check</param>
        /// <returns></returns>
        public bool ButtonPressed(InputAction action)
        {
            return (this.PreviousButtonState[(int)action] == false && this.ButtonState[(int)action] == true);
        }

        /// <summary>
        /// Returns true if the specified button was held since the last update
        /// </summary>
        /// <param name="action">Input button to check</param>
        /// <returns></returns>
        public bool ButtonHeld(InputAction action)
        {
            return (this.PreviousButtonState[(int)action] == true && this.ButtonState[(int)action] == true);
        }

        /// <summary>
        /// Returns true if the specified button was released since the last update
        /// </summary>
        /// <param name="action">Input button to check</param>
        /// <returns></returns>
        public bool ButtonReleased(InputAction action)
        {
            return (this.PreviousButtonState[(int)action] == true && this.ButtonState[(int)action] == false);
        }

        #region IRenderable

        public bool InitializeGraphics(IRenderManager manager, Device device)
        {
            // Initialize the direct input object.
            this.directInput = new DirectInput();

            // Initialize the keyboard device.
            this.keyboardDevice = new Keyboard(this.directInput);
            this.keyboardDevice.SetCooperativeLevel(this.formHandle, CooperativeLevel.Foreground | CooperativeLevel.NonExclusive);
            this.keyboardDevice.Properties.BufferSize = 128;

            // Initialize the mouse device.
            this.mouseDevice = new Mouse(this.directInput);
            this.mouseDevice.SetCooperativeLevel(this.formHandle, CooperativeLevel.Foreground | CooperativeLevel.NonExclusive);

            // Initialize the xbox game pad.
            this.gamepadDevice = new Controller(UserIndex.Any);
            if (this.gamepadDevice.IsConnected == true)
            {
                // Get the initial gamepad state.
                this.gamepadState = this.gamepadDevice.GetState();
            }

            // Successfully initialized.
            return true;
        }

        public bool DrawFrame(IRenderManager manager, Device device)
        {
            // Try to acquire the input devices.
            try
            {
                this.keyboardDevice.Acquire();
                this.mouseDevice.Acquire();
            }
            catch
            {
                return false;
            }

            // Save the previous input state.
            for (int i = 0; i < (int)InputAction.InputAction_Max; i++)
                this.PreviousButtonState[i] = this.ButtonState[i] == true;

            // Set the current button state to all false.
            for (int i = 0; i < (int)InputAction.InputAction_Max; i++)
                this.ButtonState[i] = false;

            // Poll the keyboard state and update input state.
            this.keyboardDevice.Poll();
            KeyboardState keyState = this.keyboardDevice.GetCurrentState();
            foreach (Key key in keyState.PressedKeys)
            {
                // Check the key code and update input state accordingly.
                switch (key)
                {
                    case Key.W: this.ButtonState[(int)InputAction.MoveForward] = true; break;
                    case Key.S: this.ButtonState[(int)InputAction.MoveBackward] = true; break;
                    case Key.A: this.ButtonState[(int)InputAction.StrafeLeft] = true; break;
                    case Key.D: this.ButtonState[(int)InputAction.StrafeRight] = true; break;
                    case Key.Z: this.ButtonState[(int)InputAction.MoveUp] = true; break;
                    case Key.X: this.ButtonState[(int)InputAction.MoveDown] = true; break;
                    case Key.Equals:
                    case Key.Add: this.ButtonState[(int)InputAction.CamSpeedIncrease] = true; break;
                    case Key.Minus:
                    case Key.Subtract: this.ButtonState[(int)InputAction.CamSpeedDecrease] = true; break;
                    case Key.PageUp: this.ButtonState[(int)InputAction.NextAnimation] = true; break;
                    case Key.PageDown: this.ButtonState[(int)InputAction.PreviousAnimation] = true; break;
                }
            }

            // Poll the mouse state.
            this.mouseDevice.Poll();
            MouseState mouseState = this.mouseDevice.GetCurrentState();

            // Update the mouse position and button state.
            this.MousePosition[0] = mouseState.X;
            this.MousePosition[1] = mouseState.Y;
            this.ButtonState[(int)InputAction.LeftClick] = mouseState.Buttons[0];
            this.ButtonState[(int)InputAction.RightClick] = mouseState.Buttons[2];

            // If the gamepad is connected update its state.
            if (this.gamepadDevice.IsConnected == true)
            {
                // Get the state of the gamepad and make sure input has changed since we last polled.
                State newState = this.gamepadDevice.GetState();
                if (newState.PacketNumber != this.gamepadState.PacketNumber)
                {
                    // Update button state.
                    this.ButtonState[(int)InputAction.MoveUp] = newState.Gamepad.RightTrigger > 0;
                    this.ButtonState[(int)InputAction.MoveDown] = newState.Gamepad.LeftTrigger > 0;
                    this.ButtonState[(int)InputAction.CamSpeedIncrease] = newState.Gamepad.Buttons.HasFlag(GamepadButtonFlags.Y);
                    this.ButtonState[(int)InputAction.CamSpeedDecrease] = newState.Gamepad.Buttons.HasFlag(GamepadButtonFlags.B);
                    this.ButtonState[(int)InputAction.NextAnimation] = newState.Gamepad.Buttons.HasFlag(GamepadButtonFlags.RightShoulder);
                    this.ButtonState[(int)InputAction.PreviousAnimation] = newState.Gamepad.Buttons.HasFlag(GamepadButtonFlags.LeftShoulder);

                    // Save the new gamepad state.
                    this.gamepadState = newState;
                }
            }

            // Release device access.
            //this.keyboardDevice.Unacquire();
            //this.mouseDevice.Unacquire();

            return true;
        }

        public void CleanupGraphics(IRenderManager manager, Device device)
        {
            // Release device access.
            this.keyboardDevice.Unacquire();
            this.mouseDevice.Unacquire();

            // Cleanup device objects.
            this.gamepadDevice = null;
            this.mouseDevice.Dispose();
            this.keyboardDevice.Dispose();
            this.directInput.Dispose();
        }

        #endregion
    }
}
