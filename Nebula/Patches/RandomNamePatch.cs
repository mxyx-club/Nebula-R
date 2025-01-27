using System.Reflection;
using System.Text;

namespace Nebula.Patches;

[HarmonyPatch]
internal class RandomNamePatch
{
    private static List<string> RandomNames = new List<string>();

    private static void LoadNames()
    {
        Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Nebula.Resources.RandomName.dat");

        using (StreamReader sr = new StreamReader(
                stream, Encoding.GetEncoding("utf-8")))
        {
            while (sr.Peek() >= 0)
            {
                RandomNames.Add(sr.ReadLine());
            }
        }
    }

    public static string GetRandomName()
    {
        if (RandomNames.Count == 0) LoadNames();

        return RandomNames[NebulaPlugin.rnd.Next(RandomNames.Count)];
    }

    [HarmonyPatch(typeof(RandomNameGenerator), nameof(RandomNameGenerator.GetName))]
    public static class GetRandomNamePatch
    {
        public static bool Prefix(ref string __result, RandomNameGenerator __instance)
        {
            __result = GetRandomName();
            return false;
        }
    }
}