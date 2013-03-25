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
 * information about a light.  to be used in light messages
 * and within light objects.
 * 
 * @author cedeno
 *
 */
public class TerrainDecalData implements Serializable {

    public TerrainDecalData() {
        super();
    }
    
    /**
     * initial loc is specified by initLoc.
     * once constructed and spawned, the location should be accessed
     * via the world node
     */
    public TerrainDecalData(String imageName,
                 int posX,
                 int posZ,
                 float sizeX,
                 float sizeZ,
                 float rotation,
                 int priority) {
        super();
        setImageName(imageName);
        setPosX(posX);
        setPosZ(posZ);
        setSizeX(sizeX);
        setSizeZ(sizeZ);
        setRotation(rotation);
        setPriority(priority);
    }

    public String toString() {
        return "[TerrainDecalData: " +
        "ImageName=" + getImageName() +
        ", PosX=" + getPosX() +
        ", PosZ=" + getPosZ() +
        ", SizeX=" + getSizeX() +
        ", SizeZ=" + getSizeZ() +
        ", Rotation=" + getRotation() +
        ", Priority=" + getPriority() +
        "]";        
    }

    public void setImageName(String imageName) {
        this.imageName = imageName;
    }
    public String getImageName() {
        return this.imageName;
    }
    
    public void setPosX(int val) {
        posX = val;
    }
    public int getPosX() {
        return posX;
    }
    public void setPosZ(int val) {
        posZ = val;
    }
    public int getPosZ() {
        return posZ;
    }
    
    public void setSizeX(float val) {
        sizeX = val;
    }
    public float getSizeX() {
        return sizeX;
    }
    public void setSizeZ(float val) {
        sizeZ = val;
    }
    public float getSizeZ() {
        return sizeZ;
    }
    
    public void setRotation(float val) {
        rotation = val;
    }
    public float getRotation() {
        return rotation;
    }
    
    public void setPriority(int val) {
        priority = val;
    }
    public int getPriority() {
        return priority;
    }
    
    private String imageName = null;
    private int posX = 0;
    private int posZ = 0;
    private float sizeX = 0;
    private float sizeZ = 0;
    private float rotation = 0;
    private int priority = 0;

    private static final long serialVersionUID = 1L;
}
