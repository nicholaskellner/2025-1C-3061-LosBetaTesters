using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using System;
public class Shell{
    private Model Model { get; set; }
    private Effect Effect { get; set; }
    public Vector3 _direction;
    public Vector3 _position;
    private float gravity = -9.8f; 
    private Matrix World { get; set; }
    private Vector3 velocity;
    
    public bool isExpired = false;
    private float lifetime = 5f; // Tiempo desde que se detiene
    private float currentSpeed = 10f; 
    private float speed = 30f;
    public Shell(Model model, Effect effect, Vector3 position, Vector3 direction)
    {
        Model = model;
        Effect = effect;
        _position = position;
        direction.Normalize();
        velocity = direction * speed;

        foreach (var mesh in Model.Meshes)
            foreach (var part in mesh.MeshParts)
                part.Effect = Effect;
    }

    public void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        
        _position += velocity * dt;
        
        lifetime -= dt;
        if (lifetime <= 0) isExpired = true;
        
        Vector3 forward = Vector3.Normalize(velocity);
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
                effect.Parameters["shellColor"]?.SetValue(new Vector4(0.686f,0.608f,0.376f,1f));

                //effect.Parameters["KAmbient"]?.SetValue(0.5f);
                //effect.EnableDefaultLighting();
            }
            mesh.Draw();
        }
    }
}