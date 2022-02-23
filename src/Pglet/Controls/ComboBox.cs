using System.Collections.Generic;

namespace Pglet
{
    public class ComboBox : Control
    {
        protected override string ControlName => "combobox";

        IList<ComboBoxOption> _options = new List<ComboBoxOption>();

        public IList<ComboBoxOption> Options
        {
            get { return _options; }
            set { _options = value; }
        }

        public string Value
        {
            get { return GetAttr("value"); }
            set { SetAttr("value", value); }
        }

        public string Label
        {
            get { return GetAttr("label"); }
            set { SetAttr("label", value); }
        }

        public string Placeholder
        {
            get { return GetAttr("placeholder"); }
            set { SetAttr("placeholder", value); }
        }

        public string ErrorMessage
        {
            get { return GetAttr("errorMessage"); }
            set { SetAttr("errorMessage", value); }
        }

        public bool MultiSelect
        {
            get { return GetBoolAttr("multiSelect"); }
            set { SetBoolAttr("multiSelect", value); }
        }

        public bool AllowFreeForm
        {
            get { return GetBoolAttr("allowFreeForm"); }
            set { SetBoolAttr("allowFreeForm", value); }
        }

        public bool AutoComplete
        {
            get { return GetBoolAttr("autoComplete"); }
            set { SetBoolAttr("autoComplete", value); }
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

        protected override IEnumerable<Control> GetChildren()
        {
            return _options;
        }
    }
}
