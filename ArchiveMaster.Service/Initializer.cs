using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ArchiveMaster.Configs;
using ArchiveMaster.Platforms;
using ArchiveMaster.ViewModels;
using ArchiveMaster.Views;
using FzLib.Avalonia.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ArchiveMaster;

public class Initializer
{
    public void Initialize(IServiceCollection services, IMvcBuilder mvcBuilder)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);


        foreach (var module in ModuleInitializers)
        {
            module.AddServices(services);
            mvcBuilder.AddApplicationPart(module.GetType().Assembly).AddControllersAsServices();
        }
    }

    public IServiceModuleInitializer[] ModuleInitializers { get; } =
    [
        new FileBackupperIServiceModuleInitializer(),
    ];
}