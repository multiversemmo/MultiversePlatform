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
using System.Drawing;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using Axiom.Core;
using Axiom.MathLib;
using Axiom.Graphics;
using Axiom.Utility;
using Axiom.Controllers;

namespace Axiom.Core {

    /// <summary>
    ///		Subclass of BillboardChain which automatically leaves a trail behind
    ///     one or more Node instances.
    /// </summary>
    /// <remarks>
    ///    A billboard chain operates much like a traditional billboard, ie its
    ///    An instance of this class will watch one or more Node instances, and
    ///    automatically generate a trail behind them as they move. Because this
    ///    class can monitor multiple modes, it generates its own geometry in 
    ///    world space and thus, even though it has to be attached to a SceneNode
    ///    to be visible, changing the position of the scene node it is attached to
    ///    makes no difference to the geometry rendered.
    ///    <p/>
    ///    The 'head' element grows smoothly in size until it reaches the required size,
    ///    then a new element is added. If the segment is full, the tail element
    ///    shrinks by the same proportion as the head grows before disappearing.
    ///    <p/>
    ///    Elements can be faded out on a time basis, either by altering their color
    ///    or altering their alpha. The width can also alter over time.
    ///    <p/>
    ///    'v' texture coordinates are fixed at 0.0 if used, meaning that you can
    ///    use a 1D texture to 'smear' a color pattern along the ribbon if you wish.
    ///    The 'u' coordinates are by default (0.0, 1.0), but you can alter this 
    ///    using setOtherTexCoordRange if you wish.
    /// </remarks>
	public class RibbonTrail : BillboardChain, IControllerValue<float>
    {
        #region members

        // Create a logger for use in this class
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(RibbonTrail));

		/// List of nodes being trailed
		protected List<Node> nodeList = new List<Node>();

		/// Ordered like nodeList, contains chain index
        protected List<int> nodeToChainSegment = new List<int>();

		// fast lookup node->chain index
		// we use positional map too because that can be useful
        protected Dictionary<Node, int> nodeToSegMap = new Dictionary<Node, int>();

		// chains not in use
		protected List<int> freeChains = new List<int>();

		/// Total length of trail in world units
		protected float trailLength;

		/// length of each element
		protected float elemLength;

		/// Squared length of each element
		protected float squaredElemLength;

		/// A list of trail attributes for each chain
        protected TrailAttributes[] trailAttributes;

		/// controller used to hook up frame time to fader
		protected Controller<float> fadeController;
        
        #endregion members

        /// <summary>
        ///		Constructor
        /// </summary>
        /// <remarks>
		/// <param name="name"> The name to give this object</param>
		/// <param name="maxElements"> The maximum number of elements per chain</param>
		/// <param name="numberOfChains"> The number of separate chain segments contained in this object,
		///	    ie the maximum number of nodes that can have trails attached
		/// </param>
        /// <param name="useTextureCoords"> If true, use texture coordinates from the chain elements</param>
		/// <param name="useVertexColors"> If true, use vertex colors from the chain elements (must
		///     be true if you intend to use fading)
        /// </param>
		public RibbonTrail(string name, int maxElementPerChain, int numberOfChains, 
			bool useTextureCoords, bool useVertexColors) : base(name) {
            this.maxElementsPerChain = maxElementsPerChain;
            this.numberOfChains = numberOfChains;
			this.useTextureCoords = useTextureCoords;
            this.useVertexColors = useVertexColors;
            InitializeRibbonTrail();
        }
            
        /// <summary>
        ///		The constructor most callers will use
        /// </summary>
		public RibbonTrail(string name, params Object[] parameters) : base(name, parameters) {
            InitializeRibbonTrail();
        }

        protected void InitializeRibbonTrail() {
            this.TexCoordDirection = TextureCoordDirection.V;
            trailAttributes = new TrailAttributes[numberOfChains];
            for (int i=0; i<numberOfChains; i++) {
                trailAttributes[i] = new TrailAttributes();
                freeChains.Add(i);
            }
        }

		/// destructor
        // virtual ~RibbonTrail();

        /// <summary>
        ///		Add a node to be tracked.
        /// </summary>
		/// <param name="n"> The node that will be tracked</param>
        public virtual void AddNode(Node n) {
            if (nodeList.Count == numberOfChains) {
                log.Error("RibbonTrail.AddNode: " + name + " cannot monitor any more nodes, chain count exceeded");
            }
            // get chain index
            int chainIndex = freeChains[0];
            freeChains.Remove(0);
            nodeToChainSegment.Add(chainIndex);
            nodeToSegMap[n] = chainIndex;

            // initialise the chain
            ResetTrail(chainIndex, n);

            nodeList.Add(n);
            n.NodeUpdatedEvent += NodeUpdated;
            n.NodeDestroyedEvent += NodeDestroyed;
        }

