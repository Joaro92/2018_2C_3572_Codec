using BulletSharp.Math;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using TGC.Core.Direct3D;
using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Core.Text;
using TGC.Examples.Camara;
using TGC.Group.Model.Interfaces;
using TGC.Group.Model.Vehicles;
using TGC.Group.Model.World;
using TGC.Group.Physics;
using TGC.Group.Utils;
using Button = TGC.Group.Model.Input.Button;
using Key = Microsoft.DirectX.DirectInput.Key;

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

        private bool mirarHaciaAtras = false;
        private float anguloCamara = 0f;
        private float halfsPI = 0f;
        private Func<double, float> sma;

        private bool paused = false;
        private TgcText2D pauseMsg;
        private bool endMatch = false;
        private TgcText2D gameOverMsg;
        private bool gameOverWin = false;
        private bool gameOverLose = false;
        private bool toStartMenu = false;
        //private TgcText2D enemyHP;

        private readonly int matchInitialTime = 15; //en minutos
        private float matchTime; //en segundos
        private readonly string levelSong = "Twisted Metal Small Brawl - Now Slaying.mp3";
        private readonly string youLoseSong = "Game Over.mp3";
        private readonly string youWinSong = "FFVII Victory Fanfare.mp3";

        public Partida(GameModel gameModel, Vehiculo vehiculoP1)
        {
            this.gameModel = gameModel;

            sma = SMA(30);

            matchTime = matchInitialTime * 60;

            // Preparamos el mundo físico con todos los elementos que pertenecen a el
            world = new NivelUno(vehiculoP1, gameModel);

            //Configuramos el player para que sea el Listener
            gameModel.DirectSound.ListenerTracking = world.player1.Mesh;

            // Inicializo el HUD
            hud = new HUD(world.player1, matchTime);

            // Configuramos la Cámara en tercera persona para que siga a nuestro Player 1
            camaraInterna = new TgcThirdPersonCamera(new TGCVector3(world.player1.RigidBody.CenterOfMassPosition), new TGCVector3(0, 2, 0), modoCamara.AlturaCamara(), modoCamara.ProfundidadCamara());
            this.gameModel.Camara = camaraInterna;

            // Creamos una flecha que representara el vector UP del auto
            directionArrow = new TgcArrow
            {
                BodyColor = Color.Red,
                HeadColor = Color.Green,
                Thickness = 0.1f,
                HeadSize = new TGCVector2(0.6f, 1f)
            };

            var sm = gameModel.SoundManager;
            sm.LoadMp3(levelSong);
            sm.Mp3Player.play(true);

            // Font para mensaje de pausa
            var pauseFont = UtilMethods.createFont("Minecraft", 100);
            // Font para mensaje de fin de juego
            var gameOverFont = UtilMethods.createFont("Twisted Stallions", 200);
            // Font para vida enemigo
            //var enemyHPFont = UtilMethods.createFont("Minecraft", 40);

            //Leo las dimensiones de la ventana
            var screenHeight = D3DDevice.Instance.Device.Viewport.Height;
            var screenWidth = D3DDevice.Instance.Device.Viewport.Width;

            //Pause
            pauseMsg = new TgcText2D
            {
                Text = "PAUSE",
                Color = Color.White,
                Position = new Point(0, (int)(screenHeight * 0.46f)),
            };
            pauseMsg.changeFont(pauseFont);

            ////Enemy HP
            //enemyHP = new TgcText2D
            //{
            //    Color = Color.White,
            //};
            //enemyHP.changeFont(enemyHPFont);

            //GameOver
            gameOverMsg = new TgcText2D
            {
                Position = new Point(0, (int)(screenHeight * 0.4f)),
            };
            gameOverMsg.changeFont(gameOverFont);
        }

        public void Update()
        {
            // Update listener
            //gameModel.DirectSound.UpdateListener3d();

            // Manejar los inputs del teclado y joystick
            ManageInputs(gameModel);
            var sm = gameModel.SoundManager;

            // Condiciones de fin de partida (ademas de presionar ESC)
            if (!endMatch)
            {
                if (world.player1.hitPoints <= 0 || matchTime <= 0)
                {
                    endMatch = true;
                    gameOverLose = true;
                    sm.LoadMp3(youLoseSong);
                    sm.Mp3Player.play(false);
                }
                else if (world.enemy.hitPoints <= 0)
                {
                    endMatch = true;
                    gameOverWin = true;
                    sm.LoadMp3(youWinSong);
                    sm.Mp3Player.play(false);
                }
            }

            // Si pausado o match terminado no computo nada mas
            if (paused || endMatch)
                return;

            // Actualizar la posición y rotación de la cámara para que apunte a nuestro Player 1
            camaraInterna.Target = new TGCVector3(world.player1.RigidBody.CenterOfMassPosition);
            var rightStick = gameModel.Input.JoystickRightStick();
            float grades = 0;
            if (FastMath.Abs(rightStick) > 1800)
            {
                grades = ((FastMath.Abs(rightStick) - 1800f) / 81000f) * (FastMath.Abs(rightStick) / rightStick);
            }

            var angular = sma(world.player1.RigidBody.InterpolationAngularVelocity.Y) * 0.055f;

            camaraInterna.RotationY = world.player1.yawPitchRoll.Y + anguloCamara + halfsPI + grades - angular + (mirarHaciaAtras ? FastMath.PI : 0);

            // Actualizar el Vector UP si se dibuja
            if (drawUpVector)
            {
                //directionArrow.PStart = new TGCVector3(world.player1.RigidBody.CenterOfMassPosition);
                //directionArrow.PEnd = directionArrow.PStart + new TGCVector3(Vector3.TransformNormal(Vector3.UnitY, world.player1.RigidBody.InterpolationWorldTransform)) * 3.5f;
                var asd = Matrix.Translation(-world.player1.Mesh.BoundingBox.calculateAxisRadius().X, 0, 0) * world.player1.RigidBody.CenterOfMassTransform;
                directionArrow.PStart = new TGCVector3(asd.Origin);
                directionArrow.PEnd = new TGCVector3(asd.Origin) + TGCVector3.Up * 3.5f;
                directionArrow.updateValues();
            }

            // Actualizar el mundo físico
            world.Update(gameModel, camaraInterna, modoCamara);

            // Actualizar los stats
            var p1 = world.player1;
            if (p1.turbo)
            {
                p1.specialPoints = FastMath.Max(0f, p1.specialPoints - p1.costTurbo * gameModel.ElapsedTime);
            }
            if (p1.specialPoints < p1.maxSpecialPoints)
            {
                p1.specialPoints = FastMath.Min(p1.maxSpecialPoints, p1.specialPoints + p1.specialPointsGain * gameModel.ElapsedTime);
            }
            var e = world.enemy;
            if (e.turbo)
            {
                e.specialPoints = FastMath.Max(0f, e.specialPoints - e.costTurbo * gameModel.ElapsedTime);
            }
            if (e.specialPoints < e.maxSpecialPoints)
            {
                e.specialPoints = FastMath.Min(p1.maxSpecialPoints, e.specialPoints + e.specialPointsGain * gameModel.ElapsedTime);
            }

            // Actualizar el tiempo
            matchTime -= gameModel.ElapsedTime;

            // Actualizar el HUD
            hud.Update(matchTime);
        }

        public void Render()
        {
            // Si derrota muestro el mensaje
            if (gameOverLose)
            {
                gameOverMsg.Text = "YOU LOSE!";
                gameOverMsg.Color = Color.Red;
                gameOverMsg.render();
            }
            // Si victoria muestro el mensaje
            if (gameOverWin)
            {
                gameOverMsg.Text = "YOU WIN!";
                gameOverMsg.Color = Color.Green;
                gameOverMsg.render();
            }
            // Si pausado muestro el mensaje
            if (paused)
            {
                pauseMsg.render();
            }

            // Renderiza todo lo perteneciente al mundo físico
            world.Render(gameModel);

            // Renderizar el HUD
            if (!endMatch)
            {
                hud.Render(gameModel);

               /* enemyHP.Text = world.enemy.hitPoints.ToString();
                var x = world.enemy.Mesh.Transform.Origin.X;
                var y = world.enemy.Mesh.Transform.Origin.Y;
                var z = world.enemy.Mesh.Transform.Origin.Z;
                enemyHP.Position = // usar matriz de proyeccion NO SE COMO ES
                enemyHP.render();*/
            }

            // Renderizar el Vector UP
            if (drawUpVector)
            {
                directionArrow.Render();
            }

            // Acción para volver al menu inicial 
            if (toStartMenu)
            {
                gameModel.GameState = new MenuInicial(gameModel);
                this.Dispose();
            }
        }

        public void Dispose()
        {
            world.Dispose();
            directionArrow.Dispose();
            hud.Dispose();
            pauseMsg.Dispose();
            gameOverMsg.Dispose();
            //enemyHP.Dispose();
        }


        // ----------------------------------

        private static Func<double, float> SMA(int p)
        {
            Queue<double> s = new Queue<double>(p);
            return (x) =>
            {
                if (s.Count >= p)
                {
                    s.Dequeue();
                }
                s.Enqueue(x);
                return (float)s.Average();
            };
        }

        private void ManageInputs(GameModel gameModel)
        {
            var Input = gameModel.Input;
            var sm = gameModel.SoundManager;

            if (endMatch)
            {
                if (Input.keyPressed(Key.Return) || Input.buttonPressed(Button.START))
                {
                    toStartMenu = true;
                }
                return;
            }
            // Si pausado me fijo si quitaron la pausa
            if (paused)
            {
                if (Input.keyPressed(Key.Return) || Input.buttonPressed(Button.START))
                {
                    paused = false;
                    sm.Mp3Player.resume();
                }
                return;
            }

            // Dibujar el Vector UP
            if (Input.keyPressed(Key.F1))
            {
                drawUpVector = !drawUpVector;
            }

            // Girar la cámara unos grados
            if (Input.keyPressed(Key.F2))
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
            if (Input.keyPressed(Key.F3))
            {
                halfsPI = (halfsPI + FastMath.PI_HALF) % FastMath.TWO_PI;
            }

            // Acercar la cámara
            if (Input.keyPressed(Key.F4) || Input.buttonPressed(Button.R3))
            {
                modoCamara = modosCamara.getNextOption(modoCamara);

                camaraInterna.OffsetHeight = modoCamara.AlturaCamara();
                camaraInterna.OffsetForward = modoCamara.ProfundidadCamara();
            }

            // Mirar hacia atras
            if (Input.keyDown(Key.C) || Input.buttonDown(Button.L1))
            {
                mirarHaciaAtras = true;
                halfsPI = 0;
            }
            else mirarHaciaAtras = false;

            if (gameModel.Input.keyPressed(Key.Return) || Input.buttonPressed(Button.START))
            {
                paused = true;
                sm.Mp3Player.pause();
            }

            if (gameModel.Input.keyPressed(Key.Escape))
            {
                endMatch = true;
                toStartMenu = true;
            }
        }
    }
}

