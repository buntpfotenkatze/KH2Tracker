using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Linq;
using System.Collections.Generic;

namespace KhTracker;

public partial class MainWindow
{
    ///
    /// Helpers
    ///

    private void HandleItemToggle(bool toggle, Item button, bool init)
    {
        switch (toggle)
        {
            case true when button.IsEnabled == false:
            {
                button.IsEnabled = true;
                button.Visibility = Visibility.Visible;
                if (!init)
                {
                    SetTotal(true);
                }

                break;
            }
            case false when button.IsEnabled:
                button.IsEnabled = false;
                button.Visibility = Visibility.Hidden;
                SetTotal(false);

                button.HandleItemReturn();
                break;
        }
    }

    private void HandleWorldToggle(bool toggle, Button button, UniformGrid grid)
    {
        switch (toggle)
        {
            //make grid visible
            case true when !button.IsEnabled:
            {
                var outerGrid = ((button.Parent as Grid)!.Parent as Grid)!.Parent as Grid;
                var row = (int)
                    ((button.Parent as Grid)!.Parent as Grid)!.GetValue(Grid.RowProperty);
                outerGrid!.RowDefinitions[row].Height = new GridLength(1, GridUnitType.Star);
                button.IsEnabled = true;
                button.Visibility = Visibility.Visible;
                break;
            }
            case false when button.IsEnabled:
            {
                //if world was previously selected, unselect it and reset highlighted visual
                if (Data.Selected == button)
                {
                    foreach (
                        var box in Data.WorldsData[button.Name].Top.Children.OfType<Rectangle>()
                    )
                    {
                        if (Math.Abs(box.Opacity - 0.9) > 0.0001 && !box.Name.EndsWith("SelWG"))
                            box.Fill = (SolidColorBrush)FindResource("DefaultRec");

                        if (box.Name.EndsWith("SelWG"))
                            box.Visibility = Visibility.Collapsed;
                    }
                    Data.Selected = null;
                }

                //return all items in world to itempool
                for (var i = grid.Children.Count - 1; i >= 0; --i)
                {
                    var item = grid.Children[i] as Item;
                    item!.HandleItemReturn();
                }

                //resize grid and collapse it
                var outerGrid = ((button.Parent as Grid)!.Parent as Grid)!.Parent as Grid;
                var row = (int)
                    ((button.Parent as Grid)!.Parent as Grid)!.GetValue(Grid.RowProperty);
                outerGrid!.RowDefinitions[row].Height = new GridLength(0, GridUnitType.Star);
                button.IsEnabled = false;
                button.Visibility = Visibility.Collapsed;
                break;
            }
        }
    }

    ///
    /// Options
    ///

    private void AutoSaveProgressToggle(object sender, RoutedEventArgs e)
    {
        Properties.Settings.Default.AutoSaveProgress = AutoSaveProgressOption.IsChecked;
    }

    private void AutoSaveProgress2Toggle(object sender, RoutedEventArgs e)
    {
        Properties.Settings.Default.AutoSaveProgress2 = AutoSaveProgress2Option.IsChecked;
    }

    private void TopMostToggle(object sender, RoutedEventArgs e)
    {
        Properties.Settings.Default.TopMost = TopMostOption.IsChecked;
        Topmost = TopMostOption.IsChecked;
    }

    private void DragDropToggle(object sender, RoutedEventArgs e)
    {
        Properties.Settings.Default.DragDrop = DragAndDropOption.IsChecked;
        Data.DragDrop = DragAndDropOption.IsChecked;

        var itempools = new List<Grid>();
        foreach (Grid pool in ItemPool.Children)
        {
            itempools.Add(pool);
        }

        foreach (var key in Data.Items.Keys)
        {
            var item = Data.Items[key].Item1;
            if (!itempools.Contains(item.Parent))
                continue;

            if (Data.DragDrop == false)
            {
                item.MouseDoubleClick -= item.Item_Click;
                item.MouseMove -= item.Item_MouseMove;

                item.MouseDown -= item.Item_MouseDown;
                item.MouseDown += item.Item_MouseDown;
                item.MouseUp -= item.Item_MouseUp;
                item.MouseUp += item.Item_MouseUp;
            }
            else
            {
                item.MouseDoubleClick -= item.Item_Click;
                item.MouseDoubleClick += item.Item_Click;
                item.MouseMove -= item.Item_MouseMove;
                item.MouseMove += item.Item_MouseMove;

                item.MouseDown -= item.Item_MouseDown;
                item.MouseUp -= item.Item_MouseUp;
            }
        }
    }

    private void AutoConnectToggle(object sender, RoutedEventArgs e)
    {
        Properties.Settings.Default.AutoDetect = AutoConnectOption.IsChecked;
    }

    //private void AutoDetectToggle(object sender, RoutedEventArgs e)
    //{
    //    Properties.Settings.Default.AutoDetect = AutoDetectOption.IsChecked;
    //
    //    if (AutoDetectOption.IsChecked)
    //    {
    //        Connect.Visibility = Visibility.Visible;
    //        Connect2.Visibility = Visibility.Collapsed;
    //        SetAutoDetectTimer();
    //
    //        SettingRow.Height = new GridLength(0.5, GridUnitType.Star);
    //
    //        TrackPCSX2Toggle.IsEnabled = false;
    //        TrackPCToggle.IsEnabled = false;
    //    }
    //    else
    //    {
    //        Connect.Visibility = Visibility.Collapsed;
    //        Connect2.Visibility = Visibility.Collapsed;
    //
    //        if (SettingsText.Text == "Settings:")
    //            SettingRow.Height = new GridLength(0.5, GridUnitType.Star);
    //        else
    //            SettingRow.Height = new GridLength(0, GridUnitType.Star);
    //
    //        TrackPCSX2Toggle.IsEnabled = true;
    //        TrackPCToggle.IsEnabled = true;
    //    }
    //}

    ///
    /// Toggles
    ///

