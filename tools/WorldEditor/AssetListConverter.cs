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
using System.ComponentModel;

namespace Multiverse.Tools.WorldEditor
{
    public abstract class AssetListConverter : StringConverter
    {
        public static AssetCollection assetCollection;

        public abstract string AssetType { get; }

        public abstract bool AllowNone { get; }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            List<AssetDesc> assets = AssetListConverter.assetCollection.Select(AssetType);
            int count = assets.Count;
            int countAdjust = 0;

            if (AllowNone)
            {
                countAdjust = 1;
            }

            string[] assetNames = new string[count + countAdjust];

            for (int i = 0; i < assets.Count; i++)
            {
                assetNames[i+countAdjust] = assets[i].Name;
            }

            if (AllowNone)
            {
                assetNames[0] = "None";
            }

            return new StandardValuesCollection(assetNames);
        }
    }

    public class SkyboxAssetListConverter : AssetListConverter
    {

        public override string AssetType
        {
            get
            {
                return "Skybox";
            }
        }

        public override bool AllowNone
        {
            get
            {
                return true;
            }
        }
    }

    public class SoundAssetListConverter : AssetListConverter
    {

        public override string AssetType
        {
            get
            {
                return "Sound";
            }
        }

        public override bool AllowNone
        {
            get
            {
                return true;
            }
        }
    }
}
