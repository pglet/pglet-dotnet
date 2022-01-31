using System.ComponentModel;

namespace Pglet
{
    public enum MessageType
    {
        [Description("info")]
        Info,

        [Description("error")]
        Error,

        [Description("blocked")]
        Blocked,

        [Description("severeWarning")]
        SevereWarning,

        [Description("success")]
        Success,

        [Description("warning")]
        Warning
    }
}
