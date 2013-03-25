// SpeedTreeWrapper.h

#pragma once

#using <mscorlib.dll>

using namespace System;

using System::Runtime::InteropServices::Marshal;

#include "SpeedTreeRT.h"
#include "SpeedWind.h"

namespace Multiverse {

	public __value enum WindMethod
    {
        WindGPU, WindCPU, WindNone
    };

    enum LodMethod
    {
        LODPop, LODSmooth, LODNone = 3
    };

    enum LightingMethod
    {
        LightDynamic, LightStatic
    };

    enum StaticLightingType
    {
        SLSBasic, SLSUseLightSources, SLSSimulateShadows
    };

    public enum CollisionObjectType
    {
        ColSphere, ColCylinder, ColBox
    };

	public __gc struct TreeTextures
	{
		// branches
		String __gc *BranchTextureFilename; 

		// leaves
		String __gc *LeafTextureFilenames[];

		// fronds
		String __gc *FrondTextureFilenames[]; 

		// composite
		String __gc *CompositeFilename;

		// self-shadow
		String __gc *SelfShadowFilename;
	};

	public __value struct Color
	{
		float r;
		float g;
		float b;
		Color() : r(0), g(0), b(0) {}
		Color(float r, float g, float b) : r(r), g(g), b(b) {}
	};

	public __value struct V3
	{
		float x;
		float y;
		float z;
		V3() : x(0), y(0), z(0) {}
		V3(float x, float y, float z) : x(x), y(y), z(z) {}
	};

	public __value struct V4
	{
		float x;
		float y;
		float z;
		float w;
		V4() : x(0), y(0), z(0), w(0) {}
		V4(float x, float y, float z, float w) : x(x), y(y), z(z), w(w) {}
	};

	public __gc struct TreeMaterial
	{
		Color Diffuse;
		Color Ambient;
		Color Specular;
		Color Emmisive;
		float shininess;
	};

	public __gc struct TreeLight
	{
		V3 position;
		Color Diffuse;
		Color Ambient;
		Color Specular;
		bool directional;
		float attenuationConst;
		float attenuationLinear;
		float attenuationQuad;
	};

	public __gc struct TreeCamera
	{
		V3 position;
		V3 direction;
	};

	public __gc struct TreeBox
	{
		V3 min;
		V3 max;
		TreeBox(V3 min, V3 max) : min(min), max(max) {}
	};

	public __gc class TreeGeometry
	{
	public:

		__gc class Indexed
		{
		public:
			Indexed(CSpeedTreeRT::SGeometry::SIndexed __nogc *init);

			// these values change depending on the active discrete LOD level
			__property int get_DiscreteLodLevel();
			__property unsigned short get_NumStrips();
			__property const unsigned short __nogc * get_StripLengths();
			__property const unsigned short __nogc * __nogc * get_Strips();

			// these values are shared across all discrete LOD levels
			__property unsigned short get_VertexCount();
			__property const unsigned long* get_Colors();
			__property const float* get_Normals();
			__property const float* get_Binormals();
			__property const float* get_Tangents();
			__property const float* get_Coords();
			__property const float* get_TexCoords0();
			__property const float* get_TexCoords1();
			__property const float* get_WindWeights();
			__property const unsigned char* get_WindMatrixIndices();

		private:
			CSpeedTreeRT::SGeometry::SIndexed __nogc *indexed;
		};

		__gc class Leaf
		{
		public:
			Leaf(CSpeedTreeRT::SGeometry::SLeaf __nogc *init);

			// active LOD level data
			__property bool get_IsActive();
			__property float get_AlphaTestValue();
			__property int get_DiscreteLodLevel();
			__property unsigned short get_LeafCount();

			// tables for referencing the leaf cluster table
			__property const unsigned char* get_LeafMapIndices();
			__property const unsigned char* get_LeafClusterIndices();
			__property const float* get_CenterCoords();
			__property const float** get_LeafMapTexCoords();
			__property const float** get_LeafMapCoords();

			// remaining vertex attributes
			__property const unsigned long* get_Colors();
			__property const float* get_Normals();
			__property const float* get_Binormals();
			__property const float* get_Tangents();
			__property const float* get_WindWeights();
			__property const unsigned char* get_WindMatrixIndices();

		private:
			CSpeedTreeRT::SGeometry::SLeaf __nogc *leaf;
		};

		__gc class Billboard
		{
		public:
			Billboard(CSpeedTreeRT::SGeometry::SBillboard __nogc *init);

			__property bool get_IsActive();
			__property const float* get_TexCoords();
			__property const float* get_Coords();
			__property float get_AlphaTestValue();

		private:
			CSpeedTreeRT::SGeometry::SBillboard __nogc *billboard;
		};

		TreeGeometry();
		~TreeGeometry();

		__property CSpeedTreeRT::SGeometry __nogc *get_UnmanagedGeometry();

		__property Indexed *get_Branches();
		__property Indexed *get_Fronds();
		__property float get_BranchAlphaTestValue();
		__property float get_FrondAlphaTestValue();

		__property Leaf *get_Leaves0();
		__property Leaf *get_Leaves1();

		__property Billboard *get_Billboard0();
		__property Billboard *get_Billboard1();
		__property Billboard *get_HorizontalBillboard();

	private:
		CSpeedTreeRT::SGeometry __nogc *geometry;
	};

    public __gc struct TreeCollisionObject
	{
    public:
        CollisionObjectType type;
        V3 position;
		V3 dimensions;
        TreeCollisionObject(CollisionObjectType type, V3 position, V3 dimensions) :
            type(type), position(position), dimensions(dimensions) {};
    };

