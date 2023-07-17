using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using KhTracker.Hotkeys;

namespace KhTracker;

public class Data
{
    public Mode Mode = Mode.None;
    public bool HintsLoaded = false;
    public Button Selected = null;
    public bool DragDrop = true;
    public int UsedPages = 0;
    public bool ForcedFinal;
    public bool DataSplit = false;
    public string SeedgenVersion = "";
    public bool AltFinalTracking = false;
    public int ConvertedSeedHash = 0;
    public string[] SeedHashVisual = null;
    public readonly bool ShouldResetHash = true;
    public bool SeedHashLoaded = false;
    public bool SpoilerWorldCompletion = false;
    public bool SpoilerReportMode = false;
    public string OpenKhHintText = "None";
    public string OpenKhBossText = "None";
    public string[] HintFileText = new string[3];
    public bool LegacyJsmartee = false;
    public bool LegacyShan = false;
    public string[] ShanHintFileText = null;
    public bool SaveFileLoaded = false;
    public bool SeedLoaded = false;
    public int LastVersion = 0;
    public bool WasTracking = false;
    public readonly Codes Codes = new();

    //extra world stuff
    public readonly Dictionary<string, List<string>> ProgressKeys = new();
    public Dictionary<string, Grid> WorldsTop = new();
    public readonly Dictionary<string, WorldData> WorldsData = new();

    //Item lists
    public readonly List<Item> TornPages = new();
    public readonly List<Item> VisitLocks = new();
    public readonly Dictionary<string, Tuple<Item, Grid>> Items = new();

    //event tracking
    public readonly List<Tuple<string, int, int, int, int, int>> EventLog = new();
    public readonly List<Tuple<string, int, int, int, int, int>> BossEventLog = new();

    //auto-detect
    public BitmapImage AdConnect;
    public BitmapImage AdPc;
    public BitmapImage AdPCred;
    public BitmapImage AdPs2;
    public BitmapImage AdCross;

    //for points hints
    public readonly Dictionary<string, Item> GhostItems = new();
    public readonly Dictionary<string, int> PointsDatanew =
        new()
        {
            //items
            { "proof", 0 },
            { "form", 0 },
            { "magic", 0 },
            { "summon", 0 },
            { "ability", 0 },
            { "page", 0 },
            { "report", 0 },
            { "other", 0 },
            { "visit", 0 },
            //bossrelated
            { "boss_as", 0 },
            { "boss_datas", 0 },
            { "boss_sephi", 0 },
            { "boss_terra", 0 },
            { "boss_other", 0 },
            { "boss_final", 0 },
            //other
            { "complete", 0 },
            { "bonus", 0 },
            { "formlv", 0 },
            { "deaths", 0 },
            //collection bonus
            { "collection_magic", 0 },
            { "collection_page", 0 },
            { "collection_pouches", 0 },
            { "collection_proof", 0 },
            { "collection_form", 0 },
            { "collection_summon", 0 },
            { "collection_ability", 0 },
            { "collection_report", 0 },
            { "collection_visit", 0 },
        };
    public static readonly Dictionary<string, List<string>> WorldItems = new();
    public readonly List<string> TrackedReports = new();
    public readonly List<string> SpoilerRevealTypes = new();

    //for boss rando points
    public bool BossRandoFound = false;
    public readonly Dictionary<string, string> BossList = new();
    public readonly List<string> EnabledWorlds = new();

    //Progression JsmarteeHints stuff
    public int ProgressionPoints = 0;
    public int TotalProgressionPoints = 0;
    public int WorldsEnabled = 0;
    public readonly bool RevealFinalXemnas = false;

    #region Hint Order Logic
    public List<int> HintCosts =
        new() { 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7, 8, 8, 9, 9, 10, 10 };

    //public int NumOfHints = 20;
    public int ProgressionCurrentHint = 0;
    public List<string> HintRevealOrder = new();
    public readonly List<Tuple<string, string, string, bool, bool, bool>> HintRevealsStored = new();
    public bool SynthOn = false;
    public bool PuzzlesOn = false;
    public string PreviousWorldHinted = "";
    #endregion

