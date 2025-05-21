using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TGC.MonoGame.TP
{
    public class Bullet
    {
        public Model Model { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Direction { get; set; }
        public float Speed { get; set; } = 10f;

        public Matrix World => Matrix.CreateScale(0.2f) *
                               Matrix.CreateTranslation(Position);

        public Bullet(Model model, Vector3 startPosition, Vector3 direction)
        {
            Model = model;
            Position = startPosition;
            Direction = Vector3.Normalize(direction);
        }

        public void Update(GameTime gameTime)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Position += Direction * Speed * delta;
        }

        public void Draw(Matrix view, Matrix projection)
        {
            foreach (var mesh in Model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = World;
                    effect.View = view;
                    effect.Projection = projection;
                    effect.EnableDefaultLighting();
                }
                mesh.Draw();
            }
        }
    }

}
