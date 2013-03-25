// This is the main DLL file.

#include "stdafx.h"

#include "SpeedTreeWrapper.h"

using namespace Multiverse;

// default constructor
SpeedTreeWrapper::SpeedTreeWrapper()
{

	speedTree = new CSpeedTreeRT();

}

SpeedTreeWrapper::SpeedTreeWrapper(bool isInstance, SpeedTreeWrapper *instanceOf, CSpeedTreeRT *speedTree):
	isInstance(isInstance),
	instanceOf(instanceOf),
	speedTree(speedTree)
{

}

SpeedTreeWrapper *
SpeedTreeWrapper::MakeInstance()
{
	CSpeedTreeRT *tmpTree = speedTree->MakeInstance();

	return new SpeedTreeWrapper(true, this, tmpTree);
}

SpeedTreeWrapper * 
SpeedTreeWrapper::Clone(V3 position, unsigned int seed)
{
	CSpeedTreeRT *tmpTree = speedTree->Clone(position.x, position.y, position.z, seed);

	return new SpeedTreeWrapper(false, NULL, tmpTree);	
}


bool 
SpeedTreeWrapper::LoadTree(String *filename)
{
	char *unmanagedFilename = GetUnManagedString(filename);

	bool ret = speedTree->LoadTree(unmanagedFilename);

	Marshal::FreeHGlobal(static_cast<IntPtr>(const_cast<void*>(static_cast<const void*>(unmanagedFilename))));

	return ret;
}

bool 
SpeedTreeWrapper::LoadTree(unsigned char buffer __gc [], unsigned int len)
{
	unsigned char *umbuf = (unsigned char *)((Marshal::AllocHGlobal(len)).ToPointer());

	
	for ( int i = 0; i < len; i++ ) {
		umbuf[i] = buffer[i];
	}

	bool ret = speedTree->LoadTree(umbuf, len);

	Marshal::FreeHGlobal(static_cast<IntPtr>(const_cast<void*>(static_cast<const void*>(umbuf))));

	return ret;
}

bool
SpeedTreeWrapper::Compute(float transform __gc [], unsigned int seed, bool compositeStrips)
{
	float m[16];
	for ( int i = 0; i < 16; i++ ) {
		m[i] = transform[i];
	}

	bool ret = speedTree->Compute(m, seed, compositeStrips);

	return ret;
}

void
SpeedTreeWrapper::DeleteTransientData()
{
	speedTree->DeleteTransientData();
}

V3
SpeedTreeWrapper::get_TreePosition()
{
	const float *pos;

	pos = speedTree->GetTreePosition();

	return V3(pos[0], pos[1], pos[2]);
}

void 
SpeedTreeWrapper::set_TreePosition(V3 position)
{
	speedTree->SetTreePosition(position.x, position.y, position.z);
}


void 
SpeedTreeWrapper::GetTreeSize(float __gc &size, float __gc &variance)
{
	float locSize;
	float locVar;
	speedTree->GetTreeSize(locSize, locVar);

	size = locSize;
	variance = locVar;
}

void 
SpeedTreeWrapper::SetTreeSize(float size, float variance)
{
	speedTree->SetTreeSize(size, variance);
}

unsigned int 
SpeedTreeWrapper::get_Seed() 
{ 
	return speedTree->GetSeed();
}

void 
SpeedTreeWrapper::set_LeafTargetAlphaMask(unsigned char mask)
{
	speedTree->SetLeafTargetAlphaMask(mask);
}

LightingMethod 
SpeedTreeWrapper::get_BranchLightingMethod()
{
	CSpeedTreeRT::ELightingMethod m = speedTree->GetBranchLightingMethod();

	LightingMethod ret;

	switch ( m ) 
	{
	case CSpeedTreeRT::LIGHT_STATIC:
		ret = LightStatic;
		break;
	case CSpeedTreeRT::LIGHT_DYNAMIC:
		ret = LightDynamic;
		break;
	}

	return ret;
}

void 
SpeedTreeWrapper::
set_BranchLightingMethod(LightingMethod method)
{
	CSpeedTreeRT::ELightingMethod m;
	switch ( method ) 
	{
	case LightStatic:
		m = CSpeedTreeRT::LIGHT_STATIC;
		break;
	case LightDynamic:
		m = CSpeedTreeRT::LIGHT_DYNAMIC;
		break;
	}
	speedTree->SetBranchLightingMethod(m);
}

