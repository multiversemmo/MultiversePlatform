/* Copyright (c) 2007 by Vivox Inc.
 *
 * Permission to use, copy, modify or distribute this software in binary or source form 
 * for any purpose is allowed only under explicit prior consent in writing from Vivox Inc.
 *
 * THE SOFTWARE IS PROVIDED "AS IS" AND VIVOX DISCLAIMS
 * ALL WARRANTIES WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL VIVOX
 * BE LIABLE FOR ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL
 * DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR
 * PROFITS, WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS
 * ACTION, ARISING OUT OF OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS
 * SOFTWARE.
 */

#include "StateManager.h"

using namespace std;

#define GUID_START 1000		//base number for internal test app requestIDs

StateManager::StateManager()
{
	guid = GUID_START;
    isConnectorInitialized = false;
    isAccountLoggedIn = false;
    connectorHandle = "";
    accountHandle = "";

    listenerHeadingDegrees = 0;
    listenerPosition[0] = 0.0;
    listenerPosition[1] = 0.0;
    listenerPosition[2] = 0.0;
    listenerOrientation[0] = 1.0 * sin(2 * PI *(listenerHeadingDegrees/360.0));
    listenerOrientation[1] = 0.0;
    listenerOrientation[2] = -1.0 * cos(2 * PI * (listenerHeadingDegrees/360.0));
    listenerOrientation[3] = 0.0;
    listenerOrientation[4] = 1.0;
    listenerOrientation[5] = 0.0;
}

string StateManager::GenerateID()
{
    guid++;
    stringstream ss;
	ss << guid;
	return ss.str();
}

bool StateManager::SetConnectorInitialized(string p_connectorHandle, string p_version)
{
    if (connectorHandle != p_connectorHandle || !isConnectorInitialized)
    {
        isConnectorInitialized = true;
        connectorHandle = p_connectorHandle;
        accountHandle = "";
        isAccountLoggedIn = false;
        sessionGroups.clear();
        version = p_version;
    }
    return true;
}

void StateManager::SetConnectorUninitialized()
{
    isConnectorInitialized = false;
    connectorHandle = "";
    isAccountLoggedIn = false;
    accountHandle = "";
    this->ClearSessionGroups();
    version.clear();
}

bool StateManager::SetStateAccountLoggedIn(string p_accountHandle)
{
    bool ret = false;
    if (isConnectorInitialized)
    {
        if (accountHandle != p_accountHandle || !isAccountLoggedIn)
        {
            isAccountLoggedIn = true;
            accountHandle = p_accountHandle;
            this->ClearSessionGroups();
        }
        ret = true;
    }
    return ret;
}

void StateManager::SetStateAccountLoggedOut()
{
    isAccountLoggedIn = false;
    accountHandle = "";
    this->ClearSessionGroups();
}

bool StateManager::AddSessionGroup(string session_group_handle)
{
    bool ret = false;
    if (isAccountLoggedIn && isConnectorInitialized)
    {
        VxSessionGroup* sg = new VxSessionGroup(session_group_handle);
        this->sessionGroups.insert(make_pair(session_group_handle,sg));
        ret = true;
    }
    return ret;
}

void StateManager::RemoveSessionGroup(string session_group_handle)
{
    map<string,VxSessionGroup*>::const_iterator itr;
	itr = sessionGroups.find(session_group_handle);
	if (itr != sessionGroups.end())
    {
        VxSessionGroup *tmpGroup = itr->second;
        sessionGroups.erase(session_group_handle);
        delete tmpGroup;
    }
}

VxSessionGroup* StateManager::GetSessionGroup(string session_group_handle)
{
    map<string,VxSessionGroup*>::const_iterator itr;
    itr = sessionGroups.find(session_group_handle);
    if (itr != sessionGroups.end())
        return (VxSessionGroup*)itr->second;
    else
        return NULL;
}

