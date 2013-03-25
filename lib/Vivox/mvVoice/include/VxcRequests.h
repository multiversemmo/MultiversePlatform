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

#ifndef __VXCREQUESTS_H__
#define __VXCREQUESTS_H__

#include "Vxc.h"


#ifdef __cplusplus
extern "C" {
#endif
    /* The Vivox request structs. These structs all contain a vx_request action in the beginning so they can be identified, and
    have a cookie (VX_COOKIE). */

typedef struct vx_req_connector_create {
    vx_req_base_t base;
    char* client_name;
    char* acct_mgmt_server;
    int minimum_port;
    int maximum_port;
    vx_attempt_stun attempt_stun;
    vx_connector_mode mode;
    char* log_folder;
    char* log_filename_prefix;
    char* log_filename_suffix;
    int log_level; // <= 0 to turn off
} vx_req_connector_create_t;
VIVOXSDK_DLLEXPORT void vx_req_connector_create_create(vx_req_connector_create_t ** req);

typedef struct vx_req_connector_initiate_shutdown {
    vx_req_base_t base;
    VX_HANDLE connector_handle;
    char* client_name;
} vx_req_connector_initiate_shutdown_t;
VIVOXSDK_DLLEXPORT void vx_req_connector_initiate_shutdown_create(vx_req_connector_initiate_shutdown_t ** req);

typedef struct vx_req_account_login {
    vx_req_base_t base;
    VX_HANDLE connector_handle;
    char* acct_name;
    char* acct_password;
    vx_session_answer_mode answer_mode;
    vx_text_mode enable_text;
    int participant_property_frequency;
    int enable_buddies_and_presence;
//  VxAudioEvtUpdFreq participant_property_frequency;
} vx_req_account_login_t;
VIVOXSDK_DLLEXPORT void vx_req_account_login_create(vx_req_account_login_t ** req);

typedef struct vx_req_account_logout {
    vx_req_base_t base;
    VX_HANDLE account_handle;
} vx_req_account_logout_t;
VIVOXSDK_DLLEXPORT void vx_req_account_logout_create(vx_req_account_logout_t ** req);

typedef struct vx_req_account_set_login_properties {
    vx_req_base_t base;
    VX_HANDLE account_handle;
    vx_session_answer_mode answer_mode;
    int participant_property_frequency;
} vx_req_account_set_login_properties_t;
VIVOXSDK_DLLEXPORT void vx_req_account_set_login_properties_create(vx_req_account_set_login_properties ** req);

typedef enum {
    password_hash_algorithm_cleartext,
    password_hash_algorithm_sha1_username_hash,
} vx_password_hash_algorithm_t;

typedef struct vx_req_sessiongroup_create {
    vx_req_base_t base;
    VX_HANDLE account_handle;
} vx_req_sessiongroup_create_t;
VIVOXSDK_DLLEXPORT void vx_req_sessiongroup_create_create(vx_req_sessiongroup_create_t ** req);

typedef struct vx_req_sessiongroup_terminate {
    vx_req_base_t base;
    VX_HANDLE sessiongroup_handle;
} vx_req_sessiongroup_terminate_t;
VIVOXSDK_DLLEXPORT void vx_req_sessiongroup_terminate_create(vx_req_sessiongroup_terminate_t ** req);

typedef struct vx_req_sessiongroup_add_session {
    vx_req_base_t base;
    VX_HANDLE sessiongroup_handle;
    char* uri;
	char* name;
    char* password;
	int connect_audio;
    vx_password_hash_algorithm_t password_hash_algorithm;
} vx_req_sessiongroup_add_session_t;
VIVOXSDK_DLLEXPORT void vx_req_sessiongroup_add_session_create(vx_req_sessiongroup_add_session_t ** req);

typedef struct vx_req_sessiongroup_remove_session {
    vx_req_base_t base;
    VX_HANDLE session_handle;
    VX_HANDLE sessiongroup_handle;
} vx_req_sessiongroup_remove_session_t;
VIVOXSDK_DLLEXPORT void vx_req_sessiongroup_remove_session_create(vx_req_sessiongroup_remove_session_t ** req);

typedef struct vx_req_sessiongroup_set_focus {
    vx_req_base_t base;
    VX_HANDLE session_handle;
    VX_HANDLE sessiongroup_handle;
} vx_req_sessiongroup_set_focus_t;
VIVOXSDK_DLLEXPORT void vx_req_sessiongroup_set_focus_create(vx_req_sessiongroup_set_focus_t ** req);

typedef struct vx_req_sessiongroup_unset_focus {
    vx_req_base_t base;
    VX_HANDLE session_handle;
    VX_HANDLE sessiongroup_handle;
} vx_req_sessiongroup_unset_focus_t;
VIVOXSDK_DLLEXPORT void vx_req_sessiongroup_unset_focus_create(vx_req_sessiongroup_unset_focus_t ** req);

typedef struct vx_req_sessiongroup_reset_focus {
    vx_req_base_t base;
    VX_HANDLE sessiongroup_handle;
} vx_req_sessiongroup_reset_focus_t;
VIVOXSDK_DLLEXPORT void vx_req_sessiongroup_reset_focus_create(vx_req_sessiongroup_reset_focus_t ** req);

typedef struct vx_req_sessiongroup_set_tx_session {
    vx_req_base_t base;
    VX_HANDLE session_handle;
    VX_HANDLE sessiongroup_handle;
} vx_req_sessiongroup_set_tx_session_t;
VIVOXSDK_DLLEXPORT void vx_req_sessiongroup_set_tx_session_create(vx_req_sessiongroup_set_tx_session_t ** req);

typedef struct vx_req_sessiongroup_set_tx_all_sessions {
    vx_req_base_t base;
    VX_HANDLE sessiongroup_handle;
} vx_req_sessiongroup_set_tx_all_sessions_t;
VIVOXSDK_DLLEXPORT void vx_req_sessiongroup_set_tx_all_sessions_create(vx_req_sessiongroup_set_tx_all_sessions_t ** req);

typedef struct vx_req_sessiongroup_set_tx_no_session {
    vx_req_base_t base;
    VX_HANDLE sessiongroup_handle;
} vx_req_sessiongroup_set_tx_no_session_t;
VIVOXSDK_DLLEXPORT void vx_req_sessiongroup_set_tx_no_session_create(vx_req_sessiongroup_set_tx_no_session ** req);

typedef struct vx_req_session_create {
    vx_req_base_t base;
    VX_HANDLE account_handle;
    char* name;
    char* uri;
    char* password;
    int connect_audio;
    int join_audio; // DEPRECATED  1 true, <= 0 false
    int join_text;  // DEPRECATED  1 true, <= 0 false
    vx_password_hash_algorithm_t password_hash_algorithm;
} vx_req_session_create_t;
VIVOXSDK_DLLEXPORT void vx_req_session_create_create(vx_req_session_create_t ** req);

typedef struct vx_req_session_connect {     //DEPRECATED
    vx_req_base_t base;
    VX_HANDLE session_handle;
    char* audio_media;
} vx_req_session_connect_t;
VIVOXSDK_DLLEXPORT void vx_req_session_connect_create(vx_req_session_connect_t ** req);

typedef struct vx_req_session_media_connect {
    vx_req_base_t base;
    VX_HANDLE sessiongroup_handle;
    VX_HANDLE session_handle;
    vx_media_type media;        //DEPRECATED
} vx_req_session_media_connect_t;
VIVOXSDK_DLLEXPORT void vx_req_session_media_connect_create(vx_req_session_media_connect_t ** req);

typedef struct vx_req_session_media_disconnect {
    vx_req_base_t base;
    VX_HANDLE sessiongroup_handle;
    VX_HANDLE session_handle;
    vx_media_type media;        //DEPRECATED
} vx_req_session_media_disconnect_t;
VIVOXSDK_DLLEXPORT void vx_req_session_media_disconnect_create(vx_req_session_media_disconnect_t ** req);

typedef struct vx_req_session_terminate {
    vx_req_base_t base;
    VX_HANDLE session_handle;
} vx_req_session_terminate_t;
VIVOXSDK_DLLEXPORT void vx_req_session_terminate_create(vx_req_session_terminate_t ** req);

typedef struct vx_req_session_mute_local_speaker {
    vx_req_base_t base;
    VX_HANDLE session_handle;
    int mute_level;
} vx_req_session_mute_local_speaker_t;
VIVOXSDK_DLLEXPORT void vx_req_session_mute_local_speaker_create(vx_req_session_mute_local_speaker_t ** req);

typedef struct vx_req_session_set_local_speaker_volume {
    vx_req_base_t base;
    VX_HANDLE session_handle;
    int volume;
} vx_req_session_set_local_speaker_volume_t;
VIVOXSDK_DLLEXPORT void vx_req_session_set_local_speaker_volume_create(vx_req_session_set_local_speaker_volume_t ** req);

typedef struct vx_req_session_get_local_audio_info {
    vx_req_base_t base;
    VX_HANDLE session_handle;
} vx_req_session_get_local_audio_info_t;
VIVOXSDK_DLLEXPORT void vx_req_session_get_local_audio_info_create(vx_req_session_get_local_audio_info_t ** req);

typedef struct vx_req_session_channel_invite_user {
    vx_req_base_t base;
    VX_HANDLE session_handle;
    char* participant_uri;
} vx_req_session_channel_invite_user_t;
VIVOXSDK_DLLEXPORT void vx_req_session_channel_invite_user_create(vx_req_session_channel_invite_user_t ** req);

typedef struct vx_req_session_set_participant_volume_for_me {
    vx_req_base_t base;
    VX_HANDLE session_handle;
    char* participant_uri;
    int volume;
} vx_req_session_set_participant_volume_for_me_t;
VIVOXSDK_DLLEXPORT void vx_req_session_set_participant_volume_for_me_create(vx_req_session_set_participant_volume_for_me_t ** req);

typedef struct vx_req_session_set_participant_mute_for_me {
    vx_req_base_t base;
    VX_HANDLE session_handle;
    char* participant_uri;
    int mute;
} vx_req_session_set_participant_mute_for_me_t;
VIVOXSDK_DLLEXPORT void vx_req_session_set_participant_mute_for_me_create(vx_req_session_set_participant_mute_for_me_t ** req);

typedef enum {
	req_disposition_reply_required,
	req_disposition_no_reply_required
} req_disposition_type_t;

typedef struct vx_req_session_set_3d_position {
    vx_req_base_t base;
    VX_HANDLE session_handle;
    double speaker_position[3]; // {x, y, z}
    double speaker_velocity[3]; // {x, y, z}
    double speaker_at_orientation[3]; // {x, y, z}
    double speaker_up_orientation[3]; // {x, y, z}
    double speaker_left_orientation[3]; // {x, y, z}
    double listener_position[3]; // {x, y, z}
    double listener_velocity[3]; // {x, y, z}
    double listener_at_orientation[3]; // {x, y, z}
    double listener_up_orientation[3]; // {x, y, z}
    double listener_left_orientation[3]; // {x, y, z}
    orientation_type type;
	req_disposition_type_t req_disposition_type;
} vx_req_session_set_3d_position_t;
VIVOXSDK_DLLEXPORT void vx_req_session_set_3d_position_create(vx_req_session_set_3d_position_t ** req);

typedef struct vx_req_session_render_audio_start {
    vx_req_base_t base;
    VX_HANDLE session_handle;
    char* sound_file_path;
    int loop;
} vx_req_session_render_audio_start_t;
VIVOXSDK_DLLEXPORT void vx_req_session_render_audio_start_create(vx_req_session_render_audio_start_t ** req);

typedef struct vx_req_session_render_audio_stop {
    vx_req_base_t base;
    VX_HANDLE session_handle;
    char* sound_file_path;
} vx_req_session_render_audio_stop_t;
VIVOXSDK_DLLEXPORT void vx_req_session_render_audio_stop_create(vx_req_session_render_audio_stop_t ** req);

typedef struct vx_req_session_channel_get_participants {
    vx_req_base_t base;
    VX_HANDLE session_handle;
} vx_req_session_channel_get_participants_t;
VIVOXSDK_DLLEXPORT void vx_req_session_channel_get_participants_create(vx_req_session_channel_get_participants_t ** req);

typedef struct vx_req_account_channel_get_list {
    vx_req_base_t base;
    VX_HANDLE account_handle;
} vx_req_account_channel_get_list_t;
VIVOXSDK_DLLEXPORT void vx_req_account_channel_get_list_create(vx_req_account_channel_get_list_t ** req);

typedef struct vx_req_account_channel_create {
    vx_req_base_t base;
    VX_HANDLE account_handle;
    char* channel_name;
    char* channel_desc;
    vx_channel_type channel_type;
    int parent_id;
    int set_persistent; // 1 true, <= 0 false
    int set_protected;  // 1 true, <= 0 false
    char* protected_password;
    int capacity;
    int max_participants;
    int max_range;
    int clamping_dist;
    double roll_off;
    double max_gain;
    int dist_model;
} vx_req_account_channel_create_t;
VIVOXSDK_DLLEXPORT void vx_req_account_channel_create_create(vx_req_account_channel_create_t ** req);

typedef struct vx_req_account_channel_update {
    vx_req_base_t base;
    VX_HANDLE account_handle;
    char* channel_uri;
    char* channel_name;
    char* channel_desc;
    int set_persistent;
    int set_protected;
    char* protected_password;
    int capacity;
    int max_participants;
    int max_range;
    int clamping_dist;
    double roll_off;
    double max_gain;
    int dist_model;
} vx_req_account_channel_update_t;
VIVOXSDK_DLLEXPORT void vx_req_account_channel_update_create(vx_req_account_channel_update_t ** req);

typedef struct vx_req_account_channel_delete {
    vx_req_base_t base;
    VX_HANDLE account_handle;
    char* channel_uri;
} vx_req_account_channel_delete_t;
VIVOXSDK_DLLEXPORT void vx_req_account_channel_delete_create(vx_req_account_channel_delete_t ** req);

typedef struct vx_req_account_channel_create_and_invite {
    vx_req_base_t base;
    VX_HANDLE account_handle;
    char* channel_name;
    char* channel_desc;
    int parent_id;
    int is_persistent;  // 1 true, <= 0 false
    int is_protected;   // 1 true, <= 0 false
    char* protected_password;
    int capacity;
    int max_participants;
    char** participant_uris;
} vx_req_account_channel_create_and_invite_t;
VIVOXSDK_DLLEXPORT void vx_req_account_channel_create_and_invite_create(vx_req_account_channel_create_and_invite_t ** req);

typedef struct vx_req_account_channel_folder_create {
    vx_req_base_t base;
    VX_HANDLE account_handle;
    char* folder_name;
    char* folder_desc;
    int parent_id;
    int set_persistent; // 1 true, <= 0 false
    int set_protected;  // 1 true, <= 0 false
    char* protected_password;
} vx_req_account_channel_folder_create_t;
VIVOXSDK_DLLEXPORT void vx_req_account_channel_folder_create_create(vx_req_account_channel_folder_create_t ** req);

typedef struct vx_req_account_channel_folder_update {
    vx_req_base_t base;
    VX_HANDLE account_handle;
    int folder_id;
    char* folder_name;
    char* folder_desc;
    int set_persistent; // 1 true, <= 0 false
    int set_protected;  // 1 true, <= 0 false
    char* protected_password;
} vx_req_account_channel_folder_update_t;
VIVOXSDK_DLLEXPORT void vx_req_account_channel_folder_update_create(vx_req_account_channel_folder_update_t ** req);

typedef struct vx_req_account_channel_folder_delete {
    vx_req_base_t base;
    VX_HANDLE account_handle;
    int folder_id;
} vx_req_account_channel_folder_delete_t;
VIVOXSDK_DLLEXPORT void vx_req_account_channel_folder_delete_create(vx_req_account_channel_folder_delete_t ** req);

typedef struct vx_req_account_channel_folder_get_info {
    vx_req_base_t base;
    VX_HANDLE account_handle;
    int folder_id;
} vx_req_account_channel_folder_get_info_t;
VIVOXSDK_DLLEXPORT void vx_req_account_channel_folder_get_info_create(vx_req_account_channel_folder_get_info_t ** req);

typedef struct vx_req_account_channel_favorites_get_list {
    vx_req_base_t base;
    VX_HANDLE account_handle;
} vx_req_account_channel_favorites_get_list_t;
VIVOXSDK_DLLEXPORT void vx_req_account_channel_favorites_get_list_create(vx_req_account_channel_favorites_get_list_t ** req);

typedef struct vx_req_account_channel_favorite_set {
    vx_req_base_t base;
    VX_HANDLE account_handle;
    int channel_favorite_id;
    char* channel_favorite_label;
    char* channel_favorite_uri;
    char* channel_favorite_data;
    int channel_favorite_group_id;
} vx_req_account_channel_favorite_set_t;
VIVOXSDK_DLLEXPORT void vx_req_account_channel_favorite_set_create(vx_req_account_channel_favorite_set_t ** req);

typedef struct vx_req_account_channel_favorite_delete {
    vx_req_base_t base;
    VX_HANDLE account_handle;
    int channel_favorite_id;
} vx_req_account_channel_favorite_delete_t;
VIVOXSDK_DLLEXPORT void vx_req_account_channel_favorite_delete_create(vx_req_account_channel_favorite_delete_t ** req);

typedef struct vx_req_account_channel_favorite_group_set {
    vx_req_base_t base;
    VX_HANDLE account_handle;
    int channel_favorite_group_id;
    char* channel_favorite_group_name;
    char* channel_favorite_group_data;
} vx_req_account_channel_favorite_group_set_t;
VIVOXSDK_DLLEXPORT void vx_req_account_channel_favorite_group_set_create(vx_req_account_channel_favorite_group_set_t ** req);

typedef struct vx_req_account_channel_favorite_group_delete {
    vx_req_base_t base;
    VX_HANDLE account_handle;
    int channel_favorite_group_id;
} vx_req_account_channel_favorite_group_delete_t;
VIVOXSDK_DLLEXPORT void vx_req_account_channel_favorite_group_delete_create(vx_req_account_channel_favorite_group_delete_t ** req);

typedef struct vx_req_account_channel_get_info {
    vx_req_base_t base;
    VX_HANDLE account_handle;
    char* channel_uri;
} vx_req_account_channel_get_info_t;
VIVOXSDK_DLLEXPORT void vx_req_account_channel_get_info_create(vx_req_account_channel_get_info_t ** req);

typedef struct vx_req_account_channel_search {
    vx_req_base_t base;
    VX_HANDLE account_handle;
    int page_number;
    int page_size;
    char* channel_name;
    char* channel_description;
    int channel_active;
    vx_channel_search_type channel_type;
} vx_req_account_channel_search_t;
VIVOXSDK_DLLEXPORT void vx_req_account_channel_search_create(vx_req_account_channel_search_t ** req);

typedef struct vx_req_account_buddy_search {
    vx_req_base_t base;
    VX_HANDLE account_handle;
    int page_number;
    int page_size;
    char* buddy_first_name;
    char* buddy_last_name;
    char* buddy_user_name;
    char* buddy_email;
} vx_req_account_buddy_search_t;
VIVOXSDK_DLLEXPORT void vx_req_account_buddy_search_create(vx_req_account_buddy_search_t ** req);

typedef struct vx_req_account_channel_add_moderator {
    vx_req_base_t base;
    VX_HANDLE account_handle;
    char* channel_uri;
    char* channel_name;
    char* moderator_uri;
} vx_req_account_channel_add_moderator_t;
VIVOXSDK_DLLEXPORT void vx_req_account_channel_add_moderator_create(vx_req_account_channel_add_moderator_t ** req);

typedef struct vx_req_account_channel_remove_moderator {
    vx_req_base_t base;
    VX_HANDLE account_handle;
    char* channel_uri;
    char* channel_name;
    char* moderator_uri;
} vx_req_account_channel_remove_moderator_t;
VIVOXSDK_DLLEXPORT void vx_req_account_channel_remove_moderator_create(vx_req_account_channel_remove_moderator_t ** req);

typedef struct vx_req_account_channel_get_moderators {
    vx_req_base_t base;
    VX_HANDLE account_handle;
    char* channel_uri;
} vx_req_account_channel_get_moderators_t;
VIVOXSDK_DLLEXPORT void vx_req_account_channel_get_moderators_create(vx_req_account_channel_get_moderators_t ** req);

typedef struct vx_req_account_channel_add_acl {
    vx_req_base_t base;
    VX_HANDLE account_handle;
    char* channel_uri;
    char* acl_uri;
} vx_req_account_channel_add_acl_t;
VIVOXSDK_DLLEXPORT void vx_req_account_channel_add_acl_create(vx_req_account_channel_add_acl_t ** req);

typedef struct vx_req_account_channel_remove_acl {
    vx_req_base_t base;
    VX_HANDLE account_handle;
    char* channel_uri;
    char* acl_uri;
} vx_req_account_channel_remove_acl_t;
VIVOXSDK_DLLEXPORT void vx_req_account_channel_remove_acl_create(vx_req_account_channel_remove_acl_t ** req);

typedef struct vx_req_account_channel_get_acl {
    vx_req_base_t base;
    VX_HANDLE account_handle;
    char* channel_uri;
} vx_req_account_channel_get_acl_t;
VIVOXSDK_DLLEXPORT void vx_req_account_channel_get_acl_create(vx_req_account_channel_get_acl_t ** req);

typedef struct vx_req_channel_mute_user {
    vx_req_base_t base;
    VX_HANDLE account_handle;
    char* channel_name;
    char* channel_uri;
    char* participant_uri;
    int set_muted;
} vx_req_channel_mute_user_t;
VIVOXSDK_DLLEXPORT void vx_req_channel_mute_user_create(vx_req_channel_mute_user_t ** req);

typedef struct vx_req_channel_ban_user {
    vx_req_base_t base;
    VX_HANDLE account_handle;
    char* channel_name;
    char* channel_uri;
    char* participant_uri;
    int set_banned;
} vx_req_channel_ban_user_t;
VIVOXSDK_DLLEXPORT void vx_req_channel_ban_user_create(vx_req_channel_ban_user_t ** req);

typedef struct vx_req_channel_get_banned_users {
    vx_req_base_t base;
    VX_HANDLE account_handle;
    char* channel_uri;
} vx_req_channel_get_banned_users_t;
VIVOXSDK_DLLEXPORT void vx_req_channel_get_banned_users_create(vx_req_channel_get_banned_users_t ** req);

typedef struct vx_req_channel_kick_user {
    vx_req_base_t base;
    VX_HANDLE account_handle;
    char* channel_name;
    char* channel_uri;
    char* participant_uri;
} vx_req_channel_kick_user_t;
VIVOXSDK_DLLEXPORT void vx_req_channel_kick_user_create(vx_req_channel_kick_user_t ** req);

typedef struct vx_req_channel_mute_all_users {
    vx_req_base_t base;
    VX_HANDLE account_handle;
    char* channel_name;
    char* channel_uri;
    int set_muted;
} vx_req_channel_mute_all_users_t;
VIVOXSDK_DLLEXPORT void vx_req_channel_mute_all_users_create(vx_req_channel_mute_all_users_t ** req);

typedef struct vx_req_connector_mute_local_mic {
    vx_req_base_t base;
    VX_HANDLE connector_handle;
    int mute_level;
} vx_req_connector_mute_local_mic_t;
VIVOXSDK_DLLEXPORT void vx_req_connector_mute_local_mic_create(vx_req_connector_mute_local_mic_t ** req);

typedef struct vx_req_connector_mute_local_speaker {
    vx_req_base_t base;
    VX_HANDLE connector_handle;
    int mute_level;
} vx_req_connector_mute_local_speaker_t;
VIVOXSDK_DLLEXPORT void vx_req_connector_mute_local_speaker_create(vx_req_connector_mute_local_speaker_t ** req);

typedef struct vx_req_connector_set_local_mic_volume {
    vx_req_base_t base;
    VX_HANDLE connector_handle;
    int volume; // a number between 0 and 100 where 50 represents "normal" speaking volume
} vx_req_connector_set_local_mic_volume_t;
VIVOXSDK_DLLEXPORT void vx_req_connector_set_local_mic_volume_create(vx_req_connector_set_local_mic_volume_t ** req);

typedef struct vx_req_connector_set_local_speaker_volume {
    vx_req_base_t base;
    VX_HANDLE connector_handle;
    int volume; // a number between 0 and 100 where 50 represents "normal" speaking volume
} vx_req_connector_set_local_speaker_volume_t;
VIVOXSDK_DLLEXPORT void vx_req_connector_set_local_speaker_volume_create(vx_req_connector_set_local_speaker_volume_t ** req);

typedef struct vx_req_connector_get_local_audio_info {
    vx_req_base_t base;
    VX_HANDLE connector_handle;
} vx_req_connector_get_local_audio_info_t;
VIVOXSDK_DLLEXPORT void vx_req_connector_get_local_audio_info_create(vx_req_connector_get_local_audio_info_t ** req);

typedef struct vx_req_account_buddy_set {
    vx_req_base_t base;
    VX_HANDLE account_handle;
    char* buddy_uri;
    char* display_name;
    char* buddy_data;
    int group_id;
} vx_req_account_buddy_set_t;
VIVOXSDK_DLLEXPORT void vx_req_account_buddy_set_create(vx_req_account_buddy_set_t ** req);

typedef struct vx_req_account_buddy_delete {
    vx_req_base_t base;
    VX_HANDLE account_handle;
    char* buddy_uri;
} vx_req_account_buddy_delete_t;
VIVOXSDK_DLLEXPORT void vx_req_account_buddy_delete_create(vx_req_account_buddy_delete_t ** req);

typedef struct vx_req_account_buddygroup_set {
    vx_req_base_t base;
    VX_HANDLE account_handle;
    int group_id;
    char* group_name;
    char* group_data;
} vx_req_account_buddygroup_set_t;
VIVOXSDK_DLLEXPORT void vx_req_account_buddygroup_set_create(vx_req_account_buddygroup_set_t ** req);

typedef struct vx_req_account_buddygroup_delete {
    vx_req_base_t base;
    VX_HANDLE account_handle;
    int group_id;
} vx_req_account_buddygroup_delete_t;
VIVOXSDK_DLLEXPORT void vx_req_account_buddygroup_delete_create(vx_req_account_buddygroup_delete_t ** req);

typedef struct vx_req_account_list_buddies_and_groups {
    vx_req_base_t base;
    VX_HANDLE account_handle;
} vx_req_account_list_buddies_and_groups_t;
VIVOXSDK_DLLEXPORT void vx_req_account_list_buddies_and_groups_create(vx_req_account_list_buddies_and_groups_t ** req);

typedef struct vx_req_session_send_message {
    vx_req_base_t base;
    VX_HANDLE session_handle;
    char* message_header;
    char* message_body;
} vx_req_session_send_message_t;
VIVOXSDK_DLLEXPORT void vx_req_session_send_message_create(vx_req_session_send_message_t ** req);

typedef struct vx_req_account_set_presence {
    vx_req_base_t base;
    VX_HANDLE account_handle;
    vx_buddy_presence_state presence;
    char* custom_message;
} vx_req_account_set_presence_t;
VIVOXSDK_DLLEXPORT void vx_req_account_set_presence_create(vx_req_account_set_presence_t ** req);

typedef struct vx_req_account_send_subscription_reply {
    vx_req_base_t base;
    VX_HANDLE account_handle;
    vx_rule_type rule_type;
    int auto_accept;
    char* buddy_uri;
    char* subscription_handle;
} vx_req_account_send_subscription_reply_t;
VIVOXSDK_DLLEXPORT void vx_req_account_send_subscription_reply_create(vx_req_account_send_subscription_reply_t ** req);

typedef struct vx_req_session_send_notification {
    vx_req_base_t base;
    VX_HANDLE session_handle;
    vx_notification_type notification_type;
} vx_req_session_send_notification_t;
VIVOXSDK_DLLEXPORT void vx_req_session_send_notification_create(vx_req_session_send_notification_t ** req);

typedef struct vx_req_account_create_block_rule {
    vx_req_base_t base;
    VX_HANDLE account_handle;
    char* block_mask;
    int presence_only;
} vx_req_account_create_block_rule_t;
VIVOXSDK_DLLEXPORT void vx_req_account_create_block_rule_create(vx_req_account_create_block_rule_t ** req);

typedef struct vx_req_account_delete_block_rule {
    vx_req_base_t base;
    VX_HANDLE account_handle;
    char* block_mask;
} vx_req_account_delete_block_rule_t;
VIVOXSDK_DLLEXPORT void vx_req_account_delete_block_rule_create(vx_req_account_delete_block_rule_t ** req);

typedef struct vx_req_account_list_block_rules {
    vx_req_base_t base;
    VX_HANDLE account_handle;
} vx_req_account_list_block_rules_t;
VIVOXSDK_DLLEXPORT void vx_req_account_list_block_rules_create(vx_req_account_list_block_rules_t ** req);

typedef struct vx_req_account_create_auto_accept_rule {
    vx_req_base_t base;
    VX_HANDLE account_handle;
    char* auto_accept_mask;
    int auto_add_as_buddy;
} vx_req_account_create_auto_accept_rule_t;
VIVOXSDK_DLLEXPORT void vx_req_account_create_auto_accept_rule_create(vx_req_account_create_auto_accept_rule_t ** req);

typedef struct vx_req_account_delete_auto_accept_rule {
    vx_req_base_t base;
    VX_HANDLE account_handle;
    char* auto_accept_mask;
} vx_req_account_delete_auto_accept_rule_t;
VIVOXSDK_DLLEXPORT void vx_req_account_delete_auto_accept_rule_create(vx_req_account_delete_auto_accept_rule_t ** req);

typedef struct vx_req_account_list_auto_accept_rules {
    vx_req_base_t base;
    VX_HANDLE account_handle;
} vx_req_account_list_auto_accept_rules_t;
VIVOXSDK_DLLEXPORT void vx_req_account_list_auto_accept_rules_create(vx_req_account_list_auto_accept_rules_t ** req);

typedef struct vx_req_account_update_account {
    vx_req_base_t base;
    VX_HANDLE account_handle;
    char* phone;
    char* carrier;
} vx_req_account_update_account_t;
VIVOXSDK_DLLEXPORT void vx_req_account_update_account_create(vx_req_account_update_account_t ** req);

typedef struct vx_req_account_get_account {
    vx_req_base_t base;
    VX_HANDLE account_handle;
} vx_req_account_get_account_t;
VIVOXSDK_DLLEXPORT void vx_req_account_get_account_create(vx_req_account_get_account_t ** req);

typedef struct vx_req_account_send_sms {
    vx_req_base_t base;
    VX_HANDLE account_handle;
    char* recipient_uri;
    char* content;
} vx_req_account_send_sms_t;
VIVOXSDK_DLLEXPORT void vx_req_account_send_sms_create(vx_req_account_send_sms_t ** req);

typedef struct vx_req_aux_connectivity_info {
    vx_req_base_t base;
    char* well_known_ip;
    char* stun_server;
    char* echo_server;
    int echo_port;
    int timeout;
    char* acct_mgmt_server;
} vx_req_aux_connectivity_info_t;
VIVOXSDK_DLLEXPORT void vx_req_aux_connectivity_info_create(vx_req_aux_connectivity_info_t ** req);

typedef struct vx_req_aux_get_render_devices {
    vx_req_base_t base;
} vx_req_aux_get_render_devices_t;
VIVOXSDK_DLLEXPORT void vx_req_aux_get_render_devices_create(vx_req_aux_get_render_devices_t ** req);

typedef struct vx_req_aux_get_capture_devices {
    vx_req_base_t base;
} vx_req_aux_get_capture_devices_t;
VIVOXSDK_DLLEXPORT void vx_req_aux_get_capture_devices_create(vx_req_aux_get_capture_devices_t ** req);

typedef struct vx_req_aux_set_render_device {
    vx_req_base_t base;
    char* render_device_specifier;
} vx_req_aux_set_render_device_t;
VIVOXSDK_DLLEXPORT void vx_req_aux_set_render_device_create(vx_req_aux_set_render_device_t ** req);

typedef struct vx_req_aux_set_capture_device {
    vx_req_base_t base;
    char* capture_device_specifier;
} vx_req_aux_set_capture_device_t;
VIVOXSDK_DLLEXPORT void vx_req_aux_set_capture_device_create(vx_req_aux_set_capture_device_t ** req);

typedef struct vx_req_aux_get_mic_level {
    vx_req_base_t base;
} vx_req_aux_get_mic_level_t;
VIVOXSDK_DLLEXPORT void vx_req_aux_get_mic_level_create(vx_req_aux_get_mic_level_t ** req);

typedef struct vx_req_aux_get_speaker_level {
    vx_req_base_t base;
} vx_req_aux_get_speaker_level_t;
VIVOXSDK_DLLEXPORT void vx_req_aux_get_speaker_level_create(vx_req_aux_get_speaker_level_t ** req);

typedef struct vx_req_aux_set_mic_level {
    vx_req_base_t base;
    int level;
} vx_req_aux_set_mic_level_t;
VIVOXSDK_DLLEXPORT void vx_req_aux_set_mic_level_create(vx_req_aux_set_mic_level_t ** req);

typedef struct vx_req_aux_set_speaker_level {
    vx_req_base_t base;
    int level;
} vx_req_aux_set_speaker_level_t;
VIVOXSDK_DLLEXPORT void vx_req_aux_set_speaker_level_create(vx_req_aux_set_speaker_level_t ** req);

typedef struct vx_req_aux_render_audio_start {
    vx_req_base_t base;
    char* sound_file_path;
    int loop;
} vx_req_aux_render_audio_start_t;
VIVOXSDK_DLLEXPORT void vx_req_aux_render_audio_start_create(vx_req_aux_render_audio_start_t ** req);

typedef struct vx_req_aux_render_audio_stop {
    vx_req_base_t base;
} vx_req_aux_render_audio_stop_t;
VIVOXSDK_DLLEXPORT void vx_req_aux_render_audio_stop_create(vx_req_aux_render_audio_stop_t ** req);

typedef struct vx_req_aux_capture_audio_start {
    vx_req_base_t base;
    int duration;
} vx_req_aux_capture_audio_start_t;
VIVOXSDK_DLLEXPORT void vx_req_aux_capture_audio_start_create(vx_req_aux_capture_audio_start_t ** req);

typedef struct vx_req_aux_capture_audio_stop {
    vx_req_base_t base;
} vx_req_aux_capture_audio_stop_t;
VIVOXSDK_DLLEXPORT void vx_req_aux_capture_audio_stop_create(vx_req_aux_capture_audio_stop_t ** req);

typedef struct {
    vx_req_base_t base;
    char * name;
    int code_count;
    int codes[10];
} vx_req_aux_global_monitor_keyboard_mouse_t;
VIVOXSDK_DLLEXPORT void vx_req_aux_global_monitor_keyboard_mouse_create(vx_req_aux_global_monitor_keyboard_mouse_t ** req);

typedef struct {
    vx_req_base_t base;
    int seconds;
} vx_req_aux_set_idle_timeout_t;
VIVOXSDK_DLLEXPORT void vx_req_aux_set_idle_timeout_create(vx_req_aux_set_idle_timeout_t ** req);



typedef struct {
    char *admin_username;
    char *admin_password;
    char *grant_document;
    char *server_url;
} vx_generic_credentials;

typedef struct {
    vx_req_base_t base;
    vx_generic_credentials credentials;
    char *user_name;
    char *password;
    char *email;
    char *number;
    char *displayname;
    char *firstname;
    char *lastname;
    char *phone;
    char *lang;
    char *age;
    char *gender;
    char *timezone;
    char *alias;
    char *ext_profile;
    char *ext_id;
} vx_req_aux_create_account_t;
VIVOXSDK_DLLEXPORT void vx_req_aux_create_account_create(vx_req_aux_create_account_t ** req);

typedef struct {
    vx_req_base_t base;
    vx_generic_credentials credentials;
    char *user_name;
} vx_req_aux_reactivate_account_t;
VIVOXSDK_DLLEXPORT void vx_req_aux_reactivate_account_create(vx_req_aux_reactivate_account_t ** req);

typedef struct {
    vx_req_base_t base;
    vx_generic_credentials credentials;
    char *user_name;
} vx_req_aux_deactivate_account_t;
VIVOXSDK_DLLEXPORT void vx_req_aux_deactivate_account_create(vx_req_aux_deactivate_account_t ** req);

VIVOXSDK_DLLEXPORT extern void destroy_req(vx_req_base_t *pCmd);

#ifdef __cplusplus
}
#endif


#endif /*ndef __VXCREQUESTS_H__*/

