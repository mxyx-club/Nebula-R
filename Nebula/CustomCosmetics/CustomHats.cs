using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Cryptography;
using AmongUs.Data;
using Newtonsoft.Json.Linq;

namespace Nebula.CustomCosmetics;

[HarmonyPatch]
public class CustomHats
{
    private static bool LOADED = false;
    private static bool RUNNING = false;
    public static Material hatShader;

    public static Dictionary<string, HatExtension> CustomHatRegistry = new Dictionary<string, HatExtension>();
    public static HatExtension TestExt = null;

    public class HatExtension
    {
        public string author { get; set; }
        public string package { get; set; }
        public string condition { get; set; }
        public Sprite FlipImage { get; set; }
        public Sprite BackFlipImage { get; set; }
    }

    public class CustomHat
    {
        public string author { get; set; }
        public string package { get; set; }
        public string condition { get; set; }
        public string name { get; set; }
        public string resource { get; set; }
        public string flipresource { get; set; }
        public string backflipresource { get; set; }
        public string backresource { get; set; }
        public string climbresource { get; set; }
        public bool bounce { get; set; }
        public bool adaptive { get; set; }
        public bool behind { get; set; }
    }

    private static List<CustomHat> createCustomHatDetails(string[] hats, bool fromDisk = false)
    {
        var fronts = new Dictionary<string, CustomHat>();
        var backs = new Dictionary<string, string>();
        var flips = new Dictionary<string, string>();
        var backflips = new Dictionary<string, string>();
        var climbs = new Dictionary<string, string>();

        for (var i = 0; i < hats.Length; i++)
        {
            var s = fromDisk ? hats[i].Substring(hats[i].LastIndexOf("\\") + 1).Split('.')[0] : hats[i].Split('.')[3];
            var p = s.Split('_');

            var options = new HashSet<string>();
            for (var j = 1; j < p.Length; j++)
                options.Add(p[j]);

            if (options.Contains("back") && options.Contains("flip"))
                backflips.Add(p[0], hats[i]);
            else if (options.Contains("climb"))
                climbs.Add(p[0], hats[i]);
            else if (options.Contains("back"))
                backs.Add(p[0], hats[i]);
            else if (options.Contains("flip"))
                flips.Add(p[0], hats[i]);
            else
            {
                var custom = new CustomHat { resource = hats[i] };
                custom.name = p[0].Replace('-', ' ');
                custom.bounce = options.Contains("bounce");
                custom.adaptive = options.Contains("adaptive");
                custom.behind = options.Contains("behind");

                fronts.Add(p[0], custom);
            }
        }

        var customhats = new List<CustomHat>();

        foreach (var k in fronts.Keys)
        {
            var hat = fronts[k];
            string br, cr, fr, bfr;
            backs.TryGetValue(k, out br);
            climbs.TryGetValue(k, out cr);
            flips.TryGetValue(k, out fr);
            backflips.TryGetValue(k, out bfr);
            if (br != null)
                hat.backresource = br;
            if (cr != null)
                hat.climbresource = cr;
            if (fr != null)
                hat.flipresource = fr;
            if (bfr != null)
                hat.backflipresource = bfr;
            if (hat.backresource != null)
                hat.behind = true;

            customhats.Add(hat);
        }

        return customhats;
    }

