using System;
using DuckGame;
using Microsoft.Xna.Framework.Graphics;

namespace DuckGame.DuckUtils {

    public class EnergySphere : Thing
    {
        public static readonly Vec2 Gravity = new Vec2(0, 0.7f);

        public StateBinding PositionBinding { get; private set; }

	    public StateBinding VelocityBinding { get; private set; }

        private Vec2 travel;
        private Thing sender;

        public EnergySphere(float xval, float yval, Thing owner, Vec2 velocity)
            : base(xval, yval)
        {           
            PositionBinding = new CompressedVec2Binding("position", 2147483647, isvelocity: false, doLerp: true);
            VelocityBinding = new CompressedVec2Binding("travel", 1);

            depth += 2;

            travel = velocity;
            sender = owner;

            SpriteMap sprite = new SpriteMap(DuckUtils.GetAsset("part/energy_sphere.png"), 8, 8);
            sprite.AddAnimation("anim", 0.2f, true, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15);
            sprite.SetAnimation("anim");
            graphic = sprite;
        }

        public override void Update() {
            base.Update();

            travel += Gravity;
            position += travel;

            if (isServerForObject && 
                base.x > Level.current.bottomRight.x + 200f || 
                base.x < Level.current.topLeft.x - 200f ||
                base.y > Level.current.bottomRight.y + 200f || 
                base.y < Level.current.topLeft.y - 200f)
            {
                Level.Remove(this);
            }

            Vec2 predictedVel = travel + Gravity;
            Vec2 predictedPos = position + predictedVel;

            Vec2 hit;
            MaterialThing t = Level.current.CollisionRay<MaterialThing>(position, predictedPos, out hit);
            bool isViable = t != null && (t.thickness > 1f || t is IAmADuck) && t != sender;
            
            if(!isViable) {
                t = Level.CheckCircle<IAmADuck>(position, 8f) as MaterialThing;
                if(t != null) hit = t.position;
            } 

            if(t != null) {
                foreach (MaterialThing item in Level.CheckCircleAll<MaterialThing>(hit, 16f))
                {
                    Fondle(item);
                    item.Zap(this);
                }

                Level.Remove(this);
            }
        }
    }
}