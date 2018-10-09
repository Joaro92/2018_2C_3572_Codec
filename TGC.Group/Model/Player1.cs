using BulletSharp;
using BulletSharp.Math;
using System;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using static TGC.Group.Utils.WheelContactInfo;

namespace TGC.Group.PlayerOne
{
    public class Player1
    {
        private TgcMesh _tgcMesh;
        private RigidBody _rigidBody;
        private RaycastVehicle vehicle;
        private TgcMesh wheel;

        // Variables de Control
        public bool jumped = false;
        public Vector3 yawPitchRoll;
        public float flippedTime = 0;
        public string linealVelocity;
        public bool collision = false;
        public float hitPoints = 100f;
        public float specialPoints = 100f;

        // Atributos constantes
        public readonly float engineForce = -500f;
        public readonly float steeringAngle = -0.25f;
        public readonly float mass = 90f;
        protected float wheelDistance = 0.05f;
        protected float rearWheelsHeight = 0;
        protected float suspensionRestLength = 0.7f;
        protected float SuspensionStiffness = 60;
        protected float DampingCompression = 0.21f;
        protected float DampingRelaxation = 0.57f;
        protected float FrictionSlip = 0.62f;
        protected float RollInfluence = 1.86f;

        /// <summary>
        ///  Crea un Vehiculo con propiedades de Bullet y TgcMesh y lo agrega al mundo a partir de un archivo 'TgcScene.xml'
        /// </summary>
        public Player1(DiscreteDynamicsWorld world, String xmlChassisPath, String xmlWheelPath, TGCVector3 position)
        {
            var loader = new TgcSceneLoader();
            this._tgcMesh = loader.loadSceneFromFile(Game.Default.MediaDirectory + xmlChassisPath).Meshes[0];
            this.wheel = loader.loadSceneFromFile(Game.Default.MediaDirectory + xmlWheelPath).Meshes[0];

            this._tgcMesh.AutoTransform = false;
            this.Wheel.AutoTransform = false;

            var meshAxisRadius = this._tgcMesh.BoundingBox.calculateAxisRadius().ToBsVector;
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
            this._rigidBody = createChassisRigidBodyFromShape(compound, position);

		    //Adds the vehicle chassis to the world
		    world.AddRigidBody(this._rigidBody);

            //RaycastVehicle
            DefaultVehicleRaycaster vehicleRayCaster = new DefaultVehicleRaycaster(world);
            VehicleTuning tuning = new VehicleTuning();

            //Creates a new instance of the raycast vehicle
            vehicle = new RaycastVehicle(tuning, this._rigidBody, vehicleRayCaster);

            //Never deactivate the vehicle
            this._rigidBody.ActivationState = ActivationState.DisableDeactivation;

            //Reduce even further the Center of Mass for more stability
            this._rigidBody.CenterOfMassTransform = TGCMatrix.Translation(new TGCVector3(0, -(meshAxisRadius.Y * 0.95f) , 0)).ToBsMatrix * this._rigidBody.CenterOfMassTransform;

            //Adds the vehicle to the world
            world.AddAction(vehicle);

		    //Adds the wheels to the vehicle
		    addWheels(meshAxisRadius, vehicle, tuning, wheelRadius);
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

            Vector4 points = contactInfoByChassis(tgcMesh.Name);

            //The height where the wheels are connected to the chassis
            float connectionHeight = -1.148f + 1f + wheelDistance - wheelRadius / 2;

            //All the wheel configuration assumes the vehicle is centered at the origin and a right handed coordinate system is used
            Vector3 wheelConnectionPoint = new Vector3(points.X, points.Y + 1f + wheelDistance - wheelRadius / 2, points.Z);

            //Adds the rear wheels
            vehicle.AddWheel(wheelConnectionPoint, wheelDirectionCS0, wheelAxleCS, suspensionRestLength, wheelRadius, tuning, false);
            vehicle.AddWheel(wheelConnectionPoint * new Vector3(-1, 1, 1), wheelDirectionCS0, wheelAxleCS, suspensionRestLength, wheelRadius, tuning, false);

            //Adds the front wheels
            wheelConnectionPoint.Y -= 0.05f - rearWheelsHeight;
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

        public RigidBody rigidBody
        {
            get { return _rigidBody; }
            set { _rigidBody = value; }
        }

        public TgcMesh tgcMesh
        {
            get { return _tgcMesh; }
            set { _tgcMesh = value; }
        }

        public RaycastVehicle Vehicle
        {
            get { return vehicle; }
        }

        public TgcMesh Wheel
        {
            get { return wheel; }
        }
    }
}
