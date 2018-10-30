using Microsoft.DirectX.DirectInput;
using System.Collections.Generic;
using System.Drawing;
using TGC.Core.Direct3D;
using TGC.Core.Mathematica;
using TGC.Core.Shaders;
using TGC.Examples.Camara;
using TGC.Group.Model.Items;
using TGC.Group.Model.Vehicles;
using TGC.Group.Model.World.Weapons;
using TGC.Group.Physics;
using TGC.Group.Utils;
using Button = TGC.Group.Model.Input.Button;
using Effect = Microsoft.DirectX.Direct3D.Effect;

namespace TGC.Group.Model.World
{
    public class NivelUno : PhysicsGame
    {
        private readonly TGCVector3 initialPos = new TGCVector3(144f, 7.5f, 0f);
        private List<Colisionable> objetos = new List<Colisionable>();

        private Effect toonFX;
        private string posX, posY, posZ;

        public NivelUno(Vehiculo vehiculoP1, GameModel gameModel)
        {
            // Cargamos el escenario y lo agregamos al mundo
            var dir = Game.Default.MediaDirectory + Game.Default.ScenariosDirectory;
            escenario = new Scenario(world, dir + "scene-level1final-TgcScene.xml");
            
            // Creamos a nuestro jugador y lo agregamos al mundo
            player1 = new Player1(world, vehiculoP1, initialPos, gameModel); // mover a Partida

            // Le damos unas armas a nuestro jugador
            player1.AddWeapon(new Power());
            player1.SelectedWeapon.Ammo += 1;

            // Crear SkyBox
            skyBox = Skybox.InitSkybox();

            //Cargar Shader personalizado
            toonFX = TgcShaders.loadEffect(Game.Default.ShadersDirectory + "ToonShading.fx");

            objetos.Add(new Colisionable(world, dir + "barrel-TgcScene.xml", new TGCVector3(144f, 13.5f, 20f)));

            // Spawneamos algunos items
            SpawnItems();

            player1.Mesh.Effect = toonFX;
            player1.Mesh.Technique = "ToonShadingWithBorder";

            foreach (var block in escenario.TgcScene.Meshes)
            {
                if (block.Name.Contains("Arbol") || block.Name.Contains("Palmera") || char.IsLower(block.Name[0]) || block.Name.Equals("Roca") || block.Name.Equals("ParedCastillo") || block.Name.Equals("PilarEgipcio"))
                {
                    block.D3dMesh.ComputeNormals();
                    block.Effect = toonFX;
                    block.Technique = "ToonShading";
                }
            }
        }

        public override void Update(GameModel gameModel, TgcThirdPersonCamera camaraInterna, ModoCamara modoCamara)
        {
            // Determinar que la simulación del mundo físico se va a procesar 60 veces por segundo
            world.StepSimulation(1 / 60f, 30);

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
            // Información Útil
            posX = (player1.RigidBody.WorldTransform.Origin.X.ToString().Length >= 5) ? player1.RigidBody.WorldTransform.Origin.X.ToString().Substring(0, 5) : player1.RigidBody.WorldTransform.Origin.X.ToString();
            posY = (player1.RigidBody.WorldTransform.Origin.Y.ToString().Length >= 5) ? player1.RigidBody.WorldTransform.Origin.Y.ToString().Substring(0, 5) : player1.RigidBody.WorldTransform.Origin.Y.ToString();
            posZ = (player1.RigidBody.WorldTransform.Origin.Z.ToString().Length >= 5) ? player1.RigidBody.WorldTransform.Origin.Z.ToString().Substring(0, 5) : player1.RigidBody.WorldTransform.Origin.Z.ToString();

            gameModel.DrawText.drawText("Resolución: " + D3DDevice.Instance.Device.Viewport.Width + "x" + D3DDevice.Instance.Device.Viewport.Height, 3, 15, Color.Black);
            gameModel.DrawText.drawText("Mouse X: " + gameModel.Input.Xpos() + " + Y: " + gameModel.Input.Ypos(), 3, 30, Color.Black);
            gameModel.DrawText.drawText("Posición P1: X=" + posX + " Y=" + posY + " Z=" + posZ, 3, 45, Color.Black);
            // ----------------

            player1.Render();
            escenario.Render();
            skyBox.Render();

            bullets.ForEach(bullet => bullet.Render());
            foreach (Item item in items)
            {
                if (item.IsPresent)
                    item.Mesh.Render();
            }

            foreach (Colisionable obj in objetos)
            {
                obj.Mesh.Transform = new TGCMatrix(obj.RigidBody.InterpolationWorldTransform);
                obj.Mesh.Render();
            }
        }

        // ------- Métodos Privados -------

        private void SpawnItems()
        {
            //base propia
            items.Add(new Health(new TGCVector3(168f, 4f, 24f)));
            items.Add(new Energy(new TGCVector3(72f, 4f, 24f)));
            items.Add(new PowerItem(new TGCVector3(168f, 4f, 72f)));

            //base enemiga
            items.Add(new Health(new TGCVector3(-216f, 4f, 552f)));
            items.Add(new Energy(new TGCVector3(-120f, 4f, 552f)));
            items.Add(new PowerItem(new TGCVector3(-216f, 4f, 504f)));

            //zonas dificiles
            items.Add(new PowerItem(new TGCVector3(-72f, 4f, 240f)));
            items.Add(new Health(new TGCVector3(216f, 10f, 264f)));
            items.Add(new Energy(new TGCVector3(-120f, 10f, 240f)));
            
            foreach(var item in items)
            {
                item.Mesh.Effect = toonFX;
                item.Mesh.Technique = "ToonShadingWithBorder";
            }
        }
    }
}
