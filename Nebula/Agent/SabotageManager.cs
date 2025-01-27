namespace Nebula.Agent;

public static class SabotageManager
{
    public static bool ExistAnySabotages()
    {
        return Helpers.SabotageIsActive();
    }

    public static void BeginSabotage(SystemTypes room)
    {
        switch (room)
        {
            case SystemTypes.Reactor:
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Sabotage, 3);
                break;
            case SystemTypes.Comms:
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Sabotage, 14);
                break;
            case SystemTypes.LifeSupp:
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Sabotage, 8);
                break;
            case SystemTypes.Electrical:
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Sabotage, 7);
                break;
            case SystemTypes.GapRoom:
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Sabotage, 21);
                break;
        }
    }

    public static void BeginReactorSabotage()
    {
        ShipStatus.Instance.RpcRepairSystem(SystemTypes.Sabotage, 3);
    }

    public static void BeginCommsSabotage()
    {
        ShipStatus.Instance.RpcRepairSystem(SystemTypes.Sabotage, 14);
    }

    public static void BeginOxygenSabotage()
    {
        ShipStatus.Instance.RpcRepairSystem(SystemTypes.Sabotage, 8);
    }

    public static void BeginLightsSabotage()
    {
        ShipStatus.Instance.RpcRepairSystem(SystemTypes.Sabotage, 7);
    }

    public static void BeginSeismicSabotage()
    {
        ShipStatus.Instance.RpcRepairSystem(SystemTypes.Sabotage, 21);
    }

    public static void BeginDoorSabotage(SystemTypes room)
    {
        Info("Close " + room + "'s Door");
        ShipStatus.Instance.RpcCloseDoorsOfType(room);
    }
}
