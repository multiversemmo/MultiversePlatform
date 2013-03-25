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

#region Using directives

using System;
using System.Text;

#endregion

namespace Multiverse.MathLib
{
	public class ObbTree
	{
		public ObbTree() {
#include "meshinfo.h"
#include "mathutil.h"
#include "obbtree.h"
#include <math.h>

#include <iostream>
using namespace std;

#define ALGO2

static inline void rotate_matrix(Matrix &mMatrix, double s, double tau, 
				 int i, int j, int k, int l) {
  double g, h;
  g = mMatrix(i, j);
  h = mMatrix(k, l);
  mMatrix(i, j) = g - s * (h + g * tau);
  mMatrix(k, l) = h + s * (g - h * tau);
}

// destroys mMatrix
static inline void jacobi(Matrix &mEigenVectors, const Matrix &mMatrix) {
  Matrix mTmp = mMatrix;
  Vector vEigenVals;
  Vector vTmpB, vTmpZ;
  const int max_rounds = 50;
  
  int n = 3; // dimension
  int i, j, p, q;
  for (p = 0; p < n; ++p) {
    for (q = 0; q < n; ++q)
      mEigenVectors(p, q) = 0.0;
    mEigenVectors(p, p) = 1.0; // cheaper to set twice than check
  }
  for (p = 0; p < n; ++p) {
    vTmpB[p] = vEigenVals[p] = mTmp(p, p);
    vTmpZ[p] = 0.0;
  }
  for (i = 1; i < max_rounds; ++i) {
    double sm = 0.0;
    for (p = 0; p < n - 1; ++p) 
      for (q = p + 1; q < n; ++q)
	sm += fabs(mTmp(p, q));
    if (sm == 0.0) 
      return; // we are done.. 
    
    double tresh;
    if (i < 4)
      tresh = .2 * sm / (n * n);
    else
      tresh = 0.0;
    for (p = 0; p < n - 1; ++p) 
      for (q = p + 1; q < n; ++q) {
	double g = 100 * fabs(mTmp(p, q));
	if (i > 4 
	    && (fabs(vEigenVals[p]) + g) == fabs(vEigenVals[p])
	    && (fabs(vEigenVals[q]) + g) == fabs(vEigenVals[q]))
	  mTmp(p, q) = 0.0;
	else if (fabs(mTmp(p, q)) > tresh) {
	  double h = vEigenVals[q] - vEigenVals[p];
	  double t;
	  if ((fabs(h) + g) == fabs(h))
	    t = mTmp(p, q) / h;
	  else {
	    double theta = 0.5 * h / mTmp(p, q);
	    t = 1.0 / (fabs(theta) + sqrt(1.0 + theta * theta));
	    if (theta < 0.0)
	      t = -t;
	  }
	  double c = 1.0 / sqrt(1.0 + t * t);
	  double s = t * c;
	  double tau = s / (1.0 + c);
	  h = t * mTmp(p, q);
	  vTmpZ[p] -= h;
	  vTmpZ[q] += h;
	  vEigenVals[p] -= h;
	  vEigenVals[q] += h;
	  mTmp(p, q) = 0.0;
	  for (j = 0; j < p; ++j)
	    rotate_matrix(mTmp, s, tau, j, p, j, q);
	  for (j = p + 1; j < q; ++j)
	    rotate_matrix(mTmp, s, tau, p, j, j, q);
	  for (j = q + 1; j < n; ++j)
	    rotate_matrix(mTmp, s, tau, p, j, q, j);
	  for (j = 0; j < n; ++j)
	    rotate_matrix(mEigenVectors, s, tau, j, p, j, q);
	}
      }
    for (p = 0; p < n; ++p) {
      vTmpB[p] += vTmpZ[p];
      vEigenVals[p] = vTmpB[p];
      vTmpZ[p] = 0.0;
    }
  }
  cerr << "too many rounds!" << endl;
}
  

OBBTree::OBBTree() {
  D3DXMatrixIdentity(&m_mInverseTransform);
  D3DXMatrixIdentity(&m_mTransform);
}

void
OBBTree::setTransform(const Rotation &rRot, const Vector &vScale, 
		      const Vector &vPos) {
  Matrix mTrans;
  MathUtil::ComputeTransform(&mTrans, rRot, vScale, vPos);
  setTransform(rRot, vScale, vPos, mTrans);
}

void
OBBTree::setTransform(const Rotation &rRot, const Vector &vScale, 
		      const Vector &vPos, const Matrix &mTransform) {
  assert(vScale.x == vScale.y && vScale.y == vScale.z);
  m_rRot = rRot;
  m_vScale = vScale;
  m_vPos = vPos;
  m_mTransform = mTransform;
  D3DXMatrixInverse(&m_mInverseTransform, NULL, &m_mTransform);
}

void
OBBTree::init(const MeshInfo *mesh_info) {
  m_pMeshInfo = mesh_info;
  m_oTriangles.clear();
  m_oTriangleAreas.clear();
  m_oTriangleMeans.clear();
  m_oAdjustedMeans.clear();
  HRESULT hr = m_pMeshInfo->FetchMesh(m_oTriangles);
  if (FAILED(hr))
    assert(!"Failed to fetch mesh");
  // Set up the triangle areas
  int n = m_oTriangles.size();
  vector<int> triangle_indices; // indices of the triangles in the root node
  triangle_indices.resize(n);
  for (int i = 0; i < n; ++i) {
    Triangle &t = m_oTriangles[i];
    Vector e1, e2, cross;
    e1 = t.p[1] - t.p[2];
    e2 = t.p[2] - t.p[0];
    D3DXVec3Cross(&cross, &e1, &e2);
    float fArea = D3DXVec3Length(&cross) / 2;
    Vector vMean = (t.p[0] + t.p[1] + t.p[2]) / 3;
    Vector vAdjustedMean = vMean / fArea;
    m_oTriangleAreas.push_back(fArea);
    m_oTriangleMeans.push_back(vMean);
    m_oAdjustedMeans.push_back(vAdjustedMean);
    triangle_indices[i] = i;
  }
  m_pRootNode = new OBBTreeNode(this, triangle_indices);
  cerr << "Created an OBB tree with " << m_pRootNode->getCount() 
       << " nodes and with depth of " << m_pRootNode->getDepth() << endl;
}

// For debugging.. gets the nodes at a given level
void
OBBTree::getNodesAtLevels(std::vector<const OBBTreeNode *> &nodes, 
			  unsigned int bits) const {
  m_pRootNode->getNodesAtLevels(nodes, bits);
}

// This is a monstrous function
OBBTreeNode::OBBTreeNode(const OBBTree *pTree, 
			 const vector<int> &triangle_indices) {
  m_pOBBTree = pTree;
  m_oTriangleIndices = triangle_indices;
  
  // Generate the covariance matrix
  int i, j, k, l;
  int n = m_oTriangleIndices.size();
  Matrix mCovariance;
  D3DXMatrixIdentity(&mCovariance);
  Vector u(0, 0, 0);
#ifdef ALGO2
  for (i = 0; i < n; ++i)
    u += m_pOBBTree->m_oAdjustedMeans[m_oTriangleIndices[i]];
  u /= 2 * n; // this would be 6 * n, but I used the adjusted mean
#else
  for (i = 0; i < n; ++i)
    u += m_pOBBTree->m_oTriangleMeans[m_oTriangleIndices[i]];
  u /= n;
#endif
  vector<Triangle> triangles_prime;
  Triangle t;
  for (i = 0; i < n; ++i) {
    for (j = 0; j < 3; ++j) 
      t.p[j] = m_pOBBTree->m_oTriangles[m_oTriangleIndices[i]].p[j] - u;
    triangles_prime.push_back(t);
  }
  float tmp, fj[3], fk[3];
  for (j = 0; j < 3; ++j) {
    for (k = 0; k < 3; ++k) {
      tmp = 0.0;
#ifdef ALGO2
      for (i = 0; i < n; ++i) {
	for (l = 0; l < 3; ++l) {
	  fj[l] = triangles_prime[i].p[l][j];
	  fk[l] = triangles_prime[i].p[l][k];
	}
	tmp += m_pOBBTree->m_oTriangleAreas[m_oTriangleIndices[i]] * 
	  (((fj[0] + fj[1] + fj[2]) * (fk[0] + fk[1] + fk[2])) +
	   (fj[0] * fk[0]) + (fj[1] * fk[1]) + (fj[2] * fk[2]));
      }
      mCovariance(j, k) = tmp / (24 * n);
#else
      for (i = 0; i < n; ++i) {
	for (l = 0; l < 3; ++l) {
	  fj[l] = triangles_prime[i].p[l][j];
	  fk[l] = triangles_prime[i].p[l][k];
	}
	tmp += (fj[0] * fk[0]) + (fj[1] * fk[1]) + (fj[2] * fk[2]);
      }
      mCovariance(j, k) = tmp / (3 * n);
#endif
    }
  }
  
  // Now compute the eigenvectors of the covariance matrix
  // then normalize to get basis vectors
  Matrix mEigenVectors;
  jacobi(mEigenVectors, mCovariance);
  
//    cerr << "mEigenVectors = " << mEigenVectors << endl;
//    cerr << "mCovariance = " << mCovariance << endl;
  Vector vBasis[3];
  for (i = 0; i < 3; ++i) {
    Vector vTmp(mEigenVectors(0, i), mEigenVectors(1, i), mEigenVectors(2, i));
    D3DXVec3Normalize(&vBasis[i], &vTmp);
  }
 
  Matrix mTmp;
  D3DXMatrixIdentity(&mTmp);
  for (i = 0; i < 3; ++i)
    for (j = 0; j < 3; ++j)
      mTmp(i, j) = vBasis[j][i];
  
  // Build a rotation object based on the three basis vectors in mEigenVectors
  D3DXQuaternionRotationMatrix(&m_rRot, &mTmp);
  // I'm not sure why, but the matrix I get back seems to be the
  // inverse of what I expected.  Adjust.
  D3DXQuaternionInverse(&m_rRot, &m_rRot);

  // Now project the various points of the triangle onto the eigenvectors to
  // compute the box bounds
  float min, max, tmp_min, tmp_max;
  m_vPos = Vector(0, 0, 0);
  for (j = 0; j < 3; ++j) {
    for (i = 0; i < n; ++i) {
      MathUtil::ComputeBounds(&tmp_min, &tmp_max, 
			      m_pOBBTree->m_oTriangles[m_oTriangleIndices[i]],
			      vBasis[j]);
      if (i == 0 || tmp_min < min)
	min = tmp_min;
      if (i == 0 || tmp_max > max)
	max = tmp_max;
    }
    m_vDims[j] = max - min;
    m_vPos += ((min + max) / 2) * vBasis[j];
  }
  // update origpos, origrot and origdims - leave in same ref. frame
  m_vOrigDims = m_vDims;
  m_vOrigPos = m_vPos;
  m_rOrigRot = m_rRot;
  
//    cerr << "Basis vectors are: " << vBasis[0] << ", " << vBasis[1]
//         << ", " << vBasis[2] << endl;
//    cerr << "Rotation is: " << m_rRot << endl;
//    cerr << "Center of object is at: " << m_vPos << endl;
//    cerr << "Dimensions of object are: " << m_vDims << endl;
//    cerr << "Covariance matrix for object is: " << mCovariance << endl;

  if (n == 1) { // we can't divide.. we should already be a leaf node
    m_pChildNode[0] = m_pChildNode[1] = NULL;
    return;
  }
    

  // Now build the two triangle lists (for our child nodes) and process them 
  // sort dimensions by length, so that indexes[0] is the index of the 
  // longest axis, indexes[1] is the second longest, etc..
  int indexes[3];
  vector<float> dimensions;
  dimensions.resize(3);
  for (i = 0; i < 3; ++i)
    dimensions[i] = m_vDims[i];
  sort(dimensions.begin(), dimensions.end());
  for (i = 0; i < 3; ++i) // offset into dimensions
    for (j = 0; j < 3; ++j) { // offset into m_vDims
      if (dimensions[i] == m_vDims[j]) {
	bool dup = false;
	for (k = 0; k < i; ++k)
	  if (indexes[k] == j) // we have already used this axis
	    dup = true;
	if (!dup) {
	  indexes[i] = j;
	  break; // out to the i loop
	}
      }
    }

  vector<int> vTriangles[2];
  // try to split it along the different axes
  for (i = 2; i >= 0; --i) {
    if (split(vTriangles, vBasis[indexes[i]]))
      break;
  }
  
  if (vTriangles[0].size() == 0 || vTriangles[1].size() == 0) {
    // we are indivisible (a leaf node)
    m_pChildNode[0] = m_pChildNode[1] = NULL;
    return;
  }

  m_pChildNode[0] = new OBBTreeNode(m_pOBBTree, vTriangles[0]);
  m_pChildNode[1] = new OBBTreeNode(m_pOBBTree, vTriangles[1]);
}

// try to split it along this axis -- return true if the axis is good
bool
OBBTreeNode::split(vector<int> *vTriangles, const Vector& vAxis) const {
  int n = m_oTriangleIndices.size();
  int i;
  vTriangles[0].clear();
  vTriangles[1].clear();

  vector<float> vMeans;
  float pt;
  for (i = 0; i < n; ++i) {
    pt =  D3DXVec3Dot(&m_pOBBTree->m_oTriangleMeans[m_oTriangleIndices[i]],
		      &vAxis);
    vMeans.push_back(pt);
  }
  sort(vMeans.begin(), vMeans.end());
  float median = vMeans[vMeans.size() / 2];
  
  for (i = 0; i < n; ++i) {
    if (D3DXVec3Dot(&m_pOBBTree->m_oTriangleMeans[m_oTriangleIndices[i]],
		    &vAxis) < median)
      vTriangles[0].push_back(m_oTriangleIndices[i]);
    else
      vTriangles[1].push_back(m_oTriangleIndices[i]);
  }

  if (vTriangles[0].size() == 0 || vTriangles[1].size() == 0)
    return false; // we didn't divide, and we may be able to.
  return true; // we divided the triangles along the axis
}

int
OBBTreeNode::getDepth() const {
  if (m_pChildNode[0] == NULL)
    return 1;
  int tmp[2];
  tmp[0] = m_pChildNode[0]->getDepth();
  tmp[1] = m_pChildNode[1]->getDepth();
  return 1 + ((tmp[0] > tmp[1]) ? tmp[0] : tmp[1]);
}

int
OBBTreeNode::getCount() const {
  if (m_pChildNode[0] == NULL)
    return 1;
  return 1 + m_pChildNode[0]->getCount() + m_pChildNode[0]->getCount();
}

// Get all the nodes that contain the given indexed triangle.
bool
OBBTreeNode::getNodes(vector<const OBBTreeNode *> &nodes, int t_index) const {
  int i;
  bool found = false;
  for (i = 0; i < m_oTriangleIndices.size(); ++i) 
    if (m_oTriangleIndices[i] == t_index) {
      found = true;
      break;
    }
  
  if (found) {
    nodes.push_back(this);
    if (m_pChildNode[0])
      m_pChildNode[0]->getNodes(nodes, t_index);
    if (m_pChildNode[1])
      m_pChildNode[1]->getNodes(nodes, t_index);
  }
  return found;
}

// For debugging.. gets the nodes at a given level
void
OBBTreeNode::getNodesAtLevels(std::vector<const OBBTreeNode *> &nodes, 
			      unsigned int bits) const {
  if (bits & 1) // include this level
    nodes.push_back(this);
  if (m_pChildNode[0])
    m_pChildNode[0]->getNodesAtLevels(nodes, bits >> 1);
  if (m_pChildNode[1])
    m_pChildNode[1]->getNodesAtLevels(nodes, bits >> 1);
}

bool
OBBTreeNode::checkPoints() const {
  vector<int>::const_iterator iter;
  for (iter = m_oTriangleIndices.begin(); 
       iter != m_oTriangleIndices.end(); ++iter) 
    for (int j = 0; j < 3; ++j) {
      const Triangle &t = m_pOBBTree->m_oTriangles[(*iter)];
      if (!containsPoint(t.p[j])) {
	assert(!"not pt not contained by box");
 	return false;
      }
    }
  if (m_pChildNode[0] && !m_pChildNode[0]->checkPoints())
    return false;
  if (m_pChildNode[1] && !m_pChildNode[1]->checkPoints())
    return false;
  return true;
}

		}
	}
}
