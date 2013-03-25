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

#include "RequestManager.h"
#include <stdio.h>

using namespace std;

RequestManager::RequestManager(StateManager* sMgr)
{
	debuglevel = 0;
	stateMgr = sMgr;
}

void RequestManager::SetDebug(int debugval)
{
	debuglevel = debugval;
}

int RequestManager::req_ConnectorCreate(std::string acctmgmtserver, int minimumPort, int maximumPort)
{
	std::string newReqID = stateMgr->GenerateID();
	stateMgr->InsertCommandReqID(newReqID, "vx_req_connector_create_t");

	vx_req_connector_create_t* reqStruct = NULL;
	vx_req_connector_create_create(&reqStruct);
    reqStruct->mode = connector_mode_normal;
	reqStruct->acct_mgmt_server = vx_strdup(acctmgmtserver.c_str());
	reqStruct->minimum_port = minimumPort;
	reqStruct->maximum_port = maximumPort;
	reqStruct->client_name = vx_strdup("TestApp");
	reqStruct->log_filename_prefix = vx_strdup("vx");
	reqStruct->log_filename_suffix = vx_strdup(".txt");
#ifdef _WIN32
	reqStruct->log_folder = vx_strdup("C:\\VivoxLogs\\");
#else
    reqStruct->log_folder = vx_strdup("./VivoxLogs/");
#endif
	reqStruct->log_level = 4;
	reqStruct->base.cookie = vx_strdup(newReqID.c_str());

	int ret = vx_issue_request((vx_req_base_t*)reqStruct);
	return ret;
}

int RequestManager::req_AccountLogin(std::string chandle, std::string username, std::string password, int pres, int autoanswer, int text, int participantPropertyFrequnecy)
{
    int ret = -1;
    if (stateMgr->GetIsConnectorInitialized() && stateMgr->GetConnectorHandle() == chandle)
	{
		std::string newReqID = stateMgr->GenerateID();
		stateMgr->InsertCommandReqID(newReqID, "vx_req_account_login_t");

		vx_req_account_login_t* reqStruct = NULL;
		vx_req_account_login_create(&reqStruct);
		reqStruct->acct_name = vx_strdup(username.c_str());
		reqStruct->acct_password = vx_strdup(password.c_str());
        if (autoanswer == 0)
        {
		    reqStruct->answer_mode = mode_verify_answer;
        }
        else
        {
            reqStruct->answer_mode = mode_auto_answer;
        }
        reqStruct->participant_property_frequency = participantPropertyFrequnecy;
		reqStruct->base.cookie = vx_strdup(newReqID.c_str());
		reqStruct->connector_handle = vx_strdup(chandle.c_str());
        if (pres > 0)
        {
            reqStruct->enable_buddies_and_presence = 1;
        }
        if (text > 0)
        {
            reqStruct->enable_text = text_mode_enabled;
        }
        else
        {
            reqStruct->enable_text = text_mode_disabled;
        }

		ret = vx_issue_request((vx_req_base_t*)reqStruct);
	}
	else
	{
		cout << "Connector handle " << chandle << " not found." << endl;
	}
	return ret;
}

int RequestManager::req_SessionGroupCreate(std::string ahandle)
{
    int ret = -1;
    if (stateMgr->GetIsAccountLoggedIn() && stateMgr->GetAccountHandle() == ahandle)
	{
	    vx_req_sessiongroup_create_t* reqStruct;
	    vx_req_sessiongroup_create_create(&reqStruct);

        reqStruct->account_handle = vx_strdup(ahandle.c_str());
	    std::string newReqID = stateMgr->GenerateID();
	    reqStruct->base.cookie = vx_strdup(newReqID.c_str());

	    stateMgr->InsertCommandReqID(newReqID, "vx_req_sessiongroup_create_t");
	    ret = vx_issue_request((vx_req_base_t*)reqStruct);
    }
    else
	{
		cout << "Account handle " << ahandle << " not found." << endl;
	}
    return ret;
}

int RequestManager::req_SessionGroupTerminate(std::string sghandle)
{
    int ret = -1;
    if (!stateMgr->GetSessionGroupExists(sghandle))
    {
        cout << "SessionGroup handle " << sghandle << " not found." << endl;
        return ret;
    }

    vx_req_sessiongroup_terminate_t* reqStruct = NULL;
    vx_req_sessiongroup_terminate_create(&reqStruct);

    reqStruct->sessiongroup_handle = vx_strdup(sghandle.c_str());
    std::string newReqID = stateMgr->GenerateID();
    reqStruct->base.cookie = vx_strdup(newReqID.c_str());

    stateMgr->InsertCommandReqID(newReqID, "vx_req_sessiongroup_terminate_t");
    ret = vx_issue_request((vx_req_base_t*)reqStruct);
	return ret;
}

int RequestManager::req_SessionGroupAddSession(std::string sghandle, std::string uri, int connect_audio, std::string pw)
{
    int ret = -1;
    if (!stateMgr->GetSessionGroupExists(sghandle))
    {
        cout << "SessionGroup handle " << sghandle << " not found." << endl;
        return ret;
    }

    vx_req_sessiongroup_add_session_t* reqStruct = NULL;
    vx_req_sessiongroup_add_session_create(&reqStruct);

    reqStruct->sessiongroup_handle = vx_strdup(sghandle.c_str());
    reqStruct->uri = vx_strdup(uri.c_str());
    reqStruct->connect_audio = connect_audio;
    reqStruct->password = vx_strdup(pw.c_str());
    std::string newReqID = stateMgr->GenerateID();
    reqStruct->base.cookie = vx_strdup(newReqID.c_str());

    stateMgr->InsertCommandReqID(newReqID, "vx_req_sessiongroup_add_session_t");
    ret = vx_issue_request((vx_req_base_t*)reqStruct);
	return ret;
}

int RequestManager::req_SessionGroupRemoveSession(std::string sghandle, std::string shandle)
{
    int ret = -1;
    if (!stateMgr->GetSessionGroupExists(sghandle))
    {
        cout << "SessionGroup handle " << sghandle << " not found." << endl;
        return ret;
    }

    vx_req_sessiongroup_remove_session_t* reqStruct = NULL;
    vx_req_sessiongroup_remove_session_create(&reqStruct);

    reqStruct->sessiongroup_handle = vx_strdup(sghandle.c_str());
    reqStruct->session_handle = vx_strdup(shandle.c_str());
    std::string newReqID = stateMgr->GenerateID();
    reqStruct->base.cookie = vx_strdup(newReqID.c_str());

    stateMgr->InsertCommandReqID(newReqID, "vx_req_sessiongroup_remove_session_t");
    ret = vx_issue_request((vx_req_base_t*)reqStruct);
	return ret;
}

int RequestManager::req_SessionGroupSetFocus(std::string sghandle, std::string shandle)
{
    int ret = -1;
    if (!stateMgr->GetSessionGroupExists(sghandle))
    {
        cout << "SessionGroup handle " << sghandle << " not found." << endl;
        return ret;
    }

    vx_req_sessiongroup_set_focus_t* reqStruct = NULL;
    vx_req_sessiongroup_set_focus_create(&reqStruct);

    reqStruct->sessiongroup_handle = vx_strdup(sghandle.c_str());
    reqStruct->session_handle = vx_strdup(shandle.c_str());
    std::string newReqID = stateMgr->GenerateID();
    reqStruct->base.cookie = vx_strdup(newReqID.c_str());

    stateMgr->InsertCommandReqID(newReqID, "vx_req_sessiongroup_set_focus_t");
    ret = vx_issue_request((vx_req_base_t*)reqStruct);
	return ret;
}

int RequestManager::req_SessionGroupUnsetFocus(std::string sghandle, std::string shandle)
{
    int ret = -1;
    if (!stateMgr->GetSessionGroupExists(sghandle))
    {
        cout << "SessionGroup handle " << sghandle << " not found." << endl;
        return ret;
    }

    vx_req_sessiongroup_unset_focus_t* reqStruct = NULL;
    vx_req_sessiongroup_unset_focus_create(&reqStruct);

    reqStruct->sessiongroup_handle = vx_strdup(sghandle.c_str());
    reqStruct->session_handle = vx_strdup(shandle.c_str());
    std::string newReqID = stateMgr->GenerateID();
    reqStruct->base.cookie = vx_strdup(newReqID.c_str());

    stateMgr->InsertCommandReqID(newReqID, "vx_req_sessiongroup_unset_focus_t");
    ret = vx_issue_request((vx_req_base_t*)reqStruct);
	return ret;
}

int RequestManager::req_SessionGroupResetFocus(std::string sghandle)
{
    int ret = -1;
    if (!stateMgr->GetSessionGroupExists(sghandle))
    {
        cout << "SessionGroup handle " << sghandle << " not found." << endl;
        return ret;
    }

    vx_req_sessiongroup_reset_focus_t* reqStruct = NULL;
    vx_req_sessiongroup_reset_focus_create(&reqStruct);

    reqStruct->sessiongroup_handle = vx_strdup(sghandle.c_str());
    std::string newReqID = stateMgr->GenerateID();
    reqStruct->base.cookie = vx_strdup(newReqID.c_str());

    stateMgr->InsertCommandReqID(newReqID, "vx_req_sessiongroup_reset_focus_t");
    ret = vx_issue_request((vx_req_base_t*)reqStruct);
	return ret;
}

int RequestManager::req_SessionGroupSetTx(std::string sghandle, std::string shandle)
{
    int ret = -1;
    if (!stateMgr->GetSessionGroupExists(sghandle))
    {
        cout << "SessionGroup handle " << sghandle << " not found." << endl;
        return ret;
    }

    vx_req_sessiongroup_set_tx_session_t* reqStruct = NULL;
    vx_req_sessiongroup_set_tx_session_create(&reqStruct);

    reqStruct->sessiongroup_handle = vx_strdup(sghandle.c_str());
    reqStruct->session_handle = vx_strdup(shandle.c_str());
    std::string newReqID = stateMgr->GenerateID();
    reqStruct->base.cookie = vx_strdup(newReqID.c_str());

    stateMgr->InsertCommandReqID(newReqID, "vx_req_sessiongroup_set_tx_session_t");
    ret = vx_issue_request((vx_req_base_t*)reqStruct);
	return ret;
}

int RequestManager::req_SessionGroupSetTxAll(std::string sghandle)
{
    int ret = -1;
    if (!stateMgr->GetSessionGroupExists(sghandle))
    {
        cout << "SessionGroup handle " << sghandle << " not found." << endl;
        return ret;
    }

    vx_req_sessiongroup_set_tx_all_sessions_t* reqStruct = NULL;
    vx_req_sessiongroup_set_tx_all_sessions_create(&reqStruct);

    reqStruct->sessiongroup_handle = vx_strdup(sghandle.c_str());
    std::string newReqID = stateMgr->GenerateID();
    reqStruct->base.cookie = vx_strdup(newReqID.c_str());

    stateMgr->InsertCommandReqID(newReqID, "vx_req_sessiongroup_set_tx_all_sessions_t");
    ret = vx_issue_request((vx_req_base_t*)reqStruct);
	return ret;
}

int RequestManager::req_SessionGroupSetTxNone(std::string sghandle)
{
    int ret = -1;
    if (!stateMgr->GetSessionGroupExists(sghandle))
    {
        cout << "SessionGroup handle " << sghandle << " not found." << endl;
        return ret;
    }

    vx_req_sessiongroup_set_tx_no_session_t* reqStruct = NULL;
    vx_req_sessiongroup_set_tx_no_session_create(&reqStruct);

    reqStruct->sessiongroup_handle = vx_strdup(sghandle.c_str());
    std::string newReqID = stateMgr->GenerateID();
    reqStruct->base.cookie = vx_strdup(newReqID.c_str());

    stateMgr->InsertCommandReqID(newReqID, "vx_req_sessiongroup_set_tx_no_session_t");
    ret = vx_issue_request((vx_req_base_t*)reqStruct);
	return ret;
}

int RequestManager::req_SessionCreate(std::string ahandle, std::string desturi, std::string channelName, std::string pw, int connectAudio, int passwordHashed)
{
    int ret = -1;
	if (stateMgr->GetIsAccountLoggedIn() && stateMgr->GetAccountHandle() == ahandle)
	{
		std::string newReqID = stateMgr->GenerateID();
		stateMgr->InsertCommandReqID(newReqID, "vx_req_session_create_t");

		vx_req_session_create_t* reqStruct = NULL;
		vx_req_session_create_create(&reqStruct);
        if (connectAudio == 0)
        {
            reqStruct->connect_audio = 0;
        }
        else
        {
            reqStruct->connect_audio = 1;
        }
		reqStruct->name = vx_strdup("");
		reqStruct->uri = vx_strdup(desturi.c_str());
		reqStruct->base.cookie = vx_strdup(newReqID.c_str());
		reqStruct->account_handle = vx_strdup(ahandle.c_str());
		if (channelName.size() > 0)
		{
			reqStruct->name = vx_strdup(channelName.c_str());
		}
		if (pw.size() > 0)
		{
			reqStruct->password = vx_strdup(pw.c_str());
		}

        if(passwordHashed == 0)
            reqStruct->password_hash_algorithm = password_hash_algorithm_cleartext;
        else if (passwordHashed == 1)
            reqStruct->password_hash_algorithm = password_hash_algorithm_sha1_username_hash;
        else
            reqStruct->password_hash_algorithm = password_hash_algorithm_cleartext;

        /*reqStruct->join_audio = hasAudio;
        reqStruct->join_text = hasText;*/
 
		ret = vx_issue_request((vx_req_base_t*)reqStruct);
	}
	else
	{
		cout << "Account handle " << ahandle << " not found." << endl;
	}
	return ret;
}

