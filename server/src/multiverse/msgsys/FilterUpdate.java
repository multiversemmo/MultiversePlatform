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

package multiverse.msgsys;

import java.util.*;


/** Filter update instructions.  A filter update contains a list of
instructions to apply to a subscription filter.  An instruction is
an op-code (set, add, remove), a field identifier, and a value.
The field identifiers are {@link Filter} implementation specific.  The
exact semantics of the op-codes are also defined by the Filter
implementation.  See the filter documentation for details.
<p>
Not all Filters support FilterUpdates.
*/
public class FilterUpdate
{
    public FilterUpdate()
    {
        instructions = new LinkedList<Instruction>();
    }

    /** Create filter update an pre-allocate space for instructions.
        @param capacity Number of instructions
    */
    public FilterUpdate(int capacity)
    {
        instructions = new ArrayList<Instruction>(capacity);
    }

    /** Add instruction to set a field.
        @param fieldId Field identifier.
        @param value Field value.
    */
    public void setField(int fieldId, Object value)
    {
        instructions.add(new Instruction(OP_SET,fieldId,value));
    }

    /** Add instruction to add a value to an existing field.
        @param fieldId Field identifier.
        @param value Field value.
    */
    public void addFieldValue(int fieldId, Object value)
    {
        instructions.add(new Instruction(OP_ADD,fieldId,value));
    }

    /** Add instruction to remove a value from an existing field.
        @param fieldId Field identifier.
        @param value Field value.
    */
    public void removeFieldValue(int fieldId, Object value)
    {
        instructions.add(new Instruction(OP_REMOVE,fieldId,value));
    }

    /** Get filter update instructions.
    */
    public List<Instruction> getInstructions()
    {
        return instructions;
    }

    public static final int OP_SET=1;
    public static final int OP_ADD=2;
    public static final int OP_REMOVE=3;

    /** Filter update instruction.
    */
    public static class Instruction {
        public Instruction() {
        }

        /**
            @param op Op-code: OP_SET, OP_ADD, or OP_REMOVE
            @param fieldId Field identifier.
            @param value Instruction value.
        */
        public Instruction(int op, int fieldId, Object value) {
            opCode = op;
            this.fieldId = fieldId;
            this.value = value;
        }
        public int opCode;
        public int fieldId;
        public Object value;
    }

    protected List<Instruction> instructions = new LinkedList<Instruction>();

}

