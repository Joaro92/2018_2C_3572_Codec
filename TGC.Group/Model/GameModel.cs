using TGC.Core.Example;
using TGC.Group.Model.GameStates;
using TGC.Group.Form;
using System.Windows.Forms;
using SharpDX.DirectInput;
using System;
using TGC.Group.Model.Interfaces;

namespace TGC.Group.Model
{
    public class GameModel : TgcExample
    {
        public IGameState GameState { get; set; }

        public static readonly string[] VehicleNames = { "Coupe", "Hatchback", "Microcargo", "Micro", "Minibus", "MPV", "Normal", "Pickup-Small", "Pickup", "Station" };
        public static readonly string[] VehicleColors = { "Blue", "Citrus", "Green", "Orange", "Red", "Silver", "Violet" };


        private Joystick joystick;
        private bool[] joyFlag = { false , false, false, false, false, false, false, false, false, false };
        private bool[] dpadFlag = { false, false, false, false };

        public GameModel(string mediaDir, string shadersDir) : base(mediaDir, shadersDir)
        {
            Category = Game.Default.Category;
            Name = Game.Default.Name;
            Description = "Idea seleccionada: Twisted Metal - Derby de demolición";
        }

        public override void Init()
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

            GameState = new MenuInicial(this);
        }

        public override void Update()
        {
            PreUpdate();

            GameState.Update();

            PostUpdate();
        }

        public override void Render()
        {
            PreRender();

            GameState.Render();
          
            PostRender();
        }

        public override void Dispose()
        {
            GameState.Dispose();
        }

        public void Exit()
        {
            var formEnumerator = Application.OpenForms.GetEnumerator();
            formEnumerator.MoveNext();
            var gameForm = (GameForm) formEnumerator.Current;
            gameForm.ShutDown();
            gameForm.Close();
        }

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

        public bool JoystickDpadPressed(Utils.JoystickDpad arrow)
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
    }
}