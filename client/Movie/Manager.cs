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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using Axiom.Core;
using Axiom.Graphics;
using Axiom.Media;
using Multiverse.Base;
using Multiverse.Interface;
using Multiverse.Lib.LogUtil;

namespace Multiverse.Movie
{
    /// <summary>
    /// A movie codec is the code that decompresses the movie file
    /// into individual frames, and places them on the target texture.
    /// The codec should implement the IPlugin.Start() method and use
    /// that to register itself to the movie manager.  There should be
    /// one codec per movie player type, but the codec itself can 
    /// spawn an arbitrary number of movies.
    /// </summary>
    public interface ICodec
    {
        /// <summary>
        /// Called from the manager's plugin start method, to avoid 
        /// dependency issues.
        /// </summary>
        void Start();

        /// <summary>
        /// Called from the manager's plugin stop method, to avoid
        /// dependency issues.
        /// </summary>
        void Stop();

        /// <summary>
        /// Each codec should have a string name that will be used
        /// to identify it generically (without API specifics.)
        /// </summary>
        /// <returns>
        /// The string name of the codec.
        /// </returns>
        string Name();

        /// <summary>
        /// Check to see whether this codec understands the given string 
        /// parameter, and whether the given value is valid.
        /// </summary>
        /// <param name="name">
        /// The name of the parameter to check.
        /// </param>
        /// <param name="val">
        /// The intended value to set.
        /// </param>
        /// <returns></returns>
        bool ValidateParameter(string name, string val);

        /// <summary>
        /// Creates a delay-loaded movie texture object.  This texture will 
        /// likely need to override the platform-specific Texture class and
        /// use its Load() method to create itself only when the texture is
        /// in use.  
        /// 
        /// If the texture already exists, it will return null.  The movie
        /// texture source object should check for duplicate names and only
        /// return a single movie texture.
        /// </summary>
        /// <param name="name">
        /// The name of the texture to create, which will later be referred
        /// to in loading operations.
        /// </param>
        /// <returns>The movie texture object, or null if the operation failed.</returns>
        IMovieTexture CreateMovieTexture(string name);

        /// <summary>
        /// Load a movie file from the Media/Movies directory.
        /// </summary>
        /// <param name="name">
        /// A unique name for this movie.
        /// </param>
        /// <param name="file">
        /// The filename (not the full path) of this movie, in the Movies directory.
        /// </param>
        /// <returns>
        /// An IMovie object for the movie, or null if the load failed.
        /// </returns>
        IMovie LoadFile(string name, string file);

        /// <summary>
        /// Load a movie file from the Media/Movies directory.
        /// </summary>
        /// <param name="name">
        /// A unique name for this movie.
        /// </param>
        /// <param name="file">
        /// The filename (not the full path) of this movie, in the Movies directory.
        /// </param>
        /// <param name="textureName">
        /// The name of the texture we should display to, which will be created
        /// if it doesn't exist.
        /// </param>
        /// <returns>
        /// An IMovie object for the movie, or null if the load failed.
        /// </returns>
        IMovie LoadFile(string name, string file, string textureName);

        /// <summary>
        /// Load a movie from the Internet.
        /// </summary>
        /// <param name="name">
        /// A unique name for this stream
        /// </param>
        /// <param name="url">
        /// The URL to the stream
        /// </param>
        /// <returns>
        /// An IMovie object for the movie, or null if the load failed.
        /// </returns>
        IMovie LoadStream(string name, string url);

        /// <summary>
        /// Load a movie from the Internet.
        /// </summary>
        /// <param name="name">
        /// A unique name for this stream
        /// </param>
        /// <param name="url">
        /// The URL to the stream
        /// </param>
        /// <param name="textureName">
        /// The name of the texture we should display to, which will be created
        /// if it doesn't exist.
        /// </param>
        /// <returns>
        /// An IMovie object for the movie, or null if the load failed.
        /// </returns>
        IMovie LoadStream(string name, string url, string textureName);

        /// <summary>
        /// Check to see if this codec has a movie with the given name.
        /// </summary>
        /// <param name="name">The name to search for</param>
        /// <returns>
        /// The movie if it was found, or null if it's not in this codec.
        /// </returns>
        IMovie FindMovie(string name);

