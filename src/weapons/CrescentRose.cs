/*using DuckGame;
using System;

namespace DuckGame.DuckUtils
{
    //https://media.discordapp.net/attachments/295148076145049602/674532287731204106/7f156b4.jpg
    [EditorGroup("duckutils")]
    [BaggedProperty("isFatal", true)]
    public class CrescentRose : Gun
    {
        private static readonly float AnimationTimeRifle2Melee = 0.2f;
        private static readonly float AnimationTimeMelee2Rifle = 0.2f;
        private static readonly float AnimationTimeLocked2Melee = 0.2f;
        private static readonly float AnimationTimeLocked2Rifle = 0.2f;
        private static readonly float AnimationTimeRifle2Locked = 0.2f;
        private static readonly float AnimationTimeMelee2Locked = 0.2f;

        private SpriteMap spriteMap;

        private bool inMeleeMode = false;
        private bool isLocked = true;

        private bool InMeleeMode
        {
            get
            {
                return inMeleeMode;
            }

            set
            {
                if (inMeleeMode != value)
                {
                    if (!IsLocked)
                    {
                        if (value)
                        {
                            AnimationTimer = AnimationTimeRifle2Melee;
                            spriteMap.SetAnimation("rifle2melee");
                        }
                        else
                        {
                            AnimationTimer = AnimationTimeMelee2Rifle;
                            spriteMap.SetAnimation("melee2rifle");
                        }
                    }

                    inMeleeMode = value;
                }
            }
        }

        private bool IsLocked
        {
            get
            {
                return isLocked;
            }

            set
            {
                if (isLocked != value)
                {
                    if (value && InMeleeMode)
                    {
                        AnimationTimer = AnimationTimeMelee2Locked;
                        spriteMap.SetAnimation("melee2locked");
                    }
                    else if (value && !InMeleeMode)
                    {
                        AnimationTimer = AnimationTimeRifle2Locked;
                        spriteMap.SetAnimation("rifle2locked");
                    }
                    else if (!value && InMeleeMode)
                    {
                        AnimationTimer = AnimationTimeLocked2Melee;
                        spriteMap.SetAnimation("locked2melee");
                    }
                    else
                    {
                        AnimationTimer = AnimationTimeLocked2Rifle;
                        spriteMap.SetAnimation("locked2rifle");
                    }

                    isLocked = value;
                }
            }
        }

        private float AnimationTimer { get; set; }
        private bool Animating
        {
            get
            {
                return AnimationTimer > 0f;
            }
        }

        #region Rifle Mode

        public static readonly float ReloadingDuration = 0.4f;

        private float loadTimer = ReloadingDuration;
        private bool reloading = false;

        private bool Reloading
        {
            get
            {
                return reloading;
            }

            set
            {
                reloading = value;
                if (value) loadTimer = 0f;
            }
        }

        private bool Loaded
        {
            get
            {
                return !Reloading && loadTimer >= ReloadingDuration;
            }
        }

        private bool Unloaded
        {
            get
            {
                return !Reloading && loadTimer <= 0;
            }
        }

        private void OnPressActionInRifleMode()
        {
            if (Loaded)
            {
                Fire();
                loadTimer = 0f;
            }
            else
            {
                if (Unloaded && ammo > 1)
                {
                    SFX.Play(DuckUtils.GetAsset("sounds/crescent_reload.wav"));
                    Reloading = true;
                }
                else if (ammo <= 1) DoAmmoClick();
            }
        }

        public override void Fire()
        {
            base.Fire();
            Recoil();
        }

        private void Recoil()
        {
            MaterialThing o = owner as MaterialThing;

            if (o != null)
            {
                float mult = o.grounded ? 0.5f : 1f;
                owner.hSpeed = mult * (-barrelVector.x * 8f);
                owner.vSpeed = mult * (-barrelVector.y * 8f - 2f);
            }
            else
            {
                hSpeed = -barrelVector.x * 8f;
                vSpeed = -barrelVector.y * 8f - 2f;
            }
        }

        #endregion

        #region Melee Mode 

        #endregion

        public CrescentRose(float xval, float yval)
            : base(xval, yval)
        {
            ammo = 11;
            _ammoType = new ATHighCalSniper();

            _type = "gun";
            _editorName = "Crescent Rose";
            _bio = "rwby";

            graphic = spriteMap = new SpriteMap(DuckUtils.GetAsset("weapons/crescent_rose.png"), 64, 51);

            spriteMap.AddAnimation("locked", 0.1f, false, 6);
			spriteMap.AddAnimation("locked2melee", 0.3f, false, 6, 5, 4, 3, 2, 1, 0);
			spriteMap.AddAnimation("locked2rifle", 0.3f, false, 6, 5, 4, 3);
			spriteMap.AddAnimation("rifle2melee", 0.3f, false, 3, 2, 1, 0);
			spriteMap.AddAnimation("melee2locked", 0.3f, false, 0, 1, 2, 3, 4, 5, 6);
			spriteMap.AddAnimation("rifle2locked", 0.3f, false, 3, 4, 5, 6);
			spriteMap.AddAnimation("melee2rifle", 0.3f, false, 0, 1, 2, 3);

			spriteMap.SetAnimation("locked");

            center = new Vec2(48f, 12f);
            collisionOffset = new Vec2(-8f, 0f);
            collisionSize = new Vec2(16f, 10f);
            _barrelOffsetTL = new Vec2(8f, 12f);
            _holdOffset = new Vec2(0f, -5f);
            _fireSound = DuckUtils.GetAsset("sounds/crescent_shot.wav");

            _fullAuto = false;
            _fireWait = 0.5f;
            _kickForce = 0f;

            IsLocked = true;
            InMeleeMode = false;
        }

        public override void OnPressAction()
        {
            if (!Animating && !IsLocked)
            {
                if (!InMeleeMode) OnPressActionInRifleMode();
            }
        }

        public override void OnHoldAction()
        {
			
        }

        public override void OnReleaseAction()
        {

        }

        public override void Update()
        {
            laserSight = !IsLocked && !InMeleeMode;

            if (Animating)
            {
                AnimationTimer -= Maths.IncFrameTimer();
            }
            else
            {
                IsLocked = owner == null;
                if (!IsLocked)
                {
                    if (InMeleeMode)
                    {
                        if (duck.IsKeyPressed("QUACK") && ammo > 1) InMeleeMode = false;
                    }
                    else
                    {
                        if (duck.IsKeyPressed("QUACK") || ammo <= 1) InMeleeMode = true;

                        if (Reloading)
                        {
                            loadTimer += Maths.IncFrameTimer();
                            if (loadTimer >= ReloadingDuration)
                                Reloading = false;
                        }
                    }
                }
            }

            base.Update();
        }

        public override void Draw()
        {
            base.Draw();
        }
    }
}*/