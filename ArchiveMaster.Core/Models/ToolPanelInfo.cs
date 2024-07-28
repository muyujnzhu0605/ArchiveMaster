using ArchiveMaster.Views;
using System;

namespace ArchiveMaster.ViewModels;

public class ToolPanelGroupInfo
{
    public string GroupName { get; set; }
    public List<ToolPanelInfo> Panels { get; } = new List<ToolPanelInfo>();
    public List<ModuleMenuItemInfo> MenuItems { get; } = new List<ModuleMenuItemInfo>();
}

public class ToolPanelInfo
{
    private ToolPanelInfo()
    {
    }

    public ToolPanelInfo(Type type, string title, string description, string iconUri = null)
    {
        PanelType = type;
        Title = title;
        Description = description;
        IconUri = iconUri;
    }

    public string Description { get; private set; }

    public string IconUri { get; private set; }

    public PanelBase PanelInstance { get; set; }

    public Type PanelType { get; private set; }

    public string Title { get; private set; }
}