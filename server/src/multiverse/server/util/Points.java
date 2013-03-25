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

package multiverse.server.util;

import multiverse.server.math.Point;
import java.util.Random;
import java.lang.Math;

/**
 * this is a utility class for Point objects
 */
public class Points {
    private static Random random = new Random();
    // return a new Point within the circle of size radius around point
    public static Point findNearby(Point point, int radius) {
	long radiusSQ = (long)radius * (long)radius;
	long dx, dz;
	long distSQ;
	do {
	    dx = random.nextInt(2*radius + 1) - radius;
	    dz = random.nextInt(2*radius + 1) - radius;
	    distSQ = dx*dx + dz*dz;
	} while (distSQ > radiusSQ);
	Point newPoint = (Point)point.clone();
	newPoint.add((int)dx, 0, (int)dz);
	return newPoint;
    }

    // return a new Point on the circle of size radius around point
    public static Point findAdjacent(Point point, int radius) {
	int dx, dz;
	double angle = 2 * Math.PI * random.nextDouble();
	dx = (int)(Math.sin(angle) * radius);
	dz = (int)(Math.cos(angle) * radius);
	Point newPoint = (Point)point.clone();
	newPoint.add(dx, 0, dz);
	return newPoint;
    }

    public static boolean isClose(Point p1, Point p2, int radius) {
	long dx = p1.getX() - p2.getX();
	long dz = p1.getZ() - p2.getZ();
	long distSQ = dx*dx + dz*dz;
	long radiusSQ = (long)radius * (long)radius;
	if (distSQ <= radiusSQ)
	    return true;
	return false;
    }

    // return a new Point a fixed offset away
    public static Point offset(Point p, int x, int z) {
	Point point = (Point)p.clone();
	point.add(x, 0, z);
	return point;
    }
};
