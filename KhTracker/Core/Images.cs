using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace KhTracker;

public partial class MainWindow
{
    //dumb stuff to help figure out what to do about custom images
    public static bool CustomSwordFound;
    public static bool CustomStaffFound;
    public static bool CustomShieldFound;
    private bool customLevelFound;
    private bool customStrengthFound;
    private bool customMagicFound;
    private bool customDefenseFound;
    private bool customProgFound;

    //item | dictionary key | shadow | path
    private Dictionary<Item, (string Key, ContentControl Shadow, string Path)> cusItemCheck;

    //handle adding all custom images and such
    private void InitImages()
    {
        //for autodetect (won't bother making this customizable for now)
        Data.AdConnect = new BitmapImage(
            new Uri("Images/System/config/searching.png", UriKind.Relative)
        );
        Data.AdPc = new BitmapImage(
            new Uri("Images/System/config/pc_connected.png", UriKind.Relative)
        );
        Data.AdPCred = new BitmapImage(
            new Uri("Images/System/config/pc_detected.png", UriKind.Relative)
        );
        Data.AdCross = new BitmapImage(new Uri("Images/System/cross.png", UriKind.Relative));

        //check for custom stat and weapon icons
        if (File.Exists("CustomImages/System/stats/sword.png"))
            CustomSwordFound = true;
        if (File.Exists("CustomImages/System/stats/staff.png"))
            CustomStaffFound = true;
        if (File.Exists("CustomImages/System/stats/shield.png"))
            CustomShieldFound = true;
        if (File.Exists("CustomImages/System/stats/level.png"))
            customLevelFound = true;
        if (File.Exists("CustomImages/System/stats/strength.png"))
            customStrengthFound = true;
        if (File.Exists("CustomImages/System/stats/magic.png"))
            customMagicFound = true;
        if (File.Exists("CustomImages/System/stats/defence.png"))
            customDefenseFound = true;

        //check for custom progression icons. if these 4 are there then assume all progression icons are there
        //no reason for these 4 specific ones, i just picked 4 completely unrelated ones at random.
        if (
            File.Exists("CustomImages/Progression/chest.png")
            && File.Exists("CustomImages/Progression/emblem.png")
            && File.Exists("CustomImages/Progression/100_acre_wood/hunny_slider.png")
            && File.Exists("CustomImages/Progression/halloween_town/oogie_boogie.png")
        )
            customProgFound = true;

        //helps determine what item images need replacing with custom image loading
        cusItemCheck = new Dictionary<Item, (string Key, ContentControl Shadow, string Path)>
        {
            { Fire1, ("Cus-Fire1", SFire, "CustomImages/Checks/magic_fire.png") },
            { Fire2, ("Cus-Fire2", null, "CustomImages/Checks/magic_fire.png") },
            { Fire3, ("Cus-Fire3", null, "CustomImages/Checks/magic_fire.png") },
            { Blizzard1, ("Cus-Blizzard1", SBlizzard, "CustomImages/Checks/magic_blizzard.png") },
            { Blizzard2, ("Cus-Blizzard2", null, "CustomImages/Checks/magic_blizzard.png") },
            { Blizzard3, ("Cus-Blizzard3", null, "CustomImages/Checks/magic_blizzard.png") },
            { Thunder1, ("Cus-Thunder1", SThunder, "CustomImages/Checks/magic_thunder.png") },
            { Thunder2, ("Cus-Thunder2", null, "CustomImages/Checks/magic_thunder.png") },
            { Thunder3, ("Cus-Thunder3", null, "CustomImages/Checks/magic_thunder.png") },
            { Cure1, ("Cus-Cure1", SCure, "CustomImages/Checks/magic_cure.png") },
            { Cure2, ("Cus-Cure2", null, "CustomImages/Checks/magic_cure.png") },
            { Cure3, ("Cus-Cure3", null, "CustomImages/Checks/magic_cure.png") },
            { Reflect1, ("Cus-Reflect1", SReflect, "CustomImages/Checks/magic_reflect.png") },
            { Reflect2, ("Cus-Reflect2", null, "CustomImages/Checks/magic_reflect.png") },
            { Reflect3, ("Cus-Reflect3", null, "CustomImages/Checks/magic_reflect.png") },
            { Magnet1, ("Cus-Magnet1", SMagnet, "CustomImages/Checks/magic_magnet.png") },
            { Magnet2, ("Cus-Magnet2", null, "CustomImages/Checks/magic_magnet.png") },
            { Magnet3, ("Cus-Magnet3", null, "CustomImages/Checks/magic_magnet.png") },
            { TornPage1, ("Cus-TornPage1", STornPage, "CustomImages/Checks/torn_pages.png") },
            { TornPage2, ("Cus-TornPage2", null, "CustomImages/Checks/torn_pages.png") },
            { TornPage3, ("Cus-TornPage3", null, "CustomImages/Checks/torn_pages.png") },
            { TornPage4, ("Cus-TornPage4", null, "CustomImages/Checks/torn_pages.png") },
            { TornPage5, ("Cus-TornPage5", null, "CustomImages/Checks/torn_pages.png") },
            { Valor, ("Cus-Valor", SValor, "CustomImages/Checks/form_valor.png") },
            { Wisdom, ("Cus-Wisdom", SWisdom, "CustomImages/Checks/form_wisdom.png") },
            { Limit, ("Cus-Limit", SLimit, "CustomImages/Checks/form_limit.png") },
            { Master, ("Cus-Master", SMaster, "CustomImages/Checks/form_master.png") },
            { Final, ("Cus-Final", SFinal, "CustomImages/Checks/form_final.png") },
            { Lamp, ("Cus-Lamp", SLamp, "CustomImages/Checks/summon_genie.png") },
            { Ukulele, ("Cus-Ukulele", SUkulele, "CustomImages/Checks/summon_stitch.png") },
            {
                Baseball,
                ("Cus-Baseball", SBaseball, "CustomImages/Checks/summon_chicken_little.png")
            },
            { Feather, ("Cus-Feather", SFeather, "CustomImages/Checks/summon_peter_pan.png") },
            {
                Nonexistence,
                ("Cus-Nonexistence", SNonexistence, "CustomImages/Checks/proof_nonexistence.png")
            },
            {
                Connection,
                ("Cus-Connection", SConnection, "CustomImages/Checks/proof_connection.png")
            },
            { Peace, ("Cus-Peace", SPeace, "CustomImages/Checks/proof_peace.png") },
            {
                PromiseCharm,
                ("Cus-PromiseCharm", SPromiseCharm, "CustomImages/Checks/promise_charm.png")
            },
            { OnceMore, ("Cus-OnceMore", SOnceMore, "CustomImages/Checks/once_more.png") },
            {
                SecondChance,
                ("Cus-SecondChance", SSecondChance, "CustomImages/Checks/second_chance.png")
            },
            {
                MulanWep1,
                ("Cus-MulanWep1", SMulanWep, "CustomImages/Checks/lock_AncestorSword.png")
            },
            { MulanWep2, ("Cus-MulanWep2", null, "CustomImages/Checks/lock_AncestorSword.png") },
            {
                AuronWep1,
                ("Cus-AuronWep2", SAuronWep, "CustomImages/Checks/lock_BattlefieldsofWar.png")
            },
            {
                AuronWep2,
                ("Cus-AuronWep2", null, "CustomImages/Checks/lock_BattlefieldsofWar.png")
            },
            { BeastWep1, ("Cus-BeastWep1", SBeastWep, "CustomImages/Checks/lock_BeastClaw.png") },
            { BeastWep2, ("Cus-BeastWep2", null, "CustomImages/Checks/lock_BeastClaw.png") },
            { JackWep1, ("Cus-JackWep1", SJackWep, "CustomImages/Checks/lock_BoneFist.png") },
            { JackWep2, ("Cus-JackWep2", null, "CustomImages/Checks/lock_BoneFist.png") },
            { IceCream1, ("Cus-IceCream1", SIceCream, "CustomImages/Checks/lock_IceCream.png") },
            { IceCream2, ("Cus-IceCream2", null, "CustomImages/Checks/lock_IceCream.png") },
            { IceCream3, ("Cus-IceCream3", null, "CustomImages/Checks/lock_IceCream.png") },
            { TronWep1, ("Cus-TronWep1", STronWep, "CustomImages/Checks/lock_IdentityDisk.png") },
            { TronWep2, ("Cus-TronWep2", null, "CustomImages/Checks/lock_IdentityDisk.png") },
            {
                NaminesSketches,
                (
                    "Cus-NaminesSketches",
                    SNaminesSketches,
                    "CustomImages/Checks/lock_NaminesSketches.png"
                )
            },
            {
                DisneyCastleKey1,
                (
                    "Cus-DisneyCastleKey1",
                    SDisneyCastleKey,
                    "CustomImages/Checks/lock_DisneyCastleKey.png"
                )
            },
            {
                DisneyCastleKey2,
                ("Cus-DisneyCastleKey2", null, "CustomImages/Checks/lock_DisneyCastleKey.png")
            },
            {
                WayToTheDawn,
                ("Cus-WayToTheDawn", SWayToTheDawn, "CustomImages/Checks/lock_WayToTheDawn.png")
            },
            {
                MembershipCard1,
                (
                    "Cus-MembershipCard1",
                    SMembershipCard,
                    "CustomImages/Checks/lock_membershipcard.png"
                )
            },
            {
                MembershipCard2,
                ("Cus-MembershipCard2", null, "CustomImages/Checks/lock_membershipcard.png")
            },
            { SimbaWep1, ("Cus-SimbaWep1", SSimbaWep, "CustomImages/Checks/lock_ProudFang.png") },
            { SimbaWep2, ("Cus-SimbaWep2", null, "CustomImages/Checks/lock_ProudFang.png") },
            {
                AladdinWep1,
                ("Cus-AladdinWep1", SAladdinWep, "CustomImages/Checks/lock_Scimitar.png")
            },
            { AladdinWep2, ("Cus-AladdinWep2", null, "CustomImages/Checks/lock_Scimitar.png") },
            {
                SparrowWep1,
                ("Cus-SparrowWep1", SSparrowWep, "CustomImages/Checks/lock_SkillCrossbones.png")
            },
            {
                SparrowWep2,
                ("Cus-SparrowWep2", null, "CustomImages/Checks/lock_SkillCrossbones.png")
            },
            {
                MunnyPouch1,
                ("Cus-MunnyPouch1", SMunnyPouch, "CustomImages/Checks/aux_munny_pouch.png")
            },
            { MunnyPouch2, ("Cus-MunnyPouch2", null, "CustomImages/Checks/aux_munny_pouch.png") },
            { Anti, ("Cus-Anti", SAnti, "CustomImages/Checks/form_anti.png") }
        };
    }

