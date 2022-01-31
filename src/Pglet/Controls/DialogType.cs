using System.ComponentModel;

namespace Pglet
{
    public enum DialogType
    {
        [Description("normal")]
        Normal,

        [Description("largeHeader")]
        LargeHeader,

        [Description("close")]
        Close
    }
}
