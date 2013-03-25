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
using System.Text;

namespace Multiverse.Interface
{
    public interface IUIObject 
    {
        /// <summary>
        ///   Get the alpha value for this widget.  This does not 
        ///   include the effect of the alpha values of the parent 
        ///   widgets.
        /// </summary>
        /// <returns>the alpha value for this widget</returns>
        float GetAlpha();
        /// <summary>
        ///   Gets the name of the widget
        /// </summary>
        /// <returns>name of the widget</returns>
        string GetName();
        /// <summary>
        ///   Sets the alpha value of a widget.
        /// </summary>
        /// <param name="alpha">alpha value of this widget.  A value of 0 corresponds to a transparent widget, while a value of 1 corresponds to an opaque widget</param>
        void SetAlpha(float alpha);
    }
    
    /// <summary>
    ///   IRegion is the base interface for the various widgets.
	/// </summary>
	public interface IRegion : IUIObject
	{
        /// <summary>
        ///   Get the y coordinate of the top edge of this widget
        /// </summary>
        /// <returns>the y coordinate in pixels of the top edge where 
        ///          higher values are higher on the screen</returns>
        int GetTop();
        /// <summary>
        ///   Get the y coordinate of the bottom edge of this widget
        /// </summary>
        /// <returns>the y coordinate in pixels of the bottom edge where 
        ///          higher values are higher on the screen</returns>
        int GetBottom();
        /// <summary>
        ///   Get the x coordinate of the left edge of this widget
        /// </summary>
        /// <returns>the x coordinate in pixels of the left edge</returns>
        int GetLeft();
        /// <summary>
        ///   Get the x coordinate of the right edge of this widget
        /// </summary>
        /// <returns>the x coordinate in pixels of the right edge</returns>
        int GetRight();
        /// <summary>
        ///   Get this region's parent frame.  This is the parent in layout, 
        ///   rather than the parent in inheritance.
        /// </summary>
        /// <returns>this region's parent frame</returns>
        IRegion GetParent();
        /// <summary>
		///   Clears all the anchors for this object
		/// </summary>
		void ClearAllPoints();		
		/// <summary>
		///   Gets the height of this widget
		/// </summary>
		/// <returns>the height of the widget in pixels</returns>
		int GetHeight();
		/// <summary>
		///   Sets the height of this widget
		/// </summary>
		/// <param name="height">the height in pixels</param>
		void SetHeight(int height);
		/// <summary>
		///   Gets the width of this widget
		/// </summary>
		/// <returns>the width of the widget in pixels</returns>
		int GetWidth();
		/// <summary>
		///   Sets the width of this widget
		/// </summary>
		/// <param name="width">the width in pixels</param>
		void SetWidth(int width);
		/// <summary>
		///   Mark the widget as hidden.  If the widget, or any of the 
		///   widget's parents are hidden, the widget will not be displayed.
		/// </summary>
		void Hide();
		/// <summary>
		///   Mark the widget as visible.  If the widget, or any of the 
		///   widget's parents are hidden, the widget will not be displayed.
		/// </summary>
		void Show();
		/// <summary>
		///   Check to see if the widget is marked as visible.  If the 
		///   widget, or any of the widget's parents are hidden, 
		///   the widget will not be displayed.
		/// </summary>
		/// <returns>Whether the widget is marked as visible</returns>
		bool IsVisible();
		/// <summary>
		///   Set an anchor point for the widget.
		/// </summary>
		/// <param name="point">Which point of our widget that we are anchoring</param>
		/// <param name="relativeTo">the name of the widget to which we are anchoring</param>
		/// <param name="relativePoint">Which point of the target widget that we are anchoring to</param>
		void SetPoint(string point, string relativeTo, string relativePoint);
		/// <summary>
		///   Set an anchor point for the widget.
		/// </summary>
		/// <param name="point">Which point of our widget that we are anchoring</param>
		/// <param name="relativeTo">the name of the widget to which we are anchoring</param>
		/// <param name="relativePoint">Which point of the target widget that we are anchoring to</param>
		/// <param name="xOffset">Horizontal offset in pixels from the target</param>
		/// <param name="yOffset">Vertical offset in pixels from the target.  Higher points on the screen have a larger yOffset.</param>
		void SetPoint(string point, string relativeTo, string relativePoint, int xOffset, int yOffset);
	}

