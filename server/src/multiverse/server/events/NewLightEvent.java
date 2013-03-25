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

package multiverse.server.events;

import multiverse.server.engine.*;
import multiverse.server.objects.*;
import multiverse.server.network.*;
import multiverse.server.util.*;
import multiverse.server.math.*;

/**
 * new light information
 * when receiving this event, please note that the loc information 
 * for the point light is not set in the light.
 * you have to get it from the event (getLoc()) and spawn the light with it
 *
 * the light is NOT spawned when a server receives this message
 */
public class NewLightEvent extends Event {
    public NewLightEvent() {
	super();
    }

    public NewLightEvent(MVByteBuffer buf, ClientConnection con) {
	super(buf,con);
    }

    public NewLightEvent(Long notifyOid, Long lightOid, LightData lightData) {
	super(notifyOid);
        setLightOid(lightOid);
	setLightData(lightData);
//	if (light instanceof PointLight) {
//	    setLoc(light.getLoc());
//	}
//	if (light instanceof DirectionalLight) {
//	    DirectionalLight dirLight = (DirectionalLight) light;
//	    setDir(dirLight.getLightData().getOrientation());
//	}
    }

    public String getName() {
	return "NewLightEvent";
    }

    public void setLightData(LightData lightData) {
	this.lightData = lightData;
    }
    public LightData getLightData() {
	return lightData;
    }
    LightData lightData = null;

    Long lightOid = null;
    
//    /**
//     * used for the point light.  the loc isnt available through the
//     * light object
//     */
//    public void setLoc(Point loc) {
//	this.loc = loc;
//    }
//    public Point getLoc() {
//	return loc;
//    }
//
//    public void setDir(Quaternion dir) {
//	this.dir = dir;
//    }
//    public Quaternion getDir() {
//	return dir;
//    }
//    Quaternion dir = null;

    public MVByteBuffer toBytes() {
	int msgId = Engine.getEventServer().getEventID(this.getClass());
	
	long notifyObjOid = getObjectOid();
	LightData lightData = getLightData();

	// create the message
	MVByteBuffer buf = new MVByteBuffer(200);
	buf.putLong(notifyObjOid);
	buf.putInt(msgId);
	buf.putLong(getLightOid());
	boolean isPoint = false; // is this light a point light source
	boolean isDir = false;

	Log.debug("NewLightEvent: lightName=" + lightData.getName());
        if (lightData.getInitLoc() != null) {
            Log.debug("NewLightEvent: got lightType=" + Light.LightType.Point.ordinal());
	    isPoint = true;
	    buf.putInt(Light.LightType.Point.ordinal());
	}
	else if (lightData.getOrientation() != null) {
	    Log.debug("NewLightEvent: lightType=" + Light.LightType.Directional.ordinal());
            isDir = true;
	    buf.putInt(Light.LightType.Directional.ordinal());
	}
	else {
	    throw new MVRuntimeException("NewLightEvent.toBytes: unknown light type");
	}
	
	buf.putString(lightData.getName());
	buf.putColor(lightData.getDiffuse());
	buf.putColor(lightData.getSpecular());
	buf.putFloat(lightData.getAttenuationRange());
	buf.putFloat(lightData.getAttenuationConstant());
	buf.putFloat(lightData.getAttenuationLinear());
	buf.putFloat(lightData.getAttenuationQuadradic());
	
//        Log.debug("NewLightEvent: notifyOid=" + notifyObjOid + ", msgId=" + msgId + ", lightOid=" + getLightOid() + ", ld=" + lightData);
	if (isPoint) {
//            Log.debug("NewLightEvent: is loc: " + lightData.getInitLoc());
	    Point loc = lightData.getInitLoc();
	    buf.putPoint((loc == null) ? (new Point()) : loc);
	}
        else if (isDir) {
//            Log.debug("NewLightEvent: is dir: " + lightData.getOrientation());
	    Quaternion orient = lightData.getOrientation();
	    buf.putQuaternion((orient == null) ? (new Quaternion()) : orient);
	}
	buf.flip();
	return buf;
    }

    protected void parseBytes(MVByteBuffer buf) {
	buf.rewind();
	setObjectOid(buf.getLong());
	/* int msgId = */ buf.getInt();
	long lightOid = buf.getLong();
	int lightType = buf.getInt();
// 	if (lightType != Light.POINT_TYPE) {
// 	    throw new MVRuntimeException("NewLightEvent.parseBytes: current only supports point light type");
// 	}
        
        LightData ld = new LightData();
	ld.setName(buf.getString());
	ld.setDiffuse(buf.getColor());
	ld.setSpecular(buf.getColor());
	ld.setAttenuationRange(buf.getFloat());
	ld.setAttenuationConstant(buf.getFloat());
        ld.setAttenuationLinear(buf.getFloat());
        ld.setAttenuationQuadradic(buf.getFloat());
	if (lightType == Light.LightType.Point.ordinal()) {
	    ld.setInitLoc(buf.getPoint());
//	    PointLight light = new PointLight(lightName,
//					      diffuse,
//					      specular,
//					      range,
//					      constant,
//					      linear,
//					      quadradic);
//	    light.setOid(lightOid);

	}
	else if (lightType == Light.LightType.Directional.ordinal()) {
	    ld.setOrientation(buf.getQuaternion());
//	    DirectionalLight light = new DirectionalLight(lightName,
//							  diffuse,
//							  specular,
//							  range,
//							  constant,
//							  linear,
//							  quadradic,
//							  dir);
//	    light.setOid(lightOid);
	}
	else {
	    throw new MVRuntimeException("NewLightEvent.parseBytes: only point light supported at the moment");
	}
	setLightOid(lightOid);
	setLightData(ld);
    }

    public Long getLightOid() {
        return lightOid;
    }

    public void setLightOid(Long lightOid) {
        this.lightOid = lightOid;
    }
}
