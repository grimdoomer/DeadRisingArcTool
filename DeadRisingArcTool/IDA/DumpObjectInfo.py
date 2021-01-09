"""

"""

from idaapi import *
import idautils
from idc import *
from collections import namedtuple

MtDTI = namedtuple('MtDTI', 'name, size, parentObjAddr')
VTableLayout = namedtuple('VTableLayout', 'className, parentClass, vtableLayout')

# Dictionary of MtDTI objects key'd by the address of the DTI pointer.
g_ObjectDTIs = { }

# Dictionary of vtable layouts for MtDTI objects.
g_VtableLayouts = {
	"MtObject" : VTableLayout("MtObject", "", {
		0x0 : "dtor",
		0x8 : "",
		0x10 : "",
		0x18 : "RegisterDebugOptions",
		0x20 : "GetDTI"
	}),
	"MtAllocator" : VTableLayout("MtAllocator", "MtObject", {
		0x28 : "Alloc",
		0x30 : "Free",
		0x38 : ""
	}),
	"MtDataReader" : VTableLayout("MtDataReader", "", {
		0x00 : "dtor",
		0x08 : "ReadUInt16",
		0x10 : "ReadUInt32",
		0x18 : "ReadUInt64",
		0x20 : "",	# Duplicates of the previous 3
		0x28 : "",
		0x30 : "",
		0x38 : "ReadFloat",
		0x40 : "ReadDouble"
	}),
	"MtFile" : VTableLayout("MtFile", "MtObject", {
		0x28 : "OpenFile",
		0x30 : "CloseFile",
		0x38 : "ReadFile",
		0x40 : "ReadFileAsync",
		0x48 : "WaitForCompletion",
		0x50 : "WriteFile",
		0x58 : "Seek",
		0x60 : "GetCurrentPosition",
		0x68 : "GetFileSize",
		0x70 : "SetFileSize",
		0x78 : "CanRead",
		0x80 : "CanWrite",
		0x88 : ""
	}),
	"MtStream" : VTableLayout("MtStream", "MtObject", {
		0x28 : "CanRead",
		0x30 : "CanWrite",
		0x38 : "",
		0x40 : "",
		0x48 : "GetCurrentPosition",
		0x50 : "Close",
		0x58 : "",
		0x60 : "ReadData",
		0x68 : "ReadDataAsync",
		0x70 : "WaitForCompletion",
		0x78 : "WriteData",
		0x80 : "",
		0x88 : "GetLength",
		0x90 : "Seek"
	}),

	"cResource" : VTableLayout("cResource", "MtObject", {
		0x28 : "GetFileInfo",
		0x30 : "GetFileExtension",
		0x38 : "",
		0x40 : "LoadResource",
		0x48 : "SaveResource",
		0x50 : "",
		0x58 : "CleanupResources"
	}),
	"cSystem" : VTableLayout("cSystem", "MtObject", {
		0x28 : "SystemCleanup",
		0x30 : "SystemUpdate",
		0x38 : "BuildSystemMenu"
	}),
	"cUnit" : VTableLayout("cUnit", "MtObject", {
		0x28 : "",
		0x30 : "",
		0x38 : "",
		0x40 : "",
		0x48 : "",
		0x50 : "GetObjectName"
	}),
	"sUnit" : VTableLayout("sUnit", "cSystem", {
		0x40 : "",
		0x48 : "",
		0x50 : "GetMoveLineName",
		0x58 : ""
	}),
}

# Dictionary to record while object vtables have already been labeled.
g_LabeledVtables = { }

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
		return None, None

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
		return None, None

	# Label the vtable and dtor.
	idaapi.set_name(vtableXref - 0x20, "%s::vtable" % typeName)
	objectDtor = ida_bytes.get_qword(vtableXref - 0x20)
	idaapi.set_name(objectDtor, "%s::dtor" % typeName)

	# Return the type name and vtable address.
	return (typeName, vtableXref - 0x20)


def processDTIObject(objectAddress, typeName):

	# Label the vtable.
	idaapi.set_name(objectAddress, "%s::MyDTI::vtable" % typeName)

	dtor = ida_bytes.get_qword(objectAddress)
	idaapi.set_name(dtor, "%s::MyDTI::dtor" % typeName)
	createObject = ida_bytes.get_qword(objectAddress + 8)
	idaapi.set_name(createObject, "%s::MyDTI::CreateInstance" % typeName)
	