	/// <summary>
	///   Class used for display of text.
	/// </summary>
	public interface IFontString : ILayeredRegion
	{
		/// <summary>
		///   Get the width of the text in the widget.
		/// </summary>
		/// <returns>the width in pixels of the string that is displayed</returns>
		int GetStringWidth();
		/// <summary>
		///   Get the text of the widget
		/// </summary>
		/// <returns>the text that is displayed</returns>
		string GetText();
		/// <summary>
		///   Set the text of the widget
		/// </summary>
		/// <param name="text">the text that will be displayed</param>
		void SetText(string text);
		/// <summary>
		///   Sets the horizontal justification of a widget.
		/// </summary>
		/// <param name="justify">justification value'</param>
		void SetJustifyH(string justify);
		/// <summary>
		///   Sets the vertical justification of a widget.
		/// </summary>
		/// <param name="justify">justification value'</param>
		void SetJustifyV(string justify);
        /// <summary>
        ///   Sets the color of the text
        /// </summary>
        /// <param name="r">Red value (from 0 to 1)</param>
        /// <param name="g">Green value (from 0 to 1)</param>
        /// <param name="b">Blue value (from 0 to 1)</param>
        void SetTextColor(float r, float g, float b);
        /// <summary>
        ///   Sets the color of the text
        /// </summary>
        /// <param name="r">Red value (from 0 to 1)</param>
        /// <param name="g">Green value (from 0 to 1)</param>
        /// <param name="b">Blue value (from 0 to 1)</param>
        /// <param name="a">Alpha value (from 0 to 1)</param>
        void SetTextColor(float r, float g, float b, float a);		
		/// <summary>
		///   Sets the text height
		/// </summary>
		/// <param name="pixelHeight">the lineheight in pixels</param>
		void SetTextHeight(int pixelHeight);

        #region Unimplemented
        //FontString:SetAlphaGradient(start,length)
        #endregion
	}

	/// <summary>
	///   Class used for displaying an image
	/// </summary>
	public interface ITexture : ILayeredRegion {
        /// <summary>
        ///   Sets the texture coordinates for this widget.  This allows for 
        ///   the display of a portion of the base texture.
        /// </summary>
        /// <param name="x0">x coordinate of the left (from 0 to 1)</param>
        /// <param name="y0">y coordinate of the top (from 0 to 1)</param>
        /// <param name="x1">x coordinate of the right (from 0 to 1)</param>
        /// <param name="y1">x coordinate of the bottom (from 0 to 1)</param>
        void SetTexCoord(float x0, float y0, float x1, float y1);
        /// <summary>
        ///    The complex way of setting up coordinates for situations 
        ///    where the texture is either not being laid out with its 
        ///    default orientation or not rectangular.
        /// </summary>
        /// <param name="ul_x">x coordinate of the upper left corner (from 0 to 1)</param>
        /// <param name="ul_y">y coordinate of the upper left corner (from 0 to 1)</param>
        /// <param name="ll_x">x coordinate of the lower left corner (from 0 to 1)</param>
        /// <param name="ll_y">y coordinate of the lower left corner (from 0 to 1)</param>
        /// <param name="ur_x">x coordinate of the upper right corner (from 0 to 1)</param>
        /// <param name="ur_y">y coordinate of the upper right corner (from 0 to 1)</param>
        /// <param name="lr_x">x coordinate of the lower right corner (from 0 to 1)</param>
        /// <param name="lr_y">y coordinate of the lower right corner (from 0 to 1)</param>
        void SetTexCoord(float ul_x, float ul_y, float ll_x, float ll_y, float ur_x, float ur_y, float lr_x, float lr_y);
		/// <summary>
		///   Sets the image texture to use.  This should be a three part 
		///   string, like 'Interface\\ContainerFrame\\UI-Backpack-Background'.
		///   The first portion is ignored, the second portion indicates 
		///   which imageset should be used, and the third portion indicates
		///   which image within that imageset should be used.
		/// </summary>
		/// <param name="textureFile">texture to use</param>
		void SetTexture(string textureFile);

