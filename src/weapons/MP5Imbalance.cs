using DuckGame;
using System;

//author: zumaster
namespace DuckGame.DuckUtils {

    [EditorGroup("duckutils")]
	[BaggedProperty("isInDemo", true)]
    [BaggedProperty("isSuperWeapon", true)]
    [BaggedProperty("isFatal", true)]
    public class MP5Imbalance : Gun
    {
        public static readonly int MaxAmmo = 99;
        public readonly SpriteMap spriteMap;

		private NetSoundEffect storeSound;
		public StateBinding SoundBinding { get; private set; }
        
        public MP5Imbalance(float xval, float yval)
            : base(xval, yval)
        {
			ammo = MaxAmmo;
			_ammoType = new ATMissile();
			_ammoType.penetration = 2f;
			_ammoType.range = 200f;
			_ammoType.accuracy = 1f;

			_type = "gun";
			_editorName = "MP5";
			graphic = new SpriteMap(DuckUtils.GetAsset("weapons/mp5_imba.png"), 24, 14);

			center = new Vec2(8f, 4f);
			collisionOffset = new Vec2(-8f, -4f);
			collisionSize = new Vec2(20f, 10f);

			_barrelOffsetTL = new Vec2(23f, 5f);
			_fireSound = DuckUtils.GetAsset("sounds/mp5_imba_shot.wav");
			_fullAuto = true;
			_fireWait = 0f;
			_kickForce = 0.4f;
			_holdOffset = new Vec2(-5f, -3f);

			storeSound = new NetSoundEffect(DuckUtils.GetAsset("sounds/mp5_imba_store.wav"));
			SoundBinding = new NetSoundBinding("storeSound");
        }

		public override void Update() {
			base.Update();

			ammo = MaxAmmo;
			if(duck != null && isServerForObject) {
				StoredItem stored = PurpleBlock.GetStoredItem(duck.profile);

				if(stored.type == GetType()) {
					storeSound.Play();
					duck.onFire = true;
					duck.Zap(this);

					stored.sprite = null;
					stored.type = null;
					stored.thing = null;
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
                    SFX.Play(_fireSound, 1f, 0f);
                }
            }
        }
    }
}