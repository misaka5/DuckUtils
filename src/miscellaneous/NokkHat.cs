using DuckGame;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DuckGame.DuckUtils {

    [EditorGroup("duckutils|equipment")]
    public class NokkHat : AbstractHat
    {
        public class HiddenState {

            #region Connection Indicator Override Hacks

            private static readonly Func<Dictionary<NetworkConnection, ConnectionIndicatorElement>> cieHook;

            private static Func<Dictionary<NetworkConnection, ConnectionIndicatorElement>> CreateConnectionIndicatorHook() {
                return Expression.Lambda<Func<Dictionary<NetworkConnection, ConnectionIndicatorElement>>>(Expression.Field(null, typeof(ConnectionIndicator), "_connections")).Compile();
            }

            private static void DrawHandler() {
                if(Network.isActive) {
                    Dictionary<NetworkConnection, ConnectionIndicatorElement> h = cieHook.Invoke();
                    foreach(ConnectionIndicatorElement elem in h.Values) {
                        if(elem.duck != null && states.ContainsKey(elem.duck)) elem.duck = null;
                    }
                }
            }

            #endregion

            static HiddenState() {
                DuckUtils.Updated += (e, args) => UpdateAll();
                DuckUtils.Drawing += (e, args) => DrawHandler();
                DuckUtils.LevelChanged += (e, args) => Clear();

                cieHook = CreateConnectionIndicatorHook();
            }

            private static readonly IDictionary<Thing, HiddenState> states = new Dictionary<Thing, HiddenState>();
        
            private static void Clear() {
                states.Clear();
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
                if(Math.Abs(Thing.alpha - targetAlpha) < 0.01f) Thing.alpha = targetAlpha;
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