    private void PromiseCharmToggle(object sender, RoutedEventArgs e)
    {
        PromiseCharmToggle(PromiseCharmOption.IsChecked);
    }

    private void PromiseCharmToggle(bool toggle)
    {
        Properties.Settings.Default.PromiseCharm = toggle;
        PromiseCharmOption.IsChecked = toggle;
        PromiseCharmCol.Width = toggle
            ? new GridLength(1.0, GridUnitType.Star)
            : new GridLength(0, GridUnitType.Star);

        HandleItemToggle(toggle, PromiseCharm, false);
    }

    private void AbilitiesToggle(object sender, RoutedEventArgs e)
    {
        AbilitiesToggle(AbilitiesOption.IsChecked);
    }

    private void AbilitiesToggle(bool toggle)
    {
        Properties.Settings.Default.Abilities = toggle;
        AbilitiesOption.IsChecked = toggle;
        if (toggle)
        {
            OnceMoreCol.Width = new GridLength(1.0, GridUnitType.Star);
            SecondChanceCol.Width = new GridLength(1.0, GridUnitType.Star);
        }
        else
        {
            OnceMoreCol.Width = new GridLength(0, GridUnitType.Star);
            SecondChanceCol.Width = new GridLength(0, GridUnitType.Star);
        }

        HandleItemToggle(toggle, OnceMore, false);
        HandleItemToggle(toggle, SecondChance, false);
    }

    private void AntiFormToggle(object sender, RoutedEventArgs e)
    {
        AntiFormToggle(AntiFormOption.IsChecked);
    }

    private void AntiFormToggle(bool toggle)
    {
        Properties.Settings.Default.AntiForm = toggle;
        AntiFormOption.IsChecked = toggle;

        AntiFormCol.Width = toggle
            ? new GridLength(1.0, GridUnitType.Star)
            : new GridLength(0, GridUnitType.Star);

        HandleItemToggle(toggle, Anti, false);
    }

    private void VisitLockToggle(object sender, RoutedEventArgs e)
    {
        VisitLockToggle(VisitLockOption.IsChecked);
    }

    private void VisitLockToggle(bool toggle)
    {
        Properties.Settings.Default.WorldVisitLock = toggle;
        VisitLockOption.IsChecked = toggle;

        var value = 0;

        if (toggle)
        {
            value = 1;

            SMulanWep.Visibility = Visibility.Visible;
            SAuronWep.Visibility = Visibility.Visible;
            SBeastWep.Visibility = Visibility.Visible;
            SJackWep.Visibility = Visibility.Visible;
            SIceCream.Visibility = Visibility.Visible;
            STronWep.Visibility = Visibility.Visible;
            SPicture.Visibility = Visibility.Visible;
            SMembershipCard.Visibility = Visibility.Visible;
            SSimbaWep.Visibility = Visibility.Visible;
            SAladdinWep.Visibility = Visibility.Visible;
            SSparrowWep.Visibility = Visibility.Visible;

            Cross01.Visibility = Visibility.Collapsed;
            Cross02.Visibility = Visibility.Collapsed;
            Cross03.Visibility = Visibility.Collapsed;
            Cross04.Visibility = Visibility.Collapsed;
            Cross05.Visibility = Visibility.Collapsed;
            Cross06.Visibility = Visibility.Collapsed;
            Cross07.Visibility = Visibility.Collapsed;
            Cross08.Visibility = Visibility.Collapsed;
            Cross09.Visibility = Visibility.Collapsed;
            Cross10.Visibility = Visibility.Collapsed;
            Cross11.Visibility = Visibility.Collapsed;

            VisitsRow.Height = new GridLength(1, GridUnitType.Star);
        }
        else
        {
            SMulanWep.Visibility = Visibility.Collapsed;
            SAuronWep.Visibility = Visibility.Collapsed;
            SBeastWep.Visibility = Visibility.Collapsed;
            SJackWep.Visibility = Visibility.Collapsed;
            SIceCream.Visibility = Visibility.Collapsed;
            STronWep.Visibility = Visibility.Collapsed;
            SPicture.Visibility = Visibility.Collapsed;
            SMembershipCard.Visibility = Visibility.Collapsed;
            SSimbaWep.Visibility = Visibility.Collapsed;
            SAladdinWep.Visibility = Visibility.Collapsed;
            SSparrowWep.Visibility = Visibility.Collapsed;

            Cross01.Visibility = Visibility.Visible;
            Cross02.Visibility = Visibility.Visible;
            Cross03.Visibility = Visibility.Visible;
            Cross04.Visibility = Visibility.Visible;
            Cross05.Visibility = Visibility.Visible;
            Cross06.Visibility = Visibility.Visible;
            Cross07.Visibility = Visibility.Visible;
            Cross08.Visibility = Visibility.Visible;
            Cross09.Visibility = Visibility.Visible;
            Cross10.Visibility = Visibility.Visible;
            Cross11.Visibility = Visibility.Visible;

            VisitsRow.Height = ExtraChecksOption.IsChecked
                ? new GridLength(1, GridUnitType.Star)
                : new GridLength(0, GridUnitType.Star);
        }

        //set lock values
        Data.WorldsData["TwilightTown"].VisitLocks = value * 11;
        Data.WorldsData["HollowBastion"].VisitLocks = value;
        Data.WorldsData["BeastsCastle"].VisitLocks = value;
        Data.WorldsData["OlympusColiseum"].VisitLocks = value;
        Data.WorldsData["Agrabah"].VisitLocks = value;
        Data.WorldsData["LandofDragons"].VisitLocks = value;
        Data.WorldsData["PrideLands"].VisitLocks = value;
        Data.WorldsData["HalloweenTown"].VisitLocks = value;
        Data.WorldsData["PortRoyal"].VisitLocks = value;
        Data.WorldsData["SpaceParanoids"].VisitLocks = value;

        foreach (var visitLock in Data.VisitLocks)
        {
            HandleItemToggle(toggle, visitLock, false);
        }

        VisitLockCheck();
    }

