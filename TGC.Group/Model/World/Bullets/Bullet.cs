using BulletSharp;
using Microsoft.DirectX.DirectSound;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Sound;
using TGC.Group.Model.World;

namespace TGC.Group.World
{
    public abstract class Bullet
    {
        protected TgcMesh mesh;
        protected RigidBody rigidBody;
        protected Tgc3dSound sound;
        protected int id;
        protected float lifeTime;
        
        public Bullet() { }

        public abstract void fireFrom(Player1 player1, Device dsDevice);

        public void Render()
        {
            mesh.Transform = new TGCMatrix(rigidBody.InterpolationWorldTransform);
            mesh.Render();
        }

        public void Dispose()
        {
            mesh.Dispose();
            rigidBody.Dispose();
        }

        public abstract void Dispose(Device dsDevice);

        public RigidBody RigidBody
        {
            get { return rigidBody; }
        }

        public TgcMesh Mesh
        {
            get { return mesh; }
        }

        public float LifeTime
        {
            get { return lifeTime; }
            set { lifeTime = value; }
        }
    }
}
