using BulletSharp.Math;
using Microsoft.DirectX.Direct3D;
using System;
using TGC.Core.Mathematica;
using TGC.Core.Particle;
using TGC.Core.SceneLoader;
using TGC.Core.Shaders;

namespace TGC.Group.Model.World
{
    public class Explosion
    {
        protected Random randomGenerator = new Random();
        protected Effect explosionFX;
        protected TGCVector3 initialPos;
        protected ParticleEmitter emitter, emitter2;
        protected TgcMesh explosionMesh, ringMesh;
        public bool expired = false;
        protected float time = 0, rnd, neg = 1;

        public Explosion (Vector3 pos, Effect fx)
        {
            initialPos = new TGCVector3(pos);

            // Emisor de Particulas
            var smokeParticlePath = Game.Default.MediaDirectory + "Images\\smoke.png";
            
            emitter = new ParticleEmitter(smokeParticlePath, 10);
            emitter.MinSizeParticle = 1.7f;
            emitter.MaxSizeParticle = 2.65f;
            emitter.ParticleTimeToLive = 0.35f;
            emitter.CreationFrecuency = 0.15f;
            emitter.Dispersion = 100;
            emitter.Playing = false;
            emitter.Position = new TGCVector3(pos.X - 4, pos.Y + 4, pos.Z);
 
            emitter2 = new ParticleEmitter(smokeParticlePath, 10);
            emitter2.MinSizeParticle = 1.6f;
            emitter2.MaxSizeParticle = 2.4f;
            emitter2.ParticleTimeToLive = 0.35f;
            emitter2.CreationFrecuency = 0.20f;
            emitter2.Dispersion = 100;

            // Meshes
            var loader = new TgcSceneLoader();

            explosionMesh = loader.loadSceneFromFile(Game.Default.MediaDirectory + "Scenarios\\ball-TgcScene.xml").Meshes[0];
            explosionMesh.Scale = new TGCVector3(0.4f, 0.4f, 0.4f);
            explosionMesh.Position = initialPos;

            ringMesh = loader.loadSceneFromFile(Game.Default.MediaDirectory + "Scenarios\\ring-TgcScene.xml").Meshes[0];
            ringMesh.Scale = new TGCVector3(0.48f, 0.48f, 0.48f);
            ringMesh.Position = initialPos;

            // Cargar Efecto
            explosionFX = fx;

            explosionMesh.Effect = explosionFX;
            ringMesh.Effect = explosionFX;

            explosionMesh.Technique = "Explosion";
            ringMesh.Technique = "Ring";
            ringMesh.AlphaBlendEnable = true;
        }

        public void Render(float ElapsedTime)
        {
            if (expired) return;

            time += ElapsedTime;
            explosionMesh.Effect.SetValue("time2", time);
            ringMesh.Effect.SetValue("time2", time);

            if (FastMath.PI + 1 < time * 2.2f)
            {
                expired = true;
                return;
            }

            explosionMesh.Render();
            ringMesh.Render();
            
            if (FastMath.Max(FastMath.Sin(time * 2.2f), 0) > 0.2f)
                emitter.Playing = true;
            else
                emitter.Playing = false;

            rnd = (float)randomGenerator.NextDouble();

            emitter.Speed = new TGCVector3(rnd * 130, 19, rnd * 130) * 0.5f;

            if (rnd < 0.25f)
                emitter.Position = new TGCVector3(initialPos.X + 4 * neg, initialPos.Y, initialPos.Z);
            else if (rnd < 0.5f)
                emitter.Position = new TGCVector3(initialPos.X, initialPos.Y + 4 * neg, initialPos.Z);
            else if (rnd < 0.75f)
                emitter.Position = new TGCVector3(initialPos.X, initialPos.Y, initialPos.Z + 3 * neg);

            emitter.render(ElapsedTime);

            emitter2.Position = new TGCVector3(emitter.Position.X + neg * -8, emitter.Position.Y + neg * -8, emitter.Position.Z);
            emitter2.Speed = emitter.Speed;
            emitter2.Playing = emitter.Playing;
            emitter2.render(ElapsedTime);

            neg = neg * -1;
        }

        public void Dispose()
        {
            emitter.dispose();
            emitter2.dispose();
            explosionMesh.Dispose();
            ringMesh.Dispose();
            explosionFX.Dispose();
        }
    }
}
