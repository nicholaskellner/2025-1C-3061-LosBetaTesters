using System;
using System.Collections.Generic;
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
        private SpriteBatch SpriteBatch { get; set; }

        private Effect Effect { get; set; }
        private Matrix View { get; set; }
        private Matrix Projection { get; set; }

        private Model grass;

        private Model rock;

        private Model tree;
        private Tank tanque;

        protected override void Initialize()
        {
            // La logica de inicializacion que no depende del contenido se recomienda poner en este metodo.

            // Apago el backface culling.
            // Esto se hace por un problema en el diseno del modelo del logo de la materia.
            // Una vez que empiecen su juego, esto no es mas necesario y lo pueden sacar.
            var rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            GraphicsDevice.RasterizerState = rasterizerState;
            View = Matrix.CreateLookAt(new Vector3(15,5,0), Vector3.Zero, Vector3.Up);
            Projection =
                Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 1, 250);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            grass = Content.Load<Model>(ContentFolder3D + "ground_grass");
            rock = Content.Load<Model>(ContentFolder3D + "rockA");
            tree = Content.Load<Model>(ContentFolder3D + "tree");
            Effect = Content.Load<Effect>(ContentFolderEffects + "BasicShader");
    
            tanque = new Tank(Content, GraphicsDevice);
            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                //Salgo del juego.
                Exit();
            }
            
            tanque.Update(gameTime);
            //Para posicionar la camara atras
            View = Matrix.CreateLookAt(tanque._position - tanque._rotation*15 + new Vector3(50,15,0), tanque._position, Vector3.Up);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            tanque.Draw(GraphicsDevice, View, Projection);
            for(int i = 0; i < 10; i++)
            {
                rock.Draw(Matrix.CreateTranslation(-i*15 + 10, 0, -i*25), View, Projection);
            }
            for(int i = 0; i < 10; i++)
            {
                for(int j = 0; j < 5; j++)
                {
                    tree.Draw(Matrix.CreateTranslation(i*4, 0, j*4), View, Projection);
                }
            }
            //Para tener algo de piso
            grass.Draw(Matrix.CreateScale(100,0,100) * Matrix.CreateTranslation(1,-2,1),View,Projection);
        }
        protected override void UnloadContent()
        {
            // Libero los recursos.
            Content.Unload();

            base.UnloadContent();
        }
    }
}