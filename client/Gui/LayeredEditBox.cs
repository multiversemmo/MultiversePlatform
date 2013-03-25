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
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Drawing;
using System.Drawing.Text;
using System.Xml;

using Axiom.MathLib;
using Axiom.Core;
using Axiom.Input;

using FontFamily = System.Drawing.FontFamily;

namespace Multiverse.Gui {
	/// <summary>
	///   Variant of EditBox that allows me to control which widget
	///   is in front.  I would like to redo a great deal of this design, 
	///   so that there are two components.  One for text display, and 
	///   one for text input.
	/// </summary>
	public class LayeredEditBox : LayeredStaticText {
		protected bool textMasked = false;
		protected char maskChar = '*';
		protected float lastTextOffset = 0;
                 
        bool dragging = false;
		int dragAnchorIndex = 0;
		int maxTextLength = int.MaxValue;

		bool readOnly = false;
		int selectionStartIndex = 0;
		int selectionEndIndex = 0;
		TextStyle selectedTextStyle;

        Dictionary<KeyCodes, bool> keysPressed = new Dictionary<KeyCodes, bool>();

        protected StringBuilder editBuffer = new StringBuilder();
        protected string editText = string.Empty;
		protected int caretIndex = 0;
		// caret (cursor)
		protected TextureInfo caret;

		// What they have just typed, but not entered
		protected string historyTmp;
		// History lines
		protected List<string> historyLines = null;
		// index from the end of the history list.
		// a value of 0 corresponds to the most recent entry in the list
		// a value of -1 corresponds to an entry that is not yet in the list
		protected int historyIndex = -1;


		#region Event Declarations

		#region Unused events
#if UNUSED_EVENTS
		/// <summary>
		///		The read-only mode for the edit box has been changed.
		/// </summary>
		public event EventHandler ReadOnlyChanged;
		/// <summary>
		///		The masked rendering mode (password mode) has been changed.
		/// </summary>
		public event EventHandler MaskedRenderingModeChanged;
		/// <summary>
		///		The code point (character) to use for masked text has been changed.
		/// </summary>
		public event EventHandler MaskCodePointChanged;
		/// <summary>
		///		The validation string has been changed.
		/// </summary>
		public event EventHandler ValidationStringChanged;
		/// <summary>
		///		The maximum allowable string length has been changed.
		/// </summary>
		public event EventHandler MaximumTextLengthChanged;
		/// <summary>
		///		Some operation has made the current text invalid with regards to the validation string.
		/// </summary>
		public event EventHandler TextInvalidated;
		/// <summary>
		///		The user attempted to modify the text in a way that would have made it invalid.
		/// </summary>
		public event EventHandler InvalidEntryAttempted;
        /// <summary>
        ///		The text carat (insert point) has changed.
        /// </summary>
        public event EventHandler CaratMoved;
        /// <summary>
        ///		The text widget has been enabled.
        /// </summary>
        public event EventHandler Enabled;
        /// <summary>
        ///		The text widget has been disabled.
        /// </summary>
        public event EventHandler Disabled;
#endif // UNUSED_EVENTS
		#endregion Unused events

        public event EventHandler PostCharacter;
		/// <summary>
		///		The current text selection has changed.
		/// </summary>
		public event EventHandler TextSelectionChanged;
		/// <summary>
		///		The number of characters in the edit box has reached the current maximum.
		/// </summary>
		public event EventHandler EditboxFull;
		/// <summary>
		///		The user has accepted the current text by pressing Return, Enter, or Tab.
		/// </summary>
		public event EventHandler TextAccepted;

        /// <summary>
        ///   New events for the Interface system
        /// </summary>
        public event EventHandler EnterPressed;
        public event EventHandler EscapePressed;
        public event EventHandler SpacePressed;
        public event EventHandler TabPressed;
        // Use the BlahEvent version instead of the Blah version, since
        // Char is a class name.
        public event KeyboardEventHandler CharEvent;

		#endregion Event Declarations

		public LayeredEditBox(string name, Window clipWindow)
			: base(name, clipWindow) {
			selectedTextStyle = new TextStyle(this.NormalTextStyle);
			selectedTextStyle.bgColor.a = 0.5f;
			selectedTextStyle.bgColor.r = 0.5f;
			selectedTextStyle.bgColor.g = 0.5f;
			selectedTextStyle.bgColor.b = 0.5f;
			this.TextSelectionChanged += this.GenerateTextChunks;
			this.ScrollFromBottom = true;
		}

