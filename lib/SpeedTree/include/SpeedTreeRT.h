///////////////////////////////////////////////////////////////////////  
//         Name: SpeedTreeRT.h
//
//  *** INTERACTIVE DATA VISUALIZATION (IDV) PROPRIETARY INFORMATION ***
//
//      Copyright (c) 2001-2004 IDV, Inc.
//      All Rights Reserved.
//
//      IDV, Inc.
//      1233 Washington St. Suite 610
//      Columbia, SC 29201
//      Voice: (803) 799-1699
//      Fax:   (803) 931-0320
//      Web:   http://www.idvinc.com
//
//  This software is supplied under the terms of a license agreement or
//  nondisclosure agreement with Interactive Data Visualization and may not 
//  be copied or disclosed except in accordance with the terms of that 
//  agreement.
//
//      Release version 1.6.0 (December 19, 2003)

#pragma once

//  define static or dynamic library build
#ifdef SPEEDTREERT_DYNAMIC_LIB
    #ifdef IDV_SPEEDTREERT_EXPORTS
        #define BUILD_ST_RT __declspec(dllexport)
    #else
        #define BUILD_ST_RT __declspec(dllimport)
    #endif
#else
    #define BUILD_ST_RT
#endif

//  big or little endian system
#ifdef __ppc__
    #define ST_BIG_ENDIAN
#else
    #define ST_LITTLE_ENDIAN
#endif

//  macintosh-specific
#ifdef __ppc__
    #pragma export on
#define EXP_ST_RT
#else
    #define EXP_ST_RT __cdecl
#endif

//  includes
#include <cstddef>

//  forward refernces
class CIndexedGeometry;
class CTreeEngine;
class CLeafGeometry;
class CLightingEngine;
class CWindEngine;
class CTreeFileAccess;
class CSimpleBillboard;
class CFrondEngine;
struct STreeInstanceData;
struct SInstanceList;
struct SEmbeddedTexCoords;
struct SCollisionObjects;
class CProjectedShadow;


///////////////////////////////////////////////////////////////////////  
//  class SpeedTreeRT
//
//  In an effort to make the SpeedTreeRT.h header file dependency free
//  and easy to include into almost any project, a number of steps have
//  been taken:
//
//      1. No external header files need to be included by SpeedTreeRT.h
//         or by the application before including it.
//
//      2. Most of the implementation of the class is hidden by pointers
//         to the major sections of the library (the internal classes
//         can then just be forward-referenced)
//
//      3. Where possible, basic C++ datatypes are used to define the
//         member functions' parameters.
//
//  Because almost all of the implementation details are hidden, none of
//  the functions for CSpeedTreeRT are inlined.  However, inlined functions
//  were copiously used within the library.
    
class BUILD_ST_RT CSpeedTreeRT
{
public:
        ///////////////////////////////////////////////////////////////////////  
        //  Enumerations

        enum EWindMethod
        {
            WIND_GPU, WIND_CPU, WIND_NONE
        };

        enum ELodMethod
        {
            LOD_POP, LOD_SMOOTH, LOD_NONE = 3
        };

        enum ELightingMethod
        {
            LIGHT_DYNAMIC, LIGHT_STATIC
        };

        enum EStaticLightingStyle
        {
            SLS_BASIC, SLS_USE_LIGHT_SOURCES, SLS_SIMULATE_SHADOWS
        };

        enum ECollisionObjectType
        {
            CO_SPHERE, CO_CYLINDER, CO_BOX
        };


        ///////////////////////////////////////////////////////////////////////
        //  SGeometry bit vectors
        //
        //  Passed into GetGeometry() in order to mask out unneeded geometric elements

        #define                 SpeedTree_BranchGeometry            (1 << 0)
        #define                 SpeedTree_FrondGeometry             (1 << 1)
        #define                 SpeedTree_LeafGeometry              (1 << 2)
        #define                 SpeedTree_BillboardGeometry         (1 << 3)
        #define                 SpeedTree_SimpleBillboardOverride   (1 << 4)
        #define                 SpeedTree_Nearest360Override        (1 << 5)
        #define                 SpeedTree_AllGeometry               (SpeedTree_BranchGeometry + SpeedTree_FrondGeometry + SpeedTree_LeafGeometry + SpeedTree_BillboardGeometry)


        ///////////////////////////////////////////////////////////////////////
        //  struct SGeometry declaration
        
