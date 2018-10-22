using SharpDX.DirectInput;
using TGC.Core.Input;
using Key = Microsoft.DirectX.DirectInput.Key;

namespace TGC.Group.Model
{
    public class Input
    {
        private TgcD3dInput keyboard;
        private Joystick joystick;

        public enum Button { X, CIRCLE, SQUARE, TRIANGLE, L1, R1, SELECT, START, L3, R3, L2, R2 };
        public enum Dpad { UP, RIGHT, DOWN, LEFT };


        private bool[] joyFlag = { false, false, false, false, false, false, false, false, false, false };
        private bool[] dpadFlag = { false, false, false, false };
        private bool L2flag = false;
        private bool R2flag = false;

        private int doubleTap = 0;
        private float elapsedTime = 0;

        public Input(TgcD3dInput _keyboard, Joystick _joystick)
        {
            this.keyboard = _keyboard;
            this.joystick = _joystick;
        }

        public bool keyPressed(Key key)
        {
            return keyboard.keyPressed(key);
        }

        public bool keyDown(Key key)
        {
            return keyboard.keyDown(key);
        }

        public bool keyUp(Key key)
        {
            return keyboard.keyUp(key);
        }

        public bool buttonPressed(Button button)
        {
            if (joystick == null) return false;
            joystick.Poll();

            int buttonID = (int)button;

            switch (button)
            {
                case Button.L2:
                    return L2Pressed();
                case Button.R2:
                    return R2Pressed();
                default:
                    return NormalButtonPressed(buttonID);
            }
        }
        public bool buttonDown(Button button)
        {
            if (joystick == null) return false;
            joystick.Poll();

            int buttonID = (int)button;

            switch (button)
            {
                case Button.L2:
                    return L2Down();
                case Button.R2:
                    return R2Down();
                default:
                    return NormalButtonDown(buttonID);
            }
        }

        public bool buttonPressed(Dpad arrow)
        {
            if (joystick == null) return false;
            joystick.Poll();

            int arrowID = (int)arrow;

            return ArrowPressed(arrowID);
        }

        public bool buttonDown(Dpad arrow)
        {
            if (joystick == null) return false;
            joystick.Poll();

            int arrowID = (int)arrow;

            return ArrowDown(arrowID);
        }

        public int JoystickRightStick()
        {
            if (joystick == null) return 0;
            joystick.Poll();

            return joystick.GetCurrentState().RotationX - 32768;
        }

        public int JoystickLeftStick()
        {
            if (joystick == null) return 0;
            joystick.Poll();

            return joystick.GetCurrentState().X - 32768;
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

        public void Dispose()
        {
            if (joystick != null) joystick.Dispose();
        }


        // ------- Metodos Privados -------


        private bool NormalButtonPressed(int id)
        {
            if (!joyFlag[id])
            {
                if (joystick.GetCurrentState().Buttons[id])
                {
                    joyFlag[id] = true;
                    return true;
                }
                return false;
            }
            else
            {
                if (!joystick.GetCurrentState().Buttons[id])
                {
                    joyFlag[id] = false;
                }
                return false;
            }
        }

        private bool L2Pressed()
        {
            if (!L2flag)
            {
                if (joystick.GetCurrentState().Z > 51000)
                {
                    L2flag = true;
                    return true;
                }
                return false;
            }
            else
            {
                if (joystick.GetCurrentState().Z < 51000)
                {
                    L2flag = false;
                }
                return false;
            }
        }

        private bool R2Pressed()
        {
            if (!R2flag)
            {
                if (joystick.GetCurrentState().Z < 13000)
                {
                    R2flag = true;
                    return true;
                }
                return false;
            }
            else
            {
                if (joystick.GetCurrentState().Z > 13000)
                {
                    R2flag = false;
                }
                return false;
            }
        }

        private bool ArrowPressed(int id)
        {
            var value = 9000 * id;

            if (!dpadFlag[id])
            {
                if (joystick.GetCurrentState().PointOfViewControllers[0] == value)
                {
                    dpadFlag[id] = true;
                    return true;
                }
                return false;
            }
            else
            {
                if (joystick.GetCurrentState().PointOfViewControllers[0] != value)
                {
                    dpadFlag[id] = false;
                }
                return false;
            }
        }

        private bool NormalButtonDown(int id)
        {
            return joystick.GetCurrentState().Buttons[id];
        }

        private bool L2Down()
        {
            return joystick.GetCurrentState().Z > 51000;
        }

        private bool R2Down()
        {
            return joystick.GetCurrentState().Z < 13000;
        }

        private bool ArrowDown(int id)
        {
            var value = 9000 * id;

            return joystick.GetCurrentState().PointOfViewControllers[0] == value;
        }
    }
}
