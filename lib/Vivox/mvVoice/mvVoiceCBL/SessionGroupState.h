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

#ifndef __SESSIONGROUPSTATE_H
#define __SESSIONGROUPSTATE_H

#include "SessionState.h"

class VxSessionGroup
{
private:
    std::string sessionGroupHandle;
    std::map<std::string,VxSession*> sessions;		    //stores session handles and corresponding states
    bool IsFirstAudioSession();
public:
    VxSessionGroup(std::string session_group_handle);
    ~VxSessionGroup();
    void AddSession(std::string session_handle, std::string uri, int incoming);
    void RemoveSession(std::string session_handle);
    void UpdateMediaState(std::string session_handle, int state);
    void UpdateTextState(std::string session_handle, int state);
    std::set<std::string> GetSessionHandles();
    VxSession* GetSession(std::string session_handle);
    std::string GetSessionGroupHandle();
    int GetNumberOfSessions();
    void SetSessionsTxValue(bool state);
};

#endif