def processVTableLayout(typeName):

	# Make sure we haven't already labeled this object before.
	if typeName in g_LabeledVtables == False or g_LabeledVtables[typeName][2] == True:
		return

	# Recursively label parent classes this object inherits.
	dtiAddress = g_LabeledVtables[typeName][1]
	while g_ObjectDTIs[dtiAddress].parentObjAddr != 0:

		# Recursively label parent object vtable.
		parent = g_ObjectDTIs[dtiAddress].parentObjAddr
		parentName = g_ObjectDTIs[parent].name
		processVTableLayout(parentName)

		# Get the next parent dti address for labeling.
		dtiAddress = g_ObjectDTIs[dtiAddress].parentObjAddr

	# Walk the vtable mapping dictionary and label functions.
	dtiAddress = g_LabeledVtables[typeName][1]
	while dtiAddress != 0:

		# Check if there is a vtable mapping for this object type.
		name = g_ObjectDTIs[dtiAddress].name
		if name in g_VtableLayouts:

			# Loop through all the vtable functions in the mapping and label each one.
			for vtableOffset in g_VtableLayouts[name].vtableLayout:

				# If there is no function name for this entry skip it.
				if g_VtableLayouts[name].vtableLayout[vtableOffset] == "":
					continue

				# Check if the function at this vtable address has been labeled or not.
				funcAddress = ida_bytes.get_qword(g_LabeledVtables[typeName][0] + vtableOffset)
				funcName = idaapi.get_func_name(funcAddress)
				if funcName.startswith("sub_") == True:

					# Label the function.
					idaapi.set_name(funcAddress, typeName + "::" + g_VtableLayouts[name].vtableLayout[vtableOffset], idaapi.SN_PUBLIC | idaapi.SN_FORCE)

		# Get the next parent dti address for labeling.
		dtiAddress = g_ObjectDTIs[dtiAddress].parentObjAddr

	# Flag that this object type has been labeled.
	g_LabeledVtables[typeName][2] = True


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
		typeName, vtableAddress = processObject(rcxQword, rdxName, objectSize, callingFunc.startEA)

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

		# Add the object to the list of object vtables to label.
		if typeName is not None and vtableAddress is not None:

			# Add the object info to the list of vtables to label.
			g_LabeledVtables[typeName] = [vtableAddress, dtiAddress, False]
		
	# Print the total number of xrefs and failed xref attempts.
	print("Xrefs found: %d" % xrefsFound)
	print("Xrefs null: %d" % xrefsNull)
	print("Xrefs failed: %d" % xrefsFailed)
	print("")

	# Loop through all of the vtables to process and label each one.
	print("Labeling vtables...")
	for key in g_LabeledVtables:

		# Process the object vtable.
		processVTableLayout(key)

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
	print("Object Hierarchy:")
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



