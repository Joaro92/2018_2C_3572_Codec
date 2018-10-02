using BulletSharp;
using BulletSharp.Math;
using System;
using System.Collections.Generic;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Group.Bullet.Physics;

namespace TGC.Group.PlayerOne
{
    public class Player1
    {
        private TgcMesh _tgcMesh;
        private RigidBody _rigidBody;
        private RaycastVehicle vehicle;
        private TgcMesh wheel;

        /// <summary>
        ///  Crea un Cuerpo Rígido que posee masa y un coeficiente de rozamiento, con propiedades de Bullet y TgcMesh,
        ///  a partir de un archivo 'TgcScene.xml'
        /// </summary>
        public Player1(String xmlPath, TGCVector3 position, float mass, float friction)
        {
            var loader = new TgcSceneLoader();
            this._tgcMesh = loader.loadSceneFromFile(Game.Default.MediaDirectory + xmlPath).Meshes[0];
            this._rigidBody = BulletRigidBodyConstructor.CreateRigidBodyFromTgcMesh(_tgcMesh, position, mass, friction);
        }


        // ---------------------------------------------------


        public Player1(DiscreteDynamicsWorld world, String xmlChassisPath, String xmlWheelPath, TGCVector3 position)
        {
            var loader = new TgcSceneLoader();
            this._tgcMesh = loader.loadSceneFromFile(Game.Default.MediaDirectory + xmlChassisPath).Meshes[0];
            this.wheel = loader.loadSceneFromFile(Game.Default.MediaDirectory + xmlWheelPath).Meshes[0];

            var meshAxisRadius = this._tgcMesh.BoundingBox.calculateAxisRadius().ToBsVector;
            var wheelRadius = this.wheel.BoundingBox.calculateAxisRadius().Y;

            //The btBoxShape is centered at the origin
            CollisionShape chassisShape = new BoxShape(meshAxisRadius);

		    //A compound shape is used so we can easily shift the center of gravity of our vehicle to its bottom
		    //This is needed to make our vehicle more stable
		    CompoundShape compound = new CompoundShape();

            //Matrix localTransform = TGCMatrix.Translation(0, 1.1f, 0).ToBsMatrix;
            var localTransform = Matrix.Translation(new Vector3(0,-0.2f,0));

            //The center of gravity of the compound shape is the origin. When we add a rigidbody to the compound shape
            //it's center of gravity does not change. This way we can add the chassis rigidbody one unit above our center of gravity
            //keeping it under our chassis, and not in the middle of it
            compound.AddChildShape(localTransform, chassisShape);

            //Creates a rigid body
            this._rigidBody = createChassisRigidBodyFromShape(compound, position);

		    //Adds the vehicle chassis to the world
		    world.AddRigidBody(this._rigidBody);

            DefaultVehicleRaycaster vehicleRayCaster = new DefaultVehicleRaycaster(world);

            //RaycastVehicle
            VehicleTuning tuning = new VehicleTuning();

            //Creates a new instance of the raycast vehicle
            vehicle = new RaycastVehicle(tuning, this._rigidBody, vehicleRayCaster);

            //Never deactivate the vehicle
            this._rigidBody.ActivationState = ActivationState.DisableDeactivation;

		    //Adds the vehicle to the world
		    world.AddAction(vehicle);

		    //Adds the wheels to the vehicle
		    addWheels(meshAxisRadius, vehicle, tuning, this.wheel.BoundingBox.calculateAxisRadius());
        }

        private RigidBody createChassisRigidBodyFromShape(CollisionShape chassisShape, TGCVector3 position)
        {
            //chassis mass 
            var mass = 380f;

            //since it is dynamic, we calculate its local inertia
            var localInertia = chassisShape.CalculateLocalInertia(mass);

            var transformationMatrix = TGCMatrix.RotationYawPitchRoll(FastMath.PI, 0, 0).ToBsMatrix;
            transformationMatrix.Origin = position.ToBsVector;
            DefaultMotionState motionState = new DefaultMotionState(transformationMatrix);

            var bodyInfo = new RigidBodyConstructionInfo(mass, motionState, chassisShape, localInertia);
            var rigidBody = new RigidBody(bodyInfo);

            return rigidBody;
        }

        private void addWheels(Vector3 halfExtents, RaycastVehicle vehicle, VehicleTuning tuning, TGCVector3 wheelAxisRadius)
        {
            //The direction of the raycast, the btRaycastVehicle uses raycasts instead of simiulating the wheels with rigid bodies
            Vector3 wheelDirectionCS0 = new Vector3(0, -1, 0);

            //The axis which the wheel rotates arround
            Vector3 wheelAxleCS = new Vector3(-1, 0, 0);

            float suspensionRestLength = 0.7f;

            float wheelWidth = 0.44f;

            float wheelRadius = 0.54f;

            //The height where the wheels are connected to the chassis
            float connectionHeight = -0.974f + 1.1f - wheelRadius;

            //All the wheel configuration assumes the vehicle is centered at the origin and a right handed coordinate system is used
            Vector3 wheelConnectionPoint = new Vector3(1.215f, connectionHeight, 2.294f);

            //Adds the front wheels
            vehicle.AddWheel(wheelConnectionPoint, wheelDirectionCS0, wheelAxleCS, suspensionRestLength, wheelRadius, tuning, true);

            vehicle.AddWheel(wheelConnectionPoint * new Vector3(-1, 1, 1), wheelDirectionCS0, wheelAxleCS, suspensionRestLength, wheelRadius, tuning, true);

       
            wheelConnectionPoint = new Vector3(1.215f, connectionHeight, 2.08f);

            //Adds the rear wheels
            vehicle.AddWheel(wheelConnectionPoint * new Vector3(1, 1, -1), wheelDirectionCS0, wheelAxleCS, suspensionRestLength, wheelRadius, tuning, false);

            vehicle.AddWheel(wheelConnectionPoint * new Vector3(-1, 1, -1), wheelDirectionCS0, wheelAxleCS, suspensionRestLength, wheelRadius, tuning, false);

            //Configures each wheel of our vehicle, setting its friction, damping compression, etc.
            //For more details on what each parameter does, refer to the docs
            for (int i = 0; i < vehicle.NumWheels; i++)
            {
                WheelInfo wheel = vehicle.GetWheelInfo(i);
                
                wheel.SuspensionStiffness = 40;
                wheel.WheelsDampingCompression = 0.3f * 2 * FastMath.Sqrt(wheel.SuspensionStiffness);//btScalar(0.8);
                wheel.WheelsDampingRelaxation = 0.5f * 2 * FastMath.Sqrt(wheel.SuspensionStiffness);//1;
                                                                                                          //Larger friction slips will result in better handling
                wheel.FrictionSlip = 0.6f;
                wheel.RollInfluence = 1.5f;
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
