using System.ComponentModel;

namespace Pglet
{
    public enum ImageFit
    {
        [Description("none")]
        None,

        [Description("contain")]
        Contain,

        [Description("cover")]
        Cover,

        [Description("center")]
        Center,

        [Description("centerContain")]
        CenterContain,

        [Description("centerCover")]
        CenterCover
    }
}
