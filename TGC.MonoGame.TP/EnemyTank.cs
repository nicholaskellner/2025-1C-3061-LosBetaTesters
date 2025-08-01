using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

public class EnemyTank
{
    public Vector3 _position;
    public Vector3 _rotation = Vector3.Forward;
    private GraphicsDevice graphicsDevice;
    private ContentManager content;

    private Model Model;
    private Effect Effect;
    private Texture2D Texture;
    private Model ShellModel;
    private Effect ShellEffect;

    private float reloadTime = 10.0f;
    private float reloadTimer = 0f;

    public List<Shell> shells = new List<Shell>();
    private Random random = new Random();

    public int MaxHealth { get; private set; } = 50;
    public int CurrentHealth { get; private set; } = 50;

    public bool IsDead => CurrentHealth <= 0;

    public delegate float TerrainHeightFunction(float x, float z);

    public EnemyTank(ContentManager content, GraphicsDevice graphicsDevice)
    {
        this.graphicsDevice = graphicsDevice;
        this.content = content;

        Model = content.Load<Model>("Models/T90");
        Effect = content.Load<Effect>("Effects/ShaderTanque");
        Texture = content.Load<Texture2D>("Models/textures_mod/hullA");

        ShellModel = content.Load<Model>("Models/shell");
        ShellEffect = content.Load<Effect>("Effects/ShaderShell");
    }

    public void Update(GameTime gameTime, TerrainHeightFunction getHeight, Vector3 playerPosition, List<Shell> playerShells, List<EnemyTank> allEnemies)
    {
        if (IsDead) return;

        float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

        Vector3 toPlayer = playerPosition - _position;
        toPlayer.Y = 0;

        float distanceToPlayer = toPlayer.Length();

        Vector3 moveDirection = Vector3.Zero;

        float minDistance = 30f; // distancia mínima para acercarse

        if (distanceToPlayer > minDistance)
        {
            // Si está lejos, se acerca hacia el jugador
            moveDirection = Vector3.Normalize(toPlayer);
        }
        else
        {
            // Si está cerca o igual a la distancia mínima, se mueve hacia un costado para esquivar

            // Para el caso que distanciaToPlayer sea cero (ej: mismo punto), evitar dividir por cero
            Vector3 dirNormalized = (distanceToPlayer > 0.001f) ? Vector3.Normalize(toPlayer) : Vector3.Forward;

            // Movimiento lateral: vector perpendicular en XZ
            moveDirection = new Vector3(-dirNormalized.Z, 0, dirNormalized.X);

            // Alternar costado para evitar que todos vayan al mismo lado
            if (random.NextDouble() > 0.5)
                moveDirection = -moveDirection;
        }

        // --- Separación con otros enemigos ---
        Vector3 separation = Vector3.Zero;
        float separationRadius = 15f;
        foreach (var other in allEnemies)
        {
            if (other == this || other.IsDead) continue;

            Vector3 toOther = _position - other._position;
            float dist = toOther.Length();

            if (dist < separationRadius && dist > 0.01f)
            {
                toOther.Normalize();
                float strength = (separationRadius - dist) / separationRadius;
                separation += toOther * strength;
            }
        }

        // Combinar dirección con separación
        Vector3 finalDirection = moveDirection + separation;
        if (finalDirection.LengthSquared() > 0.001f)
        {
            finalDirection.Normalize();

            float speed = 1.5f;
            _position += finalDirection * speed * elapsed;

            _rotation = finalDirection;
        }

        // Ajustar altura al terreno
        _position.Y = getHeight(_position.X, _position.Z) - 18f;

        // Colisiones con balas del jugador
        foreach (var shell in playerShells)
        {
            if (this.GetBoundingBox().Intersects(shell.BoundingBox))
            {
                TakeDamage(10); // Daño arbitrario
                shell.isExpired = true;
            }
        }

        // Disparo con cooldown
        reloadTimer -= elapsed;
        if (reloadTimer <= 0f)
        {
            TryShoot(playerPosition);
            reloadTimer = reloadTime;
        }

        // Actualizar balas propias
        foreach (var shell in shells)
            shell.Update(gameTime);
        shells.RemoveAll(s => s.isExpired);
    }



    // Método para obtener BoundingBox del tanque
    public BoundingBox GetBoundingBox()
    {
        float size = 2f; // Ajusta según tu modelo
        return new BoundingBox(_position - new Vector3(size, size, size), _position + new Vector3(size, size, size));
    }

