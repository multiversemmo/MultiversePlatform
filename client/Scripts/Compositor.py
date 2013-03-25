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
from Axiom.Media import PixelFormat as PixelFormat

class Compositor:
    #
    # Constructor
    #
    def __init__(self, scriptName):
        vp = ClientAPI._sceneManager.CurrentViewport
        self.__dict__['_compositorInstance'] = CompositorManager.Instance.AddCompositor(vp, scriptName)
        
    #
    # Property Getters
    #
    def _get_Enabled(self):
        return self._compositorInstance.Enabled
        
    def __getattr__(self, attrname):
        if attrname in self._getters:
            return self._getters[attrname](self)
        else:
            raise AttributeError, attrname

    #
    # Property Setters
    #
    def _set_Enabled(self, value):
        self._compositorInstance.Enabled = value
        
    def __setattr__(self, attrname, value):
        if attrname in self._setters:
            self._setters[attrname](self, value)
        else:
            raise AttributeError, attrname

    _getters = { 'Enabled': _get_Enabled }
    _setters = { 'Enabled': _set_Enabled }
    
    #
    # Methods
    #
    
    def Dispose(self):
        self.Enabled = False
        CompositorManager.Instance.RemoveCompositor(self._compositorInstance)
        
    def AddListener(self, listener):
        self._compositorInstance.AddListener(listener)
        
    def SaveRenderTarget(self, name, filename, format=None):
        if format is not None:
            self._compositorInstance.GetTargetForTex(name).Save(filename, format)
        else:
            self._compositorInstance.GetTargetForTex(name).Save(filename)

    def SetTextureResolution(self, textureName, width, height):
        compositionTechnique = self._compositorInstance.Technique
        for textureDefinition in compositionTechnique.TextureDefinitions:
            if textureDefinition.Name == textureName:
                textureDefinition.Width = width
                textureDefinition.Height = height
        if self.Enabled:
            # We need to reset the textures, which we can do by disabling
            # and reenabling the compositor instance.
            self.Enabled = False
            self.Enabled = True

from System import Array, Single
from System.Drawing import PointF, SizeF
from Axiom.MathLib import MathUtil, Vector4
from Axiom.Core import PlatformManager, TextureManager
from Axiom.Graphics import CompositorManager, CompositorInstanceListener
from Multiverse.Gui import AtlasManager

class GaussianListener(CompositorInstanceListener):
    def __init__(self):
        self.vpWidth = 0
        self.vpHeight = 0
        self.bloomTexWeights = Array[Single](range(60))
        self.bloomTexOffsetsHorz = Array[Single](range(60))
        self.bloomTexOffsetsVert = Array[Single](range(60))

    def NotifyViewportSize(self, width, height):
        self.vpWidth = width
        self.vpHeight = height
        # Calculate gaussian texture offsets & weights
        deviation = 3.0
        texelSize = 1.0 / min(self.vpWidth, self.vpHeight)

        # central sample, no offset
        self.bloomTexOffsetsHorz[0] = 0.0
        self.bloomTexOffsetsHorz[1] = 0.0
        self.bloomTexOffsetsVert[0] = 0.0
        self.bloomTexOffsetsVert[1] = 0.0
        self.bloomTexWeights[0] = self.bloomTexWeights[1] = self.bloomTexWeights[2] = MathUtil.GaussianDistribution(0, 0, deviation)
        self.bloomTexWeights[3] = 1.0;

        # 'pre' samples
        for i in range(1, 8):
            self.bloomTexWeights[4 * i] = self.bloomTexWeights[4 * i + 1] = self.bloomTexWeights[4 * i + 2] = MathUtil.GaussianDistribution(i, 0, deviation)
            self.bloomTexWeights[4 * i + 3] = 1.0
            self.bloomTexOffsetsHorz[4 * i] = i * texelSize
            self.bloomTexOffsetsHorz[4 * i + 1] = 0.0
            self.bloomTexOffsetsVert[4 * i] = 0.0
            self.bloomTexOffsetsVert[4 * i + 1] = i * texelSize
        # 'post' samples
        for i in range(8, 15):
            self.bloomTexWeights[4 * i] = self.bloomTexWeights[4 * i + 1] = self.bloomTexWeights[4 * i + 2] = self.bloomTexWeights[4 * (i - 7)]
            self.bloomTexWeights[4 * i + 3] = 1.0
            self.bloomTexOffsetsHorz[4 * i] = -1 * self.bloomTexOffsetsHorz[4 * (i - 7)]
            self.bloomTexOffsetsHorz[4 * i + 1] = 0.0
            self.bloomTexOffsetsVert[4 * i] = 0.0
            self.bloomTexOffsetsVert[4 * i + 1] = -1 * self.bloomTexOffsetsVert[4 * (i - 7)]

    def NotifyMaterialSetup(self, pass_id, mat):
        # Prepare the fragment params offsets
        if pass_id == 701:
            # horizontal bloom
            mat.Load()
            fparams = mat.GetBestTechnique().GetPass(0).FragmentProgramParameters
            progName = mat.GetBestTechnique().GetPass(0).FragmentProgramName
            # A bit hacky - Cg & HLSL index arrays via [0], GLSL does not
            if progName.find("GLSL") != -1:
                fparams.SetNamedConstant("sampleOffsets", self.bloomTexOffsetsHorz)
                fparams.SetNamedConstant("sampleWeights", self.bloomTexWeights)
            else:
                fparams.SetNamedConstant("sampleOffsets[0]", self.bloomTexOffsetsHorz)
                fparams.SetNamedConstant("sampleWeights[0]", self.bloomTexWeights)
        elif pass_id == 700:
            # vertical bloom 
            mat.Load()
            fparams = mat.GetBestTechnique().GetPass(0).FragmentProgramParameters
            progName = mat.GetBestTechnique().GetPass(0).FragmentProgramName
            # A bit hacky - Cg & HLSL index arrays via [0], GLSL does not
            if progName.find("GLSL") != -1:
                fparams.SetNamedConstant("sampleOffsets", self.bloomTexOffsetsVert)
                fparams.SetNamedConstant("sampleWeights", self.bloomTexWeights)
            else:
                fparams.SetNamedConstant("sampleOffsets[0]", self.bloomTexOffsetsVert)
                fparams.SetNamedConstant("sampleWeights[0]", self.bloomTexWeights)

