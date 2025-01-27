namespace Nebula.Roles.RoleSystem;

public static class HackSystem
{
    public static void showAdminMap(bool ignoreCommSabotage, Patches.AdminPatch.AdminMode adminMode)
    {
        Patches.AdminPatch.isAffectedByCommAdmin = !ignoreCommSabotage;
        Patches.AdminPatch.isStandardAdmin = false;
        Patches.AdminPatch.adminMode = adminMode;
        Patches.AdminPatch.shouldChangeColor = true;

        PlayerControl.LocalPlayer.NetTransform.Halt();
        FastDestroyableSingleton<HudManager>.Instance.ToggleMapVisible(new MapOptions
        {
            Mode = MapOptions.Modes.CountOverlay,
            AllowMovementWhileMapOpen = false
        });
    }
}
