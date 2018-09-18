using System.Drawing;
using TGC.Core.Example;
using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Examples.Camara;
using TGC.Group.Bullet.Physics;
using TGC.Group.Bullet_TGC_Object;
using TGC.Group.Nivel1;

namespace TGC.Group.Model
{
    public class GameModel : TgcExample
    {
        private PhysicsGame physicsEngine;
        private Bullet_TGC player1;
        private TgcThirdPersonCamera camaraInterna;
        private TgcArrow directionArrow;

        public GameModel(string mediaDir, string shadersDir) : base(mediaDir, shadersDir)
        {
            Category = Game.Default.Category;
            Name = Game.Default.Name;
            Description = "Idea seleccionada: Twisted Metal - Derby de demolición";
        }

        public override void Init()
        {
            physicsEngine = new NivelUno();
            player1 = physicsEngine.Init();

            // Configuramos la Cámara en tercera persona para que siga a nuestro Player 1
            camaraInterna = new TgcThirdPersonCamera(new TGCVector3(player1.rigidBody.CenterOfMassPosition), new TGCVector3(0, 2, 0), 3, 15);
            Camara = camaraInterna;

            // Creamos una flecha que representa 
            directionArrow = new TgcArrow();
            directionArrow.BodyColor = Color.Red;
            directionArrow.HeadColor = Color.Green;
            directionArrow.Thickness = 0.1f;
            directionArrow.HeadSize = new TGCVector2(1, 2);
        }

        public override void Update()
        {
            PreUpdate();
            player1 = physicsEngine.Update(Input);

            camaraInterna.Target = new TGCVector3(player1.rigidBody.CenterOfMassPosition);
            camaraInterna.RotationY = Quat.ToEulerAngles(player1.rigidBody.Orientation).Y;

            directionArrow.PStart = new TGCVector3(player1.rigidBody.CenterOfMassPosition);
            directionArrow.PEnd = directionArrow.PStart + Quat.rotate_vector_by_quaternion(new TGCVector3(0, 0, -1), player1.rigidBody.Orientation) * 20;
            directionArrow.updateValues();
 
            PostUpdate();
        }

        public override void Render()
        {
            PreRender();
            physicsEngine.Render();
            directionArrow.Render();
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