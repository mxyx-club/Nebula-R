namespace Nebula.Patches;

[HarmonyPatch]
internal class MapBehaviorPatch
{
    public static void UpdateMapSize(MapBehaviour __instance)
    {
        if (minimapFlag)
        {
            __instance.gameObject.transform.localScale = new Vector3(0.225f, 0.225f, 1f);
            __instance.gameObject.transform.localPosition = new Vector3(4.2f, 0f, -100f);
        }
        else
        {
            __instance.gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
            __instance.gameObject.transform.localPosition = new Vector3(0f, 0f, -25f);
        }


    }

    public static bool minimapFlag = false;

    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.Awake))]
    private class MapBehaviourAwakePatch
    {
        private static void Prefix(MapBehaviour __instance)
        {
            minimapFlag = false;
        }

    }

    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.Close))]
    private class MapBehaviourClosePatch
    {
        private static void Postfix(MapBehaviour __instance)
        {
            Helpers.RoleAction(Game.GameData.data.myData.getGlobalData(), (r) =>
            {
                r.OnMapClose(__instance);
            });
        }

    }
}

[HarmonyPatch]
internal class ForcelyShowSabotagePatch
{
    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.ShowNormalMap))]
    private class MapBehaviourShowNormalMapPatch
    {
        private static void Postfix(MapBehaviour __instance)
        {
            if (Game.GameData.data.myData.getGlobalData().role.CanInvokeSabotage && !MeetingHud.Instance)
            {
                if (__instance.IsOpen)
                {
                    __instance.infectedOverlay.gameObject.SetActive(true);
                    __instance.ColorControl.SetColor(Palette.ImpostorRed);
                    DestroyableSingleton<HudManager>.Instance.SetHudActive(false);
                    ConsoleJoystick.SetMode_Sabotage();
                    return;
                }
            }


            if (__instance.IsOpen) DestroyableSingleton<HudManager>.Instance.SetHudActive(MapBehaviorPatch.minimapFlag);
            MapBehaviorPatch.UpdateMapSize(__instance);
        }

    }
}

[HarmonyPatch]
internal class DontShowSabotagePatch
{
    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.ShowSabotageMap))]
    private class MapBehaviourShowNormalMapPatch
    {
        private static bool Prefix(MapBehaviour __instance)
        {
            if (Game.GameData.data.myData.getGlobalData().role.CanInvokeSabotage)
                return true;
            else
            {
                if (__instance.IsOpen)
                {
                    __instance.Close();
                    return false;
                }
                if (!PlayerControl.LocalPlayer.CanMove && !MeetingHud.Instance)
                {
                    return false;
                }

                PlayerControl.LocalPlayer.SetPlayerMaterialColors(__instance.HerePoint);
                __instance.GenericShow();
                __instance.taskOverlay.Show();
                __instance.ColorControl.SetColor(new Color(0.05f, 0.2f, 1f, 1f));
                DestroyableSingleton<HudManager>.Instance.SetHudActive(MapBehaviorPatch.minimapFlag);
                return false;
            }
        }

    }
}

[HarmonyPatch]
internal class MapUpdatePatch
{
    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.FixedUpdate))]
    private class MapBehaviourUpdatePatch
    {
        private static void Postfix(MapBehaviour __instance)
        {
            Helpers.RoleAction(Game.GameData.data.myData.getGlobalData(), (r) =>
             {
                 r.MyMapUpdate(__instance);
             });
        }

    }
}

[HarmonyPatch]
internal class MapTaskOverlayPatch
{
    [HarmonyPatch(typeof(MapTaskOverlay), nameof(MapTaskOverlay.SetIconLocation))]
    private class SetIconLocationPatch
    {
        private static bool Prefix(MapTaskOverlay __instance)
        {
            return !Game.GameData.data.myData.getGlobalData().role.BlocksShowTaskOverlay;
        }

    }

    [HarmonyPatch(typeof(MapTaskOverlay), nameof(MapTaskOverlay.Show))]
    private class ShowPatch
    {
        private static void Postfix(MapTaskOverlay __instance)
        {

            void GenerateIcon(Vector2 pos, bool pulse)
            {
                Vector3 localPosition = pos / ShipStatus.Instance.MapScale;
                localPosition.z = -1f;
                PooledMapIcon pooledMapIcon = __instance.icons.Get<PooledMapIcon>();
                pooledMapIcon.transform.localScale = new Vector3(pooledMapIcon.NormalSize, pooledMapIcon.NormalSize, pooledMapIcon.NormalSize);

                pooledMapIcon.rend.color = Color.yellow;
                pooledMapIcon.name = __instance.data.Count.ToString();
                pooledMapIcon.lastMapTaskStep = pulse ? 1 : 0;
                pooledMapIcon.transform.localPosition = localPosition;
                if (pulse)
                {
                    pooledMapIcon.alphaPulse.enabled = true;
                    pooledMapIcon.rend.material.SetFloat("_Outline", 1f);
                }

                if (!__instance.data.ContainsKey(pooledMapIcon.name))
                {
                    __instance.data.Add(pooledMapIcon.name, pooledMapIcon);
                }
            }

            Helpers.RoleAction(Game.GameData.data.myData.getGlobalData(), (r) => r.OnShowMapTaskOverlay(__instance, GenerateIcon));

        }

    }
}