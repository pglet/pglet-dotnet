namespace Pglet
{
    public class Image : Control
    {
        protected override string ControlName => "image";

        public string Src
        {
            get { return GetAttr("src"); }
            set { SetAttr("src", value); }
        }

        public string Alt
        {
            get { return GetAttr("alt"); }
            set { SetAttr("alt", value); }
        }

        public string Title
        {
            get { return GetAttr("title"); }
            set { SetAttr("title", value); }
        }

        public bool MaximizeFrame
        {
            get { return GetBoolAttr("maximizeFrame"); }
            set { SetBoolAttr("maximizeFrame", value); }
        }

        public ImageFit Fit
        {
            get { return GetEnumAttr<ImageFit>("fit"); }
            set { SetEnumAttr("fit", value); }
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
