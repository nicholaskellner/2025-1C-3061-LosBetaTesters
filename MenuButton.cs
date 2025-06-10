public class MenuButton
{
    public BoundingBox BoundingBox;
    public Action OnClick;
    public Texture2D Texture;
    public Vector3 Position;
    public VertexPositionTexture[] Vertices;
    public short[] Indices;
    public Matrix World;

    public MenuButton(GraphicsDevice graphicsDevice, Texture2D texture, Vector3 position, Vector2 size, Action onClick)
    {
        Texture = texture;
        Position = position;
        OnClick = onClick;

        World = Matrix.CreateTranslation(Position);

        // Create quad
        Vertices = new VertexPositionTexture[4];
        Indices = new short[] { 0, 1, 2, 2, 1, 3 };

        Vector3 topLeft = new Vector3(-size.X / 2, size.Y / 2, 0);
        Vector3 bottomLeft = new Vector3(-size.X / 2, -size.Y / 2, 0);
        Vector3 topRight = new Vector3(size.X / 2, size.Y / 2, 0);
        Vector3 bottomRight = new Vector3(size.X / 2, -size.Y / 2, 0);

        Vertices[0] = new VertexPositionTexture(topLeft, new Vector2(0, 0));
        Vertices[1] = new VertexPositionTexture(bottomLeft, new Vector2(0, 1));
        Vertices[2] = new VertexPositionTexture(topRight, new Vector2(1, 0));
        Vertices[3] = new VertexPositionTexture(bottomRight, new Vector2(1, 1));

        var corners = new Vector3[]
        {
            Vector3.Transform(topLeft, World),
            Vector3.Transform(bottomLeft, World),
            Vector3.Transform(topRight, World),
            Vector3.Transform(bottomRight, World)
        };

        BoundingBox = BoundingBox.CreateFromPoints(corners);
    }

    public void Draw(GraphicsDevice graphicsDevice, BasicEffect effect, Matrix view, Matrix projection)
    {
        effect.World = World;
        effect.View = view;
        effect.Projection = projection;
        effect.TextureEnabled = true;
        effect.Texture = Texture;
        effect.CurrentTechnique.Passes[0].Apply();

        graphicsDevice.DrawUserIndexedPrimitives(
            PrimitiveType.TriangleList,
            Vertices, 0, 4,
            Indices, 0, 2
        );
    }
}
