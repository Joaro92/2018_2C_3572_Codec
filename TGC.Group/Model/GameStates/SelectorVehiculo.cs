using Microsoft.DirectX.DirectInput;
using System.Collections.Generic;
using System.Drawing;
using TGC.Core.Direct3D;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
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
            this.background = loader.loadSceneFromFile(gameModel.MediaDir + "Scenarios\\selector-vehiculo-TgcScene.xml");

            this.gameModel.Camara = new TgcThirdPersonCamera(selected.SampleMesh.Position, 100f, 170f);

            //Cargo los sprites
            //flecha izquierda
            flechaIzq = new CustomSprite
            {
                Bitmap = new CustomBitmap(gameModel.MediaDir + "Images\\left-arrow.png", D3DDevice.Instance.Device),
                Scaling = TGCVector2.One * 0.25f,
                Position = new TGCVector2(screenWidth * 0.25f, screenHeight * 0.4f)
            };

            //flecha derecha
            flechaDer = new CustomSprite
            {
                Bitmap = new CustomBitmap(gameModel.MediaDir + "Images\\right-arrow.png", D3DDevice.Instance.Device),
                Scaling = TGCVector2.One * 0.25f,
                Position = new TGCVector2(screenWidth * 0.7f, screenHeight * 0.4f)
            };

            //flecha arriba
            flechaArriba = new CustomSprite
            {
                Bitmap = new CustomBitmap(gameModel.MediaDir + "Images\\up-arrow.png", D3DDevice.Instance.Device),
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
            if (gameModel.Input.keyPressed(Key.LeftArrow) || gameModel.JoystickDpadPressed(JoystickDpad.LEFT))
            {
                selected = vehiculos.ToArray().getNextOption(selected,-1);    
            }

            if (gameModel.Input.keyPressed(Key.RightArrow) || gameModel.JoystickDpadPressed(JoystickDpad.RIGHT))
            {
                selected = vehiculos.ToArray().getNextOption(selected);
            }

            if (gameModel.Input.keyPressed(Key.UpArrow) || gameModel.JoystickDpadPressed(JoystickDpad.UP))
            {
                var newColor = GameModel.VehicleColors.getNextOption(selected.Color);
                selected.ChangeColor(newColor);
            }

            if (gameModel.Input.keyPressed(Key.Return) || gameModel.JoystickButtonPressed(0))
            {
                gameModel.Mp3Player.stop();
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
