using Dalamud.Plugin.Services;
using Dalamud.Utility;
using Lumina.Excel.Sheets;
using OmenTools;
using OmenTools.Extensions;
using OmenTools.Interop.Game.Lumina;
using OmenTools.OmenService;
using OmniToolbox.Common.Module.Abstractions;
using OmniToolbox.Common.Module.Enums;
using OmniToolbox.Common.Module.Models;
using OmniToolbox.UI;

namespace OmniToolbox.TreePublic;

public sealed class CopyItemName : ModuleBase
{
    public override ModuleInfo Info { get; } = new()
    {
        Title = OmniLoc.Get("CopyItemNameTitle"),
        Description = OmniLoc.Get("CopyItemNameDescription"),
        Category = ModuleCategory.Item
    };

    private bool hotkeyHeld;

    protected override void OnEnable()
    {
        if (!FrameworkManager.Instance().Reg(OnUpdate))
        {
            throw new InvalidOperationException("Copy item name update registration failed.");
        }
    }

    protected override void OnDisable()
    {
        FrameworkManager.Instance().Unreg(OnUpdate);
        hotkeyHeld = false;
    }

    private void OnUpdate(IFramework _)
    {
        if (!CopyShortcut.IsPressed(ref hotkeyHeld))
            return;
        var hoveredItem = DService.Instance().GameGUI.HoveredItem;
        if (hoveredItem == 0)
        {
            return;
        }

        CopyShortcut.Consume();
        if (!LuminaGetter.TryGetRow<Item>(ItemUtil.GetBaseId((uint)hoveredItem).ItemId, out var item))
        {
            return;
        }

        var name = item.Name.ExtractText();
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        ImGui.SetClipboardText(name);
    }
}