        /// <summary>
        ///		Remove tracking on a given node.
        /// </summary>
		/// <param name="n"> The node that will be tracked</param>
        public virtual void RemoveNode(Node n) {
            int index = nodeList.IndexOf(n);
            if (index >= 0) {
                n.NodeUpdatedEvent -= NodeUpdated;
                n.NodeDestroyedEvent -= NodeDestroyed;
                // also get matching chain segment
                int chainIndex = nodeToChainSegment[index];
                ClearChain(chainIndex);
                // mark as free now
                freeChains.Add(chainIndex);
                nodeList.RemoveAt(index);
                nodeToChainSegment.RemoveAt(index);
                nodeToSegMap.Remove(n);
            }
        }
            
        /// <summary>
        ///		Update the trail of the node
        /// </summary>
        /// <param name="node"> The node whose trail should be updated</param>
        public void NodeUpdated(Node node){
            int chainIndex = GetChainIndexForNode(node);
            UpdateTrail(chainIndex, node);
        }

        /// <summary>
        ///		Update the trail of the node
        /// </summary>
        /// <param name="node"> The node whose trail should be updated</param>
        public void NodeDestroyed(Node node) {
            RemoveNode(node);
        }

        /// <summary>
        ///		Get the chain index for a given Node being tracked.
        /// </summary>
        public virtual int GetChainIndexForNode(Node n) {
            int value = 0;
            if (nodeToSegMap.TryGetValue(n, out value))
                return value;
            else {
                log.Error("RibbonTrail.GetChainIndexForNode: Node " + n + " is not being tracked");
                return -1;
            }
        }

        /// <summary>
        ///		Gets or sets the chain index for a given Node being tracked.
        /// </summary>
        /// <remarks>
        ///    This sets the length of the trail, in world units. It also sets how
        ///    far apart each segment will be, ie length / max_elements. 
        /// </remarks>
        public virtual float TrailLength {
            get {
                return trailLength;
            }
            set {
                trailLength = value;
                elemLength = trailLength / maxElementsPerChain;
                squaredElemLength = elemLength * elemLength;
            }
        }
        
		public override int MaxChainElements {
            set {
                base.MaxChainElements = value;
                elemLength = trailLength / maxElementsPerChain;
                squaredElemLength = elemLength * elemLength;
                ResetAllTrails();
            }
        }

        public override int NumberOfChains {
            set {
                if (numberOfChains == value)
                    return;
                if (value < nodeList.Count) {
                    log.Error("RibbonTrail.NumberOfChains: Can't shrink the number of chains less than number " + 
                        nodeList.Count + " of tracking nodes");
                    return;
                }
                int oldChains = numberOfChains;
                base.NumberOfChains = value;
                TrailAttributes[] oldTrailAttributes = trailAttributes;
                trailAttributes = new TrailAttributes[value];
                Array.Copy(oldTrailAttributes, trailAttributes, oldTrailAttributes.Length);
                if (oldChains < numberOfChains) {
                    // add new chains
                    int count = numberOfChains - oldChains;
                    for (int i=0; i<count; i++) {
                        freeChains.Add(oldChains + i);
                        trailAttributes[oldChains + i] = new TrailAttributes();
                    }
                }
                ResetAllTrails();
            }
        }
        

		/** @copydoc BillboardChain::clearChain */
		public override void ClearChain(int chainIndex) {
            base.ClearChain(chainIndex);

            // Reset if we are tracking for this chain
            int nodeIndex = nodeToChainSegment.IndexOf(chainIndex);
            if (nodeIndex >= 0)
                ResetTrail(chainIndex, nodeList[nodeIndex]);
        }
            
        /// <summary>
        ///		Set the starting ribbon color for a given segment. 
        /// </summary>
        /// <param name="chainIndex"> The index of the chain</param>
        /// <param name="color"> The initial color</param>
        public virtual void SetInitialColor(int chainIndex, ColorEx col) {
            trailAttributes[chainIndex].initialColor = (ColorEx)col.Clone();
        }

        public virtual void SetInitialColor(int chainIndex, float r, float g, float b, float a) {
            if (ChainIndexAllowed(chainIndex, "SetInitialColor"))
                trailAttributes[chainIndex].initialColor = new ColorEx(a, r, g, b);
        }