        struct BUILD_ST_RT SGeometry
        {
            SGeometry( );
            ~SGeometry( );

            ///////////////////////////////////////////////////////////////////////
            //  struct SGeometry::SIndexed declaration

            struct BUILD_ST_RT SIndexed
            {
                SIndexed( );
                ~SIndexed( );

                // these values change depending on the active discrete LOD level
                int                     m_nDiscreteLodLevel;    // range: [0, GetNumBranch/FrondLodLevels( ) - 1], -1 if inactive
                unsigned short          m_usNumStrips;          // total number of strips in current LOD
                const unsigned short*   m_pStripLengths;        // lengths of strips in current LOD (m_usNumStrips in length)
                const unsigned short**  m_pStrips;              // triangle strip indices (m_usNumStrips in length)

                // these values are shared across all discete LOD levels
                unsigned short          m_usVertexCount;        // total vertex count in tables, referenced by all LOD levels
                const unsigned long*    m_pColors;              // RGBA values for each leaf - static lighting only (m_usVertexCount in length)
                const float*            m_pNormals;             // normals for each vertex (3 * m_usVertexCount in length)    
                const float*            m_pBinormals;           // binormals (bump mapping) for each vertex (3 * m_usVertexCount in length)    
                const float*            m_pTangents;            // tangents (bump mapping) for each vertex (3 * m_usVertexCount in length)        
                const float*            m_pCoords;              // coordinates for each vertex (3 * m_usVertexCount in length)
                const float*            m_pTexCoords0;          // 1st layer (s,t) texcoords for each vertex (2 * m_usVertexCount in length)
                const float*            m_pTexCoords1;          // 2nd layer (s,t) texcoords for each vertex (2 * m_usVertexCount in length)
                const float*            m_pWindWeights;         // values from from 0.0 for rigid to 1.0 for flexible (m_usVertexCount in length)
                const unsigned char*    m_pWindMatrixIndices;   // table of wind matrix indices (m_usVertexCount in length)
            };


            ///////////////////////////////////////////////////////////////////////
            //  struct SGeometry::SLeaf declaration

            struct BUILD_ST_RT SLeaf
            {
                SLeaf( );
                ~SLeaf( );

                // active LOD level data
                bool                    m_bIsActive;            // flag indicating visibility
                float                   m_fAlphaTestValue;      // 0.0 to 255.0 alpha testing value, used for fading
                int                     m_nDiscreteLodLevel;    // range: [0, GetNumLeafLodLevels( ) - 1]
                unsigned short          m_usLeafCount;          // number of leaves stored in this structure

                // tables for referencing the leaf cluster table
                const unsigned char*    m_pLeafMapIndices;      // references which leaf texture map used for each leaf (m_usLeafCount in length)
                const unsigned char*    m_pLeafClusterIndices;  // references which leaf cluster used for each leaf (m_usLeafCount in length)
                const float*            m_pCenterCoords;        // (x,y,z) values for the centers of leaf clusters (3 * m_usLeafCount in length)
                const float**           m_pLeafMapTexCoords;    // table of unique leaf cluster texcoords (m_usLeafCount in length) - each entry
                                                                // points to 4 pairs of (s,t) texcoords stored in one contiguous array
                const float**           m_pLeafMapCoords;       // table of unique leaf cluster coordinates (m_usLeafCount in length) - each entry
                                                                // points to 4 sets of (x,y,z,0) coordinates stored in one contiguous array
                // remaining vertex attributes
                const unsigned long*    m_pColors;              // RGBA values for each leaf (m_usLeafCount in length)
                const float*            m_pNormals;             // normals for each leaf (3 * m_usLeafCount in length)
                const float*            m_pBinormals;           // binormals (bump mapping) for each leaf (3 * m_usLeafCount in length)
                const float*            m_pTangents;            // tangents (bump mapping) for each leaf (3 * m_usLeafCount in length)
                const float*            m_pWindWeights;         // values from from 0.0 for rigid to 1.0 for flexible (m_usLeafCount in length)
                const unsigned char*    m_pWindMatrixIndices;   // table of wind matrix indices (m_usLeafCount in length)
            };


            ///////////////////////////////////////////////////////////////////////
            //  struct SGeometry::SBillboard declaration

            struct BUILD_ST_RT SBillboard
            {
                SBillboard( );
                ~SBillboard( );

