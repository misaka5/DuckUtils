using System;
using DuckGame;

namespace DuckGame.DuckUtils {

    public delegate AmmoType ATProvider();
    
    public static class Explosion {

        public static readonly ATProvider Default = () => {
            ATShrapnel shrapnel = new ATShrapnel();
            shrapnel.range = 50f + Rando.Float(26f);
            return shrapnel;
        };

        public struct Config {
            public static readonly Config Default = new Config();

            private ATProvider ammoType;
            private string sound;
            private bool flash;

            public ATProvider AmmoType {
                get {
                    return ammoType ?? Explosion.Default;
                }
            }
            public string Sound {
                get {
                    return sound ?? "explode";
                }
            }

            public bool Flash {
                get {
                    return flash;
                }
            }

            public Config(ATProvider at = null, string sound = null, bool flash = true) {
                this.ammoType = at;
                this.sound = sound;
                this.flash = flash;
            }
        }

        public static void Create(Thing owner, Vec2 pos, Config cfg) {
            if(cfg.Flash) Graphics.FlashScreen();
            float x = pos.x;
            float y = pos.y - 2f;
            Level.Add(new ExplosionPart(x, y));

            int amount = Rando.Int(1, 2);
            for (int i = 0; i < amount; i++)
            {
                float deg = (float)i * 360f / amount + Rando.Float(-10f, 10f);
                float rad = Maths.DegToRad(deg);
                float range = Rando.Float(12f, 20f);

                float xDir = (float)Math.Cos(rad);
                float yDir = (float)Math.Sin(rad);

                ExplosionPart thing = new ExplosionPart(x + xDir * range, y - yDir * range);
                Level.Add(thing);
            }

            for (int i = 0; i < 20; i++)
            {
                float deg = (float)i * 18f - 5f + Rando.Float(10f);
                float rad = Maths.DegToRad(deg);

                float xDir = (float)Math.Cos(rad);
                float yDir = (float)Math.Sin(rad);

                Bullet bullet = new Bullet(x + xDir * 6, y - yDir * 6, cfg.AmmoType.Invoke(), deg);
                bullet.firedFrom = owner;
                Level.Add(bullet);
            }

            SFX.Play(cfg.Sound);
        }
        
        public static void Create(Thing owner, Vec2 pos, ATProvider provider) {
            Create(owner, pos, new Config(provider));
        }

        public static void Create(Thing owner, Vec2 pos) {
            Create(owner, pos, Config.Default);
        }
    }
}