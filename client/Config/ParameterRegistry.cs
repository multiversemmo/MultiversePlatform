/********************************************************************

The Multiverse Platform is made available under the MIT License.

Copyright (c) 2012 The Multiverse Foundation

Permission is hereby granted, free of charge, to any person 
obtaining a copy of this software and associated documentation 
files (the "Software"), to deal in the Software without restriction, 
including without limitation the rights to use, copy, modify, 
merge, publish, distribute, sublicense, and/or sell copies 
of the Software, and to permit persons to whom the Software 
is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be 
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE 
OR OTHER DEALINGS IN THE SOFTWARE.

*********************************************************************/

#region Using directives

using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Text;

#endregion

namespace Multiverse.Config
{
	
	// The registry has only 
	public class ParameterRegistry
	{

#region ParameterRegistry Protected Members
		
		static protected ParameterRegistry instance = null;

		protected struct SubsystemHandlers
		{
			internal string subsystemName;
	        internal SetParameterHandler setHandler;
			internal GetParameterHandler getHandler;
			internal SubsystemHandlers(string subsystemName, SetParameterHandler setHandler, 
                                       GetParameterHandler getHandler)
			{
				this.subsystemName = subsystemName;
				this.setHandler = setHandler;
				this.getHandler = getHandler;
			}
		}
	
		protected Dictionary<string, SubsystemHandlers> getSetHandlers = new Dictionary<string, SubsystemHandlers>();

		protected bool GetHandlersForSubsystem(string SubsystemName, out SubsystemHandlers value)
		{
			if (getSetHandlers.TryGetValue(SubsystemName, out value))
				return true;
			else
				return false;
		}

		// Breaks apart a string of the form subsystem.parameter into
		// the two strings; if it doesn't have this form, returns
		// false, otherwise true
		static protected bool BreakSubsystemAndParameter(string subsystemAndParameter, 
														 out string subsystem, out string parameter)
		{
			int i = subsystemAndParameter.IndexOf('.');
			if (i > 0 && i < subsystemAndParameter.Length - 1) {
				subsystem = subsystemAndParameter.Substring(0, i);
				parameter = subsystemAndParameter.Substring(i + 1);
				return true;
			}
			else {
				subsystem = "";
				parameter = "";
				return false;
			}
		}
		
		static protected ParameterRegistry Instance
		{
			get {
				if (instance == null)
					instance = new ParameterRegistry();
				return instance;
			}
		}
		
		protected void RegisterSubsystemHandlersInternal(string subsystemName,
														 SetParameterHandler setHandler, 
														 GetParameterHandler getHandler)
		{
			// SubsystemHandlers handlers; (unused)
			UnregisterSubsystemHandlersInternal(subsystemName);
			getSetHandlers.Add(subsystemName, new SubsystemHandlers(subsystemName, setHandler, getHandler));
		}
		
		protected void UnregisterSubsystemHandlersInternal(string subsystemName)
		{
			SubsystemHandlers handlers;
			if (GetHandlersForSubsystem(subsystemName, out handlers))
				getSetHandlers.Remove(subsystemName);
		}
		
		protected bool SetParameterInternal (string subsystemName, string parameterName, string value)
		{
			SubsystemHandlers handlers;
			if (GetHandlersForSubsystem(subsystemName, out handlers))
				return handlers.setHandler(parameterName, value);
			else
				return false;
		}

		protected string GetParameterInternal (string subsystemName, string parameterName)
		{
			SubsystemHandlers handlers;
			string value;
            if (GetHandlersForSubsystem(subsystemName, out handlers))
                handlers.getHandler(parameterName, out value);
            else
                value = "";
			return value;
		}
	
#endregion ParameterRegistry Protected Members

#region ParameterRegistry Public Interface

		// The delegate returns true if setting the parameter succeeded; false otherwise
		public delegate bool SetParameterHandler(string parameterName, string parameterValue);
		// The delegate returns true if the value was returned in the out parameter; false otherwise
		public delegate bool GetParameterHandler(string parameterName, out string parameterValue);

		static public void RegisterSubsystemHandlers(string subsystemName,
													 SetParameterHandler setHandler, 
													 GetParameterHandler getHandler)
		{
			Instance.RegisterSubsystemHandlersInternal(subsystemName, setHandler, getHandler);
		}
		

		static public void UnregisterSubsystemHandlers(string subsystemName)
		{
			Instance.UnregisterSubsystemHandlersInternal(subsystemName);
		}
		
		static public bool SetParameter (string subsystemNameAndParameter, string value)
		{
			string subsystemName;
			string parameterName;
			if (BreakSubsystemAndParameter(subsystemNameAndParameter, out subsystemName, out parameterName))
				return Instance.SetParameterInternal(subsystemName, parameterName, value);
			else
				return false;
		}

		static bool SetParameter (string subsystemName, string parameterName, string value)
		{
			return Instance.SetParameterInternal(subsystemName, parameterName, value);
		}
		
		static public string GetParameter (string subsystemNameAndParameter)
		{
			string subsystemName;
			string parameterName;
			// Special case: if the subsystemNameAndParameter is
			// "Help", return the help for all the subsystems
			if (subsystemNameAndParameter == "Help") {
                string value = "";
				foreach (SubsystemHandlers handlers in Instance.getSetHandlers.Values) {
					value += "Documentation for subsystem " + handlers.subsystemName + "\r\n";
					value += Instance.GetParameterInternal(handlers.subsystemName, "Help") + "\r\n";
				}
				return value;
			}
			else if (BreakSubsystemAndParameter(subsystemNameAndParameter, out subsystemName, out parameterName))
				return Instance.GetParameterInternal(subsystemName, parameterName);
			else {
				return "";
			}
		}

		// Returns a string containing all the subsystems separated by spaces 
		static public string GetSubsystems ()
		{
			string s = "";
			foreach (SubsystemHandlers handlers in Instance.getSetHandlers.Values) {
				if (s != "")
					s += " ";
				s += handlers.subsystemName;
			}
			return s;
		}

		static public string GetParameter (string subsystemName, string parameterName)
		{
			return Instance.GetParameterInternal(subsystemName, parameterName);
		}

#endregion ParameterRegistry Public Interface

	}

	public class ClientParameter
	{
		public static string GetClientParameter(string parameterName)
		{
			return ParameterRegistry.GetParameter(parameterName);
		}
		
		public static void SetClientParameter(string parameterName, string parameterValue)
		{
			ParameterRegistry.SetParameter(parameterName, parameterValue);
		}
	}
	
}
