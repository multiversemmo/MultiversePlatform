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
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.MultiverseInterfaceStudio.FrameXml.Controls;
using Microsoft.MultiverseInterfaceStudio.FrameXml.Serialization;
using Microsoft.MultiverseInterfaceStudio.Services;

namespace Microsoft.MultiverseInterfaceStudio.FrameXml
{
    public partial class FrameXmlDesignerLoader : BasicDesignerLoader
	{
		#region members

		private const string hostedBaseClassName = "UserControl";

		/// <summary>
		/// Publishes the loader host
		/// </summary>
		/// <value>The loader host.</value>
		public IDesignerLoaderHost Host
		{
			get { return this.LoaderHost; }
		}

		public string DocumentMoniker { get; private set; }

		private IVsTextLines textLines;

		private uint ItemID { get; set; }

		#endregion

		#region construction and dispose

		/// <summary>
		/// Holds a reference for every designer loader opened. This map is indexed by itemId.
		/// </summary>
		private static SortedDictionary<uint, FrameXmlDesignerLoader> designerLoaders = new SortedDictionary<uint, FrameXmlDesignerLoader>();

		/// <summary>
		/// Represents the ItemID of the active designer. It is set by the SelectionEventsMonitor
		/// </summary>
		/// <value>The active designer's itemID.</value>
		public static uint ActiveItemID { get; set; }

		/// <summary>
		/// Gets the active designer loader.
		/// </summary>
		/// <value>The active designer loader.</value>
		public static FrameXmlDesignerLoader ActiveDesignerLoader
		{
			get
			{
				if (designerLoaders.ContainsKey(ActiveItemID))
					return designerLoaders[ActiveItemID];

				return null;
			}
		}

		public FrameXmlDesignerLoader(IVsTextLines textLines, string documentMoniker, uint itemid)
		{
			this.textLines = textLines;
			this.DocumentMoniker = documentMoniker;
			this.ItemID = itemid;

            if (designerLoaders.Remove(itemid))
            {
                Trace.WriteLine(String.Format("A designer loader with the id {0} already existsed and has been removed.", itemid));
            }
			designerLoaders.Add(itemid, this);
			ActiveItemID = itemid;

			this.IsSerializing = true;
		}

		public override void Dispose()
		{
			this.IsLoading = true;
			try
			{
				designerLoaders.Remove(this.ItemID);
				DestroyControls();
			}
			finally
			{
				this.IsLoading = false;
				base.Dispose();
			}
		}

		#endregion

		#region serialization

		private static void ValidateOnWrite(string xml)
		{
			XmlReaderSettings readerSettings = Serialization.XmlSettings.CreateReaderSettings();

			using (StringReader stringReader = new StringReader(xml))
			using (XmlReader reader = XmlReader.Create(stringReader, readerSettings))
			{
				while (reader.Read()) ;
			}

		}

		public string VirtualControlName { get; set; }

		public bool IsSerializing { get; set; }

		private string SerializeFrameXml(Serialization.SerializationObject ui)
        {
            const string xmlIndentChars = "\t";

            StringBuilder sb = new StringBuilder();

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = xmlIndentChars;
			settings.OmitXmlDeclaration = true;

			this.IsSerializing = true;
			try
			{
				using (XmlWriter writer = XmlWriter.Create(sb, settings))
				{
					XmlSerializer serializer = new XmlSerializer(typeof(Serialization.Ui));
					serializer.Serialize(writer, ui);
				}
			}
			finally
			{
				this.IsSerializing = false;
			}
            return sb.ToString();
        }

		private Serialization.Ui DeserializeFrameXml(string buffer)
		{
			var useSchema = true;
			return DeserializeFrameXml(buffer, useSchema);
		}

