using DuckGame;
using System;

namespace DuckGame.DuckUtils {

    [EditorGroup("duckutils|equipment")]
    public class BrekotkinHat : AbstractHat
    {
        private static readonly float BeatTimeDelta = 60f / 130f;

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

        private float timer = 0f;

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
            timer = BeatTimeDelta;
        }

	    public override void CloseHat() 
        {
            Playing = false;
        }

        public override void Update() 
        {
            base.Update();

            if(Playing) 
            {
                timer -= Maths.IncFrameTimer();
                if(timer <= 0f)
                {
                    timer += BeatTimeDelta;
                    OnBeat();
                }   
            }     
        }

        private void OnBeat()
        {
            foreach(PhysicsObject d in Level.current.things[typeof(PhysicsObject)]) 
            {
                d.vSpeed -= 1.25f;
            }
        }
    }
}