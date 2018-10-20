using BulletSharp;
using BulletSharp.Math;
using System;
using System.Collections.Generic;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Group.Model.Vehicles;
using TGC.Group.Model.World.Weapons;
using TGC.Group.Utils;
using static TGC.Group.Utils.WheelContactInfo;

namespace TGC.Group.Model.World
{
    public class Player1
    {
        private Vehiculo vehiculo;
        private TgcMesh mesh;
        private RigidBody rigidBody;
        private RaycastVehicle vehicle;
        private TgcMesh wheel;
        private int worldID;

        // Variables de Control
        public bool jumped = false;
        public Vector3 yawPitchRoll;
        public TGCVector3 frontVector;
        public int currentSpeed;
        public float flippedTime = 0;
        //public string linealVelocity;
        public bool collision = false;
        public float hitPoints;
        public float specialPoints;
        public bool turbo = false;

        // Atributos constantes
        public readonly float maxSpecialPoints = 100f;
        public readonly float costTurbo = 6f; //por segundo
        public readonly float specialPointsGain = 1f; //por segundo
        public readonly float turboMultiplier = 15f;
        public readonly float jumpImpulse = 1900;
        protected readonly float mass = 200f;

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

        private readonly float meshRealHeight = 0.4f;
        private readonly float suspensionLength = 0.9f;
        
        // Armas
        public List<Weapon> Weapons { get; } = new List<Weapon>();
        public Weapon SelectedWeapon { get; set; } = null;

        public Player1(DiscreteDynamicsWorld world, Vehiculo vehiculo, TGCVector3 position)
        {
            this.vehiculo = vehiculo;

            var loader = new TgcSceneLoader();
            this.mesh = loader.loadSceneFromFile(vehiculo.ChassisXmlPath).Meshes[0];
            this.wheel = loader.loadSceneFromFile(vehiculo.WheelsXmlPath).Meshes[0];

            Vehiculo.ChangeTextureColor(this.mesh, vehiculo.Color);

            this.mesh.AutoTransform = false;
            this.wheel.AutoTransform = false;

            maxHitPoints = float.Parse(mesh.UserProperties["maxHitPoints"]);
            engineForce = -float.Parse(mesh.UserProperties["engineForce"]);
            brakeForce = float.Parse(mesh.UserProperties["brakeForce"]);
            steeringAngle = -float.Parse(mesh.UserProperties["steeringAngle"]);
            turboImpulse = float.Parse(mesh.UserProperties["turboImpulse"]);
            frictionSlip = float.Parse(mesh.UserProperties["frictionSlip"]);
            rollInfluence = float.Parse(mesh.UserProperties["rollInfluence"]);
            rearWheelsHeight = float.Parse(mesh.UserProperties["rearWheelsHeight"]);
            frontWheelsHeight = float.Parse(mesh.UserProperties["frontWheelsHeight"]);
            suspensionRestLength = float.Parse(mesh.UserProperties["suspensionRestLength"]);
            suspensionStiffness = float.Parse(mesh.UserProperties["suspensionStiffness"]);
            dampingCompression = float.Parse(mesh.UserProperties["dampingCompression"]);
            dampingRelaxation = float.Parse(mesh.UserProperties["dampingRelaxation"]);

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
            var localTransform = Matrix.Translation(0, (meshAxisRadius.Y * 2) - (meshRealHeight / 2f), 0);
            compound.AddChildShape(localTransform, chassisShape);
            //Creates a rigid body
            this.rigidBody = createChassisRigidBodyFromShape(compound, position);

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
		    addWheels(meshAxisRadius, vehicle, tuning, wheelRadius);

            //Inicializo puntos
            hitPoints = maxHitPoints;
            specialPoints = maxSpecialPoints;
        }

        private RigidBody createChassisRigidBodyFromShape(CollisionShape compound, TGCVector3 position)
        {
            //since it is dynamic, we calculate its local inertia
            var localInertia = compound.CalculateLocalInertia(mass);

            var transformationMatrix = TGCMatrix.RotationYawPitchRoll(FastMath.PI, 0, 0).ToBsMatrix;
            transformationMatrix.Origin = position.ToBsVector;
            DefaultMotionState motionState = new DefaultMotionState(transformationMatrix);
            var bodyInfo = new RigidBodyConstructionInfo(mass, motionState, compound, localInertia);
            var rigidBody = new RigidBody(bodyInfo);
            
            return rigidBody;
        }

        private void addWheels(Vector3 halfExtents, RaycastVehicle vehicle, VehicleTuning tuning, float wheelRadius)
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

        // ----------------------------------------------------

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
                if(SelectedWeapon == null)
                    SelectedWeapon = newWeapon;
            }
        }

        public void ReassignWeapon()
        {
            if(SelectedWeapon.Ammo == 0)
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
            velocityVector.Multiply(2.5f);

            currentSpeed = (int)velocityVector.Length();

            if (speedAngle >= FastMath.PI_HALF)
            {
                currentSpeed *= -1;
            }

            yawPitchRoll = Quat.ToEulerAngles(RigidBody.Orientation);
        }

        public void Respawn(bool inflictDmg, TGCVector3 initialPos)
        {
            var transformationMatrix = TGCMatrix.RotationYawPitchRoll(FastMath.PI, 0, 0).ToBsMatrix;
            transformationMatrix.Origin = initialPos.ToBsVector;

            RigidBody.MotionState = new DefaultMotionState(transformationMatrix);
            RigidBody.LinearVelocity = Vector3.Zero;
            RigidBody.AngularVelocity = Vector3.Zero;

            if (inflictDmg) hitPoints -= 30;
        }

        public void Straighten()
        {
            var transformationMatrix = TGCMatrix.RotationYawPitchRoll(FastMath.PI, 0, 0).ToBsMatrix;
            transformationMatrix.Origin = RigidBody.WorldTransform.Origin + new Vector3(0, 10, 0);

            RigidBody.MotionState = new DefaultMotionState(transformationMatrix);
            RigidBody.LinearVelocity = Vector3.Zero;
            RigidBody.AngularVelocity = Vector3.Zero;
            flippedTime = 0;
        }

        public void Accelerate()
        {
            //Pequeño impulso adicional cuando la velocidad es baja
            var multi = 1f;
            if (currentSpeed < 15)
                multi = 1.8f;

            vehicle.ApplyEngineForce(engineForce * multi, 2);
            vehicle.ApplyEngineForce(engineForce * multi, 3);
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

        public void Render()
        {
            // Renderizar la malla del auto, en este caso solo el Chasis
            Mesh.Transform = TGCMatrix.Translation(new TGCVector3(0, meshAxisRadius.Y - (meshRealHeight / 2f), 0)) * new TGCMatrix(rigidBody.MotionState.WorldTransform);
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
    }
}
