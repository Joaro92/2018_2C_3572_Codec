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
using TGC.Core.Text;
using TGC.Core.Direct3D;

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

        private bool paused = false;
        private TgcText2D pauseMsg;
        private bool endMatch = false;

        private readonly int matchInitialTime = 15; //en minutos
        private float matchTime; //en segundos
        private readonly string levelSong = "Twisted Metal Small Brawl - Now Slaying.mp3";

        public Partida(GameModel gameModel, Vehiculo vehiculoP1)
        {
            this.gameModel = gameModel;

            matchTime = matchInitialTime * 60;

            // Preparamos el mundo físico con todos los elementos que pertenecen a el
            world = new NivelUno(vehiculoP1);

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

            //Leo las dimensiones de la ventana
            var screenHeight = D3DDevice.Instance.Device.Viewport.Height;
            var screenWidth = D3DDevice.Instance.Device.Viewport.Width;

            //Play
            pauseMsg = new TgcText2D
            {
                Text = "PAUSE",
                Color = Color.White,
                Position = new Point(0, screenHeight / 2),
            };
            pauseMsg.changeFont(pauseFont);

        }

        public void Update()
        {
            // Manejar los inputs del teclado y joystick
            ManageInputs(gameModel);


            // Condiciones de fin de partida (ademas de presionar ESC)
            if (world.player1.hitPoints <= 0 || matchTime <= 0)
                endMatch = true;

            // Si pausado o match terminado no computo nada mas
            if (paused || endMatch)
                return;

            // Actualizar la posición y rotación de la cámara para que apunte a nuestro Player 1
            camaraInterna.Target = new TGCVector3(world.player1.RigidBody.CenterOfMassPosition);
            var rightStick = gameModel.JoystickHandler.JoystickRightStick();
            float grades = 0;
            if (FastMath.Abs(rightStick) > 1800)
            {
                grades = ((FastMath.Abs(rightStick) - 1800f) / 81000f) * (FastMath.Abs(rightStick) / rightStick);
            }
            camaraInterna.RotationY = Quat.ToEulerAngles(world.player1.RigidBody.Orientation).Y + anguloCamara + halfsPI + grades + (mirarHaciaAtras ? FastMath.PI : 0);

            //world.player1.RigidBody.CenterOfMassTransform = Matrix.Translation(0,-0.020f,0) * world.player1.RigidBody.CenterOfMassTransform;

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

            // Actualizar el tiempo
            matchTime -= gameModel.ElapsedTime;

            // Actualizar el HUD
            hud.Update(matchTime);

            

        }

        public void Render()
        {
            // Si pausado muestro el mensaje
            if (paused)
            {
                pauseMsg.render();
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

            // Acción cuando se termina la partida 
            if (endMatch)
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
        }

        private void ManageInputs(GameModel gameModel)
        {
            var jh = gameModel.JoystickHandler;
            var sm = gameModel.SoundManager;

            // Si pausado me fijo si quitaron la pausa
            if (paused)
            {
                if (gameModel.Input.keyPressed(Key.Return) || jh.JoystickButtonPressed(7))
                {
                    paused = false;
                    sm.Mp3Player.resume();
                }
                return;
            }

            // Dibujar el Vector UP
            if (gameModel.Input.keyPressed(Key.F1))
            {
                drawUpVector = !drawUpVector;
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

            // Rotar 90° la cámara
            if (gameModel.Input.keyPressed(Key.F3))
            {
                halfsPI = (halfsPI + FastMath.PI_HALF) % FastMath.TWO_PI;
            }

            // Acercar la cámara
            if (gameModel.Input.keyPressed(Key.F4) || jh.JoystickButtonPressed(6))
            {
                modoCamara = modosCamara.getNextOption(modoCamara);

                camaraInterna.OffsetHeight = modoCamara.AlturaCamara();
                camaraInterna.OffsetForward = modoCamara.ProfundidadCamara();
            }

            // Mirar hacia atras
            if (gameModel.Input.keyDown(Key.C) || jh.JoystickButtonDown(4))
            {
                mirarHaciaAtras = true;
                halfsPI = 0;
            }
            else mirarHaciaAtras = false;


            // Cambiar de arma especial
            if (gameModel.Input.keyPressed(Key.Q) || jh.JoystickButtonPressed(5))
            {
                if(world.player1.Weapons.Count != 0)
                {
                    var arrayWeapons = world.player1.Weapons.ToArray();
                    world.player1.SelectedWeapon = arrayWeapons.getNextOption(world.player1.SelectedWeapon);
                }
            }

            if (gameModel.Input.keyPressed(Key.Return) || jh.JoystickButtonPressed(7))
            {
                paused = true;
                sm.Mp3Player.pause();
            }

            if (gameModel.Input.keyPressed(Key.Escape))
            {
                endMatch = true;
            }
        }
    }
}

            