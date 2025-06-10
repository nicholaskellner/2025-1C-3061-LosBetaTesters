using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
public class Tank
{
    public const string ContentFolder3D = "Models/";
    public const string ContentFolderEffects = "Effects/";
    public const string ContentFolderTextures = "Models/textures_mod/";
    private Model Model { get; set; }
    private Model ShellModel { get; set; }
    private Effect Effect { get; set; }
    private Effect ShellEffect { get; set; }
    private Texture2D Texture;
    private Texture2D TreadmillTexture;
    public Vector3 _rotation { get; set; } = Vector3.Forward;
    public Vector3 turretRotation { get; set; } = Vector3.Forward;
    private GraphicsDevice graphicsDevice;
    private ContentManager content;
    private float elapsedTime = 0;
    private SoundEffect shootSound;
    SoundEffect movementSound;
    SoundEffectInstance movementSoundInstance;

    private float yaw = 0;
    private float turret_yaw = 0;
    private float turret_pitch = 0;
    private float speed = 0;
    private float rotationSpeed = 0.2f;

    private float wheelRotationRight = 0;
    private float wheelRotationLeft = 0;
    public Vector3 _position = new Vector3(0,6,0);

    public Matrix World { get; set; }
    private Matrix[] _boneTransforms;
    private Matrix cannonRepo = Matrix.CreateTranslation(0.08f, -1.3f, 0.3f); // 1.2f adelante, 0.2f arriba


    private ModelBone[] leftWheels = new ModelBone[8];
    private Matrix[] leftWheelsTransforms = new Matrix[8];

    private ModelBone[] rightWheels = new ModelBone[8];
    private Matrix[] rightWheelsTransforms = new Matrix[8];
    private Shell shell = null;

    public Vector3 PreviousPosition { get; private set; }
    public void RevertPosition() => _position = PreviousPosition;
    /*
    public BoundingBox BoundingBox => new BoundingBox(
    _position - new Vector3(1f, 0f, 1f),
    _position + new Vector3(1f, 2f, 1f)
);*/
    public BoundingBox BoundingBox => new BoundingBox(
    _position - new Vector3(1f, 0.5f, 1f),
    _position + new Vector3(1f, 1.5f, 1f)
);
    public List<BoundingBox> MeshBoundingBoxes { get; private set; } = new();
    private List<BoundingBox> originalMeshBoundingBoxes = new();

    //props para disparar
    private ModelBone cannonBone;
    private Matrix cannonBaseTransform;

    private readonly Vector3 cannonMuzzleOffset = new Vector3(0f, 0f, 1.5f);


    private float reloadTime = 2.0f; // tiempo de recarga en segundos
    private float reloadTimer = 0.0f;
    public List<Shell> shells = new List<Shell>(); // lista de balas disparadas


    public Tank(ContentManager content, GraphicsDevice graphicsDevice)
    {
        this.graphicsDevice = graphicsDevice;
        this.content = content;

        //Cree esta porque viene mirando para arriba el tanque.
        Model = content.Load<Model>(ContentFolder3D + "T90");
        Effect = content.Load<Effect>(ContentFolderEffects + "ShaderTanque");
        Texture = content.Load<Texture2D>(ContentFolderTextures + "hullA");
        ShellModel = content.Load<Model>(ContentFolder3D + "shell");
        ShellEffect = content.Load<Effect>(ContentFolderEffects + "ShaderShell");
        TreadmillTexture = content.Load<Texture2D>(ContentFolderTextures + "treadmills");
        shootSound = content.Load<SoundEffect>("Sounds/shoot");
        movementSound = content.Load<SoundEffect>("Sounds/movement_sound"); // pon el nombre correcto del archivo
        movementSoundInstance = movementSound.CreateInstance();
        movementSoundInstance.IsLooped = true; 


        cannonBone = Model.Bones["Cannon"];
        cannonBaseTransform = cannonBone.Transform;
        foreach (var mesh in Model.Meshes)
        {
            // Un mesh puede tener mas de 1 mesh part (cada 1 puede tener su propio efecto).
            foreach (var meshPart in mesh.MeshParts)
            {
                meshPart.Effect = Effect;
            }
        }
        _boneTransforms = new Matrix[Model.Bones.Count];
        for (var i = 0; i < 8; i++)
        {
            rightWheels[i] = Model.Bones["Wheel" + (i + 1)];
            rightWheelsTransforms[i] = Model.Bones["Wheel" + (i + 1)].Transform;
            leftWheels[i] = Model.Bones["Wheel" + (i + 9)];
            leftWheelsTransforms[i] = Model.Bones["Wheel" + (i + 9)].Transform;
        }
        GenerateOriginalMeshBoundingBoxes();
    }

