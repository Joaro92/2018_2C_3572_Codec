using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.Direct3D;
using TGC.Core.SceneLoader;
using TGC.Core.Textures;

namespace TGC.Group.Model.Vehicles
{
    public class Vehiculo
    {
        public string Name { get; }
        public string Color { get; private set; }

        public TgcMesh SampleMesh { get; }

        public string ChassisXmlPath { get; }
        public string WheelsXmlPath { get; }

        public Vehiculo(string name, string color = "Blue")
        {
            this.Name = name;
            this.Color = color;
            var lowercaseName = Name.ToLower();

            var dir = Game.Default.MediaDirectory + Game.Default.VehiclesDirectory;
            var loader = new TgcSceneLoader();
            this.SampleMesh = loader.loadSceneFromFile(dir + "car-" + lowercaseName + "-TgcScene.xml" ).Meshes[0];
            this.ChassisXmlPath = dir + "chassis-" + lowercaseName + "-TgcScene.xml";
            this.WheelsXmlPath = dir + "tires-common-TgcScene.xml";

            if (this.Color != "Blue")
            {
                this.ChangeColor(Color);
            }

            
        }

        public void ChangeColor(string newColor)
        {
            ChangeTextureColor(SampleMesh, newColor);

            this.Color = newColor;
        }

        public static void ChangeTextureColor(TgcMesh mesh, string newColor)
        {
            TgcTexture[] diffuseMaps = mesh.DiffuseMaps;

            string newTexturePath = "";
            int index = 0;
            foreach (TgcTexture texture in diffuseMaps)
            {
                if (texture.FileName.Contains("Car Material"))
                {
                    newTexturePath = texture.FilePath;
                    break;
                }
                index++;
            }

            string oldColor = newTexturePath.Split('\\')[5].Split(' ')[2].Split('.')[0];
            newTexturePath = newTexturePath.Replace(oldColor, newColor);

            var textureAux = TgcTexture.createTexture(D3DDevice.Instance.Device, newTexturePath.Split('\\')[5], newTexturePath);
            mesh.addDiffuseMap(textureAux);
            mesh.deleteDiffuseMap(index, diffuseMaps.Length - 1);
        }
    }
}
