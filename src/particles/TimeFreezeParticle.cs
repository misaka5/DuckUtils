using System;
using System.Collections.Generic;
using DuckGame;

namespace DuckGame.DuckUtils {

    public class TimeFreezeParticle : Thing {

        public static readonly float MaxLifetime = 10f;
        public static readonly float MaxRadius = 70f;
        private float lifetime = MaxLifetime;
        private float radius = 0;

        public StateBinding PositionBinding { get; private set; }
        public StateBinding RadiusBinding { get; private set; }

        public TimeFreezeParticle(float x, float y) : base(x, y) {         
            RadiusBinding = new StateBinding("radius");
            PositionBinding = new CompressedVec2Binding("position", 2147483647, isvelocity: false, doLerp: true);
            
            this.depth = -1f;
            //this.material = new MaterialGlitch(this);
        }

        private float CalculateRadius() {
            float rlifetime = lifetime / MaxLifetime - 0.5f;
            float rad = (1 - 4 * rlifetime * rlifetime) * 3;
            float crad = Math.Min(1, rad);
            float wobble = (float)Math.Sin(lifetime) * crad * 5f;
            return crad * MaxRadius + wobble;
        }

        public override void Update() {
            base.Update();

            radius = CalculateRadius();

            if(radius > 0) {
                float radiusPow = (float)Math.Pow(radius, 8);
                IEnumerable<Thing> things = Level.CheckCircleAll<Thing>(position, radius);
                foreach(Thing thing in things) {
                    float deltaSq = (thing.position - position).lengthSq;
                    float factor = (float)Math.Min(Math.Pow(deltaSq, 4) / radiusPow, 1);
                    
                    thing.hSpeed *= factor;
                    thing.vSpeed *= factor;
                }
            }

            if(lifetime > 0) {
                lifetime -= Maths.IncFrameTimer();
            }else{
                Level.Remove(this);
            }
        }

        public override void Draw() {
           // Graphics.material = this.material;
            Graphics.DrawCircle(position, radius, DuckGame.Color.CornflowerBlue, 2f, depth);
          //  Graphics.material = null;
        }
    }
}