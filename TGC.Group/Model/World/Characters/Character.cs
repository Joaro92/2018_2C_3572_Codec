using BulletSharp;
using BulletSharp.Math;
using System.Collections.Generic;
using System.Globalization;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Sound;
using TGC.Group.Model.Vehicles;
using TGC.Group.Model.World.Weapons;
using TGC.Group.Physics;
using TGC.Group.Utils;
using TGC.Group.World;
using TGC.Group.World.Bullets;
using TGC.Group.World.Weapons;
using static TGC.Group.Utils.WheelContactInfo;

namespace TGC.Group.Model.World.Characters
{
    public abstract class Character
    {
        protected Vehiculo vehiculo;
        protected TgcMesh mesh;
        protected RigidBody rigidBody;
        protected RaycastVehicle vehicle;
        protected TgcMesh wheel;
        protected int worldID;

        // Variables de Control
        public bool jumped = false;
        public Vector3 yawPitchRoll;
        public TGCVector3 frontVector;
        public int currentSpeed;
        public float flippedTime = 0;
        public bool collision = false;
        public float hitPoints;
        public float specialPoints;
        public bool turbo = false;
        public float distanceToExplosion = -1f;
        protected bool canJump = false;
        protected bool onTheFloor = false;
        protected bool falling = false;
        protected int neg = 1;
        public float timerMachineGun;

        // Atributos constantes
        public readonly float maxSpecialPoints = 100f;
        public readonly float costTurbo = 6f; //por segundo
        public readonly float specialPointsGain = 1f; //por segundo
        public readonly float turboMultiplier = 20f;
        public readonly float jumpImpulse = 1800f;
        protected readonly float mass = 200f;
        public readonly float FireFrecuencyMachineGun = 0.25f;
        public readonly float damageByFalling = 30f;

        // Atributos importantes
        public readonly Vector3 meshAxisRadius;
        public readonly float maxHitPoints;
        public readonly float engineForce; // [negativo]
        public readonly float brakeForce;
        public readonly float steeringAngle; //max 0.39 o se va a romper [negativo]
        public readonly float turboImpulse;
        public readonly float frictionSlip; //de menos tracción a más
        public readonly float rollInfluence; //de mas facil de rotar a menos

        protected readonly float rearWheelsHeight;
        protected readonly float frontWheelsHeight;
        protected readonly float suspensionRestLength;
        protected readonly float suspensionStiffness;
        protected readonly float dampingCompression;
        protected readonly float dampingRelaxation;

        protected readonly float meshRealHeight = 0.52f;
        protected readonly float suspensionLength = 0.9f;

        public TgcStaticSound turboSound;

        // Armas
        public List<Weapon> Weapons { get; } = new List<Weapon>();
        public Weapon SelectedWeapon { get; set; } = null;