		#region Unimplemented
        //Texture:GetBlendMode() - Return the blend mode set by SetBlendMode() 
        //Texture:GetTexCoord() - Gets the 8 texture coordinates that map to the Texture's corners - New in 1.11. 
        //Texture:GetTexCoordModifiesRect() - Get the SetTexCoordModifiesRect setting - New in 1.11 
        //Texture:GetTexture() - Gets this texture's current texture path. 
        //Texture:GetVertexColor() - Gets the vertex color for the Texture. 
        //Texture:IsDesaturated() - Gets the desaturation state of this Texture. - New in 1.11 
        //Texture:SetBlendMode("mode") - Set the alphaMode of the texture. 
        //Texture:SetDesaturated(flag) - Set whether this texture should be displayed with no saturation (Note: This has a return value) 
        //Texture:SetGradient("orientation", minR, minG, minB, maxR, maxG, maxB) 
        //Texture:SetGradientAlpha("orientation", minR, minG, minB, minA, maxR, maxG, maxB, maxA) 
        //Texture:SetTexCoordModifiesRect(enableFlag) - Set whether future SetTexCoord operations should modify the display rectangle rather than stretch the texture. - New in 1.11 
        //Texture:SetTexture(r, g, b[, a]) - Sets the texture to be displayed from a file or to a solid color.
		#endregion
	}


	/// <summary>
	///   This class is the base for widgets that can contain other widgets.
	/// </summary>
	public interface IFrame : IRegion {
        /// <summary>
        ///   Get the id associated with this widget
        /// </summary>
        /// <returns>id associated with this widget</returns>
        int GetID();
        /// <summary>
        ///   Gets the FrameLevel of this frame.  Generally, this is one more 
        ///   than our UIParent's frame level.
        /// </summary>
        /// <returns>the frame level of this frame</returns>
        int GetFrameLevel();
        /// <summary>
        ///   Gets the FrameStrata of this frame.
        /// </summary>
        /// <returns>the frame strata of this frame</returns>
        string GetFrameStrata();

        /// <summary>
        ///   Register for callbacks for events with the given name
        /// </summary>
        /// <param name="eventName">name of the event we are interested in</param>
		void RegisterEvent(string eventName);
        /// <summary>
        ///   Unregister for callbacks for events with the given name
        /// </summary>
        /// <param name="eventName">name of the event we are no longer interested in</param>
        void UnregisterEvent(string eventName);
        /// <summary>
        ///   Sets the id associated with this widget
        /// </summary>
        /// <param name="id">id to be associated with this widget</param>
        void SetID(int id);
        /// <summary>
        ///   Sets the strata of the frame
        /// </summary>
        /// <param name="strata">the desired frame strata</param>
        void SetFrameStrata(string strata);

        void SetBackdropColor(float r, float g, float b);
        void SetBackdropBorderColor(float r, float g, float b);

        /// <summary>
        ///   Map of properties, keyed by string name.
        ///   This is so that scripts can associate arbitrary data with a 
        ///   widget, and then look it up later.
        /// </summary>
        Dictionary<string, object> Properties { get; }

		#region Unimplemented
		//Frame:DisableDrawLayer 
		//Frame:EnableDrawLayer 
		//Frame:EnableKeyboard(enableFlag) - Set whether this frame will get keyboard input. 
		//Frame:EnableMouse(enableFlag) - Set whether this frame will get mouse input. 
		//Frame:GetCenter();
		//Frame:GetScale() - Get the scale factor of this object relative to its parent. 
		//Frame:IsMovable() - Determine if the frame can be moved 
		//Frame:IsResizable() - Determine if the frame can be resized 
		//Frame:IsShown() - Determine if this frame is shown. 
		//Frame:IsUserPlaced() - Determine if this frame has been relocated by the user. 
		//Frame:Lower() - Lower this frame behind other frames. 
		//Frame:Raise() - Raise this frame above other frames. 
		//Frame:RegisterForDrag("buttonType"{,"buttonType"...}) - Inidicate that this frame should be notified of drag events for the specified buttons. 
		//Frame:SetAllPoints("frame") 
		//Frame:SetFrameLevel(level) - Set the level of this frame (determines which of overlapping frames shows on top). 
		//Frame:SetMaxResize(maxWidth,maxHeight) - Set the maximum dimensions this frame can be resized to. 
		//Frame:SetMinResize(minWidth,minHeight) - Set the minimum dimensions this frame can be resized to. 
		//Frame:SetMovable(isMovable) - Set whether frame can be moved. 
		//Frame:SetResizable(isResizable) - Set whether frame can be resized. 
		//Frame:SetScale(scale) - Set the scale factor of this frame relative to its parent. 
		//Frame:StartMoving() - Start moving this frame. 
		//Frame:StartSizing("point") - Start sizing this frame using the specified anchor point. 
		//Frame:StopMovingOrSizing() - Stop moving and/or sizing this frame. 
		#endregion
	}


