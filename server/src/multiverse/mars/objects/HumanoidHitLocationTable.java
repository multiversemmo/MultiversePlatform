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

package multiverse.mars.objects;

public class HumanoidHitLocationTable extends HitLocationTable {
	public static HitLocation getHitLocation(int loc) {
		HitLocation rv = new HitLocation();
		if (loc <=5) {
			rv.name = "head";
			rv.stunMultiplier = 5;
			rv.nStunMultiplier = 2;
			rv.bodyMultiplier = 2;
		} else if (loc <=6) {
			rv.name = "hand";
			rv.stunMultiplier = 1;
			rv.nStunMultiplier = 0.5f;
			rv.bodyMultiplier = 0.5f;
		} else if (loc <= 8) {
			rv.name = "arm";
			rv.stunMultiplier = 2;
			rv.nStunMultiplier = 0.5f;
			rv.bodyMultiplier = 0.5f;
		} else if (loc <= 9) {
			rv.name = "shoulder";
			rv.stunMultiplier = 3;
			rv.nStunMultiplier = 1;
			rv.bodyMultiplier = 1;
		} else if (loc <= 11) {
			rv.name = "chest";
			rv.stunMultiplier = 3;
			rv.nStunMultiplier = 1;
			rv.bodyMultiplier = 1;
		} else if (loc <= 12) {
			rv.name = "stomach";
			rv.stunMultiplier = 4;
			rv.nStunMultiplier = 1.5f;
			rv.bodyMultiplier = 1;
		} else if (loc <= 13) {
			rv.name = "vitals";
			rv.stunMultiplier = 4;
			rv.nStunMultiplier = 1.5f;
			rv.bodyMultiplier = 2;
		} else if (loc <= 14) {
			rv.name = "thigh";
			rv.stunMultiplier = 2;
			rv.nStunMultiplier = 1;
			rv.bodyMultiplier = 1;
		} else if (loc <= 16) {
			rv.name = "leg";
			rv.stunMultiplier = 2;
			rv.nStunMultiplier = 0.5f;
			rv.bodyMultiplier = 0.5f;
		} else if (loc <= 18) {
			rv.name = "foot";
			rv.stunMultiplier = 1;
			rv.nStunMultiplier = 0.5f;
			rv.bodyMultiplier = 0.5f;
		}
		return rv;
	} 
}
