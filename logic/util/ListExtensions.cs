using System;
using System.Collections.Generic;

namespace MPAutoChess.logic.util;

public static class ListExtensions {
    public static void Shuffle<T>(this IList<T> list, Random random) {
        int n = list.Count;
        while (n > 1) {
            n--;
            int k = random.Next(n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }
    }
}