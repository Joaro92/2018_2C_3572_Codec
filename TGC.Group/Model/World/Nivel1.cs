using Microsoft.DirectX.DirectInput;
using System.Drawing;
using TGC.Core.Direct3D;
using TGC.Core.Mathematica;
using TGC.Examples.Camara;
using TGC.Group.Model.Items;
using TGC.Group.Model.Vehicles;
using TGC.Group.Model.World.Weapons;
using TGC.Group.Physics;
using TGC.Group.Utils;
using TGC.Group.World;
using Button = TGC.Group.Model.Input.Button;

namespace TGC.Group.Model.World
{
    public class NivelUno : PhysicsGame
    {
        private readonly TGCVector3 initialPosP1 = new TGCVector3(144f, 7.5f, 0f);
        private readonly TGCVector3 initialPosEnemy = new TGCVector3(-192f, 7.5f, 576f);

        private string posX, posY, posZ;
        private string dir = Game.Default.MediaDirectory + Game.Default.ScenariosDirectory;

        public NivelUno(Vehiculo vehiculoP1, GameModel gameModel)
        {
            // Cargamos el escenario y lo agregamos al mundo
            escenario = new Scenario(world, dir + "scene-level1final-TgcScene.xml");
            
            // Creamos a nuestro jugador y lo agregamos al mundo
            player1 = new Player1(world, vehiculoP1, initialPosP1, 0f, gameModel);

            // Le damos unas armas a nuestro jugador
            player1.AddWeapon(new Power());
            player1.SelectedWeapon.Ammo += 1;

            // Creamos a un enemigo y lo ubicamos en el extremo opuesto del escenario 
            enemy = new Enemy(world, new TGCVector3(144f, 7.5f, 22f), FastMath.PI, gameModel); 

            // Crear SkyBox
            skyBox = Skybox.InitSkybox();

            // Spawneamos algunos obstaculos dinámicos
            objetos.Add(new Colisionable(world, dir + "barrel-TgcScene.xml", new TGCVector3(110f, 10f, 20f)));

            // Spawneamos algunos items
            SpawnItems();

            // Inicializar los shaders en todo el escenario
            ApplyShadersToWorld();
        }

        public override void Update(GameModel gameModel, TgcThirdPersonCamera camaraInterna, ModoCamara modoCamara)
        {
            // Determinar que la simulación del mundo físico se va a procesar 60 veces por segundo
            world.StepSimulation(1 / 60f, 30);

            // Actualizar variables de control
            UpdateControlVariables(gameModel.ElapsedTime);

            // Actualizar variables del jugador que requieren calculos complejos una sola vez
            player1.UpdateInternalValues();
            enemy.UpdateInternalValues();

            // Si el jugador cayó a más de 100 unidades en Y, se lo hace respawnear
            if (player1.RigidBody.CenterOfMassPosition.Y < -100)
            {
                player1.Respawn(inflictDmg, initialPosP1);
            }

            // Intenta enderezar si hace falta
            player1.TryStraighten(gameModel.ElapsedTime);
            enemy.TryStraighten(gameModel.ElapsedTime);

            // Manejar los inputs del teclado y joystick
            player1.ReactToInputs(gameModel);
            // Accion del enemigo
            enemy.TakeAction(this);

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
            enemy.Render();
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
           
        }

        private void ApplyShadersToWorld()
        {
            foreach (var obj in objetos)
            {
                obj.Mesh.Effect = toonFX;
                obj.Mesh.Technique = "ToonShadingWithBorder";
            }

            player1.Mesh.Effect = toonFX;
            player1.Mesh.Technique = "ToonShadingWithBorder";

            enemy.Mesh.Effect = toonFX;
            enemy.Mesh.Technique = "ToonShadingWithBorder";

            foreach (var block in escenario.TgcScene.Meshes)
            {
                if (block.Name.Contains("Arbol") || block.Name.Contains("Palmera") || char.IsLower(block.Name[0]) || block.Name.Equals("Roca") || block.Name.Equals("ParedCastillo") || block.Name.Equals("PilarEgipcio"))
                {
                    block.D3dMesh.ComputeNormals();
                    block.Effect = toonFX;
                    block.Technique = "ToonShading";
                }
            }

            foreach (var item in items)
            {
                item.Mesh.Effect = toonFX;
                item.Mesh.Technique = "ToonShadingWithBorder";
            }
        }
    }
}
