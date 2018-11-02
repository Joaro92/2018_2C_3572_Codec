using TGC.Core.Mathematica;
using TGC.Group.Model.World.Characters;

namespace TGC.Group.Model.Items
{
    public class Health : Item
    {
        private readonly float healingRate = 0.5f;

        public Health(TGCVector3 pos) : base(pos, "Heart", Game.Default.MediaDirectory + Game.Default.FXDirectory + "healthPickup.wav")
        {
            respawnTime = 10f;
        }

        public override float DesplazamientoY => 0.3f;

        public override void Effect(Character character)
        {
            var hitPointsGained =  healingRate * character.maxHitPoints;
            character.hitPoints = FastMath.Min(character.maxHitPoints, character.hitPoints + hitPointsGained);
        }
    }
}
