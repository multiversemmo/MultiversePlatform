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

#include "ResponseManager.h"

using namespace std;

ofstream logFile;

#ifdef _WIN32
const std::string logPath = "c:\\VivoxLogs\\SampleApp\\";
#else
const std::string logPath = "./VivoxLogs/SampleApp/";
#endif

ResponseManager::ResponseManager(StateManager * sMgr)
{
	debuglevel = 0;
	stateMgr = sMgr;
	stopOnShutdownResponse = false;
	stopNow = false;
}

ResponseManager::~ResponseManager()
{
	if (debuglevel >= 1)
	{
		logFile.close();
	}
}

std::string ResponseManager::CreateTimeStamp()
{
    time_t curr;
    tm local;
    time(&curr);
    local=*(localtime(&curr));
    std::stringstream timestamp;
    timestamp << (local.tm_year + 1900) << "."
              << (local.tm_mon + 1) << "."
              << local.tm_mday << "-"
              << local.tm_hour << "."
              << local.tm_min << "."
              << local.tm_sec;
    return timestamp.str();
}

void ResponseManager::CreateFolder(std::string path)
{
    std::vector<std::string> ss;
    size_t lastpos = 0;
    for(;;)
    {
	    size_t pos = path.find("\\", lastpos);
	    if(pos == std::string::npos)
        {
		    if(lastpos < path.size())
            {
			    ss.push_back(path.substr(lastpos, path.size() - lastpos));
		    }
		    break;
	    }
        else
        {
		    ss.push_back(path.substr(lastpos, pos - lastpos));
	    }
	    lastpos = pos + 1;
	    if(lastpos >= path.size())
		    break;
    }

    std::stringstream st;
    st << ss[0] << "\\";
    for (size_t i=1;i<ss.size();i++)
    {
        st << ss[i] << "\\";
#ifdef WIN32        
        mkdir(st.str().c_str());
#else
        mkdir(st.str().c_str(), 777 );
#endif        
    }
}

void ResponseManager::WriteResponseToFile(vx_resp_base_t* respObj)
{
	char* thexml = NULL;
	vx_response_to_xml(respObj, &thexml);
    std::string xmlstr(thexml);
	WriteToFile(FormatXml(xmlstr).c_str());
    vx_free(thexml);
}

void ResponseManager::WriteEventToFile(vx_evt_base_t* evtObj)
{
	char* thexml = NULL;
	vx_event_to_xml(evtObj, &thexml);
    std::string xmlstr(thexml);
	WriteToFile(FormatXml(xmlstr).c_str());
    vx_free(thexml);
}

std::string ResponseManager::FormatXml(std::string thexml)
{
    std::stringstream res;
    int offset = 0;
    int d1 = 0;
    int d2 = 0;
    d1 = thexml.find("<",offset);
    d2 = thexml.find(">",offset);
    res << thexml.substr(0,d2 - d1 + 1);
    int current_indent = 0;
    res << endl;
    bool prevend = false;
    while (d2 > 0)
    {
        offset = d2 + 1;
        if (offset < (int)thexml.length())
        {
            d1 = thexml.find("<",offset);
            d2 = thexml.find(">",offset);

            //determine indent
            bool indent = true;
            if (thexml.substr(d1+1,1) == "/" && d1 == offset)
            {
                current_indent--;
                prevend = true;
            }
            else
            {
                if (thexml.substr(d2-1,1) != "/" && thexml.substr(d1+1,1) != "/") 
                {
                    if (prevend == false)
                    {
                        current_indent++;
                    }
                    prevend = false;
                }
                else
                {
                    prevend = true;
                }
            }

            //write content
            if (d1 > offset)
            {
                indent = false;
                res << thexml.substr(offset,d1 - offset);
            }

            //write indent
            if (indent) { for (int i=0;i<current_indent;i++) res << "   "; }

            //write tag
            res << thexml.substr(d1,d2 - d1 + 1);  

            //write line break
            if (thexml.substr(d2+1,1) == "<") { res << endl;}
        }
        else
        {
            d2 = 0;
        }
    }
    return res.str();
}

