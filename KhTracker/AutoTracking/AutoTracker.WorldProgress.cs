namespace KhTracker;

using System;
using System.Windows;

public partial class MainWindow
{
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
                            _ => newProg,
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
                            _ => newProg,
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
                            _ => newProg,
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
                            _ => newProg,
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
                            _ => newProg,
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
                                        _ => newProg,
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
                                        _ => newProg,
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
                                        _ => newProg,
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
                            _ => newProg,
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
                            _ => newProg,
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
                            _ => newProg,
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
}
