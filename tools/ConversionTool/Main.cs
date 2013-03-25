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
using System.IO;
using System.Data;
using System.Diagnostics;
using System.Collections.Generic;
using System.Xml;
using System.Text.RegularExpressions;
using System.Security.Permissions;
using System.Threading;
using System.Globalization;

using log4net;
using log4net.Appender;
using log4net.Config;
using ICSharpCode.SharpZipLib.Zip;

using Axiom.Core;
using Axiom.MathLib;
using Axiom.Utility;
using Axiom.Graphics;
using Axiom.Collections;
using Axiom.Serialization;
using Axiom.Configuration;
using Axiom.Animating;
using Axiom.Media;

using Multiverse.Serialization;
using Multiverse.Serialization.Collada;
using Multiverse.CollisionLib;

// Notify the CLR that we will need to be able to control our thread (to set culture)
[assembly: SecurityPermission( SecurityAction.RequestMinimum, ControlThread = true )]
namespace Multiverse.ConversionTool
{

    /// <summary>
    ///     This tool reads in a file in one format, and outputs an xml file.
    /// </summary>
    public class Convert
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger( typeof( Convert ) );

        const string DisplayConfigFile = "../DisplayConfig.xml";
        const string EngineConfigFile = "../EngineConfig.xml";

        private static void Configure()
        {
            List<RenderSystem> renderSystems = Root.Instance.RenderSystems;
            RenderSystem renderSystem = renderSystems[0];
            DisplayConfig displayConfig = renderSystem.ConfigOptions;
            displayConfig.SelectMode(640, 480, 32, false);
            Root.Instance.RenderSystem = renderSystem;

            // SetupResources();

            RenderWindow window = Root.Instance.Initialize( true, "Axiom Engine Window" );
        }

        private static void ConfigureLogDefaults( string logfile )
        {
            log4net.Repository.Hierarchy.Hierarchy hierarchy = log4net.LogManager.GetRepository() as log4net.Repository.Hierarchy.Hierarchy;
            log4net.Layout.ILayout layout = new log4net.Layout.PatternLayout( "%-5p [%d{ISO8601}] %-20.20c{1} %m%n" );
            RollingFileAppender rfa = new RollingFileAppender();
            rfa.Layout = layout;
            rfa.File = logfile;
            rfa.AppendToFile = false;
            rfa.MaximumFileSize = "50MB";
            rfa.MaxSizeRollBackups = 3;
            rfa.Threshold = log4net.Core.Level.Debug;
            rfa.ActivateOptions();
            // hierarchy.Root.AddAppender(rfa);
            BasicConfigurator.Configure( rfa );
            ConsoleAppender ca = new ConsoleAppender();
            ca.Layout = layout;
            ca.Target = "Console.Out";
            ca.Threshold = log4net.Core.Level.Warn;
            ca.ActivateOptions();
            hierarchy.Root.AddAppender( ca );
        }
#if NOT
        /// <summary>
        ///		Loads default resource configuration if one exists.
        /// </summary>
        protected static void SetupResources()
        {
            string resourceConfigPath = Path.GetFullPath( EngineConfigFile );

            if( File.Exists( resourceConfigPath ) )
            {
                EngineConfig config = new EngineConfig();

                // load the config file
                // relative from the location of debug and releases executables
                config.ReadXml( EngineConfigFile );

                // interrogate the available resource paths
                foreach( EngineConfig.FilePathRow row in config.FilePath )
                {
                    ResourceManager.AddCommonArchive( row.src, row.type );
                }
            }
        }
#endif
        public struct AnimationEntry
        {
            public string animation_name;
            public string animation_file;
        }
        public struct LodEntry
        {
            public float distance;
            public string meshFile;
        }

#if NOT_USED
        public static void TestSockets() {
            {
                Quaternion offsetOrientation = Quaternion.FromAngleAxis((float)Math.PI / 2, new Vector3(1, 0, 0));
                Vector3 offsetPosition = new Vector3(220, 50, 0);
                Matrix4 transform = Matrix4.FromMatrix3(offsetOrientation.ToRotationMatrix());
                transform.Translation = offsetPosition;
                Console.WriteLine("human_male shield LeftForeArm {0}", transform);
            }
            {
                Quaternion offsetOrientation = Quaternion.FromAngleAxis((float)Math.PI, new Vector3(0, 0, 1))
                        * Quaternion.FromAngleAxis((float)Math.PI / 2, new Vector3(1, 0, 0));
                Vector3 offsetPosition = new Vector3(65, 20, 0);
                Matrix4 transform = Matrix4.FromMatrix3(offsetOrientation.ToRotationMatrix());
                transform.Translation = offsetPosition;
                Console.WriteLine("human_female shield L_Wrist_BIND_jjj {0}", transform);
            }
            {
                Quaternion offsetOrientation = Quaternion.FromAngleAxis((float)Math.PI / 2, new Vector3(1, 0, 0));
                Vector3 offsetPosition = new Vector3(75, -20, 0);
                Matrix4 transform = Matrix4.FromMatrix3(offsetOrientation.ToRotationMatrix());
                transform.Translation = offsetPosition;
                Console.WriteLine("human_male secondaryWeapon LeftHand {0}", transform);
            }
            {
                Quaternion offsetOrientation = Quaternion.FromAngleAxis((float)Math.PI, new Vector3(0, 0, 1))
                        * Quaternion.FromAngleAxis((float)Math.PI / 2, new Vector3(1, 0, 0));
                Vector3 offsetPosition = new Vector3(65, 20, 0);
                Matrix4 transform = Matrix4.FromMatrix3(offsetOrientation.ToRotationMatrix());
                transform.Translation = offsetPosition;
                Console.WriteLine("human_female secondaryWeapon L_Wrist_BIND_jjj {0}", transform);
            }
            {
                Quaternion offsetOrientation = Quaternion.FromAngleAxis((float)Math.PI, new Vector3(0, 0, 1))
                        * Quaternion.FromAngleAxis((float)Math.PI / 2, new Vector3(1, 0, 0));
                Vector3 offsetPosition = new Vector3(-75, -20, 0);
                Matrix4 transform = Matrix4.FromMatrix3(offsetOrientation.ToRotationMatrix());
                transform.Translation = offsetPosition;
                Console.WriteLine("human_male primaryWeapon RightHand {0}", transform);
            }
            {
                Quaternion offsetOrientation = Quaternion.FromAngleAxis((float)Math.PI / 2, new Vector3(1, 0, 0));
                Vector3 offsetPosition = new Vector3(65, 20, 0);
                Matrix4 transform = Matrix4.FromMatrix3(offsetOrientation.ToRotationMatrix());
                transform.Translation = offsetPosition;
                Console.WriteLine("human_female primaryWeapon R_Wrist_BIND_jjj {0}", transform);
            }
            {
                Quaternion offsetOrientation = Quaternion.Identity;
                Vector3 offsetPosition = new Vector3(0, 350, 0);
                Matrix4 transform = Matrix4.FromMatrix3(offsetOrientation.ToRotationMatrix());
                transform.Translation = offsetPosition;
                Console.WriteLine("human_male questavailable Head {0}", transform);
            }
            {
                Quaternion offsetOrientation = Quaternion.FromAngleAxis(-1 * (float)Math.PI / 2, Vector3.UnitZ);
                Vector3 offsetPosition = new Vector3(300, 0, 0);
                Matrix4 transform = Matrix4.FromMatrix3(offsetOrientation.ToRotationMatrix());
                transform.Translation = offsetPosition;
                Console.WriteLine("human_female questavailable Head_BIND_jjj {0}", transform);
            }
            {
                Matrix4 transform = Matrix4.Identity;
                Console.WriteLine("human_male name Head {0}", transform);
            }
            {
                Matrix4 transform = Matrix4.Identity;
                Console.WriteLine("human_female name Head_BIND_jjj {0}", transform);
            }

            {
                Matrix4 transform = Matrix4.Identity;
                Console.WriteLine("human_male bubble Head {0}", transform);
            }
            {
                Matrix4 transform = Matrix4.Identity;
                Console.WriteLine("human_female bubble Head_BIND_jjj {0}", transform);
            }
            {
                Quaternion offsetOrientation = Quaternion.FromAngleAxis((float)Math.PI / 2, new Vector3(1, 0, 0));
                Vector3 offsetPosition = new Vector3(170, 60, 0);
                Matrix4 transform = Matrix4.FromMatrix3(offsetOrientation.ToRotationMatrix());
                transform.Translation = offsetPosition;
                Console.WriteLine("orc primaryWeapon Right_Wrist_BIND_jjj {0}", transform);
            }
            {
                Quaternion offsetOrientation = Quaternion.FromAngleAxis((float)Math.PI, new Vector3(0, 0, 1))
						* Quaternion.FromAngleAxis((float)Math.PI / 2, new Vector3(1, 0, 0));
                Vector3 offsetPosition = new Vector3(170, 60, 0);
                Matrix4 transform = Matrix4.FromMatrix3(offsetOrientation.ToRotationMatrix());
                transform.Translation = offsetPosition;
                Console.WriteLine("orc secondaryWeapon Left_Wrist_BIND_jjj {0}", transform);
            }
        }
#endif