		/// <summary>
		///   Setting this will reset the selection to be at the caret.
		///   This is modifying the edit version of the text rather than
		///   what is drawn (which may be mask characters).
		/// </summary>
		public void SetText(string str, bool moveCaretToEnd, bool resetSelection) {
			editBuffer.Length = 0;
            editBuffer.Append(str);
            SetText(editBuffer, moveCaretToEnd, resetSelection);
        }

        protected void SetText(StringBuilder buffer, bool moveCaretToEnd, bool resetSelection) {
			if (moveCaretToEnd || caretIndex > buffer.Length)
                CaretIndex = buffer.Length;
			else if (caretIndex < 0)
				CaretIndex = 0;
            if (resetSelection) {
                SelectionStartIndex = caretIndex;
			    SelectionEndIndex = caretIndex;
            }
            if (textMasked)
                base.SetText(new string(maskChar, buffer.Length));
            else
                base.SetText(buffer.ToString());
		}
		public override void SetText(string str) {
			SetText(str, true, true);
		}

		// variant that is called from TextSelectionChanged
		protected void GenerateTextChunks(object sender, EventArgs e) {
			GenerateTextChunks(GetAllText());
		}

		protected override void GenerateTextChunks(string str) {
			List<TextChunk> chunks = this.TextChunks;
			chunks.Clear();
            if (str == null)
                str = string.Empty;
			// The portion before the highlight section
			if (selectionStartIndex > 0) {
				TextChunk chunk = 
                    new TextChunk(new TextRange(0, selectionStartIndex), 
                                  new TextStyle(this.NormalTextStyle));
				chunks.Add(chunk);
			}

			if (selectionEndIndex > selectionStartIndex) {
				TextChunk chunk = 
                    new TextChunk(new TextRange(selectionStartIndex, selectionEndIndex), 
                                  new TextStyle(this.SelectedTextStyle));
				chunks.Add(chunk);
			}

			if (selectionEndIndex <= str.Length) {
				TextChunk chunk = 
                    new TextChunk(new TextRange(selectionEndIndex, str.Length),
                                  new TextStyle(this.NormalTextStyle));
				chunks.Add(chunk);
			}
		}

        /// <summary>
        ///   This computes how much vertical space would be required to draw 
        ///   all the text, wrapping based on window width.
        /// </summary>
        /// <returns>number of pixels of vertical space required to draw the text</returns>
        public override float GetTextHeight(bool includeEmpty) {
            float rv = base.GetTextHeight(includeEmpty);
            if (rv == 0.0)
                // if there is no text, but we have our cursor, make sure
                // we leave room for this empty line.
                return this.Font.LineSpacing;
            return rv;
        }

		protected override void DrawSelf(float z) {
            if (lines.Count == 0) {
                // add an empty line
                TextRange range = new TextRange();
                range.start = 0;
                range.end = 0;
                lines.Add(range);
            }
			base.DrawSelf(z);
			Rect clipRect = this.PixelRect;
			// Draw the caret
			PointF pt = GetOffset(caretIndex);
			Vector3 drawPos = new Vector3(pt.X, pt.Y, z);

            float zOffset = (int)frameStrata * GuiZFrameStrataStep +
                            (int)layerLevel * GuiZLayerLevelStep +
                            (int)frameLevel * GuiZFrameLevelStep;
            float maxOffset = (int)FrameStrata.Maximum * GuiZFrameStrataStep;
            float curOffset = maxOffset - zOffset;

            drawPos.z = drawPos.z + curOffset - (int)SubLevel.Caret * GuiZSubLevelStep;
			if (drawPos.x < clipRect.Left)
				drawPos.x = clipRect.Left;
			else if (drawPos.x + caret.Width > clipRect.Right)
				drawPos.x = clipRect.Right - caret.Width;
			SizeF caretSize = new SizeF(caret.Width, this.Font.LineSpacing);
			ColorRect caretColorRect = new ColorRect(ColorEx.White);
			caret.Draw(drawPos, caretSize, clipRect, caretColorRect);
		}

