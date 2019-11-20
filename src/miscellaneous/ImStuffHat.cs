using DuckGame;
using System;

namespace DuckGame.DuckUtils {

    [EditorGroup("duckutils|equipment")]
    public class ImStuffHat : AbstractHat
    {
        public static readonly ATProvider ExplosionShrapnel = () => {
            ATShrapnel shrapnel = new ATShrapnel();
            shrapnel.range = 120f + Rando.Float(30f);
            shrapnel.penetration = 99f;
            return shrapnel;
        };

        private Sound sound;

        public StateBinding PlayingBinding { get; private set; }
        public StateBinding TimerBinding { get; private set; }

        private bool _playing;
        public bool Playing {
            get {
                return _playing;
            }

            set {
                if(value != _playing) {
                    if(value) sound.Play();
                    else sound.Stop();
                    _playing = value;
                }
            }
        }

        public float Timer { get; set; }

        public ImStuffHat(float x, float y) : base(x, y) {
            PlayingBinding = new StateBinding("Playing");
            TimerBinding = new StateBinding("Timer");

            _sprite = new SpriteMap(DuckUtils.GetAsset("hats/imstuff.png"), 129, 153);
            sound = SFX.Get(DuckUtils.GetAsset("sounds/imstuff.wav"), 1f, 0f, 0f, false);
        }

        public override void Update() {

            if (netEquippedDuck != null && !Playing)
            {
                sound.Play();
                Playing = true;
            }

            if(Playing) {
                Timer += Maths.IncFrameTimer();
                graphic = pickupSprite = _sprite = new SpriteMap(DuckUtils.GetAsset("hats/imstuff.png"), 129, 153);
                
                if(Timer > 18.2f) {
                    Explosion.Create(this, position, ExplosionShrapnel);

                    netEquippedDuck = null;
                    Level.Remove(this);
                }
            }

            base.Update();
        }
    }
}