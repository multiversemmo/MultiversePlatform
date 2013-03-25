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

package multiverse.server.objects;

import java.io.*;
import java.util.*;

/**
 * regions are 'areas' in the world with specific attributes they all contain a
 * boundary, and also config data for that area regions can have multiple
 * configs, like for trees, sounds, lights, etc.
 * 
 * RegionConfig is for config objects, such as a SoundConfig
 */
public class SoundRegionConfig extends RegionConfig implements Serializable {
    public SoundRegionConfig() {
        setType(SoundRegionConfig.RegionType);
    }

    public String toString() {
        return "[SoundConfig: soundData=" + soundData + "]";
    }

    public void setSoundData(List<SoundData> soundData) {
        this.soundData = soundData;
    }

    public List<SoundData> getSoundData() {
        return this.soundData;
    }

    public boolean containsSound(String fileName) {
	if (soundData == null)
	    return false;
	for (SoundData data : soundData)  {
	    if (fileName.equals(data.getFileName()))
		return true;
	}
	return false;
    }

    public static String RegionType = "SoundRegion";

    List<SoundData> soundData = null;

    private static final long serialVersionUID = 1L;
}
