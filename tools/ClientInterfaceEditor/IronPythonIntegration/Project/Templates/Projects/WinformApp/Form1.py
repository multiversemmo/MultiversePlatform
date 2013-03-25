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
from System.Windows.Forms import *
from System.ComponentModel import *
from System.Drawing import *
from clr import *

class $safeprojectname$: #namespace
    class Form1(System.Windows.Forms.Form):
        """"""
        __slots__ = []
        
        def __init__(self):
            self.InitializeComponent()
        
        @accepts(object, bool)
        @returns(None)
        def Dispose(self, disposing):
            #if disposing and (components != None):
            #    components.Dispose()
            
            super(type(self), self).Dispose(disposing)
        
        @returns(None)
        def InitializeComponent(self):
            self.SuspendLayout()
            #  
            # 
            #  Form1
            # 
            #  
            # 
            self.ClientSize = Size(292, 266)
            self.Name = 'Form1'
            self.Text = 'Form1'
            self.ResumeLayout(False)
            self.PerformLayout()
