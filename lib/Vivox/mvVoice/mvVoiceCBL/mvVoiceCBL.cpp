// mvVoiceCBL.cpp

#include "mvVoiceCBL.h"
#include <string>
#include <stdio.h>
#include <vector>
#include "StateManager.h"
#include "RequestManager.h"
#include "ResponseManager.h"
#include "vxplatform/vxcplatform.h"
#ifdef _WIN32
#include <io.h>
#else
#include <iostream>
#include <ext/stdio_filebuf.h>
#endif

using namespace std;
using namespace vxplatform;

namespace mvVoiceCBL
{
  os_event_handle listenerThreadTerminatedEvent;
  os_event_handle g_responseHandle;
  os_error_t ListenerThread(void *arg);  //For catching Reponses and Events

  StateManager * stateMgr;	  //Keeps track of state for requests/reponses/objects
  RequestManager * reqMgr;	  //Handles all out-going Requests
  ResponseManager * respMgr;	  //Handles all incoming Responses and Events

  Lock mutex2;

  std::string m_ConnectorID;
  std::string m_AccountID;
  std::string m_SessionID;

  std::string VoiceCBL::GetConnectorID()
  {
	  return m_ConnectorID;
  }
  std::string VoiceCBL::GetAccountID()
  {
	  return m_AccountID;
  }
  std::string VoiceCBL::GetSessionID()
  {
	  return m_SessionID;
  }
  int VoiceCBL::Init(std::string server)
  {
	if (GetConnectorID() != "") {
		cout << "Not properly shut down." << endl;
		return 0;
	}
	g_responseHandle = NULL;
    /* Debug modes:
       0 - None (default)
       1 - Logs only
       2 - Handles on stdout, Logs
       3 - Handles on stdout, Logs, Responses and Events on stdout */
    int debugval = 2;

    stateMgr = new StateManager();
    reqMgr = new RequestManager(stateMgr);	
    respMgr = new ResponseManager(stateMgr);

    reqMgr->SetDebug(debugval);
    respMgr->SetDebug(debugval);

    cout << "mvVoiceCBL" << endl;

    os_thread_handle listenerThread;
    create_event(&listenerThreadTerminatedEvent);
	create_thread(ListenerThread, NULL, &listenerThread);
    if(listenerThread == NULL) {
      cout << "Unable to spawn listener thread." << endl;
    }
	// -----
    int minPort = 0;
    int maxPort = 0;
    int retcode = 0;
    cout << "Initializing Connector:\n";
    mutex2.Take();
    retcode = reqMgr->req_ConnectorCreate(server.c_str(), minPort, maxPort);
    mutex2.Release();

	cout << "Connecting";
	int i = 0;
	for (i = 0; ((i < 10) && (GetConnectorID() == "")); i++) {
		Sleep(1000);
		cout << ".";
	}
	cout << "Connector ID: " << GetConnectorID() << endl;
    return retcode;
  }
  int VoiceCBL::Login(std::string connectorID, std::string username, std::string password)
  {
    int pres = 1;
    int autoanswer = 0;
    int text = 1;
    int participantPropertyFrequency = 100;
    int retcode = 0;
    cout << "Logging in to Connector:\n";
    mutex2.Take();
    retcode = reqMgr->req_AccountLogin(connectorID, username, password, pres, autoanswer, text, participantPropertyFrequency);
    mutex2.Release();

	cout << "Logging in";
	int i = 0;
	for (i = 0; ((i < 10) && (GetAccountID() == "")); i++) {
		Sleep(1000);
		cout << ".";
	}
	cout << "Account ID: " << GetAccountID() << endl;
    return retcode;
  }
  int VoiceCBL::Call(std::string accountID, std::string voiceServer)
  {
	std::string channelName = "";
	std::string pw = "";
	std::string msg;
    int retcode = 0;
    cout << "Calling " << voiceServer << "..." << endl;
    mutex2.Take();
    retcode = reqMgr->req_SessionCreate(accountID, voiceServer, channelName, pw, 1, 0);
    mutex2.Release();

	cout << "Calling";
	int i = 0;
	for (i = 0; ((i < 10) && (GetSessionID() == "")); i++) {
		Sleep(1000);
		cout << ".";
	}
	cout << "Session ID: " << GetSessionID() << endl;
    return retcode;
  }
  int VoiceCBL::Hangup(std::string sessionID)
  {
    int retcode = 0;
    cout << "Hanging up..." << endl;
    mutex2.Take();
    retcode = reqMgr->req_MediaDisconnect(sessionID);
    mutex2.Release();
	cout << "Hanging up";
	int i = 0;
	for (i = 0; ((i < 10) && (GetSessionID() != "SessionRemoved")); i++) {
		Sleep(1000);
		cout << ".";
	}
	m_SessionID = std::string("");
	cout << "Session closed." << endl;
    return retcode;
  }
  int VoiceCBL::Logout(std::string accountID)
  {
    int retcode = 0;
    if (stateMgr->GetAccountHandle() != accountID) {
      cout << endl << "Account handle " << accountID << " not found." << endl << endl;
      return retcode;
    }
    cout << "Logging out of Connector..." << endl;
    std::set<std::string> sghandles = stateMgr->GetSessionGroupHandles();
    set<string>::const_iterator sgh_itr;
    for (sgh_itr = sghandles.begin(); sgh_itr != sghandles.end(); ++sgh_itr) {
      VxSessionGroup* sg = stateMgr->GetSessionGroup(sgh_itr->data());
      std::set<std::string> handles = sg->GetSessionHandles();
      std::set<std::string>::const_iterator sh_itr;
      for (sh_itr = handles.begin(); sh_itr != handles.end(); ++sh_itr) {
	mutex2.Take();
	reqMgr->req_SessionTerminate(sh_itr->data());
	mutex2.Release();
	sg->RemoveSession(sh_itr->data());
      }
    }
    mutex2.Take();
    retcode = reqMgr->req_AccountLogout(accountID);
    mutex2.Release();

	cout << "Logging out";
	int i = 0;
	for (i = 0; ((i < 10) && (GetAccountID() != "LoggedOut")); i++) {
		Sleep(1000);
		cout << ".";
	}
	m_AccountID = std::string("");
	cout << "Account logged out." << endl;
    return retcode;
  }
  int VoiceCBL::Shutdown(std::string connectorID)
  {
    string msg;
    int retcode = 0;
    bool submitted = true;
    if (stateMgr->GetConnectorHandle() != connectorID) {
      cout << endl << "Connector handle " << connectorID << " not found." << endl << endl;
      return retcode;
    }
    cout << "Shutting Down Connector..." << endl;
    std::set<std::string> sghandles = stateMgr->GetSessionGroupHandles();
    set<string>::const_iterator sgh_itr;
    for (sgh_itr = sghandles.begin(); sgh_itr != sghandles.end(); ++sgh_itr) {
      VxSessionGroup* sg = stateMgr->GetSessionGroup(sgh_itr->data());
      std::set<std::string> handles = sg->GetSessionHandles();
      std::set<std::string>::const_iterator sh_itr;
      for (sh_itr = handles.begin(); sh_itr != handles.end(); ++sh_itr) {
	mutex2.Take();
	reqMgr->req_SessionTerminate(sh_itr->data());
	mutex2.Release();
	sg->RemoveSession(sh_itr->data());
      }
    }
    if (stateMgr->GetIsAccountLoggedIn() && stateMgr->GetAccountHandle().size() > 0) {
      mutex2.Take();
      reqMgr->req_AccountLogout(stateMgr->GetAccountHandle());
      mutex2.Release();
      stateMgr->SetStateAccountLoggedOut();
    }
    mutex2.Take();
    retcode = reqMgr->req_ConnectorShutdown(connectorID);
    mutex2.Release();

    bool shutdown = false;
    if (stateMgr->GetIsConnectorInitialized() && stateMgr->GetConnectorHandle().size() > 0) {
      shutdown = true;
      reqMgr->req_ConnectorShutdown(stateMgr->GetConnectorHandle());
      stateMgr->SetConnectorUninitialized();
    }
    respMgr->Stop(shutdown);
    vxplatform::set_event(g_responseHandle);
    cout << "Quitting.\n";

    // wait_event(listenerThreadTerminatedEvent);

    // delete stateMgr;
    // delete reqMgr;
    // delete respMgr;

    m_ConnectorID = std::string("");
    cout << "Connecting shutdown." << endl;

    return retcode;
  }
  int VoiceCBL::MicMute(std::string connectorID, bool mute)
  {
    int retcode = 0;
    mutex2.Take();
    retcode = reqMgr->req_MicMute(connectorID, mute);
    mutex2.Release();
    return retcode;
  }