    private void MainBG_DefToggle(object sender, RoutedEventArgs e)
    {
        // Mimicing radio buttons so you cant toggle a button off
        if (MainDefOption.IsChecked == false)
        {
            MainDefOption.IsChecked = true;
            return;
        }

        MainImg1Option.IsChecked = false;
        MainImg2Option.IsChecked = false;
        MainImg3Option.IsChecked = false;

        App.Settings.MainBG = 0;

        if (MainDefOption.IsChecked)
        {
            Background = Application.Current.Resources["BG-Default"] as SolidColorBrush;
        }
    }

    private void MainBG_Img1Toggle(object sender, RoutedEventArgs e)
    {
        // Mimicing radio buttons so you cant toggle a button off
        if (MainImg1Option.IsChecked == false)
        {
            MainImg1Option.IsChecked = true;
            return;
        }

        MainDefOption.IsChecked = false;
        MainImg2Option.IsChecked = false;
        MainImg3Option.IsChecked = false;

        App.Settings.MainBG = 1;

        if (MainImg1Option.IsChecked)
        {
            if (File.Exists("CustomImages/background.png"))
                Background = Application.Current.Resources["BG-Image1"] as ImageBrush;
            else
                Background = Application.Current.Resources["BG-ImageDef1"] as ImageBrush;
        }
    }

