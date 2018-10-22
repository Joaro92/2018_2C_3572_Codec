using BulletSharp;
using BulletSharp.Math;
using Microsoft.DirectX.DirectSound;
using System.Drawing;
using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Core.Sound;
using TGC.Group.Model.World;

namespace TGC.Group.World.Weapons
{
    class MachinegunBullet : Bullet
    {
        public MachinegunBullet(DiscreteDynamicsWorld world)
        {
            var boxRadius = new TGCVector3(0.093f, 0.093f, 0.836f);
            var tgcBox = TGCBox.fromSize(boxRadius, Color.FromArgb(255, 255, 249, 56));
            mesh = tgcBox.ToMesh("MachinegunBullet");
            tgcBox.Dispose();

            var boxShape = new BoxShape(boxRadius.ToBsVector * 0.5f);
            var boxTransform = TGCMatrix.RotationYawPitchRoll(0, 0, 0).ToBsMatrix;
            boxTransform.Origin = new TGCVector3(0, -100, 0).ToBsVector;
            var boxMotionState = new DefaultMotionState(boxTransform);
            var boxLocalInertia = boxShape.CalculateLocalInertia(0.5f);
            var boxInfo = new RigidBodyConstructionInfo(0.5f, boxMotionState, boxShape, boxLocalInertia);
            rigidBody = new RigidBody(boxInfo);
            rigidBody.ForceActivationState(ActivationState.DisableDeactivation);
            rigidBody.Flags = RigidBodyFlags.DisableWorldGravity;
            rigidBody.CollisionFlags = CollisionFlags.NoContactResponse;

            world.AddCollisionObject(rigidBody);
        }

        public void fireFrom(Player1 player1, int opposite, Device dsDevice)
        {
            rigidBody.WorldTransform = Matrix.Translation(opposite * player1.meshAxisRadius.X * 0.8f, 0.27f, -player1.meshAxisRadius.Z - player1.currentSpeed * 0.01f - 0.47f) * Matrix.RotationY(player1.yawPitchRoll.Y) * Matrix.Translation(player1.Mesh.Transform.Origin.ToBsVector);
            rigidBody.ApplyCentralImpulse(new Vector3(player1.frontVector.X, 0, player1.frontVector.Z) * (25 + (FastMath.Sqrt(FastMath.Abs(player1.currentSpeed)) / 2)));

            sound = new Tgc3dSound(Game.Default.MediaDirectory + "Sounds\\FX\\machinegun.wav", player1.Mesh.Transform.Origin, dsDevice);
            sound.MinDistance = 150f;
            sound.play(false);
        }

        public override void fireFrom(Player1 player1, Device dsDevice) { }
        
        public override void Dispose(Device dsDevice)
        {
            sound = new Tgc3dSound(Game.Default.MediaDirectory + "Sounds\\FX\\machinegunDestroy.wav", mesh.Transform.Origin, dsDevice);
            sound.MinDistance = 47f;
            sound.play(false);
            mesh.Dispose();
            rigidBody.Dispose();
        }
    }
}