        private Serialization.Ui DeserializeFrameXml(string buffer, bool useSchema)
        {
			XmlReaderSettings readerSettings = Serialization.XmlSettings.CreateReaderSettings(ref useSchema);
			this.IsSerializing = true;
			try
			{
				using (StringReader stringReader = new StringReader(buffer))
				using (XmlReader xmlReader = XmlReader.Create(stringReader, readerSettings))
				{

					XmlSerializer serializer = new XmlSerializer(typeof(Serialization.Ui));
					var ui = (Serialization.Ui)serializer.Deserialize(xmlReader);

					PostProcessSerializationHierarchy(ui);
					return ui;
				}
			}
			catch (InvalidOperationException exception)
			{
                
				if (useSchema)
				{
					var message = exception.InnerException == null ?
						exception.Message :
						exception.InnerException.Message;
					message = String.Format(VSPackage.DISABLE_SCHEMA, message);

					bool tryWithoutSchema = MessageBox.Show(message, null, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

					if (tryWithoutSchema)
						return DeserializeFrameXml(buffer, false);
				}

				if (exception.InnerException != null)
					throw exception.InnerException;

				throw;
			}
			finally
			{
				this.IsSerializing = false;
			}
        }

		private void PostProcessSerializationHierarchy(Serialization.Ui ui)
		{
			foreach (var layoutFrame in ui.Controls.OfType<LayoutFrameType>())
			{
				PostProcessSerializationHierarchy(layoutFrame, null);
			}
		}

		private void PostProcessSerializationHierarchy(LayoutFrameType layoutFrame, LayoutFrameType parent)
		{
			layoutFrame.Parent = parent;
			foreach (var child in layoutFrame.Children)
			{
				PostProcessSerializationHierarchy(child, layoutFrame);
			}
		}

        /// <summary>
        /// Retrieves the serialization object hierarchy from the controls
        /// </summary>
        /// <returns></returns>
        private static SerializationObject GetSerializationObject(Controls.ISerializableControl serializableControl)
        {
            SerializationObject serializableObject = serializableControl.SerializationObject;

			serializableObject.Controls.Clear();

			Controls.IFrameControl parentFrame = serializableControl as Controls.IFrameControl;
            if (parentFrame != null)
            {
                if (parentFrame.Frames.Count<Controls.IFrameControl>() > 0)
                {
                    FrameTypeFrames frames = new FrameTypeFrames();
					serializableObject.Controls.Add(frames);
					foreach (var childFrame in parentFrame.Frames)
                    {
                        SerializationObject childObject = GetSerializationObject(childFrame);
                        frames.Controls.Add(childObject);
                    }
                }

                Serialization.FrameType frameType = serializableObject as Serialization.FrameType;
                if (frameType != null)
                {
                    Dictionary<DRAWLAYER, FrameTypeLayersLayer> layerDictionary = new Dictionary<DRAWLAYER, FrameTypeLayersLayer>();

                    foreach (Controls.ILayerable layerable in parentFrame.Layerables)
                    {
                        if (!layerDictionary.ContainsKey(layerable.LayerLevel))
                            layerDictionary.Add(layerable.LayerLevel, new FrameTypeLayersLayer());

                        FrameTypeLayersLayer layer = layerDictionary[layerable.LayerLevel];
                        layer.level = layerable.LayerLevel;
						layer.Layerables.Add(layerable.SerializationObject);
                    }

                    frameType.LayersList.Clear();
                    if (layerDictionary.Count > 0)
                    {
                        FrameTypeLayers layers = new FrameTypeLayers();
                        layers.Layer.AddRange(layerDictionary.Values);
                        frameType.LayersList.Add(layers);
                    }
                }
            }

            return serializableObject;
		}

		/// <summary>
		/// Retrieves the serialization object hierarchy of the root control.
		/// </summary>
		/// <returns></returns>
		private Serialization.Ui GetSerializationObject()
		{
			Controls.Ui rootControl = this.RootControl;
			Serialization.Ui rootObject = rootControl.TypedSerializationObject;

			// remove objects that are shown in the view - they will be serialized
			rootObject.Controls.RemoveAll(obj => objectsInView.Contains(obj));

			List<SerializationObject> objects = new List<SerializationObject>(rootObject.Controls);

			foreach (Controls.ISerializableControl childControl in rootControl.BaseControls)
			{
				Serialization.SerializationObject childObject = GetSerializationObject(childControl);
				objects.Add(childObject);
				
				// add eventual new controls to the view
				if (!objectsInView.Contains(childObject))
					objectsInView.Add(childObject);
			}

			rootObject.Controls.Clear();
			rootObject.Controls.AddRange(objects.SortRootObjects());

			return rootObject;
		}

		#endregion

		#region document handling

		private string GetText()
        {
            int line, index;
            string buffer;

            if (textLines.GetLastLineIndex(out line, out index) != VSConstants.S_OK)
                return String.Empty;
            if (textLines.GetLineText(0, 0, line, index, out buffer) != VSConstants.S_OK)
                return String.Empty;

            return buffer;
        }

        private void SetText(string text)
        {
			try
			{
				FrameXmlDesignerLoader.ValidateOnWrite(text);
			}
			catch (Exception ex)
			{
				// TODO: show error in the Error List
				Trace.WriteLine(ex);
			}

			int endLine, endCol;
            textLines.GetLastLineIndex(out endLine, out endCol);
            int len = (text == null) ? 0 : text.Length;

            //fix location of the string
            IntPtr pText = Marshal.StringToCoTaskMemAuto(text);
            try
            {
                textLines.ReplaceLines(0, 0, endLine, endCol, pText, len, null);
            }
            finally
            {
                Marshal.FreeCoTaskMem(pText);
            }
        }

        private string Buffer
        {
            get { return GetText(); }
            set
			{
				SetText(value);
			}
        }

		private Serialization.Ui frameXmlHierarchy = null;

        /// <summary>
        /// Loads a designer from persistence.
        /// </summary>
        /// <param name="serializationManager">An <see cref="T:System.ComponentModel.Design.Serialization.IDesignerSerializationManager"/> to use for loading state for the designers.</param>
        protected override void PerformLoad(IDesignerSerializationManager serializationManager)
        {
            // The loader will put error messages in here.
			ICollection errors = new ArrayList();
			bool successful = true;

            Trace.WriteLine("PerformLoad");

			frameXmlHierarchy = DeserializeFrameXml(this.Buffer);
			this.LayoutFrames = new LayoutFrameCollection(frameXmlHierarchy);

			this.CreateRootControl(frameXmlHierarchy);

            // Query for the settings service and get the background image setting
            IInterfaceStudioSettings settings = GetService(typeof(IInterfaceStudioSettings)) as IInterfaceStudioSettings;
            if (settings != null)
                this.RootControl.BackgroundImagePath = settings.BackgroundImageFile;

            // Add PropertyValueUIHandler to PropertyValueUIService
            this.AddPropertyValueUIHandler();

            // Let the host know we are done loading.
            Host.EndLoad(FrameXmlDesignerLoader.hostedBaseClassName, successful, errors);
		}

        private void AddPropertyValueUIHandler()
        {
            IPropertyValueUIService uiService = (IPropertyValueUIService)Host.GetService(typeof(IPropertyValueUIService));
            if (uiService != null)
                uiService.AddPropertyValueUIHandler(new PropertyValueUIHandler(this.InheritedPropertyValueUIHandler));
        }

		private FrameXmlPane frameXmlPane = null;

		public void InitializeFrameXmlPane(FrameXmlPane frameXmlPane)
		{
			this.frameXmlPane = frameXmlPane;
			ReloadControls();

            if (frameXmlPane != null)
            {
                frameXmlPane.SelectedPaneChanged += new EventHandler(frameXmlPane_SelectedPaneChanged);
            }
		}

		void frameXmlPane_SelectedPaneChanged(object sender, EventArgs e)
		{
			this.VirtualControlName = frameXmlPane.SelectedPane;
			ReloadControls();
		}

		/// <summary>
		/// Flushes all changes to the designer.
		/// </summary>
		/// <param name="serializationManager">An <see cref="T:System.ComponentModel.Design.Serialization.IDesignerSerializationManager"/> to use for persisting the state of loaded designers.</param>
		protected override void PerformFlush(IDesignerSerializationManager serializationManager)
		{
			bool success = true;
			ArrayList errors = new ArrayList();
			IDesignerHost idh = (IDesignerHost)this.Host.GetService(typeof(IDesignerHost));

			Controls.ISerializableControl serializable = (LoaderHost.RootComponent as Controls.ISerializableControl);
			if (serializable == null)
			{
				throw new ApplicationException("Invalid root control type in designer.");
			}

			Serialization.SerializationObject serializationObject = this.GetSerializationObject();

			try
			{
				this.Buffer = this.SerializeFrameXml(serializationObject);
			}
			catch (Exception exception)
			{
				Debug.WriteLine(exception);

				success = false;
				errors.Add(exception);
			}

			IDesignerLoaderHost host = this.LoaderHost;
			host.EndLoad(FrameXmlDesignerLoader.hostedBaseClassName, success, errors);

			Trace.WriteLine("PerformFlush");

		}

		#endregion

		#region inherits support

		public LayoutFrameCollection LayoutFrames { get; private set; }

		#endregion

		public void NotifyControlRemoval(Control control)
		{
			if (!IsLoading)
			{
				OnPropertyChanged(control, new PropertyChangedEventArgs("deleted"));
			}
		}

		#region property change handling

		public void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "virtual":
					OnVirtualChanged(sender);
					break;
				case "name":
					OnNameChanged(sender);
					break;
			}
			Serialization.SerializationObject ui = GetSerializationObject();
			this.Buffer = this.SerializeFrameXml(ui);

