using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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

    public WorldGrid()
    {
        InitializeComponent();
    }

    public void Handle_WorldGrid(Item button, bool add)
    {
        var addRemove = 1;

        if (add)
        {
            //Default should be children count so that items are
            //always added to the end if no ghosts are found
            var firstGhost = Children.Count;

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

        if (_numProofsObtained != proofsObtained && App.Settings.EmitProofKeystroke)
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

    private void Item_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(typeof(Item)))
        {
            var item = e.Data.GetData(typeof(Item)) as Item;

            if (ReportHandler())
                Add_Item(item);
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

    private void UpdateMulti(Item item, bool add)
    {
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
                    window.FireCount.Fill = (SolidColorBrush)FindResource("ColorBlack");
                    window.FireCount.Stroke = (SolidColorBrush)FindResource("ColorTrans");
                }
                else
                {
                    window.FireCount.Fill = (LinearGradientBrush)FindResource("ColorFire");
                    window.FireCount.Stroke = (SolidColorBrush)FindResource("ColorBlack");
                }
                return;
            case "Blizzard":
                RealBlizzard += addRemove;
                window.BlizzardCount.Text = (3 - RealBlizzard).ToString();
                if (RealBlizzard == 3)
                {
                    window.BlizzardCount.Fill = (SolidColorBrush)FindResource("ColorBlack");
                    window.BlizzardCount.Stroke = (SolidColorBrush)FindResource("ColorTrans");
                }
                else
                {
                    window.BlizzardCount.Fill = (LinearGradientBrush)FindResource("ColorBlizzard");
                    window.BlizzardCount.Stroke = (SolidColorBrush)FindResource("ColorBlack");
                }
                return;
            case "Thunder":
                RealThunder += addRemove;
                window.ThunderCount.Text = (3 - RealThunder).ToString();
                if (RealThunder == 3)
                {
                    window.ThunderCount.Fill = (SolidColorBrush)FindResource("ColorBlack");
                    window.ThunderCount.Stroke = (SolidColorBrush)FindResource("ColorTrans");
                }
                else
                {
                    window.ThunderCount.Fill = (LinearGradientBrush)FindResource("ColorThunder");
                    window.ThunderCount.Stroke = (SolidColorBrush)FindResource("ColorBlack");
                }
                return;
            case "Cure":
                RealCure += addRemove;
                window.CureCount.Text = (3 - RealCure).ToString();
                if (RealCure == 3)
                {
                    window.CureCount.Fill = (SolidColorBrush)FindResource("ColorBlack");
                    window.CureCount.Stroke = (SolidColorBrush)FindResource("ColorTrans");
                }
                else
                {
                    window.CureCount.Fill = (LinearGradientBrush)FindResource("ColorCure");
                    window.CureCount.Stroke = (SolidColorBrush)FindResource("ColorBlack");
                }
                return;
            case "Magnet":
                RealMagnet += addRemove;
                window.MagnetCount.Text = (3 - RealMagnet).ToString();
                if (RealMagnet == 3)
                {
                    window.MagnetCount.Fill = (SolidColorBrush)FindResource("ColorBlack");
                    window.MagnetCount.Stroke = (SolidColorBrush)FindResource("ColorTrans");
                }
                else
                {
                    window.MagnetCount.Fill = (LinearGradientBrush)FindResource("ColorMagnet");
                    window.MagnetCount.Stroke = (SolidColorBrush)FindResource("ColorBlack");
                }
                return;
            case "Reflect":
                RealReflect += addRemove;
                window.ReflectCount.Text = (3 - RealReflect).ToString();
                if (RealReflect == 3)
                {
                    window.ReflectCount.Fill = (SolidColorBrush)FindResource("ColorBlack");
                    window.ReflectCount.Stroke = (SolidColorBrush)FindResource("ColorTrans");
                }
                else
                {
                    window.ReflectCount.Fill = (LinearGradientBrush)FindResource("ColorReflect");
                    window.ReflectCount.Stroke = (SolidColorBrush)FindResource("ColorBlack");
                }
                return;
            case "TornPage":
                RealPages += addRemove;
                window.PageCount.Text = (5 - RealPages).ToString();
                if (RealPages == 5)
                {
                    window.PageCount.Fill = (SolidColorBrush)FindResource("ColorBlack");
                    window.PageCount.Stroke = (SolidColorBrush)FindResource("ColorTrans");
                }
                else
                {
                    window.PageCount.Fill = (LinearGradientBrush)FindResource("ColorPage");
                    window.PageCount.Stroke = (SolidColorBrush)FindResource("ColorBlack");
                }
                return;
            case "MunnyPouch":
                RealPouches += addRemove;
                window.MunnyCount.Text = (2 - RealPouches).ToString();
                if (RealPouches == 2)
                {
                    window.MunnyCount.Fill = (SolidColorBrush)FindResource("ColorBlack");
                    window.MunnyCount.Stroke = (SolidColorBrush)FindResource("ColorTrans");
                }
                else
                {
                    window.MunnyCount.Fill = (LinearGradientBrush)FindResource("ColorPouch");
                    window.MunnyCount.Stroke = (SolidColorBrush)FindResource("ColorBlack");
                }
                return;
            default:
                return;
        }
    }

    private static bool ReportHandler() => true;

    ///
    /// world value handling
    ///

    private void Handle_Shadows(Item item, bool add)
    {
        //don't hide shadows for the multi items
        if (
            Codes.FindItemType(item.Name) == "magic"
            || Codes.FindItemType(item.Name) == "page"
            || item.Name.StartsWith("Munny")
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

        shadow.Opacity = add ? 1.0 : 0.0;
    }

    private void SetVisitLock(string itemName, int addRemove)
    {
        var data = MainWindow.Data;
        // reminder: 1 >= locked | 0 = unlocked

        if (int.TryParse(itemName.Last().ToString(), out _))
        {
            itemName = itemName[..^1];
        }

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
                data.WorldsData["TwilightTown"].VisitLocks -= addRemove;
                break;
            case "MembershipCard":
                data.WorldsData["HollowBastion"].VisitLocks -= addRemove;
                break;
            case "NaminesSketches":
                data.WorldsData["SimulatedTwilightTown"].VisitLocks -= addRemove;
                break;
            case "DisneyCastleKey":
                data.WorldsData["DisneyCastle"].VisitLocks -= addRemove;
                break;
            case "WayToTheDawn":
                data.WorldsData["TWTNW"].VisitLocks -= addRemove;
                break;
            default:
                return;
        }

        window.VisitLockCheck();
    }
}