        private static string MaybeAddTrailingBackslash( string path )
        {
            if( path.Length > 0 )
            {
                Char ch = path[ path.Length - 1 ];
                if( ch != '\\' && ch != '/' )
                    path += "\\";
            }
            return path;
        }

        public class ParameterInfo
        {
            public void ProcessArgs( string[] args )
            {
                for( int i = 0; i < args.Length; ++i )
                {
                    switch( args[ i ] )
                    {
                        case "--transform":
                            for( int j = 0; j < 16; ++j )
                                if( i + 1 < args.Length )
                                    transform[ j ] = float.Parse( args[ ++i ] );
                                else
                                    Console.WriteLine( "Invalid transform" );
                            break;
                        case "--build_skeleton":
                            build_skeleton = true;
                            break;
                        case "--base_skeleton":
                            // This is overloaded.  It is used for adding multiple animations
                            // into a single skeleton, but it is also used to specify the name
                            // of the skeleton that will be referenced by the mesh file.
                            skeleton_file = args[ ++i ];
                            break;
                        case "--build_tangents":
                            build_tangents = true;
                            break;
                        case "--out_skeleton":
                            out_skeleton_file = args[ ++i ];
                            break;
                        case "--optimize_mesh":
                        case "--optimise_mesh":
                            optimize_mesh = true;
                            break;
                        case "--animation":
                            {
                                AnimationEntry entry = new AnimationEntry();
                                entry.animation_name = args[ ++i ];
                                entry.animation_file = args[ ++i ];
                                animations.Add( entry );
                                break;
                            }
                        case "--manual_lod":
                            {
                                LodEntry entry = new LodEntry();
                                entry.distance = float.Parse( args[ ++i ] );
                                entry.meshFile = args[ ++i ];
                                manualLodEntries.Add( entry );
                                break;
                            }
                        case "--socket":
                            {
                                Matrix4 attachTransform = Matrix4.Identity;
                                string name = args[ ++i ];
                                string parentBone = args[ ++i ];
                                for( int j = 0; j < 16; ++j )
                                    if( i + 1 < args.Length )
                                        attachTransform[ j ] = float.Parse( args[ ++i ] );
                                    else
                                        Console.WriteLine( "Invalid transform" );
                                AttachmentPointNode ap = new AttachmentPointNode( name, parentBone, attachTransform );
                                attachPoints.Add( ap );
                                break;
                            }
                        case "--test_physics":
                            {
                                test_physics = true;
                                return;
                            }
                        // LOD options
                        case "--lod_levels":
                            {
                                lodlevels = int.Parse( args[ ++i ] );
                                break;
                            }
                        case "--lod_distance":
                            {
                                loddist = float.Parse( args[ ++i ] );
                                break;
                            }
                        case "--lod_percent":
                            {
                                lodpercent = float.Parse( args[ ++i ] );
                                break;
                            }
                        case "--lod_num_triangles":
                            {
                                // lodnumtris
                                break;
                            }
#if NOT_USED
                        case "--merge_animations": {
                                // Deprecated
                                string destFile = args[++i];
                                string rigFile = args[++i];
                                List<string> animFiles = new List<string>();
                                while (i + 1 < args.Length)
                                    animFiles.Add(args[++i]);
                                MergeAnimations(srcDir, dstDir, destFile, rigFile, animFiles);
                                return;
                            }
                        case "--merge_collada": {
                                // Deprecated
                                string destFile = args[++i];
                                string rigFile = args[++i];
                                string animFile = args[++i];
                                MergeColladaFiles(srcDir, dstDir, destFile, rigFile, animFile);
                                return;
                            }
#endif
                        case "--3ds":
                            {
                                // Convert from a right handed system where z is up to a right handed system where y is up.
                                Matrix4 yupTrans = new Matrix4( 1, 0, 0, 0, 0, 0, 1, 0, 0, -1, 0, 0, 0, 0, 0, 1 );
                                transform = transform * yupTrans;
                                break;
                            }
                        case "--src_dir":
                            srcDir = MaybeAddTrailingBackslash( args[ ++i ] );
                            break;
                        case "--dst_dir":
                            dstDir = MaybeAddTrailingBackslash( args[ ++i ] );
                            break;
#if NOT_USED
                        case "--test_sockets":
                            TestSockets();
                            break;
#endif
                        case "--dont_extract_collision_volumes":
                            extract_collision_volumes = false;
                            break;
                        case "--log_collision_volumes":
                            CVExtractor.InitLog( true );
                            break;
                        case "--args_file":
                            ProcessArgumentFile( args[ ++i ] );
                            break;
                        case "--log_file":
                            log_file = args[ ++i ];
                            break;
                        case "--throw":
                            rethrow = true;
                            break;
                        case "--version":
                            Console.WriteLine( string.Format( "ConversionTool version: {0}",
                                                            System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() ) );
                            break;
                        case "--no_rigging_culling":
                            ColladaMeshReader.NoRiggingCulling = true;
                            break;
                        case "--":
                        case "--usage":
                        case "--help":
                            Convert.Usage();
                            abort = true;
                            break;
                        default:
                            last_arg = args[ i ];
                            break;
                    }
                    if( abort )
                        break;
                }
            }
            public void ProcessArgumentFile( string filename )
            {
                Stream argData = new FileStream( filename, FileMode.Open );
                StreamReader reader = new StreamReader( argData );
                string all_data = reader.ReadToEnd();
                reader.Close();
                argData.Close();
                string[] args = ParseArgumentString( all_data );
                ProcessArgs( args );
            }

