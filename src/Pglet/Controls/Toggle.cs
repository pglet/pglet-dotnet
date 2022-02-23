namespace Pglet
{
    public class Toggle : Control
    {
        protected override string ControlName => "toggle";

        public string Label
        {
            get { return GetAttr("label"); }
            set { SetAttr("label", value); }
        }

        public bool Value
        {
            get { return GetBoolAttr("value"); }
            set { SetBoolAttr("value", value); }
        }

        public string ValueField
        {
            get { return GetAttr("value"); }
            set { SetAttr("value", $"{{{value}}}"); }
        }

        public bool Inline
        {
            get { return GetBoolAttr("inline"); }
            set { SetBoolAttr("inline", value); }
        }

        public string OnText
        {
            get { return GetAttr("onText"); }
            set { SetAttr("onText", value); }
        }

        public string OffText
        {
            get { return GetAttr("offText"); }
            set { SetAttr("offText", value); }
        }

        public bool Focused
        {
            get { return GetBoolAttr("focused"); }
            set { SetBoolAttr("focused", value); }
        }

        public EventHandler OnChange
        {
            get { return GetEventHandler("change"); }
            set { SetEventHandler("change", value); }
        }

        public EventHandler OnFocus
        {
            get { return GetEventHandler("focus"); }
            set { SetEventHandler("focus", value); }
        }

        public EventHandler OnBlur
        {
            get { return GetEventHandler("blur"); }
            set { SetEventHandler("blur", value); }
        }
    }
}
