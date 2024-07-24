using FzLib.Avalonia.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchiveMaster.Messages
{
    public class InputDialogMessage : DialogHostMessage
    {
        public enum InputDialogType
        {
            Text,
            Integer,
            Float,
            Password,
            MultipleLinesText
        }
        public object DefaultValue { get; set; }
        public string Message { get; init; }
        public string Title { get; init; }
        public InputDialogType Type { get; set; }
        public string Watermark { get; set; }
        public Action<string> validation { get; set; }
    }
}
