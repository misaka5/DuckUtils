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
    }
}