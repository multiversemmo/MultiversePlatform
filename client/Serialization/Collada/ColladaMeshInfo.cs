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

// It looks like sometimes the local transform that is supposed to counter the
// position of a bone (when an object is rigidly bound to a bone that has a 
// transform chain), the bones will be in a different pose, but the counter
// transform will not be updated.  To still work correctly in this case, 
// we need to define COUNTER_BIND, which will set the transform to undo the
// effect of the parent hierarchy.  This was noted with the 2.08 release of
// the 3ds Max exporter.

// #define COUNTER_BIND

using System;
using System.Collections.Generic;
using System.Diagnostics;

using Axiom.Animating;
using Axiom.Core;
using Axiom.MathLib;
using Axiom.Graphics;

using Multiverse.Serialization.Collada;

namespace Multiverse.Serialization
{
    public class ColladaMeshInfo
    {
        #region Member variables

        private static readonly log4net.ILog m_Log = log4net.LogManager.GetLogger( typeof( ColladaMeshInfo ) );


        public Mesh AxiomMesh
        {
            get { return m_AxiomMesh; }
            set { m_AxiomMesh = value; }
        }
        Mesh m_AxiomMesh;

        /// <summary>
        /// These are the units in the source Collada file.
        /// </summary>
        public float SourceUnits
        {
            get { return m_SourceUnits; }
            set 
            { 
                m_SourceUnits = value;

                if( ! SourceUnitsAreBroken )
                {
                    UnitConversion = value * UnitsPerMeter;
                }
            }
        }
        private float m_SourceUnits;
        public string UpAxis
        {
            set {
                if (value == "Y_UP")
                    UpAxisConversion = Matrix4.Identity;
                else if (value == "Z_UP")
                    UpAxisConversion = new Matrix4(1, 0, 0, 0, 0, 0, 1, 0, 0, -1, 0, 0, 0, 0, 0, 1);
                else if (value == "X_UP")
                    UpAxisConversion = new Matrix4(0, -1, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1);
                else
                    throw new Exception("Unsupported up_axis value: " + value);
            }
        }

        /// <summary>
        ///   We use millimeters as our in game unit for assets.  
        ///   Since collada will be using meters as the default unit,
        ///   specify the conversion factor here.
        /// </summary>
        public static float UnitsPerMeter = 1000f;


        /// <summary>
        ///     Some versions of the collada exporter break the units.
        ///     Keep track of that here.
        /// </summary>
        public bool SourceUnitsAreBroken
        {
            get { return m_SourceUnitsAreBroken; }
            set
            {
                m_SourceUnitsAreBroken = value;

                if( m_SourceUnitsAreBroken )
                {
                    m_UnitConversion = .01f * UnitsPerMeter;
                }
            }
        }
        private bool m_SourceUnitsAreBroken = false;


        // List of attachment points
        public List<AttachmentPointNode> AttachmentPoints
        {
            get { return m_AttachmentPoints; }
            set { m_AttachmentPoints = value; }
        }
        private List<AttachmentPointNode> m_AttachmentPoints;

        /// <summary>
        ///   Mapping from geometry library to the list of submeshes 
        ///   that are in that library
        /// </summary>
        public Dictionary<string, GeometrySet> Geometries
        {
            get { return m_Geometries; }
            set { m_Geometries = value; }
        }
        private Dictionary<string, GeometrySet> m_Geometries;


        // List of geometry instances
        public List<GeometryInstance> GeoInstances
        {
            get { return m_GeoInstances; }
            set { m_GeoInstances = value; }
        }
        private List<GeometryInstance> m_GeoInstances;


        // SkinControllers
        public Dictionary<string, Controller> Controllers
        {
            get { return m_Controllers; }
            set { m_Controllers = value; }
        }
        private Dictionary<string, Controller> m_Controllers;

        /// <summary>
        ///   This is the non-accumulated transform for a joint.  The list of 
        ///   transforms relative to the parent bone.  The key is the node id.
        /// </summary>
        public Dictionary<string, List<NamedTransform>> JointTransformChains
        {
            get { return m_JointTransformChains; }
            set { m_JointTransformChains = value; }
        }
        private Dictionary<string, List<NamedTransform>> m_JointTransformChains;


        /// <summary>
        ///   Maintain a mapping from sid to id
        /// </summary>
        public Dictionary<string, string> NodeSidMap
        {
            get { return m_NodeSidMap; }
            set { m_NodeSidMap = value; }
        }
        private Dictionary<string, string> m_NodeSidMap;


        /// <summary>
        ///   Dictionary mapping from bone name (from node id) to the name of
        ///   the bone's parent
        /// </summary>
        public Dictionary<string, string> BoneParents
        {
            get { return m_BoneParents; }
            set { m_BoneParents = value; }
        }
        private Dictionary<string, string> m_BoneParents;

        // Samplers
        public Dictionary<string, Sampler> Samplers
        {
            get { return m_Samplers; }
            set { m_Samplers = value; }
        }
        private Dictionary<string, Sampler> m_Samplers;

        // Channels
        public List<Channel> Channels
        {
            get { return m_Channels; }
            set { m_Channels = value; }
        }
        private List<Channel> m_Channels;

        // Layers (maya)
        // TODO: This looks like something entirely unused; good candidate for deletion. --jrl
        public Dictionary<string, string> Layers
        {
            get { return m_Layers; }
            set { m_Layers = value; }
        }
        private Dictionary<string, string> m_Layers;


        /// <summary>
        ///   Conversion factor for changing units from the collada
        ///   measurements (default of meters) to our measurement
        /// </summary>
        private float UnitConversion
        {
            get { return m_UnitConversion; }
            set { m_UnitConversion = value; }
        }
        private float m_UnitConversion = UnitsPerMeter;
        /// <summary>
        ///   Conversion matrix to make Y the up axis.
        /// </summary>
        private Matrix4 UpAxisConversion
        {
            get { return m_UpAxisConversion; }
            set { m_UpAxisConversion = value; }
        }
        private Matrix4 m_UpAxisConversion = Matrix4.Identity;

        private List<PoseInfo> PoseInfoList
        {
            get { return m_PoseInfoList; }
            set { m_PoseInfoList = value; }
        }
        private List<PoseInfo> m_PoseInfoList;


        private Dictionary<string, PoseInfo> PoseInfoCollection
        {
            get { return m_PoseInfoCollection; }
        }
        private Dictionary< string, PoseInfo> m_PoseInfoCollection = new Dictionary<string, PoseInfo>();

        PoseInfoCatalog m_PoseInfoCatalog = new PoseInfoCatalog();

        bool m_NoRiggingCulling;

        #endregion

        private HardwareBufferHelper HWBuffer
        {
            get { return m_HardwareBufferHelper; }
            set { m_HardwareBufferHelper = value; }
        }

        internal bool NoRiggingCulling
        {
            get { return m_NoRiggingCulling; }
            set { m_NoRiggingCulling = value; }
        }

        HardwareBufferHelper m_HardwareBufferHelper;

        public ColladaMeshInfo( Mesh axiomMesh )
        {
            m_AxiomMesh = axiomMesh;

            m_HardwareBufferHelper = new HardwareBufferHelper( axiomMesh, m_Log );

            m_Geometries = new Dictionary<string, GeometrySet>();
            m_GeoInstances = new List<GeometryInstance>();
            m_Controllers = new Dictionary<string, Controller>();
            m_JointTransformChains = new Dictionary<string, List<NamedTransform>>();
            m_NodeSidMap = new Dictionary<string, string>();
            m_BoneParents = new Dictionary<string, string>();
            m_Samplers = new Dictionary<string, Sampler>();
            m_Channels = new List<Channel>();
            m_PoseInfoList = new List<PoseInfo>();
            m_Layers = new Dictionary<string, string>();
            m_AttachmentPoints = new List<AttachmentPointNode>();
        }

        /// <summary>
        ///   Explode the matrix into the translate, rotate and scale components.
        /// </summary>
        /// <param name="mat"></param>
        /// <param name="translate"></param>
        /// <param name="rotate"></param>
        /// <param name="scale"></param>
        /// <summary>
        ///   Get the initial pose transform for the given bone.  This has not
        ///   been modified by the world transform matrix
        /// </summary>
        /// <param name="boneName">the name (id) of the bone</param>
        /// <returns>the transform matrix of the specified bone in the initial pose</returns>
        public Matrix4 GetBonePoseTransform( string boneName )
        {
            List<NamedTransform> transformChain = new List<NamedTransform>();
            while( boneName != null )
            {
                transformChain.AddRange( JointTransformChains[ boneName ] );
                if( BoneParents.ContainsKey( boneName ) )
                    boneName = BoneParents[ boneName ];
                else
                    boneName = null;
            }

            return GetBakedTransform( transformChain );
        }