            public string[] ParseArgumentString( string argStr )
            {
                List<string> args = new List<string>();
                int cur = 0;
                bool in_quote = false;
                string arg = "";
                while( cur < argStr.Length )
                {
                    switch( argStr[ cur ] )
                    {
                        case '\"':
                            in_quote = !in_quote;
                            break;
                        case '\\':
                            if( argStr.Length > (cur + 1) )
                                arg += argStr[ ++cur ];
                            break;
                        case ' ':
                        case '\t':
                            if( in_quote )
                                arg += argStr[ cur ];
                            else if( arg.Length > 0 )
                            {
                                args.Add( arg );
                                arg = "";
                            }
                            break;
                        default:
                            arg += argStr[ cur ];
                            break;
                    }
                    ++cur;
                }
                if( arg.Length > 0 )
                    args.Add( arg );
                return args.ToArray();
            }

            public string srcDir = string.Empty;
            public string dstDir = string.Empty;

            public List<AnimationEntry> animations = new List<AnimationEntry>();
            public List<AttachmentPointNode> attachPoints = new List<AttachmentPointNode>();
            public bool abort = false;
            public string out_skeleton_file = null;
            public bool build_skeleton = false;
            public bool build_tangents = false;
            // ??? Should this only be done as a result of a
            // command-line argument, or should it be done by default?
            public bool extract_collision_volumes = true;
            public string skeleton_file = null;
            public Matrix4 transform = Matrix4.Identity;
            public bool test_physics = false;
            public string last_arg = String.Empty;
            public int lodlevels = 0;
            public float loddist = 1000 * 10; // change lod every 10 meters
            public float lodpercent = 20; // at each level, have 80% of the vertices from the previous level
            public List<LodEntry> manualLodEntries = new List<LodEntry>();
            public string argumentFile;
            public bool rethrow = false; // do we rethrow exceptions or suppress them
            public bool optimize_mesh = false;
            public string log_file = "trace.txt";
        }

        private static int Main( string[] args )
        {
            // Changes the CurrentCulture of the current thread to the invariant
            // culture.
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            ParameterInfo paramInfo = new ParameterInfo();
            string argsString = "";
            foreach( string arg in args )
            {
                if( argsString.Length == 0 )
                    argsString = arg;
                else
                    argsString = argsString + " " + arg;
            }

            args = paramInfo.ParseArgumentString( argsString );
            paramInfo.ProcessArgs( args );
            if( paramInfo.abort )
                return 0;

            ConfigureLogDefaults( paramInfo.log_file );

            try
            {
                // get a reference to the engine singleton
                Axiom.Core.Root engine = new Root( EngineConfigFile, null );
                Configure();

                Skeleton skeleton = null;
                if( paramInfo.skeleton_file != null )
                    skeleton = ReadSkeleton( paramInfo.srcDir, paramInfo.skeleton_file );

                if( paramInfo.test_physics )
                    TestPhysics( paramInfo.srcDir, paramInfo.dstDir, "test.physics" );
                else if( paramInfo.build_skeleton )
                    BuildSkeletonMesh( paramInfo.dstDir, paramInfo.last_arg );
                else if( paramInfo.animations.Count > 0 )
                    // we are merging animations
                    AddAnimations( paramInfo.srcDir, paramInfo.dstDir, paramInfo.out_skeleton_file,
                                  paramInfo.transform, skeleton, paramInfo.animations );
                else if( paramInfo.attachPoints.Count > 0 )
                    AddAttachments( paramInfo.dstDir, paramInfo.out_skeleton_file, skeleton, paramInfo.attachPoints );
                else if( paramInfo.manualLodEntries.Count > 0 )
                    SetupManualLodLevels( paramInfo.srcDir, paramInfo.dstDir, paramInfo.last_arg, paramInfo.manualLodEntries );
                else if( paramInfo.lodlevels > 0 )
                    GenerateLodLevels( paramInfo.srcDir, paramInfo.dstDir, paramInfo.last_arg,
                                      paramInfo.lodlevels, paramInfo.loddist, paramInfo.lodpercent * 0.01f );
                else
                {
                    // we are either transforming a collada or ogre mesh, or just getting skeleton and
                    // mesh data out of one.
                    ConvertFile( paramInfo.srcDir, paramInfo.dstDir, paramInfo.last_arg, paramInfo.transform,
                                paramInfo.build_tangents, paramInfo.extract_collision_volumes, paramInfo.optimize_mesh,
                                paramInfo.skeleton_file );
                }
            }
            catch( Exception e )
            {
                log.ErrorFormat( "Error converting files: {0}", e );
                if( paramInfo.rethrow )
                    throw;
                return 1;
            }
            return 0;
        }

