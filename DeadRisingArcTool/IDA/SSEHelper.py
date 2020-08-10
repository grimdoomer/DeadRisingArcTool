"""

"""

from idaapi import *
import idautils
from idc import *

"""
XMLoadFloat3(XMFLOAT3 *pSource)
	movss		xmm1, dword ptr [pSource.x]		// __m128 x = _mm_load_ss( &pSource->x );
	movss		xmm0, dword ptr [pSource.y]		// __m128 y = _mm_load_ss( &pSource->y );
	unpcklps	xmm1, xmm0						// __m128 xy = _mm_unpacklo_ps( x, y );
	movss		xmm0, dword ptr [pSource.z]		// __m128 z = _mm_load_ss( &pSource->z );
	movlhps		xmm1, xmm0						// return _mm_movelh_ps( xy, z );

"""

class SSEHelperPlugin(plugin_t):

	flags = idaapi.PLUGIN_PROC
	comment = "Provides high level comments for SSE instructions."
	help = "Provides high level comments for SSE instructions."
	wanted_name = "SSE Helper"
	wanted_hotkey = "Alt+F5"

	def init(self):

		#menu_item_desc = idaapi.action_desc_t("ssehelper:add_comment", "SSEHelper", self.add_comment, 
		#	"Alt+F5", "Adds a comment for the SSE instruction at the current address.", 122)

		# Add our menu item entry.
		#idaapi.register_action(menu_item_desc)
		#idaapi.attach_action_to_menu("Edit/Plugins/", "ssehelper:add_comment", idaapi.SETMENU_APP)

		return idaapi.PLUGIN_KEEP

	def run(self, arg):
		self.add_comment()

	def term(self):
		pass

	def add_comment(self):

		comp = ['x', 'y', 'z', 'w']

		# Decode the instruction at the current address.
		ins = idautils.DecodeInstruction(ScreenEA())

		# Check if this instruction is supported and handle accordingly.
		print("Instruction at 0x%08x is %s" % (ScreenEA(), ins.get_canon_mnem()))
		if ins.get_canon_mnem() == "shufps":
			
			# Get register names.
			reg_dst = "xmm%s" % str(ins.Op1.reg - 64)
			reg_src = "xmm%s" % str(ins.Op2.reg - 64)

			# Get the shuffle indices.
			shuff_a = (ins.Op3.value & 0x3)
			shuff_b = (ins.Op3.value >> 2) & 0x3
			shuff_c = (ins.Op3.value >> 4) & 0x3
			shuff_d = (ins.Op3.value >> 6) & 0x3

			# Add the comment.
			MakeRptCmt(ScreenEA(), "%s = [%s.%c | %s.%c | %s.%c | %s.%c]" % 
				(reg_dst, reg_src, comp[shuff_d], reg_src, comp[shuff_c], reg_dst, comp[shuff_b], reg_dst, comp[shuff_a]))

		elif ins.get_canon_mnem() == "unpcklps":

			# Get register names.
			reg_dst = "xmm%c" % chr(ord('0') + ins.Op1.reg - 64)
			reg_src = "xmm%c" % chr(ord('0') + ins.Op2.reg - 64)

			# Add the comment.
			MakeRptCmt(ScreenEA(), "%s = [%s.y | %s.y | %s.x | %s.x]" % (reg_dst, reg_src, reg_dst, reg_src, reg_dst))

		elif ins.get_canon_mnem() == "movlhps":

			# Get register names.
			reg_dst = "xmm%c" % chr(ord('0') + ins.Op1.reg - 64)
			reg_src = "xmm%c" % chr(ord('0') + ins.Op2.reg - 64)

			# Add the comment.
			MakeRptCmt(ScreenEA(), "%s = [%s.y | %s.x | %s.y | %s.x]" % (reg_dst, reg_src, reg_src, reg_dst, reg_dst))


def PLUGIN_ENTRY():
	return SSEHelperPlugin()
