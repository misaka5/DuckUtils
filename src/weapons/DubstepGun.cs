using DuckGame;
using System;

//author: zumaster
namespace DuckGame.DuckUtils {

    [EditorGroup("duckutils")]
	[BaggedProperty("isInDemo", true)]
    [BaggedProperty("isFatal", true)]
    public class DubstepGun : Gun
    {
        public readonly SpriteMap spriteMap;

		public StateBinding ActiveBinding { get; private set; }
		
        private bool playing = false;
        public bool Active {
            get {
                return playing;
            }

            set {
                if(value != playing) {
                    if(value) sound.Play();
                    else sound.Stop();
                    playing = value;
                }
            }
        }

		private Sound sound;
        
        public DubstepGun(float xval, float yval)
            : base(xval, yval)
        {
			ammo = 60 + Rando.Int(-50, 10);
			_ammoType = new ATMissile();

			_type = "gun";
			_editorName = "Dubstep Gun";
            _bio = "Plays generic dubstep loop and destroys everything";

			graphic = spriteMap = new SpriteMap(DuckUtils.GetAsset("weapons/dubstep_gun.png"), 32, 16);
			spriteMap.AddAnimation("active", 0.1f, true, 1, 0, 2, 0, 3, 0, 4, 0);
			spriteMap.AddAnimation("idle", 0.1f, true, 0);

			center = new Vec2(8f, 4f);
			collisionOffset = new Vec2(-8f, 0f);
			collisionSize = new Vec2(16f, 10f);
			_barrelOffsetTL = new Vec2(20f, 8f);
			_fullAuto = true;
			_fireWait = 1.75f;
			_kickForce = 0.6f;
			_holdOffset = new Vec2(0f, -5f);
            _fireSound = "";

			sound = SFX.Get(DuckUtils.GetAsset("sounds/dubstep_sample.wav"), 1f, 0f, 0f, true);
        
            ActiveBinding = new StateBinding("Active");
        }

		public override void OnPressAction()
		{
			Active = true;
		}

		public override void OnReleaseAction()
		{
			Active = false;
		}
        
		public override void Update() 
        {
			base.Update();

            spriteMap.SetAnimation(Active ? "active" : "idle");

			if (ammo == 0)
			{
                Active = false;
				Explosion.Create(this, position);
				Level.Remove(this);
			}
		}
    }
}
