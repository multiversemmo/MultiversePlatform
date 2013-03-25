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
using System.Diagnostics;
using Axiom.MathLib;
using Axiom.Graphics;
using Axiom.Animating;

namespace Axiom.SceneManagers.Multiverse
{

    public class DecalElement : IComparable<DecalElement>, IAnimableObject
    {
        protected List<string> imageNames;
        protected float timePerFrame = 0;

        protected float posX;
        protected float posZ;
        protected float sizeX;
        protected float sizeZ;
        protected float rot;
        protected float lifetime;
        protected float deleteRadius;

        protected bool dirty = true;
        protected float lastUpdateTime;
        protected float timeAlive;
        protected bool firstUpdate = true;

        protected float maxX;
        protected float maxZ;

        protected PageCoord minPC;
        protected PageCoord maxPC;

        protected int priority;
        protected TerrainDecalManager manager;

        private DecalElement(float timePerFrame,
            float posX, float posZ, float sizeX, float sizeZ, float rot,
            float lifetime, float deleteRadius, int priority, TerrainDecalManager manager)
        {
            this.timePerFrame = timePerFrame;
            this.posX = posX;
            this.posZ = posZ;
            this.sizeX = sizeX;
            this.sizeZ = sizeZ;
            this.rot = rot;
            this.lifetime = lifetime;
            this.deleteRadius = deleteRadius;
            this.priority = priority;
            this.manager = manager;

            lastUpdateTime = TerrainManager.Instance.Time;
        }

        public DecalElement(string imageName, float posX, float posZ, float sizeX, float sizeZ, float rot,
            float lifetime, float deleteRadius, int priority, TerrainDecalManager manager)
            : this(0, posX, posZ, sizeX, sizeZ, rot, lifetime, deleteRadius, priority, manager)
        {
            imageNames = new List<string>();
            imageNames.Add(imageName);
        }

        public DecalElement(List<string> imageNames, float timePerFrame,
            float posX, float posZ, float sizeX, float sizeZ, float rot, 
            float lifetime, float deleteRadius, int priority, TerrainDecalManager manager)
            : this(timePerFrame, posX, posZ, sizeX, sizeZ, rot, lifetime, deleteRadius, priority, manager)
        {
            this.imageNames = imageNames;
        }

        public bool Update(float cameraX, float cameraZ)
        {
            float time = TerrainManager.Instance.Time;
            bool deleteMe = false;

            if (firstUpdate)
            {
                timeAlive = 0;
                firstUpdate = false;
                dirty = true;
            }

            if (deleteRadius != 0)
            {
                // compute distance from camera
                float dx = posX - cameraX;
                float dz = posZ - cameraZ;
                float dxsq = dx * dx;
                float dzsq = dz * dz;

                if ((dxsq + dzsq) > (deleteRadius * deleteRadius))
                {
                    // if distance from the camera is greater than deleteRadius, then delete this element
                    deleteMe = true;
                }
            }

            float deltaT = time - lastUpdateTime;
            timeAlive += deltaT;

            if ((lifetime != 0) && (timeAlive > lifetime))
            {
                // time has run out, so delete this element
                deleteMe = true;
            }

            lastUpdateTime = time;

            if (dirty)
            {
                float halfx = sizeX / 2f;
                float halfz = sizeZ / 2f;

                // compute area possibly touched by the decal
                if (rot != 0)
                {
                    float theta = MathUtil.DegreesToRadians(rot);
                    float cos = MathLib.MathUtil.Cos(theta);
                    float sin = MathLib.MathUtil.Sin(theta);

                    float x1 = halfx * cos - halfz * sin;
                    float z1 = halfx * sin + halfz * cos;

                    float x2 = -halfx * cos - halfz * sin;
                    float z2 = -halfx * sin + halfz * cos;

                    float x3 = halfx * cos + halfz * sin;
                    float z3 = halfx * sin - halfz * cos;

                    float x4 = -halfx * cos + halfz * sin;
                    float z4 = -halfx * sin - halfz * cos;

                    maxX = Math.Max(Math.Max(x1, x2), Math.Max(x3, x4));
                    maxZ = Math.Max(Math.Max(z1, z2), Math.Max(z3, z4));
                }
                else
                {
                    maxX = halfx;
                    maxZ = halfz;
                }

                float worldX1 = posX - maxX;
                float worldX2 = posX + maxX;
                float worldZ1 = posZ - maxZ;
                float worldZ2 = posZ + maxZ;

                minPC = new PageCoord(new Vector3(worldX1, 0, worldZ1), TerrainManager.Instance.PageSize);
                maxPC = new PageCoord(new Vector3(worldX2, 0, worldZ2), TerrainManager.Instance.PageSize);


            }

            return deleteMe;
        }

        public void UpdateTextureTransform(TextureUnitState texUnit, float pageX, float pageZ)
        {
            //Axiom.Core.LogManager.Instance.Write("Decal: {0}, {1} page:{2}, {3}", posX, posZ, pageX, pageZ);
            float pageSize = TerrainManager.Instance.PageSize * TerrainManager.oneMeter;
            float scaleX = sizeX/pageSize;
            float scaleZ = sizeZ/pageSize;
            texUnit.SetTextureScale(scaleX, scaleZ);
            float centerX = pageX + pageSize / 2f;
            float centerZ = pageZ + pageSize / 2f;
            texUnit.SetTextureScroll((centerX - posX)/(pageSize*scaleX), (centerZ - posZ)/(pageSize*scaleZ));
            texUnit.SetTextureRotate(rot);
        }