    private void MainBG_Img2Toggle(object sender, RoutedEventArgs e)
    {
        // Mimicing radio buttons so you cant toggle a button off
        if (MainImg2Option.IsChecked == false)
        {
            MainImg2Option.IsChecked = true;
            return;
        }

        MainDefOption.IsChecked = false;
        MainImg1Option.IsChecked = false;
        MainImg3Option.IsChecked = false;

        App.Settings.MainBG = 2;

        if (MainImg2Option.IsChecked)
        {
            if (File.Exists("CustomImages/background.png"))
                Background = Application.Current.Resources["BG-Image2"] as ImageBrush;
            else
                Background = Application.Current.Resources["BG-ImageDef2"] as ImageBrush;
        }
    }

    private void MainBG_Img3Toggle(object sender, RoutedEventArgs e)
    {
        // Mimicing radio buttons so you cant toggle a button off
        if (MainImg3Option.IsChecked == false)
        {
            MainImg3Option.IsChecked = true;
            return;
        }

        MainDefOption.IsChecked = false;
        MainImg1Option.IsChecked = false;
        MainImg2Option.IsChecked = false;

        App.Settings.MainBG = 3;

        if (MainImg3Option.IsChecked)
        {
            if (File.Exists("CustomImages/background.png"))
                Background = Application.Current.Resources["BG-Image3"] as ImageBrush;
            else
                Background = Application.Current.Resources["BG-ImageDef3"] as ImageBrush;
        }
    }