        /// <summary>
        ///   Get the initial pose transform for the given bone relative to 
        ///   its parent bone.  This has not been modified by the world 
        ///   transform matrix
        /// </summary>
        /// <param name="boneName">the name (id) of the bone</param>
        /// <returns>the transform matrix of the specified bone relative to its parent in the initial pose</returns>
        public Matrix4 GetRelativeBonePoseTransform( string boneName )
        {
            return GetBakedTransform( JointTransformChains[ boneName ] );
        }

        public static List<VertexBoneAssignment> GetAssignmentsForVertex( int vertexId, List<VertexBoneAssignment> vbaList )
        {
            List<VertexBoneAssignment> rv = new List<VertexBoneAssignment>();
            foreach( VertexBoneAssignment vba in vbaList )
            {
                if( vba.vertexIndex == vertexId )
                    rv.Add( new VertexBoneAssignment( vba ) );
            }
            return rv;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="transform">the transform to apply to this object</param>
        /// <param name="skeleton">the skeleton to which we will add animations, or null to create one</param>
        /// <param name="baseFile">the prefix to be used for the generated skeleton</param>
        /// <param name="animationName">the name to be used for the animation data</param>
        public void Process( Matrix4 transform, Skeleton skeleton, string baseFile, string animationName )
        {
            m_AxiomMesh.BoundingBox = new AxisAlignedBox();

            if( null == skeleton )
            {
                skeleton = SynthesizeSkeletonFromBones( transform * m_UpAxisConversion, baseFile );
            }
            else
            {
                m_AxiomMesh.NotifySkeleton( skeleton );
            }

            // Put any attachment point data in the skeleton or mesh as neeeded
            ProcessAttachmentPoints( transform * m_UpAxisConversion, skeleton, m_AxiomMesh);

            Matrix4 unitConversionMatrix = Matrix4.Identity;
            unitConversionMatrix.Scale = new Vector3( m_UnitConversion, m_UnitConversion, m_UnitConversion );
            Matrix4 worldTransform = transform * m_UpAxisConversion * unitConversionMatrix;

            foreach( GeometryInstance geoInstance in GeoInstances )
            {
                // TODO: This is stuffing yet another transform matrix into the instance
                // (alongside "transfom" and "localTransform").  There ought to be a way
                // to merge this global transform into one of those (likely "transform"),
                // but first I need to understand all the places that touch the transforms.
                geoInstance.GlobalTransform = worldTransform;

                // Removing empty sets prevents the hardware buffer allocation from breaking.
                // TODO: I'm not sure yet if this can happen after BuildVertexBuffers() and
                // SetUpIntialPose(), but it would be nice if it could happen at allocation time.
                geoInstance.RemoveEmptyGeometrySets();

                // This is actually semantically relevant to instances!  You can have more
                // than one instance of a given source geometry, each with its own material
                // binding.  This resolves the binding.
                geoInstance.ApplyMaterialBindings();
            }

            GeneratePoseList();

            BuildVertexBuffers( worldTransform );

            SetUpInitialPose( skeleton );

            foreach( GeometryInstance geoInstance in GeoInstances )
            {
                foreach( MeshGeometry geometry in geoInstance.geoSet )
                {
                    HWBuffer.AllocateBuffer( geometry.VertexData, 0, geometry.VertexDataEntries );
                    ColladaMeshInfo.BuildBoundingBox( m_AxiomMesh, geometry );
                }
            }

            // Build any poses that we need to support morph/pose style animation
            BuildPoses();

            if( skeleton != null && animationName != null )
            {
                Animation anim = CreateAnimation( animationName );
                if( anim != null )
                {
                    // Build skeletal animations
                    ProcessChannels( worldTransform, anim, skeleton );
                    // Remove redundant linear keyframes
                    AnimationHelper.CleanupAnimation( skeleton, animationName );
                }
            }

            // Handle morph (and perhaps pose) animation
            ProcessVertexAnimationChannels();

            float len = 0;
            foreach( Vector3 corner in m_AxiomMesh.BoundingBox.Corners )
            {
                len = Math.Max( len, corner.LengthSquared );
            }
            m_AxiomMesh.BoundingSphereRadius = (float) Math.Sqrt( len );
        }

        private Skeleton SynthesizeSkeletonFromBones( Matrix4 transform, string baseFile )
        {
            Skeleton skeleton = null;

            if( BoneParents.Count != 0 )
            {
                skeleton = new Skeleton( baseFile + ".skeleton" );
                m_AxiomMesh.NotifySkeleton( skeleton );
                BuildSkeletonAtBindPose( transform, skeleton );
            }

            return skeleton;
        }

        private void SetUpInitialPose( Skeleton skeleton )
        {
            // Set up the initial pose for this model
            if( skeleton != null &&
                0 < BoneParents.Count )
            {
                // Ok, now we have a skeleton, so we can go ahead and rig the model
                // and animate it.
                foreach( GeometryInstance geoInstance in GeoInstances )
                {
                    SkinController skinController = geoInstance.controller as SkinController;

                    if( null != skinController )
                    {
                        m_Log.Debug( "Skinning: " + geoInstance.name );
                        geoInstance.SkinGeometry( m_AxiomMesh, skeleton, NodeSidMap, skinController );

                        foreach( PoseInfo info in m_PoseInfoCatalog.TargetInfos( geoInstance.geoSet.Name ) )
                        {
                            geoInstance.SkinMorphTarget( info.TargetGeometry, m_AxiomMesh, skeleton, NodeSidMap, skinController );
                        }
                    }
                    else
                    {
                        // If there is no controller for this geometry, then it probably is a rigging 
                        // artifact.  We need to remove it from the Axiom.Mesh, or Axiom will choke because
                        // it will be missing skinning bindings for the mesh.
                        //
                        // I've added this flag to allow disabling this culling because I'm worried that
                        // the culling will break in some unaccounted-for condition.  What, me worry?
                        if( ! NoRiggingCulling )
                        {
                            try
                            {
                                // Make sure it's not any kind of controller--morph, for example.
                                if( null == geoInstance.controller )
                                {
                                    m_Log.Debug( "Removing: " + geoInstance.name );
                                    // remove (maybe conditionally) submeshes for the unskinned instance
                                    foreach( MeshGeometry mesh in geoInstance.geoSet )
                                    {
                                        m_Log.Debug( "    ==> " + mesh.Id );
                                        m_AxiomMesh.RemoveSubMesh( mesh.Id );
                                    }
                                }
                            }
                            catch( Exception ex )
                            {
                                // There is no query on Axiom.Mesh to see if it contains a given submesh, 
                                // so it throws an exception if you try to remove an unknown submesh. It
                                // is not an error in this case, so we catch and swallow the exception.
                                m_Log.Debug( "Cannot remove submesh; " + Environment.NewLine + ex.Message );
                            }
                        }
                    }
                }
            }
        }

        private void BuildVertexBuffers( Matrix4 worldTransform )
        {
            Dictionary<GeometrySet, Matrix4> uniqueGeometrySets = new Dictionary<GeometrySet, Matrix4>();

            // Find all geometry sets
            foreach( GeometrySet geomSet in Geometries.Values )
            {
                if( ! uniqueGeometrySets.ContainsKey( geomSet ) )
                {
                    uniqueGeometrySets.Add( geomSet, Matrix4.Identity );
                }
            }

            // Apply instance transforms to the bound Geometries
            foreach( GeometryInstance geomInstance in GeoInstances )
            {
                if( uniqueGeometrySets.ContainsKey( geomInstance.geoSet ) )
                {
                    uniqueGeometrySets[ geomInstance.geoSet ] =
                        geomInstance.transform * geomInstance.BindShapeMatrix;
                }
            }

            foreach( GeometrySet geomSet in uniqueGeometrySets.Keys )
            {
                Matrix4 instanceXform = uniqueGeometrySets[ geomSet ];

                foreach( MeshGeometry mesh in geomSet )
                {
                    // Build vertex and index buffers in the mesh object
                    // based on the geometry object, transforms, and inputs.
                    BuildOneVertexBuffer( worldTransform, instanceXform, mesh );
                }
            }
        }


        public static void BuildBoundingBox( Mesh mesh, MeshGeometry geometry )
        {
            Vector3 max = mesh.BoundingBox.Maximum;
            Vector3 min = mesh.BoundingBox.Minimum;
            foreach( VertexDataEntry vde in geometry.VertexDataEntries )
            {
                if( vde.semantic != VertexElementSemantic.Position )
                    continue;
                for( int i = 0; i < geometry.VertexData.vertexCount; ++i )
                {
                    if( vde.fdata[ i, 0 ] < min.x )
                        min.x = vde.fdata[ i, 0 ];
                    if( vde.fdata[ i, 0 ] > max.x )
                        max.x = vde.fdata[ i, 0 ];
                    if( vde.fdata[ i, 1 ] < min.y )
                        min.y = vde.fdata[ i, 1 ];
                    if( vde.fdata[ i, 1 ] > max.y )
                        max.y = vde.fdata[ i, 1 ];
                    if( vde.fdata[ i, 2 ] < min.z )
                        min.z = vde.fdata[ i, 2 ];
                    if( vde.fdata[ i, 2 ] > max.z )
                        max.z = vde.fdata[ i, 2 ];
                }
            }
            mesh.BoundingBox = new AxisAlignedBox( min, max );
        }


        /// <summary>
        ///   Get the transform of the child bone at bind pose relative to 
        ///   the parent bone at bind pose.  Note that this could be different
        ///   for a different controller, but that I assume that it will be
        ///   the same for now, and just assert that all skinned objects use
        ///   the same bind pose for the skeleton.
        /// </summary>
        /// <param name="invBindMatrices">Dictionary mapping bone names to the 
        ///                             inverse matrix that defines the transform 
        ///								of that joint in the pose that we will 
        ///								use for the skeleton (the base of 
        ///								animations)</param>
        /// <param name="boneName">Name of the bone</param>
        /// <param name="parentName">Name of the parent bone</param>
        /// <param name="debug">Set this to true to dump the matrices</param>
        /// <returns></returns>
        protected Matrix4 GetLocalBindMatrix( Dictionary<string, Matrix4> invBindMatrices,
                                             string boneName, string parentName )
        {
            if( !invBindMatrices.ContainsKey( boneName ) )
            {
                m_Log.WarnFormat( "No skin seems to use bone: {0}", boneName );
                return Matrix4.Identity;
            }

            Matrix4 transform = invBindMatrices[ boneName ].Inverse();
            m_Log.DebugFormat( "BIND_MATRIX[{0}] = \n{1}", boneName, transform );
            
            if( parentName != null )
            {
                if( !invBindMatrices.ContainsKey( parentName ) )
                    m_Log.WarnFormat( "No skin uses parent bone: {0}", parentName );
                else
                    transform = invBindMatrices[ parentName ] * transform;
            }
            
            m_Log.DebugFormat( "LOCAL_BIND_MATRIX[{0}] = \n{1}", boneName, transform );
            
            return transform;
        }

        protected List<string> GetChildBones( string parentName )
        {
            List<string> rv = new List<string>();
            foreach( string boneName in BoneParents.Keys )
            {
                if( BoneParents[ boneName ] != parentName )
                    // only process the children of the parent on this pass
                    continue;
                rv.Add( boneName );
            }
            return rv;
        }

        /// <summary>
        ///   Build the bind pose for the skeleton based on the bind pose of the 
        ///   various Controllers (skin clusters).
        ///   This will also set up bones for the tag points.
        /// </summary>
        /// <param name="transform">the transform matrix to convert from the 
        ///                         system used by the modeling tool to the 
        ///                         system that will be used by multiverse</param>
        /// <param name="skeleton">Skeleton that will be built by this call</param>
        protected void BuildSkeletonAtBindPose( Matrix4 transform,
                                    Skeleton skeleton )
        {
            // Construct a list of bones that are of interest to us
            // This will be every bone that influences the skin, including the
            // parent bones (since they indirectly influence the skin).
            List<string> boneNames = new List<string>();
            foreach( Controller controller in Controllers.Values )
            {
                SkinController skinController = controller as SkinController;
                if( skinController == null )
                    continue;
                foreach( string key in skinController.InverseBindMatrices.Keys )
                {
                    string boneName = key;
                    // The entries in the skin controller may use sid.  If so,
                    // change these to use the bone name.
                    if( NodeSidMap.ContainsKey( boneName ) )
                        boneName = NodeSidMap[ boneName ];
                    if( boneNames.Contains( boneName ) )
                        continue;
                    boneNames.Add( boneName );
                    // Add all the parent bones
                    string currentBone = boneName;
                    while( BoneParents.ContainsKey( currentBone ) )
                    {
                        string parentBone = BoneParents[ currentBone ];
                        if( parentBone == null )
                            break;
                        if( !boneNames.Contains( parentBone ) )
                            boneNames.Add( parentBone );
                        currentBone = parentBone;
                    }
                }
            }
            Matrix4 unitConversionMatrix = Matrix4.Identity;
            unitConversionMatrix.Scale = new Vector3( m_UnitConversion, m_UnitConversion, m_UnitConversion );
            Matrix4 worldTransform = transform * unitConversionMatrix;

            // Build the invBindMatrices with the inverse bind matrices for 
            // the bones
            Dictionary<string, Matrix4> invBindMatrices =
                new Dictionary<string, Matrix4>();
            Quaternion worldRotate;
            Vector3 worldScale, worldTranslate;
            Matrix4.DecomposeMatrix( ref worldTransform, out worldTranslate, out worldRotate, out worldScale );
            foreach( string boneName in boneNames )
            {
                Matrix4 bonePoseTransform = GetBonePoseTransform( boneName );

                Quaternion bonePoseRotate;
                Vector3 bonePoseTranslate, bonePoseScale;
                Matrix4.DecomposeMatrix( ref bonePoseTransform, out bonePoseTranslate, out bonePoseRotate, out bonePoseScale );
                
                Vector3 boneTranslate = worldTranslate + worldRotate * MathHelpers.ScaleVector( worldScale, bonePoseTranslate );
                Quaternion boneOrient = worldRotate * bonePoseRotate;
                
                Matrix4 boneTransform = Multiverse.MathLib.MathUtil.GetTransform( boneOrient, boneTranslate );
                invBindMatrices[ boneName ] = boneTransform.Inverse();
            }
            ProcessBones( invBindMatrices, skeleton, null );
            skeleton.SetBindingPose();
        }

        /// <summary>
        ///   Set up the skeleton based on the information in the 
        ///   invBindMatrices dictionary.
        /// </summary>
        /// <param name="invBindMatrices">Dictionary of inverse matrices for 
        ///                               each of the joints</param>
        /// <param name="skeleton">Skeleton to build</param>
        /// <param name="parentBone">Bone for which we are building children (or null to build the root)</param>
        protected void ProcessBones( Dictionary<string, Matrix4> invBindMatrices,
                                    Skeleton skeleton, Bone parentBone )
        {
            string parentName = (parentBone == null) ? null : parentBone.Name;
            Quaternion rotate;
            Vector3 translate, scale;
            foreach( string boneName in GetChildBones( parentName ) )
            {
                // Get the relative transform by this bone in transformed space
                Matrix4 boneTransform = GetLocalBindMatrix( invBindMatrices, boneName, parentName );
                Matrix4.DecomposeMatrix( ref boneTransform, out translate, out rotate, out scale );
                Bone bone = skeleton.CreateBone( boneName );
                m_Log.DebugFormat( "Bone Transform for {0}:\n{1}", boneName, boneTransform );
                if( parentBone != null )
                    parentBone.AddChild( bone );
                // Apply the inverse of the parent transform
                bone.Orientation = rotate;
                bone.Position = translate;

                ProcessBones( invBindMatrices, skeleton, bone );
            }
        }


        /// <summary>
        ///   Set up the attachment points in the skeleton or mesh based on the 
        ///   information in the attachmentPoints.
        /// </summary>
        /// <param name="transform">the world transform to apply to this object</param>
        /// <param name="skeleton">the skeleton for whcih we may create the attachment (if there is a parent bone)</param>
        /// <param name="mesh">the mesh for which we may create the attachment point (if there is no parent bone)</param>
        protected void ProcessAttachmentPoints( Matrix4 transform,
                                               Skeleton skeleton, Mesh mesh )
        {
            foreach( AttachmentPointNode attachPoint in AttachmentPoints )
                ProcessAttachmentPointNode( transform, attachPoint, skeleton, mesh );
        }

        /// <summary>
        ///   Set up the attachment point in the skeleton or mesh based on the 
        ///   information in the attachPoint.
        /// </summary>
        /// <param name="transform">the world transform to apply to this object</param>
        /// <param name="attachPoint">the AttachmentPointNode with information about parent bone, and the relative transform</param>
        /// <param name="skeleton">the skeleton for whcih we may create the attachment (if there is a parent bone)</param>
        /// <param name="mesh">the mesh for which we may create the attachment point (if there is no parent bone)</param>
        protected void ProcessAttachmentPointNode( Matrix4 transform,
                                                  AttachmentPointNode attachPoint,
                                                  Skeleton skeleton, Mesh mesh )
        {
            string parentName = attachPoint.ParentBone;
            Bone parentBone = null;
            if( parentName != null )
                parentBone = skeleton.GetBone( parentName );

            Matrix4 attachPointTransform = attachPoint.Transform;
            if( parentBone == null )
            {
                // The parent bone will already have the transform in the hierarchy
                attachPointTransform = transform * attachPointTransform;
            }
            Quaternion orientation = MathHelpers.GetRotation( attachPointTransform );
            Vector3 position = m_UnitConversion * attachPointTransform.Translation;
            if( parentBone != null )
                skeleton.CreateAttachmentPoint( attachPoint.Name, parentBone.Handle, orientation, position );
            else
                mesh.CreateAttachmentPoint( attachPoint.Name, orientation, position );
            m_Log.InfoFormat( "Created attachment point: {0}", attachPoint.Name );
            m_Log.InfoFormat( "Parent Bone: {0}", parentName );
            m_Log.InfoFormat( "Orientation: {0}", orientation );
            m_Log.InfoFormat( "Position: {0}", position );
        }

        #region Animation

        protected float GetMaxTime( List<Channel> channels )
        {
            float maxTrack = 0;
            foreach( Channel channel in channels )
            {
                float maxTime = GetMaxTime( channel );
                maxTrack = Math.Max( maxTrack, maxTime );
            }
            return maxTrack;
        }

        protected float GetMaxTime( Channel channel )
        {
            float maxTime = 0;
            Accessor timeAccessor = channel.Sampler.Input.Accessor;
            for( int j = 0; j < timeAccessor.Count; ++j )
            {
                // TODO: Hardcoded parameter names
                float time = 0.0f;
                if( timeAccessor.ContainsParam( "TIME" ) )
                    time = (float) timeAccessor.GetParam( "TIME", j );
                else if( timeAccessor.ContainsParam( "time" ) )
                    time = (float) timeAccessor.GetParam( "time", j );
                else
                    Debug.Assert( false, "No time parameter for sampler" );
                maxTime = Math.Max( maxTime, time );
            }
            return maxTime;
        }

        /// <summary>
        ///   Creates the Animation object that will be populated with
        ///   key frame data.
        /// </summary>
        /// <param name="animationName">the name of the animation</param>
        /// <returns>the newly created and empty Animation object</returns>
        protected Animation CreateAnimation( string animationName )
        {
            if( Channels.Count == 0 )
                // no animations
                return null;

            // Get the maximum time sample on any channel
            float maxTrack = GetMaxTime( Channels );

            if( m_AxiomMesh.Skeleton.ContainsAnimation( animationName ) )
            {
                m_Log.WarnFormat( "Replacing skeleton animation: {0}", animationName );
                m_AxiomMesh.Skeleton.RemoveAnimation( animationName );
            }
            else if( m_AxiomMesh.ContainsAnimation( animationName ) )
            {
                m_Log.WarnFormat( "Replacing mesh animation: {0}", animationName );
                m_AxiomMesh.RemoveAnimation( animationName );
            }
            // We need to decide if this is a morph animation or a skeleton 
            // animation.  In the future we may also allow animations of other
            // objects to expose all the functionality of the Collada system.
            // bool isSkeletalAnimation = false;
            foreach( Channel channel in Channels )
            {
                // if( m_AxiomMesh.Skeleton.ContainsBone( channel.TargetComponent ) )
                //     isSkeletalAnimation = true;
            }
            Animation anim = m_AxiomMesh.Skeleton.CreateAnimation( animationName, maxTrack );
            anim.InterpolationMode = InterpolationMode.Linear;
            return anim;
        }

        /// <summary>
        ///   Convert a quaternion into a more standard form, where w > 0
        /// </summary>
        /// <param name="quat"></param>
        /// <returns></returns>
        public static Quaternion CleanupQuaternion( Quaternion quat )
        {
            if( quat.w < 0 )
                return -1 * quat;
            return quat;
        }

        /// <summary>
        ///   Get the indices in the channel sampler for either the preceding 
        ///   and successive keyframes, or the exact keyframe if there is a
        ///   matching time.
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        protected Dictionary<Channel, Triple<int, int, float>> GetKeyframeIndicesByTime( List<Channel> boneChannels, float time )
        {
            Dictionary<Channel, Triple<int, int, float>> frames =
                new Dictionary<Channel, Triple<int, int, float>>();
            foreach( Channel channel in boneChannels )
            {
                float preTime = float.MinValue;
                float postTime = float.MaxValue;
                int preIndex = -1;
                int postIndex = -1;
                float interpFactor = -1;
                Accessor timeAccessor = channel.Sampler.Input.Accessor;
                for( int j = 0; j < timeAccessor.Count; ++j )
                {
                    // TODO: Hardcoded parameter names
                    float channelTime = 0.0f;
                    if( timeAccessor.ContainsParam( "TIME" ) )
                        channelTime = (float) timeAccessor.GetParam( "TIME", j );
                    else if( timeAccessor.ContainsParam( "time" ) )
                        channelTime = (float) timeAccessor.GetParam( "time", j );
                    if( channelTime < time )
                    {
                        if( preIndex == -1 || channelTime > preTime )
                        {
                            preTime = channelTime;
                            preIndex = j;
                        }
                    }
                    else if( channelTime > time )
                    {
                        if( postIndex == -1 || channelTime < postTime )
                        {
                            postTime = channelTime;
                            postIndex = j;
                        }
                    }
                    else
                    { // we found a keyframe in this channel that matches the time
                        preIndex = postIndex = j;
                        interpFactor = 0;
                        break;
                    }
                }
                if( preIndex == -1 && postIndex != -1 )
                {
                    // If we only had one relevant keyframe, ust it as post and pre
                    preIndex = postIndex;
                    interpFactor = 0;
                }
                else if( postIndex == -1 && preIndex != -1 )
                {
                    postIndex = preIndex;
                    interpFactor = 0;
                }
                else if( preIndex == -1 && postIndex == -1 )
                {
                    Debug.Assert( false, "No keyframes in channel" );
                }
                if( preIndex != postIndex )
                    interpFactor = (time - preTime) / (postTime - preTime);
                frames[ channel ] = new Triple<int, int, float>( preIndex, postIndex, interpFactor );
            }
            return frames;
        }

        /// <summary>
        ///   Get the interpolation mode used for this channel
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="interpMode"></param>
        /// <returns></returns>
        protected bool GetInterpolationMode( Channel channel, int index, ref InterpolationMode interpMode )
        {
            // We have a datapoint that matches the time
            Sampler sampler = channel.Sampler;
            if( sampler.Interpolation != null )
            {
                Accessor interpAccessor = sampler.Interpolation.Accessor;
                string mode = null;
                if( interpAccessor.ContainsParam( "TRANSFORM" ) )
                    mode = (string) interpAccessor.GetParam( "TRANSFORM", index );
                if( mode == "LINEAR" )
                {
                    interpMode = InterpolationMode.Linear;
                    return true;
                }
                else if( mode == "BSPLINE" )
                {
                    interpMode = InterpolationMode.Spline;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        ///   Set the appropriate transform entries in the transforms change 
        ///   to be the result of linear interpolation of the entries from 
        ///   channel at preIndex and postIndex (weighted by interpFactor)
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="preIndex"></param>
        /// <param name="postIndex"></param>
        /// <param name="interpFactor">0 implies we just use preIndex.  1 implies we just use postIndex</param>
        /// <returns></returns>
        protected void SetTransformForChannel( List<NamedTransform> transforms, Channel channel, int preIndex, int postIndex, float interpFactor )
        {
            Sampler sampler = channel.Sampler;

            Accessor transformAccessor = sampler.Output.Accessor;
            NamedTransform namedTransform = null;
            foreach( NamedTransform trans in transforms )
            {
                string transName = trans.Name;
                if( transName == string.Empty )
                    transName = "transform";
                if( transName != channel.TargetComponent )
                    continue;
                namedTransform = trans;
                break;
            }

			if( null == namedTransform )
			{
				m_Log.DebugFormat(
					"Cannot find namedTransform for channel '{0}'", channel.TargetComponent );
			}
			else
			{
				namedTransform.SetFromChannel( channel, preIndex, postIndex, interpFactor, transformAccessor );
			}
		}

        /// <summary>
        ///   Use the data in the channels structures to create animations on
        ///   our skeleton.  This variant ignores the morph/pose animations,
        ///   and only considers the skeletal node animations.
        /// </summary>
        /// <param name="worldTransform"></param>
        /// <param name="anim">animation which will be populated with the data from channels</param>
        /// <param name="skeleton">the skeleton that contians the bone data</param>
        protected void ProcessChannels( Matrix4 worldTransform,
                                       Animation anim, Skeleton skeleton )
        {
            // We will get the rotation and translation transforms 
            // from our parent, but we need the scale transform 
            // as well.
            Quaternion worldRotate;
            Vector3 worldTranslate, worldScale;
            Matrix4.DecomposeMatrix( ref worldTransform, out worldTranslate, out worldRotate, out worldScale );

            // Run through the bones, find the channels associated with that bone.
            // create a track for that bone, and set up keyframes within the track.
            for( ushort i = 0; i < m_AxiomMesh.Skeleton.BoneCount; i++ )
            {
                Bone bone = m_AxiomMesh.Skeleton.GetBone( i );
                string boneName = bone.Name;
                // Build a list of the channels that apply to this bone.
                List<Channel> boneChannels = new List<Channel>();
                foreach( Channel channel in Channels )
                {
                    if( channel.TargetNode != bone.Name )
                        // this channel isn't for this bone.. just continue;
                        continue;
                    boneChannels.Add( channel );
                }

                // Get the bind pose data out of the skeleton
                Quaternion bindRotation = bone.Orientation;
                // bind translation -- the bone.Position has has already been
                // converted, and we are going to compare this to the 
                // converted data in the joint transform chains
                Vector3 bindTranslation = bone.Position;
                // Get the scene data that shows the current bone position
                if( !JointTransformChains.ContainsKey( bone.Name ) )
                {
                    m_Log.WarnFormat( "Missing joint transform chain for possible tag point {0}.", bone.Name );
                    continue;
                }
                List<NamedTransform> transforms = JointTransformChains[ bone.Name ];
                Matrix4 posedBoneTransform = MathHelpers.Normalize( worldTransform * GetBonePoseTransform( bone.Name ) );
                Matrix4 posedParentTransform = Matrix4.Identity;
                if( bone.Parent != null )
                    posedParentTransform = MathHelpers.Normalize( worldTransform * GetBonePoseTransform( bone.Parent.Name ) );
                Matrix4 initialPoseTransform = posedParentTransform.Inverse() * posedBoneTransform;
                m_Log.DebugFormat( "Matrices for {0}: posedBoneTransform:\n{1}\nposedParentTransform:\n{2}", bone.Name, posedBoneTransform, posedParentTransform );

                if( boneChannels.Count == 0 )
                {
                    Matrix4 bindTransform = Multiverse.MathLib.MathUtil.GetTransform( bindRotation, bindTranslation );
                    // Check to see if the bind pose and the initial pose for this 
                    // scene are the same.  If they are not the same, generate 
                    // a track that has one keyframe, just to put the bone in 
                    // the right position.
                    m_Log.InfoFormat( "No bone channels for {0}", bone.Name );
                    if( initialPoseTransform == bindTransform )
                    {
                        // No need to create an animation track to move the 
                        // bone to the initial pose, since this is the bind
                        // pose already.
                        continue;
                    }
                    // I used to skip bones for which I lacked bind pose data (from a skin cluster)
                    // but now that I have moved the channel processing code after the skeleton
                    // generation code, I am certain to have bind pose data for the bones in the
                    // skeleton.
                }
                if( m_Log.IsDebugEnabled )
                    m_Log.DebugFormat( "Matrices for {0}: Bind:\n{1}\nInitial:\n{2}", bone.Name, Multiverse.MathLib.MathUtil.GetTransform( bindRotation, bindTranslation ), initialPoseTransform );

                NodeAnimationTrack track = anim.CreateNodeTrack( bone.Handle, bone );

                if( boneChannels.Count == 0 )
                {
                    // Generate a track that has one keyframe, just to put 
                    // the bone in the right position.
                    m_Log.InfoFormat( "Generating initial keyframe based on bind pose for {0}", bone.Name );
                    float time = 0;
                    TransformKeyFrame keyFrame = (TransformKeyFrame) track.CreateKeyFrame( time );
                    Matrix4 frameTransform = initialPoseTransform;
                    Quaternion frameRotate;
                    Vector3 frameTranslate, frameScale;
                    Matrix4.DecomposeMatrix( ref frameTransform, out frameTranslate, out frameRotate, out frameScale );
                    m_Log.DebugFormat( "Initial pose for {0}:\n{1}", bone.Name, frameTransform );
                    keyFrame.Rotation = CleanupQuaternion( bindRotation.Inverse() * frameRotate );
                    keyFrame.Translate = frameTranslate - bindTranslation;
                    continue;
                }

                // Set the interpolation mode based on the bone channel
                bool interpModeSet = false;
                foreach( Channel channel in boneChannels )
                {
                    InterpolationMode interpMode = InterpolationMode.Linear;
                    for( int j = 0; j < channel.Sampler.Interpolation.Count; ++j )
                    {
                        if( GetInterpolationMode( channel, j, ref interpMode ) )
                        {
                            if( interpModeSet && anim.InterpolationMode != interpMode )
                                m_Log.WarnFormat( "Mismatched interpolation modes for different channels: {0} != {1}", anim.InterpolationMode, interpMode );
                            anim.InterpolationMode = interpMode;
                            interpModeSet = true;
                        }
                    }
                }

                // Assert that all the time accessors for the controlling channels are the same.
                // This will break if we don't bake transforms, since in that case, we will get
                // independent timeChannels for the various boneChannels.
                if( boneChannels.Count != 1 )
                    m_Log.Warn( "Multiple bone channels for a single bone.  Check that transforms have been baked." );
                // Build a list of all the times that we access any bone channel
                List<float> times = new List<float>();
                foreach( Channel channel in boneChannels )
                {
                    Accessor timeAccessor = channel.Sampler.Input.Accessor;
                    for( int j = 0; j < timeAccessor.Count; ++j )
                    {
                        // TODO: Hardcoded parameter names
                        float time = 0.0f;
                        if( timeAccessor.ContainsParam( "TIME" ) )
                            time = (float) timeAccessor.GetParam( "TIME", j );
                        else if( timeAccessor.ContainsParam( "time" ) )
                            time = (float) timeAccessor.GetParam( "time", j );
                        else
                            Debug.Assert( false, "No time parameter for sampler" );
                        if( !times.Contains( time ) )
                            times.Add( time );
                    }
                }
                foreach( float time in times )
                {
                    TransformKeyFrame keyFrame = (TransformKeyFrame) track.CreateKeyFrame( time );
                    Dictionary<Channel, Triple<int, int, float>> channelIndices =
                        GetKeyframeIndicesByTime( boneChannels, time );
                    foreach( Channel channel in boneChannels )
                    {
                        Triple<int, int, float> interpInfo = channelIndices[ channel ];
                        SetTransformForChannel( transforms, channel, interpInfo.first, interpInfo.second, interpInfo.third );
                    }
                    // Matrix4 fullTransform = worldTransform * GetBakedTransform(transforms) * bone.FullTransform;
                    Matrix4 frameTransform = GetBakedTransform( transforms );
                    Quaternion frameRotate;
                    Vector3 frameTranslate, frameScale;
                    Matrix4.DecomposeMatrix( ref frameTransform, out frameTranslate, out frameRotate, out frameScale );
                    //log.InfoFormat("frameRotate = {0}", GetEulerString(frameRotate));
                    //{
                    //foreach (NamedTransform nmt in transforms) {
                    //    if (nmt is NamedRotateTransform) {
                    //        Matrix4 tmp = nmt.Transform;
                    //        Quaternion foo = Quaternion.Identity;
                    //        foo.FromRotationMatrix(tmp.GetMatrix3());
                    //        log.InfoFormat("NamedRotateTransform for {0} = {1}", nmt.Name, GetEulerString(foo));
                    //    }
                    //}
                    //}
                    // FIXME
                    if( bone.Parent != null )
                    {
                        // we have a parent, so apply scale factor to our 
                        // translation, but any orientation or translation 
                        // from the world will already be included in our 
                        // parent.
                        frameTranslate = MathHelpers.ScaleVector( worldScale, frameTranslate );
                    }
                    else
                    {
                        // no parent, so we need to incorporate the world 
                        // transform's scale, orientation and translation.
                        frameTranslate = worldTransform * frameTranslate;
                        frameRotate = worldRotate * frameRotate;
                    }

                    // Frame transform is our transform relative to the
                    // world-transformed parent bone. If there is no parent, 
                    // we are using the identity matrix.
                    // transform from the dae file is based on the position of 
                    // the bone relative to the parent bone.  however, in 
                    // axiom, the keyframes are based on the position of the 
                    // bone relative to the bind position of the bone.
                    // log.DebugFormat("bindRotation = {0}", GetEulerString(bindRotation));
                    // log.DebugFormat("bindRotation.Inverse = {0}", GetEulerString(bindRotation.Inverse()));
                    // log.DebugFormat("frameRotate = {0}", GetEulerString(frameRotate));
                    keyFrame.Rotation = CleanupQuaternion( bindRotation.Inverse() * frameRotate );
                    keyFrame.Translate = frameTranslate - bindTranslation;
                }
            }
        }

        #endregion Animation

        /// <summary>
        ///   Find the morph controller that is modified by this target source
        /// </summary>
        /// <param name="targetSource"></param>
        /// <returns></returns>
        private MorphController FindMorphController( string targetSource )
        {
            foreach( Controller controller in Controllers.Values )
            {
                MorphController morphController = controller as MorphController;
                if( morphController == null )
                    continue;
                foreach( List<InputSourceCollection> sources in morphController.InputSources.Values )
                {
                    foreach( InputSourceCollection sourceCollection in sources )
                    {
                        foreach( InputSource source in sourceCollection.GetSources() )
                        {
                            if( source.Source == targetSource )
                                return morphController;
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        ///   Generate the list of pose info objects that describe the mapping
        ///   from base poses and target poses.  This also updates the 
        ///   Geometries so that the target pose has a reference to the base 
        ///   pose.
        /// </summary>
        protected void GeneratePoseList()
        {
            foreach( Controller controller in Controllers.Values )
            {
                MorphController morphController = controller as MorphController;

                if( morphController == null ) // not a morph controller
                {
                    morphController = (controller as SkinController).Morph;
                }

                if( null == morphController )
                {
                    continue;
                }   
                
                InputSource morphTargetSource =
                    morphController.GetInputSourceBySemantic( "MORPH_TARGET" );

                for( int i = 0; i < morphTargetSource.Accessor.Count; ++i )
                {
                    string morphTarget = (string) morphTargetSource.Accessor.GetParam( "MORPH_TARGET", i );

                    if( null != Geometries[ morphTarget ].BaseGeometrySet )
                    {
                        Debug.Assert(
                            Geometries[ morphTarget ].BaseGeometrySet.Equals( morphController.Target ),
                            "Morph base geometry does not correlate to the morph controller geometry." );
                    }
                    else
                    {
                        // The nomenclature gets a little confused here. With controllers, we use
                        // the term 'target' to refer to the the geometry that gets deformed, whereas
                        // with morphs one commonly uses 'target' to indicate the final shape of a
                        // deformation.
                        GeometrySet baseGeometry = morphController.Target;

                        Geometries[ morphTarget ].BaseGeometrySet = baseGeometry;
                        
                        PoseInfoList.Add(
                            new PoseInfo( baseGeometry, Geometries[ morphTarget ] ) );

                        m_PoseInfoCatalog.Add( baseGeometry, Geometries[ morphTarget ] );
                    }
                }
            }
        }

        protected void BuildPoses()
        {
            foreach( PoseInfo info in m_PoseInfoCatalog.AllInfos() )
            {
                info.Build( AxiomMesh );
            }
        }


        /// <summary>
        ///   Get the pose info object that contains information about
        ///   the pose indices for pose entries for the morph from src
        ///   to dst.
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        /// <returns></returns>
        public PoseInfo GetPoseInfo( GeometrySet src, GeometrySet dst )
        {
            return m_PoseInfoCatalog.FindInfo( src, dst );
        }

        protected void BuildVertexKeyFrameData( Dictionary<MeshGeometry, List<PoseRef>> initialPoses,
                                               Dictionary<MeshGeometry, VertexAnimationTrack> tracks )
        {
            // Now iterate through my channels
            bool firstChannel = true;
            foreach( Channel channel in Channels )
            {
                if( channel.TargetSource == null )
                    // not a morph channel
                    continue;
                Accessor timeAccessor = channel.Sampler.Input.Accessor;
                Accessor influenceAccessor = channel.Sampler.Output.Accessor;
                // The data for which morph target's influence is being 
                // animated is in channel.TargetMember.  In the simplest case, 
                // this will be '(0)' which indicates that this channel
                // is animating the influence of the first morph target.
                int targetIndex = MorphController.GetTargetAttributeIndex( channel.TargetMember );
                MorphController morphController = FindMorphController( channel.TargetSource );
                InputSource morphTargetSource = morphController.GetInputSourceBySemantic( "MORPH_TARGET" );
                string targetName = (string) morphTargetSource.Accessor.GetParam( "MORPH_TARGET", targetIndex );
                GeometrySet geoSet = morphController.Target;
                GeometrySet targetSet = Geometries[ targetName ];
                PoseInfo poseInfo = GetPoseInfo( geoSet, targetSet );
                for( int j = 0; j < timeAccessor.Count; ++j )
                {
                    // TODO: Hardcoded parameter names
                    float time = 0.0f;
                    if( timeAccessor.ContainsParam( "TIME" ) )
                        time = (float) timeAccessor.GetParam( "TIME", j );
                    else if( timeAccessor.ContainsParam( "time" ) )
                        time = (float) timeAccessor.GetParam( "time", j );
                    else
                        Debug.Assert( false, "No time parameter for sampler" );
                    float newInfluence = (float) influenceAccessor.GetParam( "", j );
                    for( int geoIndex = 0; geoIndex < geoSet.Count; ++geoIndex )
                    {
                        MeshGeometry geometry = geoSet[ geoIndex ];
                        VertexAnimationTrack track = tracks[ geometry ];
                        VertexPoseKeyFrame vkf = null;
                        foreach( KeyFrame kf in track.KeyFrames )
                        {
                            if( kf.Time != time )
                                continue;
                            vkf = kf as VertexPoseKeyFrame;
                            break;
                        }
                        // Check to see if this is the first channel with a keyframe at this time
                        if( firstChannel )
                            Debug.Assert( vkf == null );
                        else
                            Debug.Assert( vkf != null );
                        if( vkf == null )
                        {
                            vkf = track.CreateVertexPoseKeyFrame( time );
                            // Copy the initial pose data into this keyframe
                            // TODO: This would not handle the case where multiple 
                            // channels have different time data, since we would be
                            // using the original pose info for all the other 
                            // channels at that time.
                            foreach( PoseRef poseRef in initialPoses[ geometry ] )
                                vkf.PoseRefs.Add( poseRef );
                        }
                        // If this pose ref has a pose index that we know about
                        // and should be animating with this channel, update the 
                        // influence to be the value from the animated channel
                        // instead of the value from the controller.
                        ushort poseIndex = (ushort) poseInfo.poseIndices[ geoIndex ];
                        vkf.UpdatePoseReference( poseIndex, newInfluence );
                    }
                }
                firstChannel = false;
            }
        }

        protected void BuildInitialPoseInfo( Dictionary<MeshGeometry, List<PoseRef>> initialPoses,
                                            Dictionary<MeshGeometry, VertexAnimationTrack> tracks,
                                            Animation anim, MorphController morphController )
        {
            InputSource morphTargetSource = morphController.GetInputSourceBySemantic( "MORPH_TARGET" );
            InputSource morphWeightSource = morphController.GetInputSourceBySemantic( "MORPH_WEIGHT" );
            List<MeshGeometry> initialGeometries = morphController.Target;
            for( int geoIndex = 0; geoIndex < initialGeometries.Count; ++geoIndex )
            {
                MeshGeometry geometry = initialGeometries[ geoIndex ];
                // These geometry.Id are the submesh names (e.g. head01-mesh.0)
                for( ushort i = 0; i < m_AxiomMesh.SubMeshCount; ++i )
                {
                    SubMesh subMesh = m_AxiomMesh.GetSubMesh( i );
                    if( subMesh.Name != geometry.Id )
                        continue;
                    VertexData vData = null;
                    ushort target;
                    if( subMesh.useSharedVertices )
                    {
                        target = 0;
                        vData = m_AxiomMesh.SharedVertexData;
                    }
                    else
                    {
                        target = (ushort) (i + 1);
                        vData = subMesh.vertexData;
                    }
                    VertexAnimationTrack track =
                        anim.CreateVertexTrack( target, vData, VertexAnimationType.Pose );
                    tracks[ geometry ] = track;
                    // This is the initial set of influences on the geometry
                    List<PoseRef> initialPoseInfo = new List<PoseRef>();
                    initialPoses[ geometry ] = initialPoseInfo;
                    for( int k = 0; k < morphTargetSource.Accessor.Count; ++k )
                    {
                        // morphTargetName is the id of the mesh we are blending in (e.g. head02-mesh)
                        string morphTargetName = (string) morphTargetSource.Accessor.GetParam( "MORPH_TARGET", k );
                        float influence = (float) morphWeightSource.Accessor.GetParam( "MORPH_WEIGHT", k );
                        GeometrySet morphDest = Geometries[ morphTargetName ];
                        // Get the pose index for the target submesh
                        // First find the PoseInfo object that is about this combination
                        PoseInfo poseInfo = GetPoseInfo( morphController.Target, morphDest );
                        // Now that I have the pose info for this combination,
                        // I still need to find the pose index for the portion 
                        // that is associated with my MeshGeometry (instead of 
                        // the GeometrySet).  Since I added these in order, I 
                        // should be able to retrieve it in order as well.
                        ushort poseIndex = (ushort) poseInfo.poseIndices[ geoIndex ];
                        initialPoseInfo.Add( new PoseRef( poseIndex, influence ) );
                    }
                }
            }
        }

        public void ProcessVertexAnimationChannels()
        {
            // This is the list of morph targets (e.g. head01-mesh)
            List<string> morphTargetList = new List<string>();
            string animationName = "morph_animation";
            foreach( Channel channel in Channels )
            {
                if( channel.TargetSource == null )
                    continue;
                morphTargetList.Add( channel.TargetSource );
                if( channel.ParentAnimationName != null )
                    animationName = channel.ParentAnimationName;
            }
            if( morphTargetList.Count == 0 )
                return;
            m_Log.InfoFormat( "Creating vertex animation named {0}", animationName );
            // I need to run through all the channels, and find the longest one
            if( !m_AxiomMesh.ContainsAnimation( animationName ) )
            {
                float maxTime = GetMaxTime( Channels );
                m_AxiomMesh.CreateAnimation( animationName, maxTime );
            }
            Animation anim = m_AxiomMesh.GetAnimation( animationName );

            Dictionary<MeshGeometry, List<PoseRef>> initialPoses = new Dictionary<MeshGeometry, List<PoseRef>>();
            Dictionary<MeshGeometry, VertexAnimationTrack> tracks = new Dictionary<MeshGeometry, VertexAnimationTrack>();
            foreach( string morphTarget in morphTargetList )
            {
                // What I do here is build the influence data for the initial 
                // pose based on the morph controller (ignoring the animated 
                // channel), then run through the keyframes for each channel, 
                // and update the influences.
                // One problem with this approach is that the times for the 
                // different channels may not match.  I suppose I could 
                // linearly interpolate the other targets though.  I should 
                // at least add code to throw an exception if the various
                // channels don't have the same times
                MorphController morphController = FindMorphController( morphTarget );
                if( morphController == null )
                {
                    m_Log.Warn( "Controller is not a morph controller" );
                    continue;
                }
                BuildInitialPoseInfo( initialPoses, tracks, anim, morphController );
            }

            // Ok, at this point, I have my initial pose information.  I also
            // have the animation with a track for each morph controller.

            BuildVertexKeyFrameData( initialPoses, tracks );
        }

        /// <summary>
        ///   Combine the list of transforms to generate a baked transform.
        /// </summary>
        /// <param name="transforms"></param>
        /// <returns></returns>
        public static Matrix4 GetBakedTransform( List<NamedTransform> transforms )
        {
            // applied in the order in the list
            Matrix4 rv = Matrix4.Identity;
            foreach( NamedTransform transform in transforms )
                rv = transform.Transform * rv;
            return rv;
        }

        /// <summary>
        ///   Take the data from input, and use it to build a two dimensional 
        ///   array of data which will be added to the geometry's 
        ///   VertexDataElements.
        /// </summary>
        /// <param name="geometry">the mesh geometry that owns the data</param>
        /// <param name="input">the input source with the information to populate the vertex data</param>
        /// <param name="accessIndex">the index where our data is in the source</param>
        /// <param name="textureIndex">the texture index when appropriate or 0</param>
        /// <param name="vertexCount">the number of vertices</param>
        protected void ProcessInput( MeshGeometry geometry,
                                    InputSource input,
                                    int accessIndex,
                                    int textureIndex,
                                    int vertexCount )
        {
            VertexDataEntry vde = null;
            switch( input.Semantic )
            {
            case "POSITION":
            case "BIND_SHAPE_POSITION":
                vde = HWBuffer.ExtractData( 
                                  VertexElementSemantic.Position,
                                  vertexCount, input,
                                  geometry.IndexSets, accessIndex );
                break;
            case "NORMAL":
            case "BIND_SHAPE_NORMAL":
                vde = HWBuffer.ExtractData( 
                                  VertexElementSemantic.Normal,
                                  vertexCount, input,
                                  geometry.IndexSets, accessIndex );
                break;
            case "COLOR":
                vde = HWBuffer.ExtractData( 
                                  VertexElementSemantic.Diffuse,
                                  vertexCount, input,
                                  geometry.IndexSets, accessIndex );
                break;
            case "UV":
            case "TEXCOORD":
                vde = HWBuffer.ExtractData( 
                                  VertexElementSemantic.TexCoords,
                                  vertexCount, input,
                                  geometry.IndexSets, accessIndex );
                break;
            case "TANGENT":
            case "TEXTANGENT":
                vde = HWBuffer.ExtractData( 
                                  VertexElementSemantic.Tangent,
                                  vertexCount, input,
                                  geometry.IndexSets, accessIndex );
                break;
            case "BINORMAL":
            case "TEXBINORMAL":
                vde = HWBuffer.ExtractData( 
                                  VertexElementSemantic.Binormal,
                                  vertexCount, input,
                                  geometry.IndexSets, accessIndex );
                break;
            default:
                m_Log.WarnFormat( "Not yet handling input semantic: {0}", input.Semantic );
                break;
            }
            if( vde != null )
            {
                vde.textureIndex = textureIndex;
                geometry.VertexDataEntries.Add( vde );
            }
        }

        protected bool CheckForCollisions( InputSource input, List<InputSource> list )
        {
            foreach( InputSource entry in list )
                if( input.Semantic == entry.Semantic )
                    return true;
            return false;
        }

        protected bool CheckForCollisions( List<InputSource> unnumberedInputs,
                                          Dictionary<InputSource, int> inputSetMap,
                                          int indexToCheck )
        {
            foreach( InputSource input in unnumberedInputs )
                foreach( InputSource key in inputSetMap.Keys )
                    if( key.Set == indexToCheck && key.Semantic == input.Semantic )
                        return true;
            return false;
        }

        protected void AddUnnumberedInput( InputSource input, Dictionary<int, List<InputSource>> inputMap )
        {
            foreach( int i in inputMap.Keys )
            {
                if( !CheckForCollisions( input, inputMap[ i ] ) )
                {
                    // no collision -- we can add to this index
                    inputMap[ i ].Add( input );
                    return;
                }
            }
            // We have run through the existing sets, and failed to find a 
            // place where could add our entry.  Add a new set.
            List<InputSource> newSet = new List<InputSource>();
            newSet.Add( input );
            inputMap[ inputMap.Count ] = newSet;
        }

        /// <summary>
        ///   Add the set of unnumbered inputs to our input set map, moving
        ///   existing entries up, if needed.
        /// </summary>
        /// <param name="unnumberedInputs"></param>
        /// <param name="inputSetMap"></param>
        protected void AddUnnumberedInputs( List<InputSource> unnumberedInputs,
                                           Dictionary<InputSource, int> inputSetMap )
        {
            // We have some items that need a set. If there is no 
            // collision with the inputs associated with 0, we can just 
            // assign these entries to 0.  In that case, if there is no
            // collision with the lowest numbered set, we will also
            // want to move the lowest numbered set down to zero.
            int lowestKey = -1;
            foreach( InputSource key in inputSetMap.Keys )
                if( lowestKey == -1 || inputSetMap[ key ] < lowestKey )
                    lowestKey = inputSetMap[ key ];
            if( lowestKey == -1 )
                // there were no numbered units - put the unnumbered inputs at 0,
                foreach( InputSource input in unnumberedInputs )
                    inputSetMap[ input ] = 0;
            else if( CheckForCollisions( unnumberedInputs, inputSetMap, lowestKey ) )
            {
                if( lowestKey == 0 )
                {
                    // In order to use the zero set for these unnumbered 
                    // entries, I need to move the lowest set.  Just move
                    // all the existing sets up by one.
                    foreach( InputSource key in inputSetMap.Keys )
                        inputSetMap[ key ] = inputSetMap[ key ] + 1;
                }
                // put the unnumbered inputs at 0, distinct from the lowest key
                foreach( InputSource input in unnumberedInputs )
                    inputSetMap[ input ] = 0;
            }
            else
                // put the unnumbered inputs at the lowest key
                foreach( InputSource input in unnumberedInputs )
                    inputSetMap[ input ] = lowestKey;
        }

        protected void BuildOneVertexBuffer( Matrix4 worldTransform,
                                             Matrix4 bindShapeMatrix,
                                             MeshGeometry geometry )
        {
            VertexData vertexData = geometry.VertexData;
            vertexData.vertexStart = 0;
            vertexData.vertexCount = geometry.IndexSets.Count;
            List<InputSource> geometryInputs = geometry.GetAllInputSources();

            #region Texture stuff

            // We need to build a compact set of texture coordinates.  There can be multiple
            // sets of texture coordinates, but the indices must start at 0 to work for all
            // graphics cards.
            Dictionary<InputSource, int> inputSetMap = new Dictionary<InputSource, int>();
            int maxInputSet = -1;
            Dictionary<int, List<InputSource>> unnumberedInputs = new Dictionary<int, List<InputSource>>();
            // First make a pass through, storing the index sets for the inputs
            for( int accessIndex = 0; accessIndex < geometryInputs.Count; ++accessIndex )
            {
                InputSource input = geometryInputs[ accessIndex ];
                if( input.Set != -1 )
                {
                    inputSetMap[ input ] = input.Set;
                    if( input.Set > maxInputSet )
                        maxInputSet = input.Set;
                }
                else
                    AddUnnumberedInput( input, unnumberedInputs );
            }
            // If we have stuff associated with input set 0, we need to find 
            // out where to put the default data, such as position and normals.
            // We would like to assign input set 0 to these as well, but if
            // those are already used, we need to move whatever is there.
            if( unnumberedInputs.Count > 0 )
            {
                // We have some items that need a set. If there is no 
                // collision with the inputs associated with 0, we can just 
                // assign these entries to 0.  In that case, if there is no
                // collision with the lowest numbered set, we will also
                // want to move the lowest numbered set down to zero.
                List<int> keys = new List<int>( unnumberedInputs.Keys );

                keys.Sort();
                keys.Reverse();
                foreach( int i in keys )
                    AddUnnumberedInputs( unnumberedInputs[ i ], inputSetMap );
            }

            // Now build a mapping from input sets to texture indices
            // This collapses the set and removes gaps
            Dictionary<int, int> textureIndexMap = new Dictionary<int, int>();
            int maxTextureIndex = -1;
            List<int> inputIndices = new List<int>( inputSetMap.Values );
            inputIndices.Sort();
            foreach( int inputSetId in inputIndices )
            {
                if( textureIndexMap.ContainsKey( inputSetId ) )
                    continue;
                textureIndexMap[ inputSetId ] = ++maxTextureIndex;
            }

            #endregion Texture stuff

            // What is the offset into point components of the given input
            for( int accessIndex = 0; accessIndex < geometryInputs.Count; ++accessIndex )
            {
                InputSource input = geometryInputs[ accessIndex ];
                int textureIndex = textureIndexMap[ inputSetMap[ input ] ];
                m_Log.InfoFormat( "Assigned texture index {0} to input {1} with set {2}", textureIndex, input.Source, input.Set );
                ProcessInput( geometry, input, accessIndex, textureIndex, geometry.IndexSets.Count );
            }

            // If we are a pose target, we want to skip building the faces 
            // and the submesh, and just skip to the next section where we 
            // transform the geometry.
            if( !geometry.GeometrySet.IsPoseTarget )
            {
                foreach( SubMeshInfo smInfo in geometry.SubMeshes )
                {
                    // Ok, now use the geometry to make a submesh
                    IndexType indexType = IndexType.Size16;
                    if( geometry.IndexSets.Count > short.MaxValue )
                    {
                        indexType = IndexType.Size32;
                        m_Log.Warn( "Warning: submesh requires 32 bit index.  This will not work on DirectX 6 cards such as the common Intel series." );
                    }
                    ProcessFaces( smInfo, geometry, indexType );
                }
                if( geometry.SubMeshes.Count > 1 )
                {
                    if( m_AxiomMesh.SharedVertexData != null )
                    {
                        m_Log.Error( "Failed to load file: multiple shared submeshes" );
                        throw new Exception( "Failed to load file: multiple shared submeshes" );
                    }
                    m_AxiomMesh.SharedVertexData = vertexData;
                    foreach( SubMeshInfo smInfo in geometry.SubMeshes )
                    {
                        SubMesh subMesh = m_AxiomMesh.GetSubMesh( smInfo.name );
                        subMesh.useSharedVertices = true;
                    }
                }
                else if( geometry.SubMeshes.Count == 1 )
                {
                    string subMeshName = geometry.SubMeshes[ 0 ].name;
                    SubMesh subMesh = m_AxiomMesh.GetSubMesh( subMeshName );
                    subMesh.vertexData = vertexData;
                    subMesh.useSharedVertices = false;
                }
            }

            // Do the 'freeze transforms' step where we bake the transform
            // of this geometry into the data.  This is only the first part
            // of this.  We will also need to multiply it by the transform 
            // matrix that was passed in via the command line.  Finally, we
            // generally need to transform this data to reskin it to the 
            // skeleton's bind pose.
            // TODO: At some point, I could handle instanced geometry here by
            //       generating other submeshes.
            geometry.Transform( worldTransform * bindShapeMatrix );
        }

        /// <summary>
        ///   The geometry object has a list of polygon faces.  
        ///   Take those, and use them to populate the subMesh object.
        ///   TODO: Do I need to compact those first?
        ///         Do I need to build consolidated sets of points that the face indexes can refer to?
        /// </summary>
        /// <param name="subMesh">the Axiom SubMesh object that we will populate</param>
        /// <param name="geometry">the geometry information that contains the polygons</param>
        /// <param name="indexType">the type of face index (16 bit or 32 bit)</param>
        protected void ProcessFaces( SubMeshInfo subMeshInfo, MeshGeometry geometry,
                                    IndexType indexType )
        {
            SubMesh subMesh = m_AxiomMesh.CreateSubMesh( subMeshInfo.name );
            subMesh.MaterialName = subMeshInfo.material;

            int[ , ] data = geometry.GetFaceData();
            subMesh.indexData.indexStart = 0;
            subMesh.indexData.indexCount = data.GetLength( 0 ) * data.GetLength( 1 );

            HardwareIndexBuffer idxBuffer = null;

            // create the index buffer
            idxBuffer =
                HardwareBufferManager.Instance.CreateIndexBuffer( indexType,
                                                                subMesh.indexData.indexCount,
                                                                m_AxiomMesh.IndexBufferUsage,
                                                                m_AxiomMesh.UseIndexShadowBuffer );
            
            HWBuffer.FillBuffer( idxBuffer, subMesh.indexData.indexCount, indexType, data );

            // save the index buffer
            subMesh.indexData.indexBuffer = idxBuffer;
        }
    }
}
