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

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;

using Microsoft.MultiverseInterfaceStudio.FrameXml.Serialization;

namespace Microsoft.MultiverseInterfaceStudio.FrameXml.Controls
{
	public class FrameDesigner : BaseControlDesigner
	{
		private const string enableMovingDisplay = "Make frame movable";
		private const string disableMovingDisplay = "Make frame fixed";

		const string startMoveScript = "self:StartMoving();";
		const string stopMoveScript = "self:StopMovingOrSizing();";

		private DesignerActionListCollection actionLists;

        /// <summary>
        /// Gets the design-time action lists supported by the component associated with the designer.
        /// </summary>
        /// <value></value>
        /// <returns>The design-time action lists supported by the component associated with the designer.</returns>
		public override DesignerActionListCollection ActionLists
		{
			get
			{
				BaseControl baseControl = this.Component as BaseControl;

				if (actionLists == null && baseControl != null && baseControl.HasActions)
				{
					actionLists = new DesignerActionListCollection();

					FrameDesignerActionList actionList = new FrameDesignerActionList(baseControl);
                    actionList.ComponentChanged += new EventHandler(OnComponentChanged);

                    actionLists.Add(actionList);
				}

				return actionLists;
			}
		}

        /// <summary>
        /// Creates a method signature in the source code file for the default event on the component and navigates the user's cursor to that location.
        /// </summary>
        /// <exception cref="T:System.ComponentModel.Design.CheckoutException">An attempt to check out a file that is checked into a source code management program failed.</exception>
		public override void DoDefaultAction()
        {
            base.DoDefaultAction();

            BaseControl baseControl = this.Component as BaseControl;

            if (baseControl == null)
                return;
            if (baseControl.Inherited)
                return;

			if (baseControl.DefaultEventChoice.HasValue)
			{
				FrameType frameType = baseControl.SerializationObject as FrameType;
				if (frameType == null)
					return;

				if (frameType.Scripts.Count == 0)
					frameType.Scripts.Add(new ScriptsType());

				IDictionary<EventChoice, string> events = frameType.Scripts[0].Events;
				EventChoice eventChoice = baseControl.DefaultEventChoice.Value;
				string eventHandlerName;

				if (events.ContainsKey(eventChoice))
				{
					eventHandlerName = events[eventChoice].Trim().TrimEnd(';').TrimEnd(')').TrimEnd('(');
				}
				else
				{
					eventHandlerName = String.Format("{0}_{1}", frameType.name, eventChoice);
					events.Add(eventChoice, eventHandlerName + "();");
				}

				//LuaInterface luaInterface = new LuaInterface(baseControl.DesignerLoader);
				//luaInterface.CreateShowFunction(eventHandlerName);
			}
        }

        private void OnComponentChanged(object sender, EventArgs e)
        {
            this.RaiseComponentChanged(null, null, null);
        }

		private class FrameDesignerActionList : DesignerActionList
		{
            private DesignerActionUIService designerActionUIService;
            private BaseControl control;

            /// <summary>
            /// Raised when the component changed.
            /// </summary>
            public event EventHandler ComponentChanged;

            /// <summary>
            /// Initializes a new instance of the <see cref="FrameDesignerActionList"/> class.
            /// </summary>
            /// <param name="control">The base control.</param>
            public FrameDesignerActionList(BaseControl control)
                : base(control)
            {
                if (control == null)
                    throw new ArgumentNullException("control");

                this.control = control;

                // Get the Designer Action UI service
                this.designerActionUIService = GetService(typeof(DesignerActionUIService)) as DesignerActionUIService;
            }

            /// <summary>
            /// Returns the collection of <see cref="T:System.ComponentModel.Design.DesignerActionItem"/> objects contained in the list.
            /// </summary>
            /// <returns>
            /// A <see cref="T:System.ComponentModel.Design.DesignerActionItem"/> array that contains the items in this list.
            /// </returns>
			public override DesignerActionItemCollection GetSortedActionItems()
			{
				// Don't add action to inherited controls
				if (!this.control.Inherited)
				{
					// TODO: refreshing action item display name
					//bool isMovingEnabled = this.IsMovingEnabled();

                    //string methodName = (isMovingEnabled) ? "DisableMoving" : "EnableMoving";
                    //string displayName = (isMovingEnabled) ? FrameDesigner.disableMovingDisplay : enableMovingDisplay;

                    return new DesignerActionItemCollection 
                            {
                                //new DesignerActionMethodItem(this, methodName, displayName, true)
                            };
				}

                return null;
			}

