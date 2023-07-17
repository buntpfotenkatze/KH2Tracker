using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using GregsStack.InputSimulatorStandard;
using GregsStack.InputSimulatorStandard.Native;

namespace KhTracker;

/// <summary>
/// Interaction logic for WorldGrid.xaml
/// </summary>
public partial class WorldGrid
{
    //let's simplyfy some stuff and remove a ton of redundant code
    private readonly MainWindow window = (MainWindow)Application.Current.MainWindow;

    //real versions for itempool counts
    public static int RealFire;
    public static int RealBlizzard;
    public static int RealThunder;
    public static int RealCure;
    public static int RealReflect;
    public static int RealMagnet;
    public static int RealPages;
    public static int RealPouches;

    //public static int localLevelCount = 0;
    public static int GhostFire = 0;
    public static int GhostBlizzard = 0;
    public static int GhostThunder = 0;
    public static int GhostCure = 0;
    public static int GhostReflect = 0;
    public static int GhostMagnet = 0;
    public static int GhostPages = 0;
    public static int GhostPouches = 0;

    //amount of obtained ghost magic/pages
    public static int GhostFireObtained = 0;
    public static int GhostBlizzardObtained = 0;
    public static int GhostThunderObtained = 0;
    public static int GhostCureObtained = 0;
    public static int GhostReflectObtained = 0;
    public static int GhostMagnetObtained = 0;
    public static int GhostPagesObtained = 0;
    public static int GhostPouchesObtained = 0;

    //track other types of collections
    public static int ProofCount;
    public static int FormCount;
    public static int SummonCount;
    public static int AbilityCount;
    public static int ReportCount;
    public static int VisitCount;

    private static bool _proofOfConnectionObtained;
    private static bool _proofOfNonExistenceObtained;
    private static bool _proofOfPeaceObtained;
    private static int _numProofsObtained;

    //A single spot to have referenced for the opacity of the ghost checks idk where to put this
    public static double UniversalOpacity = 0.5;

    public WorldGrid()
    {
        InitializeComponent();
    }

    public void Handle_WorldGrid(Item button, bool add)
    {
        var data = MainWindow.Data;
        var addRemove = 1;

        if (add)
        {
            //Default should be children count so that items are
            //always added to the end if no ghosts are found
            var firstGhost = Children.Count;

            //search for ghost items
            foreach (Item child in Children)
            {
                if (child.Name.StartsWith("Ghost_"))
                {
                    //when one is found get the index of it
                    firstGhost = Children.IndexOf(child);
                    break;
                }
            }

            //add the item
            try
            {
                Children.Insert(firstGhost, button);
            }
            catch (Exception)
            {
                return;
            }
        }
        else
        {
            Children.Remove(button);
            addRemove = -1;
        }

        UpdateMulti(button, add);

        var gridremainder = 0;
        if (Children.Count % 5 != 0)
            gridremainder = 1;

        var gridnum = Math.Max((Children.Count / 5) + gridremainder, 1);
        Rows = gridnum;

        // default 1, add .5 for every row
        var length = 1 + ((Children.Count - 1.0) / 5) / 2.0;
        var outerGrid = (Parent as Grid)!.Parent as Grid;
        var row = (int)Parent.GetValue(Grid.RowProperty);
        outerGrid!.RowDefinitions[row].Height = new GridLength(length, GridUnitType.Star);
        var worldName = Name.Substring(0, Name.Length - 4);

        //visit lock check first
        if (window.VisitLockOption.IsChecked)
        {
            SetVisitLock(button.Name, addRemove);
        }

        switch (button.Name)
        {
            case "Connection":
                _proofOfConnectionObtained = add;
                break;
            case "Nonexistence":
                _proofOfNonExistenceObtained = add;
                break;
            case "Peace":
                _proofOfPeaceObtained = add;
                break;
            default:
                return;
        }

        var proofsObtained =
            (_proofOfConnectionObtained ? 1 : 0)
            + (_proofOfNonExistenceObtained ? 1 : 0)
            + (_proofOfPeaceObtained ? 1 : 0);

        if (_numProofsObtained != proofsObtained && Properties.Settings.Default.EmitProofKeystroke)
        {
            _numProofsObtained = proofsObtained;
            var simu = new InputSimulator();
            var keycode = _numProofsObtained switch
            {
                1 => VirtualKeyCode.F6,
                2 => VirtualKeyCode.F7,
                3 => VirtualKeyCode.F8,
                _ => VirtualKeyCode.F5
            };

            simu.Keyboard
                .KeyDown(VirtualKeyCode.CONTROL)
                .KeyDown(VirtualKeyCode.SHIFT)
                .KeyDown(keycode)
                .Sleep(100)
                .KeyUp(keycode)
                .KeyUp(VirtualKeyCode.SHIFT)
                .KeyUp(VirtualKeyCode.CONTROL);
        }
    }

