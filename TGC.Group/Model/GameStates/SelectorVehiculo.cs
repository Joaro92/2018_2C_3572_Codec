using Microsoft.DirectX.DirectInput;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using TGC.Core.Direct3D;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using Button = TGC.Group.Model.Input.Button;
using Dpad = TGC.Group.Model.Input.Dpad;
using TGC.Core.Text;
using TGC.Examples.Camara;
using TGC.Group.Model.Interfaces;
using TGC.Group.Model.TGCUtils;
using TGC.Group.Model.Vehicles;
using TGC.Group.Utils;

namespace TGC.Group.Model.GameStates
{
    class SelectorVehiculo : IGameState
    {
        private GameModel gameModel;
        private List<Vehiculo> vehiculos = new List<Vehiculo>();
        private string[] colors;
        private Vehiculo selected;
        private TgcScene background;
        private Drawer2D drawer2D;
        private CustomSprite flechaIzq;
        private CustomSprite flechaDer;
        private CustomSprite flechaArriba;
        private TgcText2D vehicleName;
        private TgcText2D select;

        private bool confirmed = false;
        private bool back = false;

        public SelectorVehiculo(GameModel gameModel)
        {
            this.gameModel = gameModel;

            var screenHeight = D3DDevice.Instance.Device.Viewport.Height;
            var screenWidth = D3DDevice.Instance.Device.Viewport.Width;

            drawer2D = new Drawer2D();

            //cargo los vehiculos disponibles para seleccionar
            foreach (string name in Game.Default.VehicleNames) {
                Vehiculo v = new Vehiculo(name);
                v.SampleMesh.Scale = TGCVector3.One * 15f;
                vehiculos.Add(v);
            }
            selected = vehiculos[0];

            //cargo los colores
            colors = new string[Game.Default.VehicleColors.Count];
            Game.Default.VehicleColors.CopyTo(colors, 0);

            //cargo el fondo en 3D del selector
            var loader = new TgcSceneLoader();
            var scenesDir = Game.Default.MediaDirectory + Game.Default.ScenariosDirectory;
            this.background = loader.loadSceneFromFile(scenesDir + "selector-vehiculo-TgcScene.xml");

            this.gameModel.Camara = new TgcThirdPersonCamera(selected.SampleMesh.Position, 100f, 170f);

            //Cargo los sprites
            var imgDir = Game.Default.MediaDirectory + Game.Default.ImagesDirectory;
            //flecha izquierda
            flechaIzq = new CustomSprite
            {
                Bitmap = new CustomBitmap(imgDir + "left-arrow.png", D3DDevice.Instance.Device),
                Scaling = TGCVector2.One * 0.25f,
                Position = new TGCVector2(screenWidth * 0.25f, screenHeight * 0.4f)
            };

            //flecha derecha
            flechaDer = new CustomSprite
            {
                Bitmap = new CustomBitmap(imgDir + "right-arrow.png", D3DDevice.Instance.Device),
                Scaling = TGCVector2.One * 0.25f,
                Position = new TGCVector2(screenWidth * 0.7f, screenHeight * 0.4f)
            };

            //flecha arriba
            flechaArriba = new CustomSprite
            {
                Bitmap = new CustomBitmap(imgDir + "up-arrow.png", D3DDevice.Instance.Device),
                Scaling = TGCVector2.One * 0.25f,
                Position = new TGCVector2(screenWidth * 0.475f, screenHeight * 0.05f)
            };

            //Nombre vehiculo
            vehicleName = new TgcText2D
            {
                Text = selected.Name,
                Color = Color.Gold,
                Position = new Point(0, (int)(screenHeight * 0.65f)),
            };
            vehicleName.changeFont(UtilMethods.createFont("Twisted Stallions", 45));

            //Select Vehicle
            select = new TgcText2D
            {
                Text = "SELECT VEHICLE",
                Color = Color.DarkOrange,
                Position = new Point(0, (int)(screenHeight * 0.75f)),
            };
            select.changeFont(UtilMethods.createFont("Twisted Stallions",75));
        }

        public void Update()
        {
            var Input = gameModel.Input;
            var sm = gameModel.SoundManager;

            if (Input.keyPressed(Key.LeftArrow) || Input.buttonPressed(Dpad.LEFT))
            {
                selected = vehiculos.ToArray().getNextOption(selected,-1);
                sm.PlaySound("menuLeft.wav");
            }

            if (Input.keyPressed(Key.RightArrow) || Input.buttonPressed(Dpad.RIGHT))
            {
                selected = vehiculos.ToArray().getNextOption(selected);
                sm.PlaySound("menuRight.wav");
            }

            if (Input.keyPressed(Key.UpArrow) || Input.buttonPressed(Dpad.UP))
            {              
                var newColor = colors.getNextOption(selected.Color);
                selected.ChangeColor(newColor);
            }

            if (Input.keyPressed(Key.Return) || Input.buttonPressed(Button.START) || Input.buttonPressed(Button.X))
            {
                sm.Mp3Player.stop();
                sm.PlaySound("menuEngine.wav");
                Thread.Sleep(1000);
                confirmed = true;
            }

            selected.SampleMesh.RotateY(FastMath.QUARTER_PI * gameModel.ElapsedTime);
            vehicleName.Text = selected.Name;

            if (Input.keyPressed(Key.Escape))
            {
                sm.PlaySound("menuEnter.wav");
                back = true;
            }
        }

        public void Render()
        {
            selected.SampleMesh.Render();

            background.RenderAll();

            drawer2D.BeginDrawSprite();

            drawer2D.DrawSprite(flechaIzq);
            drawer2D.DrawSprite(flechaDer);
            drawer2D.DrawSprite(flechaArriba);

            drawer2D.EndDrawSprite();

            select.render();
            vehicleName.render();

            if (confirmed)
            {
                gameModel.GameState = new Partida(gameModel, selected);
                this.Dispose();
            }
            else if (back)
            {
                gameModel.GameState = new MenuInicial(gameModel);
                this.Dispose();
            }
        }

        public void Dispose()
        {
            background.DisposeAll();
            vehiculos.ForEach(v => v.SampleMesh.Dispose());
            flechaArriba.Dispose();
            flechaIzq.Dispose();
            flechaDer.Dispose();
            select.Dispose();
            vehicleName.Dispose();
        }
    }
}