void ResponseManager::WriteToFile(const char* msg)
{
    if (debuglevel >= 3)
	{
		cout << msg << endl << endl;
	}
	if (debuglevel >= 1)
	{
        logFile << this->CreateTimeStamp() << ": ";
        logFile << msg << std::endl << std::endl;
		logFile.flush();
	}
}

void ResponseManager::SetDebug(int debugval)
{
	debuglevel = debugval;
    if (logFile.is_open())
    {
        logFile.close();
    }
    if (debuglevel >= 1)
	{
        this->CreateFolder(logPath);
        std::string path = logPath;
        if (path.substr(path.length()-1,1) != "\\")
        {
            path.append("\\");
        }
		std::string filename = path + "vivox";
		filename.append(this->CreateTimeStamp());
		filename.append(".txt");
		logFile.open(filename.c_str());
	}
}

void ResponseManager::resp_ConnectorCreate(vx_resp_connector_create_t* respObj)
{
	std::string connectorHandle = respObj->connector_handle;		//retrieve the object ID from the response
    std::string ver(respObj->version_id);
	stateMgr->SetConnectorInitialized(connectorHandle,ver);
	if (debuglevel >= 2)
	{
		cout << "Connector Handle: " << connectorHandle << endl << endl;
	}
}

void ResponseManager::resp_ConnectorShutdown(vx_resp_connector_initiate_shutdown_t* respObj)
{
    stateMgr->SetConnectorUninitialized();

	if(stopOnShutdownResponse) {
		stopNow = true;
	}
}

void ResponseManager::resp_SGSetTXSession(vx_resp_sessiongroup_set_tx_session_t* respObj)
{
    vx_req_sessiongroup_set_tx_session_t* req = (vx_req_sessiongroup_set_tx_session_t*)respObj->base.request;
    VxSessionGroup* sg = stateMgr->GetSessionGroup(req->sessiongroup_handle);
    sg->SetSessionsTxValue(false);
    VxSession* sess = sg->GetSession(req->session_handle);
    sess->SetIsTransmitting(true);
}

void ResponseManager::resp_SGSetTXAll(vx_resp_sessiongroup_set_tx_all_sessions_t* respObj)
{
    vx_req_sessiongroup_set_tx_all_sessions_t* req = (vx_req_sessiongroup_set_tx_all_sessions_t*)respObj->base.request;
    VxSessionGroup* sg = stateMgr->GetSessionGroup(req->sessiongroup_handle);
    sg->SetSessionsTxValue(true);
}

void ResponseManager::resp_SGSetTXNone(vx_resp_sessiongroup_set_tx_no_session_t* respObj)
{
	vx_req_sessiongroup_set_tx_no_session_t* req = (vx_req_sessiongroup_set_tx_no_session_t*)respObj->base.request;
    VxSessionGroup* sg = stateMgr->GetSessionGroup(req->sessiongroup_handle);
    sg->SetSessionsTxValue(false);
}

void ResponseManager::resp_ChannelCreate(vx_resp_account_channel_create_t* respObj)
{
    if (debuglevel >= 2)
        cout << endl << "Channel URI: " << respObj->channel_uri << endl;
}

void ResponseManager::resp_ChannelFolderCreate(vx_resp_account_channel_folder_create_t* respObj)
{
    if (debuglevel >= 2)
        cout << endl << "Channel Folder ID: " << respObj->folder_id << endl;
}

void ResponseManager::resp_ChannelFavSet(vx_resp_account_channel_favorite_set_t* respObj)
{
    if (debuglevel >= 2)
        cout << endl << "Fav ID: " << respObj->channel_favorite_id << endl;
}

