/********************************************************************

The Multiverse Platform is made available under the MIT License.

Copyright (c) 2012 The Multiverse Foundation

Permission is hereby granted, free of charge, to any person 
obtaining a copy of this software and associated documentation 
files (the "Software"), to deal in the Software without restriction, 
including without limitation the rights to use, copy, modify, 
merge, publish, distribute, sublicense, and/or sell copies 
of the Software, and to permit persons to whom the Software 
is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be 
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE 
OR OTHER DEALINGS IN THE SOFTWARE.

*********************************************************************/

#define NO_CULL_BACKFACES

#region Using directives

using System;
using System.Text;
using System.Diagnostics;

using Axiom.MathLib;

#endregion

namespace Multiverse.MathLib
{
	public class MathUtil
	{
		const float Epsilon = 0.000001f;

		public MathUtil() {
		}

        public static Matrix4 GetTransform(ref Quaternion orientation, ref Vector3 position) {
            Matrix4 rv = Matrix4.FromMatrix3(orientation.ToRotationMatrix());
            rv.Translation = position;
            return rv;
        }

        public static Matrix4 GetTransform(Quaternion orientation, Vector3 position) {
            return GetTransform(ref orientation, ref position);
        }
#if CPP
static vector<int> g_oDebugTriangleIndices;
static vector<const OBBTreeNode *> g_oDebugCollisionNodes;

static inline bool
equals(float a, float b, float epsilon = .001) {
  return (fabs(a - b) < epsilon);
}

// Check the time of collision on an axis, replacing the min_time with a higher
// collision time if the collision (on this axis) is later.
// We pass in the minimum and maximum projections points for the two objects,
// as well as the projected velocity vector magnitude.
// I also added an epsilon that will allow objects to move away in an axis 
// where they are close, but will prevent them from getting any closer.
// This version is used for checking collisions with triangles.
static inline bool
CheckTime(float *min_time, float *max_time,
	  float min1, float max1, float min2, float max2, float w, 
	  float epsilon = UNITS_PER_METER / 100.0f) {
  float mult;
  if (w >= 0) { // moving right
    if (max1 < min2) { // object 1 is to the left of object 2
      if ((max1 + epsilon) + w < min2) // too far away
        return false;
      mult = (min2 - (max1 + epsilon)) / w;
      if (mult > *min_time)
	*min_time = mult;
      if (min1 + w > max2) { // will move partway past 2
	mult = (max2 - min1) / w;
	if (mult < *max_time)
	  *max_time = mult;
      }
    } else if (max2 < min1) { // object 2 is to the left of object 1
      return false;
    } else { // objects start out overlapping
      if (min1 + w > max2) { // will move partway past 2
	mult = (max2 - min1) / w;
	if (mult < *max_time)
	  *max_time = mult;
      }
    }
  } else { // moving left
    if (max1 < min2) { // object 1 is to the left of object 2
      return false;
    } else if (max2 < min1) { // object 2 is to the left of object 1
      if ((max2 + epsilon) < min1 + w) // too far away
	return false;
      mult = ((max2 + epsilon) - min1) / w;
      if (mult > *min_time)
	*min_time = mult;
      if (max1 + w < min2) { // we will move partway past 2
	mult = (min2 - max1) / w;
	if (mult < *max_time)
	  *max_time = mult;
      }
    } else { // objects start out overlapping
      if (max1 + w < min2) { // we will move partway past 2
	mult = (min2 - max1) / w;
	if (mult < *max_time)
	  *max_time = mult;
      }
    }
  }
  return (*min_time < *max_time);
}

// Get the vectors that go from the object's center to the faces
static inline void
GetFaceVectors(Vector *vResults, const Vector3 &vOBBDims) {
  vResults[0].x = vOBBDims.x / 2;
  vResults[0].y = 0;
  vResults[0].z = 0;

  vResults[1].x = 0; 
  vResults[1].y = vOBBDims.y / 2;
  vResults[1].z = 0;
  
  vResults[2].x = 0;
  vResults[2].y = 0;
  vResults[2].z = vOBBDims.z / 2;
}

// Get the rotated version of the vectors that go from the object's
// center to the faces
static inline void
GetRotatedFaceVectors(Vector *vResults, const Vector &vOBBDims, 
		      const Rotation &rRot) {
  Vector vTmp[3];
  GetFaceVectors(vTmp, vOBBDims);
  Matrix mRot;
  D3DXMatrixRotationQuaternion(&mRot, &rRot);
  D3DXVec3TransformCoord(&vResults[0], &vTmp[0], &mRot);
  D3DXVec3TransformCoord(&vResults[1], &vTmp[1], &mRot);
  D3DXVec3TransformCoord(&vResults[2], &vTmp[2], &mRot);
}

// Computes the minimum point and the maximum point for the projection of 
// the points of the triangle along vP
void MathUtil::ComputeBounds(float *min, float *max, 
			     const Triangle &triangle, 
			     const Vector &vP) {
  float tmp = D3DXVec3Dot(&triangle.p[0], &vP);
  *min = tmp;
  *max = tmp;
  tmp = D3DXVec3Dot(&triangle.p[1], &vP);
  if (tmp < *min)
    *min = tmp;
  if (tmp > *max)
    *max = tmp;
  tmp = D3DXVec3Dot(&triangle.p[2], &vP);
  if (tmp < *min)
    *min = tmp;
  if (tmp > *max)
    *max = tmp;
}

// Computes the minimum point and the maximum point for the projection of 
// the points of the obb centered at vC with the three vectors from the 
// array vB defining the vectors to the faces.
void
MathUtil::ComputeBounds(float *min, float *max, const Vector *vB,
			const Vector &vC, const Vector &vP) {
  float p = D3DXVec3Dot(&vC, &vP);
  float r = 
    fabs(D3DXVec3Dot(&vB[0], &vP)) +
    fabs(D3DXVec3Dot(&vB[1], &vP)) + 
    fabs(D3DXVec3Dot(&vB[2], &vP));
  *min = p - r;
  *max = p + r;
}

static void
ComputePlane(Plane &p, const Matrix &m, int index, float x, float y, float z) {
  Vector temp;
  p.a = x * m(0, 0) + y * m(0, 1) + z * m(0, 2);
  p.b = x * m(1, 0) + y * m(1, 1) + z * m(1, 2);
  p.c = x * m(2, 0) + y * m(2, 1) + z * m(2, 2);
  p.d = p.a * m(3, 0) + p.b * m(3, 1) + p.c * m(3, 2);
}

static bool
IsBoxInside(const Plane &p, const Vector &min, const Vector &max) {
  Vector v;
  v.x = ( p.a > 0 ) ? max.x : min.x;
  v.y = ( p.b > 0 ) ? max.y : min.y;
  v.z = ( p.c > 0 ) ? max.z : min.z;
  return (p.a * v.x + p.b * v.y + p.c * v.z >= p.d);
}

void
MathUtil::GetFrustrumPlanes(Plane *a_Planes, const Matrix &mView) {
  ComputePlane(a_Planes[0], mView, 0, -1,  0, 1);
  ComputePlane(a_Planes[1], mView, 1,  1,  0, 1);
  ComputePlane(a_Planes[2], mView, 2,  0,  1, 1);
  ComputePlane(a_Planes[3], mView, 3,  0, -1, 1);
}

bool
MathUtil::IsInFrustrum(const Plane *a_Planes, const Vector &min, const Vector &max) {
  return(IsBoxInside(a_Planes[0], min, max) &&
	 IsBoxInside(a_Planes[1], min, max) &&
	 IsBoxInside(a_Planes[2], min, max) &&
	 IsBoxInside(a_Planes[3], min, max));
}
    
void 
MathUtil::GetRotationVectors(const Rotation &rRot, 
			     Vector *vUp, Vector *vRight, Vector *vForward) {
  Matrix mRot;
  D3DXMatrixRotationQuaternion(&mRot, &rRot);
  Vector vBasisUp(0, 1, 0);
  Vector vBasisForward(0, 0, 1);
  Vector vBasisRight(1, 0, 0);
  D3DXVec3TransformCoord(vUp, &vBasisUp, &mRot);
  D3DXVec3TransformCoord(vRight, &vBasisRight, &mRot);
  D3DXVec3TransformCoord(vForward, &vBasisForward, &mRot);
}

// 
// This version is slower, but clearer -- it also doesn't handle the time
// based collision checks
//
//  bool
//  MathUtil::CheckCollision(const Rotation &rRotA, const Rotation rRotB, 
//  			 const Vector &vOBBCenterA, const Vector &vOBBCenterB, 
//  			 const Vector &vOBBDimsA, const Vector &vOBBDimsB) {
//    Vector vL[15]; // separating axis candidates
//    Vector vT = vOBBCenterB - vOBBCenterA;
//    float ra, rb; // radius of projection
//    Vector vA[3], vB[3]; // Basis vectors of OBBs
//    int i, j;
//    GetRotationVectors(rRotA, &vA[0], &vA[1], &vA[2]);
//    GetRotationVectors(rRotB, &vB[0], &vB[1], &vB[2]);
//    for (i = 0; i < 3; i++)
//      vL[i] = vA[i];
//    for (i = 0; i < 3; i++)
//      vL[3 + i] = vB[i];
//    for (i = 0; i < 3; i++)
//      for (j = 0; j < 3; j++)
//        D3DXVec3Cross(&vL[6 + 3 * j + i], &vA[i], &vB[j]);
//    for (i = 0; i < 15; i++) {
//      ra = 
//        vOBBDimsA[0] * fabs(D3DXVec3Dot(&vA[0], &vL[i])) +
//        vOBBDimsA[1] * fabs(D3DXVec3Dot(&vA[1], &vL[i])) +
//        vOBBDimsA[2] * fabs(D3DXVec3Dot(&vA[2], &vL[i]));
//      rb = 
//        vOBBDimsB[0] * fabs(D3DXVec3Dot(&vB[0], &vL[i])) +
//        vOBBDimsB[1] * fabs(D3DXVec3Dot(&vB[1], &vL[i])) +
//        vOBBDimsB[2] * fabs(D3DXVec3Dot(&vB[2], &vL[i]));
//      if (fabs(D3DXVec3Dot(&vT, &vL[i]) > (ra + rb)/2))
//        return false;
//    }
//    return true;
//  }  

/* OBB and vPosDeltaA are assumed to be in the frame of reference of
 * the OBBTree.  The triangles are all in the frame of reference of
 * the OBBTree as well. */
void
MathUtil::getPotentialTriangles(vector<int> &triangle_indices,
				const OBB &obb, const OBBTreeNode *pNode,
				const Vector &vTranslate,
				const Vector &vPosDeltaA) {
  float portion = 1.0;
  Vector vNormal;
  if (!CheckCollision(obb, *pNode, vTranslate, vPosDeltaA, &portion, vNormal))
    return;

  // add the node to the list of nodes we collided with for debugging
  g_oDebugCollisionNodes.push_back(pNode);

  // we could collide with the outermost bounding box of this node
  if (pNode->m_pChildNode[0] && pNode->m_pChildNode[1]) {
    // cerr << "processing child node 0: " << pNode->m_pChildNode[0] << endl;
    getPotentialTriangles(triangle_indices, obb, pNode->m_pChildNode[0], 
			  vTranslate, vPosDeltaA);
    // cerr << "processing child node 1: " << pNode->m_pChildNode[1] << endl;
    getPotentialTriangles(triangle_indices, obb, pNode->m_pChildNode[1], 
			  vTranslate, vPosDeltaA);
    // cerr << "done processing child nodes" << endl;
  } else if (!pNode->m_pChildNode[0] && !pNode->m_pChildNode[1]) {
    // cerr << "NO CHILDREN HERE: " << pNode << ", " 
    //      << pNode->m_oTriangleIndices.size() << endl;
    // no children, but our obb was hit.. add our triangles
    vector<int>::const_iterator iter;
    for (iter = pNode->m_oTriangleIndices.begin();
	 iter != pNode->m_oTriangleIndices.end(); ++iter)
      triangle_indices.push_back(*iter);
  } else {
    // The node should either have 2 child nodes, or none
    assert(!"Encountered OBBTreeNode with one child node.");
  }
}

/* OBB and vPosDeltaA are assumed to be in the absolute frame of
   reference. */
bool
MathUtil::CheckCollision(const OBB &obb, const OBBTree *pOBBTree,
			 const Vector &vTranslate,
			 const Vector &vPosDeltaA, 
			 float *portion, Vector &vNormal) {
  // All the OBBs in pOBBTree are not adjusted with the object's
  // transform matrix, so instead, apply the inverse transform to obb
  // and to vPosDeltaA.  When we are done, apply the normal transform
  // to the vNormal (and renormalize) before we return it to put it
  // back in absolute coordinates.

  OBB obb_trans;
  Rotation rRot;
  D3DXQuaternionInverse(&rRot, &pOBBTree->m_rRot);
  Vector vScale;
  vScale.x = 1.0 / pOBBTree->m_vScale.x;
  vScale.y = 1.0 / pOBBTree->m_vScale.y;
  vScale.z = 1.0 / pOBBTree->m_vScale.z;
  obb.make_trans(&obb_trans, rRot, vScale, pOBBTree->m_mInverseTransform);

  Vector vOBBCenter = obb.m_vPos;
  Vector vTranslatedCenter = obb.m_vPos + vTranslate;
  Vector vTranslatedMovedCenter = obb.m_vPos + vTranslate + vPosDeltaA;
 
  Vector vNewOBBCenter, vNewTranslatedCenter, vNewTranslatedMovedCenter;
  D3DXVec3TransformCoord(&vNewOBBCenter, &vOBBCenter, 
			 &pOBBTree->m_mInverseTransform);
  D3DXVec3TransformCoord(&vNewTranslatedCenter, &vTranslatedCenter, 
			 &pOBBTree->m_mInverseTransform);
  D3DXVec3TransformCoord(&vNewTranslatedMovedCenter, &vTranslatedMovedCenter, 
			 &pOBBTree->m_mInverseTransform);
  Vector vNewTrans = vNewTranslatedCenter - vNewOBBCenter;
  Vector vRelVel = vNewTranslatedMovedCenter - vNewTranslatedCenter;
  
  vector<int> triangle_indices;
  // get the list of potential triangles in the collision
  // later we might want to make this more efficient

  // For debugging, I want to buld a list of nodes that we collided with
  g_oDebugCollisionNodes.erase(g_oDebugCollisionNodes.begin(),
			       g_oDebugCollisionNodes.end());

  // use the obbtree to prune the list of triangles that we need to consider
  // for collision detection.
  getPotentialTriangles(triangle_indices, obb_trans, pOBBTree->m_pRootNode, 
			vNewTrans, vRelVel);
  if (!g_Config.m_bPruneTriangles) // Consider all triangles
    triangle_indices = pOBBTree->m_pRootNode->m_oTriangleIndices;

  // for debugging
  const_cast<OBBTree *>(pOBBTree)->m_oPotentialTriangles = triangle_indices;

#if CPP_DEBUG
//    float pre_y = obb.m_vPos.y - obb.m_vDims.y / 2;
//    float post_y = obb.m_vPos.y + vPosDeltaA.y - obb.m_vDims.y / 2;
//    if (pre_y > -128 && post_y < -128) {
//      if (triangles.size() == 0) { 
//        cerr << "missed node: ";
//      } else {
//        cerr << "hit node: ";
//      }
//      cerr << pOBBTree->m_pRootNode 
//  	 << ": " << (OBB)*(pOBBTree->m_pRootNode)
//  	 << " with: " << obb_trans << "; vRelVel = " << vRelVel << endl
//  	 << " or with: " << obb << "; vPosDeltaA = " << vPosDeltaA << endl
//  	 << " and with vNewTrans = " << vNewTrans
//  	 << " and mInverseTransform = " << pOBBTree->m_mInverseTransform
//  	 << endl;
//    }
#endif

  if (triangle_indices.size() == 0)
    return false;

  assert(*portion == 1.0); // this should always be 1, but make sure
  
  // For debugging, I will set up the list of triangles that we collided with.
  g_oDebugTriangleIndices.erase(g_oDebugTriangleIndices.begin(),
				g_oDebugTriangleIndices.end());

  Vector vTmp; 
  bool rc = CheckCollision(obb_trans, vNewTrans, vRelVel, triangle_indices,
			   pOBBTree->m_oTriangles, portion, vTmp);
  if (rc) {
    Vector vZero(0, 0, 0);
    D3DXVec3TransformCoord(&vZero, &vZero, &pOBBTree->m_mTransform);
    D3DXVec3TransformCoord(&vTmp, &vTmp, &pOBBTree->m_mTransform);
    vTmp = vTmp - vZero;
    D3DXVec3Normalize(&vNormal, &vTmp);
    if (*portion > 0)
      debug_info << "vNormal = " << vNormal
		 << "; portion = " << *portion << endl;
  }

  // Debugging of course.
  const_cast<OBBTree *>(pOBBTree)->m_oMissedTriIndices = 
    g_oDebugTriangleIndices;

  vector<int>::const_iterator iter;
  for (iter = g_oDebugTriangleIndices.begin(); 
       iter != g_oDebugTriangleIndices.end(); ++iter) {
    vector<const OBBTreeNode *> nodes;
    pOBBTree->m_pRootNode->getNodes(nodes, *iter);
    // do something with all those nodes -- i should collide with *all* of them
    vector<const OBBTreeNode *>::const_iterator iter2;
    vector<const OBBTreeNode *> missed_nodes;
    for (iter2 = nodes.begin(); iter2 != nodes.end(); ++iter2) {
      bool found = false;
      vector<const OBBTreeNode *>::const_iterator iter3;
      for (iter3 = g_oDebugCollisionNodes.begin(); 
	   iter3 != g_oDebugCollisionNodes.end(); ++iter3)
	if (*iter3 == *iter2) {
	  found = true;
	  break;
	}
      if (!found)
	missed_nodes.push_back(*iter2);
    }

    const_cast<OBBTree *>(pOBBTree)->m_oMissedNodes = missed_nodes;

    if (missed_nodes.size() != 0) {
      cerr << "should have collided with the following nodes: " << endl;
      for (iter2 = missed_nodes.begin(); 
	   iter2 != missed_nodes.end(); ++iter2)
	cerr << *iter2 << " ";
      cerr << endl;
    }
  }
  
  return rc;
}


// Here, A is the mover, and if A's colliding portion in projected onto 
// the resulting normal vector, A will slide off of B.
bool
MathUtil::CheckCollision(const OBB &obb1, const OBB &obb2,
			 const Vector &vTranslate, 
			 const Vector &vPosDeltaA,
			 float *portion, Vector &vNormal) {
  return CheckCollision(obb1.m_rRot, obb2.m_rRot, 
			obb1.m_vPos + vTranslate, obb2.m_vPos,
			obb1.m_vDims, obb2.m_vDims,
			vPosDeltaA, portion, vNormal);
}

static inline bool
debug_return(float debug[16][6], float min1, float max1, 
	     float min2, float max2, float w, int index) {
  debug_info << "failed to collide on index: " << index << endl;
  for (int i = 0; i < index; ++i) 
    debug_info << "in check collision2; "
	       << "(" << debug[i][0] << ", " << debug[i][1] << "), "
	       << "(" << debug[i][2] << ", " << debug[i][3] << "), "
	       << "(" << debug[i][4] << ", " << debug[i][5] << ")" << endl;
  debug_info << "* (" << min1 << ", " << max1 << "), "
	     << "* (" << min2 << ", " << max2 << ") " << w << endl;
  return false;
}

// Here, A is the mover (an OBB), and if A's colliding portion is projected 
// onto the resulting normal vector, A will slide off of the triangle.
bool
MathUtil::CheckCollision(const Rotation &rRot, const Vector &vOBBCenter, 
			 const Vector &vOBBDims, const Vector &vPosDelta, 
			 const Triangle &triangle,
			 float *portion, Vector &vNormal) {
  static const float epsilon = UNITS_PER_METER / 100.0f;
  // This Heuristic comes from David Eberly
  const Vector &vW = vPosDelta;
  float w; // projection of vW;
  Vector vA[3]; // Basis vectors of OBB
  Vector vB[3]; // Vectors along the edges of the triangle
  Vector vN; // Vector normal to planar face of the triangle
  float min1, max1, min2, max2;
  int i, j;

//    cerr << "vOBBCenter: " << vOBBCenter << endl;
//    cerr << "vOBBDims: " << vOBBDims << endl;
//    cerr << "vPosDelta: " << vPosDelta << endl;
//    cerr << "Triangle: " << triangle.p[0] << ", " 
//         << triangle.p[1] << ", " << triangle.p[2] << endl;
  GetRotationVectors(rRot, &vA[1], &vA[0], &vA[2]);
  
  // the triangle's three edge vectors
  vB[0] = triangle.p[1] - triangle.p[0];
  vB[1] = triangle.p[2] - triangle.p[1];
  vB[2] = triangle.p[0] - triangle.p[2];
  D3DXVec3Normalize(&vB[0], &vB[0]);
  D3DXVec3Normalize(&vB[1], &vB[1]);
  D3DXVec3Normalize(&vB[2], &vB[2]);
  // the triangle's face normal vector
  // i should choose a pair of faces with a non-tiny cross product
  D3DXVec3Cross(&vB[3], &vB[0], &vB[1]);
  D3DXVec3Normalize(&vB[3], &vB[3]);

  // Compute the vectors to the obb faces
  Vector vOBBFaces[3];
  GetRotatedFaceVectors(vOBBFaces, vOBBDims, rRot);

  // This will hold the times that the object collides in each projection
  float max_time = 1.0f;
  float min_time = 0.0f;
  float tmp_time;
  int last_index = 0;

  *portion = 1.0f;
  
  // For A's basis vectors
  for (i = 0; i < 3; i++) {
    ComputeBounds(&min1, &max1, vOBBFaces, vOBBCenter, vA[i]);
    ComputeBounds(&min2, &max2, triangle, vA[i]);
    w = D3DXVec3Dot(&vA[i], &vW);
    tmp_time = min_time;
    if (!CheckTime(&min_time, &max_time, min1, max1, min2, max2, w, epsilon))
      return false;
    if (min_time > tmp_time)
      last_index = i;
  }
  
  // For triangle's edge vectors (and face vector)
  for (i = 0; i < 4; i++) {
    ComputeBounds(&min1, &max1, vOBBFaces, vOBBCenter, vB[i]);
    ComputeBounds(&min2, &max2, triangle, vB[i]);
    w = D3DXVec3Dot(&vB[i], &vW);
    tmp_time = min_time;
    if (!CheckTime(&min_time, &max_time, min1, max1, min2, max2, w, epsilon))
      return false;
    if (min_time > tmp_time)
      last_index = 3 + i;
  }
  
  Vector vCross;
  // For the 9 cross products
  for (i = 0; i < 3; i++)
    for (j = 0; j < 3; j++) {
      D3DXVec3Cross(&vCross, &vA[i], &vB[j]);
      ComputeBounds(&min1, &max1, vOBBFaces, vOBBCenter, vCross);
      ComputeBounds(&min2, &max2, triangle, vCross);
      w = D3DXVec3Dot(&vCross, &vW);
      tmp_time = min_time;
      if (!CheckTime(&min_time, &max_time, min1, max1, min2, max2, w, epsilon))
	return false;
      if (min_time > tmp_time)
	last_index = 7 + 3 * i + j;
    }
  
  // Ok, they will hit. Find out what axis (in last_index), and when.
  // Find the last axis that we collide on.  This is the time we really collide
  Vector vTemp;
  if (last_index < 3)
    vTemp = vA[last_index];
  else if (last_index < 7)
    vTemp = vB[last_index - 3];
  else
    D3DXVec3Cross(&vTemp, &vA[(last_index - 7) / 3], 
		  &vB[(last_index - 7) % 3]);

  D3DXVec3Normalize(&vNormal, &vTemp);
  if (D3DXVec3Dot(&vNormal, &vW) > 0)
    vNormal *= -1;
  
  *portion = min_time;
//    debug_info << "collided on index: " << last_index
//  	     << "; *portion = " << *portion << endl; 
  return true;
}  

// Here, A is the mover (an OBB), and if A's colliding portion is projected 
// onto the resulting normal vector, A will slide off of the triangle.
bool
MathUtil::CheckCollision(const Rotation &rRotA, const Rotation rRotB, 
			 const Vector &vOBBCenterA, const Vector &vOBBCenterB, 
			 const Vector &vOBBDimsA, const Vector &vOBBDimsB, 
			 const Vector &vPosDeltaA, 
			 float *portion, Vector &vNormal) {
  static const float epsilon = UNITS_PER_METER / 100.0f;
  // This Heuristic comes from David Eberly
  const Vector &vW = vPosDeltaA;
  float w; // projection of vW;
  Vector vA[3]; // Basis vectors of OBB A
  Vector vB[3]; // Basis vectors of OBB B
  Vector vN; // Vector normal to obb collided with
  float min1, max1, min2, max2;
  int i, j;

  GetRotationVectors(rRotA, &vA[1], &vA[0], &vA[2]);
  GetRotationVectors(rRotB, &vB[1], &vB[0], &vB[2]);

  // Compute the vectors to the obb faces
  Vector vOBBFacesA[3], vOBBFacesB[3];
  GetRotatedFaceVectors(vOBBFacesA, vOBBDimsA, rRotA);
  GetRotatedFaceVectors(vOBBFacesB, vOBBDimsB, rRotB);

  // This will hold the times that the object collides in each projection
  float max_time = 1.0f;
  float min_time = 0.0f;
  float tmp_time;
  int last_index = 0;

  *portion = 1.0f;
  
  // For A's basis vectors
  for (i = 0; i < 3; i++) {
    ComputeBounds(&min1, &max1, vOBBFacesA, vOBBCenterA, vA[i]);
    ComputeBounds(&min2, &max2, vOBBFacesB, vOBBCenterB, vA[i]);
    w = D3DXVec3Dot(&vA[i], &vW);
    tmp_time = min_time;
    if (!CheckTime(&min_time, &max_time, min1, max1, min2, max2, w, epsilon))
      return false;
    if (min_time > tmp_time)
      last_index = i;
  }
  
  // For B's basis vectors
  for (i = 0; i < 3; i++) {
    ComputeBounds(&min1, &max1, vOBBFacesA, vOBBCenterA, vB[i]);
    ComputeBounds(&min2, &max2, vOBBFacesB, vOBBCenterB, vB[i]);
    w = D3DXVec3Dot(&vB[i], &vW);
    tmp_time = min_time;
    if (!CheckTime(&min_time, &max_time, min1, max1, min2, max2, w, epsilon))
      return false;
    if (min_time > tmp_time)
      last_index = i;
  }
  
  Vector vCross;
  // For the 9 cross products
  for (i = 0; i < 3; i++)
    for (j = 0; j < 3; j++) {
      D3DXVec3Cross(&vCross, &vA[i], &vB[j]);
      ComputeBounds(&min1, &max1, vOBBFacesA, vOBBCenterA, vCross);
      ComputeBounds(&min2, &max2, vOBBFacesB, vOBBCenterB, vCross);
      w = D3DXVec3Dot(&vCross, &vW);
      tmp_time = min_time;
      if (!CheckTime(&min_time, &max_time, min1, max1, min2, max2, w, epsilon))
	return false;
      if (min_time > tmp_time)
	last_index = 7 + 3 * i + j;
    }
  
  // Ok, they will hit. Find out what axis (in last_index), and when.
  // Find the last axis that we collide on.  This is the time we really collide
  Vector vTemp;
  if (last_index < 3)
    vTemp = vA[last_index];
  else if (last_index < 6)
    vTemp = vB[last_index - 3];
  else
    D3DXVec3Cross(&vTemp, &vA[(last_index - 6) / 3], 
		  &vB[(last_index - 6) % 3]);
  
  D3DXVec3Normalize(&vNormal, &vTemp);
  if (D3DXVec3Dot(&vNormal, &vW) > 0)
    vNormal *= -1;
  
  *portion = min_time;

//   debug_info << "collided on index: " << last_index
//   	     << "; *portion = " << *portion << endl 
// 	     << rRotA << rRotB << vOBBCenterA 
// 	     << vOBBCenterB << vOBBDimsA << vOBBDimsB << vPosDeltaA << endl;

  return true;
}  

// variant that takes triangle indices and a complete list of triangles
bool
MathUtil::CheckCollision(const OBB &obb, const Vector &vTranslate,
			 const Vector &vPosDelta,
			 const std::vector<int> &triangle_indices,
			 const std::vector<Triangle> &triangles,
			 float *portion, Vector &vNormal) {
  return CheckCollision(obb.m_rRot, obb.m_vPos + vTranslate, obb.m_vDims, 
			vPosDelta, triangle_indices, triangles, 
			portion, vNormal);
}

// variant that takes triangle indices and a complete list of triangles
bool
MathUtil::CheckCollision(const Rotation &rRot, const Vector &vOBBCenter, 
			 const Vector &vOBBDims, const Vector &vPosDelta, 
			 const std::vector<int> &triangle_indices,
			 const std::vector<Triangle> &triangles,
			 float *portion, Vector &vNormal) {
  float saved = 1.0f;
  Vector vTmp;
  std::vector<int>::const_iterator iter;
  for (iter = triangle_indices.begin(); 
       iter != triangle_indices.end(); ++iter) {
    if (CheckCollision(rRot, vOBBCenter, vOBBDims, vPosDelta, 
		       triangles[*iter], portion, vTmp) && *portion < saved) {
      // debug_info << "Collided with triangle #" << *iter << endl;
      g_oDebugTriangleIndices.push_back(*iter);
      vNormal = vTmp;
      saved = *portion;
    }
  }
  *portion = saved;
  return *portion < 1.0f;
}

// variant that just takes a list of triangles
bool
MathUtil::CheckCollision(const OBB &obb, const Vector &vTranslate,
			 const Vector &vPosDelta,
			 const std::vector<Triangle> &triangles,
			 float *portion, Vector &vNormal) {
  return CheckCollision(obb.m_rRot, obb.m_vPos + vTranslate, obb.m_vDims, 
			vPosDelta, triangles, portion, vNormal);
}

// variant that just takes a list of triangles
bool
MathUtil::CheckCollision(const Rotation &rRot, const Vector &vOBBCenter, 
			 const Vector &vOBBDims, const Vector &vPosDelta, 
			 const std::vector<Triangle> &triangles,
			 float *portion, Vector &vNormal) {
  float saved = 1.0f;
  Vector vTmp;
  std::vector<Triangle>::const_iterator iter;
  for (iter = triangles.begin(); iter != triangles.end(); ++iter) {
    if (CheckCollision(rRot, vOBBCenter, vOBBDims, vPosDelta, 
		       *iter, portion, vTmp) && *portion < saved) {
      vNormal = vTmp;
      saved = *portion;
    }
  }
  *portion = saved;
  return *portion < 1.0f;
}

const float MathUtil::EPSILON = 0.000001f;
bool
MathUtil::RayIntersectBox(const Vector &orig, const Vector &dir,
			  const Vector &min, const Vector &max) {
  bool inside = true;
  int i;
  Vector maxt(-1, -1, -1);
  
  // Find candidate planes.
  for (i = 0; i < 3; ++i) {
    if (orig[i] < min[i]) {
      inside = false;
      // Calculate distance to slab
      if (dir[i])
	maxt[i] = (min[i] - orig[i]) / dir[i];
    } else if (orig[i] > max[i]) {
      inside = false;
      // Calculate distance to slab
      if (dir[i])
	maxt[i] = (max[i] - orig[i]) / dir[i];
    }
  }
  
  if (inside)
    return true; // origin is inside the box
  
  int max_plane = 0;
  if (maxt[1] > maxt[max_plane])
    max_plane = 1;
  if (maxt[2] > maxt[max_plane])
    max_plane = 2;
  if (maxt[max_plane] < 0)
    return false;

  for (i = 0; i < 3; ++i) {
    float tmp = orig[i] + maxt[max_plane] * dir[i]; // go to slab it hits last
    if (tmp < min[i] - EPSILON || tmp > max[i] + EPSILON)
      return false; // not in one of the slabs
  }
  return true;
}

// Ironically this method is slower, but oh well
bool
MathUtil::RayIntersectTriangle(const Vector &orig, const Vector &dir,
			       const Vector &vert0, const Vector &vert1, 
			       const Vector &vert2) {
  float tmp[3];
  return RayIntersectTriangle(orig, dir, vert0, vert1, vert2, 
			      &tmp[0], &tmp[1], &tmp[2]);
}
#endif
		public static Quaternion FromEulerAngles(float yaw, float pitch, float roll) {
			double c1 = Math.Cos(yaw / 2);
			double s1 = Math.Sin(yaw / 2);
			double c2 = Math.Cos(pitch / 2);
			double s2 = Math.Sin(pitch / 2);
			double c3 = Math.Cos(roll / 2);
			double s3 = Math.Sin(roll / 2);
			return new Quaternion((float)(c1 * c2 * c3 - s1 * s2 * s3),
								  (float)(c1 * c2 * s3 + s1 * s2 * c3),
								  (float)(s1 * c2 * c3 + c1 * s2 * s3),
								  (float)(c1 * s2 * c3 - s1 * c2 * s3));
		}

