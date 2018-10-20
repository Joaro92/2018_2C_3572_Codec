using BulletSharp;
using BulletSharp.Math;
using Microsoft.DirectX.DirectInput;
using System.Collections.Generic;
using TGC.Core.BoundingVolumes;
using TGC.Core.Collision;
using TGC.Core.Mathematica;
using TGC.Core.Sound;
using TGC.Examples.Camara;
using TGC.Group.Model.Items;
using TGC.Group.Model.Vehicles;
using TGC.Group.Physics;
using TGC.Group.Utils;
using TGC.Group.World;
using TGC.Group.World.Bullets;
using TGC.Group.World.Weapons;

namespace TGC.Group.Model.World
{
    public class NivelUno : PhysicsGame
    {
        private readonly TGCVector3 initialPos = new TGCVector3(144f, 7.5f, 0f);

        public NivelUno(Vehiculo vehiculoP1)
        {
            // Cargamos el escenario y lo agregamos al mundo
            var dir = Game.Default.MediaDirectory + Game.Default.ScenariosDirectory;
            escenario = new Escenario(world, dir + "scene-level1a-TgcScene.xml");
            
            // Creamos a nuestro jugador y lo agregamos al mundo
            player1 = new Player1(world, vehiculoP1, initialPos); // mover a Partida

            // Crear SkyBox
            skyBox = Skybox.InitSkybox();

            // Spawneamos algunos items
            SpawnItems();
        }

        public override void Update(GameModel gameModel, TgcThirdPersonCamera camaraInterna, ModoCamara modoCamara)
        {
            // Determinar que la simulación del mundo físico se va a procesar 60 veces por segundo
            world.StepSimulation(1 / 60f, 10);
            time += gameModel.ElapsedTime;

            // Reiniciar variables de control
            moving = false;
            rotating = false;
            jump = false;
            braking = false;

            // Actualizar variables que requieren calculos complejos una sola vez
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
                    afterJump = true;
                }
            }
            else
            {
                player1.flippedTime = 0;
            }

            // Manejar los inputs del teclado y joystick
            ManageInputs(gameModel);

            // Metodo que se encarga de manejar las colisiones según corresponda
            CollisionsHandler();

            // Actualizar la lista de balas con aquellas que todavía siguen en el mundo después de las colisiones
            bullets = ObtainExistingBullets();

            if (bulletFlag > 0) bulletFlag += gameModel.ElapsedTime;
            if (bulletFlag > 0.25f) bulletFlag = 0;

            // Ajustar la posicion de la cámara segun la colisión con los objetos del escenario
            AdjustCameraPosition(camaraInterna, modoCamara);

            // Método que se encarga de la administración completa de cada item coleccionable del mundo
            ItemsHandler(gameModel);
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

        // ------------------------------------------------------