		/// <summary>
		///   Set the widget's history list to be the one of the frame
		/// </summary>
		/// <param name="hist"></param>
		public void SetHistory(List<string> hist) {
			historyLines = hist;
		}


		#region Key Handlers
		/// <summary>
		///		Processing for the backspace key.
		/// </summary>
        protected void HandleBackspace() {
            if (!ReadOnly) {
                StringBuilder tmpBuffer = new StringBuilder(editBuffer.ToString());
                int tmpIndex = caretIndex;

                if (SelectionLength != 0) {
                    tmpBuffer = tmpBuffer.Remove(SelectionStartIndex, SelectionLength);
                    tmpIndex = SelectionStartIndex;
                }  else if (CaretIndex > 0) {
                    tmpBuffer = tmpBuffer.Remove(CaretIndex - 1, 1);
                    tmpIndex = CaretIndex - 1;
                }
                if (IsStringValid(tmpBuffer.ToString())) {
                    // erase selection using mode that does not modify 'text' (we just want to update state)
                    EraseSelectedText(false);
                    // set text to the newly modified string
                    editBuffer = tmpBuffer;

                    // set the displayed text, update the caret and selection
                    SetText(editBuffer, false, true);
                    CaretIndex = tmpIndex;
                    HandleTextChanged();
                } else {
                    // trigger invalid modification attempted event
                    OnInvalidEntryAttempted(new EventArgs());
                }
            }
        }

		/// <summary>
		///		Processing for the delete key.
		/// </summary>
		protected void HandleDelete() {
			if (!ReadOnly) {
                StringBuilder tmpBuffer = new StringBuilder(editBuffer.ToString());
                int tmpIndex = caretIndex;

                if (SelectionLength != 0) {
                    tmpBuffer = tmpBuffer.Remove(SelectionStartIndex, SelectionLength);
                    tmpIndex = SelectionStartIndex;
                } else if (CaretIndex < tmpBuffer.Length) {
                    tmpBuffer = tmpBuffer.Remove(CaretIndex, 1);
                    tmpIndex = CaretIndex;
                }
                
                if (IsStringValid(tmpBuffer.ToString())) {
				    // erase selection using mode that does not modify 'text' (we just want to update state)
					EraseSelectedText(false);
					// set text to the newly modified string
                    editBuffer = tmpBuffer;
                    
                    // set the displayed text, update the caret and selection
                    SetText(editBuffer, false, true);
                    CaretIndex = tmpIndex;
                    HandleTextChanged();
			    } else {
				    // trigger invalid modification attempted event
					OnInvalidEntryAttempted(new EventArgs());
				}
			}
		}

		/// <summary>
		///		Processing to move carat one character left.
		/// </summary>
		/// <param name="sysKeys">Current state of the system keys.</param>
        protected void HandleCharLeft(ModifierKeys sysKeys) {
			if (caretIndex > 0) {
				this.CaretIndex = caretIndex - 1;
			}

            if ((sysKeys & ModifierKeys.Shift) > 0) {
				SetSelection(caretIndex, dragAnchorIndex);
			} else {
				ClearSelection();
			}
		}

		/// <summary>
		///		Processing to move carat one word left.
		/// </summary>
		/// <param name="sysKeys">Current state of the system keys.</param>
        protected void HandleWordLeft(ModifierKeys sysKeys) {
			if (caretIndex > 0) {
				this.CaretIndex = TextUtil.GetWordStartIndex(text, caretIndex - 1);
			}

            if ((sysKeys & ModifierKeys.Shift) > 0) {
				SetSelection(caretIndex, dragAnchorIndex);
			} else {
				ClearSelection();
			}
		}

		/// <summary>
		///		Processing to move carat one character right.
		/// </summary>
		/// <param name="sysKeys">Current state of the system keys.</param>
        protected void HandleCharRight(ModifierKeys sysKeys) {
			if (caretIndex < text.Length) {
				CaretIndex = caretIndex + 1;
			}

            if ((sysKeys & ModifierKeys.Shift) > 0) {
				SetSelection(caretIndex, dragAnchorIndex);
			} else {
				ClearSelection();
			}
		}

