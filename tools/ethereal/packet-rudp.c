/* Packet-rudp.c
 * Routines for Reliable UDP Protocol.
 * Copyright 2004, Duncan Sargeant <dunc-ethereal@rcpt.to>
 *
 * $Id: packet-rudp.c 792 2005-02-17 20:05:09Z mccollum $
 *
 * Ethereal - Network traffic analyzer
 * By Gerald Combs <gerald@ethereal.com>
 * Copyright 1998 Gerald Combs
 *
 * Modified to use the Multiverse modified version of RDP over UDP
 * Robin McCollum <mccollum@multiverse.net>

 * Copied from packet-data.c, README.developer, and various other files.
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.


 * Reliable UDP is a lightweight protocol for providing TCP-like flow
 * control over UDP.  Cisco published an PFC a long time ago, and
 * their actual implementation is slightly different, having no
 * checksum field.
 *
 * I've cheated here - RUDP could be used for anything, but I've only
 * seen it used to switched telephony calls, so we just call the Cisco SM
 * dissector from here.
 *
 * Here are some links:
 * 
 * http://www.watersprings.org/pub/id/draft-ietf-sigtran-reliable-udp-00.txt
 * http://www.javvin.com/protocolRUDP.html
 * http://www.cisco.com/univercd/cc/td/doc/product/access/sc/rel7/omts/omts_apb.htm#30052

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

#ifndef ENABLE_STATIC
G_MODULE_EXPORT const gchar version[] = VERSION;
#endif

#ifdef HAVE_CONFIG_H
# include "config.h"
#endif

#include <stdio.h>
#include <string.h>
#include <glib.h>
#include <epan/packet.h>

#define RUDP_PORT_1	5005
#define RUDP_PORT_2	5010
#define RUDP_PORT_3	5050
#define RUDP_PORT_4     6001
#define RUDP_PORT_5     6010
#define RUDP_PORT_6     9010

static int proto_rudp = -1;

static int hf_rudp_flags = -1;
static int hf_rudp_flags_syn = -1;
static int hf_rudp_flags_ack = -1;
static int hf_rudp_flags_eak = -1;
static int hf_rudp_flags_rst = -1;
static int hf_rudp_flags_nul = -1;
static int hf_rudp_flags_ver = -1;
/*
  static int hf_rudp_flags_chk = -1;
  static int hf_rudp_flags_tcs = -1;
  static int hf_rudp_flags_0 = -1;
*/

static int hf_rudp_hlen = -1;
static int hf_rudp_dlen = -1;
static int hf_rudp_seq = -1;
static int hf_rudp_ack = -1;

static int hf_rudp_options = -1;
static int hf_rudp_max_segs = -1;
static int hf_rudp_max_seg_size = -1;
static int hf_rudp_sequenced = -1;
static int hf_rudp_eak_list = -1;

static int hf_rudp_data = -1;
/* static int hf_rudp_cksum = -1; */

static gint ett_rudp = -1;
static gint ett_rudp_flags = -1;
static gint ett_rudp_options = -1;

static dissector_table_t subdissector_table;

