using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Polybeat
{
    public struct IntPoint
    {
        public int First;
        public int Second;

        public IntPoint(int first, int second)
        {
            First = first;
            Second = second;
        }

        public static bool operator ==(IntPoint p1, IntPoint p2)
        {
            if (p1.First == p2.First && p1.Second == p2.Second)
                return true;
            return false;
        }

        public static bool operator !=(IntPoint p1, IntPoint p2)
        {
            return !(p1 == p2);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
    }
}