class HeatVisionListener(CompositorInstanceListener):
    def __init__(self):
        self.timer = PlatformManager.Instance.CreateTimer()
        self.start = self.end = self.curr = 0.0
        self.fparams = None
        
    def NotifyMaterialSetup(self, pass_id, mat):
        if pass_id == 616:
            self.timer.Reset()
            self.fparams = mat.GetBestTechnique().GetPass(0).FragmentProgramParameters
            
    def NotifyMaterialRender(self, pass_id, mat):
        if pass_id == 616:
            # "random_fractions" parameter
            self.fparams.SetNamedConstant("random_fractions", Vector4(MathUtil.RangeRandom(0.0, 1.0), MathUtil.RangeRandom(0.0, 1.0), 0, 0))
            
            # "depth_modulator" parameter
            inc = self.timer.Milliseconds / 1000.0
            if abs(self.curr - self.end) <= 0.001:
                # take a new value to reach
                self.end = MathUtil.RangeRandom(0.95, 1.0)
                self.start = self.curr
            else:
                if self.curr > self.end:
                    self.curr -= inc
                else:
                    self.curr += inc
            self.timer.Reset()
            self.fparams.SetNamedConstant("depth_modulator", Vector4(self.curr, 0, 0, 0))

class HDRListener(GaussianListener):
    def __init__(self):
        GaussianListener.__init__(self)
        self.lumSize = [0,1,2,3,4]

    def NotifyCompositor(self, instance):
        # Get some RTT dimensions for later calculations
        defs = instance.Technique.TextureDefinitions
        for definition in defs:
            # store the sizes of downscaled textures (size can be tweaked in script)
            if definition.Name.startswith("rt_lum"):
                idx = int(definition.Name[6:7])
                self.lumSize[idx] = definition.Width
            elif definition.Name == "rt_bloom0":
                GaussianListener.NotifyViewportSize(self, definition.Width, definition.Height)

    def NotifyMaterialSetup(self, pass_id, mat):
        # Prepare the fragment params offsets
        if pass_id == 993 or pass_id == 992 or pass_id == 991 or pass_id == 990:
            # Need to set the texel size
            # Set from source, which is the one higher in the chain
            mat.Load()
            idx = pass_id - 990 + 1
            texelSize = 1.0 / self.lumSize[idx]
            fparams = mat.GetBestTechnique().GetPass(0).FragmentProgramParameters
            fparams.SetNamedConstant("texelSize", texelSize)
        else:
            GaussianListener.NotifyMaterialSetup(self, pass_id, mat)

def SetupHDRListener(instance):
    listener = HDRListener()
    instance.AddListener(listener)
    vp = ClientAPI._sceneManager.CurrentViewport
    listener.NotifyViewportSize(vp.ActualWidth, vp.ActualHeight)
    listener.NotifyCompositor(instance._compositorInstance)
    
def SetupGaussianListener(instance):
    listener = GaussianListener()
    instance.AddListener(listener)
    vp = ClientAPI._sceneManager.CurrentViewport
    listener.NotifyViewportSize(vp.ActualWidth, vp.ActualHeight)

def SetupHeatVisionListener(instance):
    listener = HeatVisionListener()
    instance.AddListener(listener)

_debugRttAtlases = []
def GetCompositorChainInfo():
    global _debugRttAtlases
    DisposeCompositorChainInfo()
    vp = Client.Instance.Viewport
    chain = CompositorManager.Instance.GetCompositorChain(vp)
    rv = []
    for inst in chain.Instances:
        if inst.Enabled:
            entry = [inst.Name, []]
            for texDef in inst.Technique.TextureDefinitions:
                instName = inst.GetTextureInstanceName(texDef.Name)
                tex = TextureManager.Instance.GetByName(instName)
                atlas = AtlasManager.Instance.CreateAtlas(tex.Name, tex)
                _debugRttAtlases.append(atlas)
                atlas.DefineImage("RttImage", PointF(0, 0), SizeF(tex.Width, tex.Height))
                entry[1].append([texDef.Name, tex.Name])
            rv.append(entry)
    return rv

def DisposeCompositorChainInfo():
    global _debugRttAtlases
    for atlas in _debugRttAtlases:
        AtlasManager.Instance.DestroyAtlas(atlas)
    _debugRttAtlases = []
    
