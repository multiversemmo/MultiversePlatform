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

import java.io.Serializable;

/**
 * as used by worldeditorreader and sent to proxy to the client
 * @author cedeno
 *
 */
public class OceanData implements Serializable {
    public OceanData() {
    }
    
    public String toString() {
        return "(displayOcean=" + displayOcean +
        ",useParams=" + useParams +
        ",waveHeight=" + waveHeight +
        ",seaLevel=" + seaLevel +
        ",bumpScale=" + bumpScale +
        ",bumpSpeedX=" + bumpSpeedX +
        ",bumpSpeedZ=" + bumpSpeedZ +
        ",textureScaleX=" + textureScaleX +
        ",textureScaleZ=" + textureScaleZ +
        ",deepColor=" + deepColor +
        ",shallowColor=" + shallowColor +
        ")";
    }
    
    public Boolean displayOcean;
    public Boolean useParams;
    public Float waveHeight;
    public Float seaLevel;
    public Float bumpScale;
    public Float bumpSpeedX;
    public Float bumpSpeedZ;
    public Float textureScaleX;
    public Float textureScaleZ;
    public Color deepColor;
    public Color shallowColor;
    private static final long serialVersionUID = 1L;
}
