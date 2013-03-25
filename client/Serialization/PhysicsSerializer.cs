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
using System.Text;
using System.Xml;
using System.IO;
using System.Diagnostics;

using Axiom.MathLib;

using Multiverse.Serialization.Collada;
using Multiverse.CollisionLib;

namespace Multiverse.Serialization {
    public class PhysicsData {
        Dictionary<string, List<CollisionShape>> collisionShapes =
            new Dictionary<string, List<CollisionShape>>();
        public void AddCollisionShape(string submeshName, CollisionShape shape) {
            if (!collisionShapes.ContainsKey(submeshName))
                collisionShapes.Add(submeshName, new List<CollisionShape>());
            collisionShapes[submeshName].Add(shape);
        }
        public List<CollisionShape> GetCollisionShapes(string submeshName) {
            if (!collisionShapes.ContainsKey(submeshName))
                return new List<CollisionShape>();
            return collisionShapes[submeshName];
        }
        public List<string> GetCollisionObjects() {
            return new List<string>(collisionShapes.Keys);
        }
        // Region volume machinery needs to iterate over the whole
        // dictionary
        public Dictionary<string, List<CollisionShape>> CollisionShapes 
        {
            get { return collisionShapes;}
        }
    }

    public class PhysicsSerializer {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(PhysicsSerializer));

        // If two scale vectors are this close, consider them to be the same
        public const float ScaleEpsilon = 0.0001f;
        // If the angle between two vectors is this small, consider it zero
        public const float RotateEpsilon = 0.0001f;

        public float unitConversion = ColladaMeshInfo.UnitsPerMeter;

        public void DebugMessage(XmlNode node) {
            if (node.NodeType == XmlNodeType.Comment)
                return;
            log.InfoFormat("Unhandled node type: {0} with parent of {1}", node.Name, node.ParentNode.Name);
        }

        public void ImportPhysics(PhysicsData physicsData, string filename) {
            Stream inStream = new FileStream(filename, FileMode.Open);
            try {
                ImportPhysics(physicsData, inStream);
            } finally {
                inStream.Close();
            }
        }

        public void ImportPhysics(PhysicsData physicsData, Stream stream) {
            XmlDocument document = new XmlDocument();
            document.Load(stream);
            foreach (XmlNode childNode in document.ChildNodes) {
                switch (childNode.Name) {
                    case "physics_model":
                        ReadPhysicsModel(childNode, physicsData);
                        break;
                    default:
                        DebugMessage(childNode);
                        break;
                }
            }
        }

        public void ExportPhysics(PhysicsData physicsData, string filename) {
            Stream outStream = new FileStream(filename, FileMode.Create);
            try {
                ExportPhysics(physicsData, outStream);
            } finally {
                outStream.Close();
            }
        }

        public void ExportPhysics(PhysicsData physicsData, Stream stream) {
            XmlDocument document = new XmlDocument();
            XmlNode node = Write(document, physicsData);
            document.AppendChild(node);
            document.Save(stream);
        }


        public void ReadRotate(ref float angle, ref Vector3 axis, XmlNode node) {
            string[] values = node.InnerText.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
            Debug.Assert(values.Length == 4);
            axis = Vector3.Zero;
            for (int i = 0; i < 3; ++i)
                axis[i] = float.Parse(values[i]);
            angle = MathUtil.DegreesToRadians(float.Parse(values[3]));
        }

        public void ReadVector(ref Vector3 vec, XmlNode node) {
            string[] values = node.InnerText.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
            Debug.Assert(values.Length == 3);
            for (int i = 0; i < 3; ++i)
                vec[i] = float.Parse(values[i]);
        }

        public XmlNode WriteRotate(Quaternion rot, XmlDocument document) {
            XmlNode node = document.CreateElement("rotate");
            float angle = 0;
            Vector3 axis = Vector3.Zero;
            rot.ToAngleAxis(ref angle, ref axis);
            float degrees = MathUtil.RadiansToDegrees(angle);
            node.InnerText = string.Format("{0} {1} {2} {3}", axis.x, axis.y, axis.z, degrees);
            return node;
        }

        public XmlNode WriteTranslate(Vector3 translate, XmlDocument document) {
            XmlNode node = document.CreateElement("translate");
            node.InnerText = string.Format("{0} {1} {2}", translate.x, translate.y, translate.z);
            return node;
        }


