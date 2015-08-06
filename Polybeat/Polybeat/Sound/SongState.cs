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

        public MelodyChange Change;

        public SongState(bool isBeat, MelodyChange change, bool isOnset)
        {
            IsBeat = isBeat;
            Change = change;
            IsOnset = isOnset;
        }
    }

    /// <summary>
    /// Muutos melodiassa.
    /// None = ei uusia nuotteja
    /// SameNote = sama nuotti uudestaan
    /// HigherNote = korkeampi nuotti
    /// LowerNote = matalampi nuotti
    /// </summary>
    public enum MelodyChange { None, SameNote, HigherNote, LowerNote }
}
