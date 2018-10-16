using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Group.Utils;

namespace TGC.Group.Model
{
    public class JoystickHandler
    {
        private Joystick joystick;

        private bool[] joyFlag = { false, false, false, false, false, false, false, false, false, false };
        private bool[] dpadFlag = { false, false, false, false };
        private int doubleTap = 0;
        private float elapsedTime = 0;

        public JoystickHandler()
        {
            // Initialize DirectInput
            var directInput = new DirectInput();

            // Find a Joystick Guid
            var joystickGuid = Guid.Empty;
            foreach (var deviceInstance in directInput.GetDevices(DeviceType.Gamepad, DeviceEnumerationFlags.AllDevices))
            {
                joystickGuid = deviceInstance.InstanceGuid;
            }

            // If Gamepad not found, look for a Joystick
            if (joystickGuid == Guid.Empty)
                foreach (var deviceInstance in directInput.GetDevices(DeviceType.Joystick, DeviceEnumerationFlags.AllDevices))
                {
                    joystickGuid = deviceInstance.InstanceGuid;
                }

            // Configure and set Joystick only if found
            if (joystickGuid != Guid.Empty)
            {
                // Instantiate the joystick
                joystick = new Joystick(directInput, joystickGuid);

                Console.WriteLine("Found Joystick/Gamepad with GUID: {0}", joystickGuid);

                // Set BufferSize in order to use buffered data.
                joystick.Properties.BufferSize = 2048;

                // Acquire the joystick
                joystick.Acquire();
            }


        }

        // ----------------------------------------------
        // --------------- Joystick input ---------------
        // ----------------------------------------------

        public bool JoystickButtonPressed(int buttonID)
        {
            if (joystick == null) return false;
            joystick.Poll();

            if (joyFlag[buttonID] == false)
            {
                joyFlag[buttonID] = joystick.GetCurrentState().Buttons[buttonID];

                return false;
            }
            else
            {
                if (joystick.GetCurrentState().Buttons[buttonID] == false)
                {
                    joyFlag[buttonID] = false;
                    return true;
                }
                else return false;
            }
        }
        public bool JoystickButtonPressedDouble(int buttonID, float ElapsedTime)
        {
            if (joystick == null) return false;
            joystick.Poll();


            if (doubleTap == 1)
            {
                elapsedTime += ElapsedTime;

                if (joystick.GetCurrentState().Buttons[buttonID])
                {
                    doubleTap = 2;
                    elapsedTime = 0;
                    return true;
                }
                
                if (elapsedTime > 0.15f)
                {
                    elapsedTime = 0;
                    doubleTap = 0;
                    return false;
                }
                return false;
            }

            if (doubleTap == 2 && joystick.GetCurrentState().Buttons[buttonID]) return true;
            else doubleTap = 0;

            if (joyFlag[buttonID] == false)
            {
                joyFlag[buttonID] = joystick.GetCurrentState().Buttons[buttonID];

                return false;
            }
            else
            {
                if (joystick.GetCurrentState().Buttons[buttonID] == false)
                {
                    joyFlag[buttonID] = false;
                    doubleTap = 1;
                    return false;
                }
                else return false;
            }
        }

        public bool JoystickDpadPressed(JoystickDpad arrow)
        {
            if (joystick == null) return false;
            int value = 0;
            joystick.Poll();

            switch (arrow)
            {
                case Utils.JoystickDpad.UP:
                    value = 0;
                    break;
                case Utils.JoystickDpad.RIGHT:
                    value = 9000;
                    break;
                case Utils.JoystickDpad.DOWN:
                    value = 18000;
                    break;
                case Utils.JoystickDpad.LEFT:
                    value = 27000;
                    break;
            }

            if (dpadFlag[(int)arrow] == false)
            {
                dpadFlag[(int)arrow] = joystick.GetCurrentState().PointOfViewControllers[0] == value;

                return false;
            }
            else
            {
                if (joystick.GetCurrentState().PointOfViewControllers[0] != value)
                {
                    dpadFlag[(int)arrow] = false;
                    return true;
                }
                else return false;
            }
        }

        public bool JoystickButtonDown(int buttonID)
        {
            if (joystick == null) return false;
            joystick.Poll();

            return joystick.GetCurrentState().Buttons[buttonID];
        }

        public bool JoystickDpadLeft()
        {
            if (joystick == null) return false;

            joystick.Poll();

            return joystick.GetCurrentState().PointOfViewControllers[0] == 27000;
        }

        public bool JoystickDpadRight()
        {
            if (joystick == null) return false;

            joystick.Poll();

            return joystick.GetCurrentState().PointOfViewControllers[0] == 9000;
        }

        public bool JoystickR2Down()
        {
            if (joystick == null) return false;
            joystick.Poll();

            return joystick.GetCurrentState().Z < 13000;
        }

        public int JoystickRightStick()
        {
            if (joystick == null) return 0;
            joystick.Poll();

            return joystick.GetCurrentState().RotationX - 32768;
        }

        public void Dispose()
        {
            if (joystick != null) joystick.Dispose();
        }
    }
}