void ResponseManager::resp_ChannelFavGroupSet(vx_resp_account_channel_favorite_group_set_t* respObj)
{
    if (debuglevel >= 2)
        cout << endl << "Fav Group ID: " << respObj->group_id << endl;
}

void ResponseManager::resp_ListRenderDevices(vx_resp_aux_get_render_devices_t* respObj)
{
    if (respObj->count > 0)
    {
        cout << endl << endl << "Render Device List: " << endl;

        for (int i=0; i < respObj->count; i++)
        {
            cout << " " << i << ": " << respObj->render_devices[i]->device << endl;
        }
        cout << endl;
    }

    if (respObj->current_render_device && respObj->current_render_device->device)
    {
        cout << "Current Render Device: " << respObj->current_render_device->device << endl << endl;
    }
}

void ResponseManager::resp_ListCaptureDevices(vx_resp_aux_get_capture_devices_t* respObj)
{
    if (respObj->count > 0) {
        cout << endl << endl << "Capture Device List: " << endl;

        for (int i=0; i < respObj->count; i++) {
            cout << " " << i << ": " << respObj->capture_devices[i]->device << endl;
        }
        cout << endl;
    }

    if (respObj->current_capture_device && respObj->current_capture_device->device) {
        cout << "Current Capture Device: " << respObj->current_capture_device->device << endl << endl;
    }
}

