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

    public static List<ToolPanelGroupInfo> Groups { get; } = new List<ToolPanelGroupInfo>();

    public string Description { get; private set; }

    public string GroupName { get; private set; }

    public string IconUri { get; private set; }

    public object PanelInstance { get; set; }

    public Type PanelType { get; private set; }

    public string Title { get; private set; }

    public static ToolPanelInfo Register<T>(string groupName, string title, string description, string iconUri = null)
    {
        var tool = new ToolPanelInfo
        {
            GroupName = groupName,
            Title = title,
            Description = description,
            IconUri = iconUri,
            PanelType = typeof(T)
        };
        ToolPanelGroupInfo group = Groups.FirstOrDefault(p => p.GroupName == groupName);
        if (group == null)
        {
            group = new ToolPanelGroupInfo()
            {
                GroupName = groupName
            };
            Groups.Add(group);
        }
        group.Panels.Add(tool);

        return tool;
    }
}
