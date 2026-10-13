using System;
using System.Globalization;
using System.Numerics;
using OmenTools;
using OmenTools.Dalamud;
using OmniToolbox.UI;

namespace OmniToolbox.TreePublic;

public sealed partial class AutoIshgardRestoration
{
    private bool vnavPathStartedByModule;

    private DateTime? movableSince;

    private Vector3 initialDestination;

    private void DriveWaitForFirmament()
    {
        if (DService.Instance().ClientState.TerritoryType == FIRMAMENT_TERRITORY_ID)
        {
            movableSince = null;
            EnterPhase(AutomationPhase.WaitForPlayerMovable);
            status = OmniLoc.Get("Feature.AutoIshgardRestoration.EnteredFirmament");
            return;
        }

        status = OmniLoc.Get("Feature.AutoIshgardRestoration.WaitingForFirmament");
        if (PhaseTimedOut(TimeSpan.FromMinutes(3)))
        {
            FailAutomation(OmniLoc.Get("Feature.AutoIshgardRestoration.FirmamentTeleportTimedOut"));
        }
    }

    private void DriveWaitForPlayerMovable()
    {
        if (DService.Instance().ClientState.TerritoryType != FIRMAMENT_TERRITORY_ID)
        {
            movableSince = null;
            status = OmniLoc.Get("Feature.AutoIshgardRestoration.WaitingForStableTerritory");
            return;
        }

        if (!IsPlayerMovable())
        {
            movableSince = null;
            status = OmniLoc.Get("Feature.AutoIshgardRestoration.WaitingForPlayerMovable");
            return;
        }

        movableSince ??= DateTime.UtcNow;
        if (DateTime.UtcNow - movableSince.Value < TimeSpan.FromSeconds(1))
        {
            status = OmniLoc.Get("Feature.AutoIshgardRestoration.WaitingForContinuousMovability");
            return;
        }

        if (!vnavmeshIPC.IsPluginEnabled())
        {
            FailAutomation(OmniLoc.Get("Feature.AutoIshgardRestoration.NavigationUnavailable"));
            return;
        }

        if (!vnavmeshIPC.GetIsNavReady())
        {
            status = OmniLoc.Get("Feature.AutoIshgardRestoration.WaitingForNavigationReady");
            if (PhaseTimedOut(TimeSpan.FromSeconds(90)))
            {
                FailAutomation(OmniLoc.Get("Feature.AutoIshgardRestoration.NavigationNotReady"));
            }
            return;
        }

        initialDestination = new Vector3(
            38f + (Random.Shared.NextSingle() * 9f),
            -16f,
            162f + (Random.Shared.NextSingle() * 14f));
        vnavmeshIPC.PathfindAndMoveTo(initialDestination, false);
        vnavPathStartedByModule = true;
        EnterPhase(AutomationPhase.MoveToInitialPoint);
        nextActionAt = DateTime.UtcNow.AddSeconds(5);
        status = string.Format(CultureInfo.InvariantCulture, OmniLoc.Get("Feature.AutoIshgardRestoration.MovingToCraftingPoint"), FormatPosition(initialDestination));
    }

    private void DriveMoveToInitialPoint(RecipeOption recipe)
    {
        if (DService.Instance().ClientState.TerritoryType != FIRMAMENT_TERRITORY_ID)
        {
            FailAutomation(OmniLoc.Get("Feature.AutoIshgardRestoration.LeftFirmamentDuringTravel"));
            return;
        }

        var player = DService.Instance().ObjectTable.LocalPlayer;
        if (player is null)
        {
            status = OmniLoc.Get("Feature.AutoIshgardRestoration.WaitingForPlayerPosition");
            return;
        }

        if (Vector3.Distance(player.Position, initialDestination) <= 2f)
        {
            StopOwnedVnavPath();
            StartCraftingCycle(recipe, OmniLoc.Get("Feature.AutoIshgardRestoration.ReachedCraftingPoint"));
            return;
        }

        if (!vnavmeshIPC.GetIsPathfindRunning() && DateTime.UtcNow >= nextActionAt)
        {
            vnavmeshIPC.PathfindAndMoveTo(initialDestination, false);
            vnavPathStartedByModule = true;
            nextActionAt = DateTime.UtcNow.AddSeconds(5);
        }

        status = string.Format(CultureInfo.InvariantCulture, OmniLoc.Get("Feature.AutoIshgardRestoration.MovingToCraftingPoint"), FormatPosition(initialDestination));
        if (PhaseTimedOut(TimeSpan.FromSeconds(90)))
        {
            FailAutomation(OmniLoc.Get("Feature.AutoIshgardRestoration.CraftingPointTravelTimedOut"));
        }
    }

    private void StopOwnedVnavPath()
    {
        if (!vnavPathStartedByModule)
        {
            return;
        }

        vnavmeshIPC.StopPathfind();
        vnavPathStartedByModule = false;
    }

    #region 常量

    private const uint FIRMAMENT_TERRITORY_ID = 886;

    private const string FIRMAMENT_TELEPORT_COMMAND = "/omni bts 无名众人广场";

    #endregion
}
