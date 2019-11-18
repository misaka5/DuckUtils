using DuckGame;
using System;

namespace DuckGame.DuckUtils {

    [EditorGroup("duckutils")]
    [BaggedProperty("isInDemo", true)]
    [BaggedProperty("isFatal", true)]
    public class DubstepGun : Gun
    {
        private static readonly float[] LoopTimings = { 3.38f, 3.96f, 4.39f, 4.82f, 5.28f, 5.87f, 6.47f, 6.97f, 7.39f, 7.69f, 8.25f, 8.66f, 9.07f, 9.53f, 9.75f, 10f, 10.39f, 10.81f, 11.23f, 11.46f, 11.81f, 12.13f, 12.54f, 13.85f, 14.64f, 15.11f, 15.56f, 16.35f, 16.78f, 17.19f, 17.64f, 18.09f, 18.91f, 19.53f, 20.19f, 20.67f, 21.09f, 21.48f, 21.71f, 21.94f, 22.39f, 22.78f, 23.25f, 23.63f, 23.78f, 24.11f, 24.53f, 24.73f, 24.96f, 25.39f, 25.82f, 26.19f, 26.64f, 26.89f, 27.09f, 27.31f };
        
        public StateBinding PlayBinding { get; private set; }

        private SpriteMap sprite;

        private LoopData loop = new LoopData(LoopTimings);

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

        public DubstepGun(float xval, float yval)
            : base(xval, yval)
        {
            PlayBinding = new StateBinding("Playing");

            _bio = "Plays generic dubstep loop and destroys everything";
            _editorName = "Dubstep Gun";

            ammo = LoopTimings.Length + Rando.Int(-5, 5);
            _ammoType = new ATGrenade();
            _type = "gun";

            sprite = new SpriteMap(DuckUtils.GetAsset("weapons/dubstep_gun.png"), 16, 16);
            sprite.AddAnimation("anim", Maths.IncFrameTimer() * 4f, true, 0, 1, 2, 3);
            sprite.SetAnimation("anim");
            graphic = sprite;
            center = new Vec2(8f, 10f);

            sound = SFX.Get(DuckUtils.GetAsset("sounds/dubstep_sample.wav"), 1f, 0f, 0f, false);

            collisionOffset = new Vec2(-8f, -6f);
            collisionSize = new Vec2(16f, 11f);
            _barrelOffsetTL = new Vec2(16f, 6f);
            _fireSound = "pistolFire";
            _kickForce = 3f;
            _holdOffset = new Vec2(-1f, 0f);
            loseAccuracy = 0.1f;
            maxAccuracyLost = 1f;
            physicsMaterial = PhysicsMaterial.Metal;
        }

        public override void OnPressAction() {
            Playing = true;
        }

        public override void OnReleaseAction() {
            Playing = false;
            loop.Reset();
        }

        private void Explode() {
            Explosion.Create(this, position);
            Playing = false;
        }

        public override void OnHoldAction() {
            if(ammo == 0) {
                Explode();
                return;
            }

            switch(loop.Add(Maths.IncFrameTimer())) {
                case LoopDataResult.Fire: base.Fire(); break;
                case LoopDataResult.End: 
                    Explode();
                    break;
                default: break;
            }
        }
    }
}

