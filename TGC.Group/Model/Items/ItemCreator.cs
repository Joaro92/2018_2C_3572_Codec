using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;

namespace TGC.Group.Model.Items
{
    public static class ItemCreator
    {
        public static TgcMesh SpawnItem(string name, TGCVector3 position)
        {
            var loader = new TgcSceneLoader();
            var mesh = loader.loadSceneFromFile(Game.Default.MediaDirectory + "Items\\" + name.ToLower() + "-item-TgcScene.xml").Meshes[0];
            mesh.Position = position;
            return mesh;
        }
    }
}
