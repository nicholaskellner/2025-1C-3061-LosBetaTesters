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
    public int MaxHealth { get; private set; } = 100;
    public int CurrentHealth { get; private set; } = 100;

    public void TakeDamage(int amount)
    {
        CurrentHealth -= amount;
        if (CurrentHealth < 0) CurrentHealth = 0;
    }

    public bool IsDead => CurrentHealth <= 0;
    private GraphicsDevice graphicsDevice;
    private ContentManager content;
    private float elapsedTime = 0;
    private SoundEffect shootSound;
    SoundEffect movementSound;
    SoundEffectInstance movementSoundInstance;
    public delegate float TerrainHeightFunction(float x, float z);

    private float yaw = 0;
    private float turret_yaw = 0;
    private float turret_pitch = 0;
    private float speed = 0;
    private float rotationSpeed = 0.2f;

    private float wheelRotationRight = 0;
    private float wheelRotationLeft = 0;
    public Vector3 _position = new Vector3(0, 6, 0);

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
    public List<OrientedBoundingBox> MeshOBBs { get; private set; } = new();


    public Tank(ContentManager content, GraphicsDevice graphicsDevice)
    {
        CurrentHealth = MaxHealth;
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

    public void Update(GameTime gameTime, TerrainHeightFunction getHeight)
    {
        PreviousPosition = _position;
        bool isMoving = false;
        elapsedTime = Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);
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

        // ACTUALIZAR ALTURA DEL TANQUE CON EL TERRENO
        //_position.Y = getHeight(_position.X, _position.Z);
        _position.Y = getHeight(_position.X, _position.Z) - 18f;

        float modelBaseCorrection = -5f;

        World = Matrix.CreateScale(0.02f) * Matrix.CreateRotationY(yaw) * Matrix.CreateTranslation(_position);
        Mouse.SetPosition(910, 490);

        // Actualizar el temporizador de recarga
        reloadTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (Keyboard.GetState().IsKeyDown(Keys.Space) && reloadTimer <= 0.0f)
        {
            shootSound.Play();
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            reloadTimer -= dt;

            Matrix mYaw = Matrix.CreateRotationY(turret_yaw + yaw);
            Matrix mPitch = Matrix.CreateRotationX(-turret_pitch);
            turretRotation = Vector3.Transform(Vector3.Forward, mPitch * mYaw);
            turretRotation.Normalize();

            Vector3 shellPos = _position + turretRotation * 12f + new Vector3(0, 1f, 0);
            Vector3 shellDir = turretRotation;

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

        foreach (var shell in shells)
        {
            shell.Update(gameTime);
        }
        shells.RemoveAll(shell => shell.isExpired);

        UpdateMeshBoundingBoxes();
    }


    public void Draw(GraphicsDevice graphicsDevice, Matrix View, Matrix Projection, Vector3 cameraPosition, Matrix lightViewProjectionMatrix, RenderTarget2D shadowMap)
    {
        graphicsDevice.BlendState = BlendState.Opaque;
        graphicsDevice.DepthStencilState = DepthStencilState.Default;

        // Animaciones de ruedas
        var leftWheelRotation = Matrix.CreateRotationX(wheelRotationRight);
        var rightWheelRotation = Matrix.CreateRotationX(wheelRotationLeft);
        for (var i = 0; i < 8; i++)
        {
            leftWheels[i].Transform = leftWheelRotation * leftWheelsTransforms[i];
            rightWheels[i].Transform = rightWheelRotation * rightWheelsTransforms[i];
        }

        // Matrices de transformación del modelo
        Model.CopyAbsoluteBoneTransformsTo(_boneTransforms);

        // Rotaciones de torreta y cañón
        int turretIndex = Model.Bones["Turret"].Index;
        _boneTransforms[turretIndex] = Matrix.CreateRotationZ(turret_yaw) * _boneTransforms[turretIndex];

        int cannonIndex = Model.Bones["Cannon"].Index;
        _boneTransforms[cannonIndex] = Matrix.CreateRotationX(turret_pitch) * cannonRepo * Matrix.CreateRotationZ(turret_yaw) *
                                       Matrix.CreateTranslation(-0.08f, 1.3f, -0.3f) * _boneTransforms[cannonIndex];

        // Calcular WorldInverseTranspose (para iluminación)
        //Matrix worldInverseTranspose = Matrix.Transpose(Matrix.Invert(World));

        foreach (var mesh in Model.Meshes)
        {
            foreach (var meshPart in mesh.MeshParts)
            {
                var effect = Effect.Clone();
                meshPart.Effect = effect;

                effect.CurrentTechnique = effect.Techniques[0]; // Asegurarse de setear la técnica

                var localWorld = _boneTransforms[mesh.ParentBone.Index] * World;
                Matrix worldInverseTranspose = Matrix.Transpose(Matrix.Invert(localWorld));

                // Seteamos parámetros del shader
                effect.Parameters["World"]?.SetValue(localWorld);
                effect.Parameters["View"]?.SetValue(View);
                effect.Parameters["Projection"]?.SetValue(Projection);
                effect.Parameters["WorldInverseTranspose"]?.SetValue(worldInverseTranspose);

                effect.Parameters["cameraPosition"]?.SetValue(cameraPosition);
                effect.Parameters["lightPosition"]?.SetValue(new Vector3(50, 100, 30));

                effect.Parameters["ambientColor"]?.SetValue(new Vector3(1f, 1f, 1f));
                effect.Parameters["KAmbient"]?.SetValue(0.7f);                  // menos luz ambiental
                effect.Parameters["diffuseColor"]?.SetValue(new Vector3(0.5f, 0.5f, 0.5f)); // difusa más tenue
                effect.Parameters["specularColor"]?.SetValue(new Vector3(0.8f, 0.85f, 0.9f)); // brillo azul/plateado metálico
                effect.Parameters["shininess"]?.SetValue(80f); // brillo fuerte y concentrado
                effect.Parameters["LightViewProjection"]?.SetValue(lightViewProjectionMatrix);
                effect.Parameters["ShadowMap"]?.SetValue(shadowMap);

                // Textura según mesh
                if (mesh.Name == "Treadmill2" || mesh.Name == "Treadmill1")
                    effect.Parameters["Texture"]?.SetValue(TreadmillTexture);
                else
                    effect.Parameters["Texture"]?.SetValue(Texture);

                // Dibujar el meshPart
                graphicsDevice.SetVertexBuffer(meshPart.VertexBuffer);
                graphicsDevice.Indices = meshPart.IndexBuffer;

                foreach (var pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    graphicsDevice.DrawIndexedPrimitives(
                        PrimitiveType.TriangleList,
                        meshPart.VertexOffset,
                        meshPart.StartIndex,
                        meshPart.PrimitiveCount
                    );
                }
            }
        }

        // Dibujar proyectiles
        shell?.Draw(View, Projection);
        foreach (var s in shells)
            s.Draw(View, Projection);
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
        MeshOBBs.Clear(); // nuevo

        var boneTransforms = new Matrix[Model.Bones.Count];
        Model.CopyAbsoluteBoneTransformsTo(boneTransforms);

        for (int i = 0; i < Model.Meshes.Count; i++)
        {
            var mesh = Model.Meshes[i];
            var originalBox = originalMeshBoundingBoxes[i];
            var worldTransform = boneTransforms[mesh.ParentBone.Index] * World;

            // AABB tradicional (opcional)
            var transformedBox = TransformBoundingBox(originalBox, worldTransform);
            MeshBoundingBoxes.Add(transformedBox);

            // Nuevo: OBB
            var center = (originalBox.Min + originalBox.Max) / 2;
            var halfExtents = (originalBox.Max - originalBox.Min) / 2;
            var orientedCenter = Vector3.Transform(center, worldTransform);
            var orientation = worldTransform;
            orientation.Translation = Vector3.Zero; // solo rotación/escala

            MeshOBBs.Add(new OrientedBoundingBox(orientedCenter, halfExtents, orientation));
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


    public void CheckCollisionsWithEnemyShells(List<Shell> enemyShells)
    {
        foreach (var shell in enemyShells)
        {
            if (this.BoundingBox.Intersects(shell.BoundingBox))
            {
                TakeDamage(10);
                shell.isExpired = true;
            }
        }
    }   
    
     public void DrawShadow(GraphicsDevice graphicsDevice, Effect shadowEffect, Matrix lightViewProj)
{
    Matrix world = Matrix.CreateScale(1.5f)
                 * Matrix.CreateRotationX(-MathHelper.PiOver2)
                 * Matrix.CreateRotationY((float)Math.Atan2(_rotation.X, _rotation.Z))
                 * Matrix.CreateTranslation(_position);

    shadowEffect.CurrentTechnique = shadowEffect.Techniques["ShadowPass"];

    foreach (ModelMesh mesh in Model.Meshes)
    {
        foreach (ModelMeshPart part in mesh.MeshParts)
        {
            part.Effect = shadowEffect;
            shadowEffect.Parameters["World"].SetValue(world);
            shadowEffect.Parameters["LightViewProjection"].SetValue(lightViewProj);

            foreach (var pass in shadowEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                graphicsDevice.SetVertexBuffer(part.VertexBuffer);
                graphicsDevice.Indices = part.IndexBuffer;

                graphicsDevice.DrawIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    part.VertexOffset,
                    part.StartIndex,
                    part.PrimitiveCount);
            }
        }
    }
}
    
}