    private void TornPagesToggle(object sender, RoutedEventArgs e)
    {
        TornPagesToggle(TornPagesOption.IsChecked);
    }

    private void TornPagesToggle(bool toggle)
    {
        Properties.Settings.Default.TornPages = toggle;
        TornPagesOption.IsChecked = toggle;

        if (toggle)
        {
            PageSep.Width = new GridLength(0.1, GridUnitType.Star);
            PageNum.Width = new GridLength(0.6, GridUnitType.Star);
            PageImg.Width = new GridLength(1.0, GridUnitType.Star);
        }
        else
        {
            PageSep.Width = new GridLength(0, GridUnitType.Star);
            PageNum.Width = new GridLength(0, GridUnitType.Star);
            PageImg.Width = new GridLength(0, GridUnitType.Star);
        }

        HandleItemToggle(toggle, TornPage1, false);
        HandleItemToggle(toggle, TornPage2, false);
        HandleItemToggle(toggle, TornPage3, false);
        HandleItemToggle(toggle, TornPage4, false);
        HandleItemToggle(toggle, TornPage5, false);
    }

    private void ExtraChecksToggle(object sender, RoutedEventArgs e)
    {
        ExtraChecksToggle(ExtraChecksOption.IsChecked);
    }

    private void ExtraChecksToggle(bool toggle)
    {
        Properties.Settings.Default.ExtraChecks = toggle;
        ExtraChecksOption.IsChecked = toggle;

        if (toggle)
        {
            HadesCupCol.Width = new GridLength(1.0, GridUnitType.Star);
            OlympusStoneCol.Width = new GridLength(1.0, GridUnitType.Star);
            UnknownDiskCol.Width = new GridLength(1.0, GridUnitType.Star);

            MunnySep.Width = new GridLength(0.1, GridUnitType.Star);
            MunnyNum.Width = new GridLength(0.6, GridUnitType.Star);
            MunnyImg.Width = new GridLength(1.0, GridUnitType.Star);

            VisitsRow.Height = new GridLength(1, GridUnitType.Star);
        }
        else
        {
            HadesCupCol.Width = new GridLength(0, GridUnitType.Star);
            OlympusStoneCol.Width = new GridLength(0, GridUnitType.Star);
            UnknownDiskCol.Width = new GridLength(0, GridUnitType.Star);

            MunnySep.Width = new GridLength(0, GridUnitType.Star);
            MunnyNum.Width = new GridLength(0, GridUnitType.Star);
            MunnyImg.Width = new GridLength(0, GridUnitType.Star);

            if (VisitLockOption.IsChecked)
                VisitsRow.Height = new GridLength(1, GridUnitType.Star);
            else
                VisitsRow.Height = new GridLength(0, GridUnitType.Star);
        }

        HandleItemToggle(toggle, HadesCup, false);
        HandleItemToggle(toggle, OlympusStone, false);
        HandleItemToggle(toggle, UnknownDisk, false);
        HandleItemToggle(toggle, MunnyPouch1, false);
        HandleItemToggle(toggle, MunnyPouch2, false);
    }

    private void SeedHashToggle(object sender, RoutedEventArgs e)
    {
        SeedHashToggle(SeedHashOption.IsChecked);
    }

    private void SeedHashToggle(bool toggle)
    {
        Properties.Settings.Default.SeedHash = toggle;
        SeedHashOption.IsChecked = toggle;

        if (toggle)
        {
            HintTextBegin.Text = "";
            HintTextMiddle.Text = "";
            HintTextEnd.Text = "";
        }

        if (Data.SeedHashLoaded && toggle)
            HashGrid.Visibility = Visibility.Visible;
        else
            HashGrid.Visibility = Visibility.Collapsed;
    }

    private void WorldProgressToggle(object sender, RoutedEventArgs e)
    {
        Properties.Settings.Default.WorldProgress = WorldProgressOption.IsChecked;
        if (WorldProgressOption.IsChecked)
        {
            foreach (var key in Data.WorldsData.Keys.ToList())
            {
                if (Data.WorldsData[key].Progression != null)
                    Data.WorldsData[key].Progression.Visibility = Visibility.Visible;
            }
        }
        else
        {
            foreach (var key in Data.WorldsData.Keys.ToList())
            {
                if (Data.WorldsData[key].Progression != null)
                    Data.WorldsData[key].Progression.Visibility = Visibility.Hidden;
            }
        }
    }

    private void FormsGrowthToggle(object sender, RoutedEventArgs e)
    {
        Properties.Settings.Default.FormsGrowth = FormsGrowthOption.IsChecked;

        if (FormsGrowthOption.IsChecked && _aTimer != null)
            FormRow.Height = new GridLength(0.5, GridUnitType.Star);
        else
            FormRow.Height = new GridLength(0, GridUnitType.Star);
    }

    private void NextLevelCheckToggle(object sender, RoutedEventArgs e)
    {
        NextLevelCheckToggle(NextLevelCheckOption.IsChecked);
    }

    private void NextLevelCheckToggle(bool toggle)
    {
        Properties.Settings.Default.NextLevelCheck = toggle;
        NextLevelCheckOption.IsChecked = toggle;

        NextLevelDisplay();
    }

    private void NextLevelDisplay()
    {
        var visible = NextLevelCheckOption.IsChecked;

        //default
        var levelsetting = 1;

        if (SoraLevel50Option.IsChecked)
            levelsetting = 50;

        if (SoraLevel99Option.IsChecked)
            levelsetting = 99;

        if (memory != null && stats != null)
        {
            try
            {
                stats.SetMaxLevelCheck(levelsetting);
                stats.SetNextLevelCheck(stats.Level);
            }
            catch
            {
                Console.WriteLine(@"Tried to edit while loading");
            }
        }

        if (levelsetting == 1 || Level.Visibility != Visibility.Visible)
        {
            NextLevelCol.Width = new GridLength(0, GridUnitType.Star);
            return;
        }

        if (visible && memory != null)
        {
            NextLevelCol.Width = new GridLength(0.8, GridUnitType.Star);
        }
        else
        {
            NextLevelCol.Width = new GridLength(0, GridUnitType.Star);
        }
    }

