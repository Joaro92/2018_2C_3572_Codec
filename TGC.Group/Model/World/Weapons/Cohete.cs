using BulletSharp;
using BulletSharp.Math;
using TGC.Core.Mathematica;

namespace TGC.Group.Model.World.Weapons
{
    public class Cohete : Weapon
    {
        public Cohete() : base(2, "Power Missile", 5)
        {
            //Otras inicializaciones
        }

        public override void Fire(DiscreteDynamicsWorld world, Player1 player1)
        {
            //Fisica del disparo
            base.Fire(world, player1);

            //var xmlPath = Game.Default.MediaDirectory + Game.Default.ItemsDirectory + "power-missile-TgcScene.xml";
            //CreateRigidBodyFromMesh(world, xmlPath);
            //RigidBody.WorldTransform = Matrix.Translation(0, 0.26f, -player1.meshAxisRadius.Z - player1.currentSpeed * 0.01f - 0.54f) * Matrix.RotationY(player1.yawPitchRoll.Y) * Matrix.Translation(player1.Mesh.Transform.Origin.ToBsVector);
            //RigidBody.ApplyCentralImpulse(new Vector3(player1.frontVector.X, 0, player1.frontVector.Z) * (23 + (FastMath.Sqrt(player1.currentSpeed / 2))));
        }
    }
}
