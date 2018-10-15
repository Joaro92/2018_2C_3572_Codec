using BulletSharp;
using BulletSharp.Math;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Group.Model.Vehicles;
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
        public TGCVector3 frontVector, velocityVector;
        public float flippedTime = 0;
        public string linealVelocity;
        public bool collision = false;
        public float hitPoints;
        public float specialPoints;
        private TGCMatrix wheelTransform;

        // Atributos constantes
        public readonly float maxHitPoints = 100f;
        public readonly float maxSpecialPoints = 100f;

        public readonly float engineForce = -1100;
        public readonly float steeringAngle = -0.27f;
        public readonly float mass = 480f;
        protected float wheelDistance = 0;
        protected float rearWheelsHeight = 0f;
        protected float suspensionRestLength = 2.4f;
        protected float SuspensionStiffness = 21f;
        protected float DampingCompression = 0.18f * 0.7f;
        protected float DampingRelaxation = 0.93f * 0.9f;
        protected float FrictionSlip = 0.66f;
        protected float RollInfluence = 0.7f;

        /// <summary>
        ///  Crea un Vehiculo con propiedades de Bullet y TgcMesh y lo agrega al mundo a partir de un archivo 'TgcScene.xml'
        /// </summary>
        public Player1(DiscreteDynamicsWorld world, Vehiculo vehiculo, TGCVector3 position)
        {
            this.vehiculo = vehiculo;

            var loader = new TgcSceneLoader();
            this.mesh = loader.loadSceneFromFile(Game.Default.MediaDirectory + vehiculo.ChassisXmlPath).Meshes[0];
            this.wheel = loader.loadSceneFromFile(Game.Default.MediaDirectory + vehiculo.WheelsXmlPath).Meshes[0];

            Vehiculo.ChangeTextureColor(this.mesh, vehiculo.Color);

            this.mesh.AutoTransform = false;
            this.Wheel.AutoTransform = false;

            var meshAxisRadius = this.mesh.BoundingBox.calculateAxisRadius().ToBsVector;
            var wheelRadius = this.wheel.BoundingBox.calculateAxisRadius().Y;

            //The btBoxShape is centered at the origin
            CollisionShape chassisShape = new BoxShape(meshAxisRadius);

		    //A compound shape is used so we can easily shift the center of gravity of our vehicle to its bottom
		    //This is needed to make our vehicle more stable
		    CompoundShape compound = new CompoundShape();

            //The center of gravity of the compound shape is the origin. When we add a rigidbody to the compound shape
            //it's center of gravity does not change. This way we can add the chassis rigidbody one unit above our center of gravity
            //keeping it under our chassis, and not in the middle of it
            var localTransform = Matrix.Translation(Vector3.UnitY);
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

            //Reduce even further the Center of Mass for more stability
            this.rigidBody.CenterOfMassTransform = TGCMatrix.Translation(new TGCVector3(0, -(meshAxisRadius.Y * 0.95f) , 0)).ToBsMatrix * this.rigidBody.CenterOfMassTransform;

            //Adds the vehicle to the world
            world.AddAction(vehicle);

		    //Adds the wheels to the vehicle
		    addWheels(meshAxisRadius, vehicle, tuning, wheelRadius);

            //Inicializo puntos
            hitPoints = maxHitPoints;
            specialPoints = maxSpecialPoints;
        }

        private RigidBody createChassisRigidBodyFromShape(CollisionShape chassisShape, TGCVector3 position)
        {
            //since it is dynamic, we calculate its local inertia
            var localInertia = chassisShape.CalculateLocalInertia(mass);

            var transformationMatrix = TGCMatrix.RotationYawPitchRoll(FastMath.PI, 0, 0).ToBsMatrix;
            transformationMatrix.Origin = position.ToBsVector;
            DefaultMotionState motionState = new DefaultMotionState(transformationMatrix);

            var bodyInfo = new RigidBodyConstructionInfo(mass, motionState, chassisShape, localInertia);
            var rigidBody = new RigidBody(bodyInfo);

            return rigidBody;
        }

        private void addWheels(Vector3 halfExtents, RaycastVehicle vehicle, VehicleTuning tuning, float wheelRadius)
        {
            //The direction of the raycast, the btRaycastVehicle uses raycasts instead of simiulating the wheels with rigid bodies
            Vector3 wheelDirectionCS0 = new Vector3(0, -1, 0);

            //The axis which the wheel rotates arround
            Vector3 wheelAxleCS = new Vector3(-1, 0, 0);

            Vector4 points = contactInfoByChassis(mesh.Name);

            //The height where the wheels are connected to the chassis
            //float connectionHeight = -1.148f + 1f - wheelDistance - wheelRadius / 2;
            //connectionHeight += rearWheelsHeight;

            //All the wheel configuration assumes the vehicle is centered at the origin and a right handed coordinate system is used
            Vector3 wheelConnectionPoint = new Vector3(points.X, points.Y + suspensionRestLength - 0.08f, points.Z);

            //Adds the rear wheels
            vehicle.AddWheel(wheelConnectionPoint, wheelDirectionCS0, wheelAxleCS, suspensionRestLength, wheelRadius, tuning, false);
            vehicle.AddWheel(wheelConnectionPoint * new Vector3(-1, 1, 1), wheelDirectionCS0, wheelAxleCS, suspensionRestLength, wheelRadius, tuning, false);

            //Adds the front wheels
            wheelConnectionPoint.Y -= 0.02f;
            wheelConnectionPoint.Z = points.W;
            vehicle.AddWheel(wheelConnectionPoint * new Vector3(1, 1, -1), wheelDirectionCS0, wheelAxleCS, suspensionRestLength, wheelRadius, tuning, true);
            vehicle.AddWheel(wheelConnectionPoint * new Vector3(-1, 1, -1), wheelDirectionCS0, wheelAxleCS, suspensionRestLength, wheelRadius, tuning, true);

            //Configures each wheel of our vehicle, setting its friction, damping compression, etc.
            //For more details on what each parameter does, refer to the docs
            for (int i = 0; i < vehicle.NumWheels; i++)
            {
                WheelInfo wheel = vehicle.GetWheelInfo(i);
                
                wheel.SuspensionStiffness = SuspensionStiffness;
                wheel.WheelsDampingCompression = DampingCompression * 2 * FastMath.Sqrt(wheel.SuspensionStiffness);
                wheel.WheelsDampingRelaxation = DampingRelaxation * 2 * FastMath.Sqrt(wheel.SuspensionStiffness);
                wheel.FrictionSlip = FrictionSlip;
                wheel.RollInfluence = RollInfluence;
            }
        }

        // -----------------------------------------------------

        public void Render()
        {
            // Renderizar la malla del auto, en este caso solo el Chasis
            Mesh.Transform = TGCMatrix.Translation(new TGCVector3(0, 0.11f, 0)) * new TGCMatrix(Vehicle.ChassisWorldTransform);
            Mesh.Render();

            // Como las ruedas no son cuerpos rigidos (aún) se procede a realizar las transformaciones de las ruedas para renderizar
            wheelTransform = TGCMatrix.RotationY(Vehicle.GetSteeringValue(0)) * TGCMatrix.RotationTGCQuaternion(new TGCQuaternion(RigidBody.Orientation.X, RigidBody.Orientation.Y, RigidBody.Orientation.Z, RigidBody.Orientation.W)) * TGCMatrix.Translation(new TGCVector3(Vehicle.GetWheelInfo(0).WorldTransform.Origin));
            Wheel.Transform = wheelTransform;
            Wheel.Render();

            wheelTransform = TGCMatrix.RotationY(Vehicle.GetSteeringValue(1) + FastMath.PI) * TGCMatrix.RotationTGCQuaternion(new TGCQuaternion(RigidBody.Orientation.X, RigidBody.Orientation.Y, RigidBody.Orientation.Z, RigidBody.Orientation.W)) * TGCMatrix.Translation(new TGCVector3(Vehicle.GetWheelInfo(1).WorldTransform.Origin));
            Wheel.Transform = wheelTransform;
            Wheel.Render();

            wheelTransform = TGCMatrix.RotationY(-Vehicle.GetSteeringValue(2)) * TGCMatrix.RotationTGCQuaternion(new TGCQuaternion(RigidBody.Orientation.X, RigidBody.Orientation.Y, RigidBody.Orientation.Z, RigidBody.Orientation.W)) * TGCMatrix.Translation(new TGCVector3(Vehicle.GetWheelInfo(2).WorldTransform.Origin));
            Wheel.Transform = wheelTransform;
            Wheel.Render();

            wheelTransform = TGCMatrix.RotationY(-Vehicle.GetSteeringValue(3) + FastMath.PI) * TGCMatrix.RotationTGCQuaternion(new TGCQuaternion(RigidBody.Orientation.X, RigidBody.Orientation.Y, RigidBody.Orientation.Z, RigidBody.Orientation.W)) * TGCMatrix.Translation(new TGCVector3(Vehicle.GetWheelInfo(3).WorldTransform.Origin));
            Wheel.Transform = wheelTransform;
            Wheel.Render();
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

        public TgcMesh Wheel
        {
            get { return wheel; }
        }

        public int WorldID
        {
            get { return worldID; }
        }
    }
}