    public __gc class SpeedTreeWrapper
	{
	public:

		__value enum GeometryFlags
		{
			BranchGeometry = 1,
			FrondGeometry = 2,
			LeafGeometry = 4,
			BillboardGeometry = 8,
			SimpleBillboardOverride = 16,
			Nearest360Override = 32,
			AllGeometry = 15
		};

		SpeedTreeWrapper();
		~SpeedTreeWrapper() {}

		SpeedTreeWrapper * MakeInstance();
		bool LoadTree(String *filename);
		bool LoadTree(unsigned char buffer __gc [], unsigned int len);
		bool Compute(float transform __gc [], unsigned int seed, bool compositeStrips);
		SpeedTreeWrapper * Clone(V3 position, unsigned int seed);
		void DeleteTransientData();

		__property V3 get_TreePosition();
		__property void set_TreePosition(V3 position);

		void GetTreeSize(float __gc & size, float __gc &variance);
		void SetTreeSize(float size, float variance);


		__property unsigned int get_Seed();

		__property SpeedTreeWrapper * get_InstanceOf() { return instanceOf; };
		__property bool get_IsInstance() { return isInstance; };

		__property void set_LeafTargetAlphaMask(unsigned char mask);

		__property LightingMethod get_BranchLightingMethod();
		__property void set_BranchLightingMethod(LightingMethod method);

		__property LightingMethod get_FrondLightingMethod();
		__property void set_FrondLightingMethod(LightingMethod method);

		__property LightingMethod get_LeafLightingMethod();
		__property void set_LeafLightingMethod(LightingMethod method);

		__property TreeMaterial __gc *get_BranchMaterial();
		__property void set_BranchMaterial(TreeMaterial __gc *mat);

		__property TreeMaterial __gc *get_FrondMaterial();
		__property void set_FrondMaterial(TreeMaterial __gc *mat);

		__property TreeMaterial __gc *get_LeafMaterial();
		__property void set_LeafMaterial(TreeMaterial __gc *mat);

		__property float get_LeafLightingAdjustment();
		__property void set_LeafLightingAdjustment(float adj);

		__property TreeLight __gc *get_LightAttributes(unsigned int lightIndex);
		__property void set_LightAttributes(unsigned int lightIndex, TreeLight __gc *light);

		__property bool get_LightState(unsigned int lightIndex);
		__property void set_LightState(unsigned int lightIndex, bool state);

		__property StaticLightingType get_StaticLightingStyle();
		__property void set_StaticLightingStyle(StaticLightingType style);

		__property static TreeCamera __gc *get_Camera();
		__property static void set_Camera(TreeCamera __gc *camera);

		void GetGeometry(TreeGeometry __gc *treeGeometry, GeometryFlags flags, 
			short overrideBranchLodValue, short overrideFrondLodValue, short overrideLeafLodValue);

		__property TreeTextures __gc *get_Textures();
		__property bool get_TextureFlip();
		__property void set_TextureFlip(bool flip);

		__property TreeBox __gc *get_BoundingBox();

        void GetLodLimits(float __gc & near, float __gc &far);
		void SetLodLimits(float near, float far);

		static void SetDropToBillboard(bool flag);
		void ComputeLodLevel();

		void SetWindStrengthAndLeafAngles(float newStrength, float rockAngles __gc [], float rustleAngles __gc []);

		__property WindMethod get_BranchWindMethod();
		__property void set_BranchWindMethod(WindMethod method);

		__property WindMethod get_FrondWindMethod();
		__property void set_FrondWindMethod(WindMethod method);

		__property WindMethod get_LeafWindMethod();
		__property void set_LeafWindMethod(WindMethod method);

		__property void set_NumLeafRockingGroups(unsigned int n);

		__property unsigned int get_NumBranchLodLevels();
		__property unsigned int get_NumFrondLodLevels();
		__property unsigned int get_NumLeafLodLevels();
		__property V4 get_LeafBillboardTable() __gc[];

		__property float get_LeafLodSizeAdjustments() __gc[];

		__property static void set_Time(float t);
		
		__property unsigned int get_CollisionObjectCount();
        TreeCollisionObject __gc *CollisionObject(unsigned int index);
        
        __property float get_OriginalSize();
        __property void set_OriginalSize(float size);
        
    private:
		CSpeedTreeRT *speedTree;

		bool isInstance;
		SpeedTreeWrapper * instanceOf;

		float originalSize;
        
        SpeedTreeWrapper(bool isInstance, SpeedTreeWrapper *instanceOf, CSpeedTreeRT *speedTree);
		char *GetUnManagedString(String * s);

		TreeMaterial __gc *MaterialFromFloats(const float *m);
		void MaterialToFloats(TreeMaterial __gc *mat, float *m);
	};

	public __gc class SpeedWindWrapper
	{
	public:

		SpeedWindWrapper();
		~SpeedWindWrapper() {}

		void CreateWindMatrices();
		void ResetMatrices();
		float Advance(float time, float strength, V3 direction);
		void UpdateSpeedTreeRT();
		__property float get_ActualStrength();
		__property int get_NumWindMatrices();
		__property float get_WindMatrix(unsigned int index) __gc [];
		__property int get_NumLeafAngles();
		void BuildLeafAngleMatrices(V3 cameraDirection);
		__property float get_LeafAngleMatrix(unsigned int index) __gc[];
		bool Load(String __gc *filename);
		
	private:
		CSpeedWind *speedWind;
		char *GetUnManagedString(String * s);
	};


}
