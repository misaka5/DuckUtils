using DuckGame;
using System;
using System.Collections.Generic;

namespace DuckGame.DuckUtils
{
    [EditorGroup("duckutils|explosives")]
    [BaggedProperty("isInDemo", true)]
    [BaggedProperty("isGrenade", true)]
    public class TimeFreezeGrenade : GrenadeBase
    {
        public TimeFreezeGrenade(float x, float y) : base(x, y) {
            _editorName = "Freeze Grenade";
            _bio = "Whoosh!";

            Timer = 2f;

            graphic = new SpriteMap(DuckUtils.GetAsset("weapons/timefreeze_grenade.png"), 16, 16);
            center = new Vec2(8f, 8f);
            collisionOffset = new Vec2(-3f, -6f);
            collisionSize = new Vec2(7f, 13f);
            bouncy = 0.5f;
            friction = 0.04f;
        }

        protected override void CreateExplosion(Vec2 pos) {

            if (isServerForObject)
            {
                float x = pos.x;
                float y = pos.y - 2f;
                Level.Add(new TimeFreezeParticle(x, y));
            }
        }
    }
}