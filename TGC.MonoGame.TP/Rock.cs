using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using System;
public class Rock : Prop
{
    public Rock(Model model, Effect effect, Vector3 position, Vector3 direction, BoundingBox box) : base(model, effect, position, direction)
    {
        Model = model;
        Effect = effect;
        _position = position;
        direction.Normalize();
        hitBox = box;
        world = Matrix.CreateTranslation(position);
        color = new Vector3(0.943f, 0.588f, 0.325f);

        foreach (var mesh in Model.Meshes)
            foreach (var part in mesh.MeshParts)
                part.Effect = Effect;
    }
    public Rock(Model model, Effect effect, Vector3 position, Vector3 direction, BoundingBox box, Vector3 color) : base(model, effect, position, direction)
    {
        Model = model;
        Effect = effect;
        _position = position;
        direction.Normalize();
        hitBox = box;
        world = Matrix.CreateTranslation(position);
        this.color = color;

        foreach (var mesh in Model.Meshes)
            foreach (var part in mesh.MeshParts)
                part.Effect = Effect;
    }
    public override void Update(GameTime gameTime)
    {
        //Algo que hacer
    }

    public override void getHit()
    {
        isExpired = true;
    }
}