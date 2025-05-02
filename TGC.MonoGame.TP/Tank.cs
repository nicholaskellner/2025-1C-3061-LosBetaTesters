using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using System;
public class Tank{
    public const string ContentFolder3D = "Models/";
    public const string ContentFolderEffects = "Effects/";
    public const string ContentFolderTextures = "Models/textures_mod/";
    private Model Model { get; set; }
    private Effect Effect { get; set; }
    private Texture2D Texture;
    private Texture2D TreadmillTexture;
    public Vector3 _rotation { get; set; }
    private GraphicsDevice graphicsDevice;
    private float elapsedTime = 0;

    private float yaw = 0;
    private float turret_yaw = 0;
    private float speed = 0;
    private float rotationSpeed = 0.2f;

    private float wheelRotationRight = 0;
    private float wheelRotationLeft = 0;
    public Vector3 _position = Vector3.Zero;

    private Matrix localRotation;

    private Matrix World { get; set; }
    private Matrix World2;
    private Matrix World3;
    private Matrix World4;


    public Tank(ContentManager content, GraphicsDevice graphicsDevice)
    {
        this.graphicsDevice = graphicsDevice;
        //Cree esta porque viene mirando para arriba el tanque.
        localRotation = Matrix.Identity * Matrix.CreateRotationX(MathHelper.ToRadians(-90)) * Matrix.CreateRotationY(MathHelper.ToRadians(90));
        _rotation = Vector3.Transform(Vector3.Up,localRotation);
        Model = content.Load<Model>(ContentFolder3D + "T90");
        Effect = content.Load<Effect>(ContentFolderEffects + "ShaderTanque");
        Texture = content.Load<Texture2D>(ContentFolderTextures + "hullA");
        TreadmillTexture = content.Load<Texture2D>(ContentFolderTextures + "treadmills");
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
        if (Keyboard.GetState().IsKeyDown(Keys.W))
        {
            speed = 1f;
            _position += _rotation * speed * elapsedTime * 3f;
        }

        if (Keyboard.GetState().IsKeyDown(Keys.S))
        {
            speed = -1f;
            _position += _rotation * speed * elapsedTime * 3f;
        }

        if (Keyboard.GetState().IsKeyDown(Keys.A))
        {
            if (speed < 0){
                yaw = yaw - rotationSpeed * elapsedTime;
                _rotation = Vector3.Transform(_rotation,Matrix.CreateRotationY(-rotationSpeed * elapsedTime));
                wheelRotationRight += elapsedTime;
            }
                
            else{
                yaw = yaw + rotationSpeed * elapsedTime;
                _rotation = Vector3.Transform(_rotation,Matrix.CreateRotationY(rotationSpeed * elapsedTime));
                wheelRotationRight -= elapsedTime;
            }
            _position += _rotation*0.01f * elapsedTime;
        }
        if (Keyboard.GetState().IsKeyDown(Keys.D))
        {
            if (speed < 0){
                yaw = yaw + rotationSpeed * elapsedTime;
                _rotation = Vector3.Transform(_rotation,Matrix.CreateRotationY(rotationSpeed * elapsedTime));
                wheelRotationLeft += elapsedTime;
            }
                
            else{
                yaw = yaw - rotationSpeed * elapsedTime;
                _rotation = Vector3.Transform(_rotation,Matrix.CreateRotationY(-rotationSpeed * elapsedTime));
                wheelRotationLeft -= elapsedTime;
            }
            _position += _rotation*0.015f;
        }

        if (Mouse.GetState().X > 910)
        {
            turret_yaw -= elapsedTime;
        }
        if (Mouse.GetState().X < 910)
        {
            turret_yaw += elapsedTime;
        }
        
        wheelRotationRight -= speed * elapsedTime;
        wheelRotationLeft -= speed * elapsedTime;
        World = localRotation * Matrix.CreateRotationY(yaw) * Matrix.CreateTranslation(_position);
        World2 = Matrix.Identity * Matrix.CreateRotationZ(turret_yaw) * Matrix.CreateRotationZ(MathHelper.ToRadians(180));
        //El World3 es para el cañon que lleva un offset
        World3 = Matrix.Identity *  Matrix.CreateTranslation(0.07f,-1.3f,0.3f) * Matrix.CreateRotationZ(turret_yaw) * Matrix.CreateRotationZ(MathHelper.ToRadians(180));
        World4 = Matrix.Identity  * Matrix.CreateRotationX(wheelRotationRight) * Matrix.CreateTranslation(-0.1f,0.62f,0);//Matrix.CreateTranslation(Vector3.Transform(_rotation, Matrix.CreateRotationX(MathHelper.ToRadians(90)))*1.5f);
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
                if (Model.Meshes[10] == mesh)
                    effect.Parameters["World"].SetValue(World2 * World);
                else if (Model.Meshes[11] == mesh)
                    effect.Parameters["World"].SetValue(World3 * World);
                else if (Model.Meshes[4] == mesh)
                    effect.Parameters["World"].SetValue(Matrix.CreateRotationX(wheelRotationRight) * Matrix.CreateTranslation(0,5.75f,0) * mesh.ParentBone.Transform * World);
                else if (Model.Meshes[2] == mesh)
                    effect.Parameters["World"].SetValue(Matrix.CreateRotationX(wheelRotationRight) * Matrix.CreateTranslation(-0.1f,-4.85f,0.03f) * mesh.ParentBone.Transform * World);
                else if (Model.Meshes[3] == mesh || Model.Meshes[5] == mesh || Model.Meshes[6] == mesh || Model.Meshes[7] == mesh || Model.Meshes[8] == mesh || Model.Meshes[9] == mesh)
                    effect.Parameters["World"].SetValue(World4 * mesh.ParentBone.Transform * World);
                else if (Model.Meshes[16] == mesh || Model.Meshes[13] == mesh || Model.Meshes[14] == mesh || Model.Meshes[15] == mesh || Model.Meshes[18] == mesh || Model.Meshes[17] == mesh || Model.Meshes[19] == mesh || Model.Meshes[20] == mesh)
                    effect.Parameters["World"].SetValue(Matrix.CreateRotationX(wheelRotationLeft) * mesh.ParentBone.Transform * World);

                //effect.EnableDefaultLighting();
            }
            
            foreach (var meshPart in mesh.MeshParts){
                
                graphicsDevice.SetVertexBuffer(meshPart.VertexBuffer);
                graphicsDevice.Indices = meshPart.IndexBuffer;
                if (Model.Meshes[12] != mesh && Model.Meshes[1] != mesh)
                    meshPart.Effect.Parameters["Texture"].SetValue(Texture);
                else
                    meshPart.Effect.Parameters["Texture"].SetValue(TreadmillTexture);

                foreach (var effectPass in meshPart.Effect.CurrentTechnique.Passes){
                    effectPass.Apply();
                    graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList,meshPart.VertexOffset, meshPart.StartIndex,meshPart.PrimitiveCount);
                }
            }
        }
    }
}