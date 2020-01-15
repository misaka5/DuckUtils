using System;
using DuckGame;

namespace DuckGame.DuckUtils {

    public static class Extensions {
        
        public static Duck ToDuck(this IAmADuck rc, bool aliveOnly = true) {
            if(rc == null) return null;
            
            Duck duck = null;
            if(rc is TrappedDuck) duck = ((TrappedDuck)rc).captureDuck;
            else if(rc is RagdollPart) duck = ((RagdollPart)rc).doll.captureDuck;
            else if(rc is Ragdoll) duck = ((Ragdoll)rc).captureDuck;
            else if(rc is Duck) duck = (Duck)rc;

            if(aliveOnly && duck.dead)
                return null;
            return duck;
        }

        public static bool IsKeyPressed(this Duck duck, string key) {
            return duck != null && 
                   duck.inputProfile != null && 
                   duck.inputProfile.Pressed(key, false);
        }

        #region DrawAs

        public static void DrawAs(this Thing spr, Thing target) {
            Vec2 cpos = spr.position;
            float calpha = spr.alpha;
            float cangle = spr.angle;
            Depth cdepth = spr.depth;
            bool cfliph = spr.graphic == null ? false : spr.graphic.flipH;
            sbyte coffdir = spr.offDir;
            Vec2 cscale = spr.scale;

            if(spr.graphic != null) spr.graphic.flipH = target.offDir < 0;
            spr.offDir = target.offDir;
            spr.scale = target.scale;
            spr.depth = target.depth;
            spr.angle = target.angle;
            spr.alpha = target.alpha;
            spr.position = target.position;

            spr.Draw();

            if(spr.graphic != null) spr.graphic.flipH = cfliph;
            spr.offDir = coffdir;
            spr.scale = cscale;
            spr.depth = cdepth;
            spr.angle = cangle;
            spr.alpha = calpha;
            spr.position = cpos;
        }

        public static void DrawAs(this Thing spr, Thing target, float alpha, int d = 0) {
            Vec2 cpos = spr.position;
            float calpha = spr.alpha;
            float cangle = spr.angle;
            Depth cdepth = spr.depth;
            bool cfliph = spr.graphic == null ? false : spr.graphic.flipH;
            sbyte coffdir = spr.offDir;
            Vec2 cscale = spr.scale;

            if(spr.graphic != null) spr.graphic.flipH = target.offDir < 0;
            spr.offDir = target.offDir;
            spr.scale = target.scale;
            spr.depth = target.depth + d;
            spr.angle = target.angle;
            spr.alpha = alpha;
            spr.position = target.position;

            spr.Draw();

            if(spr.graphic != null) spr.graphic.flipH = cfliph;
            spr.offDir = coffdir;
            spr.scale = cscale;
            spr.depth = cdepth;
            spr.angle = cangle;
            spr.alpha = calpha;
            spr.position = cpos;
        }

        #endregion
    }
}