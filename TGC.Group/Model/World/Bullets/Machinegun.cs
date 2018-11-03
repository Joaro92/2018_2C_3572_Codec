using BulletSharp;
using BulletSharp.Math;
using Microsoft.DirectX.DirectSound;
using System.Drawing;
using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Core.Sound;
using TGC.Group.Model.World;
using TGC.Group.Model.World.Characters;

namespace TGC.Group.World.Weapons
{
    class MachinegunBullet : Bullet
    {
        public MachinegunBullet(DiscreteDynamicsWorld world, Character origin) : base(5f, origin)
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

        public void fire(int opposite, Device dsDevice)
        {
            rigidBody.WorldTransform = Matrix.Translation(opposite * origin.meshAxisRadius.X * 0.8f, 0.265f, -origin.meshAxisRadius.Z - origin.currentSpeed * 0.01f - 0.47f) * Matrix.RotationY(origin.yawPitchRoll.Y) * Matrix.Translation(origin.Mesh.Transform.Origin.ToBsVector);
            rigidBody.ApplyCentralImpulse(new Vector3(origin.frontVector.X, 0, origin.frontVector.Z) * (25 + (FastMath.Sqrt(FastMath.Abs(origin.currentSpeed)) / 2)));

            sound = new Tgc3dSound(Game.Default.MediaDirectory + Game.Default.FXDirectory + "machinegun.wav", origin.Mesh.Transform.Origin, dsDevice);
            sound.MinDistance = 150f;
            sound.play(false);
        }

        public override void fire(Device dsDevice) { }

        public override void Dispose(Device dsDevice)
        {
            sound = new Tgc3dSound(Game.Default.MediaDirectory + Game.Default.FXDirectory + "machinegunDestroy.wav", mesh.Transform.Origin, dsDevice);
            sound.MinDistance = 47f;
            sound.play(false);
            mesh.Dispose();
            rigidBody.Dispose();
        }

    }
}
