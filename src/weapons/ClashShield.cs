using DuckGame;
using System;
using System.Collections.Generic;

namespace DuckGame.DuckUtils {
    
    [EditorGroup("duckutils")]
    [BaggedProperty("isFatal", true)]
    public class ClashShield : Gun
    {
        public static readonly int[] LightningPattern1 = new int[] { 5, 2, 12, 8, 14, 4, 10, 9, 11, 13, 7, 3, 6, 1, 15, 
                                                                     11, 13, 8, 7, 12, 5, 14, 2, 1, 6, 9, 4, 10, 3, 15 };
        
        public static readonly int[] LightningPattern2 = new int[] { 14, 13, 12, 3, 11, 6, 5, 2, 7, 4, 10, 8, 9, 1, 15,
                                                                     7, 1, 14, 9, 15, 13, 12, 10, 5, 6, 8, 2, 4, 3, 11 };

        public static readonly float SlowdownFactor = 0.3f;
        public static readonly float ZapCooldown = 0.5f;
        public static readonly float ZapRagdollizeThreshold = 3.2f;
        public static readonly float ZapKillThreshold = 3.6f;
        public static readonly float ShieldMaxCharge = 7f;
        public static readonly float ShieldLowCharge = 0.7f;
        public static readonly float ZapRange = 150f;

        public static readonly float TopCollisionVerticalEnergy = 1f;
        public static readonly float TopCollisionHorizontalEnergy = 1.8f;
        public static readonly float CollisionEnergy = 1.8f;

        public StateBinding ActiveBinding { get; private set; }
        public StateBinding LowChargeBinding { get; private set; }

        private SpriteMap spriteMap;
        private SpriteMap lightning1;
        private SpriteMap lightning2;

        private Sound sound;

        private bool shieldActive;
        private bool Active { 
            get {
                return shieldActive;
            }

            set {
                if(shieldActive != value) {
                    if(value) sound.Play();
                    else sound.Stop();
                    shieldActive = value;
                }
            }
        }

        private bool LowCharge { get; set; }

        private float charge = ShieldMaxCharge;
        private readonly IDictionary<Duck, ZapState> zapState = new Dictionary<Duck, ZapState>();

        public ClashShield(float xval, float yval)
            : base(xval, yval)
        {
            ActiveBinding = new StateBinding("Active");
            LowChargeBinding = new StateBinding("LowCharge");

            _editorName = "Clash Shield";

            graphic = spriteMap = new SpriteMap(DuckUtils.GetAsset("weapons/clash_shield.png"), 32, 32);
            lightning1 = new SpriteMap(DuckUtils.GetAsset("weapons/clash_shield_lightnings.png"), 12, 6);
            lightning2 = new SpriteMap(DuckUtils.GetAsset("weapons/clash_shield_lightnings.png"), 12, 6);

            depth = depth - 2;

            spriteMap.AddAnimation("idle", 0.4f, false, 0);
            spriteMap.AddAnimation("active", 0.2f, true, 1, 2, 3, 4, 5, 6, 7, 6, 5, 4, 3, 2);
            spriteMap.AddAnimation("low", 0.2f, true, 1, 0, 2, 0, 0, 3, 4, 0, 5, 0, 6, 0, 7, 0, 5, 0, 4, 0, 3, 0, 2);
            
            lightning1.AddAnimation("idle", 0.4f, false, 0);
            lightning1.AddAnimation("active", 0.2f, true, LightningPattern1);
            lightning1.AddAnimation("low", 0.2f, true, 15, 0, 12, 0, 0, 3, 7, 0, 1, 0, 9, 0, 5, 0, 10, 0, 9, 0, 4);

            lightning2.AddAnimation("idle", 0.4f, false, 0);
            lightning2.AddAnimation("active", 0.2f, true, LightningPattern2);
            lightning2.AddAnimation("low", 0.2f, true, 5, 0, 7, 0, 0, 1, 8, 0, 12, 0, 11, 0, 8, 0, 3, 0, 2, 0, 4);

            sound = SFX.Get(DuckUtils.GetAsset("sounds/clash_shield.wav"), 1f, 0f, 0f, true);

            ammo = 99;
            thickness = 8.9f;
            weight = 9f;
            center = new Vec2(16f, 18f);
            collisionOffset = new Vec2(-3f, -18f);
            collisionSize = new Vec2(16f, 27f);
            handOffset = new Vec2(0f, 1000000f);
            _holdOffset = new Vec2(-3f, 4f);
            _barrelOffsetTL = new Vec2(23f, 12f);
        }

