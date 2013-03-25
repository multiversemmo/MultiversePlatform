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

#include <iostream>
#include "StateManager.h"
//#include "SDKSampleApp.h"

class RequestManager
{
		StateManager * stateMgr;
		int debuglevel;

	public:
		RequestManager(StateManager*);
		void SetDebug(int);
		// Requests
		int req_ConnectorCreate(std::string, int, int);
		int req_ConnectorShutdown(std::string);
		int req_AccountLogin(std::string, std::string, std::string, int, int, int, int);
		int req_AccountLogout(std::string);
        int req_SetLoginProperties(std::string, int, int);
        int req_SessionGroupCreate(std::string);
        int req_SessionGroupTerminate(std::string);
        int req_SessionGroupAddSession(std::string, std::string, int, std::string);
        int req_SessionGroupRemoveSession(std::string, std::string);
        int req_SessionGroupSetFocus(std::string, std::string);
        int req_SessionGroupUnsetFocus(std::string, std::string);
        int req_SessionGroupResetFocus(std::string);
        int req_SessionGroupSetTx(std::string, std::string);
        int req_SessionGroupSetTxAll(std::string);
        int req_SessionGroupSetTxNone(std::string);
        int req_SessionCreate(std::string, std::string, std::string, std::string, int, int);
        int req_ConnectSession(std::string);
        int req_MediaConnect(std::string);
        int req_MediaDisconnect(std::string);
		int req_SessionTerminate(std::string);
        int req_SessionMuteLocalSpeaker(std::string, int);
        int req_SessionSetLocalSpeakerVolume(std::string, int);
        int req_SessionGetLocalAudioInfo(std::string);
		int req_CreateChannelAndInvite(std::string, std::string, std::string, int, int, int, int, std::string, std::string);
		int req_CreateChannel(std::string, std::string, std::string, vx_channel_type, int, int, int, int, int, int, double, double, int, std::string);
		int req_UpdateChannel(std::string, std::string, std::string, std::string, int, int, int, int, int, int, double, double, int, std::string);
		int req_DeleteChannel(std::string, std::string);
		int req_CreateFolder(std::string, std::string, std::string, int);
		int req_UpdateFolder(std::string, int, std::string, std::string);
		int req_DeleteFolder(std::string, int);
        int req_GetFolderInfo(std::string, int);
        int req_GetFavs(std::string);
        int req_SetFav(std::string, int, std::string, std::string, int, std::string);
        int req_DeleteFav(std::string, int);
        int req_SetFavGroup(std::string, int, std::string, std::string);
        int req_DeleteFavGroup(std::string, int);
        int req_GetChannelInfo(std::string, std::string);
        int req_SearchChannels(std::string, int, int, std::string, std::string, int);
        int req_SearchAccounts(std::string, int, int, std::string, std::string, std::string, std::string);
		int req_ModeratorAdd(std::string, std::string, std::string);
		int req_ModeratorRemove(std::string, std::string, std::string);
        int req_ModeratorGet(std::string, std::string);
		int req_ACLAdd(std::string, std::string, std::string);
		int req_ACLRemove(std::string, std::string, std::string);
        int req_ACLGet(std::string, std::string);
		int req_MuteUser(std::string, int, std::string, std::string);
		int req_MuteAllUsers(std::string, int, std::string);
		int req_BanUser(std::string, int, std::string, std::string);
        int req_GetBannedUsers(std::string, std::string);
		int req_KickUser(std::string, std::string, std::string);
		int req_InviteUser(std::string, std::string);
        int req_LocalUserMute(std::string, int, std::string);
        int req_LocalUserVolume(std::string, int, std::string);
        int req_GetChannels(std::string);
		int req_GetParts(std::string);
		int req_AudioInfo(std::string);
		int req_SpeakerVol(std::string, int);
		int req_SpeakerMute(std::string, int);
		int req_MicVol(std::string, int);
		int req_MicMute(std::string, int);
        int req_BuddySet(const std::string &, std::string, std::string, std::string, int);
        int req_BuddyDelete(const std::string &, std::string);
        int req_BuddyGroupSet(std::string, int, std::string, std::string);
        int req_BuddyGroupDelete(std::string, int);
        int req_ListBuddiesAndGroups(std::string);
        int req_CreateBlockRule(std::string, std::string, int);
        int req_DeleteBlockRule(std::string, std::string);
        int req_ListBlockRules(std::string);
        int req_CreateAutoAcceptRule(std::string, std::string, int);
        int req_DeleteAutoAcceptRule(std::string, std::string);
        int req_ListAutoAcceptRules(std::string);
        int req_SendMessage(std::string, std::string);
        int req_NetworkTest(std::string, std::string, std::string, int, int);
        int req_NetworkTest(std::string);
        int req_moveToOrigin(std::string);
        int req_moveLeft(std::string, double);
        int req_moveForward(std::string, double);
        int req_moveBack(std::string, double);
        int req_moveRight(std::string, double);
        int req_turnLeft45(std::string);
        int req_turnRight45(std::string);
        int req_ListRenderDevices(void);
        int req_SetRenderDevice(std::string);
        int req_SetDefaultRenderDevice(void);
        int req_ListCaptureDevices(void);
        int req_SetCaptureDevice(std::string);
        int req_SetDefaultCaptureDevice(void);
        int req_RenderAudioStart(std::string, int);
        int req_RenderAudioStop(void);
        int req_MasterMicSetVol(int vol);
        int req_MasterSpeakerSetVol(int vol);
        int req_MasterMicGetVol(void);
        int req_MasterSpeakerGetVol(void);
        int req_CaptureAudioStart(void);
        int req_CaptureAudioStop(void);
        int req_SetPresence(const std::string &acct_handle, const std::string &state, const std::string &message);
        int req_SendNotification(std::string sessionID, int state);
        int req_UpdateAccount(std::string, std::string, std::string);
        int req_GetAccount(std::string);
        int req_SendSMS(std::string, std::string, std::string);
        int req_SetIdleTimeout(int seconds);
        int req_BindKey(const std::string &handle, const std::set<int> &codes);
        int req_CreateAccount(const std::string &adminAcct, const std::string &adminPassword, const std::string &grantDocument, 
            const std::string &username, const std::string &password, const std::string &serverUrl);
};
