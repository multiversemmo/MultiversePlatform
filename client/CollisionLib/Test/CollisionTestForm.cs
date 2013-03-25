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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Axiom.MathLib;
using Multiverse.CollisionLib;

namespace CollisionLibTest
{
    public partial class CollisionTestForm : Form
    {

        public CollisionTestForm()
        {
            InitializeComponent();
        }

        private void FourCollisionsButton_Click(object sender, EventArgs e)
        {
            CollisionAPI API = new CollisionAPI();
            // Create some obstacles, at 4 corners of a square
            List<CollisionShape> shapes = new List<CollisionShape>();
            
            CollisionSphere sphere = new CollisionSphere(new Vector3(0f, 0f, 2f), 1f);
            shapes.Add(sphere);
            API.AddCollisionShape(sphere, 1);
            
            CollisionCapsule capsule = new CollisionCapsule(new Vector3(10f,0f,0f),
                                                            new Vector3(10f,0f,4f),
                                                            1f);
            shapes.Add(capsule);
            API.AddCollisionShape(capsule, 2);

            // Now, an AABB
            CollisionAABB aabb = new CollisionAABB(new Vector3(9.5f,9.5f,0f),
                                                   new Vector3(10.5f,10.5f,4f));
            shapes.Add(aabb);
            API.AddCollisionShape(aabb, 3);

            CollisionOBB obb = new CollisionOBB(new Vector3(0f,10f,2),
                                                   new Vector3[3] {
                                                       new Vector3(1f,0f,0f),
                                                       new Vector3(0f,1f,0f),
                                                       new Vector3(0f,0f,1f)},
                                                new Vector3(1f, 1f, 1f));
            shapes.Add(obb);
            API.AddCollisionShape(obb, 4);

            FileStream f = new FileStream("C:\\Junk\\DumpSphereTree.txt", FileMode.Create, FileAccess.Write);
            StreamWriter writer = new StreamWriter(f);

            API.DumpSphereTree(writer);
            API.SphereTreeRoot.VerifySphereTree();
            
            // Now a moving object capsule in the center of the square
            CollisionCapsule moCap = new CollisionCapsule(new Vector3(5f,5f,0f),
                                                          new Vector3(5f,5f,4f),
                                                          1f);

            // Remember where the moving object started
            Vector3 start = moCap.center;

            // Move the moving object toward each of the obstacles
            foreach (CollisionShape s in shapes) {
                moCap.AddDisplacement(start - moCap.center);
                MoveToObject(writer, API, s, moCap);
            }

            writer.Close();
        }

        private void MoveToObject(StreamWriter stream,
                                  CollisionAPI API, CollisionShape collisionShape,
                                  CollisionShape movingShape)
        {
            stream.Write("\n\n\nEntering MoveToObject\n");
            // Create a MovingObject, and add movingShape to it
            MovingObject mo = new MovingObject();
            API.AddPartToMovingObject(mo, movingShape);

            // Move movingObject 1 foot at a time toward the sphere
            Vector3 toShape = collisionShape.center - movingShape.center;
            stream.Write(string.Format("movingShape {0}\n", movingShape.ToString()));
            stream.Write(string.Format("collisionShape {0}\nDisplacement Vector {1}\n",
                                       collisionShape.ToString(), toShape.ToString()));
            // We'll certainly get there before steps expires
            int steps = (int)Math.Ceiling(toShape.Length);
            // 1 foot step in the right direction
            Vector3 stepVector = toShape.ToNormalized();
            stream.Write(string.Format("Steps {0}, stepVector {1}\n", steps, stepVector.ToString()));
            bool hitIt = false;
            // Loop til we smack into something
            CollisionParms parms = new CollisionParms();
            for (int i=0; i<steps; i++) {
                // Move 1 foot; if returns true, we hit something
                hitIt = (API.TestCollision (mo, stepVector, parms));
                stream.Write(string.Format("i = {0}, hitIt = {1}, movingShape.center = {2}\n",
                                           i, hitIt, movingShape.center.ToString()));
                if (hitIt) {
                    stream.Write(string.Format("collidingPart {0}\nblockingObstacle {1}\n, normPart {2}, normObstacle {3}\n",
                                               parms.part.ToString(), parms.obstacle.ToString(), 
                                               parms.normPart.ToString(), parms.normObstacle.ToString()));
                    return;
                }
                stream.Write("\n");
            }
            Debug.Assert(hitIt, "Didn't hit the obstacle");
        }
        