LightingMethod 
SpeedTreeWrapper::get_FrondLightingMethod()
{
	CSpeedTreeRT::ELightingMethod m = speedTree->GetFrondLightingMethod();

	LightingMethod ret;

	switch ( m ) 
	{
	case CSpeedTreeRT::LIGHT_STATIC:
		ret = LightStatic;
		break;
	case CSpeedTreeRT::LIGHT_DYNAMIC:
		ret = LightDynamic;
		break;
	}

	return ret;
}

void 
SpeedTreeWrapper::
set_FrondLightingMethod(LightingMethod method)
{
	CSpeedTreeRT::ELightingMethod m;
	switch ( method ) 
	{
	case LightStatic:
		m = CSpeedTreeRT::LIGHT_STATIC;
		break;
	case LightDynamic:
		m = CSpeedTreeRT::LIGHT_DYNAMIC;
		break;
	}
	speedTree->SetFrondLightingMethod(m);
}

LightingMethod 
SpeedTreeWrapper::get_LeafLightingMethod()
{
	CSpeedTreeRT::ELightingMethod m = speedTree->GetLeafLightingMethod();

	LightingMethod ret;

	switch ( m ) 
	{
	case CSpeedTreeRT::LIGHT_STATIC:
		ret = LightStatic;
		break;
	case CSpeedTreeRT::LIGHT_DYNAMIC:
		ret = LightDynamic;
		break;
	}

	return ret;
}

void 
SpeedTreeWrapper::
set_LeafLightingMethod(LightingMethod method)
{
	CSpeedTreeRT::ELightingMethod m;
	switch ( method ) 
	{
	case LightStatic:
		m = CSpeedTreeRT::LIGHT_STATIC;
		break;
	case LightDynamic:
		m = CSpeedTreeRT::LIGHT_DYNAMIC;
		break;
	}
	speedTree->SetLeafLightingMethod(m);
}


TreeMaterial __gc *
SpeedTreeWrapper::MaterialFromFloats(const float *m)
{
	TreeMaterial __gc *mat = new TreeMaterial();

	mat->Diffuse = Color(m[0], m[1], m[2]);
	mat->Ambient = Color(m[3], m[4], m[5]);
	mat->Specular = Color(m[6], m[7], m[8]);
	mat->Emmisive = Color(m[9], m[10], m[11]);
	mat->shininess = m[12];

	return mat;
}

void 
SpeedTreeWrapper::MaterialToFloats(TreeMaterial __gc *mat, float m[])
{
	m[0] = mat->Diffuse.r;
	m[1] = mat->Diffuse.g;
	m[2] = mat->Diffuse.b;

	m[3] = mat->Ambient.r;
	m[4] = mat->Ambient.g;
	m[5] = mat->Ambient.b;

	m[6] = mat->Specular.r;
	m[7] = mat->Specular.g;
	m[8] = mat->Specular.b;

	m[9] = mat->Emmisive.r;
	m[10] = mat->Emmisive.g;
	m[11] = mat->Emmisive.b;

	m[12] = mat->shininess;

	return;
}


TreeMaterial __gc *
SpeedTreeWrapper::get_BranchMaterial()
{
	const float *material;
	material = speedTree->GetBranchMaterial();

	return MaterialFromFloats(material);
}

void 
SpeedTreeWrapper::set_BranchMaterial(TreeMaterial __gc *mat)
{
	float material[13];
	MaterialToFloats(mat, material);

	speedTree->SetBranchMaterial(material);
}


TreeMaterial __gc *
SpeedTreeWrapper::get_FrondMaterial()
{
	const float *material;
	material = speedTree->GetFrondMaterial();

	return MaterialFromFloats(material);
}

void 
SpeedTreeWrapper::set_FrondMaterial(TreeMaterial __gc *mat)
{
	float material[13];

	MaterialToFloats(mat, material);

	speedTree->SetFrondMaterial(material);
}

TreeMaterial __gc *
SpeedTreeWrapper::get_LeafMaterial()
{
	const float *material;
	material = speedTree->GetLeafMaterial();

	return MaterialFromFloats(material);
}

