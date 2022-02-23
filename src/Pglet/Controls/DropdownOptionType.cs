using System.ComponentModel;

namespace Pglet
{
    public enum DropdownOptionType
    {
        [Description("normal")]
        Normal,

        [Description("divider")]
        Divider,

        [Description("header")]
        Header
    }
}