        public override void OnImpact(MaterialThing with, ImpactedFrom from)
        {
            if(from == ImpactedFrom.Top) {
                float grav = with is PhysicsObject ? (with as PhysicsObject).currentGravity : 0.2f;

                with.vSpeed = -TopCollisionVerticalEnergy * grav;
                with.hSpeed = (offDir > 0 ? TopCollisionHorizontalEnergy : -TopCollisionHorizontalEnergy);

                if(with is Gun) (with as Gun).PressAction();
            }

            if(offDir > 0 && from == ImpactedFrom.Right) {
                with.hSpeed = CollisionEnergy;
                if(with is Gun) (with as Gun).PressAction();
            }

            if(offDir < 0 && from == ImpactedFrom.Left) {
                with.hSpeed = -CollisionEnergy;
                if(with is Gun) (with as Gun).PressAction();
            }

            base.OnImpact(with, from);
        }

        public override bool Hit(Bullet bullet, Vec2 hitPos)
        {
            Level.Add(MetalRebound.New(hitPos.x, hitPos.y, (bullet.travelDirNormalized.x > 0f) ? 1 : -1));
			hitPos -= bullet.travelDirNormalized;
			for (int i = 0; i < 3; i++) 
                Level.Add(Spark.New(hitPos.x, hitPos.y, bullet.travelDirNormalized));

            SFX.Play("ting");
            return thickness > bullet.ammo.penetration;
        }

        private ZapState GetZapState(Duck duck) {
            ZapState state;
            if(zapState.TryGetValue(duck, out state))
                return state;

            state = new ZapState();
            zapState[duck] = state;
            return state;
        }

        private void DoZapping() {
            foreach (KeyValuePair<Duck, ZapState> p in zapState) {
                p.Value.Cooldown();
            }

            Vec2 hit;
            IAmADuck duck = Level.current.CollisionRay<IAmADuck>(barrelPosition, barrelPosition + barrelVector * ZapRange, out hit);

            Duck current = duck.ToDuck();
            if(current != null) {
                ZapState state = GetZapState(current);

                switch(state.Increase()) {
                    case ZapResult.Slowdown: 
                        current.vSpeed *= SlowdownFactor;
                        current.hSpeed *= SlowdownFactor;
                        break;
                    case ZapResult.Zap:
                        if(!current.dead) current.Zap(this);
                        current.Swear();
                        break;
                    case ZapResult.Death:
                        current.Destroy(new DTIncinerate(this));
                        state.Clear();
                        break;
                }
            }
        }

        public override void Update()
        {
            angle = 0;
            if(duck != null) duck._disarmDisable = 2;

            if(Active) {
                if(charge <= 0) {
                    Active = false;
                } else {
                    charge -= Maths.IncFrameTimer();
                    DoZapping();
                }
            } else {
                if(charge < ShieldMaxCharge) {
                    charge += Maths.IncFrameTimer();
                }
            }

            string anim = Active ? charge > ShieldLowCharge ? "active" : "low" : "idle";
            
            spriteMap.SetAnimation(anim);
            lightning1.SetAnimation(anim);
            lightning2.SetAnimation(anim);

            base.Update();
        }

        public override void OnPressAction()
        {
            if(charge > ShieldLowCharge) {
                Active = true;
            }
        }

        public override void OnReleaseAction() 
        {
            Active = false;
        }
    
        public override void Draw()
        {
            base.Draw();

            if(Active) {
                Draw(lightning1, new Vec2(-2f, -13f));
                Draw(lightning2, new Vec2(-2f, -13f) + new Vec2(8f, -1f));
            }
        }

        public class ZapState {
            public float Time { get; private set; }

            public void Cooldown() {
                if(Time > 0) Time -= Maths.IncFrameTimer() * ZapCooldown;
            }

            public void Clear() {
                Time = 0;
            }

            public ZapResult Increase() {
                Time += Maths.IncFrameTimer() * (1 + ZapCooldown);

                if(Time > ZapKillThreshold) return ZapResult.Death;
                if(Time > ZapRagdollizeThreshold) return ZapResult.Zap;
                return ZapResult.Slowdown;
            }
        }

        public enum ZapResult {
            Slowdown,
            Zap,
            Death
        }
    }
}