            /// <summary>
            /// Makes the frame movable.
            /// </summary>
			public void EnableMoving()
			{
                FrameType frameType = control.SerializationObject as FrameType;
				if (frameType != null)
				{
                    //frameType.movable = true;

					ScriptsType scripts = GetScripts(frameType, true);

                    if (scripts != null)
                    {
                        // Retrieve current OnMouseDown event
                        string currentOnDown = (scripts.Events.ContainsKey(EventChoice.OnMouseDown)) ? scripts.Events[EventChoice.OnMouseDown] : String.Empty;

                        // Append start move script, if not found
                        if (!currentOnDown.Contains(FrameDesigner.startMoveScript))
                            currentOnDown += FrameDesigner.startMoveScript;

                        // Set the event handler
                        scripts.Events[EventChoice.OnMouseDown] = currentOnDown;

                        // Retrieve current OnMouseUp event
                        string currentOnUp = (scripts.Events.ContainsKey(EventChoice.OnMouseUp)) ? scripts.Events[EventChoice.OnMouseUp] : String.Empty;

                        // APpend stop move script, if not found
                        if (!currentOnUp.Contains(FrameDesigner.stopMoveScript))
                            currentOnUp += FrameDesigner.stopMoveScript;

                        // Set the event handler
                        scripts.Events[EventChoice.OnMouseUp] = currentOnUp;
                    }

                    // Raise ComponentChangedEvent
                    this.OnComponentChanged(EventArgs.Empty);

                    // Refresh the Designer Action UI
                    designerActionUIService.Refresh(this.Component);
				}
			}

            /// <summary>
            /// Makes the frame fixed.
            /// </summary>
			public void DisableMoving()
			{
                FrameType frameType = control.SerializationObject as FrameType;
				if (frameType != null)
				{
                    //frameType.movable = false;

					ScriptsType scripts = FrameDesigner.FrameDesignerActionList.GetScripts(frameType, false);

					if (scripts != null)
					{
						if (scripts.Events.ContainsKey(EventChoice.OnMouseDown))
						{
                            // Retrieve current OnMouseDown event
                            string currentDown = scripts.Events[EventChoice.OnMouseDown] ?? String.Empty;

                            // Remove the start move snippet from the event handler script
							currentDown = currentDown.Replace(FrameDesigner.startMoveScript, String.Empty);

                            // If there is no code left, remove the event handler script altogether, update otherwise
							if (String.IsNullOrEmpty(currentDown))
								scripts.Events.Remove(EventChoice.OnMouseDown);
							else
								scripts.Events[EventChoice.OnMouseDown] = currentDown;
						}

						if (scripts.Events.ContainsKey(EventChoice.OnMouseUp))
						{
                            // Retrieve current OnMouseUp event
							string currentUp = (scripts.Events[EventChoice.OnMouseUp] ?? String.Empty);

                            // Remvoe the stop move snippet from the event handler sciprt
							currentUp = currentUp.Replace(FrameDesigner.stopMoveScript, String.Empty);

                            // If there is no code left, remove the event handler script altogether, update otherwise
							if (String.IsNullOrEmpty(currentUp))
								scripts.Events.Remove(EventChoice.OnMouseUp);
							else
								scripts.Events[EventChoice.OnMouseUp] = currentUp;
						}

                        // Remove the <Scripts> element if no events have handlers now
						if (scripts.Events.Count == 0)
							frameType.Scripts.Remove(scripts);
					}

                    // Raise ComponentChangedEvent
                    this.OnComponentChanged(EventArgs.Empty);

                    // Refresh the Designer Action UI
                    designerActionUIService.Refresh(this.Component);
				}
			}

            private static bool MergeScripts(ScriptsType from, ScriptsType to, EventChoice eventChoice)
            {
                if (from.Events.ContainsKey(eventChoice))
                {
                    if (to == null)
                    {
                        to = from;
                        return false;
                    }
                    else
                    {
                        // Retrieve current event handler script
                        string currentScript = to.Events.ContainsKey(eventChoice) && to.Events[eventChoice] != null ? to.Events[eventChoice] : String.Empty;

                        // Append script to be merged
                        currentScript += from.Events[eventChoice] ?? String.Empty;

                        // Update event handler script
                        to.Events[eventChoice] = currentScript;

                        return true;
                    }
                }

                return false;

            }

            private static ScriptsType GetScripts(FrameType frame, bool createOnNull)
            {
                ScriptsType result = null;

                if (frame.Scripts.Count == 0)
                {
                    if (createOnNull)
                    {
                        result = new ScriptsType();
                        frame.Scripts.Add(result);
                    }
                }
                else
                {
                    for (int index = frame.Scripts.Count - 1; index >= 0; index--)
                    {
                        ScriptsType scripts = frame.Scripts[index];

                        if (MergeScripts(scripts, result, EventChoice.OnMouseDown) ||
                            MergeScripts(scripts, result, EventChoice.OnMouseUp))
                        {
                            frame.Scripts.Remove(scripts);
                        }

                    }

                    if (result == null)
                        result = frame.Scripts[0];
                }

                return result;

            }

            private bool IsMovingEnabled()
            {
                FrameType frame = control.SerializationObject as FrameType;
                if (frame == null)
                    return false;

                ScriptsType scripts = GetScripts(frame, false);

                return frame.movable && scripts != null && scripts.Events.ContainsKey(EventChoice.OnMouseDown) &&
                                                           scripts.Events.ContainsKey(EventChoice.OnMouseUp);
            }

            /// <summary>
            /// Raises the <see cref="ComponentChanged"/> event.
            /// </summary>
            /// <param name="e"></param>
			protected void OnComponentChanged(EventArgs e)
			{
				if (ComponentChanged != null)
					ComponentChanged(this, e);
			}
		}
	}
}