    public void Add_Item(Item item)
    {
        //remove item from itempool
        Grid itemRow;
        try
        {
            itemRow = VisualTreeHelper.GetParent(item) as Grid;
        }
        catch
        {
            return;
        }

        if (itemRow == null || itemRow.Parent != window.ItemPool)
            return;

        itemRow.Children.Remove(item);

        //add it to the world grid
        Handle_WorldGrid(item, true);

        //fix shadow opacity if needed
        Handle_Shadows(item, true);

        //Reset any obtained item to be normal transparency
        item.Opacity = 1.0;

        // update collection count
        window.SetCollected(true);

        // update mouse actions
        if (MainWindow.Data.DragDrop)
        {
            item.MouseDoubleClick -= item.Item_Click;
            item.MouseMove -= item.Item_MouseMove;
        }
        else
        {
            item.MouseDown -= item.Item_MouseDown;
            item.MouseUp -= item.Item_MouseUp;
        }
        item.MouseDown -= item.Item_Return;
        item.MouseDown += item.Item_Return;
    }

    public void UpdateMulti(Item item, bool add)
    {
        //do nothing for ghost items
        if (item.Name.StartsWith("Ghost_"))
            return;

        var addRemove = 1;
        if (!add)
            addRemove = -1;

        if (
            Codes.FindItemType(item.Name) != "magic"
            && Codes.FindItemType(item.Name) != "page"
            && !item.Name.Contains("Munny")
        ) //Codes.FindItemType(item.Name) != "other")
        {
            //yeah just gonna do things here..
            //track collection for things that aren't multi's
            switch (Codes.FindItemType(item.Name))
            {
                case "proof":
                    ProofCount += addRemove;
                    return;
                case "form":
                    FormCount += addRemove;
                    return;
                case "ability":
                    AbilityCount += addRemove;
                    return;
                case "summon":
                    SummonCount += addRemove;
                    return;
                case "visit":
                    VisitCount += addRemove;
                    return;
                case "report":
                    ReportCount += addRemove;
                    return;
                //collecting the other aux checks does nothing for now. maybe someday
                //case "other":
                //    Aux_Count += addRemove;
                //    return;
                default:
                    return;
            }
        }

        char[] numbers = { '1', '2', '3', '4', '5' };
        var itemname = item.Name.TrimEnd(numbers);

        switch (itemname)
        {
            case "Fire":
                RealFire += addRemove;
                window.FireCount.Text = (3 - RealFire).ToString();
                if (RealFire == 3)
                {
                    window.FireCount.Fill = (SolidColorBrush)FindResource("Color_Black");
                    window.FireCount.Stroke = (SolidColorBrush)FindResource("Color_Trans");
                }
                else
                {
                    window.FireCount.Fill = (LinearGradientBrush)FindResource("Color_Fire");
                    window.FireCount.Stroke = (SolidColorBrush)FindResource("Color_Black");
                }
                return;
            case "Blizzard":
                RealBlizzard += addRemove;
                window.BlizzardCount.Text = (3 - RealBlizzard).ToString();
                if (RealBlizzard == 3)
                {
                    window.BlizzardCount.Fill = (SolidColorBrush)FindResource("Color_Black");
                    window.BlizzardCount.Stroke = (SolidColorBrush)FindResource("Color_Trans");
                }
                else
                {
                    window.BlizzardCount.Fill = (LinearGradientBrush)FindResource("Color_Blizzard");
                    window.BlizzardCount.Stroke = (SolidColorBrush)FindResource("Color_Black");
                }
                return;
            case "Thunder":
                RealThunder += addRemove;
                window.ThunderCount.Text = (3 - RealThunder).ToString();
                if (RealThunder == 3)
                {
                    window.ThunderCount.Fill = (SolidColorBrush)FindResource("Color_Black");
                    window.ThunderCount.Stroke = (SolidColorBrush)FindResource("Color_Trans");
                }
                else
                {
                    window.ThunderCount.Fill = (LinearGradientBrush)FindResource("Color_Thunder");
                    window.ThunderCount.Stroke = (SolidColorBrush)FindResource("Color_Black");
                }
                return;
            case "Cure":
                RealCure += addRemove;
                window.CureCount.Text = (3 - RealCure).ToString();
                if (RealCure == 3)
                {
                    window.CureCount.Fill = (SolidColorBrush)FindResource("Color_Black");
                    window.CureCount.Stroke = (SolidColorBrush)FindResource("Color_Trans");
                }
                else
                {
                    window.CureCount.Fill = (LinearGradientBrush)FindResource("Color_Cure");
                    window.CureCount.Stroke = (SolidColorBrush)FindResource("Color_Black");
                }
                return;
            case "Magnet":
                RealMagnet += addRemove;
                window.MagnetCount.Text = (3 - RealMagnet).ToString();
                if (RealMagnet == 3)
                {
                    window.MagnetCount.Fill = (SolidColorBrush)FindResource("Color_Black");
                    window.MagnetCount.Stroke = (SolidColorBrush)FindResource("Color_Trans");
                }
                else
                {
                    window.MagnetCount.Fill = (LinearGradientBrush)FindResource("Color_Magnet");
                    window.MagnetCount.Stroke = (SolidColorBrush)FindResource("Color_Black");
                }
                return;
            case "Reflect":
                RealReflect += addRemove;
                window.ReflectCount.Text = (3 - RealReflect).ToString();
                if (RealReflect == 3)
                {
                    window.ReflectCount.Fill = (SolidColorBrush)FindResource("Color_Black");
                    window.ReflectCount.Stroke = (SolidColorBrush)FindResource("Color_Trans");
                }
                else
                {
                    window.ReflectCount.Fill = (LinearGradientBrush)FindResource("Color_Reflect");
                    window.ReflectCount.Stroke = (SolidColorBrush)FindResource("Color_Black");
                }
                return;
            case "TornPage":
                RealPages += addRemove;
                window.PageCount.Text = (5 - RealPages).ToString();
                if (RealPages == 5)
                {
                    window.PageCount.Fill = (SolidColorBrush)FindResource("Color_Black");
                    window.PageCount.Stroke = (SolidColorBrush)FindResource("Color_Trans");
                }
                else
                {
                    window.PageCount.Fill = (LinearGradientBrush)FindResource("Color_Page");
                    window.PageCount.Stroke = (SolidColorBrush)FindResource("Color_Black");
                }
                return;
            case "MunnyPouch":
                RealPouches += addRemove;
                window.MunnyCount.Text = (2 - RealPouches).ToString();
                if (RealPouches == 2)
                {
                    window.MunnyCount.Fill = (SolidColorBrush)FindResource("Color_Black");
                    window.MunnyCount.Stroke = (SolidColorBrush)FindResource("Color_Trans");
                }
                else
                {
                    window.MunnyCount.Fill = (LinearGradientBrush)FindResource("Color_Pouch");
                    window.MunnyCount.Stroke = (SolidColorBrush)FindResource("Color_Black");
                }
                return;
            default:
                return;
        }
    }