                bool                    m_bIsActive;            // flag indicating visibility
                const float*            m_pTexCoords;           // 4 pairs of (s,t) texcoords stored in one contiguous array
                const float*            m_pCoords;              // 4 sets of (x,y,z) coordindates stored in one contiguous array
                float                   m_fAlphaTestValue;      // 0.0 to 255.0 alpha testing value, used for fading 
            };

            ///////////////////////////////////////////////////////////////////////
            //  branch geometry

            SIndexed                m_sBranches;                // holds the branch vertices and index buffers for all
                                                                // of the discrete LOD levels
            float                   m_fBranchAlphaTestValue;    // 0.0 to 255.0 alpha testing value, used for fading

            ///////////////////////////////////////////////////////////////////////
            //  frond geometry

            SIndexed                m_sFronds;                  // holds the frond vertices and index buffers for all
                                                                // of the discrete LOD levels
            float                   m_fFrondAlphaTestValue;     // 0.0 to 255.0 alpha testing value, used for fading

            ///////////////////////////////////////////////////////////////////////
            //  leaf geometry

            SLeaf                   m_sLeaves0;                 // holds the primary leaf geometry, alpha fades into
            SLeaf                   m_sLeaves1;                 // m_sLeaves1 during LOD transitions

            ///////////////////////////////////////////////////////////////////////
            //  billboard geometry

            SBillboard              m_sBillboard0;              // holds the main simple billboard geometry, alpha fades 
            SBillboard              m_sBillboard1;              // into m_sBillboard1 in 360 degree mode
            SBillboard              m_sHorizontalBillboard;     // optional horizontal billboard used for aerial views
        };

        
        ///////////////////////////////////////////////////////////////////////
        //  struct SGeometry::STextures declaration

        struct BUILD_ST_RT STextures
        {
            STextures( );
            ~STextures( );

            // branches
            const char*         m_pBranchTextureFilename;   // null-terminated string

            // leaves
            unsigned int        m_uiLeafTextureCount;       // the number of char* elements in m_pLeafTextureFilenames 
            const char**        m_pLeafTextureFilenames;    // array of null-terminated strings m_uiLeafTextureCount in size

            // fronds
            unsigned int        m_uiFrondTextureCount;      // the number of char* elements in m_pFrondTextureFilenames 
            const char**        m_pFrondTextureFilenames;   // array of null-terminated strings m_uiFrondTextureCount in size

            // composite
            const char*         m_pCompositeFilename;       // null-terminated string

            // self-shadow
            const char*         m_pSelfShadowFilename;      // null-terminated string
        };


        ///////////////////////////////////////////////////////////////////////  
        //  Constructor/Destructor

        CSpeedTreeRT( );
        ~CSpeedTreeRT( );


        ///////////////////////////////////////////////////////////////////////  
        //  Memory allocation

static  void* EXP_ST_RT         operator new(size_t nSize);
static  void* EXP_ST_RT         operator new[](size_t nSize);
static  void EXP_ST_RT          operator delete(void* pRawMemory);
static  void EXP_ST_RT          operator delete[](void* pRawMemory);


        ///////////////////////////////////////////////////////////////////////  
        //  Specifying a tree model

        bool                    Compute(const float* pTransform = 0, unsigned int nSeed = 1, bool bCompositeStrips = true);
        CSpeedTreeRT*           Clone(float x = 0.0f, float y = 0.0f, float z = 0.0f, unsigned int nSeed = 0) const;
        const CSpeedTreeRT*     InstanceOf(void) const;
        CSpeedTreeRT*           MakeInstance(void);
        void                    DeleteTransientData(void);

        bool                    LoadTree(const char* pFilename);
        bool                    LoadTree(const unsigned char* pBlock, unsigned int nNumBytes);
        unsigned char*          SaveTree(unsigned int& nNumBytes, bool bSaveLeaves = false) const;

        void                    GetTreeSize(float& fSize, float& fVariance) const;
        void                    SetTreeSize(float fNewSize, float fNewVariance = 0.0f);
        unsigned int            GetSeed(void) const;

        const float*            GetTreePosition(void) const;
        void                    SetTreePosition(float x, float y, float z);

        void                    SetLeafTargetAlphaMask(unsigned char ucMask = 0x54);


