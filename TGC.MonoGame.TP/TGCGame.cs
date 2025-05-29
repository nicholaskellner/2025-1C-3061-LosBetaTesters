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

        public List<Vector3> trees { get; set; }
        public List<BoundingBox> TreeBoundingBoxes { get; set; } = new();
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

        private IndexBuffer _indices;

        

        protected override void Initialize()
        {
            var r = new Random();
            trees = new List<Vector3>();
            for (int i = 0; i < 250; i++)
            {
                var x = r.NextSingle() * 200;
                var y = r.NextSingle() * 200;
                if (i % 2 == 1) x = -x;
                if (i % 3 == 0) y = -y;
                var scale = r.NextSingle() + 1f;

                trees.Add(new Vector3(x, y, scale));

                // Crear bounding box para árbol
                var boxMin = new Vector3(x - scale, 0, y - scale);
                var boxMax = new Vector3(x + scale, 5 * scale, y + scale);
                TreeBoundingBoxes.Add(new BoundingBox(boxMin, boxMax));
            }

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
            tanque = new Tank(Content, GraphicsDevice);
            base.LoadContent();
            foreach (ModelMesh mesh in tree.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    part.Effect = _effect2;
                }
            }
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
                        
            tanque.Update(gameTime);
            
            foreach (var meshBox in tanque.MeshBoundingBoxes)
            {
                foreach (var treeBox in TreeBoundingBoxes)
                {
                    if (meshBox.Intersects(treeBox))
                    {
                        tanque.RevertPosition();
                        break;
                    }
                }
            }
            var i = 0;
            foreach (var treeBox in TreeBoundingBoxes)
            {
                foreach (var shell in tanque.shells)
                {
                    if (shell._position.X > treeBox.Min.X && shell._position.X < treeBox.Max.X)
                    {
                        if (shell._position.Z > treeBox.Min.Z && shell._position.Z < treeBox.Max.Z)
                        {
                            trees[i] = new Vector3(1, 100, 1);
                        }
                    }
                }
                i++;
            }
            View = Matrix.CreateLookAt(tanque._position - tanque._rotation * 20 + new Vector3(0, 7, 0), tanque._position, Vector3.Up);
                
                
            base.Update(gameTime);
        }

        private void DrawTree(Model model, Matrix world)
        {
            foreach (var mesh in model.Meshes)
            {
                var i = 0;
                foreach (var effect in mesh.Effects)
                {
                    effect.Parameters["World"].SetValue(world);
                    effect.Parameters["View"].SetValue(View);
                    effect.Parameters["Projection"].SetValue(Projection);
                    effect.Parameters["ambientColor"].SetValue(new Vector3(1f, 1f, 1f));
                    effect.Parameters["KAmbient"].SetValue(0.5f);
                }
                foreach (var part in mesh.MeshParts)
                {
                    GraphicsDevice.SetVertexBuffer(part.VertexBuffer);
                    GraphicsDevice.Indices = part.IndexBuffer;
                    if (i == 0)
                        part.Effect.Parameters["color"].SetValue(new Vector4(0.943f, 0.588f, 0.325f, 1));
                    else
                        part.Effect.Parameters["color"].SetValue(new Vector4(0.260f, 0.849f, 0.603f, 1));
                    foreach (var pass in part.Effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, part.VertexOffset, part.StartIndex, part.PrimitiveCount);
                    }
                    i++;
                }
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.BlendState = BlendState.Opaque;

            tanque.Draw(GraphicsDevice, View, Projection);

            for (int i = 0; i < 50; i++)
            {
                rock.Draw(Matrix.CreateScale(trees[i].Z) * Matrix.CreateRotationY(trees[i].Z * 5) * Matrix.CreateTranslation(trees[i].X, -2, trees[i].Y), View, Projection);
                DrawHitBox(Matrix.CreateScale((TreeBoundingBoxes[i].Max.X - TreeBoundingBoxes[i].Min.X) / 2, (TreeBoundingBoxes[i].Max.Y - TreeBoundingBoxes[i].Min.Y) / 2, (TreeBoundingBoxes[i].Max.Z - TreeBoundingBoxes[i].Min.Z) / 2) * Matrix.CreateTranslation((TreeBoundingBoxes[i].Max.X + TreeBoundingBoxes[i].Min.X) / 2, (TreeBoundingBoxes[i].Max.Y + TreeBoundingBoxes[i].Min.Y) / 2 - 2f, (TreeBoundingBoxes[i].Max.Z + TreeBoundingBoxes[i].Min.Z) / 2));
            }

            for (int i = 50; i < 250; i++)
            {
                DrawTree(tree,Matrix.CreateScale(trees[i].Z) * Matrix.CreateTranslation(trees[i].X, -2, trees[i].Y));
                DrawHitBox(Matrix.CreateScale((TreeBoundingBoxes[i].Max.X - TreeBoundingBoxes[i].Min.X) / 2, (TreeBoundingBoxes[i].Max.Y - TreeBoundingBoxes[i].Min.Y) / 2, (TreeBoundingBoxes[i].Max.Z - TreeBoundingBoxes[i].Min.Z) / 2) * Matrix.CreateTranslation((TreeBoundingBoxes[i].Max.X + TreeBoundingBoxes[i].Min.X) / 2, (TreeBoundingBoxes[i].Max.Y + TreeBoundingBoxes[i].Min.Y) / 2 - 2f, (TreeBoundingBoxes[i].Max.Z + TreeBoundingBoxes[i].Min.Z) / 2));
            }

            grass.Draw(Matrix.CreateScale(100, 0, 100) * Matrix.CreateTranslation(1, -2, 1), View, Projection);
            DrawHitBox(Matrix.CreateScale((tanque.MeshBoundingBoxes[1].Max.X - tanque.MeshBoundingBoxes[1].Min.X) / 2, 2f, (tanque.MeshBoundingBoxes[1].Max.Z - tanque.MeshBoundingBoxes[1].Min.Z) / 2) * Matrix.CreateTranslation((tanque.MeshBoundingBoxes[1].Max.X + tanque.MeshBoundingBoxes[1].Min.X) / 2, 0f, (tanque.MeshBoundingBoxes[1].Max.Z + tanque.MeshBoundingBoxes[1].Min.Z) / 2));
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