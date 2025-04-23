using System;
using System.Net.Mime;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

public class Tank{
    private Model Model { get; set; }
    private Effect Effect { get; set; }
    //private TextureD tex { get; set; }
    private Vector3 _rotation { get; set; }
    //private float _speed = 0;

    private float yaw = 0;
    private Vector3 _position = Vector3.Zero;

    private Matrix localRotation;

    private Matrix World { get; set; }

    public Tank(Model modelo,Effect efecto)
    {
        //Cree esta porque viene mirando para arriba el tanque.
        localRotation = Matrix.Identity * Matrix.CreateRotationX(MathHelper.ToRadians(-90)) * Matrix.CreateRotationY(MathHelper.ToRadians(90));
        _rotation = Vector3.Transform(Vector3.Up,localRotation);
        Model = modelo;
        Effect = efecto;
        //tex = t;
        foreach (var mesh in Model.Meshes)
        {
            // Un mesh puede tener mas de 1 mesh part (cada 1 puede tener su propio efecto).
            foreach (var meshPart in mesh.MeshParts)
            {
                meshPart.Effect = Effect;
            }
        }
        
        //Model.Meshes[0].MeshParts[0].Effect.Parameters["Texture"].SetValue(t); 
}

    /*protected override void LoadContent(String ontent, String effect)
    {

        // Cargo el modelo del logo.
        Model = Content.Load<Model>(ContentFolder3D + "T90");

        // Cargo un efecto basico propio declarado en el Content pipeline.
        // En el juego no pueden usar BasicEffect de MG, deben usar siempre efectos propios.
        Effect = Content.Load<Effect>(ContentFolderEffects + "BasicShader");

        // Asigno el efecto que cargue a cada parte del mesh.
        // Un modelo puede tener mas de 1 mesh internamente.
        foreach (var mesh in Model.Meshes)
        {
            // Un mesh puede tener mas de 1 mesh part (cada 1 puede tener su propio efecto).
            foreach (var meshPart in mesh.MeshParts)
            {
                meshPart.Effect = Effect;
            }
        }

        base.LoadContent();
    }*/


    public void Update(GameTime gameTime)
    {
        
        
        // Falta el elapsed time y velocidades
        if (Keyboard.GetState().IsKeyDown(Keys.W))
        {
            _position += _rotation *0.1f;
        }

        if (Keyboard.GetState().IsKeyDown(Keys.S))
        {
            _position -= _rotation *0.1f;
        }

        if (Keyboard.GetState().IsKeyDown(Keys.A))
        {
            yaw += 0.01f;
            _rotation = Vector3.Transform(_rotation,Matrix.CreateRotationY(0.01f));
            _position += Vector3.Transform(_rotation,Matrix.CreateRotationY(0.01f))*0.02f;

        }
        if (Keyboard.GetState().IsKeyDown(Keys.D))
        {
            yaw -= 0.01f;
            _rotation = Vector3.Transform(_rotation,Matrix.CreateRotationY(-0.01f));
            _position += Vector3.Transform(_rotation,Matrix.CreateRotationY(-0.01f))*0.02f;
        }
        
        
        World = localRotation * Matrix.CreateRotationY(yaw) * Matrix.CreateTranslation(_position);
    }

    public void Draw(Matrix View, Matrix Projection)
    {
        Effect.Parameters["View"].SetValue(View);
        Effect.Parameters["Projection"].SetValue(Projection);

        foreach (var mesh in Model.Meshes)
        {
            Effect.Parameters["World"].SetValue(mesh.ParentBone.Transform * World);
            mesh.Draw();
        }
    }
}