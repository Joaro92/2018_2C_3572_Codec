using Microsoft.Win32;
using System.Windows.Forms;
using TGC.Core.Example;
using TGC.Core.Sound;
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
        public TgcMp3Player Mp3Player { get; private set; }
        private string currentMp3File = null;

        private readonly string MusicDir = "Sounds\\Music\\";
        //private readonly string FXSoundsDir = "Sounds\\FX\\";
        //private readonly string UISoundsDir = "Sounds\\UI\\";

        public GameModel(string mediaDir, string shadersDir) : base(mediaDir, shadersDir)
        {
            Category = Game.Default.Category;
            Name = Game.Default.Name;
            Description = "Idea seleccionada: Twisted Metal - Derby de demolición";
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

            //initialize Mp3Player
            Mp3Player = new TgcMp3Player();

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
        }

        public void Exit()
        {
            var formEnumerator = Application.OpenForms.GetEnumerator();
            formEnumerator.MoveNext();
            var gameForm = (GameForm) formEnumerator.Current;
            gameForm.ShutDown();
            gameForm.Close();
        }



        public void LoadMp3(string fileName)
        {
            if (currentMp3File == null || currentMp3File != fileName)
            {
                currentMp3File = fileName;

                //Cargar archivo
                Mp3Player.closeFile();
                Mp3Player.FileName = MediaDir + MusicDir + currentMp3File;
            }
        }


        
    }
}