def printItemInfo():

	itemIds = [ 
		"Item_0",
		"Item_Pylon",
		"Item_Shopping_Cart",
		"Item_3",
		"Item_Baseball_Bat",
		"Item_Garbage_Can",
		"Item_Chair",
		"Item_Shovel",
		"Item_Push_Broom",
		"Item_Push_Broom_Handle",
		"Item_10",
		"Item_First_Aid_Kit",
		"Item_Beam_Sword",
		"Item_13",
		"Item_Paint_Can",
		"Item_15",
		"Item_Hockey_Stick",
		"Item_Handbag",
		"Item_Chair1",
		"Item_Cleaver",
		"Item_Skateboard",
		"Item_Toy_Cube",
		"Item_Gems",
		"Item_Battle_Axe",
		"Item_Store_Display",
		"Item_Store_Display1",
		"Item_Bass_Guitar",
		"Item_Acoustic_Guitar",
		"Item_Store_Display2",
		"Item_Grenade",
		"Item_Water_Gun",
		"Item_31",
		"Item_Toy_Laser_Sword",
		"Item_Pick_Ax",
		"Item_34",
		"Item_Chair2",
		"Item_Golf_Club",
		"Item_Soccer_Ball",
		"Item_Snack",
		"Item_39",
		"Item_40",
		"Item_41",
		"Item_Raw_Meat",
		"Item_Well_Done_Steak",
		"Item_Spoiled_Meat",
		"Item_45",
		"Item_Fire_Extinguisher",
		"Item_Book_Camera_2",
		"Item_Parasol",
		"Item_Fire_Ax",
		"Item_Sledgehammer",
		"Item_Frying_Pan",
		"Item_Katana",
		"Item_Chainsaw",
		"Item_Dumbbell",
		"Item_Hanger",
		"Item_Book_Survival",
		"Item_Potted_Plant",
		"Item_Mega_Man_Buster",
		"Item_59",
		"Item_Shotgun",
		"Item_61",
		"Item_62",
		"Item_63",
		"Item_Sickle",
		"Item_65",
		"Item_Vase",
		"Item_Painting",
		"Item_Book_Cult_Initiation_Guide",
		"Item_Queen",
		"Item_Lawn_Mower",
		"Item_Bowing_Ball",
		"Item_Stun_Gun",
		"Item_73",
		"Item_Hedge_Trimmer",
		"Item_Handgun",
		"Item_Knife",
		"Item_Propane_Tank",
		"Item_78",
		"Item_Lead_Pipe",
		"Item_Sign",
		"Item_TV",
		"Item_Submachine_Gun",
		"Item_Mannequin",
		"Item_Mannequin_Limb",
		"Item_Mannequin_Limb1",
		"Item_Mannequin_Limb2",
		"Item_Mannequin_Limb3",
		"Item_Mannequin_Limb4",
		"Item_Mannequin1",
		"Item_Mannequin_Limb5",
		"Item_Mannequin_Limb6",
		"Item_Mannequin_Limb7",
		"Item_Mannequin_Limb8",
		"Item_Mannequin_Limb9",
		"Item_Potted_Plant1",
		"Item_Potted_Plant2",
		"Item_Cactus",
		"Item_Potted_Plant3",
		"Item_Potted_Plant4",
		"Item_Potted_Plant5",
		"Item_Barbell",
		"Item_Wine_Cask",
		"Item_103",
		"Item_104",
		"Item_Gumball_Machine",
		"Item_Mega_Man_Buster1",
		"Item_107",
		"Item_Plasma_TV",
		"Item_109",
		"Item_Uncooked_Pizza",
		"Item_Golden_Brown_Pizza",
		"Item_Rotten_Pizza",
		"Item_Pie",
		"Item_Nailgun",
		"Item_Smokestack",
		"Item_Chair3",
		"Item_Stepladder",
		"Item_Toolbox",
		"Item_Excavator",
		"Item_4Door_Car",
		"Item_Track",
		"Item_Hammer",
		"Item_Bike",
		"Item_124",
		"Item_Bicycle",
		"Item_126",
		"Item_Small_Chainsaw",
		"Item_128",
		"Item_129",
		"Item_130",
		"Item_131",
		"Item_Chair4",
		"Item_Chair5",
		"Item_Machinegun",
		"Item_Sniper_Rifle",
		"Item_2_x_4_",
		"Item_Boomerang",
		"Item_Bucket",
		"Item_Nightstick",
		"Item_Wine",
		"Item_Electric_Guitar",
		"Item_142",
		"Item_King_Salmon",
		"Item_144",
		"Item_Zucchini",
		"Item_2Door_Car",
		"Item_Chinese_Cleaver",
		"Item_148",
		"Item_Book_Japanese_Conversation",
		"Item_150",
		"Item_151",
		"Item_Oil_Bucket",
		"Item_Cash_Register",
		"Item_Propane_Tank1",
		"Item_Book_Wrestling",
		"Item_156",
		"Item_157",
		"Item_158",
		"Item_Rock",
		"Item_Dishes",
		"Item_161",
		"Item_162",
		"Item_163",
		"Item_164",
		"Item_RC_Bomb",
		"Item_Weapon_Cart",
		"Item_167",
		"Item_Sword",
		"Item_169",
		"Item_170",
		"Item_171",
		"Item_172",
		"Item_173",
		"Item_174",
		"Item_175",
		"Item_176",
		"Item_177",
		"Item_178",
		"Item_179",
		"Item_180",
		"Item_Cardboard_Box",
		"Item_182",
		"Item_183",
		"Item_Shower_Head",
		"Item_185",
		"Item_Antimaterial_Rifle",
		"Item_Pipe_Bomb",
		"Item_188",
		"Item_Combat_Knife",
		"Item_Molotov_Cocktail",
		"Item_191",
		"Item_192",
		"Item_Book_Hobby",
		"Item_194",
		"Item_Ceremonial_Sword",
		"Item_Novelty_Mask",
		"Item_Novelty_Mask1",
		"Item_Novelty_Mask2",
		"Item_Novelty_Mask3",
		"Item_Painting1",
		"Item_201",
		"Item_Bench",
		"Item_Steel_Rack",
		"Item_Shelf",
		"Item_Sausage_Rack",
		"Item_206",
		"Item_207",
		"Item_Corn",
		"Item_Squash",
		"Item_Cabbage",
		"Item_Japanese_Radish",
		"Item_Lettuce",
		"Item_Red_Cabbage",
		"Item_Baguette",
		"Item_Melon",
		"Item_Grapefruit",
		"Item_Orange",
		"Item_Orange_Juice",
		"Item_Milk",
		"Item_Coffee_Creamer",
		"Item_Yogurt",
		"Item_Cheese",
		"Item_223",
		"Item_224",
		"Item_225",
		"Item_226",
		"Item_227",
		"Item_Shampoo",
		"Item_Pet_Food",
		"Item_Cookies",
		"Item_Baking_Ingredients",
		"Item_Cooking_Oil",
		"Item_Condiment",
		"Item_Canned_Sauce",
		"Item_Canned_Food",
		"Item_Drink_Cans",
		"Item_Frozen_Vegetables",
		"Item_Apple",
		"Item_Ice_Pops",
		"Item_Milk1",
		"Item_Rat_Stick",
		"Item_Rat_Saucer",
		"Item_Painting2",
		"Item_Saw_Blade",
		"Item_245",
		"Item_Skylight",
		"Item_Fence",
		"Item_Painting3",
		"Item_249",
		"Item_250",
		"Item_Stuffed_Bear",
		"Item_Mailbox",
		"Item_253",
		"Item_Mailbox_Post",
		"Item_Painting4",
		"Item_Sign1",
		"Item_Severed_Arm",
		"Item_258",
		"Item_259",
		"Item_Plywood_Panel",
		"Item_261",
		"Item_262",
		"Item_263",
		"Item_CD",
		"Item_Heavy_Machine_Gun",
		"Item_266",
		"Item_267",
		"Item_268",
		"Item_Perfume_Prop",
		"Item_Lipstick_Prop",
		"Item_271",
		"Item_Book_Cooking",
		"Item_Book_Lifestyle_Magazine",
		"Item_Book_Tools",
		"Item_Book_Sports",
		"Item_Book_Criminal_Biography",
		"Item_Book_Travel",
		"Item_Book_Interior_Design",
		"Item_Book_Entertainment",
		"Item_Book_Camera_1",
		"Item_Book_Skateboarding",
		"Item_Book_Wartime_Photography",
		"Item_Book_Weekly_Photo_Magazine",
		"Item_Book_Horror_Novel_1",
		"Item_Book_World_News",
		"Item_Book_Health_1",
		"Item_Book_Cycling",
		"Item_Book_Health_2",
		"Item_Book_Horror_Novel_2",
		"Item_Electric_Guitar1",
		"Item_Chair6",
		"Item_Chair7",
		"Item_Chair8",
		"Item_Stool",
		"Item_Chair9",
		"Item_296",
		"Item_297",
		"Item_298",
		"Item_299",
		"Item_300",
		"Item_301",
		"Item_Juice_Quick_Step",
		"Item_Juice_Randomizer",
		"Item_Juice_Untouchable",
		"Item_Juice_Spitfire",
		"Item_Juice_Nectar",
		"Item_Juice_Energizer",
		"Item_Juice_Zombait",
		"Item_309",
		"Item_310",
		"Item_311",
		"Item_Melted_Ice_Pops",
		"Item_Thawed_Vegetables" ]

	# Loop and print all item info.
	addr = 0x14122C1E0
	while addr < 0x14122DF68:

		# Get the item info.
		itemNumber = ida_bytes.get_dword(addr)
		itemName = readString(ida_bytes.get_qword(addr + 8))
		itemPath = readString(ida_bytes.get_qword(addr + 0x10))
		addr += 0x18

		print("%d - %s %s" % (itemNumber, itemName, itemPath))


#main()
main()