		/// <summary>
		///		Processing to move carat one word right.
		/// </summary>
		/// <param name="sysKeys">Current state of the system keys.</param>
        protected void HandleWordRight(ModifierKeys sysKeys) {
			if (caretIndex < text.Length) {
				CaretIndex = TextUtil.GetNextWordStartIndex(text, caretIndex + 1);
			}

            if ((sysKeys & ModifierKeys.Shift) > 0) {
				SetSelection(caretIndex, dragAnchorIndex);
			} else {
				ClearSelection();
			}
		}

		/// <summary>
		///		Processing to move carat to the start of the text.
		/// </summary>
		/// <param name="sysKeys">Current state of the system keys.</param>
        protected void HandleHome(ModifierKeys sysKeys) {
			if (caretIndex > 0) {
				CaretIndex = 0;
			}

            if ((sysKeys & ModifierKeys.Shift) > 0) {
				SetSelection(caretIndex, dragAnchorIndex);
			} else {
				ClearSelection();
			}
		}

		/// <summary>
		///		Processing to move carat to the end of the text.
		/// </summary>
		/// <param name="sysKeys">Current state of the system keys.</param>
        protected void HandleEnd(ModifierKeys sysKeys) {
			if (caretIndex < text.Length) {
				CaretIndex = text.Length;
			}

            if ((sysKeys & ModifierKeys.Shift) > 0) {
				SetSelection(caretIndex, dragAnchorIndex);
			} else {
				ClearSelection();
			}
		}
		#endregion Key Handlers


        protected void HandleHistoryNext(ModifierKeys sysKeys) {
			// do we have command history?
			if (historyLines == null)
				return;
			// are we on the last line already?
			if (historyIndex == -1)
				return;
			int firstEntry = historyLines.Count - 1;
			if (historyIndex == 0) {
				historyIndex--;
				SetText(historyTmp, true, true);
			} else {
				historyIndex--;
				int offset = firstEntry - historyIndex;
				SetText(historyLines[offset], true, true);
			}
		}
        protected void HandleHistoryPrev(ModifierKeys sysKeys) {
			// do we have command history?
			if (historyLines == null)
				return;
			int firstEntry = historyLines.Count - 1;
			// are we already on the first line?
			if (historyIndex == firstEntry)
				return;
			// are we on the last line
			if (historyIndex == -1) {
				historyIndex++;
				historyTmp = this.Text;
				int offset = firstEntry - historyIndex;
				SetText(historyLines[offset], true, true);
			} else {
				historyIndex++;
				int offset = firstEntry - historyIndex;
				SetText(historyLines[offset], true, true);
			}
		}
		#region Miscellaneous methods from EditBox

		/// <summary>
		///		return true if the Editbox has input focus.
		/// </summary>
		/// <value>
		///		true if the Editbox has keyboard input focus.
		///		false if the Editbox does not have keyboard input focus.
		/// </value>
		public bool HasInputFocus {
			get {
				return this.IsActive;
			}
		}

		/// <summary>
		///		Using the current regex, the supplied text is validated.
		/// </summary>
		/// <param name="text">Text to validate.</param>
		/// <returns>True if the text is valid according to the validation string, false otherwise.</returns>
		protected bool IsStringValid(string text) {
			return true;
		}

		/// <summary>
		///		Erase the currently selected text.
		/// </summary>
		/// <param name="modifyText">
		///		When true, the actual text will be modified.  
		///		When false, everything is done except erasing the characters.
		///	</param>
		protected void EraseSelectedText(bool modifyText) {
			if (SelectionLength != 0) {
				// setup new carat position and remove selection highlight
				CaretIndex = SelectionStartIndex;

				// erase the selected characters (if required)
				if (modifyText) {
					// remove the text
					editBuffer.Remove(SelectionStartIndex, SelectionLength);
                    SetText(editBuffer, false, true);

					// trigger notifications that the text has changed
					OnTextChanged(new EventArgs());
				}
				ClearSelection();
			}
		}
		/// <summary>
		///		Clear the current selection setting.
		/// </summary>
		protected void ClearSelection() {
    		SetSelection(caretIndex, caretIndex);
		}

