using Microsoft.DirectX.DirectSound;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Sound;
using TGC.Group.Model.World.Characters;

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

        protected Tgc3dSound sound { get; set; }
        public readonly string SoundPath;

        public Item(TGCVector3 pos, string name, string soundPath)
        {
            Name = name;
            Position = pos;
            SoundPath = soundPath;
            this.spawn();
        }

        public void Dissapear(Device dsDevice)
        {
            if (IsPresent)
            {
                IsPresent = false;
                Mesh.Dispose();
                timer = respawnTime;
            }
            if (dsDevice != null)
            {
                sound = new Tgc3dSound(SoundPath, Position, dsDevice);
                sound.MinDistance = 150f;
                sound.play(false);
            }
        }

        public void Update(float elapsedTime, float time)
        {
            Mesh.RotateY(FastMath.PI_HALF * elapsedTime);
            Mesh.Position = new TGCVector3(Position.X, Position.Y + FastMath.Sin(time * FastMath.PI_HALF) * DesplazamientoY, Position.Z);
        }

        public void UpdateTimer(float elapsedTime)
        {
            timer -= elapsedTime;
            if (timer <= 0)
                this.spawn();
        }

        protected virtual void spawn()
        {
            Mesh = LoadMesh(Name, Position);
            IsPresent = true;
        }

        public abstract void Effect(Character character);

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
