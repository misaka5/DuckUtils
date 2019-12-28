using System;
using DuckGame;
using Microsoft.Xna.Framework.Graphics;

namespace DuckGame.DuckUtils {

    public class FututreSphere : Thing
    {
        public StateBinding PositionBinding { get; private set; }

	    public StateBinding VelocityBinding { get; private set; }

        private float radius;

        private Vec2 travel;
        private Thing sender;

        public FututreSphere(float xval, float yval, Thing owner, Vec2 velocity)
            : base(xval, yval)
        {           
            PositionBinding = new CompressedVec2Binding("position", 2147483647, isvelocity: false, doLerp: true);
            VelocityBinding = new CompressedVec2Binding("travel", 1);

            radius = 10f;
            depth = 0.5f;

            travel = velocity;
            sender = owner;
        }

        public override void Update() {
            base.Update();

            position += travel * radius * 20f * Maths.IncFrameTimer();
            radius += 600f / (radius + 1f) * Maths.IncFrameTimer();

            if (isServerForObject && 
                base.x > Level.current.bottomRight.x + 200f || 
                base.x < Level.current.topLeft.x - 200f ||
                base.y > Level.current.bottomRight.y + 200f || 
                base.y < Level.current.topLeft.y - 200f)
            {
                Level.Remove(this);
            }

            foreach (MaterialThing item in Level.CheckCircleAll<MaterialThing>(position, radius))
            {
                bool itemIsSender = item == sender;
                bool itemIsDuckSender = (item is IAmADuck) && (sender is IAmADuck) && ((IAmADuck)item).ToDuck() == sender;
                
                if(!(itemIsSender || itemIsDuckSender) && item.isServerForObject) {
                    item.Destroy(new DTIncinerate(this));
                }
            }
        }

        public override void Draw()
        {
            Graphics.DrawCircle(position, radius, DuckGame.Color.Red, 2f, depth);
        }
    }
}