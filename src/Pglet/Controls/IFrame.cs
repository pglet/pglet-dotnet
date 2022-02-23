namespace Pglet
{
    public class IFrame : Control
    {
        protected override string ControlName => "iframe";

        public string Src
        {
            get { return GetAttr("src"); }
            set { SetAttr("src", value); }
        }

        public string Title
        {
            get { return GetAttr("title"); }
            set { SetAttr("title", value); }
        }

        public SideValues<BorderStyle> BorderStyle
        {
            get
            {
                var v = GetAttr("borderStyle");
                return v != null ? SideValues<BorderStyle>.Parse(v) : null;
            }
            set
            {
                SetAttr("borderStyle", value != null ? value.ToString() : null);
            }
        }

        public SideValues<string> BorderWidth
        {
            get
            {
                var v = GetAttr("borderWidth");
                return v != null ? SideValues<string>.Parse(v) : null;
            }
            set
            {
                SetAttr("borderWidth", value != null ? value.ToString() : null);
            }
        }

        public SideValues<string> BorderColor
        {
            get
            {
                var v = GetAttr("borderColor");
                return v != null ? SideValues<string>.Parse(v) : null;
            }
            set
            {
                SetAttr("borderColor", value != null ? value.ToString() : null);
            }
        }

        public SideValues<string> BorderRadius
        {
            get
            {
                var v = GetAttr("borderRadius");
                return v != null ? SideValues<string>.Parse(v) : null;
            }
            set
            {
                SetAttr("borderRadius", value != null ? value.ToString() : null);
            }
        }
    }
}