    private void TryShoot(Vector3 playerPosition)
    {
        // Probabilidad de acierto del 30%
        if (random.NextDouble() <= 0.3)
        {
            Vector3 shootDir = Vector3.Normalize(playerPosition - _position);
            Vector3 shootPos = _position + shootDir * 5 + new Vector3(0, 2, 0);

            shells.Add(new Shell(ShellModel, ShellEffect, shootPos, shootDir));
            // Aquí podrías agregar sonido si querés
        }
    }

    public void TakeDamage(int amount)
    {
        CurrentHealth -= amount;
        if (CurrentHealth < 0) CurrentHealth = 0;
    }

    public void Draw(GraphicsDevice graphicsDevice, Matrix view, Matrix projection, Vector3 cameraPosition,Matrix lightViewProjectionMatrix, RenderTarget2D shadowMap)
    {
        if (IsDead) return;

        Matrix world = Matrix.CreateScale(1.5f)
                     * Matrix.CreateRotationX(-MathHelper.PiOver2)
                     * Matrix.CreateRotationY((float)Math.Atan2(_rotation.X, _rotation.Z))
                     * Matrix.CreateTranslation(_position);

        Matrix worldInvTrans = Matrix.Transpose(Matrix.Invert(world));

        foreach (var mesh in Model.Meshes)
        {

            foreach (var meshPart in mesh.MeshParts)
            {
                // ⚠️ Usar TU shader
                var effect = Effect.Clone();
                meshPart.Effect = effect;

                // Establecer técnica (por si acaso)
                effect.CurrentTechnique = effect.Techniques[0];

                // Setear parámetros
                if (mesh.Name.Equals("Wheel1") || mesh.Name.Equals("Wheel2") ||
                    mesh.Name.Equals("Wheel3") || mesh.Name.Equals("Wheel4") ||
                    mesh.Name.Equals("Wheel5") || mesh.Name.Equals("Wheel6") ||
                    mesh.Name.Equals("Wheel7") || mesh.Name.Equals("Wheel8") ||
                    mesh.Name.Equals("Wheel9") || mesh.Name.Equals("Wheel10") ||
                    mesh.Name.Equals("Wheel11") || mesh.Name.Equals("Wheel12") ||
                    mesh.Name.Equals("Wheel13") || mesh.Name.Equals("Wheel14") ||
                    mesh.Name.Equals("Wheel15") || mesh.Name.Equals("Wheel16") ||
                    mesh.Name.Equals("Wheel17") || mesh.Name.Equals("Wheel18") ||
                    mesh.Name.Equals("Wheel19"))
                {
                    effect.Parameters["World"]?.SetValue(mesh.ParentBone.Transform * world);
                }

                else effect.Parameters["World"]?.SetValue(world);
                effect.Parameters["View"]?.SetValue(view);
                effect.Parameters["Projection"]?.SetValue(projection);
                effect.Parameters["WorldInverseTranspose"]?.SetValue(worldInvTrans);

                effect.Parameters["ambientColor"]?.SetValue(new Vector3(1f, 1f, 1f));
                effect.Parameters["lightPosition"]?.SetValue(new Vector3(50, 50, 30));
                effect.Parameters["cameraPosition"]?.SetValue(cameraPosition);
                effect.Parameters["diffuseColor"]?.SetValue(new Vector3(0.5f, 0.5f, 0.5f)); // difusa más tenue
                effect.Parameters["specularColor"]?.SetValue(new Vector3(0.8f, 0.85f, 0.9f));
                effect.Parameters["shininess"]?.SetValue(80f);
                effect.Parameters["KAmbient"]?.SetValue(0.7f);
                effect.Parameters["Texture"]?.SetValue(Texture);
                effect.Parameters["LightViewProjection"]?.SetValue(lightViewProjectionMatrix);
                effect.Parameters["ShadowMap"]?.SetValue(shadowMap);

                // Dibujar meshPart manualmente
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

        // Dibujar shells
        foreach (var shell in shells)
            shell.Draw(view, projection);
    }


    public BoundingBox BoundingBox
    {
        get
        {
            // Ajusta estos valores según el tamaño de tu modelo/enemigo
            float width = 1f;
            float height = 1.5f;
            float depth = 1f;

            return new BoundingBox(
                _position - new Vector3(width, 0.5f, depth),
                _position + new Vector3(width, height, depth)
            );
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
