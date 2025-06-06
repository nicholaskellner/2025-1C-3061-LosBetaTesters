using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using System;
public class Shell
{
    private Model Model { get; set; }
    private Effect Effect { get; set; }
    public Vector3 _direction;
    public Vector3 _position;
    private Vector3 gravity = new Vector3(0,-9.8f,0);
    private Matrix World { get; set; }

    public bool isExpired = false;
    private float lifetime = 5f; // Tiempo desde que se detiene
    private Vector3 speed;
    private float fireSpeed = 130f;
    public Shell(Model model, Effect effect, Vector3 position, Vector3 direction)
    {
        Model = model;
        Effect = effect;
        _position = position;
        direction.Normalize();
        speed = direction * fireSpeed;

        foreach (var mesh in Model.Meshes)
            foreach (var part in mesh.MeshParts)
                part.Effect = Effect;
    }

    public void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        speed += gravity * dt;
        _position += speed * dt;

        lifetime -= dt;
        if (lifetime <= 0) isExpired = true;

        Vector3 forward = Vector3.Normalize(speed);
        World = Matrix.CreateScale(0.1f)
              * Matrix.CreateWorld(_position, forward, Vector3.Up);
    }

    public void Draw(Matrix View, Matrix Projection)
    {
        foreach (var mesh in Model.Meshes)
        {
            foreach (var effect in mesh.Effects)
            {
                effect.Parameters["World"].SetValue(World);
                effect.Parameters["View"].SetValue(View);
                effect.Parameters["Projection"].SetValue(Projection);
                //Lo setee asi porque el modelo no tiene color
                effect.Parameters["shellColor"]?.SetValue(new Vector4(0.686f, 0.608f, 0.376f, 1f));

                //effect.Parameters["KAmbient"]?.SetValue(0.5f);
                //effect.EnableDefaultLighting();
            }
            mesh.Draw();
        }
    }
}