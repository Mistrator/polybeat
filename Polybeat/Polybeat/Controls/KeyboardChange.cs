using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Polybeat
{
    public enum KeyboardChange
    {
        Unchanged,
        SamePressed,
        Higher,
        Lower,
        BothHigherAndLower,
        NonePressed
    }
}