        ///////////////////////////////////////////////////////////////////////  
        //  Lighting

        //  lighting style
        ELightingMethod         GetBranchLightingMethod(void) const;
        void                    SetBranchLightingMethod(ELightingMethod eMethod);
        ELightingMethod         GetLeafLightingMethod(void) const;
        void                    SetLeafLightingMethod(ELightingMethod eMethod);
        ELightingMethod         GetFrondLightingMethod(void) const;
        void                    SetFrondLightingMethod(ELightingMethod eMethod);

        EStaticLightingStyle    GetStaticLightingStyle(void) const;
        void                    SetStaticLightingStyle(EStaticLightingStyle eStyle);
        float                   GetLeafLightingAdjustment( ) const;
        void                    SetLeafLightingAdjustment(float fScalar);

        //  global lighting state
static  bool EXP_ST_RT          GetLightState(unsigned int nLightIndex);
static  void EXP_ST_RT          SetLightState(unsigned int nLightIndex, bool bLightOn);
static  const float*EXP_ST_RT   GetLightAttributes(unsigned int nLightIndex);
static  void EXP_ST_RT          SetLightAttributes(unsigned int nLightIndex, const float* pLightAttributes);

        //  branch material
        const float*            GetBranchMaterial(void) const;
        void                    SetBranchMaterial(const float* pMaterial);

        //  leaf material
        const float*            GetLeafMaterial(void) const;
        void                    SetLeafMaterial(const float* pMaterial);

        //  frond material
        const float*            GetFrondMaterial(void) const;
        void                    SetFrondMaterial(const float* pMaterial);


        ///////////////////////////////////////////////////////////////////////  
        //  Camera

static  void EXP_ST_RT          GetCamera(float* pPosition, float* pDirection);
static  void EXP_ST_RT          SetCamera(const float* pPosition, const float* pDirection);


        ///////////////////////////////////////////////////////////////////////  
        //  Wind 

static  void EXP_ST_RT          SetTime(float fTime);
        void                    ComputeWindEffects(bool bBranches, bool bLeaves, bool bFronds = true);
        void                    ResetLeafWindState(void);

        bool                    GetLeafRockingState(void) const;
        void                    SetLeafRockingState(bool bFlag);
        void                    SetNumLeafRockingGroups(unsigned int nRockingGroups);
        
        EWindMethod             GetLeafWindMethod(void) const;
        void                    SetLeafWindMethod(EWindMethod eMethod);
        EWindMethod             GetBranchWindMethod(void) const;
        void                    SetBranchWindMethod(EWindMethod eMethod);
        EWindMethod             GetFrondWindMethod(void) const;
        void                    SetFrondWindMethod(EWindMethod eMethod);

        float                   GetWindStrength(void) const;
        float                   SetWindStrength(float fNewStrength, float fOldStrength = -1.0f, float fFrequencyTimeOffset = -1.0f);
        void                    SetWindStrengthAndLeafAngles(float fNewStrength, const float* pRockAngles = NULL, const float* pRustleAngles = NULL, unsigned int uiNumRockAngles = 0);

static  void EXP_ST_RT          SetNumWindMatrices(int nNumMatrices);
static  void EXP_ST_RT          SetWindMatrix(unsigned int nMatrixIndex, const float* pMatrix);
        void                    GetLocalMatrices(unsigned int& nStartingIndex, unsigned int& nMatrixSpan);
        void                    SetLocalMatrices(unsigned int nStartingMatrix, unsigned int nMatrixSpan);

        
        ///////////////////////////////////////////////////////////////////////  
        //  LOD

        void                    ComputeLodLevel(void);
        float                   GetLodLevel(void) const;
        void                    SetLodLevel(float fLodLevel);
static  void EXP_ST_RT          SetDropToBillboard(bool bFlag);

        void                    GetLodLimits(float& fNear, float& fFar) const;
        void                    SetLodLimits(float fNear, float fFar);

        short                   GetDiscreteBranchLodLevel(float fLodLevel = -1.0f) const;
        unsigned short          GetDiscreteLeafLodLevel(float fLodLevel = -1.0f) const;
        short                   GetDiscreteFrondLodLevel(float fLodLevel = -1.0f) const;

