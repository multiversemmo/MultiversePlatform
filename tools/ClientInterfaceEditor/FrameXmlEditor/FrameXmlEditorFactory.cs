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
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Runtime.InteropServices;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Designer.Interfaces;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;

using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;
using Microsoft.MultiverseInterfaceStudio.Services;
using System.Windows.Forms;

namespace Microsoft.MultiverseInterfaceStudio.FrameXml
{
    [ComVisible(true)]
    [Guid(GuidStrings.FrameXmlEditorFactory)]
    public class FrameXmlEditorFactory : IVsEditorFactory
    {
        private FrameXmlEditorPackage package;
        private ServiceProvider serviceProvider;

        public FrameXmlEditorFactory(FrameXmlEditorPackage package)
        {
            if (package == null)
                throw new ArgumentNullException("package");

            this.package = package;
        }

        public virtual int SetSite(IOleServiceProvider serviceProvider)
        {
            this.serviceProvider = new ServiceProvider(serviceProvider);
            return VSConstants.S_OK;
        }

        public virtual object GetService(Type serviceType)
        {
            return serviceProvider.GetService(serviceType);
        }

        public int MapLogicalView(ref Guid logicalView, out string physicalView)
        {
            // Initialize out parameter
            physicalView = null;

            bool isSupportedView = false;

            // Determine the physical view
            if (VSConstants.LOGVIEWID_Primary == logicalView)
            {
                physicalView = "Design";
                isSupportedView = true;
            }
            else if (VSConstants.LOGVIEWID_Designer == logicalView)
            {
                physicalView = "Design";
                isSupportedView = true;
            }
            else if (VSConstants.LOGVIEWID_TextView == logicalView)
            {
                isSupportedView = true;
            }
            else if (VSConstants.LOGVIEWID_Code == logicalView)
            {
                isSupportedView = true;
            }

            return isSupportedView ? VSConstants.S_OK : VSConstants.E_NOTIMPL;
        }

        public int Close()
        {
            return VSConstants.S_OK;
        }

        public virtual int CreateEditorInstance(
                        uint createEditorFlags,
                        string documentMoniker,
                        string physicalView,
                        IVsHierarchy hierarchy,
                        uint itemid,
                        IntPtr docDataExisting,
                        out IntPtr docView,
                        out IntPtr docData,
                        out string editorCaption,
                        out Guid commandUIGuid,
                        out int createDocumentWindowFlags)
        {
            // Initialize out parameters
            docView = IntPtr.Zero;
            docData = IntPtr.Zero;
            commandUIGuid = Guids.FrameXmlEditorFactory;
            createDocumentWindowFlags = 0;
            editorCaption = null;

            // Validate inputs
            if ((createEditorFlags & (VSConstants.CEF_OPENFILE | VSConstants.CEF_SILENT)) == 0)
                return VSConstants.E_INVALIDARG;

            // Get the text buffer
            IVsTextLines textLines = GetTextBuffer(docDataExisting, documentMoniker);

            // Assign docData IntPtr to either existing docData or the new text buffer
            if (docDataExisting != IntPtr.Zero)
            {
                docData = docDataExisting;
                Marshal.AddRef(docData);
            }
            else
            {
                docData = Marshal.GetIUnknownForObject(textLines);
            }

            try
            {
                docView = CreateDocumentView(physicalView, hierarchy, itemid, textLines, out editorCaption, out commandUIGuid, documentMoniker);
            }
            finally
            {
                if (docView == IntPtr.Zero)
                {
                    if (docDataExisting != docData && docData != IntPtr.Zero)
                    {
                        // Cleanup the instance of the docData that we have addref'ed
                        Marshal.Release(docData);

                        docData = IntPtr.Zero;
                    }
                }
            }

            return VSConstants.S_OK;
        }

        private IVsTextLines GetTextBuffer(IntPtr docDataExisting, string documentMoniker)
        {
            IVsTextLines textLines;

            if (docDataExisting == IntPtr.Zero)
            {
                // Create a new IVsTextLines buffer
                textLines = this.CreateInstance<IVsTextLines, VsTextBufferClass>();

                // set the buffer's site
                ((IObjectWithSite)textLines).SetSite(serviceProvider.GetService(typeof(IOleServiceProvider)));

                // Fcuk COM
                Guid GUID_VsBufferMoniker = typeof(IVsUserData).GUID;

                // Explicitly load the data through IVsPersistDocData
                ((IVsPersistDocData)textLines).LoadDocData(documentMoniker);
            }
            else
            {
                // Use the existing text buffer
                object dataObject = Marshal.GetObjectForIUnknown(docDataExisting);

                textLines = dataObject as IVsTextLines;

                if (textLines == null)
                {
                    // Try get the text buffer from textbuffer provider
                    IVsTextBufferProvider textBufferProvider = dataObject as IVsTextBufferProvider;

                    if (textBufferProvider != null)
                        textBufferProvider.GetTextBuffer(out textLines);
                }

                if (textLines == null)
                {
                    // Unknown docData type then, so we have to force VS to close the other editor.
                    ErrorHandler.ThrowOnFailure(VSConstants.VS_E_INCOMPATIBLEDOCDATA);
                }
            }

            return textLines;
        }

