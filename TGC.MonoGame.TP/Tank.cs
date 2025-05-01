using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
public class Tank{
    public const string ContentFolder3D = "Models/";
    public const string ContentFolderEffects = "Effects/";
    public const string ContentFolderTextures = "Models/textures_mod/";
    private Model Model { get; set; }
    private Effect Effect { get; set; }
    private Texture2D Texture;
    public Vector3 _rotation { get; set; }
    private GraphicsDevice graphicsDevice;

    private float yaw = 0;
    private float turret_yaw = 0;
    private float speed = 0;
    public Vector3 _position = Vector3.Zero;

    private Matrix localRotation;

    private Matrix World { get; set; }
    private Matrix World2;
    private Matrix World3;

    public Tank(ContentManager content, GraphicsDevice graphicsDevice)
    {
        this.graphicsDevice = graphicsDevice;
        //Cree esta porque viene mirando para arriba el tanque.
        localRotation = Matrix.Identity * Matrix.CreateRotationX(MathHelper.ToRadians(-90)) * Matrix.CreateRotationY(MathHelper.ToRadians(90)) * Matrix.CreateTranslation(0,0,0);
        _rotation = Vector3.Transform(Vector3.Up,localRotation);
        Model = content.Load<Model>(ContentFolder3D + "T90");
        Effect = content.Load<Effect>(ContentFolderEffects + "ShaderTanque");
        Texture = content.Load<Texture2D>(ContentFolderTextures + "hullA");
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

        if (Mouse.GetState().X > 910)
        {
            turret_yaw -= 0.02f;
        }
        if (Mouse.GetState().X < 910)
        {
            turret_yaw += 0.02f;
        }
        
        speed = 0;
        World = localRotation * Matrix.CreateRotationY(yaw) * Matrix.CreateTranslation(_position);
        World2 = Matrix.Identity * Matrix.CreateRotationZ(turret_yaw) * Matrix.CreateRotationZ(MathHelper.ToRadians(180));
        //El World3 es para el cañon que lleva un offset
        World3 = Matrix.Identity *  Matrix.CreateTranslation(0.07f,-1.3f,0.3f) * Matrix.CreateRotationZ(turret_yaw) * Matrix.CreateRotationZ(MathHelper.ToRadians(180));
        Mouse.SetPosition(910,490);
    }

    public void Draw(GraphicsDevice graphicsDevice, Matrix View, Matrix Projection)
    {
        foreach (var mesh in Model.Meshes)
        {
            
            foreach (var effect in mesh.Effects)
            {
                effect.Parameters["World"].SetValue(mesh.ParentBone.Transform * World);
                effect.Parameters["View"].SetValue(View);
                effect.Parameters["Projection"].SetValue(Projection);
                effect.Parameters["Text"]?.SetValue(Texture);
                if (Model.Meshes[10] == mesh)
                    effect.Parameters["World"].SetValue(World2 * World);
                if (Model.Meshes[11] == mesh)
                    effect.Parameters["World"].SetValue(World3 * World);


                //effect.EnableDefaultLighting();
            }
            foreach (var meshPart in mesh.MeshParts){
                
                graphicsDevice.SetVertexBuffer(meshPart.VertexBuffer);
                graphicsDevice.Indices = meshPart.IndexBuffer;
                
                foreach (var effectPass in meshPart.Effect.CurrentTechnique.Passes){
                    effectPass.Apply();
                    graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList,meshPart.VertexOffset, meshPart.StartIndex,meshPart.PrimitiveCount);
                }
            }
        }
    }
}