    #region Bonuses and Sora/Drive Levels
    public int ReportBonus = 1;
    public int WorldCompleteBonus = 0;
    public Dictionary<string, int> StoredWorldCompleteBonus =
        new()
        {
            { "SorasHeart", 0 },
            { "DriveForms", 0 },
            { "SimulatedTwilightTown", 0 },
            { "TwilightTown", 0 },
            { "HollowBastion", 0 },
            { "BeastsCastle", 0 },
            { "OlympusColiseum", 0 },
            { "Agrabah", 0 },
            { "LandofDragons", 0 },
            { "HundredAcreWood", 0 },
            { "PrideLands", 0 },
            { "DisneyCastle", 0 },
            { "HalloweenTown", 0 },
            { "PortRoyal", 0 },
            { "SpaceParanoids", 0 },
            { "TWTNW", 0 },
            { "GoA", 0 },
            { "Atlantica", 0 },
            { "PuzzSynth", 0 }
        };

    //                                              Sora Level - 10 20 30 40 50
    public List<int> LevelsProgressionValues = new() { 1, 1, 1, 2, 4 };
    public int LevelsPreviousIndex = 0;
    public int NextLevelMilestone = 9;

    //                                             Drive Level -  2  3  4  5  6  7
    public List<int> DrivesProgressionValues = new() { 0, 0, 0, 1, 0, 2 };
    public List<int> DriveLevels = new() { 1, 1, 1, 1, 1 };
    #endregion

    #region World Progression Values
    public List<int> SttProgressionValues = new() { 1, 2, 3, 4, 5, 6, 7, 8 };
    public List<int> TtProgressionValues = new() { 1, 2, 3, 4, 5, 6, 7 };
    public List<int> HbProgressionValues = new() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };
    public List<int> CoRProgressionValues = new() { 0, 0, 0, 0, 0 };
    public List<int> BcProgressionValues = new() { 1, 2, 3, 4, 5, 6, 7 };
    public List<int> OcProgressionValues = new() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
    public List<int> AgProgressionValues = new() { 1, 2, 3, 4, 5, 6, 7, 8 };
    public List<int> LoDProgressionValues = new() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
    public List<int> HawProgressionValues = new() { 1, 2, 3, 4, 5, 6 };
    public List<int> PlProgressionValues = new() { 1, 2, 3, 4, 5, 6, 7 };
    public List<int> AtProgressionValues = new() { 1, 2, 3 };
    public List<int> DcProgressionValues = new() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
    public List<int> HtProgressionValues = new() { 1, 2, 3, 4, 5, 6, 7, 8 };
    public List<int> PrProgressionValues = new() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
    public List<int> SpProgressionValues = new() { 1, 2, 3, 4, 5, 6 };
    public List<int> TwtnwProgressionValues = new() { 1, 2, 3, 4, 5, 6, 7 };
    #endregion

    //Hotkey stuff
    public bool UsedHotkey = false;
    public GlobalHotkey StartAutoTracker1,
        StartAutoTracker2;
}

public class WorldData
{
    public bool Hinted; //currently hinted? (for hinted hint logic)
    public bool HintedHint; //currently hinted hint?

    //Progression JsmarteeHints
    public bool HintedProgression;
    public bool Complete; //are all checks found?
    public int Progress; //current world progression
    public bool ContainsGhost; //contains ghost item?
    public int VisitLocks; //visit lock progress

    public List<string> CheckCount = new();

    public Grid Top;
    public Button World;
    public ContentControl Progression;
    public OutlinedTextBlock Value;
    public WorldGrid WorldGrid;

    public WorldData(
        Grid top,
        Button world,
        ContentControl progression,
        OutlinedTextBlock value,
        WorldGrid itemgrid,
        bool hinted,
        int visitLock
    )
    {
        this.Top = top;
        this.World = world;
        this.Progression = progression;
        this.Value = value;
        WorldGrid = itemgrid;
        this.Hinted = hinted;
        HintedHint = false;
        Complete = false;
        Progress = 0;
        ContainsGhost = false;
        VisitLocks = visitLock;
    }
}

public enum Mode
{
    None
}
