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

#ifndef __STATEMANAGER_H
#define __STATEMANAGER_H

#ifdef _WIN32
#include <windows.h>
#endif

#include <sstream>
#include "SessionState.h"
#include "SessionGroupState.h"
#include "Vxc.h"
#include "VxcResponses.h"
#include "VxcEvents.h"
#include "math.h"

#define PI 3.1415926535897932384626433832795

class StateManager
{
        int guid;
        bool isConnectorInitialized;
        bool isAccountLoggedIn;
        std::string connectorHandle;
        std::string accountHandle;
		std::map<std::string,VxSessionGroup*> sessionGroups;        //stores session handles and corresponding states
        std::map<std::string,std::string> requests;                 //stores requestIDs and corresponding types

        //positional info
        double listenerHeadingDegrees; // Listener's heading in degrees (North (Negative Z axis) is 0 deg, East (Positive X axis) is +90 deg etc)
        double listenerPosition[3];
        double listenerOrientation[6]; 

        //version info
        std::string version;
  public:
        StateManager();
        std::string GenerateID();
        static std::string GetTime();

        //Application state methods
        bool SetConnectorInitialized(std::string, std::string);
        void SetConnectorUninitialized();

        bool SetStateAccountLoggedIn(std::string account_handle);
        void SetStateAccountLoggedOut();

        bool AddSessionGroup(std::string session_group_handle);
        void RemoveSessionGroup(std::string session_group_handle);
        VxSessionGroup* GetSessionGroup(std::string session_group_handle);
        void ClearSessionGroups();
        std::set<std::string> GetSessionGroupHandles();

        bool AddSession(std::string session_group_handle, std::string session_handle, std::string uri, int incoming);
        void RemoveSession(std::string session_group_handle, std::string session_handle);

        void UpdateMediaStreamState(std::string session_group_handle, std::string session_handle, int state);
        void UpdateTextStreamState(std::string session_group_handle, std::string session_handle, int state);

        //State info
        bool GetIsConnectorInitialized();
        bool GetIsAccountLoggedIn();
        std::string GetConnectorHandle();
        std::string GetAccountHandle();
        bool GetSessionGroupExists(std::string);
        std::set<std::string> GetAllSessionGroupHandles();
        std::set<std::string> GetAllSessionHandles();
        int GetMediaSessionState(std::string);
        int GetTextSessionState(std::string);

        //State info dump
		void DumpStateToFile(std::ostream*);

		//request map methods
        void InsertCommandReqID(std::string, std::string);
		void DeleteCommandReqID(std::string);

        //positional info methods
        void GetListenerPosition(double *);
        void GetListenerOrientation(double *);
        void SetListenerPosition(double *);
        void SetListenerOrientation(double *);
        void GetListenerHeadingDegrees(double *);
        void SetListenerHeadingDegrees(double);
};

#endif
