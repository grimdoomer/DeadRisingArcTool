"""

"""

from idaapi import *
from idautils import *
from idc import *

def readString(ea):

	str = ""
	
	# Loop and read util we hit a null terminator.
	while True:
		if Byte(ea) != 0:
			str += chr(Byte(ea))
			ea += 1
			
		else:
			break
			
	# Return the string.
	return str
	

def processObject(rcxQword, rdxName, objectSize):

	# Print the name of the object.
	typeName = readString(rdxName)
	print("Found object: Size=0x%x Name=%s" % (objectSize, typeName))

	# Name the global DTI variable.
	#set_name(rcxDword, typeName)

	# Get a list of xrefs to the qword pointer.
	qwordXrefs = idautils.XrefsTo(rcxQword)
	

def main():

	xrefsFound = 0
	xrefsNull = 0
	xrefsFailed = 0

	# Get the function that is called in all the object registration functions.
	func = idaapi.get_func(0x1406184C0)
	
	# Get a collection of xrefs to this function.
	for xref in idautils.XrefsTo(func.startEA):
	
		rdxName = 0
		rcxQword = 0
		objectSize = 0
	
		# Get the function the xref is in.
		xrefsFound += 1
		callingFunc = idaapi.get_func(xref.frm)
		if callingFunc is None:
			xrefsNull += 1
			continue
		
		# Scan all instructions from the start of the function to the xref call.
		# We need to find where rdx and rcx are set.
		rip = callingFunc.startEA
		while rip < xref.frm:
		
			# Decode the next instruction.
			insSize = idaapi.decode_insn(rip)
			ins = idaapi.cmd
			
			# Check if this is a lea instruction.
			if ins.get_canon_mnem() == "lea":
			
				# Check if we are loading rcx or rdx.
				if ins.Operands[0].reg == 1: # rcx
				
					# Get the qword address being loaded into rcx.
					rcxQword = ins.Operands[1].addr
					
				elif ins.Operands[0].reg == 2: # rdx
				
					# Get the object name address.
					rdxName = ins.Operands[1].addr

				elif ins.Operands[0].reg == 9: # r9d

					# Get the object size from the immediate.
					# lea     r9d, [rax+30h]
					objectSize = ins.Operands[1].addr

			elif ins.get_canon_mnem() == "mov":

				# Check if we are loading r9.
				if ins.Operands[0].reg == 9:

					# Get the object size from the instruction immediate.
					objectSize = ins.Operands[1].value
			
			# Next instruction.
			rip += insSize
			
		# Check if we found both rcx and rdx.
		if rcxQword == 0 or rdxName == 0:
		
			# TODO: Check xrefs to this function to find the object name and qword pointer.
			xrefsFailed += 1
			continue
			
		# Process the object.
		processObject(rcxQword, rdxName, objectSize)
		
	# Print the total number of xrefs and failed xref attempts.
	print("Xrefs found: %d" % xrefsFound)
	print("Xrefs null: %d" % xrefsNull)
	print("Xrefs failed: %d" % xrefsFailed)
	
	
main()