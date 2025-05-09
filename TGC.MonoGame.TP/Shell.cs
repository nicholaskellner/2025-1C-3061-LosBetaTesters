using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using System;
public class Shell{
    public const string ContentFolder3D = "Models/";
    public const string ContentFolderEffects = "Effects/";
    public const string ContentFolderTextures = "Models/textures_mod/";
    private Model Model { get; set; }
    private Effect Effect { get; set; }
    public Vector3 _direction { get; set; } = Vector3.Forward;
    private GraphicsDevice graphicsDevice;
    private float elapsedTime = 0;

    private float yaw = 0;
    private float speed = 0;
    private float rotationSpeed = 0.2f;
    public Vector3 _position;

    private Matrix World { get; set; }


    public Shell(Model model, Effect effect, Vector3 position, Vector3 direction)
    {
        this.Model = model;
        this.Effect = effect;
        _position = new Vector3(position.X,position.Y+3f,position.Z);
        _direction = Vector3.Transform(new Vector3(direction.Z,direction.Y,direction.X),Matrix.CreateRotationY(MathHelper.ToRadians(-90)));
        foreach (var mesh in Model.Meshes)
        {
            // Un mesh puede tener mas de 1 mesh part (cada 1 puede tener su propio efecto).
            foreach (var meshPart in mesh.MeshParts)
            {
                meshPart.Effect = Effect;
            }
        }
    }

    public void Update(GameTime gameTime)
    {
        elapsedTime = Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);
        //Falta agregar acceleracion si es que los tanques tienen
        speed = 0;
        World = Matrix.CreateScale(0.1f) * Matrix.CreateLookAt(Vector3.Zero,_direction,Vector3.Up) * Matrix.CreateTranslation(_position);
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
                effect.Parameters["ambientColor"]?.SetValue(new Vector3(1f, 1f,1f));

                effect.Parameters["KAmbient"]?.SetValue(0.5f);
                //effect.EnableDefaultLighting();
            }
            mesh.Draw();
        }
    }
}