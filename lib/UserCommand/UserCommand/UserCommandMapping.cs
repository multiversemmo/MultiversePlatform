using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Windows.Forms;
using log4net;

namespace Multiverse.ToolBox
{
    public class ExcludedKey
    {
        protected string key;
        protected string modifier;
        protected Keys keyCode;
        protected Keys modifierCode;

        public ExcludedKey(string key, string modifier)
        {
            this.key = key;
            this.modifier = modifier;
        }

        public string Key
        {
            get
            {
                return key;
            }
            set
            {
                key = value;
            }
        }

        public string Modifier
        {
            get
            {
                return modifier;
            }
            set
            {
                modifier = value;
            }
        }

        public Keys KeyCode
        {
            get
            {
                return keyCode;
            }
            set
            {
                keyCode = value;
            }
        }

        public Keys ModifierCode
        {
            get
            {
                return modifierCode;
            }
            set
            {
                modifierCode = value;
            }
        }

    }

    public class EventObject
    {
        protected EventHandler handler;
        protected string context;
        protected string text;
        protected string evstring;
        protected bool mouseButtonEvent = false;

        public EventObject(EventHandler hand, string con, string txt, string evstr)
        {
            handler = hand;
            context = con;
            text = txt;
            evstring = evstr;
        }

        public EventObject(EventHandler hand, string con, string txt, string evstr, bool mouseEvent)
        {
            handler = hand;
            context = con;
            text = txt;
            evstring = evstr;
            mouseButtonEvent = mouseEvent;
        }

        public EventObject(XmlReader r)
        {
            fromXml(r);
        }

        public void fromXml(XmlReader r)
        {
            for( int i = 0; i < r.AttributeCount; i++)
            {
                r.MoveToAttribute(i);
                switch (r.Name)
                {
                    case "Context":
                        context = r.Value;
                        break;
                    case "Text":
                        text = r.Value;
                        break;
                    case "EventString":
                        evstring = r.Value;
                        break;
                }
            }
            r.MoveToElement();
        }

        public void ToXml(XmlWriter w)
        {
            w.WriteStartElement("Event");
            w.WriteAttributeString("EventString",evstring);
            w.WriteAttributeString("Text", text);
            w.WriteAttributeString("Context", context);
            w.WriteEndElement();
        }

        public EventHandler Handler
        {
            get
            {
                return handler;
            }
            set
            {
                handler = value;
            }
        }

        public string Context
        {
            get
            {
                return context;
            }
        }

        public string Text
        {
            get
            {
                return text;
            }
        }

        public string EvString
        {
            get
            {
                return evstring;
            }
        }

        public bool MouseButtonEvent
        {
            get
            {
                return mouseButtonEvent;
            }
            set
            {
                mouseButtonEvent = value;
            }
        }
    }

    public class UserCommand
    {
        protected Keys keycode;
        protected string keystring;
        protected string context;
        protected string activity;
        protected string modifier;
        protected Keys modifiercode;
        protected string evstring;
        protected EventObject ev;
        protected UserCommandMapping parent;
        protected List<ExcludedKey> exKeys;
        log4net.ILog log = log4net.LogManager.GetLogger(typeof(UserCommand));
        
        
        public UserCommand( XmlReader r, UserCommandMapping par)
        {
            parent = par;
            fromXml(r);
        }

        public UserCommand(EventObject handler, string key, string activity, string modifier, string evstr, UserCommandMapping par)
        {
            //if (handler == null)
            //{
            //    log.ErrorFormat("handler == null");
            //}
            //if (par == null)
            //{
            //    log.ErrorFormat("par == null");
            //}
            ev = handler;
            parent = par;
            this.activity = activity;
            this.modifier = modifier;
            this.modifiercode = parent.ParseStringToKeyCode(modifier);
            this.evstring = evstr;
            this.keystring = key;
            this.keycode = parent.ParseStringToKeyCode(keystring);
            this.context = ev.Context;
            
        }

        public UserCommand Clone(UserCommandMapping par)
        {
            UserCommand rv = new UserCommand(ev, keystring, activity, modifier, evstring, par);
            return rv;
        }