        #region Read methods

        public void ReadPhysicsModel(XmlNode node, PhysicsData physicsData) {
            foreach (XmlNode childNode in node.ChildNodes) {
                switch (childNode.Name) {
                    case "rigid_body":
                        ReadRigidBody(childNode, physicsData);
                        break;
                    default:
                        DebugMessage(childNode);
                        break;
                }
            }
        }

        public void ReadRigidBody(XmlNode node, PhysicsData physicsData) {
            string objectId = node.Attributes["sid"].Value;
            foreach (XmlNode childNode in node.ChildNodes) {
                switch (childNode.Name) {
                    case "technique_common":
                        ReadRigidBody_TechniqueCommon(objectId, childNode, physicsData);
                        break;
                    default:
                        DebugMessage(childNode);
                        break;
                }
            }
        }

        public void ReadRigidBody_TechniqueCommon(string objectId, XmlNode node, PhysicsData physicsData) {
            foreach (XmlNode childNode in node.ChildNodes) {
                switch (childNode.Name) {
                    case "shape":
                        ReadShape(objectId, childNode, physicsData);
                        break;
                    default:
                        DebugMessage(childNode);
                        break;
                }
            }
        }

        public void ReadShape(string objectId, XmlNode node, PhysicsData physicsData) {
            List<NamedTransform> transformChain = new List<NamedTransform>();
            foreach (XmlNode childNode in node.ChildNodes) {
                switch (childNode.Name) {
                    case "rotate": {
                            float angle = 0;
                            Vector3 axis = Vector3.UnitY;
                            ReadRotate(ref angle, ref axis, childNode);
                            transformChain.Add(new NamedRotateTransform(null, angle, axis));
                        }
                        break;
                    case "scale": {
                            Vector3 scale = Vector3.UnitScale;
                            ReadVector(ref scale, childNode);
                            transformChain.Add(new NamedScaleTransform(null, scale));
                        }
                        break;
                    case "translate": {
                            Vector3 translation = Vector3.Zero;
                            ReadVector(ref translation, childNode);
                            transformChain.Add(new NamedTranslateTransform(null, translation));
                        }
                        break;
                    case "box": {
                            Matrix4 transform = Matrix4.Identity;
                            foreach (NamedTransform t in transformChain) {
                                transform = t.Transform * transform;
                                Debug.Assert(transform != Matrix4.Zero);
                            }
                            ReadBox(objectId, transform, childNode, physicsData);
                        }
                        break;
                    case "sphere": {
                            Matrix4 transform = Matrix4.Identity;
                            foreach (NamedTransform t in transformChain) {
                                transform = t.Transform * transform;
                                Debug.Assert(transform != Matrix4.Zero);
                            }
                            ReadSphere(objectId, transform, childNode, physicsData);
                        }
                        break;
                    case "capsule": {
                            Matrix4 transform = Matrix4.Identity;
                            foreach (NamedTransform t in transformChain) {
                                transform = t.Transform * transform;
                                Debug.Assert(transform != Matrix4.Zero);
                            }
                            ReadCapsule(objectId, transform, childNode, physicsData);
                        }
                        break;
                    default:
                        DebugMessage(childNode);
                        break;
                }
            }
        }

        public void ReadBox(string objectId, Matrix4 transform, XmlNode node, PhysicsData physicsData) {
            Quaternion rot = MathHelpers.GetRotation(transform);
            Matrix4 tmpTransform = rot.Inverse().ToRotationMatrix() * transform;
            Vector3 scaleDelta = tmpTransform.Scale - Vector3.UnitScale;
            if (scaleDelta.Length > PhysicsSerializer.ScaleEpsilon)
                log.Error("Scale is not currently supported for box shapes");
            Vector3 halfExtents = Vector3.Zero;
            foreach (XmlNode childNode in node.ChildNodes) {
                switch (childNode.Name) {
                    case "half_extents":
                        ReadVector(ref halfExtents, childNode);
                        break;
                    default:
                        DebugMessage(childNode);
                        break;
                }
            }
            Vector3 center = unitConversion * transform.Translation;
            halfExtents *= unitConversion;

            CollisionShape collisionShape;
            float angle = 0;
            Vector3 axis = Vector3.Zero;
            rot.ToAngleAxis(ref angle, ref axis);
            // I could build AABB shapes for boxes that are aligned, but then 
            // I can't drop instances in the world with arbitrary rotations.
            // Instead, just always use an OBB.
            //if (angle < PhysicsSerializer.RotateEpsilon) {
            //    collisionShape = new CollisionAABB(center - halfExtents, center + halfExtents);
            //} else {
            // TODO: I'm not sure I understand the OBB constructor
            Vector3[] axes = new Vector3[3];
            axes[0] = rot.XAxis;
            axes[1] = rot.YAxis;
            axes[2] = rot.ZAxis;
            collisionShape = new CollisionOBB(center, axes, halfExtents);
            physicsData.AddCollisionShape(objectId, collisionShape);
        }

