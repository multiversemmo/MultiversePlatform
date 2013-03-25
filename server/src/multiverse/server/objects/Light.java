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

public class Light extends MVObject implements Cloneable {

    public Light() {
	super();
    }
    
    public Light(Long oid) {
        super(oid);
    }

    public Light(String name) {
	super(name);
    }

    /**
     * long constructor
     */
    public Light(String name,
		 Color diffuse,
		 Color specular,
		 float attenuationRange,
		 float attenuationConstant,
		 float attenuationLinear,
		 float attenuationQuadradic) {
	super();
        LightData ld = new LightData();
        ld.setName(name);
	ld.setDiffuse(diffuse);
	ld.setSpecular(specular);
	ld.setAttenuationRange(attenuationRange);
	ld.setAttenuationConstant(attenuationConstant);
	ld.setAttenuationLinear(attenuationLinear);
	ld.setAttenuationQuadradic(attenuationQuadradic);
        setLightData(ld);
    }

    /**
     * @see multiverse.server.objects.MVObject#getType()
     */
    public ObjectType getType() {
        return ObjectTypes.light;
    }
    
    public Object clone() throws CloneNotSupportedException {
	throw new CloneNotSupportedException("Light.clone: inherited class must implement clone");
    }

    public String toString() {
	return "[Light: " + super.toString() + "]";
    }

    public LightData getLightData() {
        return (LightData) getProperty(LightDataPropertyKey);
    }
    public void setLightData(LightData ld) {
        setProperty(LightDataPropertyKey, ld);
    }
    public static String LightDataPropertyKey = "lightData";
    
    public String getName() {
        return getLightData().getName();
    }
    public Color getDiffuse() {
        return getLightData().getDiffuse();
    }
    public Color getSpecular() {
        return getLightData().getSpecular();
    }
    public float getAttenuationRange() {
        return getLightData().getAttenuationRange();
    }
    public float getAttenuationConstant() {
        return getLightData().getAttenuationConstant();
    }
    public float getAttenuationLinear() {
        return getLightData().getAttenuationLinear();
    }
    public float getAttenuationQuadradic() {
        return getLightData().getAttenuationQuadradic();
    }
    
    public enum LightType {
        Point, Directional;
    }
    
    private static final long serialVersionUID = 1L;
}