static void
dissect_rudp(tvbuff_t *tvb, packet_info *pinfo _U_ , proto_tree *tree)
{
  tvbuff_t * next_tvb = NULL;
  proto_tree *rudp_tree = NULL, *flags_tree, *options_tree;
  proto_item *ti_root = NULL, *ti_flags = NULL, *ti_options = NULL;
  int flags[] = { 0, 0, 0, 0, 0, 0, 0, 0 };
  /* int i; */
  guint8 flag_data;
  guint8 hlen;
  guint16 dlen;
  /* guint8 *data; */

  flags[0] = hf_rudp_flags_syn;
  flags[1] = hf_rudp_flags_ack;
  flags[2] = hf_rudp_flags_eak;
  flags[3] = hf_rudp_flags_rst;
  flags[4] = hf_rudp_flags_nul;
  /*
    flags[5] = hf_rudp_flags_chk;
    flags[6] = hf_rudp_flags_tcs;
    flags[7] = hf_rudp_flags_0;
  */

  flag_data = tvb_get_guint8(tvb, 0);
  hlen = tvb_get_guint8(tvb, 1) * 2;

  if (check_col(pinfo->cinfo, COL_PROTOCOL)) 
    col_set_str(pinfo->cinfo, COL_PROTOCOL, "RUDP");
  /*
    if (check_col(pinfo->cinfo, COL_INFO)) 
    col_clear(pinfo->cinfo, COL_INFO);
  */

  if (tree) {
    ti_root = proto_tree_add_item(tree, proto_rudp, tvb, 0, hlen, FALSE);
    rudp_tree = proto_item_add_subtree(ti_root, ett_rudp);

    ti_flags = proto_tree_add_item(rudp_tree, hf_rudp_flags, tvb, 0, 1, FALSE);
    flags_tree = proto_item_add_subtree(ti_flags, ett_rudp_flags);

    proto_tree_add_item(flags_tree, hf_rudp_flags_syn, tvb, 0, 1, FALSE);
    proto_tree_add_item(flags_tree, hf_rudp_flags_ack, tvb, 0, 1, FALSE);
    proto_tree_add_item(flags_tree, hf_rudp_flags_eak, tvb, 0, 1, FALSE);
    proto_tree_add_item(flags_tree, hf_rudp_flags_rst, tvb, 0, 1, FALSE);
    proto_tree_add_item(flags_tree, hf_rudp_flags_nul, tvb, 0, 1, FALSE);
    proto_tree_add_item(flags_tree, hf_rudp_flags_ver, tvb, 0, 1, FALSE);

    proto_tree_add_item(rudp_tree, hf_rudp_hlen, tvb, 1, 1, FALSE);
    proto_tree_add_item(rudp_tree, hf_rudp_dlen, tvb, 2, 2, FALSE);
    proto_tree_add_item(rudp_tree, hf_rudp_seq, tvb, 4, 4, FALSE);
    proto_tree_add_item(rudp_tree, hf_rudp_ack, tvb, 8, 4, FALSE);

    if (hlen > 12) {
      ti_options = proto_tree_add_item(rudp_tree, hf_rudp_options, tvb, 12, hlen - 12, FALSE);
      options_tree = proto_item_add_subtree(ti_options, ett_rudp_options);
      if ((flag_data & 0x80) && hlen >= 18) {
	/* syn is set - should be an open */
	proto_tree_add_item(options_tree, hf_rudp_max_segs, tvb, 12, 2, FALSE);
	proto_tree_add_item(options_tree, hf_rudp_max_seg_size, tvb, 14, 2, FALSE);
	proto_tree_add_item(options_tree, hf_rudp_sequenced, tvb, 16, 1, FALSE);
      }
      if ((flag_data & 0x20)) {
	/* eak is set */
	/* int entries = (hlen - 12) / 4; */
	proto_tree_add_item(options_tree, hf_rudp_eak_list, tvb, 12, hlen - 12, FALSE);
      }
    }
		
    dlen = tvb_get_ntohs(tvb, 2);
    if (dlen > 0)
      proto_tree_add_item(rudp_tree, hf_rudp_data, tvb, hlen, dlen, FALSE);
  }

  next_tvb = tvb_new_subset(tvb, hlen, -1, -1);
  if (tvb_length(next_tvb) && find_dissector("rudp.mvwp"))
    call_dissector(find_dissector("rudp.mvwp"), next_tvb, pinfo, tree);
}

