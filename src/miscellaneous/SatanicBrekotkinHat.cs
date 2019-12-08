using DuckGame;
using System;

namespace DuckGame.DuckUtils {

    [EditorGroup("duckutils|equipment")]
    public class SatanicBrekotkinHat : AbstractHat
    {
        public static readonly ATProvider ExplosionShrapnel = ExplosionAT.From<ATShrapnel>().WithRange(120f, 150f).WithPenetration(99f);

        public StateBinding PlayingBinding { get; private set; }
        public StateBinding TimeOpenedBinding { get; private set; }

        private bool exploded;
        private bool Exploded {
            get {
                return exploded;
            }

            set {
                if(exploded != value) {
                    if(value) {
                        Explosion.Create(new ExplosionInfo(this) {
                            AmmoType = ExplosionShrapnel,
                            Bullets = 40
                        });
                    }

                    exploded = value;
                }
            }
        }

        private Sound sound;

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

        public SatanicBrekotkinHat(float x, float y) : base(x, y) 
        {
            PlayingBinding = new StateBinding("Playing");
            TimeOpenedBinding = new StateBinding("TimeOpened");

            graphic = _pickupSprite = _sprite = new SpriteMap(DuckUtils.GetAsset("hats/brekotkin_satanic.png"), 32, 32);
            _editorName = "Satanic Brekotkin Hat";

            sound = SFX.Get(DuckUtils.GetAsset("sounds/umri.wav"), 1f, 0f, 0f, false);
        }

        public override void Quack(float volume, float pitch) {}

        public override void OpenHat() 
        {
            Playing = true;
        }
        
	    public override void CloseHat() 
        {
            Playing = false;
        }

        public override void Update() 
        {
            if (TimeOpened > 4f) {
                Exploded = true;
            }

            base.Update();
        }
    }
}