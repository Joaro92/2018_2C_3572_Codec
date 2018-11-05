using TGC.Group.Model.Items;
using TGC.Group.Physics;

namespace TGC.Group.World.Characters.ArtificialIntelligence
{
    public class SeekItem : Seek
    {
        private Item item;

        public SeekItem(PhysicsGame nivel, Item item) : base(nivel)
        {
            this.item = item;
        }

        public override void Do(IA ia)
        {
            DoSearch(item.Mesh.Position);
            if (!item.IsPresent)
            {
                ia.currentMode = new SeekPlayer(nivel);
            }
        }
    }
}