    private void CustomChecksCheck()
    {
        if (!CustomFolderOption.IsChecked)
            return;

        var checkFiles = Array.Empty<string>();

        if (Directory.Exists("CustomImages/Checks/"))
        {
            checkFiles = Directory.GetFiles(
                "CustomImages/Checks/",
                "*.png",
                SearchOption.TopDirectoryOnly
            );
        }

        //if list isn't empty then compare against dictionary to determine what icons to replace

        //    key     |  item1  | item2  | item3
        //main window | dic key | shadow | path

        if (checkFiles.Length > 0)
        {
            //check if i actually need this lowercase edit
            checkFiles = checkFiles.Select(s => s.ToLowerInvariant()).ToArray();

            foreach (var (mainWindow, (key, shadow, path)) in cusItemCheck)
            {
                if (checkFiles.Contains(path.ToLower()))
                {
                    //main item
                    mainWindow.SetResourceReference(ContentProperty, key);

                    //item shadow
                    shadow?.SetResourceReference(ContentProperty, key);
                }
            }
        }

        //check if folders exists then start checking if each file exists in it
        if (Directory.Exists("CustomImages/Checks/"))
        {
            if (File.Exists("CustomImages/System/drive_growth/high_jump.png"))
                HighJump.SetResourceReference(ContentProperty, "Cus-HighJump");
            if (File.Exists("CustomImages/System/drive_growth/quick_run.png"))
                QuickRun.SetResourceReference(ContentProperty, "Cus-QuickRun");
            if (File.Exists("CustomImages/System/drive_growth/dodge_roll.png"))
                DodgeRoll.SetResourceReference(ContentProperty, "Cus-DodgeRoll");
            if (File.Exists("CustomImages/System/drive_growth/aerial_didge.png"))
                AerialDodge.SetResourceReference(ContentProperty, "Cus-AerialDodge");
            if (File.Exists("CustomImages/System/drive_growth/glide.png"))
                Glide.SetResourceReference(ContentProperty, "Cus-Glide");

            if (File.Exists("CustomImages/System/drive_growth/valor.png"))
                ValorM.SetResourceReference(ContentProperty, "Cus-Valor");
            if (File.Exists("CustomImages/System/drive_growth/wisdom.png"))
                WisdomM.SetResourceReference(ContentProperty, "Cus-Wisdom");
            if (File.Exists("CustomImages/System/drive_growth/limit.png"))
                LimitM.SetResourceReference(ContentProperty, "Cus-Limit");
            if (File.Exists("CustomImages/System/drive_growth/master.png"))
                MasterM.SetResourceReference(ContentProperty, "Cus-Master");
            if (File.Exists("CustomImages/System/drive_growth/final.png"))
                FinalM.SetResourceReference(ContentProperty, "Cus-Final");
        }

        if (customLevelFound)
            LevelIcon.SetResourceReference(ContentProperty, "Cus-LevelIcon");

        if (customStrengthFound)
            StrengthIcon.SetResourceReference(ContentProperty, "Cus-StrengthIcon");

        if (customMagicFound)
            MagicIcon.SetResourceReference(ContentProperty, "Cus-MagicIcon");

        if (customDefenseFound)
            DefenseIcon.SetResourceReference(ContentProperty, "Cus-DefenseIcon");

        //visit locks
        if (Directory.Exists("CustomImages/Worlds/Locks/"))
        {
            if (File.Exists("CustomImages/Worlds/Locks/HB.png"))
                HollowBastionLock1.Source = new BitmapImage(
                    new Uri(
                        "pack://application:,,,/CustomImages/Worlds/Locks/HB.png",
                        UriKind.Absolute
                    )
                );
            HollowBastionLock2.Source = new BitmapImage(
                new Uri("pack://application:,,,/CustomImages/Worlds/Locks/HB.png", UriKind.Absolute)
            );
            if (File.Exists("CustomImages/Worlds/Locks/OC.png"))
                OlympusColiseumLock1.Source = new BitmapImage(
                    new Uri(
                        "pack://application:,,,/CustomImages/Worlds/Locks/OC.png",
                        UriKind.Absolute
                    )
                );
            OlympusColiseumLock2.Source = new BitmapImage(
                new Uri("pack://application:,,,/CustomImages/Worlds/Locks/OC.png", UriKind.Absolute)
            );
            if (File.Exists("CustomImages/Worlds/Locks/LD.png"))
                LandofDragonsLock1.Source = new BitmapImage(
                    new Uri(
                        "pack://application:,,,/CustomImages/Worlds/Locks/LD.png",
                        UriKind.Absolute
                    )
                );
            LandofDragonsLock1.Source = new BitmapImage(
                new Uri("pack://application:,,,/CustomImages/Worlds/Locks/LD.png", UriKind.Absolute)
            );
            if (File.Exists("CustomImages/Worlds/Locks/PL.png"))
                PrideLandsLock1.Source = new BitmapImage(
                    new Uri(
                        "pack://application:,,,/CustomImages/Worlds/Locks/PL.png",
                        UriKind.Absolute
                    )
                );
            PrideLandsLock1.Source = new BitmapImage(
                new Uri("pack://application:,,,/CustomImages/Worlds/Locks/PL.png", UriKind.Absolute)
            );
            if (File.Exists("CustomImages/Worlds/Locks/HT.png"))
                HalloweenTownLock1.Source = new BitmapImage(
                    new Uri(
                        "pack://application:,,,/CustomImages/Worlds/Locks/HT.png",
                        UriKind.Absolute
                    )
                );
            HalloweenTownLock1.Source = new BitmapImage(
                new Uri("pack://application:,,,/CustomImages/Worlds/Locks/HT.png", UriKind.Absolute)
            );
            if (File.Exists("CustomImages/Worlds/Locks/SP.png"))
                SpaceParanoidsLock1.Source = new BitmapImage(
                    new Uri(
                        "pack://application:,,,/CustomImages/Worlds/Locks/SP.png",
                        UriKind.Absolute
                    )
                );
            SpaceParanoidsLock1.Source = new BitmapImage(
                new Uri("pack://application:,,,/CustomImages/Worlds/Locks/SP.png", UriKind.Absolute)
            );
            if (File.Exists("CustomImages/Worlds/Locks/BC.png"))
                BeastsCastleLock1.Source = new BitmapImage(
                    new Uri(
                        "pack://application:,,,/CustomImages/Worlds/Locks/BC.png",
                        UriKind.Absolute
                    )
                );
            BeastsCastleLock1.Source = new BitmapImage(
                new Uri("pack://application:,,,/CustomImages/Worlds/Locks/BC.png", UriKind.Absolute)
            );
            if (File.Exists("CustomImages/Worlds/Locks/AG.png"))
                AgrabahLock1.Source = new BitmapImage(
                    new Uri(
                        "pack://application:,,,/CustomImages/Worlds/Locks/AG.png",
                        UriKind.Absolute
                    )
                );
            AgrabahLock1.Source = new BitmapImage(
                new Uri("pack://application:,,,/CustomImages/Worlds/Locks/AG.png", UriKind.Absolute)
            );
            if (File.Exists("CustomImages/Worlds/Locks/PR.png"))
                PortRoyalLock1.Source = new BitmapImage(
                    new Uri(
                        "pack://application:,,,/CustomImages/Worlds/Locks/PR.png",
                        UriKind.Absolute
                    )
                );
            PortRoyalLock1.Source = new BitmapImage(
                new Uri("pack://application:,,,/CustomImages/Worlds/Locks/PR.png", UriKind.Absolute)
            );
            if (File.Exists("CustomImages/Worlds/Locks/TT2.png"))
            {
                SimulatedTwilightTownLock.Source = new BitmapImage(
                    new Uri(
                        "pack://application:,,,/CustomImages/Worlds/Locks/TT2.png",
                        UriKind.Absolute
                    )
                );
                DisneyCastleLock1.Source = new BitmapImage(
                    new Uri(
                        "pack://application:,,,/CustomImages/Worlds/Locks/TT2.png",
                        UriKind.Absolute
                    )
                );
                DisneyCastleLock2.Source = new BitmapImage(
                    new Uri(
                        "pack://application:,,,/CustomImages/Worlds/Locks/TT2.png",
                        UriKind.Absolute
                    )
                );
                TwtnwLock.Source = new BitmapImage(
                    new Uri(
                        "pack://application:,,,/CustomImages/Worlds/Locks/TT2.png",
                        UriKind.Absolute
                    )
                );
            }
            if (File.Exists("CustomImages/Worlds/Locks/TT3.png"))
            {
                TwilightTownLock1.Source = new BitmapImage(
                    new Uri(
                        "pack://application:,,,/CustomImages/Worlds/Locks/TT3.png",
                        UriKind.Absolute
                    )
                );
                TwilightTownLock2.Source = new BitmapImage(
                    new Uri(
                        "pack://application:,,,/CustomImages/Worlds/Locks/TT3.png",
                        UriKind.Absolute
                    )
                );
                TwilightTownLock3.Source = new BitmapImage(
                    new Uri(
                        "pack://application:,,,/CustomImages/Worlds/Locks/TT3.png",
                        UriKind.Absolute
                    )
                );
            }
        }

        //world cross
        if (File.Exists("CustomImages/System/crossworld.png"))
        {
            SorasHeartCross.Source = new BitmapImage(
                new Uri(
                    "pack://application:,,,/CustomImages/System/crossworld.png",
                    UriKind.Absolute
                )
            );
            DriveFormsCross.Source = new BitmapImage(
                new Uri(
                    "pack://application:,,,/CustomImages/System/crossworld.png",
                    UriKind.Absolute
                )
            );
            SimulatedTwilightTownCross.Source = new BitmapImage(
                new Uri(
                    "pack://application:,,,/CustomImages/System/crossworld.png",
                    UriKind.Absolute
                )
            );
            TwilightTownCross.Source = new BitmapImage(
                new Uri(
                    "pack://application:,,,/CustomImages/System/crossworld.png",
                    UriKind.Absolute
                )
            );
            HollowBastionCross.Source = new BitmapImage(
                new Uri(
                    "pack://application:,,,/CustomImages/System/crossworld.png",
                    UriKind.Absolute
                )
            );
            BeastsCastleCross.Source = new BitmapImage(
                new Uri(
                    "pack://application:,,,/CustomImages/System/crossworld.png",
                    UriKind.Absolute
                )
            );
            OlympusColiseumCross.Source = new BitmapImage(
                new Uri(
                    "pack://application:,,,/CustomImages/System/crossworld.png",
                    UriKind.Absolute
                )
            );
            AgrabahCross.Source = new BitmapImage(
                new Uri(
                    "pack://application:,,,/CustomImages/System/crossworld.png",
                    UriKind.Absolute
                )
            );
            LandofDragonsCross.Source = new BitmapImage(
                new Uri(
                    "pack://application:,,,/CustomImages/System/crossworld.png",
                    UriKind.Absolute
                )
            );
            HundredAcreWoodCross.Source = new BitmapImage(
                new Uri(
                    "pack://application:,,,/CustomImages/System/crossworld.png",
                    UriKind.Absolute
                )
            );
            PrideLandsCross.Source = new BitmapImage(
                new Uri(
                    "pack://application:,,,/CustomImages/System/crossworld.png",
                    UriKind.Absolute
                )
            );
            DisneyCastleCross.Source = new BitmapImage(
                new Uri(
                    "pack://application:,,,/CustomImages/System/crossworld.png",
                    UriKind.Absolute
                )
            );
            HalloweenTownCross.Source = new BitmapImage(
                new Uri(
                    "pack://application:,,,/CustomImages/System/crossworld.png",
                    UriKind.Absolute
                )
            );
            PortRoyalCross.Source = new BitmapImage(
                new Uri(
                    "pack://application:,,,/CustomImages/System/crossworld.png",
                    UriKind.Absolute
                )
            );
            TwtnwCross.Source = new BitmapImage(
                new Uri(
                    "pack://application:,,,/CustomImages/System/crossworld.png",
                    UriKind.Absolute
                )
            );
            SpaceParanoidsCross.Source = new BitmapImage(
                new Uri(
                    "pack://application:,,,/CustomImages/System/crossworld.png",
                    UriKind.Absolute
                )
            );
            AtlanticaCross.Source = new BitmapImage(
                new Uri(
                    "pack://application:,,,/CustomImages/System/crossworld.png",
                    UriKind.Absolute
                )
            );
            PuzzSynthCross.Source = new BitmapImage(
                new Uri(
                    "pack://application:,,,/CustomImages/System/crossworld.png",
                    UriKind.Absolute
                )
            );
            GoACross.Source = new BitmapImage(
                new Uri(
                    "pack://application:,,,/CustomImages/System/crossworld.png",
                    UriKind.Absolute
                )
            );
        }

        //DeathCounter counter skull
        if (File.Exists("CustomImages/System/stats/deaths.png"))
            DeathIcon.Source = new BitmapImage(
                new Uri(
                    "pack://application:,,,/CustomImages/System/stats/deaths.png",
                    UriKind.Absolute
                )
            );
    }