    private static Sprite CreateHatSprite(string path, bool fromDisk = false)
    {
        var texture = fromDisk ? Helpers.loadTextureFromDisk(path) : Helpers.loadTextureFromResources(path);
        if (texture == null)
            return null;
        var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.53f, 0.575f), texture.width * 0.375f);
        if (sprite == null)
            return null;
        texture.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontUnloadUnusedAsset;
        sprite.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontUnloadUnusedAsset;

        return sprite;

    }

    private static HatData CreateHatBehaviour(CustomHat ch, bool fromDisk = false, bool testOnly = false)
    {
        if (hatShader == null)
        {
            var tmpShader = FastDestroyableSingleton<HatManager>.Instance.PlayerMaterial;
            hatShader = tmpShader;
        }

        var hat = ScriptableObject.CreateInstance<HatData>();
        hat.hatViewData.viewData = ScriptableObject.CreateInstance<HatViewData>();
        hat.hatViewData.viewData.MainImage = CreateHatSprite(ch.resource, fromDisk);
        if (ch.backresource != null)
        {
            hat.hatViewData.viewData.BackImage = CreateHatSprite(ch.backresource, fromDisk);
            ch.behind = true; // Required to view backresource
        }
        if (ch.climbresource != null)
            hat.hatViewData.viewData.ClimbImage = CreateHatSprite(ch.climbresource, fromDisk);
        hat.name = ch.name;
        hat.displayOrder = 99;
        hat.ProductId = "hat_" + ch.name.Replace(' ', '_');
        hat.InFront = !ch.behind;
        hat.NoBounce = !ch.bounce;
        hat.ChipOffset = new Vector2(0f, 0.2f);
        hat.Free = true;

        if (ch.adaptive && hatShader != null)
            hat.hatViewData.viewData.AltShader = hatShader;

        var extend = new HatExtension();
        extend.author = ch.author != null ? ch.author : "Unknown";
        extend.package = ch.package != null ? ch.package : "Misc.";
        extend.condition = ch.condition != null ? ch.condition : "none";

        if (ch.flipresource != null)
            extend.FlipImage = CreateHatSprite(ch.flipresource, fromDisk);
        if (ch.backflipresource != null)
            extend.BackFlipImage = CreateHatSprite(ch.backflipresource, fromDisk);

        if (testOnly)
        {
            TestExt = extend;
            TestExt.condition = hat.name;
        }
        else
        {
            CustomHatRegistry.Add(hat.name, extend);
        }

        return hat;
    }

    private static HatData CreateHatBehaviour(CustomHatLoader.CustomHatOnline chd, bool fromDisk = true)
    {
        if (fromDisk)
        {
            var filePath = Path.GetDirectoryName(Application.dataPath) + @"\CustomHats\";
            chd.resource = filePath + chd.resource;
            if (chd.backresource != null)
                chd.backresource = filePath + chd.backresource;
            if (chd.climbresource != null)
                chd.climbresource = filePath + chd.climbresource;
            if (chd.flipresource != null)
                chd.flipresource = filePath + chd.flipresource;
            if (chd.backflipresource != null)
                chd.backflipresource = filePath + chd.backflipresource;
        }

        return CreateHatBehaviour(chd, fromDisk, false);
    }

    [HarmonyPatch(typeof(HatManager), nameof(HatManager.GetHatById))]
    private static class HatManagerPatch
    {
        private static List<HatData> allHatsList;

        private static void Prefix(HatManager __instance)
        {
            if (RUNNING) return;
            RUNNING = true; // prevent simultanious execution
            allHatsList = __instance.allHats.ToList();

            try
            {
                while (CustomHatLoader.hatdetails.Count > 0)
                {
                    bool fromDisk = !(CustomHatLoader.hatdetails[0].name.Contains("HorseHat_"));
                    allHatsList.Add(CreateHatBehaviour(CustomHatLoader.hatdetails[0], fromDisk));
                    CustomHatLoader.hatdetails.RemoveAt(0);
                }
                __instance.allHats = allHatsList.ToArray();
            }
            catch (Exception e)
            {
                if (!LOADED)
                    Error("Unable to add Custom Hats\n" + e);
            }
            LOADED = true;
        }

        private static void Postfix(HatManager __instance)
        {
            RUNNING = false;
        }
    }

    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.HandleAnimation))]
    private static class PlayerPhysicsHandleAnimationPatch
    {
        private static void Postfix(PlayerPhysics __instance)
        {
            var currentAnimation = __instance.Animations.Animator.GetCurrentAnimation();
            if (currentAnimation == __instance.Animations.group.ClimbUpAnim || currentAnimation == __instance.Animations.group.ClimbDownAnim) return;
            var hp = __instance.myPlayer.cosmetics.hat;
            if (hp.Hat == null) return;
            var extend = hp.Hat.getHatExtension();
            if (extend == null) return;
            if (extend.FlipImage != null)
            {
                if (__instance.FlipX)
                    hp.FrontLayer.sprite = extend.FlipImage;
                else
                {
                    hp.FrontLayer.sprite = hp.hatView.MainImage;
                }
            }
            if (extend.BackFlipImage != null)
            {
                if (__instance.FlipX)
                    hp.BackLayer.sprite = extend.BackFlipImage;
                else
                {
                    hp.BackLayer.sprite = hp.hatView.BackImage;
                }
            }
        }
    }

    [HarmonyPatch(typeof(HatsTab), nameof(HatsTab.OnEnable))]
    public class HatsTabOnEnablePatch
    {
        public static string innerslothPackageName = "Innersloth Hats";
        private static TMPro.TMP_Text textTemplate;

        public static float createHatPackage(List<Tuple<HatData, HatExtension>> hats, string packageName, float YStart, HatsTab __instance)
        {
            var isDefaultPackage = innerslothPackageName == packageName;
            if (!isDefaultPackage)
                hats = hats.OrderBy(x => x.Item1.name).ToList();
            var offset = YStart;

            if (textTemplate != null)
            {
                var title = UnityEngine.Object.Instantiate(textTemplate, __instance.scroller.Inner);
                title.transform.localPosition = new Vector3(2.25f, YStart, -1f);
                title.transform.localScale = Vector3.one * 1.5f;
                title.fontSize *= 0.5f;
                title.enableAutoSizing = false;
                __instance.StartCoroutine(Effects.Lerp(0.1f, new Action<float>((p) => { title.SetText(packageName); })));
                offset -= 0.8f * __instance.YOffset;
            }
            for (var i = 0; i < hats.Count; i++)
            {
                var hat = hats[i].Item1;
                var ext = hats[i].Item2;

                var xpos = __instance.XRange.Lerp(i % __instance.NumPerRow / (__instance.NumPerRow - 1f));
                var ypos = offset - i / __instance.NumPerRow * (isDefaultPackage ? 1f : 1.5f) * __instance.YOffset;
                var colorChip = UnityEngine.Object.Instantiate(__instance.ColorTabPrefab, __instance.scroller.Inner);
                if (ActiveInputManager.currentControlType == ActiveInputManager.InputType.Keyboard)
                {
                    colorChip.Button.OnMouseOver.AddListener((Action)(() => __instance.SelectHat(hat)));
                    colorChip.Button.OnMouseOut.AddListener((Action)(() => __instance.SelectHat(FastDestroyableSingleton<HatManager>.Instance.GetHatById(DataManager.Player.Customization.Hat))));
                    colorChip.Button.OnClick.AddListener((Action)(() => __instance.ClickEquip()));
                }
                else
                {
                    colorChip.Button.OnClick.AddListener((Action)(() => __instance.SelectHat(hat)));
                }
                colorChip.Button.ClickMask = __instance.scroller.Hitbox;
                var background = colorChip.transform.FindChild("Background");
                var foreground = colorChip.transform.FindChild("ForeGround");

                if (ext != null)
                {
                    if (background != null)
                    {
                        background.localPosition = Vector3.down * 0.243f;
                        background.localScale = new Vector3(background.localScale.x, 0.8f, background.localScale.y);
                    }
                    if (foreground != null)
                        foreground.localPosition = Vector3.down * 0.243f;

                    if (textTemplate != null)
                    {
                        var description = UnityEngine.Object.Instantiate(textTemplate, colorChip.transform);
                        description.transform.localPosition = new Vector3(0f, -0.65f, -1f);
                        description.alignment = TMPro.TextAlignmentOptions.Center;
                        description.transform.localScale = Vector3.one * 0.65f;
                        __instance.StartCoroutine(Effects.Lerp(0.1f, new Action<float>((p) => { description.SetText($"{hat.name}\nby {ext.author}"); })));
                    }
                }

                colorChip.transform.localPosition = new Vector3(xpos, ypos, -1f);
                colorChip.Inner.SetHat(hat, __instance.HasLocalPlayer() ? PlayerControl.LocalPlayer.Data.DefaultOutfit.ColorId : DataManager.Player.Customization.Color);
                colorChip.Inner.transform.localPosition = hat.ChipOffset;
                colorChip.Tag = hat;
                colorChip.SelectionHighlight.gameObject.SetActive(false);
                __instance.ColorChips.Add(colorChip);
            }
            return offset - (hats.Count - 1) / __instance.NumPerRow * (isDefaultPackage ? 1f : 1.5f) * __instance.YOffset - 1.75f;
        }

        public static void Postfix(HatsTab __instance)
        {
            for (var i = 0; i < __instance.scroller.Inner.childCount; i++)
                UnityEngine.Object.Destroy(__instance.scroller.Inner.GetChild(i).gameObject);
            __instance.ColorChips = new Il2CppSystem.Collections.Generic.List<ColorChip>();

            HatData[] unlockedHats = FastDestroyableSingleton<HatManager>.Instance.GetUnlockedHats();
            var packages = new Dictionary<string, List<Tuple<HatData, HatExtension>>>();

            foreach (var hatBehaviour in unlockedHats)
            {
                var ext = hatBehaviour.getHatExtension();

                if (ext != null)
                {
                    if (!packages.ContainsKey(ext.package))
                        packages[ext.package] = new List<Tuple<HatData, HatExtension>>();
                    packages[ext.package].Add(new Tuple<HatData, HatExtension>(hatBehaviour, ext));
                }
                else
                {
                    if (!packages.ContainsKey(innerslothPackageName))
                        packages[innerslothPackageName] = new List<Tuple<HatData, HatExtension>>();
                    packages[innerslothPackageName].Add(new Tuple<HatData, HatExtension>(hatBehaviour, null));
                }
            }

            packages.Remove("Horse Hats");  // Cannot be selected!

            var YOffset = __instance.YStart;
            textTemplate = GameObject.Find("HatsGroup").transform.FindChild("Text").GetComponent<TMPro.TMP_Text>();

            var orderedKeys = packages.Keys.OrderBy((x) =>
            {
                if (x == innerslothPackageName) return 1000;
                if (x == "Developer Hats") return 0;
                return 500;
            });
            foreach (var key in orderedKeys)
            {
                var value = packages[key];
                YOffset = createHatPackage(value, key, YOffset, __instance);
            }

            __instance.scroller.ContentYBounds.max = -(YOffset + 4.1f);
        }
    }

}

