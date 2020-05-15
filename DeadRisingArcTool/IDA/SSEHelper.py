"""

"""

from idaapi import *
from idautils import *
from idc import *

class SSEHelperPlugin(plugin_t):

	flags = idaapi.PLUGIN_PROC
    comment = "Provides high level comments for SSE instructions."
    help = "Provides high level comments for SSE instructions."
    wanted_name = "SSE Helper"
    wanted_hotkey = "Alt+F5"

	def init(self):

		# Add our menu item entry.
		addmenu_item_ctx = idaapi.add_menu_item("Edit/Plugins", "SSE Helper", "Alt+F5", 0, self.add_comment, None)
		if addmenu_item_ctx is None:	
		
			# Failed to add the menu item.
			print "Failed to add menu item entry!"
			return idaapi.PLUGIN_SKIP

		return idaapi.PLUGIN_KEEP

	def run(self, arg):
		pass

	def term(self):
		pass

	def add_comment(self):

		# Decode the instruction at the current address.
		insSize = idaapi.decode_ins(ScreenEA())
		ins = idaapi.cmd

		# Check if this instruction is supported and handle accordingly.
		print("Instruction at 0x%08x is %s" % (ScreenEA(), ins.get_canon_mnem()))
		if ins.get_canon_mnem() == "shufps":
			pass

def PLUGIN_ENTRY():
	return SSEHelperPlugin()
