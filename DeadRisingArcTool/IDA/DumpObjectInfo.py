"""

"""

from idaapi import *
from idautils import *
from idc import *
from collections import namedtuple

MtDTI = namedtuple('MtDTI', 'name, size, parentObjAddr')

# Dictionary of MtDTI objects key'd by the address of the DTI pointer.
g_ObjectDTIs = { }


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
	

def processObject(rcxQword, rdxName, objectSize, callingFunc):

	# Print the name of the object.
	typeName = readString(rdxName)
	print("Found object: Size=0x%x Name=%s" % (objectSize, typeName))

	# Set the name of the registration function.
	idaapi.set_name(callingFunc, "RegisterDTI_%s" % typeName)

	# Name the global DTI variable.
	idaapi.set_name(rcxQword, "g_%sDTI" % typeName)

	# Get a list of xrefs to the qword pointer.
	xrefsFound = 0
	getDTIFunc = -1
	for xref in idautils.XrefsTo(rcxQword):

		# Decode the instruction at the xref address.
		insSize = idaapi.decode_insn(xref.frm)
		ins = idaapi.cmd

		# Check if this is the GetDTI() function.
		if ins.get_canon_mnem() == "lea" and ins.Operands[0].reg == 0:

			# Save the function address.
			xrefsFound += 1
			callingFunc = idaapi.get_func(xref.frm)
			getDTIFunc = callingFunc.startEA

	# Make sure we found exactly 1 xref.
	if xrefsFound != 1:

		# Print an error and return.
		print("Found %d xrefs for g_%sDTI!" % (xrefsFound, typeName))
		return

	# Name the DTI function.
	idaapi.set_name(getDTIFunc, "%s::GetDTI" % typeName)

	# Get the xrefs to the GetDTI function.
	vtableXref = 0
	for xref in idautils.XrefsTo(getDTIFunc):

		# Save the xref address.
		vtableXref = xref.frm
		break

	# Make sure only 1 xref was found.
	if vtableXref == 0:

		# Print an error and return.
		print("Xref not found for %s::GetDTI()!" % typeName)
		return

	# Label some of the vtable functions.
	idaapi.set_name(vtableXref - 0x20, "%s::vtable" % typeName)
	objectDtor = ida_bytes.get_qword(vtableXref - 0x20)
	idaapi.set_name(objectDtor, "%s::dtor" % typeName)
	objectDbgPrint = ida_bytes.get_qword(vtableXref - 8)
	idaapi.set_name(objectDbgPrint, "%s::RegisterDebugOptions" % typeName)

	# Return the type name.
	return typeName


def processDTIObject(objectAddress, typeName):

	# Label the vtable.
	idaapi.set_name(objectAddress, "%s::MyDTI::vtable" % typeName)

	dtor = ida_bytes.get_qword(objectAddress)
	idaapi.set_name(dtor, "%s::MyDTI::dtor" % typeName)
	createObject = ida_bytes.get_qword(objectAddress + 8)
	idaapi.set_name(createObject, "%s::MyDTI::CreateInstance" % typeName)
	

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
		r8ParentObj = 0
		objectSize = 0
		dtiVtableAddress = 0
		dtiAddress = 0
	
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

				elif ins.Operands[0].reg == 8: # r8

					# Get the parent object DTI pointer.
					r8ParentObj = ins.Operands[1].addr

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
		typeName = processObject(rcxQword, rdxName, objectSize, callingFunc.startEA)

		# Scan instructions after the call for the dti vtable.
		rip = xref.frm
		while rip < callingFunc.endEA:

			# Decode the current instruction and check for lea.
			insSize = idaapi.decode_insn(rip)
			ins = idaapi.cmd

			# Check for a lea instruction.
			if ins.get_canon_mnem() == "lea" and ins.Operands[0].reg == 0:

				# Save the vtable address and break the loop.
				dtiVtableAddress = ins.Operands[1].addr

			elif ins.get_canon_mnem() == "mov" and ins.Operands[1].reg == 0:

				# Get the address of the dti variable.
				dtiAddress = ins.Operands[0].addr

			# Next instruction.
			rip += insSize

		# Make sure we found the dti object vtable.
		if dtiVtableAddress == 0:

			# Print an error and continue.
			print("Failed to find dti vtable address!")
			continue

		# Label the dti object vtable.
		processDTIObject(dtiVtableAddress, typeName)

		# Add the DTI object to the list.
		g_ObjectDTIs[dtiAddress] = MtDTI(typeName, objectSize, r8ParentObj)
		
	# Print the total number of xrefs and failed xref attempts.
	print("Xrefs found: %d" % xrefsFound)
	print("Xrefs null: %d" % xrefsNull)
	print("Xrefs failed: %d" % xrefsFailed)
	print("")

	# Loop through all of the DTI objects found and print the object hierarchy.
	objectTypeStrings = []
	for key in g_ObjectDTIs:

		# Print the object name and size.
		objStr = "%s 0x%x" % (g_ObjectDTIs[key].name, g_ObjectDTIs[key].size)

		# Walk the object hierarchy and append the name of each parent type.
		parent = g_ObjectDTIs[key].parentObjAddr
		while parent != 0:

			# Make sure the parent key exists in the dictionary.
			if parent in g_ObjectDTIs:

				# Print the parent.
				objStr += " -> %s 0x%x" % (g_ObjectDTIs[parent].name, g_ObjectDTIs[parent].size)
				parent = g_ObjectDTIs[parent].parentObjAddr

			else:
				print("Failed to traverse object hierarchy for %s" % g_ObjectDTIs[key].name)
				break

		# Add the string to the list.
		objectTypeStrings.append(objStr)

	# Sort the object type list alphabetically.
	objectTypeStrings.sort()

	# Print the list of strings.
	for s in objectTypeStrings:
		print(s)
	
	
def printItemNumbers():

	# Dictionary for item ids.
	itemDict = { }
	itemNumbers = []

	# Loop through the item table and print the name and ID of each item.
	addr = 0x140CDA420
	while addr < 0x140CDA950:

		# Get the name of the item.
		itemName = readString(ida_bytes.get_qword(addr))
		itemNumber = ida_bytes.get_dword(addr + 8)
		addr += 0x10

		# Add to the dictionary.
		itemDict[itemNumber] = itemName
		itemNumbers.append(itemNumber)

	# Sort the dictionary.
	itemNumbers.sort()

	# Loop and print all of the items.
	for item in itemNumbers:

		print("%s = %d" % (itemDict[item], item))


#main()
printItemNumbers()