	/// <summary>
	///   Class for the button widget
	/// </summary>
	public interface IButton : IFrame
	{
		/// <summary>
		///   Trigger a click event on the button.
		/// </summary>
		void Click();
		/// <summary>
		///   Disable the button.  This will generally cause the button to 
		///   show up as disabled, and will prevent click events.
		/// </summary>
		void Disable();
		/// <summary>
		///   Enable the button.  This will generally cause the button to 
		///   show up as normal, and will allow click events.
		/// </summary>
		void Enable();
        /// <summary>
        ///   Gets the button state (NORMAL or PUSHED)
        /// </summary>
        /// <returns>the current button state</returns>
        string GetButtonState();
		/// <summary>
		///   Get the text associated with the button.
		/// </summary>
		/// <returns>the text associated with the button</returns>
		string GetText();
		/// <summary>
		///   Get the lineheight of the text associated with the button.
		/// </summary>
		/// <returns>the lineheight of the text associated with the button</returns>
		int GetTextHeight();
		/// <summary>
		///   Get the width of the text associated with the button.
		/// </summary>
		/// <returns>the width of the current text associated with the button</returns>
		int GetTextWidth();
		/// <summary>
		///   Determine whether the button is currently enabled.
		/// </summary>
		/// <returns>true if the button is enabled, or false if it is not</returns>
		bool IsEnabled();
		/// <summary>
		///   Lock the highlight mode.  This will generally cause the button to
		///   show up as highlighted.
		/// </summary>
		void LockHighlight();
        /// <summary>
        ///   Set the color of the text for when the button is disabled.
        /// </summary>
        /// <param name="r">Red value (from 0 to 1)</param>
        /// <param name="g">Green value (from 0 to 1)</param>
        /// <param name="b">Blue value (from 0 to 1)</param>
        void SetDisabledTextColor(float r, float g, float b);
        /// <summary>
        ///   Set the color of the text for when the button is disabled.
        /// </summary>
        /// <param name="r">Red value (from 0 to 1)</param>
        /// <param name="g">Green value (from 0 to 1)</param>
        /// <param name="b">Blue value (from 0 to 1)</param>
        /// <param name="a">Alpha value (from 0 to 1)</param>
        void SetDisabledTextColor(float r, float g, float b, float a);
        /// <summary>
        ///   Set the texture to use when the button is disabled.
        /// </summary>
        /// <param name="texture"></param>
        /// <seealso cref="ITexture.SetTexture"/>
        void SetDisabledTexture(string texture);
        /// <summary>
        ///   Set the texture to use when the button is disabled.
        /// </summary>
        /// <param name="texture"></param>
        /// <seealso cref="ITexture.SetTexture"/>
        void SetDisabledTexture(ITexture texture);
        /// <summary>
        ///   Set the color of the text used when the button is highlighted
        /// </summary>
        /// <param name="r">Red value (from 0 to 1)</param>
        /// <param name="g">Green value (from 0 to 1)</param>
        /// <param name="b">Blue value (from 0 to 1)</param>
        void SetHighlightTextColor(float r, float g, float b);		/// <summary>
        ///   Set the color of the text used when the button is highlighted
        /// </summary>
        /// <param name="r">Red value (from 0 to 1)</param>
        /// <param name="g">Green value (from 0 to 1)</param>
        /// <param name="b">Blue value (from 0 to 1)</param>
        /// <param name="a">Alpha value (from 0 to 1)</param>
        void SetHighlightTextColor(float r, float g, float b, float a);
        /// <summary>
        ///   Set the texture to use when the button is highlighted.
        /// </summary>
        /// <param name="texture"></param>
        /// <seealso cref="ITexture.SetTexture"/>
        void SetHighlightTexture(string texture);
        /// <summary>
        ///   Set the texture to use when the button is highlighted.
        /// </summary>
        /// <param name="texture"></param>
        /// <seealso cref="ITexture.SetTexture"/>
        void SetHighlightTexture(ITexture texture);
        /// <summary>
        ///   Set the texture to use when the button is in normal mode.
        /// </summary>
        /// <param name="texture"></param>
        /// <seealso cref="ITexture.SetTexture"/>
        void SetNormalTexture(string texture);        /// <summary>
        ///   Set the texture to use when the button is in normal mode.
        /// </summary>
        /// <param name="texture"></param>
        /// <seealso cref="ITexture.SetTexture"/>
        void SetNormalTexture(ITexture texture);
        /// <summary>
        ///   Set the texture to use when the button is pushed.
        /// </summary>
        /// <param name="texture"></param>
        /// <seealso cref="ITexture.SetTexture"/>
        void SetPushedTexture(string texture);		/// <summary>
        ///   Set the texture to use when the button is pushed.
        /// </summary>
        /// <param name="texture"></param>
        /// <seealso cref="ITexture.SetTexture"/>
        void SetPushedTexture(ITexture texture);
		/// <summary>
		///   Set the text to be displayed when the button is in normal mode
		/// </summary>
		/// <param name="text"></param>
		void SetText(string text);
		/// <summary>
		///   Set the color of the text used when the button is in normal mode
		/// </summary>
		/// <param name="r">Red value (from 0 to 1)</param>
		/// <param name="g">Green value (from 0 to 1)</param>
		/// <param name="b">Blue value (from 0 to 1)</param>
		void SetTextColor(float r, float g, float b);
		/// <summary>
		///   Unlock the highlight mode.  This will generally cause the button to
		///   show up as normal.
		/// </summary>
		void UnlockHighlight();
        /// <summary>
        ///   Set the button state, but do not lock it
        /// </summary>
        /// <param name="state">the button state (NORMAL or PUSHED)</param>
        void SetButtonState(string state);
        /// <summary>
        ///   Set the button state, and optionally lock it
        /// </summary>
        /// <param name="state">the button state (NORMAL or PUSHED)</param>
        /// <param name="locked">whether the state should be locked (0 for not locked)</param>
        void SetButtonState(string state, int locked);
        /// <summary>
        ///   Set the button state, and optionally lock it
        /// </summary>
        /// <param name="state">the button state (NORMAL or PUSHED)</param>
        /// <param name="locked">whether the state should be locked (false for not locked)</param>
        void SetButtonState(string state, bool locked);

