using BulletSharp;
using System.Drawing;
using TGC.Core.Geometry;
using TGC.Core.Mathematica;

namespace TGC.Group.Machinegun
{
    class MachinegunBullet
    {
        private TGCBox tgcBox;
        private RigidBody ghostObject;
        private float liveTime;
        private int worldID;
        
        public MachinegunBullet(DiscreteDynamicsWorld world)
        {
            var boxRadius = new TGCVector3(0.093f, 0.093f, 0.836f);
            tgcBox = TGCBox.fromSize(boxRadius, Color.FromArgb(255, 255, 249, 56));

            var boxShape = new BoxShape(boxRadius.ToBsVector * 0.5f);
            var boxTransform = TGCMatrix.RotationYawPitchRoll(0, 0, 0).ToBsMatrix;
            boxTransform.Origin = new TGCVector3(0, -100, 0).ToBsVector;
            var boxMotionState = new DefaultMotionState(boxTransform);
            var boxLocalInertia = boxShape.CalculateLocalInertia(0.5f);
            var boxInfo = new RigidBodyConstructionInfo(0.5f, boxMotionState, boxShape, boxLocalInertia);
            ghostObject = new RigidBody(boxInfo);
            ghostObject.ForceActivationState(ActivationState.DisableDeactivation);
            ghostObject.Flags = RigidBodyFlags.DisableWorldGravity;
            ghostObject.CollisionFlags = CollisionFlags.NoContactResponse;

            world.AddCollisionObject(ghostObject);
            worldID = world.CollisionObjectArray.IndexOf(ghostObject);
        }

        public void Dispose()
        {
            tgcBox.Dispose();
            ghostObject.Dispose();
        }

        public RigidBody GhostObject
        {
            get { return ghostObject; }
        }

        public TGCBox TgcBox
        {
            get { return tgcBox; }
        }

        public float LiveTime
        {
            get { return liveTime; }
            set { liveTime = value; }
        }

        public int WorldID
        {
            get { return worldID; }
        }
    }
}