void ResponseManager::resp_NetworkTest(vx_resp_aux_connectivity_info_t* respObj)
{
    if (respObj->count > 0 && respObj->test_results)
    {
        cout << endl<< endl;
        for (int i = 0; i < respObj->count; ++i)
        {
            vx_connectivity_test_result_t* r = respObj->test_results[i];
   			cout << "Test type: ";

            switch (r->test_type)
            {
                case ND_TEST_LOCATE_INTERFACE:
                    cout << "ND_TEST_LOCATE_INTERFACE"; break;
                case ND_TEST_PING_GATEWAY:
                    cout << "ND_TEST_PING_GATEWAY"; break;
                case ND_TEST_DNS:
                    cout << "ND_TEST_DNS"; break;
                case ND_TEST_STUN:
                    cout << "ND_TEST_STUN"; break;
                case ND_TEST_ECHO:
                    cout << "ND_TEST_ECHO"; break;
                case ND_TEST_ECHO_SIP_FIRST_PORT:
                    cout << "ND_TEST_ECHO_SIP_FIRST_PORT"; break;
                case ND_TEST_ECHO_SIP_FIRST_PORT_INVITE_REQUEST:
                    cout << "ND_TEST_ECHO_SIP_FIRST_PORT_INVITE_REQUEST"; break;
                case ND_TEST_ECHO_SIP_FIRST_PORT_INVITE_RESPONSE:
                    cout << "ND_TEST_ECHO_SIP_FIRST_PORT_INVITE_RESPONSE"; break;
                case ND_TEST_ECHO_SIP_FIRST_PORT_REGISTER_REQUEST:
                    cout << "ND_TEST_ECHO_SIP_FIRST_PORT_REGISTER_REQUEST"; break;
                case ND_TEST_ECHO_SIP_FIRST_PORT_REGISTER_RESPONSE:
                    cout << "ND_TEST_ECHO_SIP_FIRST_PORT_REGISTER_RESPONSE"; break;
                case ND_TEST_ECHO_SIP_SECOND_PORT:
                    cout << "ND_TEST_ECHO_SIP_SECOND_PORT"; break;
                case ND_TEST_ECHO_SIP_SECOND_PORT_INVITE_REQUEST:
                    cout << "ND_TEST_ECHO_SIP_SECOND_PORT_INVITE_REQUEST"; break;
                case ND_TEST_ECHO_SIP_SECOND_PORT_INVITE_RESPONSE:
                    cout << "ND_TEST_ECHO_SIP_SECOND_PORT_INVITE_RESPONSE"; break;
                case ND_TEST_ECHO_SIP_SECOND_PORT_REGISTER_REQUEST:
                    cout << "ND_TEST_ECHO_SIP_SECOND_PORT_REGISTER_REQUEST"; break;
                case ND_TEST_ECHO_SIP_SECOND_PORT_REGISTER_RESPONSE:
                    cout << "ND_TEST_ECHO_SIP_SECOND_PORT_REGISTER_RESPONSE"; break;
                case ND_TEST_ECHO_MEDIA:
                    cout << "ND_TEST_ECHO_MEDIA"; break;
                case ND_TEST_ECHO_MEDIA_LARGE_PACKET:
                    cout << "ND_TEST_ECHO_MEDIA_LARGE_PACKET"; break;
            }
            cout << endl;
            cout << "    Error Code: ";
            switch (r->test_error_code)
            {
                case ND_E_NO_ERROR:
                    cout << "ND_E_NO_ERROR"; break;
                case ND_E_TEST_NOT_RUN:
                    cout << "ND_E_TEST_NOT_RUN"; break;
                case ND_E_NO_INTERFACE:
                    cout << "ND_E_NO_INTERFACE"; break;
                case ND_E_NO_INTERFACE_WITH_GATEWAY:
                    cout << "ND_E_NO_INTERFACE_WITH_GATEWAY"; break;
                case ND_E_NO_INTERFACE_WITH_ROUTE:
                    cout << "ND_E_NO_INTERFACE_WITH_ROUTE"; break;
                case ND_E_TIMEOUT:
                    cout << "ND_E_TIMEOUT"; break;
                case ND_E_CANT_ICMP:
                    cout << "ND_E_CANT_ICMP"; break;
                case ND_E_CANT_RESOLVE_VIVOX_UDP_SERVER:
                    cout << "ND_E_CANT_RESOLVE_VIVOX_UDP_SERVER"; break;
                case ND_E_CANT_RESOLVE_ROOT_DNS_SERVER:
                    cout << "ND_E_CANT_RESOLVE_ROOT_DNS_SERVER"; break;
                case ND_E_CANT_CONVERT_LOCAL_IP_ADDRESS:
                    cout << "ND_E_CANT_CONVERT_LOCAL_IP_ADDRESS"; break;
                case ND_E_CANT_CONTACT_STUN_SERVER_ON_UDP_PORT_3478:
                    cout << "ND_E_CANT_CONTACT_STUN_SERVER_ON_UDP_PORT_3478"; break;
                case ND_E_CANT_CREATE_TCP_SOCKET:
                    cout << "ND_E_CANT_CREATE_TCP_SOCKET"; break;
                case ND_E_CANT_LOAD_ICMP_LIBRARY:
                    cout << "ND_E_CANT_LOAD_ICMP_LIBRARY"; break;
                case ND_E_CANT_FIND_SENDECHO2_PROCADDR:
                    cout << "ND_E_CANT_FIND_SENDECHO2_PROCADDR"; break;
                case ND_E_CANT_CONNECT_TO_ECHO_SERVER:
                    cout << "ND_E_CANT_CONNECT_TO_ECHO_SERVER"; break;
                case ND_E_ECHO_SERVER_LOGIN_SEND_FAILED:
                    cout << "ND_E_ECHO_SERVER_LOGIN_SEND_FAILED"; break;
                case ND_E_ECHO_SERVER_LOGIN_RECV_FAILED:
                    cout << "ND_E_ECHO_SERVER_LOGIN_RECV_FAILED"; break;
                case ND_E_ECHO_SERVER_LOGIN_RESPONSE_MISSING_STATUS:
                    cout << "ND_E_ECHO_SERVER_LOGIN_RESPONSE_MISSING_STATUS"; break;
                case ND_E_ECHO_SERVER_LOGIN_RESPONSE_FAILED_STATUS:
                    cout << "ND_E_ECHO_SERVER_LOGIN_RESPONSE_FAILED_STATUS"; break;
                case ND_E_ECHO_SERVER_LOGIN_RESPONSE_MISSING_SESSIONID:
                    cout << "ND_E_ECHO_SERVER_LOGIN_RESPONSE_MISSING_SESSIONID"; break;
                case ND_E_ECHO_SERVER_LOGIN_RESPONSE_MISSING_SIPPORT:
                    cout << "ND_E_ECHO_SERVER_LOGIN_RESPONSE_MISSING_SIPPORT"; break;
                case ND_E_ECHO_SERVER_LOGIN_RESPONSE_MISSING_AUDIORTP:
                    cout << "ND_E_ECHO_SERVER_LOGIN_RESPONSE_MISSING_AUDIORTP"; break;
                case ND_E_ECHO_SERVER_LOGIN_RESPONSE_MISSING_AUDIORTCP:
                    cout << "ND_E_ECHO_SERVER_LOGIN_RESPONSE_MISSING_AUDIORTCP"; break;
                case ND_E_ECHO_SERVER_LOGIN_RESPONSE_MISSING_VIDEORTP:
                    cout << "ND_E_ECHO_SERVER_LOGIN_RESPONSE_MISSING_VIDEORTP"; break;
                case ND_E_ECHO_SERVER_LOGIN_RESPONSE_MISSING_VIDEORTCP:
                    cout << "ND_E_ECHO_SERVER_LOGIN_RESPONSE_MISSING_VIDEORTCP"; break;
                case ND_E_ECHO_SERVER_CANT_ALLOCATE_SIP_SOCKET:
                    cout << "ND_E_ECHO_SERVER_CANT_ALLOCATE_SIP_SOCKET"; break;
                case ND_E_ECHO_SERVER_CANT_ALLOCATE_MEDIA_SOCKET:
                    cout << "ND_E_ECHO_SERVER_CANT_ALLOCATE_MEDIA_SOCKET"; break;
                case ND_E_ECHO_SERVER_SIP_UDP_SEND_FAILED:
                    cout << "ND_E_ECHO_SERVER_SIP_UDP_SEND_FAILED"; break;
                case ND_E_ECHO_SERVER_SIP_UDP_RECV_FAILED:
                    cout << "ND_E_ECHO_SERVER_SIP_UDP_RECV_FAILED"; break;
                case ND_E_ECHO_SERVER_SIP_TCP_SEND_FAILED:
                    cout << "ND_E_ECHO_SERVER_SIP_TCP_SEND_FAILED"; break;
                case ND_E_ECHO_SERVER_SIP_TCP_RECV_FAILED:
                    cout << "ND_E_ECHO_SERVER_SIP_TCP_RECV_FAILED"; break;
                case ND_E_ECHO_SERVER_SIP_NO_UDP_OR_TCP:
                    cout << "ND_E_ECHO_SERVER_SIP_NO_UDP_OR_TCP"; break;
                case ND_E_ECHO_SERVER_SIP_NO_UDP:
                    cout << "ND_E_ECHO_SERVER_SIP_NO_UDP"; break;
                case ND_E_ECHO_SERVER_SIP_NO_TCP:
                    cout << "ND_E_ECHO_SERVER_SIP_NO_TCP"; break;
                case ND_E_ECHO_SERVER_SIP_MALFORMED_TCP_PACKET:
                    cout << "ND_E_ECHO_SERVER_SIP_MALFORMED_TCP_PACKET"; break;
                case ND_E_ECHO_SERVER_SIP_UDP_DIFFERENT_LENGTH:
                    cout << "ND_E_ECHO_SERVER_SIP_UDP_DIFFERENT_LENGTH"; break;
                case ND_E_ECHO_SERVER_SIP_UDP_DATA_DIFFERENT:
                    cout << "ND_E_ECHO_SERVER_SIP_UDP_DATA_DIFFERENT"; break;
                case ND_E_ECHO_SERVER_SIP_TCP_PACKETS_DIFFERENT:
                    cout << "ND_E_ECHO_SERVER_SIP_TCP_PACKETS_DIFFERENT"; break;
                case ND_E_ECHO_SERVER_SIP_TCP_PACKETS_DIFFERENT_SIZE:
                    cout << "ND_E_ECHO_SERVER_SIP_TCP_PACKETS_DIFFERENT_SIZE"; break;
                case ND_E_ECHO_SERVER_LOGIN_RECV_FAILED_TIMEOUT:
                    cout << "ND_E_ECHO_SERVER_LOGIN_RECV_FAILED_TIMEOUT"; break;
                case ND_E_ECHO_SERVER_TCP_SET_ASYNC_FAILED:
                    cout << "ND_E_ECHO_SERVER_TCP_SET_ASYNC_FAILED"; break;
                case ND_E_ECHO_SERVER_UDP_SET_ASYNC_FAILED:
                    cout << "ND_E_ECHO_SERVER_UDP_SET_ASYNC_FAILED"; break;
                case ND_E_ECHO_SERVER_CANT_RESOLVE_NAME:
                    cout << "ND_E_ECHO_SERVER_CANT_RESOLVE_NAME"; break;
            }
            cout << endl;
            cout << "    Additional Info: ";
            cout << r->test_additional_info;
            cout << endl<< endl;
        }
	}
}

