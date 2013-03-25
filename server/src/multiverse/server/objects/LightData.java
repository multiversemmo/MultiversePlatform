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
import java.io.*;

import multiverse.server.math.Point;
import multiverse.server.math.Quaternion;
import multiverse.server.objects.Entity;

/**
 * information about a light.  to be used in light messages
 * and within light objects.
 * 
 * @author cedeno
 *
 */
public class LightData implements Serializable {

    public LightData() {
        super();
    }
    
    /**
     * initial loc is specified by initLoc.
     * once constructed and spawned, the location should be accessed
     * via the world node
     */
    public LightData(String name,
                 Color diffuse,
                 Color specular,
                 float attenuationRange,
                 float attenuationConstant,
                 float attenuationLinear,
                 float attenuationQuadradic,
                 Point initLoc,
                 Quaternion orient) {
        super();
        setName(name);
        setDiffuse(diffuse);
        setSpecular(specular);
        setAttenuationRange(attenuationRange);
        setAttenuationConstant(attenuationConstant);
        setAttenuationLinear(attenuationLinear);
        setAttenuationQuadradic(attenuationQuadradic);
        setOrientation(orient);
        setInitLoc(loc);
}

    public String toString() {
        return "[LightData: " +
        "name=" + getName() +
        ", diffuse=" + getDiffuse() +
        ", specular=" + getSpecular() +
        ", attenuationRange=" + getAttenuationRange() +
        ", attenuationConstant=" + getAttenuationConstant() +
        ", attenuationLinear=" + getAttenuationLinear() +
        ", attenuationQuadradic=" + getAttenuationQuadradic() +
        ", orient=" + getOrientation() +
        ", initLoc=" + getInitLoc() +
        "]";        
    }

    public void setName(String name) {
        this.name = name;
    }
    public String getName() {
        return this.name;
    }
    
    public void setDiffuse(Color color) {
        diffuse = color;
    }
    public Color getDiffuse() {
        return diffuse;
    }

    public void setSpecular(Color color) {
        specular = color;
    }
    public Color getSpecular() {
        return specular;
    }

    public void setAttenuationRange(float val) {
        attenuationRange = val;
    }
    public float getAttenuationRange() {
        return attenuationRange;
    }
    
    public void setAttenuationConstant(float val) {
        attenuationConstant = val;
    }
    public float getAttenuationConstant() {
        return attenuationConstant;
    }
    
    public void setAttenuationLinear(float val) {
        attenuationLinear = val;
    }
    public float getAttenuationLinear() {
        return attenuationLinear;
    }

    public void setAttenuationQuadradic(float val) {
        attenuationQuadradic = val;
    }
    public float getAttenuationQuadradic() {
        return attenuationQuadradic;
    }

    public void setOrientation(Quaternion orient) {
        this.orient = orient;
    }
    public Quaternion getOrientation() {
        return this.orient;
    }
    
    /**
     * initial loc is specified by initLoc.
     * once constructed and spawned, the location should be accessed
     * via the world node
     */
    public void setInitLoc(Point loc) {
        this.loc = loc;
    }
    /**
     * initial loc is specified by initLoc.
     * once constructed and spawned, the location should be accessed
     * via the world node
     */
    public Point getInitLoc() {
        return this.loc;
    }

    private void writeObject(ObjectOutputStream out)
	throws IOException, ClassNotFoundException {
        out.defaultWriteObject();
    }
    
    private void readObject(ObjectInputStream in) throws IOException,
            ClassNotFoundException {
        in.defaultReadObject();
    }

    public final static String DirLightRegionType =
        (String)Entity.registerTransientPropertyKey("DirLight");
    public final static String AmbientLightRegionType =
        (String)Entity.registerTransientPropertyKey("AmbientLight");

    private String name = null;
    private Color diffuse = null;
    private Color specular = null;
    float attenuationRange = 0;
    float attenuationConstant = 0;
    float attenuationLinear = 0;
    float attenuationQuadradic = 0;
    Quaternion orient;
    Point loc;
    private static final long serialVersionUID = 1L;
}
