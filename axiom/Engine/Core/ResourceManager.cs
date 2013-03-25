#region LGPL License
/*
Axiom Game Engine Library
Copyright (C) 2003  Axiom Project Team

The overall design, and a majority of the core engine and rendering code 
contained within this library is a derivative of the open source Object Oriented 
Graphics Engine OGRE, which can be found at http://ogre.sourceforge.net.  
Many thanks to the OGRE team for maintaining such a high quality project.

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public
License as published by the Free Software Foundation; either
version 2.1 of the License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public
License along with this library; if not, write to the Free Software
Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
*/
#endregion

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Axiom.Configuration;
using Axiom.FileSystem;

namespace Axiom.Core {
    /// <summary>
    ///		Defines a generic resource handler.
    /// </summary>
    /// <remarks>
    ///		A resource manager is responsible for managing a pool of
    ///		resources of a particular type. It must index them, look
    ///		them up, load and destroy them. It may also need to stay within
    ///		a defined memory budget, and temporaily unload some resources
    ///		if it needs to to stay within this budget.
    ///		<p/>
    ///		Resource managers use a priority system to determine what can
    ///		be unloaded, and a Least Recently Used (LRU) policy within
    ///		resources of the same priority.
    /// </remarks>
    public abstract class ResourceManager : IDisposable {
        #region Fields

        /// <summary>
        /// If overrideName is set, then when a request is made for a resource file named
        ///   filename.ext, we will first check for filename_overrideName.ext and return
        ///   it instead if it exists.
        /// </summary>
        protected string overrideName = null;

		public const string ZipFileResourceType = "ZipFile";
		public const string FolderResourceType = "Folder";
        protected long memoryBudget;
        protected long memoryUsage;
        /// <summary>
        ///		A cached list of all resources in memory.
        ///	</summary>
        protected Hashtable resourceList = CollectionsUtil.CreateCaseInsensitiveHashtable();
		protected Hashtable resourceHandleMap = new Hashtable();
        /// <summary>
        ///		A lookup table used to find a common archive associated with a filename.
        ///	</summary>
        protected Hashtable filePaths = CollectionsUtil.CreateCaseInsensitiveHashtable();
        /// <summary>
        ///		A cached list of archives specific to a resource type.
        ///	</summary>
        protected ArrayList archives = new ArrayList();
        /// <summary>
        ///		A lookup table used to find a archive associated with a filename.
        ///	</summary>
        static protected Hashtable commonFilePaths = CollectionsUtil.CreateCaseInsensitiveHashtable();
        /// <summary>
        ///		A cached list of archives common to all resource types.
        ///	</summary>
        static protected ArrayList commonArchives = new ArrayList();
		/// <summary>
		///		Next available handle to assign to a new resource.
		/// </summary>
		static protected int nextHandle;

        #endregion Fields

        #region Constructors

        /// <summary>
        ///		Default constructor
        /// </summary>
        public ResourceManager() {
            memoryBudget = long.MaxValue;
            memoryUsage = 0;
        }

        #endregion

		#region Properties

		public static ICollection CommonFilePaths { get { return commonFilePaths.Keys; } }
		public ICollection FilePaths { get { return filePaths.Keys; } }
		public ICollection ResourceNames { get { return resourceList.Keys; } }
		public ICollection Resources { get { return resourceList.Values; } }

        /// <summary>
        /// If overrideName is set, then when a request is made for a resource file named
        ///   filename.ext, we will first check for filename_overrideName.ext and return
        ///   it instead if it exists.
        /// </summary>
        public string OverrideName
        {
            get
            {
                return overrideName;
            }
            set
            {
                overrideName = value;
            }
        }

        /// <summary>
        ///		Sets a limit on the amount of memory this resource handler may use.	
        /// </summary>
        /// <remarks>
        ///		If, when asked to load a new resource, the manager believes it will exceed this memory
        ///		budget, it will temporarily unload a resource to make room for the new one. This unloading
        ///		is not permanent and the Resource is not destroyed; it simply needs to be reloaded when
        ///		next used.
        /// </remarks>
        public long MemoryBudget {
            //get { return memoryBudget; }
            set { 
                memoryBudget = value;

                CheckUsage();
            }
        }

        /// <summary>
        ///		Gets/Sets the current memory usages by all resource managers.
        /// </summary>
        public long MemoryUsage {
            get { 
                return memoryUsage; 
            }
            set { 
                memoryUsage = value; 
            }
        }

        #endregion

        #region Virtual/Abstract methods

		/// <summary>
		///		Add a resource to this manager; normally only done by subclasses.
		/// </summary>
		/// <param name="resource">Resource to add.</param>
		public virtual void Add(Resource resource) {
			resource.Handle = GetNextHandle();

			// note: just overwriting existing for now
			resourceList[resource.Name] = resource;
			resourceHandleMap[resource.Handle] = resource;
		}

