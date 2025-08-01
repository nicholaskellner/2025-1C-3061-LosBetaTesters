using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
public class Tree : Prop
{
    private List<Vector3> colors;
    public Tree(Model model, Effect effect, Vector3 position, Vector3 direction, BoundingBox box, float scale) : base(model, effect, position, direction,scale)
    {
        Model = model;
        Effect = effect;
        _position = position;
        direction.Normalize();
        hitBox = box;
        world = Matrix.CreateScale(scale) * Matrix.CreateTranslation(position);
        color = new Vector3(0.943f, 0.588f, 0.325f);

        foreach (var mesh in Model.Meshes)
            foreach (var part in mesh.MeshParts)
                part.Effect = Effect;
    }
    public Tree(Model model, Effect effect, Vector3 position, Vector3 direction, BoundingBox box, List<Vector3> colores,float scale) : base(model, effect, position, direction,scale)
    {
        Model = model;
        Effect = effect;
        _position = position;
        direction.Normalize();
        hitBox = box;
        world = Matrix.CreateScale(scale) * Matrix.CreateTranslation(position);
        colors = colores;

        foreach (var mesh in Model.Meshes)
            foreach (var part in mesh.MeshParts)
                part.Effect = Effect;
    }
    public override void Update(GameTime gameTime)
    {
        //Algo que hacer
    }

    public override void getHit()
    {
        isExpired = true;
    }

   public override void Draw(GraphicsDevice graphicsDevice, Matrix View, Matrix Projection, Vector3 lightPos, Vector3 cameraPos, Matrix lightViewProjection, RenderTarget2D shadowMap)
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
            effect.Parameters["KDiffuse"]?.SetValue(0.5f);
            effect.Parameters["KSpecular"]?.SetValue(0.2f);
            effect.Parameters["shininess"]?.SetValue(32f);
        }

        foreach (var part in mesh.MeshParts)
        {
            graphicsDevice.SetVertexBuffer(part.VertexBuffer);
            graphicsDevice.Indices = part.IndexBuffer;

            int colorIndex = Math.Min(mesh.MeshParts.IndexOf(part), colors.Count - 1);
            part.Effect.Parameters["color"].SetValue(new Vector4(colors[colorIndex], 1f));

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