        private static void Usage()
        {
            Console.WriteLine( "\nUsage:\n" +
                              "--tranform options:                 Apply a transformation consisting of 16 float \n" +
                              "                                    arguments found after the --transform option\n" +
                              "\n" +
                              "--build_skeleton:                   Boolean option causing a skeleton to be built\n" +
                              "\n" +
                              "--base_skeleton skeletonfile:       Reads the base skeleton from the file\n" +
                              "\n" +
                              "--build_tangents:                   Build tangents\n" +
                              "\n" +
                              "--out_skeleton outputfile:          Build the new skeleton into the file\n" +
                              "\n" +
                              "--animation name file:              Add the animation name, from file file, to the\n" +
                              "                                    base animation file.\n" +
                              "\n" +
                              "--socket name parent tranform:      Adds an attachment point with the 'name' to the \n" +
                              "                                    parent bone 'parent', using the 16-float 'transform'\n" +
                              "\n" +
                              "--test_physics:                     Run a self-test of physics serializer code\n" +
                              "\n" +
                              "--manual_lod distance meshfile:     Set up manual levels of detail, using meshfile at distance\n" +
                              "\n" +
                              "--lod_levels lodlevels:             Generate lodlevels levels of detail\n" +
                              "\n" +
                              "--lod_distance distance:            Distance in millimeters between each level of detail\n" +
                              "\n" +
                              "--lod_percent lodpercent:           Degrade the model so that each level of detail has \n" +
                              "                                    lodpercent as many triangles as the previous level\n" +
                              "\n" +
                              "--src_dir directory:                Sets up the source directory from which meshes will be\n" +
                              "                                    read\n" +
                              "\n" +
                              "--dst_dir directory:                Sets up the target directory from which outputs will be\n" +
                              "                                    written\n" +
                              "\n" +
                              "--dont_extract_collision_volumes:   By default, the conversion tool will extract collision\n" +
                              "                                    volumes from submeshes whose names begin with mvcv_obb_,\n" +
                              "                                    mvcv_sphere_, mvcv_obb_ or mvcv_aabb, but that behavior\n" +
                              "                                    is suppressed with this option\n" +
                              "\n" +
                              "--log_collision_volumes:            Log the collision volume extraction process in the file\n" +
                              "                                    ExtractLog.txt\n" +
                              "\n" +
                              "--args_file argsfile:               Get additional arguments from the given file\n" +
                              "\n" +
                              "--log_file logfile:                 Log messages to the given file\n" +
                              "\n" +
                              "--version:                          Display version number\n" +
                              "\n" +
                              "--throw:                            Throw an exception on any fatal error\n" +
                              "\n");
        }


        /// <summary>
        ///   Loads up a mesh and the associated skeleton, and uses the 
        ///   skeleton to create a joint mesh.
        /// </summary>
        /// <param name="dstDir"></param>
        /// <param name="name"></param>
        private static void BuildSkeletonMesh( string dstDir, string name )
        {
            Mesh mesh = MeshManager.Instance.CreateBoneMesh( name );
            string meshFile = name + ".mesh";
            MeshSerializer meshWriter = new MeshSerializer();
            meshWriter.ExportMesh( mesh, dstDir + meshFile );
        }

        private static void Test( string srcDir, string dstDir, string name )
        {
            MeshSerializer meshReader = new MeshSerializer();
            Stream data = new FileStream( srcDir + name, FileMode.Open );
            // import the .mesh file
            Mesh mesh = new Mesh( "testmesh" );
            meshReader.ImportMesh( data, mesh );
            meshReader.ExportMesh( mesh, dstDir + name );
        }

        private static void TestPhysics( string srcDir, string dstDir, string name )
        {
            float OneMeter = 1000;
            PhysicsData pd = new PhysicsData();
            Vector3 center = new Vector3( 10 * OneMeter, 1 * OneMeter, 1 * OneMeter );
            Vector3[] obbAxes = new Vector3[ 3 ];
            obbAxes[ 0 ] = new Vector3( 1, 0, -1 );
            obbAxes[ 0 ].Normalize();
            obbAxes[ 1 ] = Vector3.UnitY;
            obbAxes[ 2 ] = new Vector3( 1, 0, 1 );
            obbAxes[ 2 ].Normalize();
            Vector3 obbExtents = new Vector3( 2.5f * OneMeter, 1 * OneMeter, 1 * OneMeter );
            CollisionOBB obb = new CollisionOBB( center, obbAxes, obbExtents );
            pd.AddCollisionShape( "submesh01", obb );
            CollisionAABB aabb = new CollisionAABB( center - obbExtents, center + obbExtents );
            pd.AddCollisionShape( "submesh01", aabb );
            CollisionSphere sphere = new CollisionSphere( center, 2 * OneMeter );
            pd.AddCollisionShape( "submesh01", sphere );
            Vector3 capExtents = new Vector3( -1 * OneMeter, 0, 0 );
            CollisionCapsule capsule = new CollisionCapsule( center + capExtents, center - capExtents, .5f * OneMeter );
            pd.AddCollisionShape( "submesh01", capsule );
            PhysicsSerializer ps = new PhysicsSerializer();
            ps.ExportPhysics( pd, name );

            pd = new PhysicsData();
            ps.ImportPhysics( pd, name );
            foreach( string objectId in pd.GetCollisionObjects() )
            {
                log.InfoFormat( "Collision shapes for: {0}", objectId );
                List<CollisionShape> collisionShapes = pd.GetCollisionShapes( objectId );
                foreach( CollisionShape shape in collisionShapes )
                    log.InfoFormat( "Shape: {0}", shape );
            }
        }

        private static Skeleton ReadSkeleton( string srcDir, string skelFile )
        {
            return ReadSkeleton( Matrix4.Identity, srcDir, skelFile );
        }