void 
SpeedTreeWrapper::set_LeafMaterial(TreeMaterial __gc *mat)
{
	float material[13];

	MaterialToFloats(mat, material);

	speedTree->SetLeafMaterial(material);
}

float 
SpeedTreeWrapper::get_LeafLightingAdjustment()
{
	return speedTree->GetLeafLightingAdjustment();
}

void 
SpeedTreeWrapper::set_LeafLightingAdjustment(float adj)
{
	speedTree->SetLeafLightingAdjustment(adj);
}

TreeLight __gc *
SpeedTreeWrapper::get_LightAttributes(unsigned int lightIndex)
{
	const float *lightAttr;
	lightAttr = speedTree->GetLightAttributes(lightIndex);

	TreeLight __gc *light = new TreeLight();

	light->position = V3(lightAttr[0], lightAttr[1], lightAttr[2]);
	light->Diffuse = Color(lightAttr[3], lightAttr[4], lightAttr[5]);
	light->Ambient = Color(lightAttr[6], lightAttr[7], lightAttr[8]);
	light->Specular = Color(lightAttr[9], lightAttr[10], lightAttr[11]);
	light->directional = ( lightAttr[12] == 0 );
	light->attenuationConst = lightAttr[13];
	light->attenuationLinear = lightAttr[14];
	light->attenuationQuad = lightAttr[15];

	return light;
}

void 
SpeedTreeWrapper::set_LightAttributes(unsigned int lightIndex, TreeLight __gc *light)
{
	float lightAttr[16];

	lightAttr[0] = light->position.x;
	lightAttr[1] = light->position.y;
	lightAttr[2] = light->position.z;

	lightAttr[3] = light->Diffuse.r;
	lightAttr[4] = light->Diffuse.g;
	lightAttr[5] = light->Diffuse.b;

	lightAttr[6] = light->Ambient.r;
	lightAttr[7] = light->Ambient.g;
	lightAttr[8] = light->Ambient.b;

	lightAttr[9] = light->Specular.r;
	lightAttr[10] = light->Specular.g;
	lightAttr[11] = light->Specular.b;

	if ( light->directional ) {
		lightAttr[12] = 0;
	} else {
		lightAttr[12] = 1;
	}

	lightAttr[13] = light->attenuationConst;
	lightAttr[14] = light->attenuationLinear;
	lightAttr[15] = light->attenuationQuad;

	speedTree->SetLightAttributes(lightIndex, lightAttr);

	return;
}

bool 
SpeedTreeWrapper::get_LightState(unsigned int lightIndex)
{
	return speedTree->GetLightState(lightIndex);
}

void 
SpeedTreeWrapper::set_LightState(unsigned int lightIndex, bool state)
{
	speedTree->SetLightState(lightIndex, state);
}

StaticLightingType
SpeedTreeWrapper::get_StaticLightingStyle()
{
	return (StaticLightingType)speedTree->GetStaticLightingStyle();
}

void 
SpeedTreeWrapper::set_StaticLightingStyle(StaticLightingType style)
{
	speedTree->SetStaticLightingStyle((CSpeedTreeRT::EStaticLightingStyle)style);
}

TreeCamera __gc *
SpeedTreeWrapper::get_Camera()
{
	float position[3];
	float direction[3];

	CSpeedTreeRT::GetCamera(position, direction);

	TreeCamera __gc *camera = new TreeCamera();
	camera->position = V3(position[0], position[1], position[2]);
	camera->direction = V3(direction[0], direction[1], direction[2]);

	return camera;
}

void 
SpeedTreeWrapper::set_Camera(TreeCamera __gc *camera)
{
	float position[3];
	float direction[3];

	position[0] = camera->position.x;
	position[1] = camera->position.y;
	position[2] = camera->position.z;

	direction[0] = camera->direction.x;
	direction[1] = camera->direction.y;
	direction[2] = camera->direction.z;

	CSpeedTreeRT::SetCamera(position, direction);
}

void 
SpeedTreeWrapper::GetGeometry(TreeGeometry __gc *treeGeometry, GeometryFlags flags, 
	short overrideBranchLodValue, short overrideFrondLodValue, short overrideLeafLodValue)
{
	speedTree->GetGeometry(*treeGeometry->UnmanagedGeometry, flags, overrideBranchLodValue, overrideFrondLodValue, overrideLeafLodValue);
}

