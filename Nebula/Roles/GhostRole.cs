namespace Nebula.Roles;

public class GhostRole : Assignable
{
    public byte id { get; private set; }

    //使用済みロールID
    private static byte maxId = 0;

    protected GhostRole(string name, string localizeName, Color color) :
       base(name, localizeName, color)
    {
        this.id = maxId;
        maxId++;
    }

    public virtual bool IsAssignableTo(Game.PlayerData player) => true;

    public sealed override void SetupRoleOptionData()
    {
        SetupRoleOptionData(Module.CustomOptionTab.GhostRoles);
    }

    public static void LoadAllOptionData()
    {
        foreach (GhostRole role in Roles.AllGhostRoles)
        {
            role.CreateRoleOption();
        }
    }

    public static GhostRole? GetRoleById(byte id)
    {
        if (id == Byte.MaxValue) return null;
        foreach (GhostRole role in Roles.AllGhostRoles) if (role.id == id) return role;
        return null;
    }
}