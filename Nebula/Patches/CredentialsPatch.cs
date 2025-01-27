using Nebula.CustomCosmetics;
using Nebula.Module;
using UnityEngine.Events;

namespace Nebula.Patches;

[HarmonyPatch]
public static class CredentialsPatch
{
    [HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
    private static class VersionShowerPatch
    {
        private static void Postfix(VersionShower __instance)
        {
            var amongUsLogo = GameObject.Find("bannerLogo_AmongUs");
            if (amongUsLogo == null) return;

            RuntimePrefabs.TextPrefab = UnityEngine.Object.Instantiate(__instance.text);
            RuntimePrefabs.TextPrefab.enableAutoSizing = true;
            RuntimePrefabs.TextPrefab.text = "";
            RuntimePrefabs.TextPrefab.gameObject.hideFlags = HideFlags.HideAndDontSave;
            GameObject.DontDestroyOnLoad(RuntimePrefabs.TextPrefab.gameObject);

            var credentials = UnityEngine.Object.Instantiate(__instance.text);

            credentials.transform.position = new Vector3(0, -0.6f, 0);

            if (Nebula.NebulaPlugin.PluginStage != null)
            {
                credentials.SetText(Nebula.NebulaPlugin.PluginStage + " v" + Nebula.NebulaPlugin.PluginVisualVersion);
            }
            else
            {
                credentials.SetText($"v{Nebula.NebulaPlugin.PluginVisualVersion}");
            }
            credentials.alignment = TMPro.TextAlignmentOptions.Center;
            credentials.fontSize *= 0.75f;

            credentials.transform.SetParent(amongUsLogo.transform);
        }
    }

    [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
    public static class PingTrackerPatch
    {
        private static float DeltaTime;
        private static void Postfix(PingTracker __instance)
        {
            DeltaTime += (Time.deltaTime - DeltaTime) * 0.1f;
            var fps = Mathf.Ceil(1f / DeltaTime);
            var PingText = $"<size=80%>Ping: {AmongUsClient.Instance.Ping}ms{$" FPS: {fps}"}</size>";
            __instance.text.SetOutlineThickness(0.1f);
            var host = $"<size=80%>房主: {GameData.Instance?.GetHost()?.PlayerName}</size>";

            __instance.text.alignment = TMPro.TextAlignmentOptions.TopRight;
            __instance.text.text = $"<size=130%><color=#9579ce>星云舰</color></size> v{NebulaPlugin.PluginVisualVersion}\n<color=#FFB793FF><size=80%>沫夏悠轩 - mxyx.club</color>\n{PingText}\n{host}</size>";

            if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started)
            {
                __instance.gameObject.GetComponent<AspectPosition>().DistanceFromEdge = new Vector3(2.85f, 0.1f, 0);
            }
            else if (PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.Data.IsDead)
            {
                __instance.gameObject.GetComponent<AspectPosition>().DistanceFromEdge = new Vector3(2.0f, 0.1f, 0f);
            }
            else
            {
                __instance.gameObject.GetComponent<AspectPosition>().DistanceFromEdge = new Vector3(2.25f, 0.1f, 0);
            }
            __instance.gameObject.GetComponent<AspectPosition>().AdjustPosition();
        }
    }

    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
    private static class LogoPatch
    {
        private static SpriteLoader DesignerToolButtonSprite = new("Nebula.Resources.DesignerToolButton.png", 100f);

        private static void Prefix(MainMenuManager __instance)
        {
            CustomHatLoader.LaunchHatFetcher();
        }

        private static void Postfix(MainMenuManager __instance)
        {
            var amongUsLogo = GameObject.Find("bannerLogo_AmongUs");
            if (amongUsLogo != null)
            {
                amongUsLogo.transform.localScale *= 0.6f;
                amongUsLogo.transform.position += new Vector3(0f, -0.1f, 0f);
            }

            var nebulaLogo = new GameObject("bannerLogo_Nebula");
            nebulaLogo.transform.position = new Vector3(0f, 0.4f, 0f);
            var renderer = nebulaLogo.AddComponent<SpriteRenderer>();
            renderer.sprite = Helpers.loadSpriteFromResources("Nebula.Resources.Logo.png", 115f);

            GameObject.Find("PlayOnlineButton").transform.position = new Vector3(1.025f, -1.5f, 0f);
            GameObject.Find("PlayLocalButton").transform.position = new Vector3(-1.025f, -1.5f, 0f);
            GameObject.Find("HowToPlayButton").active = false;
            GameObject.Find("FreePlayButton").active = false;

            var bottomButtons = GameObject.Find("BottomButtons");
            var buttonObj = GameObject.Instantiate(bottomButtons.transform.GetChild(0).gameObject, bottomButtons.transform);
            buttonObj.name = "DesignerToolButton";

            buttonObj.GetComponent<SpriteRenderer>().sprite = DesignerToolButtonSprite.GetSprite();
            var button = buttonObj.GetComponent<PassiveButton>();
            button.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
            button.OnClick.AddListener((UnityAction)(() => { NebulaPlugin.isFoolDay = !NebulaPlugin.isFoolDay; SoundPlayer.PlaySound(AudioAsset.Uskneko); }));

            bottomButtons.GetComponent<DotAligner>().Start();
        }
    }
}