		/// <summary>
		///   Returns the yaw associated with the quaternion.  Note that while the
		///   range of the pitch and roll is complete, the range of the yaw is only 
		///   half of the circle.
		/// </summary>
		/// <param name="q"></param>
		/// <returns>An angle, θ, measured in radians, such that -π/2≤θ≤π/2</returns>
		public static float GetYaw(Quaternion q) {
			return (float)Math.Asin(-2 * (q.x * q.z - q.w * q.y));
		}

		/// <summary>
		///   Returns the yaw associated with the quaternion.  This variant
		///   allows the yaw any value in the circle.  If the roll or pitch
		///   are outside the range (-π/2, π/2), this may return a misleading
		///   value.
		/// </summary>
		/// <param name="q"></param>
		/// <returns>An angle, θ, measured in radians, such that -π≤θ≤π</returns>
		public static float GetFullYaw(Quaternion q) {
			Vector3 dir = Vector3.UnitZ;
			Vector3 newDir = q * dir;
			// map x => y and z => x
			return (float)Math.Atan2(newDir.x, newDir.z);
		}

		/// <summary>
		///   Get the roll associated with this quaternion.
		/// </summary>
		/// <param name="q"></param>
		/// <returns>An angle, θ, measured in radians, such that -π≤θ≤π.</returns>
		public static float GetRoll(Quaternion q) {
			return (float)Math.Atan2(2 * (q.x * q.y + q.w * q.z), 
									 q.w * q.w + q.x * q.x - q.y * q.y - q.z * q.z);
		}