int RequestManager::req_ConnectSession(std::string shandle)
{
    int ret = -1;
    int state = stateMgr->GetMediaSessionState(shandle);
    if (state < 0)
    {
        cout << "Session handle " << shandle << " not found." << endl;
        return ret;
    }

    std::string newReqID = stateMgr->GenerateID();
    stateMgr->InsertCommandReqID(newReqID, "vx_req_session_connect_t");

    vx_req_session_connect_t* reqStruct = NULL;
    vx_req_session_connect_create(&reqStruct);
    reqStruct->base.cookie = vx_strdup(newReqID.c_str());
    reqStruct->session_handle = vx_strdup(shandle.c_str());

    ret = vx_issue_request((vx_req_base_t*)reqStruct);
	return ret;
}

int RequestManager::req_MediaConnect(std::string shandle)
{
    int ret = -1;
    int state = stateMgr->GetMediaSessionState(shandle);
    if (state < 0)
    {
        cout << "Session handle " << shandle << " not found." << endl;
        return ret;
    }

    std::string newReqID = stateMgr->GenerateID();
    stateMgr->InsertCommandReqID(newReqID, "vx_req_session_media_connect_t");

    vx_req_session_media_connect_t* reqStruct = NULL;
    vx_req_session_media_connect_create(&reqStruct);
    reqStruct->base.cookie = vx_strdup(newReqID.c_str());
    reqStruct->session_handle = vx_strdup(shandle.c_str());
    reqStruct->media = media_type_audio;
    ret = vx_issue_request((vx_req_base_t*)reqStruct);
	return ret;
}

int RequestManager::req_MediaDisconnect(std::string shandle)
{
    int ret = -1;
    int state = stateMgr->GetMediaSessionState(shandle);
    if (state < 0)
    {
        cout << "Session handle " << shandle << " not found." << endl;
        return ret;
    }

    std::string newReqID = stateMgr->GenerateID();
    stateMgr->InsertCommandReqID(newReqID, "vx_req_session_media_disconnect_t");

    vx_req_session_media_disconnect_t* reqStruct = NULL;
    vx_req_session_media_disconnect_create(&reqStruct);
    reqStruct->base.cookie = vx_strdup(newReqID.c_str());
    reqStruct->session_handle = vx_strdup(shandle.c_str());
    reqStruct->media = media_type_audio;
    ret = vx_issue_request((vx_req_base_t*)reqStruct);
	return ret;
}

int RequestManager::req_SessionTerminate(std::string shandle)
{
    int ret = -1;
    int state = stateMgr->GetMediaSessionState(shandle);
    if (state < 0)
    {
        cout << "Session handle " << shandle << " not found." << endl;
        return ret;
    }

    std::string newReqID = stateMgr->GenerateID();
    stateMgr->InsertCommandReqID(newReqID, "vx_req_session_terminate_t");

    vx_req_session_terminate_t* reqStruct = NULL;
    vx_req_session_terminate_create(&reqStruct);
    reqStruct->base.cookie = vx_strdup(newReqID.c_str());
    reqStruct->session_handle = vx_strdup(shandle.c_str());
    ret = vx_issue_request((vx_req_base_t*)reqStruct);
	return ret;
}

int RequestManager::req_SessionMuteLocalSpeaker(std::string shandle, int mute)
{
    int ret = -1;
    int state = stateMgr->GetMediaSessionState(shandle);
    if (state < 0)
    {
        cout << "Session handle " << shandle << " not found." << endl;
        return ret;
    }

    vx_req_session_mute_local_speaker_t* reqStruct = NULL;
    vx_req_session_mute_local_speaker_create(&reqStruct);

    reqStruct->session_handle = vx_strdup(shandle.c_str());
    reqStruct->mute_level = mute;

    std::string newReqID = stateMgr->GenerateID();
    reqStruct->base.cookie = vx_strdup(newReqID.c_str());
    
    stateMgr->InsertCommandReqID(newReqID, "vx_req_session_mute_local_speaker_t");
    ret = vx_issue_request((vx_req_base_t*)reqStruct);
	return ret;
}

int RequestManager::req_SessionSetLocalSpeakerVolume(std::string shandle, int vol)
{
    int ret = -1;
    int state = stateMgr->GetMediaSessionState(shandle);
    if (state < 0)
    {
        cout << "Session handle " << shandle << " not found." << endl;
        return ret;
    }

    vx_req_session_set_local_speaker_volume_t* reqStruct = NULL;
    vx_req_session_set_local_speaker_volume_create(&reqStruct);

    reqStruct->session_handle = vx_strdup(shandle.c_str());
    reqStruct->volume = vol;

    std::string newReqID = stateMgr->GenerateID();
    reqStruct->base.cookie = vx_strdup(newReqID.c_str());
    
    stateMgr->InsertCommandReqID(newReqID, "vx_req_session_set_local_speaker_volume_t");
    ret = vx_issue_request((vx_req_base_t*)reqStruct);
	return ret;
}

int RequestManager::req_SessionGetLocalAudioInfo(std::string shandle)
{
    int ret = -1;
    int state = stateMgr->GetMediaSessionState(shandle);
    if (state < 0)
    {
        cout << "Session handle " << shandle << " not found." << endl;
        return ret;
    }

    vx_req_session_get_local_audio_info_t* reqStruct = NULL;
    vx_req_session_get_local_audio_info_create(&reqStruct);

    reqStruct->session_handle = vx_strdup(shandle.c_str());

    std::string newReqID = stateMgr->GenerateID();
    reqStruct->base.cookie = vx_strdup(newReqID.c_str());
    
    stateMgr->InsertCommandReqID(newReqID, "vx_req_session_get_local_audio_info_t");
    ret = vx_issue_request((vx_req_base_t*)reqStruct);
	return ret;
}

int RequestManager::req_AccountLogout(std::string ahandle)
{
    int ret = -1;
	if (stateMgr->GetIsAccountLoggedIn() && stateMgr->GetAccountHandle() == ahandle)
	{
		std::string newReqID = stateMgr->GenerateID();
		stateMgr->InsertCommandReqID(newReqID, "vx_req_account_logout_t");

		vx_req_account_logout_t* reqStruct = NULL;
		vx_req_account_logout_create(&reqStruct);
		reqStruct->base.cookie = vx_strdup(newReqID.c_str());
		reqStruct->account_handle = vx_strdup(ahandle.c_str());

		ret = vx_issue_request((vx_req_base_t*)reqStruct);
	}
	else
	{
		cout << "Account handle " << ahandle << " not found." << endl;
	}
	return ret;
}

int RequestManager::req_SetLoginProperties(std::string ahandle, int autoanswer, int participantPropertyFrequnecy)
{
    int ret = -1;
    if (stateMgr->GetIsAccountLoggedIn() && stateMgr->GetAccountHandle() == ahandle)
	{
        std::string newReqID = stateMgr->GenerateID();
        stateMgr->InsertCommandReqID(newReqID, "vx_req_account_set_login_properties_t");

		vx_req_account_set_login_properties_t* reqStruct = NULL;
		vx_req_account_set_login_properties_create(&reqStruct);
		reqStruct->base.cookie = vx_strdup(newReqID.c_str());
		reqStruct->account_handle = vx_strdup(ahandle.c_str());
        reqStruct->participant_property_frequency = participantPropertyFrequnecy;
        if (autoanswer == 0)
        {
		    reqStruct->answer_mode = mode_verify_answer;
        }
        else
        {
            reqStruct->answer_mode = mode_auto_answer;
        }

		ret = vx_issue_request((vx_req_base_t*)reqStruct);
	}
	else
	{
		cout << "Account handle " << ahandle << " not found." << endl;
	}
    return ret;
}

int RequestManager::req_ConnectorShutdown(std::string chandle)
{
    int ret = -1;
	if (stateMgr->GetIsConnectorInitialized() && stateMgr->GetConnectorHandle() == chandle)
	{
		std::string newReqID = stateMgr->GenerateID();
		stateMgr->InsertCommandReqID(newReqID, "vx_req_connector_initiate_shutdown_t");

		vx_req_connector_initiate_shutdown_t* reqStruct = NULL;
		vx_req_connector_initiate_shutdown_create(&reqStruct);
		reqStruct->client_name = vx_strdup("");
		reqStruct->base.cookie = vx_strdup(newReqID.c_str());
		reqStruct->connector_handle = vx_strdup(chandle.c_str());

		ret = vx_issue_request((vx_req_base_t*)reqStruct);
	}
	else
	{
		cout << "Connector handle " << chandle << " not found." << endl;
	}
	return ret;
}

int RequestManager::req_CreateChannelAndInvite(std::string ahandle, std::string chan_name, std::string chan_desc, int chan_parentID, int chan_cap, int chan_maxparts, int chan_ispersistent, std::string uri, std::string chan_password)
{
    int ret = -1;
    if (stateMgr->GetIsAccountLoggedIn() && stateMgr->GetAccountHandle() == ahandle)
	{
	    vx_req_account_channel_create_and_invite_t* reqStruct;
	    vx_req_account_channel_create_and_invite_create(&reqStruct);

        reqStruct->account_handle = vx_strdup(ahandle.c_str());
	    reqStruct->channel_name = vx_strdup(chan_name.c_str());
	    reqStruct->channel_desc = vx_strdup(chan_desc.c_str());
	    reqStruct->parent_id = chan_parentID;
	    if (chan_cap > 0)
	    {
		    reqStruct->capacity = chan_cap;
	    }
	    reqStruct->max_participants = chan_maxparts;
	    reqStruct->is_persistent = chan_ispersistent;
	    vx_string_list_create(1,&reqStruct->participant_uris);
	    reqStruct->participant_uris[0] = vx_strdup(uri.c_str());
	    if (chan_password.size() > 0)
	    {
		    reqStruct->is_protected = 1;
		    reqStruct->protected_password = vx_strdup(chan_password.c_str());
	    }

	    std::string newReqID = stateMgr->GenerateID();
	    reqStruct->base.cookie = vx_strdup(newReqID.c_str());
	    stateMgr->InsertCommandReqID(newReqID, "vx_req_account_channel_create_and_invite_t");
	    ret = vx_issue_request((vx_req_base_t*)reqStruct);
    }
    else
	{
		cout << "Account handle " << ahandle << " not found." << endl;
	}
	return ret;
}

int RequestManager::req_CreateChannel(std::string ahandle, std::string chan_name, std::string chan_desc, vx_channel_type channel_type, int chan_parentID, int chan_cap, int chan_maxparts, int chan_ispersistent, int maxrange, int clampingdist, double rolloff, double maxgain, int distmodel, std::string chan_password)
{
    int ret = -1;
    if (stateMgr->GetIsAccountLoggedIn() && stateMgr->GetAccountHandle() == ahandle)
	{
	    vx_req_account_channel_create_t* reqStruct;
	    vx_req_account_channel_create_create(&reqStruct);

        reqStruct->account_handle = vx_strdup(ahandle.c_str());
        reqStruct->channel_name = vx_strdup(chan_name.c_str());
        reqStruct->channel_desc = vx_strdup(chan_desc.c_str());
        reqStruct->parent_id = chan_parentID;
	    reqStruct->channel_type = channel_type;
        reqStruct->capacity = chan_cap;
        reqStruct->max_participants = chan_maxparts;
        reqStruct->set_persistent = chan_ispersistent;
        reqStruct->max_range = maxrange;
        reqStruct->clamping_dist = clampingdist;
        reqStruct->roll_off = rolloff;
        reqStruct->max_gain = maxgain;
        reqStruct->dist_model = distmodel;

	    if (chan_password.length() > 0)
	    {
		    reqStruct->set_protected = 1;
		    reqStruct->protected_password = vx_strdup(chan_password.c_str());
	    }
        else
	    {
		    reqStruct->set_protected = 0;
	    }

	    std::string newReqID = stateMgr->GenerateID();
	    reqStruct->base.cookie = vx_strdup(newReqID.c_str());
	    stateMgr->InsertCommandReqID(newReqID, "vx_req_account_channel_create_t");
	    ret = vx_issue_request((vx_req_base_t*)reqStruct);
    }
    else
	{
		cout << "Account handle " << ahandle << " not found." << endl;
	}
	return ret;
}

int RequestManager::req_UpdateChannel(std::string ahandle, std::string chan_uri, std::string chan_name, std::string chan_desc, int chan_cap, int chan_maxparts, int chan_ispersistent, int chan_isprotected, int maxrange, int clampingdist, double rolloff, double maxgain, int distmodel, std::string chan_password)
{
    int ret = -1;
    if (stateMgr->GetIsAccountLoggedIn() && stateMgr->GetAccountHandle() == ahandle)
	{
	    vx_req_account_channel_update_t* reqStruct;
	    vx_req_account_channel_update_create(&reqStruct);

        reqStruct->account_handle = vx_strdup(ahandle.c_str());
	    reqStruct->channel_uri = vx_strdup(chan_uri.c_str());
	    reqStruct->channel_name = vx_strdup(chan_name.c_str());
	    reqStruct->channel_desc = vx_strdup(chan_desc.c_str());
		reqStruct->capacity = chan_cap;
        reqStruct->max_participants = chan_maxparts;
        reqStruct->set_persistent = chan_ispersistent;
        reqStruct->max_range = maxrange;
        reqStruct->clamping_dist = clampingdist;
        reqStruct->roll_off = rolloff;
        reqStruct->max_gain = maxgain;
        reqStruct->dist_model = distmodel;

	    if (chan_isprotected == 1)
	    {
		    reqStruct->set_protected = 1;
		    reqStruct->protected_password = vx_strdup(chan_password.c_str());
	    }
	    else if (chan_isprotected == 0)
	    {
		    reqStruct->set_protected = 0;
	    }

	    std::string newReqID = stateMgr->GenerateID();
	    reqStruct->base.cookie = vx_strdup(newReqID.c_str());
	    stateMgr->InsertCommandReqID(newReqID, "vx_req_account_channel_update_t");
	    ret = vx_issue_request((vx_req_base_t*)reqStruct);
    }
    else
	{
		cout << "Account handle " << ahandle << " not found." << endl;
	}
	return ret;
}