        private IntPtr CreateDocumentView(string physicalView, IVsHierarchy hierarchy, uint itemid, IVsTextLines textLines, out string editorCaption, out Guid cmdUI, string documentPath)
        {
            //Init out params
            editorCaption = String.Empty;
            cmdUI = Guid.Empty;

            if (String.IsNullOrEmpty(physicalView))
            {
                // create code window as default physical view
                return this.CreateCodeView(textLines, ref editorCaption, ref cmdUI);
            }
            else if (String.Compare(physicalView, "design", true, CultureInfo.InvariantCulture) == 0)
            {
				try
				{
					// Create Form view
					return this.CreateDesignerView(hierarchy, itemid, textLines, ref editorCaption, ref cmdUI, documentPath);
				}
				catch(InvalidOperationException ex)
				{
					var message = String.Format(VSPackage.OPEN_CODE_EDITOR, ex.Message);

					var openCodeEditor = MessageBox.Show(message, null, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes;
					if (openCodeEditor)
					{
						// Create Code view instead
						return this.CreateCodeView(textLines, ref editorCaption, ref cmdUI);
					}
					else
					{
						throw;
					}
				}
            }

            // We couldn't create the view
            // Return special error code so VS can try another editor factory.
            ErrorHandler.ThrowOnFailure(VSConstants.VS_E_UNSUPPORTEDFORMAT);

            return IntPtr.Zero;
        }


        private IntPtr CreateDesignerView(IVsHierarchy hierarchy, uint itemid, IVsTextLines textLines, ref string editorCaption, ref Guid cmdUI, string documentMoniker)
        {
            // Request the Designer Service
            IVSMDDesignerService designerService = (IVSMDDesignerService)GetService(typeof(IVSMDDesignerService));

            try
            {
                // Get the service provider
                IOleServiceProvider provider = serviceProvider.GetService(typeof(IOleServiceProvider)) as IOleServiceProvider;

                // Create loader for the designer
                FrameXmlDesignerLoader designerLoader = new FrameXmlDesignerLoader(textLines, documentMoniker, itemid);

                // Create the designer using the provider and the loader
                IVSMDDesigner designer = designerService.CreateDesigner(provider, designerLoader);

#if !HIDE_FRAME_XML_PANE
                // Retrieve the design surface
                DesignSurface designSurface = (DesignSurface)designer;

                // Create pane with this surface
                FrameXmlPane frameXmlPane = new FrameXmlPane(designSurface);

				designerLoader.InitializeFrameXmlPane(frameXmlPane);

				// Get command guid from designer
				cmdUI = frameXmlPane.CommandGuid;
				editorCaption = " [Design]";

				// Return FrameXmlPane
				return Marshal.GetIUnknownForObject(frameXmlPane);
#else
                object view = designer.View;

                cmdUI = designer.CommandGuid;
                editorCaption = " [Design]";

				designerLoader.InitializeFrameXmlPane(null);
                // Return view
                return Marshal.GetIUnknownForObject(view);
#endif
			}
            catch (Exception ex)
            {
                // Just rethrow for now
                throw;
            }
        }

        private IntPtr CreateCodeView(IVsTextLines textLines, ref string editorCaption, ref Guid cmdUI)
        {
            IVsCodeWindow window = this.CreateInstance<IVsCodeWindow, VsCodeWindowClass>();

            ErrorHandler.ThrowOnFailure(window.SetBuffer(textLines));
            ErrorHandler.ThrowOnFailure(window.SetBaseEditorCaption(null));
            ErrorHandler.ThrowOnFailure(window.GetEditorCaption(READONLYSTATUS.ROSTATUS_Unknown, out editorCaption));

            cmdUI = VSConstants.GUID_TextEditorFactory;

            return Marshal.GetIUnknownForObject(window);
        }

        private TInterface CreateInstance<TInterface, TClass>()
            where TInterface : class
            where TClass : class
        {
            Guid clsid = typeof(TClass).GUID;
            Guid riid = typeof(TInterface).GUID;

            return (TInterface)package.CreateInstance(ref clsid, ref riid, typeof(TInterface));
        }
    }
}