        private static Skeleton ReadSkeleton( Matrix4 transform, string srcDir, string skelFile )
        {
            Stream skelData = new FileStream( srcDir + skelFile, FileMode.Open );
            Skeleton skeleton = new Skeleton( skelFile );
            if( skelFile.EndsWith( ".skeleton" ) )
            {
                OgreSkeletonSerializer skelReader = new OgreSkeletonSerializer();
                skelReader.ImportSkeleton( skelData, skeleton );
            }
            else if( skelFile.EndsWith( ".skeleton.xml" ) )
            {
                OgreXmlSkeletonReader skelReader = new OgreXmlSkeletonReader( skelData );
                skelReader.Import( skeleton );
            }
            else
            {
                skelData.Close();
                string extension = Path.GetExtension( skelFile );
                throw new AxiomException( "Unsupported skeleton format '{0}'", extension );
            }
            skelData.Close();
            return skeleton;
        }

        private static Mesh ReadMesh( string srcDir, string dstDir, string meshFile )
        {
            string dummyMaterialScript = null;
            return ReadMesh( ref dummyMaterialScript, Matrix4.Identity, srcDir, dstDir, meshFile );
        }

        private static Mesh ReadMesh( ref string materialScript, string srcDir, string dstDir, string meshFile )
        {
            return ReadMesh( ref materialScript, Matrix4.Identity, srcDir, dstDir, meshFile );
        }

        private static Mesh ReadMesh( ref string materialScript, Matrix4 transform, string srcDir, string dstDir, string meshFile )
        {
            FileStream meshData = new FileStream( srcDir + meshFile, FileMode.Open );
            Mesh mesh = new Mesh( meshFile );
            if( meshFile.EndsWith( ".mesh", StringComparison.CurrentCultureIgnoreCase ) )
            {
                MeshSerializer meshReader = new MeshSerializer();
                meshReader.ImportMesh( meshData, mesh );
            }
            else if( meshFile.EndsWith( ".mesh.xml", StringComparison.CurrentCultureIgnoreCase ) )
            {
                OgreXmlMeshReader meshReader = new OgreXmlMeshReader( meshData );
                meshReader.Import( mesh );
            }
            else if( meshFile.EndsWith( ".dae", StringComparison.CurrentCultureIgnoreCase ) )
            {
                string extension = Path.GetExtension( meshFile );
                string baseFile = Path.GetFileNameWithoutExtension( meshFile );
                string basename = meshFile.Substring( 0, meshFile.Length - extension.Length );
                ColladaMeshReader meshReader = new ColladaMeshReader( meshData, baseFile );
                // import the .dae file
                meshReader.Import( transform, mesh, null, "base", basename );
                log.Info( "Optimizing mesh to reduce vertex buffer size" );
                materialScript = meshReader.MaterialScript;
            } else if (meshFile.EndsWith(".kmz", StringComparison.CurrentCultureIgnoreCase)) {
                ImportKMZFile( ref materialScript, transform, dstDir, meshFile, meshData, mesh );
            }
            else
            {
                meshData.Close();
                string extension = Path.GetExtension( meshFile );
                throw new AxiomException( "Unsupported mesh format '{0}'", extension );
            }
            meshData.Close();
            return mesh;
        }

        #region KMZ (SketchUp) support

        // mesh will be null if this fails
        static void ImportKMZFile( 
            ref string materialScript, Matrix4 transform,
            string dstDir, string meshFile, 
            FileStream kmzStream, Mesh mesh )
        {
            string daeFile = ExtractKMZComponentFiles( dstDir, kmzStream );

            if( ! String.IsNullOrEmpty( daeFile ) )
            {
                string materialNamespace  = Path.GetFileNameWithoutExtension( daeFile );
                string animationNamespace = materialNamespace;

                FileStream daeStream = new FileStream( daeFile, FileMode.Open );

                ColladaMeshReader meshReader = new ColladaMeshReader( daeStream, animationNamespace );
                
                // Convert from a left handed system where z is up to a right handed system where y is up.
                Matrix4 yupTrans = new Matrix4( 1, 0, 0, 0, 0, 0, 1, 0, 0, -1, 0, 0, 0, 0, 0, 1 );
                Matrix4 yupTransform = transform * yupTrans;
                
                // import the .dae file
                meshReader.Import( yupTransform, mesh, null, "base", materialNamespace );
                
                materialScript = meshReader.MaterialScript;
            }
        }

        // return absolute path to the dae file
        private static string ExtractKMZComponentFiles( string dstDir, FileStream kmzStream )
        {
            string daeFile = String.Empty;

            ZipFile zipFile = new ZipFile( kmzStream );

            foreach( ZipEntry anEntry in zipFile )
            {
                string newFile = ExtractKMZEntry( zipFile.GetInputStream( anEntry ), anEntry, dstDir );

                if( newFile.EndsWith( ".dae", StringComparison.CurrentCultureIgnoreCase ) )
                {
                    daeFile = newFile;
                }
            }

            if( String.IsNullOrEmpty( daeFile ) )
            {
                log.Warn( "No Collada model data found in file." );
            }

            zipFile.Close();

            return daeFile;
        }

        // return the absolute path to the file created
        private static string ExtractKMZEntry( Stream inputStream, ZipEntry zipEntry, string dstDir )
        {
            FileInfo target = new FileInfo( Path.Combine( dstDir, zipEntry.Name ) );

            if( ! target.Directory.Exists )
            {
                target.Directory.Create();
            }

            if( target.Directory.Name.EndsWith( "image", StringComparison.CurrentCultureIgnoreCase ) )
            {
                ILImageCodec.ReshapeToPowersOf2AndSave( inputStream, (int) zipEntry.Size, target.FullName );
            }
            else
            {
                WriteStreamToFile( inputStream, target );
            }

            return target.FullName;
        }

        private static void WriteStreamToFile( Stream sourceStream, FileInfo target )
        {
            FileStream destination = target.OpenWrite();

            int Length = 256;
            Byte[] buffer = new Byte[ Length ];
            int bytesRead = sourceStream.Read( buffer, 0, Length );

            // write the required bytes
            while( bytesRead > 0 )
            {
                destination.Write( buffer, 0, bytesRead );

                bytesRead = sourceStream.Read( buffer, 0, Length );
            }

            destination.Close();
        }

        #endregion KMZ (SketchUp) support