//////////////////////////////////////////////////////////////////////////////
// Events ////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////

void ResponseManager::evt_Generic(vx_evt_base_t* evtObj)
{
}

void ResponseManager::evt_AccountLoginStateChange(vx_evt_account_login_state_change_t* evtObj)
{
	char* acctHandle = strdup(evtObj->account_handle);
	int state = evtObj->state;

    switch (state)
    {
    case login_state_logged_in:
        stateMgr->SetStateAccountLoggedIn(evtObj->account_handle);
        if (debuglevel >= 2)
		{
			cout << "Account Handle: " << evtObj->account_handle << endl << endl;
		}
        break;
    case login_state_logged_out:
        stateMgr->SetStateAccountLoggedOut();
        break;
    }
}

void ResponseManager::evt_SessionGroupAdded(vx_evt_sessiongroup_added_t* evtObj)
{
    std::string sessionGroupHandle(evtObj->sessiongroup_handle);
    cout << "SessionGroup " << sessionGroupHandle << " Added." << std::endl;
    stateMgr->AddSessionGroup(sessionGroupHandle);
}

void ResponseManager::evt_SessionGroupRemoved(vx_evt_sessiongroup_removed_t* evtObj)
{
    std::string sessionGroupHandle(evtObj->sessiongroup_handle);
    cout << "SessionGroup " << sessionGroupHandle << " Terminated." << std::endl;
    stateMgr->RemoveSessionGroup(sessionGroupHandle);
}

