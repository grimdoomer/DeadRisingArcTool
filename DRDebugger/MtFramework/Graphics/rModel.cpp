/*

*/

#include "rModel.h"

void rModelImpl::InitializeLua()
{
	// Register Material:
	g_LuaState.new_usertype<Material>("Material",
		CPP_FIELD(Material, Flags),
		CPP_FIELD(Material, Unk1),
		CPP_FIELD(Material, Unk2),
		CPP_FIELD(Material, Unk3),
		CPP_FIELD(Material, Unk4),
		CPP_FIELD(Material, PrimaryShader),
		CPP_FIELD(Material, Unk5),
		CPP_FIELD(Material, Unk5),
		CPP_FIELD(Material, Unk6),
		CPP_FIELD(Material, Unk7),
		CPP_FIELD(Material, Unk8),
		CPP_FIELD(Material, BaseMapTexture),
		CPP_FIELD(Material, NormalMapTexture),
		CPP_FIELD(Material, MaskMapTexture),
		CPP_FIELD(Material, LightmapTexture),
		CPP_FIELD(Material, TextureIndex5),
		CPP_FIELD(Material, TextureIndex6),
		CPP_FIELD(Material, TextureIndex7),
		CPP_FIELD(Material, TextureIndex8),
		CPP_FIELD(Material, TextureIndex9),
		CPP_FIELD(Material, Transparency),
		CPP_FIELD(Material, Unk11),
		CPP_FIELD(Material, FresnelFactor),
		CPP_FIELD(Material, FresnelBias),
		CPP_FIELD(Material, SpecularPow),
		CPP_FIELD(Material, EnvmapPower),
		CPP_FIELD(Material, LightMapScale),
		CPP_FIELD(Material, DetailFactor),
		CPP_FIELD(Material, DetailWrap),
		CPP_FIELD(Material, Unk22),
		CPP_FIELD(Material, Unk23),
		CPP_FIELD(Material, Transmit),
		CPP_FIELD(Material, Parallax),
		CPP_FIELD(Material, Unk32),
		CPP_FIELD(Material, Unk33),
		CPP_FIELD(Material, Unk34),
		CPP_FIELD(Material, Unk35));

	// Register rModel:
	g_LuaState.new_usertype<rModel>("rModel",
		sol::base_classes, sol::bases<cResource>(),
		CPP_FIELD(rModel, pJointData1),
		CPP_FIELD(rModel, JointCount),
		CPP_FIELD(rModel, pJointData2),
		CPP_FIELD(rModel, pJointData3),
		CPP_FIELD(rModel, pPrimitiveData),
		CPP_FIELD(rModel, PrimitiveCount),
		//CPP_FIELD(rModel, pMaterials),
		"pMaterials", sol::property([](rModel& model) { return sol::as_container(*model.pMaterials); }),
		CPP_FIELD(rModel, MaterialCount),
		CPP_FIELD(rModel, PolygonCount),
		CPP_FIELD(rModel, VertexCount),
		CPP_FIELD(rModel, IndiceCount),
		CPP_FIELD(rModel, Count1),
		CPP_FIELD(rModel, Count2),
		CPP_FIELD(rModel, pIndiceManager),
		CPP_FIELD(rModel, pVertexManager1),
		CPP_FIELD(rModel, pVertexManager2),
		CPP_FIELD(rModel, BoundingBoxMin),
		CPP_FIELD(rModel, BoundingBoxMax),
		CPP_FIELD(rModel, MidDist),
		CPP_FIELD(rModel, LowDist),
		CPP_FIELD(rModel, LightGroup),
		CPP_FIELD(rModel, Scale),
		CPP_FIELD(rModel, Unk));

	// Register rModelImpl:
}