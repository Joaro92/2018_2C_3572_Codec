using System.Drawing;
using TGC.Core.Example;
using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Examples.Camara;
using TGC.Group.Bullet.Physics;
using TGC.Group.Nivel1;
using TGC.Group.PlayerOne;
using TGC.Group.Utils;
using Microsoft.DirectX.DirectInput;
using TGC.Core.Direct3D;
using BulletSharp.Math;
using BulletSharp;
using TGC.Core.Textures;
using Microsoft.DirectX.Direct3D;

using TGC.Group.Model.GameStates;
using TGC.Group.Form;
using  System.Windows.Forms.VisualStyles;

namespace TGC.Group.Model
{
    public class GameModel : TgcExample
    {
        public IGameState GameState { get ; set ; }

        public GameModel(string mediaDir, string shadersDir) : base(mediaDir, shadersDir)
        {
            Category = Game.Default.Category;
            Name = Game.Default.Name;
            Description = "Idea seleccionada: Twisted Metal - Derby de demolición";
        }

        public override void Init()
        {
            GameState = new MenuInicial(this);
        }

        public override void Update()
        {
            PreUpdate();

            GameState.Update();

            PostUpdate();
        }

        public override void Render()
        {
            PreRender();

            GameState.Render();
          
            PostRender();
        }

        public override void Dispose()
        {
            GameState.Dispose();
        }
    }
}