  void OnMessage(void *)
  {
    set_event(g_responseHandle);
  }

  void ProcessMessage(vx_message_base_t *basemsg);

  // Pulls a message from the queue.  If no message found, waits 100ms, then tries again.
  // Type of the message, and then type of the Event or Response is discovered, and the
  // appropriate call is made to process that message type.
  os_error_t ListenerThread(void *arg)
  {
    create_event(&g_responseHandle);
	vx_register_message_notification_handler(OnMessage, NULL);
    for (;!respMgr->IsStopped();) {
      vx_message_base_t* basemsg = NULL;
      int ret = vx_get_message(&basemsg);			//Pull a message from the queue
      if(ret == 0) {
	ProcessMessage(basemsg);
	continue;
      } else {
	wait_event(g_responseHandle);
      }
    }
	vx_unregister_message_notification_handler(OnMessage, NULL);
    vxplatform::set_event(listenerThreadTerminatedEvent);
    return 0;
  }

  void ProcessMessage(vx_message_base_t *basemsg)
  {
    vx_message_type messageType;
    mutex2.Take();
    messageType = basemsg->type;		//Get the type of the message (Response or Event)
    switch (messageType)
      {
      case msg_event:
	{
	  vx_evt_base_t* theEvent;
	  theEvent = (vx_evt_base_t*)basemsg;
	  vx_event_type eventType;
	  eventType = theEvent->type;

	  //These are events that we want to perform state mgmt or special reporting for .
	  switch (eventType)		//Find the specific Event type, process the Event
	    {
	    case evt_account_login_state_change:
	      respMgr->evt_AccountLoginStateChange((vx_evt_account_login_state_change_t*)theEvent);
	      cout << "The Account Handle is : " << (((vx_evt_account_login_state_change_t*)theEvent)->account_handle) << endl;
		  if (((vx_evt_account_login_state_change_t*)theEvent)->state) { // login_state_logged_in == 1, login_state_logged_out == 0
			  cout << "Logging in." << endl;
			  m_AccountID = std::string(((vx_evt_account_login_state_change_t*)theEvent)->account_handle);
		  } else {
			  cout << "Logging out." << endl;
			  m_AccountID = std::string("LoggedOut");
		  }
	      break;
	    case evt_media_stream_updated:
	      respMgr->evt_MediaStreamUpdated((vx_evt_media_stream_updated_t*)theEvent);
		  cout << "The Session Handle is : " << (((vx_evt_media_stream_updated_t*)theEvent)->session_handle) << endl;
		  m_SessionID = std::string(((vx_evt_media_stream_updated_t*)theEvent)->session_handle);
	      break;
	    case evt_text_stream_updated:
	      respMgr->evt_TextStreamUpdated((vx_evt_text_stream_updated_t*)theEvent);
	      break;
	    case evt_sessiongroup_added:
	      respMgr->evt_SessionGroupAdded((vx_evt_sessiongroup_added_t*)theEvent);
	      break;
	    case evt_sessiongroup_removed:
	      respMgr->evt_SessionGroupRemoved((vx_evt_sessiongroup_removed_t*)theEvent);
	      break;
	    case evt_session_added:
	      respMgr->evt_SessionAdded((vx_evt_session_added_t*)theEvent);
	      break;
	    case evt_session_removed:
	      respMgr->evt_SessionRemoved((vx_evt_session_removed_t*)theEvent);
		  cout << "Event Session Removed" << endl;
		  m_SessionID = std::string("SessionRemoved");
	      break;
	    case evt_message:
	      respMgr->evt_Message((vx_evt_message_t*)theEvent);
	      break;
	    case evt_buddy_presence:
	      respMgr->evt_BuddyPresenceChange((vx_evt_buddy_presence_t*)theEvent);
	      break;
	    case evt_buddy_changed:
	      respMgr->evt_BuddyChanged((vx_evt_buddy_changed_t *)theEvent);
	      break;
	    case evt_buddy_group_changed:
	      respMgr->evt_BuddyGroupChanged((vx_evt_buddy_group_changed_t *)theEvent);
	      break;
	    case evt_aux_audio_properties:
	      respMgr->evt_AuxAudioProperties((vx_evt_aux_audio_properties_t*)theEvent);
	      break;
	    default:
	      //respMgr->evt_Generic(theEvent);
	      break;
	    }
	  if (theEvent->type != evt_aux_audio_properties)     //suppress these beacuse they are too numerous
	    {
	      respMgr->WriteEventToFile(theEvent);
	    }
	  destroy_evt(theEvent);
	  break;
	}
      case msg_response:
	{
	  vx_resp_base_t* theResponse;
	  theResponse = (vx_resp_base_t*)basemsg;
	  vx_response_type responseType;
	  responseType = theResponse->type;

	  if (theResponse->return_code != 0)
	    {
	      cout << endl << endl << "ERROR in response.  Status Code: " << theResponse->status_code << endl << "Status String: " << theResponse->status_string << endl << endl;
	    }
	  else
	    {
	      //These are responses that we want to perform state mgmt or special reporting for .
	      switch (responseType)		//Find the specific Response type, process the Response
		{
		case resp_connector_create:
		  respMgr->resp_ConnectorCreate((vx_resp_connector_create_t*)theResponse);
		  cout << "The Connector Handle is : " << (((vx_resp_connector_create_t*)theResponse)->connector_handle) << endl;
		  m_ConnectorID = string(((vx_resp_connector_create_t*)theResponse)->connector_handle);
		  break;
		case resp_connector_initiate_shutdown:
		  respMgr->resp_ConnectorShutdown((vx_resp_connector_initiate_shutdown_t*)theResponse);
		  break;
		case resp_sessiongroup_set_tx_session:
		  respMgr->resp_SGSetTXSession((vx_resp_sessiongroup_set_tx_session_t*)theResponse);
		  break;
		case resp_sessiongroup_set_tx_all_sessions:
		  respMgr->resp_SGSetTXAll((vx_resp_sessiongroup_set_tx_all_sessions_t*)theResponse);
		  break;
		case resp_sessiongroup_set_tx_no_session:
		  respMgr->resp_SGSetTXNone((vx_resp_sessiongroup_set_tx_no_session_t*)theResponse);
		  break;
		case resp_account_channel_create:
		  respMgr->resp_ChannelCreate((vx_resp_account_channel_create_t*)theResponse);
		  break;
		case resp_account_channel_folder_create:
		  respMgr->resp_ChannelFolderCreate((vx_resp_account_channel_folder_create_t*)theResponse);
		  break;
		case resp_account_channel_favorite_set:
		  respMgr->resp_ChannelFavSet((vx_resp_account_channel_favorite_set_t*)theResponse);
		  break;
		case resp_account_channel_favorite_group_set:
		  respMgr->resp_ChannelFavGroupSet((vx_resp_account_channel_favorite_group_set_t*)theResponse);
		  break;
		case resp_aux_connectivity_info:
		  respMgr->resp_NetworkTest((vx_resp_aux_connectivity_info_t*)theResponse);
		  break;
		case resp_aux_get_render_devices:
		  respMgr->resp_ListRenderDevices((vx_resp_aux_get_render_devices_t*)theResponse);
		  break;
		case resp_aux_get_capture_devices:
		  respMgr->resp_ListCaptureDevices((vx_resp_aux_get_capture_devices_t*)theResponse);
		  break;
		}
	    }
	  respMgr->WriteResponseToFile(theResponse);
	  std::string reqID(theResponse->request->cookie ? theResponse->request->cookie : "");
	  stateMgr->DeleteCommandReqID(reqID);
	  destroy_resp(theResponse);
	  break;
	}
      default:
	break;
      }
    mutex2.Release();
  }
}
