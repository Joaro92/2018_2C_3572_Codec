using System.Drawing;
using TGC.Core.Example;
using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Examples.Camara;
using TGC.Group.Bullet.Physics;
using TGC.Group.Bullet_TGC_Object;
using TGC.Group.Nivel1;
using TGC.Group.PlayerOne;
using TGC.Group.Utils;
using Microsoft.DirectX.DirectInput;
using TGC.Core.Direct3D;
using BulletSharp.Math;
using BulletSharp;

namespace TGC.Group.Model
{
    public class GameModel : TgcExample
    {
        private PhysicsGame physicsEngine;
        private Player1 player1;
        private TgcThirdPersonCamera camaraInterna;
        private bool drawUpVector = false;
        public int ScreenHeight, ScreenWidth;
        private TgcArrow directionArrow;
        private Vector3 yawPitchRoll;
        private float flippedTime;
        private float anguloCamara;
        private ModoCamara modoCamara = ModoCamara.NORMAL;

        public GameModel(string mediaDir, string shadersDir) : base(mediaDir, shadersDir)
        {
            Category = Game.Default.Category;
            Name = Game.Default.Name;
            Description = "Idea seleccionada: Twisted Metal - Derby de demolición";
        }

        public override void Init()
        {
            ScreenWidth = D3DDevice.Instance.Device.Viewport.Width;
            ScreenHeight = D3DDevice.Instance.Device.Viewport.Height;

            physicsEngine = new NivelUno();
            player1 = physicsEngine.Init();

            // Configuramos la Cámara en tercera persona para que siga a nuestro Player 1
            camaraInterna = new TgcThirdPersonCamera(new TGCVector3(player1.rigidBody.CenterOfMassPosition), new TGCVector3(0, 2, 0), modoCamara.AlturaCamara() , modoCamara.ProfundidadCamara());
            Camara = camaraInterna;

            // Creamos una flecha que representara el vector UP del auto
            directionArrow = new TgcArrow();
            directionArrow.BodyColor = Color.Red;
            directionArrow.HeadColor = Color.Green;
            directionArrow.Thickness = 0.1f;
            directionArrow.HeadSize = new TGCVector2(1, 2);
        }

        public override void Update()
        {
            PreUpdate();
            player1 = physicsEngine.Update(Input, camaraInterna);

            camaraInterna.Target = new TGCVector3(player1.rigidBody.CenterOfMassPosition);
            
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

            if (Input.keyPressed(Key.V))
            {
                if(modoCamara == ModoCamara.NORMAL)
                {
                    modoCamara = ModoCamara.LEJOS;        
                }
                else if (modoCamara == ModoCamara.LEJOS)
                {
                    modoCamara = ModoCamara.CERCA;
                }
                else
                {
                    modoCamara = ModoCamara.NORMAL;
                }
                camaraInterna.OffsetHeight = modoCamara.AlturaCamara();
                camaraInterna.OffsetForward = modoCamara.ProfundidadCamara();
            }

            if (Input.keyPressed(Key.F3))
            {
                drawUpVector = !drawUpVector;
            }

          

            camaraInterna.RotationY = Quat.ToEulerAngles(player1.rigidBody.Orientation).Y + anguloCamara;

            if (drawUpVector)
            {
                directionArrow.PStart = new TGCVector3(player1.rigidBody.CenterOfMassPosition);
                directionArrow.PEnd = directionArrow.PStart + new TGCVector3(Vector3.TransformNormal(Vector3.UnitY, player1.rigidBody.InterpolationWorldTransform)) * 3.5f;
                directionArrow.updateValues();
            }

            PostUpdate();
        }

        public override void Render()
        {
            PreRender();

            var aux = DrawText.Size;
            DrawText.drawText("Con la tecla F1 se dibuja el bounding box", 3, 20, Color.YellowGreen);
            DrawText.drawText("Con la tecla F2 se rota el ángulo de la cámara", 3, 35, Color.YellowGreen);
            DrawText.drawText("Con la tecla F3 se dibuja el Vector UP del vehículo", 3, 50, Color.YellowGreen);
            DrawText.drawText("Con la tecla V se cambia el modo de cámara (NORMAL, LEJOS, CERCA)", 3, 65, Color.YellowGreen);
            DrawText.drawText("W A S D para el movimiento básico", 3, 80, Color.YellowGreen);
            DrawText.drawText("Control Izquierdo para frenar", 3, 95, Color.YellowGreen);

            string carSpeed = player1.rigidBody.InterpolationLinearVelocity.Length.ToString();
            if (carSpeed.Length > 4)
            {
                DrawText.drawText("Velocidad: " + carSpeed.Substring(0, 4), 15, ScreenHeight - 50, Color.White);
            }
            else
            {
                DrawText.drawText("Velocidad: " + carSpeed, 15, ScreenHeight - 50, Color.White);
            }

            yawPitchRoll = Quat.ToEulerAngles(player1.rigidBody.Orientation);

            if (FastMath.Abs(yawPitchRoll.X) > 1.4f || FastMath.Abs(yawPitchRoll.Z) > 1.4f)
            {
                flippedTime += ElapsedTime;
                DrawText.drawText("Tiempo dado vuelta: " + flippedTime, 15, ScreenHeight - 35, Color.White);

                if (flippedTime > 3)
                {
                    var transformationMatrix = TGCMatrix.RotationYawPitchRoll(FastMath.PI, 0, 0).ToBsMatrix;
                    transformationMatrix.Origin = player1.rigidBody.WorldTransform.Origin + new Vector3(0, 10, 0);

                    player1.rigidBody.MotionState = new DefaultMotionState(transformationMatrix);
                    player1.rigidBody.LinearVelocity = Vector3.Zero;
                    player1.rigidBody.AngularVelocity = Vector3.Zero;
                    flippedTime = 0;
                }
            }
            else
            {
                flippedTime = 0;
            }

            physicsEngine.Render();
            if (drawUpVector)
            {
                directionArrow.Render();
            }

            PostRender();
        }

        public override void Dispose()
        {
            physicsEngine.Dispose();
            directionArrow.Dispose();
            player1.rigidBody.Dispose();
        }
    }
}