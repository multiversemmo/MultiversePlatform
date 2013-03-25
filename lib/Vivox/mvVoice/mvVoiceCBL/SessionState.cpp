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

#include "SessionState.h"

using namespace std;

VxSession::VxSession(string session_handle, string uri)
{
    sessionHandle = session_handle;
    sessionUri = uri;
    incoming = false;
    mediaState = 0;
    textState = 0;
    isTransmitting = false;
}

VxSession::~VxSession()
{
}

string VxSession::GetSessionHandle()
{
    return sessionHandle;
}

string VxSession::GetSessionURI()
{
    return sessionUri;
}

int VxSession::GetMediaState()
{
    return mediaState;
}

void VxSession::SetMediaState(int state)
{
    mediaState = state;
}

int VxSession::GetTextState()
{
    return textState;
}

void VxSession::SetTextState(int state)
{
    textState = state;
}

int VxSession::GetIsIncoming()
{
    return incoming;
}

void VxSession::SetIsIncoming(int state)
{
    incoming = state;
}

bool VxSession::GetIsTransmitting()
{
    return isTransmitting;
}

void VxSession::SetIsTransmitting(bool state)
{
    isTransmitting = state;
}