int RequestManager::req_DeleteChannel(std::string ahandle, std::string chan_uri)
{
    int ret = -1;
    if (stateMgr->GetIsAccountLoggedIn() && stateMgr->GetAccountHandle() == ahandle)
	{
	    vx_req_account_channel_delete_t* reqStruct;
	    vx_req_account_channel_delete_create(&reqStruct);

        reqStruct->account_handle = vx_strdup(ahandle.c_str());
	    reqStruct->channel_uri = vx_strdup(chan_uri.c_str());

	    std::string newReqID = stateMgr->GenerateID();
	    reqStruct->base.cookie = vx_strdup(newReqID.c_str());
	    stateMgr->InsertCommandReqID(newReqID, "vx_req_account_channel_delete_t");
	    ret = vx_issue_request((vx_req_base_t*)reqStruct);
	    return ret;
    }
    else
	{
		cout << "Account handle " << ahandle << " not found." << endl;
	}
    return ret;
}

int RequestManager::req_CreateFolder(std::string ahandle, std::string folder_name, std::string folder_desc, int folder_parentID)
{
    int ret = -1;
    if (stateMgr->GetIsAccountLoggedIn() && stateMgr->GetAccountHandle() == ahandle)
	{
        vx_req_account_channel_folder_create_t* reqStruct;
        vx_req_account_channel_folder_create_create(&reqStruct);

        reqStruct->account_handle = vx_strdup(ahandle.c_str());
        reqStruct->folder_name = vx_strdup(folder_name.c_str());
        reqStruct->folder_desc = vx_strdup(folder_desc.c_str());
        reqStruct->parent_id = folder_parentID;

        std::string newReqID = stateMgr->GenerateID();
        reqStruct->base.cookie = vx_strdup(newReqID.c_str());
        stateMgr->InsertCommandReqID(newReqID, "vx_req_account_channel_folder_create_t");
        ret = vx_issue_request((vx_req_base_t*)reqStruct);
    }
    else
	{
		cout << "Account handle " << ahandle << " not found." << endl;
	}
    return ret;
}

int RequestManager::req_UpdateFolder(std::string ahandle, int folderID, std::string folder_name, std::string folder_desc)
{
    int ret = -1;
    if (stateMgr->GetIsAccountLoggedIn() && stateMgr->GetAccountHandle() == ahandle)
	{
	    vx_req_account_channel_folder_update_t* reqStruct;
	    vx_req_account_channel_folder_update_create(&reqStruct);

        reqStruct->account_handle = vx_strdup(ahandle.c_str());
	    reqStruct->folder_id = folderID;
	    reqStruct->folder_name = vx_strdup(folder_name.c_str());
	    reqStruct->folder_desc = vx_strdup(folder_desc.c_str());

	    std::string newReqID = stateMgr->GenerateID();
	    reqStruct->base.cookie = vx_strdup(newReqID.c_str());
	    stateMgr->InsertCommandReqID(newReqID, "vx_req_account_channel_folder_update_t");
	    ret = vx_issue_request((vx_req_base_t*)reqStruct);
    }
    else
	{
		cout << "Account handle " << ahandle << " not found." << endl;
	}
	return ret;
}

int RequestManager::req_DeleteFolder(std::string ahandle, int folderID)
{
    int ret = -1;
    if (stateMgr->GetIsAccountLoggedIn() && stateMgr->GetAccountHandle() == ahandle)
	{
	    vx_req_account_channel_folder_delete_t* reqStruct;
	    vx_req_account_channel_folder_delete_create(&reqStruct);

        reqStruct->account_handle = vx_strdup(ahandle.c_str());
	    reqStruct->folder_id = folderID;

	    std::string newReqID = stateMgr->GenerateID();
	    reqStruct->base.cookie = vx_strdup(newReqID.c_str());
	    stateMgr->InsertCommandReqID(newReqID, "vx_req_account_channel_folder_delete_t");
	    ret = vx_issue_request((vx_req_base_t*)reqStruct);
    }
    else
	{
		cout << "Account handle " << ahandle << " not found." << endl;
	}
	return ret;
}

int RequestManager::req_GetFolderInfo(std::string ahandle, int folderID)
{
    int ret = -1;
    if (stateMgr->GetIsAccountLoggedIn() && stateMgr->GetAccountHandle() == ahandle)
	{
	    vx_req_account_channel_folder_get_info_t* reqStruct;
	    vx_req_account_channel_folder_get_info_create(&reqStruct);

        reqStruct->account_handle = vx_strdup(ahandle.c_str());
	    reqStruct->folder_id = folderID;

	    std::string newReqID = stateMgr->GenerateID();
	    reqStruct->base.cookie = vx_strdup(newReqID.c_str());
	    stateMgr->InsertCommandReqID(newReqID, "vx_req_account_channel_folder_get_info_t");
	    ret = vx_issue_request((vx_req_base_t*)reqStruct);
    }
    else
	{
		cout << "Account handle " << ahandle << " not found." << endl;
	}
	return ret;
}

int RequestManager::req_GetFavs(std::string ahandle)
{
    int ret = -1;
    if (stateMgr->GetIsAccountLoggedIn() && stateMgr->GetAccountHandle() == ahandle)
	{
	    vx_req_account_channel_favorites_get_list_t* reqStruct;
	    vx_req_account_channel_favorites_get_list_create(&reqStruct);

        reqStruct->account_handle = vx_strdup(ahandle.c_str());

	    std::string newReqID = stateMgr->GenerateID();
	    reqStruct->base.cookie = vx_strdup(newReqID.c_str());
	    stateMgr->InsertCommandReqID(newReqID, "vx_req_account_channel_favorites_get_list_t");
	    ret = vx_issue_request((vx_req_base_t*)reqStruct);
    }
    else
	{
		cout << "Account handle " << ahandle << " not found." << endl;
	}
	return ret;
}

int RequestManager::req_SetFav(std::string ahandle, int favID, std::string chanUri, std::string chanData, int groupID, std::string label)
{
    int ret = -1;
    if (stateMgr->GetIsAccountLoggedIn() && stateMgr->GetAccountHandle() == ahandle)
	{
	    vx_req_account_channel_favorite_set_t* reqStruct;
	    vx_req_account_channel_favorite_set_create(&reqStruct);

        reqStruct->account_handle = vx_strdup(ahandle.c_str());
        reqStruct->channel_favorite_id = favID;
        if (chanUri.length() > 0)
            reqStruct->channel_favorite_uri = vx_strdup(chanUri.c_str());
        reqStruct->channel_favorite_group_id = groupID;
        if (chanData.length() > 0)
            reqStruct->channel_favorite_data = vx_strdup(chanData.c_str());
        if (label.length() > 0)
            reqStruct->channel_favorite_label = vx_strdup(label.c_str());

	    std::string newReqID = stateMgr->GenerateID();
	    reqStruct->base.cookie = vx_strdup(newReqID.c_str());
	    stateMgr->InsertCommandReqID(newReqID, "vx_req_account_channel_favorite_set_t");
	    ret = vx_issue_request((vx_req_base_t*)reqStruct);
    }
    else
	{
		cout << "Account handle " << ahandle << " not found." << endl;
	}
	return ret;
}

int RequestManager::req_DeleteFav(std::string ahandle, int favID)
{
    int ret = -1;
    if (stateMgr->GetIsAccountLoggedIn() && stateMgr->GetAccountHandle() == ahandle)
	{
	    vx_req_account_channel_favorite_delete_t* reqStruct;
	    vx_req_account_channel_favorite_delete_create(&reqStruct);

        reqStruct->account_handle = vx_strdup(ahandle.c_str());
        reqStruct->channel_favorite_id = favID;

	    std::string newReqID = stateMgr->GenerateID();
	    reqStruct->base.cookie = vx_strdup(newReqID.c_str());
	    stateMgr->InsertCommandReqID(newReqID, "vx_req_account_channel_favorite_delete_t");
	    ret = vx_issue_request((vx_req_base_t*)reqStruct);
    }
    else
	{
		cout << "Account handle " << ahandle << " not found." << endl;
	}
	return ret;
}

int RequestManager::req_SetFavGroup(std::string ahandle, int groupID, std::string name, std::string data)
{
    int ret = -1;
    if (stateMgr->GetIsAccountLoggedIn() && stateMgr->GetAccountHandle() == ahandle)
	{
	    vx_req_account_channel_favorite_group_set_t* reqStruct;
	    vx_req_account_channel_favorite_group_set_create(&reqStruct);

        reqStruct->account_handle = vx_strdup(ahandle.c_str());
        reqStruct->channel_favorite_group_id = groupID;
        reqStruct->channel_favorite_group_name = vx_strdup(name.c_str());
        reqStruct->channel_favorite_group_data = vx_strdup(data.c_str());

	    std::string newReqID = stateMgr->GenerateID();
	    reqStruct->base.cookie = vx_strdup(newReqID.c_str());
	    stateMgr->InsertCommandReqID(newReqID, "vx_req_account_channel_favorite_group_set_t");
	    ret = vx_issue_request((vx_req_base_t*)reqStruct);
    }
    else
	{
		cout << "Account handle " << ahandle << " not found." << endl;
	}
	return ret;
}

int RequestManager::req_DeleteFavGroup(std::string ahandle, int groupID)
{
    int ret = -1;
    if (stateMgr->GetIsAccountLoggedIn() && stateMgr->GetAccountHandle() == ahandle)
	{
	    vx_req_account_channel_favorite_group_delete_t* reqStruct;
	    vx_req_account_channel_favorite_group_delete_create(&reqStruct);

        reqStruct->account_handle = vx_strdup(ahandle.c_str());
        reqStruct->channel_favorite_group_id = groupID;

	    std::string newReqID = stateMgr->GenerateID();
	    reqStruct->base.cookie = vx_strdup(newReqID.c_str());
	    stateMgr->InsertCommandReqID(newReqID, "vx_req_account_channel_favorite_group_delete_t");
	    ret = vx_issue_request((vx_req_base_t*)reqStruct);
    }
    else
	{
		cout << "Account handle " << ahandle << " not found." << endl;
	}
	return ret;
}

int RequestManager::req_GetChannelInfo(std::string ahandle, std::string chanUri)
{
    int ret = -1;
    if (stateMgr->GetIsAccountLoggedIn() && stateMgr->GetAccountHandle() == ahandle)
	{
	    vx_req_account_channel_get_info_t* reqStruct;
	    vx_req_account_channel_get_info_create(&reqStruct);

        reqStruct->account_handle = vx_strdup(ahandle.c_str());
	    reqStruct->channel_uri = vx_strdup(chanUri.c_str());

	    std::string newReqID = stateMgr->GenerateID();
	    reqStruct->base.cookie = vx_strdup(newReqID.c_str());
	    stateMgr->InsertCommandReqID(newReqID, "vx_req_account_channel_get_info_t");
	    ret = vx_issue_request((vx_req_base_t*)reqStruct);
    }
    else
	{
		cout << "Account handle " << ahandle << " not found." << endl;
	}
	return ret;
}

int RequestManager::req_SearchChannels(std::string ahandle, int pgNum, int pgSize, std::string name, std::string desc, int active)
{
    int ret = -1;
    if (stateMgr->GetIsAccountLoggedIn() && stateMgr->GetAccountHandle() == ahandle)
	{
	    vx_req_account_channel_search_t* reqStruct;
	    vx_req_account_channel_search_create(&reqStruct);

        reqStruct->account_handle = vx_strdup(ahandle.c_str());
        reqStruct->page_number = pgNum;
        reqStruct->page_size = pgSize;
        if (name != "''")
	        reqStruct->channel_name = vx_strdup(name.c_str());
        if (desc != "''")
            reqStruct->channel_description = vx_strdup(desc.c_str());
        if (active == 1)
            reqStruct->channel_active = 1;

	    std::string newReqID = stateMgr->GenerateID();
	    reqStruct->base.cookie = vx_strdup(newReqID.c_str());
	    stateMgr->InsertCommandReqID(newReqID, "vx_req_account_channel_search_t");
	    ret = vx_issue_request((vx_req_base_t*)reqStruct);
    }
    else
	{
		cout << "Account handle " << ahandle << " not found." << endl;
	}
	return ret;
}