    private void DeathCounterToggle(object sender, RoutedEventArgs e)
    {
        DeathCounterToggle(DeathCounterOption.IsChecked);
    }

    private void DeathCounterToggle(bool toggle)
    {
        Properties.Settings.Default.DeathCounter = toggle;
        DeathCounterOption.IsChecked = toggle;

        DeathCounterDisplay();
    }

    private void DeathCounterDisplay()
    {
        if (DeathCounterOption.IsChecked && memory != null)
        {
            DeathCounterGrid.Visibility = Visibility.Visible;
            DeathCol.Width = new GridLength(0.2, GridUnitType.Star);

            HashSpacer1.Width = new GridLength(0.00, GridUnitType.Star);
            HashSpacer2.Width = new GridLength(1.75, GridUnitType.Star);
        }
        else
        {
            DeathCounterGrid.Visibility = Visibility.Collapsed;
            DeathCol.Width = new GridLength(0, GridUnitType.Star);

            HashSpacer1.Width = new GridLength(0.75, GridUnitType.Star);
            HashSpacer2.Width = new GridLength(0.75, GridUnitType.Star);
        }
    }

    private void EmitProofKeystrokeToggle(object sender, RoutedEventArgs e)
    {
        EmitProofKeystrokeToggle(EmitProofKeystrokeOption.IsChecked);
    }

    private void EmitProofKeystrokeToggle(bool toggle)
    {
        Properties.Settings.Default.EmitProofKeystroke = toggle;
        EmitProofKeystrokeOption.IsChecked = toggle;
    }

    private void SoraLevel01Toggle(object sender, RoutedEventArgs e)
    {
        SoraLevel01Toggle(SoraLevel01Option.IsChecked);
    }

    private void SoraLevel01Toggle(bool toggle)
    {
        //mimic radio button
        if (SoraLevel01Option.IsChecked == false)
        {
            SoraLevel01Option.IsChecked = true;
            //return;
        }
        SoraLevel50Option.IsChecked = false;
        SoraLevel99Option.IsChecked = false;
        Properties.Settings.Default.WorldLevel50 = SoraLevel50Option.IsChecked;
        Properties.Settings.Default.WorldLevel99 = SoraLevel99Option.IsChecked;
        Properties.Settings.Default.WorldLevel1 = toggle;

        NextLevelDisplay();
    }

    private void SoraLevel50Toggle(object sender, RoutedEventArgs e)
    {
        SoraLevel50Toggle(SoraLevel50Option.IsChecked);
    }

    private void SoraLevel50Toggle(bool toggle)
    {
        //mimic radio button
        if (SoraLevel50Option.IsChecked == false)
        {
            SoraLevel50Option.IsChecked = true;
            //return;
        }
        SoraLevel01Option.IsChecked = false;
        SoraLevel99Option.IsChecked = false;
        Properties.Settings.Default.WorldLevel1 = SoraLevel50Option.IsChecked;
        Properties.Settings.Default.WorldLevel99 = SoraLevel99Option.IsChecked;
        Properties.Settings.Default.WorldLevel50 = toggle;

        NextLevelDisplay();
    }

    private void SoraLevel99Toggle(object sender, RoutedEventArgs e)
    {
        SoraLevel99Toggle(SoraLevel99Option.IsChecked);
    }

    private void SoraLevel99Toggle(bool toggle)
    {
        //mimic radio button
        if (SoraLevel99Option.IsChecked == false)
        {
            SoraLevel99Option.IsChecked = true;
            //return;
        }
        SoraLevel50Option.IsChecked = false;
        SoraLevel01Option.IsChecked = false;
        Properties.Settings.Default.WorldLevel50 = SoraLevel50Option.IsChecked;
        Properties.Settings.Default.WorldLevel1 = SoraLevel01Option.IsChecked;
        Properties.Settings.Default.WorldLevel99 = toggle;

        NextLevelDisplay();
    }

    ///
    /// Visual
    ///

    private void WorldHighlightToggle(object sender, RoutedEventArgs e)
    {
        Properties.Settings.Default.WorldHighlight = WorldHighlightOption.IsChecked;

        if (WorldHighlightOption.IsChecked && Data.Selected != null) //set previousl selected world to default colors
        {
            foreach (
                var box in Data.WorldsData[Data.Selected.Name].Top.Children.OfType<Rectangle>()
            )
            {
                if (box.Name.EndsWith("SelWG"))
                    box.Visibility = Visibility.Collapsed;
            }
        }
    }