    private void CustomWorldCheck()
    {
        if (!CustomFolderOption.IsChecked)
            return;

        ///TODO: update and maybe set up dictionary?
        //Main Window
        if (Directory.Exists("CustomImages/Worlds/"))
        {
            if (File.Exists("CustomImages/Worlds/simulated_twilight_town.png"))
            {
                SimulatedTwilightTown.SetResourceReference(ContentProperty, "Cus-SimulatedImage");
            }
            if (File.Exists("CustomImages/Worlds/land_of_dragons.png"))
            {
                LandofDragons.SetResourceReference(ContentProperty, "Cus-LandofDragonsImage");
            }
            if (File.Exists("CustomImages/Worlds/pride_land.png"))
            {
                PrideLands.SetResourceReference(ContentProperty, "Cus-PrideLandsImage");
            }
            if (File.Exists("CustomImages/Worlds/halloween_town.png"))
            {
                HalloweenTown.SetResourceReference(ContentProperty, "Cus-HalloweenTownImage");
            }
            if (File.Exists("CustomImages/Worlds/space_paranoids.png"))
            {
                SpaceParanoids.SetResourceReference(ContentProperty, "Cus-SpaceParanoidsImage");
            }
            if (File.Exists("CustomImages/Worlds/drive_form.png"))
            {
                DriveForms.SetResourceReference(ContentProperty, "Cus-DriveFormsImage");
            }
            if (File.Exists("CustomImages/Worlds/twilight_town.png"))
            {
                TwilightTown.SetResourceReference(ContentProperty, "Cus-TwilightTownImage");
            }
            if (File.Exists("CustomImages/Worlds/beast's_castle.png"))
            {
                BeastsCastle.SetResourceReference(ContentProperty, "Cus-BeastCastleImage");
            }
            if (File.Exists("CustomImages/Worlds/agrabah.png"))
            {
                Agrabah.SetResourceReference(ContentProperty, "Cus-AgrabahImage");
            }
            if (File.Exists("CustomImages/Worlds/100_acre_wood.png"))
            {
                HundredAcreWood.SetResourceReference(ContentProperty, "Cus-HundredAcreImage");
            }
            if (File.Exists("CustomImages/Worlds/port_royal.png"))
            {
                PortRoyal.SetResourceReference(ContentProperty, "Cus-PortRoyalImage");
            }
            if (File.Exists("CustomImages/Worlds/the_world_that_never_was.png"))
            {
                Twtnw.SetResourceReference(ContentProperty, "Cus-TWTNWImage");
            }
            if (File.Exists("CustomImages/Worlds/atlantica.png"))
            {
                Atlantica.SetResourceReference(ContentProperty, "Cus-AtlanticaImage");
            }
            if (File.Exists("CustomImages/Worlds/replica_data.png"))
            {
                GoA.SetResourceReference(ContentProperty, "Cus-GardenofAssemblageImage");
            }
            if (File.Exists("CustomImages/Worlds/level.png"))
            {
                SorasHeart.SetResourceReference(ContentProperty, "Cus-SoraHeartImage");
            }
            if (File.Exists("CustomImages/Worlds/disney_castle.png"))
            {
                DisneyCastle.SetResourceReference(ContentProperty, "Cus-DisneyCastleImage");
            }
            if (File.Exists("CustomImages/Worlds/hollow_bastion.png"))
            {
                HollowBastion.SetResourceReference(ContentProperty, "Cus-HollowBastionImage");
            }
            if (File.Exists("CustomImages/Worlds/olympus_coliseum.png"))
            {
                OlympusColiseum.SetResourceReference(ContentProperty, "Cus-OlympusImage");
            }

            //check for custom cavern, timeless, and cups toggles
            //if (File.Exists("CustomImages/Worlds/Level01.png") && SoraLevel01Option.IsChecked)
            //{
            //    SorasHeartType.SetResourceReference(ContentProperty, "Cus-SoraLevel01");
            //}
            //if (File.Exists("CustomImages/Worlds/Level50.png") && SoraLevel50Option.IsChecked)
            //{
            //    SorasHeartType.SetResourceReference(ContentProperty, "Cus-SoraLevel50");
            //}
            //if (File.Exists("CustomImages/Worlds/Level99.png") && SoraLevel99Option.IsChecked)
            //{
            //    SorasHeartType.SetResourceReference(ContentProperty, "Cus-SoraLevel99");
            //}

            //puzzle/synth display
            if (
                File.Exists("CustomImages/Worlds/PuzzSynth.png")
                && PuzzleOption.IsChecked
                && SynthOption.IsChecked
            ) //both on
            {
                PuzzSynth.SetResourceReference(ContentProperty, "Cus-PuzzSynth");
            }
            if (
                File.Exists("CustomImages/Worlds/Synth.png")
                && !PuzzleOption.IsChecked
                && SynthOption.IsChecked
            ) //synth on puzzle off
            {
                PuzzSynth.SetResourceReference(ContentProperty, "Cus-PuzzSynth_S");
            }
            if (
                File.Exists("CustomImages/Worlds/Puzzle.png")
                && PuzzleOption.IsChecked
                && !SynthOption.IsChecked
            ) //synth off puzzle on
            {
                PuzzSynth.SetResourceReference(ContentProperty, "Cus-PuzzSynth_P");
            }
        }
    }