void proto_register_rudp(void) 
{

  static hf_register_info hf[] = {
    { &hf_rudp_flags,
      { "RUDP Header flags",           "rudp.flags",
	FT_UINT8, BASE_DEC, NULL, 0x0,
	"", HFILL }
    },
    { &hf_rudp_flags_syn,
      { "Syn",           "rudp.flags.syn",
	FT_BOOLEAN, 8, NULL, 0x80,
	"", HFILL }
    },
    { &hf_rudp_flags_ack,
      { "Ack",           "rudp.flags.ack",
	FT_BOOLEAN, 8, NULL, 0x40,
	"", HFILL }
    },
    { &hf_rudp_flags_eak,
      { "Eak",           "rudp.flags.eak",
	FT_BOOLEAN, 8, NULL, 0x20,
	"Extended Ack", HFILL }
    },
    { &hf_rudp_flags_rst,
      { "Rst",           "rudp.flags.rst",
	FT_BOOLEAN, 8, NULL, 0x10,
	"Reset flag", HFILL }
    },
    { &hf_rudp_flags_nul,
      { "Nul",           "rudp.flags.nul",
	FT_BOOLEAN, 8, NULL, 0x08,
	"Null flag", HFILL }
    },
    { &hf_rudp_flags_ver,
      { "Version",       "rudp.flags.version",
	FT_UINT8, BASE_DEC, NULL, 0x03,
	"", HFILL }
    },
    { &hf_rudp_hlen,
      { "Header Length", "rudp.hlen",
	FT_UINT8, BASE_DEC, NULL, 0x0,
	"", HFILL }
    },
    { &hf_rudp_dlen,
      { "Data Length",   "rudp.dlen",
	FT_UINT16, BASE_DEC, NULL, 0x0,
	"", HFILL }
    },
    { &hf_rudp_seq,
      { "Seq",           "rudp.seq",
	FT_UINT32, BASE_DEC, NULL, 0x0,
	"Sequence Number", HFILL }
    },
    { &hf_rudp_ack,
      { "Ack",           "rudp.ack",
	FT_UINT32, BASE_DEC, NULL, 0x0,
	"Acknowledgement Number", HFILL }
    },
    { &hf_rudp_max_segs,
      { "MaxSegs",       "rudp.opts.maxsegs",
	FT_UINT16, BASE_DEC, NULL, 0x0,
	"Maximum Segments", HFILL }
    },
    { &hf_rudp_max_seg_size,
      { "MaxSegSize",    "rudp.opts.maxsegsize",
	FT_UINT16, BASE_DEC, NULL, 0x0,
	"Maximum Segment Size", HFILL }
    },
    { &hf_rudp_sequenced,
      { "Sequenced",     "rudp.opts.sequenced",
	FT_BOOLEAN, 8, NULL, 0x80,
	"", HFILL }
    },
    { &hf_rudp_eak_list,
      { "EakList",     "rudp.opts.eaklist",
	FT_BYTES, BASE_HEX, NULL, 0x0,
	"Extended Ack List", HFILL }
    },
    { &hf_rudp_options,
      { "RUDP Header options", "rudp.opts",
	FT_BYTES, BASE_HEX, NULL, 0x0,
	"", HFILL }
    },
    { &hf_rudp_data,
      { "Data",          "rudp.data",
	FT_BYTES, BASE_HEX, NULL, 0x0,
	"RUDP Data", HFILL }
    },

    /*
      { &hf_rudp_eack,
      { "Extended acknowledgement numbers", "rudp.eack", 
      FT_UINT32, BASE_DEC, NULL, 0x0,
      "", HFILL }},
      { &hf_rudp_sequenced,
      { "Sequenced", "rudp.sequenced", 
      FT_BOOLEAN, BASE_NONE, NULL, 0x0,
      "", HFILL }},
      { &hf_rudp_data,
      { "RUDP Data", "rudp.data", 
      FT_BYTES, BASE_HEX,	NULL, 0x0,
      "RUDP data", HFILL }},
    */
  };

  /* Setup protocol subtree array */
  static gint *ett[] = {
    &ett_rudp,
    &ett_rudp_flags,
    &ett_rudp_options,
  };

  if (proto_rudp == -1) {
    proto_rudp = proto_register_protocol (
					  "Reliable UDP",	/* name */
					  "RUDP",		/* short name */
					  "rudp"		/* abbrev */
					  );
  }

  proto_register_field_array(proto_rudp, hf, array_length(hf));
  proto_register_subtree_array(ett, array_length(ett));
}

void proto_reg_handoff_rudp(void) 
{

  dissector_handle_t rudp_handle = NULL;

  rudp_handle = create_dissector_handle(dissect_rudp, proto_rudp);

  dissector_add("udp.port", RUDP_PORT_1, rudp_handle);
  dissector_add("udp.port", RUDP_PORT_2, rudp_handle);
  dissector_add("udp.port", RUDP_PORT_3, rudp_handle);
  dissector_add("udp.port", RUDP_PORT_4, rudp_handle);
  dissector_add("udp.port", RUDP_PORT_5, rudp_handle);
  dissector_add("udp.port", RUDP_PORT_6, rudp_handle);

  subdissector_table = register_dissector_table("rudp.mvwp", "Multiverse World Protocol", FT_UINT8, 0);

}

#ifndef ENABLE_STATIC

G_MODULE_EXPORT void
plugin_reg_handoff(void){
  proto_reg_handoff_rudp();
}

G_MODULE_EXPORT void
plugin_init(plugin_address_table_t *pat
#ifndef PLUGINS_NEED_ADDRESS_TABLE
	    _U_
#endif
	    ){
  /* initialise the table of pointers needed in Win32 DLLs */
  plugin_address_table_init(pat);
  /* register the new protocol, protocol fields, and subtrees */
  if (proto_rudp == -1) { /* execute protocol initialization only once */
    proto_register_rudp();
  }
}

#endif



