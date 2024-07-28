using System.Windows.Input;

namespace ArchiveMaster.ViewModels;

public class ModuleMenuItemInfo
{
    public ModuleMenuItemInfo()
    {
        
    }

    public ModuleMenuItemInfo(string header, ICommand command)
    {
        Header = header;
        Command = command;
    }
    
    public string Header { get; set; }
    public ICommand Command { get; set; }
}