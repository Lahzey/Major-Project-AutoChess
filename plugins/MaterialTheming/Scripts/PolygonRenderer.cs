using System.Collections.Generic;
using Godot;

namespace MaterialTheming.Scripts;

public class PolygonRenderer {
    // these colors can be used like bitflags by the shader to detect what the pixel its rendering is (r: 0 = other, 1 = polygon | g: 0 = background, 1 = border)
    public static readonly Color BACKGROUND_CUSTOM = new Color(1, 0, 0);
    public static readonly Color BORDER_CUSTOM = new Color(1, 1, 0);

    private readonly Dictionary<Rid, ArrayMesh> createdMeshes = new Dictionary<Rid, ArrayMesh>();

    public void Render(PolygonInfo polygon, Rid toCanvasItem, Rect2 bounds) {
        SurfaceTool surfaceTool = CreateMesh(polygon, bounds);
        if (surfaceTool == null) return;

        // this may potentially cause memory issues when too many canvas items with this style box are destroyed, as we never do the cleanup TODO: add TreeExited callback to canvas item
        // also, this will cause issues if the same canvas item tries to render the same style box (and thus uses the same PolygonRenderer) multiple times
        ArrayMesh mesh;
        if (createdMeshes.TryGetValue(toCanvasItem, out mesh)) {
            mesh.Dispose();
            mesh = surfaceTool.Commit();
            createdMeshes[toCanvasItem] = mesh;
        } else {
            mesh = surfaceTool.Commit();
            createdMeshes.Add(toCanvasItem, mesh);
        }
        RenderingServer.Singleton.CanvasItemAddMesh(toCanvasItem, mesh.GetRid());
    }
    
    protected virtual SurfaceTool CreateMesh(PolygonInfo polygon, Rect2 bounds) {
        if (polygon.PolygonIndices == null) return null;
        
        SurfaceTool surfaceTool = new SurfaceTool();
        surfaceTool.Begin(Mesh.PrimitiveType.Triangles);
        surfaceTool.SetCustomFormat(0, SurfaceTool.CustomFormat.RgbaFloat);
        
        bool isCounterClockwise = IsCounterClockwise(polygon.PolygonPoints);
        Vector2 borderOffset = new Vector2(polygon.BorderThickness, polygon.BorderThickness);
        Rect2 backgroundRect = new Rect2(bounds.Position + borderOffset, bounds.Size - borderOffset*2);
        
        if (backgroundRect.Size.X < 0 || backgroundRect.Size.Y < 0) {
            return surfaceTool;
        }
        
        // If X or Y size is exactly zero, make it very small but not zero to avoid math issues
        if (backgroundRect.Size.X == 0) backgroundRect.Size = new Vector2(0.001f, backgroundRect.Size.Y);
        if (backgroundRect.Size.Y == 0) backgroundRect.Size = new Vector2(backgroundRect.Size.X, 0.001f);
        
        // calculate sizes of fixed width sides in polygon (anything outside the 0-1 range, sort of like a NinePatchRect)
        Vector2 min = Vector2.Zero;
        Vector2 max = Vector2.One;
        foreach (Vector2 point in polygon.PolygonPoints) {
            if (point.X < min.X) min.X = point.X;
            else if (point.X > max.X) max.X = point.X;
            if (point.Y < min.Y) min.Y = point.Y;
            else if (point.Y > max.Y) max.Y = point.Y;
        }
        Vector2 topLeftGap = -min;
        Vector2 bottomRightGap = max - Vector2.One;
        Rect2 innerBackgroundRect = new Rect2(backgroundRect.Position + topLeftGap, backgroundRect.Size - topLeftGap - bottomRightGap);
        
        PrecomputedEdge[] edges = new PrecomputedEdge[polygon.PolygonPoints.Length];

        // add border rectangles
        for (int i = 0; i < polygon.PolygonPoints.Length; i++) {
            Vector2 current = ScalePolygonPoint(polygon.PolygonPoints[i], innerBackgroundRect);
            Vector2 next = ScalePolygonPoint(polygon.PolygonPoints[(i + 1) % polygon.PolygonPoints.Length], innerBackgroundRect);
            Vector2 edge = next - current;

            // Rotate edge 90 degrees to get outward normal
            Vector2 edgeNormal = edge.Normalized();
            Vector2 outwardNormal = isCounterClockwise ? new Vector2(edge.Y, -edge.X) : new Vector2(-edge.Y, edge.X);
            outwardNormal = outwardNormal.Normalized(); // Normalize for direction vector
            
            edges[i] = new PrecomputedEdge(current, next, edgeNormal, outwardNormal);
        }

        for (int i = 0; i < edges.Length; i++) {
            int nextI = (i + 1) % edges.Length;
            PrecomputedEdge current = edges[i];
            PrecomputedEdge next = edges[nextI];
            Vector2 endCornerOutwards = (current.outwardNormal + next.outwardNormal).Normalized();
            Vector2 endOuter = current.end + current.outwardNormal * polygon.BorderThickness;
            Vector2? endCornerOuter = TryGetLineIntersection(current.end, endCornerOutwards, endOuter, current.edgeNormal);
            if (endCornerOuter == null) {
                GD.PrintErr($"Failed to calculate border corners, polygon contains parallel lines at {i}&{nextI} ({current.start}-{current.end} & {next.start}-{next.end}).");
                return surfaceTool;
            }
            edges[i].endOuterCorner = (Vector2) endCornerOuter;
            edges[nextI].startOuterCorner = (Vector2) endCornerOuter;
        }
        
        foreach (PrecomputedEdge edge in edges) {
            CreateBorderGeometry(polygon, surfaceTool, edge, bounds);
        }

        // add content polygon
        CreateBackgroundGeometry(polygon, surfaceTool, innerBackgroundRect, bounds);

        return surfaceTool;
    }

