using System.Drawing;
using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Examples.Camara;
using TGC.Group.Bullet.Physics;
using TGC.Group.Model.World;
using TGC.Group.Utils;
using TGC.Group.Model.Interfaces;
using BulletSharp.Math;
using Key = Microsoft.DirectX.DirectInput.Key;
using TGC.Group.Model.Vehicles;

namespace TGC.Group.Model.GameStates
{
    public class Partida : IGameState
    {
        private readonly ModoCamara[] modosCamara = new ModoCamara[] { ModoCamara.NORMAL, ModoCamara.LEJOS, ModoCamara.CERCA };
        private ModoCamara modoCamara = ModoCamara.NORMAL;

        private GameModel gameModel;
        private PhysicsGame world;
        private TgcThirdPersonCamera camaraInterna;
        private HUD hud;
        private TgcArrow directionArrow;
        private bool drawUpVector = false;
        private bool mirarHaciaAtras;
        private float anguloCamara;
        private float halfsPI;
        private string levelSong = "Sounds\\Twisted Metal Small Brawl - Now Slaying.mp3";

        public Partida(GameModel gameModel, Vehiculo vehiculoP1)
        {
            this.gameModel = gameModel;

            hud = new HUD(gameModel);

            // Preparamos el mundo físico con todos los elementos que pertenecen a el
            world = new NivelUno(vehiculoP1);

            // Configuramos la Cámara en tercera persona para que siga a nuestro Player 1
            camaraInterna = new TgcThirdPersonCamera(new TGCVector3(world.player1.RigidBody.CenterOfMassPosition), new TGCVector3(0, 2, 0), modoCamara.AlturaCamara(), modoCamara.ProfundidadCamara());
            this.gameModel.Camara = camaraInterna;

            // Creamos una flecha que representara el vector UP del auto
            directionArrow = new TgcArrow
            {
                BodyColor = Color.Red,
                HeadColor = Color.Green,
                Thickness = 0.1f,
                HeadSize = new TGCVector2(0.5f, 1f)
            };

            this.gameModel.loadMp3(gameModel.MediaDir + levelSong);
            this.gameModel.Mp3Player.play(true);
        }

        public void Update()
        {
            // Manejar los inputs del teclado y joystick
            ManageInputs(gameModel);

            // Actualizar la posición y rotación de la cámara para que apunte a nuestro Player 1
            camaraInterna.Target = new TGCVector3(world.player1.RigidBody.CenterOfMassPosition);
            camaraInterna.RotationY = Quat.ToEulerAngles(world.player1.RigidBody.Orientation).Y + anguloCamara + halfsPI + (mirarHaciaAtras ? FastMath.PI : 0);

            // Actualizar el Vector UP si se dibuja
            if (drawUpVector)
            {
                directionArrow.PStart = new TGCVector3(world.player1.RigidBody.CenterOfMassPosition);
                directionArrow.PEnd = directionArrow.PStart + new TGCVector3(Vector3.TransformNormal(Vector3.UnitY, world.player1.RigidBody.InterpolationWorldTransform)) * 3.5f;
                directionArrow.updateValues();
            }

            // Actualizar el mundo físico
            world.Update(gameModel, camaraInterna, modoCamara);

            // Actualizar el HUD
            hud.Update(gameModel, world.player1);
        }

        public void Render()
        {
            // Acción cuando nuestro Player 1 pierde todos sus puntos de vida
            if (world.player1.hitPoints <= 0)
            {
                gameModel.Exit();
                return;
            }

            {
                // Texto en pantalla sobre los comandos disponibles
                //var DrawText = gameModel.DrawText;
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
            }

            // Renderiza todo lo perteneciente al mundo físico
            world.Render(gameModel);

            // Renderizar el HUD
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

        private void ManageInputs(GameModel gameModel)
        {
            // Dibujar el Vector UP
            if (gameModel.Input.keyPressed(Key.F3))
            {
                drawUpVector = !drawUpVector;
            }

            // Girar la cámara unos grados
            if (gameModel.Input.keyPressed(Key.F5))
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

            // Rotar 90° la cámara
            if (gameModel.Input.keyPressed(Key.F6))
            {
                halfsPI = (halfsPI + FastMath.PI_HALF) % FastMath.TWO_PI;
            }

            // Acercar la cámara
            if (gameModel.Input.keyPressed(Key.F8) || gameModel.JoystickButtonPressed(6))
            {
                modoCamara = modosCamara.getNextOption(modoCamara);

                camaraInterna.OffsetHeight = modoCamara.AlturaCamara();
                camaraInterna.OffsetForward = modoCamara.ProfundidadCamara();
            }

            // Mirar hacia atras
            if (gameModel.Input.keyDown(Key.C) || gameModel.JoystickButtonDown(4))
            {
                mirarHaciaAtras = true;
                halfsPI = 0;
            }
            else mirarHaciaAtras = false;
        }
    }
}