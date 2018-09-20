using BulletSharp;
using System;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Group.Bullet.Physics;

namespace TGC.Group.PlayerOne
{
    public class Player1
    {
        private TgcMesh _tgcMesh;
        private RigidBody _rigidBody;

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
    }
}
