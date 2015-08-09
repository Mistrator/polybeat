using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Polybeat
{
    public struct Onset
    {
        public float Time;
        public bool Clicked;

        public Onset(float time, bool clicked)
        {
            Time = time;
            Clicked = clicked;
        }
    }
}
