using Microsoft.DirectX.DirectInput;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using TGC.Core.Direct3D;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Sound;
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
        private Vehiculo selected;
        private TgcScene background;
        private Drawer2D drawer2D;
        private CustomSprite flechaIzq;
        private CustomSprite flechaDer;
        private CustomSprite flechaArriba;
        private TgcText2D select;

        private bool confirmed = false;

        public SelectorVehiculo(GameModel gameModel)
        {
            this.gameModel = gameModel;

            var screenHeight = D3DDevice.Instance.Device.Viewport.Height;
            var screenWidth = D3DDevice.Instance.Device.Viewport.Width;

            drawer2D = new Drawer2D();

            //cargo los vehiculos disponibles para seleccionar
            foreach (string name in GameModel.VehicleNames) {
                Vehiculo v = new Vehiculo(name);
                v.SampleMesh.Scale = TGCVector3.One * 15f;
                vehiculos.Add(v);
            }
            selected = vehiculos[0];

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
            var jh = gameModel.JoystickHandler;
            var sm = gameModel.SoundManager;

            if (gameModel.Input.keyPressed(Key.LeftArrow) || jh.JoystickDpadPressed(JoystickDpad.LEFT))
            {
                selected = vehiculos.ToArray().getNextOption(selected,-1);
                sm.PlaySound("menuLeft.wav");
            }

            if (gameModel.Input.keyPressed(Key.RightArrow) || jh.JoystickDpadPressed(JoystickDpad.RIGHT))
            {
                selected = vehiculos.ToArray().getNextOption(selected);
                sm.PlaySound("menuRight.wav");
            }

            if (gameModel.Input.keyPressed(Key.UpArrow) || jh.JoystickDpadPressed(JoystickDpad.UP))
            {
                var newColor = GameModel.VehicleColors.getNextOption(selected.Color);
                selected.ChangeColor(newColor);
            }

            if (gameModel.Input.keyPressed(Key.Return) || jh.JoystickButtonPressed(0) || jh.JoystickButtonPressed(7))
            {
                sm.Mp3Player.stop();
                sm.PlaySound("menuEngine.wav");
                Thread.Sleep(1000);
                confirmed = true;
            }

            selected.SampleMesh.RotateY(FastMath.QUARTER_PI * gameModel.ElapsedTime);

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

            if (confirmed)
            {
                gameModel.GameState = new Partida(gameModel, selected);
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
        }
    }
}