		public void SetSelection(int startPos, int endPos) {
			// ensure selection start point is within the valid range
			if (startPos > textBuffer.Length) {
				startPos = textBuffer.Length;
			}

			// ensure selection end point is within the valid range
			if (endPos > textBuffer.Length) {
				endPos = textBuffer.Length;
			}

			// swap values if start is after end
			if (startPos > endPos) {
				int tmp = endPos;
				endPos = startPos;
				startPos = tmp;
			}

			// only change state if values are different
			if ((startPos != selectionStartIndex) || endPos != selectionEndIndex) {
				// setup selection
				SelectionStartIndex = startPos;
				SelectionEndIndex = endPos;

                log.DebugFormat("Set selection: {0} {1}", startPos, endPos);
                // event trigger
				TextSelectionChanged(this, new EventArgs());
			}
		}
		#endregion

		#region Overridden Event Trigger Methods

		protected internal override void OnMouseDown(MouseEventArgs e) {
			// base class handling
			base.OnMouseDown(e);

            if (GuiSystem.IsMouseButtonSet(e.Button, MouseButtons.Left)) {
				// grab inputs
				CaptureInput();

				// handle mouse down
				ClearSelection();
				dragging = true;
                Rect absRect = GetVisibleTextArea();
                PointF pt = new PointF(e.X - absRect.Left, e.Y - absRect.Top);
				dragAnchorIndex = GetTextIndexFromPosition(pt);
				this.CaretIndex = dragAnchorIndex;

				e.Handled = true;
			}
		}

		protected internal override void OnMouseUp(MouseEventArgs e) {
			// base class processing
			base.OnMouseUp(e);

            if (GuiSystem.IsMouseButtonSet(e.Button, MouseButtons.Left)) {
				//ReleaseInput();

				e.Handled = true;
			}
		}

		protected internal override void OnMouseDoubleClicked(MouseEventArgs e) {
			// base class processing
			base.OnMouseDoubleClicked(e);

			if (GuiSystem.IsMouseButtonSet(e.Button, MouseButtons.Left)) {
				// if masked, set up to select all
				if (TextMasked) {
					dragAnchorIndex = 0;
					this.CaretIndex = text.Length;
				} else {
					// not masked, so select the word that was double clicked
                    dragAnchorIndex = TextUtil.GetWordStartIndex(text, (caretIndex == text.Length) ? caretIndex : caretIndex + 1);
					CaretIndex = TextUtil.GetNextWordStartIndex(text, (caretIndex == text.Length) ? caretIndex : caretIndex + 1);
				}

				// perform actual selection operation
				SetSelection(dragAnchorIndex, caretIndex);

				e.Handled = true;
			}
		}

        //protected internal override void OnMouseTripleClicked(MouseEventArgs e) {
        //    // base class processing
        //    base.OnMouseTripleClicked(e);

        //    if (e.Button == MouseButton.Left) {
        //        dragAnchorIndex = 0;
        //        CaretIndex = text.Length;
        //        SetSelection(dragAnchorIndex, caretIndex);
        //        e.Handled = true;
        //    }
        //}

		protected internal override void OnMouseMoved(MouseEventArgs e) {
			// base class processing
			base.OnMouseMoved(e);

			if (dragging) {
                Rect absRect = GetVisibleTextArea();
                PointF pt = new PointF(e.X - absRect.Left, e.Y - absRect.Top);
                CaretIndex = GetTextIndexFromPosition(pt);
				SetSelection(caretIndex, dragAnchorIndex);
			}

			e.Handled = true;
		}

		protected internal override void OnCaptureLost(EventArgs e) {
			dragging = false;
            keysPressed.Clear();

			// base class processing
			base.OnCaptureLost(e);

			// e.Handled = true;
		}