        #region Unimplemented
		//Button:GetDisabledFontObject()
        //Button:GetDisabledTextColor()
        //Button:GetDisabledTexture()
        //Button:GetFont()
        //Button:GetFontString()
        //Button:GetHighlightFontObject()
        //Button:GetHighlightTextColor()
        //Button:GetHighlightTexture()
        //Button:GetNormalTexture()
        //Button:GetPushedTextOffset()
        //Button:GetPushedTexture()
        //Button:GetTextColor()
        //Button:GetTextFontObject()
        //Button:RegisterForClicks("clickType"{,"clickType"...}) - Indicate which types of clicks this button should receive. 
        //Button:SetDisabledFontObject()
        //Button:SetFont(...)
        //Button:SetFontString(fontString)
        //Button:SetHighlightFontObject()
        //Button:SetPushedTextOffset()
        //Button:SetTextFontObject()
        #endregion
	}

	public interface IColorSelect : IFrame {
	}

	/// <summary>
	///   Editable text frame.  This is used for widgets like the chat input window.
	/// </summary>
	public interface IEditBox : IFrame {
		/// <summary>
		///   Store this text in the history for this widget
		/// </summary>
		/// <param name="text">the text that should be stored</param>
		void AddHistoryLine(string text);
		/// <summary>
		///   Get the text that is currently entered in the edit box.
		/// </summary>
		/// <returns></returns>
		string GetText();
		/// <summary>
		///   Set the text for the edit box
		/// </summary>
		/// <param name="text">text to put in the edit box</param>
		void SetText(string text);
		/// <summary>
		///   Set the color of the text.
		/// </summary>
		/// <param name="r">Red value (from 0 to 1)</param>
		/// <param name="g">Green value (from 0 to 1)</param>
		/// <param name="b">Blue value (from 0 to 1)</param>
		void SetTextColor(float r, float g, float b);
		/// <summary>
		///   This isn't really implemented yet, but should shift the text in from the outside of the widget.
		/// </summary>
        /// <param name="l">inset from the left (in pixels)</param>
		/// <param name="r">inset from the right (in pixels)</param>
        /// <param name="t">inset from the top (in pixels)</param>
        /// <param name="b">inset from the bottom (in pixels)</param>
		void SetTextInsets(int l, int r, int t, int b);
        /// <summary>
        ///   Clear the focus (release input)
        /// </summary>
        void ClearFocus();
        /// <summary>
        ///   Grab the focus
        /// </summary>
        void SetFocus();

