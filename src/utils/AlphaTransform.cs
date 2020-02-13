using System;
using DuckGame;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DuckGame.DuckUtils
{

    public delegate float AlphaFunction(Thing thing);
    public class AlphaTransform
    {

        #region Connection Indicator Override Hacks

        private static readonly Func<Dictionary<NetworkConnection, ConnectionIndicatorElement>> cieHook;

        private static Func<Dictionary<NetworkConnection, ConnectionIndicatorElement>> CreateConnectionIndicatorHook()
        {
            return Expression.Lambda<Func<Dictionary<NetworkConnection, ConnectionIndicatorElement>>>(Expression.Field(null, typeof(ConnectionIndicator), "_connections")).Compile();
        }

        private static void DrawHandler()
        {
            if (Network.isActive)
            {
                Dictionary<NetworkConnection, ConnectionIndicatorElement> h = cieHook.Invoke();
                foreach (ConnectionIndicatorElement elem in h.Values)
                {
                    if (elem.duck != null && states.ContainsKey(elem.duck)) elem.duck = null;
                }
            }
        }

        #endregion

        static AlphaTransform()
        {
            DuckUtils.Updated += (e, args) => UpdateAll();
            DuckUtils.Drawing += (e, args) => DrawHandler();
            DuckUtils.LevelChanged += (e, args) => Clear();

            cieHook = CreateConnectionIndicatorHook();
        }

        private static readonly IDictionary<Thing, AlphaTransform> states = new Dictionary<Thing, AlphaTransform>();

        private static void Clear()
        {
            states.Clear();
        }

        private static void UpdateAll()
        {
            foreach (KeyValuePair<Thing, AlphaTransform> p in states)
            {
                p.Value.Update();
            }
        }

        public static void Transform(Thing thing, AlphaFunction func)
        {
            if (thing == null) return;
            if (func == null) throw new ArgumentNullException("func");

            AlphaTransform state = null;
            if (!states.TryGetValue(thing, out state))
            {
                state = new AlphaTransform(thing);
                states.Add(thing, state);
            }

            state.Hidden = true;
            state.Function = func;
        }

        private int ttl;
        private bool lastState = false;

        private float prevAlpha;
        private float targetAlpha;

        public AlphaFunction Function { get; set; }
        public Thing Thing { get; set; }
        public bool Hidden
        {
            get
            {
                return ttl > 0;
            }

            set
            {
                ttl = value ? 2 : 0;
            }
        }

        private float TargetHiddenAlpha
        {
            get
            {
                return Function.Invoke(Thing);
            }
        }

        public AlphaTransform(Thing t)
        {
            Thing = t;
            targetAlpha = Thing.alpha;
        }

        private void Update()
        {
            if (Thing.removeFromLevel) return;

            ttl--;
            if (lastState != Hidden)
            {
                if (Hidden) Hide();
                else Show();
                lastState = Hidden;
            }

            if (Hidden) targetAlpha = TargetHiddenAlpha;

            Thing.alpha += (targetAlpha - Thing.alpha) * 0.1f;
            if (Math.Abs(Thing.alpha - targetAlpha) < 0.01f) Thing.alpha = targetAlpha;
        }

        private void Hide()
        {
            prevAlpha = targetAlpha;
            targetAlpha = TargetHiddenAlpha;
        }

        private void Show()
        {
            targetAlpha = prevAlpha;
        }
    }
}