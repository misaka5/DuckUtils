using System;
using DuckGame;
using Microsoft.Xna.Framework.Graphics;

namespace DuckGame.DuckUtils {

    public class ElectricParticle : Thing
    {
        public static readonly float Lifetime = 0.1f;

        private static int ptr = 0;
        private static readonly int kMaxObjects = 128;
        private static readonly ElectricParticle[] objects = new ElectricParticle[kMaxObjects];

        public static ElectricParticle New(Vec2 pos, Vec2 vel) {
            ElectricParticle particle = objects[ptr];
            if(particle == null) {
                particle = objects[ptr] = new ElectricParticle();
            }

            if(!particle.removeFromLevel) Level.Remove(particle);
            
            particle.Init(pos, vel);

            ptr++;
            if(ptr >= kMaxObjects) ptr = 0;
            return particle;
        }

        private float lifetime;
        private ElectricParticle() : base(0, 0)
        {           
            graphic = new Sprite(DuckUtils.GetAsset("part/electric_particle.png"));
            center = new Vec2(1f, 1f);
            depth += 2;
        }

        private void Init(Vec2 pos, Vec2 vel) {
            position = pos;
            hSpeed = vel.x;
            vSpeed = vel.y;
            
            lifetime = Lifetime;
            alpha = 1f;
        }

        public override void Update()
        {
            base.Update();

            if(lifetime > 0) {
                lifetime -= Maths.IncFrameTimer();
                if(lifetime <= 0f) Level.Remove(this);

                alpha = (lifetime / Lifetime);
            }

            if(vSpeed < 1f) vSpeed += 0.1f;

            vSpeed += Rando.Float(-0.2f, 0.2f);
            hSpeed += Rando.Float(-0.2f, 0.2f);

            x += hSpeed;
	        y += vSpeed;
        }
    }
}