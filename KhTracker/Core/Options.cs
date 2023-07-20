using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.IO;
using Microsoft.Win32;
using System.Linq;
using System.Text;
using System.Text.Json;
using Path = System.IO.Path;
using KhTracker.Hotkeys;
using System.Windows.Input;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using MessageForm = System.Windows.Forms;

namespace KhTracker;

public partial class MainWindow
{
    ///
    /// Save/load progress
    ///

    private void DropFile(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            return;

        var files = (string[])e.Data.GetData(DataFormats.FileDrop);

        if (files == null)
            return;

        switch (Path.GetExtension(files[0])?.ToUpper())
        {
            case ".TSV":
                Load(files[0]);
                break;
            case ".YAML":
                LoadArchipelagoSettings(files[0]);
                break;
        }
    }

    private void SaveProgress(object sender, RoutedEventArgs e)
    {
        var saveFileDialog = new SaveFileDialog
        {
            DefaultExt = ".tsv",
            Filter = "Tracker Save File (*.tsv)|*.tsv",
            FileName = "kh2fm-tracker-save",
            InitialDirectory = AppDomain.CurrentDomain.BaseDirectory
        };
        if (saveFileDialog.ShowDialog() == true)
        {
            Save(saveFileDialog.FileName);
        }
    }

    public void Save(string filename)
    {
        #region Settings
        var settingInfo = new bool[31];
        //Display toggles
        settingInfo[0] = false;
        settingInfo[1] = TornPagesOption.IsChecked;
        settingInfo[2] = false;
        settingInfo[3] = AbilitiesOption.IsChecked;
        settingInfo[4] = AntiFormOption.IsChecked;
        settingInfo[5] = VisitLockOption.IsChecked;
        settingInfo[6] = ExtraChecksOption.IsChecked;
        settingInfo[7] = SoraLevel01Option.IsChecked;
        settingInfo[8] = SoraLevel50Option.IsChecked;
        settingInfo[9] = SoraLevel99Option.IsChecked;
        //World toggles
        settingInfo[10] = SoraHeartOption.IsChecked;
        settingInfo[11] = DrivesOption.IsChecked;
        settingInfo[12] = SimulatedOption.IsChecked;
        settingInfo[13] = TwilightTownOption.IsChecked;
        settingInfo[14] = HollowBastionOption.IsChecked;
        settingInfo[15] = BeastCastleOption.IsChecked;
        settingInfo[16] = OlympusOption.IsChecked;
        settingInfo[17] = AgrabahOption.IsChecked;
        settingInfo[18] = LandofDragonsOption.IsChecked;
        settingInfo[19] = DisneyCastleOption.IsChecked;
        settingInfo[20] = PrideLandsOption.IsChecked;
        settingInfo[21] = PortRoyalOption.IsChecked;
        settingInfo[22] = HalloweenTownOption.IsChecked;
        settingInfo[23] = SpaceParanoidsOption.IsChecked;
        settingInfo[24] = TwtnwOption.IsChecked;
        settingInfo[25] = HundredAcreWoodOption.IsChecked;
        settingInfo[26] = AtlanticaOption.IsChecked;
        settingInfo[27] = SynthOption.IsChecked;
        settingInfo[28] = PuzzleOption.IsChecked;
        //other
        settingInfo[29] = false;
        settingInfo[30] = false;
        #endregion

        #region WorldInfo
        var worldvalueInfo = new Dictionary<string, object>();
        foreach (var worldKey in Data.WorldsData.Keys.ToList())
        {
            var worldData = Data.WorldsData[worldKey];
            var worldItems = new List<string>();
            foreach (Item item in worldData.WorldGrid.Children)
            {
                worldItems.Add(item.Name);
            }
            var testingthing = new
            {
                //Value = worldData.value.Text, //do i need this?
                //Progression = worldData.progress, //or this?
                Items = worldItems
                //Hinted = worldData.hinted,
                //HintedHint = worldData.hintedHint,
                //GhostHint = worldData.containsGhost,
                //Complete = worldData.complete,
                //Locks = worldData.visitLocks,
            };
            worldvalueInfo.Add(worldKey, testingthing);
        }

        #endregion

        #region Counters
        var counterInfo = new[] { 1, 1, 1, 1, 1, 1, 0, 0 };
        counterInfo[0] = Data.DriveLevels[0];
        counterInfo[1] = Data.DriveLevels[1];
        counterInfo[2] = Data.DriveLevels[2];
        counterInfo[3] = Data.DriveLevels[3];
        counterInfo[4] = Data.DriveLevels[4];
        if (stats != null)
            counterInfo[5] = stats.Level;
        counterInfo[6] = deathCounter;
        counterInfo[7] = Data.UsedPages;
        #endregion

        var file = File.Create(filename);
        var writer = new StreamWriter(file);
        var saveInfo = new
        {
            Version = Title,
            SeedHash = Data.SeedHashVisual,
            Settings = settingInfo,
            RandomSeed = Data.ConvertedSeedHash,
            Worlds = worldvalueInfo,
            Counters = counterInfo,
            Data.ForcedFinal,
            Events = Data.EventLog,
            BossEvents = Data.BossEventLog,
        };

        var saveFinal = JsonSerializer.Serialize(saveInfo);
        var saveFinal64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(saveFinal));
        var saveScrambled = ScrambleText(saveFinal64, true);
        writer.WriteLine(saveScrambled);
        writer.Close();
    }

    private void LoadProgress(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog
        {
            DefaultExt = ".tsv",
            Filter = "Tracker Save File (*.tsv)|*.tsv",
            FileName = "kh2fm-tracker-save",
            InitialDirectory = AppDomain.CurrentDomain.BaseDirectory
        };
        if (openFileDialog.ShowDialog() == true)
        {
            Load(openFileDialog.FileName);
        }
    }

    private void Load(string filename)
    {
        if (!InProgressCheck("tsv"))
            return;

        //open file
        var reader = new StreamReader(File.Open(filename, FileMode.Open));
        var savescrambled = reader.ReadLine();
        reader.Close();

        //start reading save
        var save64 = ScrambleText(savescrambled, false);
        var saveData = Encoding.UTF8.GetString(Convert.FromBase64String(save64));
        var saveObject = JsonSerializer.Deserialize<Dictionary<string, object>>(saveData);

        //check save version
        if (saveObject.TryGetValue("Version", out var value))
        {
            var saveVer = value.ToString();
            if (saveVer != Title)
            {
                //Console.WriteLine("Different save version!");
                const string message =
                    "This save was made with a different version of the tracker. "
                    + "\n Loading this may cause unintended effects. "
                    + "\n Do you still want to continue loading?";
                const string caption = "Save Version Mismatch";
                const MessageForm.MessageBoxButtons buttons = MessageForm.MessageBoxButtons.YesNo;

                var result = MessageForm.MessageBox.Show(message, caption, buttons);
                if (result == MessageForm.DialogResult.No)
                {
                    return;
                }
            }
        }

        //reset
        OnReset(null, null);

        //continue to loading normally
        LoadNormal(saveObject);

        if (Data.WasTracking)
        {
            InitTracker();
        }
    }

    private void LoadNormal(IDictionary<string, object> savefile)
    {
        //Check Settings
        if (savefile.TryGetValue("Settings", out var value))
        {
            var setting = JsonSerializer.Deserialize<bool[]>(value.ToString()!);
            //Display toggles
            TornPagesToggle(setting[1]);
            AbilitiesToggle(setting[3]);
            AntiFormToggle(setting[4]);
            VisitLockToggle(setting[5]);
            ExtraChecksToggle(setting[6]);
            if (setting[7])
                SoraLevel01Toggle(true);
            else if (setting[8])
                SoraLevel50Toggle(true);
            else if (setting[9])
                SoraLevel99Toggle(true);
            //World toggles
            SoraHeartToggle(setting[10]);
            DrivesToggle(setting[11]);
            SimulatedToggle(setting[12]);
            TwilightTownToggle(setting[13]);
            HollowBastionToggle(setting[14]);
            BeastCastleToggle(setting[15]);
            OlympusToggle(setting[16]);
            AgrabahToggle(setting[17]);
            LandofDragonsToggle(setting[18]);
            DisneyCastleToggle(setting[19]);
            PrideLandsToggle(setting[20]);
            PortRoyalToggle(setting[21]);
            HalloweenTownToggle(setting[22]);
            SpaceParanoidsToggle(setting[23]);
            TwtnwToggle(setting[24]);
            HundredAcreWoodToggle(setting[25]);
            AtlanticaToggle(setting[26]);
            SynthToggle(setting[27]);
            PuzzleToggle(setting[28]);
            ////other
            //settingInfo[29] = GhostItemOption.IsChecked;
            //settingInfo[30] = GhostMathOption.IsChecked;
        }

        //use random seed from save
        if (savefile.ContainsKey("RandomSeed"))
        {
            if (savefile["RandomSeed"] != null)
            {
                var seednumber = JsonSerializer.Deserialize<int>(
                    savefile["RandomSeed"].ToString()!
                );
                Data.ConvertedSeedHash = seednumber;
            }
        }

        //forced final check (unsure if this will actually help with it not mistracking)
        if (savefile.TryGetValue("ForcedFinal", out var forcedFinal))
        {
            var forced = forcedFinal.ToString()?.ToLower();
            Data.ForcedFinal = forced == "true";
        }

        //track obtained items
        if (savefile.TryGetValue("Worlds", out var worldsData))
        {
            var worlds = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, object>>>(
                worldsData.ToString()!
            );
            foreach (var worldDict in worlds)
            {
                var itemlist = JsonSerializer.Deserialize<List<string>>(
                    worldDict.Value["Items"].ToString()!
                );
                foreach (var item in itemlist)
                {
                    var grid = FindName(worldDict.Key + "Grid") as WorldGrid;
                    var importantCheck = FindName(item) as Item;
                    grid!.Add_Item(importantCheck);
                }
            }
        }

        //track events/progression
        if (savefile.TryGetValue("Events", out var events))
        {
            var eventlist = JsonSerializer.Deserialize<
                List<Tuple<string, int, int, int, int, int>>
            >(events.ToString()!);
            foreach (var e in eventlist)
            {
                UpdateWorldProgress(true, e);
            }
        }

        //track boss events
        if (savefile.ContainsKey("BossEvents") && Data.BossRandoFound)
        {
            var bossEventlist = JsonSerializer.Deserialize<
                List<Tuple<string, int, int, int, int, int>>
            >(savefile["BossEvents"].ToString()!);
            foreach (var bossEvent in bossEventlist)
            {
                GetBoss(true, bossEvent);
            }
        }

        //check hash
        if (savefile.ContainsKey("SeedHash"))
        {
            if (savefile["SeedHash"] != null)
            {
                try
                {
                    var hash = JsonSerializer.Deserialize<string[]>(
                        savefile["SeedHash"].ToString()!
                    );
                    Data.SeedHashVisual = hash;

                    //Set Icons
                    HashIcon1.SetResourceReference(ContentProperty, hash[0]);
                    HashIcon2.SetResourceReference(ContentProperty, hash[1]);
                    HashIcon3.SetResourceReference(ContentProperty, hash[2]);
                    HashIcon4.SetResourceReference(ContentProperty, hash[3]);
                    HashIcon5.SetResourceReference(ContentProperty, hash[4]);
                    HashIcon6.SetResourceReference(ContentProperty, hash[5]);
                    HashIcon7.SetResourceReference(ContentProperty, hash[6]);
                    Data.SeedHashLoaded = true;

                    //make visible
                    if (SeedHashOption.IsChecked)
                    {
                        HashGrid.Visibility = Visibility.Visible;
                    }
                }
                catch
                {
                    Data.SeedHashVisual = null;
                    HashGrid.Visibility = Visibility.Hidden;
                    App.Logger?.Record("error while trying to parse seed hash. text corrupted?");
                }
            }
        }

        //end of loading
        Data.SaveFileLoaded = true;
    }

    //progress helpers

    private string ScrambleText(string input, bool scramble)
    {
        //scrambles/unscrambles input text based on a seed
        //why have this? i dunno i suppose to make saves more "secure"
        //figure if people really want to cheat they would have to look at this code
        var r = new Random(16964); //why this number? who knows... (let me know if you figure it out lol)
        if (scramble)
        {
            var chars = input.ToArray();
            for (var i = 0; i < chars.Length; i++)
            {
                var randomIndex = r.Next(0, chars.Length);
                (chars[randomIndex], chars[i]) = (chars[i], chars[randomIndex]);
            }
            return new string(chars);
        }
        else
        {
            var scramChars = input.ToArray();
            var swaps = new List<int>();
            for (var i = 0; i < scramChars.Length; i++)
            {
                swaps.Add(r.Next(0, scramChars.Length));
            }
            for (var i = scramChars.Length - 1; i >= 0; i--)
            {
                (scramChars[swaps[i]], scramChars[i]) = (scramChars[i], scramChars[swaps[i]]);
            }
            return new string(scramChars);
        }
    }

    ///
    /// Load hints
    ///

    // Archipelago settings
    private void LoadArchipelagoSettings(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog
        {
            DefaultExt = ".yaml",
            Filter = "Archipelago Settings (*.yaml)|*.yaml",
            Title = "Select Settings File"
        };
        if (openFileDialog.ShowDialog() == true)
            LoadArchipelagoSettings(openFileDialog.FileName);
    }

    private void LoadArchipelagoSettings(string filename)
    {
        if (!InProgressCheck("seed"))
            return;

        var file = File.ReadAllText(filename);

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(PascalCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

        var settings = deserializer.Deserialize<ArchipelagoSettings>(file);

        if (settings.KingdomHearts2.SuperBosses)
        {
            SettingAbsent.Width = new GridLength(1, GridUnitType.Star);
            SettingSephiroth.Width = new GridLength(1, GridUnitType.Star);
            SettingTerra.Width = new GridLength(1, GridUnitType.Star);
            SettingDatas.Width = new GridLength(1, GridUnitType.Star);
            SettingTransport.Width = new GridLength(1, GridUnitType.Star);
        }

        switch (settings.KingdomHearts2.Cups)
        {
            case "no_cups":
                break;
            case "cups":
                SettingCups.Width = new GridLength(1, GridUnitType.Star);
                break;
            case "cups_and_hades_paradox":
                SettingCups.Width = new GridLength(1, GridUnitType.Star);
                SettingHadesCup.Width = new GridLength(1, GridUnitType.Star);
                break;
        }

        switch (settings.KingdomHearts2.LevelDepth)
        {
            case "level_1":
                SoraLevel01Toggle(true);
                break;
            case "level_50":
                SoraLevel50Toggle(true);
                break;
            case "level_99":
                SoraLevel99Toggle(true);
                break;
        }

        switch (settings.KingdomHearts2.VisitLocking)
        {
            case "no_visit_locking":
                VisitLockToggle(false);
                break;
            case "second_visit_locking":
            case "first_and_second_visit_locking":
                VisitLockToggle(true);
                break;
        }

        switch (settings.KingdomHearts2.Goal)
        {
            case "lucky_emblem_hunt":
                LuckyEmblemsToggle(true);
                LuckyEmblemsRequired.Text = settings.KingdomHearts2.LuckyEmblemsRequired.ToString(
                    "00"
                );
                break;
            default:
                LuckyEmblemsToggle(false);
                break;
        }

        if (Data.WasTracking)
        {
            InitTracker();
        }
    }

    private class ArchipelagoSettings
    {
        [YamlMember(Alias = "Kingdom Hearts 2")]
        public KingdomHearts2Settings KingdomHearts2 { get; set; }

        public class KingdomHearts2Settings
        {
            public string LevelDepth { get; set; }

            [YamlMember(Alias = "Promise_Charm")]
            public bool PromiseCharm { get; set; }

            public bool SuperBosses { get; set; }

            public string Cups { get; set; }

            [YamlMember(Alias = "Visitlocking")]
            public string VisitLocking { get; set; }

            public string Goal { get; set; }

            public int LuckyEmblemsAmount { get; set; }

            public int LuckyEmblemsRequired { get; set; }

            public int BountyAmount { get; set; }

            public int BountyRequired { get; set; }
        }
    }

    //hint helpers

    private void LoadSettings(string settings)
    {
        //item settings
        AbilitiesToggle(false);
        VisitLockToggle(false);
        ExtraChecksToggle(false);
        AntiFormToggle(false);
        TornPagesToggle(false);

        //world settings
        SoraHeartToggle(true);
        DrivesToggle(false);
        SimulatedToggle(false);
        TwilightTownToggle(false);
        HollowBastionToggle(false);
        BeastCastleToggle(false);
        OlympusToggle(false);
        AgrabahToggle(false);
        LandofDragonsToggle(false);
        DisneyCastleToggle(false);
        PrideLandsToggle(false);
        PortRoyalToggle(false);
        HalloweenTownToggle(false);
        SpaceParanoidsToggle(false);
        TwtnwToggle(false);
        HundredAcreWoodToggle(false);
        AtlanticaToggle(false);
        PuzzleToggle(false);
        SynthToggle(false);

        var settinglist = settings.Split('-');

        foreach (var setting in settinglist)
        {
            var trimmed = setting.Trim();
            switch (trimmed)
            {
                case "Second Chance & Once More":
                    AbilitiesToggle(true);
                    break;
                case "AntiForm":
                    AntiFormToggle(true);
                    break;
                case "Visit Locks":
                    VisitLockToggle(true);
                    break;
                case "Extra Checks":
                    ExtraChecksToggle(true);
                    break;
                case "Level01":
                    SoraLevel01Toggle(true);
                    break;
                case "Level50":
                    SoraLevel50Toggle(true);
                    break;
                case "Level99":
                    SoraLevel99Toggle(true);
                    break;
                case "Sora's Heart":
                    SoraHeartToggle(true);
                    break;
                case "Drive Forms":
                    DrivesToggle(true);
                    break;
                case "Simulated Twilight Town":
                    SimulatedToggle(true);
                    Data.EnabledWorlds.Add("STT");
                    break;
                case "Twilight Town":
                    TwilightTownToggle(true);
                    Data.EnabledWorlds.Add("TT");
                    break;
                case "Hollow Bastion":
                    HollowBastionToggle(true);
                    Data.EnabledWorlds.Add("HB");
                    break;
                case "Beast Castle":
                    BeastCastleToggle(true);
                    Data.EnabledWorlds.Add("BC");
                    break;
                case "Olympus":
                    OlympusToggle(true);
                    Data.EnabledWorlds.Add("OC");
                    break;
                case "Agrabah":
                    AgrabahToggle(true);
                    Data.EnabledWorlds.Add("AG");
                    break;
                case "Land of Dragons":
                    LandofDragonsToggle(true);
                    Data.EnabledWorlds.Add("LoD");
                    break;
                case "Disney Castle":
                    DisneyCastleToggle(true);
                    Data.EnabledWorlds.Add("DC");
                    break;
                case "Pride Lands":
                    PrideLandsToggle(true);
                    Data.EnabledWorlds.Add("PL");
                    break;
                case "Port Royal":
                    PortRoyalToggle(true);
                    Data.EnabledWorlds.Add("PR");
                    break;
                case "Halloween Town":
                    HalloweenTownToggle(true);
                    Data.EnabledWorlds.Add("HT");
                    break;
                case "Space Paranoids":
                    SpaceParanoidsToggle(true);
                    Data.EnabledWorlds.Add("SP");
                    break;
                case "TWTNW":
                    TwtnwToggle(true);
                    Data.EnabledWorlds.Add("TWTNW");
                    break;
                case "100 Acre Wood":
                    HundredAcreWoodToggle(true);
                    break;
                case "Atlantica":
                    AtlanticaToggle(true);
                    break;
                case "Puzzles":
                    PuzzleToggle(true);
                    break;
                case "Synthesis":
                    SynthToggle(true);
                    break;
                case "Boss Rando":
                    Data.BossRandoFound = true;
                    break;
                case "Torn Pages":
                    TornPagesToggle(true);
                    break;
            }
        }
    }

    private void OnReset(object sender, RoutedEventArgs e)
    {
        if (_aTimer != null)
        {
            _aTimer.Stop();
            _aTimer = null;
            pcFilesLoaded = false;
        }

        if (sender != null && !AutoConnectOption.IsChecked)
            Data.WasTracking = false;

        //chnage visuals based on if autotracking was done before
        if (Data.WasTracking)
        {
            //connection trying visual
            Connect.Visibility = Visibility.Visible;
            Connect2.Visibility = Visibility.Collapsed;
        }
        else
        {
            Connect.Visibility = Visibility.Collapsed;
            Connect2.Visibility = Visibility.Collapsed;

            SettingRow.Height = new GridLength(0, GridUnitType.Star);
            FormRow.Height = new GridLength(0, GridUnitType.Star);
            Level.Visibility = Visibility.Collapsed;
            Strength.Visibility = Visibility.Collapsed;
            Magic.Visibility = Visibility.Collapsed;
            Defense.Visibility = Visibility.Collapsed;
        }

        collectedChecks.Clear();
        newChecks.Clear();
        ModeDisplay.Header = "";
        HintTextMiddle.Text = "";
        HintTextBegin.Text = "";
        HintTextEnd.Text = "";
        Data.Mode = Mode.None;
        collected = 0;
        Data.UsedPages = 0;
        CollectedValue.Text = "0";
        Data.ForcedFinal = false;
        Data.BossRandoFound = false;
        Data.DataSplit = false;
        Data.BossList.Clear();
        Data.ConvertedSeedHash = 0;
        Data.EnabledWorlds.Clear();
        Data.SeedgenVersion = "";
        Data.AltFinalTracking = false;
        Data.EventLog.Clear();
        Data.HintsLoaded = false;
        Data.SeedLoaded = false;
        Data.SaveFileLoaded = false;

        //clear progression hints stuff
        Data.WorldsEnabled = 0;
        Data.LevelsPreviousIndex = 0;
        Data.NextLevelMilestone = 9;
        Data.LevelsProgressionValues = new List<int> { 1, 1, 1, 2, 4 };
        Data.DrivesProgressionValues = new List<int> { 0, 0, 0, 1, 0, 2 };
        Data.DriveLevels = new List<int> { 1, 1, 1, 1, 1 };
        Data.HintRevealsStored.Clear();
        Data.WorldsData["GoA"].Value.Visibility = Visibility.Hidden;
        //clear last hinted green world
        if (Data.PreviousWorldHinted != "")
        {
            foreach (
                var box in Data.WorldsData[
                    Data.PreviousWorldHinted
                ].Top.Children.OfType<Rectangle>()
            )
            {
                if (Math.Abs(box.Opacity - 0.9) > 0.0001 && !box.Name.EndsWith("SelWG"))
                    box.Fill = (SolidColorBrush)FindResource("DefaultRec");

                if (box.Name.EndsWith("SelWG") && !WorldHighlightOption.IsChecked)
                    box.Visibility = Visibility.Collapsed;
            }
        }
        Data.PreviousWorldHinted = "";
        Data.StoredWorldCompleteBonus = new Dictionary<string, int>
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
        // csharpier-ignore-start
        Data.SttProgressionValues = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8 };
        Data.TtProgressionValues = new List<int> { 1, 2, 3, 4, 5, 6, 7 };
        Data.HbProgressionValues = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };
        Data.CoRProgressionValues = new List<int> { 0, 0, 0, 0, 0 };
        Data.BcProgressionValues = new List<int> { 1, 2, 3, 4, 5, 6, 7 };
        Data.OcProgressionValues = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        Data.AgProgressionValues = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8 };
        Data.LoDProgressionValues = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        Data.HawProgressionValues = new List<int> { 1, 2, 3, 4, 5, 6 };
        Data.PlProgressionValues = new List<int> { 1, 2, 3, 4, 5, 6, 7 };
        Data.AtProgressionValues = new List<int> { 1, 2, 3 };
        Data.DcProgressionValues = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        Data.HtProgressionValues = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8 };
        Data.PrProgressionValues = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        Data.SpProgressionValues = new List<int> { 1, 2, 3, 4, 5, 6 };
        Data.TwtnwProgressionValues = new List<int> { 1, 2, 3, 4, 5, 6, 7 };
        Data.HintCosts = new List<int> { 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7, 8, 8, 9, 9, 10, 10 };
        // csharpier-ignore-end

        //hotkey stuff
        Data.UsedHotkey = false;

        //unselect any currently selected world grid
        if (Data.Selected != null)
        {
            foreach (
                var box in Data.WorldsData[Data.Selected.Name].Top.Children.OfType<Rectangle>()
            )
            {
                if (Math.Abs(box.Opacity - 0.9) > 0.0001 && !box.Name.EndsWith("SelWG"))
                    box.Fill = (SolidColorBrush)FindResource("DefaultRec");

                if (box.Name.EndsWith("SelWG"))
                    box.Visibility = Visibility.Collapsed;
            }
        }
        Data.Selected = null;

        //return items to itempool
        foreach (var worldData in Data.WorldsData.Values.ToList())
        {
            for (var j = worldData.WorldGrid.Children.Count - 1; j >= 0; --j)
            {
                var item = worldData.WorldGrid.Children[j] as Item;

                var pool = Data.Items[item!.Name].Item2;

                worldData.WorldGrid.Children.Remove(worldData.WorldGrid.Children[j]);
                pool.Children.Add(item);

                item.MouseDown -= item.Item_Return;
                if (Data.DragDrop)
                {
                    item.MouseDoubleClick -= item.Item_Click;
                    item.MouseDoubleClick += item.Item_Click;
                    item.MouseMove -= item.Item_MouseMove;
                    item.MouseMove += item.Item_MouseMove;
                }
                else
                {
                    item.MouseDown -= item.Item_MouseDown;
                    item.MouseDown += item.Item_MouseDown;
                    item.MouseUp -= item.Item_MouseUp;
                    item.MouseUp += item.Item_MouseUp;
                }
            }
        }

        // Reset 1st column row heights
        var rows1 = (
            (Data.WorldsData["SorasHeart"].WorldGrid.Parent as Grid)!.Parent as Grid
        )!.RowDefinitions;
        foreach (var row in rows1)
        {
            // don't reset turned off worlds
            if (row.Height.Value != 0)
                row.Height = new GridLength(1, GridUnitType.Star);
        }

        // Reset 2nd column row heights
        var rows2 = (
            (Data.WorldsData["DriveForms"].WorldGrid.Parent as Grid)!.Parent as Grid
        )!.RowDefinitions;
        foreach (var row in rows2)
        {
            // don't reset turned off worlds
            if (row.Height.Value != 0)
                row.Height = new GridLength(1, GridUnitType.Star);
        }

        //fix puzzsynth value if it was hidden (progression hints)
        if (Data.WorldsData["PuzzSynth"].Value.Visibility == Visibility.Hidden)
        {
            Data.WorldsData["PuzzSynth"].Value.Visibility = Visibility.Visible;
        }

        foreach (var key in Data.WorldsData.Keys.ToList())
        {
            Data.WorldsData[key].Progress = 0;

            //world cross reset
            var crossname = key + "Cross";

            if (Data.WorldsData[key].Top.FindName(crossname) is Image cross)
            {
                cross.Visibility = Visibility.Collapsed;
            }

            //reset highlighted world
            foreach (
                var box in Data.WorldsData[key].Top.Children
                    .OfType<Rectangle>()
                    .Where(box => box.Name.EndsWith("SelWG"))
            )
            {
                box.Visibility = Visibility.Collapsed;
            }
        }

        DriveFormsCap.SetResourceReference(ContentProperty, "");
        ChestIcon.SetResourceReference(ContentProperty, "Chest");

        SorasHeartWeapon.SetResourceReference(ContentProperty, "");

        NextLevelCol.Width = new GridLength(0, GridUnitType.Star);

        ValorM.Opacity = .45;
        WisdomM.Opacity = .45;
        LimitM.Opacity = .45;
        MasterM.Opacity = .45;
        FinalM.Opacity = .45;
        HighJump.Opacity = .45;
        QuickRun.Opacity = .45;
        DodgeRoll.Opacity = .45;
        AerialDodge.Opacity = .45;
        Glide.Opacity = .45;

        ValorLevel.Text = "1";
        WisdomLevel.Text = "1";
        LimitLevel.Text = "1";
        MasterLevel.Text = "1";
        FinalLevel.Text = "1";
        HighJumpLevel.Text = "";
        QuickRunLevel.Text = "";
        DodgeRollLevel.Text = "";
        AerialDodgeLevel.Text = "";
        GlideLevel.Text = "";

        fireLevel = 0;
        blizzardLevel = 0;
        thunderLevel = 0;
        cureLevel = 0;
        reflectLevel = 0;
        magnetLevel = 0;
        tornPageCount = 0;

        if (fire != null)
            fire.Level = 0;
        if (blizzard != null)
            blizzard.Level = 0;
        if (thunder != null)
            thunder.Level = 0;
        if (cure != null)
            cure.Level = 0;
        if (reflect != null)
            reflect.Level = 0;
        if (magnet != null)
            magnet.Level = 0;
        if (pages != null)
            pages.Quantity = 0;
        if (luckyEmblems != null)
            luckyEmblems.Quantity = 0;

        visitItemAuronWepQuantity = 0;
        visitItemMulanWepQuantity = 0;
        visitItemBeastWepQuantity = 0;
        visitItemJackWepQuantity = 0;
        visitItemSimbaWepQuantity = 0;
        visitItemSparrowWepQuantity = 0;
        visitItemAladdinWepQuantity = 0;
        visitItemTronWepQuantity = 0;
        visitItemMembershipCardQuantity = 0;
        visitItemIceCreamQuantity = 0;
        visitItemDisneyCastleKeyQuantity = 0;

        if (visitItemAuronWep != null)
            visitItemAuronWep.Quantity = 0;
        if (visitItemMulanWep != null)
            visitItemMulanWep.Quantity = 0;
        if (visitItemBeastWep != null)
            visitItemBeastWep.Quantity = 0;
        if (visitItemJackWep != null)
            visitItemJackWep.Quantity = 0;
        if (visitItemSimbaWep != null)
            visitItemSimbaWep.Quantity = 0;
        if (visitItemSparrowWep != null)
            visitItemSparrowWep.Quantity = 0;
        if (visitItemAladdinWep != null)
            visitItemAladdinWep.Quantity = 0;
        if (visitItemTronWep != null)
            visitItemTronWep.Quantity = 0;
        if (visitItemMembershipCard != null)
            visitItemMembershipCard.Quantity = 0;
        if (visitItemIceCream != null)
            visitItemIceCream.Quantity = 0;
        if (visitItemNaminesSketches != null)
            visitItemNaminesSketches.Quantity = 0;
        if (visitItemDisneyCastleKey != null)
            visitItemDisneyCastleKey.Quantity = 0;
        if (visitItemWayToTheDawn != null)
            visitItemWayToTheDawn.Quantity = 0;

        if (highJump != null)
            highJump.Level = 0;
        if (quickRun != null)
            quickRun.Level = 0;
        if (dodgeRoll != null)
            dodgeRoll.Level = 0;
        if (aerialDodge != null)
            aerialDodge.Level = 0;
        if (glide != null)
            glide.Level = 0;

        //hide & reset seed hash
        if (Data.ShouldResetHash)
        {
            HashGrid.Visibility = Visibility.Collapsed;
            Data.SeedHashLoaded = false;
        }

        WorldGrid.RealFire = 0;
        WorldGrid.RealBlizzard = 0;
        WorldGrid.RealThunder = 0;
        WorldGrid.RealCure = 0;
        WorldGrid.RealReflect = 0;
        WorldGrid.RealMagnet = 0;
        WorldGrid.RealPages = 0;
        WorldGrid.RealPouches = 0;
        WorldGrid.ProofCount = 0;
        WorldGrid.FormCount = 0;
        WorldGrid.SummonCount = 0;
        WorldGrid.AbilityCount = 0;
        WorldGrid.ReportCount = 0;
        WorldGrid.VisitCount = 0;

        FireCount.Text = "3";
        BlizzardCount.Text = "3";
        ThunderCount.Text = "3";
        CureCount.Text = "3";
        ReflectCount.Text = "3";
        MagnetCount.Text = "3";
        PageCount.Text = "5";
        MunnyCount.Text = "2";

        FireCount.Fill = (SolidColorBrush)FindResource("ColorBlack");
        FireCount.Stroke = (SolidColorBrush)FindResource("ColorTrans");
        FireCount.Fill = (LinearGradientBrush)FindResource("ColorFire");
        FireCount.Stroke = (SolidColorBrush)FindResource("ColorBlack");
        BlizzardCount.Fill = (SolidColorBrush)FindResource("ColorBlack");
        BlizzardCount.Stroke = (SolidColorBrush)FindResource("ColorTrans");
        BlizzardCount.Fill = (LinearGradientBrush)FindResource("ColorBlizzard");
        BlizzardCount.Stroke = (SolidColorBrush)FindResource("ColorBlack");
        ThunderCount.Fill = (SolidColorBrush)FindResource("ColorBlack");
        ThunderCount.Stroke = (SolidColorBrush)FindResource("ColorTrans");
        ThunderCount.Fill = (LinearGradientBrush)FindResource("ColorThunder");
        ThunderCount.Stroke = (SolidColorBrush)FindResource("ColorBlack");
        CureCount.Fill = (SolidColorBrush)FindResource("ColorBlack");
        CureCount.Stroke = (SolidColorBrush)FindResource("ColorTrans");
        CureCount.Fill = (LinearGradientBrush)FindResource("ColorCure");
        CureCount.Stroke = (SolidColorBrush)FindResource("ColorBlack");
        MagnetCount.Fill = (SolidColorBrush)FindResource("ColorBlack");
        MagnetCount.Stroke = (SolidColorBrush)FindResource("ColorTrans");
        MagnetCount.Fill = (LinearGradientBrush)FindResource("ColorMagnet");
        MagnetCount.Stroke = (SolidColorBrush)FindResource("ColorBlack");
        ReflectCount.Fill = (SolidColorBrush)FindResource("ColorBlack");
        ReflectCount.Stroke = (SolidColorBrush)FindResource("ColorTrans");
        ReflectCount.Fill = (LinearGradientBrush)FindResource("ColorReflect");
        ReflectCount.Stroke = (SolidColorBrush)FindResource("ColorBlack");
        PageCount.Fill = (SolidColorBrush)FindResource("ColorBlack");
        PageCount.Stroke = (SolidColorBrush)FindResource("ColorTrans");
        PageCount.Fill = (LinearGradientBrush)FindResource("ColorPage");
        PageCount.Stroke = (SolidColorBrush)FindResource("ColorBlack");
        MunnyCount.Fill = (SolidColorBrush)FindResource("ColorBlack");
        MunnyCount.Stroke = (SolidColorBrush)FindResource("ColorTrans");
        MunnyCount.Fill = (LinearGradientBrush)FindResource("ColorPouch");
        MunnyCount.Stroke = (SolidColorBrush)FindResource("ColorBlack");

        Data.WorldItems.Clear();
        Data.TrackedReports.Clear();

        CollectionGrid.Visibility = Visibility.Visible;
        ScoreGrid.Visibility = Visibility.Hidden;
        ProgressionCollectionGrid.Visibility = Visibility.Hidden;

        //reset settings row
        SettingsText.Text = "";
        SettingBetterStt.Width = new GridLength(0, GridUnitType.Star);
        SettingLevel01.Width = new GridLength(0, GridUnitType.Star);
        SettingLevel50.Width = new GridLength(0, GridUnitType.Star);
        SettingLevel99.Width = new GridLength(0, GridUnitType.Star);
        SettingAbsent.Width = new GridLength(0, GridUnitType.Star);
        SettingAbsentSplit.Width = new GridLength(0, GridUnitType.Star);
        SettingDatas.Width = new GridLength(0, GridUnitType.Star);
        SettingSephiroth.Width = new GridLength(0, GridUnitType.Star);
        SettingTerra.Width = new GridLength(0, GridUnitType.Star);
        SettingCups.Width = new GridLength(0, GridUnitType.Star);
        SettingHadesCup.Width = new GridLength(0, GridUnitType.Star);
        SettingCavern.Width = new GridLength(0, GridUnitType.Star);
        SettingTransport.Width = new GridLength(0, GridUnitType.Star);
        SettingSpacer.Width = new GridLength(10, GridUnitType.Star);

        TornPagesToggle(true);
        VisitLockToggle(VisitLockOption.IsChecked);

        deathCounter = 0;
        DeathValue.Text = "0";
        DeathCol.Width = new GridLength(0, GridUnitType.Star);

        foreach (Grid itempool in ItemPool.Children)
        {
            foreach (var item in itempool.Children)
            {
                if (item is ContentControl check && !check.Name.Contains("Ghost"))
                    check.Opacity = 1.0;
            }
        }

        NextLevelDisplay();

        //reset progression visuals
        PpCount.Width = new GridLength(1.15, GridUnitType.Star);
        PpSep.Width = new GridLength(0.3, GridUnitType.Star);

        if (Data.WasTracking && sender != null)
            InitTracker();
    }

    private bool InProgressCheck(string type)
    {
        var message = "";
        var caption = "";

        if (Data.SeedLoaded | Data.SaveFileLoaded)
        {
            if (type == "tsv")
            {
                message =
                    "Hints were already loaded into the tracker!"
                    + "\n Any progress made so far would be lost if you continue."
                    + "\n Proceed anyway?";
                caption = "Progress Load Confirmation";
            }
            if (type == "seed")
            {
                message =
                    "A Randomizer Seed was already loaded into the tracker!"
                    + "\n Any progress made so far would be lost if you continue."
                    + "\n Proceed anyway?";
                caption = "Seed Load Confirmation";
            }
            if (type == "hints")
            {
                message =
                    "Hints were already loaded into the tracker!"
                    + "\n Any progress made so far would be lost if you continue."
                    + "\n Proceed anyway?";
                caption = "Hints Load Confirmation";
            }

            var result = MessageForm.MessageBox.Show(
                message,
                caption,
                MessageForm.MessageBoxButtons.OKCancel
            );
            if (result == MessageForm.DialogResult.Cancel)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        else
            return true;
    }

    ///
    /// Hotkey logic
    ///

    private void LoadHotkeyBind()
    {
        if (!Directory.Exists("./KhTrackerSettings"))
        {
            Directory.CreateDirectory("./KhTrackerSettings");
        }

        if (!File.Exists("./KhTrackerSettings/AutoTrackerKeybinds.txt"))
        {
            //Console.WriteLine("File not found, making");
            using var fs = File.Create("./KhTrackerSettings/AutoTrackerKeybinds.txt");

            // Add some text to file
            var title = new UTF8Encoding(true).GetBytes("Control\n");
            fs.Write(title, 0, title.Length);
            var author = new UTF8Encoding(true).GetBytes("F12");
            fs.Write(author, 0, author.Length);
        }
        var lines = File.ReadAllLines("./KhTrackerSettings/AutoTrackerKeybinds.txt");
        var mod1 = "";
        var _mod1 = ModifierKeys.None;
        var mod2 = "";
        var _mod2 = ModifierKeys.None;
        var mod3 = "";
        var _mod3 = ModifierKeys.None;
        var key = "";
        var modsUsed = 0;
        Key _key;

        Console.WriteLine(lines[1]);

        //break out early if empty file
        if (lines.Length == 0)
        {
            Console.WriteLine(@"No keybind set");
            Data.StartAutoTracker1 = null;
            return;
        }

        if (lines.Length > 1)
            key = lines[1];

        //get first line, split around +'s
        var modifiers = lines[0].ToLower();
        if (modifiers.IndexOf('+') > 0)
        {
            mod1 = modifiers[..modifiers.IndexOf('+')];
            modifiers = modifiers[(modifiers.IndexOf('+') + 1)..];
            modsUsed++;
        }
        else
        {
            mod1 = modifiers;
        }
        if (modifiers.IndexOf('+') > 0)
        {
            mod2 = modifiers[..modifiers.IndexOf('+')];
            modifiers = modifiers[(modifiers.IndexOf('+') + 1)..];
            modsUsed++;
        }
        else
        {
            mod2 = modifiers;
        }
        if (modifiers.Length > 0)
        {
            mod3 = modifiers;
            modsUsed++;
        }

        if (mod1.Contains("ctrl"))
            mod1 = "control";
        if (mod2.Contains("ctrl"))
            mod2 = "control";
        if (mod3.Contains("ctrl"))
            mod3 = "control";

        //capitalize all letters
        mod1 = UpperCaseFirst(mod1);
        mod2 = UpperCaseFirst(mod2);
        mod3 = UpperCaseFirst(mod3);
        key = UpperCaseFirst(key);

        //if no modifiers, only 1 key
        if (key == "")
        {
            _ = Enum.TryParse(mod1, out _key);
            Data.StartAutoTracker1 = new GlobalHotkey(ModifierKeys.None, _key, StartHotkey);
            HotkeysManager.AddHotkey(Data.StartAutoTracker1);
            return;
        }

        //check for modifiers, however many
        if (mod1 != "")
            _ = Enum.TryParse(mod1, out _mod1);
        if (mod2 != "")
            _ = Enum.TryParse(mod2, out _mod2);
        if (mod3 != "")
            _ = Enum.TryParse(mod3, out _mod3);

        switch (modsUsed)
        {
            //per used amount
            case 3:
            {
                Console.WriteLine($@"idk = {mod1} {mod2} {mod3} {key}");
                if (key is "1" or "2" or "3" or "4" or "5" or "6" or "7" or "8" or "9" or "0")
                {
                    _ = Enum.TryParse(ConvertKeyNumber(key, true), out _key);
                    Data.StartAutoTracker1 = new GlobalHotkey(
                        (_mod1 | _mod2 | _mod3),
                        _key,
                        StartHotkey
                    );
                    HotkeysManager.AddHotkey(Data.StartAutoTracker1);

                    _ = Enum.TryParse(ConvertKeyNumber(key, false), out _key);
                    Data.StartAutoTracker2 = new GlobalHotkey(
                        (_mod1 | _mod2 | _mod3),
                        _key,
                        StartHotkey
                    );
                    HotkeysManager.AddHotkey(Data.StartAutoTracker2);
                    return;
                }
                _ = Enum.TryParse(key, out _key);
                Data.StartAutoTracker1 = new GlobalHotkey(
                    (_mod1 | _mod2 | _mod3),
                    _key,
                    StartHotkey
                );
                HotkeysManager.AddHotkey(Data.StartAutoTracker1);
                return;
            }
            case 2:
            {
                Console.WriteLine($@"idk = {mod1} {mod2} {key}");
                if (key is "1" or "2" or "3" or "4" or "5" or "6" or "7" or "8" or "9" or "0")
                {
                    _ = Enum.TryParse(ConvertKeyNumber(key, true), out _key);
                    Data.StartAutoTracker1 = new GlobalHotkey((_mod1 | _mod2), _key, StartHotkey);
                    HotkeysManager.AddHotkey(Data.StartAutoTracker1);

                    _ = Enum.TryParse(ConvertKeyNumber(key, false), out _key);
                    Data.StartAutoTracker2 = new GlobalHotkey((_mod1 | _mod2), _key, StartHotkey);
                    HotkeysManager.AddHotkey(Data.StartAutoTracker2);
                    return;
                }
                _ = Enum.TryParse(key, out _key);
                Data.StartAutoTracker1 = new GlobalHotkey((_mod1 | _mod2), _key, StartHotkey);
                HotkeysManager.AddHotkey(Data.StartAutoTracker1);
                return;
            }
            default:
            {
                Console.WriteLine(@$"idk = {mod1} {key}");
                if (key is "1" or "2" or "3" or "4" or "5" or "6" or "7" or "8" or "9" or "0")
                {
                    _ = Enum.TryParse(ConvertKeyNumber(key, true), out _key);
                    Data.StartAutoTracker1 = new GlobalHotkey(_mod1, _key, StartHotkey);
                    HotkeysManager.AddHotkey(Data.StartAutoTracker1);

                    _ = Enum.TryParse(ConvertKeyNumber(key, false), out _key);
                    Data.StartAutoTracker2 = new GlobalHotkey(_mod1, _key, StartHotkey);
                    HotkeysManager.AddHotkey(Data.StartAutoTracker2);
                    return;
                }
                _ = Enum.TryParse(ConvertKey(key), out _key);
                Data.StartAutoTracker1 = new GlobalHotkey(_mod1, _key, StartHotkey);
                HotkeysManager.AddHotkey(Data.StartAutoTracker1);
                return;
            }
        }
    }

    private string UpperCaseFirst(string word)
    {
        if (word.Length <= 0)
            return "";

        var firstLetter1 = word[..1];
        var firstLetter2 = firstLetter1.ToUpper();
        var rest = word[1..];

        return firstLetter2 + rest;
    }

    private string ConvertKey(string key)
    {
        return key switch
        {
            "." => "OemPeriod",
            "," => "OemComma",
            "?" => "OemPeriod",
            "\"" => "OemQuestion",
            "'" => "OemQuotes",
            "[" => "OemOpenBrackets",
            "{" => "OemOpenBrackets",
            "]" => "OemCloseBrackets",
            "}" => "OemCloseBrackets",
            "\\" => "OemBackslash",
            ":" => "OemSemicolon",
            ";" => "OemSemicolon",
            "-" => "OemMinus",
            "_" => "OemMinus",
            "+" => "OemPlus",
            "=" => "OemPlus",
            "|" => "OemPipe",
            _ => key
        };
    }

    private string ConvertKeyNumber(string num, bool type)
    {
        return num switch
        {
            "1" => type ? "D1" : "NumPad1",
            "2" => type ? "D2" : "NumPad2",
            "3" => type ? "D3" : "NumPad3",
            "4" => type ? "D4" : "NumPad4",
            "5" => type ? "D5" : "NumPad5",
            "6" => type ? "D6" : "NumPad6",
            "7" => type ? "D7" : "NumPad7",
            "8" => type ? "D8" : "NumPad8",
            "9" => type ? "D9" : "NumPad9",
            _ => type ? "D0" : "NumPad0"
        };
    }
}
