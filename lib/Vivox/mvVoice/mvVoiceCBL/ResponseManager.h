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

//#include "SDKSampleApp.h"
#include <fstream>
#include <iostream>
#ifdef WIN32
#include <direct.h>
#else
#include <sys/types.h>
#include <sys/stat.h>
#endif
#include <vector>
#include <ctime>
#include "StateManager.h"

class ResponseManager
{
		StateManager * stateMgr;
        void WriteToFile(const char*);
        std::string CreateTimeStamp();
        void CreateFolder(std::string);
		int debuglevel;
		bool stopOnShutdownResponse;
		bool stopNow;
        std::string FormatXml(std::string);

	public:
        ResponseManager(StateManager *);
        ~ResponseManager();

        void WriteResponseToFile(vx_resp_base_t*);
        void WriteEventToFile(vx_evt_base_t*);
        void SetDebug(int);

        // Response methods.  These are called to handle the appropriate Response.
        void resp_ConnectorCreate(vx_resp_connector_create_t*);                 //required for state mgmt
        void resp_ConnectorShutdown(vx_resp_connector_initiate_shutdown_t*);    //required for state mgmt
        void resp_SGSetTXSession(vx_resp_sessiongroup_set_tx_session_t*);
        void resp_SGSetTXAll(vx_resp_sessiongroup_set_tx_all_sessions_t*);
        void resp_SGSetTXNone(vx_resp_sessiongroup_set_tx_no_session_t*);
        void resp_ChannelCreate(vx_resp_account_channel_create_t*);
        void resp_ChannelFolderCreate(vx_resp_account_channel_folder_create_t*);
        void resp_ChannelFavSet(vx_resp_account_channel_favorite_set_t*);
        void resp_ChannelFavGroupSet(vx_resp_account_channel_favorite_group_set_t*);
        void resp_NetworkTest(vx_resp_aux_connectivity_info_t*);
        void resp_ListRenderDevices(vx_resp_aux_get_render_devices_t* respObj);
        void resp_ListCaptureDevices(vx_resp_aux_get_capture_devices_t* respObj);

        // Events
        void evt_Generic(vx_evt_base_t*);
        void evt_AccountLoginStateChange(vx_evt_account_login_state_change_t*);
        void evt_SessionGroupAdded(vx_evt_sessiongroup_added_t*);
        void evt_SessionGroupRemoved(vx_evt_sessiongroup_removed_t*);
        void evt_SessionAdded(vx_evt_session_added_t*);
        void evt_SessionRemoved(vx_evt_session_removed_t*);
        void evt_MediaStreamUpdated(vx_evt_media_stream_updated_t*);
        void evt_TextStreamUpdated(vx_evt_text_stream_updated_t*);

        void evt_Message(vx_evt_message_t*);
        void evt_BuddyPresenceChange(vx_evt_buddy_presence_t* evtObj);
        void evt_BuddyChanged(vx_evt_buddy_changed_t *evtObj);
        void evt_BuddyGroupChanged(vx_evt_buddy_group_changed_t *evtObj);
        void evt_AuxAudioProperties(vx_evt_aux_audio_properties_t* evtObj);

        void Stop(bool waitForShutdown);
		bool IsStopped() const;
};
