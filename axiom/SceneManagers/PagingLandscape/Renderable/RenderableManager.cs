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

#endregion LGPL License



#region Using Directives



using System;

using System.Collections;



using Axiom.Core;

using Axiom.MathLib;

using Axiom.Collections;

using Axiom.Media;



using Axiom.SceneManagers.PagingLandscape.Collections;

using Axiom.SceneManagers.PagingLandscape.Tile;



#endregion Using Directives



namespace Axiom.SceneManagers.PagingLandscape.Renderable

{

	/// <summary>

	/// Summary description for RenderableManager.

	/// </summary>

	public class RenderableManager

	{

		#region Fields
		protected IndexBuffer indexes;

		protected ArrayList renderables;

		protected RenderableQueue queue;

		/** Queue to batch the process of loading the Renderables.
			This avoid the plug-in to load a lot of renderables in a single Frame, 
			droping the FPS.
		*/
		protected RenderableQueue renderablesLoadQueue;
		protected TileQueue tilesLoadQueue;

		protected long renderablesVisibles;
		protected long numRenderables;

		#endregion Fields


		#region Singleton Implementation

		

		/// <summary>

		/// Constructor

		/// </summary>

		private RenderableManager() 

		{

			numRenderables = 0;

			indexes = new IndexBuffer();

			renderables = new ArrayList();
			queue = new RenderableQueue();

			renderablesLoadQueue = new RenderableQueue();
			tilesLoadQueue = new TileQueue();

			// Add the requested initial number
			addBatch(Options.Instance.Num_Renderables);
		}





		private static RenderableManager instance = null;



		public static RenderableManager Instance 

		{

			get 

			{

				if ( instance == null ) instance = new RenderableManager();

				return instance;

			}

		}





		#endregion Singleton Implementation



		#region IDisposable Implementation



		public void Dispose()

		{

			if (instance == this) 

			{

				renderables.Clear();

				indexes = null;

				instance = null;

			}

		}



		#endregion IDisposable Implementation


		public Renderable GetRenderable( )
		{
			if ( queue.Size == 0 )
			{
				// We don´t have more renderables, so we are going to add more
				addBatch(Options.Instance.Num_Renderables_Increment);
				// Increment the next batch by a 10%
				Options.Instance.Num_Renderables_Increment += (long) (Options.Instance.Num_Renderables_Increment * 0.1f);
			}
			return queue.Pop( );
		}

		/** Make a renderable free.
		*/
		public void FreeRenderable( Renderable rend )
		{
			queue.Push( rend );
		}

		/** Set this renderable to be loaded
		*/
		public void QueueRenderableLoading( Renderable rend, Tile.Tile tile )
		{
			renderablesLoadQueue.Push( rend );
			tilesLoadQueue.Push( tile );
		}
		/** Load a set of renderables
		*/
		public void ExecuteRenderableLoading()
		{
			long j = (long)renderablesLoadQueue.Size;
			if (j != 0)
			{ 
				if (j > Options.Instance.Num_Renderables_Loading)
				{
					j = Options.Instance.Num_Renderables_Loading;
				}
				for (long i = 0; i < j ; i++ )
				{
					Renderable rend = renderablesLoadQueue.Pop( );
					if ( rend != null )
					{
						rend.Load();
						Tile.Tile tile = tilesLoadQueue.Pop( );
						// make sure tile had not been unloaded til queue insertion
						if (tile.IsLoaded )
						{
//							Debug.Assert(tile != null &&  tile.getTileNode ());
							// only attach if renderable loaded
							tile.TileNode.AttachObject( rend );
							tile.LinkRenderableNeighbor ();  
						}  
					}
				}
			}
		}

		public long RenderablesCount
		{
			get
			{
				return numRenderables;
			}
		}
		public int FreeCount 
		{
			get
			{
				return queue.Size;
			}
		}
		public int LoadingCount
		{
			get
			{
				return this.renderablesLoadQueue.Size;
			}
		}

    
		public long VisibleCount 
		{
			get
			{
				return renderablesVisibles;
			}
		}

		public void ResetVisibles ()
		{
			renderablesVisibles = 0;
		}

		public void AddVisible ()
		{
			renderablesVisibles ++;
		}

		

		protected void addBatch(long num)
		{
			numRenderables += num;
			//    mRenderables.reserve (mNumRenderables);
			//    mRenderables.resize (mNumRenderables);

			for (long i = 0; i < num; i++ )
			{
				Renderable rend = new Renderable();
				renderables.Add( rend );
				queue.Push( rend );
			}
		}

	}

}