public class CustomHatLoader
{
    public static bool running = false;
    private const string REPO = "https://raw.githubusercontent.com/TheOtherRolesAU/TheOtherHats/master";

    public static List<CustomHatOnline> hatdetails = new List<CustomHatOnline>();
    public static void LaunchHatFetcher()
    {
        if (running) return;
        running = true;
        Message("Load Hats Is Running");
        _ = LaunchHatFetcherAsync();
    }

    private static async Task LaunchHatFetcherAsync()
    {
        try
        {
            var status = await FetchHats();
            if (status != HttpStatusCode.OK)
                Message("Custom Hats could not be loaded\n");
        }
        catch (Exception e)
        {
            Error("Unable to fetch hats\n" + e.Message);
        }
        running = false;
    }

    private static string sanitizeResourcePath(string res)
    {
        if (res == null || !res.EndsWith(".png"))
            return null;

        res = res.Replace("\\", "")
                 .Replace("/", "")
                 .Replace("*", "")
                 .Replace("..", "");
        return res;
    }

    public static async Task<HttpStatusCode> FetchHats()
    {
        var http = new HttpClient();
        http.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
        var response = await http.GetAsync(new Uri($"{REPO}/CustomHats.json"), HttpCompletionOption.ResponseContentRead);
        try
        {
            if (response.StatusCode != HttpStatusCode.OK) return response.StatusCode;
            if (response.Content == null)
            {
                Message("Server returned no data: " + response.StatusCode.ToString());
                return HttpStatusCode.ExpectationFailed;
            }
            var json = await response.Content.ReadAsStringAsync();
            var jobj = JObject.Parse(json)["hats"];
            if (!jobj.HasValues) return HttpStatusCode.ExpectationFailed;

            var hatdatas = new List<CustomHatOnline>();

            for (var current = jobj.First; current != null; current = current.Next)
            {
                if (current.HasValues)
                {
                    var info = new CustomHatOnline();

                    info.name = current["name"]?.ToString();
                    info.resource = sanitizeResourcePath(current["resource"]?.ToString());
                    if (info.resource == null || info.name == null) // required
                        continue;
                    info.reshasha = current["reshasha"]?.ToString();
                    info.backresource = sanitizeResourcePath(current["backresource"]?.ToString());
                    info.reshashb = current["reshashb"]?.ToString();
                    info.climbresource = sanitizeResourcePath(current["climbresource"]?.ToString());
                    info.reshashc = current["reshashc"]?.ToString();
                    info.flipresource = sanitizeResourcePath(current["flipresource"]?.ToString());
                    info.reshashf = current["reshashf"]?.ToString();
                    info.backflipresource = sanitizeResourcePath(current["backflipresource"]?.ToString());
                    info.reshashbf = current["reshashbf"]?.ToString();

                    info.author = current["author"]?.ToString();
                    info.package = current["package"]?.ToString();
                    info.condition = current["condition"]?.ToString();
                    info.bounce = current["bounce"] != null;
                    info.adaptive = current["adaptive"] != null;
                    info.behind = current["behind"] != null;
                    hatdatas.Add(info);
                }
            }

            var markedfordownload = new List<string>();

            var filePath = Path.GetDirectoryName(Application.dataPath) + @"\CustomHats\";
            if (!Directory.Exists(filePath)) Directory.CreateDirectory(filePath);
            var md5 = MD5.Create();
            foreach (var data in hatdatas)
            {
                if (doesResourceRequireDownload(filePath + data.resource, data.reshasha, md5))
                    markedfordownload.Add(data.resource);
                if (data.backresource != null && doesResourceRequireDownload(filePath + data.backresource, data.reshashb, md5))
                    markedfordownload.Add(data.backresource);
                if (data.climbresource != null && doesResourceRequireDownload(filePath + data.climbresource, data.reshashc, md5))
                    markedfordownload.Add(data.climbresource);
                if (data.flipresource != null && doesResourceRequireDownload(filePath + data.flipresource, data.reshashf, md5))
                    markedfordownload.Add(data.flipresource);
                if (data.backflipresource != null && doesResourceRequireDownload(filePath + data.backflipresource, data.reshashbf, md5))
                    markedfordownload.Add(data.backflipresource);
            }

            foreach (var file in markedfordownload)
            {

                var hatFileResponse = await http.GetAsync($"{REPO}/hats/{file}", HttpCompletionOption.ResponseContentRead);
                if (hatFileResponse.StatusCode != HttpStatusCode.OK) continue;
                using (var responseStream = await hatFileResponse.Content.ReadAsStreamAsync())
                {
                    using (var fileStream = File.Create($"{filePath}\\{file}"))
                    {
                        responseStream.CopyTo(fileStream);
                    }
                }
            }
            hatdetails = hatdatas;
        }
        catch (Exception ex)
        {
            Error(ex.ToString());
            System.Console.WriteLine(ex);
        }
        return HttpStatusCode.OK;
    }

    private static bool doesResourceRequireDownload(string respath, string reshash, MD5 md5)
    {
        if (reshash == null || !File.Exists(respath))
            return true;

        using (var stream = File.OpenRead(respath))
        {
            var hash = BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
            return !reshash.Equals(hash);
        }
    }

    public class CustomHatOnline : CustomHats.CustomHat
    {
        public string reshasha { get; set; }
        public string reshashb { get; set; }
        public string reshashc { get; set; }
        public string reshashf { get; set; }
        public string reshashbf { get; set; }
    }
}
public static class CustomHatExtensions
{
    public static CustomHats.HatExtension getHatExtension(this HatData hat)
    {
        CustomHats.HatExtension ret = null;
        if (CustomHats.TestExt != null && CustomHats.TestExt.condition.Equals(hat.name))
            return CustomHats.TestExt;
        CustomHats.CustomHatRegistry.TryGetValue(hat.name, out ret);
        return ret;
    }
}
