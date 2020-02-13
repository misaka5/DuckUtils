// CombatShotgun
using DuckGame;
using System;

namespace DuckGame.DuckUtils {
    
    [EditorGroup("duckutils")]
    [BaggedProperty("isFatal", true)]
    public class TTSShotgun : Gun
    {
        public static readonly int MaxAmmo = 6;
        public static readonly float ReloadingDuration = 0.16f;

        public StateBinding ReloadBinding { get; private set; }
        public StateBinding LoadProgressBinding { get; private set; }

        private float loadProgress = 1f;
        private bool reloading = false;

        private SpriteMap loaderSprite;
        private SpriteMap ammoSprite;

        public TTSShotgun(float xval, float yval)
            : base(xval, yval)
        {
            ReloadBinding = new StateBinding("reloading");
            LoadProgressBinding = new StateBinding("loadProgress");

            _bio = "pew pew";
            _editorName = "TTS Shotgun";

            ammo = MaxAmmo;
            _ammoType = new ATShotgun();
            _ammoType.range = 140f;
            _type = "gun";
            graphic = new Sprite("combatShotgun");
            center = new Vec2(16f, 16f);
            collisionOffset = new Vec2(-12f, -3f);
            collisionSize = new Vec2(24f, 9f);
            _barrelOffsetTL = new Vec2(29f, 15f);
            _fireSound = DuckUtils.GetAsset("sounds/tts_fire.wav");
            _clickSound = DuckUtils.GetAsset("sounds/tts_click.wav");
            _kickForce = 5f;
            _numBulletsPerFire = 7;
            _manualLoad = true;
            loaderSprite = new SpriteMap("combatShotgunLoader", 16, 16);
            loaderSprite.center = new Vec2(8f, 8f);
            ammoSprite = new SpriteMap("combatShotgunAmmo", 16, 16);
            ammoSprite.center = new Vec2(8f, 8f);
            handOffset = new Vec2(0f, 1f);
            _holdOffset = new Vec2(4f, 0f);
        }

        public override void Update()
        {
            ammoSprite.frame = Maths.Clamp(MaxAmmo - ammo, 0, MaxAmmo);
            base.Update();
            
            if (reloading)
            {
                if (loadProgress == 0f) {
                    SFX.Play(DuckUtils.GetAsset("sounds/tts_load.wav"));
                    Reload();
                }

                if (loadProgress < ReloadingDuration) {
                    loadProgress += Maths.IncFrameTimer();
                } else {
                    reloading = false;
                }
            }
        }

        public override void HeatUp(Vec2 location)
        {
            if (_ammoType != null && ammo > 0 && heat > 1f && Rando.Float(1f) > 0.5f)
            {
                heat -= 0.05f;
                PressAction();
                if (Rando.Float(1f) > 0.8f)
                {
                    SFX.Play(DuckUtils.GetAsset("sounds/tts_pop.wav"), Rando.Float(0.5f, 1f), Rando.Float(-1f, 1f));
                }
            }
        }

        public override void OnPressAction()
        {
            if (loadProgress >= ReloadingDuration || ammo <= 0)
            {
                base.OnPressAction();
                loadProgress = 0f;
                reloading = false;
            } else if(!reloading) {
                reloading = true;
            }
        }

        public override void Draw()
        {
            base.Draw();
            Vec2 vec = new Vec2(13f, -1f);
            float xReloadAnimOffset = (float)Math.Sin(loadProgress * 3.14f) * 3f;
            Draw(loaderSprite, new Vec2(vec.x - 12f - xReloadAnimOffset, vec.y + 4f));
            Draw(ammoSprite, new Vec2(-3f, -2f), 2);
        }
    }
}