int RequestManager::req_SearchAccounts(std::string ahandle, int pgNum, int pgSize, std::string first, std::string last, std::string user, std::string email)
{
    int ret = -1;
    if (stateMgr->GetIsAccountLoggedIn() && stateMgr->GetAccountHandle() == ahandle)
	{
	    vx_req_account_buddy_search_t* reqStruct;
	    vx_req_account_buddy_search_create(&reqStruct);

        reqStruct->account_handle = vx_strdup(ahandle.c_str());
        reqStruct->page_number = pgNum;
        reqStruct->page_size = pgSize;
        if (first != "''")
	        reqStruct->buddy_first_name = vx_strdup(first.c_str());
        if (last != "''")
            reqStruct->buddy_last_name = vx_strdup(last.c_str());
        if (user != "''")
            reqStruct->buddy_user_name = vx_strdup(user.c_str());
        if (email != "''")
            reqStruct->buddy_email = vx_strdup(email.c_str());

	    std::string newReqID = stateMgr->GenerateID();
	    reqStruct->base.cookie = vx_strdup(newReqID.c_str());
	    stateMgr->InsertCommandReqID(newReqID, "vx_req_account_buddy_search_t");
	    ret = vx_issue_request((vx_req_base_t*)reqStruct);
    }
    else
	{
		cout << "Account handle " << ahandle << " not found." << endl;
	}
	return ret;
}

int RequestManager::req_ModeratorAdd(std::string ahandle, std::string chan_uri, std::string mod_uri)
{
    int ret = -1;
    if (stateMgr->GetIsAccountLoggedIn() && stateMgr->GetAccountHandle() == ahandle)
	{
	    vx_req_account_channel_add_moderator_t* reqStruct;
	    vx_req_account_channel_add_moderator_create(&reqStruct);

        reqStruct->account_handle = vx_strdup(ahandle.c_str());
	    reqStruct->channel_uri = vx_strdup(chan_uri.c_str());
	    reqStruct->moderator_uri = vx_strdup(mod_uri.c_str());

	    std::string newReqID = stateMgr->GenerateID();
	    reqStruct->base.cookie = vx_strdup(newReqID.c_str());
	    stateMgr->InsertCommandReqID(newReqID, "vx_req_account_channel_add_moderator_t");
	    ret = vx_issue_request((vx_req_base_t*)reqStruct);
    }
    else
	{
		cout << "Account handle " << ahandle << " not found." << endl;
	}
	return ret;
}

int RequestManager::req_ModeratorRemove(std::string ahandle, std::string chan_uri, std::string mod_uri)
{
    int ret = -1;
    if (stateMgr->GetIsAccountLoggedIn() && stateMgr->GetAccountHandle() == ahandle)
	{
	    vx_req_account_channel_remove_moderator_t* reqStruct;
	    vx_req_account_channel_remove_moderator_create(&reqStruct);

        reqStruct->account_handle = vx_strdup(ahandle.c_str());
	    reqStruct->channel_uri = vx_strdup(chan_uri.c_str());
	    reqStruct->moderator_uri = vx_strdup(mod_uri.c_str());

	    std::string newReqID = stateMgr->GenerateID();
	    reqStruct->base.cookie = vx_strdup(newReqID.c_str());
	    stateMgr->InsertCommandReqID(newReqID, "vx_req_account_channel_remove_moderator_t");
	    ret = vx_issue_request((vx_req_base_t*)reqStruct);
    }
    else
	{
		cout << "Account handle " << ahandle << " not found." << endl;
	}
	return ret;
}

int RequestManager::req_ModeratorGet(std::string ahandle, std::string chan_uri)
{
    int ret = -1;
    if (stateMgr->GetIsAccountLoggedIn() && stateMgr->GetAccountHandle() == ahandle)
	{
	    vx_req_account_channel_get_moderators_t* reqStruct;
	    vx_req_account_channel_get_moderators_create(&reqStruct);

        reqStruct->account_handle = vx_strdup(ahandle.c_str());
	    reqStruct->channel_uri = vx_strdup(chan_uri.c_str());

	    std::string newReqID = stateMgr->GenerateID();
	    reqStruct->base.cookie = vx_strdup(newReqID.c_str());
	    stateMgr->InsertCommandReqID(newReqID, "vx_req_account_channel_get_moderators_t");
	    ret = vx_issue_request((vx_req_base_t*)reqStruct);
    }
    else
	{
		cout << "Account handle " << ahandle << " not found." << endl;
	}
	return ret;
}

int RequestManager::req_ACLGet(std::string ahandle, std::string chan_uri)
{
    int ret = -1;
    if (stateMgr->GetIsAccountLoggedIn() && stateMgr->GetAccountHandle() == ahandle)
	{
	    vx_req_account_channel_get_acl_t* reqStruct;
	    vx_req_account_channel_get_acl_create(&reqStruct);

        reqStruct->account_handle = vx_strdup(ahandle.c_str());
	    reqStruct->channel_uri = vx_strdup(chan_uri.c_str());

	    std::string newReqID = stateMgr->GenerateID();
	    reqStruct->base.cookie = vx_strdup(newReqID.c_str());
	    stateMgr->InsertCommandReqID(newReqID, "vx_req_account_channel_get_acl_t");
	    ret = vx_issue_request((vx_req_base_t*)reqStruct);
    }
    else
	{
		cout << "Account handle " << ahandle << " not found." << endl;
	}
	return ret;
}

int RequestManager::req_ACLAdd(std::string ahandle, std::string chan_uri, std::string acl_uri)
{
    int ret = -1;
    if (stateMgr->GetIsAccountLoggedIn() && stateMgr->GetAccountHandle() == ahandle)
	{
	    vx_req_account_channel_add_acl_t* reqStruct;
	    vx_req_account_channel_add_acl_create(&reqStruct);

        reqStruct->account_handle = vx_strdup(ahandle.c_str());
	    reqStruct->channel_uri = vx_strdup(chan_uri.c_str());
	    reqStruct->acl_uri = vx_strdup(acl_uri.c_str());

	    std::string newReqID = stateMgr->GenerateID();
	    reqStruct->base.cookie = vx_strdup(newReqID.c_str());
	    stateMgr->InsertCommandReqID(newReqID, "vx_req_account_channel_add_acl_t");
	    ret = vx_issue_request((vx_req_base_t*)reqStruct);
    }
    else
	{
		cout << "Account handle " << ahandle << " not found." << endl;
	}
	return ret;
}

int RequestManager::req_ACLRemove(std::string ahandle, std::string chan_uri, std::string acl_uri)
{
    int ret = -1;
    if (stateMgr->GetIsAccountLoggedIn() && stateMgr->GetAccountHandle() == ahandle)
	{
	    vx_req_account_channel_remove_acl_t* reqStruct;
	    vx_req_account_channel_remove_acl_create(&reqStruct);

        reqStruct->account_handle = vx_strdup(ahandle.c_str());
	    reqStruct->channel_uri = vx_strdup(chan_uri.c_str());
	    reqStruct->acl_uri = vx_strdup(acl_uri.c_str());

	    std::string newReqID = stateMgr->GenerateID();
	    reqStruct->base.cookie = vx_strdup(newReqID.c_str());
	    stateMgr->InsertCommandReqID(newReqID, "vx_req_account_channel_remove_acl_t");
	    ret = vx_issue_request((vx_req_base_t*)reqStruct);
    }
    else
	{
		cout << "Account handle " << ahandle << " not found." << endl;
	}
	return ret;
}

int RequestManager::req_MuteUser(std::string ahandle, int setMute, std::string chan_uri, std::string user_uri)
{
    int ret = -1;
    if (stateMgr->GetIsAccountLoggedIn() && stateMgr->GetAccountHandle() == ahandle)
	{
	    vx_req_channel_mute_user_t* reqStruct;
	    vx_req_channel_mute_user_create(&reqStruct);

        reqStruct->account_handle = vx_strdup(ahandle.c_str());
	    reqStruct->set_muted = setMute;
	    reqStruct->channel_uri = vx_strdup(chan_uri.c_str());
	    reqStruct->participant_uri = vx_strdup(user_uri.c_str());

	    std::string newReqID = stateMgr->GenerateID();
	    reqStruct->base.cookie = vx_strdup(newReqID.c_str());
	    stateMgr->InsertCommandReqID(newReqID, "vx_req_channel_mute_user_t");
	    ret = vx_issue_request((vx_req_base_t*)reqStruct);
    }
    else
	{
		cout << "Account handle " << ahandle << " not found." << endl;
	}
	return ret;
}

int RequestManager::req_MuteAllUsers(std::string ahandle, int setMuteAll, std::string chan_uri)
{
    int ret = -1;
    if (stateMgr->GetIsAccountLoggedIn() && stateMgr->GetAccountHandle() == ahandle)
	{
	    vx_req_channel_mute_all_users_t* reqStruct;
	    vx_req_channel_mute_all_users_create(&reqStruct);

        reqStruct->account_handle = vx_strdup(ahandle.c_str());
	    reqStruct->set_muted = setMuteAll;
	    reqStruct->channel_uri = vx_strdup(chan_uri.c_str());

	    std::string newReqID = stateMgr->GenerateID();
	    reqStruct->base.cookie = vx_strdup(newReqID.c_str());
	    stateMgr->InsertCommandReqID(newReqID, "vx_req_channel_mute_all_users_t");
	    ret = vx_issue_request((vx_req_base_t*)reqStruct);
    }
    else
	{
		cout << "Account handle " << ahandle << " not found." << endl;
	}
	return ret;
}

int RequestManager::req_BanUser(std::string ahandle, int setBan, std::string chan_uri, std::string user_uri)
{
    int ret = -1;
    if (stateMgr->GetIsAccountLoggedIn() && stateMgr->GetAccountHandle() == ahandle)
	{
	    vx_req_channel_ban_user_t* reqStruct;
	    vx_req_channel_ban_user_create(&reqStruct);

        reqStruct->account_handle = vx_strdup(ahandle.c_str());
	    reqStruct->set_banned = setBan;
	    reqStruct->channel_uri = vx_strdup(chan_uri.c_str());
	    reqStruct->participant_uri = vx_strdup(user_uri.c_str());

	    std::string newReqID = stateMgr->GenerateID();
	    reqStruct->base.cookie = vx_strdup(newReqID.c_str());
        stateMgr->InsertCommandReqID(newReqID, "vx_req_channel_ban_user_t");
	    ret = vx_issue_request((vx_req_base_t*)reqStruct);
    }
    else
	{
		cout << "Account handle " << ahandle << " not found." << endl;
	}
	return ret;
}

int RequestManager::req_GetBannedUsers(std::string ahandle, std::string chan_uri)
{
    int ret = -1;
    if (stateMgr->GetIsAccountLoggedIn() && stateMgr->GetAccountHandle() == ahandle)
    {
        vx_req_channel_get_banned_users_t* reqStruct;
	    vx_req_channel_get_banned_users_create(&reqStruct);

        reqStruct->account_handle = vx_strdup(ahandle.c_str());
	    reqStruct->channel_uri = vx_strdup(chan_uri.c_str());

	    std::string newReqID = stateMgr->GenerateID();
	    reqStruct->base.cookie = vx_strdup(newReqID.c_str());
        stateMgr->InsertCommandReqID(newReqID, "vx_req_channel_get_banned_users_t");
	    ret = vx_issue_request((vx_req_base_t*)reqStruct);
    }
    else
    {
        cout << "Account handle " << ahandle << " not found." << endl;
    }
    return ret;
}

int RequestManager::req_KickUser(std::string ahandle, std::string chan_uri, std::string user_uri)
{
    int ret = -1;
    if (stateMgr->GetIsAccountLoggedIn() && stateMgr->GetAccountHandle() == ahandle)
	{
	    vx_req_channel_kick_user_t* reqStruct;
	    vx_req_channel_kick_user_create(&reqStruct);

        reqStruct->account_handle = vx_strdup(ahandle.c_str());
	    reqStruct->channel_uri = vx_strdup(chan_uri.c_str());
	    reqStruct->participant_uri = vx_strdup(user_uri.c_str());

	    std::string newReqID = stateMgr->GenerateID();
	    reqStruct->base.cookie = vx_strdup(newReqID.c_str());
        stateMgr->InsertCommandReqID(newReqID, "vx_req_channel_kick_user_t");
	    ret = vx_issue_request((vx_req_base_t*)reqStruct);
    }
    else
	{
		cout << "Account handle " << ahandle << " not found." << endl;
	}
	return ret;
}

int RequestManager::req_InviteUser(std::string shandle, std::string user_uri)
{
    int ret = -1;
    int state = stateMgr->GetMediaSessionState(shandle);
    if (state < 0)
    {
        cout << "Session handle " << shandle << " not found." << endl;
        return ret;
    }
    if (state != session_media_connected)
	{
		cout << shandle << " is not connected to audio." << endl;
        return ret;
	}

	vx_req_session_channel_invite_user_t* reqStruct;
	vx_req_session_channel_invite_user_create(&reqStruct);

	reqStruct->session_handle = vx_strdup(shandle.c_str());
	reqStruct->participant_uri = vx_strdup(user_uri.c_str());

	std::string newReqID = stateMgr->GenerateID();
	reqStruct->base.cookie = vx_strdup(newReqID.c_str());
    stateMgr->InsertCommandReqID(newReqID, "vx_req_session_channel_invite_user_t");
	ret = vx_issue_request((vx_req_base_t*)reqStruct);
	return ret;
}

int RequestManager::req_LocalUserMute(std::string shandle, int mute, std::string user_uri)
{
    int ret = -1;
    int state = stateMgr->GetMediaSessionState(shandle);
    if (state < 0)
    {
        cout << "Session handle " << shandle << " not found." << endl;
        return ret;
    }
    if (state != session_media_connected)
	{
		cout << shandle << " is not connected to audio." << endl;
        return ret;
	}

    vx_req_session_set_participant_mute_for_me_t* reqStruct;
    vx_req_session_set_participant_mute_for_me_create(&reqStruct);

    reqStruct->session_handle = vx_strdup(shandle.c_str());
    reqStruct->participant_uri = vx_strdup(user_uri.c_str());
    reqStruct->mute = mute;

    std::string newReqID = stateMgr->GenerateID();
    reqStruct->base.cookie = vx_strdup(newReqID.c_str());
    stateMgr->InsertCommandReqID(newReqID, "vx_req_session_set_participant_mute_for_me_t");
    ret = vx_issue_request((vx_req_base_t*)reqStruct);
	return ret;
}

