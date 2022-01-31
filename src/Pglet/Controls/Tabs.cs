﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pglet
{
    public class Tabs : Control
    {
        protected override string ControlName => "tabs";

        IList<Tab> _tabs = new List<Tab>();
        public IList<Tab> TabItems
        {
            get { return _tabs; }
            set { _tabs = value; }
        }

        public string Value
        {
            get { return GetAttr("value"); }
            set { SetAttr("value", value); }
        }

        public bool Solid
        {
            get { return GetBoolAttr("solid"); }
            set { SetBoolAttr("solid", value); }
        }

        public EventHandler OnChange
        {
            get { return GetEventHandler("change"); }
            set { SetEventHandler("change", value); }
        }

        protected override IEnumerable<Control> GetChildren()
        {
            return _tabs;
        }

        public override async Task CleanAsync(CancellationToken cancellationToken)
        {
            await base.CleanAsync(cancellationToken);
            _tabs.Clear();
        }
    }
}
