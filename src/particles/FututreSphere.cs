using System;
using DuckGame;
using Microsoft.Xna.Framework.Graphics;

namespace DuckGame.DuckUtils {

    public class FututreSphere : Thing
    {
        public StateBinding PositionBinding { get; private set; }

	    public StateBinding VelocityBinding { get; private set; }

        private Sprite sprite;

        private float radius;

        private Vec2 travel;

        public FututreSphere(float xval, float yval, Thing owner, Vec2 velocity)
            : base(xval, yval)
        {           
            PositionBinding = new CompressedVec2Binding("position", 2147483647, isvelocity: false, doLerp: true);
            VelocityBinding = new CompressedVec2Binding("travel", 1);

            sprite = new Sprite(DuckUtils.GetAsset("part/fututre_beam.png"));
            radius = 10f;
            depth = 0.5f;

            travel = velocity;
            this.owner = owner;
        }

        public override void Update() {
            base.Update();

            position += travel * radius * 15f * Maths.IncFrameTimer();
            radius += 35f * Maths.IncFrameTimer();

            if (base.isServerForObject && (base.x > Level.current.bottomRight.x + 200f || base.x < Level.current.topLeft.x - 200f))
            {
                Level.Remove(this);
            }

            foreach (MaterialThing item in Level.CheckCircleAll<MaterialThing>(position, radius))
            {
                if(item != owner) {
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