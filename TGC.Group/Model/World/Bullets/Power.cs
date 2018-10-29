using BulletSharp;
using BulletSharp.Math;
using Microsoft.DirectX.DirectSound;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Sound;
using TGC.Group.Model.World;

namespace TGC.Group.World.Bullets
{
    class PowerMissile : Bullet
    {
        public PowerMissile(DiscreteDynamicsWorld world)
        {
            var loader = new TgcSceneLoader();
            var xmlPath = Game.Default.MediaDirectory + Game.Default.ItemsDirectory + "power-missile-TgcScene.xml";

            mesh = loader.loadSceneFromFile(xmlPath).Meshes[0];
            mesh.AutoTransform = false;

            var meshAxisRadius = mesh.BoundingBox.calculateAxisRadius().ToBsVector;
            var boxShape = new BoxShape(meshAxisRadius);
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

        public override void fireFrom(Player1 player1, Device dsDevice)
        {
            rigidBody.WorldTransform = Matrix.Translation(0, 0.5f, -player1.meshAxisRadius.Z - player1.currentSpeed * 0.01f - 1.8f) * Matrix.RotationY(player1.yawPitchRoll.Y) * Matrix.Translation(player1.Mesh.Transform.Origin.ToBsVector);
            rigidBody.ApplyCentralImpulse(new Vector3(player1.frontVector.X, 0, player1.frontVector.Z) * (30 + (FastMath.Sqrt(FastMath.Abs(player1.currentSpeed)) / 2)));

            sound = new Tgc3dSound(Game.Default.MediaDirectory + "Sounds\\FX\\power.wav", player1.Mesh.Transform.Origin, dsDevice);
            sound.MinDistance = 150f;
            sound.play(false);
        }

        public override void Dispose(Device dsDevice)
        {
            sound.stop();
            sound = new Tgc3dSound(Game.Default.MediaDirectory + "Sounds\\FX\\explosionStrong.wav", mesh.Transform.Origin, dsDevice);
            sound.MinDistance = 80f;
            sound.play(false);
            mesh.Dispose();
            rigidBody.Dispose();
        }
    }
}
