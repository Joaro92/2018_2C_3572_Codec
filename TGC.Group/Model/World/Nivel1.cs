using BulletSharp;
using BulletSharp.Math;
using Microsoft.DirectX.DirectInput;
using System.Collections.Generic;
using TGC.Core.BoundingVolumes;
using TGC.Core.Collision;
using TGC.Core.Mathematica;
using TGC.Core.Sound;
using TGC.Examples.Camara;
using TGC.Group.Bullet.Physics;
using TGC.Group.Model.Items;
using TGC.Group.Model.Vehicles;
using TGC.Group.Model.World.Weapons;
using TGC.Group.Utils;

namespace TGC.Group.Model.World
{
    public class NivelUno : PhysicsGame
    {
        private readonly TGCVector3 initialPos = new TGCVector3(144f, 7.5f, 0f);
        private bool moving = false;
        private bool rotating = false;
        private bool braking = false;
        private bool jump = false;
        private bool jumped = false;
        private bool flag = false;
        private bool afterJump = true;
        private bool inflictDmg = true;
        private float bulletFlag = 0;
        private float neg = 1f;

        //private TGCVector3 currentCameraPosition;
        private List<MachinegunBullet> mBullets = new List<MachinegunBullet>();
        private List<Item> items = new List<Item>();

        public NivelUno(Vehiculo vehiculoP1)
        {
            // Cargamos el escenario y lo agregamos al mundo
            var dir = Game.Default.MediaDirectory + Game.Default.ScenariosDirectory;
            escenario = new Escenario(world, dir + "scene-level1a-TgcScene.xml");
            
            // Creamos a nuestro jugador y lo agregamos al mundo
            player1 = new Player1(world, vehiculoP1, initialPos);

            // Crear SkyBox
            skyBox = Skybox.InitSkybox();

            // Spawneamos algunos items
            SpawnItems();
        }

        public override void Update(GameModel gameModel, TgcThirdPersonCamera camaraInterna, ModoCamara modoCamara)
        {
            // Determinar que la simulación del mundo físico se va a procesar 60 veces por segundo
            world.StepSimulation(1 / 60f, 10);

            // Reiniciar variables de control
            moving = false;
            rotating = false;
            jump = false;
            braking = false;

            // Actualizar la velocidad lineal instantanea del vehiculo
            UpdatePlayer1LinearVelocity();

            // Si el jugador cayó a más de 100 unidades en Y, se lo hace respawnear
            if (player1.RigidBody.CenterOfMassPosition.Y < -100)
            {
                RespawnPlayer1(inflictDmg);
            }

            // Manejar los inputs del teclado y joystick
            ManageInputs(gameModel);

            // Actualizar la inclinación del vehiculo
            player1.yawPitchRoll = Quat.ToEulerAngles(player1.RigidBody.Orientation);

            // Si está lo suficientemente rotado en los ejes X o Z no se va a poder mover, por eso lo enderezamos
            if (FastMath.Abs(player1.yawPitchRoll.X) > 1.39f || FastMath.Abs(player1.yawPitchRoll.Z) > 1.39f)
            {
                player1.flippedTime += gameModel.ElapsedTime;
                if (player1.flippedTime > 3)
                {
                    StraightenPlayer1();
                }
            }
            else
            {
                player1.flippedTime = 0;
            }

            // Ajustar la posicion de la cámara segun la colisión con los objetos del escenario
            AdjustCameraPosition(camaraInterna, modoCamara);

            // Método que se encarga de la administración completa de cada bala de la Machinegun
            MachinegunHandler(gameModel);

            // Método que se encarga de la administración completa de cada item coleccionable del mundo
            ItemsHandler(gameModel);
        }

        public override void Render(GameModel gameModel)
        {
            // Renderizar el Player 1
            player1.Render();

            // Renderizar el Escenario
            escenario.Render();

            // Renderizar cada bala de Machinegun
            foreach (var b in mBullets)
            {
                b.TgcBox.Transform = new TGCMatrix(b.GhostObject.WorldTransform);
                b.TgcBox.Render();
            }

            // Renderizar Items
            foreach (Item i in items)
            {
                if (i.IsPresent)
                    i.Mesh.Render();
            }

            // Renderizar el SkyBox
            skyBox.Render();
        }