        private void ManageInputs(GameModel gameModel)
        {
            var jh = gameModel.JoystickHandler;
            float multiplier;

            // Adelante
            if (gameModel.Input.keyDown(Key.W) || gameModel.Input.keyDown(Key.UpArrow) || jh.JoystickButtonDown(0))
            {
                //Pequeño impulso adicional cuando la velocidad es baja
                var multi = 1f;
                if (player1.currentSpeed < 15)
                    multi = 1.8f;

                player1.Vehicle.ApplyEngineForce(player1.engineForce * multi, 2);
                player1.Vehicle.ApplyEngineForce(player1.engineForce * multi, 3);
                moving = true;
            }

            // Atras
            if (gameModel.Input.keyDown(Key.S) || gameModel.Input.keyDown(Key.DownArrow) || jh.JoystickButtonDown(3))
            {
                //player1.Vehicle.ApplyEngineForce(-player1.engineForce * 0.1f, 0);
                //player1.Vehicle.ApplyEngineForce(-player1.engineForce * 0.1f, 1);
                player1.Vehicle.ApplyEngineForce(-player1.engineForce * 0.4f, 2);
                player1.Vehicle.ApplyEngineForce(-player1.engineForce * 0.4f, 3);
                moving = true;
            }

            // Derecha
            if (gameModel.Input.keyDown(Key.D) || gameModel.Input.keyDown(Key.RightArrow) || jh.JoystickDpadRight())
            {
                player1.Vehicle.SetSteeringValue(player1.steeringAngle, 2);
                player1.Vehicle.SetSteeringValue(player1.steeringAngle, 3);
                rotating = true;
            }

            // Izquierda
            if (gameModel.Input.keyDown(Key.A) || gameModel.Input.keyDown(Key.LeftArrow) || jh.JoystickDpadLeft())
            {
                player1.Vehicle.SetSteeringValue(-player1.steeringAngle, 2);
                player1.Vehicle.SetSteeringValue(-player1.steeringAngle, 3);
                rotating = true;
            }

            // Si no se está moviendo
            if (!rotating)
            {
                player1.Vehicle.SetSteeringValue(0, 2);
                player1.Vehicle.SetSteeringValue(0, 3);
            }
            if (!moving)
            {
                player1.Vehicle.ApplyEngineForce(0, 0);
                player1.Vehicle.ApplyEngineForce(0, 1);
                player1.Vehicle.ApplyEngineForce(0, 2);
                player1.Vehicle.ApplyEngineForce(0, 3);
            }

            // Turbo
            if (player1.specialPoints >= player1.costTurbo && (gameModel.Input.keyDown(Key.LeftShift) || jh.JoystickButtonPressedDouble(0, gameModel.ElapsedTime)))
            {
                multiplier = player1.turboMultiplier;
                player1.turbo = true;
                player1.Vehicle.ApplyEngineForce(player1.engineForce * multiplier, 2);
                player1.Vehicle.ApplyEngineForce(player1.engineForce * multiplier, 3);
                player1.RigidBody.ApplyCentralImpulse(player1.frontVector.ToBsVector * 17);
            }
            else
            {
                multiplier = 1f;
                player1.turbo = false;
            }

            // Frenar
            if (gameModel.Input.keyDown(Key.LeftControl) || jh.JoystickButtonDown(2))
            {
                player1.Vehicle.SetBrake(player1.brakeForce, 0);
                player1.Vehicle.SetBrake(player1.brakeForce, 1);
                player1.Vehicle.SetBrake(player1.brakeForce * 0.66f, 2);
                player1.Vehicle.SetBrake(player1.brakeForce * 0.66f, 3);
                braking = true;
            }
            
            // Si no está frenando
            if (!braking)
            {
                //Default braking force, always added otherwise there is no friction on the wheels
                if (!moving)
                {
                    player1.Vehicle.SetBrake(1.05f, 0);
                    player1.Vehicle.SetBrake(1.05f, 1);
                    player1.Vehicle.SetBrake(1.05f, 2);
                    player1.Vehicle.SetBrake(1.05f, 3);
                }
                else
                {
                    player1.Vehicle.SetBrake(0.05f, 0);
                    player1.Vehicle.SetBrake(0.05f, 1);
                    player1.Vehicle.SetBrake(0.05f, 2);
                    player1.Vehicle.SetBrake(0.05f, 3);
                }
            }

            // Disparar Machinegun
            if (gameModel.Input.keyDown(Key.E) || jh.JoystickR2Down())
            {
                if (bulletFlag == 0)
                {
                    var b = new MachinegunBullet(world);
                    b.fireFrom(player1, neg, gameModel.DirectSound.DsDevice);
                    bullets.Add(b);

                    bulletFlag += gameModel.ElapsedTime;
                    neg *= -1;
                }
            }

            // Disparar arma especial
            if (gameModel.Input.keyPressed(Key.R) || jh.JoystickL2Pressed())
            {
                if (player1.SelectedWeapon != null)
                {
                    Bullet b = null;
                    switch (player1.SelectedWeapon.Name)
                    {
                        case "Power Missile":
                            b = new PowerMissile(world);
                            break;
                    }
                    b.fireFrom(player1, gameModel.DirectSound.DsDevice);
                    player1.SelectedWeapon.Ammo--;
                    bullets.Add(b);
                    player1.ReassignWeapon();
                }
            }

            // Saltar
            if (gameModel.Input.keyDown(Key.Space) || jh.JoystickButtonPressed(1))
            {
                jump = true;
            }
            
            // Realizar el salto
            if (jump && !jumped && !flag)
            {
                if (player1.specialPoints > 12)
                {
                    player1.RigidBody.ApplyCentralImpulse(new Vector3(0, player1.jumpImpulse, 0));
                    player1.specialPoints -= 12;
                    afterJump = jumped = true;
                }
            }
            if (jumped && player1.RigidBody.LinearVelocity.Y < -0.1f)
            {
                flag = true;
                jumped = false;
            }
            if (player1.RigidBody.LinearVelocity.Y > -0.05f)
            {
                flag = false;

                if (afterJump && !jumped)
                {
                    var sound = new Tgc3dSound(gameModel.MediaDir + "Sounds\\FX\\afterJump.wav", player1.Mesh.Transform.Origin, gameModel.DirectSound.DsDevice);
                    sound.MinDistance = 150f;
                    sound.play(false);
                    afterJump = false;
                }
            }
        }