    private void NewWorldLayoutToggle(object sender, RoutedEventArgs e)
    {
        // Mimicing radio buttons so you cant toggle a button off
        if (NewWorldLayoutOption.IsChecked == false)
        {
            NewWorldLayoutOption.IsChecked = true;
            return;
        }

        OldWorldLayoutOption.IsChecked = false;
        Properties.Settings.Default.NewWorldLayout = NewWorldLayoutOption.IsChecked;
        Properties.Settings.Default.OldWorldLayout = OldWorldLayoutOption.IsChecked;

        if (!NewWorldLayoutOption.IsChecked)
            return;

        var tempWorldState = new bool[19];
        tempWorldState[0] = Properties.Settings.Default.SoraHeart;
        tempWorldState[1] = Properties.Settings.Default.Drives;
        tempWorldState[2] = Properties.Settings.Default.Simulated;
        tempWorldState[3] = Properties.Settings.Default.TwilightTown;
        tempWorldState[4] = Properties.Settings.Default.HollowBastion;
        tempWorldState[5] = Properties.Settings.Default.BeastCastle;
        tempWorldState[6] = Properties.Settings.Default.Olympus;
        tempWorldState[7] = Properties.Settings.Default.Agrabah;
        tempWorldState[8] = Properties.Settings.Default.LandofDragons;
        tempWorldState[9] = Properties.Settings.Default.DisneyCastle;
        tempWorldState[10] = Properties.Settings.Default.PrideLands;
        tempWorldState[11] = Properties.Settings.Default.PortRoyal;
        tempWorldState[12] = Properties.Settings.Default.HalloweenTown;
        tempWorldState[13] = Properties.Settings.Default.SpaceParanoids;
        tempWorldState[14] = Properties.Settings.Default.TWTNW;
        tempWorldState[15] = Properties.Settings.Default.HundredAcre;
        tempWorldState[16] = Properties.Settings.Default.Atlantica;
        tempWorldState[17] = Properties.Settings.Default.Puzzle;
        tempWorldState[18] = Properties.Settings.Default.Synth;

        ReloadWorlds(null);

        WorldsLeft.Children.Clear();
        WorldsRight.Children.Clear();

        WorldsLeft.Children.Add(SorasHeartTop);
        Grid.SetRow(SorasHeartTop, 0);
        WorldsLeft.Children.Add(SimulatedTwilightTownTop);
        Grid.SetRow(SimulatedTwilightTownTop, 1);
        WorldsLeft.Children.Add(TwilightTownTop);
        Grid.SetRow(TwilightTownTop, 2);
        WorldsLeft.Children.Add(HollowBastionTop);
        Grid.SetRow(HollowBastionTop, 3);
        WorldsLeft.Children.Add(LandofDragonsTop);
        Grid.SetRow(LandofDragonsTop, 4);
        WorldsLeft.Children.Add(BeastsCastleTop);
        Grid.SetRow(BeastsCastleTop, 5);
        WorldsLeft.Children.Add(OlympusColiseumTop);
        Grid.SetRow(OlympusColiseumTop, 6);
        WorldsLeft.Children.Add(DisneyCastleTop);
        Grid.SetRow(DisneyCastleTop, 7);
        WorldsLeft.Children.Add(GoATop);
        Grid.SetRow(GoATop, 8);

        WorldsRight.Children.Add(DriveFormsTop);
        Grid.SetRow(DriveFormsTop, 0);
        WorldsRight.Children.Add(PortRoyalTop);
        Grid.SetRow(PortRoyalTop, 1);
        WorldsRight.Children.Add(AgrabahTop);
        Grid.SetRow(AgrabahTop, 2);
        WorldsRight.Children.Add(HalloweenTownTop);
        Grid.SetRow(HalloweenTownTop, 3);
        WorldsRight.Children.Add(PrideLandsTop);
        Grid.SetRow(PrideLandsTop, 4);
        WorldsRight.Children.Add(SpaceParanoidsTop);
        Grid.SetRow(SpaceParanoidsTop, 5);
        WorldsRight.Children.Add(TwtnwTop);
        Grid.SetRow(TwtnwTop, 6);
        WorldsRight.Children.Add(HundredAcreWoodTop);
        Grid.SetRow(HundredAcreWoodTop, 7);
        WorldsRight.Children.Add(AtlanticaTop);
        Grid.SetRow(AtlanticaTop, 8);
        WorldsRight.Children.Add(PuzzSynthTop);
        Grid.SetRow(PuzzSynthTop, 9);

        ReloadWorlds(tempWorldState);
    }

    private void OldWorldLayoutToggle(object sender, RoutedEventArgs e)
    {
        // Mimicing radio buttons so you cant toggle a button off
        if (OldWorldLayoutOption.IsChecked == false)
        {
            OldWorldLayoutOption.IsChecked = true;
            return;
        }

        NewWorldLayoutOption.IsChecked = false;
        Properties.Settings.Default.NewWorldLayout = NewWorldLayoutOption.IsChecked;
        Properties.Settings.Default.OldWorldLayout = OldWorldLayoutOption.IsChecked;

        if (!OldWorldLayoutOption.IsChecked)
            return;

        var tempWorldState = new bool[19];
        tempWorldState[0] = Properties.Settings.Default.SoraHeart;
        tempWorldState[1] = Properties.Settings.Default.Drives;
        tempWorldState[2] = Properties.Settings.Default.Simulated;
        tempWorldState[3] = Properties.Settings.Default.TwilightTown;
        tempWorldState[4] = Properties.Settings.Default.HollowBastion;
        tempWorldState[5] = Properties.Settings.Default.BeastCastle;
        tempWorldState[6] = Properties.Settings.Default.Olympus;
        tempWorldState[7] = Properties.Settings.Default.Agrabah;
        tempWorldState[8] = Properties.Settings.Default.LandofDragons;
        tempWorldState[9] = Properties.Settings.Default.DisneyCastle;
        tempWorldState[10] = Properties.Settings.Default.PrideLands;
        tempWorldState[11] = Properties.Settings.Default.PortRoyal;
        tempWorldState[12] = Properties.Settings.Default.HalloweenTown;
        tempWorldState[13] = Properties.Settings.Default.SpaceParanoids;
        tempWorldState[14] = Properties.Settings.Default.TWTNW;
        tempWorldState[15] = Properties.Settings.Default.HundredAcre;
        tempWorldState[16] = Properties.Settings.Default.Atlantica;
        tempWorldState[17] = Properties.Settings.Default.Puzzle;
        tempWorldState[18] = Properties.Settings.Default.Synth;

        ReloadWorlds(null);

        WorldsLeft.Children.Clear();
        WorldsRight.Children.Clear();

        WorldsLeft.Children.Add(SorasHeartTop);
        Grid.SetRow(SorasHeartTop, 0);
        WorldsLeft.Children.Add(SimulatedTwilightTownTop);
        Grid.SetRow(SimulatedTwilightTownTop, 1);
        WorldsLeft.Children.Add(HollowBastionTop);
        Grid.SetRow(HollowBastionTop, 2);
        WorldsLeft.Children.Add(OlympusColiseumTop);
        Grid.SetRow(OlympusColiseumTop, 3);
        WorldsLeft.Children.Add(LandofDragonsTop);
        Grid.SetRow(LandofDragonsTop, 4);
        WorldsLeft.Children.Add(PrideLandsTop);
        Grid.SetRow(PrideLandsTop, 5);
        WorldsLeft.Children.Add(HalloweenTownTop);
        Grid.SetRow(HalloweenTownTop, 6);
        WorldsLeft.Children.Add(SpaceParanoidsTop);
        Grid.SetRow(SpaceParanoidsTop, 7);
        WorldsLeft.Children.Add(GoATop);
        Grid.SetRow(GoATop, 8);

        WorldsRight.Children.Add(DriveFormsTop);
        Grid.SetRow(DriveFormsTop, 0);
        WorldsRight.Children.Add(TwilightTownTop);
        Grid.SetRow(TwilightTownTop, 1);
        WorldsRight.Children.Add(BeastsCastleTop);
        Grid.SetRow(BeastsCastleTop, 2);
        WorldsRight.Children.Add(AgrabahTop);
        Grid.SetRow(AgrabahTop, 3);
        WorldsRight.Children.Add(HundredAcreWoodTop);
        Grid.SetRow(HundredAcreWoodTop, 4);
        WorldsRight.Children.Add(DisneyCastleTop);
        Grid.SetRow(DisneyCastleTop, 5);
        WorldsRight.Children.Add(PortRoyalTop);
        Grid.SetRow(PortRoyalTop, 6);
        WorldsRight.Children.Add(TwtnwTop);
        Grid.SetRow(TwtnwTop, 7);
        WorldsRight.Children.Add(AtlanticaTop);
        Grid.SetRow(AtlanticaTop, 8);
        WorldsRight.Children.Add(PuzzSynthTop);
        Grid.SetRow(PuzzSynthTop, 9);

        ReloadWorlds(tempWorldState);
    }