TreeTextures __gc *
SpeedTreeWrapper::get_Textures()
{
	CSpeedTreeRT::STextures tex;

	TreeTextures *outTex = new TreeTextures(); 

	speedTree->GetTextures(tex);

	outTex->BranchTextureFilename = new String(tex.m_pBranchTextureFilename);
	outTex->CompositeFilename = new String(tex.m_pCompositeFilename);
	outTex->SelfShadowFilename = new String(tex.m_pSelfShadowFilename);

	if ( tex.m_uiFrondTextureCount > 0 ) {
		outTex->FrondTextureFilenames = __gc new String __gc*[tex.m_uiFrondTextureCount];
		for ( int i = 0; i < tex.m_uiFrondTextureCount; i++ ) {
			outTex->FrondTextureFilenames[i] = new String(tex.m_pFrondTextureFilenames[i]);
		}
	}
	if ( tex.m_uiLeafTextureCount > 0 ) {
		outTex->LeafTextureFilenames = __gc new String __gc *[tex.m_uiLeafTextureCount];
		for ( int i = 0; i < tex.m_uiLeafTextureCount; i++ ) {
			outTex->LeafTextureFilenames[i] = new String(tex.m_pLeafTextureFilenames[i]);
		}
	}

	return outTex;
}

bool 
SpeedTreeWrapper::get_TextureFlip()
{
	return speedTree->GetTextureFlip();
}

void 
SpeedTreeWrapper::set_TextureFlip(bool flip)
{
	return speedTree->SetTextureFlip(flip);
}

TreeBox __gc *
SpeedTreeWrapper::get_BoundingBox()
{
	float bounds[6];

	speedTree->GetBoundingBox(bounds);

	return __gc new TreeBox(V3(bounds[0], bounds[1], bounds[2]),
		V3(bounds[3], bounds[4], bounds[5]));
}

void 
SpeedTreeWrapper::GetLodLimits(float __gc & near, float __gc &far)
{
	float locNear;
	float locFar;
	speedTree->GetLodLimits(locNear, locFar);

	near = locNear;
	far = locFar;
}

void
SpeedTreeWrapper::SetLodLimits(float near, float far)
{
	speedTree->SetLodLimits(near, far);
}

void 
SpeedTreeWrapper::SetDropToBillboard(bool flag)
{
	CSpeedTreeRT::SetDropToBillboard(flag);
}

void
SpeedTreeWrapper::ComputeLodLevel()
{
	speedTree->ComputeLodLevel();
}

void 
SpeedTreeWrapper::SetWindStrengthAndLeafAngles(float newStrength, float rockAngles __gc [], float rustleAngles __gc[])
{
	float *rock = (float *)((Marshal::AllocHGlobal(sizeof(float) * rockAngles->Length )).ToPointer());
	float *rustle = (float *)((Marshal::AllocHGlobal(sizeof(float) * rustleAngles->Length )).ToPointer());

	for ( int i = 0; i < rockAngles->Length; i++ ) {
		rock[i] = rockAngles[i];
		rustle[i] = rustleAngles[i];
	}

	speedTree->SetWindStrengthAndLeafAngles(newStrength, rock, rustle, rockAngles->Length);

	Marshal::FreeHGlobal(static_cast<IntPtr>(const_cast<void*>(static_cast<const void*>(rock))));
	Marshal::FreeHGlobal(static_cast<IntPtr>(const_cast<void*>(static_cast<const void*>(rustle))));
}


WindMethod 
SpeedTreeWrapper::get_BranchWindMethod()
{
	return (WindMethod) speedTree->GetBranchWindMethod();
}

void 
SpeedTreeWrapper::set_BranchWindMethod(WindMethod method)
{
	speedTree->SetBranchWindMethod((CSpeedTreeRT::EWindMethod)method);
}

WindMethod 
SpeedTreeWrapper::get_FrondWindMethod()
{
	return (WindMethod) speedTree->GetFrondWindMethod();
}

void 
SpeedTreeWrapper::set_FrondWindMethod(WindMethod method)
{
	speedTree->SetFrondWindMethod((CSpeedTreeRT::EWindMethod)method);
}

WindMethod 
SpeedTreeWrapper::get_LeafWindMethod()
{
	return (WindMethod) speedTree->GetLeafWindMethod();
}

