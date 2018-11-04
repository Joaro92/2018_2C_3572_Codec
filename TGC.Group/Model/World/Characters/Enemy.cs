﻿using BulletSharp;
using TGC.Core.Mathematica;
using TGC.Group.Model;
using TGC.Group.Model.Vehicles;
using TGC.Group.Model.World.Characters;
using TGC.Group.Physics;

namespace TGC.Group.World
{
    public class Enemy : Character
    {
        private IA ia;

        public Enemy(DiscreteDynamicsWorld world, TGCVector3 position, float orientation, GameModel gameModel) : base(world, Vehiculo.GetRandom(), position, orientation, gameModel)
        {
            ia = new IA();
        }

        public void TakeAction(GameModel gameModel, PhysicsGame nivel)
        {
            // Chequea y actualiza el status del Salto
            CheckJumpStatus(gameModel);

            ia.TakeAction(this,gameModel, nivel);

        }

    }
}
