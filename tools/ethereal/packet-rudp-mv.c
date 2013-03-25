/* packet-rudp.c
 * Routines for Reliable UDP Protocol.
 * Copyright 2004, Duncan Sargeant <dunc-ethereal@rcpt.to>
 *
 * $Id: packet-rudp-mv.c 792 2005-02-17 20:05:09Z mccollum $
 *
 * Ethereal - Network traffic analyzer
 *
 */

#ifdef HAVE_CONFIG_H
# include "config.h"
#endif

#include <gmodule.h>
#include <epan/packet.h>

#include "plugins/plugin_api.h"
#include "plugins/plugin_api_defs.h"
 /* Define version if we are not building ethereal statically */

/*
#include "moduleinfo.h"
*/

static int proto_rudp_mv = -1;

static int hf_rudp_mv_oid = -1;
static int hf_rudp_mv_msgtype = -1;
static int hf_rudp_mv_data = -1;

static gint ett_rudp_mv = -1;

static void
dissect_mv_rudp(tvbuff_t *tvb, packet_info *pinfo _U_ , proto_tree *tree)
{
	proto_tree *rudp_mv_tree = NULL;
	proto_item *ti = NULL;

	if (tree) {
		ti = proto_tree_add_item(tree, proto_rudp_mv, tvb, 0, 12, FALSE);
		rudp_mv_tree = proto_item_add_subtree(ti, ett_rudp_mv);

		proto_tree_add_item(rudp_mv_tree, hf_rudp_mv_oid, tvb, 0, 8, FALSE);
		proto_tree_add_item(rudp_mv_tree, hf_rudp_mv_msgtype, tvb, 8, 4, FALSE);
		proto_tree_add_item(rudp_mv_tree, hf_rudp_mv_data, tvb, 12, -1, FALSE);
	}
}

void proto_register_rudp_mv(void) 
{

  static hf_register_info hf[] = {
		{ &hf_rudp_mv_oid,
			{ "Oid",           "rudp.mvwp.oid",
			FT_UINT64, BASE_DEC, NULL, 0x0,
			"Multiverse Oid", HFILL }
		},
		{ &hf_rudp_mv_msgtype,
			{ "MsgType",       "rudp.mvwp.msgtype",
			FT_UINT32, BASE_DEC, NULL, 0x80,
			"", HFILL }
		},
		{ &hf_rudp_mv_data,
			{ "Data",       "rudp.mvwp.data",
			FT_BYTES, BASE_HEX, NULL, 0x0,
			"Multiverse Data", HFILL }
		},
  };

/* Setup protocol subtree array */
	static gint *ett[] = {
		&ett_rudp_mv
	};

	if (proto_rudp_mv == -1) {
	    proto_rudp_mv = proto_register_protocol (
		"Multiverse World Protocol",		/* name */
		"MVWP",		/* short name */
		"rudp.mvwp"		/* abbrev */
		);
	}

	proto_register_field_array(proto_rudp_mv, hf, array_length(hf));
	proto_register_subtree_array(ett, array_length(ett));
}

void proto_reg_handoff_rudp_mv(void) 
{

  dissector_handle_t rudp_mv_handle = NULL;

  rudp_mv_handle = create_dissector_handle(dissect_mv_rudp, proto_rudp_mv);
  dissector_add("rudp.mvwp", 0, rudp_mv_handle);
}

