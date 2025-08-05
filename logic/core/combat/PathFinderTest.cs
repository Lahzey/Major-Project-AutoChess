using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;

namespace MPAutoChess.logic.core.combat;

public class PathFinderTest {


    
    public static void Main() {
        float gridScale = 0.5f;
        List<Vector2> nodePositions = new List<Vector2> {
            new Vector2(2f, 2f),
            new Vector2(13f, 1f),
            new Vector2(10.5f, 3.5f),
            new Vector2(6.5f, 6.5f),
            new Vector2(4f, 8f),
            new Vector2(12.5f, 10.5f)
            
        };
        List<float> nodeSizes = new List<float> {
            2f, 2f, 1f, 3f, 2f, 3f
        };
        float oneOverGridScale = 1f / gridScale;
        List<Vector2I> gridPositions = nodePositions.Select(pos => PathFinder.ToGridCoord(pos, oneOverGridScale)).ToList();

        int sourceIndex = 5;
        int targetIndex = 0;
        float sourceRadius = nodeSizes[sourceIndex] * 0.5f;
        float targetRadius = nodeSizes[targetIndex] * 0.5f;
        float gridSourceRadius = sourceRadius * oneOverGridScale;
        
        Dictionary<int, bool> walkableCache = new Dictionary<int, bool>();
        
        Func<Vector2I, bool> isWalkable = pos => {
            if (walkableCache.TryGetValue(PathFinder.Hash(pos), out bool isWalkableCached)) {
                return isWalkableCached;
            }
            for (int i = 0; i < gridPositions.Count; i++) {
                if (i == sourceIndex) continue;
                Vector2I nodePos = gridPositions[i];
                float nodeRadius = nodeSizes[i] * 0.5f * oneOverGridScale;
                float minDistanceSquared = (gridSourceRadius + nodeRadius) * (gridSourceRadius + nodeRadius);
                if (pos.DistanceSquaredTo(nodePos) < minDistanceSquared) {
                    walkableCache[PathFinder.Hash(pos)] = false;
                    return false;
                }
            }
            walkableCache[PathFinder.Hash(pos)] = true;
            return true;
        };
        
        Rect2 bounds = new Rect2(0, 0, 14, 12);
        Vector2 start = nodePositions[sourceIndex];
        Vector2 target = nodePositions[targetIndex];
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        PathFinder.Path path = null;
        for (int i = 0; i < 100; i++) {
            // Run the pathfinding multiple times to measure performance
            path = PathFinder.FindPath(start, target, sourceRadius + targetRadius + 0.5f, bounds, isWalkable, gridScale);
        }
        stopwatch.Stop();
        if (path != null) {
            Console.WriteLine("Path found:");

            Vector2I gridSize = new Vector2I((int)(bounds.Size.X * oneOverGridScale) + 1, (int)(bounds.Size.Y * oneOverGridScale) + 1);
            int[,] grid = new int[gridSize.X, gridSize.Y];
            for (int i = 0; i < nodePositions.Count; i++) {
                Vector2 pos = nodePositions[i];
                int halfSizeInGrid = (int)(nodeSizes[i] * oneOverGridScale) / 2;
                for (int x = -halfSizeInGrid; x <= halfSizeInGrid; x++) {
                    for (int y = -halfSizeInGrid; y <= halfSizeInGrid; y++) {
                        int gridX = (int)(pos.X * oneOverGridScale) + x;
                        int gridY = (int)(pos.Y * oneOverGridScale) + y;
                        grid[gridX, gridY] = -1;
                    }
                }
            }
            
            foreach (Vector2 point in path.Points) {
                Vector2I gridPos = PathFinder.ToGridCoord(point, oneOverGridScale);
                grid[gridPos.X, gridPos.Y] = 1; // Mark path
            }

            Console.Write("o");
            for (int i = 0; i < gridSize.X; i++) {
                Console.Write("---");
            }
            Console.WriteLine("o");
            for (int y = 0; y < gridSize.Y; y++) {
                Console.Write("|");
                for (int x = 0; x < gridSize.X; x++) {
                    int value = grid[x, y];
                    if (value == -1) {
                        Console.Write("-X-");
                    } else if (value == 0) {
                        Console.Write("   ");
                    } else if (value == 1) {
                        Console.Write("<0>");
                    } else {
                        Console.Write(".?.");
                    }
                }
                Console.WriteLine("|");
            }
            Console.Write("o");
            for (int i = 0; i < gridSize.X; i++) {
                Console.Write("---");
            }
            Console.WriteLine("o");
            Console.WriteLine($"Total Length: {path.TotalLength}");
        } else {
            Console.WriteLine("No path found.");
        }
        Console.WriteLine($"Pathfinding took {stopwatch.ElapsedMilliseconds} ms");
    }
    
}