using DuckGame;
using System;

namespace DuckGame.DuckUtils {

    [EditorGroup("duckutils|equipment")]
    public class SatanicBrekotkinHat : AbstractHat
    {
        public static readonly ATProvider ExplosionShrapnel = () => {
            ATShrapnel shrapnel = new ATShrapnel();
            shrapnel.range = 120f + Rando.Float(26f);
            shrapnel.penetration = 99f;
            return shrapnel;
        };

        public StateBinding PlayingBinding { get; private set; }

        private Sound sound;

        private bool exploded = false;
        
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

            graphic = _pickupSprite = _sprite = new SpriteMap(DuckUtils.GetAsset("hats/brekotkin_satanic.png"), 32, 32);
            _editorName = "Satanic Brekotkin Hat";

            sound = SFX.Get(DuckUtils.GetAsset("sounds/umri.wav"), 1f, 0f, 0f, false);
        }

        public override void OpenHat() {
            Playing = true;
        }
        
	    public override void CloseHat() {
            Playing = false;
        }

        public override void Update() {
            if (TimeOpened > 4f && !exploded) {
                Explosion.Create(this, position, ExplosionShrapnel);
                exploded = true;
            }

            base.Update();
        }
    }
}