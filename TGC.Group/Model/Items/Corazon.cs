using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.Mathematica;
using TGC.Group.Model.World;

namespace TGC.Group.Model.Items
{
    public class Corazon : Item
    {
        private readonly float healingRate = 0.5f;

        public Corazon(TGCVector3 pos) : base(pos)
        {
            respawnTime = 10f;
            this.spawn();
        }

        public override void Effect(Player1 player1)
        {
            var hitPointsGained =  healingRate * player1.maxHitPoints;
            player1.hitPoints = FastMath.Min(player1.maxHitPoints, player1.hitPoints + hitPointsGained);
        }

        protected override void spawn()
        {
            Mesh = ItemCreator.SpawnItem("Heart", Position);
            base.spawn();
        }
    }
}
