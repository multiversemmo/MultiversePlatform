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
import GPUProgramType

class Pass:
    #
    # Constructor
    #
    def __init__(self):
        assert False

    #
    # Methods
    #
    def GetTextureUnit(self, nameOrIndex):
        return ClientAPI.TextureUnit._ExistingTextureUnit(self._pass.GetTextureUnitState(nameOrIndex))
    
    def _setVertexParam(self, name, value):
        self._pass.VertexProgramParameters.SetNamedConstant(name, value)

    def _setFragmentParam(self, name, value):
        self._pass.FragmentProgramParameters.SetNamedConstant(name, value)

    def _setShadowCasterVertexParam(self, name, value):
        self._pass.ShadowCasterVertexProgramParameters.SetNamedConstant(name, value)

    def _setShadowCasterFragmentParam(self, name, value):
        self._pass.ShadowCasterFragmentProgramParameters.SetNamedConstant(name, value)

    def _setShadowReceiverVertexParam(self, name, value):
        self._pass.ShadowReceiverVertexProgramParameters.SetNamedConstant(name, value)

    def _setShadowReceiverFragmentParam(self, name, value):
        self._pass.ShadowReceiverFragmentProgramParameters.SetNamedConstant(name, value)
    
    _paramSetters = { GPUProgramType.Vertex : _setVertexParam, GPUProgramType.Fragment : _setFragmentParam, GPUProgramType.ShadowCasterVertex : _setShadowCasterVertexParam, GPUProgramType.ShadowCasterFragment : _setShadowCasterFragmentParam, GPUProgramType.ShadowReceiverVertex : _setShadowReceiverVertexParam, GPUProgramType.ShadowReceiverFragment : _setShadowReceiverFragmentParam }
    
    def SetGPUParam(self, programType, name, value):
        self._paramSetters[programType](self, name, value)
        
    
    #
    # Property Getters
    #
    def _get_Name(self):
        return self._pass.Name
        
    def _get_NumTextureUnits(self):
        return self._pass.NumTextureUnits
    
    def _get_Parent(self):
        return ClientAPI.Technique._ExistingTechnique(self._pass.Parent)
    
    def _get_FogOverride(self):
        return self._pass.FogOverride
        
    def _get_FogMode(self):
        return self._pass.FogMode
        
    def _get_FogColor(self):
        return self._pass.FogColor
        
    def _get_FogDensity(self):
        return self._pass.FogDensity

    def _get_FogStart(self):
        return self._pass.FogStart
        
    def _get_FogEnd(self):
        return self._pass.FogEnd
        
    def _get_SourceBlendFactor(self):
        return self._pass.SourceBlendFactor

    def _get_DestBlendFactor(self):
        return self._pass.DestBlendFactor

    def _get_AlphaRejectFunction(self):
        return self._pass.AlphaRejectFunction

    def _get_AlphaRejectValue(self):
        return self._pass.AlphaRejectValue

    def _get_DepthBiasConstant(self):
        return self._pass.DepthBiasConstant

    def _get_DepthBiasSlopeScale(self):
        return self._pass.DepthBiasSlopeScale
        
    def _get_Ambient(self):
        return self._pass.Ambient    

    def _get_ColorWrite(self):
        return self._pass.ColorWrite    

    def _get_HardwareCullMode(self):
        return self._pass.CullMode

    def _get_SoftwareCullMode(self):
        return self._pass.ManualCullMode
        
    def _get_DepthCheck(self):
        return self._pass.DepthCheck

    def _get_DepthFunction(self):
        return self._pass.DepthFunction

    def _get_DepthWrite(self):
        return self._pass.DepthWrite

    def _get_Diffuse(self):
        return self._pass.Diffuse

    def _get_Emissive(self):
        return self._pass.Emissive

    def _get_IsProgrammable(self):
        return self._pass.IsProgrammable

    def _get_IsTransparent(self):
        return self._pass.IsTransparent
        
    def _get_LightingEnabled(self):
        return self._pass.LightingEnabled

    def _get_MaxLights(self):
        return self._pass.MaxLights

    def _get_StartLight(self):
        return self._pass.StartLight

    def _get_SceneDetail(self):
        return self._pass.SceneDetail

    def _get_ShadingMode(self):
        return self._pass.ShadingMode

    def _get_Specular(self):
        return self._pass.Specular

    def _get_Shininess(self):
        return self._pass.Shininess                
                
    def __getattr__(self, attrname):
        if attrname in self._getters:
            return self._getters[attrname](self)
        else:
            raise AttributeError, attrname
    
    #
    # Property Setters
    #
    def _set_FogOverride(self, value):
        self._pass.FogOverride = value
    
    def _set_FogMode(self, value):
        self._pass.FogMode = value
    
    def _set_FogColor(self, value):
        self._pass.FogColor = value
    
    def _set_FogDensity(self, value):
        self._pass.FogDensity = value
    
    def _set_FogStart(self, value):
        self._pass.FogStart = value
    
    def _set_FogEnd(self, value):
        self._pass.FogEnd = value

    def _set_SourceBlendFactor(self, value):
        self._pass.SourceBlendFactor = value
                
    def _set_DestBlendFactor(self, value):
        self._pass.DestBlendFactor = value    

    def _set_AlphaRejectFunction(self, value):
        self._pass.AlphaRejectFunction = value    

    def _set_AlphaRejectValue(self, value):
        self._pass.AlphaRejectValue = value    

    def _set_DepthBiasConstant(self, value):
        self._pass.DepthBiasConstant = value     

    def _set_DepthBiasSlopeScale(self, value):
        self._pass.DepthBiasSlopeScale = value    
        
    def _set_Ambient(self, value):
        self._pass.Ambient = value 

    def _set_ColorWrite(self, value):
        self._pass.ColorWrite = value 

    def _set_HardwareCullMode(self, value):
        self._pass.CullMode = value 

    def _set_SoftwareCullMode(self, value):
        self._pass.ManualCullMode = value 
        
    def _set_DepthCheck(self, value):
        self._pass.DepthCheck = value 

    def _set_DepthFunction(self, value):
        self._pass.DepthFunction = value 

    def _set_DepthWrite(self, value):
        self._pass.DepthWrite = value 

    def _set_Diffuse(self, value):
        self._pass.Diffuse = value 

    def _set_Emissive(self, value):
        self._pass.Emissive = value 

    def _set_LightingEnabled(self, value):
        self._pass.LightingEnabled = value 

    def _set_MaxLights(self, value):
        self._pass.MaxLights = value 

    def _set_StartLight(self, value):
        self._pass.StartLight = value 

    def _set_SceneDetail(self, value):
        self._pass.SceneDetail = value 

    def _set_ShadingMode(self, value):
        self._pass.ShadingMode = value 

    def _set_Specular(self, value):
        self._pass.Specular = value 

    def _set_Shininess(self, value):
        self._pass.Shininess = value 
                
    def __setattr__(self, attrname, value):
        if attrname in self._setters:
            self._setters[attrname](self, value)
        else:
            raise AttributeError, attrname
            
    _getters = {
            'Name': _get_Name, 'NumTextureUnits': _get_NumTextureUnits, 'Parent': _get_Parent, 'FogOverride': _get_FogOverride,
            'FogMode': _get_FogMode, 'FogColor': _get_FogColor, 'FogDensity': _get_FogDensity,
            'FogStart': _get_FogStart, 'FogEnd': _get_FogEnd, 'SourceBlendFactor' : _get_SourceBlendFactor,
            'DestBlendFactor' : _get_DestBlendFactor, 'AlphaRejectFunction' : _get_AlphaRejectFunction, 
            'AlphaRejectValue' : _get_AlphaRejectValue, 'DepthBiasConstant' : _get_DepthBiasConstant,
            'DepthBiasSlopeScale' : _get_DepthBiasSlopeScale, 'Ambient': _get_Ambient, 'ColorWrite' : _get_ColorWrite,
            'HardwareCullMode' : _get_HardwareCullMode, 'SoftwareCullMode' : _get_SoftwareCullMode, 'DepthCheck' : _get_DepthCheck, 
            'DepthFunction' : _get_DepthFunction, 
            'DepthWrite' : _get_DepthWrite, 'Diffuse' : _get_Diffuse, 'Emissive' : _get_Emissive, 
            'IsProgrammable' : _get_IsProgrammable, 'IsTransparent' : _get_IsTransparent, 
            'LightingEnabled' : _get_LightingEnabled, 'MaxLights' : _get_MaxLights,
            'StartLight' : _get_StartLight, 'SceneDetail' : _get_SceneDetail, 'ShadingMode' : _get_ShadingMode,
            'Specular' : _get_Specular, 'Shininess' : _get_Shininess
            }
    _setters = {
            'FogOverride': _set_FogOverride, 'FogMode': _set_FogMode, 'FogColor': _set_FogColor,
            'FogDensity': _set_FogDensity, 'FogStart': _set_FogStart, 'FogEnd': _set_FogEnd,
            'SourceBlendFactor' : _set_SourceBlendFactor, 'DestBlendFactor' : _set_DestBlendFactor,
            'AlphaRejectFunction' : _set_AlphaRejectFunction, 'AlphaRejectValue' : _set_AlphaRejectValue,
            'DepthBiasConstant' : _set_DepthBiasConstant, 'DepthBiasSlopeScale' : _set_DepthBiasSlopeScale,
            'Ambient' : _set_Ambient, 'ColorWrite' : _set_ColorWrite, 'HardwareCullMode' : _set_HardwareCullMode,
            'SoftwareCullMode' : _set_SoftwareCullMode,
            'DepthCheck' : _set_DepthCheck, 'DepthFunction' : _set_DepthFunction, 
            'DepthWrite' : _set_DepthWrite, 'Diffuse' : _set_Diffuse, 'Emissive' : _set_Emissive, 
            'LightingEnabled' : _set_LightingEnabled, 'MaxLights' : _set_MaxLights,
            'StartLight' : _set_StartLight, 'SceneDetail' : _set_SceneDetail, 'ShadingMode' : _set_ShadingMode,
            'Specular' : _set_Specular, 'Shininess' : _set_Shininess
            }
    
#
# This class is just another way of making a Pass, with a different constructor,
#  since we don't have constructor overloading within a single class.  This should only
#  be used internally by the API.
#
# The way to get a Pass is to call Technique.GetPass()
#
class _ExistingPass(Pass):
    #
    # Constructor
    #
    def __init__(self, passIn):
        self.__dict__['_pass'] = passIn

    def __setattr__(self, attrname, value):
        Pass.__setattr__(self, attrname, value)
        
