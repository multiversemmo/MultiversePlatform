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

import java.io.*;

/**
 * can be 'add' or 'clear' add command takes in the animation name and looping
 * flag clear command takes no options
 */
public class AnimationCommand implements Serializable {

    public AnimationCommand() {
    }

    public String toString() {
        return "[AnimationCommand: cmd=" + getCommand() +
        ", animName=" + getAnimName() + 
        ", isLoop=" + isLoop() +
        "]";
    }
    
    public String getCommand() {
        return command;
    }

    public void setCommand(String command) {
        this.command = command;
    }

    public String getAnimName() {
        return animName;
    }

    public void setAnimName(String animName) {
        this.animName = animName;
    }

    public boolean isLoop() {
        return isLoopFlag;
    }

    public void isLoop(boolean flag) {
        isLoopFlag = flag;
    }

    public static AnimationCommand clear() {
        AnimationCommand ac = new AnimationCommand();
        ac.setCommand(CLEAR_CMD);
        ac.setAnimName("");
        ac.isLoop(false);
        return ac;
    }

    public static AnimationCommand add(String animName, boolean isLoop) {
        AnimationCommand ac = new AnimationCommand();
        ac.setCommand(ADD_CMD);
        ac.setAnimName(animName);
        ac.isLoop(isLoop);
        return ac;
    }

    String command = null;
    String animName = null;

    boolean isLoopFlag = false;
    
    public static final String ADD_CMD = "add";
    public static final String CLEAR_CMD = "clear";
    private static final long serialVersionUID = 1L;
}