        /// <summary>
        ///   Mark key down events as handled if we handle the associated
        ///   key press event.
        /// </summary>
        /// <param name="e"></param>
        protected internal override void OnKeyDown(KeyEventArgs e) {
            // base class processing
            base.OnKeyDown(e);

  			// only need to take notice if we have focus
            if (HasInputFocus && !ReadOnly) {
                // First see if it is a special character
                switch (e.Key) {
                    case KeyCodes.LeftShift:
                    case KeyCodes.RightShift:
                    case KeyCodes.Backspace:
                    case KeyCodes.Delete:
                    case KeyCodes.Return:
                    case KeyCodes.Escape:
                    case KeyCodes.Enter:
                    case KeyCodes.Left:
                    case KeyCodes.Right:
                    case KeyCodes.Home:
                    case KeyCodes.End:
                        e.Handled = true;
                        break;
                    case KeyCodes.Up:
                    case KeyCodes.Down:
                    case KeyCodes.A:
                    case KeyCodes.B:
                    case KeyCodes.C:
                    case KeyCodes.D:
                    case KeyCodes.F:
                    case KeyCodes.K:
                    case KeyCodes.U:
                    case KeyCodes.V:
                    case KeyCodes.W:
                    case KeyCodes.X:
                        if ((e.Modifiers & ModifierKeys.Control) > 0)
                            e.Handled = true;
                        break;
                    default:
                        break;
                } // switch
                // If we didn't have special handling for that character, see if
                // we can handle it as a displayed character
                if (!e.Handled && !e.IsAltDown && !e.IsControlDown && 
                    this.Font.IsCharacterAvailable(e.KeyChar))
                    e.Handled = true;
                // Space and Tab may or may not be displayed, but create a keydown, 
                // so that we will generate a keyPress event for them.
                if (!e.Handled)
                {
                    switch (e.Key)
                    {
                        case KeyCodes.Space:
                        case KeyCodes.Tab:
                            e.Handled = true;
                            break;
                        default:
                            break;
                    }
                }
                if (e.Handled)
                    keysPressed[e.Key] = true;
            }
        }