        private void AdjustCameraPosition(TgcThirdPersonCamera camaraInterna, ModoCamara modoCamara)
        {
            //if (!Player1.collision)
            //{
            //    currentCameraPosition = camaraInterna.Position;
            //}

            if (camaraInterna.OffsetHeight == 0.1f) return;

            camaraInterna.OffsetHeight = 0.1f;
            camaraInterna.OffsetForward = 30;

            //Pedirle a la camara cual va a ser su proxima posicion
            TGCVector3 position;
            TGCVector3 target;
            camaraInterna.CalculatePositionTarget(out position, out target);

            //Detectar colisiones entre el segmento de recta camara-personaje y todos los objetos del escenario
            TGCVector3 q;
            var minDistSq = FastMath.Pow2(camaraInterna.OffsetForward);
            foreach (var obstaculo in escenario.TgcScene.Meshes)
            {
                //Hay colision del segmento camara-personaje y el objeto
                if (TgcCollisionUtils.intersectSegmentAABB(target, position, obstaculo.BoundingBox, out q))
                {
                    //Si hay colision, guardar la que tenga menor distancia
                    var distSq = TGCVector3.Subtract(q, target).LengthSq();
                    //Hay dos casos singulares, puede que tengamos mas de una colision hay que quedarse con el menor offset.
                    //Si no dividimos la distancia por 2 se acerca mucho al target.
                    minDistSq = FastMath.Min(distSq / 2, minDistSq);
                }
            }

            //Acercar la camara hasta la minima distancia de colision encontrada (pero ponemos un umbral maximo de cercania)
            var newOffsetForward = FastMath.Sqrt(minDistSq);

            if (FastMath.Abs(newOffsetForward) < 10f)
            {
                newOffsetForward = 10f;
            }
            if (newOffsetForward > modoCamara.ProfundidadCamara())
            {
                newOffsetForward = modoCamara.ProfundidadCamara();
            }
            if (modoCamara.AlturaCamara() > 1)
            {
                camaraInterna.OffsetHeight = 1.1f;
            }
            else
            {
                camaraInterna.OffsetHeight = modoCamara.AlturaCamara();
            }

            camaraInterna.OffsetForward = newOffsetForward;

            //Asignar la ViewMatrix haciendo un LookAt desde la posicion final anterior al centro de la camara
            camaraInterna.CalculatePositionTarget(out position, out target);
            camaraInterna.SetCamera(position, target);
        }

        private void CollisionsHandler()
        {
            var overlappedPairs = world.Broadphase.OverlappingPairCache.OverlappingPairArray;
            if (overlappedPairs.Count == 0) return;

            RigidBody obj0, obj1;
            BroadphaseNativeType shapeType;
            List<RigidBody> toRemove = new List<RigidBody>();
            foreach (var pair in overlappedPairs)
            {
                obj0 = (RigidBody)pair.Proxy0.ClientObject;
                obj1 = (RigidBody)pair.Proxy1.ClientObject;

                shapeType = obj0.CollisionShape.ShapeType;
                if (shapeType == BroadphaseNativeType.BoxShape)
                {
                    if (obj1.CollisionShape.ShapeType == BroadphaseNativeType.BoxShape || obj1.Equals(player1.RigidBody)) continue;
                    toRemove.Add(obj0);
                    continue;
                }

                shapeType = obj1.CollisionShape.ShapeType;
                if (shapeType == BroadphaseNativeType.BoxShape)
                {
                    if (obj0.Equals(player1.RigidBody)) continue;
                    toRemove.Add(obj1);
                    continue;
                }
            }

            toRemove.ForEach(rigid => world.RemoveRigidBody(rigid));
        }

