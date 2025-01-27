namespace Nebula.Patches;


[HarmonyPatch(typeof(AspectPosition), nameof(AspectPosition.OnEnable))]
internal class AspectPositionPatch
{
    //parentCamを 必ずしもMain Cameraではなく、自身を描画するカメラに設定するよう変更
    public static void Prefix(AspectPosition __instance)
    {
        if (__instance.gameObject.layer == LayerExpansion.GetUILayer())
            __instance.parentCam = Camera.allCameras.FirstOrDefault((c) => c.name == "UI Camera") ?? Camera.main;
        else
            __instance.parentCam = Camera.main;
    }
}