            // Refresh the property browser
            this.RefreshPropertyBrowser();
		}

        public void RefreshPropertyBrowser()
        {
            IVsUIShell shell = this.GetService(typeof(SVsUIShell)) as IVsUIShell;
            if (shell != null)
                shell.RefreshPropertyBrowser(0);
        }

        private void OnNameChanged(object sender)
		{
			BaseControl baseControl = sender as BaseControl;
			if (baseControl == null)
				return;

			if (baseControl.LayoutFrameType.@virtual)
			{
				VirtualControlName = baseControl.LayoutFrameType.name;
				RecreateFrameXmlPanes();
			}
				
		}

		private void OnVirtualChanged(object sender)
		{
			BaseControl baseControl = sender as BaseControl;
			if (baseControl == null)
				return;

			var paneName = baseControl.LayoutFrameType.@virtual ?
				baseControl.Name : null;

			VirtualControlName = paneName;
			ReloadControls();
		}

		#endregion

        private void InheritedPropertyValueUIHandler(ITypeDescriptorContext context, PropertyDescriptor propertyDescriptor, ArrayList itemList)
        {
            if (propertyDescriptor.Attributes.Cast<Attribute>().Any(attribute => attribute is InheritedAttribute))
            {
                Bitmap bitmap = VSPackage.InheritedGlyph;
                bitmap.MakeTransparent();

                // Add glyph, no click event handler for now
                itemList.Add(new PropertyValueUIItem(bitmap, delegate { }, "Inherited Property"));
            }
        }
	}
}