        /// <summary>
        /// Stop and unload all the running movies for this codec.
        /// </summary>
        void UnloadAll();

        /// <summary>
        /// Stop a movie from playing and free its resources.
        /// </summary>
        /// <param name="im">The movie to destroy</param>
        /// <returns>True if this movie was owned by this codec, false if not.</returns>
        bool UnloadMovie(IMovie im);
    }

    /// <summary>
    /// The IMovie object is the representation of a movie that's loaded 
    /// and playing.  It also responds to play, pause, stop, and volume
    /// commands.
    /// </summary>
    public interface IMovie
    {
        /// <summary>
        /// The name of the codec used to create this movie.
        /// </summary>
        /// <returns>
        /// The string name.
        /// </returns>
        string CodecName();

        /// <summary>
        /// The name of this movie, currently the file name.
        /// </summary>
        /// <returns>
        /// The movie file name.
        /// </returns>
        string Name();

        /// <summary>
        /// The full path to this movie.
        /// </summary>
        /// <returns>
        /// The full path - in Windows form if it's a file, or a URL if it's a stream.
        /// </returns>
        string Path();

        /// <summary>
        /// The name of the texture we're displaying on.
        /// </summary>
        /// <returns>
        /// Aforementioned name
        /// </returns>
        string TextureName();

        /// <summary>
        /// A unique identifier for this movie.  This number should 
        /// be generated by a call to Manager.GetNewIdentifier()
        /// </summary>
        /// <returns>
        /// The movie's unique ID.
        /// </returns>
        int ID();

        /// <summary>
        /// How large the movie itself is, in pixels.
        /// </summary>
        /// <returns>
        /// The movie's width and height in pixels.
        /// </returns>
        Size VideoSize();

        /// <summary>
        /// The texture size is likely to be different from the movie
        /// size, because it will probably be rounded up to the next
        /// power of two.
        /// </summary>
        /// <returns>
        /// The width and height in pixels of the texture that this
        /// movie targets.
        /// </returns>
        Size TextureSize();

        /// <summary>
        /// Gets the Axiom texture object we're displaying the movie to.
        /// </summary>
        /// <returns>
        /// The Texture object this movie is playing on, which is registered
        /// with the Axiom TextureManager.
        /// </returns>
        Axiom.Core.Texture Texture();

        /// <summary>
        /// Gets the name of an alternate image to display when the movie 
        /// isn't being displayed.  This image will be automatically displayed 
        /// when the movie ends.
        /// </summary>
        /// <returns>
        /// The name of the image in the Textures directory.
        /// </returns>
        string AltImage();

        /// <summary>
        /// Sets the name of an alternate image to display when the movie 
        /// isn't being displayed.  This image will be automatically displayed
        /// when the movie ends.
        /// </summary>
        /// <param name="image">
        /// The name of the image in the Textures directory.
        /// </param>
        void SetAltImage(string image);

        /// <summary>
        /// Bring up the alternate image for the movie.  Note that this will
        /// not work if the movie is playing; it should be stopped.
        /// </summary>
        /// <returns>
        /// True if the alt image was displayed, false if not.
        /// </returns>
        bool ShowAltImage();

        /// <summary>
        /// Unloads the alt image from display.  This will unload the texture 
        /// itself, so make sure there is something else to take its place.
        /// </summary>
        /// <returns>
        /// True if the alt image was removed, false if nothing was done.
        /// </returns>
        bool HideAltImage();

        /// <summary>
        /// Completely replace an entity in the scene, keeping its position,
        /// orientation, and scale, but replacing it with a movie width by movie
        /// height sized plane to play the movie on.
        /// </summary>
        /// <param name="name">
        /// The name of the world object to replace in the scene.  Replaces the
        /// mesh, material, and texture of the object.
        /// </param>
        /// <returns></returns>
        bool ReplaceWorldObject(string name);

