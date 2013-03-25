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

#ifndef __SESSIONSTATE_H
#define __SESSIONSTATE_H

#include <map>
#include <set>
#include <string>

class VxSession
{
private:
    std::string sessionHandle;
    std::string sessionUri;
    int mediaState;
    int textState;
    int incoming;
    bool isTransmitting;
public:
    VxSession(std::string session_handle, std::string uri);
    ~VxSession();
    std::string GetSessionHandle();
    std::string GetSessionURI();
    int GetMediaState();
    void SetMediaState(int state);
    int GetTextState();
    void SetTextState(int state);
    int GetIsIncoming();
    void SetIsIncoming(int state);
    bool GetIsTransmitting();
    void SetIsTransmitting(bool state);
};

#endif
