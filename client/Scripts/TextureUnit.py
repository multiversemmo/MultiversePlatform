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
import Axiom.Graphics

class TextureUnit:
    #
    # Constructor
    #
    def __init__(self):
        assert False

    #
    # Methods
    #
    def SetColorOperation(self, operation):
        self._textureUnit.SetColorOperation(operation)
    
    def SetColorOperationEx(self, operation, source1, source2, arg1=ClientAPI.ColorEx.White, arg2=ClientAPI.ColorEx.White, blendFactor=0.0):
        self._textureUnit.SetColorOperationEx(operation, source1, source2, arg1, arg2, blendFactor)
    
    def SetAlphaOperation(self, operation, source1, source2, arg1=1.0, arg2=1.0, blendFactor=0.0):
        self._textureUnit.SetAlphaOperation(operation, source1, source2, arg1, arg2, blendFactor)
        
    def SetTextureFiltering(self, minFilter=ClientAPI.FilterOptions.Linear, magFilter=ClientAPI.FilterOptions.Linear, mipFilter=ClientAPI.FilterOptions.Point):
        self._textureUnit.SetTextureFiltering(minFilter, magFilter, mipFilter)
    
    def SetTextureAddressingMode(self, u, v, w):
        self._textureUnit.SetTextureAddressingMode(u, v, w)
        
    def SetTextureName(self, name, type=ClientAPI.TextureType.TwoD):
        self._textureUnit.SetTextureName(name, type)
        
    def SetScrollAnimation(self, uSpeed, vSpeed):
        self._textureUnit.SetScrollAnimation(uSpeed, vSpeed)
        
    def SetRotationAnimation(self, speed):
        self._textureUnit.SetRotationAnimation(speed)
    
    def RemoveAllEffects(self):
        self._textureUnit.RemoveAllEffects()
        
    #
    # Property Getters
    #
    def _get_Name(self):
        return self._textureUnit.Name
        
    def _get_NumFrames(self):
        return self.textureUnit.NumFrames
    
    def _get_Parent(self):
        return ClientAPI.Pass._ExistingPass(self._textureUnit.Parent)
    
    def _get_TextureAnisotropy(self):
        return self._textureUnit.TextureAnisotropy
        
    def _get_TextureCoordSet(self):
        return self._textureUnit.TextureCoordSet

    def _get_TextureBorderColor(self):
        return self._textureUnit.TextureBorderColor

    def _get_MipmapBias(self):
        return self._textureUnit.MipmapBias

    def _get_CurrentFrame(self):
        return self._textureUnit.CurrentFrame

    def _get_TextureName(self):
        return self._textureUnit.TextureName

    def _get_TextureType(self):
        return self._textureUnit.TextureType

    def _get_IsBlank(self):
        return self._textureUnit.IsBlank
        
    def _get_TextureScrollU(self):
        return self._textureUnit.TextureScrollU

    def _get_TextureScrollV(self):
        return self._textureUnit.TextureScrollV

    def _get_TextureScaleU(self):
        return self._textureUnit.TextureScaleU

    def _get_TextureScaleV(self):
        return self._textureUnit.TextureScaleV
 
    def _get_TextureRotation(self):
        return self._textureUnit.TextureRotation

    def _get_TextureMatrix(self):
        return self._textureUnit.TextureMatrix
                                        
    def __getattr__(self, attrname):
        if attrname in self._getters:
            return self._getters[attrname](self)
        else:
            raise AttributeError, attrname
    
    #
    # Property Setters
    #
    
    def _set_TextureAnisotropy(self, value):
        self._textureUnit.TextureAnisotropy = value
        
    def _set_TextureCoordSet(self, value):
        self._textureUnit.TextureCoordSet = value

    def _set_TextureBorderColor(self, value):
        self._textureUnit.TextureBorderColor = value

    def _set_MipmapBias(self, value):
        self._textureUnit.MipmapBias = value

    def _set_CurrentFrame(self, value):
        self._textureUnit.CurrentFrame = value

    def _set_TextureScrollU(self, value):
        self._textureUnit.TextureScrollU = value

    def _set_TextureScrollV(self, value):
        self._textureUnit.TextureScrollV = value

    def _set_TextureScaleU(self, value):
        self._textureUnit.TextureScaleU = value

    def _set_TextureScaleV(self, value):
        self._textureUnit.TextureScaleV = value

    def _set_TextureRotation(self, value):
        self._textureUnit.TextureRotation = value

    def _set_TextureMatrix(self, value):
        self._textureUnit.TextureMatrix = value
                                
    def __setattr__(self, attrname, value):
        if attrname in self._setters:
            self._setters[attrname](self, value)
        else:
            raise AttributeError, attrname
            
    _getters = { 'Name': _get_Name, 'NumFrames': _get_NumFrames, 'Parent': _get_Parent, 'TextureAnisotropy': _get_TextureAnisotropy,
               'TextureCoordSet': _get_TextureCoordSet, 'TextureBorderColor': _get_TextureBorderColor,
               'MipmapBias': _get_MipmapBias, 'CurrentFrame': _get_CurrentFrame, 'TextureName': _get_TextureName,
               'TextureType': _get_TextureType, 'IsBlank': _get_IsBlank, 
               'TextureScrollU': _get_TextureScrollU, 'TextureScrollV': _get_TextureScrollV,
               'TextureScaleU': _get_TextureScaleU, 'TextureScaleV': _get_TextureScaleV, 'TextureRotation': _get_TextureRotation,
               'TextureMatrix': _get_TextureMatrix
               }
    _setters = { 'TextureAnisotropy': _set_TextureAnisotropy, 'TextureCoordSet': _set_TextureCoordSet, 
               'TextureBorderColor': _set_TextureBorderColor, 'MipmapBias': _set_MipmapBias, 'CurrentFrame': _set_CurrentFrame,
               'TextureScrollU': _set_TextureScrollU, 'TextureScrollV': _set_TextureScrollV,
               'TextureScaleU': _set_TextureScaleU, 'TextureScaleV': _set_TextureScaleV, 'TextureRotation': _set_TextureRotation,
               'TextureMatrix': _set_TextureMatrix
               }
    
#
# This class is just another way of making a TextureUnit, with a different constructor,
#  since we don't have constructor overloading within a single class.  This should only
#  be used internally by the API.
#
# The way to get a TextureUnit is to call Pass.GetTextureUnit()
#
class _ExistingTextureUnit(TextureUnit):
    #
    # Constructor
    #
    def __init__(self, textureUnit):
        self.__dict__['_textureUnit'] = textureUnit

    def __setattr__(self, attrname, value):
        TextureUnit.__setattr__(self, attrname, value)
