using TGC.Group.Physics;

namespace TGC.Group.World.Characters.ArtificialIntelligence
{
    internal class SeekHealth : SeekItem
    {

        public SeekHealth(PhysicsGame nivel) : base(nivel, nivel.items[3]) //health item en la base enemiga
        {
        }
    }
}