    protected virtual void CreateBackgroundGeometry(PolygonInfo polygon, SurfaceTool surfaceTool, Rect2 backgroundRect, Rect2 totalRect) {
        foreach (int pointIndex in polygon.PolygonIndices) {
            Vector2 point = polygon.PolygonPoints[pointIndex];
            point = ScalePolygonPoint(point, backgroundRect);
            surfaceTool.SetColor(polygon.BackgroundColor);
            surfaceTool.SetUV((point - totalRect.Position) / totalRect.Size);
            surfaceTool.SetCustom(0, BACKGROUND_CUSTOM);
            surfaceTool.AddVertex(point.ToVector3());
        }
    }

    protected virtual void CreateBorderGeometry(PolygonInfo polygon, SurfaceTool surfaceTool, PrecomputedEdge edge, Rect2 totalRect) {
        AddRectToMesh(surfaceTool, edge.startOuterCorner, edge.endOuterCorner, edge.end, edge.start, polygon.BorderColor, BORDER_CUSTOM, totalRect);
    }

    static Vector2? TryGetLineIntersection(Vector2 pointA, Vector2 directionA, Vector2 pointB, Vector2 directionB) {
        float denominator = directionA.X * directionB.Y - directionA.Y * directionB.X;

        // Check if lines are parallel (denominator is zero)
        if (Mathf.Abs(denominator) < 1e-6) {
            return null;
        }

        float tA = ((pointB.X - pointA.X) * directionB.Y - (pointB.Y - pointA.Y) * directionB.X) / denominator;

        return new Vector2(pointA.X + tA * directionA.X, pointA.Y + tA * directionA.Y);
    }

    private static Vector2 ScalePolygonPoint(Vector2 point, Rect2 innerBounds) {
        // point values from 0 to 1 are scaled up to totalRect sizes, values that are above or below act as a fixed size edge (like a NinePatchRect) example: 3.0 / -10.5 -> xSize + 2.0 / -10.5
        if (point.X > 1f) {
            point.X = innerBounds.Size.X + (point.X - 1f);
        } else if (point.X >= 0f) {
            point.X *= innerBounds.Size.X;
        }
        if (point.Y > 1f) {
            point.Y = innerBounds.Size.Y + (point.Y - 1f);
        } else if (point.Y >= 0f) {
            point.Y *= innerBounds.Size.Y;
        }
        
        return innerBounds.Position + point;
    }

    private static bool IsCounterClockwise(Vector2[] points) {
        float area = 0;
        for (int i = 0; i < points.Length; i++) {
            Vector2 current = points[i];
            Vector2 next = points[(i + 1) % points.Length];
            area += (next.X - current.X) * (next.Y + current.Y);
        }

        return area < 0;
    }

    protected static void AddRectToMesh(SurfaceTool surfaceTool, Vector2 aa, Vector2 ba, Vector2 bb, Vector2 ab, Color color, Color custom, Rect2 totalRect) {
        // triangle aa - ba - bb
        surfaceTool.SetColor(color);
        surfaceTool.SetUV((aa - totalRect.Position) / totalRect.Size);
        surfaceTool.SetCustom(0, custom);
        surfaceTool.AddVertex(aa.ToVector3());

        surfaceTool.SetColor(color);
        surfaceTool.SetUV((ba - totalRect.Position) / totalRect.Size);
        surfaceTool.SetCustom(0, custom);
        surfaceTool.AddVertex(ba.ToVector3());

        surfaceTool.SetColor(color);
        surfaceTool.SetUV((bb - totalRect.Position) / totalRect.Size);
        surfaceTool.SetCustom(0, custom);
        surfaceTool.AddVertex(bb.ToVector3());

        // triangle aa - bb - ab
        surfaceTool.SetColor(color);
        surfaceTool.SetUV((aa - totalRect.Position) / totalRect.Size);
        surfaceTool.SetCustom(0, custom);
        surfaceTool.AddVertex(aa.ToVector3());

        surfaceTool.SetColor(color);
        surfaceTool.SetUV((bb - totalRect.Position) / totalRect.Size);
        surfaceTool.SetCustom(0, custom);
        surfaceTool.AddVertex(bb.ToVector3());

        surfaceTool.SetColor(color);
        surfaceTool.SetUV((ab - totalRect.Position) / totalRect.Size);
        surfaceTool.SetCustom(0, custom);
        surfaceTool.AddVertex(ab.ToVector3());
    }

    protected struct PrecomputedEdge {
        public Vector2 start;
        public Vector2 startOuterCorner = Vector2.Zero;
        public Vector2 end;
        public Vector2 endOuterCorner = Vector2.Zero;
        
        public Vector2 edgeNormal;
        public Vector2 outwardNormal;
        
        public PrecomputedEdge(Vector2 start, Vector2 end, Vector2 edgeNormal, Vector2 outwardNormal) {
            this.start = start;
            this.end = end;
            this.edgeNormal = edgeNormal;
            this.outwardNormal = outwardNormal;
        }
    }
}