        public virtual void Remove(string name) {
            Resource resource = (Resource)resourceList[name];
            resourceList.Remove(resource.Name);
            resourceHandleMap.Remove(resource.Handle);
        }

        /// <summary>
        ///		Creates a new blank resource, compatible with this manager.
        /// </summary>
        /// <remarks>
        ///		Resource managers handle disparate types of resources. This method returns a pointer to a
        ///		valid new instance of the kind of resource managed here. The caller should  complete the
        ///		details of the returned resource and call ResourceManager.Load to load the resource. Note
        ///		that it is the CALLERS responsibility to destroy this object when it is no longer required
        ///		(after calling ResourceManager.Unload if it had been loaded).
        /// </remarks>
        /// <param name="name"></param>
        /// <returns></returns>
        public abstract Resource Create(string name, bool isManual);
        
        public Resource Create(string name) {
            return Create(name, false);
        }

        /// <summary>
        ///     Gets the next available unique resource handle.
        /// </summary>
        /// <returns></returns>
        protected int GetNextHandle() {
			return nextHandle++;
		}

        /// <summary>
        ///		Loads a resource.  Resource will be subclasses of Resource.
        /// </summary>
        /// <param name="resource">Resource to load.</param>
        /// <param name="priority"></param>
        public virtual void Load(Resource resource, int priority) {
            // load and touch the resource
            resource.Load();
            resource.Touch();

            // cache the resource
            Add(resource);
        }

        /// <summary>
        ///		Unloads a Resource from the managed resources list, calling it's Unload() method.
        /// </summary>
        /// <remarks>
        ///		This method removes a resource from the list maintained by this manager, and unloads it from
        ///		memory. It does NOT destroy the resource itself, although the memory used by it will be largely
        ///		freed up. This would allow you to reload the resource again if you wished. 
        /// </remarks>
        /// <param name="resource"></param>
        public virtual void Unload(Resource resource) {
            // unload the resource
            resource.Unload();

            // remove the resource 
            resourceList.Remove(resource.Name);

            // update memory usage
            memoryUsage -= resource.Size;
        }

        /// <summary>
        ///		
        /// </summary>
        public virtual void UnloadAndDestroyAll() {
            foreach(Resource resource in resourceList.Values) {
                // unload and dispose of resource
                resource.Unload();
                resource.Dispose();
            }

            // empty the resource list
            resourceList.Clear();
            filePaths.Clear();
            commonArchives.Clear();
            commonFilePaths.Clear();
            archives.Clear();
        }

        #endregion
		
        #region Public methods

        /// <summary>
        ///		Adds a relative path to search for resources of this type.
        /// </summary>
        /// <remarks>
        ///		This method adds the supplied path to the list of relative locations that that will be searched for
        ///		a single type of resource only. Each subclass of ResourceManager will maintain it's own list of
        ///		specific subpaths, which it will append to the current path as it searches for matching files.
        /// </remarks>
        /// <param name="path"></param>
        public void AddSearchPath(string path) {
            AddArchive(path, "Folder");
        }

        /// <summary>
        ///		Adds a relative search path for resources of ALL types.
        /// </summary>
        /// <remarks>
        ///		This method has the same effect as ResourceManager.AddSearchPath, except that the path added
        ///		applies to ALL resources, not just the one managed by the subclass in question.
        /// </remarks>
        /// <param name="path"></param>
        public static void AddCommonSearchPath(string path) {
            // record the common file path
            AddCommonArchive(path, "Folder");
        }

        public static StringCollection GetAllCommonNamesLike(string startPath, string extension) {
            StringCollection allFiles = new StringCollection();

            for(int i = 0; i < commonArchives.Count; i++) {
                Archive archive = (Archive)commonArchives[i];
                string[] files = archive.GetFileNamesLike(startPath, extension);

                // add each one to the final list
                foreach(string fileName in files) {
                    if (!allFiles.Contains(fileName))
                        allFiles.Add(fileName);
                }
            }

            return allFiles;
        }

        private static Archive CreateArchive(string name, string type) {
            IArchiveFactory factory = ArchiveManager.Instance.GetArchiveFactory(type);

            if (factory == null) {
                throw new AxiomException(string.Format("Archive type {0} is not a valid archive type.", type));
            }

            Archive archive = factory.CreateArchive(name);

            // TODO: Shouldn't be calling this manually here, but good enough until the resource loading is rewritten
            archive.Load();

            return archive;
        }

        /// <summary>
        ///		Adds an archive to 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        public void AddArchive(string name, string type) {
            Archive archive = CreateArchive(name, type);