        public string ImageName
        {
            get
            {
                return imageNames[0];
            }
        }

        public float PosX
        {
            get
            {
                return posX;
            }
            set
            {
                posX = value;
                dirty = true;
            }
        }

        public float PosZ
        {
            get
            {
                return posZ;
            }
            set
            {
                posZ = value;
                dirty = true;
            }
        }

        public float SizeX
        {
            get
            {
                return sizeX;
            }
            set
            {
                sizeX = value;
                Axiom.Core.LogManager.Instance.Write("setting SizeX: {0}", sizeX);
                dirty = true;
            }
        }

        public float SizeZ
        {
            get
            {
                return sizeZ;
            }
            set
            {
                sizeZ = value;
                dirty = true;
            }
        }

        public float Rot
        {
            get
            {
                return rot;
            }
            set
            {
                rot = value;
                dirty = true;
            }
        }

        public PageCoord MinPageCoord
        {
            get
            {
                return minPC;
            }
        }

        public PageCoord MaxPageCoord
        {
            get
            {
                return maxPC;
            }
        }

        public int Priority
        {
            get
            {
                return priority;
            }
            set
            {
                priority = value;
                manager.NeedsSorting();
            }
        }

        #region IComparable<DecalElement> Members

        public int CompareTo(DecalElement other)
        {
            return priority - other.priority;
        }

        #endregion

        #region IAnimableObject Members

        public static string[] animableAttributes = {
            "Rot",
            "SizeX",
            "SizeZ",
            "PosX",
            "PosZ"
		};

        public AnimableValue CreateAnimableValue(string valueName)
        {
            switch (valueName)
            {
                case "Rot":
                    return new DecalRotationValue(this);
                case "SizeX":
                    return new DecalSizeXValue(this);
                case "SizeZ":
                    return new DecalSizeZValue(this);
                case "PosX":
                    return new DecalPosXValue(this);
                case "PosZ":
                    return new DecalPosZValue(this);
            }
            throw new Exception(string.Format("Could not find animable attribute '{0}'", valueName));
        }

        public string[] AnimableValueNames
        {
            get
            {
                return animableAttributes;
            }
        }

        protected class DecalRotationValue : AnimableValue
        {
            protected DecalElement decal;
            public DecalRotationValue(DecalElement decal)
                : base(AnimableType.Float)
            {
                this.decal = decal;
                SetAsBaseValue(0.0f);
            }

            public override void SetValue(float val)
            {
                decal.Rot = val;
            }

            public override void ApplyDeltaValue(float val)
            {
                SetValue(decal.Rot + val);
            }

            public override void SetCurrentStateAsBaseValue()
            {
                SetAsBaseValue(decal.Rot);
            }
        }

        protected class DecalSizeXValue : AnimableValue
        {
            protected DecalElement decal;
            public DecalSizeXValue(DecalElement decal)
                : base(AnimableType.Float)
            {
                this.decal = decal;
                SetAsBaseValue(0.0f);
            }

            public override void SetValue(float val)
            {
                decal.SizeX = val;
            }

            public override void ApplyDeltaValue(float val)
            {
                SetValue(decal.SizeX + val);
            }

            public override void SetCurrentStateAsBaseValue()
            {
                SetAsBaseValue(decal.SizeX);
            }
        }

        protected class DecalSizeZValue : AnimableValue
        {
            protected DecalElement decal;
            public DecalSizeZValue(DecalElement decal)
                : base(AnimableType.Float)
            {
                this.decal = decal;
                SetAsBaseValue(0.0f);
            }

            public override void SetValue(float val)
            {
                decal.SizeZ = val;
            }

            public override void ApplyDeltaValue(float val)
            {
                SetValue(decal.SizeZ + val);
            }

            public override void SetCurrentStateAsBaseValue()
            {
                SetAsBaseValue(decal.SizeZ);
            }
        }

        protected class DecalPosXValue : AnimableValue
        {
            protected DecalElement decal;
            public DecalPosXValue(DecalElement decal)
                : base(AnimableType.Float)
            {
                this.decal = decal;
                SetAsBaseValue(0.0f);
            }

            public override void SetValue(float val)
            {
                decal.PosX = val;
            }

            public override void ApplyDeltaValue(float val)
            {
                SetValue(decal.PosX + val);
            }

            public override void SetCurrentStateAsBaseValue()
            {
                SetAsBaseValue(decal.PosX);
            }
        }

        protected class DecalPosZValue : AnimableValue
        {
            protected DecalElement decal;
            public DecalPosZValue(DecalElement decal)
                : base(AnimableType.Float)
            {
                this.decal = decal;
                SetAsBaseValue(0.0f);
            }

            public override void SetValue(float val)
            {
                decal.PosZ = val;
            }

            public override void ApplyDeltaValue(float val)
            {
                SetValue(decal.PosZ + val);
            }

            public override void SetCurrentStateAsBaseValue()
            {
                SetAsBaseValue(decal.PosZ);
            }
        }

        #endregion
    }
}
