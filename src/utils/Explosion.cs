using System;
using DuckGame;

namespace DuckGame.DuckUtils {

    public delegate AmmoType ATProvider();
    
    public static class ExplosionAT {

        public static readonly ATProvider Default = 
            () => {
                AmmoType type = new ATShrapnel();
                type.range = Rando.Float(50f, 76f);
                return type;
            };

        public static ATProvider From<T>() where T : AmmoType, new() {
            return () => new T();
        }

        public static ATProvider WithRange(this ATProvider provider, float range) {
            if(provider == null) throw new ArgumentNullException("provider");
            return () => {
                AmmoType type = provider.Invoke();
                type.range = range;
                return type;
            };
        }

         public static ATProvider WithRange(this ATProvider provider, float rangeMin, float rangeMax) {
            if(provider == null) throw new ArgumentNullException("provider");
            return () => {
                AmmoType type = provider.Invoke();
                type.range = Rando.Float(rangeMin, rangeMax);
                return type;
            };
        }

        public static ATProvider WithPenetration(this ATProvider provider, float p) {
            if(provider == null) throw new ArgumentNullException("provider");
            return () => {
                AmmoType type = provider.Invoke();
                type.penetration = p;
                return type;
            };
        }
    }

    public struct ExplosionInfo {
        public Thing Owner { get; set; }
        public Vec2 Position { get; set; }
        public ATProvider AmmoType { get; set; }
        public string Sound { get; set; }
        public bool Flash { get; set; }
        public int Bullets { get; set; }

        public ExplosionInfo(Thing owner) : this() {
            Owner = owner;
            Position = owner.position;
            AmmoType = ExplosionAT.Default;
            Sound = "explode";
            Flash = true;
            Bullets = 20;
        }
    }

    public static class Explosion {
        public static void Create(ExplosionInfo info) {
            if(info.Flash) Graphics.FlashScreen();
            float x = info.Position.x;
            float y = info.Position.y - 2f;
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

            float f = 360f / info.Bullets;
            for (int i = 0; i < info.Bullets; i++)
            {
                float deg = (float)i * f - 5f + Rando.Float(10f);
                float rad = Maths.DegToRad(deg);

                float xDir = (float)Math.Cos(rad);
                float yDir = (float)Math.Sin(rad);

                Bullet bullet = new Bullet(x + xDir * 6, y - yDir * 6, info.AmmoType.Invoke(), deg);
                bullet.firedFrom = info.Owner;
                Level.Add(bullet);
            }

            SFX.Play(info.Sound);
        }
        
        public static void Create(Thing owner, Vec2 pos, ATProvider provider) {
            Create(new ExplosionInfo(owner) {
                Position = pos,
                AmmoType = provider ?? ExplosionAT.Default
            });
        }

        public static void Create(Thing owner, ATProvider provider) {
            Create(new ExplosionInfo(owner) {
                AmmoType = provider ?? ExplosionAT.Default
            });
        }

        public static void Create(Thing owner, Vec2 pos) {
            Create(new ExplosionInfo(owner) {
                Position = pos
            });
        }

        public static void Create(Thing owner) {
            Create(new ExplosionInfo(owner));
        }
    }
}