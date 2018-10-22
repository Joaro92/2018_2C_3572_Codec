using BulletSharp;
using BulletSharp.Math;
using Microsoft.DirectX.DirectInput;
using System.Collections.Generic;
using TGC.Core.BoundingVolumes;
using TGC.Core.Collision;
using TGC.Core.Mathematica;
using Button = TGC.Group.Model.Input.Button;
using TGC.Examples.Camara;
using TGC.Group.Model.Items;
using TGC.Group.Model.Vehicles;
using TGC.Group.Physics;
using TGC.Group.Utils;
using TGC.Group.World;
using TGC.Group.World.Bullets;
using TGC.Group.World.Weapons;
using TGC.Group.Model.World.Weapons;

namespace TGC.Group.Model.World
{
    public class NivelUno : PhysicsGame
    {
        private readonly TGCVector3 initialPos = new TGCVector3(144f, 7.5f, 0f);

        public NivelUno(Vehiculo vehiculoP1)
        {
            // Cargamos el escenario y lo agregamos al mundo
            var dir = Game.Default.MediaDirectory + Game.Default.ScenariosDirectory;
            escenario = new Scenario(world, dir + "scene-level1a-TgcScene.xml");
            
            // Creamos a nuestro jugador y lo agregamos al mundo
            player1 = new Player1(world, vehiculoP1, initialPos); // mover a Partida

            // Le damos unas armas a nuestro jugador
            player1.AddWeapon(new Power());
            player1.SelectedWeapon.Ammo += 1;

            // Crear SkyBox
            skyBox = Skybox.InitSkybox();

            // Spawneamos algunos items
            SpawnItems();
        }

        public override void Update(GameModel gameModel, TgcThirdPersonCamera camaraInterna, ModoCamara modoCamara)
        {
            // Determinar que la simulación del mundo físico se va a procesar 60 veces por segundo
            world.StepSimulation(1 / 60f, 10);

            // Actualizar variables de control
            UpdateControlVariables(gameModel);

            // Actualizar variables del jugador que requieren calculos complejos una sola vez
            player1.UpdateInternalValues();

            // Si el jugador cayó a más de 100 unidades en Y, se lo hace respawnear
            if (player1.RigidBody.CenterOfMassPosition.Y < -100)
            {
                player1.Respawn(inflictDmg, initialPos);
            }

            //Si está lo suficientemente rotado en los ejes X o Z no se va a poder mover, por eso lo enderezamos
            if (FastMath.Abs(player1.yawPitchRoll.X) > 1.39f || FastMath.Abs(player1.yawPitchRoll.Z) > 1.39f)
            {
                player1.flippedTime += gameModel.ElapsedTime;
                if (player1.flippedTime > 3)
                {
                    player1.Straighten();
                }
            }
            else
            {
                player1.flippedTime = 0;
            }

            // Manejar los inputs del teclado y joystick
            player1.ReactToInputs(gameModel);

            // Disparar Machinegun
            if (gameModel.Input.keyDown(Key.E) || gameModel.Input.buttonDown(Button.R2))
            {
                FireMachinegun(gameModel);
            }

            // Disparar arma especial
            if (gameModel.Input.keyPressed(Key.R) || gameModel.Input.buttonPressed(Button.L2))
            {
                FireWeapon(gameModel, player1.SelectedWeapon);
            }

            // Metodo que se encarga de manejar las colisiones según corresponda
            CollisionsHandler(gameModel);

            // Ajustar la posicion de la cámara segun la colisión con los objetos del escenario
            AdjustCameraPosition(camaraInterna, modoCamara);
        }

        public override void Render(GameModel gameModel)
        {
            player1.Render();
            escenario.Render();
            skyBox.Render();

            bullets.ForEach(bullet => bullet.Render());
            foreach (Item item in items)
            {
                if (item.IsPresent)
                    item.Mesh.Render();
            }
        }

        // ------- Métodos Privados -------

        private void SpawnItems()
        {
            items.Add(new Health(new TGCVector3(144f, 4f, 24f)));
            items.Add(new Energy(new TGCVector3(168f, 4f, 36f)));
            items.Add(new PowerItem(new TGCVector3(168f, 4f, 48f)));
        }
    }
}
