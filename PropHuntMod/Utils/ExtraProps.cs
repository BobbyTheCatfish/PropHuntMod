

using System.Collections.Generic;

namespace PropHuntMod.Utils
{
    internal static class ExtraProps
    {
        public static Dictionary<string, string[]> props;

        public static void Init()
        {
            string[] cogwork =
            {
                "CC_metal__0001_28",
                "CC_metal__0005_24",
                "CC_metal__0010_19",
                "CC_metal__0012_17",
                "CC_metal__0023_6",
                "CC_metal__0025_4",
                "CC_metal__0027_2",
                "CC_destroyed_0000_1",
                "CC_destroyed_0001_1",
                "CC_destroyed_0002_1",
                "Cog_Choir"
            };

            string[] abyss =
            {
                "abyss_0001_blue_root_10",
                "abyss_0002_blue_grass_02",
                "abyss_wall_small",
                "abyss_vine_packed_01",
                "Abyss_MID",
                "abyss_egg"
            };

            props = new Dictionary<string, string[]>
            {
                { "Cog_", cogwork },
                { "Abyss_", abyss },
            };
        }
    }
}