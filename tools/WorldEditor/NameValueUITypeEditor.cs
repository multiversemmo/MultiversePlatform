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
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.ComponentModel;
using System.Drawing.Design;
using System.Collections.Specialized;
using Multiverse.ToolBox;

namespace Multiverse.Tools.WorldEditor
{

	public class NameValueUITypeEditor : UITypeEditor
	{
		public string type;
		public static NameValueTemplateCollection nvt;
		public NameValueObject nvc;
	    public bool exit = false;
		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{

			if (context != null
				&& context.Instance != null
				&& provider != null)
			{

				IWindowsFormsEditorService edSvc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));

				if (edSvc != null)
				{
					NameValueObject nvc = value as NameValueObject;
					if (nvc != null)
					{
						 NameValueDialog nameValueEditor = new NameValueDialog(nvc,type,nvt);

					    do
					    {
					        DialogResult dlgRes = edSvc.ShowDialog(nameValueEditor);
					        if (dlgRes == DialogResult.OK)
					        {
					            return nameValueEditor.NameValueCollection;
					        }
					        if (MessageBox.Show("Closing this window will remove all the changes made.",
					                            "Close Window?",
					                            MessageBoxButtons.OKCancel) == DialogResult.Cancel)
					        {
					            exit = false;
					        }
					        else
					        {
					            exit = true;
					        }
					    } while (!exit) ;
					}
				}
			}

			return value;
		}

		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			if (context != null && context.Instance != null)
			{
				return UITypeEditorEditStyle.Modal;
			}
			return base.GetEditStyle(context);
		}
	}

	public class NameValueUITypeEditorObject : NameValueUITypeEditor
	{
		public NameValueUITypeEditorObject()
		{
			type = "Object";
		}
	}
	

    public class NameValueUITypeEditorBoundary : NameValueUITypeEditor
    {
        public NameValueUITypeEditorBoundary()
        {
            type = "Region";
        }
    }

    public class NameValueUITypeEditorMarker : NameValueUITypeEditor
    {
        public NameValueUITypeEditorMarker()
        {
            type = "Marker";
        }
    }

    public class NameValueUITypeEditorMob : NameValueUITypeEditor
    {
        public NameValueUITypeEditorMob()
        {
            type = "Mob";
        }
    }

    public class NameValueUITypeEditorRoad : NameValueUITypeEditor
    {
        public NameValueUITypeEditorRoad()
        {
            type = "Road";
        }
    }

}