    public void Update(GameTime gameTime)
    {
        PreviousPosition = _position;
        bool isMoving = false;
        elapsedTime = Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);
        //Falta agregar acceleracion si es que los tanques tienen
        speed = 0;
        if (Keyboard.GetState().IsKeyDown(Keys.W))
        {
            isMoving = true;
            speed = 1f;
            _position += _rotation * speed * elapsedTime * 3f;
        }

        if (Keyboard.GetState().IsKeyDown(Keys.S))
        {
            isMoving = true;
            speed = -1f;
            _position += _rotation * speed * elapsedTime * 3f;
        }

        if (Keyboard.GetState().IsKeyDown(Keys.A))
        {
            isMoving = true;
            if (speed < 0)
            {
                yaw -= rotationSpeed * elapsedTime;
                _rotation = Vector3.Transform(_rotation, Matrix.CreateRotationY(-rotationSpeed * elapsedTime));
                wheelRotationRight -= elapsedTime;
            }

            else
            {
                yaw += rotationSpeed * elapsedTime;
                _rotation = Vector3.Transform(_rotation, Matrix.CreateRotationY(rotationSpeed * elapsedTime));
                wheelRotationRight += elapsedTime;
            }
            _position += _rotation * 0.01f * elapsedTime;
        }

        if (Keyboard.GetState().IsKeyDown(Keys.D))
        {
            isMoving = true;
            if (speed < 0)
            {
                yaw += rotationSpeed * elapsedTime;
                _rotation = Vector3.Transform(_rotation, Matrix.CreateRotationY(rotationSpeed * elapsedTime));
                wheelRotationLeft -= elapsedTime;
            }

            else
            {
                yaw -= rotationSpeed * elapsedTime;
                _rotation = Vector3.Transform(_rotation, Matrix.CreateRotationY(-rotationSpeed * elapsedTime));
                wheelRotationLeft += elapsedTime;
            }
            _position += _rotation * 0.015f;
        }

        if (Mouse.GetState().X > 910)
        {
            turret_yaw -= elapsedTime * 0.1f;
        }
        else if (Mouse.GetState().X < 910)
        {
            turret_yaw += elapsedTime * 0.1f;
        }

        if (Mouse.GetState().Y > 490)
        {
            turret_pitch += elapsedTime * 0.1f;
            turret_pitch = Math.Min(turret_pitch, 0.2f);
        }
        else if (Mouse.GetState().Y < 490)
        {
            turret_pitch -= elapsedTime * 0.1f;
            turret_pitch = Math.Max(turret_pitch, -0.2f);
        }

        wheelRotationRight += speed * elapsedTime;
        wheelRotationLeft += speed * elapsedTime;
        //World = Matrix.CreateScale(0.02f) * Matrix.CreateRotationY(yaw) * Matrix.CreateTranslation(_position) * Matrix.CreateTranslation(0, 1f, 0);
        World = Matrix.CreateScale(0.02f) * Matrix.CreateRotationY(yaw) * Matrix.CreateTranslation(_position);
        Mouse.SetPosition(910, 490);

        // Actualizar el temporizador de recarga
        reloadTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (Keyboard.GetState().IsKeyDown(Keys.Space) && reloadTimer <= 0.0f)
        {
            shootSound.Play();
            var ms = Mouse.GetState();
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            reloadTimer -= dt;

            Matrix mYaw = Matrix.CreateRotationY(turret_yaw + yaw);
            Matrix mPitch = Matrix.CreateRotationX(-turret_pitch);
            turretRotation = Vector3.Transform(Vector3.Forward, mPitch * mYaw);
            turretRotation.Normalize();

            Vector3 shellPos = _position
                                + turretRotation * 12f + new Vector3(0, 1f, 0);


            Vector3 shellDir = turretRotation;

            // Crear la shell
            shells.Add(new Shell(ShellModel, ShellEffect, shellPos, shellDir));
            reloadTimer = reloadTime;
        }
        if (isMoving)
        {
            if (movementSoundInstance.State != SoundState.Playing)
                movementSoundInstance.Play();
        }
        else
        {
            if (movementSoundInstance.State == SoundState.Playing)
                movementSoundInstance.Pause();
        }

