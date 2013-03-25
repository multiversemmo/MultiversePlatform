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
using System.Linq;
using System.Text;
using System.ComponentModel.Design.Serialization;
using System.Windows.Forms;
using Microsoft.MultiverseInterfaceStudio.FrameXml.Serialization;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Collections;
using Microsoft.MultiverseInterfaceStudio.FrameXml.Controls;

namespace Microsoft.MultiverseInterfaceStudio.FrameXml
{
	public partial class FrameXmlDesignerLoader
	{
		/// <summary>
		/// Returns the type of the control corresponding to the serialization object passed
		/// </summary>
		/// <param name="serializationObject">The serialization object.</param>
		/// <returns></returns>
		private static Type GetControlType(SerializationObject serializationObject)
		{
			//LayoutFrameType layoutFrame = serializationObject as LayoutFrameType;
			//if (layoutFrame != null && layoutFrame.@virtual)
			//    return typeof(VirtualComponent);

			// converts serialization object type name to control type name
			string typeName = serializationObject.GetType().Name;

			if (typeName.EndsWith("Type"))
				typeName = typeName.Substring(0, typeName.Length - 4);

			typeName = typeof(ISerializableControl).Namespace + '.' + typeName;

			return Type.GetType(typeName);
		}

		/// <summary>
		/// Creates a control corresponding to the serialization object passed.
		/// </summary>
		/// <param name="serializationObject">The serialization object.</param>
		/// <param name="parent">parent control</param>
		/// <returns></returns>
		private ISerializableControl CreateControl(SerializationObject serializationObject, Control parent)
		{
			return CreateControl(serializationObject, parent, false);
		}

		/// <summary>
		/// Creates a control corresponding to the serialization object passed.
		/// </summary>
		/// <param name="serializationObject">The serialization object.</param>
		/// <param name="parent">parent control</param>
		/// <param name="inherited">true if the control is inherited (should be locked)</param>
		/// <returns></returns>
		private ISerializableControl CreateControl(SerializationObject serializationObject, Control parent, bool inherited)
		{
			Type controlType = GetControlType(serializationObject);
			if (controlType == null)
				return null;

			IComponent component = LoaderHost.CreateComponent(controlType);
			ISerializableControl iControl = (ISerializableControl) component;
            iControl.DesignerLoader = this;
            iControl.SerializationObject = serializationObject;

			LayoutFrameType layoutFrameType = serializationObject as LayoutFrameType;
			if (layoutFrameType != null)
			{
				if (!inherited)
					component.Site.Name = layoutFrameType.ExpandedName;

				BaseControl control = iControl as BaseControl;
				if (control != null)
				{
					control.Inherited = inherited;
					Size size = layoutFrameType.SizeInPixels;
					if (!size.IsEmpty)
						control.Size = size;
					else
					{
						control.SetDefaultSize();
					}

					if (parent != null)
					{
						control.Parent = parent;
					}
				}
			}

			return iControl;
		}

        public Controls.Ui RootControl 
		{
			get
			{
				return LoaderHost.RootComponent as Controls.Ui;
			}
		}

		public BaseControlCollection BaseControls { get; private set; }

		public bool IsLoading { get; set; }

		private void CreateRootControl(Serialization.Ui ui)
		{
			this.IsLoading = true;
			try
			{
				// just create the root component
				this.CreateControl(ui, null);
				this.BaseControls = new BaseControlCollection(this.RootControl);
			}
			finally
			{
				this.IsLoading = false;
			}
		}

		/// <summary>
		/// holds the top level objects from the designer
		/// it is used during serialization (these objects will be removed from the hierarchy)
		/// </summary>
		private List<SerializationObject> objectsInView = new List<SerializationObject>();
		