        private static void SplitDrive( ref string drive, ref string path, string pathname )
        {
            if( pathname.Length > 2 && pathname[ 1 ] == ':' )
            {
                drive = pathname.Substring( 0, 2 );
                path = pathname.Substring( 2 );
            }
            else
            {
                drive = string.Empty;
                path = pathname;
            }
        }
        private static string NormalizePath( string filename )
        {
            filename = filename.Replace( '/', '\\' );
            string prefix = string.Empty;
            string path = string.Empty;
            SplitDrive( ref prefix, ref path, filename );
            if( prefix == string.Empty )
            {
                // No drive letter - preserve initial backslashes
                while( path.Length > 0 && path[ 0 ] == '\\' )
                {
                    prefix = prefix + "\\";
                    path = path.Substring( 1 );
                }
            }
            else
            {
                // We have a drive letter - collapse initial backslashes
                if( path.StartsWith( "\\" ) )
                {
                    prefix = prefix + '\\';
                    while( path.StartsWith( "\\" ) )
                        path = path.Substring( 1 );
                }
            }
            string[] comps = path.Split( '\\' );
            List<string> newComps = new List<string>();
            foreach( string comp in comps )
            {
                if( comp == "." || comp == string.Empty )
                {
                    continue;
                }
                else if( comp == ".." )
                {
                    if( newComps.Count > 0 && newComps[ newComps.Count - 1 ] != ".." )
                    {
                        newComps.RemoveAt( newComps.Count - 1 );
                        continue;
                    }
                    else if( newComps.Count == 0 && prefix.EndsWith( "\\" ) )
                    {
                        continue;
                    }
                    else
                    {
                        newComps.Add( comp );
                    }
                }
                else
                {
                    newComps.Add( comp );
                }
            }
            if( prefix == string.Empty && newComps.Count == 0 )
                newComps.Add( "." );
            string rv = prefix;
            for( int i = 0; i < newComps.Count; ++i )
            {
                if( i == newComps.Count - 1 )
                    // last entry
                    rv = rv + newComps[ i ];
                else
                    rv = rv + newComps[ i ] + "\\";
            }
            return rv;
        }
        private static void SplitPath( ref string dir, ref string path, string pathname )
        {
            pathname = NormalizePath( pathname );
            int i = pathname.LastIndexOf( '\\' );
            if( i < 0 )
            {
                dir = string.Empty;
                path = pathname;
            }
            else
            {
                dir = pathname.Substring( 0, i + 1 );
                path = pathname.Substring( i + 1 );
            }
        }
        private static void SetupManualLodLevels( string srcDir, string dstDir, string name,
                                                 List<LodEntry> lodEntries )
        {
            string dir = string.Empty;
            string path = string.Empty;
            SplitPath( ref dir, ref path, name );
            if( srcDir == string.Empty )
                srcDir = dir;
            if( dstDir == string.Empty )
                dstDir = dir;
            name = path;
            // get the resource data from MeshManager
            string extension = Path.GetExtension( name ).ToLower();

            Mesh mesh = ReadMesh( srcDir, dstDir, name );
            if( mesh.LodLevelCount > 1 )
            {
                log.Warn( "Mesh already contains level of detail information" );
                mesh.RemoveLodLevels();
            }
            List<MeshLodUsage> manualLodList = new List<MeshLodUsage>();
            foreach( LodEntry lodEntry in lodEntries )
            {
                Mesh lodMesh = ReadMesh( dstDir, dstDir, lodEntry.meshFile );
                MeshLodUsage lodUsage = new MeshLodUsage();
                lodUsage.fromSquaredDepth = lodEntry.distance * lodEntry.distance;
                lodUsage.manualMesh = lodMesh;
                lodUsage.manualName = lodMesh.Name;
                manualLodList.Add( lodUsage );
            }
            mesh.AddManualLodEntries( manualLodList );

            MeshSerializer meshWriter = new MeshSerializer();
            meshWriter.ExportMesh( mesh, dstDir + name );
        }

        private static void GenerateLodLevels( string srcDir, string dstDir, string name,
                                              int lodlevels, float loddist, float lodpercent )
        {
            string dir = string.Empty;
            string path = string.Empty;
            SplitPath( ref dir, ref path, name );
            if( srcDir == string.Empty )
                srcDir = dir;
            if( dstDir == string.Empty )
                dstDir = dir;
            name = path;
            // get the resource data from MeshManager
            string extension = Path.GetExtension( name ).ToLower();
            string materialScript = null;

            Mesh mesh = ReadMesh( ref materialScript, Matrix4.Identity, srcDir, dstDir, name );
            if( mesh.LodLevelCount > 1 )
            {
                log.Warn( "Mesh already contains level of detail information" );
                mesh.RemoveLodLevels();
            }
            List<float> lodDistanceList = new List<float>();
            for( int i = 0; i < lodlevels; ++i )
                lodDistanceList.Add( loddist * (i + 1) );
            mesh.GenerateLodLevels( lodDistanceList, ProgressiveMesh.VertexReductionQuota.Proportional, lodpercent );

            MeshSerializer meshWriter = new MeshSerializer();
            meshWriter.ExportMesh( mesh, dstDir + name );
        }

