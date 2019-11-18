using DuckGame;
using System;
using System.Collections.Generic;

namespace DuckGame.DuckUtils
{
    public abstract class GrenadeBase : Gun
    {
        public StateBinding TimerBinding { get; private set; }
	    public StateBinding PinBinding { get; private set; }

        private bool _pin = true;
        public bool HasPin {
            get {
                return _pin;
            }

            set {
                if(_pin != value) {
                    if(graphic is SpriteMap) (graphic as SpriteMap).frame = value ? 0 : 1;
                    if(!value) CreatePinParticle();
                    _pin = value;
                }
            }
        }

        private float _timer = 0;
        public float Timer {
            get {
                return _timer;
            }

            set {
                _timer = value;
            }
        }

        private bool lastExplosionCreated;
        public bool DidBonus { get; private set; }

        public GrenadeBase(float xval, float yval)
            : base(xval, yval)
        {
            TimerBinding = new StateBinding("Timer");
            PinBinding = new StateBinding("HasPin");
            
            ammo = 1;
            _type = "gun";
            _ammoType = new ATShrapnel();
            _ammoType.penetration = 0.2f;
        }

        public override void OnNetworkBulletsFired(Vec2 pos)
        {
            CallLastExplosion(pos);
        }

        protected void CallLastExplosion(Vec2 pos) {
            if(!lastExplosionCreated) {
                lastExplosionCreated = true;
                CreateExplosion(pos);
                
                Level.Remove(this);
            }
        }

        protected abstract void CreateExplosion(Vec2 pos);

        public override void Update()
        {
            base.Update();
            if (!HasPin)
            {
                Timer -= Maths.IncFrameTimer();
            }

            if (Timer < 0.5f && owner == null && !DidBonus)
            {
                DidBonus = true;
                Recorder.LogBonus();
            }
            
            if (Timer < 0f)
            {
                CallLastExplosion(position);
            }
        }

        protected virtual void CreatePinParticle() {
            Thing grenadePin = new GrenadePin(x, y);
            grenadePin.hSpeed = (float)(-offDir) * (1.5f + Rando.Float(0.5f));
            grenadePin.vSpeed = -2f;
            Level.Add(grenadePin);
            SFX.Play("pullPin");
        }

        public override void OnPressAction() {
            HasPin = false;
        }

        public override void OnSolidImpact(MaterialThing with, ImpactedFrom from)
        {
            Vec2 collisionVelocity = new Vec2(with.hSpeed - this.hSpeed, with.vSpeed - this.vSpeed);
            float lengthSq = collisionVelocity.lengthSq;
            if(lengthSq > 8.5f * 8.5f) {
                HasPin = false;
            }

            base.OnSolidImpact(with, from);
        }
    }
}
