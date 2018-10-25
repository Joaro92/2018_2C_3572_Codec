using Microsoft.DirectX.Direct3D;
using Microsoft.Win32;
using SharpDX.DirectInput;
using System;
using System.Drawing;
using System.Windows.Forms;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Textures;
using TGC.Group.Form;
using TGC.Group.Model.GameStates;
using TGC.Group.Model.Interfaces;
using DeviceType = SharpDX.DirectInput.DeviceType;

namespace TGC.Group.Model
{
    public class GameModel : TgcExample
    {
        public IGameState GameState { get; set; }

        private Joystick joystick { get; set; }
        public SoundManager SoundManager { get; private set; }
        public new Input Input { get; private set; }

        public GameModel(string mediaDir, string shadersDir) : base(mediaDir, shadersDir)
        {
            Category = Game.Default.Category;
            Name = Game.Default.Name;
            Description = Game.Default.Description;
        }

        public static int GetWindowsScaling()
        {
            return (int)Registry.GetValue("HKEY_CURRENT_USER\\Control Panel\\Desktop\\WindowMetrics", "AppliedDPI", 96);
        }

        // ----------------------------------------------

        public override void Init()
        {
            //Initialize Joystick
            ObtainJoystickController();

            //New Input interface
            Input = new Input(base.Input, joystick);

            //Initialize Sound Manager
            SoundManager = new SoundManager(this.DirectSound.DsDevice);

            //Start the game
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
            Input.Dispose();
            SoundManager.Dispose();
        }

        public void Exit()
        {
            var formEnumerator = Application.OpenForms.GetEnumerator();
            formEnumerator.MoveNext();
            var gameForm = (GameForm)formEnumerator.Current;
            gameForm.ShutDown();
            gameForm.Close();
        }


        // --------------------------------------------


        protected override void PreRender()
        {
            D3DDevice.Instance.Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.FromArgb(1, 211 / 2, 206 / 2, 170 / 2), 1, 0);
            D3DDevice.Instance.Device.BeginScene();
            TexturesManager.Instance.clearAll();
        }

        protected override void PostRender()
        {
            RenderFPS();
            D3DDevice.Instance.Device.EndScene();
            D3DDevice.Instance.Device.Present();
        }

        private void ObtainJoystickController()
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

    }
}