        private List<Bullet> ObtainExistingBullets()
        {
            List<Bullet> bullets2 = new List<Bullet>();
            bullets.ForEach(bullet =>
            {
                if (bullet.RigidBody.IsInWorld) bullets2.Add(bullet);
                else bullet.Dispose();
            });

            return bullets2;
        }
        //private void MachinegunHandler(GameModel gameModel)
        //{
        //    List<int> bulletsID = new List<int>();
        //    if (world.Broadphase.OverlappingPairCache.OverlappingPairArray.Count > 0)
        //    {
        //        foreach (var overlappingPair in world.Broadphase.OverlappingPairCache.OverlappingPairArray)
        //        {
        //            RigidBody obj0 = (RigidBody)overlappingPair.Proxy0.ClientObject;
        //            RigidBody obj1 = (RigidBody)overlappingPair.Proxy1.ClientObject;


        //            if (obj1.CollisionShape.ShapeType == BroadphaseNativeType.BoxShape)
        //            {
        //                if (obj0.WorldArrayIndex != player1.RigidBody.WorldArrayIndex)
        //                    bulletsID.Add(obj1.WorldArrayIndex);
        //            }

        //            if (obj0.CollisionShape.ShapeType == BroadphaseNativeType.BoxShape)
        //            {
        //                if (obj1.WorldArrayIndex != player1.RigidBody.WorldArrayIndex)
        //                    bulletsID.Add(obj0.WorldArrayIndex);
        //            }
        //        }
        //    }

        //    var count = 0;
        //    if (bulletsID.Count > 0)
        //    {
        //        var aux = mBullets.FindAll(b =>
        //        {
        //            if (bulletsID.Contains(b.GhostObject.WorldArrayIndex + count))
        //            {
        //                world.RemoveRigidBody(b.GhostObject);
        //                b.Dispose();
        //                count = count + bulletsID.Count + 2;
        //                return false;
        //            }
        //            else return true;
        //        });

        //        mBullets = aux;
        //    }

        //    foreach(var b in mBullets)
        //    {
        //        b.LiveTime += gameModel.ElapsedTime;
        //    }

        //    count = 0;
        //    var aux2 = mBullets.FindAll(b =>
        //    {
        //        if (b.LiveTime > 10)
        //        {
        //            world.RemoveRigidBody(b.GhostObject);
        //            b.Dispose();
        //            count = count + bulletsID.Count + 2;
        //            return false;
        //        }
        //        else return true;
        //    });

        //    mBullets = aux2;

        //    if (bulletFlag > 0) bulletFlag += gameModel.ElapsedTime;
        //    if (bulletFlag > 0.25f) bulletFlag = 0;
        //}

        private void SpawnItems()
        {
            items.Add(new Corazon(new TGCVector3(144f, 4f, 24f)));
            items.Add(new Energia(new TGCVector3(168f, 4f, 36f)));
            //items.Add(new BombaItem(new TGCVector3(120f, 4f, 36f)));
            items.Add(new CoheteItem(new TGCVector3(168f, 4f, 48f)));
            //items.Add(new BombaHieloItem(new TGCVector3(144f, 4f, 48f)));
        }

        private void ItemsHandler(GameModel gameModel)
        {
            //Obtengo BoundingBox de Player1 para determinar colision con items
            player1.RigidBody.GetAabb(out Vector3 min, out Vector3 max);
            min.Y -= player1.meshAxisRadius.Y;
            var player1AABB = new TgcBoundingAxisAlignBox(new TGCVector3(min), new TGCVector3(max));
            
            //Rotar items, desaparecerlos y hacer efecto si colisionan y contar el tiempo que falta para que vuelvan a aparecer los que no estan
            foreach (Item i in items)
            {
                if (i.IsPresent)
                {
                    i.Mesh.RotateY(FastMath.PI_HALF * gameModel.ElapsedTime);
                    i.Mesh.Position = new TGCVector3(i.Mesh.Position.X, i.Mesh.Position.Y + FastMath.Cos(time * 2) * 0.004f, i.Mesh.Position.Z);

                    if (TgcCollisionUtils.testAABBAABB(player1AABB, i.Mesh.BoundingBox))
                    {
                        i.Dissapear();
                        i.Effect(player1);
                    }
                }
                else
                    i.UpdateTimer(gameModel.ElapsedTime);
            }
        }
    }
}
