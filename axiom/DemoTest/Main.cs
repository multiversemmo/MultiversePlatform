#region LGPL License
/*
Axiom Game Engine Library
Copyright (C) 2003  Axiom Project Team

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
using System.Globalization;
using System.Threading;
using Axiom.Utility;

namespace Demos {

    /// <summary>
    ///     Demo browser entry point.
    /// </summary>
    public class DemoTest {
        [STAThread]
        private static void Main(string[] args) {
            try {
				//using(DemoBrowser browser = new DemoBrowser()) {
				//	browser.Start();
				//}

				// Change me to whatever demo you want to run for the meantime until the new browser is done
				Type demoType = Type.GetType("Axiom.Demos.Shadows");
//				Type demoType = Type.GetType("Axiom.Demos.FacialAnimation");
//				Type demoType = Type.GetType("Axiom.Demos.ParticleFX");

				using(DemoBase demo = (DemoBase)Activator.CreateInstance(demoType)) {
					demo.Start();
				}
            }
            catch(Exception ex) {
				Console.WriteLine(ex.ToString());
				Console.WriteLine("An exception has occurred.  Press enter to continue...");
				Console.ReadLine();
            }
        }
    }

}