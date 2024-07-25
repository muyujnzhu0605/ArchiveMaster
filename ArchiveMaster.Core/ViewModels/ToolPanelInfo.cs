using ArchiveMaster.Views;
using System;

namespace ArchiveMaster.ViewModels;

public class ToolPanelGroupInfo
{
    public string GroupName { get; set; }
    public List<ToolPanelInfo> Panels { get; } = new List<ToolPanelInfo>();
}

public class ToolPanelInfo
{
    private ToolPanelInfo()
    {
    }

    public ToolPanelInfo(Type type, string groupName, string title, string description, string iconUri = null)
    {
        PanelType = type;
        GroupName = groupName;
        Title = title;
        Description = description;
        IconUri = iconUri;
    }

    public static List<ToolPanelGroupInfo> Groups { get; } = new List<ToolPanelGroupInfo>();

    public string Description { get; private set; }

    public string GroupName { get; private set; }

    public string IconUri { get; private set; }

    public PanelBase PanelInstance { get; set; }

    public Type PanelType { get; private set; }

    public string Title { get; private set; }

    public void Register()
    {
        ToolPanelGroupInfo group = Groups.FirstOrDefault(p => p.GroupName == GroupName);
        if (group == null)
        {
            group = new ToolPanelGroupInfo()
            {
                GroupName = GroupName
            };
            Groups.Add(group);
        }

        group.Panels.Add(this);
    }
}