            // add a lookup for all these files so they know what archive they are in
            foreach (string file in archive.GetFileNamesLike("", "")) {
                filePaths[file] = archive;
            }

            // add the archive to the common archives
            archives.Add(archive);
        }

        /// <summary>
        ///		Adds an archive to 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        public void AddArchive(List<string> directories, string type) {
            foreach (string name in directories) {
                Archive archive = CreateArchive(name, type);
                // add a lookup for all these files so they know what archive they are in
                foreach (string file in archive.GetFileNamesLike("", ""))
                    if (!filePaths.ContainsKey(file))
                        filePaths[file] = archive;

                // add the archive to the common archives
                archives.Add(archive);
            }
        }

        /// <summary>
        ///		Adds an archive to 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        public static void AddCommonArchive(string name, string type) {
            Archive archive = CreateArchive(name, type);

            // add a lookup for all these files so they know what archive they are in
            foreach (string file in archive.GetFileNamesLike("", "")) {
                commonFilePaths[file] = archive;
            }

            // add the archive to the common archives
            commonArchives.Add(archive);
		}


        /// <summary>
        ///		Adds an archive to 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        public static void AddCommonArchive(List<string> directories, string type) {
            foreach (string name in directories) {
                Archive archive = CreateArchive(name, type);
                // add a lookup for all these files so they know what archive they are in
                foreach (string file in archive.GetFileNamesLike("", ""))
                    if (!commonFilePaths.ContainsKey(file))
                        commonFilePaths[file] = archive;

                // add the archive to the common archives
                commonArchives.Add(archive);
            }
        }
        
        /// <summary>
		///		Gets a material with the specified name.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public Resource LoadExisting(string name) 
		{
			//get the resource
			Resource resource = GetByName(name);
			//ensure that it exists
			if(resource == null)
				throw new ArgumentException("There is no resource with the name '{0}' that already exists.",name);
			//ensure that it is loaded
			resource.Load();
			return resource;
		}

		/// <summary>
		///		Gets a resource with the given handle.
		/// </summary>
		/// <param name="handle">Handle of the resource to retrieve.</param>
		/// <returns>A reference to a Resource with the given handle.</returns>
		public virtual Resource GetByHandle(int handle) {
			Debug.Assert(resourceHandleMap != null, "A resource was being retreived, but the list of Resources is null.", "");

			Resource resource = null;

			// find the resource in the Hashtable and return it
			if(resourceHandleMap[handle] != null) {
				resource = (Resource)resourceHandleMap[handle];
				resource.Touch();
			}

			return resource;
		}

        /// <summary>
        ///    Gets a reference to the specified named resource.
        /// </summary>
        /// <param name="name">Name of the resource to retreive.</param>
        /// <returns></returns>
        public virtual Resource GetByName(string name) {
            Debug.Assert(resourceList != null, "A resource was being retreived, but the list of Resources is null.", "");

			Resource resource = null;

            // find the resource in the Hashtable and return it
			if(resourceList[name] != null) {
				resource = (Resource)resourceList[name];
			}

			return resource;
        }

        #endregion

        #region Protected methods

        /// <summary>
        ///		Makes sure we are still within budget.
        /// </summary>
        protected void CheckUsage() {
            // TODO: Implementation of CheckUsage.
            // Keep a sorted list of resource by LastAccessed for easy removal of oldest?
        }

        public static bool HasCommonResourceData(string fileName)
        {
            return commonFilePaths.ContainsKey(fileName);
        }

        public bool HasResource(string name)
        {
            return resourceList.ContainsKey(name);
        }

		public bool HasResourceData(string fileName) 
		{
			return filePaths.ContainsKey(fileName) || commonFilePaths.ContainsKey(fileName);
		}

        public string ResolveResourceData(string fileName)
        {
            if (filePaths.ContainsKey(fileName))
            {
                Archive archive = (Archive)filePaths[fileName];
                if (archive is Folder)
                    return Path.Combine(archive.Name, fileName);
            }

            // search common file cache			
            if (commonFilePaths.ContainsKey(fileName))
            {
                Archive archive = (Archive)commonFilePaths[fileName];
                if (archive is Folder)
                    return Path.Combine(archive.Name, fileName);
            }
            return null;
        }
        /// <summary>
        ///		Locates resource data within the archives known to the ResourceManager.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public Stream FindResourceData(string fileName) {
            if (overrideName != null && overrideName.Length > 0 && fileName.Contains("."))
            {
                int extStart = fileName.LastIndexOf('.');
                string baseName = fileName.Substring(0, extStart);
                string ext = fileName.Substring(extStart + 1);
                string overrideFileName = string.Format("{0}_{1}.{2}", baseName, overrideName, ext);

                // look in local file cache first
                if (filePaths.ContainsKey(overrideFileName))
                {
                    Archive archive = (Archive)filePaths[overrideFileName];
                    return archive.ReadFile(overrideFileName);
                }

                // search common file cache			
                if (commonFilePaths.ContainsKey(overrideFileName))
                {
                    Archive archive = (Archive)commonFilePaths[overrideFileName];
                    return archive.ReadFile(overrideFileName);
                }

                LogManager.Instance.Write("Unable to find override file: {0}", overrideFileName);
            }
            // look in local file cache first
            if(filePaths.ContainsKey(fileName)) {
                Archive archive = (Archive)filePaths[fileName];
                return archive.ReadFile(fileName);
            }

            // search common file cache			
            if(commonFilePaths.ContainsKey(fileName)) {
                Archive archive = (Archive)commonFilePaths[fileName];
                return archive.ReadFile(fileName);
            }

            //not found in the cache, load the resource manually, but log the unsuggested practice
			if(File.Exists(fileName)) 
			{
				string fileNameWithoutDirectory = Path.GetFileName(fileName);
				if(filePaths.ContainsKey(fileNameWithoutDirectory) || commonFilePaths.ContainsKey(fileNameWithoutDirectory)) 
				{
					LogManager.Instance.Write("Resource names should not be relative file paths but just as a file name when located in searched directories, "
						+ "however resource '{0}' is registered so it will be loaded for the specified resource name of '{1}'.", fileNameWithoutDirectory, fileName); 
					return FindResourceData(fileNameWithoutDirectory);
				}
                LogManager.Instance.Write("File '{0}' is being loaded manually since it exists, however it should be located in a registered media archive or directory.", fileNameWithoutDirectory);
				return File.OpenRead(fileName);
			}
			
            // TODO: Load resources manually
            throw new AxiomException(string.Format("Resource '{0}' could not be found.  Be sure it is located in a known directory "
				+ "or that it is not qualified by a directory name unless that directory is located inside a zip archive.", fileName));
        }
		public StringCollection GetResourceNamesWithExtension(string fileExtension) 
		{
			StringCollection list = new StringCollection();
			foreach(string name in filePaths.Keys) 
			{
				if(name.EndsWith(fileExtension))
					list.Add(name);
			}
			foreach(string name in commonFilePaths.Keys) 
			{
				if(name.EndsWith(fileExtension))
					list.Add(name);
			}
			return list;
		}
		public StringCollection GetResourceNamesWithExtension(params string[] fileExtensions) 
		{
			StringCollection list = new StringCollection();
			foreach(string name in filePaths.Keys) 
			{
				foreach(string fileExtension in fileExtensions) 
				{
					if(name.EndsWith(fileExtension)) 
					{
						list.Add(name);
						break;
					}
				}
			}
			foreach(string name in commonFilePaths.Keys) 
			{
				foreach(string fileExtension in fileExtensions) 
				{
					if(name.EndsWith(fileExtension)) 
					{
						list.Add(name);
						break;
					}
				}
			}
			return list;
		}


        public static string GetCommonResourceDataFilePath(string fileName)
        {
            // search common file cache			
            if (commonFilePaths.ContainsKey(fileName))
            {
                Archive archive = (Archive)commonFilePaths[fileName];
                return Path.Combine(archive.Name, fileName);
            }

            throw new AxiomException(string.Format("Resource '{0}' could not be found.  Be sure it is located in a known directory.", fileName));
        }


        /// <summary>
        ///		Locates resource data within the archives known to the ResourceManager.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static Stream FindCommonResourceData(string fileName) {

            // search common file cache			
            if(commonFilePaths.ContainsKey(fileName)) {
                Archive archive = (Archive)commonFilePaths[fileName];
                return archive.ReadFile(fileName);
            }

            // not found in the cache, load the resource manually
			
            // TODO: Load resources manually
            throw new AxiomException(string.Format("Resource '{0}' could not be found.  Be sure it is located in a known directory.", fileName));
        }

        public static string ResolveCommonResourceData(string fileName)
        {
            // search common file cache			
            if (commonFilePaths.ContainsKey(fileName))
            {
                Archive archive = (Archive)commonFilePaths[fileName];
                if (archive is Folder)
                    return archive.Name + System.IO.Path.DirectorySeparatorChar + fileName;
            }
            return null;
        }
        #endregion

        #region IDisposable Implementation

        /// <summary>
        ///     Called when the engine is shutting down.
        /// </summary>
        public virtual void Dispose() {
            // unload and destroy all resources
            UnloadAndDestroyAll();
        }

        #endregion IDisposable Implementation
    }
}
