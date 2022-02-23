using System.ComponentModel;

namespace Pglet
{
    public enum ComboBoxOptionType
    {
        [Description("normal")]
        Normal,

        [Description("divider")]
        Divider,

        [Description("header")]
        Header,

        [Description("select_all")]
        SelectAll
    }
}
