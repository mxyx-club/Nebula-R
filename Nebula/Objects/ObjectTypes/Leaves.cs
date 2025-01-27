namespace Nebula.Objects.ObjectTypes;

public class Leaves : TypeWithImage
{
    public Leaves() : base(128, "Leaves", new SpriteLoader("Nebula.Resources.Leaves.png", 200f))
    {
    }

    public override bool RequireMonoBehaviour => true;

    public override bool CanSeeInShadow(CustomObject? obj) { return false; }

    public override void Update(CustomObject obj, int command)
    {
        obj.Renderer.color = Color.green;
    }

    public override void Initialize(CustomObject obj)
    {
        base.Initialize(obj);
    }
}