    private void ReloadWorlds(IReadOnlyList<bool> stateArr)
    {
        if (stateArr == null)
        {
            SoraHeartToggle(true);
            DrivesToggle(true);
            SimulatedToggle(true);
            TwilightTownToggle(true);
            HollowBastionToggle(true);
            BeastCastleToggle(true);
            OlympusToggle(true);
            AgrabahToggle(true);
            LandofDragonsToggle(true);
            DisneyCastleToggle(true);
            PrideLandsToggle(true);
            PortRoyalToggle(true);
            HalloweenTownToggle(true);
            SpaceParanoidsToggle(true);
            TwtnwToggle(true);
            HundredAcreWoodToggle(true);
            AtlanticaToggle(true);
            PuzzleToggle(true);
            SynthToggle(true);
        }
        else
        {
            SoraHeartToggle(stateArr[0]);
            DrivesToggle(stateArr[1]);
            SimulatedToggle(stateArr[2]);
            TwilightTownToggle(stateArr[3]);
            HollowBastionToggle(stateArr[4]);
            BeastCastleToggle(stateArr[5]);
            OlympusToggle(stateArr[6]);
            AgrabahToggle(stateArr[7]);
            LandofDragonsToggle(stateArr[8]);
            DisneyCastleToggle(stateArr[9]);
            PrideLandsToggle(stateArr[10]);
            PortRoyalToggle(stateArr[11]);
            HalloweenTownToggle(stateArr[12]);
            SpaceParanoidsToggle(stateArr[13]);
            TwtnwToggle(stateArr[14]);
            HundredAcreWoodToggle(stateArr[15]);
            AtlanticaToggle(stateArr[16]);
            PuzzleToggle(stateArr[17]);
            SynthToggle(stateArr[18]);
        }
    }

    private void ColorHintToggle(object sender, RoutedEventArgs e)
    {
        Properties.Settings.Default.ColorHints = ColorHintOption.IsChecked;
    }

    private void MinCheckToggle(object sender, RoutedEventArgs e)
    {
        // Mimicing radio buttons so you cant toggle a button off
        if (MinCheckOption.IsChecked == false)
        {
            MinCheckOption.IsChecked = true;
            return;
        }

        OldCheckOption.IsChecked = false;
        Properties.Settings.Default.MinCheck = MinCheckOption.IsChecked;
        Properties.Settings.Default.OldCheck = OldCheckOption.IsChecked;

        SetItemImage();
        CustomChecksCheck();
    }

    private void OldCheckToggle(object sender, RoutedEventArgs e)
    {
        // Mimicing radio buttons so you cant toggle a button off
        if (OldCheckOption.IsChecked == false)
        {
            OldCheckOption.IsChecked = true;
            return;
        }

        MinCheckOption.IsChecked = false;
        Properties.Settings.Default.MinCheck = MinCheckOption.IsChecked;
        Properties.Settings.Default.OldCheck = OldCheckOption.IsChecked;

        SetItemImage();
        CustomChecksCheck();
    }

    private void MinWorldToggle(object sender, RoutedEventArgs e)
    {
        // Mimicing radio buttons so you cant toggle a button off
        if (MinWorldOption.IsChecked == false)
        {
            MinWorldOption.IsChecked = true;
            return;
        }

        OldWorldOption.IsChecked = false;
        Properties.Settings.Default.MinWorld = MinWorldOption.IsChecked;
        Properties.Settings.Default.OldWorld = OldWorldOption.IsChecked;

        SetWorldImage();
        CustomWorldCheck();
    }

    private void OldWorldToggle(object sender, RoutedEventArgs e)
    {
        // Mimicing radio buttons so you cant toggle a button off
        if (OldWorldOption.IsChecked == false)
        {
            OldWorldOption.IsChecked = true;
            return;
        }

        MinWorldOption.IsChecked = false;
        Properties.Settings.Default.MinWorld = MinWorldOption.IsChecked;
        Properties.Settings.Default.OldWorld = OldWorldOption.IsChecked;

        SetWorldImage();
        CustomWorldCheck();
    }

