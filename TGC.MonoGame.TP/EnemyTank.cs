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

    // Dirección hacia jugador (XZ)
    Vector3 direction = playerPosition - _position;
    direction.Y = 0;
    if (direction.LengthSquared() > 0.001f)
        direction.Normalize();

    // --- Fuerza de separación con otros enemigos ---
    Vector3 separation = Vector3.Zero;
    float separationRadius = 15f;
    foreach (var other in allEnemies)
    {
        if (other == this || other.IsDead) continue;

        Vector3 toOther = _position - other._position;
        float distance = toOther.Length();

        if (distance < separationRadius && distance > 0.01f)
        {
            toOther.Normalize();
            float strength = (separationRadius - distance) / separationRadius;
            separation += toOther * strength;
        }
    }

    // Dirección final combinada
    Vector3 finalDirection = direction + separation;
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

    public void Draw(GraphicsDevice graphicsDevice, Matrix view, Matrix projection, Vector3 cameraPosition)
    {
        if (IsDead) return;

        Matrix world = Matrix.CreateScale(1.5f) // opcional si necesitás escalar
                 * Matrix.CreateRotationX(-MathHelper.PiOver2) // 🔁 gira 90° hacia adelante (desde “mirando al piso”)
                 * Matrix.CreateRotationY((float)Math.Atan2(_rotation.X, _rotation.Z))
                 * Matrix.CreateTranslation(_position);

        foreach (var mesh in Model.Meshes)
        {
            foreach (Effect effect in mesh.Effects) // Cambiado BasicEffect por Effect
            {
                effect.Parameters["World"].SetValue(world);
                effect.Parameters["View"].SetValue(view);
                effect.Parameters["Projection"].SetValue(projection);

                // Si tu shader tiene iluminación y otros parámetros:
                effect.Parameters["ambientColor"]?.SetValue(new Vector3(1f, 1f, 1f));
                effect.Parameters["lightPosition"]?.SetValue(new Vector3(50, 50, 30));
                effect.Parameters["cameraPosition"]?.SetValue(cameraPosition);
                effect.Parameters["diffuseColor"]?.SetValue(new Vector3(1, 1, 1));
                effect.Parameters["specularColor"]?.SetValue(new Vector3(1, 1, 1));
                effect.Parameters["shininess"]?.SetValue(32f);
                effect.Parameters["KAmbient"]?.SetValue(0.5f);

                // Aplicar la técnica y pase del shader
                foreach (var pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                }
            }
            mesh.Draw();
        }

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
}
