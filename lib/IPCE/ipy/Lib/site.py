#####################################################################################
#  
#  Copyright (c) Microsoft Corporation. All rights reserved.
# 
#  This source code is subject to terms and conditions of the Shared Source License
#  for IronPython. A copy of the license can be found in the License.html file
#  at the root of this distribution. If you can not locate the Shared Source License
#  for IronPython, please send an email to ironpy@microsoft.com.
#  By using this source code in any fashion, you are agreeing to be bound by
#  the terms of the Shared Source License for IronPython.
# 
#  You must not remove this notice, or any other, from this software.
# 
######################################################################################


import sys
import clr
from System.IO import Path, Directory, FileInfo

dir = Path.Combine(sys.prefix, 'DLLs')
if Directory.Exists(dir):
    sys.path.append(dir)
    files = Directory.GetFiles(dir)
    for file in files:
        if file.lower().endswith('.dll'):
            try:
                clr.AddReference(FileInfo(file).Name)
            except:
                pass

import sys
from System.IO import Directory, Path

parent = Path.Combine(sys.prefix, '..')
lib = Path.Combine(parent, 'Lib')
if Directory.Exists(lib):
    sys.path.append(lib)

import fepy
fepy.install()
