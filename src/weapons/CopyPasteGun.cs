using DuckGame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace DuckGame.DuckUtils {
    
    [EditorGroup("duckutils")]
    public class CopyPasteGun : Gun
    {
        public static readonly float CopyingDuration = 0.1f;
        public static readonly float CopyRange = 200f;
        public static readonly float SwitchTime = 0.8f;

        public StateBinding ActiveBinding { get; private set; }
        public StateBinding SoundBinding { get; private set; }

        private float progress = 0;
        private NetSoundEffect sound;
        
        public bool Active { get; set; }

        public Holdable Selected { get; private set; }

        private Vec2 selectorPosition;

        private SpriteMap selector;

        private Holdable current;
        private Holdable next;
        private float time;

        public Holdable CurrentSprite {
            get {
                if(current != null && current.graphic != null) return current;
                return this;
            }
        }

        public Holdable NextSprite {
            get {
                if(next != null && next.graphic != null) return next;
                return this;
            }
        }

        public CopyPasteGun(float xval, float yval)
            : base(xval, yval)
        {
            ActiveBinding = new StateBinding("Active");

            _bio = "owo";
            _editorName = "Copy Paster";

            ammo = 1;

            graphic = new Sprite(DuckUtils.GetAsset("weapons/copypaste.png"));
            selector = new SpriteMap(DuckUtils.GetAsset("part/copypaste_selector.png"), 16, 16);

            selector.AddAnimation("spawn", 1f, false, 0, 1, 2, 3, 4, 5, 6, 7);
            selector.SetAnimation("spawn");

            _type = "gun";
            center = new Vec2(8f, 8f);
            collisionOffset = new Vec2(-8f, -8f);
            collisionSize = new Vec2(16f, 16f);
            _barrelOffsetTL = new Vec2(16f, 4f);
            //_fireSound = DuckUtils.GetAsset("sounds/copypaste_fire.wav");
            _kickForce = 0.2f;
            handOffset = new Vec2(0f, -1f);
            _holdOffset = new Vec2(2f, 0f);

            sound = new NetSoundEffect(_fireSound);
            SoundBinding = new NetSoundBinding("sound");
        }

        private void UpdateSprites() {
            time += Maths.IncFrameTimer() / SwitchTime;

            if(time >= 1) {
                current = next;
                next = Selected;
                time = 0;
            }
        }

        private static void CloneObjectParameters(Type type, object src, object dest) {
            foreach (Type tp in Editor.AllBaseTypes[type])
            {
                if (!tp.IsInterface)
                {
                    foreach (FieldInfo info in Editor.AllEditorFields[tp])
                    {
                        info.SetValue(dest, info.GetValue(src));
                    }
                }
            }
        }

        private static Holdable CreateCopy(Holdable h) {
            Holdable n = (Holdable)Editor.CreateThing(h.GetType()); 
            CloneObjectParameters(h.GetType(), h, n);
            return n;
        }

        private void Copy(Holdable h) {
            Holdable thing = CreateCopy(h);
            thing.position = position;

            Level.Add(thing);
            Level.Remove(this);

            if(owner != null) {
                if(owner is Duck) {
                    (owner as Duck).GiveHoldable(thing);
                } else if(owner is PhysicsObject) {
                    (owner as PhysicsObject).holdObject = thing;
                    thing.owner = owner;
                }
            }

            sound.Play();
        }

        public override void Update()
        {
            base.Update();
            
            if(Selected != null && Selected.removeFromLevel) Selected = null;

            if(Selected != null) {
                selectorPosition += (Selected.position - selectorPosition) * 0.5f;
                if((Selected.position - selectorPosition).lengthSq < 4f) selectorPosition = Selected.position;
            }

            Vec2 hit;
            Holdable h = Level.current.CollisionRay<Holdable>(barrelPosition, barrelPosition + barrelVector * CopyRange, this, out hit);
            if(h != null && !(h is RagdollPart)) {
                if(Selected != h) {
                    selector.SetAnimation("");
                    selector.SetAnimation("spawn");
                }

                Selected = h;
            }

            if (Active && isServerForObject) {
                progress += Maths.IncFrameTimer();
                if(progress > CopyingDuration) {
                    if(Selected != null) Copy(Selected);
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
            Active = true;
        }

        private void DrawSprite(Sprite spr, Vec2 pos, float alpha, int d = 1) {
            spr.flipH = graphic.flipH;
	        spr.angle = angle;
	        spr.alpha = alpha;
	        spr.depth = depth + d;
	        spr.scale = scale;
            spr.center = pos;

            Graphics.Draw(spr, x, y);
        }

        public override void Draw()
        {
            UpdateSprites();

            DrawSprite(CurrentSprite.graphic, CurrentSprite.center, 1 - time, 1);
            DrawSprite(NextSprite.graphic, NextSprite.center, time, 2);

            if(isServerForObject) {
                if (Selected != null && owner != null) {
                    Graphics.Draw(selector, selectorPosition.x - 8, selectorPosition.y - 8, depth + 1);
                }
            }
        }
    }
}