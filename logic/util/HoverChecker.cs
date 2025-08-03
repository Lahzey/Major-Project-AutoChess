using System.Collections.Generic;
using Godot;
using MPAutoChess.logic.core;

namespace MPAutoChess.logic.util;

public class HoverChecker {
    
    public static T[] GetHoveredNodes<T>(CollisionLayers collisionMask, Node2D nodeRef) {
        Vector2 mousePosition = nodeRef.GetViewport().GetCamera2D().GetGlobalMousePosition();
        PhysicsDirectSpaceState2D spaceState = nodeRef.GetWorld2D().DirectSpaceState;
        PhysicsPointQueryParameters2D queryParameters = new PhysicsPointQueryParameters2D {
            Position = mousePosition,
            CollideWithAreas = true,
            CollisionMask = (uint) collisionMask
        };

        List<T> hoveredNodes = new List<T>();
        foreach (IntersectionHit2D intersectionHit in spaceState.IntersectPointTyped(queryParameters)) {
            if (intersectionHit.Collider.Obj.GetType().IsAssignableTo(typeof(T))) {
                hoveredNodes.Add((T) intersectionHit.Collider.Obj);
            }
        }
        return hoveredNodes.ToArray();
    }
    
    public static T? GetHoveredNodeOrNull<T>(CollisionLayers collisionMask, Node2D nodeRef) {
        // it would be cleaner to call GetHoveredNodes and just return the first element (or null if empty), but that would allocate a list and an array for no reason
        Vector2 mousePosition = nodeRef.GetViewport().GetCamera2D().GetGlobalMousePosition();
        PhysicsDirectSpaceState2D spaceState = nodeRef.GetWorld2D().DirectSpaceState;
        PhysicsPointQueryParameters2D queryParameters = new PhysicsPointQueryParameters2D {
            Position = mousePosition,
            CollideWithAreas = true,
            CollisionMask = (uint) collisionMask
        };

        foreach (IntersectionHit2D intersectionHit in spaceState.IntersectPointTyped(queryParameters)) {
            if (intersectionHit.Collider.Obj.GetType().IsAssignableTo(typeof(T))) {
                return (T) intersectionHit.Collider.Obj;
            }
        }

        return default; // will always be null in practice, as trying to find not nullable types (like bool, int or structs) in the world makes no sense-
    }
    
}