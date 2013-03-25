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

#ifndef __VXCEVENTS_H__
#define __VXCEVENTS_H__

#include "Vxc.h"

#ifdef __cplusplus
extern "C" {
#endif

/* Begin Vivox events */

//DEPRECATED
typedef struct vx_evt_login_state_change {
    vx_evt_base base;
    vx_login_state_change_state state;
    VX_HANDLE account_handle;
    int status_code;
    char* status_string;
} vx_evt_login_state_change_t;
VIVOXSDK_DLLEXPORT void vx_evt_login_state_change_free(vx_evt_login_state_change_t * evt);

typedef struct vx_evt_account_login_state_change {
    vx_evt_base base;
    vx_login_state_change_state state;
    VX_HANDLE account_handle;
    int status_code;
    char* status_string;
} vx_evt_account_login_state_change_t;
VIVOXSDK_DLLEXPORT void vx_evt_account_login_state_change_free(vx_evt_account_login_state_change_t * evt);

//DEPRECATED
typedef struct vx_evt_session_new {
    vx_evt_base base;
    vx_session_new_state state;
    VX_HANDLE account_handle;
    VX_HANDLE session_handle;
    char* uri;
    char* name;
    int is_channel; /* 1 true, <= 0 false */
    char* audio_media; /* FIXME */
    int has_text;
    int has_audio;
    int has_video;
} vx_evt_session_new_t;
VIVOXSDK_DLLEXPORT void vx_evt_session_new_free(vx_evt_session_new_t * evt);

//DEPRECATED
typedef struct vx_evt_session_state_change {
    vx_evt_base base;
    vx_session_state_change_state state;
    VX_HANDLE session_handle;
    int status_code;
    char* status_string;
    char* uri;
    int is_channel;
    char* channel_name;
} vx_evt_session_state_change_t;
VIVOXSDK_DLLEXPORT void vx_evt_session_state_change_free(vx_evt_session_state_change_t * evt);

//DEPRECATED
typedef struct vx_evt_participant_state_change {
    vx_evt_base base;
    vx_participant_state_change_state state;
    VX_HANDLE session_handle;
    int status_code;
    char* status_string;
    char* participant_uri;
    char* account_name;
    int participant_type;
} vx_evt_participant_state_change_t;
VIVOXSDK_DLLEXPORT void vx_evt_participant_state_change_free(vx_evt_participant_state_change_t * evt);

//DEPRECATED
typedef struct vx_evt_participant_properties {
    vx_evt_base base;
    vx_participant_properties_state state;
    VX_HANDLE session_handle;
    char* participant_uri;
    int is_moderator_muted;
    int is_locally_muted;
    int is_speaking;
    int volume;
    double energy;
} vx_evt_participant_properties_t;
VIVOXSDK_DLLEXPORT void vx_evt_participant_properties_free(vx_evt_participant_properties_t * evt);

typedef struct vx_evt_buddy_presence {
    vx_evt_base base;
    vx_buddy_presence_state state;
    VX_HANDLE account_handle;
    char* buddy_uri;
    vx_buddy_presence_state presence;
    char* custom_message;
} vx_evt_buddy_presence_t;
VIVOXSDK_DLLEXPORT void vx_evt_buddy_presence_free(vx_evt_buddy_presence_t * evt);

typedef struct vx_evt_subscription {
    vx_evt_base base;
    vx_subscription_state state;
    VX_HANDLE account_handle;
    char* buddy_uri;
    char* subscription_handle;
    vx_subscription_type subscription_type;
} vx_evt_subscription_t;
VIVOXSDK_DLLEXPORT void vx_evt_subscription_free(vx_evt_subscription_t * evt);

typedef struct vx_evt_session_notification {
    vx_evt_base base;
    vx_session_notification_state state;
    VX_HANDLE session_handle;
    char* participant_uri;
    vx_notification_type notification_type;
} vx_evt_session_notification_t;
VIVOXSDK_DLLEXPORT void vx_evt_session_notification_free(vx_evt_session_notification_t * evt);

typedef struct vx_evt_message {
    vx_evt_base base;
    vx_message_state state;
    VX_HANDLE sessiongroup_handle;
    VX_HANDLE session_handle;
    char* participant_uri;
    char* message_header;
    char* message_body;
} vx_evt_message_t;
VIVOXSDK_DLLEXPORT void vx_evt_message_free(vx_evt_message_t * evt);

typedef struct vx_evt_aux_audio_properties {
    vx_evt_base base;
    vx_aux_audio_properties_state state;
    int mic_is_active;
    int mic_volume;
    double mic_energy;
    int speaker_volume;
} vx_evt_aux_audio_properties_t;
VIVOXSDK_DLLEXPORT void vx_evt_aux_audio_properties_free(vx_evt_aux_audio_properties_t * evt);

//DEPRECATED
typedef struct vx_evt_participant_list {
    vx_evt_base base;
    vx_participant_list_state state;
    VX_HANDLE session_handle;
    int participants_size;
    vx_participant_t** participants;
} vx_evt_participant_list_t;
VIVOXSDK_DLLEXPORT void vx_evt_participant_list_free(vx_evt_participant_list_t * evt);

typedef struct vx_evt_session_participant_list {
    vx_evt_base base;
    VX_HANDLE sessiongroup_handle;
    VX_HANDLE session_handle;
    int participants_size;
    vx_participant_t** participants;
} vx_evt_session_participant_list_t;
VIVOXSDK_DLLEXPORT void vx_evt_session_participant_list_free(vx_evt_session_participant_list_t * evt);

//DEPRECATED
typedef struct vx_evt_session_media {
    vx_evt_base base;
    vx_session_media_state state;
    VX_HANDLE session_handle;
    int has_text;
    int has_audio;
    int has_video;
    bool terminated;
} vx_evt_session_media_t;
VIVOXSDK_DLLEXPORT void vx_evt_session_media_free(vx_evt_session_media_t * evt);

typedef enum {
    change_type_set = 1,
    change_type_delete = 2,
} vx_change_type_t;

typedef struct vx_evt_buddy_changed {
    vx_evt_base base;
    VX_HANDLE account_handle;
    vx_change_type_t change_type;
    char *buddy_uri;
    char *display_name;
    char *buddy_data;
    int group_id;
} vx_evt_buddy_changed_t;
VIVOXSDK_DLLEXPORT void vx_evt_buddy_changed_free(vx_evt_buddy_changed * evt);

typedef struct vx_evt_buddy_group_changed {
    vx_evt_base base;
    VX_HANDLE account_handle;
    vx_change_type_t change_type;
    int group_id;
    char *group_name;
    char *group_data;
} vx_evt_buddy_group_changed_t;
VIVOXSDK_DLLEXPORT void vx_evt_buddy_group_changed_free(vx_evt_buddy_group_changed * evt);

typedef struct {
	vx_evt_base base;
    VX_HANDLE account_handle;
	int buddy_count;
	vx_buddy_t **buddies;
	int group_count;
	vx_group_t **groups;
} vx_evt_buddy_and_group_list_changed_t;
VIVOXSDK_DLLEXPORT void vx_evt_buddy_and_group_list_changed_free(vx_evt_buddy_and_group_list_changed_t * evt);

typedef struct vx_evt_keyboard_mouse {
    vx_evt_base base;
    char *name;
    int is_down;
} vx_evt_keyboard_mouse_t;
VIVOXSDK_DLLEXPORT void vx_evt_keyboard_mouse_free(vx_evt_keyboard_mouse_t * evt);

typedef struct vx_evt_idle_state_changed {
    vx_evt_base base;
    int is_idle;
} vx_evt_idle_state_changed_t;
VIVOXSDK_DLLEXPORT void vx_evt_idle_state_changed_free(vx_evt_idle_state_changed_t * evt);

typedef struct vx_evt_media_stream_updated {
    vx_evt_base base;
    VX_HANDLE sessiongroup_handle;
    VX_HANDLE session_handle;
    int status_code;
	char* status_string;
	vx_session_media_state state;
	int incoming;
} vx_evt_media_stream_updated_t;
VIVOXSDK_DLLEXPORT void vx_evt_media_stream_updated_free(vx_evt_media_stream_updated_t * evt);

typedef struct vx_evt_text_stream_updated {
    vx_evt_base base;
    VX_HANDLE sessiongroup_handle;
    VX_HANDLE session_handle;
    int enabled;
    vx_session_text_state state;
	int incoming;
} vx_evt_text_stream_updated_t;
VIVOXSDK_DLLEXPORT void vx_evt_text_stream_updated_free(vx_evt_text_stream_updated_t * evt);

typedef struct vx_evt_sessiongroup_added {
    vx_evt_base base;
    VX_HANDLE sessiongroup_handle;
} vx_evt_sessiongroup_added_t;
VIVOXSDK_DLLEXPORT void vx_evt_sessiongroup_added_free(vx_evt_sessiongroup_added_t * evt);

typedef struct vx_evt_sessiongroup_removed {
    vx_evt_base base;
    VX_HANDLE sessiongroup_handle;
} vx_evt_sessiongroup_removed_t;
VIVOXSDK_DLLEXPORT void vx_evt_sessiongroup_removed_free(vx_evt_sessiongroup_removed_t * evt);

typedef struct vx_evt_session_added {
    vx_evt_base base;
    VX_HANDLE sessiongroup_handle;
    VX_HANDLE session_handle;
    char* uri;
	int is_channel;
    int incoming;
	char* channel_name;
} vx_evt_session_added_t;
VIVOXSDK_DLLEXPORT void vx_evt_session_added_free(vx_evt_session_added_t * evt);

typedef struct vx_evt_session_removed {
    vx_evt_base base;
    VX_HANDLE sessiongroup_handle;
    VX_HANDLE session_handle;
} vx_evt_session_removed_t;
VIVOXSDK_DLLEXPORT void vx_evt_session_removed_free(vx_evt_session_removed_t * evt);

typedef struct vx_evt_participant_added {
    vx_evt_base base;
    VX_HANDLE sessiongroup_handle;
    VX_HANDLE session_handle;
    char* participant_uri;
    char* account_name;
    char* display_name;
    int participant_type;
} vx_evt_participant_added_t;
VIVOXSDK_DLLEXPORT void vx_evt_participant_added_free(vx_evt_participant_added_t * evt);

typedef struct vx_evt_participant_removed {
    vx_evt_base base;
    VX_HANDLE sessiongroup_handle;
    VX_HANDLE session_handle;
    char* participant_uri;
    char* account_name;
} vx_evt_participant_removed_t;
VIVOXSDK_DLLEXPORT void vx_evt_participant_removed_free(vx_evt_participant_removed_t * evt);

typedef struct vx_evt_participant_updated {
    vx_evt_base base;
    VX_HANDLE sessiongroup_handle;
    VX_HANDLE session_handle;
    char* participant_uri;
    int is_moderator_muted;
    int is_speaking;
    int volume;
    double energy;
} vx_evt_participant_updated_t;
VIVOXSDK_DLLEXPORT void vx_evt_participant_updated_free(vx_evt_participant_updated_t * evt);

/* End Vivox events */

void VIVOXSDK_DLLEXPORT destroy_evt(vx_evt_base_t *pCmd);

#ifdef __cplusplus
}
#endif

#endif /* ndef __VXCEVENTS_H__ */