        private void fromXml(XmlReader r)
        {
            for (int i = 0; i < r.AttributeCount; i++)
            {
                r.MoveToAttribute(i);
                switch (r.Name)
                {
                    case "Event":
                        this.evstring = r.Value;
                        break;
                    case "Modifier":
                        foreach(string mod in parent.Modifiers)
                        {
                            if(String.Equals(mod, r.Value))
                            {
                                this.modifier = mod;
                                modifiercode = parent.ParseModifierToCode(mod);
                                break;
                            }
                        }
                        break;
                    case "Key":
                        keystring = r.Value;
                        keycode = parent.ParseStringToKeyCode(r.Value);
                        break;
                    case "Activity":
                        foreach (string act in parent.Activities)
                        {
                            if (String.Equals(act.ToString(), r.Value))
                            {
                                this.activity = act;
                                break;
                            }
                        }
                        break;
                    case "Context":
                        context = r.Value;
                        break;
                }
            }
            r.MoveToElement();
            foreach (EventObject ev in parent.Events)
            {
                if (String.Equals(ev.EvString, this.evstring))
                {
                    this.ev = ev;
                }
            }
            return;
        }

        public void ToXml(XmlWriter w)
        {
            w.WriteStartElement("CommandBinding");
            w.WriteAttributeString("Event", this.evstring);
            w.WriteAttributeString("Modifier", this.modifier);
            w.WriteAttributeString("Key", this.keystring);
            w.WriteAttributeString("Activity", this.activity);
            w.WriteAttributeString("Context", this.context);
            w.WriteEndElement();
        }



        public EventObject Event
        {
            get
            {
                return ev;
            }
        }

        public string Context
        {
            get
            {
                return context;
            }
        }

        public string Activity
        {
            get
            {
                return activity;
            }
            set
            {
                activity = value;
            }
        }

        public string Modifier
        {
            get
            {
                return modifier;
            }
            set
            {
                modifier = value;
                modifiercode = parent.ParseStringToKeyCode(value);
            }
        }

        public Keys ModifierCode
        {
            get
            {
                return modifiercode;
            }
        }

        public Keys KeyCode
        {
            get
            {
                return keycode;
            }
        }

        public string Key
        {
            get
            {
                return keystring;
            }
            set
            {
                keystring = value;
                keycode = parent.ParseStringToKeyCode(value);
            }
        }

        public string EvString
        {
            get
            {
                return evstring;
            }
        }
    }

    public class UserCommandMapping
    {
        protected static string[] activityarray = { "up", "down" };
        protected static string[] modifiersarray = { "Ctrl", "Alt", "Shift", "none" };
        protected List<string> activities = new List<string>(activityarray);
        protected List<string> modifiers = new List<string>(modifiersarray);
        protected List<UserCommand> commands = new List<UserCommand>();
        protected List<EventObject> events;
        protected List<string> context;
        protected static string[] keysarray = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S",
            "T", "U", "V", "W", "X", "Y", "Z", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "-", "[", "]", "\\", ";", "'", "`", ",", ".",
            "/", "DELETE", "INSERT", "HOME", "PAGEUP", "PAGEDOWN", "END", "NUMPAD1", "NUMPAD2", "NUMPAD3", "NUMPAD4", "NUMPAD5", "NUMPAD6",
            "NUMPAD7", "NUMPAD8", "NUMPAD9", "NUMPAD0", "UP", "DOWN", "LEFT", "RIGHT", "MBUTTON", "LBUTTON", "RBUTTON", "TAB", "ADD",
            "SUBTRACT", "DIVIDE", "MULTIPLY", "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "F10", "F11", "F12" };
        protected List<string> keys = new List<string>(keysarray);
        protected List<ExcludedKey> excludedKeys;

        public UserCommandMapping(XmlReader r, List<EventObject> events, List<string> context, List<ExcludedKey> excludekeys)
        {
            this.events = events;
            this.context = context;
            this.commands = new List<UserCommand>();
            this.excludedKeys = excludekeys;
            
            while (r.Read())
            {
                switch (r.Name)
                {
                    case "CommandBindings":
                        fromXml(r);
                        break;
                }
                
				if (r.NodeType == XmlNodeType.EndElement)
				{
					break;
				}
            }

            foreach(ExcludedKey exKey in excludedKeys)
            {
                exKey.KeyCode = ParseStringToKeyCode(exKey.Key);
                exKey.ModifierCode = ParseModifierToCode(exKey.Modifier);
            }
        }


