using DuckGame;
using System;

namespace DuckGame.DuckUtils {

    [EditorGroup("duckutils")]
    [BaggedProperty("isInDemo", true)]
    [BaggedProperty("isSuperWeapon", true)]
    [BaggedProperty("isFatal", true)]
    public class VoteGun : Gun
    {
        public static readonly int LoopTimingsAmount = 18;
        public static readonly float[] LoopTimings = BuildLoopTimings(1.3f, 0.48f, LoopTimingsAmount);

        private static float[] BuildLoopTimings(float start, float interval, int amt) {
           float[] timings = new float[amt];
           for(int i = 0; i < amt; i++) {
               timings[i] = start + interval * i;
           }

           return timings;
        }

        public StateBinding PlayBinding { get; private set; }

        public StateBinding SoundBinding { get; private set; }

        private SpriteMap sprite;

        private LoopData loop = new LoopData(LoopTimings);

        private Sound sound;
        private NetSoundEffect endSound;

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

        public VoteGun(float xval, float yval)
            : base(xval, yval)
        {
            PlayBinding = new StateBinding("Playing");

            _bio = "Golosovanie!";
            _editorName = "Vote Gun";

            ammo = LoopTimingsAmount;
            _ammoType = new ATGrenade();
            _type = "gun";

            sprite = new SpriteMap(DuckUtils.GetAsset("weapons/dubstep_gun.png"), 16, 16);
            sprite.AddAnimation("anim", Maths.IncFrameTimer() * 4f, true, 0, 1, 2, 3);
            sprite.SetAnimation("anim");
            graphic = sprite;
            center = new Vec2(8f, 10f);

            sound = SFX.Get(DuckUtils.GetAsset("sounds/vote_loop.wav"), 1f, 0f, 0f, false);

            collisionOffset = new Vec2(-8f, -6f);
            collisionSize = new Vec2(16f, 11f);
            _barrelOffsetTL = new Vec2(16f, 6f);
            _kickForce = 3f;
            _holdOffset = new Vec2(-1f, 0f);
            loseAccuracy = 0.1f;
            maxAccuracyLost = 2f;
            physicsMaterial = PhysicsMaterial.Metal;

            _ammoType = new ATShotgun();
            _ammoType.range = 100f;
            _type = "gun";

            _kickForce = 10f;
            _numBulletsPerFire = 7;

            endSound = new NetSoundEffect(DuckUtils.GetAsset("sounds/vote_end.wav"));
			SoundBinding = new NetSoundBinding("endSound");
        }

        public override void OnPressAction(){
            Playing = true;
        }

        public override void OnReleaseAction(){
            Playing = false;
            loop.Reset();
        }

        private void Explode() {
            Playing = false;
            endSound.Play();
            Explosion.Create(this, position);
            Level.Remove(this);    
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

