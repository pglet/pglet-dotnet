namespace Pglet
{
    public class Text : Control
    {
        protected override string ControlName => "text";

        public string Value
        {
            get { return GetAttr("value"); }
            set { SetAttr("value", value); }
        }

        public bool Markdown
        {
            get { return GetBoolAttr("markdown"); }
            set { SetBoolAttr("markdown", value); }
        }

        public TextAlign Align
        {
            get { return GetEnumAttr<TextAlign>("align"); }
            set { SetEnumAttr("align", value); }
        }

        public TextVerticalAlign VerticalAlign
        {
            get { return GetEnumAttr<TextVerticalAlign>("verticalAlign"); }
            set { SetEnumAttr("verticalAlign", value); }
        }

        public TextSize Size
        {
            get { return GetEnumAttr<TextSize>("size"); }
            set { SetEnumAttr("size", value); }
        }

        public bool Bold
        {
            get { return GetBoolAttr("bold"); }
            set { SetBoolAttr("bold", value); }
        }

        public bool Italic
        {
            get { return GetBoolAttr("italic"); }
            set { SetBoolAttr("italic", value); }
        }

        public bool Pre
        {
            get { return GetBoolAttr("pre"); }
            set { SetBoolAttr("pre", value); }
        }

        public bool Nowrap
        {
            get { return GetBoolAttr("nowrap"); }
            set { SetBoolAttr("nowrap", value); }
        }

        public bool Block
        {
            get { return GetBoolAttr("block"); }
            set { SetBoolAttr("block", value); }
        }

        public string Color
        {
            get { return GetAttr("color"); }
            set { SetAttr("color", value); }
        }

        public string BgColor
        {
            get { return GetAttr("bgColor"); }
            set { SetAttr("bgColor", value); }
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