    private void SetItemImage()
    {
        LevelIcon.SetResourceReference(ContentProperty, "LevelIcon");
        StrengthIcon.SetResourceReference(ContentProperty, "StrengthIcon");
        MagicIcon.SetResourceReference(ContentProperty, "MagicIcon");
        DefenseIcon.SetResourceReference(ContentProperty, "DefenseIcon");
        DeathIcon.SetResourceReference(ContentProperty, "DeathIcon");

        var type = "Min-";
        if (OldCheckOption.IsChecked)
            type = "Old-";

        // Item icons
        foreach (var item in Data.Items.Keys)
        {
            Data.Items[item].Item1.SetResourceReference(ContentProperty, type + item);

            //dirty way of doing this, but oh well
            //shadows
            if (cusItemCheck[Data.Items[item].Item1].Item2 != null)
                cusItemCheck[Data.Items[item].Item1]
                    .Item2
                    .SetResourceReference(ContentProperty, type + item);
        }

        // Ghost icons
        foreach (var item in Data.GhostItems.Values)
        {
            if (Codes.FindItemType(item.Name) != "report")
                item.SetResourceReference(ContentProperty, type + item.Name.Remove(0, 6));
            else
                item.SetResourceReference(ContentProperty, type + "Report");
        }

        // stat/info icons
        ValorM.SetResourceReference(ContentProperty, "Valor");
        WisdomM.SetResourceReference(ContentProperty, "Wisdom");
        LimitM.SetResourceReference(ContentProperty, "Limit");
        MasterM.SetResourceReference(ContentProperty, "Master");
        FinalM.SetResourceReference(ContentProperty, "Final");
        HighJump.SetResourceReference(ContentProperty, "HighJump");
        QuickRun.SetResourceReference(ContentProperty, "QuickRun");
        DodgeRoll.SetResourceReference(ContentProperty, "DodgeRoll");
        AerialDodge.SetResourceReference(ContentProperty, "AerialDodge");
        Glide.SetResourceReference(ContentProperty, "Glide");

        CustomChecksCheck();
    }