        /// <summary>
        /// Adjusts the texture coordinates (via texture matrix) of any texture 
        /// in the material that refers to this movie.
        /// </summary>
        /// <param name="material">
        /// The name of the material to search.
        /// </param>
        /// <returns>
        /// True if a texture unit state was changed, false if not.
        /// </returns>
        bool SetTextureCoordinates(string material);

        /// <summary>
        /// Start the movie playing, continuing play until the end.  May pause 
        /// briefly to buffer.
        /// </summary>
        /// <returns>
        /// True if the movie was able to start playing, or false if an error 
        /// occurred.
        /// </returns>
        bool Play();

        /// <summary>
        /// Temporarily suspend playback on the current frame.  Decoding and 
        /// buffering continues to occur, but the display is frozen in position.
        /// </summary>
        /// <returns>
        /// True if the movie paused, or false if it didn't due to an error.
        /// </returns>
        bool Pause();

        /// <summary>
        /// Halt playback of this movie and reset the position to the start of
        /// the movie.  Buffering and decoding will also cease.
        /// </summary>
        /// <returns>
        /// True if the movie stopped, or false if it didn't due to an error.
        /// </returns>
        bool Stop();

        /// <summary>
        /// Immediately free any of the resources we hold.
        /// </summary>
        void Unload();

        /// <summary>
        /// Generic parameter setter, using strings as the transport.  Each codec
        /// should determine which parameters make sense for the given video type
        /// and override the ValidateParameter() method to test whether or not the
        /// codec understands it.
        /// </summary>
        /// <param name="name">
        /// The name of the parameter to set (e.g. "looping")
        /// </param>
        /// <param name="value">
        /// The value of the parameter to set (e.g. "true")
        /// </param>
        /// <returns>
        /// True if the movie was able to set the parameter, false if not.
        /// </returns>
        bool SetParameter(string name, string value);
    }

    /// <summary>
    /// The Movie Manager handles keeping track of all the codecs, and 
    /// serves as a base class for shared functionality, like generating
    /// a plane for the scene.
    /// </summary>
    public class Manager : IPlugin
    {
        #region Fields

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(Manager));

        /// <summary>
        /// The singleton instance of the movie manager, created on plugin load.
        /// </summary>
        public static Manager Instance = null;

        /// <summary>
        /// The list of all available codecs in the system, which should implement
        /// the IPlugin interface and add themselves to this list when they are
        /// loaded.
        /// </summary>
        public static List<ICodec> Codecs = new List<ICodec>();

        #endregion

        #region Generated names
        /// <summary>
        /// We use a simple movie identifier system for now.
        /// </summary>
        private static int movieIndex = 0;

        /// <summary>
        /// The base of the generated names for the movie material.
        /// </summary>
        private const string baseMaterialName = "MV_PRIVATE_MATERIAL";
        /// <summary>
        /// The base of the generated names for the movie mesh.
        /// </summary>
        private const string baseMeshName = "MV_PRIVATE_MESH";
        /// <summary>
        /// The base of the generated names for the movie texture.
        /// </summary>
        private const string baseTextureName = "MV_PRIVATE_TEXTURE";

        /// <summary>
        /// If we create a material for this movie, return what that
        /// generated name should be.
        /// </summary>
        /// <param name="movie">
        /// The movie to create a material name for.
        /// </param>
        /// <returns>
        /// A string name for the material.
        /// </returns>
        public static string MaterialName(IMovie movie)
        {
            return MaterialName(movie.CodecName(), movie.ID());
        }
        /// <summary>
        /// If we create a material for this movie, return what that
        /// generated name should be.
        /// </summary>
        /// <param name="codec">
        /// The codec that's playing the movie.
        /// </param>
        /// <param name="id">
        /// The int ID of the movie.
        /// </param>
        /// <returns>
        /// A string name for the material.
        /// </returns>
        public static string MaterialName(string codec, int id)
        {
            return baseMaterialName + "_" + codec + "_" + id;
        }

