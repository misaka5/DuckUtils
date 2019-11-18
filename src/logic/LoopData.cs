using System;
using DuckGame;

namespace DuckGame.DuckUtils {
    public class LoopData {
        
        private readonly float[] timings;
        private float current;
        private float next;
        private int index;

        public LoopData(float[] timings) {
            this.timings = timings;
            Reset();
        }

        public void Reset() {
            current = 0;
            index = 0;
            next = timings[0];
        }

        public LoopDataResult Add(float time) {
            current += time;
            if(current >= next) {
                index++;
                if(index >= timings.Length) { 
                    return LoopDataResult.End;
                }

                next = timings[index];
                return LoopDataResult.Fire;
            }

            return LoopDataResult.None;
        }
    }

    public enum LoopDataResult {
        None,
        Fire,
        End
    }
}