		public void ReloadControls()
		{
			this.IsLoading = true;
			try
			{
				// remove existing controls
				DestroyControls();
				objectsInView.Clear();

				RecreateFrameXmlPanes();

				foreach (SerializationObject serializationObject in frameXmlHierarchy.Controls)
				{
					LayoutFrameType layoutFrame = serializationObject as LayoutFrameType;
					if (ShouldDisplayObject(layoutFrame))
					{
						frameXmlPane.AddPane(VirtualControlName);
						Trace.WriteLine(String.Format("Create {0}", layoutFrame.name));
						CreateControls(serializationObject, this.RootControl);
						objectsInView.Add(serializationObject);
					}
				}
			}
			finally
			{
				this.IsLoading = false;
			}

			RootControl.RepositionUi();

		}

		private void RecreateFrameXmlPanes()
		{
			var virtualPaneNames = from layoutFrame in frameXmlHierarchy.Controls.OfType<LayoutFrameType>()
								   where layoutFrame.@virtual
								   select layoutFrame.name;

			frameXmlPane.RecreatePanes(virtualPaneNames, VirtualControlName);
		}

		private void DestroyControls()
		{
			var controls = this.RootControl.Controls.OfType<BaseControl>();
			foreach (BaseControl control in controls.ToList<BaseControl>())
			{
				Trace.WriteLine(String.Format("Destroy {0}", control.Name));
				control.DesignerLoader.Host.DestroyComponent(control);
			}
		}

		private Control CreateControls(SerializationObject serializationObject, Control parent)
		{
			return CreateControls(serializationObject, parent, false);
		}

		/// <summary>
		/// Creates the controls from the hierarchy of Serialization object.
		/// </summary>
		/// <param name="serializationObject">The serialization object.</param>
		/// <param name="parent">The parent.</param>
		/// <remarks>Recursive</remarks>
		private Control CreateControls(SerializationObject serializationObject, Control parent, bool inherited)
		{
			Control control = this.CreateControl(serializationObject, parent, inherited) as Control;
			// bypass controls that cannot be created by the control factory (and the virtual ones)
			if (control == null)
			{
				Debug.WriteLine(String.Format("Bypassing object '{0}' during control creation.", serializationObject));
			}
			else
			{
				parent = control;
			}

			CreateLayers(serializationObject, parent, false);
			foreach (SerializationObject childItem in serializationObject.Controls)
			{
				CreateControls(childItem, parent, inherited);
			}
			
			LayoutFrameType layoutFrame = serializationObject as LayoutFrameType;
			if (layoutFrame != null)
			{
				LayoutFrameType inheritedLayoutFrame = layoutFrame.InheritedObject;
				if (inheritedLayoutFrame != null)
				{
					CreateLayers(inheritedLayoutFrame, parent, true);
					foreach (SerializationObject childItem in inheritedLayoutFrame.Controls)
					{
						CreateControls(childItem, parent, true);
					}
				}
			}

			return control;
		}

        /// <summary>
        /// Determines whether the specified serialization object should be displayed in the designer
        /// </summary>
        /// <param name="serializationObject">The serialization object.</param>
        /// <returns>
        /// 	<c>true</c> if the object should be displayed; otherwise, <c>false</c>.
        /// </returns>
		private bool ShouldDisplayObject(LayoutFrameType layoutFrame)
		{
			if (layoutFrame == null)
				return false;

			return
				((layoutFrame.@virtual && (VirtualControlName == layoutFrame.name))
				|| (!layoutFrame.@virtual && String.IsNullOrEmpty(VirtualControlName)));
		}

		private void CreateLayers(SerializationObject serializationObject, Control parent, bool inherited)
		{
			FrameType frameType = serializationObject as FrameType;
			if (frameType != null)
			{
				foreach (FrameTypeLayers layers in frameType.LayersList)
				{
					foreach (FrameTypeLayersLayer layer in layers.Layer)
					{
						foreach (SerializationObject so in layer.Layerables)
						{
							ILayerable layerable = CreateControl(so, parent, inherited) as ILayerable;
							layerable.LayerLevel = layer.level;
						}
					}
				}
			}
		}
	}
}