    ///
    /// world value handling
    ///

    //public void Updatenumbers_spoil(WorldData worldData)
    //{
    //    if (worldData.complete || worldData.containsGhost == false)
    //        return;
    //
    //    if (worldData.value != null)
    //    {
    //        int WorldNumber = -1;
    //
    //        if (worldData.value.Text != "?")
    //            WorldNumber = int.Parse(worldData.hint.Text);
    //
    //        MainW.SetWorldNumber(worldData.hint, WorldNumber, "G");
    //    }
    //    else
    //        return;
    //}

    private int TableReturn(string nameButton)
    {
        var data = MainWindow.Data;
        var type = Codes.FindItemType(nameButton);
        if (type != "Unknown")
        {
            return nameButton.StartsWith("Ghost_") ? 0 : data.PointsDatanew[type];
        }
        return 0;

        //else if (MainWindow.data.PointsDatanew.Keys.Contains(type))
    }

    private void Handle_Shadows(Item item, bool add)
    {
        //don't hide shadows for the multi items
        if (
            Codes.FindItemType(item.Name) == "magic"
            || Codes.FindItemType(item.Name) == "page"
            || item.Name.StartsWith("Munny")
            || item.Name.StartsWith("Ghost_")
        )
        {
            return;
        }

        var shadowName = "S_" + item.Name;
        //ContentControl shadow = window.ItemPool.FindName(shadowName) as ContentControl;
        ContentControl shadow = null;

        foreach (Grid itemrow in window.ItemPool.Children)
        {
            shadow = itemrow.FindName(shadowName) as ContentControl;

            if (shadow != null)
                break;
        }

        if (shadow == null)
            return;

        if (add)
        {
            shadow.Opacity = 1.0;
        }
        else
        {
            shadow.Opacity = 0.0;
        }
    }