        public Keys ParseStringToKeyCode(string value)
        {
            Keys keycode = 0;
            switch (value)
            {
                case "A":
                    keycode = Keys.A;
                    break;
                case "B":
                    keycode = Keys.B;
                    break;
                case "C":
                    keycode = Keys.C;
                    break;
                case "D":
                    keycode = Keys.D;
                    break;
                case "E":
                    keycode = Keys.E;
                    break;
                case "F":
                    keycode = Keys.F;
                    break;
                case "G":
                    keycode = Keys.G;
                    break;
                case "H":
                    keycode = Keys.H;
                    break;
                case "I":
                    keycode = Keys.I;
                    break;
                case "J":
                    keycode = Keys.J;
                    break;
                case "K":
                    keycode = Keys.K;
                    break;
                case "L":
                    keycode = Keys.L;
                    break;
                case "M":
                    keycode = Keys.M;
                    break;
                case "N":
                    keycode = Keys.N;
                    break;
                case "O":
                    keycode = Keys.O;
                    break;
                case "P":
                    keycode = Keys.P;
                    break;
                case "Q":
                    keycode = Keys.Q;
                    break;
                case "R":
                    keycode = Keys.R;
                    break;
                case "S":
                    keycode = Keys.S;
                    break;
                case "T":
                    keycode = Keys.T;
                    break;
                case "U":
                    keycode = Keys.U;
                    break;
                case "V":
                    keycode = Keys.V;
                    break;
                case "W":
                    keycode = Keys.W;
                    break;
                case "X":
                    keycode = Keys.X;
                    break;
                case "Y":
                    keycode = Keys.Y;
                    break;
                case "Z":
                    keycode = Keys.Z;
                    break;
                case "1":
                    keycode = Keys.D1;
                    break;
                case "2":
                    keycode = Keys.D2;
                    break;
                case "3":
                    keycode = Keys.D3;
                    break;
                case "4":
                    keycode = Keys.D4;
                    break;
                case "5":
                    keycode = Keys.D5;
                    break;
                case "6":
                    keycode = Keys.D6;
                    break;
                case "7":
                    keycode = Keys.D7;
                    break;
                case "8":
                    keycode = Keys.D8;
                    break;
                case "9":
                    keycode = Keys.D9;
                    break;
                case "0":
                    keycode = Keys.D0;
                    break;
                case "-":
                    keycode = Keys.OemMinus;
                    break;
                case "[":
                    keycode = Keys.OemOpenBrackets;
                    break;
                case "]":
                    keycode = Keys.OemCloseBrackets;
                    break;
                case "\\":
                    keycode = Keys.OemBackslash;
                    break;
                case ";":
                    keycode = Keys.OemSemicolon;
                    break;
                case "'":
                    keycode = Keys.OemQuotes;
                    break;
                case "`":
                    keycode = Keys.Oemtilde;
                    break;
                case ",":
                    keycode = Keys.Oemcomma;
                    break;
                case ".":
                    keycode = Keys.OemPeriod;
                    break;
                case "/":
                    keycode = Keys.OemQuestion;
                    break;
                case "DELETE":
                    keycode = Keys.Delete;
                    break;
                case "INSERT":
                    keycode = Keys.Insert;
                    break;
                case "HOME":
                    keycode = Keys.Home;
                    break;
                case "PAGEUP":
                    keycode = Keys.PageUp;
                    break;
                case "PAGEDOWN":
                    keycode = Keys.PageDown;
                    break;
                case "END":
                    keycode = Keys.End;
                    break;
                case "NUMPAD1":
                    keycode = Keys.NumPad1;
                    break;
                case "NUMPAD2":
                    keycode = Keys.NumPad2;
                    break;
                case "NUMPAD3":
                    keycode = Keys.NumPad3;
                    break;
                case "NUMPAD4":
                    keycode = Keys.NumPad4;
                    break;
                case "NUMPAD5":
                    keycode = Keys.NumPad5;
                    break;
                case "NUMPAD6":
                    keycode = Keys.NumPad6;
                    break;
                case "NUMPAD7":
                    keycode = Keys.NumPad7;
                    break;
                case "NUMPAD8":
                    keycode = Keys.NumPad8;
                    break;
                case "NUMPAD9":
                    keycode = Keys.NumPad9;
                    break;
                case "NUMPAD0":
                    keycode = Keys.NumPad0;
                    break;
                case "UP":
                    keycode = Keys.Up;
                    break;
                case "DOWN":
                    keycode = Keys.Down;
                    break;
                case "LEFT":
                    keycode = Keys.Left;
                    break;
                case "RIGHT":
                    keycode = Keys.Right;
                    break;
                case "MBUTTON":
                    keycode = Keys.MButton;
                    break;
                case "LBUTTON":
                    keycode = Keys.LButton;
                    break;
                case "RBUTTON":
                    keycode = Keys.RButton;
                    break;
                case "TAB":
                    keycode = Keys.Tab;
                    break;
                case "ADD":
                    keycode = Keys.Add;
                    break;
                case "SUBTRACT":
                    keycode = Keys.Subtract;
                    break;
                case "DIVIDE":
                    keycode = Keys.Divide;
                    break;
                case "MULTIPLY":
                    keycode = Keys.Multiply;
                    break;
                case "F1":
                    keycode = Keys.F1;
                    break;
                case "F2":
                    keycode = Keys.F2;
                    break;
                case "F3":
                    keycode = Keys.F3;
                    break;
                case "F4":
                    keycode = Keys.F4;
                    break;
                case "F5":
                    keycode = Keys.F5;
                    break;
                case "F6":
                    keycode = Keys.F6;
                    break;
                case "F7":
                    keycode = Keys.F7;
                    break;
                case "F8":
                    keycode = Keys.F8;
                    break;
                case "F9":
                    keycode = Keys.F9;
                    break;
                case "F10":
                    keycode = Keys.F10;
                    break;
                case "F11":
                    keycode = Keys.F11;
                    break;
                case "F12":
                    keycode = Keys.F12;
                    break;
            }
            return keycode;
        }