        public Character(DiscreteDynamicsWorld world, Vehiculo vehiculo, TGCVector3 position, float rotation, GameModel gameModel)
        {
            //Cargar sonido
            turboSound = new TgcStaticSound();
            turboSound.loadSound(Game.Default.MediaDirectory + Game.Default.FXDirectory + "turbo.wav", gameModel.DirectSound.DsDevice);

            this.vehiculo = vehiculo;

            var loader = new TgcSceneLoader();
            this.mesh = loader.loadSceneFromFile(vehiculo.ChassisXmlPath).Meshes[0];
            this.wheel = loader.loadSceneFromFile(vehiculo.WheelsXmlPath).Meshes[0];

            Vehiculo.ChangeTextureColor(this.mesh, vehiculo.Color);

            this.mesh.AutoTransform = false;
            this.wheel.AutoTransform = false;

            maxHitPoints = float.Parse(mesh.UserProperties["maxHitPoints"], CultureInfo.InvariantCulture);
            engineForce = -float.Parse(mesh.UserProperties["engineForce"], CultureInfo.InvariantCulture);
            brakeForce = float.Parse(mesh.UserProperties["brakeForce"], CultureInfo.InvariantCulture);
            steeringAngle = -float.Parse(mesh.UserProperties["steeringAngle"], CultureInfo.InvariantCulture);
            turboImpulse = float.Parse(mesh.UserProperties["turboImpulse"], CultureInfo.InvariantCulture);
            frictionSlip = float.Parse(mesh.UserProperties["frictionSlip"], CultureInfo.InvariantCulture);
            rollInfluence = float.Parse(mesh.UserProperties["rollInfluence"], CultureInfo.InvariantCulture);
            rearWheelsHeight = float.Parse(mesh.UserProperties["rearWheelsHeight"], CultureInfo.InvariantCulture);
            frontWheelsHeight = float.Parse(mesh.UserProperties["frontWheelsHeight"], CultureInfo.InvariantCulture);
            suspensionRestLength = float.Parse(mesh.UserProperties["suspensionRestLength"], CultureInfo.InvariantCulture);
            suspensionStiffness = float.Parse(mesh.UserProperties["suspensionStiffness"], CultureInfo.InvariantCulture);
            dampingCompression = float.Parse(mesh.UserProperties["dampingCompression"], CultureInfo.InvariantCulture);
            dampingRelaxation = float.Parse(mesh.UserProperties["dampingRelaxation"], CultureInfo.InvariantCulture);

            meshAxisRadius = this.mesh.BoundingBox.calculateAxisRadius().ToBsVector;
            var wheelRadius = this.wheel.BoundingBox.calculateAxisRadius().Y;

            //The btBoxShape is centered at the origin
            CollisionShape chassisShape = new BoxShape(meshAxisRadius.X, meshRealHeight, meshAxisRadius.Z);

            //A compound shape is used so we can easily shift the center of gravity of our vehicle to its bottom
            //This is needed to make our vehicle more stable
            CompoundShape compound = new CompoundShape();

            //The center of gravity of the compound shape is the origin. When we add a rigidbody to the compound shape
            //it's center of gravity does not change. This way we can add the chassis rigidbody one unit above our center of gravity
            //keeping it under our chassis, and not in the middle of it
            var localTransform = Matrix.Translation(0, (meshAxisRadius.Y * 1.75f) - (meshRealHeight / 2f), 0);
            compound.AddChildShape(localTransform, chassisShape);
            //Creates a rigid body
            this.rigidBody = CreateChassisRigidBodyFromShape(compound, position, rotation);

            //Adds the vehicle chassis to the world
            world.AddRigidBody(this.rigidBody);
            worldID = world.CollisionObjectArray.IndexOf(this.rigidBody);

            //RaycastVehicle
            DefaultVehicleRaycaster vehicleRayCaster = new DefaultVehicleRaycaster(world);
            VehicleTuning tuning = new VehicleTuning();

            //Creates a new instance of the raycast vehicle
            vehicle = new RaycastVehicle(tuning, this.rigidBody, vehicleRayCaster);

            //Never deactivate the vehicle
            this.rigidBody.ActivationState = ActivationState.DisableDeactivation;

            //Adds the vehicle to the world
            world.AddAction(vehicle);

            //Adds the wheels to the vehicle
            AddWheels(meshAxisRadius, vehicle, tuning, wheelRadius);

            //Inicializo puntos
            hitPoints = maxHitPoints;
            specialPoints = maxSpecialPoints;
            timerMachineGun = 0f;
        }

        // ------- Métodos Públicos -------

        public void AddWeapon(Weapon newWeapon)
        {
            var existingWeapon = Weapons.Find(w => w.Id == newWeapon.Id);
            if (existingWeapon != null)
            {
                existingWeapon.Ammo += newWeapon.Ammo;
            }
            else
            {
                Weapons.Add(newWeapon);
                if (SelectedWeapon == null)
                    SelectedWeapon = newWeapon;
            }
        }

        public void ReassignWeapon()
        {
            if (SelectedWeapon.Ammo == 0)
            {
                var wastedWeapon = SelectedWeapon;
                if (Weapons.Count > 1)
                {
                    var arrayWeapons = Weapons.ToArray();
                    SelectedWeapon = arrayWeapons.getNextOption(wastedWeapon);
                }
                else
                {
                    SelectedWeapon = null;
                }
                Weapons.Remove(wastedWeapon);
                //wastedWeapon.Dispose();
            }
        }