        private Vector3 RandomPoint(Random rand, float coordRange)
        {
            return new Vector3(coordRange * (float)rand.NextDouble(),
                               coordRange * (float)rand.NextDouble(),
                               coordRange * (float)rand.NextDouble());
        }
        
        private float RandomFloat(Random rand, float range)
        {
            return range * (float)rand.NextDouble();
        }
        
        // All vector coords are positive
        private Vector3 RandomRelativeVector(Random rand, Vector3 p, float range)
        {
            Vector3 q = Vector3.Zero;
            for (int i=0; i<3; i++) {
                q[i] = p[i] + RandomFloat(rand, range);
            }
            return q;
        }
        
        
        private float RandomAngle(Random rand)
        {
            return RandomFloat(rand, (float)Math.PI);
        }
        
        
        private Vector3[] RandomAxes(Random rand)
        {
            Matrix3 m = Matrix3.Identity;
            m.FromEulerAnglesXYZ(RandomAngle(rand), RandomAngle(rand), RandomAngle(rand));
            return new Vector3[3] { m.GetColumn(0), m.GetColumn(1), m.GetColumn(2)};
        }
        
        private CollisionShape RandomObject(Random rand, float coordRange, float objectSizeRange)
        {
            Vector3 p = Vector3.Zero;
            switch((int)Math.Ceiling(4.0f * (float)rand.NextDouble())) {
            default:
            case 0:
                return new CollisionSphere(RandomPoint(rand, coordRange),
                                           RandomFloat(rand, objectSizeRange));
            case 1:
                p = RandomPoint(rand, coordRange);
                return new CollisionCapsule(p,
                                            RandomRelativeVector(rand, p, objectSizeRange),
                                            RandomFloat(rand, objectSizeRange));
            case 2:
                p = RandomPoint(rand, coordRange);
                return new CollisionAABB(p,
                                         RandomRelativeVector(rand, p, objectSizeRange));
            case 3:             
                p = RandomPoint(rand, coordRange);
                return new CollisionOBB(p,
                                        RandomAxes(rand),
                                        RandomPoint(rand, objectSizeRange));

            }
            
        }
        
        private void GenerateRandomObjects(StreamWriter stream, CollisionAPI API, 
                                           Random rand, int count, int handleStart, 
                                           float coordRange, float objectSizeRange)
        {
            // Set the seed to make the sequence deterministic
            for (int i=0; i<count; i++) {
                CollisionShape shape = RandomObject(rand, coordRange, objectSizeRange);
                //stream.Write(string.Format("\nAdding (i={0}) shape {1}", i, shape.ToString()));
                //stream.Flush();
                API.AddCollisionShape(shape, (i + handleStart));
                //API.DumpSphereTree(stream);
                //stream.Flush();
                API.SphereTreeRoot.VerifySphereTree();
            }
        }
        
        private void RandomSpheresButton_Click(object sender, EventArgs e)
        {
            const int iterations = 20;
            const int objects = 10;
            const float coordRange = 1000.0f;
            const float objectSizeRange = 20.0f;

            CollisionAPI API = new CollisionAPI();
            Random rand = new Random((int)3141526);
            FileStream f = new FileStream("C:\\Junk\\RandomSphereTree.txt", FileMode.Create, FileAccess.Write);
            StreamWriter stream = new StreamWriter(f);

            // Create and delete many randomly-placed collision
            // objects, periodically verifying the sphere tree
            for (int i=0; i<iterations; i++) {
                stream.Write("//////////////////////////////////////////////////////////////////////\n\n");
                stream.Write(string.Format("Creation Iteration {0}, adding {1} objects, object size range {2}, coordinate range {3}\n\n",
                                           i, objects, objectSizeRange, coordRange));
                GenerateRandomObjects(stream, API, rand, objects, 0, coordRange, objectSizeRange);
                stream.Write(string.Format("\n\nAfter iteration {0}:\n\n", i));
                API.DumpSphereTree(stream);
                stream.Flush();
                API.SphereTreeRoot.VerifySphereTree();
            }
            for (int i=0; i<objects; i++) {
                stream.Write("\n\n//////////////////////////////////////////////////////////////////////\n\n");
                stream.Write(string.Format("Deleting shapes with handle {0}\n\n", i));
                API.RemoveCollisionShapesWithHandle(i);
                API.DumpSphereTree(stream);
                stream.Flush();
                API.SphereTreeRoot.VerifySphereTree();
            }
            stream.Close();
        }
    }
}
