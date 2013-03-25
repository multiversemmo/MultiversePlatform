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

#region Using directives

using System;
using System.Diagnostics;
using System.Text;
using System.IO;

using Vector3 = Axiom.MathLib.Vector3;
using Matrix3 = Axiom.MathLib.Matrix3;

#endregion

namespace Multiverse.CollisionLib
{

    public class MO
    {
        public const float Meter = 1000.0f;
    
        public const float DivMeter = 1.0f/1000.0f;

        // Create a logger for use in this class
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(MO));
        
        public static string MeterString(Vector3 v)
        {
            return string.Format("({0:#.0},{1:#.0},{2:#.0})", (v.x/Meter), (v.y/Meter), (v.z/Meter));
        }
    
        public static string MeterString(float f)
        {
            return string.Format("{0}", (int)(f/Meter));
        }
    
        public static string MeterFractionString(Vector3 v)
        {
            return string.Format("({0:#.00},{1:#.00},{2:#.00})", v.x/Meter, v.y/Meter, v.z/Meter);
        }
    
        public static string MeterFractionString(float f)
        {
            return string.Format("{0:#.00}", f/Meter);
        }
    
        public static string AxisString(Vector3 v)
        {
            string s = "(";
            for (int i=0; i<3; i++) {
                float f = v[i];
                if (f == 0f || f == 1f || f == -1f)
                    s += (int)f;
                else
                    s += string.Format("{0:#.000}", f);
                if (i < 2)
                    s += ",";
            }
            return s + ")";
        }
    
        public static float InchesToMeters(float inches)
		{
			return Meter * inches / 39.36f;
		}
		
		public static string HandleString(long handle)
		{
			if (handle < 0) {
				long combined = -handle;
				int t1 = (int)(combined >> 32);
				if ((t1 & -0x40000000) != 0)
					t1 = - (t1 | unchecked((int)0x80000000));
				int t2 = (int)(combined & 0x7fffffff);
				if ((t2 & 0x40000000) != 0)
					t2 = - (t2 | unchecked((int)0x80000000));
				return string.Format("({0},{1})", t1, t2);
			}
			else
				return string.Format("{0}", handle);
		}
		
		public static string Bool(bool b)
        {
            return (b ? "true" : "false");
        }

        public static StreamWriter writer;
        
        public static bool DoLog = false;
#if NOT
        public static void InitLog(bool b)
        {
            DoLog = b;
            string p = "../CollisionLog.txt";
			if (DoLog) {
                FileStream f = new FileStream(p,
											  (File.Exists(p) ? FileMode.Append : FileMode.Create), 
											  FileAccess.Write);
                writer = new StreamWriter(f);
				writer.Write(string.Format("\n\n\n\n{0} Started writing to {1}\n",
										   DateTime.Now.ToString("hh:mm:ss"), p));
			}
        }
        
        public static void CloseLog()
        {
            if (writer != null)
                writer.Close();
        }
#endif
        public static void Log(string format, params Object[] list)
        {
#if NOT
			if (writer != null) {
                string s = string.Format(format, list);
                writer.Write(string.Format("{0} {1}\n",
                                           DateTime.Now.ToString("hh:mm:ss"), s));
                writer.Flush();
            }
            else
#endif
                log.InfoFormat(format, list);
        }
    }
}
