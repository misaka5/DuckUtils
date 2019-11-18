using DuckGame;
using System;
using System.Collections.Generic;

namespace DuckGame.DuckUtils
{
    [EditorGroup("duckutils|explosives")]
    [BaggedProperty("isInDemo", true)]
    [BaggedProperty("isGrenade", true)]
    [BaggedProperty("isFatal", true)]
    public class OniichanGrenade : GrenadeBase
    {
        private static readonly float FirstExplosion = 0.6f;
        private static readonly float SecondExplosion = 1.2f;

        private bool firstExplosion = false;
        private bool wasSoundPlayed = false;

        public OniichanGrenade(float x, float y) : base(x, y) {
            _editorName = "Onii-chan Grenade";
            _bio = "Onii-chan~ onii-chan~~";

            Timer = SecondExplosion;

            graphic = new SpriteMap(DuckUtils.GetAsset("weapons/oniichan_grenade.png"), 16, 16);
            center = new Vec2(8f, 8f);
            collisionOffset = new Vec2(-3f, -6f);
            collisionSize = new Vec2(7f, 13f);
            bouncy = 0.5f;
            friction = 0.04f;
        }

        protected override void CreateExplosion(Vec2 pos) {
            Explosion.Create(this, pos);
        }

        public override void Update() {
            base.Update();

            if(!wasSoundPlayed && !HasPin) {
                wasSoundPlayed = true;
                //play sound
                SFX.Play(DuckUtils.GetAsset("sounds/oniichan.wav"), 1f, 0f, 0f, false);
            }

            float timePassed = SecondExplosion - Timer;
            if(!firstExplosion && timePassed >= FirstExplosion) {
                firstExplosion = true;
                //call first explosion, second explosion will be called by base code
                CreateExplosion(position);
            }
        }
    }
}