        public override void Dispose()
        {
            world.Dispose();
            dispatcher.Dispose();
            collisionConfiguration.Dispose();
            constraintSolver.Dispose();
            broadphase.Dispose();
            player1.Mesh.Dispose();
            player1.RigidBody.Dispose();
            escenario.Dispose();
            mBullets.ForEach(b => b.Dispose());
            foreach (Item i in items)
            {
                if (i.IsPresent)
                    i.Mesh.Dispose();
            }
        }


        // ------------------------------------------------------

        private void ManageInputs(GameModel gameModel)
        {
            var jh = gameModel.JoystickHandler;

            // Adelante
            if (gameModel.Input.keyDown(Key.W) || gameModel.Input.keyDown(Key.UpArrow) || jh.JoystickButtonDown(0))
            {
                player1.Vehicle.ApplyEngineForce(player1.engineForce, 2);
                player1.Vehicle.ApplyEngineForce(player1.engineForce, 3);
                moving = true;
            }

            // Atras
            if (gameModel.Input.keyDown(Key.S) || gameModel.Input.keyDown(Key.DownArrow) || jh.JoystickButtonDown(3))
            {
                //player1.Vehicle.ApplyEngineForce(-player1.engineForce * 0.1f, 0);
                //player1.Vehicle.ApplyEngineForce(-player1.engineForce * 0.1f, 1);
                player1.Vehicle.ApplyEngineForce(-player1.engineForce * 0.33f, 2);
                player1.Vehicle.ApplyEngineForce(-player1.engineForce * 0.33f, 3);
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

            // Frenar
            if (gameModel.Input.keyDown(Key.LeftControl) || jh.JoystickButtonDown(2))
            {
                player1.Vehicle.SetBrake(23, 0); //Puede ser una propiedad
                player1.Vehicle.SetBrake(23, 1);
                player1.Vehicle.SetBrake(23 * 0.66f, 2); //Puede ser una propiedad
                player1.Vehicle.SetBrake(23 * 0.66f, 3);
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

                    b.GhostObject.WorldTransform = Matrix.Translation(neg * player1.Mesh.BoundingBox.calculateAxisRadius().X * 0.8f, +0.22f, -player1.Mesh.BoundingBox.calculateAxisRadius().Z - player1.velocityVector.Length() * 0.01f - 0.47f) * player1.RigidBody.InterpolationWorldTransform;
                    b.GhostObject.ApplyCentralImpulse(player1.frontVector.ToBsVector * 20);
                    mBullets.Add(b);
                    bulletFlag += gameModel.ElapsedTime;
                    neg *= -1f;

                    var sound = new Tgc3dSound(gameModel.MediaDir + "Sounds\\FX\\machinegun.wav", player1.Mesh.Transform.Origin, gameModel.DirectSound.DsDevice);
                    sound.MinDistance = 150f;
                    sound.play(false);
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
                    player1.RigidBody.ApplyCentralImpulse(new Vector3(0, 900 * 4.3f, 0)); //Puede ser una propiedad
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

        private void UpdatePlayer1LinearVelocity()
        {
            player1.frontVector = new TGCVector3(Vector3.TransformNormal(-Vector3.UnitZ, player1.RigidBody.InterpolationWorldTransform));
            player1.velocityVector = new TGCVector3(player1.RigidBody.InterpolationLinearVelocity.X, 0, player1.RigidBody.InterpolationLinearVelocity.Z);

            if (player1.velocityVector.Length() < 0.111f)
            {
                player1.velocityVector = TGCVector3.Empty;
            }
            var speedAngle = FastMath.Acos(TGCVector3.Dot(player1.frontVector, player1.velocityVector) / (player1.frontVector.Length() * player1.velocityVector.Length()));

            player1.velocityVector.Multiply(2.5f);

            if (speedAngle > FastMath.PI_HALF)
            {
                player1.linealVelocity = "-" + ((int)player1.velocityVector.Length()).ToString();
            }
            else
            {
                player1.linealVelocity = ((int)player1.velocityVector.Length()).ToString();
            }
        }

        private void RespawnPlayer1(bool InflictDmg)
        {
            var transformationMatrix = TGCMatrix.RotationYawPitchRoll(FastMath.PI, 0, 0).ToBsMatrix;
            transformationMatrix.Origin = initialPos.ToBsVector;

            player1.RigidBody.MotionState = new DefaultMotionState(transformationMatrix);
            player1.RigidBody.LinearVelocity = Vector3.Zero;
            player1.RigidBody.AngularVelocity = Vector3.Zero;

            if (inflictDmg) player1.hitPoints -= 30;
        }

        private void StraightenPlayer1()
        {
            var transformationMatrix = TGCMatrix.RotationYawPitchRoll(FastMath.PI, 0, 0).ToBsMatrix;
            transformationMatrix.Origin = player1.RigidBody.WorldTransform.Origin + new Vector3(0, 10, 0);

            player1.RigidBody.MotionState = new DefaultMotionState(transformationMatrix);
            player1.RigidBody.LinearVelocity = Vector3.Zero;
            player1.RigidBody.AngularVelocity = Vector3.Zero;
            player1.flippedTime = 0;
            afterJump = true;
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

        private void MachinegunHandler(GameModel gameModel)
        {
            List<int> bulletsID = new List<int>();
            if (world.Broadphase.OverlappingPairCache.OverlappingPairArray.Count > 0)
            {
                foreach (var overlappingPair in world.Broadphase.OverlappingPairCache.OverlappingPairArray)
                {
                    RigidBody obj0 = (RigidBody)overlappingPair.Proxy0.ClientObject;
                    RigidBody obj1 = (RigidBody)overlappingPair.Proxy1.ClientObject;


                    if (obj1.CollisionShape.ShapeType == BroadphaseNativeType.BoxShape)
                    {
                        if (obj0.WorldArrayIndex != player1.RigidBody.WorldArrayIndex)
                            bulletsID.Add(obj1.WorldArrayIndex);
                    }

                    if (obj0.CollisionShape.ShapeType == BroadphaseNativeType.BoxShape)
                    {
                        if (obj1.WorldArrayIndex != player1.RigidBody.WorldArrayIndex)
                            bulletsID.Add(obj0.WorldArrayIndex);
                    }
                }
            }

            var count = 0;
            if (bulletsID.Count > 0)
            {
                var aux = mBullets.FindAll(b =>
                {
                    if (bulletsID.Contains(b.GhostObject.WorldArrayIndex + count))
                    {
                        world.RemoveRigidBody(b.GhostObject);
                        b.Dispose();
                        count = count + bulletsID.Count + 2;
                        return false;
                    }
                    else return true;
                });

                mBullets = aux;
            }

            foreach(var b in mBullets)
            {
                b.LiveTime += gameModel.ElapsedTime;
            }

            count = 0;
            var aux2 = mBullets.FindAll(b =>
            {
                if (b.LiveTime > 10)
                {
                    world.RemoveRigidBody(b.GhostObject);
                    b.Dispose();
                    count = count + bulletsID.Count + 2;
                    return false;
                }
                else return true;
            });

            mBullets = aux2;

            if (bulletFlag > 0) bulletFlag += gameModel.ElapsedTime;
            if (bulletFlag > 0.25f) bulletFlag = 0;
        }

        private void SpawnItems()
        {
            items.Add(new Corazon(new TGCVector3(144f, 4f, 24f)));
            items.Add(new Energia(new TGCVector3(168f, 4f, 36f)));
        }

        private void ItemsHandler(GameModel gameModel)
        {
            //Obtengo BoundingBox de Player1 para determinar colision con items
            player1.RigidBody.GetAabb(out Vector3 min, out Vector3 max);
            var player1AABB = new TgcBoundingAxisAlignBox(new TGCVector3(min), new TGCVector3(max));

            //Rotar items, desaparecerlos y hacer efecto si colisionan y contar el tiempo que falta para que vuelvan a aparecer los que no estan
            foreach (Item i in items)
            {
                if (i.IsPresent)
                {
                    i.Mesh.RotateY(FastMath.PI_HALF * gameModel.ElapsedTime);

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