		/// <summary>
		///   Get the pitch associated with this quaternion.
		/// </summary>
		/// <param name="q"></param>
		/// <returns>An angle, θ, measured in radians, such that -π≤θ≤π.</returns> 
		public static float GetPitch(Quaternion q) {
			return (float)Math.Atan2(2 * (q.y * q.z + q.w * q.x), 
									 q.w * q.w - q.x * q.x - q.y * q.y + q.z * q.z);
		}

		/// <summary>
		///   Calculate whether (and where) a ray intersects a triangle
		///   Are vertices clockwise ??
		/// </summary>
		/// <param name="orig">Origin point of the ray</param>
		/// <param name="dir">Direction of the ray</param>
		/// <param name="v0">Vertex 1 of the triangle</param>
		/// <param name="v1">Vertex 1 of the triangle</param>
		/// <param name="v2">Vertex 1 of the triangle</param>
		/// <param name="t">iirc, this is the distance to the intersection</param>
		/// <param name="u">u coordinate of the intersection on the triangle</param>
		/// <param name="v">v coordinate of the intersection on the triangle</param>
		/// <returns></returns>
		public static bool RayIntersectTriangle(Vector3 orig, Vector3 dir,
												Vector3 v0, Vector3 v1, Vector3 v2,
												out float t, out float u, out float v)
		{
			t = 0.0f;
			u = 0.0f;
			v = 0.0f;
			Vector3 tvec, pvec, qvec;
			float det, inv_det;
			// find vectors for two edges sharing vert0
			Vector3 edge1 = v1 - v0;
			Vector3 edge2 = v2 - v0;
			// begin calculating determinant - also used to calculate U parameter 
			pvec = dir.Cross(edge2);
			// if determinant is near zero, ray lies in plane of triangle 
			det = edge1.Dot(pvec);

			if (det > Epsilon) {
				// calculate distance from vert0 to ray origin
				tvec = orig - v0;
				// calculate U parameter and test bounds
				u = tvec.Dot(pvec);
				if (u < 0.0f || u > det)
					return false;
				// prepare to test V parameter
				qvec = tvec.Cross(edge1);
				// calculate V parameter and test bounds
				v = dir.Dot(qvec);
				if (v < 0.0f || u + v > det)
					return false;
#if NO_CULL_BACKFACES
			} else if (det < -Epsilon) {
				// calculate distance from vert0 to ray origin
				tvec = orig - v0;
				// calculate U parameter and test bounds
				u = tvec.Dot(pvec);
				if (u > 0.0f || u < det)
					return false;
				// prepare to test V parameter
				qvec = tvec.Cross(edge1);
				// calculate V parameter and test bounds
				v = dir.Dot(qvec);
				if (v > 0.0f || u + v < det)
					return false;
#endif
			} else
				return false;

			inv_det = 1.0f / det;

			// calculate t, scale parameters, ray intersects triangle
			t = edge2.Dot(qvec) * inv_det;
			u *= inv_det;
			v *= inv_det;

			return true;
		}

#if CPP_FOO
// returns 0 (partial), -1 (none), or 1 (full -- box1 contains box2)
int
MathUtil::BoundingBoxIntersect(const Vector &vMin1, const Vector &vMax1,
			       const Vector &vMin2, const Vector &vMax2) {
  if (vMin1.x > vMax2.x || vMin1.y > vMax2.y || vMin1.z > vMax2.z ||
     vMax1.x < vMin2.x || vMax1.y < vMin2.y || vMax1.z < vMin2.z)
    return -1;
  else if (vMin1.x < vMin2.x && vMin1.y < vMin2.y && vMin1.z < vMin2.z &&
	  vMax1.x > vMax2.x && vMax1.y > vMax2.y && vMax1.z > vMax2.z)
    return 1;
  else
    // Is this right?
    return 0;
}

void
MathUtil::TransformTriangle(Triangle &result, const Matrix &m, 
			    const Triangle &t) {
  D3DXVec3TransformCoord(&result.p[0], &t.p[0], &m);
  D3DXVec3TransformCoord(&result.p[1], &t.p[1], &m);
  D3DXVec3TransformCoord(&result.p[2], &t.p[2], &m);
}

void
MathUtil::TransformTriangles(list<Triangle> &result, const Matrix &m, 
			    const list<Triangle> &t_list) {
  result.erase(result.begin(), result.end());
  Triangle tmp;
  list<Triangle>::const_iterator iter;
  for (iter = t_list.begin(); iter != t_list.end(); ++iter) {
    TransformTriangle(tmp, m, (*iter));
    result.push_back(tmp);
  }
}

bool
MathUtil::TriTriIntersect(const list<Triangle> &t_list1, 
			  const list<Triangle> &t_list2) {
  list<Triangle>::const_iterator iter1;
  list<Triangle>::const_iterator iter2;
  for (iter1 = t_list1.begin(); iter1 != t_list1.end(); ++iter1)
    for (iter2 = t_list2.begin(); iter2 != t_list2.end(); ++iter2)
      if (TriTriIntersect(*iter1, *iter2))
	return true;
  return false;
}


bool
MathUtil::TriTriIntersect(const Triangle &t1, const Triangle &t2) {
  return TriTriIntersect(t1.p[0], t1.p[1], t1.p[2], t2.p[0], t2.p[1], t2.p[2]);
}
#endif