void 
SpeedTreeWrapper::set_LeafWindMethod(WindMethod method)
{
	speedTree->SetLeafWindMethod((CSpeedTreeRT::EWindMethod)method);
}

void 
SpeedTreeWrapper::set_NumLeafRockingGroups(unsigned int n)
{
	speedTree->SetNumLeafRockingGroups(n);
}

unsigned int 
SpeedTreeWrapper::get_NumBranchLodLevels()
{
	return speedTree->GetNumBranchLodLevels();
}

unsigned int 
SpeedTreeWrapper::get_NumFrondLodLevels()
{
	return speedTree->GetNumFrondLodLevels();
}

unsigned int
SpeedTreeWrapper::get_NumLeafLodLevels()
{
	return speedTree->GetNumLeafLodLevels();
}


V4
SpeedTreeWrapper::get_LeafBillboardTable() __gc[]
{
	unsigned int count;

	const float *table = speedTree->GetLeafBillboardTable(count);

	unsigned int vectorCount = count / 4;
	V4 billboardTable[] = __gc new V4[vectorCount];

	for ( int i = 0; i < vectorCount; i++ ) {
		billboardTable[i] = V4(table[i*4], table[i*4+1], table[i*4+2], table[i*4+3]);
	}

	return billboardTable;
}

Single
SpeedTreeWrapper::get_LeafLodSizeAdjustments() __gc[]
{
	unsigned short levels = speedTree->GetNumLeafLodLevels();
	Single adjustTable[] = __gc new Single[levels];

	const float *table = speedTree->GetLeafLodSizeAdjustments();

	for ( unsigned short i = 0; i < levels; i++ ) {
		adjustTable[i] = table[i];
	}

	return adjustTable;
}

void
SpeedTreeWrapper::set_Time(float t)
{
	CSpeedTreeRT::SetTime(t);
}

unsigned int
SpeedTreeWrapper::get_CollisionObjectCount()
{
    return speedTree->GetCollisionObjectCount();
}

TreeCollisionObject __gc *
SpeedTreeWrapper::CollisionObject(unsigned int index)
{
	float position[3];
	float dimensions[3];
    CSpeedTreeRT::ECollisionObjectType coType;
    CollisionObjectType type;
    speedTree->GetCollisionObject(index, coType, position, dimensions);
    switch (coType)
    {
    case CSpeedTreeRT::CO_SPHERE:
        type = ColSphere;
        break;
    case CSpeedTreeRT::CO_CYLINDER:
        type = ColCylinder;
        break;
    case CSpeedTreeRT::CO_BOX:
        type = ColBox;
        break;
    default:
        // What should the default be?  Is there an "internal error"
        // routine in common use?
        type = ColSphere;
        break;
    }
    TreeCollisionObject __gc *co = 
        new TreeCollisionObject(type,
                                V3(position[0], position[1], position[2]),
                                V3(dimensions[0], dimensions[1], dimensions[2]));
    return co;
}

float 
SpeedTreeWrapper::get_OriginalSize()
{
	return originalSize;
}


void 
SpeedTreeWrapper::set_OriginalSize(float size)
{
	originalSize = size;
}


// AddHere
char * SpeedTreeWrapper::GetUnManagedString(String * s)
{
	char *str = 0;
	try
	{
		str = static_cast<char *>(const_cast<void*>(static_cast<const void*>(Marshal::StringToHGlobalAnsi(s))));
	}
	catch(ArgumentException * e)
	{
	// handle the exception
	}
	catch (OutOfMemoryException * e)
	{
	// handle the exception
	}
	return str;
}

TreeGeometry::TreeGeometry()
{
	geometry = (CSpeedTreeRT::SGeometry *)((Marshal::AllocHGlobal(sizeof(CSpeedTreeRT::SGeometry))).ToPointer());
}

TreeGeometry::~TreeGeometry()
{
	Marshal::FreeHGlobal(static_cast<IntPtr>(const_cast<void*>(static_cast<const void*>(geometry))));
}

CSpeedTreeRT::SGeometry __nogc *
TreeGeometry::get_UnmanagedGeometry()
{
	return geometry;
}

TreeGeometry::Indexed *
TreeGeometry::get_Branches()
{
	return new Indexed(&geometry->m_sBranches);
}

