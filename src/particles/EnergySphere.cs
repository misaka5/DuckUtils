using System;
using DuckGame;
using Microsoft.Xna.Framework.Graphics;

namespace DuckGame.DuckUtils {

    public class EnergySphere : PhysicsObject
    {
        private SpriteMap sprite;

        private Thing sender;

        public EnergySphere(float xval, float yval, Thing owner)
            : base(xval, yval)
        {           
            graphic = sprite = new SpriteMap(DuckUtils.GetAsset("part/energy_sphere.png"), 16, 16);
            sprite.AddAnimation("anim", 0.2f, true, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15);
            sprite.SetAnimation("anim");

            sender = owner;

            center = new Vec2(8f, 8f);
		    collisionOffset = new Vec2(-2f, -2f);
		    collisionSize = new Vec2(4f, 4f);
        }

        public override void Update() {
            base.Update();

            if (isServerForObject && (base.x > Level.current.bottomRight.x + 200f || base.x < Level.current.topLeft.x - 200f))
            {
                Level.Remove(this);
            }
        }

        public override void OnImpact(MaterialThing with, ImpactedFrom from) 
        {
            base.OnImpact(with, from);

            if(isServerForObject) {
                if(owner == with) return;
                if(with.thickness > 1f || with is IAmADuck) {
                    foreach (MaterialThing item in Level.CheckCircleAll<MaterialThing>(position, 16f))
                    {
                        if(item != owner) {
                            Fondle(item);
                            item.Zap(this);
                        }
                    }

                    Level.Remove(this);
                }
            }
        }
    }
}