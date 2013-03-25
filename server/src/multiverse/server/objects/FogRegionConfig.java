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

public class FogRegionConfig extends RegionConfig implements Serializable {
    public FogRegionConfig() {
        setType(FogRegionConfig.RegionType);
    }

    public boolean equals(Object other) {
    	FogRegionConfig otherConfig = (FogRegionConfig) other;
    	return (this.getColor().equals(otherConfig.getColor()) &&
    			(this.getNear() == otherConfig.getNear()) &&
    			(this.getFar() == otherConfig.getFar()));
    }
    public String toString() {
        return "[FogRegionConfig: color=" + fogColor + ", near=" + near + ", far=" + far + "]";
    }

    public void setColor(Color c) {
        this.fogColor = c;
    }
    public Color getColor() {
        return fogColor;
    }
    
    public void setNear(int near) {
        this.near = near;
    }
    public int getNear() {
        return this.near;
    }
    
    public void setFar(int far) {
        this.far = far;
    }
    public int getFar() {
        return this.far;
    }
    
    private Color fogColor;
    private int near;
    private int far;
    
    public static String RegionType = (String)Entity.registerTransientPropertyKey("FogRegion");
    private static final long serialVersionUID = 1L;
}