        public void UpdateInternalValues()
        {
            frontVector = new TGCVector3(Vector3.TransformNormal(-Vector3.UnitZ, RigidBody.InterpolationWorldTransform));
            var velocityVector = new TGCVector3(RigidBody.InterpolationLinearVelocity.X, 0, RigidBody.InterpolationLinearVelocity.Z);

            if (velocityVector.Length() < 0.12f)
            {
                velocityVector = TGCVector3.Empty;
            }
            var speedAngle = FastMath.Acos(TGCVector3.Dot(frontVector, velocityVector) / (frontVector.Length() * velocityVector.Length()));
            velocityVector.Multiply(2f);

            currentSpeed = (int)velocityVector.Length();

            if (speedAngle >= FastMath.PI_HALF)
            {
                currentSpeed *= -1;
            }

            yawPitchRoll = Quat.ToEulerAngles(RigidBody.Orientation);
        }

        public void Render()
        {
            // Renderizar la malla del auto, en este caso solo el Chasis
            Mesh.Transform = TGCMatrix.Translation(new TGCVector3(0, meshAxisRadius.Y - (meshRealHeight / 2f), 0)) * new TGCMatrix(rigidBody.InterpolationWorldTransform);
            Mesh.Render();

            TGCMatrix wheelTransform;

            // Como las ruedas no son cuerpos rigidos (aún) se procede a realizar las transformaciones de las ruedas para renderizar
            wheelTransform = TGCMatrix.RotationY(vehicle.GetSteeringValue(0)) * TGCMatrix.RotationTGCQuaternion(new TGCQuaternion(RigidBody.Orientation.X, RigidBody.Orientation.Y, RigidBody.Orientation.Z, RigidBody.Orientation.W)) * TGCMatrix.Translation(new TGCVector3(vehicle.GetWheelInfo(0).WorldTransform.Origin));
            wheel.Transform = wheelTransform;
            wheel.Render();

            wheelTransform = TGCMatrix.RotationY(vehicle.GetSteeringValue(1) + FastMath.PI) * TGCMatrix.RotationTGCQuaternion(new TGCQuaternion(RigidBody.Orientation.X, RigidBody.Orientation.Y, RigidBody.Orientation.Z, RigidBody.Orientation.W)) * TGCMatrix.Translation(new TGCVector3(vehicle.GetWheelInfo(1).WorldTransform.Origin));
            wheel.Transform = wheelTransform;
            wheel.Render();

            wheelTransform = TGCMatrix.RotationY(-vehicle.GetSteeringValue(2)) * TGCMatrix.RotationTGCQuaternion(new TGCQuaternion(RigidBody.Orientation.X, RigidBody.Orientation.Y, RigidBody.Orientation.Z, RigidBody.Orientation.W)) * TGCMatrix.Translation(new TGCVector3(vehicle.GetWheelInfo(2).WorldTransform.Origin));
            wheel.Transform = wheelTransform;
            wheel.Render();

            wheelTransform = TGCMatrix.RotationY(-vehicle.GetSteeringValue(3) + FastMath.PI) * TGCMatrix.RotationTGCQuaternion(new TGCQuaternion(RigidBody.Orientation.X, RigidBody.Orientation.Y, RigidBody.Orientation.Z, RigidBody.Orientation.W)) * TGCMatrix.Translation(new TGCVector3(vehicle.GetWheelInfo(3).WorldTransform.Origin));
            wheel.Transform = wheelTransform;
            wheel.Render();
        }

        public void Dispose()
        {
            mesh.Dispose();
            rigidBody.Dispose();
        }

        public RigidBody RigidBody
        {
            get { return rigidBody; }
            set { rigidBody = value; }
        }

        public TgcMesh Mesh
        {
            get { return mesh; }
            set { mesh = value; }
        }

        public RaycastVehicle Vehicle
        {
            get { return vehicle; }
        }

        public void TryStraighten(float elapsedTime)
        {
            //Si está lo suficientemente rotado en los ejes X o Z no se va a poder mover, por eso lo enderezamos
            if (FastMath.Abs(this.yawPitchRoll.X) > 1.39f || FastMath.Abs(this.yawPitchRoll.Z) > 1.39f)
            {
                this.flippedTime += elapsedTime;
                if (this.flippedTime > 3)
                {
                    this.Straighten();
                }
            }
            else
            {
                this.flippedTime = 0;
            }
        }

        public void Accelerate()
        {
            //Pequeño impulso adicional cuando la velocidad es baja
            var x = currentSpeed;
            float f;
            if (x < 0)
                f = 7;
            else
                f = -FastMath.Log(0.00001f * (x + 0.15f)) - 6.3f;

            vehicle.ApplyEngineForce(engineForce * f, 2);
            vehicle.ApplyEngineForce(engineForce * f, 3);
        }

