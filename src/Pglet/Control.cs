using Pglet.Protocol;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pglet
{
    public abstract class Control
    {
        public const int DEFAULT_COMMAND_TIMEOUT_MS = 5000;

        public class AttrValue
        {
            public string Value { get; set; }
            public bool IsDirty { get; set; }
        }

        protected abstract string ControlName { get; }

        readonly private Dictionary<string, AttrValue> _attrs = new(StringComparer.OrdinalIgnoreCase);
        readonly private Dictionary<string, EventHandler> _eventHandlers = new(StringComparer.OrdinalIgnoreCase);
        readonly protected List<Control> _previousChildren = new(); // hash codes of previous children
        readonly protected SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        protected string _id;
        private string _uid;
        private Page _page;
        private object _data;

        public Page Page
        {
            get { return _page; }
            internal set { _page = value; }
        }

        public string Uid
        {
            get
            {
                return _uid;
            }
        }

        internal string UniqueId
        {
            get { return _uid; }
            set { _uid = value; }
        }

        public Control()
        {
        }

        public string Id
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
            }
        }

        public string Width
        {
            get { return GetAttr("width"); }
            set { SetAttr("width", value); }
        }

        public string Height
        {
            get { return GetAttr("height"); }
            set { SetAttr("height", value); }
        }

        public string Padding
        {
            get { return GetAttr("padding"); }
            set { SetAttr("padding", value); }
        }

        public string Margin
        {
            get { return GetAttr("margin"); }
            set { SetAttr("margin", value); }
        }

        public bool Visible
        {
            get { return GetBoolAttr("visible", true); }
            set { SetBoolAttr("visible", value); }
        }

        public bool Disabled
        {
            get { return GetBoolAttr("disabled", false); }
            set { SetBoolAttr("disabled", value); }
        }

        public object Data
        {
            get
            {
                return _data;
            }
            set
            {
                _data = value;
                if (value != null)
                {
                    SetAttrInternal("data", value.ToString());
                }
            }
        }

        public void Update()
        {
            UpdateAsync().GetAwaiter().GetResult();
        }

        public void Update(CancellationToken cancellationToken)
        {
            UpdateAsync(cancellationToken).GetAwaiter().GetResult();
        }

        public async Task UpdateAsync()
        {
            using (var cts = new CancellationTokenSource(DEFAULT_COMMAND_TIMEOUT_MS))
            {
                await UpdateAsync(cts.Token);
            }
        }

        public async Task UpdateAsync(CancellationToken cancellationToken)
        {
            if (_page == null)
            {
                throw new Exception("Control must be added to the page first.");
            }

            await _page.UpdateAsync(cancellationToken, this);
        }

        public void Clean()
        {
            CleanAsync().GetAwaiter().GetResult();
        }

        public void Clean(CancellationToken cancellationToken)
        {
            CleanAsync(cancellationToken).GetAwaiter().GetResult();
        }        

        public async Task CleanAsync()
        {
            using (var cts = new CancellationTokenSource(DEFAULT_COMMAND_TIMEOUT_MS))
            {
                await CleanAsync(cts.Token);
            }
        }

        public virtual async Task CleanAsync(CancellationToken cancellationToken)
        {
            await _lock.WaitAsync(cancellationToken);
            try
            {
                if (_page == null)
                {
                    throw new Exception("Control must be added to the page first.");
                }

                _previousChildren.Clear();

                foreach (var child in GetChildren())
                {
                    RemoveControlRecursively(_page.Index, child);
                }

                await _page.SendCommand(cancellationToken, "clean", _uid);
            }
            finally
            {
                _lock.Release();
            }
        }

        internal Dictionary<string, EventHandler> EventHandlers
        {
            get { return _eventHandlers; }
        }

        protected virtual IEnumerable<Control> GetChildren()
        {
            return new Control[] { };
        }

        protected void SetEventHandler(string eventName, EventHandler handler)
        {
            if (handler != null)
            {
                _eventHandlers[eventName] = handler;
            }
            else
            {
                _eventHandlers.Remove(eventName);
            }
        }

        protected EventHandler GetEventHandler(string eventName)
        {
            return _eventHandlers.ContainsKey(eventName) ? _eventHandlers[eventName] : null;
        }

        protected void SetEnumAttr<T>(string name, T value, bool dirty = true)
        {
            var strValue = value.ToString();
            var memberInfo = typeof(T).GetMember(strValue);
            if (memberInfo != null && memberInfo.Length > 0)
            {
                object[] attrs = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (attrs != null && attrs.Length > 0)
                {
                    strValue = ((DescriptionAttribute)attrs[0]).Description;
                }
            }

            SetAttr(name, strValue, dirty);
        }

        protected T GetEnumAttr<T>(string name) where T : struct
        {
            if (_attrs.ContainsKey(name))
            {
                if (Enum.TryParse(_attrs[name].Value, out T result))
                {
                    return result;
                }
                else
                {
                    var strValue = _attrs[name].Value;
                    foreach (var field in typeof(T).GetFields())
                    {
                        if (Attribute.GetCustomAttribute(field,
                        typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
                        {
                            if (attribute.Description.Equals(strValue, StringComparison.OrdinalIgnoreCase))
                                return (T)field.GetValue(null);
                        }
                        else
                        {
                            if (field.Name.Equals(strValue, StringComparison.OrdinalIgnoreCase))
                                return (T)field.GetValue(null);
                        }
                    }
                }
            }
            return default;
        }

        protected void SetNullableIntAttr(string name, int? value)
        {
            SetAttr(name, value.HasValue ? value.Value.ToString() : "");
        }

        internal int? GetNullableIntAttr(string name)
        {
            return _attrs.ContainsKey(name) && !String.IsNullOrEmpty(_attrs[name].Value) ? Int32.Parse(_attrs[name].Value) : null;

        }

        protected void SetIntAttr(string name, int value)
        {
            SetAttr(name, value.ToString().ToLowerInvariant());
        }

        internal int GetIntAttr(string name, int defValue = 0)
        {
            return _attrs.ContainsKey(name) ? Int32.Parse(_attrs[name].Value) : defValue;

        }

        protected void SetDateAttr(string name, DateTime? value)
        {
            SetAttr(name, value.HasValue ? value.Value.ToString("s", System.Globalization.CultureInfo.InvariantCulture) : null);
        }

        internal DateTime? GetDateAttr(string name)
        {
            return _attrs.ContainsKey(name) ? DateTime.Parse(_attrs[name].Value) : null;

        }

        protected void SetBoolAttr(string name, bool value)
        {
            SetAttr(name, value.ToString().ToLowerInvariant());
        }

        internal bool GetBoolAttr(string name, bool defValue = false)
        {
            return _attrs.ContainsKey(name) ? Boolean.Parse(_attrs[name].Value) : defValue;

        }

        internal void SetAttr(string name, string value, bool dirty = true)
        {
            SetAttrInternal(name, value, dirty);

        }

        internal virtual void SetAttrInternal(string name, string value, bool dirty = true)
        {
            string origValue = null;
            if (_attrs.ContainsKey(name))
            {
                origValue = _attrs[name].Value;
            }

            if (String.IsNullOrEmpty(value) && origValue == null)
            {
                return;
            }

            if (value == null)
            {
                value = "";
            }

            if (value != origValue)
            {
                _attrs[name] = new AttrValue { Value = value, IsDirty = dirty };
            }
        }

        protected string GetAttr(string name)
        {
            return _attrs.ContainsKey(name) ? _attrs[name].Value : null;

        }

        internal void SetAttr<T>(string name, T value, bool dirty = true) where T : notnull
        {
            if (value != null)
            {
                _attrs[name] = new AttrValue { Value = value.ToString(), IsDirty = dirty };
            }
        }

        protected T GetAttr<T>(string name) where T : notnull
        {
            var sval = _attrs.ContainsKey(name) ? _attrs[name].Value : null;
            if (sval == null)
            {
                return default(T);
            }
            else if (typeof(T) == typeof(int))
            {
                return int.TryParse(sval, out int result) ? (T)(object)result : (T)(object)0;
            }
            else if (typeof(T) == typeof(long))
            {
                return long.TryParse(sval, out long result) ? (T)(object)result : (T)(object)0;
            }
            else if (typeof(T) == typeof(float))
            {
                return float.TryParse(sval, out float result) ? (T)(object)result : (T)(object)0;
            }
            else
            {
                return (T)(object)sval;
            }
        }

        internal void BuildUpdateCommands(Dictionary<string, Control> index, List<Control> addedControls, List<Command> commands)
        {
            // update control settings
            var updateCmd = this.GetCommandAttrs(update: true);

            if (updateCmd.Attrs.Count > 0)
            {
                updateCmd.Name = "set";
                commands.Add(updateCmd);
            }

            // go through children
            var previousChildren = this._previousChildren;
            var currentChildren = this.GetChildren();

            var previousHashes = new Dictionary<int, Control>();
            var currentHashes = new Dictionary<int, Control>();

            foreach (var ctrl in previousChildren)
            {
                previousHashes[ctrl.GetHashCode()] = ctrl;
            }
            foreach (var ctrl in currentChildren)
            {
                currentHashes[ctrl.GetHashCode()] = ctrl;
            }

            var previousInts = previousHashes.Keys.ToArray();
            var currentInts = currentHashes.Keys.ToArray();

            // diff sequences
            var diffs = Diff.DiffInt(previousInts, currentInts);

            int n = 0;
            for (int fdx = 0; fdx < diffs.Length; fdx++)
            {
                Diff.Item item = diffs[fdx];

                // unchanged controls
                while ((n < item.StartB) && (n < currentInts.Length))
                {
                    currentHashes[currentInts[n]].BuildUpdateCommands(index, addedControls, commands);
                    n++;
                }

                // deleted controls
                var deletedIds = new List<string>();
                for (int m = 0; m < item.deletedA; m++)
                {
                    var deletedControl = previousHashes[previousInts[item.StartA + m]];
                    RemoveControlRecursively(index, deletedControl);
                    deletedIds.Add(deletedControl.UniqueId);
                }
                if (deletedIds.Count > 0)
                {
                    commands.Add(new Command { Name = "remove", Values = deletedIds });
                }

                // added controls
                while (n < item.StartB + item.insertedB)
                {
                    var addCmd = new Command { Name = "add" };
                    addCmd.Attrs["to"] = this.UniqueId;
                    addCmd.Attrs["at"] = n.ToString();
                    addCmd.Commands = currentHashes[currentInts[n]].GetCommands(index: index, addedControls: addedControls);
                    commands.Add(addCmd);
                    n++;
                }
            } // for

            // the rest of unchanged controls
            while (n < currentInts.Length)
            {
                currentHashes[currentInts[n]].BuildUpdateCommands(index, addedControls, commands);
                n++;
            }

            _previousChildren.Clear();
            _previousChildren.AddRange(currentChildren);
        }

        protected void RemoveControlRecursively(Dictionary<string, Control> index, Control control)
        {
            // remove all children
            foreach (var child in control.GetChildren())
            {
                RemoveControlRecursively(index, child);
            }

            // remove control itself
            index.Remove(control.UniqueId);
        }

        private List<Command> GetCommands(int indent = 0, Dictionary<string, Control> index = null, IList<Control> addedControls = null)
        {
            // remove control from index
            if (_uid != null && index != null)
            {
                index.Remove(_uid);
            }

            var commands = new List<Command>();

            // main command
            var cmd = GetCommandAttrs(update: false);
            cmd.Indent = indent;
            cmd.Values.Add(ControlName);
            commands.Add(cmd);

            if (addedControls != null)
            {
                addedControls.Add(this);
            }

            var children = GetChildren();
            if (children != null)
            {
                foreach (var control in children)
                {
                    var childCmds = control.GetCommands(indent: indent + 2, index: index, addedControls: addedControls);
                    commands.AddRange(childCmds);
                }
            }

            _previousChildren.Clear();
            _previousChildren.AddRange(children);

            return commands;
        }

        private Command GetCommandAttrs(bool update = false)
        {
            var command = new Command();

            if (update && _uid == null)
            {
                return command;
            }

            foreach (string attrName in _attrs.Keys.OrderBy(k => k).Select(a => a.ToLowerInvariant()))
            {
                var dirty = _attrs[attrName].IsDirty;
                if ((update && !dirty) || attrName == "id")
                {
                    continue;
                }

                var val = _attrs[attrName].Value;
                command.Attrs[attrName] = val;
                _attrs[attrName].IsDirty = false;
            }

            if (!update && _id != null)
            {
                command.Attrs["id"] = _id;
            }
            else if (update && command.Attrs.Count > 0)
            {
                command.Values.Add(_uid);
            }

            return command;
        }

        ~Control()
        {
            // dispose lock?
        }
    }
}
