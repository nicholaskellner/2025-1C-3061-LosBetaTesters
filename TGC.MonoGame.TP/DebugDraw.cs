using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class DebugDraw
{
    private GraphicsDevice graphicsDevice;
    private BasicEffect basicEffect;

    public DebugDraw(GraphicsDevice graphicsDevice)
    {
        this.graphicsDevice = graphicsDevice;
        basicEffect = new BasicEffect(graphicsDevice)
        {
            VertexColorEnabled = true,
            LightingEnabled = false,
            TextureEnabled = false,
        };
    }

    public void DrawBoundingBox(BoundingBox box, Matrix view, Matrix projection)
    {
        var corners = box.GetCorners();

        DrawLines(corners, view, projection);
    }

    public void DrawOrientedBoundingBox(OrientedBoundingBox obb, Matrix view, Matrix projection)
    {
        var corners = obb.GetCorners();

        DrawLines(corners, view, projection);
    }

    private void DrawLines(Vector3[] corners, Matrix view, Matrix projection)
    {
        // 12 edges of a box connecting these corners:
        int[] indices = new int[]
        {
            0,1, 1,3, 3,2, 2,0, // bottom face
            4,5, 5,7, 7,6, 6,4, // top face
            0,4, 1,5, 2,6, 3,7  // vertical edges
        };

        VertexPositionColor[] vertices = new VertexPositionColor[indices.Length];

        for (int i = 0; i < indices.Length; i++)
        {
            vertices[i] = new VertexPositionColor(corners[indices[i]], Color.Red);
        }

        basicEffect.View = view;
        basicEffect.Projection = projection;
        basicEffect.World = Matrix.Identity;

        foreach (var pass in basicEffect.CurrentTechnique.Passes)
        {
            pass.Apply();
            graphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, vertices, 0, indices.Length / 2);
        }
    }
}
