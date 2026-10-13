using System.Diagnostics;
using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.System.Input;
using FFXIVClientStructs.FFXIV.Client.UI;
using OmenTools;
using OmenTools.Extensions;
using OmenTools.OmenService;
using OmniToolbox.Common.Module.Abstractions;
using OmniToolbox.Common.Module.Enums;
using OmniToolbox.Common.Module.Models;
using OmniToolbox.UI;

namespace OmniToolbox.TreePublic;

public sealed unsafe class AltF4Exit : ModuleBase
{
    public override ModuleInfo Info { get; } = new()
    {
        Title = OmniLoc.Get("AltF4ExitTitle"),
        Description = OmniLoc.Get("AltF4ExitDescription"),
        Category = ModuleCategory.Daily
    };

    private Hook<Framework.Delegates.ExitFromWindow>? ExitFromWindowHook;

    protected override void OnEnable()
    {
        ExitFromWindowHook ??= DService.Instance().Hook.HookFromMemberFunction(
            typeof(Framework.MemberFunctionPointers),
            nameof(Framework.MemberFunctionPointers.ExitFromWindow),
            (Framework.Delegates.ExitFromWindow)ExitFromWindowDetour);
        ExitFromWindowHook.Enable();

        if (!FrameworkManager.Instance().Reg(OnFrameworkUpdate))
        {
            ExitFromWindowHook.Disable();
            throw new InvalidOperationException("Alt+F4 exit update registration failed.");
        }
    }

    protected override void OnDisable()
    {
        FrameworkManager.Instance().Unreg(OnFrameworkUpdate);
        ExitFromWindowHook?.Disable();
    }

    protected override void OnDispose() => ExitFromWindowHook?.Dispose();

    private static void OnFrameworkUpdate(IFramework _)
    {
        var input = UIInputData.Instance();
        if (input is not null && input->IsKeyDown(SeVirtualKey.MENU) && input->IsKeyPressed(SeVirtualKey.F4))
        {
            EndGameProcess();
        }
    }

    private static void ExitFromWindowDetour(Framework* framework) => EndGameProcess();

    private static void EndGameProcess()
    {
        using var process = Process.GetCurrentProcess();
        process.Kill();
    }
}