    private void SpoilerWorldReveal(string worldname, string report)
    {
        var data = MainWindow.Data;
        //check if report was tracked before in this session to avoid tracking
        //multiple ghosts for removing and placing the same report back
        if (data.TrackedReports.Contains(report))
            return;
        else
            data.TrackedReports.Add(report);

        //create temp list of what a world should have
        var worldItems = data.WorldsData[worldname].CheckCount;
        char[] numbers = { '1', '2', '3', '4', '5' };

        //Get list of items we should track. we don't want to place more ghosts than is needed
        //(ex. a world has 2 blizzards and we already have 1 tracked there)
        var worldGrid = data.WorldsData[worldname].WorldGrid;
        foreach (Item item in worldGrid.Children)
        {
            //just skip if item is a ghost. checkCount should never contain ghosts anyway
            if (item.Name.StartsWith("Ghost_"))
                continue;

            //do not trim numbers if report
            if (item.Name.Contains("Report") && worldItems.Contains(item.Name))
                worldItems.Remove(item.Name);
            else if (worldItems.Contains(item.Name.TrimEnd(numbers)))
            {
                worldItems.Remove(item.Name.TrimEnd(numbers));
            }
        }

        //start tracking what's left in the temp list
        foreach (var itemname in worldItems)
        {
            //don't track item types not set in reveal list
            if (!data.SpoilerRevealTypes.Contains(Codes.FindItemType(itemname)))
            {
                continue;
            }

            //this shouldn't ever happen, but return without doing anything else if the ghost values for magic/pages are higher than expected
            switch (itemname)
            {
                case "Fire":
                    if (GhostFire >= 3)
                    {
                        return;
                    }
                    break;
                case "Blizzard":
                    if (GhostBlizzard >= 3)
                    {
                        return;
                    }
                    break;
                case "Thunder":
                    if (GhostThunder >= 3)
                    {
                        return;
                    }
                    break;
                case "Cure":
                    if (GhostCure >= 3)
                    {
                        return;
                    }
                    break;
                case "Magnet":
                    if (GhostMagnet >= 3)
                    {
                        return;
                    }
                    break;
                case "Reflect":
                    if (GhostReflect >= 3)
                    {
                        return;
                    }
                    break;
                case "TornPage":
                    if (GhostPages >= 5)
                    {
                        return;
                    }
                    break;
                case "MunnyPouch":
                    if (GhostPouches >= 2)
                    {
                        return;
                    }
                    break;
                default:
                    break;
            }
        }
    }

    private void SetWorldGhost(string worldName)
    {
        var data = MainWindow.Data;
        foreach (Item child in Children)
        {
            if (data.GhostItems.Values.Contains(child))
            {
                data.WorldsData[worldName].ContainsGhost = true;
                return;
            }
            else
            {
                data.WorldsData[worldName].ContainsGhost = false;
            }
        }
    }

    private void SetVisitLock(string itemName, int addRemove)
    {
        var data = MainWindow.Data;
        //reminder: 1 = locked | 0 = unlocked
        //reminder for TT: 10 = 3rd visit locked | 1 = 2nd visit locked | 11 = both locked | 0 = both unlocked
        switch (itemName)
        {
            case "AuronWep":
                data.WorldsData["OlympusColiseum"].VisitLocks -= addRemove;
                break;
            case "MulanWep":
                data.WorldsData["LandofDragons"].VisitLocks -= addRemove;
                break;
            case "BeastWep":
                data.WorldsData["BeastsCastle"].VisitLocks -= addRemove;
                break;
            case "JackWep":
                data.WorldsData["HalloweenTown"].VisitLocks -= addRemove;
                break;
            case "SimbaWep":
                data.WorldsData["PrideLands"].VisitLocks -= addRemove;
                break;
            case "SparrowWep":
                data.WorldsData["PortRoyal"].VisitLocks -= addRemove;
                break;
            case "AladdinWep":
                data.WorldsData["Agrabah"].VisitLocks -= addRemove;
                break;
            case "TronWep":
                data.WorldsData["SpaceParanoids"].VisitLocks -= addRemove;
                break;
            case "IceCream":
                data.WorldsData["TwilightTown"].VisitLocks -= (addRemove * 10);
                break;
            case "Picture":
                data.WorldsData["TwilightTown"].VisitLocks -= addRemove;
                break;
            case "MembershipCard":
                data.WorldsData["HollowBastion"].VisitLocks -= addRemove;
                break;
            default:
                return;
        }

        window.VisitLockCheck();
    }
}