        // Actualizar balas activas
        foreach (var shell in shells)
        {
            shell.Update(gameTime);
        }
        shells.RemoveAll(shell => shell.isExpired);
        UpdateMeshBoundingBoxes();

    }

    public void Draw(GraphicsDevice graphicsDevice, Matrix View, Matrix Projection)
    {
        var leftWheelRotation = Matrix.CreateRotationX(wheelRotationRight);
        var rightWheelRotation = Matrix.CreateRotationX(wheelRotationLeft);
        for (var i = 0; i < 8; i++)
        {
            leftWheels[i].Transform = leftWheelRotation * leftWheelsTransforms[i];
            rightWheels[i].Transform = rightWheelRotation * rightWheelsTransforms[i];
        }
        Model.CopyAbsoluteBoneTransformsTo(_boneTransforms);
        int indiceHueso = Model.Bones["Turret"].Index;
        _boneTransforms[indiceHueso] = Matrix.CreateRotationZ(turret_yaw) * _boneTransforms[indiceHueso];
        int indiceCannon = Model.Bones["Cannon"].Index;
        _boneTransforms[indiceCannon] = Matrix.CreateRotationX(turret_pitch) * cannonRepo * Matrix.CreateRotationZ(turret_yaw) * Matrix.CreateTranslation(-0.08f, 1.3f, -0.3f) * _boneTransforms[indiceCannon];

        foreach (var mesh in Model.Meshes)
        {
            foreach (var effect in mesh.Effects)
            {
                effect.Parameters["World"].SetValue(_boneTransforms[mesh.ParentBone.Index] * World);
                effect.Parameters["View"].SetValue(View);
                effect.Parameters["Projection"].SetValue(Projection);
                effect.Parameters["ambientColor"].SetValue(new Vector3(1f, 1f, 1f));

                effect.Parameters["KAmbient"].SetValue(0.5f);
                //effect.EnableDefaultLighting();
            }

            foreach (var meshPart in mesh.MeshParts)
            {
                graphicsDevice.SetVertexBuffer(meshPart.VertexBuffer);
                graphicsDevice.Indices = meshPart.IndexBuffer;
                if (Model.Meshes["Treadmill2"] == mesh || Model.Meshes["Treadmill1"] == mesh)
                    meshPart.Effect.Parameters["Texture"].SetValue(TreadmillTexture);
                else
                    meshPart.Effect.Parameters["Texture"].SetValue(Texture);
                foreach (var effectPass in meshPart.Effect.CurrentTechnique.Passes)
                {
                    effectPass.Apply();
                    graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, meshPart.VertexOffset, meshPart.StartIndex, meshPart.PrimitiveCount);
                }
            }
        }
        if (shell != null)
            shell.Draw(View, Projection);

        foreach (var shell in shells)
        {
            shell.Draw(View, Projection);
        }


    }
    private void GenerateOriginalMeshBoundingBoxes()
    {
        foreach (var mesh in Model.Meshes)
        {
            var vertices = new List<Vector3>();

            foreach (var part in mesh.MeshParts)
            {
                var vertexData = new VertexPositionNormalTexture[part.NumVertices];
                part.VertexBuffer.GetData(part.VertexOffset * part.VertexBuffer.VertexDeclaration.VertexStride, vertexData, 0, part.NumVertices, part.VertexBuffer.VertexDeclaration.VertexStride);

                vertices.AddRange(Array.ConvertAll(vertexData, v => v.Position));
            }

            if (vertices.Count > 0)
                originalMeshBoundingBoxes.Add(BoundingBox.CreateFromPoints(vertices));
            else
                originalMeshBoundingBoxes.Add(new BoundingBox());
        }
    }

    private void UpdateMeshBoundingBoxes()
    {
        MeshBoundingBoxes.Clear();
        var boneTransforms = new Matrix[Model.Bones.Count];
        Model.CopyAbsoluteBoneTransformsTo(boneTransforms);

        for (int i = 0; i < Model.Meshes.Count; i++)
        {
            var mesh = Model.Meshes[i];
            var originalBox = originalMeshBoundingBoxes[i];
            var worldTransform = boneTransforms[mesh.ParentBone.Index] * World;

            var transformedBox = TransformBoundingBox(originalBox, worldTransform);
            MeshBoundingBoxes.Add(transformedBox);
        }
    }

    private BoundingBox TransformBoundingBox(BoundingBox box, Matrix transform)
    {
        var corners = box.GetCorners();
        var transformedCorners = new Vector3[corners.Length];

        for (int i = 0; i < corners.Length; i++)
            transformedCorners[i] = Vector3.Transform(corners[i], transform);

        return BoundingBox.CreateFromPoints(transformedCorners);
    }
    
    
}