        #region Unimplemented
		//EditBox:ToggleInputLanguage()
		//EditBox:GetInputLanguage() - Get the input language (locale based not in-game) 
		//EditBox:GetNumLetters() - Gets the number of letters in the box. 
		//EditBox:GetNumber() 
		//EditBox:HighlightText({unknown1,unknown2}) 
		//EditBox:Insert("text") - Inserts text at the cursor position
		//EditBox:SetMaxBytes(maxBytes) - Set the maximum byte sizes allowed to be entered. 
		//EditBox:SetMaxLetters(maxLetters) - Set the maximum letter count allowed to be entered. 
		//EditBox:SetNumber(number) 
		#endregion
	}

    /// <summary>
    ///   This class is awkwardly placed.  It is a game specific object.
    /// </summary>
	public interface IGameTooltip : IFrame {
		/// <summary>
		///   Set the text for the game tooltip
		/// </summary>
		/// <param name="text">text to put in the tooltip</param>
		void SetText(string text);
		/// <summary>
		///   Move the game tooltip to a location based on the relative frame
		///   and the anchor string.  This is just a convenience method that
		///   does the same thing as a call to SetPoint.
		/// </summary>
		/// <param name="frame">the frame to which we will anchor</param>
        /// <param name="anchor">name of the anchor e.g. ANCHOR_TOPRIGHT</param>
		void SetOwner(IRegion frame, string anchor);

		#region Unimplemented
		//GameTooltip:AddDoubleLine 
		//GameTooltip:AddLine 
		//GameTooltip:AppendText("text") - Append text to the end of the first line of the tooltip. 
		//GameTooltip:ClearLines 
		//GameTooltip:FadeOut 
		//GameTooltip:IsOwned 
		//GameTooltip:NumLines() - Get the number of lines in the tooltip. 
		//GameTooltip:SetAction(slot) - Shows the tooltip for the specified action button. 
		//GameTooltip:SetAuctionCompareItem("type",index{,offset}) 
		//GameTooltip:SetAuctionItem("type",index) - Shows the tooltip for the specified auction item. 
		//GameTooltip:SetAuctionSellItem 
		//GameTooltip:SetBagItem 
		//GameTooltip:SetBuybackItem 
		//GameTooltip:SetCraftItem 
		//GameTooltip:SetCraftSpell 
		//GameTooltip:SetHyperlink(link) - Shows the tooltip for the specified hyperlink (usually item link). 
		//GameTooltip:SetInboxItem(index) - Shows the tooltip for the specified mail inbox item. 
		//GameTooltip:SetInventoryItem(unit,slot{,nameOnly}) 
		//GameTooltip:SetLootItem 
		//GameTooltip:SetLootRollItem(id) - Shows the tooltip for the specified loot roll item. 
		//GameTooltip:SetMerchantCompareItem("slot"{,offset}) 
		//GameTooltip:SetMerchantItem 
		//GameTooltip:SetMoneyWidth(width) 
		//GameTooltip:SetPadding 
		//GameTooltip:SetPetAction(slot) - Shows the tooltip for the specified pet action. 
		//GameTooltip:SetPlayerBuff(buffIndex) - Direct the tooltip to show information about a player's buff. 
		//GameTooltip:SetQuestItem 
		//GameTooltip:SetQuestLogItem 
		//GameTooltip:SetQuestLogRewardSpell 
		//GameTooltip:SetQuestRewardSpell 
		//GameTooltip:SetSendMailItem 
		//GameTooltip:SetShapeshift(slot) - Shows the tooltip for the specified shapeshift form. 
		//GameTooltip:SetSpell(spellId,spellbookTabNum) - Shows the tooltip for the specified spell. 
		//GameTooltip:SetTalent(tabIndex,talentIndex) - Shows the tooltip for the specified talent. 
		//GameTooltip:SetText("text",r,g,b{,alphaValue{,textWrap}}) - Set the text of the tooltip. 
		//GameTooltip:SetTrackingSpell 
		//GameTooltip:SetTradePlayerItem 
		//GameTooltip:SetTradeSkillItem 
		//GameTooltip:SetTradeTargetItem 
		//GameTooltip:SetTrainerService 
		//GameTooltip:SetUnit 
		//GameTooltip:SetUnitBuff("unit",buffIndex) - Shows the tooltip for a unit's buff. 
		//GameTooltip:SetUnitDebuff("unit",buffIndex) - Shows the tooltip for a unit's debuff.
		#endregion
	}