        private static void ConvertFile( string srcDir, string dstDir, string name,
                                        Matrix4 transform, bool build_tangents,
                                        bool extract_collision_volumes, bool optimize_mesh,
                                        string skeleton_file )
        {
            if( String.IsNullOrEmpty( name ) )
            {
                // TODO: It would be better to catch this while parsing command args, but
                // that's a bit too hairy for now. This will at least inform the user.
                throw new ArgumentException( "No file named for conversion" );
            }

            string dir = string.Empty;
            string path = string.Empty;
            SplitPath( ref dir, ref path, name );
            if( srcDir == string.Empty )
                srcDir = dir;
            if( dstDir == string.Empty )
                dstDir = dir;
            name = path;
            // get the resource data from MeshManager
            string extension = Path.GetExtension( name ).ToLower();

            string baseFile = Path.GetFileNameWithoutExtension( name );
            if( baseFile.EndsWith( ".mesh" ) )
                baseFile = baseFile.Substring( 0, baseFile.Length - 5 );

            string baseSkeletonName = null;
            if( skeleton_file != null )
                baseSkeletonName = Path.GetFileName( skeleton_file );

            // mesh loading stats
            int before, after;

            // get the tick count before loading the mesh
            before = Environment.TickCount;

            string materialScript = null;
            Mesh mesh = ReadMesh( ref materialScript, transform, srcDir, dstDir, name );
            if( optimize_mesh )
                mesh = MeshUtility.CopyMesh( mesh );

            // get the tick count after loading the mesh
            after = Environment.TickCount;

            // record the time elapsed while loading the mesh
            log.InfoFormat( "Mesh: Loaded '{0}', took {1}ms", mesh.Name, (after - before) );

            // Build tangent vectors
            if( build_tangents )
            {
                log.Info( "Building tangent vectors from uv map" );
                MeshHelper.BuildTangentVectors( mesh );
            }

            if( extract_collision_volumes )
            {
                log.InfoFormat( "Extracting collision volumes from '{0}'", mesh.Name );
                CVExtractor.ExtractCollisionShapes( mesh, dstDir + baseFile );
            }

            //// prepare the mesh for a shadow volume?
            //if (MeshManager.Instance.PrepareAllMeshesForShadowVolumes) {
            //    if (edgeListsBuilt || autoBuildEdgeLists) {
            //        PrepareForShadowVolume();
            //    }
            //    if (!edgeListsBuilt && autoBuildEdgeLists) {
            //        BuildEdgeList();
            //    }
            //}

            // Allow them to override the skeleton reference of the mesh
            if( baseSkeletonName != null )
                mesh.SkeletonName = baseSkeletonName;

            string meshFile = baseFile + ".mesh";
            MeshSerializer meshWriter = new MeshSerializer();
            meshWriter.ExportMesh( mesh, dstDir + meshFile );

            // If it was a .dae file, we will need to export the material and skeleton as well
            if( extension != ".dae" && extension != ".kmz" )
                return;

            if( materialScript != null )
            {
                string materialFile = baseFile + ".material";
                Stream materialData = new FileStream( dstDir + materialFile, FileMode.Create );
                StreamWriter materialWriter = new StreamWriter( materialData );
                materialWriter.Write( materialScript );
                materialWriter.Close();
            }

            if( mesh.Skeleton == null )
                return;
#if USE_XML
            string skelFile = baseFile + ".skeleton.xml";
            Stream skelData = new FileStream(dstDir + skelFile, FileMode.Create);
            OgreXmlSkeletonWriter skelWriter = new OgreXmlSkeletonWriter(skelData);
            skelWriter.Export(mesh.Skeleton);
            skelData.Close();
#else
            // DEBUG
            foreach( AttachmentPoint socket in mesh.Skeleton.AttachmentPoints )
            {
                log.InfoFormat( "Created attachment point with parent {0}", socket.ParentBone );
                log.InfoFormat( "  Relative Position: {0}", socket.Position );
                log.InfoFormat( "  Relative Up: {0}", socket.Orientation * Vector3.UnitZ );
                Bone bone = mesh.Skeleton.GetBone( socket.ParentBone );
                Vector3 derivedPos = bone.DerivedPosition + socket.Position;
                Vector3 derivedUp = socket.Orientation * bone.DerivedOrientation * Vector3.UnitZ;
                log.InfoFormat( "  Absolute Position: {0}", derivedPos );
                log.InfoFormat( "  Absolute Up: {0}", derivedUp );
            }

            string skelFile = baseFile + ".skeleton";
            OgreSkeletonSerializer skelWriter = new OgreSkeletonSerializer();
            skelWriter.ExportSkeleton( mesh.Skeleton, dstDir + skelFile );
#endif
        }

        /// <summary>
        ///   Utility method to add attachment points to an existing skeleton
        /// </summary>
        /// <param name="dstDir">the directory to which the modified skeleton will be saved</param>
        /// <param name="skelFile">the name of the file to which the modified skeleton will be written</param>
        /// <param name="skeleton">the original skeleton</param>
        /// <param name="attachPoints">the list of attachment points</param>
        private static void AddAttachments( string dstDir,
                                           string skelFile,
                                           Skeleton skeleton,
                                           List<AttachmentPointNode> attachPoints )
        {
            if( skeleton.AttachmentPoints.Count > 0 &&
                attachPoints.Count != skeleton.AttachmentPoints.Count )
                log.WarnFormat( "Skeleton attachment points count ({0}) does not match new count ({1})",
                                skeleton.AttachmentPoints.Count, attachPoints.Count );
            foreach( AttachmentPointNode attachPoint in attachPoints )
            {
                Quaternion rotate;
                Vector3 translate, scale;
                Matrix4 transform = attachPoint.Transform;
                Matrix4.DecomposeMatrix( ref transform, out translate, out rotate, out scale );
                Bone parentBone = skeleton.GetBone( attachPoint.ParentBone );
                bool isDup = false;
                foreach( AttachmentPoint tmp in skeleton.AttachmentPoints )
                    if( tmp.Name == attachPoint.Name )
                        isDup = true;
                if( isDup )
                    continue;
                skeleton.CreateAttachmentPoint( attachPoint.Name, parentBone.Handle, rotate, translate );
            }
            OgreSkeletonSerializer skelWriter = new OgreSkeletonSerializer();
            skelWriter.ExportSkeleton( skeleton, dstDir + skelFile );
        }

