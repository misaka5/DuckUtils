using System;
using System.Collections.Generic;
using DuckGame;

namespace DuckGame.DuckUtils {

    public class InvisibilityParticle : Thing {
        private static readonly AlphaFunction AlphaFunction = (thing) => {
            float wobble = ((float)(Math.Sin(thing.x / 20 + DuckUtils.Time * .6f) + Math.Sin(thing.y / 20 + DuckUtils.Time * .6f)) / 2f - 0.7f) / (1 - 0.7f);
            return Math.Max(0, wobble) * 0.014f + 0.001f;
        };

        public static readonly float MaxLifetime = 12f;
        public static readonly float MaxRadius = 70f;
        private float lifetime = MaxLifetime;
        private float radius = 0;

        public StateBinding PositionBinding { get; private set; }
        public StateBinding RadiusBinding { get; private set; }

        public InvisibilityParticle(float x, float y) : base(x, y) 
        {         
            RadiusBinding = new StateBinding("radius");
            PositionBinding = new CompressedVec2Binding("position", 2147483647, isvelocity: false, doLerp: true);
        }

        private float CalculateRadius() {
            float rlifetime = lifetime / MaxLifetime - 0.5f;
            float rad = (1 - 4 * rlifetime * rlifetime) * 3;
            float crad = Math.Min(1, rad);
            return crad * MaxRadius;
        }

        public override void Update() 
        {
            base.Update();

            radius = CalculateRadius();

            if(radius > 0) {
                IEnumerable<Thing> things = Level.CheckCircleAll<Thing>(position, radius);
                foreach(Thing thing in things) {
                    AlphaTransform.Transform(thing, AlphaFunction);

                    if(thing is IAmADuck) {
                        Duck d = ((IAmADuck) thing).ToDuck();
                        if(d != null) {
                            AlphaTransform.Transform(d.holdObject, AlphaFunction);

                            AlphaTransform.Transform(d._ragdollInstance, AlphaFunction);
                            AlphaTransform.Transform(d._trappedInstance, AlphaFunction);
                            AlphaTransform.Transform(d._cookedInstance, AlphaFunction);

                            foreach(Equipment eq in d._equipment) 
                            {
                                AlphaTransform.Transform(eq, AlphaFunction);
                            }
                        }
                    }
                }
            }

            if(lifetime > 0) {
                lifetime -= Maths.IncFrameTimer();
            } else {
                Level.Remove(this);
            }
        }

        public override void Draw() {}
    }
}