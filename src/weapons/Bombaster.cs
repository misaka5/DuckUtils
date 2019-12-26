using DuckGame;
using System;

//author: zumaster
namespace DuckGame.DuckUtils {

    [EditorGroup("duckutils")]
	[BaggedProperty("isInDemo", true)]
    [BaggedProperty("isFatal", true)]
    public class Bombaster : Gun
    {
        public readonly SpriteMap spriteMap;
        private Sound sound;
        public bool Active = false;
        public bool Count = false;
        public bool Explode = false;
        public float counter = 0f;

		public StateBinding ActiveBinding { get; private set; }

        public Bombaster(float x, float y)
        : base(x, y)
        {
			ammo = 1;
            _type = "gun";
			_editorName = "Bombaster";

            graphic = spriteMap = new SpriteMap(DuckUtils.GetAsset("weapons/bombaster.png"), 15, 25);
			sound = SFX.Get(DuckUtils.GetAsset("sounds/bombaster.wav"), 1f, 0f, 0f, true);

            center = new Vec2(8f, 4f);
			collisionOffset = new Vec2(-8f, -4f);
			collisionSize = new Vec2(12f, 20f);
            _holdOffset = new Vec2(-0f, -8f);
        }

        public override void OnPressAction()
        {
            Active = true;
        }

        public override void Update()
        {
            base.Update();
            if (Active)
            {
                Active = false;
                Count = true;
                sound.Play();
            }
            if (Count)
            {
                counter += Maths.IncFrameTimer();
            }
            if (counter >= 3)
            {
                sound.Stop();
                Explosion.Create(this);
                Level.Remove(this);
            }
        }
    }
}
