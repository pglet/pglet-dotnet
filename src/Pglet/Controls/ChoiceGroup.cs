using System.Collections.Generic;

namespace Pglet
{
    public class ChoiceGroup : Control
    {
        protected override string ControlName => "choicegroup";

        IList<ChoiceGroupOption> _options = new List<ChoiceGroupOption>();

        public IList<ChoiceGroupOption> Options
        {
            get { return _options; }
            set { _options = value; }
        }

        public string Label
        {
            get { return GetAttr("label"); }
            set { SetAttr("label", value); }
        }

        public string Value
        {
            get { return GetAttr("value"); }
            set { SetAttr("value", value); }
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
