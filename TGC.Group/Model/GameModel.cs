using System.Drawing;
using TGC.Core.Example;
using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Examples.Camara;
using TGC.Group.Bullet.Physics;
using TGC.Group.Bullet_TGC_Object;
using TGC.Group.Nivel1;
using TGC.Group.PlayerOne;
using Microsoft.DirectX.DirectInput;
using TGC.Core.Direct3D;

namespace TGC.Group.Model
{
    public class GameModel : TgcExample
    {
        private PhysicsGame physicsEngine;
        private Player1 player1;
        private TgcThirdPersonCamera camaraInterna;
        private float anguloCamara;
        public int ScreenHeight, ScreenWidth;
        private TgcArrow directionArrow;

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
            camaraInterna = new TgcThirdPersonCamera(new TGCVector3(player1.rigidBody.CenterOfMassPosition), new TGCVector3(0, 2, 0), 0, 20);
            Camara = camaraInterna;

            // Creamos una flecha que representa el vector dirección
            //directionArrow = new TgcArrow();
            //directionArrow.BodyColor = Color.Red;
            //directionArrow.HeadColor = Color.Green;
            //directionArrow.Thickness = 0.1f;
            //directionArrow.HeadSize = new TGCVector2(1, 2);
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
            camaraInterna.RotationY = Quat.ToEulerAngles(player1.rigidBody.Orientation).Y + anguloCamara;

            //directionArrow.PStart = new TGCVector3(player1.rigidBody.CenterOfMassPosition);
            //directionArrow.PEnd = directionArrow.PStart + Quat.rotate_vector_by_quaternion(new TGCVector3(0, 0, -1), player1.rigidBody.Orientation) * 20;
            //directionArrow.updateValues();

            PostUpdate();
        }

        public override void Render()
        {
            PreRender();

            var aux = DrawText.Size;
            DrawText.drawText("Con la tecla F1 se dibuja el bounding box", 3, 20, Color.Yellow);
            DrawText.drawText("Con la tecla F2 se rota el ángulo de la cámara", 3, 35, Color.Yellow);
            DrawText.drawText("W A S D para el movimiento básico", 3, 50, Color.Yellow);

            string carSpeed = player1.rigidBody.InterpolationLinearVelocity.Length.ToString();
            DrawText.drawText("Velocidad: " + carSpeed.Substring(0,4), 15, ScreenHeight - 50, Color.White);

            physicsEngine.Render();
            //directionArrow.Render();
            PostRender();
        }

        public override void Dispose()
        {
            physicsEngine.Dispose();
            //directionArrow.Dispose();
            player1.rigidBody.Dispose();
        }
    }
}