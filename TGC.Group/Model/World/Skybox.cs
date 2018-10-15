using TGC.Core.Mathematica;
using TGC.Core.Terrain;

namespace TGC.Group.Model.World
{
    static class Skybox
    {
        public static TgcSkyBox InitSkybox()
        {
            var skyBox = new TgcSkyBox
            {
                Center = new TGCVector3(0, 600, 0),
                Size = new TGCVector3(13000, 12000, 13000)
            };
            var texturesPath = Game.Default.MediaDirectory + "Images\\";
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Up, texturesPath + "skybox.png");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Down, texturesPath + "skybox.png");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Left, texturesPath + "skybox left.png");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Right, texturesPath + "skybox right.png");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Front, texturesPath + "skybox front.png");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Back, texturesPath + "skybox back.png");
            skyBox.Init();

            return skyBox;
        }
    }
}
