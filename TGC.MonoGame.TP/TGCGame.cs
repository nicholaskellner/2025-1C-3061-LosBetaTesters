using System;
using System.Collections.Generic;
using System.Security;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TGC.MonoGame.TP
{
    /// <summary>
    ///     Esta es la clase principal del juego.
    ///     Inicialmente puede ser renombrado o copiado para hacer mas ejemplos chicos, en el caso de copiar para que se
    ///     ejecute el nuevo ejemplo deben cambiar la clase que ejecuta Program <see cref="Program.Main()" /> linea 10.
    /// </summary>
    public class TGCGame : Game
    {
        public const string ContentFolder3D = "Models/";
        public const string ContentFolderEffects = "Effects/";
        public const string ContentFolderMusic = "Music/";
        public const string ContentFolderSounds = "Sounds/";
        public const string ContentFolderSpriteFonts = "SpriteFonts/";
        public const string ContentFolderTextures = "Models/textures_mod/";

        /// <summary>
        ///     Constructor del juego.
        /// </summary>
        public TGCGame()
        {
            // Maneja la configuracion y la administracion del dispositivo grafico.
            Graphics = new GraphicsDeviceManager(this);

            Graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width - 100;
            Graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height - 100;

            // Para que el juego sea pantalla completa se puede usar Graphics IsFullScreen.
            // Carpeta raiz donde va a estar toda la Media.
            Content.RootDirectory = "Content";
            // Hace que el mouse sea visible.
            IsMouseVisible = false;
        }

        private GraphicsDeviceManager Graphics { get; }

        public List<Prop> trees { get; set; }
        private Matrix View { get; set; }
        private Matrix Projection { get; set; }

        private Model grass;

        private Model rock;
        private Model shell;

        private Model tree;
        private Tank tanque;

        private VertexBuffer VertexBuffer;
        public VertexPosition[] _vertices = new VertexPosition[8];

        private Effect _effect;
        private Effect _effect2;
        private Effect _effect3;

        private IndexBuffer _indices;
        private VertexBuffer vertexBuffer;
        private IndexBuffer indexBuffer;
        private List<VertexPositionNormalTexture> vertices = new List<VertexPositionNormalTexture>();
        private List<short> indices = new List<short>();
        private Vector3[,] heightMapVertices;

        

        protected override void Initialize()
        {
            var rasterizerState = new RasterizerState { CullMode = CullMode.None };
            GraphicsDevice.RasterizerState = rasterizerState;
            View = Matrix.CreateLookAt(new Vector3(15, 5, 0), Vector3.Zero, Vector3.Up);
            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 1, 250);
            var triangleIndices = new short[]
            {
                0, 1, 1, 2, 2, 3, 3, 0, // bottom
                4, 5, 5, 6, 6, 7, 7, 4, // top
                0, 4, 1, 5, 2, 6, 3, 7  // sides
            };
            _indices = new IndexBuffer(GraphicsDevice, IndexElementSize.SixteenBits, 24, BufferUsage.None);
            _indices.SetData(triangleIndices);
            VertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPosition), 8, BufferUsage.WriteOnly);
            Vector3[] vertices = [new Vector3(1, -1, 1), new Vector3(-1, -1, 1), new Vector3(-1, -1, -1), new Vector3(1, -1, -1), new Vector3(1, 1, 1), new Vector3(-1, 1, 1), new Vector3(-1, 1, -1), new Vector3(1, 1, -1)];
            for (int i = 0; i < 8; i++)
                _vertices[i] = new VertexPosition(vertices[i]);
            VertexBuffer.SetData(_vertices);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            grass = Content.Load<Model>(ContentFolder3D + "ground_grass");
            rock = Content.Load<Model>(ContentFolder3D + "rockA");
            tree = Content.Load<Model>(ContentFolder3D + "tree");
            shell = Content.Load<Model>(ContentFolder3D + "shell");
            _effect = Content.Load<Effect>(ContentFolderEffects + "ShaderHitbox");
            _effect2 = Content.Load<Effect>(ContentFolderEffects + "ShaderTree");
            _effect3 = Content.Load<Effect>(ContentFolderEffects + "ShaderTerrain");
            Texture2D heightMapTexture = Content.Load<Texture2D>(ContentFolder3D + "heightmap");
            createHeightMap(heightMapTexture);
            tanque = new Tank(Content, GraphicsDevice);
            List<Vector3> treeColors = new List<Vector3> {new Vector3(0.943f, 0.588f, 0.325f), new Vector3(0.1f, 0.7f, 0.1f) };
            base.LoadContent();
            foreach (ModelMesh mesh in tree.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    part.Effect = _effect2;
                }
            }
            var r = new Random();
            trees = new List<Prop>();
            for (int i = 0; i < 250; i++)
            {
                var x = r.NextSingle() * 200;
                var y = r.NextSingle() * 200;
                if (i % 2 == 1) x = -x;
                if (i % 3 == 0) y = -y;
                var scale = r.NextSingle() + 1f;

                // Crear bounding box para árbol
                var alt = 0f;
                if (x > -200 && y > -200 && x < 1080 && y < 1080)
                    alt = heightMapVertices[(int)x / 10 + 20, (int)y / 10 + 20].Y - 20;
                var boxMin = new Vector3(x - scale, alt+2, y - scale);
                var boxMax = new Vector3(x + scale, alt+7 * scale, y + scale);
                
                
                if (i < 50)
                    trees.Add(new Rock(rock, _effect2, new Vector3(x, alt, y), new Vector3(1, 1, 1), new BoundingBox(boxMin, boxMax), new Vector3(0.7f, 0.7f, 0.7f), scale));
                else
                    trees.Add(new Tree(tree, _effect2, new Vector3(x, alt, y), new Vector3(1, 1, 1), new BoundingBox(boxMin, boxMax), treeColors, scale));
            }
        }

        private void createHeightMap(Texture2D texture)
        {
            int width = texture.Width;
            int height = texture.Height;
            heightMapVertices = new Vector3[width, height];
            Color[] heightMapColors = new Color[texture.Width * texture.Height];
            texture.GetData(heightMapColors);
            
            float heightScale = 40f; // vertical scale multiplier

            

            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    int index = x + z * width;
                    float y = heightMapColors[index].R / 255f * heightScale; // Grayscale = R
                    heightMapVertices[x, z] = new Vector3(x, y, z);
                }
            }

            for (int x = 0; x < width - 1; x++)
            {
                for (int z = 0; z < height - 1; z++)
                {
                    // Triangle 1
                    AddVertex(x, z);
                    AddVertex(x + 1, z);
                    AddVertex(x, z + 1);

                    // Triangle 2
                    AddVertex(x + 1, z);
                    AddVertex(x + 1, z + 1);
                    AddVertex(x, z + 1);
                }
            }

            void AddVertex(int x, int z)
            {
                Vector3 position = heightMapVertices[x, z];
                Vector2 texCoord = new Vector2((float)x / width, (float)z / height);
                vertices.Add(new VertexPositionNormalTexture(position, Vector3.Up, texCoord)); // normals calculated later
                indices.Add((short)(vertices.Count - 1));
            }
            vertexBuffer = new VertexBuffer(GraphicsDevice,
                typeof(VertexPositionNormalTexture), vertices.Count, BufferUsage.WriteOnly);
            vertexBuffer.SetData(vertices.ToArray());

            indexBuffer = new IndexBuffer(GraphicsDevice, IndexElementSize.SixteenBits,
                indices.Count, BufferUsage.WriteOnly);
            indexBuffer.SetData(indices.ToArray());
        }
        private void drawTerrain()
        {
            GraphicsDevice.SetVertexBuffer(vertexBuffer);
            GraphicsDevice.Indices = indexBuffer;

            var effect = _effect3;
            effect.Parameters["World"].SetValue(Matrix.CreateScale(10, 1, 10) * Matrix.CreateTranslation(-200, -20, -200));
            effect.Parameters["View"].SetValue(View);
            effect.Parameters["Projection"].SetValue(Projection);
            effect.Parameters["ambientColor"].SetValue(new Vector3(1f, 1f, 1f));
            effect.Parameters["KAmbient"].SetValue(1f);

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0,
                    indices.Count / 3);
            }
        }
        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            trees.RemoveAll(tree => tree.isExpired);
            tanque.Update(gameTime);

            foreach (var meshBox in tanque.MeshBoundingBoxes)
            {
                foreach (var tree in trees)
                {
                    if (meshBox.Intersects(tree.hitBox))
                    {
                        tanque.RevertPosition();
                        break;
                    }
                }
            }
            var i = 0;
            foreach (var tree in trees)
            {
                foreach (var shell in tanque.shells)
                {
                    if (shell._position.X > tree.hitBox.Min.X && shell._position.X < tree.hitBox.Max.X)
                    {
                        if (shell._position.Z > tree.hitBox.Min.Z && shell._position.Z < tree.hitBox.Max.Z)
                        {
                            if (shell._position.Y > tree.hitBox.Min.Y && shell._position.Y < tree.hitBox.Max.Y)
                            {
                                tree.getHit();
                                shell.isExpired = true;
                            }
                        }
                    }
                }
                i++;
            }
            View = Matrix.CreateLookAt(tanque._position - tanque._rotation * 20 + new Vector3(0, 7, 0), tanque._position, Vector3.Up);


            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.BlendState = BlendState.Opaque;
            drawTerrain();
            tanque.Draw(GraphicsDevice, View, Projection);

            foreach(var tree in trees){
                tree.Draw(GraphicsDevice, View, Projection);
                DrawHitBox(Matrix.CreateScale((tree.hitBox.Max.X - tree.hitBox.Min.X) / 2, (tree.hitBox.Max.Y - tree.hitBox.Min.Y) / 2, (tree.hitBox.Max.Z - tree.hitBox.Min.Z) / 2) * Matrix.CreateTranslation((tree.hitBox.Max.X + tree.hitBox.Min.X) / 2, (tree.hitBox.Max.Y + tree.hitBox.Min.Y) / 2 - 2f, (tree.hitBox.Max.Z + tree.hitBox.Min.Z) / 2));
            }

            //grass.Draw(Matrix.CreateScale(100, 0, 100) * Matrix.CreateTranslation(1, -2, 1), View, Projection);
            DrawHitBox(Matrix.CreateScale((tanque.MeshBoundingBoxes[1].Max.X - tanque.MeshBoundingBoxes[1].Min.X) / 2, 2f, (tanque.MeshBoundingBoxes[1].Max.Z - tanque.MeshBoundingBoxes[1].Min.Z) / 2) * Matrix.CreateTranslation((tanque.MeshBoundingBoxes[1].Max.X + tanque.MeshBoundingBoxes[1].Min.X) / 2, 6f, (tanque.MeshBoundingBoxes[1].Max.Z + tanque.MeshBoundingBoxes[1].Min.Z) / 2));
        }

        protected override void UnloadContent()
        {
            // Libero los recursos.
            Content.Unload();

            base.UnloadContent();
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
    }
}