using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using System;
public abstract class Prop
{
    protected Model Model { get; set; }
    protected Effect Effect { get; set; }
    protected Vector3 _direction;
    protected Vector3 _position;
    protected Vector3 color;
    protected Matrix world;
    protected Matrix World { get; set; }
    protected float scale;

    public bool isExpired = false;
    public BoundingBox hitBox;


    public Prop(Model model, Effect effect, Vector3 position, Vector3 direction, float scale)
    {
        Model = model;
        Effect = effect;
        _position = position;
        direction.Normalize();
        world = Matrix.CreateTranslation(position);
        this.scale = scale;

        foreach (var mesh in Model.Meshes)
            foreach (var part in mesh.MeshParts)
                part.Effect = Effect;
    }

    

    public abstract void Update(GameTime gameTime);

    public abstract void getHit();

    public virtual void Draw(GraphicsDevice graphicsDevice, Matrix View, Matrix Projection, Vector3 lightPos, Vector3 cameraPos, Matrix lightViewProjection, RenderTarget2D shadowMap)
{
    foreach (var mesh in Model.Meshes)
    {
        foreach (var effect in mesh.Effects)
        {
            effect.Parameters["World"].SetValue(world);
            effect.Parameters["View"].SetValue(View);
            effect.Parameters["Projection"].SetValue(Projection);

            var worldInverseTranspose = Matrix.Transpose(Matrix.Invert(world));
            effect.Parameters["WorldInverseTranspose"].SetValue(worldInverseTranspose);

            effect.Parameters["ambientColor"].SetValue(new Vector3(1f, 1f, 1f));
            effect.Parameters["KAmbient"].SetValue(0.5f);

            effect.Parameters["lightPosition"]?.SetValue(lightPos);
            effect.Parameters["lightColor"]?.SetValue(new Vector3(1f, 1f, 1f));
            effect.Parameters["cameraPosition"]?.SetValue(cameraPos);
            effect.Parameters["KDiffuse"]?.SetValue(0.7f);
            effect.Parameters["KSpecular"]?.SetValue(0.5f);
            effect.Parameters["shininess"]?.SetValue(32f);

            // Parámetros para sombras:
            effect.Parameters["LightViewProjection"]?.SetValue(lightViewProjection);
            effect.Parameters["ShadowMap"]?.SetValue(shadowMap);
        }

        foreach (var part in mesh.MeshParts)
        {
            graphicsDevice.SetVertexBuffer(part.VertexBuffer);
            graphicsDevice.Indices = part.IndexBuffer;

            part.Effect.Parameters["color"].SetValue(new Vector4(color, 1f));

            foreach (var pass in part.Effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                    part.VertexOffset, part.StartIndex, part.PrimitiveCount);
            }
        }
    }
}

}

