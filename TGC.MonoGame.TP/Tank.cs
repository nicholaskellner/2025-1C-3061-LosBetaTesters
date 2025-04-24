using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
public class Tank{
    public const string ContentFolder3D = "Models/";
    public const string ContentFolderEffects = "Effects/";
    private Model Model { get; set; }
    private Effect Effect { get; set; }
    public Vector3 _rotation { get; set; }

    private float yaw = 0;
    private float speed = 0;
    public Vector3 _position = Vector3.Zero;

    private Matrix localRotation;

    private Matrix World { get; set; }

    public Tank(ContentManager content)
    {
        //Cree esta porque viene mirando para arriba el tanque.
        localRotation = Matrix.Identity * Matrix.CreateRotationX(MathHelper.ToRadians(-90)) * Matrix.CreateRotationY(MathHelper.ToRadians(90));
        _rotation = Vector3.Transform(Vector3.Up,localRotation);
        Model = content.Load<Model>(ContentFolder3D + "T90");
        Effect = content.Load<Effect>(ContentFolderEffects + "BasicShader");
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

    public void Update(GameTime gameTime)
    {
        
        
        // Falta el elapsed time y velocidades
        if (Keyboard.GetState().IsKeyDown(Keys.W))
        {
            speed = 1f;
            _position += _rotation *0.1f * speed;
        }

        if (Keyboard.GetState().IsKeyDown(Keys.S))
        {
            speed = -1f;
            _position += _rotation *0.1f * speed;
        }

        if (Keyboard.GetState().IsKeyDown(Keys.A))
        {
            if (speed < 0){
                yaw = yaw - 0.01f;
                _rotation = Vector3.Transform(_rotation,Matrix.CreateRotationY(-0.01f));
            }
                
            else{
                yaw = yaw + 0.01f;
                _rotation = Vector3.Transform(_rotation,Matrix.CreateRotationY(0.01f));
            }
            _position += _rotation*0.015f;
        }
        if (Keyboard.GetState().IsKeyDown(Keys.D))
        {
            if (speed < 0){
                yaw = yaw + 0.01f;
                _rotation = Vector3.Transform(_rotation,Matrix.CreateRotationY(0.01f));
            }
                
            else{
                yaw = yaw - 0.01f;
                _rotation = Vector3.Transform(_rotation,Matrix.CreateRotationY(-0.01f));
            }
            _position += _rotation*0.015f;
        }
        
        speed = 0;
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