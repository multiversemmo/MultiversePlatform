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

import multiverse.server.math.*;

/**
 * a point light, by default it is a point light source.
 */
public class DirectionalLight extends Light {
    public DirectionalLight() {
	super();
    }

    public DirectionalLight(String name) {
	super(name);
    }

    /**
     * long constructor
     */
    public DirectionalLight(String name,
			    Color diffuse,
			    Color specular,
			    float attenuationRange,
			    float attenuationConstant,
			    float attenuationLinear,
			    float attenuationQuadradic,
			    Quaternion orientation) {
	super(name, diffuse, specular, attenuationRange, attenuationConstant,
	      attenuationLinear, attenuationQuadradic);
        getLightData().setOrientation(orientation);
    }

    public String toString() {
	return "[DirectionalLight: " + super.toString() + "]";
    }

    public Object clone() {
	DirectionalLight l = new DirectionalLight(getName(), 
						  getDiffuse(),
						  getSpecular(),
						  getAttenuationRange(),
						  getAttenuationConstant(),
						  getAttenuationLinear(),
						  getAttenuationQuadradic(),
						  getOrientation());
	return l;
    }

    private static final long serialVersionUID = 1L;
}
