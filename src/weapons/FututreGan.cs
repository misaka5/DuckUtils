using DuckGame;
using System;

namespace DuckGame.DuckUtils {

    [EditorGroup("duckutils")]
    [BaggedProperty("isSuperWeapon", true)]
    [BaggedProperty("isFatal", true)]
    public class FututreGan : Gun
    {
        private static readonly int MaxAmmo = 4;
        private static readonly float CooldownDelay = 1.2f;

        public StateBinding CooldownBinding { get; private set; }

        private SpriteMap sprite; 

        private float cooldown;

        public bool CoolingDown {
            get {
                return cooldown > 0f;
            }
        }

        public FututreGan(float xval, float yval)
            : base(xval, yval)
        {
            CooldownBinding = new StateBinding("cooldown");

            _editorName = "Fututre Gan";
            _bio = "For Fututre Use";

            ammo = MaxAmmo;
            _type = "gun";
            graphic = sprite = new SpriteMap(DuckUtils.GetAsset("weapons/fututre_gan.png"), 16, 16);
            center = new Vec2(7f, 9f);
            collisionOffset = new Vec2(-7f, -4f);
            collisionSize = new Vec2(13f, 9f);
            _barrelOffsetTL = new Vec2(14f, 6f);
            _fullAuto = false;
            _fireWait = 0f;
            _kickForce = 0.5f;
            _holdOffset = new Vec2(0f, 0f);

            UpdateSprite();
        }

        private void UpdateSprite() {
            sprite.frame = Maths.Clamp(MaxAmmo - ammo, 0, MaxAmmo) + (CoolingDown ? 8 : 0);
        }

        public override void OnPressAction() {
            if(!CoolingDown) {
                Fire();
                cooldown = CooldownDelay;
            }
        }

        public override void Fire() {
            if (ammo > 0)
            {
                SFX.Play(DuckUtils.GetAsset("sounds/fututre_gan.wav"));
                Vec2 vec = Offset(barrelOffset);
                if (isServerForObject)
                {
                    FututreSphere sphere = new FututreSphere(vec.x, vec.y, owner, barrelVector);
                    Fondle(sphere);

                    sphere.killThingType = GetType();
                    Level.Add(sphere);
                    
                    if (owner != null)
                    {
                        owner.hSpeed = -barrelVector.x * 8f;
                        owner.vSpeed = -barrelVector.y * 4f - 2f;
                    } else {
                        hSpeed = -barrelVector.x * 8f;
                        vSpeed = -barrelVector.y * 4f - 2f;
                    }
                }
                ammo--;
            }
        }

        public override void Update()
        {
            base.Update();
            UpdateSprite();

            if(CoolingDown) {
                cooldown -= Maths.IncFrameTimer();
            }
        }
    }
}