int RequestManager::req_LocalUserVolume(std::string shandle, int vol, std::string user_uri)
{
    int ret = -1;
    int state = stateMgr->GetMediaSessionState(shandle);
    if (state < 0)
    {
        cout << "Session handle " << shandle << " not found." << endl;
        return ret;
    }
    if (state != session_media_connected)
	{
		cout << shandle << " is not connected to audio." << endl;
        return ret;
	}

	vx_req_session_set_participant_volume_for_me_t* reqStruct;
	vx_req_session_set_participant_volume_for_me_create(&reqStruct);

	reqStruct->session_handle = vx_strdup(shandle.c_str());
	reqStruct->participant_uri = vx_strdup(user_uri.c_str());
    reqStruct->volume = vol;

	std::string newReqID = stateMgr->GenerateID();
	reqStruct->base.cookie = vx_strdup(newReqID.c_str());
	stateMgr->InsertCommandReqID(newReqID, "vx_req_session_set_participant_volume_for_me_t");
	ret = vx_issue_request((vx_req_base_t*)reqStruct);
	return ret;
}

int RequestManager::req_GetChannels(std::string ahandle)
{
    int ret = -1;
    if (stateMgr->GetIsAccountLoggedIn() && stateMgr->GetAccountHandle() == ahandle)
	{
	    vx_req_account_channel_get_list_t* reqStruct;
	    vx_req_account_channel_get_list_create(&reqStruct);

        reqStruct->account_handle = vx_strdup(ahandle.c_str());
	    std::string newReqID = stateMgr->GenerateID();
	    reqStruct->base.cookie = vx_strdup(newReqID.c_str());
	    stateMgr->InsertCommandReqID(newReqID, "vx_req_account_channel_get_list_t");
	    ret = vx_issue_request((vx_req_base_t*)reqStruct);
    }
    else
	{
		cout << "Account handle " << ahandle << " not found." << endl;
	}
	return ret;
}

int RequestManager::req_GetParts(std::string shandle)
{
    int ret = -1;
    int state = stateMgr->GetMediaSessionState(shandle);
    if (state < 0)
    {
        cout << "Session handle " << shandle << " not found." << endl;
        return ret;
    }

    vx_req_session_channel_get_participants_t* reqStruct;
    vx_req_session_channel_get_participants_create(&reqStruct);

    reqStruct->session_handle = vx_strdup(shandle.c_str());	

    std::string newReqID = stateMgr->GenerateID();
    reqStruct->base.cookie = vx_strdup(newReqID.c_str());
    stateMgr->InsertCommandReqID(newReqID, "vx_req_session_channel_get_participants_t");
    ret = vx_issue_request((vx_req_base_t*)reqStruct);
	return ret;	
}

int RequestManager::req_AudioInfo(std::string chandle)
{
    int ret = -1;
	if (stateMgr->GetIsConnectorInitialized() && stateMgr->GetConnectorHandle() == chandle)
	{
		vx_req_connector_get_local_audio_info_t* reqStruct;
		vx_req_connector_get_local_audio_info_create(&reqStruct);

        reqStruct->connector_handle = vx_strdup(chandle.c_str());
		std::string newReqID = stateMgr->GenerateID();
		reqStruct->base.cookie = vx_strdup(newReqID.c_str());
		stateMgr->InsertCommandReqID(newReqID, "vx_req_connector_get_local_audio_info_t");
		ret = vx_issue_request((vx_req_base_t*)reqStruct);
	}
	else
	{
		cout << "Connector handle " << chandle << " not found." << endl;
	}
	return ret;
}

int RequestManager::req_ListRenderDevices(void)
{
    int ret = -1;

    vx_req_aux_get_render_devices_t* reqStruct = NULL;
    vx_req_aux_get_render_devices_create(&reqStruct);

    std::string newReqID = stateMgr->GenerateID();
	reqStruct->base.cookie = vx_strdup(newReqID.c_str());
	stateMgr->InsertCommandReqID(newReqID, "vx_req_aux_get_render_devices_t");
	ret = vx_issue_request((vx_req_base_t*)reqStruct);
	return ret;
}

int RequestManager::req_SetRenderDevice(std::string renderDeviceString)
{
    int ret = -1;

    vx_req_aux_set_render_device_t* reqStruct = NULL;
    vx_req_aux_set_render_device_create(&reqStruct);

    std::string newReqID = stateMgr->GenerateID();
	reqStruct->base.cookie = vx_strdup(newReqID.c_str());
	stateMgr->InsertCommandReqID(newReqID, "vx_req_aux_set_render_device_t");
    reqStruct->render_device_specifier = vx_strdup(renderDeviceString.c_str());
	ret = vx_issue_request((vx_req_base_t*)reqStruct);
	return ret;
}

int RequestManager::req_SetDefaultRenderDevice(void)
{
    int ret = -1;

    vx_req_aux_set_render_device_t* reqStruct = NULL;
    vx_req_aux_set_render_device_create(&reqStruct);

    std::string newReqID = stateMgr->GenerateID();
	reqStruct->base.cookie = vx_strdup(newReqID.c_str());
	stateMgr->InsertCommandReqID(newReqID, "vx_req_aux_set_render_device_t");
    reqStruct->render_device_specifier = NULL; 
	ret = vx_issue_request((vx_req_base_t*)reqStruct);
	return ret;
}

int RequestManager::req_MasterSpeakerSetVol(int vol)
{
    int ret = -1;

    vx_req_aux_set_speaker_level_t* reqStruct = NULL;
    vx_req_aux_set_speaker_level_create(&reqStruct);

    std::string newReqID = stateMgr->GenerateID();
	reqStruct->base.cookie = vx_strdup(newReqID.c_str());
	stateMgr->InsertCommandReqID(newReqID, "vx_req_aux_set_speaker_level_t");
    reqStruct->level = vol;
	ret = vx_issue_request((vx_req_base_t*)reqStruct);
	return ret;
}

int RequestManager::req_MasterSpeakerGetVol(void)
{
    int ret = -1;

    vx_req_aux_get_speaker_level_t* reqStruct = NULL;
    vx_req_aux_get_speaker_level_create(&reqStruct);

    std::string newReqID = stateMgr->GenerateID();
	reqStruct->base.cookie = vx_strdup(newReqID.c_str());
	stateMgr->InsertCommandReqID(newReqID, "vx_req_aux_get_speaker_level_t");
	ret = vx_issue_request((vx_req_base_t*)reqStruct);
	return ret;
}

int RequestManager::req_ListCaptureDevices(void)
{
    int ret = -1;

    vx_req_aux_get_capture_devices_t* reqStruct = NULL;
    vx_req_aux_get_capture_devices_create(&reqStruct);

    std::string newReqID = stateMgr->GenerateID();
	reqStruct->base.cookie = vx_strdup(newReqID.c_str());
	stateMgr->InsertCommandReqID(newReqID, "vx_req_aux_get_capture_devices_t");
	ret = vx_issue_request((vx_req_base_t*)reqStruct);
	return ret;
}

int RequestManager::req_SetCaptureDevice(std::string captureDeviceString)
{
    int ret = -1;

    vx_req_aux_set_capture_device_t* reqStruct = NULL;
    vx_req_aux_set_capture_device_create(&reqStruct);

    std::string newReqID = stateMgr->GenerateID();
	reqStruct->base.cookie = vx_strdup(newReqID.c_str());
	stateMgr->InsertCommandReqID(newReqID, "vx_req_aux_set_capture_device_t");
    reqStruct->capture_device_specifier = vx_strdup(captureDeviceString.c_str());
	ret = vx_issue_request((vx_req_base_t*)reqStruct);
	return ret;
}

int RequestManager::req_SetDefaultCaptureDevice(void)
{
    int ret = -1;

    vx_req_aux_set_capture_device_t* reqStruct = NULL;
    vx_req_aux_set_capture_device_create(&reqStruct);

    std::string newReqID = stateMgr->GenerateID();
	reqStruct->base.cookie = vx_strdup(newReqID.c_str());
	stateMgr->InsertCommandReqID(newReqID, "vx_req_aux_set_capture_device_t");
    reqStruct->capture_device_specifier = NULL;
	ret = vx_issue_request((vx_req_base_t*)reqStruct);
	return ret;
}

int RequestManager::req_MasterMicSetVol(int vol)
{
    int ret = -1;

    vx_req_aux_set_mic_level_t* reqStruct = NULL;
    vx_req_aux_set_mic_level_create(&reqStruct);

    std::string newReqID = stateMgr->GenerateID();
	reqStruct->base.cookie = vx_strdup(newReqID.c_str());
	stateMgr->InsertCommandReqID(newReqID, "vx_req_aux_set_mic_level_t");
    reqStruct->level = vol;
	ret = vx_issue_request((vx_req_base_t*)reqStruct);
	return ret;
}

int RequestManager::req_MasterMicGetVol(void)
{
    int ret = -1;

    vx_req_aux_get_mic_level_t* reqStruct = NULL;
    vx_req_aux_get_mic_level_create(&reqStruct);

    std::string newReqID = stateMgr->GenerateID();
	reqStruct->base.cookie = vx_strdup(newReqID.c_str());
	stateMgr->InsertCommandReqID(newReqID, "vx_req_aux_get_mic_level_t");
	ret = vx_issue_request((vx_req_base_t*)reqStruct);
	return ret;
}

int RequestManager::req_RenderAudioStart(std::string wavFilePath, int loop)
{
    int ret = -1;

    vx_req_aux_render_audio_start_t* reqStruct = NULL;
    vx_req_aux_render_audio_start_create(&reqStruct);

    std::string newReqID = stateMgr->GenerateID();
	reqStruct->base.cookie = vx_strdup(newReqID.c_str());
	stateMgr->InsertCommandReqID(newReqID, "vx_req_aux_render_audio_start_create");

    reqStruct->sound_file_path = vx_strdup(wavFilePath.c_str());
    reqStruct->loop = loop;

	ret = vx_issue_request((vx_req_base_t*)reqStruct);
	return ret;
}

int RequestManager::req_RenderAudioStop(void)
{
    int ret = -1;

    vx_req_aux_render_audio_stop_t* reqStruct = NULL;
    vx_req_aux_render_audio_stop_create(&reqStruct);

    std::string newReqID = stateMgr->GenerateID();
	reqStruct->base.cookie = vx_strdup(newReqID.c_str());
	stateMgr->InsertCommandReqID(newReqID, "vx_req_aux_render_audio_stop_t");

	ret = vx_issue_request((vx_req_base_t*)reqStruct);
	return ret;
}

int RequestManager::req_CaptureAudioStart(void)
{
    int ret = -1;

    vx_req_aux_capture_audio_start_t* reqStruct = NULL;
    vx_req_aux_capture_audio_start_create(&reqStruct);

    std::string newReqID = stateMgr->GenerateID();
	reqStruct->base.cookie = vx_strdup(newReqID.c_str());
	stateMgr->InsertCommandReqID(newReqID, "vx_req_aux_capture_audio_start_t");

	ret = vx_issue_request((vx_req_base_t*)reqStruct);
	return ret;
}

int RequestManager::req_CaptureAudioStop(void)
{
    int ret = -1;

    vx_req_aux_capture_audio_stop_t* reqStruct = NULL;
    vx_req_aux_capture_audio_stop_create(&reqStruct);

    std::string newReqID = stateMgr->GenerateID();
	reqStruct->base.cookie = vx_strdup(newReqID.c_str());
	stateMgr->InsertCommandReqID(newReqID, "vx_req_aux_capture_audio_stop_t");

	ret = vx_issue_request((vx_req_base_t*)reqStruct);
	return ret;
}

int RequestManager::req_SpeakerVol(std::string chandle, int vol)
{
    int ret = -1;
	if (stateMgr->GetIsConnectorInitialized() && stateMgr->GetConnectorHandle() == chandle)
	{
		vx_req_connector_set_local_speaker_volume_t* reqStruct;
		vx_req_connector_set_local_speaker_volume_create(&reqStruct);

        reqStruct->connector_handle = vx_strdup(chandle.c_str());
		reqStruct->volume = vol;

		std::string newReqID = stateMgr->GenerateID();
		reqStruct->base.cookie = vx_strdup(newReqID.c_str());
	    stateMgr->InsertCommandReqID(newReqID, "vx_req_connector_set_local_speaker_volume_t");
		ret = vx_issue_request((vx_req_base_t*)reqStruct);
	}
	else
	{
		cout << "Connector handle " << chandle << " not found." << endl;
	}
	return ret;
}

int RequestManager::req_SpeakerMute(std::string chandle, int mute)
{
    int ret = -1;
	if (stateMgr->GetIsConnectorInitialized() && stateMgr->GetConnectorHandle() == chandle)
	{
		vx_req_connector_mute_local_speaker_t* reqStruct;
		vx_req_connector_mute_local_speaker_create(&reqStruct);

        reqStruct->connector_handle = vx_strdup(chandle.c_str());
		reqStruct->mute_level = mute;

		std::string newReqID = stateMgr->GenerateID();
		reqStruct->base.cookie = vx_strdup(newReqID.c_str());
		stateMgr->InsertCommandReqID(newReqID, "vx_req_connector_mute_local_speaker_t");
		ret = vx_issue_request((vx_req_base_t*)reqStruct);
	}
	else
	{
		cout << "Connector handle " << chandle << " not found." << endl;
	}
	return ret;
}

