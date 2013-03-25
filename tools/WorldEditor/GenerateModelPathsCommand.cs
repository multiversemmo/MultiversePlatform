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
using Multiverse.CollisionLib;

namespace Multiverse.Tools.WorldEditor
{
    public class GenerateModelPathsCommandFactory : ICommandFactory
    {
        IWorldObject modelObj;

        public GenerateModelPathsCommandFactory(IWorldObject modelObj)
        {
            this.modelObj = modelObj;
        }

        #region ICommandFactory Members

        public ICommand CreateCommand()
        {
            ICommand cmd = new GenerateModelPathsCommand(modelObj);

            return cmd;
        }

        #endregion
    }

    public class GenerateModelPathsCommand : ICommand
    {
        StaticObject modelObj;

        public GenerateModelPathsCommand(IWorldObject modelObj)
        {
            this.modelObj = (StaticObject)modelObj;
        }

        #region ICommand Members

        public bool Undoable()
        {
            return false;
        }

        public void Execute()
        {
            float modelHeight = 1.8f;
            float modelWidth = .5f;
            float maxClimbSlope = 0.3f;
            float gridResolution = .25f;
            float maxDisjointDistance = .1f;  //.1f * oneMeter;
            int minimumFeatureSize = 3;    // 3 grid cells
            float terrainLevel = WorldEditor.Instance.GetTerrainHeight(modelObj.Position.x, modelObj.Position.z) - modelObj.Position.y;
            PathObjectType poType = new PathObjectType(modelObj.Name, modelHeight, modelWidth, maxClimbSlope,
                                                       gridResolution, maxDisjointDistance, minimumFeatureSize);
            List<CollisionShape> shapes = WorldEditor.Instance.FindMeshCollisionShapes(modelObj.MeshName, modelObj.DisplayObject.Entity);
            PathGenerator pathGenerator = new PathGenerator(WorldEditor.Instance.LogPathGeneration, modelObj.Name, poType, terrainLevel,
                                                            modelObj.DisplayObject.SceneNode.FullTransform, shapes);
            // Perform traversal and creation of rectangles, arcs
            // between rectangles, and portals to the terrain
            pathGenerator.GeneratePolygonsArcsAndPortals();
      }

        public void UnExecute()
        {
        }

        #endregion
    }

}
