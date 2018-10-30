using BulletSharp;
using BulletSharp.Math;
using System;
using System.Collections.Generic;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Group.Physics;

namespace TGC.Group.Model.World
{
    public class Colisionable
    {
        private TgcMesh mesh;
        private RigidBody rigidBody;

        /// <summary>
        ///  Se crea el escenario a partir del TgcScene y se crean todos los cuerpos rigidos estáticos por cada mesh
        /// </summary>
        public Colisionable(DiscreteDynamicsWorld world, String xmlPath, TGCVector3 position)
        {
            var loader = new TgcSceneLoader();
            this.mesh = loader.loadSceneFromFile(xmlPath).Meshes[0];
            this.mesh.AutoTransform = false;

            this.rigidBody = CreateRigidBodyFromTgcMesh(mesh, position, 5);
            world.AddRigidBody(this.rigidBody);
        }

        public RigidBody RigidBody
        {
            get { return rigidBody; }
            protected set { rigidBody = value; }
        }

        public TgcMesh Mesh
        {
            get { return mesh; }
            protected set { mesh = value; }
        }

        public void Dispose()
        {
            this.mesh.Dispose();
            rigidBody.Dispose();
        }

        private static RigidBody CreateRigidBodyFromTgcMesh(TgcMesh mesh, TGCVector3 position, float mass)
        {
            var vertexCoords = mesh.getVertexPositions();
            TriangleMesh triangleMesh = new TriangleMesh();
            for (int i = 0; i < vertexCoords.Length; i = i + 3)
            {
                triangleMesh.AddTriangle(vertexCoords[i].ToBsVector, vertexCoords[i + 1].ToBsVector, vertexCoords[i + 2].ToBsVector);
            }
            
            var transformationMatrix = TGCMatrix.RotationYawPitchRoll(0, 0, 0).ToBsMatrix;
            transformationMatrix.Origin = position.ToBsVector;
            DefaultMotionState motionState = new DefaultMotionState(transformationMatrix);
            var bulletShape = new ConvexTriangleMeshShape(triangleMesh, true);
            var localInertia = bulletShape.CalculateLocalInertia(mass);

            var bodyInfo = new RigidBodyConstructionInfo(mass, motionState, bulletShape, localInertia);

            var rigidBody = new RigidBody(bodyInfo);
            
            return rigidBody;
        }
    }
}