void StateManager::ClearSessionGroups()
{
    map<string,VxSessionGroup*>::const_iterator itr;
    for (itr = sessionGroups.begin(); itr != sessionGroups.end(); ++itr)
    {
        VxSessionGroup *tmpGroup = itr->second;
        delete tmpGroup;
    }
    sessionGroups.clear();
}

std::set<string> StateManager::GetSessionGroupHandles()
{
    set<string> handles;
    map<string,VxSessionGroup*>::const_iterator itr;
    for (itr = sessionGroups.begin(); itr != sessionGroups.end(); ++itr)
    {
        handles.insert(itr->first);
    }
    return handles;
}

bool StateManager::AddSession(string session_group_handle, string session_handle, string uri, int incoming)
{
    bool ret = false;
    if (isAccountLoggedIn && isConnectorInitialized)
    {
        VxSessionGroup* sg = this->GetSessionGroup(session_group_handle);
        if (sg)
        {
            sg->AddSession(session_handle, uri, incoming);
            ret = true;
        }
    }
    return ret;
}

void StateManager::RemoveSession(string session_group_handle, string session_handle)
{
    VxSessionGroup* sg = this->GetSessionGroup(session_group_handle);
    if (sg)
        sg->RemoveSession(session_handle);
}

void StateManager::UpdateMediaStreamState(string session_group_handle, string session_handle, int state)
{
    VxSessionGroup* sg = this->GetSessionGroup(session_group_handle);
    if (sg)
        sg->UpdateMediaState(session_handle, state);
}

void StateManager::UpdateTextStreamState(string session_group_handle, string session_handle, int state)
{
    VxSessionGroup* sg = this->GetSessionGroup(session_group_handle);
    if (sg)
        sg->UpdateTextState(session_handle, state);
}

bool StateManager::GetIsConnectorInitialized()
{
    return isConnectorInitialized;
}

string StateManager::GetConnectorHandle()
{
    return connectorHandle;
}

bool StateManager::GetIsAccountLoggedIn()
{
    return isAccountLoggedIn;
}

string StateManager::GetAccountHandle()
{
    return accountHandle;
}

bool StateManager::GetSessionGroupExists(string session_group_handle)
{
    bool found = false;
    map<string,VxSessionGroup*>::const_iterator sg_itr;
    sg_itr = sessionGroups.find(session_group_handle);
    if (sg_itr != sessionGroups.end())
        found = true;
    return found;
}

int StateManager::GetMediaSessionState(string session_handle)
{
    map<string,VxSessionGroup*>::const_iterator sg_itr;
    for (sg_itr = sessionGroups.begin(); sg_itr != sessionGroups.end(); ++sg_itr)
    {
        VxSessionGroup* sg = (VxSessionGroup*)sg_itr->second;
        VxSession* sess = sg->GetSession(session_handle);
        if (sess)
            return sess->GetMediaState();
    }
    return -1;
}

int StateManager::GetTextSessionState(string session_handle)
{
    map<string,VxSessionGroup*>::const_iterator sg_itr;
    for (sg_itr = sessionGroups.begin(); sg_itr != sessionGroups.end(); ++sg_itr)
    {
        VxSessionGroup* sg = (VxSessionGroup*)sg_itr->second;
        VxSession* sess = sg->GetSession(session_handle);
        if (sess)
            return sess->GetTextState();
    }
    return -1;
}