    public interface ILayeredRegion : IRegion {
        string GetDrawLayer();
        void SetDrawLayer(string layer);
        void SetVertexColor(float r, float g, float b);
        void SetVertexColor(float r, float g, float b, float a);
    }

	public interface IMessageFrame : IFrame {
	}

	public interface IMinimap : IFrame {
	}

	public interface IModel : IFrame {
	}

	public interface IMovieFrame : IFrame {
	}

	public interface IScrollFrame : IFrame {
        float GetHorizontalScroll();
        float GetHorizontalScrollRange();
        IFrame GetScrollChild();
        float GetVerticalScroll();
        float GetVerticalScrollRange();
        void SetHorizontalScroll(float offset);
        void SetScrollChild(IFrame frame);
        void SetScrollChild(string frameName);
        void SetVerticalScroll(float offset);
        void UpdateScrollChildRect();
	}

	/// <summary>
	///   Scrollable text frame.  This is used for widgets like the chat window.
	/// </summary>
	public interface IScrollingMessageFrame : IFrame {
		void AddMessage(string text, float r, float g, float b, int id);
		int GetFontHeight(); // deprecated??
		int GetNumLinesDisplayed();
		void ScrollUp();
		void ScrollDown();
		void ScrollToTop();
		void ScrollToBottom();
		void SetFontHeight(int pixelHeight);
		void SetScrollFromBottom(bool val);
		
		#region Unimplemented
        //ScrollingMessageFrame:AtBottom() - Return true if frame is at the bottom. 
        //ScrollingMessageFrame:AtTop() - Return true if frame is at the top. 
        //ScrollingMessageFrame:Clear()
        //ScrollingMessageFrame:GetCurrentLine() 
        //ScrollingMessageFrame:GetCurrentScroll() 
        //ScrollingMessageFrame:GetFadeDuration() 
        //ScrollingMessageFrame:GetFading() 
        //ScrollingMessageFrame:GetMaxLines() 
        //ScrollingMessageFrame:GetNumMessages() 
        //ScrollingMessageFrame:GetTimeVisible() 
		//ScrollingMessageFrame:PageDown() 
        //ScrollingMessageFrame:PageUp() 
        //ScrollingMessageFrame:SetFadeDuration(seconds) 
        //ScrollingMessageFrame:SetFading() 
        //ScrollingMessageFrame:SetFading(isEnabled) 
        //ScrollingMessageFrame:SetMaxLines(lines) 
        //ScrollingMessageFrame:SetTimeVisible(seconds) 
        //ScrollingMessageFrame:UpdateColorByID(id,r,g,b)
		#endregion
	}

	public interface ISimpleHTML : IFrame {
	}