void ResponseManager::evt_SessionAdded(vx_evt_session_added_t* evtObj)
{
    std::string sessionHandle(evtObj->session_handle);
    std::string sessionGroupHandle(evtObj->sessiongroup_handle);
    cout << "Session " << sessionHandle << " Added.  Incoming = " << evtObj->incoming << std::endl;
    string uri(evtObj->uri);
    stateMgr->AddSession(sessionGroupHandle, sessionHandle, uri, evtObj->incoming);
}

void ResponseManager::evt_SessionRemoved(vx_evt_session_removed_t* evtObj)
{
    std::string sessionHandle(evtObj->session_handle);
    std::string sessionGroupHandle(evtObj->sessiongroup_handle);
    cout << "Session " << sessionHandle << " Terminated." << std::endl;
    stateMgr->RemoveSession(sessionGroupHandle, sessionHandle);
}

void ResponseManager::evt_MediaStreamUpdated(vx_evt_media_stream_updated_t* evtObj)
{
    std::string sessionHandle(evtObj->session_handle);
    std::string sessionGroupHandle(evtObj->sessiongroup_handle);
    cout << "Media Stream Updated for Session " << sessionHandle << ", State = " << evtObj->state << std::endl;
    stateMgr->UpdateMediaStreamState(sessionGroupHandle,sessionHandle,evtObj->state);
}

