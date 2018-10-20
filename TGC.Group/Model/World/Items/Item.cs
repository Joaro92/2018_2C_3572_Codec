using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Group.Model.World;

namespace TGC.Group.Model.Items
{
    public abstract class Item
    {
        public string Name { get; protected set; }
        public TgcMesh Mesh { get; protected set; }
        public TGCVector3 Position { get; }
        protected float respawnTime;
        protected float timer;
        public bool IsPresent { get; protected set; }

        public Item(TGCVector3 pos, string name)
        {
            Name = name;
            Position = pos;
            this.spawn();
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
            Mesh = LoadMesh(Name, Position);
            IsPresent = true;
        }

        public abstract void Effect(Player1 player1);

        public abstract float DesplazamientoY { get; }

        private static TgcMesh LoadMesh(string name, TGCVector3 position)
        {
            var loader = new TgcSceneLoader();
            var mesh = loader.loadSceneFromFile(Game.Default.MediaDirectory + Game.Default.ItemsDirectory + name.ToLower() + "-item-TgcScene.xml").Meshes[0];
            mesh.Position = position;
            return mesh;
        }

        public virtual void Dispose()
        {
            if (IsPresent) Mesh.Dispose();
        }
    }
}
