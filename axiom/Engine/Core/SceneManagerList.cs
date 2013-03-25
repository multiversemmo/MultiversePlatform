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


using Axiom.Graphics;

namespace Axiom.Core {
    /// <summary>
    ///     Enumerates the SceneManager classes available to applications.
    /// </summary>
    /// <remarks>
    ///     As described in the SceneManager class, SceneManagers are responsible
    ///     for organizing the scene and issuing rendering commands to the
    ///     RenderSystem. Certain scene types can benefit from different
    ///     rendering approaches, and it is intended that subclasses will
    ///     be created to special case this.
    ///     <p/>
    ///     In order to give applications easy access to these implementations,
    ///     the Root object has a GetSceneManager method to retrieve a SceneManager
    ///     which is appropriate to the scene type. However, this is the class
    ///     which implements this behavior and defines the scene types, because
    ///     it is intended that the Root class is not customized by everybody
    ///     (and it may be restricted access in the future).
    /// </remarks>
    // TODO: Class name no longer matches file name.
    public sealed class SceneManagerEnumerator {
        #region Singleton implementation

        /// <summary>
        ///     Singleton instance of this class.
        /// </summary>
        private static SceneManagerEnumerator instance = new SceneManagerEnumerator();

        /// <summary>
        ///     Internal constructor.  This class cannot be instantiated externally.
        /// </summary>
        internal SceneManagerEnumerator() {
            if (instance == null) {
                instance = this;

                // by default, use the standard scene manager.
                defaultSceneManager = new SceneManager("Default Scene Manager");

                // by default, all scenetypes use the default Scene Manager.  Note: These can be overridden by plugins.
                SetSceneManager(SceneType.Generic, defaultSceneManager);
                SetSceneManager(SceneType.ExteriorClose, defaultSceneManager);
                SetSceneManager(SceneType.ExteriorFar, defaultSceneManager);
                SetSceneManager(SceneType.Interior, defaultSceneManager);
                SetSceneManager(SceneType.Overhead, defaultSceneManager);
            }
        }

        /// <summary>
        ///     Gets the singleton instance of this class.
        /// </summary>
        public static SceneManagerEnumerator Instance {
            get { 
                return instance; 
            }
        }

        #endregion Singleton implementation

		#region Fields and Properties

        /// <summary>
        ///     Standard scene manager for default management.
        /// </summary>
        private SceneManager defaultSceneManager;
        /// <summary>
        ///     Collection of loaded scene managers, keyed by scene type.
        /// </summary>
        private Hashtable sceneManagers = new Hashtable();

		public SceneManager this[SceneType type] 
		{
			set
			{
				sceneManagers[type] = value;

				// Set rendersystem, incase this is set after the rendersystem has already been selected
				// value.TargetRenderSystem = Root.Instance.RenderSystem;
			}
			get
			{ 
				
				if (sceneManagers[type] == null) 
				{
					throw new AxiomException("Cannot find scene manager for type '{0}'", type);
				}

				return (SceneManager)sceneManagers[type];
			}
		}

        #endregion Fields

        #region Methods

        /// <summary>
        ///     Gets a reference to the scene manager of the specified type.
        /// </summary>
        /// <param name="type">Type of scene manager to retrieve.</param>
        /// <returns>A reference to the scene manager of the specified type.</returns>
        public SceneManager GetSceneManager(SceneType type) {
            return this[type];
        }

        /// <summary>
        ///     Notifies all SceneManagers of the destination rendering system.
        /// </summary>
        /// <param name="system">Current destination render system.</param>
        public void SetRenderSystem(RenderSystem system) {
            // loop through each scene manager and set the new render system
            foreach(SceneManager sceneManager in sceneManagers.Values) {
                sceneManager.TargetRenderSystem = system;
            }
        }

        /// <summary>
        ///     Sets a scene manager implementation for the given type.
        /// </summary>
        /// <param name="type">Type of scene this manager implements.</param>
        /// <param name="manager">Reference to the scene manager.</param>
        public void SetSceneManager(SceneType type, SceneManager manager) {
           this[type] = manager;
        }

        /// <summary>
        ///     Shuts down all registered scene managers.
        /// </summary>
        public void ShutdownAll() {
            // clear the scene of each registered scene manager
            foreach (SceneManager manager in sceneManagers.Values) {
                manager.ClearScene();
            }
        }

        #endregion
    }
}