        public void Reverse()
        {
            vehicle.ApplyEngineForce(engineForce * -0.44f, 2);
            vehicle.ApplyEngineForce(engineForce * -0.44f, 3);
        }

        public void TurnRight()
        {
            vehicle.SetSteeringValue(steeringAngle, 2);
            vehicle.SetSteeringValue(steeringAngle, 3);
        }

        public void TurnLeft()
        {
            vehicle.SetSteeringValue(-steeringAngle, 2);
            vehicle.SetSteeringValue(-steeringAngle, 3);
        }

        public void ResetSteering()
        {
            vehicle.SetSteeringValue(0, 2);
            vehicle.SetSteeringValue(0, 3);
        }

        public void ResetEngineForce()
        {
            vehicle.ApplyEngineForce(0, 2);
            vehicle.ApplyEngineForce(0, 3);
        }

        public void TurboOn()
        {
            turbo = true;
            vehicle.ApplyEngineForce(engineForce * turboMultiplier, 2);
            vehicle.ApplyEngineForce(engineForce * turboMultiplier, 3);
            rigidBody.ApplyCentralImpulse(frontVector.ToBsVector * turboImpulse);
        }

        public void TurboOff()
        {
            turbo = false;
        }

        public void Brake()
        {
            vehicle.SetBrake(brakeForce, 0);
            vehicle.SetBrake(brakeForce, 1);
            vehicle.SetBrake(brakeForce * 0.66f, 2);
            vehicle.SetBrake(brakeForce * 0.66f, 3);
        }

        public void ResetBrake()
        {
            vehicle.SetBrake(1.05f, 0);
            vehicle.SetBrake(1.05f, 1);
            vehicle.SetBrake(1.05f, 2);
            vehicle.SetBrake(1.05f, 3);
        }

        public void Straighten()
        {
            var transformationMatrix = TGCMatrix.RotationYawPitchRoll(FastMath.PI, 0, 0).ToBsMatrix;
            transformationMatrix.Origin = RigidBody.WorldTransform.Origin + new Vector3(0, 10, 0);

            RigidBody.MotionState = new DefaultMotionState(transformationMatrix);
            RigidBody.LinearVelocity = Vector3.Zero;
            RigidBody.AngularVelocity = Vector3.Zero;
            flippedTime = 0;
            canJump = onTheFloor = falling = false;
        }

        public void Respawn(bool inflictDmg, TGCVector3 initialPos, float rotation)
        {
            var transformationMatrix = TGCMatrix.RotationYawPitchRoll(FastMath.PI + rotation, 0, 0).ToBsMatrix;
            transformationMatrix.Origin = initialPos.ToBsVector;

            RigidBody.MotionState = new DefaultMotionState(transformationMatrix);
            RigidBody.LinearVelocity = Vector3.Zero;
            RigidBody.AngularVelocity = Vector3.Zero;

            if (inflictDmg) hitPoints -= damageByFalling;
            canJump = onTheFloor = falling = false;
        }

        public void FireMachinegun(GameModel gameModel, PhysicsGame nivel)
        {
            if (timerMachineGun == 0)
            {
                var b = new MachinegunBullet(nivel.world, this);
                b.fire(neg, gameModel.DirectSound.DsDevice);
                nivel.bullets.Add(b);

                timerMachineGun += gameModel.ElapsedTime;
                neg *= -1;
            }
        }

        public void FireWeapon(GameModel gameModel, PhysicsGame nivel, Weapon SelectedWeapon)
        {
            if (SelectedWeapon != null)
            {
                Bullet b = null;
                switch (SelectedWeapon.Name)
                {
                    case "Power Missile":
                        b = new PowerMissile(nivel.world, this);
                        break;
                }
                b.fire(gameModel.DirectSound.DsDevice);
                SelectedWeapon.Ammo--;
                nivel.bullets.Add(b);
                this.ReassignWeapon();
            }
        }

        public void CalculateImpactDistanceAndReact(Vector3 impactPos)
        {
            distanceToExplosion = (impactPos - rigidBody.CenterOfMassPosition).Length;

            if (distanceToExplosion < 25)
            {
                var forceVector = rigidBody.CenterOfMassPosition - new Vector3(impactPos.X + 0.2f, impactPos.Y - 4, impactPos.Z);
                forceVector.Normalize();
                rigidBody.ApplyImpulse(forceVector * 23, new Vector3(impactPos.X + 0.2f, impactPos.Y - 4, impactPos.Z));
            }

        }