	public interface ISlider : IFrame {
        /// <summary>
        ///   Get the current value of the slider
        /// </summary>
        /// <returns>the current value for the slider</returns>
        float GetValue();
        /// <summary>
        ///   Get the minimum and maximum values of the slider
        /// </summary>
        /// <returns>Array of two floats, containing the minimum and maximum 
        ///          values of the slider</returns>
        float[] GetMinMaxValues();
        /// <summary>
        ///   Gets the orientation for the slider
        /// </summary>
        /// <returns>one of HORIZONTAL or VERTICAL</returns>
        string GetOrientation();
        /// <summary>
        ///   Gets the value for a single step.  This might be used for 
        ///   things like the mouse wheel or arrow keys.
        /// </summary>
        /// <returns>the step size for the slider</returns>
        float GetValueStep();
        /// <summary>
        ///   Set the current value of the slider
        /// </summary>
        /// <param name="value">value for the slider</param>
        void SetValue(float value);
        /// <summary>
        ///   Set up the lower and upper limits of the slider
        /// </summary>
        /// <param name="min">the lower limit of the slider</param>
        /// <param name="max">the upper limit of the slider</param>
        void SetMinMaxValues(float min, float max);
        /// <summary>
        ///   Sets the orientation for the slider
        /// </summary>
        /// <param name="orientation">one of HORIZONTAL or VERTICAL</param>
        void SetOrientation(string orientation);
        /// <summary>
        ///   Sets the value for a single step.  This might be used for 
        ///   things like the mouse wheel or arrow keys.
        ///   Currently, the value step is not used.
        /// </summary>
        /// <param name="val">the step size for the slider</param>
        void SetValueStep(float val);
        #region Unimplemented
        //string GetThumbTexture();
        //void SetThumbTexture(string texture);
        #endregion
    }


	/// <summary>
	///   Status bar widget.
	/// </summary>
	public interface IStatusBar : IFrame {
        /// <summary>
        ///   Get the current value of the status bar
        /// </summary>
        /// <returns>the current value for the status bar</returns>
		float GetValue();
        /// <summary>
        ///   Set up the lower and upper limits of the status bar
        /// </summary>
        /// <param name="min">the lower limit of the status bar</param>
        /// <param name="max">the upper limit of the status bar</param>
		void SetMinMaxValues(float min, float max);
        /// <summary>
        ///   Sets the orientation for the status bar
        /// </summary>
        /// <param name="orientation">one of HORIZONTAL or VERTICAL</param>
        void SetOrientation(string orientation);
        /// <summary>
        ///   Set the color of the status bar
        /// </summary>
        /// <param name="r">Red value (from 0 to 1)</param>
        /// <param name="g">Green value (from 0 to 1)</param>
        /// <param name="b">Blue value (from 0 to 1)</param>
        void SetStatusBarColor(float r, float g, float b);
        /// <summary>
        ///   Sets the color and alpha of the status bar
        /// </summary>
        /// <param name="r">Red value (from 0 to 1)</param>
        /// <param name="g">Green value (from 0 to 1)</param>
        /// <param name="b">Blue value (from 0 to 1)</param>
        /// <param name="a">Alpha value (from 0 to 1)
        ///                 where 0 is transparent and 1 is opaque</param>
		void SetStatusBarColor(float r, float g, float b, float a);
        /// <summary>
        ///   Set the current value of the status bar
        /// </summary>
        /// <param name="value">value for the status bar</param>
		void SetValue(float value);
        /// <summary>
        ///   Gets the orientation for the status bar
        /// </summary>
        /// <returns>one of HORIZONTAL or VERTICAL</returns>
        string GetOrientation();
        /// <summary>
        ///   Get the minimum and maximum values of the status bar
        /// </summary>
        /// <returns>Array of two floats, containing the minimum and maximum 
        ///          values of the status bar</returns>
        float[] GetMinMaxValues();
	}

	public interface ICheckButton : IButton {
        bool GetChecked();
        void SetChecked();
        void SetChecked(bool state);
        #region Unimplemented
        //string GetCheckedTexture();
        //string GetDisabledCheckedTexture();
        //void SetCheckedTexture(string texture);
        //void SetDisabledCheckedTexture(string texture);
        #endregion
    }

    /// <summary>
    ///   Unlike the other classes, this class has no corresponding variant in 
    ///   WoW.
    /// </summary>
    public interface IWebBrowser : IFrame {
        // XXXMLM - BrowserLine
        // XXXMLM - BrowserBorder
        string GetURL();
        void SetURL(string url);
        bool GetScrollbarsEnabled();
        void SetScrollbarsEnabled();
        void SetScrollbarsEnabled(bool enabled);
        bool GetBrowserErrors();
        void SetBrowserErrors(bool enabled);
        void SetObjectForScripting(object obj);
        object GetObjectForScripting();
        object InvokeScript(string method, IEnumerable<object> args);
    }
}