        unsigned short          GetNumBranchLodLevels(void) const;
        unsigned short          GetNumLeafLodLevels(void) const;
        unsigned short          GetNumFrondLodLevels(void) const;

static  void EXP_ST_RT          SetHorzBillboardFadeAngles(float fStart, float fEnd); // in degrees
static  void EXP_ST_RT          GetHorzBillboardFadeAngles(float& fStart, float& fEnd); // in degrees


        ///////////////////////////////////////////////////////////////////////  
        //  Geometry

        void                    DeleteBranchGeometry(void);
        void                    DeleteFrondGeometry(void);
        unsigned char*          GetFrondGeometryMapIndexes(int nLodLevel) const;
        const float*            GetLeafBillboardTable(unsigned int& nEntryCount) const;
        const float*            GetLeafLodSizeAdjustments(void);
        void                    GetGeometry(SGeometry& sGeometry, unsigned long ulBitVector = SpeedTree_AllGeometry, short sOverrideBranchLodValue = -1, short sOverrideFrondLodValue = -1, short sOverrideLeafLodValue = -1);


        ///////////////////////////////////////////////////////////////////////  
        //  Textures

        void                    GetTextures(STextures& sTextures) const;
        void                    SetLeafTextureCoords(unsigned int nLeafMapIndex, const float* pTexCoords);
        void                    SetFrondTextureCoords(unsigned int nFrondMapIndex, const float* pTexCoords);
static  bool EXP_ST_RT          GetTextureFlip(void);
static  void EXP_ST_RT          SetTextureFlip(bool bFlag);
        void                    SetBranchTextureFilename(const char* pFilename);
        void                    SetLeafTextureFilename(unsigned int nLeafMapIndex, const char* pFilename);
        void                    SetFrondTextureFilename(unsigned int nFrondMapIndex, const char* pFilename);


        ///////////////////////////////////////////////////////////////////////  
        //  Statistics & information

static  void EXP_ST_RT          Authorize(const char* pKey);
static  bool EXP_ST_RT          IsAuthorized(void);
static  const char* EXP_ST_RT   GetCurrentError(void);
static  void EXP_ST_RT          ResetError(void);

        void                    GetBoundingBox(float* pBounds) const;
        unsigned int            GetLeafTriangleCount(float fLodLevel = -1.0f) const;
        unsigned int            GetBranchTriangleCount(float fLodLevel = -1.0f) const;
        unsigned int            GetFrondTriangleCount(float fLodLevel = -1.0f) const;
 
        
        ///////////////////////////////////////////////////////////////////////  
        //  Collision objects

        unsigned int            GetCollisionObjectCount(void);
        void                    GetCollisionObject(unsigned int nIndex, ECollisionObjectType& eType, float* pPosition, float* pDimensions);
 
        
        ///////////////////////////////////////////////////////////////////////  
        //  User Data

        const char*             GetUserData(void) const;

private:
        CSpeedTreeRT(const CSpeedTreeRT* pOrig);

        void                    ComputeLeafStaticLighting(void);
        void                    ComputeSelfShadowTexCoords(void);
static  void EXP_ST_RT          NotifyAllTreesOfEvent(int nMessage);
static  void EXP_ST_RT          SetError(const char* pError);


        ///////////////////////////////////////////////////////////////////////  
        //  File I/O

        void                    ParseLodInfo(CTreeFileAccess* pFile);
        void                    ParseWindInfo(CTreeFileAccess* pFile);
        void                    ParseTextureCoordInfo(CTreeFileAccess* pFile);
        void                    ParseCollisionObjects(CTreeFileAccess* pFile);
        void                    SaveTextureCoords(CTreeFileAccess* pFile) const;
        void                    SaveCollisionObjects(CTreeFileAccess* pFile) const;
        void                    ParseShadowProjectionInfo(CTreeFileAccess* pFile);
        void                    SaveUserData(CTreeFileAccess* pFile) const;
        void                    ParseUserData(CTreeFileAccess* pFile);
        void                    SaveSupplementalTexCoordInfo(CTreeFileAccess* pFile) const;
        void                    ParseSupplementalTexCoordInfo(CTreeFileAccess* pFile);
static  char* EXP_ST_RT         CopyUserData(const char* pData);

        ///////////////////////////////////////////////////////////////////////  
        //  Geometry
        