TreeGeometry::Indexed *
TreeGeometry::get_Fronds()
{
	return new Indexed(&geometry->m_sFronds);
}

float 
TreeGeometry::get_BranchAlphaTestValue()
{
	return geometry->m_fBranchAlphaTestValue;
}

float
TreeGeometry::get_FrondAlphaTestValue()
{
	return geometry->m_fFrondAlphaTestValue;
}

TreeGeometry::Leaf *
TreeGeometry::get_Leaves0()
{
	return new Leaf(&geometry->m_sLeaves0);
}

TreeGeometry::Leaf *
TreeGeometry::get_Leaves1()
{
	return new Leaf(&geometry->m_sLeaves1);
}

TreeGeometry::Billboard *
TreeGeometry::get_Billboard0()
{
	return new Billboard(&geometry->m_sBillboard0);
}

TreeGeometry::Billboard *
TreeGeometry::get_Billboard1()
{
	return new Billboard(&geometry->m_sBillboard1);
}

TreeGeometry::Billboard *
TreeGeometry::get_HorizontalBillboard()
{
	return new TreeGeometry::Billboard(&geometry->m_sHorizontalBillboard);
}

//
// TreeGeometry::Indexed methods
//

TreeGeometry::Indexed::Indexed(CSpeedTreeRT::SGeometry::SIndexed *init)
{
	indexed = init;
}


int 
TreeGeometry::Indexed::get_DiscreteLodLevel()
{
	return indexed->m_nDiscreteLodLevel;
}

unsigned short 
TreeGeometry::Indexed::get_NumStrips()
{
	return indexed->m_usNumStrips;
}

const unsigned short __nogc * 
TreeGeometry::Indexed::get_StripLengths()
{
	return indexed->m_pStripLengths;
}

const unsigned short __nogc * __nogc * 
TreeGeometry::Indexed::get_Strips()
{
	return indexed->m_pStrips;
}

unsigned short 
TreeGeometry::Indexed::get_VertexCount()
{
	return indexed->m_usVertexCount;
}

const unsigned long* 
TreeGeometry::Indexed::get_Colors()
{
	return indexed->m_pColors;
}

const float* 
TreeGeometry::Indexed::get_Normals()
{
	return indexed->m_pNormals;
}

const float* 
TreeGeometry::Indexed::get_Binormals()
{
	return indexed->m_pBinormals;
}

const float* 
TreeGeometry::Indexed::get_Tangents()
{
	return indexed->m_pTangents;
}

const float* 
TreeGeometry::Indexed::get_Coords()
{
	return indexed->m_pCoords;
}

const float* 
TreeGeometry::Indexed::get_TexCoords0()
{
	return indexed->m_pTexCoords0;
}

const float* 
TreeGeometry::Indexed::get_TexCoords1()
{
	return indexed->m_pTexCoords1;
}

const float* 
TreeGeometry::Indexed::get_WindWeights()
{
	return indexed->m_pWindWeights;
}

const unsigned char* 
TreeGeometry::Indexed::get_WindMatrixIndices()
{
	return indexed->m_pWindMatrixIndices;
}

//
// TreeGeometry::Leaf methods
//
TreeGeometry::Leaf::Leaf(CSpeedTreeRT::SGeometry::SLeaf __nogc *init)
{
	leaf = init;
}

bool
TreeGeometry::Leaf::get_IsActive()
{
	return leaf->m_bIsActive;	
}

float 
TreeGeometry::Leaf::get_AlphaTestValue()
{
	return leaf->m_fAlphaTestValue;
}

int
TreeGeometry::Leaf::get_DiscreteLodLevel()
{
	return leaf->m_nDiscreteLodLevel;
}

unsigned short 
TreeGeometry::Leaf::get_LeafCount()
{
	return leaf->m_usLeafCount;
}

const unsigned char* 
TreeGeometry::Leaf::get_LeafMapIndices()
{
	return leaf->m_pLeafMapIndices;
}

const unsigned char* 
TreeGeometry::Leaf::get_LeafClusterIndices()
{
	return leaf->m_pLeafClusterIndices;
}

const float* 
TreeGeometry::Leaf::get_CenterCoords()
{
	return leaf->m_pCenterCoords;
}

const float** 
TreeGeometry::Leaf::get_LeafMapTexCoords()
{
	return leaf->m_pLeafMapTexCoords;
}

