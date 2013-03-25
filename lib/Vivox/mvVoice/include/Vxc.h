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

#ifndef __VXC_H__
#define __VXC_H__

#include <assert.h>
#include "VxcExports.h"

#define SAFE_STRDUP(s) s ? strdup(s) : NULL;
#define SAFE_FREE(s) s ? free(s) : NULL;
#define SAFE_STR(s) (s) ? (s) : ""


#ifdef __cplusplus
extern "C" {
#endif
    VIVOXSDK_DLLEXPORT char *safe_strdup(const char *s);
    VIVOXSDK_DLLEXPORT void safe_free(char *s);

    typedef char* VX_COOKIE;
    typedef VX_COOKIE VX_HANDLE;

    VIVOXSDK_DLLEXPORT void vx_cookie_create(const char* value, VX_COOKIE* cookie);
    VIVOXSDK_DLLEXPORT void vx_cookie_free(VX_COOKIE* cookie);

	enum vx_log_level
	{
        log_error = 0,
        log_warning,
        log_info,
        log_debug,
        log_trace
    };

    typedef enum vx_attempt_stun {
        attempt_stun_unspecified,
        attempt_stun_on,
        attempt_stun_off,
    };

    typedef enum vx_connector_mode {
        connector_mode_normal=0,
        connector_mode_legacy,
    };
    
    typedef enum vx_message_type {
        msg_none=0,
        msg_request=1,
        msg_response,
        msg_event,
    };

    typedef enum vx_media_type {
        media_type_none=0,
        media_type_text,
        media_type_audio,
        media_type_video,
        media_type_audiovideo,
    };

    typedef enum vx_channel_type {
        channel_type_normal=0,
        channel_type_dir,
        channel_type_positional
    };

    typedef enum vx_channel_search_type {
        channel_search_type_all=0,
        channel_search_type_non_positional,
        channel_search_type_positional
    };

    typedef struct vx_message_base {
        vx_message_type type;
        unsigned long long create_time_ms;
        unsigned long long last_step_ms;
    } vx_message_base_t;

    /** The set of requests that can be issued. */
    typedef enum vx_request_type {
        req_none=0,
        req_connector_create,
        req_connector_initiate_shutdown,
        req_account_login,
        req_account_logout,
        req_account_set_login_properties,
        req_sessiongroup_create,
        req_sessiongroup_terminate,
        req_sessiongroup_add_session,
        req_sessiongroup_remove_session,
        req_sessiongroup_set_focus,
        req_sessiongroup_unset_focus,
        req_sessiongroup_reset_focus,
        req_sessiongroup_set_tx_session,
        req_sessiongroup_set_tx_all_sessions,
        req_sessiongroup_set_tx_no_session,
        req_session_create,
        req_session_connect,
        req_session_media_connect,
        req_session_media_disconnect,
		req_session_terminate,
        req_session_mute_local_speaker,
        req_session_set_local_speaker_volume,
        req_session_get_local_audio_info,
        req_session_channel_invite_user,
        req_session_set_participant_volume_for_me,
        req_session_set_participant_mute_for_me,
        req_session_set_3d_position,
        req_session_render_audio_start,
        req_session_render_audio_stop,
        req_session_channel_get_participants,
        req_account_channel_get_list,
		req_account_channel_create,
		req_account_channel_update,
		req_account_channel_delete,
		req_account_channel_create_and_invite,
		req_account_channel_folder_create,
		req_account_channel_folder_update,
		req_account_channel_folder_delete,
        req_account_channel_folder_get_info,
        req_account_channel_favorites_get_list,
        req_account_channel_favorite_set,
        req_account_channel_favorite_delete,
        req_account_channel_favorite_group_set,
        req_account_channel_favorite_group_delete,
        req_account_channel_get_info,
        req_account_channel_search,
        req_account_buddy_search,
		req_account_channel_add_moderator,
		req_account_channel_remove_moderator,
        req_account_channel_get_moderators,
		req_account_channel_add_acl,
		req_account_channel_remove_acl,
        req_account_channel_get_acl,
        req_channel_mute_user,
		req_channel_ban_user,
        req_channel_get_banned_users,
		req_channel_kick_user,
		req_channel_mute_all_users,
		req_connector_mute_local_mic,
        req_connector_mute_local_speaker,
		req_connector_set_local_mic_volume,
		req_connector_set_local_speaker_volume,
        req_connector_get_local_audio_info,
        req_account_buddy_set,
        req_account_buddy_delete,
        req_account_buddygroup_set,
        req_account_buddygroup_delete,
        req_account_list_buddies_and_groups,
        req_session_send_message,
        req_account_set_presence,
        req_account_send_subscription_reply,
        req_session_send_notification,
        req_account_create_block_rule,
        req_account_delete_block_rule,
        req_account_list_block_rules,
        req_account_create_auto_accept_rule,
        req_account_delete_auto_accept_rule,
        req_account_list_auto_accept_rules,
        req_account_update_account,
        req_account_get_account,
        req_account_send_sms,
        req_aux_connectivity_info,
        req_aux_get_render_devices,
        req_aux_get_capture_devices,
        req_aux_set_render_device,
        req_aux_set_capture_device,
        req_aux_get_mic_level,
        req_aux_get_speaker_level,
        req_aux_set_mic_level,
        req_aux_set_speaker_level,
        req_aux_render_audio_start,
        req_aux_render_audio_stop,
        req_aux_capture_audio_start,
        req_aux_capture_audio_stop,
        req_aux_global_monitor_keyboard_mouse,
        req_aux_set_idle_timeout,
        req_aux_create_account,
        req_aux_reactivate_account,
        req_aux_deactivate_account,
        req_max
    };

    /** Response types that will be reported back to the calling app. */
    typedef enum vx_response_type {
        resp_none=0,
        resp_connector_create,
        resp_connector_initiate_shutdown,
        resp_account_login,
        resp_account_logout,
        resp_account_set_login_properties,
        resp_sessiongroup_create,
        resp_sessiongroup_terminate,
        resp_sessiongroup_add_session,
        resp_sessiongroup_remove_session,
        resp_sessiongroup_set_focus,
        resp_sessiongroup_unset_focus,
        resp_sessiongroup_reset_focus,
        resp_sessiongroup_set_tx_session,
        resp_sessiongroup_set_tx_all_sessions,
        resp_sessiongroup_set_tx_no_session,
        resp_session_create,
        resp_session_connect,
        resp_session_media_connect,
        resp_session_media_disconnect,
		resp_session_terminate,
        resp_session_mute_local_speaker,
        resp_session_set_local_speaker_volume,
        resp_session_get_local_audio_info,
        resp_session_channel_invite_user,
        resp_session_set_participant_volume_for_me,
        resp_session_set_participant_mute_for_me,
        resp_session_set_3d_position,
        resp_session_render_audio_start,
        resp_session_render_audio_stop,
        resp_session_channel_get_participants,
        resp_account_channel_get_list,
		resp_account_channel_create,
		resp_account_channel_update,
		resp_account_channel_delete,
		resp_account_channel_create_and_invite,
		resp_account_channel_folder_create,
		resp_account_channel_folder_update,
		resp_account_channel_folder_delete,
        resp_account_channel_folder_get_info,
        resp_account_channel_favorites_get_list,
        resp_account_channel_favorite_set,
        resp_account_channel_favorite_delete,
        resp_account_channel_favorite_group_set,
        resp_account_channel_favorite_group_delete,
        resp_account_channel_get_info,
        resp_account_channel_search,
        resp_account_buddy_search,
		resp_account_channel_add_moderator,
		resp_account_channel_remove_moderator,
        resp_account_channel_get_moderators,
		resp_account_channel_add_acl,
		resp_account_channel_remove_acl,
        resp_account_channel_get_acl,
        resp_channel_mute_user,
		resp_channel_ban_user,
        resp_channel_get_banned_users,
		resp_channel_kick_user,
		resp_channel_mute_all_users,
		resp_connector_mute_local_mic,
        resp_connector_mute_local_speaker,
		resp_connector_set_local_mic_volume,
		resp_connector_set_local_speaker_volume,
        resp_connector_get_local_audio_info,
        resp_account_buddy_set,
        resp_account_buddy_delete,
        resp_account_buddygroup_set,
        resp_account_buddygroup_delete,
        resp_account_list_buddies_and_groups,
        resp_session_send_message,
        resp_account_set_presence,
        resp_account_send_subscription_reply,
        resp_session_send_notification,
        resp_account_create_block_rule,
        resp_account_delete_block_rule,
        resp_account_list_block_rules,
        resp_account_create_auto_accept_rule,
        resp_account_delete_auto_accept_rule,
        resp_account_list_auto_accept_rules,
        resp_account_update_account,
        resp_account_get_account,
        resp_account_send_sms,
        resp_aux_connectivity_info,
        resp_aux_get_render_devices,
        resp_aux_get_capture_devices,
        resp_aux_set_render_device,
        resp_aux_set_capture_device,
        resp_aux_get_mic_level,
        resp_aux_get_speaker_level,
        resp_aux_set_mic_level,
        resp_aux_set_speaker_level,
        resp_aux_render_audio_start,
        resp_aux_render_audio_stop,
        resp_aux_capture_audio_start,
        resp_aux_capture_audio_stop,
        resp_aux_global_monitor_keyboard_mouse,
        resp_aux_set_idle_timeout,
        resp_aux_create_account,
        resp_aux_reactivate_account,
        resp_aux_deactivate_account,
        resp_max
    };
    
    /** Event types that will be reported back to the calling app. */
    typedef enum vx_event_type {
        evt_none=0,
        evt_login_state_change,             //DEPRECATED, legacy mode only
        evt_account_login_state_change,
        evt_session_new,                    //DEPRECATED, legacy mode only
        evt_session_state_change,           //DEPRECATED, legacy mode only
        evt_participant_state_change,       //DEPRECATED, legacy mode only
		evt_participant_properties,         //DEPRECATED, legacy mode only
        evt_buddy_presence,
        evt_subscription,
        evt_session_notification,
        evt_message,
        evt_aux_audio_properties,
        evt_participant_list,               //DEPRECATED, legacy mode only
        evt_session_participant_list,
        evt_session_media,                  //DEPRECATED, legacy mode only
        evt_buddy_changed,
        evt_buddy_group_changed,
		evt_buddy_and_group_list_changed,
        evt_keyboard_mouse,
        evt_idle_state_changed,
        evt_media_stream_updated,
        evt_text_stream_updated,
        evt_sessiongroup_added,
        evt_sessiongroup_removed,
        evt_session_added,
        evt_session_removed,
        evt_participant_added,
        evt_participant_removed,
        evt_participant_updated,
        evt_max
    };
    
	typedef struct vx_req_base {
		vx_message_base_t message;
		vx_request_type type;
		VX_COOKIE cookie;
        void *vcookie;
	} vx_req_base_t;

	typedef struct vx_resp_base {
		vx_message_base_t message;
		vx_response_type type;
		int return_code;
		int status_code;
		char* status_string;
		vx_req_base_t* request;
	} vx_resp_base_t;

	typedef struct vx_evt_base {
		vx_message_base_t message;
		vx_event_type type;
	} vx_evt_base_t;

    typedef enum {
        ND_E_NO_ERROR = 0,
        ND_E_TEST_NOT_RUN,
        ND_E_NO_INTERFACE,
        ND_E_NO_INTERFACE_WITH_GATEWAY,
        ND_E_NO_INTERFACE_WITH_ROUTE,
        ND_E_TIMEOUT,
        ND_E_CANT_ICMP,
        ND_E_CANT_RESOLVE_VIVOX_UDP_SERVER,
        ND_E_CANT_RESOLVE_ROOT_DNS_SERVER,
        ND_E_CANT_CONVERT_LOCAL_IP_ADDRESS,
        ND_E_CANT_CONTACT_STUN_SERVER_ON_UDP_PORT_3478,
        ND_E_CANT_CREATE_TCP_SOCKET,
        ND_E_CANT_LOAD_ICMP_LIBRARY,
        ND_E_CANT_FIND_SENDECHO2_PROCADDR,
        ND_E_CANT_CONNECT_TO_ECHO_SERVER,
        ND_E_ECHO_SERVER_LOGIN_SEND_FAILED,
        ND_E_ECHO_SERVER_LOGIN_RECV_FAILED,
        ND_E_ECHO_SERVER_LOGIN_RESPONSE_MISSING_STATUS,
        ND_E_ECHO_SERVER_LOGIN_RESPONSE_FAILED_STATUS,
        ND_E_ECHO_SERVER_LOGIN_RESPONSE_MISSING_SESSIONID,
        ND_E_ECHO_SERVER_LOGIN_RESPONSE_MISSING_SIPPORT,
        ND_E_ECHO_SERVER_LOGIN_RESPONSE_MISSING_AUDIORTP,
        ND_E_ECHO_SERVER_LOGIN_RESPONSE_MISSING_AUDIORTCP,
        ND_E_ECHO_SERVER_LOGIN_RESPONSE_MISSING_VIDEORTP,
        ND_E_ECHO_SERVER_LOGIN_RESPONSE_MISSING_VIDEORTCP,
        ND_E_ECHO_SERVER_CANT_ALLOCATE_SIP_SOCKET,
        ND_E_ECHO_SERVER_CANT_ALLOCATE_MEDIA_SOCKET,
        ND_E_ECHO_SERVER_SIP_UDP_SEND_FAILED,
        ND_E_ECHO_SERVER_SIP_UDP_RECV_FAILED,
        ND_E_ECHO_SERVER_SIP_TCP_SEND_FAILED,
        ND_E_ECHO_SERVER_SIP_TCP_RECV_FAILED,
        ND_E_ECHO_SERVER_SIP_NO_UDP_OR_TCP,
        ND_E_ECHO_SERVER_SIP_NO_UDP,
        ND_E_ECHO_SERVER_SIP_NO_TCP,
        ND_E_ECHO_SERVER_SIP_MALFORMED_TCP_PACKET,
        ND_E_ECHO_SERVER_SIP_UDP_DIFFERENT_LENGTH,
        ND_E_ECHO_SERVER_SIP_UDP_DATA_DIFFERENT,
        ND_E_ECHO_SERVER_SIP_TCP_PACKETS_DIFFERENT,
        ND_E_ECHO_SERVER_SIP_TCP_PACKETS_DIFFERENT_SIZE,
        ND_E_ECHO_SERVER_LOGIN_RECV_FAILED_TIMEOUT,
        ND_E_ECHO_SERVER_TCP_SET_ASYNC_FAILED,
        ND_E_ECHO_SERVER_UDP_SET_ASYNC_FAILED,
        ND_E_ECHO_SERVER_CANT_RESOLVE_NAME
    } ND_ERROR;

    typedef enum {
        ND_TEST_LOCATE_INTERFACE,
        ND_TEST_PING_GATEWAY,
        ND_TEST_DNS,
        ND_TEST_STUN,
        ND_TEST_ECHO,
        ND_TEST_ECHO_SIP_FIRST_PORT,
        ND_TEST_ECHO_SIP_FIRST_PORT_INVITE_REQUEST,
        ND_TEST_ECHO_SIP_FIRST_PORT_INVITE_RESPONSE,
        ND_TEST_ECHO_SIP_FIRST_PORT_REGISTER_REQUEST,
        ND_TEST_ECHO_SIP_FIRST_PORT_REGISTER_RESPONSE,
        ND_TEST_ECHO_SIP_SECOND_PORT,
        ND_TEST_ECHO_SIP_SECOND_PORT_INVITE_REQUEST,
        ND_TEST_ECHO_SIP_SECOND_PORT_INVITE_RESPONSE,
        ND_TEST_ECHO_SIP_SECOND_PORT_REGISTER_REQUEST,
        ND_TEST_ECHO_SIP_SECOND_PORT_REGISTER_RESPONSE,
        ND_TEST_ECHO_MEDIA,
        ND_TEST_ECHO_MEDIA_LARGE_PACKET
    } ND_TEST_TYPE;

    typedef enum vx_session_answer_mode {
        mode_none=0,
        mode_auto_answer=1,
        mode_verify_answer,
    };

    typedef enum vx_rule_type {
        rule_none=0,
        rule_allow,
        rule_block,
        rule_hide,
    };

    typedef enum vx_subscription_type {
        subscription_presence=0,
    };

    typedef enum vx_notification_type {
        notification_not_typing,
        notification_typing//,
        //state_closed_window, no longer valid in this enum
    };

    typedef enum vx_text_mode {
        text_mode_disabled = 0,
        text_mode_enabled,
    };

    typedef enum vx_audiosource_operation {
        op_none=0,
        op_safeupdate=1,
        op_delete,
    };

    typedef enum vx_aux_audio_properties_state {
        aux_audio_properties_none=0
    };

    typedef enum vx_login_state_change_state {
        login_state_logged_out=0,
        login_state_logged_in = 1,
        login_state_logging_in = 2,
        login_state_logging_out = 3,
        login_state_resetting = 4,
        login_state_error=100
    };

    typedef enum vx_session_new_state {
        session_new_none=0
    };

	typedef	enum vx_session_state_change_state {
		session_idle=1,
		session_answering,
		session_inprogress,
		session_connected,
		session_disconnected,
		session_hold,
		session_refer,
		session_ringing,
	};

	typedef	enum vx_participant_properties_state {
		participant_properties_none=0,
	};
	
	typedef	enum vx_participant_state_change_state {
		part_idle=1,
		part_pending,
		part_incoming,
		part_answering,
		part_inprogress,
		part_alerting,
		part_connected,
		part_disconnecting,
		part_disconnected,
        part_moderatormuted,
        part_moderatorunmuted,
	};

    typedef enum {
        buddy_presence_unknown=0,   // deprecated
        buddy_presence_pending=1,
        buddy_presence_online=2,
        buddy_presence_busy=3,
        buddy_presence_brb=4,
        buddy_presence_away=5,
        buddy_presence_onthephone=6,
        buddy_presence_outtolunch=7,
        buddy_presence_closed=0,    // deprecated
        buddy_presence_offline=0,   // deprecated
    } vx_buddy_presence_state;

    typedef enum vx_subscription_state {
        subscription_none=0
    };

    typedef enum vx_session_notification_state {
        session_notification_none=0
    };

    typedef enum vx_message_state {
        message_none=0
    };

    typedef enum vx_participant_list_state {
        participant_list_none=0
    };

    /*typedef enum vx_session_media_state {
        session_media_none=0
    };*/

    typedef enum vx_session_text_state {
        session_text_disconnected = 0,
        session_text_connected
    };

    typedef enum vx_session_media_state {
        session_media_none = 0,
        session_media_disconnected,
        session_media_connected,
		session_media_ringing,
		session_media_hold,
		session_media_refer
    };

    typedef enum vx_participant_type {
        part_user=0,
        part_moderator,
        part_focus,
    };

    typedef enum {
        orientation_default = 0,
        orientation_legacy = 1,
        orientation_vivox = 2
    } orientation_type;

    /** Channel participant. */
    typedef struct vx_participant {
		char* uri;
		char* first_name;
		char* last_name;
		char* display_name;
		char* username;
		int is_moderator;
        int is_moderator_muted;
        int is_muted_for_me;
    } vx_participant_t;
    VIVOXSDK_DLLEXPORT void vx_participant_create(vx_participant_t** participant);
    VIVOXSDK_DLLEXPORT void vx_participant_free(vx_participant_t* participant);

    /** Creates a participant list with the given size. */
    typedef vx_participant_t* vx_participant_ref_t;
    typedef vx_participant_ref_t* vx_participant_list_t;
    VIVOXSDK_DLLEXPORT void vx_participant_list_create(int size, vx_participant_list_t* list_out);
    VIVOXSDK_DLLEXPORT void vx_participant_list_free(vx_participant_t** list, int size);
    
    /** Channel struct. */
    typedef struct vx_channel {
        char* channel_name;
        char* channel_desc;
        char* host;
        int channel_id;
        int limit;
        int capacity;
        char* modified; /* FIXME: What is this? */
        char* owner;
        char* owner_user_name;
        int parent_id;
        int is_persistent; /* 1 true, <= 0 false */
        int is_protected; /* 1 true, <= 0 false */
        int size;
        int type; /* FIXME: What is this? */
        char* channel_uri;
        int max_range;
        int clamping_dist;
        double roll_off;
        double max_gain;
        int dist_model;
    } vx_channel_t;
    VIVOXSDK_DLLEXPORT void vx_channel_create(vx_channel_t** channel);
    VIVOXSDK_DLLEXPORT void vx_channel_free(vx_channel_t* channel);

    typedef vx_channel_t* vx_channel_ref_t;
    typedef vx_channel_ref_t* vx_channel_list_t;
    VIVOXSDK_DLLEXPORT void vx_channel_list_create(int size, vx_channel_list_t* list_out);
    VIVOXSDK_DLLEXPORT void vx_channel_list_free(vx_channel_t** list, int size);

    /** Channel Favorite struct. */
    typedef struct vx_channel_favorite {
        int favorite_id;
        int favorite_group_id;
        char* favorite_display_name;
        char* favorite_data;
        char* channel_uri;
        char* channel_description;
        int channel_limit;
        int channel_capacity;
        char* channel_modified; /* FIXME: What is this? */
        char* channel_owner_user_name;
        int channel_is_persistent; /* 1 true, <= 0 false */
        int channel_is_protected; /* 1 true, <= 0 false */
        int channel_size;
    } vx_channel_favorite_t;
    VIVOXSDK_DLLEXPORT void vx_channel_favorite_create(vx_channel_favorite_t** channel);
    VIVOXSDK_DLLEXPORT void vx_channel_favorite_free(vx_channel_favorite_t* channel);

    typedef vx_channel_favorite_t* vx_channel_favorite_ref_t;
    typedef vx_channel_favorite_ref_t* vx_channel_favorite_list_t;
    VIVOXSDK_DLLEXPORT void vx_channel_favorite_list_create(int size, vx_channel_favorite_list_t* list_out);
    VIVOXSDK_DLLEXPORT void vx_channel_favorite_list_free(vx_channel_favorite_t** list, int size);

    /** Channel Favorite Group struct. */
    typedef struct vx_channel_favorite_group {
        int favorite_group_id;
        char* favorite_group_name;
        char* favorite_group_data;
        char* favorite_group_modified;
    } vx_channel_favorite_group_t;
    VIVOXSDK_DLLEXPORT void vx_channel_favorite_group_create(vx_channel_favorite_group_t** channel);
    VIVOXSDK_DLLEXPORT void vx_channel_favorite_group_free(vx_channel_favorite_group_t* channel);

    typedef vx_channel_favorite_group_t* vx_channel_favorite_group_ref_t;
    typedef vx_channel_favorite_group_ref_t* vx_channel_favorite_group_list_t;
    VIVOXSDK_DLLEXPORT void vx_channel_favorite_group_list_create(int size, vx_channel_favorite_group_list_t* list_out);
    VIVOXSDK_DLLEXPORT void vx_channel_favorite_group_list_free(vx_channel_favorite_group_t** list, int size);

    VIVOXSDK_DLLEXPORT void vx_string_list_create(int size, char *** list_out);
    VIVOXSDK_DLLEXPORT void vx_string_list_free(char ** list);

    typedef struct vx_block_rule {
        char* block_mask;
        int presence_only;
    } vx_block_rule_t;
    VIVOXSDK_DLLEXPORT void vx_block_rule_create(vx_block_rule_t** block_rule);
    VIVOXSDK_DLLEXPORT void vx_block_rule_free(vx_block_rule_t* block_rule);

    typedef vx_block_rule_t* vx_block_rule_ref_t;
    typedef vx_block_rule_ref_t* vx_block_rules_t;
    VIVOXSDK_DLLEXPORT void vx_block_rules_create(int size, vx_block_rules_t* list_out);
    VIVOXSDK_DLLEXPORT void vx_block_rules_free(vx_block_rule_t** list, int size);

    typedef struct vx_auto_accept_rule {
        char* auto_accept_mask;
        int auto_add_as_buddy;
    } vx_auto_accept_rule_t;
    VIVOXSDK_DLLEXPORT void vx_auto_accept_rule_create(vx_auto_accept_rule_t** auto_accept_rule);
    VIVOXSDK_DLLEXPORT void vx_auto_accept_rule_free(vx_auto_accept_rule_t* auto_accept_rule);

    typedef vx_auto_accept_rule_t* vx_auto_accept_rule_ref_t;
    typedef vx_auto_accept_rule_ref_t* vx_auto_accept_rules_t;
    VIVOXSDK_DLLEXPORT void vx_auto_accept_rules_create(int size, vx_auto_accept_rules_t* list_out);
    VIVOXSDK_DLLEXPORT void vx_auto_accept_rules_free(vx_auto_accept_rule_t** list, int size);

    typedef struct vx_connectivity_test_result {
        ND_TEST_TYPE test_type;
        ND_ERROR test_error_code;
        char* test_additional_info;
    } vx_connectivity_test_result_t;

    VIVOXSDK_DLLEXPORT void vx_connectivity_test_result_create(vx_connectivity_test_result_t** connectivity_test_result, ND_TEST_TYPE tt);
    VIVOXSDK_DLLEXPORT void vx_connectivity_test_result_free(vx_connectivity_test_result_t* connectivity_test_result);

    typedef vx_connectivity_test_result_t* vx_connectivity_test_result_ref_t;
    typedef vx_connectivity_test_result_ref_t* vx_connectivity_test_results_t;
    VIVOXSDK_DLLEXPORT void vx_connectivity_test_results_create(int size, vx_connectivity_test_results_t* list_out);
    VIVOXSDK_DLLEXPORT void vx_connectivity_test_results_free(vx_connectivity_test_result_t** list, int size);

    typedef struct vx_account {
        char* uri;
        char* firstname;
        char* lastname;
        char* username;
        char* displayname;
        char* email;
        char* phone;
        char* carrier;
    } vx_account_t;
    VIVOXSDK_DLLEXPORT void vx_account_create(vx_account_t** account);
    VIVOXSDK_DLLEXPORT void vx_account_free(vx_account_t* account);

    typedef struct vx_device {
        char* device;
    } vx_device_t;
    VIVOXSDK_DLLEXPORT void vx_device_create(vx_device_t** device);
    VIVOXSDK_DLLEXPORT void vx_device_free(vx_device_t* device);

    typedef vx_device_t* vx_device_ref_t;
    typedef vx_device_ref_t* vx_devices_t;
    VIVOXSDK_DLLEXPORT void vx_devices_create(int size, vx_devices_t* list_out);
    VIVOXSDK_DLLEXPORT void vx_devices_free(vx_device_t** list, int size);

    typedef struct vx_buddy {
        char* buddy_uri;
        char* display_name;
        int parent_group_id;
        char* buddy_data;
    } vx_buddy_t;
    VIVOXSDK_DLLEXPORT void vx_buddy_create(vx_buddy_t** buddy);
    VIVOXSDK_DLLEXPORT void vx_buddy_free(vx_buddy_t* buddy);

    typedef vx_buddy_t* vx_buddy_ref_t;
    typedef vx_buddy_ref_t* vx_buddy_list_t;
    VIVOXSDK_DLLEXPORT void vx_buddy_list_create(int size, vx_buddy_list_t* list_out);
    VIVOXSDK_DLLEXPORT void vx_buddy_list_free(vx_buddy_t** list, int size);

    typedef struct vx_group {
        int group_id;
        char* group_name;
        char* group_data;
    } vx_group_t;
    VIVOXSDK_DLLEXPORT void vx_group_create(vx_group_t** group);
    VIVOXSDK_DLLEXPORT void vx_group_free(vx_group_t* group);

    typedef vx_group_t* vx_group_ref_t;
    typedef vx_group_ref_t* vx_group_list_t;
    VIVOXSDK_DLLEXPORT void vx_group_list_create(int size, vx_group_list_t* list_out);
    VIVOXSDK_DLLEXPORT void vx_group_list_free(vx_group_t** list, int size);

    /* Vivox SDK functions */

    VIVOXSDK_DLLEXPORT char* vx_strdup(const char*);
    VIVOXSDK_DLLEXPORT void vx_free(char*);

    /**
     * The VxSDK polling function.  Should be called periodically to check for any incoming events.  
     *
     * @param message           [out] The object containing the message data.
     * @return                  Status of the poll, 0 = Success, 1 = Failure, -1 = No Mesasge Available
     */
    VIVOXSDK_DLLEXPORT int vx_get_message(vx_message_base_t** message);

    /** 
     * Execute the given request. 
     * 
     * @param request           The request object to execute.  This is of one of the vx_req_* structs. 
     * @return                  Success status of the request.
     */
    VIVOXSDK_DLLEXPORT int vx_issue_request(vx_req_base_t* request);
    
    /**
     * Get the XML for the given request.
     *
     * @param request           The request object.
     * @param xml               [out] The xml string.
     * @return                  XML string.
     */
     VIVOXSDK_DLLEXPORT void vx_request_to_xml(void* request, char** xml);
    
    /**
     * Get a request for the given XML string.
     * 
     * @param xml               XML string.
     * @param request           [out] The request struct.
     * @param error             [out] XML parse error string (if any error occurs).  NULL otherwise.
     * @return                  The request struct type.  req_none is returned if no struct could be created from the XML.
     */
    VIVOXSDK_DLLEXPORT vx_request_type vx_xml_to_request(const char* xml, void** request, char** error=0);
    
    /**
     * Get the XML for the given response.
     *
     * @param response           The response object.
     * @param xml               [out] The xml string.
     */
    VIVOXSDK_DLLEXPORT void vx_response_to_xml(void* response, char** xml);
    
    /**
     * Get a response for the given XML string.
     * 
     * @param xml               XML string.
     * @param response          [out] The response struct.
     * @param error             [out] XML parse error string (if any error occurs).  NULL otherwise.
     * @return                  The response struct type.  resp_none is returned if no struct could be created from the XML.
     */
    VIVOXSDK_DLLEXPORT vx_response_type vx_xml_to_response(const char* xml, void** response, char** error = 0);
    
    /**
     * Get the XML for the given event.
     *
     * @param event           The event object.
     * @param xml               [out] The xml string.
     */
    VIVOXSDK_DLLEXPORT void vx_event_to_xml(void* event, char** xml);
    
    /**
     * Get a event for the given XML string.
     * 
     * @param xml               XML string.
     * @param event          [out] The event struct.
     * @param error             [out] XML parse error string (if any error occurs).  NULL otherwise.
     * @return                  The event struct type.  req_none is returned if no struct could be created from the XML.
     */
    VIVOXSDK_DLLEXPORT vx_event_type vx_xml_to_event(const char* xml, void** event, char** error = 0);
    
    /**
     * Determine whether the XML refers to a request, response, or event.
     */
    VIVOXSDK_DLLEXPORT vx_message_type vx_get_message_type(const char* xml);

    /**
     * Get Millisecond Counter
     */
    VIVOXSDK_DLLEXPORT unsigned long long vx_get_time_ms();

    /**
     * Register a callback that will be called when a message is placed on the queue.
     * The application should use this to signal the main application thread that will then wakeup and call vx_get_message;
     */
    VIVOXSDK_DLLEXPORT void vx_register_message_notification_handler(void (* pf_handler)(void *), void *cookie);

    /**
     * Unregister a notification handler
     */
    VIVOXSDK_DLLEXPORT void vx_unregister_message_notification_handler(void (* pf_handler)(void *), void *cookie);

    VIVOXSDK_DLLEXPORT int vx_create_account(const char* acct_mgmt_server, const char* admin_name, const char* admin_pw, const char* uname, const char* pw);

#ifdef __cplusplus
}
#endif


#endif /* ndef __VXC_H__ */