        // ------- Métodos Privados -------

        protected RigidBody CreateChassisRigidBodyFromShape(CollisionShape compound, TGCVector3 position, float rotation)
        {
            //since it is dynamic, we calculate its local inertia
            var localInertia = compound.CalculateLocalInertia(mass);

            var transformationMatrix = TGCMatrix.RotationYawPitchRoll(FastMath.PI + rotation, 0, 0).ToBsMatrix;
            transformationMatrix.Origin = position.ToBsVector;
            DefaultMotionState motionState = new DefaultMotionState(transformationMatrix);
            var bodyInfo = new RigidBodyConstructionInfo(mass, motionState, compound, localInertia);
            var rigidBody = new RigidBody(bodyInfo);

            return rigidBody;
        }

        protected void AddWheels(Vector3 halfExtents, RaycastVehicle vehicle, VehicleTuning tuning, float wheelRadius)
        {
            //The direction of the raycast, the btRaycastVehicle uses raycasts instead of simiulating the wheels with rigid bodies
            Vector3 wheelDirectionCS0 = new Vector3(0, -1, 0);

            //The axis which the wheel rotates arround
            Vector3 wheelAxleCS = new Vector3(-1, 0, 0);

            //All the wheel configuration assumes the vehicle is centered at the origin and a right handed coordinate system is used
            Vector4 points = contactInfoByChassis(mesh.Name);
            points.Y += suspensionLength + meshAxisRadius.Y - (meshRealHeight / 2f);

            //Adds the rear wheels
            Vector3 wheelConnectionPoint = new Vector3(points.X, points.Y - rearWheelsHeight, points.Z);
            vehicle.AddWheel(wheelConnectionPoint, wheelDirectionCS0, wheelAxleCS, suspensionRestLength, wheelRadius, tuning, false);
            vehicle.AddWheel(wheelConnectionPoint * new Vector3(-1, 1, 1), wheelDirectionCS0, wheelAxleCS, suspensionRestLength, wheelRadius, tuning, false);

            //Adds the front wheels
            wheelConnectionPoint = new Vector3(points.X, points.Y - frontWheelsHeight, points.W);
            vehicle.AddWheel(wheelConnectionPoint * new Vector3(1, 1, -1), wheelDirectionCS0, wheelAxleCS, suspensionRestLength, wheelRadius, tuning, true);
            vehicle.AddWheel(wheelConnectionPoint * new Vector3(-1, 1, -1), wheelDirectionCS0, wheelAxleCS, suspensionRestLength, wheelRadius, tuning, true);

            //Configures each wheel of our vehicle, setting its friction, damping compression, etc.
            //For more details on what each parameter does, refer to the docs
            for (int i = 0; i < vehicle.NumWheels; i++)
            {
                WheelInfo wheel = vehicle.GetWheelInfo(i);
                wheel.MaxSuspensionForce = 700000;
                //wheel.MaxSuspensionTravelCm = 80;
                wheel.SuspensionStiffness = suspensionStiffness;
                wheel.WheelsDampingCompression = dampingCompression * 2 * FastMath.Sqrt(wheel.SuspensionStiffness);
                wheel.WheelsDampingRelaxation = dampingRelaxation * 2 * FastMath.Sqrt(wheel.SuspensionStiffness);
                wheel.FrictionSlip = frictionSlip;
                wheel.RollInfluence = rollInfluence;
            }
        }

        protected void CheckJumpStatus(GameModel gameModel)
        {
            if (rigidBody.InterpolationLinearVelocity.Y < -0.88f)
            {
                falling = true;
                onTheFloor = false;
                canJump = false;
            }

            if (falling)
            {
                if (rigidBody.InterpolationLinearVelocity.Y > -0.05f)
                {
                    falling = false;
                    onTheFloor = true;
                    var sound = new Tgc3dSound(Game.Default.MediaDirectory + Game.Default.FXDirectory + "afterJump.wav", mesh.Transform.Origin, gameModel.DirectSound.DsDevice);
                    sound.MinDistance = 150f;
                    sound.play(false);
                }
            }

            if (onTheFloor && !falling)
            {
                canJump = true;
            }
        }

    }
}
