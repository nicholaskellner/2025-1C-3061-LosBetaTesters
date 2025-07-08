using Microsoft.Xna.Framework; 
public class OrientedBoundingBox
{
    public Vector3 Center;
    public Vector3 HalfExtents;
    public Matrix Orientation;

    public OrientedBoundingBox(Vector3 center, Vector3 halfExtents, Matrix orientation)
    {
        Center = center;
        HalfExtents = halfExtents;
        Orientation = orientation;
    }

    public Vector3[] GetCorners()
    {
        Vector3[] localCorners = new Vector3[8];
        Vector3 he = HalfExtents;

        localCorners[0] = new Vector3(-he.X, -he.Y, -he.Z);
        localCorners[1] = new Vector3(-he.X, -he.Y,  he.Z);
        localCorners[2] = new Vector3(-he.X,  he.Y, -he.Z);
        localCorners[3] = new Vector3(-he.X,  he.Y,  he.Z);
        localCorners[4] = new Vector3( he.X, -he.Y, -he.Z);
        localCorners[5] = new Vector3( he.X, -he.Y,  he.Z);
        localCorners[6] = new Vector3( he.X,  he.Y, -he.Z);
        localCorners[7] = new Vector3( he.X,  he.Y,  he.Z);

        Vector3[] worldCorners = new Vector3[8];
        for (int i = 0; i < 8; i++)
        {
            worldCorners[i] = Vector3.Transform(localCorners[i], Orientation) + Center;
        }

        return worldCorners;
    }

    public BoundingBox GetAABB()
    {
        return BoundingBox.CreateFromPoints(GetCorners());
    }
}
