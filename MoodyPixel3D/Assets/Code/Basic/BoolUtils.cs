using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class BoolUtils
{
    public enum BoolJoin
    {
        Or,
        And,
        Xor,
    }

    public static bool Joinbools(BoolJoin join, IEnumerable<bool> bools)
    {
        switch (join)
        {
            case BoolJoin.Or:
                return bools.Any((x) => x);
            case BoolJoin.And:
                return bools.All((x) => x);
            case BoolJoin.Xor:
                return bools.Count((x) => x) == 1;
            default:
                return bools.Any();
        }
    }
}
