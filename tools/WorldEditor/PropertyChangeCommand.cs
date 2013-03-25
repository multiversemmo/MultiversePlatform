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
using System.ComponentModel;

namespace Multiverse.Tools.WorldEditor
{
    /// <summary>
    /// This command uses reflection to provide a general interface for property changes.
    /// 
    /// This prevents having to have commands for each property that might be changed.
    /// </summary>
    public class PropertyChangeCommand : ICommand
    {
        private object previousValue;
        private object newValue;
        private object component;
        //private PropertyDescriptor propertyDescriptor;
        private WorldEditor app;
        private String propertyName;

        public PropertyChangeCommand(WorldEditor worldEditor, object component, PropertyDescriptor property, object newValue, object previousValue)
        {
            this.app = worldEditor;
            this.component = component;
            //this.propertyDescriptor = property;
            this.newValue = newValue;
            this.previousValue = previousValue;
            propertyName = property.Name;
        }

        #region ICommand Members

        public bool Undoable()
        {
            return true;
        }

        public void Execute()
        {
            PropertyDescriptor propertyDescriptor = TypeDescriptor.GetProperties(component)[propertyName];

            propertyDescriptor.SetValue(component, newValue);
            app.UpdatePropertyGrid();
        }

        public void UnExecute()
        {
            PropertyDescriptor propertyDescriptor = TypeDescriptor.GetProperties(component)[propertyName];

            propertyDescriptor.SetValue(component, previousValue);
            app.UpdatePropertyGrid();
        }

        #endregion
    }
}
