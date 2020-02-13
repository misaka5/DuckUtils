using DuckGame;
using System;

//author: callbuster
//TODO refactor
namespace DuckGame.DuckUtils {

    [EditorGroup("duckutils")]
	[BaggedProperty("isInDemo", true)]
    [BaggedProperty("isFatal", true)]
    public class DubstepGun : Gun
    {
        public readonly SpriteMap spriteMap;

		public readonly float Delay = 1.65f;
		public float Counter = 0;
		public float ShotDelay  = 0.3f;

		public bool Pressed = false;
		public bool PreSound = true; // platinum-kostil;

		Sound presound = SFX.Get(DuckUtils.GetAsset("sounds/dubstep_sample2.wav"), 1f, 0f, 0f, false);

		public StateBinding ActiveBinding { get; private set; }
		
        private bool playing = false;

		public override void PressAction()
		{
			Pressed = true;
		}

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
			_kickForce = 0.8f;
			_holdOffset = new Vec2(0f, -5f);
            _fireSound = "";

			sound = SFX.Get(DuckUtils.GetAsset("sounds/dubstep_sample.wav"), 1f, 0f, 0f, true);
        
            ActiveBinding = new StateBinding("Active");
        }
        
		public override void Update() 
        {
			base.Update();
            spriteMap.SetAnimation(Active ? "active" : "idle");

			if (Pressed)
			{
				Counter += Maths.IncFrameTimer();

				if (!Active && PreSound)
				{
					presound.Play();
					PreSound = false;
				}

				if (Counter >= Delay)
				{
					Active = true;
					Counter = 0;
				}

				if (Active && Counter <= ShotDelay)
				{
					Fire();
					Counter = 0;
				}
			}

			if (ammo == 0)
			{
                Active = false;
				Explosion.Create(this);
				Level.Remove(this);
			}
		}
    }
}