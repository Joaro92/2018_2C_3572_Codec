using Microsoft.Win32;
using System.Windows.Forms;
using TGC.Core.Example;
using TGC.Group.Form;
using TGC.Group.Model.GameStates;
using TGC.Group.Model.Interfaces;

namespace TGC.Group.Model
{
    public class GameModel : TgcExample
    {
        public IGameState GameState { get; set; }

        public static readonly string[] VehicleNames = { "Coupe", "Hatchback", "Microcargo", "Micro", "Minibus", "MPV", "Normal", "Pickup-Small", "Pickup", "Station" };
        public static readonly string[] VehicleColors = { "Blue", "Citrus", "Green", "Orange", "Red", "Silver", "Violet" };

        public JoystickHandler JoystickHandler { get; private set; }
        public SoundManager SoundManager { get; private set; }

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
            //initialize Joystick
            JoystickHandler = new JoystickHandler();

            //initialize Sound Manager
            SoundManager = new SoundManager(this.DirectSound.DsDevice);

            //start the game
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
            JoystickHandler.Dispose();
            SoundManager.Dispose();
            
        }

        public void Exit()
        {
            var formEnumerator = Application.OpenForms.GetEnumerator();
            formEnumerator.MoveNext();
            var gameForm = (GameForm) formEnumerator.Current;
            gameForm.ShutDown();
            gameForm.Close();
        }
    }
}