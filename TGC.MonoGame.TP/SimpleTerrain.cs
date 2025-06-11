using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

public class SimpleTerrain
{
    private float[,] heights;
    private int width;
    private int height;
    private float cellSize;

    public int Width => width;
    public int Height => height;

    public SimpleTerrain(ContentManager content, string heightMapPath, float cellSize = 1f)
    {
        this.cellSize = cellSize;

        Texture2D heightMap = content.Load<Texture2D>(heightMapPath);
        width = heightMap.Width;
        height = heightMap.Height;

        heights = new float[width, height];
        Color[] heightMapData = new Color[width * height];
        heightMap.GetData(heightMapData);

        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                Color pixel = heightMapData[z * width + x];
                heights[x, z] = pixel.R / 255f;
            }
        }
    }

    public float GetHeight(float x, float z)
    {
        float fx = x / cellSize;
        float fz = z / cellSize;

        if (fx < 0) fx = 0;
        if (fz < 0) fz = 0;
        if (fx > width - 1) fx = width - 1;
        if (fz > height - 1) fz = height - 1;

        int x0 = (int)fx;
        int z0 = (int)fz;
        int x1 = x0 + 1 < width ? x0 + 1 : x0;
        int z1 = z0 + 1 < height ? z0 + 1 : z0;

        float h00 = heights[x0, z0];
        float h10 = heights[x1, z0];
        float h01 = heights[x0, z1];
        float h11 = heights[x1, z1];

        float tx = fx - x0;
        float tz = fz - z0;

        float height0 = MathHelper.Lerp(h00, h10, tx);
        float height1 = MathHelper.Lerp(h01, h11, tx);
        float finalHeight = MathHelper.Lerp(height0, height1, tz);

        return finalHeight * 20f; // escala máxima del terreno
    }
}