        /// <summary>
        ///		Get the starting ribbon color for a given segment. 
        /// </summary>
        /// <param name="chainIndex"> The index of the chain</param>
        public virtual ColorEx GetInitialColor(int chainIndex) {
            return trailAttributes[chainIndex].initialColor;
        }

        /// <summary>
        ///		Enables / disables fading the trail using color. 
        /// </summary>
        /// <param name="chainIndex"> The index of the chain</param>
        /// <param name="valuePerSecond"> The amount to subtract from color each second</param>
        public virtual void SetColorChange(int chainIndex, ColorEx valuePerSecond) {
            trailAttributes[chainIndex].deltaColor = (ColorEx)valuePerSecond.Clone();
        }
        
        public virtual void SetColorChange(int chainIndex, float r, float g, float b, float a) {
            if (ChainIndexAllowed(chainIndex, "SetColorChange")) {
                trailAttributes[chainIndex].deltaColor = new ColorEx(a, r, g, b);
                ManageController();
            }
        }

        /// <summary>
        ///		Set the starting ribbon width in world units. 
        /// </summary>
        /// <param name="chainIndex"> The index of the chain</param>
        /// <param name="width"> The initial width of the ribbon</param>
        public virtual void SetInitialWidth(int chainIndex, float width) {
            if (ChainIndexAllowed(chainIndex, "SetInitialWidth"))
                trailAttributes[chainIndex].initialWidth = width;
        }

        /// <summary>
        ///		Get the starting ribbon width in world units. 
        /// </summary>
        /// <param name="chainIndex"> The index of the chain</param>
        public virtual float GetInitialWidth(int chainIndex, float width) {
            if (ChainIndexAllowed(chainIndex, "SetInitialWidth"))
                return trailAttributes[chainIndex].initialWidth;
            else
                return 0f;
        }

        /// <summary>
        ///		Set the change in ribbon width per second. 
        /// </summary>
        /// <param name="chainIndex"> The index of the chain</param>
        /// <param name="widthDeltaPerSecond"> The amount the width will reduce by per second.</param>
        public virtual void SetWidthChange(int chainIndex, float widthDeltaPerSecond) {
            if (ChainIndexAllowed(chainIndex, "SetWidthChange")) {
                trailAttributes[chainIndex].deltaWidth = widthDeltaPerSecond;
                ManageController();
            }
        }

        /// <summary>
        ///		Get the starting ribbon width in world units. 
        /// </summary>
        /// <param name="chainIndex"> The index of the chain</param>
        public virtual float GetWidthChange(int chainIndex) {
            if (ChainIndexAllowed(chainIndex, "GetInitialWidth"))
                return trailAttributes[chainIndex].deltaWidth;
            else
                return 0f;
        }

        /// <summary>
        ///		Perform any fading / width delta required; internal method
        /// </summary>
        /// <param name="time"> The time of the update</param>
		/// 
        public virtual void TimeUpdate(float time) {
            // Apply all segment effects
            for (int s=0; s<chainSegmentList.Count; s++) {
                ChainSegment seg = chainSegmentList[s];
                TrailAttributes attributes = trailAttributes[s];
                if (seg.head != SEGMENT_EMPTY && seg.head != seg.tail) {
                    for(int e = seg.head + 1;; ++e) {
                        e = e % maxElementsPerChain;
                        Element elem = chainElementList[seg.start + e];
                        elem.width = elem.width - (time * attributes.deltaWidth);
                        elem.width = Math.Max(0.0f, elem.width);
                        elem.color = elem.color - (attributes.deltaColor * time);
                        elem.color.Saturate();
                        if (e == seg.tail)
                            break;
                    }
                }
            }
        }

        protected bool ChainIndexAllowed (int chainIndex, string method) {
            if (chainIndex >= numberOfChains) {
                log.Error("RibbonTrail." + method + ": chainIndex out of bounds");
                return false;
            }
            else
                return true;
        }

        protected void ManageController() {
            bool needController = false;
            for (int i = 0; i < numberOfChains; ++i) {
                if (trailAttributes[i].deltaWidth != 0 || trailAttributes[i].deltaColor.CompareTo(ColorEx.Zero) != 0) {
                    needController = true;
                    break;
                }
            }
            if (fadeController == null && needController)
                // Set up fading via frame time controller
                fadeController = ControllerManager.Instance.CreateFrameTimePassthroughController(this);
            else if (fadeController != null && !needController) {
                // destroy controller
                ControllerManager.Instance.DestroyController(fadeController);
                fadeController = null;
            }
        }

