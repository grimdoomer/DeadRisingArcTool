using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DirectInput;
using SharpDX.XInput;
using SharpDX.RawInput;
using Device = SharpDX.RawInput.Device;

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
        MiddleMouse,

        // Camera misc:
        CamSpeedIncrease, 
        CamSpeedDecrease,

        // Animation controls:
        NextAnimation,
        PreviousAnimation,

        // Misc actions:
        MiscAction1,
        MiscAction2,

        InputAction_Max
    }

    public class InputManager : IRenderable
    {
        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(ref System.Drawing.Point point);

        [DllImport("user32.dll")]
        public static extern bool ScreenToClient(IntPtr handle, ref System.Drawing.Point point);

        /// <summary>
        /// Button state from the previous input poll operation
        /// </summary>
        public bool[] PreviousButtonState { get; protected set; } = new bool[(int)InputAction.InputAction_Max];
        /// <summary>
        /// Input state for the current input poll operation
        /// </summary>
        public bool[] ButtonState { get; protected set; } = new bool[(int)InputAction.InputAction_Max];

        public bool[] KeyboardState { get; set; } = new bool[255];

        private System.Drawing.Point mousePosition;
        /// <summary>
        /// XY coordinates of the mouse position within the application window
        /// </summary>
        public System.Drawing.Point MousePosition { get { return this.mousePosition; } }
        /// <summary>
        /// Change in XY coordinates and mouse wheel position since the last update
        /// </summary>
        public int[] MousePositionDelta { get; protected set; } = new int[3];
        /// <summary>
        /// True if the xbox gamepad is connected
        /// </summary>
        public bool IsGamepadConnected { get { return this.gamepadDevice.IsConnected; } }
        /// <summary>
        /// Values of the gamepad thumbsticks in the order: LThumbX, LThumbY, RThumbX, RThumbY
        /// Thumbstick values are only > 0 when they are outside of the dead zone
        /// </summary>
        public short[] GamepadThumbSticks { get; protected set; } = new short[4];
        /// <summary>
        /// Values of the gamepad triggers in the order: Left, Right
        /// Trigger values are only > 0 when they are outside of the dead zone
        /// </summary>
        public byte[] GamepadTriggers { get; protected set; } = new byte[2];

        // Control handle we bind to for input focus.
        IntPtr formHandle;

        // Direct input HID devices.
        private DirectInput directInput = null;
        private Mouse mouseDevice = null;
        private Controller gamepadDevice = null;

        private State gamepadState;
        private State previousGamepadState;

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

        public bool InitializeGraphics(RenderManager manager)
        {
            // Initialize the direct input object.
            this.directInput = new DirectInput();

            // Initialize the mouse device.
            this.mouseDevice = new Mouse(this.directInput);
            this.mouseDevice.SetCooperativeLevel(this.formHandle, CooperativeLevel.Foreground | CooperativeLevel.NonExclusive);

            // Initialize the xbox game pad.
            this.gamepadDevice = new Controller(UserIndex.One);
            if (this.gamepadDevice.IsConnected == true)
            {
                // Get the initial gamepad state.
                this.gamepadState = this.gamepadDevice.GetState();
            }

            // Successfully initialized.
            return true;
        }

        public bool DrawFrame(RenderManager manager)
        {
            // Try to acquire the input devices.
            try
            {
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

            // Update keyboard input.
            this.ButtonState[(int)InputAction.MoveForward] = this.KeyboardState[(int)System.Windows.Forms.Keys.W];
            this.ButtonState[(int)InputAction.MoveBackward] = this.KeyboardState[(int)System.Windows.Forms.Keys.S];
            this.ButtonState[(int)InputAction.StrafeLeft] = this.KeyboardState[(int)System.Windows.Forms.Keys.A];
            this.ButtonState[(int)InputAction.StrafeRight] = this.KeyboardState[(int)System.Windows.Forms.Keys.D];
            this.ButtonState[(int)InputAction.MoveUp] = this.KeyboardState[(int)System.Windows.Forms.Keys.X];
            this.ButtonState[(int)InputAction.MoveDown] = this.KeyboardState[(int)System.Windows.Forms.Keys.Z];
            this.ButtonState[(int)InputAction.CamSpeedIncrease] = this.KeyboardState[(int)System.Windows.Forms.Keys.Oemplus] || this.KeyboardState[(int)System.Windows.Forms.Keys.Add];
            this.ButtonState[(int)InputAction.CamSpeedDecrease] = this.KeyboardState[(int)System.Windows.Forms.Keys.OemMinus] || this.KeyboardState[(int)System.Windows.Forms.Keys.Subtract];
            this.ButtonState[(int)InputAction.NextAnimation] = this.KeyboardState[(int)System.Windows.Forms.Keys.PageUp];
            this.ButtonState[(int)InputAction.PreviousAnimation] = this.KeyboardState[(int)System.Windows.Forms.Keys.PageDown];
            this.ButtonState[(int)InputAction.MiscAction1] = this.KeyboardState[(int)System.Windows.Forms.Keys.D1] || this.KeyboardState[(int)System.Windows.Forms.Keys.NumPad1];
            this.ButtonState[(int)InputAction.MiscAction2] = this.KeyboardState[(int)System.Windows.Forms.Keys.D2] || this.KeyboardState[(int)System.Windows.Forms.Keys.NumPad2];

            // Poll the mouse state.
            this.mouseDevice.Poll();
            MouseState mouseState = this.mouseDevice.GetCurrentState();

            // Update cursor position.
            GetCursorPos(ref this.mousePosition);
            ScreenToClient(this.formHandle, ref this.mousePosition);

            // Update the mouse position and button state.
            this.MousePositionDelta[0] = mouseState.X;
            this.MousePositionDelta[1] = mouseState.Y;
            this.MousePositionDelta[2] = mouseState.Z;
            this.ButtonState[(int)InputAction.LeftClick] = mouseState.Buttons[0];
            this.ButtonState[(int)InputAction.RightClick] = mouseState.Buttons[1];
            this.ButtonState[(int)InputAction.MiddleMouse] = mouseState.Buttons[2];

            // If the gamepad is connected update its state.
            if (this.gamepadDevice.IsConnected == true)
            {
                // Get the state of the gamepad and make sure input has changed since we last polled.
                State newState = this.gamepadDevice.GetState();
                if (newState.PacketNumber != this.gamepadState.PacketNumber)
                {
                    // Save the previous gamepad state, only do this when the packet number changes.
                    this.previousGamepadState = gamepadState;

                    // Update thumbstick state.
                    if (newState.Gamepad.LeftThumbX > Gamepad.LeftThumbDeadZone || newState.Gamepad.LeftThumbX < -Gamepad.LeftThumbDeadZone)
                        this.GamepadThumbSticks[0] = newState.Gamepad.LeftThumbX;
                    else
                        this.GamepadThumbSticks[0] = 0;

                    if (newState.Gamepad.LeftThumbY > Gamepad.LeftThumbDeadZone || newState.Gamepad.LeftThumbY < -Gamepad.LeftThumbDeadZone)
                        this.GamepadThumbSticks[1] = newState.Gamepad.LeftThumbY;
                    else
                        this.GamepadThumbSticks[1] = 0;

                    if (newState.Gamepad.RightThumbX > Gamepad.RightThumbDeadZone || newState.Gamepad.RightThumbX < -Gamepad.RightThumbDeadZone)
                        this.GamepadThumbSticks[2] = newState.Gamepad.RightThumbX;
                    else
                        this.GamepadThumbSticks[2] = 0;

                    if (newState.Gamepad.RightThumbY > Gamepad.RightThumbDeadZone || newState.Gamepad.RightThumbY < -Gamepad.RightThumbDeadZone)
                        this.GamepadThumbSticks[3] = newState.Gamepad.RightThumbY;
                    else
                        this.GamepadThumbSticks[3] = 0;

                    // Update trigger state.
                    if (newState.Gamepad.LeftTrigger > Gamepad.TriggerThreshold)
                        this.GamepadTriggers[0] = newState.Gamepad.LeftTrigger;
                    else
                        this.GamepadTriggers[0] = 0;

                    if (newState.Gamepad.RightTrigger > Gamepad.TriggerThreshold)
                        this.GamepadTriggers[1] = newState.Gamepad.RightTrigger;
                    else
                        this.GamepadTriggers[1] = 0;

                    // Update button state.
                    this.ButtonState[(int)InputAction.MoveUp] = newState.Gamepad.Buttons.HasFlag(GamepadButtonFlags.RightThumb);
                    this.ButtonState[(int)InputAction.MoveDown] = newState.Gamepad.Buttons.HasFlag(GamepadButtonFlags.LeftThumb);
                    this.ButtonState[(int)InputAction.NextAnimation] = newState.Gamepad.Buttons.HasFlag(GamepadButtonFlags.RightShoulder);
                    this.ButtonState[(int)InputAction.PreviousAnimation] = newState.Gamepad.Buttons.HasFlag(GamepadButtonFlags.LeftShoulder);
                    this.ButtonState[(int)InputAction.MiscAction1] = newState.Gamepad.Buttons.HasFlag(GamepadButtonFlags.X);
                    this.ButtonState[(int)InputAction.MiscAction2] = newState.Gamepad.Buttons.HasFlag(GamepadButtonFlags.Y);

                    // Save the new gamepad state.
                    this.gamepadState = newState;
                }
                else
                {
                    // Input state is the same as the previous packet. Copy over all button state values.
                    if (this.ButtonState[(int)InputAction.MoveUp] == false)
                        this.ButtonState[(int)InputAction.MoveUp] = this.previousGamepadState.Gamepad.Buttons.HasFlag(GamepadButtonFlags.RightThumb);
                    if (this.ButtonState[(int)InputAction.MoveDown] == false)
                        this.ButtonState[(int)InputAction.MoveDown] = this.previousGamepadState.Gamepad.Buttons.HasFlag(GamepadButtonFlags.LeftThumb);
                    if (this.ButtonState[(int)InputAction.MiscAction1] == false)
                        this.ButtonState[(int)InputAction.MiscAction1] = this.previousGamepadState.Gamepad.Buttons.HasFlag(GamepadButtonFlags.X);
                    if (this.ButtonState[(int)InputAction.MiscAction2] == false)
                        this.ButtonState[(int)InputAction.MiscAction2] = this.previousGamepadState.Gamepad.Buttons.HasFlag(GamepadButtonFlags.Y);
                }
            }

            // Release device access.
            //this.mouseDevice.Unacquire();

            return true;
        }

        public void DrawObjectPropertiesUI(RenderManager manager)
        {

        }

        public void CleanupGraphics(RenderManager manager)
        {
            // Release device access.
            this.mouseDevice.Unacquire();

            // Cleanup device objects.
            this.gamepadDevice = null;
            this.mouseDevice.Dispose();
            this.directInput.Dispose();
        }

        #endregion
    }
}
