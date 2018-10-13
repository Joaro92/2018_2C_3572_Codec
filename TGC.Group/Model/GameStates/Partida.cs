using System.Drawing;
using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Examples.Camara;
using TGC.Group.Bullet.Physics;
using TGC.Group.Model.World;
using TGC.Group.Utils;
using TGC.Group.Model.Interfaces;
using TGC.Core.Direct3D;
using BulletSharp.Math;
using TGC.Core.Textures;
using Key = Microsoft.DirectX.DirectInput.Key;

namespace TGC.Group.Model.GameStates
{
    public class Partida : IGameState
    {
        private GameModel gameModel;

        private readonly string[] vehicleColors = new string[] { "Blue", "Citrus", "Green", "Orange", "Red", "Silver", "Violet" };
        private readonly ModoCamara[] modosCamara = new ModoCamara[] { ModoCamara.NORMAL, ModoCamara.LEJOS, ModoCamara.CERCA };

        private PhysicsGame world;
        private TgcThirdPersonCamera camaraInterna;
        private bool drawUpVector = false;
        private bool showBoundingBox = false;
        private TgcArrow directionArrow;
        private float anguloCamara;
        private float halfsPI;
        private bool mirarHaciaAtras;
        private ModoCamara modoCamara = ModoCamara.NORMAL;
        private HUD hud;
     
        public Partida(GameModel gameModel)
        {
            this.gameModel = gameModel;

            hud = new HUD(gameModel);

            // Preparamos el mundo físico con todos los elementos que pertenecen a el
            world = new NivelUno();
            world.Init();

            // Configuramos la Cámara en tercera persona para que siga a nuestro Player 1
            camaraInterna = new TgcThirdPersonCamera(new TGCVector3(world.Player1.rigidBody.CenterOfMassPosition), new TGCVector3(0, 2, 0), modoCamara.AlturaCamara(), modoCamara.ProfundidadCamara());
            this.gameModel.Camara = camaraInterna;

            // Creamos una flecha que representara el vector UP del auto
            directionArrow = new TgcArrow
            {
                BodyColor = Color.Red,
                HeadColor = Color.Green,
                Thickness = 0.1f,
                HeadSize = new TGCVector2(1, 2)
            };
        }

        public void Update()
        {
            // Mostrar bounding box del TgcMesh
            if (gameModel.Input.keyPressed(Key.F1))
            {
                showBoundingBox = !showBoundingBox;
            }

            // Girar la cámara unos grados
            if (gameModel.Input.keyPressed(Key.F2))
            {
                if (anguloCamara == 0.33f)
                {
                    anguloCamara = -anguloCamara;
                }
                else
                {
                    anguloCamara += 0.33f;
                }
            }

            // Dibujar el Vector UP
            if (gameModel.Input.keyPressed(Key.F3))
            {
                drawUpVector = !drawUpVector;
            }

            if (gameModel.Input.keyPressed(Key.F4))
            {
                TgcTexture[] diffuseMaps = world.Player1.tgcMesh.DiffuseMaps;

                string newTextureName = "";
                int index = 0;
                foreach (TgcTexture texture in diffuseMaps)
                {
                    if (texture.FileName.Contains("Car Material"))
                    {
                        newTextureName = texture.FilePath;
                        break;
                    }
                    index++;
                }

                string oldColor = newTextureName.Split('\\')[5].Split(' ')[2].Split('.')[0];
                string newColor = vehicleColors.getNextOption(oldColor);
                newTextureName = newTextureName.Replace(oldColor, newColor);

                var textureAux = TgcTexture.createTexture(D3DDevice.Instance.Device, newTextureName.Split('\\')[5], newTextureName);
                world.Player1.tgcMesh.addDiffuseMap(textureAux);
                world.Player1.tgcMesh.deleteDiffuseMap(index, 4); 
            }

            // Mirar hacia atras
            if (gameModel.Input.keyDown(Key.C) || gameModel.JoystickButtonDown(4))
            {
                mirarHaciaAtras = true;
                halfsPI = 0;
            }
            else
                mirarHaciaAtras = false;

            // Rotar 90° la cámara
            if (gameModel.Input.keyPressed(Key.F5))
            {
                halfsPI = (halfsPI + FastMath.PI_HALF) % FastMath.TWO_PI;
            }

            // Modo cámara
            if (gameModel.Input.keyPressed(Key.V))
            {
                modoCamara = modosCamara.getNextOption(modoCamara);

                camaraInterna.OffsetHeight = modoCamara.AlturaCamara();
                camaraInterna.OffsetForward = modoCamara.ProfundidadCamara();
            }


            // Hacer que la cámara apunte a nuestro Player 1
            camaraInterna.Target = new TGCVector3(world.Player1.rigidBody.CenterOfMassPosition);
            camaraInterna.RotationY = Quat.ToEulerAngles(world.Player1.rigidBody.Orientation).Y + anguloCamara + halfsPI + (mirarHaciaAtras ? FastMath.PI : 0);

            // Actualizar el Vector UP si se dibuja
            if (drawUpVector)
            {
                directionArrow.PStart = new TGCVector3(world.Player1.rigidBody.CenterOfMassPosition);
                directionArrow.PEnd = directionArrow.PStart + new TGCVector3(Vector3.TransformNormal(Vector3.UnitY, world.Player1.rigidBody.InterpolationWorldTransform)) * 3.5f;
                directionArrow.updateValues();
            }

            // Actualizar el mundo físico
            world.Update(gameModel, camaraInterna, modoCamara);

            hud.Update(gameModel, world.Player1);
        }

        public void Render()
        {
            if (world.Player1.hitPoints <= 0)
            {
                gameModel.Exit();
                return;
            }

            // Texto en pantalla sobre los comandos disponibles
            var DrawText = gameModel.DrawText;
            //DrawText.drawText("Con la tecla F1 se dibuja el bounding box (Deprecado, las colisiones las maneja Bullet)", 3, 20, Color.YellowGreen);
            //DrawText.drawText("Con la tecla F2 se rota el ángulo de la cámara", 3, 35, Color.YellowGreen);
            //DrawText.drawText("Con la tecla F3 se dibuja el Vector UP del vehículo", 3, 50, Color.YellowGreen);
            //DrawText.drawText("Con la tecla V se cambia el modo de cámara (NORMAL, LEJOS, CERCA)", 3, 65, Color.YellowGreen);
            //DrawText.drawText("W A S D para el movimiento básico", 3, 80, Color.YellowGreen);
            //DrawText.drawText("Control Izquierdo para frenar", 3, 95, Color.YellowGreen);
            //DrawText.drawText("Tecla ESPACIO para saltar", 3, 110, Color.YellowGreen);
            //DrawText.drawText("Tecla C para mirar hacia atrás", 3, 125, Color.YellowGreen);


            // Texto en pantalla sobre el juego
            //DrawText.drawText(player1.linealVelocity + " Km", (int)(screenWidth * 0.898f), (int)(screenHeight * 0.931f), Color.Black);


            //if (player1.flippedTime > 0)
            //{
            //    DrawText.drawText("Tiempo dado vuelta: " + player1.flippedTime, 15, screenHeight - 35, Color.White);
            //}

            // Renderiza todo lo perteneciente al mundo físico
            world.Render(gameModel);

            hud.Render();

            // Renderizar el Vector UP
            if (drawUpVector)
            {
                directionArrow.Render();
            }
        }

        public void Dispose()
        {
            world.Dispose();
            directionArrow.Dispose();
            hud.Dispose();
        }
    }
}