		/// <summary>
		///   This algorithm was taken from Tomas Moller
		///   Do the triangles specified by (v0, v1, v2) and (u0, u1, u2) intersect?
		/// </summary>
		/// <param name="v0">Vertex 1 from triangle 1</param>
		/// <param name="v1">Vertex 2 from triangle 1</param>
		/// <param name="v2">Vertex 3 from triangle 1</param>
		/// <param name="u0">Vertex 1 from triangle 2</param>
		/// <param name="u1">Vertex 2 from triangle 2</param>
		/// <param name="u2">Vertex 3 from triangle 2</param>
		/// <returns>true if the triangles instersect</returns>
		static bool TriTriIntersect(Vector3 v0, Vector3 v1, Vector3 v2,
									Vector3 u0, Vector3 u1, Vector3 u2) {
			Vector3 e1, e2, n1, n2, d, du, dv, vp, up;
			float d1, d2;
			float[] isect1 = new float[2];
			float[] isect2 = new float[2];
			float du0du1, du0du2, dv0dv1, dv0dv2;
			short index;
			float b, c, max;

			// compute plane equation of triangle(v0, v1, v2)
			e1 = v1 - v0;
			e2 = v2 - v0;
			n1 = e1.Cross(e2);
			d1 = -n1.Dot(v0);
			// plane equation 1: n1 + d1 = 0

			// put v0, v1, v2 into plane equation 1 to compute distances to the plane
			du.x = n1.Dot(u0) + d1;
			du.y = n1.Dot(u1) + d1;
			du.z = n1.Dot(u2) + d1;

			// coplanarity robustness check 
			if (du.x < Epsilon && du.x > -Epsilon)
				du.x = 0.0f;
			if (du.y < Epsilon && du.y > -Epsilon)
				du.y = 0.0f;
			if (du.z < Epsilon && du.z > -Epsilon)
				du.z = 0.0f;

			du0du1 = du.x * du.y;
			du0du2 = du.x * du.z;

			if (du0du1 > 0.0f && du0du2 > 0.0f) // same sign on all of them + not equal 0
				return false;                   // no intersection occurs 

			// compute plane of triangle (u0, u1, u2)
			e1 = u1 - u0;
			e2 = u2 - u0;
			n2 = e1.Cross(e2);
			d2 = -n2.Dot(u0);
			// plane equation 2: n2 + d2 = 0

			// put u0, u1, u2 into plane equation 1 to compute distances to the plane
			dv.x = n2.Dot(v0) + d2;
			dv.y = n2.Dot(v1) + d2;
			dv.z = n2.Dot(v2) + d2;

			// coplanarity robustness check 
			if (dv.x < Epsilon && dv.x > -Epsilon)
				dv.x = 0.0f;
			if (dv.y < Epsilon && dv.y > -Epsilon)
				dv.y = 0.0f;
			if (dv.z < Epsilon && dv.z > -Epsilon)
				dv.z = 0.0f;

			dv0dv1 = dv.x * dv.y;
			dv0dv2 = dv.x * dv.z;

			if (dv0dv1 > 0.0f && dv0dv2 > 0.0f) // same sign on all of them + not equal 0
				return false;                   // no intersection occurs 

			// compute direction of intersection line
			d = n1.Cross(n2);

			// compute and index to the largest component of D
			max = Math.Abs(d.x);
			index = 0;
			b = Math.Abs(d.y);
			c = Math.Abs(d.z);
			if (b > max) {
				max = b;
				index = 1;
			}
			if (c > max) {
				max = c;
				index = 2;
			}

			// this is the simplified projection onto L
			vp.x = v0[index];
			vp.y = v1[index];
			vp.z = v2[index];

			up.x = u0[index];
			up.y = u1[index];
			up.z = u2[index];

			// compute interval for triangle 1
			if (!ComputeIntersect(vp, dv, dv0dv1, dv0dv2, ref isect1[0], ref isect1[1])) {
				Trace.TraceInformation("Coplanar triangles.. ");
				return false; // I can pretend these don't intersect
			}

			// compute interval for triangle 2
			if (!ComputeIntersect(up, du, du0du1, du0du2, ref isect2[0], ref isect2[1])) {
				Trace.TraceInformation("Coplanar triangles.. ");
				return false; // I can pretend these don't intersect
			}

			if (isect1[0] > isect1[1]) {
				float tmp = isect1[0];
				isect1[0] = isect1[1];
				isect1[1] = tmp;
			}

			if (isect2[0] > isect2[1]) {
				float tmp = isect2[0];
				isect2[0] = isect2[1];
				isect2[1] = tmp;
			}

			if (isect1[1] < isect2[0] || isect2[1] < isect1[0]) 
				return false;
			return true;
		}

