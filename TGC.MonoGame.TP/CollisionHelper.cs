using Microsoft.Xna.Framework;

public static class CollisionHelper
{
    public static bool OBBvsAABB(OrientedBoundingBox obb, BoundingBox aabb)
    {
        var obbCorners = obb.GetCorners();
        var aabbBox = BoundingBox.CreateFromPoints(aabb.GetCorners());

        foreach (var corner in obbCorners)
        {
            if (aabbBox.Contains(corner) != ContainmentType.Disjoint)
                return true;
        }

        return false;
    }
}
