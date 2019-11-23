using DuckGame;
using System;
using System.Collections.Generic;

namespace DuckGame.DuckUtils
{
    [EditorGroup("duckutils|explosives")]
    [BaggedProperty("isInDemo", true)]
    [BaggedProperty("isGrenade", true)]
    [BaggedProperty("isFatal", true)]
    public class FrogGrenade : GrenadeBase
    {
        public FrogGrenade(float x, float y) : base(x, y) {
            _editorName = "Frog Grenade";
            _bio = "Ribbit!";

            graphic = new SpriteMap(DuckUtils.GetAsset("weapons/frog_grenade.png"), 16, 16);
            center = new Vec2(8f, 8f);
            collisionOffset = new Vec2(-6f, -5f);
            collisionSize = new Vec2(12f, 12f);
            bouncy = 0.4f;
            friction = 0.07f;

            Timer = 2f;
        }

        protected override void CreateExplosion(Vec2 pos) {
            float x = pos.x;
            float y = pos.y - 2f;
            
            if(isServerForObject) {
                Level.Add(new Frog(x, y, Rando.Double() < 0.5f));

                int amount = Rando.Int(2, 4);
                for (int i = 0; i < amount; i++)
                {
                    float deg = (float)i * 360f / amount + Rando.Float(-10f, 10f);
                    float rad = Maths.DegToRad(deg);

                    float xDir = (float)Math.Cos(rad);
                    float yDir = (float)Math.Sin(rad);

                    Frog thing = new Frog(x, y, xDir > 0);
                    thing.hSpeed = xDir * 3;
                    thing.vSpeed = yDir * yDir * -3 - 2;
                    
                    Level.Add(thing);
                }
            }

            for(int i = 0; i < 3; i++)
                Level.Add(SmallSmoke.New(x + Rando.Float(-8f, 8f), y + Rando.Float(-8f, 8f)));
        }

        protected override void CreatePinParticle() {
            Thing grenadePin = new EjectedCap(x, y);
            grenadePin.hSpeed = (float)(-offDir) * (1.5f + Rando.Float(0.5f));
            grenadePin.vSpeed = -2f;
            Level.Add(grenadePin);
            SFX.Play("pullPin");
        }

        public override void OnSolidImpact(MaterialThing with, ImpactedFrom from)
        {
            Vec2 collisionVelocity = new Vec2(with.hSpeed - this.hSpeed, with.vSpeed - this.vSpeed);
            float lengthSq = collisionVelocity.lengthSq;
            if(lengthSq > 4f * 4f) {
                if(lengthSq > 9f * 9f) {
                    Timer = 0;
                }
                HasPin = false;
            }

            base.OnSolidImpact(with, from);
        }

        public class EjectedCap : EjectedShell {
            public EjectedCap(float xpos, float ypos)
		        : base(xpos, ypos, DuckUtils.GetAsset("shell/frog_grenade.png"))
            {
            }
        }
    }
}