int RequestManager::req_MicVol(std::string chandle, int vol)
{
    int ret = -1;
	if (stateMgr->GetIsConnectorInitialized() && stateMgr->GetConnectorHandle() == chandle)
	{
		vx_req_connector_set_local_mic_volume_t* reqStruct;
		vx_req_connector_set_local_mic_volume_create(&reqStruct);

        reqStruct->connector_handle = vx_strdup(chandle.c_str());
		reqStruct->volume = vol;

		std::string newReqID = stateMgr->GenerateID();
		reqStruct->base.cookie = vx_strdup(newReqID.c_str());
		stateMgr->InsertCommandReqID(newReqID, "vx_req_connector_set_local_mic_volume_t");
		ret = vx_issue_request((vx_req_base_t*)reqStruct);
	}
	else
	{
		cout << "Connector handle " << chandle << " not found." << endl;
	}
	return ret;
}

int RequestManager::req_MicMute(std::string chandle, int mute)
{
    int ret = -1;
	if (stateMgr->GetIsConnectorInitialized() && stateMgr->GetConnectorHandle() == chandle)
	{
		vx_req_connector_mute_local_mic_t* reqStruct;
		vx_req_connector_mute_local_mic_create(&reqStruct);

        reqStruct->connector_handle = vx_strdup(chandle.c_str());
		reqStruct->mute_level = mute;

		std::string newReqID = stateMgr->GenerateID();
		reqStruct->base.cookie = vx_strdup(newReqID.c_str());
		stateMgr->InsertCommandReqID(newReqID, "vx_req_connector_mute_local_mic_t");
		ret = vx_issue_request((vx_req_base_t*)reqStruct);
	}
	else
	{
		cout << "Connector handle " << chandle << " not found." << endl;
	}
	return ret;
}

int RequestManager::req_moveToOrigin(std::string shandle)
{
    int ret = -1;
    int state = stateMgr->GetMediaSessionState(shandle);
    if (state < 0)
    {
        cout << "Session handle " << shandle << " not found." << endl;
        return ret;
    }
    if (state != session_media_connected)
	{
		cout << shandle << " is not connected to audio." << endl;
        return ret;
	}

    double listenerOrientation[6];
    double listenerPosition[3];

    listenerPosition[0] = 0.0;
    listenerPosition[1] = 0.0;
    listenerPosition[2] = 0.0;

    stateMgr->GetListenerOrientation(listenerOrientation);

    vx_req_session_set_3d_position* reqStruct;
    vx_req_session_set_3d_position_create(&reqStruct);
    reqStruct->session_handle = vx_strdup(shandle.c_str());

    reqStruct->listener_position[0] = listenerPosition[0];
    reqStruct->listener_position[1] = listenerPosition[1];
    reqStruct->listener_position[2] = listenerPosition[2]; 

    reqStruct->listener_at_orientation[0] = listenerOrientation[0];
    reqStruct->listener_at_orientation[1] = listenerOrientation[1];
    reqStruct->listener_at_orientation[2] = listenerOrientation[2];

    reqStruct->listener_up_orientation[0] = listenerOrientation[3];
    reqStruct->listener_up_orientation[1] = listenerOrientation[4];
    reqStruct->listener_up_orientation[2] = listenerOrientation[5];

    reqStruct->speaker_position[0] = listenerPosition[0];
    reqStruct->speaker_position[1] = listenerPosition[1];
    reqStruct->speaker_position[2] = listenerPosition[2]; 

    reqStruct->speaker_at_orientation[0] = listenerOrientation[0];
    reqStruct->speaker_at_orientation[1] = listenerOrientation[1];
    reqStruct->speaker_at_orientation[2] = listenerOrientation[2];

    reqStruct->speaker_up_orientation[0] = listenerOrientation[3];
    reqStruct->speaker_up_orientation[1] = listenerOrientation[4];
    reqStruct->speaker_up_orientation[2] = listenerOrientation[5];

    stateMgr->SetListenerPosition(reqStruct->listener_position);

	std::string newReqID = stateMgr->GenerateID();
	reqStruct->base.cookie = vx_strdup(newReqID.c_str());
	stateMgr->InsertCommandReqID(newReqID, "vx_req_session_set_3d_position");
	ret = vx_issue_request((vx_req_base_t*)reqStruct);
	return ret;
}

int RequestManager::req_moveLeft(std::string shandle, double delta)
{
    int ret = -1;
    int state = stateMgr->GetMediaSessionState(shandle);
    if (state < 0)
    {
        cout << "Session handle " << shandle << " not found." << endl;
        return ret;
    }
    if (state != session_media_connected)
	{
		cout << shandle << " is not connected to audio." << endl;
        return ret;
	}

    double listenerPosition[3];
    double listenerOrientation[6];
    double delta_x = 0.0;
    double delta_z = 0.0;

    stateMgr->GetListenerPosition(listenerPosition);
    stateMgr->GetListenerOrientation(listenerOrientation);

    vx_req_session_set_3d_position* reqStruct;
    vx_req_session_set_3d_position_create(&reqStruct);
    reqStruct->session_handle = vx_strdup(shandle.c_str());

    delta_x = delta * listenerOrientation[2];
    delta_z = -1.0 * delta * listenerOrientation[0];

    listenerPosition[0] += delta_x;

    listenerPosition[2] += delta_z;

    reqStruct->listener_position[0] = listenerPosition[0];
    reqStruct->listener_position[1] = listenerPosition[1];
    reqStruct->listener_position[2] = listenerPosition[2];

    reqStruct->listener_at_orientation[0] = listenerOrientation[0];
    reqStruct->listener_at_orientation[1] = listenerOrientation[1];
    reqStruct->listener_at_orientation[2] = listenerOrientation[2];

    reqStruct->listener_up_orientation[0] = listenerOrientation[3];
    reqStruct->listener_up_orientation[1] = listenerOrientation[4];
    reqStruct->listener_up_orientation[2] = listenerOrientation[5];

    reqStruct->speaker_position[0] = listenerPosition[0];
    reqStruct->speaker_position[1] = listenerPosition[1];
    reqStruct->speaker_position[2] = listenerPosition[2]; 

    reqStruct->speaker_at_orientation[0] = listenerOrientation[0];
    reqStruct->speaker_at_orientation[1] = listenerOrientation[1];
    reqStruct->speaker_at_orientation[2] = listenerOrientation[2];

    reqStruct->speaker_up_orientation[0] = listenerOrientation[3];
    reqStruct->speaker_up_orientation[1] = listenerOrientation[4];
    reqStruct->speaker_up_orientation[2] = listenerOrientation[5];



    stateMgr->SetListenerPosition(listenerPosition);

	std::string newReqID = stateMgr->GenerateID();
	reqStruct->base.cookie = vx_strdup(newReqID.c_str());
	stateMgr->InsertCommandReqID(newReqID, "vx_req_session_set_3d_position");
	ret = vx_issue_request((vx_req_base_t*)reqStruct);
	return ret;
}

int RequestManager::req_moveRight(std::string shandle, double delta)
{
    int ret = -1;
    int state = stateMgr->GetMediaSessionState(shandle);
    if (state < 0)
    {
        cout << "Session handle " << shandle << " not found." << endl;
        return ret;
    }
    if (state != session_media_connected)
	{
		cout << shandle << " is not connected to audio." << endl;
        return ret;
	}

    double listenerPosition[3];
    double listenerOrientation[6];
    double delta_x = 0.0;
    double delta_z = 0.0;

    stateMgr->GetListenerPosition(listenerPosition);
    stateMgr->GetListenerOrientation(listenerOrientation);

    vx_req_session_set_3d_position* reqStruct;
    vx_req_session_set_3d_position_create(&reqStruct);
    reqStruct->session_handle = vx_strdup(shandle.c_str());

    delta_x = -1.0 * delta * listenerOrientation[2];
    delta_z = delta * listenerOrientation[0];

    listenerPosition[0] += delta_x;

    listenerPosition[2] += delta_z;

    reqStruct->listener_position[0] = listenerPosition[0];
    reqStruct->listener_position[1] = listenerPosition[1];
    reqStruct->listener_position[2] = listenerPosition[2];

    reqStruct->listener_at_orientation[0] = listenerOrientation[0];
    reqStruct->listener_at_orientation[1] = listenerOrientation[1];
    reqStruct->listener_at_orientation[2] = listenerOrientation[2];

    reqStruct->listener_up_orientation[0] = listenerOrientation[3];
    reqStruct->listener_up_orientation[1] = listenerOrientation[4];
    reqStruct->listener_up_orientation[2] = listenerOrientation[5];

    reqStruct->speaker_position[0] = listenerPosition[0];
    reqStruct->speaker_position[1] = listenerPosition[1];
    reqStruct->speaker_position[2] = listenerPosition[2]; 

    reqStruct->speaker_at_orientation[0] = listenerOrientation[0];
    reqStruct->speaker_at_orientation[1] = listenerOrientation[1];
    reqStruct->speaker_at_orientation[2] = listenerOrientation[2];

    reqStruct->speaker_up_orientation[0] = listenerOrientation[3];
    reqStruct->speaker_up_orientation[1] = listenerOrientation[4];
    reqStruct->speaker_up_orientation[2] = listenerOrientation[5];


    stateMgr->SetListenerPosition(listenerPosition);

	std::string newReqID = stateMgr->GenerateID();
	reqStruct->base.cookie = vx_strdup(newReqID.c_str());
	stateMgr->InsertCommandReqID(newReqID, "vx_req_session_set_3d_position");
	ret = vx_issue_request((vx_req_base_t*)reqStruct);
	return ret;
}

int RequestManager::req_moveForward(std::string shandle, double delta)
{
    int ret = -1;
    int state = stateMgr->GetMediaSessionState(shandle);
    if (state < 0)
    {
        cout << "Session handle " << shandle << " not found." << endl;
        return ret;
    }
    if (state != session_media_connected)
	{
		cout << shandle << " is not connected to audio." << endl;
        return ret;
	}

    double listenerPosition[3];
    double listenerOrientation[6];
    double delta_x = 0.0;
    double delta_z = 0.0;

    stateMgr->GetListenerPosition(listenerPosition);
    stateMgr->GetListenerOrientation(listenerOrientation);

    vx_req_session_set_3d_position* reqStruct;
    vx_req_session_set_3d_position_create(&reqStruct);
    reqStruct->session_handle = vx_strdup(shandle.c_str());

    delta_x = delta * listenerOrientation[0];
    delta_z = delta * listenerOrientation[2];

    listenerPosition[0] += delta_x;

    listenerPosition[2] += delta_z;

    reqStruct->listener_position[0] = listenerPosition[0];
    reqStruct->listener_position[1] = listenerPosition[1];
    reqStruct->listener_position[2] = listenerPosition[2];

    reqStruct->listener_at_orientation[0] = listenerOrientation[0];
    reqStruct->listener_at_orientation[1] = listenerOrientation[1];
    reqStruct->listener_at_orientation[2] = listenerOrientation[2];

    reqStruct->listener_up_orientation[0] = listenerOrientation[3];
    reqStruct->listener_up_orientation[1] = listenerOrientation[4];
    reqStruct->listener_up_orientation[2] = listenerOrientation[5];

    reqStruct->speaker_position[0] = listenerPosition[0];
    reqStruct->speaker_position[1] = listenerPosition[1];
    reqStruct->speaker_position[2] = listenerPosition[2]; 

    reqStruct->speaker_at_orientation[0] = listenerOrientation[0];
    reqStruct->speaker_at_orientation[1] = listenerOrientation[1];
    reqStruct->speaker_at_orientation[2] = listenerOrientation[2];

    reqStruct->speaker_up_orientation[0] = listenerOrientation[3];
    reqStruct->speaker_up_orientation[1] = listenerOrientation[4];
    reqStruct->speaker_up_orientation[2] = listenerOrientation[5];

    stateMgr->SetListenerPosition(listenerPosition);

	std::string newReqID = stateMgr->GenerateID();
	reqStruct->base.cookie = vx_strdup(newReqID.c_str());
	stateMgr->InsertCommandReqID(newReqID, "vx_req_session_set_3d_position");
	ret = vx_issue_request((vx_req_base_t*)reqStruct);
	return ret;
}

