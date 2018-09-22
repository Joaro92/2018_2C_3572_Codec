using BulletSharp;
using System;
using System.Collections.Generic;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Group.Bullet.Physics;

namespace TGC.Group.TGCEscenario
{
    public class Escenario
    {
        private TgcScene _tgcScene;
        private List<RigidBody> _rigidBodys;

        /// <summary>
        ///  asd
        /// </summary>
        public Escenario(String xmlPath)
        {
            var loader = new TgcSceneLoader();
            this._tgcScene = loader.loadSceneFromFile(Game.Default.MediaDirectory + xmlPath);

            this._rigidBodys = new List<RigidBody>();
            TGCVector3 radio; // = obj.tgcMesh.BoundingBox.calculateAxisRadius();
            TGCVector3 pmin; //= obj.tgcMesh.BoundingBox.PMin;
            RigidBody newRigid;

            foreach (TgcMesh mesh in this._tgcScene.Meshes)
            {
                radio = mesh.BoundingBox.calculateAxisRadius();
                pmin = mesh.BoundingBox.PMin;
                newRigid = BulletRigidBodyConstructor.CreateRigidBodyFromTgcMesh(mesh, radio + pmin);
                this._rigidBodys.Add(newRigid);
            }
        }

        public List<RigidBody> rigidBodys
        {
            get { return _rigidBodys; }
            set { _rigidBodys = value; }
        }

        public TgcScene tgcScene
        {
            get { return _tgcScene; }
            set { _tgcScene = value; }
        }

        public void Dispose()
        {
            this._tgcScene.DisposeAll();
            _rigidBodys.ForEach(rigid => rigid.Dispose());
        }
    }
}
