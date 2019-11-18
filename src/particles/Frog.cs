using DuckGame;
using System;

namespace DuckGame.DuckUtils {
    public class Frog : Holdable
    {
        public static readonly ATProvider ExplosionShrapnel = () => {
            ATShrapnel shrapnel = new ATShrapnel();
            shrapnel.range = 35f + Rando.Float(26f);
            return shrapnel;
        };

        private float lifetime;

        private float jumpDirection;        
        private float jumpTime;

        public Frog(float xpos, float ypos, bool direction)
            : base(xpos, ypos)
        {
           // _sprite.AddAnimation("frog", 1f, true, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
            //_sprite.SetAnimation("frog");
            graphic = new SpriteMap(DuckUtils.GetAsset("part/frog.png"), 16, 16);

            float scale = xscale = yscale = Rando.Float(0.5f, 0.8f);

            lifetime = Rando.Float(3f, 6f);
            center = new Vec2(8f, 7f);
		    collisionOffset = new Vec2(-6f, -5f);
		    collisionSize = new Vec2(11f, 14f * scale);
		    depth = -0.5f;
            bouncy = 0.1f;
            friction = 0.15f;
		    thickness = 2f;
		    weight = 1.4f;
		    _impactThreshold = 0.01f;

            jumpDirection = direction ? 1 : -1;
            jumpTime = Rando.Float(0f, 2f);
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void Update()
        {
            base.Update();

            if (lifetime < 0) {
                Explosion.Create(this, position, new Explosion.Config(ExplosionShrapnel, DuckUtils.GetAsset("sounds/frog_explosion.wav"), false));
                for(int i = 0; i < 2; i++)
                    Level.Add(SmallSmoke.New(x + Rando.Float(-8f, 8f), y + Rando.Float(-8f, 8f)));
                Level.Remove(this);
            } else {
                lifetime -= Maths.IncFrameTimer();
            }

            UpdateBehavior();
        }

        private void UpdateBehavior() {
            if(Math.Abs(hSpeed) + Math.Abs(vSpeed) < 0.1f) { //standing still
                if(jumpTime > 0) {
                    jumpTime -= Maths.IncFrameTimer();
                }else{
                    hSpeed = jumpDirection * 3;
                    vSpeed = Rando.Float(-2f, -3f);

                    jumpTime = Rando.Float(0.5f, 1.5f);
                    if(Rando.Double() < 0.15f)
                        jumpDirection = -jumpDirection;
                }
            }
        }
    }
}