int RequestManager::req_moveBack(std::string shandle, double delta)
{
    int ret = -1;
    int state = stateMgr->GetMediaSessionState(shandle);
    if (state < 0)
    {
        cout << "Session handle " << shandle << " not found." << endl;
        return ret;
    }
    if (state != session_media_connected)
	{
		cout << shandle << " is not connected to audio." << endl;
        return ret;
	}

    double listenerPosition[3];
    double listenerOrientation[6];
    double delta_x = 0.0;
    double delta_z = 0.0;

    stateMgr->GetListenerPosition(listenerPosition);
    stateMgr->GetListenerOrientation(listenerOrientation);

    vx_req_session_set_3d_position* reqStruct;
    vx_req_session_set_3d_position_create(&reqStruct);
    reqStruct->session_handle = vx_strdup(shandle.c_str());

    delta_x = -1.0 * delta * listenerOrientation[0];
    delta_z = -1.0 * delta * listenerOrientation[2];

    listenerPosition[0] += delta_x;

    listenerPosition[2] += delta_z;

    reqStruct->listener_position[0] = listenerPosition[0];
    reqStruct->listener_position[1] = listenerPosition[1];
    reqStruct->listener_position[2] = listenerPosition[2];

    reqStruct->listener_at_orientation[0] = listenerOrientation[0];
    reqStruct->listener_at_orientation[1] = listenerOrientation[1];
    reqStruct->listener_at_orientation[2] = listenerOrientation[2];

    reqStruct->listener_up_orientation[0] = listenerOrientation[3];
    reqStruct->listener_up_orientation[1] = listenerOrientation[4];
    reqStruct->listener_up_orientation[2] = listenerOrientation[5];

    reqStruct->speaker_position[0] = listenerPosition[0];
    reqStruct->speaker_position[1] = listenerPosition[1];
    reqStruct->speaker_position[2] = listenerPosition[2]; 

    reqStruct->speaker_at_orientation[0] = listenerOrientation[0];
    reqStruct->speaker_at_orientation[1] = listenerOrientation[1];
    reqStruct->speaker_at_orientation[2] = listenerOrientation[2];

    reqStruct->speaker_up_orientation[0] = listenerOrientation[3];
    reqStruct->speaker_up_orientation[1] = listenerOrientation[4];
    reqStruct->speaker_up_orientation[2] = listenerOrientation[5];

    stateMgr->SetListenerPosition(listenerPosition);

	std::string newReqID = stateMgr->GenerateID();
	reqStruct->base.cookie = vx_strdup(newReqID.c_str());
	stateMgr->InsertCommandReqID(newReqID, "vx_req_session_set_3d_position");
	ret = vx_issue_request((vx_req_base_t*)reqStruct);
	return ret;
}

int RequestManager::req_turnLeft45(std::string shandle)
{
    int ret = -1;
    int state = stateMgr->GetMediaSessionState(shandle);
    if (state < 0)
    {
        cout << "Session handle " << shandle << " not found." << endl;
        return ret;
    }
    if (state != session_media_connected)
	{
		cout << shandle << " is not connected to audio." << endl;
        return ret;
	}

    double listenerHeadingDegrees;
    double listenerPosition[3];
    double listenerOrientation[6];

    stateMgr->GetListenerHeadingDegrees(&listenerHeadingDegrees);
    stateMgr->GetListenerPosition(listenerPosition);
    stateMgr->GetListenerOrientation(listenerOrientation);

    vx_req_session_set_3d_position_t* reqStruct;
    vx_req_session_set_3d_position_create(&reqStruct);
    reqStruct->session_handle = vx_strdup(shandle.c_str());

    listenerHeadingDegrees -= 45.0;
    if (listenerHeadingDegrees < 0.0) {
        listenerHeadingDegrees += 360.0;
    }

    listenerOrientation[0] = 1.0 * sin(2 * PI *(listenerHeadingDegrees/360.0));
    listenerOrientation[2] = -1.0 * cos(2 * PI * (listenerHeadingDegrees/360.0));

    reqStruct->listener_position[0] = listenerPosition[0];
    reqStruct->listener_position[1] = listenerPosition[1];
    reqStruct->listener_position[2] = listenerPosition[2];

    reqStruct->listener_at_orientation[0] = listenerOrientation[0];
    reqStruct->listener_at_orientation[1] = listenerOrientation[1];
    reqStruct->listener_at_orientation[2] = listenerOrientation[2];

    reqStruct->listener_up_orientation[0] = listenerOrientation[3];
    reqStruct->listener_up_orientation[1] = listenerOrientation[4];
    reqStruct->listener_up_orientation[2] = listenerOrientation[5];

    reqStruct->speaker_position[0] = listenerPosition[0];
    reqStruct->speaker_position[1] = listenerPosition[1];
    reqStruct->speaker_position[2] = listenerPosition[2]; 

    reqStruct->speaker_at_orientation[0] = listenerOrientation[0];
    reqStruct->speaker_at_orientation[1] = listenerOrientation[1];
    reqStruct->speaker_at_orientation[2] = listenerOrientation[2];

    reqStruct->speaker_up_orientation[0] = listenerOrientation[3];
    reqStruct->speaker_up_orientation[1] = listenerOrientation[4];
    reqStruct->speaker_up_orientation[2] = listenerOrientation[5];


    stateMgr->SetListenerHeadingDegrees(listenerHeadingDegrees);
    stateMgr->SetListenerOrientation(listenerOrientation);

	std::string newReqID = stateMgr->GenerateID();
	reqStruct->base.cookie = vx_strdup(newReqID.c_str());
	stateMgr->InsertCommandReqID(newReqID, "vx_req_session_set_3d_position");
	ret = vx_issue_request((vx_req_base_t*)reqStruct);
	return ret;
}

int RequestManager::req_turnRight45(std::string shandle)
{
    int ret = -1;
    int state = stateMgr->GetMediaSessionState(shandle);
    if (state < 0)
    {
        cout << "Session handle " << shandle << " not found." << endl;
        return ret;
    }
    if (state != session_media_connected)
	{
		cout << shandle << " is not connected to audio." << endl;
        return ret;
	}

    double listenerHeadingDegrees;
    double listenerPosition[3];
    double listenerOrientation[6];

    stateMgr->GetListenerHeadingDegrees(&listenerHeadingDegrees);
    stateMgr->GetListenerPosition(listenerPosition);
    stateMgr->GetListenerOrientation(listenerOrientation);

    vx_req_session_set_3d_position_t* reqStruct;
    vx_req_session_set_3d_position_create(&reqStruct);
    reqStruct->session_handle = vx_strdup(shandle.c_str());

    listenerHeadingDegrees += 45.0;
    if (listenerHeadingDegrees >= 360.0) {
        listenerHeadingDegrees -= 360.0;
    }

    listenerOrientation[0] = 1.0 * sin(2 * PI *(listenerHeadingDegrees/360.0));
    listenerOrientation[2] = -1.0 * cos(2 * PI * (listenerHeadingDegrees/360.0));

    reqStruct->listener_position[0] = listenerPosition[0];
    reqStruct->listener_position[1] = listenerPosition[1];
    reqStruct->listener_position[2] = listenerPosition[2];

    reqStruct->listener_at_orientation[0] = listenerOrientation[0];
    reqStruct->listener_at_orientation[1] = listenerOrientation[1];
    reqStruct->listener_at_orientation[2] = listenerOrientation[2];

    reqStruct->listener_up_orientation[0] = listenerOrientation[3];
    reqStruct->listener_up_orientation[1] = listenerOrientation[4];
    reqStruct->listener_up_orientation[2] = listenerOrientation[5];

    reqStruct->speaker_position[0] = listenerPosition[0];
    reqStruct->speaker_position[1] = listenerPosition[1];
    reqStruct->speaker_position[2] = listenerPosition[2]; 

    reqStruct->speaker_at_orientation[0] = listenerOrientation[0];
    reqStruct->speaker_at_orientation[1] = listenerOrientation[1];
    reqStruct->speaker_at_orientation[2] = listenerOrientation[2];

    reqStruct->speaker_up_orientation[0] = listenerOrientation[3];
    reqStruct->speaker_up_orientation[1] = listenerOrientation[4];
    reqStruct->speaker_up_orientation[2] = listenerOrientation[5];

    stateMgr->SetListenerHeadingDegrees(listenerHeadingDegrees);
    stateMgr->SetListenerOrientation(listenerOrientation);

	std::string newReqID = stateMgr->GenerateID();
	reqStruct->base.cookie = vx_strdup(newReqID.c_str());
	stateMgr->InsertCommandReqID(newReqID, "vx_req_session_set_3d_position");
	ret = vx_issue_request((vx_req_base_t*)reqStruct);
	return ret;
}

int RequestManager::req_BuddySet(const std::string &acct_handle, std::string buddy_uri, std::string buddy_display_name, std::string buddy_data, int group_id)
{
    int ret = -1;
	vx_req_account_buddy_set_t* reqStruct;
	vx_req_account_buddy_set_create(&reqStruct);

	reqStruct->buddy_uri = vx_strdup(buddy_uri.c_str());
	reqStruct->display_name = vx_strdup(buddy_display_name.c_str());
    reqStruct->buddy_data = vx_strdup(buddy_data.c_str());
    reqStruct->account_handle = vx_strdup(acct_handle.c_str());
    reqStruct->group_id = group_id;

	std::string newReqID = stateMgr->GenerateID();
	reqStruct->base.cookie = vx_strdup(newReqID.c_str());
	stateMgr->InsertCommandReqID(newReqID, "vx_req_account_buddy_set_t");
	ret = vx_issue_request((vx_req_base_t*)reqStruct);
	return ret;
}

int RequestManager::req_BuddyDelete(const std::string &acct_handle, std::string buddy_uri)
{
    int ret = -1;
	vx_req_account_buddy_delete_t* reqStruct;
	vx_req_account_buddy_delete_create(&reqStruct);

	reqStruct->buddy_uri = vx_strdup(buddy_uri.c_str());
    reqStruct->account_handle = vx_strdup(acct_handle.c_str());

	std::string newReqID = stateMgr->GenerateID();
	reqStruct->base.cookie = vx_strdup(newReqID.c_str());
	stateMgr->InsertCommandReqID(newReqID, "vx_req_account_buddy_delete_t");
	ret = vx_issue_request((vx_req_base_t*)reqStruct);
	return ret;
}

int RequestManager::req_BuddyGroupSet(std::string ahandle, int group_id, std::string group_name, std::string group_data)
{
    int ret = -1;
    if (stateMgr->GetIsAccountLoggedIn() && stateMgr->GetAccountHandle() == ahandle)
	{
	    vx_req_account_buddygroup_set_t* reqStruct;
	    vx_req_account_buddygroup_set_create(&reqStruct);

        reqStruct->account_handle = vx_strdup(ahandle.c_str());
        reqStruct->group_id = group_id;
	    reqStruct->group_name = vx_strdup(group_name.c_str());
        reqStruct->group_data = vx_strdup(group_data.c_str());

	    std::string newReqID = stateMgr->GenerateID();
	    reqStruct->base.cookie = vx_strdup(newReqID.c_str());
	    stateMgr->InsertCommandReqID(newReqID, "vx_req_account_buddygroup_set_t");
	    ret = vx_issue_request((vx_req_base_t*)reqStruct);
    }
    else
	{
		cout << "Account handle " << ahandle << " not found." << endl;
	}
	return ret;
}

int RequestManager::req_BuddyGroupDelete(std::string ahandle, int group_id)
{
    int ret = -1;
    if (stateMgr->GetIsAccountLoggedIn() && stateMgr->GetAccountHandle() == ahandle)
	{
	    vx_req_account_buddygroup_delete_t* reqStruct;
	    vx_req_account_buddygroup_delete_create(&reqStruct);

        reqStruct->account_handle = vx_strdup(ahandle.c_str());
	    reqStruct->group_id = group_id;

	    std::string newReqID = stateMgr->GenerateID();
	    reqStruct->base.cookie = vx_strdup(newReqID.c_str());
	    stateMgr->InsertCommandReqID(newReqID, "vx_req_account_buddygroup_delete_t");
	    ret = vx_issue_request((vx_req_base_t*)reqStruct);
    }
    else
	{
		cout << "Account handle " << ahandle << " not found." << endl;
	}
	return ret;
}

int RequestManager::req_ListBuddiesAndGroups(std::string ahandle)
{
    int ret = -1;
    if (stateMgr->GetIsAccountLoggedIn() && stateMgr->GetAccountHandle() == ahandle)
	{
	    vx_req_account_list_buddies_and_groups_t* reqStruct;
	    vx_req_account_list_buddies_and_groups_create(&reqStruct);

        reqStruct->account_handle = vx_strdup(ahandle.c_str());
	    std::string newReqID = stateMgr->GenerateID();
	    reqStruct->base.cookie = vx_strdup(newReqID.c_str());
	    stateMgr->InsertCommandReqID(newReqID, "vx_req_account_list_buddies_and_groups_t");
	    ret = vx_issue_request((vx_req_base_t*)reqStruct);
    }
    else
	{
		cout << "Account handle " << ahandle << " not found." << endl;
	}
	return ret;
}

int RequestManager::req_CreateBlockRule(std::string ahandle, std::string mask, int presence_only)
{
    int ret = -1;
    if (stateMgr->GetIsAccountLoggedIn() && stateMgr->GetAccountHandle() == ahandle)
	{
	    vx_req_account_create_block_rule_t* reqStruct;
	    vx_req_account_create_block_rule_create(&reqStruct);

        reqStruct->account_handle = vx_strdup(ahandle.c_str());
        reqStruct->block_mask = vx_strdup(mask.c_str());
        reqStruct->presence_only = presence_only;

	    std::string newReqID = stateMgr->GenerateID();
	    reqStruct->base.cookie = vx_strdup(newReqID.c_str());
	    stateMgr->InsertCommandReqID(newReqID, "vx_req_account_create_block_rule_t");
	    ret = vx_issue_request((vx_req_base_t*)reqStruct);
    }
    else
	{
		cout << "Account handle " << ahandle << " not found." << endl;
	}
	return ret;
}

int RequestManager::req_DeleteBlockRule(std::string ahandle, std::string mask)
{
    int ret = -1;
    if (stateMgr->GetIsAccountLoggedIn() && stateMgr->GetAccountHandle() == ahandle)
	{
	    vx_req_account_delete_block_rule_t* reqStruct;
	    vx_req_account_delete_block_rule_create(&reqStruct);

        reqStruct->account_handle = vx_strdup(ahandle.c_str());
        reqStruct->block_mask = vx_strdup(mask.c_str());

	    std::string newReqID = stateMgr->GenerateID();
	    reqStruct->base.cookie = vx_strdup(newReqID.c_str());
	    stateMgr->InsertCommandReqID(newReqID, "vx_req_account_delete_block_rule_t");
	    ret = vx_issue_request((vx_req_base_t*)reqStruct);
    }
    else
	{
		cout << "Account handle " << ahandle << " not found." << endl;
	}
	return ret;
}

