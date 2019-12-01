using DuckGame;
using System;

namespace DuckGame.DuckUtils {

    public abstract class AbstractHat : Hat
    {
        public float TimeOpened { get; private set;}
        public bool IsOpened { get; set; }
        private bool wasOpened = false;

        public AbstractHat(float x, float y) : base(x, y) {}

        public override void OpenHat() {}
	    public override void CloseHat() {}

        public override void Update() {
            if(duck != null) 
            {
                IsOpened = _sprite.frame != 0;
            }
            else if (heat < 1f) 
            {
                IsOpened = false;
            }

            if(IsOpened) 
            {
                if(!wasOpened) OpenHat();
                TimeOpened += Maths.IncFrameTimer();
                wasOpened = true;
            } 
            else 
            {
                if(wasOpened) CloseHat();
                TimeOpened = 0;
                wasOpened = false;
            }

            base.Update();
        }

        public override void HeatUp(Vec2 location)
        {
            if (duck == null)
            {
                heat += 0.2f;
                IsOpened = true;
            }
        }
    }
}