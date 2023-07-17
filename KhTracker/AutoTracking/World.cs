using System.Collections.Generic;

namespace KhTracker;

internal class World
{
    private readonly Dictionary<int, string> worldCodes;

    public string PreviousWorldName;
    private string world;
    public string WorldName
    {
        get => world;
        private set
        {
            if (world == value)
                return;
            world = value;
            App.Logger?.RecordWorld(value);
        }
    }
    public int WorldNum;
    private readonly int worldAddress;
    private readonly int eventCompleteAddress;
    private readonly int sttAddress;

    public int RoomNumber;
    public int EventId1;
    public int EventId2;
    public int EventId3;
    public int EventComplete;
    private int inStt;
    public int CupRound;

    private readonly int addressOffset;

    private readonly MemoryReader memory;

    public World(MemoryReader mem, int offset, int address, int completeAddress, int sttAddress)
    {
        addressOffset = offset;
        memory = mem;
        worldAddress = address;
        eventCompleteAddress = completeAddress;
        this.sttAddress = sttAddress;

        worldCodes = new Dictionary<int, string>
        {
            { 01, "GoA" }, // Title Demo
            { 02, "TwilightTown" },
            { 03, "DestinyIsland" },
            { 04, "HollowBastion" },
            { 05, "BeastsCastle" },
            { 06, "OlympusColiseum" },
            { 07, "Agrabah" },
            { 08, "LandofDragons" },
            { 09, "HundredAcreWood" },
            { 10, "PrideLands" },
            { 11, "Atlantica" },
            { 12, "DisneyCastle" },
            { 13, "DisneyCastle" }, // Timeless River
            { 14, "HalloweenTown" },
            { 16, "PortRoyal" },
            { 17, "SpaceParanoids" },
            { 18, "TWTNW" },
            { 255, "GoA" }
        };
    }

    public void UpdateMemory()
    {
        PreviousWorldName = WorldName;

        //this shouldn't happen, but use unknown as the world in case it ever does
        WorldName ??= "Unknown";

        var worldData = memory.ReadMemory(worldAddress + addressOffset, 9);
        WorldNum = worldData[0];
        RoomNumber = worldData[1];
        EventId1 = worldData[4];
        EventId2 = worldData[6];
        EventId3 = worldData[8];
        CupRound = worldData[2];

        var eventData = memory.ReadMemory(eventCompleteAddress + addressOffset, 1);
        EventComplete = eventData[0];

        var sttData = memory.ReadMemory(sttAddress + addressOffset, 1);
        inStt = sttData[0];

        var tempWorld = worldCodes.TryGetValue(WorldNum, out var worldCode) ? worldCode : "";

        switch (tempWorld)
        {
            // Handle AS fights
            case "HollowBastion" when RoomNumber == 26:
                WorldName = "GoA";
                break;
            case "HollowBastion" when RoomNumber == 32:
                WorldName = "HalloweenTown"; // Vexen
                break;
            // Data Lexaeus
            case "HollowBastion"
                when RoomNumber == 33
                    && (
                        EventId3 == 122
                        || EventId1 == 123
                        || EventId1 == 142 // AS Lexaeus
                        || EventId3 == 132
                        || EventId1 == 133
                        || EventId1 == 147
                    ):
                WorldName = "Agrabah"; // Lexaeus
                break;
            // Data Larxene
            case "HollowBastion"
                when RoomNumber == 33
                    && (
                        EventId3 == 128
                        || EventId1 == 129
                        || EventId1 == 143 // AS Larxene
                        || EventId3 == 138
                        || EventId1 == 139
                        || EventId1 == 148
                    ):
                WorldName = "SpaceParanoids"; // Larxene
                break;
            case "HollowBastion" when RoomNumber == 34:
                WorldName = "OlympusColiseum"; // Zexion
                break;
            case "HollowBastion" when RoomNumber == 38:
                WorldName = "DisneyCastle"; // Marluxia
                break;
            case "HollowBastion":
                WorldName = "HollowBastion";
                break;
            // Handle STT
            case "TwilightTown" when inStt == 13:
                WorldName = "SimulatedTwilightTown";
                break;
            case "TwilightTown"
                when (RoomNumber == 32 && EventId1 == 1) || (RoomNumber == 1 && EventId1 == 52):
                WorldName = "GoA"; // Crit bonuses
                break;
            case "TwilightTown":
                WorldName = "TwilightTown";
                break;
            // Handle Data fights
            case "TWTNW" when RoomNumber == 10 && (EventId1 == 108):
                WorldName = "LandofDragons"; // Xigbar
                break;
            case "TWTNW" when RoomNumber == 15 && (EventId1 == 110):
                WorldName = "PrideLands"; // Saix
                break;
            case "TWTNW" when RoomNumber == 14 && (EventId1 == 112):
                WorldName = "PortRoyal"; // Luxord
                break;
            case "TWTNW" when RoomNumber == 21 && (EventId1 == 114):
                WorldName = "SimulatedTwilightTown"; // Roxas
                break;
            case "TWTNW":
                WorldName = "TWTNW";
                break;
            default:
            {
                if (WorldName != tempWorld && tempWorld != "")
                {
                    WorldName = tempWorld;
                }

                break;
            }
        }

        //(App.Current.MainWindow as MainWindow).HintText.Content = worldName;
    }
}
