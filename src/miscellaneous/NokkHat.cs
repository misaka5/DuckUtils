using DuckGame;
using System;
using System.Collections.Generic;

namespace DuckGame.DuckUtils {

    [EditorGroup("duckutils|equipment")]
    public class NokkHat : AbstractHat
    {
        public class HiddenState {

            static HiddenState() {
                DuckUtils.Updated += (e, args) => UpdateAll();
                DuckUtils.LevelChanged += (e, args) => Clear();
            }

            private static readonly IDictionary<Thing, HiddenState> states = new Dictionary<Thing, HiddenState>();
        
            private static void Clear() {
                states.Clear();
                DevConsole.Log("NOKK UPDATE: clear", Color.Yellow);
            }

            private static void UpdateAll() {
                foreach(KeyValuePair<Thing, HiddenState> p in states) {
                    p.Value.Update();
                }
            }

            public static void MarkAsHidden(Thing thing) {
                if(thing == null) return;

                HiddenState state = null;
                if(!states.TryGetValue(thing, out state))
                {
                    state = new HiddenState(thing);
                    states.Add(thing, state);
                }

                state.Hidden = true;
            }

            private int ttl;
            private bool lastState = false;

            private float prevAlpha;
            private float targetAlpha;

            public Thing Thing { get; set; }
            public bool Hidden {
                get {
                    return ttl > 0;
                }

                set {
                    ttl = value ? 2 : 0;
                }
            }

            private float TargetHiddenAlpha {
                get {
                    return Thing.isServerForObject ? 0.5f : 0f;
                }
            }

            public HiddenState(Thing t) {
                Thing = t;
                targetAlpha = Thing.alpha;
            }

            private void Update() {
                if(Thing.removeFromLevel) return;

                ttl--;
                if(lastState != Hidden) {
                    if(Hidden) Hide();
                    else Show();
                    lastState = Hidden;
                }

                if(Hidden) targetAlpha = TargetHiddenAlpha;
                Thing.alpha += (targetAlpha - Thing.alpha) * 0.1f;
            }

            private void Hide() {
                prevAlpha = targetAlpha;
                targetAlpha = TargetHiddenAlpha;
            }

            private void Show() {
                targetAlpha = prevAlpha;
            }
        }

        public static readonly float Capacity = 10f;

        public StateBinding ActiveBinding { get; private set; }
        private bool activated = false;
        public bool Active { 
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
                    DevConsole.Log("NOKK UPDATE: " + value, Color.Red);
                }
            }
        }

        public float Charge { get; private set; }

        private Sound sound;

        public NokkHat(float x, float y) : base(x, y) {
            _editorName = "Nokk Hat";

            graphic = _pickupSprite = _sprite = new SpriteMap(DuckUtils.GetAsset("hats/nokk.png"), 32, 32);
            Charge = Capacity;

            center = new Vec2(16f, 24f);

            sound = SFX.Get(DuckUtils.GetAsset("sounds/nokk_noise.wav"), 1f, 0f, 0f, true);
            ActiveBinding = new StateBinding("Active");
        }

        public override void Update() {
            if(IsPressed("QUACK")) {
                if(isServerForObject && !Active && Charge >= Capacity) {
                    Active = true;
                }
            }

            if(Active) {
                if(Charge > 0) Charge -= Maths.IncFrameTimer();
                else Active = false;
            } else {
                if(Charge < Capacity) Charge += Maths.IncFrameTimer();
            }

            if(Active) {
                HiddenState.MarkAsHidden(this);
                if(netEquippedDuck != null) {
                    HiddenState.MarkAsHidden(netEquippedDuck);
                    HiddenState.MarkAsHidden(netEquippedDuck.holdObject);

                    HiddenState.MarkAsHidden(netEquippedDuck._ragdollInstance);
					HiddenState.MarkAsHidden(netEquippedDuck._trappedInstance);
					HiddenState.MarkAsHidden(netEquippedDuck._cookedInstance);

                    foreach(Equipment eq in netEquippedDuck._equipment) 
                    {
                        HiddenState.MarkAsHidden(eq);
                    }
                }
            }

            base.Update();
        }
    }
}