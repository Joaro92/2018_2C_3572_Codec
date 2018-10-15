using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Group.Model.World;

namespace TGC.Group.Model.Items
{
    public abstract class Item
    {
        //public string Name { get; }
        public TgcMesh Mesh { get; protected set; }
        public TGCVector3 Position { get; }
        protected float respawnTime;
        protected float timer;
        public bool IsPresent { get; protected set; }

        public Item(TGCVector3 pos)
        {
            Position = pos;
        }

        public void Dissapear()
        {
            if (IsPresent)
            {
                IsPresent = false;
                Mesh.Dispose();
                timer = respawnTime;
            }
        } 

        public void UpdateTimer(float elapsedTime)
        {
            timer -= elapsedTime;
            if(timer <= 0)
                this.spawn();
        }

        protected virtual void spawn()
        {
            IsPresent = true;
        }

        public abstract void Effect(Player1 player1);
    }
}