const float** 
TreeGeometry::Leaf::get_LeafMapCoords()
{
	return leaf->m_pLeafMapCoords;
}

const unsigned long* 
TreeGeometry::Leaf::get_Colors()
{
	return leaf->m_pColors;
}

const float* 
TreeGeometry::Leaf::get_Normals()
{
	return leaf->m_pNormals;
}

const float* 
TreeGeometry::Leaf::get_Binormals()
{
	return leaf->m_pBinormals;
}

const float* 
TreeGeometry::Leaf::get_Tangents()
{
	return leaf->m_pTangents;
}

const float* 
TreeGeometry::Leaf::get_WindWeights()
{
	return leaf->m_pWindWeights;
}

const unsigned char* 
TreeGeometry::Leaf::get_WindMatrixIndices()
{
	return leaf->m_pWindMatrixIndices;
}

//
// TreeGeometry::Billboard methods
//
TreeGeometry::Billboard::Billboard(CSpeedTreeRT::SGeometry::SBillboard __nogc *init)
{
	billboard = init;
}

bool 
TreeGeometry::Billboard::get_IsActive()
{
	return billboard->m_bIsActive;
}

const float* 
TreeGeometry::Billboard::get_TexCoords()
{
	return billboard->m_pTexCoords;
}

const float* 
TreeGeometry::Billboard::get_Coords()
{
	return billboard->m_pCoords;
}

float 
TreeGeometry::Billboard::get_AlphaTestValue()
{
	return billboard->m_fAlphaTestValue;
}

SpeedWindWrapper::SpeedWindWrapper()
{
	speedWind = new CSpeedWind();
}

void 
SpeedWindWrapper::CreateWindMatrices()
{
	return speedWind->CreateWindMatrices();
}

void
SpeedWindWrapper::ResetMatrices()
{
	return speedWind->ResetMatrices();
}

float 
SpeedWindWrapper::Advance(float time, float strength, V3 direction)
{
	return speedWind->Advance(time, strength, direction.x, direction.y, direction.z);
}

void
SpeedWindWrapper::UpdateSpeedTreeRT()
{
	speedWind->UpdateSpeedTreeRT();
}

float
SpeedWindWrapper::get_ActualStrength()
{
	return speedWind->GetActualStrength();
}

int
SpeedWindWrapper::get_NumWindMatrices()
{
	return speedWind->GetNumWindMatrices();
}

Single
SpeedWindWrapper::get_WindMatrix(unsigned int index) __gc []
{
	const float *m = speedWind->GetWindMatrix(index);

	Single mat[] = __gc new Single[16];
	for ( int i = 0; i < 16; i++ ) {
		mat[i] = m[i];
	}
	return mat;
}

int
SpeedWindWrapper::get_NumLeafAngles()
{
	return speedWind->GetNumLeafAngles();
}

void 
SpeedWindWrapper::BuildLeafAngleMatrices(V3 cameraDirection)
{
	float vec[3];
	vec[0] = cameraDirection.x;
	vec[1] = cameraDirection.y;
	vec[2] = cameraDirection.z;

	speedWind->BuildLeafAngleMatrices(vec);
}

Single
SpeedWindWrapper::get_LeafAngleMatrix(unsigned int index) __gc []
{
	const float *m = speedWind->GetLeafAngleMatrix(index);

	Single mat[] = __gc new Single[16];
	for ( int i = 0; i < 16; i++ ) {
		mat[i] = m[i];
	}
	return mat;
}

bool
SpeedWindWrapper::Load(String __gc *filename)
{
	char *unmanagedFilename = GetUnManagedString(filename);

	bool ret = speedWind->Load(unmanagedFilename);

	Marshal::FreeHGlobal(static_cast<IntPtr>(const_cast<void*>(static_cast<const void*>(unmanagedFilename))));

	return ret;
}

char * SpeedWindWrapper::GetUnManagedString(String * s)
{
	char *str = 0;
	try
	{
		str = static_cast<char *>(const_cast<void*>(static_cast<const void*>(Marshal::StringToHGlobalAnsi(s))));
	}
	catch(ArgumentException * e)
	{
	// handle the exception
	}
	catch (OutOfMemoryException * e)
	{
	// handle the exception
	}
	return str;
}
