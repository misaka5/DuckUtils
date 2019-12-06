using DuckGame;
using System;

namespace DuckGame.DuckUtils {

    [EditorGroup("duckutils|equipment")]
    public class ImStuffHat : AbstractHat
    {
        public static readonly ATProvider ExplosionShrapnel = ExplosionAT.From<ATShrapnel>(120f, 150f).WithPenetration(99f);

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
            sound = SFX.Get(DuckUtils.GetAsset("sounds/imstuff.wav"), 0.8f, 0f, 0f, false);
        }

        public override void Quack(float volume, float pitch) {}

        public override void Terminate() {
            Playing = false;
        }

        public override void Update() {

            if (netEquippedDuck != null && !Playing)
            {
                Playing = true;
            }

            if(Playing) {
                Timer += Maths.IncFrameTimer();
                graphic = pickupSprite = _sprite = new SpriteMap(DuckUtils.GetAsset("hats/imstuff.png"), 129, 153);
                
                if(Timer > 18.2f) {
                    Explosion.Create(new ExplosionInfo(this) {
                        AmmoType = ExplosionShrapnel,
                        Bullets = 40
                    });

                    netEquippedDuck = null;
                    Level.Remove(this);
                }
            }

            base.Update();
        }
    }
}