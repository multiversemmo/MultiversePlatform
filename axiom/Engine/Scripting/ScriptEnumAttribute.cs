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
using System.Reflection;
using System.Text;

namespace Axiom.Scripting {
    #region Delegates

    public delegate void AttributeParserMethod(string[] values, params object[] objects);

    #endregion Delegates
    /// <summary>
    ///		This attribute is intended to be used on enum fields for enums that can be used
    ///		in script files (.material, .overlay, etc).  Placing this attribute on the field will
    ///		allow the script parsers to look up a real enum value based on the value as it is
    ///		used in the script.
    /// </summary>
    /// <remarks>
    ///		For example, texturing addressing mode can base used in .material scripts, and
    ///		the values in the script are 'wrap', 'clamp', and 'mirror'.
    ///		<p/>
    ///		The TextureAddress enum fields are defined with attributes to create the mapping
    ///		between the scriptable values and their real enum values.
    ///		<p/>
    ///		...
    ///		[ScriptEnum("wrap")]
    ///		Wrap
    ///		...
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public sealed class ScriptEnumAttribute : Attribute {
        private string scriptValue;

        /// <summary>
        ///		
        /// </summary>
        /// <param name="val">The value as it will appear when used in script files (.material, .overlay, etc).</param>
        public ScriptEnumAttribute(string val) {
            this.scriptValue = val;
        }

        public string ScriptValue {
            get { return scriptValue; }
        }

        /// <summary>
        ///		Returns an actual enum value for a enum that can be used in script files.
        /// </summary>
        /// <param name="val"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object Lookup(string val, Type type) {
            // get the list of fields in the enum
            FieldInfo[] fields = type.GetFields();

            // loop through each one and see if it is mapped to the supplied value
            for(int i = 0; i < fields.Length; i++) {
                FieldInfo field = fields[i];
				
                // find custom attributes declared for this field
                object[] atts = field.GetCustomAttributes(typeof(ScriptEnumAttribute), false);

                // if we found 1, take a look at it
                if(atts.Length > 0) {
                    // convert the first element to the right type (assume there is only 1 attribute)
                    ScriptEnumAttribute scriptAtt = (ScriptEnumAttribute)atts[0];

                    // if the values match
                    if(scriptAtt.ScriptValue == val) {
                        // return the enum value for this script equivalent
                        return Enum.Parse(type, field.Name, true);
                    }
                } // if
            } // for

            //	invalid enum value
            return null;
        }

		/// <summary>
		///		Returns a string describing the legal values for a particular enum.
		/// </summary>
		/// <param name="type"></param>
		/// <returns>
		///		A string containing legal values for script file.  
		///		i.e. "'none', 'clockwise', 'anticlockwise'"
		/// </returns>
		public static string GetLegalValues(Type type) {
			StringBuilder legalValues = new StringBuilder();

			// get the list of fields in the enum
			FieldInfo[] fields = type.GetFields();

			// loop through each one and see if it is mapped to the supplied value
			for(int i = 0; i < fields.Length; i++) {
				FieldInfo field = fields[i];
				
				// find custom attributes declared for this field
				object[] atts = field.GetCustomAttributes(typeof(ScriptEnumAttribute), false);

				// if we found 1, take a look at it
				if(atts.Length > 0) {
					// convert the first element to the right type (assume there is only 1 attribute)
					ScriptEnumAttribute scriptAtt = (ScriptEnumAttribute)atts[0];

					// if the values match
					legalValues.AppendFormat("'{0}',", scriptAtt.ScriptValue);
				} // if
			} // for

			// return the full string
			if(legalValues.Length == 0)
				return "(No values found for type " + type.Name.ToString() + ")";
			else
				return legalValues.ToString(0, legalValues.Length - 1);
		}
    }

    /// <summary>
    ///		Custom attribute to mark methods as handling the parsing for a material script attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class AttributeParserAttribute : Attribute {
        private string attributeName;
        private string parserType;

        public AttributeParserAttribute(string name, string parserType) {
            this.attributeName = name;
            this.parserType = parserType;
        }

        public string Name {
            get { return attributeName; }
        }

        public string ParserType {
            get { return parserType; }
        }
    }
}