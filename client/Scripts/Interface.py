#
#
#  The Multiverse Platform is made available under the MIT License.
#
#  Copyright (c) 2012 The Multiverse Foundation
#
#  Permission is hereby granted, free of charge, to any person 
#  obtaining a copy of this software and associated documentation 
#  files (the "Software"), to deal in the Software without restriction, 
#  including without limitation the rights to use, copy, modify, 
#  merge, publish, distribute, sublicense, and/or sell copies 
#  of the Software, and to permit persons to whom the Software 
#  is furnished to do so, subject to the following conditions:
#
#  The above copyright notice and this permission notice shall be 
#  included in all copies or substantial portions of the Software.
#
#  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
#  EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
#  OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
#  NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
#  HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
#  WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
#  FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE 
#  OR OTHER DEALINGS IN THE SOFTWARE.
#
#  

import System
import Multiverse.Interface

import ClientAPI

FrameMap = Multiverse.Interface.UiSystem.FrameMap
StringMap = Multiverse.Interface.UiSystem.StringMap

_uiEventHandlers = []
_uiVisible = True

def RegisterEventHandler(eventName, eventHandler):
    """Register a method that will receive all ui events.  The method will be
       invoked with two arguments; the eventType and an array of strings."""
    global _uiEventHandlers
    if eventName == 'UiEvent':
        if eventHandler in _uiEventHandlers:
            ClientAPI.LogError("Duplicate add of event handler")
        _uiEventHandlers.append(eventHandler)
    else:
        ClientAPI.LogError("Invalid event name '%s' passed to Interface.RegisterEventHandler" % str(eventName))

def RemoveEventHandler(eventName, eventHandler):
    """Remove a method that was previously registered to receive all ui
       events."""
    global _uiEventHandlers
    if eventName == 'UiEvent':
        if eventHandler in _uiEventHandlers:
            _uiEventHandlers.remove(eventHandler)
    else:
        ClientAPI.LogError("Invalid event name '%s' passed to Interface.RemoveEventHandler" % str(eventName))

def RunScript(args_str):
    try:
        Multiverse.Interface.UiScripting.RunScript(args_str)
    except Exception, e:
        ClientAPI.Write("Script Error: " + str(e))

def ReloadUI():
    ClientAPI._client.ReloadUiElements();

def CreateFrame(frameType, frameName, uiParent, inherits):
    return ClientAPI._client.UiSystem.CreateFrame(frameType, frameName, uiParent, inherits)

def SetVisibility(value):
    global _uiVisible
    ClientAPI._client.UiSystem.Window.Visible = value
    _uiVisible = value
    
def ToggleVisibility():
    global _uiVisible
    SetVisibility(not _uiVisible)
    
def SetCursor(priority, cursor):
    Multiverse.Interface.UiSystem.SetCursor(priority, cursor)
    
def DispatchEvent(eventType, eventArgs):
    # First dispatch to the global handlers
    # note that a few events are not inserted this way, and will not be
    # handled by the global handlers, since they come from client C# code.
    global _uiEventHandlers
    for handler in _uiEventHandlers:
        try:
            handler(eventType, eventArgs)
        except Exception, e:
            ClientAPI.LogError("UiEvent Handler Error: " + str(e))
    # now dispatch to the UiSystem (and any interested frames)
    event = Multiverse.Gui.GenericEventArgs()
    event.eventType = eventType
    event.eventArgs = System.Array.CreateInstance(System.String, len(eventArgs))
    for i in range(0, len(eventArgs)):
        event.eventArgs[i] = eventArgs[i]
    Multiverse.Interface.UiSystem.DispatchEvent(event)
