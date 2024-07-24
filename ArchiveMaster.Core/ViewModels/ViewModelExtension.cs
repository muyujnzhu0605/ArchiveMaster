using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchiveMaster.UI.ViewModels
{
    public static class ViewModelExtension
    {
        public static TMessage SendMessage<TMessage>(this ObservableObject sender, TMessage message) where TMessage : class
        {
            return WeakReferenceMessenger.Default.Send(message);
        }
    }
}