int RequestManager::req_ListBlockRules(std::string ahandle)
{
    int ret = -1;
    if (stateMgr->GetIsAccountLoggedIn() && stateMgr->GetAccountHandle() == ahandle)
	{
	    vx_req_account_list_block_rules_t* reqStruct;
	    vx_req_account_list_block_rules_create(&reqStruct);

        reqStruct->account_handle = vx_strdup(ahandle.c_str());
	    std::string newReqID = stateMgr->GenerateID();
	    reqStruct->base.cookie = vx_strdup(newReqID.c_str());
	    stateMgr->InsertCommandReqID(newReqID, "vx_req_account_list_block_rules_t");
	    ret = vx_issue_request((vx_req_base_t*)reqStruct);
    }
    else
	{
		cout << "Account handle " << ahandle << " not found." << endl;
	}
	return ret;
}

int RequestManager::req_CreateAutoAcceptRule(std::string ahandle, std::string mask, int auto_add)
{
    int ret = -1;
    if (stateMgr->GetIsAccountLoggedIn() && stateMgr->GetAccountHandle() == ahandle)
	{
	    vx_req_account_create_auto_accept_rule_t* reqStruct;
	    vx_req_account_create_auto_accept_rule_create(&reqStruct);

        reqStruct->account_handle = vx_strdup(ahandle.c_str());
        reqStruct->auto_accept_mask = vx_strdup(mask.c_str());
        reqStruct->auto_add_as_buddy = auto_add;

	    std::string newReqID = stateMgr->GenerateID();
	    reqStruct->base.cookie = vx_strdup(newReqID.c_str());
	    stateMgr->InsertCommandReqID(newReqID, "vx_req_account_create_auto_accept_rule_t");
	    ret = vx_issue_request((vx_req_base_t*)reqStruct);
    }
    else
	{
		cout << "Account handle " << ahandle << " not found." << endl;
	}
	return ret;
}

int RequestManager::req_DeleteAutoAcceptRule(std::string ahandle, std::string mask)
{
    int ret = -1;
    if (stateMgr->GetIsAccountLoggedIn() && stateMgr->GetAccountHandle() == ahandle)
	{
	    vx_req_account_delete_auto_accept_rule_t* reqStruct;
	    vx_req_account_delete_auto_accept_rule_create(&reqStruct);

        reqStruct->account_handle = vx_strdup(ahandle.c_str());
        reqStruct->auto_accept_mask = vx_strdup(mask.c_str());

	    std::string newReqID = stateMgr->GenerateID();
	    reqStruct->base.cookie = vx_strdup(newReqID.c_str());
	    stateMgr->InsertCommandReqID(newReqID, "vx_req_account_delete_auto_accept_rule_t");
	    ret = vx_issue_request((vx_req_base_t*)reqStruct);
    }
    else
	{
		cout << "Account handle " << ahandle << " not found." << endl;
	}
	return ret;
}

int RequestManager::req_ListAutoAcceptRules(std::string ahandle)
{
    int ret = -1;
    if (stateMgr->GetIsAccountLoggedIn() && stateMgr->GetAccountHandle() == ahandle)
	{
	    vx_req_account_list_auto_accept_rules_t* reqStruct;
	    vx_req_account_list_auto_accept_rules_create(&reqStruct);

        reqStruct->account_handle = vx_strdup(ahandle.c_str());
	    std::string newReqID = stateMgr->GenerateID();
	    reqStruct->base.cookie = vx_strdup(newReqID.c_str());
	    stateMgr->InsertCommandReqID(newReqID, "vx_req_account_list_auto_accept_rules_t");
	    ret = vx_issue_request((vx_req_base_t*)reqStruct);
    }
    else
	{
		cout << "Account handle " << ahandle << " not found." << endl;
	}
	return ret;
}

int RequestManager::req_SendMessage(std::string shandle, std::string msg)
{
    int ret = -1;
    if (stateMgr->GetTextSessionState(shandle) >= 0)
    {
        vx_req_session_send_message_t* reqStruct;
        vx_req_session_send_message_create(&reqStruct);

        reqStruct->session_handle = vx_strdup(shandle.c_str());
        reqStruct->message_body = vx_strdup(msg.c_str());
        std::string newReqID = stateMgr->GenerateID();
        reqStruct->base.cookie = vx_strdup(newReqID.c_str());
        stateMgr->InsertCommandReqID(newReqID, "vx_req_session_send_message_t");
	    ret = vx_issue_request((vx_req_base_t*)reqStruct);
    }
    else
    {
        cout << "Session handle " << shandle << " not found." << endl;
    }
    return ret;
}

int RequestManager::req_NetworkTest(std::string wellknownip, std::string stunserver, std::string echoserver, int echoPort, int timeout)
{
    int ret = -1;
	vx_req_aux_connectivity_info_t* reqStruct;
	vx_req_aux_connectivity_info_create(&reqStruct);

	std::string newReqID = stateMgr->GenerateID();
	reqStruct->base.cookie = vx_strdup(newReqID.c_str());
	stateMgr->InsertCommandReqID(newReqID, "vx_req_aux_connectivity_info_t");
    reqStruct->well_known_ip = vx_strdup(wellknownip.c_str());
    reqStruct->stun_server = vx_strdup(stunserver.c_str());
    reqStruct->echo_server = vx_strdup(echoserver.c_str());
    reqStruct->echo_port = echoPort;
    reqStruct->timeout = timeout;

	ret = vx_issue_request((vx_req_base_t*)reqStruct);
	return ret;
}

int RequestManager::req_NetworkTest(std::string acct_mgmt_server)
{
    int ret = -1;
	vx_req_aux_connectivity_info_t* reqStruct;
	vx_req_aux_connectivity_info_create(&reqStruct);

	std::string newReqID = stateMgr->GenerateID();
	reqStruct->base.cookie = vx_strdup(newReqID.c_str());
	stateMgr->InsertCommandReqID(newReqID, "vx_req_aux_connectivity_info_t");
    reqStruct->acct_mgmt_server = vx_strdup(acct_mgmt_server.c_str());

	ret = vx_issue_request((vx_req_base_t*)reqStruct);
	return ret;
}

int RequestManager::req_SetPresence(const std::string &acct_handle, const std::string &state, const std::string &message) {
    int ret = -1;
    vx_req_account_set_presence_t *pRequest;
    vx_req_account_set_presence_create(&pRequest);
    pRequest->account_handle = vx_strdup(acct_handle.c_str());
    vx_buddy_presence_state presence = buddy_presence_online;
    if(state == "online") {
        presence = buddy_presence_online;
    } else if(state == "away") {
        presence = buddy_presence_away;
    } else if(state == "busy") { 
        presence = buddy_presence_busy;
    } else if(state == "phone") {
        presence = buddy_presence_onthephone;
    } else if(state == "lunch") {
        presence = buddy_presence_outtolunch;
    } else if(state == "brb") {
        presence = buddy_presence_brb;
    } 
    pRequest->presence = presence;
    pRequest->custom_message = vx_strdup(message.c_str());

    std::string newReqID = stateMgr->GenerateID();
  	pRequest->base.cookie = vx_strdup(newReqID.c_str());
    stateMgr->InsertCommandReqID(newReqID, "vx_req_account_set_presence_t");

    return vx_issue_request(&pRequest->base);
}

int RequestManager::req_SendNotification(std::string shandle, int state)
{
    int ret = -1;
    if (stateMgr->GetTextSessionState(shandle) > 0)
	{
		vx_req_session_send_notification_t* reqStruct;
		vx_req_session_send_notification_create(&reqStruct);

		reqStruct->session_handle = vx_strdup(shandle.c_str());
		if(state == 0)
            reqStruct->notification_type = notification_not_typing;
        else if(state == 1)
            reqStruct->notification_type = notification_typing;
        else
        {
		    cout << "State " << state << " is not valid." << endl;
            return ret;
        }

		std::string newReqID = stateMgr->GenerateID();
        stateMgr->InsertCommandReqID(newReqID, "vx_req_session_send_notification_t");
		reqStruct->base.cookie = vx_strdup(newReqID.c_str());
		ret = vx_issue_request((vx_req_base_t*)reqStruct);
	}
	else
	{
		cout << "Session handle " << shandle << " not found." << endl;
	}
	return ret;
}

int RequestManager::req_UpdateAccount(std::string ahandle, std::string phone, std::string carrier)
{
    int ret = -1;
    if (stateMgr->GetIsAccountLoggedIn() && stateMgr->GetAccountHandle() == ahandle)
	{
	    vx_req_account_update_account_t* reqStruct;
	    vx_req_account_update_account_create(&reqStruct);

        reqStruct->account_handle = vx_strdup(ahandle.c_str());
        if (phone.length() > 0)
        {
            reqStruct->phone = vx_strdup(phone.c_str());
        }
        else
        {
            reqStruct->phone = vx_strdup("");
        }

        if (phone.length() > 0)
        {
            reqStruct->carrier = vx_strdup(carrier.c_str());
        }
        else
        {
            reqStruct->carrier = vx_strdup("");
        }

	    std::string newReqID = stateMgr->GenerateID();
	    reqStruct->base.cookie = vx_strdup(newReqID.c_str());
	    stateMgr->InsertCommandReqID(newReqID, "vx_req_account_update_account_t");
	    ret = vx_issue_request((vx_req_base_t*)reqStruct);
    }
    else
	{
		cout << "Account handle " << ahandle << " not found." << endl;
	}
	return ret;
}

int RequestManager::req_GetAccount(std::string ahandle)
{
    int ret = -1;
    if (stateMgr->GetIsAccountLoggedIn() && stateMgr->GetAccountHandle() == ahandle)
	{
	    vx_req_account_get_account_t* reqStruct;
	    vx_req_account_get_account_create(&reqStruct);

        reqStruct->account_handle = vx_strdup(ahandle.c_str());

	    std::string newReqID = stateMgr->GenerateID();
	    reqStruct->base.cookie = vx_strdup(newReqID.c_str());
	    stateMgr->InsertCommandReqID(newReqID, "vx_req_account_get_account_t");
	    ret = vx_issue_request((vx_req_base_t*)reqStruct);
    }
    else
	{
		cout << "Account handle " << ahandle << " not found." << endl;
	}
	return ret;
}

int RequestManager::req_SendSMS(std::string ahandle, std::string recipient_uri, std::string content)
{
    int ret = -1;
    if (stateMgr->GetIsAccountLoggedIn() && stateMgr->GetAccountHandle() == ahandle)
	{
	    vx_req_account_send_sms_t* reqStruct;
	    vx_req_account_send_sms_create(&reqStruct);

        reqStruct->account_handle = vx_strdup(ahandle.c_str());
	    reqStruct->recipient_uri = vx_strdup(recipient_uri.c_str());
        reqStruct->content = vx_strdup(content.c_str());

	    std::string newReqID = stateMgr->GenerateID();
	    reqStruct->base.cookie = vx_strdup(newReqID.c_str());
	    stateMgr->InsertCommandReqID(newReqID, "vx_req_account_send_sms_t");
	    ret = vx_issue_request((vx_req_base_t*)reqStruct);
    }
    else
	{
		cout << "Account handle " << ahandle << " not found." << endl;
	}
	return ret;
}

int RequestManager::req_SetIdleTimeout(int seconds)
{
    int ret = -1;
    vx_req_aux_set_idle_timeout_t *req = NULL;
    vx_req_aux_set_idle_timeout_create(&req);
    req->base.cookie = vx_strdup(stateMgr->GenerateID().c_str());
    req->seconds = seconds;
    return vx_issue_request((vx_req_base_t *)req);
}

int RequestManager::req_BindKey(const std::string &handle, const std::set<int> &codes)
{
    int ret = -1;
    vx_req_aux_global_monitor_keyboard_mouse_t *req = NULL;
    vx_req_aux_global_monitor_keyboard_mouse_create(&req);
    req->base.cookie = vx_strdup(stateMgr->GenerateID().c_str());
    req->name = vx_strdup(handle.c_str());
    int k = 0;
    for(std::set<int>::const_iterator i = codes.begin();i!=codes.end();++i, ++k) {
        req->codes[k] = *i;
    }
    req->code_count = codes.size();
    return vx_issue_request((vx_req_base_t *)req);
}

int RequestManager::req_CreateAccount(const std::string &adminAcct, const std::string &adminPassword, const std::string &grantDocument, 
                                      const std::string &username, const std::string &password, const std::string &serverUrl)
{
    int ret = -1;
    vx_req_aux_create_account_t *req = NULL;
    vx_req_aux_create_account_create(&req);
    req->base.cookie = vx_strdup(stateMgr->GenerateID().c_str());
    if(!adminAcct.empty()) {
        req->credentials.admin_username = vx_strdup(adminAcct.c_str());
        req->credentials.admin_password = vx_strdup(adminPassword.c_str());
    } else {
        req->credentials.grant_document = vx_strdup(grantDocument.c_str());
    }
    req->credentials.server_url = vx_strdup(serverUrl.c_str());
    req->user_name = vx_strdup(username.c_str());
    req->password = vx_strdup(password.c_str());
    return vx_issue_request((vx_req_base_t *)req);
}
