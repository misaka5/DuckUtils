using DuckGame;
using System;
using System.Collections.Generic;

namespace DuckGame.DuckUtils {

    [EditorGroup("duckutils|equipment")]
    public class ImStuffHat : AbstractHat
    {
        public static readonly ATProvider ExplosionShrapnel = ExplosionAT.From<ATShrapnel>().WithRange(120f, 150f).WithPenetration(99f);

        private Sound sound;

        public StateBinding PlayingBinding { get; private set; }
        public StateBinding TimerBinding { get; private set; }
        public StateBinding DisguiseTypeBinding { get; private set; }

        private bool _playing;
        public bool Playing {
            get {
                return _playing;
            }

            set {
                if(value != _playing) {
                    if(value) sound.Play();
                    else sound.Stop();
                    _playing = value;
                }
            }
        }

        public float Timer { get; set; }

        private PhysicsObject disguiseAs;
        private PhysicsObject DisguiseAs 
        {
            get {
                return disguiseAs;
            }
            
            set {
                if(disguiseAs != value) {
                    disguiseAs = value;
                    center = value.center;
                    collisionOffset = disguiseAs.collisionOffset;
                    collisionSize = disguiseAs.collisionSize;
                }
            }
        }

        private Type disguiseType;
        private Type DisguiseType
        {
            get {
                return disguiseType;
            }

            set {
                if(disguiseType != value) 
                {
                    disguiseType = value;
                    DisguiseAs = (PhysicsObject)Editor.CreateThing(value);
                }
            }
        }

        private ushort TypeId {
            get 
            {
                return Editor.IDToType[DisguiseType];
            }

            set {
                DisguiseType = Editor.IDToType[value];
            }
        }

        public ImStuffHat(float x, float y) : base(x, y) {
            PlayingBinding = new StateBinding("Playing");
            TimerBinding = new StateBinding("Timer");
            DisguiseTypeBinding = new StateBinding("TypeId");

            graphic = new Sprite(DuckUtils.GetAsset("hats/imstuff.png"));
            sound = SFX.Get(DuckUtils.GetAsset("sounds/imstuff.wav"), 0.8f, 0f, 0f, false);
        }

        private static Type GenerateDisguiseObjectType() 
        {
            List<Type> things = ItemBox.GetPhysicsObjects(Editor.Placeables);

            while(true) 
            {
                Type t = things[NetRand.Int(things.Count - 1)];

                if(t != typeof(LavaBarrel) && t != typeof(Grapple) && t != typeof(ImStuffHat))
                {
                    return t;
                }
            }
        }

        public override void Quack(float volume, float pitch) {}

        public override void Terminate() 
        {
            Playing = false;
        }

        public override void Update() 
        {
            if (netEquippedDuck != null) Playing = true;
            if (Playing) {
                Timer += Maths.IncFrameTimer();
                graphic = pickupSprite = _sprite = new SpriteMap(DuckUtils.GetAsset("hats/imstuff.png"), 129, 153);
                
                if(Timer > 2f) {
                    Explosion.Create(new ExplosionInfo(this) {
                        AmmoType = ExplosionShrapnel,
                        Bullets = 60
                    });

                    netEquippedDuck = null;
                    Level.Remove(this);
                    Playing = false;
                }
            }

            base.Update();
        }

        private bool drawingDisguised;
        public override void Draw() 
        {
            if(!Playing) 
            {
                if(isServerForObject && DisguiseAs == null) 
                {
                    DisguiseType = GenerateDisguiseObjectType();
                }

                if(!drawingDisguised && DisguiseAs != null) 
                {
                    drawingDisguised = true;
                    DisguiseAs.DrawAs(this, alpha, 0);
                    drawingDisguised = false;
                }
            } else {
                base.Draw();
            }
        }
    }
}