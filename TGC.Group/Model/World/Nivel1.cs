using BulletSharp;
using BulletSharp.Math;
using Microsoft.DirectX.DirectInput;
using System.Collections.Generic;
using TGC.Core.Collision;
using TGC.Core.Mathematica;
using TGC.Group.Bullet.Physics;
using TGC.Examples.Camara;
using TGC.Group.Utils;
using TGC.Core.Terrain;
using TGC.Group.Model.World.Weapons;
using TGC.Group.Model.Vehicles;

namespace TGC.Group.Model.World
{
    public class NivelUno : PhysicsGame
    {
        private readonly TGCVector3 initialPos = new TGCVector3(144f, 7.5f, 0f);
        private bool moving = false;
        private bool rotating = false;
        private bool jump = false;
        private bool jumped = false;
        private bool flag = false;
        private float bulletFlag = 0;
        private float neg = 1f;
        private TGCMatrix wheelTransform;
        private TGCVector3 currentCameraPosition;
        private List<MachinegunBullet> mBullets = new List<MachinegunBullet>();

        public NivelUno(Vehiculo vehiculoP1)
        {
            // Cargamos el escenario y lo agregamos al mundo
            escenario = new Escenario("Scenarios\\scene-level1a-TgcScene.xml");
            foreach (RigidBody rigid in escenario.rigidBodys)
            {
                world.AddRigidBody(rigid);
            }

            // Creamos a nuestro jugador y lo agregamos al mundo
            Player1 = new Player1(world, vehiculoP1, initialPos);

            //Crear SkyBox
            skyBox = new TgcSkyBox
            {
                Center = new TGCVector3(0, 600, 0),
                Size = new TGCVector3(13000, 12000, 13000)
            };
            var texturesPath = Game.Default.MediaDirectory + "Images\\";
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Up, texturesPath + "skybox.png");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Down, texturesPath + "skybox.png");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Left, texturesPath + "skybox left.png");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Right, texturesPath + "skybox right.png");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Front, texturesPath + "skybox front.png");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Back, texturesPath + "skybox back.png");
            skyBox.Init();
        }

        public override void Update(GameModel gameModel, TgcThirdPersonCamera camaraInterna, ModoCamara modoCamara)
        {
            // Determinar que la simulación del mundo físico se va a procesar 60 veces por segundo
            world.StepSimulation(1 / 60f, 10);

            // Reiniciar variables de control
            moving = false;
            rotating = false;
            jump = false;

            // Actualizar la velocidad lineal instantanea del vehiculo
            var frontVector = new TGCVector3(Vector3.TransformNormal(-Vector3.UnitZ, Player1.RigidBody.InterpolationWorldTransform));
            var velocityVector = new TGCVector3(Player1.RigidBody.InterpolationLinearVelocity.X, 0, Player1.RigidBody.InterpolationLinearVelocity.Z);

            if (velocityVector.Length() < 0.111f)
            {
                velocityVector = TGCVector3.Empty;
            }
            var speedAngle = FastMath.Acos(TGCVector3.Dot(frontVector, velocityVector) / (frontVector.Length() * velocityVector.Length()));

            velocityVector.Multiply(2.5f);

            Player1.linealVelocity = speedAngle.ToString();
            if (speedAngle > FastMath.PI_HALF)
            {
                Player1.linealVelocity = "-" + ((int)velocityVector.Length()).ToString();
            }
            else
            {
                Player1.linealVelocity = ((int)velocityVector.Length()).ToString();
            }

            // Si el jugador cayó a más de 100 unidades en Y, se lo hace respawnear
            if (Player1.RigidBody.CenterOfMassPosition.Y < -100)
            {
                var transformationMatrix = TGCMatrix.RotationYawPitchRoll(FastMath.PI, 0, 0).ToBsMatrix;
                transformationMatrix.Origin = initialPos.ToBsVector; 

                Player1.RigidBody.MotionState = new DefaultMotionState(transformationMatrix);
                Player1.RigidBody.LinearVelocity = Vector3.Zero;
                Player1.RigidBody.AngularVelocity = Vector3.Zero;

                Player1.hitPoints -= 30;
            }


            // Disparar Machinegun
            if (gameModel.Input.keyDown(Key.E))
            {
                if (bulletFlag == 0)
                {
                    var b = new MachinegunBullet(world);
     
                    b.GhostObject.WorldTransform = Matrix.Translation(neg * Player1.Mesh.BoundingBox.calculateAxisRadius().X * 0.8f, +0.22f, -Player1.Mesh.BoundingBox.calculateAxisRadius().Z - velocityVector.Length() * 0.01f - 0.47f) * Player1.RigidBody.InterpolationWorldTransform;
                    b.GhostObject.ApplyCentralImpulse(frontVector.ToBsVector * 13);
                    mBullets.Add(b);
                    bulletFlag += gameModel.ElapsedTime;
                    neg *= -1f;
                }
            }

            // Detectar según el Input, si va a Rotar, Avanzar y/o Saltar
            // Adelante
            if (gameModel.Input.keyDown(Key.W) || gameModel.Input.keyDown(Key.UpArrow) || gameModel.JoystickButtonDown(0))
            {
                Player1.Vehicle.ApplyEngineForce(Player1.engineForce, 2);
                Player1.Vehicle.ApplyEngineForce(Player1.engineForce, 3);
                moving = true;
            }

            // Atras
            if (gameModel.Input.keyDown(Key.S) || gameModel.Input.keyDown(Key.DownArrow) || gameModel.JoystickButtonDown(3))
            {
                //player1.Vehicle.ApplyEngineForce(-player1.engineForce * 0.1f, 0);
                //player1.Vehicle.ApplyEngineForce(-player1.engineForce * 0.1f, 1);
                Player1.Vehicle.ApplyEngineForce(-Player1.engineForce * 0.33f, 2);
                Player1.Vehicle.ApplyEngineForce(-Player1.engineForce * 0.33f, 3);
                moving = true;
            }

            // Derecha
            if (gameModel.Input.keyDown(Key.D) || gameModel.Input.keyDown(Key.RightArrow) || gameModel.JoystickDpadRight())
            {
                Player1.Vehicle.SetSteeringValue(Player1.steeringAngle, 2);
                Player1.Vehicle.SetSteeringValue(Player1.steeringAngle, 3);
                rotating = true;
            }

            // Izquierda
            if (gameModel.Input.keyDown(Key.A) || gameModel.Input.keyDown(Key.LeftArrow) || gameModel.JoystickDpadLeft())
            {
                Player1.Vehicle.SetSteeringValue(-Player1.steeringAngle, 2);
                Player1.Vehicle.SetSteeringValue(-Player1.steeringAngle, 3);
                rotating = true;
            }

            // Saltar
            if (gameModel.Input.keyDown(Key.Space) || gameModel.JoystickButtonPressed(1))
            {
                jump = true;
            }

            // Si no se presionó ninguna tecla
            if (!rotating)
            {
                Player1.Vehicle.SetSteeringValue(0, 2);
                Player1.Vehicle.SetSteeringValue(0, 3);
            }

            if (!moving)
            {
                Player1.Vehicle.ApplyEngineForce(0, 0);
                Player1.Vehicle.ApplyEngineForce(0, 1);
                Player1.Vehicle.ApplyEngineForce(0, 2);
                Player1.Vehicle.ApplyEngineForce(0, 3);
            }

            // Frenar
            if (gameModel.Input.keyDown(Key.LeftControl) || gameModel.JoystickButtonDown(2))
            {
                Player1.Vehicle.SetBrake(23, 0); //Puede ser una propiedad
                Player1.Vehicle.SetBrake(23, 1);
                Player1.Vehicle.SetBrake(23 * 0.66f, 2); //Puede ser una propiedad
                Player1.Vehicle.SetBrake(23 * 0.66f, 3);
            }
            else
            {
                //Default braking force, always added otherwise there is no friction on the wheels
                if (!moving)
                {
                    Player1.Vehicle.SetBrake(1.05f, 0);
                    Player1.Vehicle.SetBrake(1.05f, 1);
                    Player1.Vehicle.SetBrake(1.05f, 2);
                    Player1.Vehicle.SetBrake(1.05f, 3);
                }
                else
                {
                    Player1.Vehicle.SetBrake(0.05f, 0);
                    Player1.Vehicle.SetBrake(0.05f, 1);
                    Player1.Vehicle.SetBrake(0.05f, 2);
                    Player1.Vehicle.SetBrake(0.05f, 3);
                }
            }

            // Realizar el salto
            if (jump && !jumped && !flag)
            {
                if (Player1.specialPoints > 12)
                {
                    Player1.RigidBody.ApplyCentralImpulse(new Vector3(0, 900*4.3f, 0)); //Puede ser una propiedad
                    Player1.specialPoints -= 12;
                    jumped = true;
                }
            }

            if (jumped && Player1.RigidBody.LinearVelocity.Y < -0.1f)
            {
                flag = true;
                jumped = false;
            }

            if (Player1.RigidBody.LinearVelocity.Y > -0.05f)
            {
                flag = false;
            }

            // Actualizar la inclinación del vehiculo
            Player1.yawPitchRoll = Quat.ToEulerAngles(Player1.RigidBody.Orientation);

            // Si está lo suficientemente rotado en los ejes X o Z no se va a poder mover, por eso lo enderezamos
            if (FastMath.Abs(Player1.yawPitchRoll.X) > 1.39f || FastMath.Abs(Player1.yawPitchRoll.Z) > 1.39f)
            {
                Player1.flippedTime += gameModel.ElapsedTime;

                if (Player1.flippedTime > 3)
                {
                    var transformationMatrix = TGCMatrix.RotationYawPitchRoll(FastMath.PI, 0, 0).ToBsMatrix;
                    transformationMatrix.Origin = Player1.RigidBody.WorldTransform.Origin + new Vector3(0, 10, 0);

                    Player1.RigidBody.MotionState = new DefaultMotionState(transformationMatrix);
                    Player1.RigidBody.LinearVelocity = Vector3.Zero;
                    Player1.RigidBody.AngularVelocity = Vector3.Zero;
                    Player1.flippedTime = 0;
                }
            }
            else
            {
                Player1.flippedTime = 0;
            }

            if (!Player1.collision)
            {
                currentCameraPosition = camaraInterna.Position;
            }

            //Ajustar la posicion de la camara segun la colision con los objetos del escenario
            ajustarPosicionDeCamara(camaraInterna, modoCamara);


            List<int> bulletsID = new List<int>();
            if (world.Broadphase.OverlappingPairCache.OverlappingPairArray.Count > 0)
            {
                foreach (var overlappingPair in world.Broadphase.OverlappingPairCache.OverlappingPairArray)
                {
                    RigidBody obj0 = (RigidBody)overlappingPair.Proxy0.ClientObject;
                    RigidBody obj1 = (RigidBody)overlappingPair.Proxy1.ClientObject;

                    
                    if (obj1.CollisionShape.ShapeType == BroadphaseNativeType.BoxShape)
                    {
                        if (obj0.WorldArrayIndex != Player1.RigidBody.WorldArrayIndex)
                            bulletsID.Add(obj1.WorldArrayIndex);
                    }

                    if (obj0.CollisionShape.ShapeType == BroadphaseNativeType.BoxShape)
                    {
                        if (obj1.WorldArrayIndex != Player1.RigidBody.WorldArrayIndex)
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

            if (bulletFlag > 0) bulletFlag += gameModel.ElapsedTime;
            if (bulletFlag > 0.25f) bulletFlag = 0;
        }

        private void ajustarPosicionDeCamara(TgcThirdPersonCamera camaraInterna, ModoCamara modoCamara)
        {
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
            foreach (var obstaculo in escenario.tgcScene.Meshes)
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


        public override void Render(GameModel gameModel)
        { 
            // Renderizar la malla del auto, en este caso solo el Chasis
            Player1.Mesh.Transform = TGCMatrix.Translation(new TGCVector3(0, 0.11f, 0)) * new TGCMatrix(Player1.Vehicle.ChassisWorldTransform);
            Player1.Mesh.Render();
            
            // Como las ruedas no son cuerpos rigidos (aún) se procede a realizar las transformaciones de las ruedas para renderizar
            wheelTransform = TGCMatrix.RotationY(Player1.Vehicle.GetSteeringValue(0)) * TGCMatrix.RotationTGCQuaternion(new TGCQuaternion(Player1.RigidBody.Orientation.X, Player1.RigidBody.Orientation.Y, Player1.RigidBody.Orientation.Z, Player1.RigidBody.Orientation.W)) * TGCMatrix.Translation(new TGCVector3(Player1.Vehicle.GetWheelInfo(0).WorldTransform.Origin));
            Player1.Wheel.Transform = wheelTransform;
            Player1.Wheel.Render();

            wheelTransform = TGCMatrix.RotationY(Player1.Vehicle.GetSteeringValue(1) + FastMath.PI) * TGCMatrix.RotationTGCQuaternion(new TGCQuaternion(Player1.RigidBody.Orientation.X, Player1.RigidBody.Orientation.Y, Player1.RigidBody.Orientation.Z, Player1.RigidBody.Orientation.W)) * TGCMatrix.Translation(new TGCVector3(Player1.Vehicle.GetWheelInfo(1).WorldTransform.Origin));
            Player1.Wheel.Transform = wheelTransform;
            Player1.Wheel.Render();

            wheelTransform = TGCMatrix.RotationY(-Player1.Vehicle.GetSteeringValue(2)) * TGCMatrix.RotationTGCQuaternion(new TGCQuaternion(Player1.RigidBody.Orientation.X, Player1.RigidBody.Orientation.Y, Player1.RigidBody.Orientation.Z, Player1.RigidBody.Orientation.W)) * TGCMatrix.Translation(new TGCVector3(Player1.Vehicle.GetWheelInfo(2).WorldTransform.Origin));
            Player1.Wheel.Transform = wheelTransform;
            Player1.Wheel.Render();

            wheelTransform = TGCMatrix.RotationY(-Player1.Vehicle.GetSteeringValue(3) + FastMath.PI) *  TGCMatrix.RotationTGCQuaternion(new TGCQuaternion(Player1.RigidBody.Orientation.X, Player1.RigidBody.Orientation.Y, Player1.RigidBody.Orientation.Z, Player1.RigidBody.Orientation.W)) * TGCMatrix.Translation(new TGCVector3(Player1.Vehicle.GetWheelInfo(3).WorldTransform.Origin));
            Player1.Wheel.Transform = wheelTransform;
            Player1.Wheel.Render();

            // Renderizar el escenario
            escenario.Render();

            foreach (var b in mBullets)
            {
                b.TgcBox.Transform = new TGCMatrix(b.GhostObject.WorldTransform);
                b.TgcBox.Render();
                
            }

            //Render SkyBox
            skyBox.Render();
        }

        public override void Dispose()
        {
            world.Dispose();
            dispatcher.Dispose();
            collisionConfiguration.Dispose();
            constraintSolver.Dispose();
            broadphase.Dispose();
            Player1.Mesh.Dispose();
            Player1.RigidBody.Dispose();
            escenario.Dispose();
            mBullets.ForEach(b => b.Dispose());
        }
    }
}
