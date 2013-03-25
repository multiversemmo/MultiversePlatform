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
import multiverse.server.math.Point;
import multiverse.server.math.Quaternion;


public class InstanceRestorePoint implements Serializable
{
    public InstanceRestorePoint()
    {
    }

    public InstanceRestorePoint(long instanceOid, Point loc)
    {
        setInstanceOid(instanceOid);
        setLoc(loc);
    }

    public InstanceRestorePoint(String instanceName, Point loc)
    {
        setInstanceName(instanceName);
        setLoc(loc);
    }

    public InstanceRestorePoint(Long instanceOid, String instanceName,
        Point loc)
    {
        setInstanceOid(instanceOid);
        setInstanceName(instanceName);
        setLoc(loc);
    }

    public Long getInstanceOid() {
        return instanceOid;
    }

    public void setInstanceOid(Long oid) {
        instanceOid = oid;
    }

    public String getInstanceName() {
        return instanceName;
    }

    public void setInstanceName(String name) {
        instanceName = name;
    }

    public Point getLoc() {
        return loc;
    }

    public void setLoc(Point loc) {
        this.loc = loc;
    }

    public Quaternion getOrientation() {
        return orient;
    }

    public void setOrientation(Quaternion orient) {
        this.orient = orient;
    }

    public boolean getFallbackFlag() {
        return fallback;
    }
    public void setFallbackFlag(boolean flag) {
        fallback = flag;
    }

    private Long instanceOid;
    private String instanceName;
    private Point loc;
    private Quaternion orient;
    private boolean fallback;

    private static final long serialVersionUID = 1L;

}

