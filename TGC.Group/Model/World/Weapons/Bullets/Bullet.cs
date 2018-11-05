using BulletSharp;
using Microsoft.DirectX.DirectSound;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Sound;
using TGC.Group.Model.World;
using TGC.Group.Model.World.Characters;

namespace TGC.Group.World.Weapons.Bullets
{
    public abstract class Bullet
    {
        protected TgcMesh mesh;
        protected RigidBody rigidBody;
        protected Tgc3dSound sound;
        protected int id;
        protected float lifeTime;
        protected readonly float damage;

        public Character shooter { get; set; }
        
        public Bullet(float damage, Character shooter) {
            this.damage = damage;
            this.shooter = shooter;
        }

        public abstract void fire(Device dsDevice);

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

        public void DealDamage(Character c)
        {
            c.hitPoints -= damage;
        }
    }
}