//Request map keeps track of request/response pairs.
//Session map keep track of session state
void StateManager::DumpStateToFile(ostream* dumpFile)
{
    stringstream ss;

    ss << endl << "version:              " << version << endl << endl;

    ss << "Connector Handle:     ";
    if (isConnectorInitialized)
        ss << connectorHandle;
    else
        ss << "[none]";
    ss << endl;
    ss << "Account Handle:       ";
    if (isAccountLoggedIn)
        ss << accountHandle;
    else
        ss << "[none]";
    ss << endl;
    ss << "Session Groups:         ";
    if (sessionGroups.size() > 0)
    {
        ss << endl;
        map<string,VxSessionGroup*>::const_iterator itr;
	    for (itr = sessionGroups.begin(); itr != sessionGroups.end(); ++itr)
        {
            VxSessionGroup* sg = (VxSessionGroup*)itr->second;
            ss << "               Group: " << sg->GetSessionGroupHandle() << endl;
            const string session_indent = "             Session: ";
            {
                set<string> handles = sg->GetSessionHandles();
                set<string>::const_iterator h_itr;
                if (sg->GetNumberOfSessions() > 0)
                for (h_itr = handles.begin(); h_itr != handles.end(); ++h_itr)
                {
                    VxSession* sess = sg->GetSession(h_itr->data());
                    ss << session_indent << sess->GetSessionHandle() << "       " << sess->GetSessionURI() << endl;
                    ss << "                      " << "Audio: " << sess->GetMediaState() <<
                          "  Text: " << sess->GetTextState() <<
                          "  Inc: " << sess->GetIsIncoming() <<
                          "  Tx: " << sess->GetIsTransmitting() << endl << endl;
                }
            }
        }
    }
    else
    {
        ss << "[none]";
    }

    map<string,string>::const_iterator reqs;
    ss << endl;
    ss << "Outstanding Requests:   ";
    if (requests.size() > 0)
    {
        ss << endl;
        for (reqs = requests.begin(); reqs != requests.end(); ++reqs)
        {
            ss << "    Request ID: " << reqs->first << "   Type = " << reqs->second << endl;
        }
    }
    else
    {
        ss << "[none]";
    }

    ss << endl;
    ss << "listenerHeadingDegrees: " << this->listenerHeadingDegrees << endl;
    ss << "listenerPosition:       {" << this->listenerPosition[0] << ", " << this->listenerPosition[1] << ", " << this->listenerPosition[2] << "}" << endl;

    (*dumpFile) << ss.str();
}

string StateManager::GetTime()
{
    return "";      //TODO
}

// Request State ////////////////////////////////////////////////////////////////
void StateManager::InsertCommandReqID(string requestID, string type)
{
	requests.insert(make_pair(requestID,type));
}

void StateManager::DeleteCommandReqID(string requestID)
{
	map<string,string>::const_iterator itr;
	itr = requests.find(requestID);
	if (itr != requests.end())
	{
		requests.erase(requestID);
	}
}

// Positional /////////////////////////////////////////////////////////////////
void StateManager::SetListenerPosition(double *position)
{
    listenerPosition[0] = position[0];
    listenerPosition[1] = position[1];
    listenerPosition[2] = position[2];
}

void StateManager::GetListenerPosition(double *position)
{
    position[0] = listenerPosition[0];
    position[1] = listenerPosition[1];
    position[2] = listenerPosition[2]; 
}

void StateManager::SetListenerOrientation(double *orientation)
{
    listenerOrientation[0] = orientation[0];
    listenerOrientation[1] = orientation[1];
    listenerOrientation[2] = orientation[2];
    listenerOrientation[3] = orientation[3];
    listenerOrientation[4] = orientation[4];
    listenerOrientation[5] = orientation[5];   
}

void StateManager::GetListenerOrientation(double *orientation)
{
    orientation[0] = listenerOrientation[0];
    orientation[1] = listenerOrientation[1];
    orientation[2] = listenerOrientation[2];
    orientation[3] = listenerOrientation[3];
    orientation[4] = listenerOrientation[4];
    orientation[5] = listenerOrientation[5];   
}

void StateManager::GetListenerHeadingDegrees(double *headingDegrees)
{
    *headingDegrees = listenerHeadingDegrees;
}

void StateManager::SetListenerHeadingDegrees(double headingDegrees)
{
    listenerHeadingDegrees = headingDegrees;
}