void ResponseManager::evt_TextStreamUpdated(vx_evt_text_stream_updated_t* evtObj)
{
    std::string sessionHandle(evtObj->session_handle);
    std::string sessionGroupHandle(evtObj->sessiongroup_handle);
    cout << "Text Stream Updated for Session " << sessionHandle << ", State = " << evtObj->state << std::endl;
    stateMgr->UpdateTextStreamState(sessionGroupHandle,sessionHandle,evtObj->state);
}

void ResponseManager::evt_Message(vx_evt_message_t* evtObj)
{
    cout << endl << endl << "IM received from " << evtObj->participant_uri << " on session " << evtObj->session_handle << ": " << endl << evtObj->message_body << "" << endl << endl;
}

static std::string toString(vx_buddy_presence_state state)
{
    std::string presence;
    switch(state)
    {
    // !FIX make unknown = offline for now    
    //case buddy_presence_unknown: presence = "Unknown"; break;
    case buddy_presence_pending: presence = "Pending"; break;
    case buddy_presence_online: presence = "Online"; break;
    case buddy_presence_busy: presence = "Busy"; break;
    case buddy_presence_brb: presence = "BeRightBack"; break;
    case buddy_presence_away: presence = "Away"; break;
    case buddy_presence_onthephone: presence = "OnThePhone"; break;
    case buddy_presence_outtolunch: presence = "OutToLunch"; break;
    //case buddy_presence_closed: presence = "Closed"; break;
    case buddy_presence_offline: presence = "Offline"; break;
    default:
        presence="Unknown";
    };
    return presence;
}

void ResponseManager::evt_BuddyPresenceChange(vx_evt_buddy_presence_t* evtObj)
{
    cout << std::endl << "Buddy Presence Change: uri=" << evtObj->buddy_uri << ", state=" << toString(evtObj->presence) << ", note='" << 
        evtObj->custom_message << "'" << std::endl;
}

std::string safestring(const char *s)
{
    if(s == NULL) return "";
    return s;
}

void ResponseManager::evt_BuddyChanged(vx_evt_buddy_changed_t *evtObj)
{
    if(evtObj->change_type == change_type_set) {
        cout << std::endl << "Buddy Set: uri=" << evtObj->buddy_uri << ", displayName=" << safestring(evtObj->display_name) << endl;
    } else if(evtObj->change_type == change_type_delete) {
        cout << std::endl << "Buddy Deleted: uri=" << evtObj->buddy_uri << endl;
    }
}

void ResponseManager::evt_BuddyGroupChanged(vx_evt_buddy_group_changed_t *evtObj)
{
    if(evtObj->change_type == change_type_set) {
        cout << std::endl << "Buddy Group Set: id=" << evtObj->group_id << ", displayName=" << safestring(evtObj->group_name) << endl;
    } else if(evtObj->change_type == change_type_delete) {
        cout << std::endl << "Buddy Group Deleted: id=" << evtObj->group_id << endl;
    }
}

void ResponseManager::evt_AuxAudioProperties(vx_evt_aux_audio_properties_t* evtObj)
{
    cout.flush();

    for (int i=0; i < 50; i++) {
        cout << " ";
    }
    cout << "\r";

    int count = (int)(50.0 * evtObj->mic_energy);

    for (int i=0; i < count; i++) {
        cout << "*";
    }

    cout << "\r";
}

void ResponseManager::Stop(bool waitForShutdown)
{
	this->stopOnShutdownResponse = waitForShutdown;
	this->stopNow = !waitForShutdown;
}

bool ResponseManager::IsStopped() const
{
	return stopNow;
}
