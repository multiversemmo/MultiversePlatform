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

package multiverse.server.plugins;

import java.util.ArrayList;
import java.util.HashMap;

import multiverse.msgsys.GenericMessage;
import multiverse.msgsys.GenericResponseMessage;
import multiverse.msgsys.MessageType;
import multiverse.server.engine.Engine;
import multiverse.server.engine.EnginePlugin;

import org.python.core.PyDictionary;
import org.python.core.PyList;
import org.python.core.PyTuple;

/**
 * Plugin to handle Jukebox controls
 */
public class JukeboxWebPlugin extends EnginePlugin {
    public JukeboxWebPlugin() {
	super("JukeboxWeb");
	setPluginType("JukeboxWeb");
    }

    public void onActivate() {
    }

    public ArrayList getTracks() {
	GenericMessage msg = new GenericMessage();
	MessageType jukeboxGetTracks = MessageType.intern("jukeboxGetTracks");
	msg.setMsgType(jukeboxGetTracks);
	GenericResponseMessage respMsg;
        respMsg = (GenericResponseMessage) Engine.getAgent().sendRPC(msg);

	// get a response message
	PyList trackData = (PyList) respMsg.getData();

	ArrayList<HashMap<String,String>> trackList = new ArrayList<HashMap<String,String>>();

	for (int j = trackData.__len__(); j-- > 0;) {
	    PyDictionary trackInfo = (PyDictionary)trackData.__getitem__(j);
	    PyList list = trackInfo.items();
	    HashMap<String, String> trackMap = new HashMap<String, String>();
	    for (int i = list.__len__(); i-- > 0;) {
		PyTuple tup = (PyTuple)list.__getitem__(i);
		trackMap.put(tup.__getitem__(0).__str__().internedString(),
			     tup.__getitem__(1).__str__().internedString());
	    }
	    trackList.add(trackMap);
	}
	
	// print out the list
	// Log.debug("InputThread: LIST START ----------");
	//for (HashMap trackInfo : trackData) {
	// Log.debug("InputThread: trackInfo='" + trackInfo + "'");
	// }
	// Log.debug("InputThread: LIST DONE ----------");
	return trackList;
    }

    public boolean addTrack(String name, String type, String url, String cost, String description) {
	GenericMessage msg = new GenericMessage();
	MessageType jukeboxAddTrack = MessageType.intern("jukeboxAddTrack");
	msg.setMsgType(jukeboxAddTrack);
	msg.setProperty("name", name);
	msg.setProperty("type", type);
	msg.setProperty("url", url);
	msg.setProperty("cost", cost);
	msg.setProperty("description", description);
	GenericResponseMessage respMsg;
        respMsg = (GenericResponseMessage) Engine.getAgent().sendRPC(msg);

	Integer respVal = (Integer)respMsg.getData();
	return (respVal.intValue() != 0);
    }

    public boolean deleteTrack(String name) {
	GenericMessage msg = new GenericMessage();
	MessageType jukeboxDeleteTrack = MessageType.intern("jukeboxDeleteTrack");
	msg.setMsgType(jukeboxDeleteTrack);
	msg.setProperty("name", name);
	GenericResponseMessage respMsg;
        respMsg = (GenericResponseMessage) Engine.getAgent().sendRPC(msg);

	Integer respVal = (Integer)respMsg.getData();
	return (respVal.intValue() != 0);
    }

    public int getMoney(String poid) {
	GenericMessage msg = new GenericMessage();
	MessageType jukeboxGetFunds = MessageType.intern("jukeboxGetFunds");
	msg.setMsgType(jukeboxGetFunds);
	msg.setProperty("poid", poid);
	GenericResponseMessage respMsg;
        respMsg = (GenericResponseMessage) Engine.getAgent().sendRPC(msg);

	Integer respVal = (Integer)respMsg.getData();
	return respVal;
    }

    public boolean addMoney(String poid, String money) {
	GenericMessage msg = new GenericMessage();
	MessageType jukeboxAddFunds = MessageType.intern("jukeboxAddFunds");
	msg.setMsgType(jukeboxAddFunds);
	msg.setProperty("poid", poid);
	msg.setProperty("money", money);
	/* GenericResponseMessage respMsg;
        respMsg = (GenericResponseMessage) */ Engine.getAgent().sendRPC(msg);

	/* int respVal = ((BigInteger)respMsg.getData()).intValue(); */
	return true;
    }
}