        void                    GetBranchGeometry(SGeometry& sGeometry, short sOverrideLodValue = -1);
        void                    GetFrondGeometry(SGeometry& sGeometry, short sOverrideLodValue = -1);
        void                    GetLeafGeometry(SGeometry& sGeometry, short sOverrideLodValue = -1);
        void                    Get360BillboardGeometry(SGeometry& sGeometry, unsigned long ulBitVector);
        void                    GetSimpleBillboardGeometry(SGeometry& sGeometry);
static  void EXP_ST_RT          GetTransitionValues(float fLodLevel, unsigned short usLodCount, float fOverlapRadius,
                                                    float fTransitionFactor, float fCurveExponent, float fTargetAlphaValue,
                                                    float& fHighValue, float& fLowValue, short& sHighLod, short& sLowLod);
        void                    SetupHorizontalBillboard( );

        ///////////////////////////////////////////////////////////////////////  
        //  Member variables

        // general
        CTreeEngine*            m_pEngine;                  // core tree-generating engine
        CIndexedGeometry*       m_pBranchGeometry;          // abstraction mechanism for branch geometry
        CLeafGeometry*          m_pLeafGeometry;            // abstraction mechanism for leaf geometry
        CLightingEngine*        m_pLightingEngine;          // engine for computing static/dynamic lighting data
        CWindEngine*            m_pWindEngine;              // engine for computing CPU/GPU wind effects
        CSimpleBillboard*       m_pSimpleBillboard;

        // leaf lod
        ELodMethod              m_eLeafLodMethod;           // which leaf wind method is currently being used
        float                   m_fLeafLodTransitionRadius; // determines how much blending occurs between two separate leaf LOD levels
        float                   m_fLeafLodCurveExponent;    // exponent value used in the leaf LOD blending equation
        float                   m_fLeafSizeIncreaseFactor;  // value that controls how much larger leaf clusters get as LOD decreases
        float                   m_fLeafTransitionFactor;    // value that controls the intersection point of SMOOTH_1 transitions
        float*                  m_pLeafLodSizeFactors;      // array, GetNumLeafLodLevels()'s in size, containing leaf LOD scale factors

        // instancing & ref counting
        unsigned int*           m_pInstanceRefCount;        // single value shared among instances - number of active instances
        STreeInstanceData*      m_pInstanceData;            // if instance, differntiating data is stored here
        SInstanceList*          m_pInstanceList;            // each tree contains a list of its instances
static  unsigned int            m_uiAllRefCount;            // single value shared by all CSpeedTreeRT instances

        // other
        int                     m_nFrondLevel;              // from SpeedTreeCAD - branch level where fronds begin
        float*                  m_pTreeSizes;               // contains all tree extents, including billboard sizes
        unsigned char           m_ucTargetAlphaValue;       // value used for leaf alpha mask function
        bool                    m_bTreeComputed;            // some operations are not valid once the geometry has been computed
        int                     m_nBranchWindLevel;         // from SpeedTreeCAD - branch level where wind effects are active

        // texture coords
        SEmbeddedTexCoords*     m_pEmbeddedTexCoords;       // embedded leaf and billboard texture coords
static  bool                    m_bTextureFlip;             // used to flip coordinates for DirectX, Gamebryo, etc.

        // shadow projection
        CProjectedShadow*       m_pProjectedShadow;         // self-shadow projection

        // billboard
static  bool                    m_bDropToBillboard;         // flag specifying if last LOD will be simple single billboard

        // collision objects
        SCollisionObjects*      m_pCollisionObjects;        // collision objects

        // fronds
        CFrondEngine*           m_pFrondEngine;             // engine for computing fronds based on branch geometry
        CIndexedGeometry*       m_pFrondGeometry;           // abstraction mechanism for frond geometry
        unsigned short          m_usNumFrondLodLevels;      // place to store LOD count after m_pFrondGeometry is deleted

        // user data
        char*                   m_pUserData;                // user specified data

        // horizontal billboard
        bool                    m_b360Billboard;            // indicates that a 360 degree billboard sequence is present
        bool                    m_bHorizontalBillboard;     // indicates that a horizontal billboard is present in the embedded tex coords
        float                   m_afHorizontalCoords[12];   // vertices of the horizontal billboard
static  float                   m_fHorizontalFadeStartAngle;// in degrees
static  float                   m_fHorizontalFadeEndAngle;  // in degrees
static  float                   m_fHorizontalFadeValue;
};


// Macintosh-specific
#ifdef __ppc__
#pragma export off
#endif