    public void SetWorldImage()
    {
        var type = "Min-";
        if (OldWorldOption.IsChecked)
            type = "Old-";

        //main window worlds
        SorasHeart.SetResourceReference(ContentProperty, type + "SoraHeartImage");
        SimulatedTwilightTown.SetResourceReference(ContentProperty, type + "SimulatedImage");
        HollowBastion.SetResourceReference(ContentProperty, type + "HollowBastionImage");
        OlympusColiseum.SetResourceReference(ContentProperty, type + "OlympusImage");
        LandofDragons.SetResourceReference(ContentProperty, type + "LandofDragonsImage");
        PrideLands.SetResourceReference(ContentProperty, type + "PrideLandsImage");
        HalloweenTown.SetResourceReference(ContentProperty, type + "HalloweenTownImage");
        SpaceParanoids.SetResourceReference(ContentProperty, type + "SpaceParanoidsImage");
        GoA.SetResourceReference(ContentProperty, type + "GardenofAssemblageImage");
        DriveForms.SetResourceReference(ContentProperty, type + "DriveFormsImage");
        TwilightTown.SetResourceReference(ContentProperty, type + "TwilightTownImage");
        BeastsCastle.SetResourceReference(ContentProperty, type + "BeastCastleImage");
        Agrabah.SetResourceReference(ContentProperty, type + "AgrabahImage");
        HundredAcreWood.SetResourceReference(ContentProperty, type + "HundredAcreImage");
        PortRoyal.SetResourceReference(ContentProperty, type + "PortRoyalImage");
        Twtnw.SetResourceReference(ContentProperty, type + "TWTNWImage");
        Atlantica.SetResourceReference(ContentProperty, type + "AtlanticaImage");
        DisneyCastle.SetResourceReference(ContentProperty, type + "DisneyCastleImage");

        switch (PuzzleOption.IsChecked)
        {
            //puzzle/synth display
            //both on
            case true when SynthOption.IsChecked:
                PuzzSynth.SetResourceReference(ContentProperty, type + "PuzzSynth");
                break;
            //synth on puzzle off
            case false when SynthOption.IsChecked:
                PuzzSynth.SetResourceReference(ContentProperty, type + "PuzzSynth_S");
                break;
        }

        if (PuzzleOption.IsChecked && !SynthOption.IsChecked) //synth off puzzle on
        {
            PuzzSynth.SetResourceReference(ContentProperty, type + "PuzzSynth_P");
        }

        CustomWorldCheck();
    }
}
