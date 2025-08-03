using Godot;
using Godot.Collections;

namespace MPAutoChess.logic.util;

public static class SpaceStateExtensions {
    public static RaycastHit3D? IntersectRayTyped(this PhysicsDirectSpaceState3D spaceState, PhysicsRayQueryParameters3D parameters) {
        Dictionary result = spaceState.IntersectRay(parameters);
        if (!result.ContainsKey("collider")) return null;
        
        return new RaycastHit3D {
            Collider = result["collider"],
            ColliderId = (int)result["collider_id"],
            Normal = (Vector3)result["normal"],
            Position = (Vector3)result["position"],
            FaceIndex = (int)result["face_index"],
            Rid = (Rid)result["rid"],
            ShapeIndex = (int)result["shape"]
        };
    }
    
    public static IntersectionHit2D[] IntersectPointTyped(this PhysicsDirectSpaceState2D spaceState, PhysicsPointQueryParameters2D parameters) {
        Array<Dictionary> results = spaceState.IntersectPoint(parameters);
        IntersectionHit2D[] hits = new IntersectionHit2D[results.Count];
        for (int i = 0; i < results.Count; i++) {
            Dictionary result = results[i];
            hits[i] = new IntersectionHit2D {
                Collider = result["collider"],
                ColliderId = (int)result["collider_id"],
                Rid = (Rid)result["rid"],
                ShapeIndex = (int)result["shape"]
            };
        }
        return hits;
    }
}

public struct RaycastHit3D {
    public Variant Collider { get; set; }
    public int ColliderId { get; set; }
    public Vector3 Normal { get; set; }
    public Vector3 Position { get; set; }
    public int FaceIndex { get; set; } // only valid if ConcavePolygonShape3D, otherwise -1
    public Rid Rid { get; set; }
    public int ShapeIndex { get; set; }
}

public struct IntersectionHit2D {
    public Variant Collider { get; set; }
    public int ColliderId { get; set; }
    public int ShapeIndex { get; set; }
    public Rid Rid { get; set; }
}

public struct RaycastHit2D {
    public Variant Collider { get; set; }
    public int ColliderId { get; set; }
    public int ShapeIndex { get; set; }
    public Rid Rid { get; set; }
    public Vector2 Normal { get; set; }
    public Vector2 Position { get; set; }
}