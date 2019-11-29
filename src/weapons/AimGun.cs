// CombatShotgun
using DuckGame;
using System;
using System.Collections;
using System.Collections.Generic;

namespace DuckGame.DuckUtils {
    
    [EditorGroup("duckutils")]
    [BaggedProperty("isFatal", true)]
    [BaggedProperty("isSuperWeapon", true)]
    public class AimGun : Gun
    {
        public static readonly int MaxAmmo = 8;
        public static readonly float ReloadingDuration = 0.3f;
        public static readonly float LaunchSpeed = 9f;

        public StateBinding LoadProgressBinding { get; private set; }
        public StateBinding ReloadBinding { get; private set; }

        private float loadProgress = ReloadingDuration;

        public bool Reloading { get; private set; }

        public bool Reloaded {
            get {
                return !Reloading && loadProgress > ReloadingDuration;
            }
        }

        public bool Unloaded {
            get {
                return !Reloading && !Reloaded;
            }
        }

        private Sprite loaderSprite;

        private IntegratedPath path = new IntegratedPath();

        public AimGun(float xval, float yval)
            : base(xval, yval)
        {
            LoadProgressBinding = new StateBinding("loadProgress");
            ReloadBinding = new StateBinding("Reloading");

            _bio = "uwu";
            _editorName = "Aim Gun";

            ammo = MaxAmmo;

            _type = "gun";
            graphic = new Sprite(DuckUtils.GetAsset("weapons/aimgun.png"));
            center = new Vec2(10f, 5f);
            collisionOffset = new Vec2(-10f, -5f);
            collisionSize = new Vec2(20f, 9f);
            _barrelOffsetTL = new Vec2(18f, 4f);
            _fireSound = DuckUtils.GetAsset("sounds/aimgun_fire.wav");
            _kickForce = 0.2f;
            handOffset = new Vec2(0f, -1f);
            _holdOffset = new Vec2(2f, 0f);
            _manualLoad = true;

            loaderSprite = new Sprite(DuckUtils.GetAsset("weapons/aimgun_loader.png"));
            loaderSprite.center = new Vec2(8f, 8f);
        }

        private void Launch() {
            SFX.Play(_fireSound);

            if(isServerForObject) {
                EnergySphere sphere = new EnergySphere(barrelPosition.x, barrelPosition.y, owner);
                Vec2 vel = barrelVector * LaunchSpeed;
                
                sphere.hSpeed = vel.x;
                sphere.vSpeed = vel.y;

                Level.Add(sphere);
            }
        }

        public override void Update()
        {
            base.Update();
            
            if (Reloaded)
            {
                path.Integrate(barrelPosition, barrelVector * LaunchSpeed, new Vec2(0, currentGravity));
                
                Duck d = (path.Target as IAmADuck).ToDuck();
                if(d != null && d != owner) {
                    Launch();
                    loadProgress = 0f;
                }
            
            } else if(Reloading) {
                if (loadProgress == 0f) {
                    SFX.Play("shotgunLoad");
                    Reload(false);
                }

                if (loadProgress < ReloadingDuration) {
                    loadProgress += Maths.IncFrameTimer();
                } else {
                    Reloading = false;
                }
            }
        }

        public override void HeatUp(Vec2 location)
        {
            if (_ammoType != null && ammo > 0 && heat > 1f && Rando.Float(1f) > 0.5f)
            {
                heat -= 0.05f;
                PressAction();
            }
        }

        public override void OnPressAction()
        {
            if (Unloaded && ammo > 0) {
                loadProgress = 0f;
                Reloading = true;
            }
        }

        private static void DrawStringShadowed(string str, Vec2 pos, Depth d = default(Depth), float scale = 1f) {
            Graphics.DrawString(str, pos, Color.Red, d + 1, null, scale);
            Graphics.DrawString(str, new Vec2(pos.x, pos.y + scale), Color.DarkRed, d, null, scale);
        }

        private static void DrawPath(IEnumerable<Vec2> path, Vec2 offset, Color c, float w = 1f, Depth d = default(Depth)) {
            Vec2? prev = null;
            foreach(Vec2 p in path) {
                if(prev == null) prev = p;
                else {
                    Vec2 pp = prev.Value;
                    Graphics.DrawLine(new Vec2(pp.x + offset.x, pp.y + offset.y), new Vec2(p.x + offset.x, p.y + offset.y), c, w, d);
                    prev = p;
                }
            }
        }

        public override void Draw()
        {
            base.Draw();

            float xReloadAnimOffset = (float)Math.Sin((loadProgress / ReloadingDuration) * 3.14f) * 3f;
            Draw(loaderSprite, new Vec2(7f - xReloadAnimOffset, 8f));

            if(Reloaded) {
                DrawPath(path, new Vec2(0, 1), Color.DarkRed, 1f, depth + 1);
                DrawPath(path, new Vec2(0, 0), Color.Red, 1f, depth + 2);

                if(path.Target != null) {
                    DrawStringShadowed(path.Target.editorName, new Vec2(x, y - 20), depth + 1, 0.8f);
                    DrawStringShadowed((int)(path.Length / 8) + "m", new Vec2(x, y - 12), depth + 1, 0.4f);
                }
            }
        }

        public class IntegratedPath : IEnumerable<Vec2> {
            
            private readonly List<Vec2> points = new List<Vec2>();

            public MaterialThing Target { get; private set; }
            public float Length { get; private set; }

            public void Clear() {
                points.Clear();
                Target = null;
                Length = 0;
            }

            public void Integrate(Vec2 start, Vec2 vel, Vec2 acc, float step = 0.8f) {
                Clear();

                points.Add(start);

                Vec2? prev = null;
                Vec2 p = new Vec2(start.x, start.y);
                Vec2 v = new Vec2(vel.x, vel.y);

                for(float i = 0; i < 50; i += step) {
                    v += acc * step;
                    p += v * step;

                    if(prev == null) prev = p;
                    else {
                        Vec2 pp = prev.Value;

                        Vec2 hit;
                        MaterialThing t = Level.current.CollisionRay<MaterialThing>(pp, p, out hit);

                        if(t != null && (t.thickness > 1f || t is IAmADuck)) {
                            Target = t;
                            Length += new Vec2(pp.x - hit.x, pp.y - hit.y).Length();
                            points.Add(hit);
                            return;
                        }

                        Length += new Vec2(pp.x - p.x, pp.y - p.y).Length();
                        prev = p;
                    }

                    points.Add(new Vec2(p.x, p.y));
                }
            }

            public IEnumerator<Vec2> GetEnumerator() {
                return points.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator() {
                return points.GetEnumerator();
            }
        }
    }
}