    private void MinProgToggle(object sender, RoutedEventArgs e)
    {
        // Mimicing radio buttons so you cant toggle a button off
        if (MinProgOption.IsChecked == false)
        {
            MinProgOption.IsChecked = true;
            return;
        }

        OldProgOption.IsChecked = false;
        Properties.Settings.Default.MinProg = MinProgOption.IsChecked;
        Properties.Settings.Default.OldProg = OldProgOption.IsChecked;

        SetProgressIcons();
    }

    private void OldProgToggle(object sender, RoutedEventArgs e)
    {
        // Mimicing radio buttons so you cant toggle a button off
        if (OldProgOption.IsChecked == false)
        {
            OldProgOption.IsChecked = true;
            return;
        }

        MinProgOption.IsChecked = false;
        Properties.Settings.Default.MinProg = MinProgOption.IsChecked;
        Properties.Settings.Default.OldProg = OldProgOption.IsChecked;

        SetProgressIcons();
    }

    private void CustomImageToggle(object sender, RoutedEventArgs e)
    {
        Properties.Settings.Default.CustomIcons = CustomFolderOption.IsChecked;

        if (CustomFolderOption.IsChecked)
        {
            CustomChecksCheck();
            CustomWorldCheck();
            SetProgressIcons();
        }
        else
        {
            //reload check icons
            SetItemImage();
            //reload world icons
            SetWorldImage();
        }
    }

    private void SetProgressIcons()
    {
        var oldToggled = Properties.Settings.Default.OldProg;
        var customToggled = Properties.Settings.Default.CustomIcons;
        var prog = "Min-"; //Default
        if (oldToggled)
            prog = "Old-";
        if (customProgFound && customToggled)
            prog = "Cus-";

        foreach (var worldName in Data.WorldsData.Keys.ToList())
        {
            if (worldName is "SorasHeart" or "DriveForms" or "PuzzSynth")
                continue;

            Data.WorldsData[worldName].Progression.SetResourceReference(
                ContentProperty,
                prog + Data.ProgressKeys[worldName][Data.WorldsData[worldName].Progress]
            );
            Data.WorldsData[worldName].Progression.ToolTip = Data.ProgressKeys[worldName + "Desc"][
                Data.WorldsData[worldName].Progress
            ];
        }
    }

    ///
    /// Worlds
    ///

    private void SoraHeartToggle(object sender, RoutedEventArgs e)
    {
        SoraHeartToggle(SoraHeartOption.IsChecked);
    }

    private void SoraHeartToggle(bool toggle)
    {
        Properties.Settings.Default.SoraHeart = toggle;
        SoraHeartOption.IsChecked = toggle;
        HandleWorldToggle(toggle, SorasHeart, SorasHeartGrid);
    }

    private void DrivesToggle(object sender, RoutedEventArgs e)
    {
        DrivesToggle(DrivesOption.IsChecked);
    }

    private void DrivesToggle(bool toggle)
    {
        Properties.Settings.Default.Drives = toggle;
        DrivesOption.IsChecked = toggle;
        HandleWorldToggle(toggle, DriveForms, DriveFormsGrid);
    }

    private void SimulatedToggle(object sender, RoutedEventArgs e)
    {
        SimulatedToggle(SimulatedOption.IsChecked);
    }

    private void SimulatedToggle(bool toggle)
    {
        Properties.Settings.Default.Simulated = toggle;
        SimulatedOption.IsChecked = toggle;
        HandleWorldToggle(toggle, SimulatedTwilightTown, SimulatedTwilightTownGrid);
    }

    private void TwilightTownToggle(object sender, RoutedEventArgs e)
    {
        TwilightTownToggle(TwilightTownOption.IsChecked);
    }

    private void TwilightTownToggle(bool toggle)
    {
        Properties.Settings.Default.TwilightTown = toggle;
        TwilightTownOption.IsChecked = toggle;
        HandleWorldToggle(toggle, TwilightTown, TwilightTownGrid);
    }

    private void HollowBastionToggle(object sender, RoutedEventArgs e)
    {
        HollowBastionToggle(HollowBastionOption.IsChecked);
    }

    private void HollowBastionToggle(bool toggle)
    {
        Properties.Settings.Default.HollowBastion = toggle;
        HollowBastionOption.IsChecked = toggle;
        HandleWorldToggle(toggle, HollowBastion, HollowBastionGrid);
    }

    private void BeastCastleToggle(object sender, RoutedEventArgs e)
    {
        BeastCastleToggle(BeastCastleOption.IsChecked);
    }

    private void BeastCastleToggle(bool toggle)
    {
        Properties.Settings.Default.BeastCastle = toggle;
        BeastCastleOption.IsChecked = toggle;
        HandleWorldToggle(toggle, BeastsCastle, BeastsCastleGrid);
    }

    private void OlympusToggle(object sender, RoutedEventArgs e)
    {
        OlympusToggle(OlympusOption.IsChecked);
    }

    private void OlympusToggle(bool toggle)
    {
        Properties.Settings.Default.Olympus = toggle;
        OlympusOption.IsChecked = toggle;
        HandleWorldToggle(toggle, OlympusColiseum, OlympusColiseumGrid);
    }

    private void AgrabahToggle(object sender, RoutedEventArgs e)
    {
        AgrabahToggle(AgrabahOption.IsChecked);
    }

    private void AgrabahToggle(bool toggle)
    {
        Properties.Settings.Default.Agrabah = toggle;
        AgrabahOption.IsChecked = toggle;
        HandleWorldToggle(toggle, Agrabah, AgrabahGrid);
    }

    private void LandofDragonsToggle(object sender, RoutedEventArgs e)
    {
        LandofDragonsToggle(LandofDragonsOption.IsChecked);
    }

