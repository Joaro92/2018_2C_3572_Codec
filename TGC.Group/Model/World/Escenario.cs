using BulletSharp;
using System;
using System.Collections.Generic;
using TGC.Core.SceneLoader;
using TGC.Group.Physics;

namespace TGC.Group.Model.World
{
    public class Escenario
    {
        private TgcScene tgcScene;
        private List<RigidBody> rigidBodys;

        /// <summary>
        ///  Se crea el escenario a partir del TgcScene y se crean todos los cuerpos rigidos estáticos por cada mesh
        /// </summary>
        public Escenario(DiscreteDynamicsWorld world, String xmlPath)
        {
            var loader = new TgcSceneLoader();
            this.tgcScene = loader.loadSceneFromFile(xmlPath);

            this.rigidBodys = new List<RigidBody>();
            RigidBody newRigid;

            foreach (TgcMesh mesh in this.tgcScene.Meshes)
            {
                if (!(mesh.Name.Equals("Arbusto") || mesh.Name.Equals("Pasto") || mesh.Name.Equals("Flores")))
                {
                    newRigid = BulletRigidBodyConstructor.CreateRigidBodyFromTgcMesh(mesh);
                    this.rigidBodys.Add(newRigid);
                    world.AddRigidBody(newRigid);
                }
            }
        }

        public List<RigidBody> RigidBodys
        {
            get { return rigidBodys; }
            set { rigidBodys = value; }
        }

        public TgcScene TgcScene
        {
            get { return tgcScene; }
            set { tgcScene = value; }
        }

        public void Render()
        {
            tgcScene.RenderAll();
        }

        public void Dispose()
        {
            this.tgcScene.DisposeAll();
            rigidBodys.ForEach(rigid => rigid.Dispose());
        }

    }
}