        public Keys ParseModifierToCode(string mod)
        {
            Keys keycode = 0;
            switch (mod)
            {
                case "Ctrl":
                    keycode = Keys.Control;
                    break;
                case "Alt":
                    keycode = Keys.Alt;
                    break;
                case "Shift":
                    keycode = Keys.Shift;
                    break;
                case "none":
                    keycode = Keys.None;
                    break;
            }
            return keycode;
        }

        public UserCommandMapping(List<EventObject> events, List<string> context, List<ExcludedKey> exKeys)
        {
            this.events = events;
            this.context = context;

            foreach (ExcludedKey eKey in exKeys)
            {
                eKey.KeyCode = ParseStringToKeyCode(eKey.Key);
                eKey.ModifierCode = ParseModifierToCode(eKey.Modifier);
            }
        }

        public UserCommandMapping(List<EventObject> events, List<string> context, List<UserCommand> com, List<ExcludedKey> exKeys)
        {

            this.events = events;
            this.context = context;
            this.excludedKeys = exKeys;
            foreach (UserCommand comm in com)
            {
                UserCommand uc =  comm.Clone(this);
                this.commands.Add(uc);
            }
            foreach (ExcludedKey eKey in exKeys)
            {
                eKey.KeyCode = ParseStringToKeyCode(eKey.Key);
                eKey.ModifierCode = ParseModifierToCode(eKey.Modifier);
            }
        }

        private void fromXml(XmlReader r)
        {
            while (r.Read())
            {
                switch(r.Name)
                {
                    case "CommandBinding":
                        UserCommand uc = new UserCommand(r, this);
                        this.commands.Add(uc);
                        break;
                }
            }
   
        }


        public void ToXml(XmlWriter w)
        {
            w.WriteStartElement("CommandBindings");
            foreach (UserCommand command in commands)
            {
                command.ToXml(w);
            }
            w.WriteEndElement();
        }

        public List<UserCommand> Commands
        {
            get
            {
                return this.commands;
            }
            set
            {
                this.commands = value;
            }
        }

        public EventObject GetMatch(Keys mod, Keys key, string act, string con)
        {
            foreach (UserCommand cmd in commands)
            {
                if(cmd.ModifierCode == mod && cmd.KeyCode == key && String.Equals(cmd.Activity, act) && String.Equals(cmd.Context, con))
                {
                    return cmd.Event;
                }
            }
            return null;
        }

        public List<EventObject> GetEventsForContext(string con)
        {
            List<EventObject> list = new List<EventObject>();
            foreach (EventObject events in Events)
            {
                if (String.Equals(events.Context, con))
                {
                    list.Add(events);
                }
            }
            return list;
        }

        public List<UserCommand> GetCommandsForContext(string con)
        {
            List<UserCommand> rv = new List<UserCommand>();
            foreach (UserCommand command in commands)
            {
                if (String.Equals(command.Event.Context, con))
                {
                    rv.Add(command);
                }
            }
            return rv;
        }

        public UserCommand GetCommandForEvent(string evstring)
        {
            foreach (UserCommand command in commands)
            {
                if(String.Equals(command.EvString, evstring))
                {
                    return command;
                }
            }
            return null;
        }

        public List<ExcludedKey> ExcludedKeys
        {
            get
            {
                return excludedKeys;
            }
        }


        public List<string> Modifiers
        {
            get
            {
                return modifiers;
            }
        }

        public List<string> Activities
        {
            get
            {
                return activities;
            }
        }

        public List<EventObject> Events
        {
            get
            {
                return events;
            }
        }

        public List<string> Context
        {
            get
            {
                return context;
            }
        }

        public List<string> Key
        {
            get
            {
                return keys;
            }
        }

        public UserCommandMapping Clone()
        {
            UserCommandMapping clone;
            clone = new UserCommandMapping(events, context, commands, excludedKeys);
            return clone;
        }

        public void Dispose()
        {
            return;
        }
    }
}
