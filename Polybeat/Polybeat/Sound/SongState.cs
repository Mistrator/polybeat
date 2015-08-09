using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Polybeat
{
    public struct SongState
    {
        public bool IsBeat;

        public bool IsOnset;

        public SongState(bool isBeat, bool isOnset)
        {
            IsBeat = isBeat;
            IsOnset = isOnset;
        }
    }
}