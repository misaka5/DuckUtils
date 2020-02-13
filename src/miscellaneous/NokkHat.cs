using DuckGame;
using System;

namespace DuckGame.DuckUtils {

    [EditorGroup("duckutils|equipment")]
    public class NokkHat : AbstractHat
    {
        private static readonly AlphaFunction AlphaFunction = (thing) => {
            if(thing.isServerForObject) return 0.5f;
            
            float spf = Math.Min(10, thing.hSpeed * thing.hSpeed + thing.vSpeed * thing.vSpeed) / 10f;
            return spf * 0.045f;
        };

        public static readonly float Capacity = 9.76f;
        public static readonly float ChargeSpeed = 0.9f;
        public static readonly float DischargeSpeed = 1f;

        public StateBinding ActiveBinding { get; private set; }
        private bool activated = false;
        public bool Active 
        { 
            get {
                return activated;
            }

            set {
                if(activated != value) {
                    if(value) {
                        SFX.Play(DuckUtils.GetAsset("sounds/nokk_start.wav"));
                        sound.Play();
                    } else {
                        sound.Stop();
                        SFX.Play(DuckUtils.GetAsset("sounds/nokk_end.wav"));
                    }

                    activated = value;
                }
            }
        }

        public float Charge { get; private set; }

        private Sound sound;

        public NokkHat(float x, float y) : base(x, y) 
        {
            _editorName = "Nokk Hat";

            graphic = _pickupSprite = _sprite = new SpriteMap(DuckUtils.GetAsset("hats/nokk.png"), 32, 32);
            Charge = Capacity;

            center = new Vec2(16f, 24f);

            sound = SFX.Get(DuckUtils.GetAsset("sounds/nokk_noise.wav"), 1f, 0f, 0f, true);
            ActiveBinding = new StateBinding("Active");
        }

        public override void Update() 
        {
            if(IsPressed("QUACK")) {
                if(isServerForObject && !Active && Charge >= Capacity) {
                    Active = true;
                }
            }

            if(Active) {
                if(Charge > 0) Charge -= Maths.IncFrameTimer() * DischargeSpeed;
                else Active = false;
            } else {
                if(Charge < Capacity) Charge += Maths.IncFrameTimer() * ChargeSpeed;
            }

            if(Active) {
                AlphaTransform.Transform(this, AlphaFunction);
                if(netEquippedDuck != null) {
                    AlphaTransform.Transform(netEquippedDuck, AlphaFunction);
                    AlphaTransform.Transform(netEquippedDuck.holdObject, AlphaFunction);

                    AlphaTransform.Transform(netEquippedDuck._ragdollInstance, AlphaFunction);
					AlphaTransform.Transform(netEquippedDuck._trappedInstance, AlphaFunction);
					AlphaTransform.Transform(netEquippedDuck._cookedInstance, AlphaFunction);

                    foreach(Equipment eq in netEquippedDuck._equipment) 
                    {
                        AlphaTransform.Transform(eq, AlphaFunction);
                    }
                }
            }

            base.Update();
        }
    }
}