        public void ReadSphere(string objectId, Matrix4 transform, XmlNode node, PhysicsData physicsData) {
            Vector3 scaleDelta = transform.Scale - Vector3.UnitScale;
            if (scaleDelta.Length > PhysicsSerializer.ScaleEpsilon)
                log.Error("Scale is not currently supported for sphere shapes");
            float radius = 0;
            foreach (XmlNode childNode in node.ChildNodes) {
                switch (childNode.Name) {
                    case "radius":
                        radius = float.Parse(childNode.InnerText);
                        break;
                    default:
                        DebugMessage(childNode);
                        break;
                }
            }
            Vector3 center = unitConversion * transform.Translation;
            radius *= unitConversion;

            CollisionShape collisionShape = new CollisionSphere(center, radius);
            physicsData.AddCollisionShape(objectId, collisionShape);
        }

        public void ReadCapsule(string objectId, Matrix4 transform, XmlNode node, PhysicsData physicsData) {
            Quaternion rot = MathHelpers.GetRotation(transform);
            Matrix4 tmpTransform = rot.Inverse().ToRotationMatrix() * transform;
            Vector3 scaleDelta = tmpTransform.Scale - Vector3.UnitScale;
            if (scaleDelta.Length > PhysicsSerializer.ScaleEpsilon)
                log.Error("Scale is not currently supported for capsule shapes");
            float height = 0;
            float[] radius = new float[2];
            Vector3 halfExtents = Vector3.Zero;
            foreach (XmlNode childNode in node.ChildNodes) {
                switch (childNode.Name) {
                    case "height":
                        height = float.Parse(childNode.InnerText);
                        break;
                    case "radius": {
                            string[] values = childNode.InnerText.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                            Debug.Assert(values.Length == 2);
                            for (int i = 0; i < 2; ++i)
                                radius[i] = float.Parse(values[i]);
                        }
                        break;
                    default:
                        DebugMessage(childNode);
                        break;
                }
            }
            // We only support capsules where the two radii match
            if (radius[0] != radius[1])
                log.Error("Different radii for capsules are not currently supported");
            Vector3 center = unitConversion * transform.Translation;
            Vector3 halfExtent = unitConversion * (.5f * height * (rot * Vector3.UnitY));
            radius[0] *= unitConversion;
            radius[1] *= unitConversion;

            CollisionShape collisionShape;
            collisionShape = new CollisionCapsule(center - halfExtent, center + halfExtent, radius[0]);
            physicsData.AddCollisionShape(objectId, collisionShape);
        }

        #endregion Read methods

        #region Write methods

        public XmlNode Write(XmlDocument document, PhysicsData physicsData) {
            XmlNode node = document.CreateElement("physics_model");
            foreach (string objectId in physicsData.GetCollisionObjects()) {
                XmlNode childNode = WriteRigidBody(objectId, document, physicsData);
                node.AppendChild(childNode);
            }
            return node;
        }

        public XmlNode WriteRigidBody(string objectId, XmlDocument document, PhysicsData physicsData) {
            XmlNode node = document.CreateElement("rigid_body");
            XmlAttribute attr = document.CreateAttribute("sid");
            attr.Value = objectId;
            node.Attributes.Append(attr);

            XmlNode childNode = WriteRigidBody_TechniqueCommon(objectId, document, physicsData);
            node.AppendChild(childNode);
            
            return node;
        }

