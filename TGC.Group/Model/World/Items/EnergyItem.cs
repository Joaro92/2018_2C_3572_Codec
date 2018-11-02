using TGC.Core.Mathematica;
using TGC.Group.Model.World.Characters;

namespace TGC.Group.Model.Items
{
    public class Energy : Item
    {
        private readonly float gainRate = 0.5f;

        public Energy(TGCVector3 pos) : base(pos, "Energy", Game.Default.MediaDirectory + Game.Default.FXDirectory + "specialPickup.wav")
        {
            respawnTime = 5f;
        }

        public override float DesplazamientoY => 0.10f;

        public override void Effect(Character character)
        {
            var specialPointsGained = gainRate * character.maxSpecialPoints;
            character.specialPoints = FastMath.Min(character.maxSpecialPoints, character.specialPoints + specialPointsGained);
        }
    }
}
