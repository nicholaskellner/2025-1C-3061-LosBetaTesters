using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using ThunderingTanks.Objects;

namespace TGC.MonoGame.TP
{
    public class TGCGame : Game
    {
        public const string ContentFolder3D = "Models/";
        public const string ContentFolderEffects = "Effects/";
        public const string ContentFolderMusic = "Music/";
        public const string ContentFolderSounds = "Sounds/";
        public const string ContentFolderSpriteFonts = "SpriteFonts/";
        public const string ContentFolderTextures = "Models/textures_mod/";

        private enum GameState { Menu, Playing, Paused, Exit, GameOver }
        private GameState CurrentState = GameState.Menu;


        public TGCGame()
        {
            Graphics = new GraphicsDeviceManager(this);
            Graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width - 100;
            Graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height - 100;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        private GraphicsDeviceManager Graphics { get; }
        private Matrix View { get; set; }
        private Matrix Projection { get; set; }

        private SpriteBatch spriteBatch;
        private SpriteFont menuFont;
        private int selectedOption = 0;
        private string[] menuOptions = { "Iniciar Juego", "Salir" };
        private string[] pauseMenuOptions = { "Reanudar", "Volver al menu principal" };
        private int pauseSelectedOption = 0;

        public List<Prop> trees { get; set; }
        private Model grass, rock, tree, shell;
        private Tank tanque;
        private VertexBuffer VertexBuffer;
        public VertexPosition[] _vertices = new VertexPosition[8];
        private Effect _effect, _effect2, _effect3;
        private IndexBuffer _indices;
        private VertexBuffer vertexBuffer;
        private IndexBuffer indexBuffer;
        private List<VertexPositionNormalTexture> vertices = new();
        private List<short> indices = new();
        private Vector3[,] heightMapVertices;
        private Matrix MenuView;
        private Tank menuTank;
        private double inputCooldown = 0.2; // segundos entre inputs válidos
        private double timeSinceLastInput = 0;

        private Song menuMusic;
        SoundEffect shootSound;
        private bool isMuted = false;
        private KeyboardState previousKState;
        private KeyboardState previousKeyboardState;
        private Texture2D Texture23;
        private Texture2D Texture2;
        Vector3 cameraPosition = new Vector3(15, 5, 0);
        private VertexBuffer fullscreenQuad;
        private Effect skyEffect;
        private DebugDraw debugDraw;
        private int kills = 0;
        private TimeSpan gameTimeElapsed = TimeSpan.Zero;
        private SpriteFont hudFont;
        Texture2D pixel;
        List<EnemyTank> enemies = new List<EnemyTank>();
        const int ENEMY_COUNT = 10;
        Random r = new Random();
        private Texture2D whitePixel;
        Vector3 lightPosition = new Vector3(50f, 30f, 30f);
        private Skybox skybox;
        


        protected override void Initialize()
        {
            var rasterizerState = new RasterizerState { CullMode = CullMode.None };
            GraphicsDevice.RasterizerState = rasterizerState;
            View = Matrix.CreateLookAt(new Vector3(15, 5, 0), Vector3.Zero, Vector3.Up);
            MenuView = Matrix.CreateLookAt(new Vector3(150, 80, 150), Vector3.Zero, Vector3.Up);
            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 1, 250);

            short[] triangleIndices = {
                0, 1, 1, 2, 2, 3, 3, 0,
                4, 5, 5, 6, 6, 7, 7, 4,
                0, 4, 1, 5, 2, 6, 3, 7
            };
            _indices = new IndexBuffer(GraphicsDevice, IndexElementSize.SixteenBits, 24, BufferUsage.None);
            _indices.SetData(triangleIndices);

            VertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPosition), 8, BufferUsage.WriteOnly);
            Vector3[] verts = {
                new(1, -1, 1), new(-1, -1, 1), new(-1, -1, -1), new(1, -1, -1),
                new(1, 1, 1), new(-1, 1, 1), new(-1, 1, -1), new(1, 1, -1)
            };
            for (int i = 0; i < 8; i++) _vertices[i] = new VertexPosition(verts[i]);
            VertexBuffer.SetData(_vertices);


            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            menuFont = Content.Load<SpriteFont>(ContentFolderSpriteFonts + "BasicFont");
            hudFont = Content.Load<SpriteFont>(ContentFolderSpriteFonts + "BasicFont");
            pixel = new Texture2D(GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });
            menuMusic = Content.Load<Song>(ContentFolderMusic + "menu_music");
            skybox = new Skybox(100f); // Escala grande para que no se note el cubo
            skybox.LoadContent(Content);

            MediaPlayer.IsRepeating = true;
            MediaPlayer.Volume = 0.5f; // entre 0 y 1
            MediaPlayer.Play(menuMusic);

            grass = Content.Load<Model>(ContentFolder3D + "ground_grass");
            rock = Content.Load<Model>(ContentFolder3D + "rockA");
            tree = Content.Load<Model>(ContentFolder3D + "tree");
            shell = Content.Load<Model>(ContentFolder3D + "shell");
            _effect = Content.Load<Effect>(ContentFolderEffects + "ShaderHitbox");
            _effect2 = Content.Load<Effect>(ContentFolderEffects + "ShaderTree");
            _effect3 = Content.Load<Effect>(ContentFolderEffects + "ShaderTerrain");
            Texture23 = Content.Load<Texture2D>(ContentFolderTextures + "pasto");
            Texture2 = Content.Load<Texture2D>(ContentFolderTextures + "tierra");
            skyEffect = Content.Load<Effect>(ContentFolderEffects + "SkyGradientEffect");
            debugDraw = new DebugDraw(GraphicsDevice);

            whitePixel = new Texture2D(GraphicsDevice, 1, 1);
            whitePixel.SetData(new[] { Color.White });


            Texture2D heightMapTexture = Content.Load<Texture2D>(ContentFolder3D + "heightmap");
            createHeightMap(heightMapTexture);

            tanque = new Tank(Content, GraphicsDevice);
            menuTank = new Tank(Content, GraphicsDevice);
            menuTank._position = new Vector3(0, 0, 0);

            List<Vector3> treeColors = new() { new Vector3(0.943f, 0.588f, 0.325f), new Vector3(0.1f, 0.7f, 0.1f) };
            foreach (ModelMesh mesh in tree.Meshes)
                foreach (ModelMeshPart part in mesh.MeshParts)
                    part.Effect = _effect2;

            var r = new Random();
            trees = new List<Prop>();
            for (int i = 0; i < 250; i++)
            {
                var x = r.NextSingle() * 200 * ((i % 2 == 1) ? -1 : 1);
                var y = r.NextSingle() * 200 * ((i % 3 == 0) ? -1 : 1);
                var scale = r.NextSingle() + 1f;
                var alt = (x > -200 && y > -200 && x < 1080 && y < 1080) ? heightMapVertices[(int)x / 10 + 20, (int)y / 10 + 20].Y - 20 : 0f;
                var boxMin = new Vector3(x - scale, alt + 2, y - scale);
                var boxMax = new Vector3(x + scale, alt + 7 * scale, y + scale);

                if (i < 50)
                    trees.Add(new Rock(rock, _effect2, new Vector3(x, alt, y), Vector3.One, new BoundingBox(boxMin, boxMax), new Vector3(0.7f), scale));
                else
                    trees.Add(new Tree(tree, _effect2, new Vector3(x, alt, y), Vector3.One, new BoundingBox(boxMin, boxMax), treeColors, scale));
            }

            // Crear enemigos
            enemies = new List<EnemyTank>();
            Random random = new Random();

            for (int i = 0; i < 10; i++)
            {
                EnemyTank enemy = new EnemyTank(Content, GraphicsDevice);
                enemy._position = new Vector3(
                    random.Next(-100, 100),
                    0,
                    random.Next(-100, 100)
                );
                enemies.Add(enemy);
            }
        }


        private void createHeightMap(Texture2D texture)
        {
            int width = texture.Width, height = texture.Height;
            heightMapVertices = new Vector3[width, height];
            Color[] colors = new Color[width * height];
            texture.GetData(colors);
            float scale = 40f;

            for (int x = 0; x < width; x++)
                for (int z = 0; z < height; z++)
                    heightMapVertices[x, z] = new Vector3(x, colors[x + z * width].R / 255f * scale, z);

            for (int x = 0; x < width - 1; x++)
                for (int z = 0; z < height - 1; z++)
                {
                    AddVertex(x, z); AddVertex(x + 1, z); AddVertex(x, z + 1);
                    AddVertex(x + 1, z); AddVertex(x + 1, z + 1); AddVertex(x, z + 1);
                }

            void AddVertex(int x, int z)
            {
                vertices.Add(new VertexPositionNormalTexture(heightMapVertices[x, z], Vector3.Up, new Vector2((float)x / width, (float)z / height)));
                indices.Add((short)(vertices.Count - 1));
            }

            vertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionNormalTexture), vertices.Count, BufferUsage.WriteOnly);
            vertexBuffer.SetData(vertices.ToArray());

            indexBuffer = new IndexBuffer(GraphicsDevice, IndexElementSize.SixteenBits, indices.Count, BufferUsage.WriteOnly);
            indexBuffer.SetData(indices.ToArray());
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState kState = Keyboard.GetState();
            var currentKeyboardState = Keyboard.GetState(); //esto es para el menu pausa
            timeSinceLastInput += gameTime.ElapsedGameTime.TotalSeconds;

            if (CurrentState == GameState.Menu)
            {
                // Menú principal
                if (timeSinceLastInput >= inputCooldown)
                {
                    if (kState.IsKeyDown(Keys.Up))
                    {
                        selectedOption = (selectedOption + menuOptions.Length - 1) % menuOptions.Length;
                        timeSinceLastInput = 0;
                    }
                    else if (kState.IsKeyDown(Keys.Down))
                    {
                        selectedOption = (selectedOption + 1) % menuOptions.Length;
                        timeSinceLastInput = 0;
                    }
                    else if (kState.IsKeyDown(Keys.Enter))
                    {
                        if (selectedOption == 0)
                            CurrentState = GameState.Playing;
                        else if (selectedOption == 1)
                            Exit();

                        timeSinceLastInput = 0;
                    }
                }

                if (kState.IsKeyDown(Keys.M) && !previousKState.IsKeyDown(Keys.M))
                {
                    isMuted = !isMuted;
                    MediaPlayer.IsMuted = isMuted;
                }
            }

            else if (CurrentState == GameState.Playing)
            {
                if (tanque.IsDead)
                {
                    CurrentState = GameState.GameOver;
                    return;
                }
                gameTimeElapsed += gameTime.ElapsedGameTime;
                if (kState.IsKeyDown(Keys.Escape) && !previousKState.IsKeyDown(Keys.Escape))
                {
                    CurrentState = GameState.Paused;
                }

                trees.RemoveAll(t => t.isExpired);

                // Actualizar tanque jugador
                tanque.Update(gameTime, GetTerrainHeight);

                // Colisiones OBB tanque jugador contra árboles (como ya tenías)
                foreach (var obb in tanque.MeshOBBs)
                {
                    foreach (var tree in trees)
                    {
                        if (CollisionHelper.OBBvsAABB(obb, tree.hitBox))
                        {
                            tanque.RevertPosition();
                            break;
                        }
                    }
                }

                // Colisiones balas tanque jugador contra árboles
                foreach (var tree in trees)
                {
                    foreach (var shell in tanque.shells)
                    {
                        if (tree.hitBox.Contains(shell._position) != ContainmentType.Disjoint)
                        {
                            tree.getHit();
                            shell.isExpired = true;
                        }
                    }
                }

                // --- NUEVO: Actualizar y manejar tanques enemigos ---
                foreach (var enemy in enemies)
                {
                    enemy.Update(gameTime, GetTerrainHeight, tanque._position, tanque.shells,enemies); // que persigan o hagan lógica

                    // Colisiones balas enemigos contra tanque jugador con probabilidad de acierto
                    foreach (var shell in enemy.shells)
                    {
                        // Supongo que tanque.BoundingBox es el AABB del tanque jugador
                        if (tanque.BoundingBox.Intersects(shell.BoundingBox))
                        {
                            // Aplicar daño con un porcentaje de acierto
                            Random rnd = new Random();
                            if (rnd.NextDouble() < 0.5) // 50% de acierto, cambiá este valor como quieras
                            {
                                tanque.TakeDamage(10);
                            }
                            shell.isExpired = true;
                        }
                    }


                    // Eliminar balas expiradas de enemigos
                    enemy.shells.RemoveAll(s => s.isExpired);
                     
                }

                 foreach (var shell in tanque.shells)
                    {
                        foreach (var enemy in enemies)
                        {
                            if (!enemy.IsDead && enemy.BoundingBox.Intersects(shell.BoundingBox))
                            {
                                enemy.TakeDamage(10); // o el daño que quieras
                                shell.isExpired = true;
                            }
                        }
                    }

                // Crear la cámara
                View = Matrix.CreateLookAt(tanque._position - tanque._rotation * 20 + new Vector3(0, 7, 0), tanque._position, Vector3.Up);
            }

            else if (CurrentState == GameState.Paused)
            {
                if (timeSinceLastInput >= inputCooldown)
                {
                    if (kState.IsKeyDown(Keys.Up))
                    {
                        pauseSelectedOption = (pauseSelectedOption + pauseMenuOptions.Length - 1) % pauseMenuOptions.Length;
                        timeSinceLastInput = 0;
                    }
                    else if (kState.IsKeyDown(Keys.Down))
                    {
                        pauseSelectedOption = (pauseSelectedOption + 1) % pauseMenuOptions.Length;
                        timeSinceLastInput = 0;
                    }
                    else if (kState.IsKeyDown(Keys.Enter))
                    {
                        if (pauseSelectedOption == 0)
                            CurrentState = GameState.Playing;
                        else if (pauseSelectedOption == 1)
                        {
                            CurrentState = GameState.Menu;
                            MediaPlayer.Play(menuMusic); // volver a la música del menú
                        }
                        timeSinceLastInput = 0;
                    }
                    else if (kState.IsKeyDown(Keys.R))
                    {
                        CurrentState = GameState.Playing;
                        timeSinceLastInput = 0;
                    }
                }
            }
            else if (CurrentState == GameState.GameOver)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.R) && !previousKState.IsKeyDown(Keys.R))
                {
                    RestartGame();
                }
            }

            previousKState = kState;
            base.Update(gameTime);
        }


        protected override void Draw(GameTime gameTime)
{
   GraphicsDevice.Clear(Color.Black);

    GraphicsDevice.DepthStencilState = DepthStencilState.None;
    GraphicsDevice.RasterizerState = RasterizerState.CullNone;

    skybox.Draw(View, Projection, cameraPosition);

    GraphicsDevice.DepthStencilState = DepthStencilState.Default;
    GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

    if (CurrentState == GameState.Menu)
    {
        _effect3.Parameters["View"].SetValue(MenuView);
        drawTerrainWithView(MenuView);

        foreach (var tree in trees)
            tree.Draw(GraphicsDevice, MenuView, Projection,lightPosition,cameraPosition);

        spriteBatch.Begin();
        for (int i = 0; i < menuOptions.Length; i++)
        {
            var color = i == selectedOption ? Color.Yellow : Color.White;
            spriteBatch.DrawString(menuFont, menuOptions[i], new Vector2(100, 100 + i * 40), color);
        }
        spriteBatch.End();
    }
    else if (CurrentState == GameState.Playing)
    {
        drawTerrain();

        // Dibuja tanque jugador
        tanque.Draw(GraphicsDevice, View, Projection, cameraPosition);

        // Dibuja tanques enemigos
        foreach (var enemy in enemies)
        {
            enemy.Draw(GraphicsDevice, View, Projection, cameraPosition);
        }

        foreach (var tree in trees)
        {
            tree.Draw(GraphicsDevice, View, Projection,lightPosition,cameraPosition);
            DrawHitBox(tree.hitBox);
        }

        foreach (var obb in tanque.MeshOBBs)
        {
            debugDraw.DrawOrientedBoundingBox(obb, View, Projection);
        }

        // --------------------------
        // HUD + Barras de vida enemigos
        // --------------------------
        spriteBatch.Begin();

        // Vida del jugador
        int barWidth = 200;
        int barHeight = 20;
        float healthPercent = (float)tanque.CurrentHealth / tanque.MaxHealth;

        Rectangle backgroundBar = new Rectangle(20, 20, barWidth, barHeight);
        Rectangle healthBar = new Rectangle(20, 20, (int)(barWidth * healthPercent), barHeight);

        spriteBatch.Draw(pixel, backgroundBar, Color.DarkRed);
        spriteBatch.Draw(pixel, healthBar, Color.Red);

        spriteBatch.DrawString(menuFont, $"Vida: {tanque.CurrentHealth} / {tanque.MaxHealth}", new Vector2(20, 45), Color.White);
        spriteBatch.DrawString(menuFont, $"Muertes: {kills}", new Vector2(20, 70), Color.White);
        spriteBatch.DrawString(menuFont, $"Tiempo: {gameTimeElapsed.Minutes:D2}:{gameTimeElapsed.Seconds:D2}", new Vector2(20, 95), Color.White);

        // Barras de vida sobre enemigos
        foreach (var enemy in enemies)
        {
            if (enemy.IsDead) continue;

            // Posición 3D sobre el tanque
            Vector3 worldPos = enemy._position + new Vector3(0, 20, 0);
            Vector3 screenPos = GraphicsDevice.Viewport.Project(worldPos, Projection, View, Matrix.Identity);

            float enemyHealthPercent = (float)enemy.CurrentHealth / enemy.MaxHealth;
            int enemyBarWidth = 50;
            int enemyBarHeight = 6;
            Vector2 enemyBarPos = new Vector2(screenPos.X - enemyBarWidth / 2, screenPos.Y - enemyBarHeight);

            spriteBatch.Draw(pixel, new Rectangle((int)enemyBarPos.X, (int)enemyBarPos.Y, enemyBarWidth, enemyBarHeight), Color.DarkRed);
            spriteBatch.Draw(pixel, new Rectangle((int)enemyBarPos.X, (int)enemyBarPos.Y, (int)(enemyBarWidth * enemyHealthPercent), enemyBarHeight), Color.LimeGreen);
        }

        spriteBatch.End();
    }
    else if (CurrentState == GameState.Paused)
    {
        drawTerrain();
        tanque.Draw(GraphicsDevice, View, Projection, cameraPosition);
        foreach (var tree in trees)
            tree.Draw(GraphicsDevice, View, Projection,lightPosition,cameraPosition);

        spriteBatch.Begin();
        for (int i = 0; i < pauseMenuOptions.Length; i++)
        {
            var color = i == pauseSelectedOption ? Color.Yellow : Color.White;
            spriteBatch.DrawString(menuFont, pauseMenuOptions[i], new Vector2(100, 100 + i * 40), color);
        }
        spriteBatch.End();
    }
    else if (CurrentState == GameState.GameOver)
    {
        drawTerrain();
        tanque.Draw(GraphicsDevice, View, Projection, cameraPosition);
        foreach (var tree in trees)
            tree.Draw(GraphicsDevice, View, Projection,lightPosition,cameraPosition);

        spriteBatch.Begin();
        spriteBatch.DrawString(menuFont, "Has sido destruido", new Vector2(100, 100), Color.Red);
        spriteBatch.DrawString(menuFont, "Presiona R para reiniciar", new Vector2(100, 150), Color.White);
        spriteBatch.End();
    }

    base.Draw(gameTime);
}




        private void drawTerrain()
        {
            GraphicsDevice.SetVertexBuffer(vertexBuffer);
            GraphicsDevice.Indices = indexBuffer;

            _effect3.Parameters["World"].SetValue(Matrix.CreateScale(10, 1, 10) * Matrix.CreateTranslation(-200, -20, -200));
            _effect3.Parameters["View"].SetValue(View);
            _effect3.Parameters["Projection"].SetValue(Projection);
            _effect3.Parameters["ambientColor"].SetValue(Vector3.One);
            _effect3.Parameters["KAmbient"].SetValue(1f);
            _effect3.Parameters["Texture"].SetValue(Texture23);
            _effect3.Parameters["Texture2"].SetValue(Texture2);

            foreach (var pass in _effect3.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, indices.Count / 3);
            }
        }
        private void drawTerrainWithView(Matrix view)
        {
            GraphicsDevice.SetVertexBuffer(vertexBuffer);
            GraphicsDevice.Indices = indexBuffer;

            _effect3.Parameters["World"].SetValue(Matrix.CreateScale(10, 1, 10) * Matrix.CreateTranslation(-200, -20, -200));
            _effect3.Parameters["View"].SetValue(view);
            _effect3.Parameters["Projection"].SetValue(Projection);
            _effect3.Parameters["ambientColor"].SetValue(Vector3.One);
            _effect3.Parameters["KAmbient"].SetValue(1f);
            _effect3.Parameters["Texture"].SetValue(Texture23);
            _effect3.Parameters["Texture2"].SetValue(Texture2);

            foreach (var pass in _effect3.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, indices.Count / 3);
            }
        }

        private void DrawHitBox(BoundingBox box)
        {
            var scale = (box.Max - box.Min) / 2f;
            var translation = (box.Max + box.Min) / 2f;
            var world = Matrix.CreateScale(scale) * Matrix.CreateTranslation(translation);
            DrawHitBox(world);
        }

        private void DrawHitBox(Matrix world)
        {
            _effect.Parameters["World"].SetValue(world);
            _effect.Parameters["View"].SetValue(View);
            _effect.Parameters["Projection"].SetValue(Projection);

            GraphicsDevice.SetVertexBuffer(VertexBuffer);
            GraphicsDevice.Indices = _indices;

            foreach (var pass in _effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.LineList, 0, 0, 12);
            }
        }
        public float GetTerrainHeight(float worldX, float worldZ)
        {
            // Primero convertimos la posición world a índice del heightmap
            // (Tienes que adaptar estos valores según la escala y offset de tu terreno)
            float terrainScale = 10f;     // el scale usado en createHeightMap y el drawTerrain
            float offset = 200f;          // el offset negativo usado en drawTerrain para mover el terreno

            // Posición local en el heightmap
            float localX = (worldX + offset) / terrainScale;
            float localZ = (worldZ + offset) / terrainScale;

            int x0 = (int)Math.Floor(localX);
            int z0 = (int)Math.Floor(localZ);
            int x1 = x0 + 1;
            int z1 = z0 + 1;

            // Validar límites para no salir del arreglo
            if (x0 < 0 || z0 < 0 || x1 >= heightMapVertices.GetLength(0) || z1 >= heightMapVertices.GetLength(1))
                return 0f; // fuera del terreno

            // Fracciones para interpolar
            float fracX = localX - x0;
            float fracZ = localZ - z0;

            // Obtener alturas de los 4 vértices cercanos
            float h00 = heightMapVertices[x0, z0].Y;
            float h10 = heightMapVertices[x1, z0].Y;
            float h01 = heightMapVertices[x0, z1].Y;
            float h11 = heightMapVertices[x1, z1].Y;

            // Interpolación bilineal
            float h0 = MathHelper.Lerp(h00, h10, fracX);
            float h1 = MathHelper.Lerp(h01, h11, fracX);
            float height = MathHelper.Lerp(h0, h1, fracZ);

            return height;
        }

        private void RestartGame()
        {
            // Reiniciar jugador
            tanque = new Tank(Content, GraphicsDevice);

            // Reiniciar enemigos
            enemies.Clear();
            for (int i = 0; i < 10; i++)
            {
                var x = r.Next(-400, 400);
                var z = r.Next(-400, 400);
                var pos = new Vector3(x, 0, z);
                enemies.Add(new EnemyTank(Content, GraphicsDevice) { _position = pos });
            }

            // Reiniciar árboles, rocas, kills, tiempo, etc.
            gameTimeElapsed = TimeSpan.Zero;
            kills = 0;
            trees.Clear();
            GenerateProps(); // asumiendo que tenés un método que los genera
            CurrentState = GameState.Playing;
        }

        private void GenerateProps()
        {
            var r = new Random();
            trees = new List<Prop>();

            List<Vector3> treeColors = new() { new Vector3(0.943f, 0.588f, 0.325f), new Vector3(0.1f, 0.7f, 0.1f) };

            for (int i = 0; i < 250; i++)
            {
                var x = r.NextSingle() * 200 * ((i % 2 == 1) ? -1 : 1);
                var y = r.NextSingle() * 200 * ((i % 3 == 0) ? -1 : 1);
                var scale = r.NextSingle() + 1f;
                var alt = (x > -200 && y > -200 && x < 1080 && y < 1080) ? heightMapVertices[(int)x / 10 + 20, (int)y / 10 + 20].Y - 20 : 0f;
                var boxMin = new Vector3(x - scale, alt + 2, y - scale);
                var boxMax = new Vector3(x + scale, alt + 7 * scale, y + scale);

                if (i < 50)
                    trees.Add(new Rock(rock, _effect2, new Vector3(x, alt, y), Vector3.One, new BoundingBox(boxMin, boxMax), new Vector3(0.7f), scale));
                else
                    trees.Add(new Tree(tree, _effect2, new Vector3(x, alt, y), Vector3.One, new BoundingBox(boxMin, boxMax), treeColors, scale));
            }
        }
    }
    
}