#ifndef __VXCERRORS_H
#define __VXCERRORS_H
#include "VxcExports.h"

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

// Error Code Definitions
#define VX_E_INVALID_XML						1000
#define VX_E_NO_EXIST							1001
#define VX_E_MAX_CONNECTOR_LIMIT_EXCEEDED		1002
#define VX_E_MAX_SESSION_LIMIT_EXCEEDED			1003
#define VX_E_FAILED								1004
#define VX_E_ALREADY_LOGGED_IN					1005
#define VX_E_ALREADY_LOGGED_OUT					1006 // This is really not returned
#define VX_E_NOT_LOGGED_IN						1007 // this is not returned
#define VX_E_INVALID_ARGUMENT					1008
#define VX_E_INVALID_USERNAME_OR_PASSWORD		1009
#define VX_E_INSUFFICIENT_PRIVILEGE				1010

#define VX_E_NO_SUCH_SESSION                    1011
#define VX_E_NOT_INITIALIZED                    1012
#define VX_E_REQUESTCONTEXT_NOT_FOUND           1013
#define VX_E_LOGIN_FAILED                       1014
#define VX_E_SESSION_MAX                        1015 // used if already on a call
#define VX_E_WRONG_CONNECTOR                    1016
#define VX_E_NOT_IMPL                           1017
#define VX_E_REQUEST_CANCELLED					1018
#define VX_E_INVALID_SESSION_STATE				1019
#define VX_E_SESSION_CREATE_PENDING				1020
#define VX_E_SESSION_TERMINATE_PENDING			1021
#define VX_E_SESSION_CHANNEL_TEXT_DENIED		1022 // We currently do not support multi-party text chat
#define VX_E_SESSION_TEXT_DENIED		        1023 // This session does not support text messaging
#define VX_E_SESSION_MESSAGE_BUILD_FAILED       1024 // The call to am_message_build failed for an unknown reason
#define VX_E_SESSION_MSG_CONTENT_TYPE_FAILED    1025 // The call to osip_message_set_content_type failed for an unknown reason
#define VX_E_SESSION_MEDIA_CONNECT_FAILED       1026 // The media connect call failed
#define VX_E_SESSION_MEDIA_DISCONNECT_FAILED    1026 // The media disconnect call failed
#define VX_E_SESSION_DOES_NOT_HAVE_TEXT         1027 // The session does not have text
#define VX_E_SESSION_DOES_NOT_HAVE_AUDIO        1028 // The session does not have audio
#define VX_E_SESSION_MUST_HAVE_MEDIA            1029 // The session must have media specified (audio or text)
#define VX_E_SESSION_IS_NOT_3D                  1030 // The session is not a SIREN14-3D codec call, therefore it can not be a 3D call
#define VX_E_SESSIONGROUP_NOT_FOUND             1031 // The sessiongroup can not be found
#define VX_E_REQUEST_TYPE_NOT_SUPPORTED         1032 
#define VX_E_REQUEST_NOT_SUPPORTED              1033 
#define VX_E_MULTI_CHANNEL_DENIED               1034 
#define VX_E_MEDIA_DISCONNECT_NOT_ALLOWED       1035
#define VX_E_PRELOGIN_INFO_NOT_RETURNED         1036

#define VX_E_FAILED_TO_CONNECT_TO_SERVER		10007

#ifdef __cplusplus
extern "C" {
#endif
VIVOXSDK_DLLEXPORT char *vx_get_error_string(int errorCode);
#ifdef __cplusplus
}
#endif

#endif
