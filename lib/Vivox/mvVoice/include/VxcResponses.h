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

#ifndef __VXCRESPONSES_H__
#define __VXCRESPONSES_H__

#include "Vxc.h"
#include "VxcRequests.h"


#ifdef __cplusplus
extern "C" {
#endif

/* Begin Vivox responses */

typedef struct vx_resp_connector_create {
    vx_resp_base_t base;
    VX_HANDLE connector_handle;
    char* version_id;
} vx_resp_connector_create_t;
VIVOXSDK_DLLEXPORT void vx_resp_connector_create_free(vx_resp_connector_create_t * resp);

typedef struct vx_resp_connector_initiate_shutdown {
    vx_resp_base_t base;
    char* client_name;
} vx_resp_connector_initiate_shutdown_t;
VIVOXSDK_DLLEXPORT void vx_resp_connector_initiate_shutdown_free(vx_resp_connector_initiate_shutdown_t * resp);

typedef struct vx_resp_account_login {
    vx_resp_base_t base;
    VX_HANDLE account_handle;
} vx_resp_account_login_t;
VIVOXSDK_DLLEXPORT void vx_resp_account_login_free(vx_resp_account_login_t * resp);

typedef struct vx_resp_account_logout {
    vx_resp_base_t base;
} vx_resp_account_logout_t;
VIVOXSDK_DLLEXPORT void vx_resp_account_logout_free(vx_resp_account_logout_t * resp);

typedef struct vx_resp_account_set_login_properties {
    vx_resp_base_t base;
} vx_resp_account_set_login_properties_t;
VIVOXSDK_DLLEXPORT void vx_resp_account_set_login_properties_free(vx_resp_account_set_login_properties_t * resp);

typedef struct vx_resp_sessiongroup_create {
    vx_resp_base_t base;
    VX_HANDLE sessiongroup_handle;
} vx_resp_sessiongroup_create_t;
VIVOXSDK_DLLEXPORT void vx_resp_sessiongroup_create_free(vx_resp_sessiongroup_create_t * resp);

typedef struct vx_resp_sessiongroup_terminate {
    vx_resp_base_t base;
} vx_resp_sessiongroup_terminate_t;
VIVOXSDK_DLLEXPORT void vx_resp_sessiongroup_terminate_free(vx_resp_sessiongroup_terminate_t * resp);

typedef struct vx_resp_sessiongroup_add_session {
    vx_resp_base_t base;
    VX_HANDLE session_handle;
} vx_resp_sessiongroup_add_session_t;
VIVOXSDK_DLLEXPORT void vx_resp_sessiongroup_add_session_free(vx_resp_sessiongroup_add_session_t * resp);

typedef struct vx_resp_sessiongroup_remove_session {
    vx_resp_base_t base;
} vx_resp_sessiongroup_remove_session_t;
VIVOXSDK_DLLEXPORT void vx_resp_sessiongroup_remove_session_free(vx_resp_sessiongroup_remove_session_t * resp);

typedef struct vx_resp_sessiongroup_set_focus {
    vx_resp_base_t base;
} vx_resp_sessiongroup_set_focus_t;
VIVOXSDK_DLLEXPORT void vx_resp_sessiongroup_set_focus_free(vx_resp_sessiongroup_set_focus_t * resp);

typedef struct vx_resp_sessiongroup_unset_focus {
    vx_resp_base_t base;
} vx_resp_sessiongroup_unset_focus_t;
VIVOXSDK_DLLEXPORT void vx_resp_sessiongroup_unset_focus_free(vx_resp_sessiongroup_unset_focus_t * resp);

typedef struct vx_resp_sessiongroup_reset_focus {
    vx_resp_base_t base;
} vx_resp_sessiongroup_reset_focus_t;
VIVOXSDK_DLLEXPORT void vx_resp_sessiongroup_reset_focus_free(vx_resp_sessiongroup_reset_focus_t * resp);

typedef struct vx_resp_sessiongroup_set_tx_session {
    vx_resp_base_t base;
} vx_resp_sessiongroup_set_tx_session_t;
VIVOXSDK_DLLEXPORT void vx_resp_sessiongroup_set_tx_session_free(vx_resp_sessiongroup_set_tx_session_t * resp);

typedef struct vx_resp_sessiongroup_set_tx_all_sessions {
    vx_resp_base_t base;
} vx_resp_sessiongroup_set_tx_all_sessions_t;
VIVOXSDK_DLLEXPORT void vx_resp_sessiongroup_set_tx_all_sessions_free(vx_resp_sessiongroup_set_tx_all_sessions_t * resp);

typedef struct vx_resp_sessiongroup_set_tx_no_session {
    vx_resp_base_t base;
} vx_resp_sessiongroup_set_tx_no_session_t;
VIVOXSDK_DLLEXPORT void vx_resp_sessiongroup_set_tx_no_session_free(vx_resp_sessiongroup_set_tx_no_session_t * resp);

typedef struct vx_resp_session_create {
    vx_resp_base_t base;
    VX_HANDLE sessiongroup_handle;
    VX_HANDLE session_handle;
} vx_resp_session_create_t;
VIVOXSDK_DLLEXPORT void vx_resp_session_create_free(vx_resp_session_create_t * resp);

typedef struct vx_resp_session_connect {
    vx_resp_base_t base;
} vx_resp_session_connect_t;
VIVOXSDK_DLLEXPORT void vx_resp_session_connect_free(vx_resp_session_connect_t * resp);

typedef struct vx_resp_session_media_connect {
    vx_resp_base_t base;
} vx_resp_session_media_connect_t;
VIVOXSDK_DLLEXPORT void vx_resp_session_media_connect_free(vx_resp_session_media_connect_t * resp);

typedef struct vx_resp_session_media_disconnect {
    vx_resp_base_t base;
} vx_resp_session_media_disconnect_t;
VIVOXSDK_DLLEXPORT void vx_resp_session_media_disconnect_free(vx_resp_session_media_disconnect_t * resp);

typedef struct vx_resp_session_terminate {
    vx_resp_base_t base;
} vx_resp_session_terminate_t;
VIVOXSDK_DLLEXPORT void vx_resp_session_terminate_free(vx_resp_session_terminate_t * resp);

typedef struct vx_resp_session_mute_local_speaker {
    vx_resp_base_t base;
} vx_resp_session_mute_local_speaker_t;
VIVOXSDK_DLLEXPORT void vx_resp_session_mute_local_speaker_free(vx_resp_session_mute_local_speaker_t * resp);

typedef struct vx_resp_session_set_local_speaker_volume {
    vx_resp_base_t base;
} vx_resp_session_set_local_speaker_volume_t;
VIVOXSDK_DLLEXPORT void vx_resp_session_set_local_speaker_volume_free(vx_resp_session_set_local_speaker_volume_t * resp);

typedef struct vx_resp_session_get_local_audio_info {
    vx_resp_base_t base;
    int speaker_volume;
    int is_speaker_muted;
    int mic_volume;
    int is_mic_muted;
} vx_resp_session_get_local_audio_info_t;
VIVOXSDK_DLLEXPORT void vx_resp_session_get_local_audio_info_free(vx_resp_session_get_local_audio_info_t * resp);

typedef struct vx_resp_session_channel_invite_user {
    vx_resp_base_t base;
} vx_resp_session_channel_invite_user_t;
VIVOXSDK_DLLEXPORT void vx_resp_session_channel_invite_user_free(vx_resp_session_channel_invite_user_t * resp);

typedef struct vx_resp_session_set_participant_volume_for_me {
    vx_resp_base_t base;
} vx_resp_session_set_participant_volume_for_me_t;
VIVOXSDK_DLLEXPORT void vx_resp_session_set_participant_volume_for_me_free(vx_resp_session_set_participant_volume_for_me_t * resp);

typedef struct vx_resp_session_set_participant_mute_for_me {
    vx_resp_base_t base;
} vx_resp_session_set_participant_mute_for_me_t;
VIVOXSDK_DLLEXPORT void vx_resp_session_set_participant_mute_for_me_free(vx_resp_session_set_participant_mute_for_me_t * resp);

typedef struct vx_resp_session_set_3d_position {
    vx_resp_base_t base;
} vx_resp_session_set_3d_position_t;
VIVOXSDK_DLLEXPORT void vx_resp_session_set_3d_position_free(vx_resp_session_set_3d_position_t * resp);

typedef struct vx_resp_session_render_audio_start {
    vx_resp_base_t base;
} vx_resp_session_render_audio_start_t;
VIVOXSDK_DLLEXPORT void vx_resp_session_render_audio_start_free(vx_resp_session_render_audio_start_t * resp);

typedef struct vx_resp_session_render_audio_stop {
    vx_resp_base_t base;
} vx_resp_session_render_audio_stop_t;
VIVOXSDK_DLLEXPORT void vx_resp_session_render_audio_stop_free(vx_resp_session_render_audio_stop_t * resp);

typedef struct vx_resp_session_channel_get_participants {
    vx_resp_base_t base;
    int participants_size;
    vx_participant_t** participants;
} vx_resp_session_channel_get_participants_t;
VIVOXSDK_DLLEXPORT void vx_resp_session_channel_get_participants_free(vx_resp_session_channel_get_participants_t * resp);

typedef struct vx_resp_account_channel_get_list {
    vx_resp_base_t base;
    int channels_size;
    vx_channel_t** channels;
} vx_resp_account_channel_get_list_t;
VIVOXSDK_DLLEXPORT void vx_resp_account_channel_get_list_free(vx_resp_account_channel_get_list_t * resp);

typedef struct vx_resp_account_channel_create {
    vx_resp_base_t base;
    char* channel_uri;
} vx_resp_account_channel_create_t;
VIVOXSDK_DLLEXPORT void vx_resp_account_channel_create_free(vx_resp_account_channel_create_t * resp);

typedef struct vx_resp_account_channel_update {
    vx_resp_base_t base;
} vx_resp_account_channel_update_t;
VIVOXSDK_DLLEXPORT void vx_resp_account_channel_update_free(vx_resp_account_channel_update_t * resp);

typedef struct vx_resp_account_channel_delete {
    vx_resp_base_t base;
} vx_resp_account_channel_delete_t;
VIVOXSDK_DLLEXPORT void vx_resp_account_channel_delete_free(vx_resp_account_channel_delete_t * resp);

typedef struct vx_resp_account_channel_create_and_invite {
    vx_resp_base_t base;
    char* channel_uri;
} vx_resp_account_channel_create_and_invite_t;
VIVOXSDK_DLLEXPORT void vx_resp_account_channel_create_and_invite_free(vx_resp_account_channel_create_and_invite_t * resp);

typedef struct vx_resp_account_channel_folder_create {
    vx_resp_base_t base;
    int folder_id;
} vx_resp_account_channel_folder_create_t;
VIVOXSDK_DLLEXPORT void vx_resp_account_channel_folder_create_free(vx_resp_account_channel_folder_create_t * resp);

typedef struct vx_resp_account_channel_folder_update {
    vx_resp_base_t base;
} vx_resp_account_channel_folder_update_t;
VIVOXSDK_DLLEXPORT void vx_resp_account_channel_folder_update_free(vx_resp_account_channel_folder_update_t * resp);

typedef struct vx_resp_account_channel_folder_delete {
    vx_resp_base_t base;
} vx_resp_account_channel_folder_delete_t;
VIVOXSDK_DLLEXPORT void vx_resp_account_channel_folder_delete_free(vx_resp_account_channel_folder_delete_t * resp);

typedef struct vx_resp_account_channel_folder_get_info {
    vx_resp_base_t base;
    vx_channel_t* folder;
} vx_resp_account_channel_folder_get_info_t;
VIVOXSDK_DLLEXPORT void vx_resp_account_channel_folder_get_info_free(vx_resp_account_channel_folder_get_info_t * resp);

typedef struct vx_resp_account_channel_favorites_get_list {
    vx_resp_base_t base;
    int group_count;
    int favorite_count;
    vx_channel_favorite_group_t** groups;
    vx_channel_favorite_t** favorites;
} vx_resp_account_channel_favorites_get_list_t;
VIVOXSDK_DLLEXPORT void vx_resp_account_channel_favorites_get_list_free(vx_resp_account_channel_favorites_get_list_t * resp);

typedef struct vx_resp_account_channel_favorite_set {
    vx_resp_base_t base;
    int channel_favorite_id;
} vx_resp_account_channel_favorite_set_t;
VIVOXSDK_DLLEXPORT void vx_resp_account_channel_favorite_set_free(vx_resp_account_channel_favorite_set_t * resp);

typedef struct vx_resp_account_channel_favorite_delete {
    vx_resp_base_t base;
} vx_resp_account_channel_favorite_delete_t;
VIVOXSDK_DLLEXPORT void vx_resp_account_channel_favorite_delete_free(vx_resp_account_channel_favorite_delete_t * resp);

typedef struct vx_resp_account_channel_favorite_group_set {
    vx_resp_base_t base;
    int group_id;
} vx_resp_account_channel_favorite_group_set_t;
VIVOXSDK_DLLEXPORT void vx_resp_account_channel_favorite_group_set_free(vx_resp_account_channel_favorite_group_set_t * resp);

typedef struct vx_resp_account_channel_favorite_group_delete {
    vx_resp_base_t base;
} vx_resp_account_channel_favorite_group_delete_t;
VIVOXSDK_DLLEXPORT void vx_resp_account_channel_favorite_group_delete_free(vx_resp_account_channel_favorite_group_delete_t * resp);

typedef struct vx_resp_account_channel_get_info {
    vx_resp_base_t base;
    vx_channel_t* channel;
} vx_resp_account_channel_get_info_t;
VIVOXSDK_DLLEXPORT void vx_resp_account_channel_get_info_free(vx_resp_account_channel_get_info_t * resp);

typedef struct vx_resp_account_channel_search {
    vx_resp_base_t base;
    int page;
    int from;
    int to;
    int channel_count;
    vx_channel_t** channels;
} vx_resp_account_channel_search_t;
VIVOXSDK_DLLEXPORT void vx_resp_account_channel_search_free(vx_resp_account_channel_search_t * resp);

typedef struct vx_resp_account_buddy_search {
    vx_resp_base_t base;
    int page;
    int from;
    int to;
    int buddy_count;
    vx_buddy_t** buddies;
} vx_resp_account_buddy_search_t;
VIVOXSDK_DLLEXPORT void vx_resp_account_buddy_search_free(vx_resp_account_buddy_search_t * resp);

typedef struct vx_resp_account_channel_add_moderator {
    vx_resp_base_t base;
} vx_resp_account_channel_add_moderator_t;
VIVOXSDK_DLLEXPORT void vx_resp_account_channel_add_moderator_free(vx_resp_account_channel_add_moderator_t * resp);

typedef struct vx_resp_account_channel_remove_moderator {
    vx_resp_base_t base;
} vx_resp_account_channel_remove_moderator_t;
VIVOXSDK_DLLEXPORT void vx_resp_account_channel_remove_moderator_free(vx_resp_account_channel_remove_moderator_t * resp);

typedef struct vx_resp_account_channel_get_moderators {
    vx_resp_base_t base;
    int participants_size;
    vx_participant_t** participants;
} vx_resp_account_channel_get_moderators_t;
VIVOXSDK_DLLEXPORT void vx_resp_account_channel_get_moderators_free(vx_resp_account_channel_get_moderators_t * resp);

typedef struct vx_resp_account_channel_add_acl {
    vx_resp_base_t base;
} vx_resp_account_channel_add_acl_t;
VIVOXSDK_DLLEXPORT void vx_resp_account_channel_add_acl_free(vx_resp_account_channel_add_acl_t * resp);

typedef struct vx_resp_account_channel_remove_acl {
    vx_resp_base_t base;
} vx_resp_account_channel_remove_acl_t;
VIVOXSDK_DLLEXPORT void vx_resp_account_channel_remove_acl_free(vx_resp_account_channel_remove_acl_t * resp);

typedef struct vx_resp_account_channel_get_acl {
    vx_resp_base_t base;
    int participants_size;
    vx_participant_t** participants;
} vx_resp_account_channel_get_acl_t;
VIVOXSDK_DLLEXPORT void vx_resp_account_channel_get_acl_free(vx_resp_account_channel_get_acl_t * resp);

typedef struct vx_resp_channel_mute_user {
    vx_resp_base_t base;
} vx_resp_channel_mute_user_t;
VIVOXSDK_DLLEXPORT void vx_resp_channel_mute_user_free(vx_resp_channel_mute_user_t * resp);

typedef struct vx_resp_channel_ban_user {
    vx_resp_base_t base;
} vx_resp_channel_ban_user_t;
VIVOXSDK_DLLEXPORT void vx_resp_channel_ban_user_free(vx_resp_channel_ban_user_t * resp);

typedef struct vx_resp_channel_get_banned_users {
    vx_resp_base_t base;
    int banned_users_count;
    vx_participant_t** banned_users;
} vx_resp_channel_get_banned_users_t;
VIVOXSDK_DLLEXPORT void vx_resp_channel_get_banned_users_free(vx_resp_channel_get_banned_users_t * resp);

typedef struct vx_resp_channel_kick_user {
    vx_resp_base_t base;
} vx_resp_channel_kick_user_t;
VIVOXSDK_DLLEXPORT void vx_resp_channel_kick_user_free(vx_resp_channel_kick_user_t * resp);

typedef struct vx_resp_channel_mute_all_users {
    vx_resp_base_t base;
} vx_resp_channel_mute_all_users_t;
VIVOXSDK_DLLEXPORT void vx_resp_channel_mute_all_users_free(vx_resp_channel_mute_all_users_t * resp);

typedef struct vx_resp_connector_mute_local_mic {
    vx_resp_base_t base;
} vx_resp_connector_mute_local_mic_t;
VIVOXSDK_DLLEXPORT void vx_resp_connector_mute_local_mic_free(vx_resp_connector_mute_local_mic_t * resp);

typedef struct vx_resp_connector_mute_local_speaker {
    vx_resp_base_t base;
} vx_resp_connector_mute_local_speaker_t;
VIVOXSDK_DLLEXPORT void vx_resp_connector_mute_local_speaker_free(vx_resp_connector_mute_local_speaker_t * resp);

typedef struct vx_resp_connector_set_local_mic_volume {
    vx_resp_base_t base;
} vx_resp_connector_set_local_mic_volume_t;
VIVOXSDK_DLLEXPORT void vx_resp_connector_set_local_mic_volume_free(vx_resp_connector_set_local_mic_volume_t * resp);

typedef struct vx_resp_connector_set_local_speaker_volume {
    vx_resp_base_t base;
} vx_resp_connector_set_local_speaker_volume_t;
VIVOXSDK_DLLEXPORT void vx_resp_connector_set_local_speaker_volume_free(vx_resp_connector_set_local_speaker_volume_t * resp);

typedef struct vx_resp_connector_get_local_audio_info {
    vx_resp_base_t base;
    int speaker_volume;
    int is_speaker_muted;
    int mic_volume;
    int is_mic_muted;
} vx_resp_connector_get_local_audio_info_t;
VIVOXSDK_DLLEXPORT void vx_resp_connector_get_local_audio_info_free(vx_resp_connector_get_local_audio_info_t * resp);

typedef struct vx_resp_account_buddy_set {
    vx_resp_base_t base;
} vx_resp_account_buddy_set_t;
VIVOXSDK_DLLEXPORT void vx_resp_account_buddy_set_free(vx_resp_account_buddy_set_t * resp);

typedef struct vx_resp_account_buddy_delete {
    vx_resp_base_t base;
} vx_resp_account_buddy_delete_t;
VIVOXSDK_DLLEXPORT void vx_resp_account_buddy_delete_free(vx_resp_account_buddy_delete_t * resp);

typedef struct vx_resp_account_buddygroup_set {
    vx_resp_base_t base;
    int group_id;
} vx_resp_account_buddygroup_set_t;
VIVOXSDK_DLLEXPORT void vx_resp_account_buddygroup_set_free(vx_resp_account_buddygroup_set_t * resp);

typedef struct vx_resp_account_buddygroup_delete {
    vx_resp_base_t base;
} vx_resp_account_buddygroup_delete_t;
VIVOXSDK_DLLEXPORT void vx_resp_account_buddygroup_delete_free(vx_resp_account_buddygroup_delete_t * resp);

typedef struct vx_resp_account_list_buddies_and_groups {
    vx_resp_base_t base;
    int buddy_count;
    int group_count;
    vx_buddy_t** buddies;
    vx_group_t** groups;
} vx_resp_account_list_buddies_and_groups_t;
VIVOXSDK_DLLEXPORT void vx_resp_account_list_buddies_and_groups_free(vx_resp_account_list_buddies_and_groups_t * resp);

typedef struct vx_resp_session_send_message {
    vx_resp_base_t base;
} vx_resp_session_send_message_t;
VIVOXSDK_DLLEXPORT void vx_resp_session_send_message_free(vx_resp_session_send_message_t * resp);

typedef struct vx_resp_account_set_presence {
    vx_resp_base_t base;
} vx_resp_account_set_presence_t;
VIVOXSDK_DLLEXPORT void vx_resp_account_set_presence_free(vx_resp_account_set_presence_t * resp);

typedef struct vx_resp_account_send_subscription_reply {
    vx_resp_base_t base;
} vx_resp_account_send_subscription_reply_t;
VIVOXSDK_DLLEXPORT void vx_resp_account_send_subscription_reply_free(vx_resp_account_send_subscription_reply_t * resp);

typedef struct vx_resp_session_send_notification {
    vx_resp_base_t base;
} vx_resp_session_send_notification_t;
VIVOXSDK_DLLEXPORT void vx_resp_session_send_notification_free(vx_resp_session_send_notification_t * resp);

typedef struct vx_resp_account_create_block_rule {
    vx_resp_base_t base;
} vx_resp_account_create_block_rule_t;
VIVOXSDK_DLLEXPORT void vx_resp_account_create_block_rule_free(vx_resp_account_create_block_rule_t * resp);

typedef struct vx_resp_account_delete_block_rule {
    vx_resp_base_t base;
} vx_resp_account_delete_block_rule_t;
VIVOXSDK_DLLEXPORT void vx_resp_account_delete_block_rule_free(vx_resp_account_delete_block_rule_t * resp);

typedef struct vx_resp_account_list_block_rules {
    vx_resp_base_t base;
    int rule_count;
    vx_block_rule_t** block_rules;
} vx_resp_account_list_block_rules_t;
VIVOXSDK_DLLEXPORT void vx_resp_account_list_block_rules_free(vx_resp_account_list_block_rules_t * resp);

typedef struct vx_resp_account_create_auto_accept_rule {
    vx_resp_base_t base;
} vx_resp_account_create_auto_accept_rule_t;
VIVOXSDK_DLLEXPORT void vx_resp_account_create_auto_accept_rule_free(vx_resp_account_create_auto_accept_rule_t * resp);

typedef struct vx_resp_account_delete_auto_accept_rule {
    vx_resp_base_t base;
} vx_resp_account_delete_auto_accept_rule_t;
VIVOXSDK_DLLEXPORT void vx_resp_account_delete_auto_accept_rule_free(vx_resp_account_delete_auto_accept_rule_t * resp);

typedef struct vx_resp_account_list_auto_accept_rules {
    vx_resp_base_t base;
    int rule_count;
    vx_auto_accept_rule_t** auto_accept_rules;
} vx_resp_account_list_auto_accept_rules_t;
VIVOXSDK_DLLEXPORT void vx_resp_account_list_auto_accept_rules_free(vx_resp_account_list_auto_accept_rules_t * resp);

typedef struct vx_resp_account_update_account {
    vx_resp_base_t base;
} vx_resp_account_update_account_t;
VIVOXSDK_DLLEXPORT void vx_resp_account_update_account_free(vx_resp_account_update_account_t * resp);

typedef struct vx_resp_account_get_account {
    vx_resp_base_t base;
    vx_account_t* account;
} vx_resp_account_get_account_t;
VIVOXSDK_DLLEXPORT void vx_resp_account_get_account_free(vx_resp_account_get_account_t * resp);

typedef struct vx_resp_account_send_sms {
    vx_resp_base_t base;
} vx_resp_account_send_sms_t;
VIVOXSDK_DLLEXPORT void vx_resp_account_send_sms_free(vx_resp_account_send_sms_t * resp);

typedef struct vx_resp_aux_connectivity_info {
    vx_resp_base_t base;
    int count;
    vx_connectivity_test_result_t** test_results;
    char* well_known_ip;
    char* stun_server;
    char* echo_server;
    int echo_port;
    int timeout;
} vx_resp_aux_connectivity_info_t;
VIVOXSDK_DLLEXPORT void vx_resp_aux_connectivity_info_free(vx_resp_aux_connectivity_info_t * resp);

typedef struct vx_resp_aux_get_render_devices {
    vx_resp_base_t base;
    int count;
    vx_device_t** render_devices;
    vx_device_t* current_render_device;
} vx_resp_aux_get_render_devices_t;
VIVOXSDK_DLLEXPORT void vx_resp_aux_get_render_devices_free(vx_resp_aux_get_render_devices_t * resp);

typedef struct vx_resp_aux_get_capture_devices {
    vx_resp_base_t base;
    int count;
    vx_device_t** capture_devices;
    vx_device_t* current_capture_device;
} vx_resp_aux_get_capture_devices_t;
VIVOXSDK_DLLEXPORT void vx_resp_aux_get_capture_devices_free(vx_resp_aux_get_capture_devices_t * resp);

typedef struct vx_resp_aux_set_render_device {
    vx_resp_base_t base;
} vx_resp_aux_set_render_device_t;
VIVOXSDK_DLLEXPORT void vx_resp_aux_set_render_device_free(vx_resp_aux_set_render_device_t * resp);

typedef struct vx_resp_aux_set_capture_device {
    vx_resp_base_t base;
} vx_resp_aux_set_capture_device_t;
VIVOXSDK_DLLEXPORT void vx_resp_aux_set_capture_device_free(vx_resp_aux_set_capture_device_t * resp);

typedef struct vx_resp_aux_get_mic_level {
    vx_resp_base_t base;
    int level;
} vx_resp_aux_get_mic_level_t;
VIVOXSDK_DLLEXPORT void vx_resp_aux_get_mic_level_free(vx_resp_aux_get_mic_level_t * resp);

typedef struct vx_resp_aux_get_speaker_level {
    vx_resp_base_t base;
    int level;
} vx_resp_aux_get_speaker_level_t;
VIVOXSDK_DLLEXPORT void vx_resp_aux_get_speaker_level_free(vx_resp_aux_get_speaker_level_t * resp);

typedef struct vx_resp_aux_set_mic_level {
    vx_resp_base_t base;
} vx_resp_aux_set_mic_level_t;
VIVOXSDK_DLLEXPORT void vx_resp_aux_set_mic_level_free(vx_resp_aux_set_mic_level_t * resp);

typedef struct vx_resp_aux_set_speaker_level {
    vx_resp_base_t base;
} vx_resp_aux_set_speaker_level_t;
VIVOXSDK_DLLEXPORT void vx_resp_aux_set_speaker_level_free(vx_resp_aux_set_speaker_level_t * resp);

typedef struct vx_resp_aux_render_audio_start {
    vx_resp_base_t base;
} vx_resp_aux_render_audio_start_t;
VIVOXSDK_DLLEXPORT void vx_resp_aux_render_audio_start_free(vx_resp_aux_render_audio_start_t * resp);

typedef struct vx_resp_aux_render_audio_stop {
    vx_resp_base_t base;
} vx_resp_aux_render_audio_stop_t;
VIVOXSDK_DLLEXPORT void vx_resp_aux_render_audio_stop_free(vx_resp_aux_render_audio_stop_t * resp);

typedef struct vx_resp_aux_capture_audio_start {
    vx_resp_base_t base;
} vx_resp_aux_capture_audio_start_t;
VIVOXSDK_DLLEXPORT void vx_resp_aux_capture_audio_start_free(vx_resp_aux_capture_audio_start_t * resp);

typedef struct vx_resp_aux_capture_audio_stop {
    vx_resp_base_t base;
} vx_resp_aux_capture_audio_stop_t;
VIVOXSDK_DLLEXPORT void vx_resp_aux_capture_audio_stop_free(vx_resp_aux_capture_audio_stop_t * resp);

typedef struct {
    vx_resp_base_t base;
} vx_resp_aux_global_monitor_keyboard_mouse_t;
VIVOXSDK_DLLEXPORT void vx_resp_aux_global_monitor_keyboard_mouse_free(vx_resp_aux_global_monitor_keyboard_mouse_t * resp);

typedef struct {
    vx_resp_base_t base;
} vx_resp_aux_set_idle_timeout_t;
VIVOXSDK_DLLEXPORT void vx_resp_aux_set_idle_timeout_free(vx_resp_aux_set_idle_timeout_t * resp);

typedef struct {
    vx_resp_base_t base;
} vx_resp_aux_create_account_t;
VIVOXSDK_DLLEXPORT void vx_resp_aux_create_account_free(vx_resp_aux_create_account_t * resp);

typedef struct {
    vx_resp_base_t base;
} vx_resp_aux_reactivate_account_t;
VIVOXSDK_DLLEXPORT void vx_resp_aux_reactivate_account_free(vx_resp_aux_reactivate_account_t * resp);

typedef struct {
    vx_resp_base_t base;
} vx_resp_aux_deactivate_account_t;
VIVOXSDK_DLLEXPORT void vx_resp_aux_deactivate_account_free(vx_resp_aux_deactivate_account_t * resp);

/* End Vivox responses */

extern void VIVOXSDK_DLLEXPORT destroy_resp(vx_resp_base_t *pCmd);

#ifdef __cplusplus
}
#endif

#endif /* ndef __VXCRESPONSES_H__ */

