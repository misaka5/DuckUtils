using System;
using DuckGame;

namespace DuckGame.DuckUtils {

    [EditorGroup("duckutils")]
    public class FrogLauncher : Gun
    {
        private static readonly float CooldownDelay = 0.6f;
        private static readonly int MaxAmmo = 16;
        private static readonly int LeftOverThreshold = 4;

        public StateBinding AimAngleBinding { get; private set; }
        public StateBinding StateBinding { get; private set; }

        private byte stateIndex {
            get {
                return (byte)state;
            }

            set {
                state = (State)value;
            }
        }

        private State state = State.Idle;

        private float aimAngle;

        private SpriteMap baseSprite;
        private SpriteMap ammoSprite;

        public override float angle
        {
            get
            {
                return (float)Math.Min(base.angle - Maths.DegToRad(aimAngle) * offDir, Math.PI / 2);
            }
            set
            {
                _angle = value;
            }
        }

        public FrogLauncher(float xval, float yval)
            : base(xval, yval)
        {
            AimAngleBinding = new StateBinding("aimAngle");
            StateBinding = new StateBinding("stateIndex");

            _editorName = "Frog Launcher";
            _bio = "This boy can fit " + MaxAmmo + " frogs innit!";

            ammo = MaxAmmo;
            _type = "gun";
            graphic = baseSprite = new SpriteMap(DuckUtils.GetAsset("weapons/frog_launcher_base.png"), 24, 12);
            ammoSprite = new SpriteMap(DuckUtils.GetAsset("weapons/frog_launcher_ammo.png"), 26, 12);
            
            for(int i = 1; i <= 4; i++) {
                int rowStartIdx = (5 - i) * 4;
                ammoSprite.AddAnimation("idle" + i, 0.4f, false, rowStartIdx + 0);
                ammoSprite.AddAnimation("load" + i, 0.4f, false, rowStartIdx + 1, rowStartIdx + 2, rowStartIdx + 3);
            }

            ammoSprite.AddAnimation("empty", 0.4f, false, 21);
            ammoSprite.AddAnimation("idleOver", 0.4f, false, 0);
            ammoSprite.AddAnimation("loadOver", 0.4f, false, 1, 2, 3, 20);
            ammoSprite.SetAnimation("empty");

            center = new Vec2(6f, 7f);
            collisionOffset = new Vec2(-6f, -4f);
            collisionSize = new Vec2(16f, 8f);
            _barrelOffsetTL = new Vec2(25f, 4f);
            _laserOffsetTL = new Vec2(22f, 4f);
            _fireSound = "pistol";
            _kickForce = 3f;
            _holdOffset = new Vec2(-3f, 0f);
            _ammoType = new ATGrenade();

            UpdateSprites();
        }

        private void UpdateSprites() {
            if(state == State.Empty) {
                baseSprite.frame = 0;
                ammoSprite.SetAnimation("empty");
            } else {
                string id = state == State.Idle ? "idle" : "load";
                if(ammo <= LeftOverThreshold) {
                    baseSprite.frame = 0;
                    ammoSprite.SetAnimation(id + ammo);
                } else {
                    float fill = (ammo - LeftOverThreshold) / (float)(MaxAmmo - LeftOverThreshold);
                    baseSprite.frame = Maths.Clamp((int)Math.Floor(1 + 4 * fill), 0, 5);
                    ammoSprite.SetAnimation(id + "Over");
                }
            }
        }

        public override void OnPressAction()
        {
            switch(state) {
                case State.Idle:
                    state = State.Loaded;
                    laserSight = true;
                    UpdateSprites();
                    break;
                case State.Loaded: 
                    state = State.Aiming;
                    break;
                case State.Empty:
                    SFX.Play("click");
                    break;
            }
        }

        public override void OnHoldAction() {
            if(state == State.Aiming) {
                if(aimAngle < 90f) {
                    aimAngle += Maths.IncFrameTimer() * 90;
                }
            }
        }

        public override void OnReleaseAction()
        {
            if (state == State.Aiming)
            {
                Fire();

                kick = 1f;
                ammo--;
                if(ammo > 0) {
                    state = State.Idle;
                } else {
                    state = State.Empty;
                }

                UpdateSprites();

                laserSight = false;
                aimAngle = 0;
            }
        }

        public override void Fire() {
            SFX.Play(DuckUtils.GetAsset("sounds/pop.wav"));

            Vec2 barrel = Offset(barrelOffset);
            float radians = barrelAngle + Rando.Float(-0.1f, 0.1f);
            Vec2 velocity = Maths.AngleToVec(radians) * 10f;
            
            Frog frog = new Frog(barrel.x, barrel.y, velocity.x > 0);
            Fondle(frog);
            frog.hSpeed = velocity.x;
            frog.vSpeed = velocity.y;
            Level.Add(frog);
        }

        public override void Draw() {
            base.Draw();
            Draw(ammoSprite, new Vec2(-5f, -9f), 1);
        }

        public enum State : byte {
            Idle,
            Loaded,
            Aiming,
            Empty
        }
    }
}