        /// <summary>
        ///   Utility method to merge animations from other files into a single skeleton
        /// </summary>
        /// <param name="srcDir">the directory from which the new animations will be loaded</param>
        /// <param name="dstDir">the directory to which the modified skeleton will be saved</param>
        /// <param name="skelFile">the name of the file to which the modified skeleton will be written</param>
        /// <param name="transform">the transform to apply to the skeleton and animations</param>
        /// <param name="skeleton">the original skeleton</param>
        /// <param name="animations">the list of animations</param>
        private static void AddAnimations( string srcDir, string dstDir,
                                        string skelFile,
                                        Matrix4 transform, Skeleton skeleton,
                                        List<AnimationEntry> animations )
        {
            // mesh loading stats
            int before, after;

            // get the tick count before loading the mesh
            before = Environment.TickCount;

            foreach( AnimationEntry entry in animations )
            {
                Mesh mesh = new Mesh( "Mesh" );
                Stream data = new FileStream( srcDir + entry.animation_file, FileMode.Open );
                ColladaMeshReader meshReader = new ColladaMeshReader( data, null );
                // import the .dae file
                meshReader.Import( transform, mesh, skeleton, entry.animation_name, null );
                // close the stream (we don't need to leave it open here)
                data.Close();
            }

            // get the tick count after loading the mesh
            after = Environment.TickCount;

            // record the time elapsed while loading the mesh
            log.InfoFormat( "Mesh: took {0}ms", (after - before) );

            //// prepare the mesh for a shadow volume?
            //if (MeshManager.Instance.PrepareAllMeshesForShadowVolumes) {
            //    if (edgeListsBuilt || autoBuildEdgeLists) {
            //        PrepareForShadowVolume();
            //    }
            //    if (!edgeListsBuilt && autoBuildEdgeLists) {
            //        BuildEdgeList();
            //    }
            //}

            OgreSkeletonSerializer skelWriter = new OgreSkeletonSerializer();
            skelWriter.ExportSkeleton( skeleton, dstDir + skelFile );
        }
#if NOT_USED
		private static void MergeAnimations(string srcDir, string dstDir, 
											string destFile, string rigFile, 
											List<string> animFiles) {
			XmlDocument outputDoc = new XmlDocument();

			XmlNode animationsNode = outputDoc.CreateElement("animations");
			foreach (string animFile in animFiles) {
				Stream animData = new FileStream(srcDir + animFile, FileMode.Open);
				XmlDocument animInputDoc = new XmlDocument();
				animInputDoc.Load(animData);
				animData.Close();
				XmlNode node = animInputDoc.ChildNodes[0];
				Debug.Assert(node.Name == "skeleton");
				foreach (XmlNode childNode in node.ChildNodes) {
					if (childNode.Name == "animations") {
						foreach (XmlNode childNode2 in childNode.ChildNodes) {
							if (childNode2.Name == "animation") {
								XmlNode newNode = outputDoc.ImportNode(childNode2, true);
								animationsNode.AppendChild(newNode);
							}
						}
					}
				}
			}
			
			XmlNode skeletonNode = outputDoc.CreateElement("skeleton");

			Stream rigData = new FileStream(srcDir + rigFile, FileMode.Open);
			XmlDocument rigInputDoc = new XmlDocument();
			rigInputDoc.Load(rigData);
			rigData.Close();

			XmlNode firstRigNode = rigInputDoc.ChildNodes[0];
			Debug.Assert(firstRigNode.Name == "skeleton");
			foreach (XmlNode childNode in firstRigNode.ChildNodes) {
				if (childNode.Name == "bones" || childNode.Name == "bonehierarchy") {
					XmlNode newNode = outputDoc.ImportNode(childNode, true);
					skeletonNode.AppendChild(newNode);
				}
			}

			skeletonNode.AppendChild(animationsNode);
			outputDoc.AppendChild(skeletonNode);

			Stream destData = new FileStream(dstDir + destFile, FileMode.Create);
			outputDoc.Save(destData);
			destData.Close();
		}

		private static void MergeColladaFiles(string srcDir, string dstDir, 
											  string destFile, string rigFile, 
											  string animFile) {
			XmlDocument outputDoc = new XmlDocument();
			XmlNode combinationNode = outputDoc.CreateElement("COLLADA");

			// Load the rig data
			Stream rigData = new FileStream(srcDir + rigFile, FileMode.Open);
			XmlDocument rigInputDoc = new XmlDocument();
			rigInputDoc.Load(rigData);
			rigData.Close();

			Debug.Assert(rigInputDoc.ChildNodes.Count == 2);
			XmlNode rigXmlNode = rigInputDoc.ChildNodes[0];
			Debug.Assert(rigXmlNode.Name == "xml");
			XmlNode rigColladaNode = rigInputDoc.ChildNodes[1];
			Debug.Assert(rigColladaNode.Name == "COLLADA");
			foreach (XmlNode childNode in rigColladaNode.ChildNodes) {
				if (childNode.Name == "asset") {
					XmlNode newNode = outputDoc.ImportNode(childNode, true);
					combinationNode.AppendChild(newNode);
				} else if (childNode.Name == "library") {
					Debug.Assert(childNode.Attributes["type"] != null);
					if (childNode.Attributes["type"].Value == "ANIMATION") {
						continue;
					} else {
						XmlNode newNode = outputDoc.ImportNode(childNode, true);
						combinationNode.AppendChild(newNode);
					}
				} else if (childNode.Name == "scene") {
					continue;
				} else {
					Console.WriteLine("Skipping unknown node: " + childNode.Name);
				}
			}

			// Load the animation data
			Stream animData = new FileStream(srcDir + animFile, FileMode.Open);
			XmlDocument animInputDoc = new XmlDocument();
			animInputDoc.Load(animData);
			animData.Close();

			Debug.Assert(animInputDoc.ChildNodes.Count == 2);
			XmlNode animXmlNode = animInputDoc.ChildNodes[0];
			Debug.Assert(animXmlNode.Name == "xml");
			XmlNode animColladaNode = animInputDoc.ChildNodes[1];
			Debug.Assert(animColladaNode.Name == "COLLADA");
			foreach (XmlNode childNode in animColladaNode.ChildNodes) {
				if (childNode.Name == "asset") {
					continue;
				} else if (childNode.Name == "library") {
					Debug.Assert(childNode.Attributes["type"] != null);
					if (childNode.Attributes["type"].Value == "ANIMATION") {
						XmlNode newNode = outputDoc.ImportNode(childNode, true);
						combinationNode.AppendChild(newNode);
					} else {
						continue;
					}
				} else if (childNode.Name == "scene") {
					XmlNode newNode = outputDoc.ImportNode(childNode, true);
					combinationNode.AppendChild(newNode);
				} else {
					Console.WriteLine("Skipping unknown node: " + childNode.Name);
				}
			}

			// Copy the xml header from the input doc to the output doc
			XmlNode xmlHeaderNode = outputDoc.ImportNode(rigXmlNode, true);
			outputDoc.AppendChild(xmlHeaderNode);

			// Now add the collada portion with rig data from one file, and 
			// animation data from another.
			outputDoc.AppendChild(combinationNode);

			Stream destData = new FileStream(dstDir + destFile, FileMode.Create);
			outputDoc.Save(destData);
			destData.Close();
		}
#endif
    }
}
