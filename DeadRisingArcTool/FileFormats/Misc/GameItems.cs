﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadRisingArcTool.FileFormats.Misc
{
    #region ItemId

    public enum ItemId : uint
    {
        Item_0 = 0,
        Item_Pylon = 1,
        Item_Shopping_Cart = 2,
        Item_3 = 3,
        Item_Baseball_Bat = 4,
        Item_Garbage_Can = 5,
        Item_Chair = 6,
        Item_Shovel = 7,
        Item_Push_Broom = 8,
        Item_Push_Broom_Handle = 9,
        Item_10 = 10,
        Item_First_Aid_Kit = 11,
        Item_Beam_Sword = 12,
        Item_13 = 13,
        Item_Paint_Can = 14,
        Item_15 = 15,
        Item_Hockey_Stick = 16,
        Item_Handbag = 17,
        Item_Chair1 = 18,
        Item_Cleaver = 19,
        Item_Skateboard = 20,
        Item_Toy_Cube = 21,
        Item_Gems = 22,
        Item_Battle_Axe = 23,
        Item_Store_Display = 24,
        Item_Store_Display1 = 25,
        Item_Bass_Guitar = 26,
        Item_Acoustic_Guitar = 27,
        Item_Store_Display2 = 28,
        Item_Grenade = 29,
        Item_Water_Gun = 30,
        Item_31 = 31,
        Item_Toy_Laser_Sword = 32,
        Item_Pick_Ax = 33,
        Item_34 = 34,
        Item_Chair2 = 35,
        Item_Golf_Club = 36,
        Item_Soccer_Ball = 37,
        Item_Snack = 38,
        Item_39 = 39,
        Item_40 = 40,
        Item_41 = 41,
        Item_Raw_Meat = 42,
        Item_Well_Done_Steak = 43,
        Item_Spoiled_Meat = 44,
        Item_45 = 45,
        Item_Fire_Extinguisher = 46,
        Item_Book_Camera_2 = 47,
        Item_Parasol = 48,
        Item_Fire_Ax = 49,
        Item_Sledgehammer = 50,
        Item_Frying_Pan = 51,
        Item_Katana = 52,
        Item_Chainsaw = 53,
        Item_Dumbbell = 54,
        Item_Hanger = 55,
        Item_Book_Survival = 56,
        Item_Potted_Plant = 57,
        Item_Mega_Man_Buster = 58,
        Item_59 = 59,
        Item_Shotgun = 60,
        Item_61 = 61,
        Item_62 = 62,
        Item_63 = 63,
        Item_Sickle = 64,
        Item_65 = 65,
        Item_Vase = 66,
        Item_Painting = 67,
        Item_Book_Cult_Initiation_Guide = 68,
        Item_Queen = 69,
        Item_Lawn_Mower = 70,
        Item_Bowing_Ball = 71,
        Item_Stun_Gun = 72,
        Item_73 = 73,
        Item_Hedge_Trimmer = 74,
        Item_Handgun = 75,
        Item_Knife = 76,
        Item_Propane_Tank = 77,
        Item_78 = 78,
        Item_Lead_Pipe = 79,
        Item_Sign = 80,
        Item_TV = 81,
        Item_Submachine_Gun = 82,
        Item_Mannequin = 83,
        Item_Mannequin_Limb = 84,
        Item_Mannequin_Limb1 = 85,
        Item_Mannequin_Limb2 = 86,
        Item_Mannequin_Limb3 = 87,
        Item_Mannequin_Limb4 = 88,
        Item_Mannequin1 = 89,
        Item_Mannequin_Limb5 = 90,
        Item_Mannequin_Limb6 = 91,
        Item_Mannequin_Limb7 = 92,
        Item_Mannequin_Limb8 = 93,
        Item_Mannequin_Limb9 = 94,
        Item_Potted_Plant1 = 95,
        Item_Potted_Plant2 = 96,
        Item_Cactus = 97,
        Item_Potted_Plant3 = 98,
        Item_Potted_Plant4 = 99,
        Item_Potted_Plant5 = 100,
        Item_Barbell = 101,
        Item_Wine_Cask = 102,
        Item_103 = 103,
        Item_104 = 104,
        Item_Gumball_Machine = 105,
        Item_Mega_Man_Buster1 = 106,
        Item_107 = 107,
        Item_Plasma_TV = 108,
        Item_109 = 109,
        Item_Uncooked_Pizza = 110,
        Item_Golden_Brown_Pizza = 111,
        Item_Rotten_Pizza = 112,
        Item_Pie = 113,
        Item_Nailgun = 114,
        Item_Smokestack = 115,
        Item_Chair3 = 116,
        Item_Stepladder = 117,
        Item_Toolbox = 118,
        Item_Excavator = 119,
        Item_4Door_Car = 120,
        Item_Track = 121,
        Item_Hammer = 122,
        Item_Bike = 123,
        Item_124 = 124,
        Item_Bicycle = 125,
        Item_126 = 126,
        Item_Small_Chainsaw = 127,
        Item_128 = 128,
        Item_129 = 129,
        Item_130 = 130,
        Item_131 = 131,
        Item_Chair4 = 132,
        Item_Chair5 = 133,
        Item_Machinegun = 134,
        Item_Sniper_Rifle = 135,
        Item_2_x_4_ = 136,
        Item_Boomerang = 137,
        Item_Bucket = 138,
        Item_Nightstick = 139,
        Item_Wine = 140,
        Item_Electric_Guitar = 141,
        Item_142 = 142,
        Item_King_Salmon = 143,
        Item_144 = 144,
        Item_Zucchini = 145,
        Item_2Door_Car = 146,
        Item_Chinese_Cleaver = 147,
        Item_148 = 148,
        Item_Book_Japanese_Conversation = 149,
        Item_150 = 150,
        Item_151 = 151,
        Item_Oil_Bucket = 152,
        Item_Cash_Register = 153,
        Item_Propane_Tank1 = 154,
        Item_Book_Wrestling = 155,
        Item_156 = 156,
        Item_157 = 157,
        Item_158 = 158,
        Item_Rock = 159,
        Item_Dishes = 160,
        Item_161 = 161,
        Item_162 = 162,
        Item_163 = 163,
        Item_164 = 164,
        Item_RC_Bomb = 165,
        Item_Weapon_Cart = 166,
        Item_167 = 167,
        Item_Sword = 168,
        Item_169 = 169,
        Item_170 = 170,
        Item_171 = 171,
        Item_172 = 172,
        Item_173 = 173,
        Item_174 = 174,
        Item_175 = 175,
        Item_176 = 176,
        Item_177 = 177,
        Item_178 = 178,
        Item_179 = 179,
        Item_180 = 180,
        Item_Cardboard_Box = 181,
        Item_182 = 182,
        Item_183 = 183,
        Item_Shower_Head = 184,
        Item_185 = 185,
        Item_Antimaterial_Rifle = 186,
        Item_Pipe_Bomb = 187,
        Item_188 = 188,
        Item_Combat_Knife = 189,
        Item_Molotov_Cocktail = 190,
        Item_191 = 191,
        Item_192 = 192,
        Item_Book_Hobby = 193,
        Item_194 = 194,
        Item_Ceremonial_Sword = 195,
        Item_Novelty_Mask = 196,
        Item_Novelty_Mask1 = 197,
        Item_Novelty_Mask2 = 198,
        Item_Novelty_Mask3 = 199,
        Item_Painting1 = 200,
        Item_201 = 201,
        Item_Bench = 202,
        Item_Steel_Rack = 203,
        Item_Shelf = 204,
        Item_Sausage_Rack = 205,
        Item_206 = 206,
        Item_207 = 207,
        Item_Corn = 208,
        Item_Squash = 209,
        Item_Cabbage = 210,
        Item_Japanese_Radish = 211,
        Item_Lettuce = 212,
        Item_Red_Cabbage = 213,
        Item_Baguette = 214,
        Item_Melon = 215,
        Item_Grapefruit = 216,
        Item_Orange = 217,
        Item_Orange_Juice = 218,
        Item_Milk = 219,
        Item_Coffee_Creamer = 220,
        Item_Yogurt = 221,
        Item_Cheese = 222,
        Item_223 = 223,
        Item_224 = 224,
        Item_225 = 225,
        Item_226 = 226,
        Item_227 = 227,
        Item_Shampoo = 228,
        Item_Pet_Food = 229,
        Item_Cookies = 230,
        Item_Baking_Ingredients = 231,
        Item_Cooking_Oil = 232,
        Item_Condiment = 233,
        Item_Canned_Sauce = 234,
        Item_Canned_Food = 235,
        Item_Drink_Cans = 236,
        Item_Frozen_Vegetables = 237,
        Item_Apple = 238,
        Item_Ice_Pops = 239,
        Item_Milk1 = 240,
        Item_Rat_Stick = 241,
        Item_Rat_Saucer = 242,
        Item_Painting2 = 243,
        Item_Saw_Blade = 244,
        Item_245 = 245,
        Item_Skylight = 246,
        Item_Fence = 247,
        Item_Painting3 = 248,
        Item_249 = 249,
        Item_250 = 250,
        Item_Stuffed_Bear = 251,
        Item_Mailbox = 252,
        Item_253 = 253,
        Item_Mailbox_Post = 254,
        Item_Painting4 = 255,
        Item_Sign1 = 256,
        Item_Severed_Arm = 257,
        Item_258 = 258,
        Item_259 = 259,
        Item_Plywood_Panel = 260,
        Item_261 = 261,
        Item_262 = 262,
        Item_263 = 263,
        Item_CD = 264,
        Item_Heavy_Machine_Gun = 265,
        Item_266 = 266,
        Item_267 = 267,
        Item_268 = 268,
        Item_Perfume_Prop = 269,
        Item_Lipstick_Prop = 270,
        Item_271 = 271,
        Item_Book_Cooking = 272,
        Item_Book_Lifestyle_Magazine = 273,
        Item_Book_Tools = 274,
        Item_Book_Sports = 275,
        Item_Book_Criminal_Biography = 276,
        Item_Book_Travel = 277,
        Item_Book_Interior_Design = 278,
        Item_Book_Entertainment = 279,
        Item_Book_Camera_1 = 280,
        Item_Book_Skateboarding = 281,
        Item_Book_Wartime_Photography = 282,
        Item_Book_Weekly_Photo_Magazine = 283,
        Item_Book_Horror_Novel_1 = 284,
        Item_Book_World_News = 285,
        Item_Book_Health_1 = 286,
        Item_Book_Cycling = 287,
        Item_Book_Health_2 = 288,
        Item_Book_Horror_Novel_2 = 289,
        Item_Electric_Guitar1 = 290,
        Item_Chair6 = 291,
        Item_Chair7 = 292,
        Item_Chair8 = 293,
        Item_Stool = 294,
        Item_Chair9 = 295,
        Item_296 = 296,
        Item_297 = 297,
        Item_298 = 298,
        Item_299 = 299,
        Item_300 = 300,
        Item_301 = 301,
        Item_Juice_Quick_Step = 302,
        Item_Juice_Randomizer = 303,
        Item_Juice_Untouchable = 304,
        Item_Juice_Spitfire = 305,
        Item_Juice_Nectar = 306,
        Item_Juice_Energizer = 307,
        Item_Juice_Zombait = 308,
        Item_309 = 309,
        Item_310 = 310,
        Item_311 = 311,
        Item_Melted_Ice_Pops = 312,
        Item_Thawed_Vegetables = 313,
        ItemId_Max = 314
    }

    #endregion

    public class GameItem
    {
        public ItemId Id { get; private set; }
        public string DisplayName { get; private set; }
        public string FilePath { get; private set; }

        public GameItem(ItemId id, string name, string filePath)
        {
            // Initialize fields.
            this.Id = id;
            this.DisplayName = name;
            this.FilePath = filePath;
        }
    }

    public class GameItems
    {
        #region Stock game items

        public static GameItem[] StockGameItems = new GameItem[]
        {
            new GameItem(ItemId.Item_0, "-----", ""),
            new GameItem(ItemId.Item_Pylon, "Pylon", "arc\\rom\\om\\om0001\\om0001"),
            new GameItem(ItemId.Item_Shopping_Cart, "Shopping Cart", "arc\\rom\\om\\om0002\\om0002"),
            new GameItem(ItemId.Item_3, "-----", ""),
            new GameItem(ItemId.Item_Baseball_Bat, "Baseball Bat", "arc\\rom\\om\\om0004\\om0004"),
            new GameItem(ItemId.Item_Garbage_Can, "Garbage Can", "arc\\rom\\om\\om0005\\om0005"),
            new GameItem(ItemId.Item_Chair, "Chair", "arc\\rom\\om\\om0029\\om0029"),
            new GameItem(ItemId.Item_Shovel, "Shovel", "arc\\rom\\om\\om000a\\om000a"),
            new GameItem(ItemId.Item_Push_Broom, "Push Broom", "arc\\rom\\om\\om000b\\om000b"),
            new GameItem(ItemId.Item_Push_Broom_Handle, "Push Broom Handle", "arc\\rom\\om\\om000b\\om000b"),
            new GameItem(ItemId.Item_10, "-----", ""),
            new GameItem(ItemId.Item_First_Aid_Kit, "First Aid Kit", "arc\\rom\\om\\om021c\\om021c"),
            new GameItem(ItemId.Item_Beam_Sword, "Beam Sword", "arc\\rom\\om\\om000d\\om000d"),
            new GameItem(ItemId.Item_13, "-----", ""),
            new GameItem(ItemId.Item_Paint_Can, "Paint Can", "arc\\rom\\om\\om000f\\om000f"),
            new GameItem(ItemId.Item_15, "-----", "arc\\rom\\om\\om0010\\om0010"),
            new GameItem(ItemId.Item_Hockey_Stick, "Hockey Stick", "arc\\rom\\om\\om0011\\om0011"),
            new GameItem(ItemId.Item_Handbag, "Handbag", "arc\\rom\\om\\om0013\\om0013"),
            new GameItem(ItemId.Item_Chair1, "Chair", "arc\\rom\\om\\om0205\\om0205"),
            new GameItem(ItemId.Item_Cleaver, "Cleaver", "arc\\rom\\om\\om0015\\om0015"),
            new GameItem(ItemId.Item_Skateboard, "Skateboard", "arc\\rom\\om\\om0017\\om0017"),
            new GameItem(ItemId.Item_Toy_Cube, "Toy Cube", "arc\\rom\\om\\om0213\\om0213"),
            new GameItem(ItemId.Item_Gems, "Gems", "arc\\rom\\om\\om0214\\om0214"),
            new GameItem(ItemId.Item_Battle_Axe, "Battle Axe", "arc\\rom\\om\\om001a\\om001a"),
            new GameItem(ItemId.Item_Store_Display, "Store Display", "arc\\rom\\om\\om0216\\om0216"),
            new GameItem(ItemId.Item_Store_Display1, "Store Display", "arc\\rom\\om\\om0217\\om0217"),
            new GameItem(ItemId.Item_Bass_Guitar, "Bass Guitar", "arc\\rom\\om\\om0020\\om0020"),
            new GameItem(ItemId.Item_Acoustic_Guitar, "Acoustic Guitar", "arc\\rom\\om\\om0021\\om0021"),
            new GameItem(ItemId.Item_Store_Display2, "Store Display", "arc\\rom\\om\\om0218\\om0218"),
            new GameItem(ItemId.Item_Grenade, "Grenade", "arc\\rom\\om\\om0023\\om0023"),
            new GameItem(ItemId.Item_Water_Gun, "Water Gun", "arc\\rom\\om\\om0024\\om0024"),
            new GameItem(ItemId.Item_31, "-----", ""),
            new GameItem(ItemId.Item_Toy_Laser_Sword, "Toy Laser Sword", "arc\\rom\\om\\om0026\\om0026"),
            new GameItem(ItemId.Item_Pick_Ax, "Pick Ax", "arc\\rom\\om\\om0027\\om0027"),
            new GameItem(ItemId.Item_34, "-----", ""),
            new GameItem(ItemId.Item_Chair2, "Chair", "arc\\rom\\om\\om021a\\om021a"),
            new GameItem(ItemId.Item_Golf_Club, "Golf Club", "arc\\rom\\om\\om002c\\om002c"),
            new GameItem(ItemId.Item_Soccer_Ball, "Soccer Ball", "arc\\rom\\om\\om002d\\om002d"),
            new GameItem(ItemId.Item_Snack, "Snack", "arc\\rom\\om\\om002f\\om002f"),
            new GameItem(ItemId.Item_39, "-----", ""),
            new GameItem(ItemId.Item_40, "-----", ""),
            new GameItem(ItemId.Item_41, "-----", ""),
            new GameItem(ItemId.Item_Raw_Meat, "Raw Meat", "arc\\rom\\om\\om0034\\om0034"),
            new GameItem(ItemId.Item_Well_Done_Steak, "Well Done Steak", "arc\\rom\\om\\om0034\\om0034"),
            new GameItem(ItemId.Item_Spoiled_Meat, "Spoiled Meat", "arc\\rom\\om\\om0034\\om0034"),
            new GameItem(ItemId.Item_45, "-----", ""),
            new GameItem(ItemId.Item_Fire_Extinguisher, "Fire Extinguisher", "arc\\rom\\om\\om0039\\om0039"),
            new GameItem(ItemId.Item_Book_Camera_2, "Book [Camera 2]", "arc\\rom\\om\\om003e\\om003e"),
            new GameItem(ItemId.Item_Parasol, "Parasol", "arc\\rom\\om\\om0042\\om0042"),
            new GameItem(ItemId.Item_Fire_Ax, "Fire Ax", "arc\\rom\\om\\om0043\\om0043"),
            new GameItem(ItemId.Item_Sledgehammer, "Sledgehammer", "arc\\rom\\om\\om0044\\om0044"),
            new GameItem(ItemId.Item_Frying_Pan, "Frying Pan", "arc\\rom\\om\\om0045\\om0045"),
            new GameItem(ItemId.Item_Katana, "Katana", "arc\\rom\\om\\om0046\\om0046"),
            new GameItem(ItemId.Item_Chainsaw, "Chainsaw", "arc\\rom\\om\\om0047\\om0047"),
            new GameItem(ItemId.Item_Dumbbell, "Dumbbell", "arc\\rom\\om\\om004a\\om004a"),
            new GameItem(ItemId.Item_Hanger, "Hanger", "arc\\rom\\om\\om004f\\om004f"),
            new GameItem(ItemId.Item_Book_Survival, "Book [Survival]", "arc\\rom\\om\\om0082\\om0082"),
            new GameItem(ItemId.Item_Potted_Plant, "Potted Plant", "arc\\rom\\om\\om0051\\om0051"),
            new GameItem(ItemId.Item_Mega_Man_Buster, "Mega Man Buster", "arc\\rom\\om\\om00a6\\om00a6"),
            new GameItem(ItemId.Item_59, "--------", ""),
            new GameItem(ItemId.Item_Shotgun, "Shotgun", "arc\\rom\\om\\om0054\\om0054"),
            new GameItem(ItemId.Item_61, "-----", ""),
            new GameItem(ItemId.Item_62, "-----", ""),
            new GameItem(ItemId.Item_63, "-----", ""),
            new GameItem(ItemId.Item_Sickle, "Sickle", "arc\\rom\\om\\om0067\\om0067"),
            new GameItem(ItemId.Item_65, "-----", "arc\\rom\\om\\om0214\\om0214"),
            new GameItem(ItemId.Item_Vase, "Vase", "arc\\rom\\om\\om006a\\om006a"),
            new GameItem(ItemId.Item_Painting, "Painting", "arc\\rom\\om\\om006b\\om006b"),
            new GameItem(ItemId.Item_Book_Cult_Initiation_Guide, "Book [Cult Initiation Guide]", "arc\\rom\\om\\om006e\\om006e"),
            new GameItem(ItemId.Item_Queen, "Queen", "arc\\rom\\om\\om0071\\om0071"),
            new GameItem(ItemId.Item_Lawn_Mower, "Lawn Mower", "arc\\rom\\om\\om0073\\om0073"),
            new GameItem(ItemId.Item_Bowing_Ball, "Bowing Ball", "arc\\rom\\om\\om0075\\om0075"),
            new GameItem(ItemId.Item_Stun_Gun, "Stun Gun", "arc\\rom\\om\\om0076\\om0076"),
            new GameItem(ItemId.Item_73, "-----", ""),
            new GameItem(ItemId.Item_Hedge_Trimmer, "Hedge Trimmer", "arc\\rom\\om\\om0078\\om0078"),
            new GameItem(ItemId.Item_Handgun, "Handgun", "arc\\rom\\om\\om0079\\om0079"),
            new GameItem(ItemId.Item_Knife, "Knife", "arc\\rom\\om\\om007a\\om007a"),
            new GameItem(ItemId.Item_Propane_Tank, "Propane Tank", "arc\\rom\\om\\om007b\\om007b"),
            new GameItem(ItemId.Item_78, "-----", ""),
            new GameItem(ItemId.Item_Lead_Pipe, "Lead Pipe", "arc\\rom\\om\\om007d\\om007d"),
            new GameItem(ItemId.Item_Sign, "Sign", "arc\\rom\\om\\om007e\\om007e"),
            new GameItem(ItemId.Item_TV, "TV", "arc\\rom\\om\\om007f\\om007f"),
            new GameItem(ItemId.Item_Submachine_Gun, "Submachine Gun", "arc\\rom\\om\\om0080\\om0080"),
            new GameItem(ItemId.Item_Mannequin, "Mannequin", "arc\\rom\\om\\om0206\\om0206"),
            new GameItem(ItemId.Item_Mannequin_Limb, "Mannequin Limb", "arc\\rom\\om\\om0207\\om0207"),
            new GameItem(ItemId.Item_Mannequin_Limb1, "Mannequin Limb", "arc\\rom\\om\\om0208\\om0208"),
            new GameItem(ItemId.Item_Mannequin_Limb2, "Mannequin Limb", "arc\\rom\\om\\om0209\\om0209"),
            new GameItem(ItemId.Item_Mannequin_Limb3, "Mannequin Limb", "arc\\rom\\om\\om020a\\om020a"),
            new GameItem(ItemId.Item_Mannequin_Limb4, "Mannequin Limb", "arc\\rom\\om\\om020b\\om020b"),
            new GameItem(ItemId.Item_Mannequin1, "Mannequin", "arc\\rom\\om\\om020c\\om020c"),
            new GameItem(ItemId.Item_Mannequin_Limb5, "Mannequin Limb", "arc\\rom\\om\\om020d\\om020d"),
            new GameItem(ItemId.Item_Mannequin_Limb6, "Mannequin Limb", "arc\\rom\\om\\om020e\\om020e"),
            new GameItem(ItemId.Item_Mannequin_Limb7, "Mannequin Limb", "arc\\rom\\om\\om020f\\om020f"),
            new GameItem(ItemId.Item_Mannequin_Limb8, "Mannequin Limb", "arc\\rom\\om\\om0210\\om0210"),
            new GameItem(ItemId.Item_Mannequin_Limb9, "Mannequin Limb", "arc\\rom\\om\\om0211\\om0211"),
            new GameItem(ItemId.Item_Potted_Plant1, "Potted Plant", "arc\\rom\\om\\om0085\\om0085"),
            new GameItem(ItemId.Item_Potted_Plant2, "Potted Plant", "arc\\rom\\om\\om0086\\om0086"),
            new GameItem(ItemId.Item_Cactus, "Cactus", "arc\\rom\\om\\om0087\\om0087"),
            new GameItem(ItemId.Item_Potted_Plant3, "Potted Plant", "arc\\rom\\om\\om0088\\om0088"),
            new GameItem(ItemId.Item_Potted_Plant4, "Potted Plant", "arc\\rom\\om\\om0089\\om0089"),
            new GameItem(ItemId.Item_Potted_Plant5, "Potted Plant", "arc\\rom\\om\\om008a\\om008a"),
            new GameItem(ItemId.Item_Barbell, "Barbell", "arc\\rom\\om\\om0132\\om0132"),
            new GameItem(ItemId.Item_Wine_Cask, "Wine Cask", "arc\\rom\\om\\om008c\\om008c"),
            new GameItem(ItemId.Item_103, "-----", "arc\\rom\\om\\om008d\\om008d"),
            new GameItem(ItemId.Item_104, "-----", "arc\\rom\\om\\om0106\\om0106"),
            new GameItem(ItemId.Item_Gumball_Machine, "Gumball Machine", "arc\\rom\\om\\om008f\\om008f"),
            new GameItem(ItemId.Item_Mega_Man_Buster1, "Mega Man Buster", "arc\\rom\\om\\om00d5\\om00d5"),
            new GameItem(ItemId.Item_107, "-----", "arc\\rom\\om\\om00d5\\om00d5"),
            new GameItem(ItemId.Item_Plasma_TV, "Plasma TV", "arc\\rom\\om\\om0092\\om0092"),
            new GameItem(ItemId.Item_109, "-----", ""),
            new GameItem(ItemId.Item_Uncooked_Pizza, "Uncooked Pizza", "arc\\rom\\om\\om0093\\om0093"),
            new GameItem(ItemId.Item_Golden_Brown_Pizza, "Golden Brown Pizza", "arc\\rom\\om\\om0093\\om0093"),
            new GameItem(ItemId.Item_Rotten_Pizza, "Rotten Pizza", "arc\\rom\\om\\om0093\\om0093"),
            new GameItem(ItemId.Item_Pie, "Pie", "arc\\rom\\om\\om0094\\om0094"),
            new GameItem(ItemId.Item_Nailgun, "Nailgun", "arc\\rom\\om\\om00cc\\om00cc"),
            new GameItem(ItemId.Item_Smokestack, "Smokestack", "arc\\rom\\om\\om00ca\\om00ca"),
            new GameItem(ItemId.Item_Chair3, "Chair", "arc\\rom\\om\\om021e\\om021e"),
            new GameItem(ItemId.Item_Stepladder, "Stepladder", "arc\\rom\\om\\om00cb\\om00cb"),
            new GameItem(ItemId.Item_Toolbox, "Toolbox", "arc\\rom\\om\\om00c9\\om00c9"),
            new GameItem(ItemId.Item_Excavator, "Excavator", "arc\\rom\\om\\om00cf\\om00cf"),
            new GameItem(ItemId.Item_4Door_Car, "4Door Car", "arc\\rom\\om\\om0098\\om0098"),
            new GameItem(ItemId.Item_Track, "Track", "arc\\rom\\om\\om0099\\om0099"),
            new GameItem(ItemId.Item_Hammer, "Hammer", "arc\\rom\\om\\om009a\\om009a"),
            new GameItem(ItemId.Item_Bike, "Bike", "arc\\rom\\om\\om009b\\om009b"),
            new GameItem(ItemId.Item_124, "-----", "arc\\rom\\om\\om00cc\\om00cc"),
            new GameItem(ItemId.Item_Bicycle, "Bicycle", "arc\\rom\\om\\om009d\\om009d"),
            new GameItem(ItemId.Item_126, "-----", ""),
            new GameItem(ItemId.Item_Small_Chainsaw, "Small Chainsaw", "arc\\rom\\om\\om0031\\om0031"),
            new GameItem(ItemId.Item_128, "-----", ""),
            new GameItem(ItemId.Item_129, "-----", ""),
            new GameItem(ItemId.Item_130, "-----", ""),
            new GameItem(ItemId.Item_131, "-----", ""),
            new GameItem(ItemId.Item_Chair4, "Chair", "arc\\rom\\om\\om021f\\om021f"),
            new GameItem(ItemId.Item_Chair5, "Chair", "arc\\rom\\om\\om0220\\om0220"),
            new GameItem(ItemId.Item_Machinegun, "Machinegun", "arc\\rom\\om\\om00a8\\om00a8"),
            new GameItem(ItemId.Item_Sniper_Rifle, "Sniper Rifle", "arc\\rom\\om\\om00a9\\om00a9"),
            new GameItem(ItemId.Item_2_x_4_, "2 x 4 ", "arc\\rom\\om\\om0048\\om0048"),
            new GameItem(ItemId.Item_Boomerang, "Boomerang", "arc\\rom\\om\\om0049\\om0049"),
            new GameItem(ItemId.Item_Bucket, "Bucket", "arc\\rom\\om\\om004d\\om004d"),
            new GameItem(ItemId.Item_Nightstick, "Nightstick", "arc\\rom\\om\\om0074\\om0074"),
            new GameItem(ItemId.Item_Wine, "Wine", "arc\\rom\\om\\om004b\\om004b"),
            new GameItem(ItemId.Item_Electric_Guitar, "Electric Guitar", "arc\\rom\\om\\om004c\\om004c"),
            new GameItem(ItemId.Item_142, "-----", ""),
            new GameItem(ItemId.Item_King_Salmon, "King Salmon", "arc\\rom\\om\\om0006\\om0006"),
            new GameItem(ItemId.Item_144, "-----", ""),
            new GameItem(ItemId.Item_Zucchini, "Zucchini", "arc\\rom\\om\\om0008\\om0008"),
            new GameItem(ItemId.Item_2Door_Car, "2Door Car", "arc\\rom\\om\\om0009\\om0009"),
            new GameItem(ItemId.Item_Chinese_Cleaver, "Chinese Cleaver", "arc\\rom\\om\\om00ac\\om00ac"),
            new GameItem(ItemId.Item_148, "-----", ""),
            new GameItem(ItemId.Item_Book_Japanese_Conversation, "Book [Japanese Conversation]", "arc\\rom\\om\\om0019\\om0019"),
            new GameItem(ItemId.Item_150, "-----", ""),
            new GameItem(ItemId.Item_151, "-----", ""),
            new GameItem(ItemId.Item_Oil_Bucket, "Oil Bucket", "arc\\rom\\om\\om001b\\om001b"),
            new GameItem(ItemId.Item_Cash_Register, "Cash Register", "arc\\rom\\om\\om001c\\om001c"),
            new GameItem(ItemId.Item_Propane_Tank1, "Propane Tank", "arc\\rom\\om\\om001e\\om001e"),
            new GameItem(ItemId.Item_Book_Wrestling, "Book [Wrestling]", "arc\\rom\\om\\om002b\\om002b"),
            new GameItem(ItemId.Item_156, "-----", ""),
            new GameItem(ItemId.Item_157, "-----", ""),
            new GameItem(ItemId.Item_158, "-----", ""),
            new GameItem(ItemId.Item_Rock, "Rock", "arc\\rom\\om\\om0056\\om0056"),
            new GameItem(ItemId.Item_Dishes, "Dishes", "arc\\rom\\om\\om004e\\om004e"),
            new GameItem(ItemId.Item_161, "-----", "arc\\rom\\om\\om004e\\om004e"),
            new GameItem(ItemId.Item_162, "-----", ""),
            new GameItem(ItemId.Item_163, "-----", "arc\\rom\\om\\om005e\\om005e"),
            new GameItem(ItemId.Item_164, "-----", ""),
            new GameItem(ItemId.Item_RC_Bomb, "RC Bomb", "arc\\rom\\om\\om0060\\om0060"),
            new GameItem(ItemId.Item_Weapon_Cart, "Weapon Cart", "arc\\rom\\om\\om0061\\om0061"),
            new GameItem(ItemId.Item_167, "-----", ""),
            new GameItem(ItemId.Item_Sword, "Sword", "arc\\rom\\om\\om0064\\om0064"),
            new GameItem(ItemId.Item_169, "-----", "arc\\rom\\om\\om006c\\om006c"),
            new GameItem(ItemId.Item_170, "-----", "arc\\rom\\om\\om006d\\om006d"),
            new GameItem(ItemId.Item_171, "-----", ""),
            new GameItem(ItemId.Item_172, "-----", ""),
            new GameItem(ItemId.Item_173, "-----", "arc\\rom\\om\\om0222\\om0222"),
            new GameItem(ItemId.Item_174, "-----", "arc\\rom\\om\\om0223\\om0223"),
            new GameItem(ItemId.Item_175, "-----", "arc\\rom\\om\\om0224\\om0224"),
            new GameItem(ItemId.Item_176, "-----", "arc\\rom\\om\\om0225\\om0225"),
            new GameItem(ItemId.Item_177, "-----", "arc\\rom\\om\\om0226\\om0226"),
            new GameItem(ItemId.Item_178, "-----", "arc\\rom\\om\\om0227\\om0227"),
            new GameItem(ItemId.Item_179, "-----", "arc\\rom\\om\\om0228\\om0228"),
            new GameItem(ItemId.Item_180, "-----", ""),
            new GameItem(ItemId.Item_Cardboard_Box, "Cardboard Box", "arc\\rom\\om\\om022d\\om022d"),
            new GameItem(ItemId.Item_182, "-----", ""),
            new GameItem(ItemId.Item_183, "-----", ""),
            new GameItem(ItemId.Item_Shower_Head, "Shower Head", "arc\\rom\\om\\om0096\\om0096"),
            new GameItem(ItemId.Item_185, "-----", ""),
            new GameItem(ItemId.Item_Antimaterial_Rifle, "Antimaterial Rifle", "arc\\rom\\om\\om00a5\\om00a5"),
            new GameItem(ItemId.Item_Pipe_Bomb, "Pipe Bomb", "arc\\rom\\om\\om00a7\\om00a7"),
            new GameItem(ItemId.Item_188, "-----", ""),
            new GameItem(ItemId.Item_Combat_Knife, "Combat Knife", "arc\\rom\\om\\om00ad\\om00ad"),
            new GameItem(ItemId.Item_Molotov_Cocktail, "Molotov Cocktail", "arc\\rom\\om\\om005b\\om005b"),
            new GameItem(ItemId.Item_191, "-----", ""),
            new GameItem(ItemId.Item_192, "-----", "arc\\rom\\om\\om0011\\om0011"),
            new GameItem(ItemId.Item_Book_Hobby, "Book [Hobby]", "arc\\rom\\om\\om00b1\\om00b1"),
            new GameItem(ItemId.Item_194, "-----", ""),
            new GameItem(ItemId.Item_Ceremonial_Sword, "Ceremonial Sword", "arc\\rom\\om\\om00b4\\om00b4"),
            new GameItem(ItemId.Item_Novelty_Mask, "Novelty Mask", "arc\\rom\\om\\om00b5\\om00b5"),
            new GameItem(ItemId.Item_Novelty_Mask1, "Novelty Mask", "arc\\rom\\om\\om00b6\\om00b6"),
            new GameItem(ItemId.Item_Novelty_Mask2, "Novelty Mask", "arc\\rom\\om\\om00b7\\om00b7"),
            new GameItem(ItemId.Item_Novelty_Mask3, "Novelty Mask", "arc\\rom\\om\\om00b8\\om00b8"),
            new GameItem(ItemId.Item_Painting1, "Painting", "arc\\rom\\om\\om00d8\\om00d8"),
            new GameItem(ItemId.Item_201, "-----", ""),
            new GameItem(ItemId.Item_Bench, "Bench", "arc\\rom\\om\\om0052\\om0052"),
            new GameItem(ItemId.Item_Steel_Rack, "Steel Rack", "arc\\rom\\om\\om0053\\om0053"),
            new GameItem(ItemId.Item_Shelf, "Shelf", "arc\\rom\\om\\om0038\\om0038"),
            new GameItem(ItemId.Item_Sausage_Rack, "Sausage Rack", "arc\\rom\\om\\om003b\\om003b"),
            new GameItem(ItemId.Item_206, "-----", ""),
            new GameItem(ItemId.Item_207, "-----", ""),
            new GameItem(ItemId.Item_Corn, "Corn", "arc\\rom\\om\\om0022\\om0022"),
            new GameItem(ItemId.Item_Squash, "Squash", "arc\\rom\\om\\om002a\\om002a"),
            new GameItem(ItemId.Item_Cabbage, "Cabbage", "arc\\rom\\om\\om002e\\om002e"),
            new GameItem(ItemId.Item_Japanese_Radish, "Japanese Radish", "arc\\rom\\om\\om0030\\om0030"),
            new GameItem(ItemId.Item_Lettuce, "Lettuce", "arc\\rom\\om\\om0033\\om0033"),
            new GameItem(ItemId.Item_Red_Cabbage, "Red Cabbage", "arc\\rom\\om\\om0066\\om0066"),
            new GameItem(ItemId.Item_Baguette, "Baguette", "arc\\rom\\om\\om003a\\om003a"),
            new GameItem(ItemId.Item_Melon, "Melon", "arc\\rom\\om\\om003c\\om003c"),
            new GameItem(ItemId.Item_Grapefruit, "Grapefruit", "arc\\rom\\om\\om003d\\om003d"),
            new GameItem(ItemId.Item_Orange, "Orange", "arc\\rom\\om\\om0050\\om0050"),
            new GameItem(ItemId.Item_Orange_Juice, "Orange Juice", "arc\\rom\\om\\om0058\\om0058"),
            new GameItem(ItemId.Item_Milk, "Milk", "arc\\rom\\om\\om0059\\om0059"),
            new GameItem(ItemId.Item_Coffee_Creamer, "Coffee Creamer", "arc\\rom\\om\\om005a\\om005a"),
            new GameItem(ItemId.Item_Yogurt, "Yogurt", "arc\\rom\\om\\om0063\\om0063"),
            new GameItem(ItemId.Item_Cheese, "Cheese", "arc\\rom\\om\\om0065\\om0065"),
            new GameItem(ItemId.Item_223, "-----", ""),
            new GameItem(ItemId.Item_224, "-----", ""),
            new GameItem(ItemId.Item_225, "-----", "arc\\rom\\om\\om008b\\om008b"),
            new GameItem(ItemId.Item_226, "-----", ""),
            new GameItem(ItemId.Item_227, "-----", ""),
            new GameItem(ItemId.Item_Shampoo, "Shampoo", "arc\\rom\\om\\om00bc\\om00bc"),
            new GameItem(ItemId.Item_Pet_Food, "Pet Food", "arc\\rom\\om\\om00bd\\om00bd"),
            new GameItem(ItemId.Item_Cookies, "Cookies", "arc\\rom\\om\\om00be\\om00be"),
            new GameItem(ItemId.Item_Baking_Ingredients, "Baking Ingredients", "arc\\rom\\om\\om00bf\\om00bf"),
            new GameItem(ItemId.Item_Cooking_Oil, "Cooking Oil", "arc\\rom\\om\\om00c0\\om00c0"),
            new GameItem(ItemId.Item_Condiment, "Condiment", "arc\\rom\\om\\om00c1\\om00c1"),
            new GameItem(ItemId.Item_Canned_Sauce, "Canned Sauce", "arc\\rom\\om\\om00c2\\om00c2"),
            new GameItem(ItemId.Item_Canned_Food, "Canned Food", "arc\\rom\\om\\om00c3\\om00c3"),
            new GameItem(ItemId.Item_Drink_Cans, "Drink Cans", "arc\\rom\\om\\om00c4\\om00c4"),
            new GameItem(ItemId.Item_Frozen_Vegetables, "Frozen Vegetables", "arc\\rom\\om\\om00c5\\om00c5"),
            new GameItem(ItemId.Item_Apple, "Apple", "arc\\rom\\om\\om00c6\\om00c6"),
            new GameItem(ItemId.Item_Ice_Pops, "Ice Pops", "arc\\rom\\om\\om00c7\\om00c7"),
            new GameItem(ItemId.Item_Milk1, "Milk", "arc\\rom\\om\\om00c8\\om00c8"),
            new GameItem(ItemId.Item_Rat_Stick, "Rat Stick", "arc\\rom\\om\\om0090\\om0090"),
            new GameItem(ItemId.Item_Rat_Saucer, "Rat Saucer", "arc\\rom\\om\\om0097\\om0097"),
            new GameItem(ItemId.Item_Painting2, "Painting", "arc\\rom\\om\\om00d7\\om00d7"),
            new GameItem(ItemId.Item_Saw_Blade, "Saw Blade", "arc\\rom\\om\\om00b9\\om00b9"),
            new GameItem(ItemId.Item_245, "-----", ""),
            new GameItem(ItemId.Item_Skylight, "Skylight", "arc\\rom\\om\\om00ce\\om00ce"),
            new GameItem(ItemId.Item_Fence, "Fence", "arc\\rom\\om\\om0091\\om0091"),
            new GameItem(ItemId.Item_Painting3, "Painting", "arc\\rom\\om\\om00d6\\om00d6"),
            new GameItem(ItemId.Item_249, "-----", ""),
            new GameItem(ItemId.Item_250, "-----", ""),
            new GameItem(ItemId.Item_Stuffed_Bear, "Stuffed Bear", "arc\\rom\\om\\om0068\\om0068"),
            new GameItem(ItemId.Item_Mailbox, "Mailbox", "arc\\rom\\om\\om00cd\\om00cd"),
            new GameItem(ItemId.Item_253, "-----", ""),
            new GameItem(ItemId.Item_Mailbox_Post, "Mailbox Post", "arc\\rom\\om\\om00cd\\om00cd"),
            new GameItem(ItemId.Item_Painting4, "Painting", "arc\\rom\\om\\om00d3\\om00d3"),
            new GameItem(ItemId.Item_Sign1, "Sign", "arc\\rom\\om\\om00d9\\om00d9"),
            new GameItem(ItemId.Item_Severed_Arm, "Severed Arm", "arc\\rom\\om\\om00da\\om00da"),
            new GameItem(ItemId.Item_258, "-----", ""),
            new GameItem(ItemId.Item_259, "-----", ""),
            new GameItem(ItemId.Item_Plywood_Panel, "Plywood Panel", "arc\\rom\\om\\om022f\\om022f"),
            new GameItem(ItemId.Item_261, "-----", ""),
            new GameItem(ItemId.Item_262, "-----", ""),
            new GameItem(ItemId.Item_263, "-----", ""),
            new GameItem(ItemId.Item_CD, "CD", "arc\\rom\\om\\om022e\\om022e"),
            new GameItem(ItemId.Item_Heavy_Machine_Gun, "Heavy Machine Gun", "arc\\rom\\om\\om0025\\om0025"),
            new GameItem(ItemId.Item_266, "-----", ""),
            new GameItem(ItemId.Item_267, "-----", ""),
            new GameItem(ItemId.Item_268, "-----", ""),
            new GameItem(ItemId.Item_Perfume_Prop, "Perfume Prop", "arc\\rom\\om\\om00e4\\om00e4"),
            new GameItem(ItemId.Item_Lipstick_Prop, "Lipstick Prop", "arc\\rom\\om\\om00e5\\om00e5"),
            new GameItem(ItemId.Item_271, "-----", ""),
            new GameItem(ItemId.Item_Book_Cooking, "Book [Cooking]", "arc\\rom\\om\\om00e7\\om00e7"),
            new GameItem(ItemId.Item_Book_Lifestyle_Magazine, "Book [Lifestyle Magazine]", "arc\\rom\\om\\om00e8\\om00e8"),
            new GameItem(ItemId.Item_Book_Tools, "Book [Tools]", "arc\\rom\\om\\om00e9\\om00e9"),
            new GameItem(ItemId.Item_Book_Sports, "Book [Sports]", "arc\\rom\\om\\om00ea\\om00ea"),
            new GameItem(ItemId.Item_Book_Criminal_Biography, "Book [Criminal Biography]", "arc\\rom\\om\\om00eb\\om00eb"),
            new GameItem(ItemId.Item_Book_Travel, "Book [Travel]", "arc\\rom\\om\\om00ec\\om00ec"),
            new GameItem(ItemId.Item_Book_Interior_Design, "Book [Interior Design]", "arc\\rom\\om\\om00ed\\om00ed"),
            new GameItem(ItemId.Item_Book_Entertainment, "Book [Entertainment]", "arc\\rom\\om\\om00ef\\om00ef"),
            new GameItem(ItemId.Item_Book_Camera_1, "Book [Camera 1]", "arc\\rom\\om\\om00f0\\om00f0"),
            new GameItem(ItemId.Item_Book_Skateboarding, "Book [Skateboarding]", "arc\\rom\\om\\om00f1\\om00f1"),
            new GameItem(ItemId.Item_Book_Wartime_Photography, "Book [Wartime Photography]", "arc\\rom\\om\\om00f2\\om00f2"),
            new GameItem(ItemId.Item_Book_Weekly_Photo_Magazine, "Book [Weekly Photo Magazine]", "arc\\rom\\om\\om00f3\\om00f3"),
            new GameItem(ItemId.Item_Book_Horror_Novel_1, "Book [Horror Novel 1]", "arc\\rom\\om\\om00f4\\om00f4"),
            new GameItem(ItemId.Item_Book_World_News, "Book [World News]", "arc\\rom\\om\\om00f5\\om00f5"),
            new GameItem(ItemId.Item_Book_Health_1, "Book [Health 1]", "arc\\rom\\om\\om00f6\\om00f6"),
            new GameItem(ItemId.Item_Book_Cycling, "Book [Cycling]", "arc\\rom\\om\\om00f7\\om00f7"),
            new GameItem(ItemId.Item_Book_Health_2, "Book [Health 2]", "arc\\rom\\om\\om00f8\\om00f8"),
            new GameItem(ItemId.Item_Book_Horror_Novel_2, "Book [Horror Novel 2]", "arc\\rom\\om\\om00f9\\om00f9"),
            new GameItem(ItemId.Item_Electric_Guitar1, "Electric Guitar", "arc\\rom\\om\\om00a3\\om00a3"),
            new GameItem(ItemId.Item_Chair6, "Chair", "arc\\rom\\om\\om0014\\om0014"),
            new GameItem(ItemId.Item_Chair7, "Chair", "arc\\rom\\om\\om0018\\om0018"),
            new GameItem(ItemId.Item_Chair8, "Chair", "arc\\rom\\om\\om0055\\om0055"),
            new GameItem(ItemId.Item_Stool, "Stool", "arc\\rom\\om\\om0200\\om0200"),
            new GameItem(ItemId.Item_Chair9, "Chair", "arc\\rom\\om\\om0212\\om0212"),
            new GameItem(ItemId.Item_296, "-----", ""),
            new GameItem(ItemId.Item_297, "-----", ""),
            new GameItem(ItemId.Item_298, "-----", ""),
            new GameItem(ItemId.Item_299, "-----", ""),
            new GameItem(ItemId.Item_300, "-----", ""),
            new GameItem(ItemId.Item_301, "-----", ""),
            new GameItem(ItemId.Item_Juice_Quick_Step, "Juice Quick Step", "arc\\rom\\om\\om0234\\om0234"),
            new GameItem(ItemId.Item_Juice_Randomizer, "Juice Randomizer", "arc\\rom\\om\\om0235\\om0235"),
            new GameItem(ItemId.Item_Juice_Untouchable, "Juice Untouchable", "arc\\rom\\om\\om0236\\om0236"),
            new GameItem(ItemId.Item_Juice_Spitfire, "Juice Spitfire", "arc\\rom\\om\\om0237\\om0237"),
            new GameItem(ItemId.Item_Juice_Nectar, "Juice Nectar", "arc\\rom\\om\\om0238\\om0238"),
            new GameItem(ItemId.Item_Juice_Energizer, "Juice Energizer", "arc\\rom\\om\\om0239\\om0239"),
            new GameItem(ItemId.Item_Juice_Zombait, "Juice Zombait", "arc\\rom\\om\\om023A\\om023A"),
            new GameItem(ItemId.Item_309, "-----", ""),
            new GameItem(ItemId.Item_310, "-----", ""),
            new GameItem(ItemId.Item_311, "-----", ""),
            new GameItem(ItemId.Item_Melted_Ice_Pops, "Melted Ice Pops", "arc\\rom\\om\\om00c7\\om00c7"),
            new GameItem(ItemId.Item_Thawed_Vegetables, "Thawed Vegetables", "arc\\rom\\om\\om00c5\\om00c5"),
        };

        #endregion
    }
}
