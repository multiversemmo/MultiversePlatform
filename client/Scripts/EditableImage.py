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

import ClientAPI
import Multiverse.Base

class EditableImage:
    #
    # Constructor
    #
    def __init__(self, name, width, height, format=ClientAPI.PixelFormat.A8R8G8B8, color=ClientAPI.ColorEx.Black, numMipMaps=-1, isAlpha=False):
        self.__dict__['_editableImage'] = Multiverse.Base.EditableImage(name, width, height, format, color, numMipMaps, isAlpha)
        
    def Dispose(self):
        pass
        
    #
    # Methods
    #
    def SetPixel(self, x, y, color):
        self._editableImage.SetPixel(x, y, color)
        
    def GetPixel(self, x, y):
        return self._editableImage.GetPixel(x, y)
        
    def LoadTexture(self):
        self._editableImage.LoadTexture()