        protected internal override void OnKeyPress(KeyEventArgs e) {
			// base class processing
			base.OnKeyPress(e);

			// only need to take notice if we have focus
            if (HasInputFocus && !ReadOnly) {
                // First see if it is a special character
                switch (e.Key) {
                    case KeyCodes.LeftShift:
                    case KeyCodes.RightShift:
                        if (SelectionLength == 0) {
                            dragAnchorIndex = CaretIndex;
                        }
                        e.Handled = true;
                        break;
                    case KeyCodes.Backspace:
                        HandleBackspace();
                        e.Handled = true;
                        break;
                    case KeyCodes.Delete:
                        HandleDelete();
                        e.Handled = true;
                        break;
                    case KeyCodes.Escape:
                        // Pass the event that is used for the ui scripting
                        OnEscapePressed(new EventArgs());
                        e.Handled = true;
                        break;
                    case KeyCodes.Space:
                        // Pass the event that is used for the ui scripting
                        OnSpacePressed(new EventArgs());
                        // We don't mark this handled here, since we may also want to display it
                        break;
                    case KeyCodes.Tab:
                        // Pass the event that is used for the ui scripting
                        OnTabPressed(new EventArgs());
                        // We don't mark this handled here, since we may also want to display it
                        break;
                    case KeyCodes.Return:
                    case KeyCodes.Enter:
                        // Pass the event that is used for the ui scripting
                        OnEnterPressed(new EventArgs());
                        // fire input accepted event
                        OnTextAccepted(new EventArgs());
                        e.Handled = true;
                        break;
                    case KeyCodes.Left:
                        if ((e.Modifiers & ModifierKeys.Control) > 0) {
                            HandleWordLeft(e.Modifiers);
                            e.Handled = true;
                        } else {
                            HandleCharLeft(e.Modifiers);
                            e.Handled = true;
                        }
                        break;
                    case KeyCodes.Right:
                        if ((e.Modifiers & ModifierKeys.Control) > 0) {
                            HandleWordRight(e.Modifiers);
                            e.Handled = true;
                        } else {
                            HandleCharRight(e.Modifiers);
                            e.Handled = true;
                        }
                        break;
                    case KeyCodes.Up:
                        if ((e.Modifiers & ModifierKeys.Control) > 0) {
                            HandleHistoryPrev(e.Modifiers);
                            e.Handled = true;
                        }
                        break;
                    case KeyCodes.Down:
                        if ((e.Modifiers & ModifierKeys.Control) > 0) {
                            HandleHistoryNext(e.Modifiers);
                            e.Handled = true;
                        }
                        break;
                    case KeyCodes.Home:
                        HandleHome(e.Modifiers);
                        e.Handled = true;
                        break;
                    case KeyCodes.End:
                        HandleEnd(e.Modifiers);
                        e.Handled = true;
                        break;
                    case KeyCodes.A:
                        if ((e.Modifiers & ModifierKeys.Control) > 0) {
                            SetSelection(0, editBuffer.Length);
                            e.Handled = true;
                        }
                        break;
                    case KeyCodes.B:
                        if ((e.Modifiers & ModifierKeys.Control) > 0) {
                            HandleCharLeft(e.Modifiers);
                            e.Handled = true;
                        }
                        break;
                    case KeyCodes.C:
                        if ((e.Modifiers & ModifierKeys.Control) > 0) {
                            // TODO: Copy selection
                        }
                        break;
                    case KeyCodes.D:
                        if ((e.Modifiers & ModifierKeys.Control) > 0) {
                            HandleDelete();
                            e.Handled = true;
                        }
                        break;
                    case KeyCodes.F:
                        if ((e.Modifiers & ModifierKeys.Control) > 0) {
                            HandleCharRight(e.Modifiers);
                            e.Handled = true;
                        }
                        break;
                    case KeyCodes.K:
                        if ((e.Modifiers & ModifierKeys.Control) > 0) {
                            // TODO: Kill after
                        }
                        break;
                    case KeyCodes.U:
                        if ((e.Modifiers & ModifierKeys.Control) > 0) {
                            // TODO: Kill before
                        }
                        break;
                    case KeyCodes.V:
                        if ((e.Modifiers & ModifierKeys.Control) > 0) {
                            // TODO: Paste selection
                        }
                        break;
                    case KeyCodes.W:
                        if ((e.Modifiers & ModifierKeys.Control) > 0) {
                            // TODO: Kill word before
                        }
                        break;
                    case KeyCodes.X:
                        if ((e.Modifiers & ModifierKeys.Control) > 0) {
                            // TODO: Cut selection
                        }
                        break;
                } // switch

                // If we didn't have special handling for that character, see if
                // we can handle it as a displayed character
                if (!e.Handled && !e.IsAltDown && !e.IsControlDown && 
                    this.Font.IsCharacterAvailable(e.KeyChar)) {
                    // backup current text
                    StringBuilder tmpBuffer = new StringBuilder(editBuffer.ToString());

                    tmpBuffer = tmpBuffer.Remove(SelectionStartIndex, SelectionLength);

                    // if there is room
                    if (tmpBuffer.Length < maxTextLength) {
                        tmpBuffer = tmpBuffer.Insert(SelectionStartIndex, e.KeyChar.ToString());

                        if (IsStringValid(tmpBuffer.ToString())) {
                            // erase selection using mode that does not modify 'text' (we just want to update state)
                            EraseSelectedText(false);
                            // set text to the newly modified string
                            editBuffer = tmpBuffer;
                            // advance carat
                            CaretIndex++;
                            // set the displayed text, update the caret and selection
                            SetText(editBuffer, false, true);
                            HandleTextChanged();
                            OnChar(e);
                        } else {
                            // trigger invalid modification attempted event
                            OnInvalidEntryAttempted(new EventArgs());
                        }
                    } else {
                        // trigger text box full event
                        OnEditboxFull(new EventArgs());
                    }
                    e.Handled = true;
                }
                // Space and Tab may or may not be displayed, but mark them handled if we ran 
                // the OnSpacePressed and OnTabPressed events.
                if (!e.Handled)
                {
                    switch (e.Key)
                    {
                        // Ideally, I would check to see if we actually have a script registered
                        // but unfortunately, I don't have any good way of getting that information.
                        // Instead, just mark space and tab as handled.
                        case KeyCodes.Space:
                        case KeyCodes.Tab:
                            e.Handled = true;
                            break;
                        default:
                            break;
                    }
                }
            }
			if (PostCharacter != null)
				PostCharacter(this, e);
		}

        /// <summary>
        ///   Mark key up events as handled if we handle the associated
        ///   key press event.
        /// </summary>
        /// <param name="e"></param>
        protected internal override void OnKeyUp(KeyEventArgs e) {
            // base class processing
            base.OnKeyUp(e);

            // only need to take notice if we have focus
            if (HasInputFocus) {
                // Basically, if we got the key down event (and handled it),
                // handle the key up event.
                if (keysPressed.ContainsKey(e.Key)) {
                    keysPressed.Remove(e.Key);
                    e.Handled = true;
                }
            }
        }