        public XmlNode WriteRigidBody_TechniqueCommon(string objectId, XmlDocument document, PhysicsData physicsData) {
            XmlNode node = document.CreateElement("technique_common");

            List<CollisionShape> collisionShapes = physicsData.GetCollisionShapes(objectId);
            foreach (CollisionShape shape in collisionShapes) {
                XmlNode childNode = WriteShape(shape, document);
                node.AppendChild(childNode);
            }

            return node;
        }

        public XmlNode WriteShape(CollisionShape shape, XmlDocument document) {
            XmlNode node = document.CreateElement("shape");


            if (shape is CollisionAABB) {
                CollisionAABB aabbShape = shape as CollisionAABB;
                
                // convert to meters (or other scaled unit) before writing out
                Vector3 center = shape.center / unitConversion;
                Vector3 halfExtent = (aabbShape.max - aabbShape.center) / unitConversion;

                XmlNode translate = WriteTranslate(center, document);
                node.AppendChild(translate);
                XmlNode boxNode = WriteBox(halfExtent, document);
                node.AppendChild(boxNode);
            } else if (shape is CollisionOBB) {
                CollisionOBB obbShape = shape as CollisionOBB;
                
                // convert to meters (or other scaled unit) before writing out
                Vector3 center = shape.center / unitConversion;
                Vector3 halfExtent = obbShape.extents / unitConversion;
                
                Quaternion rot = Quaternion.Identity;
                rot.FromAxes(obbShape.axes[0], obbShape.axes[1], obbShape.axes[2]);
                XmlNode rotate = WriteRotate(rot, document);
                node.AppendChild(rotate);
                XmlNode translate = WriteTranslate(center, document);
                node.AppendChild(translate);

                XmlNode boxNode = WriteBox(halfExtent, document);
                node.AppendChild(boxNode);
            } else if (shape is CollisionCapsule) {
                CollisionCapsule capsuleShape = shape as CollisionCapsule;

                // convert to meters (or other scaled unit) before writing out
                Vector3 center = shape.center / unitConversion;
                float height = capsuleShape.height / unitConversion;
                float radius = capsuleShape.capRadius / unitConversion;

                Vector3 halfExtent = capsuleShape.topcenter - capsuleShape.center;
                Quaternion rot = Vector3.UnitY.GetRotationTo(halfExtent);
                XmlNode rotate = WriteRotate(rot, document);
                node.AppendChild(rotate);
                XmlNode translate = WriteTranslate(center, document);
                node.AppendChild(translate);
                XmlNode capsuleNode = WriteCapsule(height, radius, document);
                node.AppendChild(capsuleNode);
            } else if (shape is CollisionSphere) {
                // convert to meters (or other scaled unit) before writing out
                Vector3 center = shape.center / unitConversion;
                float radius = shape.radius / unitConversion;

                XmlNode translate = WriteTranslate(center, document);
                node.AppendChild(translate);
                XmlNode sphereNode = WriteSphere(radius, document);
                node.AppendChild(sphereNode);
            } else {
                log.ErrorFormat("Unsupported collision shape: {0}", shape.GetType());
            }
            return node;
        }

        public XmlNode WriteBox(Vector3 halfExtent, XmlDocument document) {
            XmlNode node = document.CreateElement("box");
            XmlNode halfExtents = document.CreateElement("half_extents");

            halfExtents.InnerText = string.Format("{0} {1} {2}", halfExtent.x, halfExtent.y, halfExtent.z);
            node.AppendChild(halfExtents);
            return node;
        }

        public XmlNode WriteCapsule(float height, float radius, XmlDocument document) {
            XmlNode node = document.CreateElement("capsule");
            XmlNode heightNode = document.CreateElement("height");

            heightNode.InnerText = height.ToString();
            node.AppendChild(heightNode);
            XmlNode radiusNode = document.CreateElement("radius");
            radiusNode.InnerText = string.Format("{0} {1}", radius, radius);
            node.AppendChild(radiusNode);
            return node;
        }

        public XmlNode WriteSphere(float radius, XmlDocument document) {
            XmlNode node = document.CreateElement("sphere");
            XmlNode radiusNode = document.CreateElement("radius");

            radiusNode.InnerText = radius.ToString();
            node.AppendChild(radiusNode);
            return node;
        }

        #endregion Write methods
    }
}
