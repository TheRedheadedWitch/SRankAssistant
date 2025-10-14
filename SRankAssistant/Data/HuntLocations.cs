namespace SRankAssistant;

internal class HuntLocations
{
    internal static readonly List<(uint id, string zoneName, string sRankName, string condition)> HuntInformation = new()
{
        // A Realm Reborn (ARR)
        (134, "Middle La Noscea", "Croque-mitaine", "Chance - Mining Grade 3 La Noscean Topsoil"),
        (135, "Lower La Noscea", "Croakadile", "Travel over spawn location during nights of a full moon"),
        (137, "Eastern La Noscea", "The Garlok", "(AUTO-SPAWNS) After 200 minutes (real, not game) of no rain/showers in the zone."),
        (138, "Western La Noscea", "Bonnacon", "Chance - Harvesting LaNoscean Leeks"),
        (139, "Upper La Noscea", "Nandi", "Chance - Travelling over spawn with any minion out"),
        (180, "Outer La Noscea", "Chernobog", "Chance - Player death"),

        (148, "Central Shroud", "Laideronnette", "(AUTO_SPAWNS) After 30 minutes (real, not game) of rain in the zone"),
        (152, "East Shroud", "Wulgaru", "Chance - Start a Battlecraft/Grand Company Levequest"),
        (153, "South Shroud", "Mindflayer", "Travel over spawn location during night of new moon"),
        (154, "North Shroud", "Thousand-cast Theda", "Chance - Fish a Judgeray"),

        (140, "Western Thanalan", "Zona Seeker", "Chance - Fish a Glimmerscale"),
        (141, "Central Thanalan", "Brontes", "Eat at Spawn location"),
        (145, "Eastern Thanalan", "Lampalagua", "Chance - Start a Battlecraft/GrandCompany Levequest"),
        (146, "Southern Thanalan", "Nunyunuwi", "1 hour of no fates failing"),
        (147, "Northern Thanalan", "Minhocao", "Kill - 100 Earh Sprites"),

        (155, "Coerthas Central Highlands", "Safat", "Drop to 1hp from falling"),
        (165, "Mor Dhona", "Agrippa the Mighty", "Chance - Open Treasure Chest"),

        // Heavensward (HW)
        (397, "Coerthas Western Highlands", "Kaiser Behemoth", "Chance - Travel over spawn with Behemoth Heir minion out"),
        (401, "Sea of Clouds", "Bird of Paradise", "Chance - Squonk uses Chirp attack"),
        (402, "Azys Lla", "Leucrotta", "Kill - 50 Allagan Chimera, 50 Lesser Hydra, 50 Meracydian Vouivre"),
        (398, "The Dravanian Forelands", "Senmurv", "Fate - Cerf's Up 5 times in a row"),
        (399, "The Dravanian Hinterlands", "The Pale Rider", "Chance - Open Treasure Chest"),
        (400, "The Churning Mists", "Gandarewa", "Gathering Strikes - 50 Aurum Regis, 50 Seventh Heaven"),

        // Stormblood (SB)
        (612, "The Fringes", "Udumbara", "Kill - 100 Leshy, 100 Diakka"),
        (620, "The Peaks", "Bone Crawler", "Chance - Using Chocobo Porter"),
        (621, "The Lochs", "Salt and Light", "Discard - Any 50 individual items"),
        (613, "The Ruby Sea", "Okina", "Kill - 100 Yumemu, 100 Naked Yumemi"),
        (622, "The Azim Steppe", "Orghana", "Chance, Fate - Suceed at Not Just a Tribute, travel over spawn spot"),
        (614, "Yanxia", "Gamma", "Chance - travel over spawn spot with Toy Alexander minion out"),

        // Shadowbringers (ShB)
        (813, "Lakeland", "Tyger", "Discard - Rai Tenderloin"),
        (814, "Kholusia", "Forgiven Pedantry", "Gather Strikes - 50 Dwarnven Cotton Boll"),
        (815, "Amh Araeng", "Tarchia", "Chance - Self Destruct on spawn location"),
        (816, "Il Mheg", "Aglaope", "Chance - Travel over spawn spot with Scarlet Peacock minion out"),
        (817, "The Rak'tika Greatwood", "Ixtab", "Kill - 100 Cracked Ronkan Doll, 100 Cracked Ronkan Thorn, 100 Cracked Ronkan Vessel"),
        (818, "The Tempest", "Gunitt", "Clionid eats 3 leeches and attack with Buccal Cones"),

        // Endwalker (EW)
        (956, "Labyrinthos", "Burfurlur The Canny", "Chance - Travel over spawn spot with Tiny Troll minion out"),
        (957, "Thavnair", "Sphatika", "Kill - 100 Asvattha, 100 Pisaca, 100 Vajralangula"),
        (958, "Garlemald", "Arch-Daimon", "Die on spawn spot wearing Mended Imperial Pot Helm and Mended Imperial Short Robe"),
        (959, "Mare Lamentorum", "Ruminator", "Kill - 100 Thinkers, 100 Wanderers, 100 Weepers"),
        (960, "Ultima Thule", "Narrow-rift", "Chance - 10 players on spawn with Wee Ea minions out"),
        (961, "Elpis", "Ophioneus", "Discard - 5 Eggs of Elpis at once"),

        // Dawntrail (DT)
        (1187, "Urqupacha", "Kirlirger the Abhorrent", "Chance - Travel over spawn spot in fog and new moon at night"),
        (1188, "Kozama'uka", "Neyoozoteel", "Chance - Travel over spawn spot with Morpho minion out"),
        (1189, "Yak T'el", "Ihnuxokiy", "Discard - 50 Fish Meal at once"),
        (1190, "Shaaloani", "Sansheya", "Fate - You Are What you Drink fate, completed 3 times in a row"),
        (1191, "Heritage Found", "Atticus the Primogenitor", "Craft a high-quality Rroneek Steak"),
        (1192, "Living Memory", "The Forecaster", "Chance - Cast Northerlies on spawn spot"),
    };

    internal static string GetCondition() => HuntInformation.FirstOrDefault(x => x.id == SERVICES.ClientState.TerritoryType) is { } location ? location.condition : null;
    internal static uint GetSRankZoneId(string sRankName) => HuntInformation.FirstOrDefault(x => string.Equals(x.sRankName, sRankName, StringComparison.OrdinalIgnoreCase)) is { } location ? location.id : 0;
    internal static uint ? GetZoneId(string zoneName) => HuntInformation.FirstOrDefault(x => string.Equals(x.zoneName, zoneName, StringComparison.OrdinalIgnoreCase)) is { } location ? location.id : null;
    internal static string GetZoneName() => HuntInformation.FirstOrDefault(x => x.id == SERVICES.ClientState.TerritoryType) is { } location ? location.zoneName : null;
    internal static string GetSRankName() => HuntInformation.FirstOrDefault(x => x.id == SERVICES.ClientState.TerritoryType) is { } location ? location.sRankName : null;
    internal static bool IsHuntLocation() => HuntInformation.Any(x => x.id == SERVICES.ClientState.TerritoryType);
}
