using DuckGame;
using System;

namespace DuckGame.DuckUtils {

    [EditorGroup("duckutils|equipment")]
    public class BrekotkinHat : AbstractHat
    {
        private Sound sound;

        public StateBinding PlayingBinding { get; private set; }

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

        public BrekotkinHat(float x, float y) : base(x, y) {
            PlayingBinding = new StateBinding("Playing");

            graphic = _pickupSprite = _sprite = new SpriteMap(DuckUtils.GetAsset("hats/brekotkin.png"), 32, 32);
            _editorName = "Brekotkin Hat";

            sound = SFX.Get(DuckUtils.GetAsset("sounds/disco_ebalo.wav"), 1f, 0f, 0f, true);
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
    }
}