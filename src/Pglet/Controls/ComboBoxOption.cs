namespace Pglet
{
    public class ComboBoxOption : Control
    {
        protected override string ControlName => "option";

        public string Key
        {
            get { return GetAttr("key"); }
            set { SetAttr("key", value); }
        }

        public string Text
        {
            get { return GetAttr("text"); }
            set { SetAttr("text", value); }
        }

        public ComboBoxOptionType ItemType
        {
            get { return GetEnumAttr<ComboBoxOptionType>("itemType"); }
            set { SetEnumAttr("itemType", value); }
        }
    }
}
