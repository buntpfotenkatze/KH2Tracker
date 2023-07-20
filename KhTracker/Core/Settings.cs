using System.IO;
using System.Text.Json;

namespace KhTracker;

public class Settings
{
    public bool PromiseCharm { get; set; }

    public bool AnsemReports { get; set; } = true;

    public bool Abilities { get; set; } = true;

    public bool TornPages { get; set; } = true;

    public bool FinalForm { get; set; } = true;

    public bool Simulated { get; set; } = true;

    public bool HundredAcre { get; set; } = true;

    public bool Atlantica { get; set; }

    public double WindowX { get; set; }

    public double WindowY { get; set; }

    public bool Cure { get; set; } = true;

    public bool SoraHeart { get; set; } = true;

    public double Width { get; set; } = 570;

    public double Height { get; set; } = 880;

    public bool DragDrop { get; set; } = true;

    public bool WorldProgress { get; set; } = true;

    public bool TopMost { get; set; }

    public bool FormsGrowth { get; set; } = true;

    public bool MinWorld { get; set; } = true;

    public bool OldWorld { get; set; }

    public bool MinCheck { get; set; } = true;

    public bool OldCheck { get; set; }

    public bool MinProg { get; set; } = true;

    public bool OldProg { get; set; }

    public int MainBG { get; set; }

    public bool CustomIcons { get; set; }

    public bool SeedHash { get; set; } = true;

    public bool Level1 { get; set; } = true;

    public bool Level50 { get; set; }

    public bool Level99 { get; set; }

    public bool CheckCount { get; set; }

    public bool WorldLevel1 { get; set; }

    public bool WorldLevel50 { get; set; } = true;

    public bool WorldLevel99 { get; set; }

    public bool NextLevelCheck { get; set; }

    public bool WorldVisitLock { get; set; }

    public bool Puzzle { get; set; }

    public bool Synth { get; set; }

    public bool DeathCounter { get; set; }

    public bool EmitProofKeystroke { get; set; }

    public bool AntiForm { get; set; }

    public bool ExtraChecks { get; set; }

    public bool Drives { get; set; } = true;

    public bool TwilightTown { get; set; } = true;

    public bool HollowBastion { get; set; } = true;

    public bool BeastCastle { get; set; } = true;

    public bool Olympus { get; set; } = true;

    public bool Agrabah { get; set; } = true;

    public bool LandofDragons { get; set; } = true;

    public bool DisneyCastle { get; set; } = true;

    public bool PrideLands { get; set; } = true;

    public bool PortRoyal { get; set; } = true;

    public bool HalloweenTown { get; set; } = true;

    public bool SpaceParanoids { get; set; } = true;

    public bool TWTNW { get; set; } = true;

    public bool ColorHints { get; set; } = true;

    public bool NewWorldLayout { get; set; } = true;

    public bool OldWorldLayout { get; set; }

    public bool AutoSaveProgress { get; set; }

    public bool WorldHighlight { get; set; } = true;

    public bool Disconnect { get; set; } = true;

    public bool AutoConnect { get; set; }

    public bool AutoSaveProgress2 { get; set; }

    public bool LuckyEmblems { get; set; }

    public void Save()
    {
        if (!Directory.Exists("KH2ArchipelagoTrackerSettings"))
        {
            Directory.CreateDirectory("KH2ArchipelagoTrackerSettings\\");
        }

        var serialized = JsonSerializer.Serialize(
            this,
            new JsonSerializerOptions { WriteIndented = true }
        );
        File.WriteAllText("./KH2ArchipelagoTrackerSettings/settings.json", serialized);
    }
}
