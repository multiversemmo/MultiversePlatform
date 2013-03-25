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

package multiverse.server.engine;

import java.io.*;
import multiverse.server.math.Point;
import multiverse.server.math.Quaternion;
import multiverse.server.math.MVVector;

/**
 * This is the form of the world node that can be passed around from
 * plugin to plugin, because it doesn't depend on being
 * interpolatable.  It contains a location, direction and orientation.
 */
public class BasicWorldNode implements IBasicWorldNode, Serializable {

    /**
     * The no-args constructor required by marshalling.
     */
    public BasicWorldNode() {
        setupTransient();
    }

    /**
     * Build a BasicWorldNode from an InterpolatedWorldNode.
     */
    public BasicWorldNode(InterpolatedWorldNode inode) {
        setupTransient();
        this.instanceOid = inode.getInstanceOid();
        this.loc = inode.getLoc();
        this.dir = inode.getDir();
        this.orient = inode.getOrientation();
    }

    public BasicWorldNode(long instanceOid, MVVector dir, Point loc,
        Quaternion orient)
    {
        setupTransient();
        this.instanceOid = instanceOid;
        this.dir = dir;
        this.loc = loc;
        this.orient = orient;
    }

    /**
     * Create a human-readable representation of the node.
     */
    public String toString() {
        return "BasicWorldNode[instanceOid="+instanceOid+ " loc=" + loc +
            " dir=" + dir + " orient=" + orient + "]";
    }

    public boolean equals(Object obj)
    {
        BasicWorldNode other = (BasicWorldNode) obj;
        return instanceOid == other.instanceOid &&
            loc.equals(other.loc) &&
            orient.equals(other.orient) &&
            dir.equals(other.dir);
    }

    /**
     * There is no lock associated with a BasicWorldNode, so there is
     * nothing for setupTransient to do.
     */
    protected void setupTransient() {
    }

    public long getInstanceOid() {
        return instanceOid;
    }

    public void setInstanceOid(long oid) {
        instanceOid = oid;
    }

    /**
     * Getter for the node location.
     * @return The current location.
     */
    public Point getLoc() {
        return loc;
    }

    /**
     * Setter for the node location.
     * @param loc The new location.
     */
    public void setLoc(Point loc) {
        this.loc = loc;
        
    }

    /**
     * Getter for the node orientation.
     * @return The current orientation.
     */
    public Quaternion getOrientation() {
        return orient;
    }

    /**
     * Setter for the node orientation.
     * @param orient The current 0rientation.
     */
    public void setOrientation(Quaternion orient) {
        this.orient = orient;
    }

    /**
     * Getter for the node direction.
     * @return The current direction.
     */
    public MVVector getDir() {
        return dir;
    }

    /**
     * Setter for the node direction.
     * @param dir The new direction.
     */
    public void setDir(MVVector dir) {
        this.dir = dir;
        
    }

    protected long instanceOid;

    /**
     * The node location.
     */
    protected Point loc = null;
    /**
     * The node direction.
     */
    protected MVVector dir = null;
    /**
     * The node orientation.
     */
    protected Quaternion orient = null;
    
    private static final long serialVersionUID = 1L;
}
