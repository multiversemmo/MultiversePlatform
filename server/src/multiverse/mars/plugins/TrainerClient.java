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

package multiverse.mars.plugins;

import multiverse.msgsys.MessageType;
import multiverse.server.engine.Namespace;

public class TrainerClient {
    //Message types used by the Trainer Plugin
    public static final MessageType MSG_TYPE_REQ_TRAINER_INFO = MessageType.intern("mv.REQ_TRAINER_INFO");
    public static final MessageType MSG_TYPE_REQ_SKILL_TRAINING = MessageType.intern("mv.REQ_SKILL_TRAINING");
    public static final MessageType MSG_TYPE_TRAINING_INFO = MessageType.intern("mv.TRAINING_INFO");
	
    public static Namespace NAMESPACE = null;
	
    private TrainerClient(){}
	
}
