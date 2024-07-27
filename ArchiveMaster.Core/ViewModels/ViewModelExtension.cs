using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using FzLib.Avalonia.Messages;
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

        public static Task ShowErrorAsync(this ObservableObject sender, string title, string message)
        {
            return SendMessage(sender, new CommonDialogMessage()
            {
                Type = CommonDialogMessage.CommonDialogType.Error,
                Title = title,
                Message = message
            }).Task;
        }

        public static Task ShowOkAsync(this ObservableObject sender, string title, string message)
        {
            return SendMessage(sender, new CommonDialogMessage()
            {
                Type = CommonDialogMessage.CommonDialogType.Ok,
                Title = title,
                Message = message
            }).Task;
        }

        public static Task ShowErrorAsync(this ObservableObject sender, string title, Exception exception)
        {
            return SendMessage(sender, new CommonDialogMessage()
            {
                Type = CommonDialogMessage.CommonDialogType.Error,
                Title = title,
                Exception = exception
            }).Task;
        }
    }
}