    private void LandofDragonsToggle(bool toggle)
    {
        Properties.Settings.Default.LandofDragons = toggle;
        LandofDragonsOption.IsChecked = toggle;
        HandleWorldToggle(toggle, LandofDragons, LandofDragonsGrid);
    }

    private void DisneyCastleToggle(object sender, RoutedEventArgs e)
    {
        DisneyCastleToggle(DisneyCastleOption.IsChecked);
    }

    private void DisneyCastleToggle(bool toggle)
    {
        Properties.Settings.Default.DisneyCastle = toggle;
        DisneyCastleOption.IsChecked = toggle;
        HandleWorldToggle(toggle, DisneyCastle, DisneyCastleGrid);
    }

    private void PrideLandsToggle(object sender, RoutedEventArgs e)
    {
        PrideLandsToggle(PrideLandsOption.IsChecked);
    }

    private void PrideLandsToggle(bool toggle)
    {
        Properties.Settings.Default.PrideLands = toggle;
        PrideLandsOption.IsChecked = toggle;
        HandleWorldToggle(toggle, PrideLands, PrideLandsGrid);
    }

    private void PortRoyalToggle(object sender, RoutedEventArgs e)
    {
        PortRoyalToggle(PortRoyalOption.IsChecked);
    }

    private void PortRoyalToggle(bool toggle)
    {
        Properties.Settings.Default.PortRoyal = toggle;
        PortRoyalOption.IsChecked = toggle;
        HandleWorldToggle(toggle, PortRoyal, PortRoyalGrid);
    }

    private void HalloweenTownToggle(object sender, RoutedEventArgs e)
    {
        HalloweenTownToggle(HalloweenTownOption.IsChecked);
    }

    private void HalloweenTownToggle(bool toggle)
    {
        Properties.Settings.Default.HalloweenTown = toggle;
        HalloweenTownOption.IsChecked = toggle;
        HandleWorldToggle(toggle, HalloweenTown, HalloweenTownGrid);
    }

    private void SpaceParanoidsToggle(object sender, RoutedEventArgs e)
    {
        SpaceParanoidsToggle(SpaceParanoidsOption.IsChecked);
    }

    private void SpaceParanoidsToggle(bool toggle)
    {
        Properties.Settings.Default.SpaceParanoids = toggle;
        SpaceParanoidsOption.IsChecked = toggle;
        HandleWorldToggle(toggle, SpaceParanoids, SpaceParanoidsGrid);
    }

    private void TwtnwToggle(object sender, RoutedEventArgs e)
    {
        TwtnwToggle(TwtnwOption.IsChecked);
    }

    private void TwtnwToggle(bool toggle)
    {
        Properties.Settings.Default.TWTNW = toggle;
        TwtnwOption.IsChecked = toggle;
        HandleWorldToggle(toggle, Twtnw, TwtnwGrid);
    }

    private void HundredAcreWoodToggle(object sender, RoutedEventArgs e)
    {
        HundredAcreWoodToggle(HundredAcreWoodOption.IsChecked);
    }

    private void HundredAcreWoodToggle(bool toggle)
    {
        Properties.Settings.Default.HundredAcre = toggle;
        HundredAcreWoodOption.IsChecked = toggle;
        HandleWorldToggle(toggle, HundredAcreWood, HundredAcreWoodGrid);
    }

    private void AtlanticaToggle(object sender, RoutedEventArgs e)
    {
        AtlanticaToggle(AtlanticaOption.IsChecked);
    }

    private void AtlanticaToggle(bool toggle)
    {
        Properties.Settings.Default.Atlantica = toggle;
        AtlanticaOption.IsChecked = toggle;
        HandleWorldToggle(toggle, Atlantica, AtlanticaGrid);
    }

    private void SynthToggle(object sender, RoutedEventArgs e)
    {
        SynthToggle(SynthOption.IsChecked);
    }

    private void SynthToggle(bool toggle)
    {
        Properties.Settings.Default.Synth = toggle;
        SynthOption.IsChecked = toggle;

        //Check puzzle state
        var puzzleOn = PuzzleOption.IsChecked;

        //hide if both off
        if (!toggle && !puzzleOn)
        {
            HandleWorldToggle(false, PuzzSynth, PuzzSynthGrid);
        }
        else //check and change display
        {
            if (!puzzleOn) //puzzles wasn't on before so show world
            {
                HandleWorldToggle(true, PuzzSynth, PuzzSynthGrid);
            }
            SetWorldImage();
        }
    }

    private void PuzzleToggle(object sender, RoutedEventArgs e)
    {
        PuzzleToggle(PuzzleOption.IsChecked);
    }

    private void PuzzleToggle(bool toggle)
    {
        Properties.Settings.Default.Puzzle = toggle;
        PuzzleOption.IsChecked = toggle;

        //Check synth state
        var synthOn = SynthOption.IsChecked;

        if (!toggle && !synthOn) //hide if both off
        {
            HandleWorldToggle(false, PuzzSynth, PuzzSynthGrid);
        }
        else //check and change display
        {
            if (!synthOn) //synth wasn't on before so show world
            {
                HandleWorldToggle(true, PuzzSynth, PuzzSynthGrid);
            }
            SetWorldImage();
        }
    }

    //private void LegacyToggle(object sender, RoutedEventArgs e)
    //{
    //    LegacyToggle(LegacyOption.IsChecked);
    //}
    //
    //private void LegacyToggle(bool toggle)
    //{
    //    Properties.Settings.Default.Legacy = toggle;
    //    LegacyOption.IsChecked = toggle;
    //}

    //toggle for Disconnect message
    private void DisconnectToggle(object sender, RoutedEventArgs e)
    {
        DisconnectToggle(Disconnect.IsChecked);
    }

    private void DisconnectToggle(bool toggle)
    {
        Properties.Settings.Default.Disconnect = toggle;
        Disconnect.IsChecked = toggle;

        //logic
    }
}
