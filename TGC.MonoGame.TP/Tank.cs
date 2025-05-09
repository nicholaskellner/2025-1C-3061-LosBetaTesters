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
    public Vector3 _rotation { get; set; } = Vector3.Forward;
    private GraphicsDevice graphicsDevice;
    private float elapsedTime = 0;

    private float yaw = 0;
    private float turret_yaw = 0;
    private float speed = 0;
    private float rotationSpeed = 0.2f;

    private float wheelRotationRight = 0;
    private float wheelRotationLeft = 0;
    public Vector3 _position = Vector3.Zero;

    private Matrix World { get; set; }
    private Matrix[] _boneTransforms;
    private Matrix cannonRepo = Matrix.CreateTranslation(0.08f,-1.3f,0.3f);

    private ModelBone[] leftWheels = new ModelBone[8];
    private Matrix[] leftWheelsTransforms = new Matrix[8];

    private ModelBone[] rightWheels = new ModelBone[8];
    private Matrix[] rightWheelsTransforms = new Matrix[8];

    public Tank(ContentManager content, GraphicsDevice graphicsDevice)
    {
        this.graphicsDevice = graphicsDevice;
        //Cree esta porque viene mirando para arriba el tanque.
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
        _boneTransforms = new Matrix[Model.Bones.Count];
        for(var i = 0;i<8;i++){
            rightWheels[i] = Model.Bones["Wheel"+(i+1)];
            rightWheelsTransforms[i] = Model.Bones["Wheel"+(i+1)].Transform;
            leftWheels[i] = Model.Bones["Wheel"+(i+9)];
            leftWheelsTransforms[i] = Model.Bones["Wheel"+(i+9)].Transform;
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
                wheelRotationRight -= elapsedTime;
            }
                
            else{
                yaw = yaw + rotationSpeed * elapsedTime;
                _rotation = Vector3.Transform(_rotation,Matrix.CreateRotationY(rotationSpeed * elapsedTime));
                wheelRotationRight += elapsedTime;
            }
            _position += _rotation*0.01f * elapsedTime;
        }
        if (Keyboard.GetState().IsKeyDown(Keys.D))
        {
            if (speed < 0){
                yaw = yaw + rotationSpeed * elapsedTime;
                _rotation = Vector3.Transform(_rotation,Matrix.CreateRotationY(rotationSpeed * elapsedTime));
                wheelRotationLeft -= elapsedTime;
            }
                
            else{
                yaw = yaw - rotationSpeed * elapsedTime;
                _rotation = Vector3.Transform(_rotation,Matrix.CreateRotationY(-rotationSpeed * elapsedTime));
                wheelRotationLeft += elapsedTime;
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
        
        wheelRotationRight += speed * elapsedTime;
        wheelRotationLeft += speed * elapsedTime;
        World = Matrix.CreateScale(0.02f) * Matrix.CreateRotationY(yaw) * Matrix.CreateTranslation(_position) * Matrix.CreateTranslation(0,1f,0);
        Mouse.SetPosition(910,490);
    }

    public void Draw(GraphicsDevice graphicsDevice, Matrix View, Matrix Projection)
    {
        var leftWheelRotation = Matrix.CreateRotationX(wheelRotationRight);
        var rightWheelRotation = Matrix.CreateRotationX(wheelRotationLeft);
        for(var i = 0;i<8;i++){
            leftWheels[i].Transform = leftWheelRotation * leftWheelsTransforms[i];
            rightWheels[i].Transform = rightWheelRotation * rightWheelsTransforms[i];
        }
        Model.CopyAbsoluteBoneTransformsTo(_boneTransforms);
        int indiceHueso = Model.Bones["Turret"].Index;
        _boneTransforms[indiceHueso] = Matrix.CreateRotationZ(turret_yaw) * _boneTransforms[indiceHueso];
        int indiceCannon = Model.Bones["Cannon"].Index;
        _boneTransforms[indiceCannon] = cannonRepo * Matrix.CreateRotationZ(turret_yaw) *  Matrix.CreateTranslation(-0.08f,1.3f,-0.3f) * _boneTransforms[indiceCannon];

        foreach (var mesh in Model.Meshes)
        {
            foreach (var effect in mesh.Effects)
            {
                effect.Parameters["World"].SetValue(_boneTransforms[mesh.ParentBone.Index] * World);
                effect.Parameters["View"].SetValue(View);
                effect.Parameters["Projection"].SetValue(Projection);
                effect.Parameters["ambientColor"].SetValue(new Vector3(1f, 1f,1f));

                effect.Parameters["KAmbient"].SetValue(0.5f);
                //effect.EnableDefaultLighting();
            }
            
            foreach (var meshPart in mesh.MeshParts){
                graphicsDevice.SetVertexBuffer(meshPart.VertexBuffer);
                graphicsDevice.Indices = meshPart.IndexBuffer;
                if (Model.Meshes["Treadmill2"] == mesh || Model.Meshes["Treadmill1"] == mesh)
                    meshPart.Effect.Parameters["Texture"].SetValue(TreadmillTexture);
                else
                    meshPart.Effect.Parameters["Texture"].SetValue(Texture);
                foreach (var effectPass in meshPart.Effect.CurrentTechnique.Passes){
                    effectPass.Apply();
                    graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList,meshPart.VertexOffset, meshPart.StartIndex,meshPart.PrimitiveCount);
                }
            }
        }
    }
}