		protected internal override void OnTextChanged(EventArgs e) {
			// base class processing
			base.OnTextChanged(e);

			// clear selection
			ClearSelection();

			// make sure carat is within the text
			if (this.CaretIndex > text.Length) {
				this.CaretIndex = text.Length;
			}

			// e.Handled = true;
		}

		#endregion Overridden Event Trigger Methods

		#region Other event trigger methods
		protected internal virtual void OnTextSelectionChanged(EventArgs e) {
			TextSelectionChanged(this, e);
		}

		/// <summary>
		///		Event fired internally when the user attempted to make a change to the edit box that would
		///		have caused it to fail validation.
		/// </summary>
		/// <param name="e">Event arguments.</param>
		protected internal virtual void OnInvalidEntryAttempted(EventArgs e) {
			return;
		}

		/// <summary>
		///		Event fired internally when the edit box text has reached the set maximum length.
		/// </summary>
		/// <param name="e">Event arguments.</param>
		protected internal virtual void OnEditboxFull(EventArgs e) {
			if (EditboxFull != null) {
				EditboxFull(this, e);
			}
		}

		/// <summary>
		///		Event fired internally when the user accepts the edit box text by pressing Return, Enter, or Tab.
		/// </summary>
		/// <param name="e">Event arguments.</param>
		protected internal virtual void OnTextAccepted(EventArgs e) {
			if (TextAccepted != null)
				TextAccepted(this, e);
		}

        protected internal virtual void OnEnterPressed(EventArgs e) {
            if (EnterPressed != null)
                EnterPressed(this, e);
        }
        protected internal virtual void OnEscapePressed(EventArgs e) {
            if (EscapePressed != null)
                EscapePressed(this, e);
        }
        protected internal virtual void OnSpacePressed(EventArgs e) {
            if (SpacePressed != null)
                SpacePressed(this, e);
        }
        protected internal virtual void OnTabPressed(EventArgs e) {
            if (TabPressed != null)
                TabPressed(this, e);
        }
        protected internal virtual void OnChar(KeyEventArgs e) {
            if (CharEvent != null)
                CharEvent(this, e);
        }

		#endregion

		public bool TextMasked {
			get { return textMasked; }
			set {
				if (textMasked != value) {
                    textMasked = value;
                    // regenerate the text buffer contents based on the edit buffer
                    SetText(editBuffer, false, false);
                    this.Dirty = true;
                }
			}
		}
		public char MaskChar {
			get { return maskChar; }
			set {
				if (maskChar != value) {
                    maskChar = value;
                    // regenerate the text buffer contents based on the edit buffer
                    SetText(editBuffer, false, false);
                    this.Dirty = true;
                }
			}
		}
		public bool ReadOnly {
			get { return readOnly; }
			set { readOnly = value; }
		}
		public int SelectionStartIndex {
			get { return selectionStartIndex; }
			set {
                if (selectionStartIndex != value) {
                    selectionStartIndex = value;
                    OnTextSelectionChanged(null);
                    this.Dirty = true;
                }
			}
		}
		public int SelectionEndIndex {
			get { return selectionEndIndex; }
			set {
                if (selectionEndIndex != value) {
                    selectionEndIndex = value;
                    OnTextSelectionChanged(null);
                    this.Dirty = true;
                }
			}
		}
		public int SelectionLength {
			get { return selectionEndIndex - selectionStartIndex; }
		}
		public TextStyle SelectedTextStyle {
			get { return selectedTextStyle; }
			set {
                ReplaceStyle(selectedTextStyle, value);
                selectedTextStyle = value;
			}
		}
		public int CaretIndex {
			get {
				return caretIndex;
			}
			set {
				if (value != caretIndex) {
                    caretIndex = value;
                    this.Dirty = true;
                }
			}
		}
		public TextureInfo Caret {
			get {
				return caret;
			}
			set {
				if (value != caret) {
                    caret = value;
                    this.Dirty = true;
                }
			}
		}

        public string EditText {
            get {
                return editBuffer.ToString();
            }
        }

	}
}
