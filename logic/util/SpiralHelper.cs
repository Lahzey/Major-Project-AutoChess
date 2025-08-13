using System;
using System.Collections.Generic;
using Godot;

namespace MPAutoChess.logic.util;

public static class SpiralHelper {
    
    // this code was written by ChatGPT, according to which these are "Manhattan rings"
    public static IEnumerable<Vector2> SpiralAround(Vector2 origin, float step) {
        yield return origin; // starting position, optional

        int x = 0, y = 0; // offsets in "grid space"
        int stepSize = 1; // number of grid cells to walk before turning
        int stepsTaken = 0; // steps taken in current leg
        int leg = 0; // how many legs completed
        int dx = 1, dy = 0; // start moving right

        while (true) {
            x += dx;
            y += dy;
            yield return origin + new Vector2(x * step, y * step);

            stepsTaken++;
            if (stepsTaken == stepSize) {
                stepsTaken = 0;
                leg++;

                // Rotate direction clockwise
                int tmp = dx;
                dx = -dy;
                dy = tmp;

                // Every two turns, increase step length
                if (leg % 2 == 0) {
                    stepSize++;
                }
            }
        }
    }
}