using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace KhTracker;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    #region Variables

    private MemoryReader memory;

    private int addressOffset;
    private static DispatcherTimer _aTimer;
    private static DispatcherTimer _checkTimer;
    private List<ImportantCheck> importantChecks;
    private Ability highJump;
    private Ability quickRun;
    private Ability dodgeRoll;
    private Ability aerialDodge;
    private Ability glide;

    private Ability secondChance;
    private Ability onceMore;

    private DriveForm valor;
    private DriveForm wisdom;
    private DriveForm master;
    private DriveForm limit;
    private DriveForm final;
    private DriveForm anti;

    private Magic fire;
    private Magic blizzard;
    private Magic thunder;
    private Magic magnet;
    private Magic reflect;
    private Magic cure;

    private Report reportItem;
    private Summon charmItem;

    private Visit visitItemAuronWep;
    private Visit visitItemMulanWep;
    private Visit visitItemBeastWep;
    private Visit visitItemJackWep;
    private Visit visitItemSimbaWep;
    private Visit visitItemSparrowWep;
    private Visit visitItemAladdinWep;
    private Visit visitItemTronWep;
    private Visit visitItemMembershipCard;
    private Visit visitItemIceCream;
    private Visit visitItemNaminesSketches;
    private Visit visitItemDisneyCastleKey;
    private Visit visitItemWayToTheDawn;

    private ImportantCheck extraItem;

    private LuckyEmblem luckyEmblems;

    private TornPage pages;

    private World world;
    private Stats stats;
    private Rewards rewards;
    private readonly List<ImportantCheck> collectedChecks;
    private readonly List<ImportantCheck> newChecks;
    private readonly List<ImportantCheck> previousChecks;

    private int fireLevel;
    private int blizzardLevel;
    private int thunderLevel;
    private int cureLevel;
    private int reflectLevel;
    private int magnetLevel;
    private int tornPageCount;

    private int visitItemAuronWepQuantity;
    private int visitItemMulanWepQuantity;
    private int visitItemBeastWepQuantity;
    private int visitItemJackWepQuantity;
    private int visitItemSimbaWepQuantity;
    private int visitItemSparrowWepQuantity;
    private int visitItemAladdinWepQuantity;
    private int visitItemTronWepQuantity;
    private int visitItemMembershipCardQuantity;
    private int visitItemIceCreamQuantity;
    private int visitItemDisneyCastleKeyQuantity;

    private CheckEveryCheck checkEveryCheck;

    private bool pcFilesLoaded;

    private bool onContinue; //for death counter
    private bool eventInProgress; //boss detection
    #endregion

    ///
    /// Autotracking Startup
    ///

    public void StartHotkey()
    {
        if (Data.UsedHotkey)
            return;

        Data.UsedHotkey = true;
        var vercheck = CheckVersion();
        if (vercheck == 2)
        {
            InitAutoTracker();
        }
        else
        {
            MessageBox.Show("No game detected.\nPlease start KH2 before using Hotkey.");
            Data.UsedHotkey = false;
        }
    }

    //buttons merged so no need for both of these anymore
    //public void InitPCSX2Tracker(object sender, RoutedEventArgs e)
    //{
    //    pcsx2tracking = true;
    //    InitAutoTracker(true);
    //}
    //
    //public void InitPCTracker(object sender, RoutedEventArgs e)
    //{
    //    pcsx2tracking = false;
    //    InitAutoTracker(false);
    //}

    public void InitTracker(object sender, RoutedEventArgs e)
    {
        if (_aTimer != null)
            return;

        InitTracker();
    }

    private void InitTracker()
    {
        //reset timer if already running
        _aTimer?.Stop();

        //connection trying visual
        Connect.Visibility = Visibility.Visible;
        Connect2.Visibility = Visibility.Collapsed;

        //start timer for checking game version
        _checkTimer = new DispatcherTimer();
        _checkTimer.Tick += InitSearch;
        _checkTimer.Interval = new TimeSpan(0, 0, 0, 2, 5);
        _checkTimer.Start();
    }

    public void InitSearch(object sender, EventArgs e)
    {
        //NOTE: connected version
        //0 = none | 1 = ps2 | 2 = pc
        var checkedVer = CheckVersion();

        if (checkedVer == 0) //no game was detected.
        {
            //return and keep trying to connect if auto-connect is enabled.
            if (AutoConnectOption.IsChecked)
            {
                return;
            }
            else
            {
                Connect.Visibility = Visibility.Collapsed;
                Connect2.Visibility = Visibility.Visible;
                Connect2.Source = Data.AdCross;
                _checkTimer.Stop();
                _checkTimer = null;
                memory = null;
                MessageBox.Show("Please start KH2 before starting the Auto Tracker.");
            }
        }
        else
        {
            //if for some reason user starts playing an different version
            if (Data.LastVersion != 0 && Data.LastVersion != checkedVer)
            {
                //reset tracker
                OnReset(null, null);
            }

            //stop timer for checking game version
            if (_checkTimer != null)
            {
                _checkTimer.Stop();
                _checkTimer = null;
            }

            Connect2.Source = Data.AdPCred;

            //make visual visible
            Connect.Visibility = Visibility.Collapsed;
            Connect2.Visibility = Visibility.Visible;

            //finally start auto-tracking process
            InitAutoTracker();
        }
    }

    private int CheckVersion()
    {
        var pcSuccess = true;
        var tries = 0;

        //check pc now
        do
        {
            memory = new MemoryReader();
            if (tries < 20)
            {
                tries++;
            }
            else
            {
                memory = null;
                //Console.WriteLine("No PC Version Detected");
                pcSuccess = false;
                break;
            }
        } while (!memory.Hooked);
        if (pcSuccess)
        {
            if (Data.LastVersion == 0)
                Data.LastVersion = 2;
            return 2;
        }

        //no version found
        return 0;
    }

    private async void InitAutoTracker()
    {
        // PC Address anchors
        const int now = 0x0714DB8;
        const int save = 0x09A70B0;
        const int sys3 = 0x2A59DF0;
        const int bt10 = 0x2A74880;
        const int btlEnd = 0x2A0D3E0;
        const int slot1 = 0x2A20C98;
        const int nextSlot = 0x278;

        //check for if the system files are loaded every 1/2 second.
        //this helps ensure that ICs on levels/drives never mistrack
        //(this was a rare instance that could happen on pc because
        //the data takes a small bit of time to fully load.)
        while (!pcFilesLoaded)
        {
            pcFilesLoaded = CheckPcLoaded();
            await Task.Delay(500);
        }

        //the pc files are fully loaded so we can change the connect icon.
        Connect2.Source = Data.AdPc;

        try
        {
            CheckPcOffset();
        }
        catch (Win32Exception)
        {
            memory = null;
            Connect2.Source = Data.AdCross;
            MessageBox.Show("Unable to access KH2FM try running KHTracker as admin");
            return;
        }
        catch
        {
            memory = null;
            Connect2.Source = Data.AdCross;
            MessageBox.Show("Error connecting to KH2FM");
            return;
        }

        FinishSetup(now, save, sys3, bt10, btlEnd, slot1, nextSlot);
    }

    private void CheckPcOffset()
    {
        const int testAddr = 0x009AA376 - 0x1000;
        const string good = "F680";
        var tester = BytesToHex(memory.ReadMemory(testAddr, 2));
        if (tester == good)
        {
            addressOffset = -0x1000;
        }
    }

    private bool CheckPcLoaded()
    {
        ////checks if these files have been loaded into memeory
        const int obj0 = 0x2A22BD0;
        var prg0 = obj0 + ReadMemInt(obj0 - 0x10) + 0x10;
        var sys3 = prg0 + ReadMemInt(prg0 - 0x10) + 0x10;
        var btl0 = ReadMemInt(sys3 - 0x10);

        // all important files loaded?
        return btl0 > 0x20;
    }

    private void FinishSetup(
        int now,
        int save,
        int sys3,
        int bt10,
        int btlEnd,
        int slot1,
        int nextSlot
    )
    {
        #region Add ICs
        importantChecks = new List<ImportantCheck>();
        importantChecks.Add(
            highJump = new Ability(memory, save + 0x25DA, addressOffset, 0x05E, "HighJump")
        );
        importantChecks.Add(
            quickRun = new Ability(memory, save + 0x25DC, addressOffset, 0x62, "QuickRun")
        );
        importantChecks.Add(
            dodgeRoll = new Ability(memory, save + 0x25DE, addressOffset, 0x234, "DodgeRoll")
        );
        importantChecks.Add(
            aerialDodge = new Ability(memory, save + 0x25E0, addressOffset, 0x066, "AerialDodge")
        );
        importantChecks.Add(
            glide = new Ability(memory, save + 0x25E2, addressOffset, 0x6A, "Glide")
        );

        importantChecks.Add(
            secondChance = new Ability(memory, save + 0x2544, addressOffset, "SecondChance", save)
        );
        importantChecks.Add(
            onceMore = new Ability(memory, save + 0x2544, addressOffset, "OnceMore", save)
        );

        importantChecks.Add(
            wisdom = new DriveForm(memory, save + 0x36C0, addressOffset, 2, save + 0x332E, "Wisdom")
        );
        importantChecks.Add(
            limit = new DriveForm(memory, save + 0x36CA, addressOffset, 3, save + 0x3366, "Limit")
        );
        importantChecks.Add(
            master = new DriveForm(memory, save + 0x36C0, addressOffset, 6, save + 0x339E, "Master")
        );
        importantChecks.Add(
            anti = new DriveForm(memory, save + 0x36C0, addressOffset, 5, save + 0x340C, "Anti")
        );

        if (!Data.AltFinalTracking)
        {
            importantChecks.Add(
                valor = new DriveForm(
                    memory,
                    save + 0x36C0,
                    addressOffset,
                    1,
                    save + 0x32F6,
                    save + 0x06B2,
                    "Valor"
                )
            );
            importantChecks.Add(
                final = new DriveForm(
                    memory,
                    save + 0x36C0,
                    addressOffset,
                    4,
                    save + 0x33D6,
                    "Final"
                )
            );
        }
        else
        {
            importantChecks.Add(
                valor = new DriveForm(
                    memory,
                    save + 0x36C0,
                    addressOffset,
                    7,
                    save + 0x32F6,
                    "Valor"
                )
            );
            importantChecks.Add(
                final = new DriveForm(
                    memory,
                    save + 0x36C2,
                    addressOffset,
                    1,
                    save + 0x33D6,
                    "Final"
                )
            );
        }

        var fireCount = fire?.Level ?? 0;
        var blizzardCount = blizzard?.Level ?? 0;
        var thunderCount = thunder?.Level ?? 0;
        var cureCount = cure?.Level ?? 0;
        var magnetCount = magnet?.Level ?? 0;
        var reflectCount = reflect?.Level ?? 0;

        importantChecks.Add(fire = new Magic(memory, save + 0x3594, addressOffset, "Fire"));
        importantChecks.Add(blizzard = new Magic(memory, save + 0x3595, addressOffset, "Blizzard"));
        importantChecks.Add(thunder = new Magic(memory, save + 0x3596, addressOffset, "Thunder"));
        importantChecks.Add(cure = new Magic(memory, save + 0x3597, addressOffset, "Cure"));
        importantChecks.Add(magnet = new Magic(memory, save + 0x35CF, addressOffset, "Magnet"));
        importantChecks.Add(reflect = new Magic(memory, save + 0x35D0, addressOffset, "Reflect"));

        fire.Level = fireCount;
        blizzard.Level = blizzardCount;
        thunder.Level = thunderCount;
        cure.Level = cureCount;
        magnet.Level = magnetCount;
        reflect.Level = reflectCount;

        importantChecks.Add(
            reportItem = new Report(memory, save + 0x36C4, addressOffset, 6, "Report1")
        );
        importantChecks.Add(
            reportItem = new Report(memory, save + 0x36C4, addressOffset, 7, "Report2")
        );
        importantChecks.Add(
            reportItem = new Report(memory, save + 0x36C5, addressOffset, 0, "Report3")
        );
        importantChecks.Add(
            reportItem = new Report(memory, save + 0x36C5, addressOffset, 1, "Report4")
        );
        importantChecks.Add(
            reportItem = new Report(memory, save + 0x36C5, addressOffset, 2, "Report5")
        );
        importantChecks.Add(
            reportItem = new Report(memory, save + 0x36C5, addressOffset, 3, "Report6")
        );
        importantChecks.Add(
            reportItem = new Report(memory, save + 0x36C5, addressOffset, 4, "Report7")
        );
        importantChecks.Add(
            reportItem = new Report(memory, save + 0x36C5, addressOffset, 5, "Report8")
        );
        importantChecks.Add(
            reportItem = new Report(memory, save + 0x36C5, addressOffset, 6, "Report9")
        );
        importantChecks.Add(
            reportItem = new Report(memory, save + 0x36C5, addressOffset, 7, "Report10")
        );
        importantChecks.Add(
            reportItem = new Report(memory, save + 0x36C6, addressOffset, 0, "Report11")
        );
        importantChecks.Add(
            reportItem = new Report(memory, save + 0x36C6, addressOffset, 1, "Report12")
        );
        importantChecks.Add(
            reportItem = new Report(memory, save + 0x36C6, addressOffset, 2, "Report13")
        );

        importantChecks.Add(
            charmItem = new Summon(memory, save + 0x36C0, addressOffset, 3, "Baseball")
        );
        importantChecks.Add(
            charmItem = new Summon(memory, save + 0x36C0, addressOffset, 0, "Ukulele")
        );
        importantChecks.Add(
            charmItem = new Summon(memory, save + 0x36C4, addressOffset, 4, "Lamp")
        );
        importantChecks.Add(
            charmItem = new Summon(memory, save + 0x36C4, addressOffset, 5, "Feather")
        );

        var visitItemAuronWepCount = visitItemAuronWep?.Quantity ?? 0;
        var visitItemMulanWepCount = visitItemMulanWep?.Quantity ?? 0;
        var visitItemBeastWepCount = visitItemBeastWep?.Quantity ?? 0;
        var visitItemJackWepCount = visitItemJackWep?.Quantity ?? 0;
        var visitItemSimbaWepCount = visitItemSimbaWep?.Quantity ?? 0;
        var visitItemSparrowWepCount = visitItemSparrowWep?.Quantity ?? 0;
        var visitItemAladdinWepCount = visitItemAladdinWep?.Quantity ?? 0;
        var visitItemTronWepCount = visitItemTronWep?.Quantity ?? 0;
        var visitItemMembershipCarpCount = visitItemMembershipCard?.Quantity ?? 0;
        var visitItemIceCreapCount = visitItemIceCream?.Quantity ?? 0;
        var visitItemNaminesSketchepCount = visitItemNaminesSketches?.Quantity ?? 0;
        var visitItemDisneyCastleKepCount = visitItemDisneyCastleKey?.Quantity ?? 0;
        var visitItemWayToTheDawpCount = visitItemWayToTheDawn?.Quantity ?? 0;

        importantChecks.Add(
            visitItemAuronWep = new Visit(memory, save + 0x35AE, addressOffset, "AuronWep")
        );
        importantChecks.Add(
            visitItemMulanWep = new Visit(memory, save + 0x35AF, addressOffset, "MulanWep")
        );
        importantChecks.Add(
            visitItemBeastWep = new Visit(memory, save + 0x35B3, addressOffset, "BeastWep")
        );
        importantChecks.Add(
            visitItemJackWep = new Visit(memory, save + 0x35B4, addressOffset, "JackWep")
        );
        importantChecks.Add(
            visitItemSimbaWep = new Visit(memory, save + 0x35B5, addressOffset, "SimbaWep")
        );
        importantChecks.Add(
            visitItemSparrowWep = new Visit(memory, save + 0x35B6, addressOffset, "SparrowWep")
        );
        importantChecks.Add(
            visitItemAladdinWep = new Visit(memory, save + 0x35C0, addressOffset, "AladdinWep")
        );
        importantChecks.Add(
            visitItemTronWep = new Visit(memory, save + 0x35C2, addressOffset, "TronWep")
        );
        importantChecks.Add(
            visitItemMembershipCard = new Visit(
                memory,
                save + 0x3643,
                addressOffset,
                "MembershipCard"
            )
        );
        importantChecks.Add(
            visitItemIceCream = new Visit(memory, save + 0x3649, addressOffset, "IceCream")
        );
        importantChecks.Add(
            visitItemNaminesSketches = new Visit(
                memory,
                save + 0x364A,
                addressOffset,
                "NaminesSketches"
            )
        );
        importantChecks.Add(
            visitItemDisneyCastleKey = new Visit(
                memory,
                save + 0x365D,
                addressOffset,
                "DisneyCastleKey"
            )
        );
        importantChecks.Add(
            visitItemWayToTheDawn = new VisitWayToTheDawn(memory, save, addressOffset)
        );

        visitItemAuronWep.Quantity = visitItemAuronWepCount;
        visitItemMulanWep.Quantity = visitItemMulanWepCount;
        visitItemBeastWep.Quantity = visitItemBeastWepCount;
        visitItemJackWep.Quantity = visitItemJackWepCount;
        visitItemSimbaWep.Quantity = visitItemSimbaWepCount;
        visitItemSparrowWep.Quantity = visitItemSparrowWepCount;
        visitItemAladdinWep.Quantity = visitItemAladdinWepCount;
        visitItemTronWep.Quantity = visitItemTronWepCount;
        visitItemMembershipCard.Quantity = visitItemMembershipCarpCount;
        visitItemIceCream.Quantity = visitItemIceCreapCount;
        visitItemNaminesSketches.Quantity = visitItemNaminesSketchepCount;
        visitItemDisneyCastleKey.Quantity = visitItemDisneyCastleKepCount;
        visitItemWayToTheDawn.Quantity = visitItemWayToTheDawpCount;

        importantChecks.Add(
            extraItem = new Extra(memory, save + 0x3696, addressOffset, "HadesCup")
        );
        importantChecks.Add(
            extraItem = new Extra(memory, save + 0x3644, addressOffset, "OlympusStone")
        );
        importantChecks.Add(
            extraItem = new Extra(memory, save + 0x365F, addressOffset, "UnknownDisk")
        );
        importantChecks.Add(
            extraItem = new Extra(memory, save + 0x363C, addressOffset, "MunnyPouch1")
        );
        importantChecks.Add(
            extraItem = new Extra(memory, save + 0x3695, addressOffset, "MunnyPouch2")
        );

        var count = pages?.Quantity ?? 0;
        importantChecks.Add(pages = new TornPage(memory, save + 0x3598, addressOffset, "TornPage"));
        pages.Quantity = count;

        count = luckyEmblems?.Quantity ?? 0;
        importantChecks.Add(
            luckyEmblems = new LuckyEmblem(memory, save + 0x3641, addressOffset, "LuckyEmblem")
        );
        luckyEmblems.Quantity = count;

        #endregion

        world = new World(memory, addressOffset, now, btlEnd + 0x820, save + 0x1CFF);

        stats = new Stats(
            memory,
            addressOffset,
            save + 0x24FE,
            slot1 + 0x188,
            save + 0x3524,
            save + 0x3700,
            nextSlot
        );
        rewards = new Rewards(memory, addressOffset, bt10);

        if (!Data.AltFinalTracking)
            checkEveryCheck = new CheckEveryCheck(
                memory,
                addressOffset,
                save,
                sys3,
                bt10,
                world,
                stats,
                rewards,
                valor,
                wisdom,
                limit,
                master,
                final
            );

        // set stat info visibiliy
        Level.Visibility = Visibility.Visible;
        Strength.Visibility = Visibility.Visible;
        Visibility = Visibility.Visible;
        Defense.Visibility = Visibility.Visible;

        if (FormsGrowthOption.IsChecked)
            FormRow.Height = new GridLength(0.5, GridUnitType.Star);

        //levelcheck visibility
        NextLevelDisplay();
        DeathCounterDisplay();
        SetBindings();
        SetTimer();
        //OnTimedEvent(null, null);
    }

    ///
    /// Autotracking general
    ///

    private void SetTimer()
    {
        _aTimer?.Stop();
        _aTimer = new DispatcherTimer();
        _aTimer.Tick += OnTimedEvent;
        _aTimer.Interval = new TimeSpan(0, 0, 0, 0, 200);
        _aTimer.Start();

        Data.WasTracking = true;
    }

    private void OnTimedEvent(object sender, EventArgs e)
    {
        previousChecks.Clear();
        previousChecks.AddRange(newChecks);
        newChecks.Clear();
        var correctSlot = 0;

        try
        {
            //current world
            world.UpdateMemory();

            //test displaying sora's correct stats for PR 1st forsed fight
            if (world.WorldNum == 16 && world.RoomNumber == 1 && world.EventId1 is 0x33 or 0x34)
                correctSlot = 2; //move forward this number of slots

            //updates
            stats.UpdateMemory(correctSlot);
            HighlightWorld();
            UpdateStatValues();
            UpdateWorldProgress(false, null);
            UpdateFormProgression();
            DeathCheck();

            importantChecks.ForEach(
                delegate(ImportantCheck importantCheck)
                {
                    importantCheck.UpdateMemory();
                }
            );

            #region For Debugging
            ////Modified to only update if any of these actually change instead of updating every tick
            //temp[0] = world.roomNumber;
            //temp[1] = world.worldNum;
            //temp[2] = world.eventID1;
            //temp[3] = world.eventID2;
            //temp[4] = world.eventID3;
            //temp[5] = world.eventComplete;
            //temp[6] = world.cupRound;
            //if (!Enumerable.SequenceEqual(temp, tempPre))
            //{
            //    Console.WriteLine("world num = " + world.worldNum);
            //    Console.WriteLine("room num  = " + world.roomNumber);
            //    Console.WriteLine("event id1 = " + world.eventID1);
            //    Console.WriteLine("event id2 = " + world.eventID2);
            //    Console.WriteLine("event id3 = " + world.eventID3);
            //    Console.WriteLine("event cpl = " + world.eventComplete);
            //    Console.WriteLine("Cup Round = " + world.cupRound);
            //    Console.WriteLine("===========================");
            //    tempPre[0] = temp[0];
            //    tempPre[1] = temp[1];
            //    tempPre[2] = temp[2];
            //    tempPre[3] = temp[3];
            //    tempPre[4] = temp[4];
            //    tempPre[5] = temp[5];
            //    tempPre[6] = temp[6];
            //}

            //string cntrl = BytesToHex(memory.ReadMemory(0x2A148E8, 1)); //sora controlable
            //Console.WriteLine(cntrl);

            //string tester = BytesToHex(memory.ReadMemory(0x2A22BC0, 4));
            //Console.WriteLine(tester);

            //int testint = BitConverter.ToInt32(memory.ReadMemory(0x2A22BC0, 4), 0);
            //Console.WriteLine(testint);
            //Console.WriteLine(testint+0x2A22BC0+0x10);
            #endregion
        }
        catch
        {
            _aTimer.Stop();
            pcFilesLoaded = false;

            if (AutoConnectOption.IsChecked)
            {
                InitTracker();
            }
            else
            {
                Connect.Visibility = Visibility.Collapsed;
                Connect2.Visibility = Visibility.Visible;
                Connect2.Source = Data.AdCross;
                if (Disconnect.IsChecked)
                {
                    MessageBox.Show("KH2FM has exited. Stopping Auto Tracker.");
                }
                Data.UsedHotkey = false;
            }

            if (AutoSaveProgress2Option.IsChecked)
            {
                if (!Directory.Exists("KH2ArchipelagoTrackerAutoSaves"))
                {
                    Directory.CreateDirectory("KH2ArchipelagoTrackerAutoSaves\\");
                }
                Save(
                    "KH2ArchipelagoTrackerAutoSaves\\"
                        + "ConnectionLost-Backup_"
                        + DateTime.Now.ToString("yy-MM-dd_H-m")
                        + ".tsv"
                );
            }

            return;
        }

        UpdateCollectedItems();
        DetermineItemLocations();
    }

    private bool CheckSynthPuzzle()
    {
        var jounal = BytesToHex(memory.ReadMemory(0x741230, 2)); //in journal
        //reminder: FF = none | 01 = save menu | 03 = load menu | 05 = moogle | 07 = item popup | 08 = pause menu (cutscene/fight) | 0A = pause Menu (normal)
        var menu = BytesToHex(memory.ReadMemory(0x741320, 2)); //in a menu

        return (jounal == "FFFF" && menu == "0500") || (jounal != "FFFF" && menu == "0A00"); // in moogle shop / in puzzle menu
    }

    //private bool CheckTornPage(Item item)
    //{
    //    //return true and track item for anything that isn't a torn page
    //    if (!item.Name.StartsWith("TornPage"))
    //        return true;
    //
    //    int Tracked = WorldGrid.Real_Pages; //current number of pages tracked to any of the world grids
    //    int Inventory = memory.ReadMemory(ADDRESS_OFFSET + 0x09A70B0 + 0x3598, 1)[0]; //number of pages currently in sora's inventory
    //    int Used = 0; //number of torn pages used so far in 100 AW
    //
    //    //don't try tracking a torn page if we already tracked 5
    //    //as there should only ever be 5 total under normal means.
    //    if(Tracked >= 5)
    //        return false;
    //
    //    //note: Save = 0x09A70B0;
    //    //check current 100 AW story flags to see what pages have been used already.
    //    if (new BitArray(memory.ReadMemory(ADDRESS_OFFSET + 0x09A70B0 + 0x1DB1, 1))[1]) //page 1 used flag
    //        Used = 1;
    //    if (new BitArray(memory.ReadMemory(ADDRESS_OFFSET + 0x09A70B0 + 0x1DB1, 1))[1]) //page 2 used flag
    //        Used = 2;
    //    if (new BitArray(memory.ReadMemory(ADDRESS_OFFSET + 0x09A70B0 + 0x1DB1, 1))[1]) //page 3 used flag
    //        Used = 3;
    //    if (new BitArray(memory.ReadMemory(ADDRESS_OFFSET + 0x09A70B0 + 0x1DB1, 1))[1]) //page 4 used flag
    //        Used = 4;
    //
    //    //if number of torn pages used + current number of pages in sora's inventory
    //    //are equal to the current number of pages tracked, then don't track anything.
    //    if (Used + Inventory == Tracked)
    //        return false;
    //
    //    return true;
    //}

    private void DeathCheck()
    {
        //Note: 04 = dying, 05 = continue screen.
        //note: if i try tracking a death when pausecheck is "0400" then that should give a
        //more accurate death count in the event that continue is selected too fast (i hope)

        var pauseCheck = BytesToHex(memory.ReadMemory(0xAB9078, 2));

        //if oncontinue is true then we want to check if the values for sora is currently dying or on continue screen.
        //we need to chck this to prevent the counter rapidly counting up every frame adnd such
        if (onContinue)
        {
            if (pauseCheck is "0400" or "0500")
                return;
            else
                onContinue = false;
        }

        // if sora is currently dying or on the continue screen
        // then increase death count and set oncontinue
        if (pauseCheck is "0400" or "0500")
        {
            deathCounter++;
            onContinue = true;
        }

        DeathValue.Text = deathCounter.ToString();
    }

    private void UpdateStatValues()
    {
        // we don't need bindings anymore (i think) so use this instead

        //Main window
        //Stats
        stats.SetNextLevelCheck(stats.Level);
        LevelValue.Text = stats.Level.ToString();
        StrengthValue.Text = stats.Strength.ToString();
        MagicValue.Text = stats.Magic.ToString();
        DefenseValue.Text = stats.Defense.ToString();
        LuckyEmblemsValue.Text = luckyEmblems.Quantity.ToString("00");
        //forms
        ValorLevel.Text = valor.VisualLevel.ToString();
        WisdomLevel.Text = wisdom.VisualLevel.ToString();
        LimitLevel.Text = limit.VisualLevel.ToString();
        MasterLevel.Text = master.VisualLevel.ToString();
        FinalLevel.Text = final.VisualLevel.ToString();
        //growth
        HighJumpLevel.Text = highJump.Level.ToString();
        QuickRunLevel.Text = quickRun.Level.ToString();
        DodgeRollLevel.Text = dodgeRoll.Level.ToString();
        AerialDodgeLevel.Text = aerialDodge.Level.ToString();
        GlideLevel.Text = glide.Level.ToString();
    }

    private void TrackItem(string itemName, WorldGrid worldGrid)
    {
        Grid itemRow;
        try //try getting itemrow grid from dictionary
        {
            itemRow = Data.Items[itemName].Item2;
        }
        catch //if item is not from pool (growth) then log the item and return
        {
            App.Logger?.Record(itemName + " tracked");
            return;
        }

        //do a check in the report handler to actually make sure reports don't
        //track to the wrong place in the case of mismatched seeds/hints
        if (itemRow.FindName(itemName) is Item { IsVisible: true } item)
        {
            worldGrid.Add_Item(item);
            App.Logger?.Record(item.Name + " tracked");
        }
    }

    private void TrackQuantities()
    {
        while (fire.Level > fireLevel)
        {
            ++fireLevel;
            var magic = new Magic(null, 0, 0, "Fire" + fireLevel);
            newChecks.Add(magic);
            collectedChecks.Add(magic);
        }
        while (blizzard.Level > blizzardLevel)
        {
            ++blizzardLevel;
            var magic = new Magic(null, 0, 0, "Blizzard" + blizzardLevel);
            newChecks.Add(magic);
            collectedChecks.Add(magic);
        }
        while (thunder.Level > thunderLevel)
        {
            ++thunderLevel;
            var magic = new Magic(null, 0, 0, "Thunder" + thunderLevel);
            newChecks.Add(magic);
            collectedChecks.Add(magic);
        }
        while (cure.Level > cureLevel)
        {
            ++cureLevel;
            var magic = new Magic(null, 0, 0, "Cure" + cureLevel);
            newChecks.Add(magic);
            collectedChecks.Add(magic);
        }
        while (reflect.Level > reflectLevel)
        {
            ++reflectLevel;
            var magic = new Magic(null, 0, 0, "Reflect" + reflectLevel);
            newChecks.Add(magic);
            collectedChecks.Add(magic);
        }
        while (magnet.Level > magnetLevel)
        {
            ++magnetLevel;
            var magic = new Magic(null, 0, 0, "Magnet" + magnetLevel);
            newChecks.Add(magic);
            collectedChecks.Add(magic);
        }
        while (pages.Quantity > tornPageCount)
        {
            ++tornPageCount;
            var page = new TornPage(null, 0, 0, "TornPage" + tornPageCount);
            newChecks.Add(page);
            collectedChecks.Add(page);
        }

        while (visitItemAuronWep.Quantity > visitItemAuronWepQuantity)
        {
            ++visitItemAuronWepQuantity;
            var visitItem = new Visit(null, 0, 0, "AuronWep" + visitItemAuronWepQuantity);
            newChecks.Add(visitItem);
            collectedChecks.Add(visitItem);
        }
        while (visitItemMulanWep.Quantity > visitItemMulanWepQuantity)
        {
            ++visitItemMulanWepQuantity;
            var visitItem = new Visit(null, 0, 0, "MulanWep" + visitItemMulanWepQuantity);
            newChecks.Add(visitItem);
            collectedChecks.Add(visitItem);
        }
        while (visitItemBeastWep.Quantity > visitItemBeastWepQuantity)
        {
            ++visitItemBeastWepQuantity;
            var visitItem = new Visit(null, 0, 0, "BeastWep" + visitItemBeastWepQuantity);
            newChecks.Add(visitItem);
            collectedChecks.Add(visitItem);
        }
        while (visitItemJackWep.Quantity > visitItemJackWepQuantity)
        {
            ++visitItemJackWepQuantity;
            var visitItem = new Visit(null, 0, 0, "JackWep" + visitItemJackWepQuantity);
            newChecks.Add(visitItem);
            collectedChecks.Add(visitItem);
        }
        while (visitItemSimbaWep.Quantity > visitItemSimbaWepQuantity)
        {
            ++visitItemSimbaWepQuantity;
            var visitItem = new Visit(null, 0, 0, "SimbaWep" + visitItemSimbaWepQuantity);
            newChecks.Add(visitItem);
            collectedChecks.Add(visitItem);
        }
        while (visitItemSparrowWep.Quantity > visitItemSparrowWepQuantity)
        {
            ++visitItemSparrowWepQuantity;
            var visitItem = new Visit(null, 0, 0, "SparrowWep" + visitItemSparrowWepQuantity);
            newChecks.Add(visitItem);
            collectedChecks.Add(visitItem);
        }
        while (visitItemAladdinWep.Quantity > visitItemAladdinWepQuantity)
        {
            ++visitItemAladdinWepQuantity;
            var visitItem = new Visit(null, 0, 0, "AladdinWep" + visitItemAladdinWepQuantity);
            newChecks.Add(visitItem);
            collectedChecks.Add(visitItem);
        }
        while (visitItemTronWep.Quantity > visitItemTronWepQuantity)
        {
            ++visitItemTronWepQuantity;
            var visitItem = new Visit(null, 0, 0, "TronWep" + visitItemTronWepQuantity);
            newChecks.Add(visitItem);
            collectedChecks.Add(visitItem);
        }
        while (visitItemMembershipCard.Quantity > visitItemMembershipCardQuantity)
        {
            ++visitItemMembershipCardQuantity;
            var visitItem = new Visit(
                null,
                0,
                0,
                "MembershipCard" + visitItemMembershipCardQuantity
            );
            newChecks.Add(visitItem);
            collectedChecks.Add(visitItem);
        }
        while (visitItemIceCream.Quantity > visitItemIceCreamQuantity)
        {
            ++visitItemIceCreamQuantity;
            var visitItem = new Visit(null, 0, 0, "IceCream" + visitItemIceCreamQuantity);
            newChecks.Add(visitItem);
            collectedChecks.Add(visitItem);
        }
        while (visitItemDisneyCastleKey.Quantity > visitItemDisneyCastleKeyQuantity)
        {
            ++visitItemDisneyCastleKeyQuantity;
            var visitItem = new Visit(
                null,
                0,
                0,
                "DisneyCastleKey" + visitItemDisneyCastleKeyQuantity
            );
            newChecks.Add(visitItem);
            collectedChecks.Add(visitItem);
        }
    }

    private void UpdateWorldProgress(
        bool usingSave,
        Tuple<string, int, int, int, int, int> saveTuple
    )
    {
        string wName;
        int wRoom;
        int wId1;
        int wId2;
        int wId3;
        int wCom;
        if (!usingSave)
        {
            wName = world.WorldName;
            wRoom = world.RoomNumber;
            wId1 = world.EventId1;
            wId2 = world.EventId2;
            wId3 = world.EventId3;
            wCom = world.EventComplete;
        }
        else
        {
            wName = saveTuple.Item1;
            wRoom = saveTuple.Item2;
            wId1 = saveTuple.Item3;
            wId2 = saveTuple.Item4;
            wId3 = saveTuple.Item5;
            wCom = 1;
        }

        if (wName is "DestinyIsland" or "Unknown")
            return;

        //check event
        var eventTuple = new Tuple<string, int, int, int, int, int>(
            wName,
            wRoom,
            wId1,
            wId2,
            wId3,
            0
        );
        if (Data.EventLog.Contains(eventTuple))
            return;

        //check for valid progression Content Controls first
        var progressionM = Data.WorldsData[wName].Progression;

        //Get current icon prefixes (simple, game, or custom icons)
        var oldToggled = App.Settings.OldProg;
        var customToggled = App.Settings.CustomIcons;
        var prog = "Min-"; //Default
        if (oldToggled)
            prog = "Old-";
        if (customProgFound && customToggled)
            prog = "Cus-";

        //progression defaults
        var curProg = Data.WorldsData[wName].Progress; //current world progress int
        var newProg = 99;
        var updateProgression = true;

        //get current world's new progress key
        switch (wName)
        {
            case "SimulatedTwilightTown":
                switch (wRoom) //check based on room number now, then based on events in each room
                {
                    case 1:
                        if (wId3 is 56 or 55 && curProg == 0) // Roxas' Room (Day 1)/(Day 6)
                            newProg = 1;
                        break;
                    case 8:
                        if (wId1 is 110 or 111) // Get Ollete Munny Pouch (min/max munny cutscenes)
                            newProg = 2;
                        break;
                    case 34:
                        if (wId1 == 157 && wCom == 1) // Twilight Thorn finish
                            newProg = 3;
                        break;
                    case 5:
                        newProg = wId1 switch
                        {
                            // Axel 1 Finish
                            87 when wCom == 1 => 4,
                            // Setzer finish
                            88 when wCom == 1 => 5,
                            _ => newProg
                        };
                        break;
                    case 21:
                        if (wId3 == 1) // Mansion: Computer Room
                            newProg = 6;
                        break;
                    case 20:
                        if (wId1 == 137 && wCom == 1) // Axel 2 finish
                            newProg = 7;
                        break;
                    default: //if not in any of the above rooms then just leave
                        updateProgression = false;
                        break;
                }
                break;
            case "TwilightTown":
                switch (wRoom)
                {
                    case 9:
                        if (wId3 == 117 && curProg == 0) // Roxas' Room (Day 1)
                            newProg = 1;
                        break;
                    case 8:
                        if (wId3 == 108 && wCom == 1) // Station Nobodies
                            newProg = 2;
                        break;
                    case 27:
                        if (wId3 == 4) // Yen Sid after new clothes
                            newProg = 3;
                        break;
                    case 4:
                        if (wId1 == 80 && wCom == 1) // Sandlot finish
                            newProg = 4;
                        break;
                    case 41:
                        if (wId1 == 186 && wCom == 1) // Mansion fight finish
                            newProg = 5;
                        break;
                    case 40:
                        if (wId1 == 161 && wCom == 1) // Betwixt and Between finish
                            newProg = 6;
                        break;
                    case 20:
                        if (wId1 == 213 && wCom == 1) // Data Axel finish
                            newProg = 7;
                        break;
                    default:
                        updateProgression = false;
                        break;
                }
                break;
            case "HollowBastion":
                switch (wRoom)
                {
                    case 0:
                    case 10:
                        if (wId3 is 1 or 2 && curProg == 0) // Villain's Vale (HB1)
                            newProg = 1;
                        break;
                    case 8:
                        if (wId1 == 52 && wCom == 1) // Bailey finish
                            newProg = 2;
                        break;
                    case 5:
                        if (wId3 == 20) // Ansem Study post Computer
                            newProg = 3;
                        break;
                    case 20:
                        if (wId1 == 86 && wCom == 1) // Corridor finish
                            newProg = 4;
                        break;
                    case 18:
                        if (wId1 == 73 && wCom == 1) // Dancers finish
                            newProg = 5;
                        break;
                    case 4:
                        switch (wId1)
                        {
                            // HB Demyx finish
                            case 55 when wCom == 1:
                                newProg = 6;
                                break;

                            // Data Demyx finish
                            case 114 when wCom == 1:
                            {
                                if (curProg == 9) //sephi finished
                                    newProg = 11; //data demyx + sephi finished
                                else if (curProg != 11) //just demyx
                                    newProg = 10;
                                break;
                            }
                        }
                        break;
                    case 16:
                        if (wId1 == 65 && wCom == 1) // FF Cloud finish
                            newProg = 7;
                        break;
                    case 17:
                        if (wId1 == 66 && wCom == 1) // 1k Heartless finish
                            newProg = 8;
                        break;
                    case 1:
                        if (wId1 == 75 && wCom == 1) // Sephiroth finish
                        {
                            if (curProg == 10) //demyx finish
                                newProg = 11; //data demyx + sephi finished
                            else if (curProg != 11) //just sephi
                                newProg = 9;
                        }
                        break;
                    //CoR
                    case 21:
                        if (wId3 is 1 or 2 && Data.WorldsData["GoA"].Progress == 0) //Enter CoR
                        {
                            GoAProgression.SetResourceReference(
                                ContentProperty,
                                prog + Data.ProgressKeys["GoA"][1]
                            );
                            Data.WorldsData["GoA"].Progress = 1;
                            Data.WorldsData["GoA"].Progression.ToolTip = Data.ProgressKeys[
                                "GoADesc"
                            ][1];
                            Data.EventLog.Add(eventTuple);
                            return;
                        }
                        break;
                    case 22:
                        if (wId3 == 1 && Data.WorldsData["GoA"].Progress <= 1 && wCom == 1) //valves after skip
                        {
                            GoAProgression.SetResourceReference(
                                ContentProperty,
                                prog + Data.ProgressKeys["GoA"][5]
                            );
                            Data.WorldsData["GoA"].Progress = 5;
                            Data.WorldsData["GoA"].Progression.ToolTip = Data.ProgressKeys[
                                "GoADesc"
                            ][5];
                            Data.EventLog.Add(eventTuple);
                            return;
                        }
                        break;
                    case 24:
                        switch (wId3)
                        {
                            //first fight
                            case 1 when wCom == 1:
                                GoAProgression.SetResourceReference(
                                    ContentProperty,
                                    prog + Data.ProgressKeys["GoA"][2]
                                );
                                Data.WorldsData["GoA"].Progress = 2;
                                Data.WorldsData["GoA"].Progression.ToolTip = Data.ProgressKeys[
                                    "GoADesc"
                                ][2];
                                Data.EventLog.Add(eventTuple);
                                return;

                            //second fight
                            case 2 when wCom == 1:
                                GoAProgression.SetResourceReference(
                                    ContentProperty,
                                    prog + Data.ProgressKeys["GoA"][3]
                                );
                                Data.WorldsData["GoA"].Progress = 3;
                                Data.WorldsData["GoA"].Progression.ToolTip = Data.ProgressKeys[
                                    "GoADesc"
                                ][3];
                                Data.EventLog.Add(eventTuple);
                                return;
                        }

                        break;
                    case 25:
                        if (wId3 == 3 && wCom == 1) //transport
                        {
                            GoAProgression.SetResourceReference(
                                ContentProperty,
                                prog + Data.ProgressKeys["GoA"][4]
                            );
                            Data.WorldsData["GoA"].Progress = 4;
                            Data.WorldsData["GoA"].Progression.ToolTip = Data.ProgressKeys[
                                "GoADesc"
                            ][4];
                            Data.EventLog.Add(eventTuple);
                            return;
                        }
                        break;
                    default:
                        updateProgression = false;
                        break;
                }
                break;
            case "BeastsCastle":
                switch (wRoom)
                {
                    case 0:
                    case 2:
                        if (wId3 is 1 or 10 && curProg == 0) // Entrance Hall (BC1)
                            newProg = 1;
                        break;
                    case 11:
                        if (wId1 == 72 && wCom == 1) // Thresholder finish
                            newProg = 2;
                        break;
                    case 3:
                        if (wId1 == 69 && wCom == 1) // Beast finish
                            newProg = 3;
                        break;
                    case 5:
                        if (wId1 == 79 && wCom == 1) // Dark Thorn finish
                            newProg = 4;
                        break;
                    case 4:
                        if (wId1 == 74 && wCom == 1) // Dragoons finish
                            newProg = 5;
                        break;
                    case 15:
                        newProg = wId1 switch
                        {
                            // Xaldin finish
                            82 when wCom == 1 => 6,
                            // Data Xaldin finish
                            97 when wCom == 1 => 7,
                            _ => newProg
                        };
                        break;
                    default:
                        updateProgression = false;
                        break;
                }
                break;
            case "OlympusColiseum":
                switch (wRoom)
                {
                    case 3:
                        if (wId3 is 1 or 12 && curProg == 0) // The Coliseum (OC1) | Underworld Entrance (OC2)
                            newProg = 1;
                        break;
                    case 7:
                        if (wId1 == 114 && wCom == 1) // Cerberus finish
                            newProg = 2;
                        break;
                    case 0:
                        if (wId3 is 1 or 12 && curProg == 0) // (reverse rando)
                            newProg = 1;
                        if (wId1 == 141 && wCom == 1) // Urns finish
                            newProg = 3;
                        break;
                    case 17:
                        if (wId1 == 123 && wCom == 1) // OC Demyx finish
                            newProg = 4;
                        break;
                    case 8:
                        if (wId1 == 116 && wCom == 1) // OC Pete finish
                            newProg = 5;
                        break;
                    case 18:
                        if (wId1 == 171 && wCom == 1) // Hydra finish
                            newProg = 6;
                        break;
                    case 6:
                        if (wId1 == 126 && wCom == 1) // Auron Statue fight finish
                            newProg = 7;
                        break;
                    case 19:
                        if (wId1 == 202 && wCom == 1) // Hades finish
                            newProg = 8;
                        break;
                    case 34:
                        newProg = wId1 switch
                        {
                            // AS Zexion finish
                            151 when wCom == 1 => 9,
                            // Data Zexion finish
                            152 when wCom == 1 => 10,
                            _ => newProg
                        };
                        //else if ((wID1 == 152) && wCom == 1) // Data Zexion finish
                        //{
                        //    if (data.UsingProgressionHints)
                        //        UpdateProgressionPoints(wName, 10);
                        //    data.eventLog.Add(eventTuple);
                        //    return;
                        //}
                        break;
                    default:
                        updateProgression = false;
                        break;
                }
                break;
            case "Agrabah":
                switch (wRoom)
                {
                    case 0:
                    case 4:
                        if (wId3 is 1 or 10 && curProg == 0) // Agrabah (Ag1) || The Vault (Ag2)
                            newProg = 1;
                        break;
                    case 9:
                        if (wId1 == 2 && wCom == 1) // Abu finish
                            newProg = 2;
                        break;
                    case 13:
                        if (wId1 == 79 && wCom == 1) // Chasm fight finish
                            newProg = 3;
                        break;
                    case 10:
                        if (wId1 == 58 && wCom == 1) // Treasure Room finish
                            newProg = 4;
                        break;
                    case 3:
                        if (wId1 == 59 && wCom == 1) // Lords finish
                            newProg = 5;
                        break;
                    case 14:
                        if (wId1 == 101 && wCom == 1) // Carpet finish
                            newProg = 6;
                        break;
                    case 5:
                        if (wId1 == 62 && wCom == 1) // Genie Jafar finish
                            newProg = 7;
                        break;
                    case 33:
                        newProg = wId1 switch
                        {
                            // AS Lexaeus finish
                            142 when wCom == 1 => 8,
                            // Data Lexaeus finish
                            147 when wCom == 1 => 9,
                            _ => newProg
                        };
                        //else if ((wID1 == 147) && wCom == 1) // Data Lexaeus
                        //{
                        //    if (data.UsingProgressionHints)
                        //        UpdateProgressionPoints(wName, 9);
                        //    data.eventLog.Add(eventTuple);
                        //    return;
                        //}
                        break;
                    default:
                        updateProgression = false;
                        break;
                }
                break;
            case "LandofDragons":
                switch (wRoom)
                {
                    case 0:
                    case 12:
                        if (wId3 is 1 or 10 && curProg == 0) // Bamboo Grove (LoD1)
                            newProg = 1;
                        break;
                    case 1:
                        if (wId1 == 70 && wCom == 1) // Mission 3 (Search) finish
                            newProg = 2;
                        break;
                    case 3:
                        if (wId1 == 71 && wCom == 1) // Mountain Climb finish
                            newProg = 3;
                        break;
                    case 5:
                        if (wId1 == 72 && wCom == 1) // Cave finish
                            newProg = 4;
                        break;
                    case 7:
                        if (wId1 == 73 && wCom == 1) // Summit finish
                            newProg = 5;
                        break;
                    case 9:
                        if (wId1 == 75 && wCom == 1) // Shan Yu finish
                            newProg = 6;
                        break;
                    case 10:
                        if (wId1 == 78 && wCom == 1) // Antechamber fight finish
                            newProg = 7;
                        break;
                    case 8:
                        if (wId1 == 79 && wCom == 1) // Storm Rider finish
                            newProg = 8;
                        break;
                    default:
                        updateProgression = false;
                        break;
                }
                break;
            case "HundredAcreWood":
                switch (wRoom)
                {
                    case 2:
                        if (wId3 is 1 or 21 or 22 && curProg == 0) // Pooh's house
                            newProg = 1;
                        break;
                    case 6:
                        if (wId1 == 55 && wCom == 1) //A Blustery Rescue Complete
                            newProg = 2;
                        break;
                    case 7:
                        if (wId1 == 57 && wCom == 1) //Hunny Slider Complete
                            newProg = 3;
                        break;
                    case 8:
                        if (wId1 == 59 && wCom == 1) //Balloon Bounce Complete
                            newProg = 4;
                        break;
                    case 9:
                        if (wId1 == 61 && wCom == 1) //The Expotition Complete
                            newProg = 5;
                        break;
                    case 1:
                        if (wId1 == 52 && wCom == 1) //The Hunny Pot Complete
                            newProg = 6;
                        break;
                    default:
                        updateProgression = false;
                        break;
                }
                break;
            case "PrideLands":
                switch (wRoom)
                {
                    case 4:
                    case 16:
                        if (wId3 is 1 or 10 && curProg == 0) // Wildebeest Valley (PL1)
                            newProg = 1;
                        break;
                    case 12:
                        if (wId3 == 1) // Oasis after talking to Simba
                            newProg = 2;
                        break;
                    case 2:
                        if (wId1 == 51 && wCom == 1) // Hyenas 1 Finish
                            newProg = 3;
                        break;
                    case 14:
                        if (wId1 == 55 && wCom == 1) // Scar finish
                            newProg = 4;
                        break;
                    case 5:
                        if (wId1 == 57 && wCom == 1) // Hyenas 2 Finish
                            newProg = 5;
                        break;
                    case 15:
                        if (wId1 == 59 && wCom == 1) // Groundshaker finish
                            newProg = 6;
                        break;
                    default:
                        updateProgression = false;
                        break;
                }
                break;
            case "Atlantica":
                switch (wRoom)
                {
                    case 2:
                        if (wId1 == 63) // Tutorial
                            newProg = 1;
                        break;
                    case 9:
                        if (wId1 == 65) // Ursula's Revenge
                            newProg = 2;
                        break;
                    case 4:
                        if (wId1 == 55) // A New Day is Dawning
                            newProg = 3;
                        break;
                    default:
                        updateProgression = false;
                        break;
                }
                break;
            case "DisneyCastle":
                switch (wRoom)
                {
                    case 0:
                        if (wId3 == 22 && curProg == 0) // Cornerstone Hill (TR) (Audience Chamber has no Evt 0x16)
                            newProg = 1;
                        else if (wId1 == 51 && wCom == 1) // Minnie Escort finish
                            newProg = 2;
                        else if (wId3 == 6) // Windows popup (Audience Chamber has no Evt 0x06)
                            newProg = 4;
                        break;
                    case 1:
                        newProg = wId1 switch
                        {
                            // Library (DC)
                            53 when curProg == 0 => 1,
                            // Old Pete finish
                            58 when wCom == 1 => 3,
                            _ => newProg
                        };
                        break;
                    case 2:
                        if (wId1 == 52 && wCom == 1) // Boat Pete finish
                            newProg = 5;
                        break;
                    case 3:
                        if (wId1 == 53 && wCom == 1) // DC Pete finish
                            newProg = 6;
                        break;
                    //case 38:
                    //    if ((wID1 == 145 || wID1 == 150) && wCom == 1) // Marluxia finish
                    //    {
                    //        if (curProg == 8)
                    //            newProg = 9; //marluxia + LW finished
                    //        else if (curProg != 9)
                    //            newProg = 7;
                    //        if(data.UsingProgressionHints)
                    //        {
                    //            if (wID1 == 145)
                    //                UpdateProgressionPoints(wName, 7); // AS
                    //            else
                    //            {
                    //                UpdateProgressionPoints(wName, 8); // Data
                    //                data.eventLog.Add(eventTuple);
                    //                return;
                    //            }
                    //
                    //            updateProgressionPoints = false;
                    //        }
                    //    }
                    //    break;
                    case 38:
                    case 7:
                        switch (wId1)
                        {
                            // Marluxia finish
                            case 145
                            or 150 when wCom == 1:
                            {
                                //Marluxia
                                if (curProg != 9 && curProg != 10 && curProg != 11)
                                {
                                    newProg = wId1 switch
                                    {
                                        //check if as/data
                                        145 => 7,
                                        150 => 8,
                                        _ => newProg
                                    };
                                }
                                //check for LW
                                else if (curProg is 9 or 10)
                                {
                                    newProg = wId1 switch
                                    {
                                        //check if as/data
                                        145 => 10,
                                        150 => 11,
                                        _ => newProg
                                    };
                                }

                                break;
                            }
                            // Lingering Will finish
                            case 67 when wCom == 1:
                            {
                                //LW
                                if (curProg != 7 && curProg != 8)
                                {
                                    newProg = 9;
                                }
                                else
                                    newProg = curProg switch
                                    {
                                        //as marluxia beaten
                                        7 => 10,
                                        //data marluxia
                                        8 => 11,
                                        _ => newProg
                                    };

                                break;
                            }
                        }

                        break;
                    //if (wID1 == 67 && wCom == 1) // Lingering Will finish
                    //{
                    //    if (curProg == 7)
                    //        newProg = 9; //marluxia + LW finished
                    //    else if (curProg != 9)
                    //        newProg = 8;
                    //    if (data.UsingProgressionHints)
                    //    {
                    //        UpdateProgressionPoints(wName, 9);
                    //        updateProgressionPoints = false;
                    //    }
                    //}
                    //break;
                    default:
                        updateProgression = false;
                        break;
                }
                break;
            case "HalloweenTown":
                switch (wRoom)
                {
                    case 1:
                    case 4:
                        if (wId3 is 1 or 10 && curProg == 0) // Hinterlands (HT1)
                            newProg = 1;
                        break;
                    case 6:
                        if (wId1 == 53 && wCom == 1) // Candy Cane Lane fight finish
                            newProg = 2;
                        break;
                    case 3:
                        if (wId1 == 52 && wCom == 1) // Prison Keeper finish
                            newProg = 3;
                        break;
                    case 9:
                        if (wId1 == 55 && wCom == 1) // Oogie Boogie finish
                            newProg = 4;
                        break;
                    case 10:
                        newProg = wId1 switch
                        {
                            // Children Fight
                            62 when wCom == 1 => 5,
                            // Presents minigame
                            63 when wCom == 1 => 6,
                            _ => newProg
                        };
                        break;
                    case 7:
                        if (wId1 == 64 && wCom == 1) // Experiment finish
                            newProg = 7;
                        break;
                    case 32:
                        newProg = wId1 switch
                        {
                            // AS Vexen finish
                            115 when wCom == 1 => 8,
                            // Data Vexen finish
                            146 when wCom == 1 => 9,
                            _ => newProg
                        };
                        //else if (wID1 == 146 && wCom == 1) // Data Vexen finish
                        //{
                        //    if(data.UsingProgressionHints)
                        //        UpdateProgressionPoints(wName, 9);
                        //    data.eventLog.Add(eventTuple);
                        //    return;
                        //}
                        break;
                    default:
                        updateProgression = false;
                        break;
                }
                break;
            case "PortRoyal":
                switch (wRoom)
                {
                    case 0:
                        if (wId3 == 1 && curProg == 0) // Rampart (PR1)
                            newProg = 1;
                        break;
                    case 10:
                        if (wId3 == 10 && curProg == 0) // Treasure Heap (PR2)
                            newProg = 1;
                        if (wId1 == 60 && wCom == 1) // Barbossa finish
                            newProg = 6;
                        break;
                    case 2:
                        if (wId1 == 55 && wCom == 1) // Town finish
                            newProg = 2;
                        break;
                    case 9:
                        if (wId1 == 59 && wCom == 1) // 1min pirates finish
                            newProg = 3;
                        break;
                    case 7:
                        if (wId1 == 58 && wCom == 1) // Medalion fight finish
                            newProg = 4;
                        break;
                    case 3:
                        if (wId1 == 56 && wCom == 1) // barrels finish
                            newProg = 5;
                        break;
                    case 18:
                        if (wId1 == 85 && wCom == 1) // Grim Reaper 1 finish
                            newProg = 7;
                        break;
                    case 14:
                        if (wId1 == 62 && wCom == 1) // Gambler finish
                            newProg = 8;
                        break;
                    case 1:
                        if (wId1 == 54 && wCom == 1) // Grim Reaper 2 finish
                            newProg = 9;
                        break;
                    default:
                        updateProgression = false;
                        break;
                }
                break;
            case "SpaceParanoids":
                switch (wRoom)
                {
                    case 1:
                        if (wId3 is 1 or 10 && curProg == 0) // Canyon (SP1)
                            newProg = 1;
                        break;
                    case 3:
                        if (wId1 == 54 && wCom == 1) // Screens finish
                            newProg = 2;
                        break;
                    case 4:
                        if (wId1 == 55 && wCom == 1) // Hostile Program finish
                            newProg = 3;
                        break;
                    case 7:
                        if (wId1 == 57 && wCom == 1) // Solar Sailer finish
                            newProg = 4;
                        break;
                    case 9:
                        if (wId1 == 59 && wCom == 1) // MCP finish
                            newProg = 5;
                        break;
                    case 33:
                        newProg = wId1 switch
                        {
                            // AS Larxene finish
                            143 when wCom == 1 => 6,
                            // Data Larxene finish
                            148 when wCom == 1 => 7,
                            _ => newProg
                        };
                        //else if (wID1 == 148 && wCom == 1) // Data Larxene finish
                        //{
                        //    if (data.UsingProgressionHints)
                        //        UpdateProgressionPoints(wName, 7);
                        //    data.eventLog.Add(eventTuple);
                        //    return;
                        //}
                        break;
                    default:
                        updateProgression = false;
                        break;
                }
                break;
            case "TWTNW":
                switch (wRoom)
                {
                    case 1:
                        if (wId3 == 1) // Alley to Between
                            newProg = 1;
                        break;
                    case 21:
                        switch (wId1)
                        {
                            // Roxas finish
                            case 65 when wCom == 1:
                                newProg = 2;
                                break;
                            // Data Roxas finish
                            case 99 when wCom == 1:
                                SimulatedTwilightTownProgression.SetResourceReference(
                                    ContentProperty,
                                    prog + Data.ProgressKeys["SimulatedTwilightTown"][8]
                                );
                                Data.WorldsData["SimulatedTwilightTown"].Progress = 8;
                                Data.WorldsData["SimulatedTwilightTown"].Progression.ToolTip =
                                    Data.ProgressKeys["SimulatedTwilightTownDesc"][8];
                                Data.EventLog.Add(eventTuple);
                                return;
                        }
                        break;
                    case 10:
                        switch (wId1)
                        {
                            // Xigbar finish
                            case 57 when wCom == 1:
                                newProg = 3;
                                break;
                            // Data Xigbar finish
                            case 100 when wCom == 1:
                                LandofDragonsProgression.SetResourceReference(
                                    ContentProperty,
                                    prog + Data.ProgressKeys["LandofDragons"][9]
                                );
                                Data.WorldsData["LandofDragons"].Progress = 9;
                                Data.WorldsData["LandofDragons"].Progression.ToolTip =
                                    Data.ProgressKeys["LandofDragonsDesc"][9];
                                Data.EventLog.Add(eventTuple);
                                return;
                        }
                        break;
                    case 14:
                        switch (wId1)
                        {
                            // Luxord finish
                            case 58 when wCom == 1:
                                newProg = 4;
                                break;
                            // Data Luxord finish
                            case 101 when wCom == 1:
                                PortRoyalProgression.SetResourceReference(
                                    ContentProperty,
                                    prog + Data.ProgressKeys["PortRoyal"][10]
                                );
                                Data.WorldsData["PortRoyal"].Progress = 10;
                                Data.WorldsData["PortRoyal"].Progression.ToolTip =
                                    Data.ProgressKeys["PortRoyalDesc"][10];
                                Data.EventLog.Add(eventTuple);
                                return;
                        }
                        break;
                    case 15:
                        switch (wId1)
                        {
                            // Saix finish
                            case 56 when wCom == 1:
                                newProg = 5;
                                break;
                            // Data Saix finish
                            case 102 when wCom == 1:
                                PrideLandsProgression.SetResourceReference(
                                    ContentProperty,
                                    prog + Data.ProgressKeys["PrideLands"][7]
                                );
                                Data.WorldsData["PrideLands"].Progress = 7;
                                Data.WorldsData["PrideLands"].Progression.ToolTip =
                                    Data.ProgressKeys["PrideLandsDesc"][7];
                                Data.EventLog.Add(eventTuple);
                                return;
                        }
                        break;
                    case 19:
                        if (wId1 == 59 && wCom == 1) // Xemnas 1 finish
                            newProg = 6;
                        break;
                    case 20:
                        switch (wId1)
                        {
                            // Data Xemnas finish
                            case 98 when wCom == 1:
                                newProg = 7;
                                break;
                            // Regular Final Xemnas finish
                            case 74 when wCom == 1 && Data.RevealFinalXemnas:
                                Data.EventLog.Add(eventTuple);
                                return;
                        }
                        break;
                    default:
                        updateProgression = false;
                        break;
                }
                break;
            case "GoA":
                if (wRoom == 32)
                {
                    if (HashGrid.Visibility == Visibility.Visible)
                    {
                        HashGrid.Visibility = Visibility.Collapsed;
                    }
                }
                return;
            default: //return if any other world
                return;
        }

        //progression wasn't updated
        if (newProg == 99 || updateProgression == false)
            return;

        //made it this far, now just set the progression icon based on newProg
        progressionM.SetResourceReference(
            ContentProperty,
            prog + Data.ProgressKeys[wName][newProg]
        );
        Data.WorldsData[wName].Progress = newProg;
        Data.WorldsData[wName].Progression.ToolTip = Data.ProgressKeys[wName + "Desc"][newProg];

        //log event
        Data.EventLog.Add(eventTuple);
    }

    // Sometimes level rewards and levels dont update on the same tick
    // Previous tick checks are placed on the current tick with the info of both ticks
    // This way level checks don't get misplaced
    //Note: apparently the above is completely untrue, but its's not like it currently breaks anything so...
    private void DetermineItemLocations()
    {
        if (previousChecks.Count == 0)
            return;

        // Get rewards between previous level and current level
        var levelRewards = rewards
            .GetLevelRewards(stats.Weapon)
            .Where(reward => reward.Item1 > stats.PreviousLevels[0] && reward.Item1 <= stats.Level)
            .Select(reward => reward.Item2)
            .ToList();
        // Get drive rewards between previous level and current level
        var driveRewards = rewards.ValorChecks
            .Where(reward => reward.Item1 > valor.PreviousLevels[0] && reward.Item1 <= valor.Level)
            .Select(reward => reward.Item2)
            .ToList();
        driveRewards.AddRange(
            rewards.WisdomChecks
                .Where(
                    reward =>
                        reward.Item1 > wisdom.PreviousLevels[0] && reward.Item1 <= wisdom.Level
                )
                .Select(reward => reward.Item2)
        );
        driveRewards.AddRange(
            rewards.LimitChecks
                .Where(
                    reward => reward.Item1 > limit.PreviousLevels[0] && reward.Item1 <= limit.Level
                )
                .Select(reward => reward.Item2)
        );
        driveRewards.AddRange(
            rewards.MasterChecks
                .Where(
                    reward =>
                        reward.Item1 > master.PreviousLevels[0] && reward.Item1 <= master.Level
                )
                .Select(reward => reward.Item2)
        );
        driveRewards.AddRange(
            rewards.FinalChecks
                .Where(
                    reward => reward.Item1 > final.PreviousLevels[0] && reward.Item1 <= final.Level
                )
                .Select(reward => reward.Item2)
        );

        if (stats.Level > stats.PreviousLevels[0] && App.Logger != null)
            App.Logger.Record("Levels " + stats.PreviousLevels[0] + " to " + stats.Level);
        if (valor.Level > valor.PreviousLevels[0] && App.Logger != null)
            App.Logger.Record("Valor Levels " + valor.PreviousLevels[0] + " to " + valor.Level);
        if (wisdom.Level > wisdom.PreviousLevels[0] && App.Logger != null)
            App.Logger.Record("Wisdom Levels " + wisdom.PreviousLevels[0] + " to " + wisdom.Level);
        if (limit.Level > limit.PreviousLevels[0] && App.Logger != null)
            App.Logger.Record("Limit Levels " + limit.PreviousLevels[0] + " to " + limit.Level);
        if (master.Level > master.PreviousLevels[0] && App.Logger != null)
            App.Logger.Record("Master Levels " + master.PreviousLevels[0] + " to " + master.Level);
        if (final.Level > final.PreviousLevels[0] && App.Logger != null)
            App.Logger.Record("Final Levels " + final.PreviousLevels[0] + " to " + final.Level);

        foreach (var str in levelRewards)
        {
            if (App.Logger != null)
                App.Logger.Record("Level reward " + str);
        }
        foreach (var str in driveRewards)
        {
            if (App.Logger != null)
                App.Logger.Record("Drive reward " + str);
        }

        foreach (var check in previousChecks)
        {
            var count = "";
            // remove magic and torn page count for comparison with item codes and readd to track specific ui copies
            if (check.GetType() == typeof(Magic) || check.GetType() == typeof(TornPage))
            {
                count = check.Name[^1..];
                check.Name = check.Name[..^1];
            }

            if (levelRewards.Exists(x => x == check.Name))
            {
                // add check to levels
                TrackItem(check.Name + count, SorasHeartGrid);
                levelRewards.Remove(check.Name);
            }
            else if (driveRewards.Exists(x => x == check.Name))
            {
                // add check to drives
                TrackItem(check.Name + count, DriveFormsGrid);
                driveRewards.Remove(check.Name);
            }
            else
            {
                //check if user is currently in shop or puzzle and track item to Creations if so
                if (CheckSynthPuzzle())
                {
                    TrackItem(check.Name + count, Data.WorldsData["PuzzSynth"].WorldGrid);
                }
                else
                {
                    if (
                        world.PreviousWorldName != null
                        && Data.WorldsData.TryGetValue(
                            world.PreviousWorldName,
                            out var prevWorldData
                        )
                    )
                    {
                        // add check to current world
                        TrackItem(check.Name + count, prevWorldData.WorldGrid);
                    }
                }
            }
        }
    }

    private void UpdateCollectedItems()
    {
        foreach (var check in importantChecks)
        {
            // handle these separately due to the way they are stored in memory
            if (check.GetType() == typeof(Magic) || check.GetType() == typeof(TornPage))
                continue;

            if (!check.Obtained || collectedChecks.Contains(check))
                continue;

            switch (check.Name)
            {
                // skip auto tracking final if it was forced and valor
                case "Valor" when valor.GenieFix && !Data.AltFinalTracking:
                    valor.Obtained = false;
                    break;

                case "Final" when !Data.AltFinalTracking:
                {
                    // if forced Final, start tracking the Final Form check
                    if (!Data.ForcedFinal && stats.Form == 5)
                    {
                        Data.ForcedFinal = true;
                        checkEveryCheck.TrackCheck(0x001D);
                    }
                    // if not forced Final, track Final Form check like normal
                    // else if Final was forced, check the tracked Final Form check
                    else if (!Data.ForcedFinal || checkEveryCheck.UpdateTargetMemory())
                    {
                        collectedChecks.Add(check);
                        newChecks.Add(check);
                    }

                    break;
                }

                default:
                    collectedChecks.Add(check);
                    newChecks.Add(check);
                    break;
            }
        }
        TrackQuantities();
    }

    private void GetBoss(bool usingSave, Tuple<string, int, int, int, int, int> saveTuple)
    {
        //temp values
        var boss = "None";
        string wName;
        int wRoom;
        int wId1;
        int wId2;
        int wId3;
        int wCup;
        if (!usingSave)
        {
            wName = world.WorldName;
            wRoom = world.RoomNumber;
            wId1 = world.EventId1;
            wId2 = world.EventId2;
            wId3 = world.EventId3;
            wCup = world.CupRound;
        }
        else
        {
            wName = saveTuple.Item1;
            wRoom = saveTuple.Item2;
            wId1 = saveTuple.Item3;
            wId2 = saveTuple.Item4;
            wId3 = saveTuple.Item5;
            wCup = saveTuple.Item6;
        }

        //stops awarding points for a single boss each tick
        if (!usingSave)
        {
            if (world.EventComplete == 1 && eventInProgress)
                return;
            else
                eventInProgress = false;
        }

        //eventlog check
        var eventTuple = new Tuple<string, int, int, int, int, int>(
            wName,
            wRoom,
            wId1,
            wId2,
            wId3,
            wCup
        );
        if (Data.BossEventLog.Contains(eventTuple))
            return;

        //boss beaten events (taken mostly from progression code)
        switch (wName)
        {
            case "SimulatedTwilightTown":
                switch (wRoom) //check based on room number now, then based on events in each room
                {
                    case 34:
                        if (wId1 == 157) // Twilight Thorn finish
                            boss = "Twilight Thorn";
                        break;
                    case 3:
                        if (wId1 == 180) // Seifer Battle (Day 4)
                            boss = "Seifer";
                        break;
                    case 4:
                        //Tutorial Seifer shouldn't give points
                        //if (wID1 == 77) // Tutorial 4 - Fighting
                        //    boss = "Seifer (1)";
                        if (wId1 == 78) // Seifer I Battle
                            boss = "Seifer (2)";
                        break;
                    case 5:
                        boss = wId1 switch
                        {
                            // Hayner Struggle
                            84 => "Hayner",
                            // Vivi Struggle
                            85 => "Vivi",
                            // Axel 1 Finish
                            87 => "Axel I",
                            // Setzer Struggle
                            88 => "Setzer",
                            _ => boss
                        };
                        break;
                    case 20:
                        if (wId1 == 137) // Axel 2 finish
                            boss = "Axel II";
                        break;
                    default:
                        break;
                }
                break;
            case "TwilightTown":
                switch (wRoom)
                {
                    case 20:
                        if (wId1 == 213) // Data Axel finish
                            boss = "Axel (Data)";
                        break;
                    case 4:
                        boss = wId1 switch
                        {
                            // Seifer II Battle
                            181 => "Seifer (3)",
                            // Hayner Battle (Struggle Competition)
                            182 => "Hayner (SR)",
                            // Setzer Battle (Struggle Competition)
                            183 => "Setzer (SR)",
                            // Seifer Battle (Struggle Competition)
                            184 => "Seifer (4)",
                            _ => boss
                        };
                        break;
                    default:
                        break;
                }
                break;
            case "HollowBastion":
                switch (wRoom)
                {
                    case 4:
                        boss = wId1 switch
                        {
                            // HB Demyx finish
                            55 => "Demyx",
                            // Data Demyx finish
                            114 => "Demyx (Data)",
                            _ => boss
                        };
                        break;
                    case 1:
                        if (wId1 == 75) // Sephiroth finish
                            boss = "Sephiroth";
                        break;
                    default:
                        break;
                }
                break;
            case "BeastsCastle":
                switch (wRoom)
                {
                    case 11:
                        if (wId1 == 72) // Thresholder finish
                            boss = "Thresholder";
                        break;
                    case 3:
                        if (wId1 == 69) // Beast finish
                            boss = "The Beast";
                        break;
                    case 5:
                        boss = wId1 switch
                        {
                            // Shadow Stalker
                            78 => "Shadow Stalker",
                            // Dark Thorn finish
                            79 => "Dark Thorn",
                            _ => boss
                        };
                        break;
                    case 15:
                        boss = wId1 switch
                        {
                            // Xaldin finish
                            82 => "Xaldin",
                            // Data Xaldin finish
                            97 => "Xaldin (Data)",
                            _ => boss
                        };
                        break;
                    default:
                        break;
                }
                break;
            case "OlympusColiseum":
                switch (wRoom)
                {
                    case 7:
                        if (wId1 == 114) // Cerberus finish
                            boss = "Cerberus";
                        break;
                    case 8:
                        if (wId1 == 116) // OC Pete finish
                            boss = "Pete OC II";
                        break;
                    case 18:
                        if (wId1 == 171) // Hydra finish
                            boss = "Hydra";
                        break;
                    case 19:
                        if (wId1 == 202) // Hades finish
                            boss = "Hades II (1)";
                        break;
                    case 34:
                        boss = wId1 switch
                        {
                            // Zexion finish
                            151 => "Zexion",
                            // Data Zexion finish
                            152 => "Zexion (Data)",
                            _ => boss
                        };
                        break;
                    case 9: //Cups
                        boss = wId1 switch
                        {
                            189 when wCup == 10 => "FF Team 1",
                            190 when wCup == 10 => "Cerberus (Cups)",
                            191 when wCup == 10 => "Hercules",
                            192 when wCup == 10 => "Hades Cups",
                            //paradox cups
                            193 when wCup == 10 => "FF Team 2",
                            194 when wCup == 10 => "Cerberus (Cups)",
                            195 when wCup == 10 => "Hercules",
                            //hades paradox
                            196 when wCup == 5 => "Volcano Lord (Cups)",
                            _ => boss
                        };
                        if (wId1 == 196 && wCup == 10)
                            boss = "FF Team 3"; // Yuffie (1) & Tifa
                        if (wId1 == 196 && wCup == 15)
                            boss = "Blizzard Lord (Cups)";
                        if (wId1 == 196 && wCup == 20)
                            boss = "Pete Cups";
                        if (wId1 == 196 && wCup == 25)
                            boss = "FF Team 4"; // Cloud & Tifa (1)
                        if (wId1 == 196 && wCup == 30)
                            boss = "Hades Cups";
                        if (wId1 == 196 && wCup == 40)
                            boss = "FF Team 5"; // Leon (1) & Cloud (1)
                        if (wId1 == 196 && wCup == 48)
                            boss = "Cerberus (Cups)";
                        if (wId1 == 196 && wCup == 49)
                            boss = "FF Team 6"; // Leon (2), Cloud (2), Yuffie (2), & Tifa (2)
                        if (wId1 == 196 && wCup == 50)
                            boss = "Hades II";
                        break;
                    default:
                        break;
                }
                break;
            case "Agrabah":
                switch (wRoom)
                {
                    case 3:
                        if (wId1 == 59) // Lords finish
                            boss = "Twin Lords";
                        break;
                    case 5:
                        if (wId1 == 62) // Genie Jafar finish
                            boss = "Jafar";
                        break;
                    case 33:
                        boss = wId1 switch
                        {
                            // Lexaeus finish
                            142 => "Lexaeus",
                            // Data Lexaeus finish
                            147 => "Lexaeus (Data)",
                            _ => boss
                        };
                        break;
                    default:
                        break;
                }
                break;
            case "LandofDragons":
                switch (wRoom)
                {
                    case 9:
                        if (wId1 == 75) // Shan Yu finish
                            boss = "Shan-Yu";
                        break;
                    case 7:
                        if (wId1 == 76) // Riku
                            boss = "Riku";
                        break;
                    case 8:
                        if (wId1 == 79) // Storm Rider finish
                            boss = "Storm Rider";
                        break;
                    default:
                        break;
                }
                break;
            case "PrideLands":
                switch (wRoom)
                {
                    case 14:
                        if (wId1 == 55) // Scar finish
                            boss = "Scar";
                        break;
                    case 15:
                        if (wId1 == 59) // Groundshaker finish
                            boss = "Groundshaker";
                        break;
                    default:
                        break;
                }
                break;
            case "DisneyCastle":
                switch (wRoom)
                {
                    case 1:
                        if (wId1 == 58) // Old Pete finish
                            boss = "Past Pete";
                        break;
                    case 2:
                        if (wId1 == 52) // Boat Pete finish
                            boss = "Boat Pete";
                        break;
                    case 3:
                        if (wId1 == 53) // DC Pete finish
                            boss = "Pete TR";
                        break;
                    case 38:
                        boss = wId1 switch
                        {
                            // Marluxia finish
                            145 => "Marluxia",
                            // Data Marluxia finish
                            150 => "Marluxia (Data)",
                            _ => boss
                        };
                        break;
                    case 7:
                        if (wId1 == 67) // Lingering Will finish
                            boss = "Terra";
                        break;
                    default:
                        break;
                }
                break;
            case "HalloweenTown":
                switch (wRoom)
                {
                    case 3:
                        if (wId1 == 52) // Prison Keeper finish
                            boss = "Prison Keeper";
                        break;
                    case 9:
                        if (wId1 == 55) // Oogie Boogie finish
                            boss = "Oogie Boogie";
                        break;
                    case 7:
                        if (wId1 == 64) // Experiment finish
                            boss = "The Experiment";
                        break;
                    case 32:
                        boss = wId1 switch
                        {
                            // Vexen finish
                            115 => "Vexen",
                            // Data Vexen finish
                            146 => "Vexen (Data)",
                            _ => boss
                        };
                        break;
                    default:
                        break;
                }
                break;
            case "PortRoyal":
                switch (wRoom)
                {
                    case 10:
                        if (wId1 == 60) // Barbossa finish
                            boss = "Barbossa";
                        break;
                    case 18:
                        if (wId1 == 85) // Grim Reaper 1 finish
                            boss = "Grim Reaper I";
                        break;
                    case 1:
                        if (wId1 == 54) // Grim Reaper 2 finish
                            boss = "Grim Reaper II";
                        break;
                    default:
                        break;
                }
                break;
            case "SpaceParanoids":
                switch (wRoom)
                {
                    case 4:
                        if (wId1 == 55) // Hostile Program finish
                            boss = "Hostile Program";
                        break;
                    case 9:
                        boss = wId1 switch
                        {
                            // Sark finish
                            58 => "Sark",
                            // MCP finish
                            59 => "MCP",
                            _ => boss
                        };
                        break;
                    case 33:
                        boss = wId1 switch
                        {
                            // Larxene finish
                            143 => "Larxene",
                            // Data Larxene finish
                            148 => "Larxene (Data)",
                            _ => boss
                        };
                        break;
                    default:
                        break;
                }
                break;
            case "TWTNW":
                switch (wRoom)
                {
                    case 21:
                        boss = wId1 switch
                        {
                            // Roxas finish
                            65 => "Roxas",
                            // Data Roxas finish
                            99 => "Roxas (Data)",
                            _ => boss
                        };
                        break;
                    case 10:
                        boss = wId1 switch
                        {
                            // Xigbar finish
                            57 => "Xigbar",
                            // Data Xigbar finish
                            100 => "Xigbar (Data)",
                            _ => boss
                        };
                        break;
                    case 14:
                        boss = wId1 switch
                        {
                            // Luxord finish
                            58 => "Luxord",
                            // Data Luxord finish
                            101 => "Luxord (Data)",
                            _ => boss
                        };
                        break;
                    case 15:
                        boss = wId1 switch
                        {
                            // Saix finish
                            56 => "Saix",
                            // Data Saix finish
                            102 => "Saix (Data)",
                            _ => boss
                        };
                        break;
                    case 19:
                        boss = wId1 switch
                        {
                            // Xemnas 1 finish
                            59 => "Xemnas",
                            // Data Xemnas I finish
                            97 => "Xemnas (Data)",
                            _ => boss
                        };
                        break;
                    case 20:
                        boss = wId1 switch
                        {
                            // Final Xemnas finish
                            74 => "Final Xemnas",
                            // Data Final Xemnas finish
                            98 => "Final Xemnas (Data)",
                            _ => boss
                        };
                        break;
                    case 23:
                        if (wId1 == 73) // Armor Xemnas II
                            boss = "Armor Xemnas II";
                        break;
                    case 24:
                        if (wId1 == 71) // Armor Xemnas I
                            boss = "Armor Xemnas I";
                        break;
                    default:
                        break;
                }
                break;
            default:
                break;
        }

        //return if bo boss beaten found


        if (!usingSave)
        {
            //if the boss was found and beaten then set flag
            //we do this to stop things happening every frame
            if (world.EventComplete == 1)
                eventInProgress = true;
            else
                return;
        }

        if (boss == "None")
            return;

        App.Logger?.Record("Beaten Boss: " + boss);

        //add to log
        Data.BossEventLog.Add(eventTuple);
    }

    private void HighlightWorld()
    {
        if (WorldHighlightOption.IsChecked == false)
            return;

        if (
            world.PreviousWorldName != null
            && Data.WorldsData.TryGetValue(world.PreviousWorldName, out var value)
        )
        {
            foreach (
                var box in value.Top.Children
                    .OfType<Rectangle>()
                    .Where(box => box.Name.EndsWith("SelWG"))
            )
            {
                box.Visibility = Visibility.Collapsed;
            }
        }

        if (Data.WorldsData.TryGetValue(world.WorldName, out var worldData))
        {
            foreach (
                var box in worldData.Top.Children
                    .OfType<Rectangle>()
                    .Where(box => box.Name.EndsWith("SelWG"))
            )
            {
                box.Visibility = Visibility.Visible;
            }
        }
    }

    ///
    /// Bindings & helpers
    ///

    private void SetBindings()
    {
        BindWeapon(SorasHeartWeapon, "Weapon", stats);

        //changes opacity for stat icons
        BindAbility(HighJump, "Obtained", highJump);
        BindAbility(QuickRun, "Obtained", quickRun);
        BindAbility(DodgeRoll, "Obtained", dodgeRoll);
        BindAbility(AerialDodge, "Obtained", aerialDodge);
        BindAbility(Glide, "Obtained", glide);

        BindForm(ValorM, "Obtained", valor);
        BindForm(WisdomM, "Obtained", wisdom);
        BindForm(LimitM, "Obtained", limit);
        BindForm(MasterM, "Obtained", master);
        BindForm(FinalM, "Obtained", final);
    }

    private void BindForm(ContentControl img, string property, object source)
    {
        var binding = new Binding(property)
        {
            Source = source,
            Converter = new ObtainedConverter()
        };
        img.SetBinding(OpacityProperty, binding);
    }

    private void BindAbility(ContentControl img, string property, object source)
    {
        var binding = new Binding(property)
        {
            Source = source,
            Converter = new ObtainedConverter()
        };
        img.SetBinding(OpacityProperty, binding);
    }

    private void BindWeapon(Image img, string property, object source)
    {
        var binding = new Binding(property) { Source = source, Converter = new WeaponConverter() };
        img.SetBinding(Image.SourceProperty, binding);
    }

    private string BytesToHex(byte[] bytes)
    {
        if (bytes.SequenceEqual(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }))
        {
            return "Service not started. Waiting for PCSX2";
        }
        return BitConverter.ToString(bytes).Replace("-", "");
    }

    public string GetWorld()
    {
        return world.WorldName;
    }

    public void UpdateUsedPages()
    {
        Data.UsedPages++;
    }

    public int GetUsedPages()
    {
        return Data.UsedPages;
    }

    private void UpdateFormProgression()
    {
        var found = 0;
        var oldToggled = App.Settings.OldProg;
        var customToggled = App.Settings.CustomIcons;
        var prog = "Min-"; //Default
        if (oldToggled)
            prog = "Old-";
        if (customProgFound && customToggled)
            prog = "Cus-";

        if (Math.Abs(ValorM.Opacity - 1) < 0.0001)
            found++;
        if (Math.Abs(WisdomM.Opacity - 1) < 0.0001)
            found++;
        if (Math.Abs(LimitM.Opacity - 1) < 0.0001)
            found++;
        if (Math.Abs(MasterM.Opacity - 1) < 0.0001)
            found++;
        if (Math.Abs(FinalM.Opacity - 1) < 0.0001)
            found++;

        var drives = found switch
        {
            1 => "Drive3",
            2 => "Drive4",
            3 => "Drive5",
            4 => "Drive6",
            5 => "Drive7",
            _ => "Drive2"
        };

        DriveFormsCap.SetResourceReference(ContentProperty, prog + drives);
    }

    private int ReadMemInt(int address)
    {
        return BitConverter.ToInt32(memory.ReadMemory(address, 4), 0);
    }
}
