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

/**
 * represents an object state that has two values.
 * this is compatible the java beans xml serialization
 */
public class BinaryState extends ObjState {
    public BinaryState() {
    }

    /**
     * stateName is the same name which is used
     * to serialize to the client.
     * it SHOULD also be used to set the state in MVObject.
     * it would be good to make a static string variable to
     * refer to this string.
     */
    public BinaryState(String stateName, Boolean value) {
	setStateName(stateName);
	this.value = value;
    }

    public Integer getIntValue() {
	return (value ? 1 : 0);
    }

    public String getStateName() {
	return this.name;
    }
    public void setStateName(String name) {
	this.name = name;
    }

    public Boolean getValue() {
	return value;
    }
    public void setValue(Boolean val) {
	this.value = val;
    }

    public Boolean isSet() {
	return getValue();
    }

    private String name = null;
    private Boolean value = null;
    private static final long serialVersionUID = 1L;
}