        /// <summary>
        /// If we create a mesh for this movie, return what that
        /// generated name should be.
        /// </summary>
        /// <param name="movie">
        /// The movie to create a mesh name for.
        /// </param>
        /// <returns>
        /// A string name for the mesh.
        /// </returns>
        public static string MeshName(IMovie movie)
        {
            return MeshName(movie.CodecName(), movie.ID());
        }
        /// <summary>
        /// If we create a mesh for this movie, return what that
        /// generated name should be.
        /// </summary>
        /// <param name="codec">
        /// The codec that's playing the movie.
        /// </param>
        /// <param name="id">
        /// The int ID of the movie.
        /// </param>
        /// <returns>
        /// A string name for the mesh.
        /// </returns>
        public static string MeshName(string codec, int id)
        {
            return baseMeshName + "_" + codec + "_" + id;
        }

        /// <summary>
        /// If we create a texture for this movie, return what that
        /// generated name should be.
        /// </summary>
        /// <param name="movie">
        /// The movie to create a texture name for.
        /// </param>
        /// <returns>
        /// A string name for the texture.
        /// </returns>
        public static string TextureName(IMovie movie)
        {
            return TextureName(movie.CodecName(), movie.ID());
        }
        /// <summary>
        /// If we create a texture for this movie, return what that
        /// generated name should be.
        /// </summary>
        /// <param name="codec">
        /// The codec that's playing the movie.
        /// </param>
        /// <param name="id">
        /// The int ID of the movie.
        /// </param>
        /// <returns>
        /// A string name for the texture.
        /// </returns>
        public static string TextureName(string codec, int id)
        {
            return baseTextureName + "_" + codec + "_" + id;
        }
        #endregion

