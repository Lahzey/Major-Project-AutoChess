using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using Godot;
using Priority_Queue;
using ProtoBuf;
using Vector2 = Godot.Vector2;

namespace MPAutoChess.logic.core.combat;

public static class PathFinder {
    
    [ProtoContract]
    public class Path {
        [ProtoMember(1)] public List<Vector2> Points { get; private set; }
        [ProtoMember(2)] public float TotalLength { get; private set; }
        [ProtoMember(3)] private int currentIndex;

        public Path() { } // for Protobuf serialization

        public Path(List<Vector2> points) {
            Points = points;
            TotalLength = 0;
            for (int i = 1; i < points.Count; i++)
                TotalLength += points[i].DistanceTo(points[i - 1]);
            if (points.Count > 1) currentIndex = 1; // any path that consists of more than just its start point should start pathing towards the next point from the get-go
        }

        public Vector2 Advance(Vector2 currentPositon, float distance) {
            Vector2 nextPoint = Points[currentIndex];
            float distanceToPoint = currentPositon.DistanceTo(nextPoint);
            while (distanceToPoint < distance) {
                currentPositon = nextPoint; // teleport the point, so we can move from there
                distance -= distanceToPoint;
                if (currentIndex < Points.Count - 1) {
                    currentIndex++;
                    nextPoint = Points[currentIndex];
                    distanceToPoint = currentPositon.DistanceTo(nextPoint);
                } else {
                    return currentPositon;
                }
            }

            return currentPositon.Lerp(nextPoint, distance / distanceToPoint);
        }
    }

    public class NodeData : FastPriorityQueueNode {
        public Vector2I Position;
        public float GCost;
        public float HCost;
        public NodeData Parent;

        public float FCost => GCost + HCost;
    }

    public static Path FindPath(Vector2 start, Vector2 target, float acceptanceRadius, Rect2 bounds, Func<Vector2I, bool> isWalkable, float gridScale) {
        float oneOverGridScale = 1f / gridScale;
        Vector2I startCoord = ToGridCoord(start, oneOverGridScale);
        Vector2I targetCoord = ToGridCoord(target, oneOverGridScale);
        Rect2I gridBounds = new Rect2I(
            (Vector2I)(bounds.Position * oneOverGridScale),
            (Vector2I)(bounds.Size * oneOverGridScale)
        );

        Dictionary<int, NodeData> open = new Dictionary<int, NodeData>();
        HashSet<int> closed = new HashSet<int>();
        FastPriorityQueue<NodeData> queue = new FastPriorityQueue<NodeData>((int)((bounds.Size.X * oneOverGridScale) * (bounds.Size.Y * oneOverGridScale)));

        NodeData startNode = new NodeData {
            Position = startCoord,
            GCost = 0,
            HCost = startCoord.DistanceTo(targetCoord)
        };

        open[Hash(startCoord)] = startNode; // this is WAY faster than hashing Vector2I and works just as well within +-16000 coords
        queue.Enqueue(startNode, startNode.FCost);

        Vector2I[] directions = {
            new Vector2I(1, 0),
            new Vector2I(-1, 0),
            new Vector2I(0, 1),
            new Vector2I(0, -1),
            new Vector2I(1, 1),
            new Vector2I(-1, -1),
            new Vector2I(-1, 1),
            new Vector2I(1, -1)
        };
        float diagonalCost = Mathf.Sqrt2; // sqrt(2) for diagonal movement
        float gridAcceptanceRadiusSquared = acceptanceRadius * oneOverGridScale;
        gridAcceptanceRadiusSquared *= gridAcceptanceRadiusSquared;

        while (queue.Count > 0) {
            NodeData current = queue.Dequeue();
            if (!closed.Add(Hash(current.Position)))
                continue;

            if (current.Position.DistanceSquaredTo(targetCoord) <= gridAcceptanceRadiusSquared) {
                // Build path
                List<Vector2> path = new List<Vector2>();
                NodeData pathNode = current;
                while (pathNode != null) {
                    path.Insert(0, FromGridCoord(pathNode.Position, gridScale));
                    pathNode = pathNode.Parent;
                }

                return new Path(path);
            }

            for (int i = 0; i < directions.Length; i++) {
                Vector2I dir = directions[i];
                Vector2I neighborGridPos = current.Position + dir;

                int neighborHash = Hash(neighborGridPos);
                if (closed.Contains(neighborHash))
                    continue;

                if (!gridBounds.HasPoint(neighborGridPos) || !isWalkable(neighborGridPos))
                    continue;

                float stepCost = i >= 4 ? diagonalCost : gridScale;
                float newGCost = current.GCost + stepCost;

                if (open.TryGetValue(neighborHash, out NodeData neighbor) && !(newGCost < neighbor.GCost)) continue;
                
                neighbor = new NodeData {
                    Position = neighborGridPos,
                    GCost = newGCost,
                    HCost = Heuristic(neighborGridPos, targetCoord),
                    Parent = current
                };
                open[neighborHash] = neighbor;
                queue.Enqueue(neighbor, neighbor.FCost);
            }
        }

        return null;
    }

    public static Vector2I ToGridCoord(Vector2 pos, float oneOverGridScale) {
        return new Vector2I(Mathf.RoundToInt(pos.X * oneOverGridScale), Mathf.RoundToInt(pos.Y * oneOverGridScale));
    }
    
    public static Vector2 FromGridCoord(Vector2I coord, float gridScale) {
        return new Vector2(coord.X * gridScale, coord.Y * gridScale);
    }

    private  static float Heuristic(Vector2I a, Vector2I b) {
        int dx = Math.Abs(a.X - b.X);
        int dy = Math.Abs(a.Y - b.Y);
        return dx + dy + (Mathf.Sqrt2 - 2f) * Math.Min(dx, dy); // Octile
    }
    
    public static int Hash(Vector2I pos) {
        // 15+15 bits for each dimension, plus 2 bits for the signs allows for roughly 32000 values in each dimension (mult instead of bitshifting because the sign is at the most significant bit and would get shifted out)
        // + 16000 makes them all positive, so X and Y do not interact with each others bits
        return ((pos.X + 16000) * 32000) + pos.Y + 16000; // this is WAY faster than actually hashing Vector2I and works just as well within +-16000 coords
    }
    
    public static bool IsValidBounds(Rect2 bounds, float gridScale) {
        Vector2 minPos = bounds.Position;
        Vector2 maxPos = bounds.Position + bounds.Size;
        minPos /= gridScale;
        maxPos /= gridScale;
        return (minPos.X > -16000) && (maxPos.X < 16000) && (minPos.Y > -16000) && (maxPos.Y < 16000);
    }
}