		static bool ComputeIntersect(Vector3 v, Vector3 d, float d0d1, float d0d2,
									 ref float isect0, ref float isect1) {
			if (d0d1 > 0.0f) {        // d0d2 <= 0.0
				isect0 = v.z + (v.x - v.z) * d.z / (d.z - d.x);
				isect1 = v.z + (v.y - v.z) * d.z / (d.z - d.y);
			} else if (d0d2 > 0.0f) { // d0d1 <= 0.0
				isect0 = v.y + (v.x - v.y) * d.y / (d.y - d.x);
				isect1 = v.y + (v.z - v.y) * d.y / (d.y - d.z);
			} else if (d.y * d.z > 0.0f || d.x != 0.0f) { // d0d1 <= 0.0 or d.x != 0.0
				isect0 = v.x + (v.y - v.x) * d.x / (d.x - d.y);
				isect1 = v.x + (v.z - v.x) * d.x / (d.x - d.z);
			} else if (d.y != 0.0f) {    
				isect0 = v.y + (v.x - v.y) * d.y / (d.y - d.x);
				isect1 = v.y + (v.z - v.y) * d.y / (d.y - d.z);
			} else if (d.z != 0.0f) {
				isect0 = v.z + (v.x - v.z) * d.z / (d.z - d.x);
				isect1 = v.z + (v.y - v.z) * d.z / (d.z - d.y);
			} else { // triangles are coplanar
				return false;
			}
			return true;
		}


#if CPP_FOO
void
MathUtil::ComputeTransform(Matrix *pmTrans, const Rotation &rRot,
			   const Vector &vScale, const Vector &vPos) {
  Matrix mScale;
  D3DXMatrixScaling(&mScale, vScale.x, vScale.y, vScale.z);
  Matrix mRot;
  D3DXMatrixRotationQuaternion(&mRot, &rRot);
  Matrix mPos;
  D3DXMatrixTranslation(&mPos, vPos.x, vPos.y, vPos.z);
  Matrix mTmp;
  // Apply rotation, scale, and translation
  // it seems like the directx docs are wrong here
  D3DXMatrixMultiply(&mTmp, &mRot, &mScale);
  D3DXMatrixMultiply(pmTrans, &mTmp, &mPos);
}
#endif

	}
}