        #region IPlugin methods
        /// <summary>
        /// Initialize the singleton member and wait for the interpreter to
        /// load to reflect ourselves.
        /// </summary>
        public void Start()
        {
            Instance = this;
            ExternalTextureSourceManager.Instance.SetExternalTextureSource(
                MovieTextureSource.MV_SOURCE_NAME, new MovieTextureSource());

            Assembly assembly = Assembly.GetAssembly(typeof(Manager));
            foreach (Type type in assembly.GetTypes())
            {
                if ((type.GetInterface("ICodec") == typeof(ICodec)) && (!type.IsInterface))
                {
                    try
                    {
                        ICodec codec = (ICodec)Activator.CreateInstance(type);
                        if (codec != null)
                        {
                            if (codec.Name() != null)
                            {
                                codec.Start();
                                Codecs.Add(codec);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        LogManager.Instance.WriteException("Failed to create instance of codec of type {0} from assembly {1}", 
                            type, assembly.FullName);
                        LogManager.Instance.WriteException(e.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Unload ourselves.
        /// </summary>
        public void Stop()
        {
            log.Info("Movie manager stop");
            foreach (ICodec codec in Codecs)
            {
                codec.Stop();
            }
            Codecs.Clear();
            Instance = null;
        }
        #endregion

        /// <summary>
        /// Silly helper class to figure out if the codec's name
        /// matches in a search.
        /// </summary>
        private class CodecFinder
        {
            string name;
            public CodecFinder(string nm) { name = nm; }
            public bool MatchName(ICodec man)
            {
                return man.Name() == name;
            }
        }

        /// <summary>
        /// Find the codec object using the name of the codec.
        /// </summary>
        /// <param name="name">
        /// The codec name, which was used to register itself to the
        /// codec array.
        /// </param>
        /// <returns>
        /// The codec object if it was found, or null if it wasn't.
        /// </returns>
        public ICodec FindCodec(string name)
        {
            CodecFinder cf = new CodecFinder(name);
            ICodec ans = Codecs.Find(new Predicate<ICodec>(cf.MatchName));
            return ans;
        }

        /// <summary>
        /// Find a movie file by its name.
        /// </summary>
        /// <param name="name">
        /// The name of the movie to find, as passed in at creation.
        /// </param>
        /// <returns>
        /// The movie file if found, or null if it wasn't.
        /// </returns>
        public IMovie FindMovie(string name)
        {
            IMovie m = null;
            foreach (ICodec c in Codecs)
            {
                m = c.FindMovie(name);
                if (m != null)
                {
                    return m;
                }
            }
            return m;
        }

        /// <summary>
        /// Find a movie from the movies directory and return a path
        /// to it.
        /// </summary>
        /// <param name="name">
        /// The filename of the movie.
        /// </param>
        /// <returns>
        /// A Windows path to the movie, possibly relative.
        /// </returns>
        public static string ResolveMovieFile(string name)
        {
            try
            {
                string ans = ResourceManager.ResolveCommonResourceData(name);
                if (ans == null)
                {
                    log.ErrorFormat("ResolveMovieFile file '{0}' not found", name);
                    return null;
                }
                else
                {
                    log.InfoFormat("ResolveMovieFile file '{0}' returned '{1}'", name, ans);
                    return ans;
                }
            }
            catch (Exception)
            {
                log.ErrorFormat("ResolveMovieFile file '{0}' not found", name);
                return null;
            }
        }

        /// <summary>
        /// Get a new unique identifier for a movie.
        /// </summary>
        /// <returns>
        /// An integer id, which should be returned by IMovie.ID().
        /// </returns>
        public static int GetNewIdentifier()
        {
            return movieIndex++;
        }

        public static int NextPowerOfTwo(int num)
        {
            // all hail wikipedia
            if (num == 0)
            {
                return num;
            }
            if (((num) & (num - 1)) == 0)
            {
                return num;
            }
            int ans = num - 1;
            ans |= (ans >> 1);
            ans |= (ans >> 2);
            ans |= (ans >> 4);
            ans |= (ans >> 8);
            ans |= (ans >> 16);
            ans++;
            return ans;
        }

        // XXXMLM - ReplaceMaterial?

        /// <summary>
        /// Replace a world object with a width by height textured plane 
        /// that plays the movie.  Keeps the original Entity object but
        /// replaces its mesh, material, and texture.
        /// </summary>
        /// <param name="movie">
        /// The movie we're going to play on the object.
        /// </param>
        /// <param name="name">
        /// The name of the world object to replace.
        /// </param>
        /// <returns>
        /// True if the object was replaced, false if it wasn't.
        /// </returns>
        public static bool ReplaceWorldObject(IMovie movie, string name)
        {
            if (Client.Instance != null)
            {
                ObjectNode node = Client.Instance.WorldManager.GetObjectNode(name);
                if (node == null)
                {
                    return false;
                }
                return AttachToNode(movie, node);
            }
            else
            {
                // could be in world editor
                return false;
            }
        }

        /// <summary>
        /// Private function to do the work of ReplaceWorldObject.
        /// </summary>
        /// <param name="movie">
        /// The movie we're going to play on the object.
        /// </param>
        /// <param name="on">
        /// The scene object to replace.
        /// </param>
        /// <returns>
        /// True if the object was replaced, false if it wasn't.
        /// </returns>
        private static bool AttachToNode(IMovie movie, ObjectNode on)
        {
            SceneNode sn = on.SceneNode;
            IEnumerator ie = sn.Objects.GetEnumerator();
            ie.MoveNext();
            MovableObject mo = (MovableObject)ie.Current;
            Entity en = (Entity)mo;
            if (ReplaceEntity(en,
                MeshName(movie),
                MaterialName(movie),
                movie.TextureName(),
                movie.VideoSize(),
                movie.TextureSize()))
            {
                return movie.SetTextureCoordinates(MaterialName(movie));
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Helper function to replace an entity in the scene.  Adjusts the
        /// texture coordinates to flip the video.  That may be wrong on 
        /// everything except DirectX / DirectShow.
        /// </summary>
        /// <param name="en">
        /// The entity we're going to replace.
        /// </param>
        /// <param name="meshName">
        /// The name of the mesh to create.
        /// </param>
        /// <param name="materialName">
        /// The name of the material to create.
        /// </param>
        /// <param name="textureName">
        /// The name of the texture to create.
        /// </param>
        /// <param name="videoSize">
        /// The size of the movie, in width by height pixels.
        /// </param>
        /// <param name="textureSize">
        /// The size of the texture, in width by height pixels.
        /// </param>
        /// <returns></returns>
        private static bool ReplaceEntity(
            Entity en, 
            string meshName, string materialName, string textureName,
            Size videoSize, Size textureSize)
        {
            Mesh me = MeshManager.Instance.CreatePlane(
                meshName, // name
                new Axiom.MathLib.Plane(new Axiom.MathLib.Vector3(0, 0, -1), new Axiom.MathLib.Vector3(0, 0, 0)),
                videoSize.Width,
                videoSize.Height,
                1, // xsegments
                1, // ysegments
                true, // normals
                1, // numtexcoords
                1.0f,// utile
                1.0f,// vtile
                new Axiom.MathLib.Vector3(0, 1, 0) // upvec
                );

            en.Mesh = me;
            Axiom.Graphics.Material m = MaterialManager.Instance.GetByName(materialName);
            if (m == null)
            {
                m = (Axiom.Graphics.Material)
                    MaterialManager.Instance.Create(materialName, true);
                ColorEx c = new ColorEx(1.0f, 1.0f, 1.0f);
                m.Ambient = c;
                m.Diffuse = c;
                for (int i = 0; i < m.GetTechnique(0).NumPasses; i++)
                {
                    Pass p = m.GetTechnique(0).GetPass(i);
                    p.RemoveAllTextureUnitStates();
                    p.CreateTextureUnitState(textureName);
                }
            }
            en.MaterialName = materialName;
            return true;
        }

        /// <summary>
        /// Reset the texture coordinates in the given material for all instances of the 
        /// movie texture to fit the actual size of the movie.  Since movies often are
        /// not sized to a power of two, texture coordinates need to remap the image to
        /// fill the texture correctly.  Sets a texture matrix to adjust the existing coordinates.
        /// </summary>
        /// <param name="movie">
        /// The movie to get the sizes and texture name from.
        /// </param>
        /// <param name="material">
        /// The name of the material to search.
        /// </param>
        /// <returns>
        /// True if coordinates were changed, false if not.
        /// </returns>
        public static bool SetTextureCoordinates(IMovie movie, string material)
        {
            return SetTextureCoordinates(movie.TextureName(), movie.VideoSize(), movie.TextureSize(), material);
        }

        /// <summary>
        /// Helper function to set the texture coordinates.  Instead of taking a movie 
        /// object, this takes a specific texture name, video size, texture size, and 
        /// material.  Sets a texture matrix to adjust the existing coordinates.
        /// </summary>
        /// <param name="textureName">
        /// The name of the texture to adjust.
        /// </param>
        /// <param name="videoSize">
        /// The size of the video in pixels.
        /// </param>
        /// <param name="textureSize">
        /// The size of the expected texture in pixels.
        /// </param>
        /// <param name="material">
        /// The name of the material to search for textures.
        /// </param>
        /// <returns>
        /// True if any texture coordinates were adjusted, false if not.
        /// </returns>
        public static bool SetTextureCoordinates(string textureName, Size videoSize, Size textureSize, string material)
        {
            bool ans = false;
            Axiom.Graphics.Material m = MaterialManager.Instance.GetByName(material);
            if (m != null)
            {
                for (int i = 0; i < m.NumTechniques; i++)
                {
                    for (int j = 0; j < m.GetTechnique(i).NumPasses; j++)
                    {
                        Pass p = m.GetTechnique(i).GetPass(j);
                        for (int k = 0; k < p.NumTextureUnitStages; k++)
                        {
                            if (p.GetTextureUnitState(k).TextureName == textureName)
                            {
                                TextureUnitState tu = p.GetTextureUnitState(k);
                                float uRatio = ((float)videoSize.Width) / ((float)textureSize.Width);
                                float vRatio = ((float)videoSize.Height) / ((float)textureSize.Height);
                                tu.SetTextureScale(1.0f / uRatio, 1.0f / vRatio);
                                tu.SetTextureScroll(-0.5f * (1.0f - uRatio), -0.5f * (1.0f - vRatio));
                                ans = true;
                            }
                        }
                    }
                }
            }
            return ans;
        }

        /// <summary>
        /// Load an alternate image into the scene for the given texture name.
        /// Future movies that play should look for this texture and unload it
        /// if it exists.
        /// </summary>
        /// <param name="name">
        /// The name of the texture to replace.
        /// </param>
        /// <param name="file">
        /// The name of the file in the Textures directory to display.
        /// </param>
        /// <returns>
        /// True if the texture was created, false if it wasn't.
        /// </returns>
        public static bool ShowAltImage(string name, string file)
        {
            Axiom.Core.Texture texture = TextureManager.Instance.GetByName(name);
            if (texture != null)
            {
                if (texture.IsLoaded)
                {
                    texture.Unload();
                }
                TextureManager.Instance.Remove(name);
            }
            try
            {
                Axiom.Media.Image img = Axiom.Media.Image.FromFile(file);
                if (img != null)
                {
                    texture = TextureManager.Instance.LoadImage(name, img);
                    return texture != null;
                }
            }
            catch (Exception e)
            {
                LogUtil.ExceptionLog.ErrorFormat("Exception: {0}", e);
                return false;
            }
            return false;
        }
        public static bool HideAltImage(string name)
        {
            Axiom.Core.Texture texture = TextureManager.Instance.GetByName(name);
            if (texture != null)
            {
                if (texture.IsLoaded)
                {
                    texture.Unload();
                }
                TextureManager.Instance.Remove(name);
                return true;
            }
            return false;
        }
    }

    /// <summary>
    /// An IMovieTexture is a delay-loaded texture representation of a movie in 
    /// a scene.  Its entire purpose in life is to override the implementation
    /// of the Axiom texture to catch when the texture is loaded and start the
    /// movie when it's loaded.  The external texture source passes through the
    /// parameters.  This object is created by the codec in ICodec.CreateMovieTexture().
    /// </summary>
    public interface IMovieTexture
    {
        /// <summary>
        /// Add a new material that this texture will be played on.  This material 
        /// will have its texture coordinates updated when the movie loads.
        /// </summary>
        /// <param name="material">
        /// The name of the material, which will be checked for uniqueness and
        /// only added once.
        /// </param>
        void AddMaterial(string material);

        /// <summary>
        /// Parse the string parameters and set the movie object appropriately.
        /// Default parameters that are required are MovieTextureSource.MV_MOVIE_NAME
        /// and MovieTextureSource.MV_CODEC_NAME.
        /// </summary>
        /// <param name="name">
        /// The name of the parameter to set, from the material file.
        /// </param>
        /// <param name="val">
        /// All values of the parameter in string form.
        /// </param>
        void SetParameter(string name, string val);
    }

    /// <summary>
    /// The movie texture source is our implementation of the external texture source API
    /// so that the material serializer knows who to call when it finds a texture_source
    /// of MovieTextureSource.MV_SOURCE_NAME.  An mvMovie is expected to have a codec 
    /// that it calls to play the movie itself.  The Movie.Manager API can also be called
    /// directly from script.  Once the movie is loaded, the movie object can be retrieved
    /// via the usual Movie.Manager calls.  The defined texture should be registered into
    /// the texture manager with TextureManager.Instance.Add() by the IMovieTexture 
    /// implementation.
    /// </summary>
    public class MovieTextureSource : ExternalTextureSource
    {
        /// <summary>
        /// The name we use to register ourselves as an external texture source.
        /// </summary>
        public const string MV_SOURCE_NAME = "mvMovie";

        /// <summary>
        /// The parameter name for the unique name to refer to this movie as.  Also 
        /// is the name of the texture that's created.  A required parameter.
        /// </summary>
        public const string MV_MOVIE_NAME = "name";

        /// <summary>
        /// The parameter name for the name of the codec to use to decode this movie.
        /// A required parameter.
        /// </summary>
        public const string MV_CODEC_NAME = "codec";

        /// <summary>
        /// The file path or URL to the movie to load.  If this is null, the movie
        /// won't be loaded on startup, but a texture will be registered for the
        /// codec with the given movie name.
        /// </summary>
        public const string MV_PATH_NAME = "path";

        /// <summary>
        /// The name of an alt texture to display from the Textures directory.
        /// Not a required parameter.
        /// </summary>
        public const string MV_ALT_NAME = "alt";

        /// <summary>
        ///   Create a class logger
        /// </summary>
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(MovieTextureSource));

        /// <summary>
        /// Store and validate parameters before we create the texture.
        /// </summary>
        private IDictionary parameters = new Hashtable();

        /// <summary>
        /// A list of all the textures we've created so far so we don't create
        /// duplicates.
        /// </summary>
        private IDictionary textures = new Hashtable();

        /// <summary>
        /// Construct this object before anything has been initialized.
        /// </summary>
        public MovieTextureSource()
        {
            pluginName = "MVMovieTextureSource";
        }

        /// <summary>
        /// The render system has been created, so initialize our data.
        /// </summary>
        /// <returns></returns>
        public override bool Initialize()
        {
            log.Info(MV_SOURCE_NAME + ": Initialize");
            return true;
        }

        /// <summary>
        /// We're all done, so stop the world.
        /// </summary>
        public override void Shutdown()
        {
            log.Info(MV_SOURCE_NAME + ": Shutdown");
        }

        /// <summary>
        /// Send a string parameter to the movie texture currently under
        /// construction.  This list is cleared once the texture is created.
        /// </summary>
        /// <param name="name">
        /// The name of the parameter to set.
        /// </param>
        /// <param name="value">
        /// What to set the parameter to.
        /// </param>
        public override void SetParameter(string name, string value)
        {
            if (name[0] != '#')
            {
                log.InfoFormat(MV_SOURCE_NAME + ": SetParameter {0}, {1}", name, value);
                parameters[name] = value;
            }
        }

        /// <summary>
        /// Turn the parameters that we've recorded so far into a texture object
        /// that can be loaded later.  The texture will simply be registered with
        /// the texture manager by the IMovieTexture object when it's successfully
        /// created.
        /// </summary>
        /// <param name="materialName">
        /// The name of the parent material, which will be used as the movie and
        /// texture name if no movie name is provided.
        /// </param>
        public override void CreateDefinedTexture(string materialName)
        {
            string name = (parameters[MV_MOVIE_NAME] as string);
            if (name == null)
            {
                log.WarnFormat(MV_SOURCE_NAME + ": No name parameter on movie, using material name ({0})", materialName);
                name = materialName;
            }
            log.InfoFormat(MV_SOURCE_NAME + ": CreateDefinedTexture {0}", name);
            if (textures[name] == null)
            {
                string codecname = (parameters[MV_CODEC_NAME] as string);
                if (codecname == null)
                {
                    log.ErrorFormat(MV_SOURCE_NAME + ": No codec parameter on texture '{0}'", name);
                }
                else
                {
                    ICodec codec = Manager.Instance.FindCodec(codecname);
                    if (codec == null)
                    {
                        log.ErrorFormat(MV_SOURCE_NAME + ": Could not find codec '{0}'", codecname);
                    }
                    else
                    {
                        IMovieTexture mt = codec.CreateMovieTexture(name);
                        if (mt == null)
                        {
                            log.ErrorFormat(MV_SOURCE_NAME + ": Could not create movie texture '{0}'", name);
                        }
                        else
                        {
                            IDictionaryEnumerator ie = parameters.GetEnumerator();
                            while (ie.MoveNext())
                            {
                                mt.SetParameter(ie.Key as string, ie.Value as string);
                            }
                            textures[name] = mt;
                        }
                    }
                }
            }
            else
            {
                log.InfoFormat(MV_SOURCE_NAME + ": Texture '{0}' already exists, adding material", name);
            }
            IMovieTexture imt = (textures[name] as IMovieTexture);
            if (imt != null)
            {
                imt.AddMaterial(materialName);
            }
            parameters.Clear();
        }

        /// <summary>
        /// Destroy an advanced texture since we're done with it.  Doesn't appear to get 
        /// called at the moment.
        /// </summary>
        /// <param name="name">
        /// The name of the texture to destroy.
        /// </param>
        public override void DestroyAdvancedTexture(string name)
        {
            log.InfoFormat(MV_SOURCE_NAME + ": DestroyAdvancedTexture {0}", name);
            textures[name] = null;
        }
    }
}