        public void UpdateTrail(int index, Node node) {
            // Repeat this entire process if chain is stretched beyond its natural length
            bool done = false;
            while (!done)
            {
                // Node has changed somehow, we're only interested in the derived position
                ChainSegment seg = chainSegmentList[index];
                Element headElem = chainElementList[seg.start + seg.head];
                int nextElemIdx = seg.head + 1;
                // wrap
                if (nextElemIdx == maxElementsPerChain)
                    nextElemIdx = 0;
                Element nextElem = chainElementList[seg.start + nextElemIdx];

                // Vary the head elem, but bake new version if that exceeds element len
                Vector3 newPos = node.DerivedPosition;
                if (parentNode != null) {
                    // Transform position to ourself space
                    newPos = parentNode.DerivedOrientation.UnitInverse() *
                        ((newPos - parentNode.DerivedPosition) / parentNode.DerivedScale);
                }
                Vector3 diff = newPos - nextElem.position;
                float sqlen = diff.LengthSquared;
                if (sqlen >= squaredElemLength)
                {
                    // Move existing head to elemLength
                    Vector3 scaledDiff = diff * (float)(elemLength / Math.Sqrt(sqlen));
                    headElem.position = nextElem.position + scaledDiff;
                    // Add a new element to be the new head
                    Element newElem = new Element(newPos, trailAttributes[index].initialWidth, 0.0f, trailAttributes[index].initialColor);
                    AddChainElement(index, newElem);
                    // alter diff to represent new head size
                    diff = newPos - headElem.position;
                    // check whether another step is needed or not
                    if (diff.LengthSquared >= squaredElemLength)   
                        done = true;
                }
                else {
                    // Extend existing head
                    headElem.position = newPos;
                    done = true;
                }

                // Is this segment full?
                if ((seg.tail + 1) % maxElementsPerChain == seg.head) {
                    // If so, shrink tail gradually to match head extension
                    Element tailElem = chainElementList[seg.start + seg.tail];
                    int preTailIdx;
                    if (seg.tail == 0)
                        preTailIdx = maxElementsPerChain - 1;
                    else
                        preTailIdx = seg.tail - 1;
                    Element preTailElem = chainElementList[seg.start + preTailIdx];

                    // Measure tail diff from pretail to tail
                    Vector3 taildiff = tailElem.position - preTailElem.position;
                    float taillen = taildiff.Length;
                    if (taillen > 1e-06f)
                    {
                        float tailsize = elemLength - diff.Length;
                        taildiff *= tailsize / taillen;
                        tailElem.position = preTailElem.position + taildiff;
                    }

                }
            } // end while


            boundsDirty = true;
            // Need to dirty the parent node, but can't do it using needUpdate() here 
            // since we're in the middle of the scene graph update (node listener), 
            // so re-entrant calls don't work. Queue.
            if (parentNode != null)
                Node.QueueNeedUpdate(parentNode);
        }

        //-----------------------------------------------------------------------
        public void ResetTrail(int index, Node node) {
            Debug.Assert(index < numberOfChains);

            ChainSegment seg = chainSegmentList[index];
            // set up this segment
            seg.head = seg.tail = SEGMENT_EMPTY;
            // Create new element, v coord is always 0.0f
            Element e = new Element(node.DerivedPosition, trailAttributes[index].initialWidth, 0.0f, trailAttributes[index].initialColor);
            // Add the start position
            AddChainElement(index, e);
            // Add another on the same spot, this will extend
            AddChainElement(index, e);
        }

        public void ResetAllTrails() {
            for (int i = 0; i < nodeList.Count; ++i)
                ResetTrail(i, nodeList[i]);
        }
        
        #region Implementation of IControllerValue

        /// <summary>
        ///		Gets/Sets the value to be used in a ControllerFunction.
        /// </summary>
        public float Value {
            get { 
                return 0; // not a source 
            }
            set {
                this.TimeUpdate(value);
            }
        }
        
        #endregion

    }

    public class TrailAttributes {

        /// Initial color of the ribbon
		public ColorEx initialColor;

		/// fade amount per second
		public ColorEx deltaColor;

		/// Initial width of the ribbon
		public float initialWidth;

		/// Delta width of the ribbon
		public float deltaWidth;

        public TrailAttributes() {
            initialColor = ColorEx.White;
            deltaColor = ColorEx.Zero;
            initialWidth = 10;
            deltaWidth = 0;
        }
    }

}

