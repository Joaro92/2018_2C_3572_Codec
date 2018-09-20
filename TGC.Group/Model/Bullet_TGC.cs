using BulletSharp;
using System;
using TGC.Core.Direct3D;
using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Textures;
using TGC.Group.Bullet.Physics;

namespace TGC.Group.Bullet_TGC_Object
{
    public class Bullet_TGC
    {
        private TgcMesh _tgcMesh;
        private RigidBody _rigidBody;

        /// <summary>
        ///  Crea un Cuerpo Rígido que posee masa y un coeficiente de rozamiento, con propiedades de Bullet y TgcMesh,
        ///  a partir de un archivo 'TgcScene.xml'
        /// </summary>
        public Bullet_TGC(String xmlPath, TGCVector3 position, float mass, float friction)
        {
            var loader = new TgcSceneLoader();
            this._tgcMesh = loader.loadSceneFromFile(Game.Default.MediaDirectory + xmlPath).Meshes[0];
            this._rigidBody = BulletRigidBodyConstructor.CreateRigidBodyFromTgcMesh(_tgcMesh, position, mass, friction);
        }

        public Bullet_TGC(String xmlPath, TGCVector3 position)
        {
            var loader = new TgcSceneLoader();
            this._tgcMesh = loader.loadSceneFromFile(Game.Default.MediaDirectory + xmlPath).Meshes[0];
            this._rigidBody = BulletRigidBodyConstructor.CreateRigidBodyFromTgcMesh(_tgcMesh, position);
        }

        /// <summary>
        ///  Se crea un plano estático de masa 0, con propiedades de Bullet y TgcMesh
        /// </summary>
        public Bullet_TGC(String texturePath, TGCVector3 origin, TGCVector3 size, TgcPlane.Orientations orientation)
        {
            var floorTexture = TgcTexture.createTexture(D3DDevice.Instance.Device, Game.Default.MediaDirectory + texturePath);
            var floorMesh = new TgcPlane(origin, size, orientation, floorTexture);
            this._tgcMesh = floorMesh.toMesh("plano");

            TGCVector3 normalVector = new TGCVector3(0, 0, 0);
            if (orientation.Equals(TgcPlane.Orientations.XYplane)) normalVector = new TGCVector3(0, 0, 1);
            if (orientation.Equals(TgcPlane.Orientations.XZplane)) normalVector = new TGCVector3(0, 1, 0);
            if (orientation.Equals(TgcPlane.Orientations.YZplane)) normalVector = new TGCVector3(1, 0, 0);

            this._rigidBody = BulletRigidBodyConstructor.CreateFloor(normalVector);
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
