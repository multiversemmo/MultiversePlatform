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
using System.Collections;
using System.Collections.Generic;
using Axiom.Controllers;
using Axiom.Core;
using Axiom.MathLib;

namespace Axiom.Graphics {
	/// <summary>
	/// 	Collects together the program parameters used for a GpuProgram.
	/// </summary>
	/// <remarks>
	///    Gpu program state includes constant parameters used by the program, and
	///    bindings to render system state which is propagated into the constants 
	///    by the engine automatically if requested.
	///    <p/>
	///    GpuProgramParameters objects should be created through the GpuProgramManager and
	///    may be shared between multiple GpuProgram instances. For this reason they
	///    are managed using a shared pointer, which will ensure they are automatically
	///    deleted when no program is using them anymore.
	///    <p/>
	///    Different types of GPU programs support different types of constant parameters.
	///    For example, it's relatively common to find that vertex programs only support
	///    floating point constants, and that fragment programs only support integer (fixed point)
	///    parameters. This can vary depending on the program version supported by the
	///    graphics card being used. You should consult the documentation for the type of
	///    low level program you are using, or alternatively use the methods
	///    provided on Capabilities to determine the options.
	///    <p/>
	///    Another possible limitation is that some systems only allow constants to be set
	///    on certain boundaries, e.g. in sets of 4 values for example. Again, see
	///    Capabilities for full details.
	/// </remarks>
	public class GpuProgramParameters {
		#region Structs

		public struct ParameterEntry {
			public GpuProgramParameterType ParameterType;
			public string ParameterName;
		}

		#endregion
		
		#region Fields
		/// <summary>
		///    Packed list of integer constants
		/// </summary>
		protected IntConstantEntryList intConstants = new IntConstantEntryList();
		/// <summary>
		///    Table of float constants - - not pointers to entry objects
		/// </summary>
		public float[] floatConstantsArray = new float[0];
        /// <summary>
        ///    The length of the conceptual 4-float array - - to get the length for floatConstantsArray, multiply by 4.
        /// </summary
        public int float4VecConstantsCount = 0;
		/// <summary>
		///    Table of booleans - - one fro every 4 float constants
		/// </summary>
		public bool[] floatIsSet = new bool[0];
        /// <summary>
		///    The high-water mark of constants set
		/// </summary>
        public int maxSetCount = 0;
        /// <summary>
		///    List of automatically updated parameters.
		/// </summary>
		protected AutoConstantEntryList autoConstantList = new AutoConstantEntryList();
		/// <summary>
		///    Lookup of constant indicies for named parameters.
		/// </summary>
		protected Hashtable namedParams = new Hashtable();
		/// <summary>
		///     Specifies whether matrices need to be transposed prior to
		///     being sent to the hardware.
		/// </summary>
		protected bool transposeMatrices;
		/// <summary>
		///		Temp array for use when passing constants around.
		/// </summary>
		protected float[] tmpVals = new float[4];
		/// <summary>
		///		Flag to indicate if names not found will be automatically added.
		/// </summary>
		protected bool autoAddParamName;

		protected ArrayList paramTypeList = new ArrayList();
		protected ArrayList paramIndexTypes = new ArrayList();

		#endregion
		
		#region Constructors
		
		/// <summary>
		///		Default constructor.
		/// </summary>
		public GpuProgramParameters(){
		}
		
		#endregion
		
		#region Methods

        public void AddParameterToDefaultsList(GpuProgramParameterType type, string name) {
			ParameterEntry p = new ParameterEntry();
			p.ParameterType = type;
			p.ParameterName = name;
			paramTypeList.Add(p);
		}

		/// <summary>
		///    Clears all the existing automatic constants.
		/// </summary>
		public void ClearAutoConstants() {
			autoConstantList.Clear();
		}

		public GpuProgramParameters Clone()
		{
			GpuProgramParameters p = new GpuProgramParameters();

			// copy int constants
			for ( int i = 0; i < intConstants.Count; i++ ) 
			{
				IntConstantEntry e = intConstants[i];
				if ( e.isSet ) 
				{
					p.SetConstant(i, e.val);
				}
			}

			// copy float constants
			p.floatConstantsArray = new float[floatConstantsArray.Length];
            Array.Copy(floatConstantsArray, p.floatConstantsArray, floatConstantsArray.Length);
            p.floatIsSet = new bool[floatIsSet.Length];
            Array.Copy(floatIsSet, p.floatIsSet, floatIsSet.Length);
            p.float4VecConstantsCount = float4VecConstantsCount;
            p.maxSetCount = maxSetCount;

			// copy auto constants
			for(int i = 0; i < autoConstantList.Count; i++) 
			{
				AutoConstantEntry entry = autoConstantList[i];
                p.SetAutoConstant(entry.Clone());
			}

			// copy named params
			foreach ( DictionaryEntry e in namedParams ) 
			{
				p.MapParamNameToIndex(e.Key as string, (int)e.Value);
			}

			for ( int i = 0; i < paramTypeList.Count; i++ )
			{
				
			}
			foreach ( ParameterEntry pEntry in paramTypeList )
			{
				p.AddParameterToDefaultsList(pEntry.ParameterType, pEntry.ParameterName);
			}

			// copy value members
			p.transposeMatrices = transposeMatrices;
			p.autoAddParamName = autoAddParamName;

			return p;
		}

		/// <summary>
		///		Copies the values of all constants (including auto constants) from another <see cref="GpuProgramParameters"/> object.
		/// </summary>
		/// <param name="source">Set of params to use as the source.</param>
        public void CopyConstantsFrom(GpuProgramParameters source)
        {

            // copy int constants
            for (int i = 0; i < source.intConstants.Count; i++)
            {
                IntConstantEntry e = source.intConstants[i];
                if (e.isSet)
                {
                    SetConstant(i, e.val);
                }
            }

            // copy float constants
            int maxFloatIndex = source.floatConstantsArray.Length;
            floatConstantsArray = new float[maxFloatIndex];
            Array.Copy(source.floatConstantsArray, floatConstantsArray, maxFloatIndex);
            int sourceLimit = source.float4VecConstantsCount;
            floatIsSet = new bool[sourceLimit];
            Array.Copy(source.floatIsSet, floatIsSet, sourceLimit);
            float4VecConstantsCount = sourceLimit;
            maxSetCount = source.maxSetCount;

            // Iterate over auto parameters
            // Clear existing auto constants
            ClearAutoConstants();

            for (int i = 0; i < source.autoConstantList.Count; i++)
            {
                AutoConstantEntry entry = (AutoConstantEntry)source.autoConstantList[i];
                SetAutoConstant(entry.Clone());
            }

            // don't forget to copy the named param lookup as well
            namedParams = (Hashtable)source.namedParams.Clone();
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public float[] GetFloatConstant(int i) {
			if(i < floatConstantsArray.Length) {
                float[] vec = new float[4];
                int b = i * 4;
                vec[0] = floatConstantsArray[b];
                vec[1] = floatConstantsArray[b+1];
                vec[2] = floatConstantsArray[b+2];
                vec[3] = floatConstantsArray[b+3];
				return vec;
			}

			return null;
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public IntConstantEntry GetIntConstant(int i) {
			if(i < intConstants.Count) {
				return intConstants[i];
			}

			return null;
		}

		/// <summary>
		///    Gets the constant index of the specified named param.
		/// </summary>
		/// <param name="name">
		///    Name of the param.
		/// </param>
		/// <returns>
		///    Constant index.
		/// </returns>
		public int GetParamIndex(string name) {
			if(namedParams[name] == null) {
				// name not found in map, should it be added to the map?
				if(autoAddParamName) {
					// determine index
					// don't know which Constants list the name is for
					// so pick the largest index
                    int index = float4VecConstantsCount > intConstants.Count ? float4VecConstantsCount : intConstants.Count;

					ResizeFloatConstants(index);
					intConstants.Resize(index + 1);
					MapParamNameToIndex(name, index);
					return index;
				}
				else {
					throw new Exception(string.Format("Cannot find a param index for a param named '{0}'.", name));
				}
			}

			return (int)namedParams[name];
		}

		/// <summary>
		///		Given an index, this function will return the name of the parameter at that index.
		/// </summary>
		/// <param name="index">Index of the parameter to look up.</param>
		/// <returns>Name of the param at the specified index.</returns>
		public string GetNameByIndex(int index) {
			foreach(DictionaryEntry entry in namedParams) {
				if((int)entry.Value == index) {
					return (string)entry.Key;
				}
			}

			return null;
		}

		/// <summary>
		///		Returns the index of entry if the name is found otherwise returns -1.
		/// </summary>
		/// <param name="name">Name of the constant to retreive.</param>
		/// <returns>A reference to the float constant entry with the specified name, else null if not found.</returns>
		public int GetNamedFloatConstantIndex(string name) {
			if(namedParams[name] != null) {
				int index = (int)namedParams[name];
				return index;
			}

			return -1;
		}

		/// <summary>
		///		Gets a Named Int Constant entry if the name is found otherwise returns a null.
		/// </summary>
		/// <param name="name">Name of the constant to retreive.</param>
		/// <returns>A reference to the int constant entry with the specified name, else null if not found.</returns>

		public IntConstantEntry GetNamedIntConstant(string name) {
			if(namedParams[name] != null) {
				int index = (int)namedParams[name];

				return GetIntConstant(index);
			}

			return null;
		}

		/// <summary>
		///    Maps a parameter name to the specified constant index.
		/// </summary>
		/// <param name="name">Name of the param.</param>
		/// <param name="index">Constant index of the param.</param>
		public void MapParamNameToIndex(string name, int index) {
			// map the param name to a constant register index
			namedParams[name] = index;
		}

		/// <summary>
		///    Sets up a constant which will automatically be updated by the engine.
		/// </summary>
		/// <remarks>
		///    Vertex and fragment programs often need parameters which are to do with the
		///    current render state, or particular values which may very well change over time,
		///    and often between objects which are being rendered. This feature allows you 
		///    to set up a certain number of predefined parameter mappings that are kept up to 
		///    date for you.
		/// </remarks>
		/// <param name="type">The type of automatic constant to set.</param>
		/// <param name="index">
		///    The location in the constant list to place this updated constant every time
		///    it is changed. Note that because of the nature of the types, we know how big the 
		///    parameter details will be so you don't need to set that like you do for manual constants.
		/// </param>
		public void SetAutoConstant(int index, AutoConstants type) {
			SetAutoConstant(index, type, 0);
		}

        /// <summary>
        ///    Overloaded method.
        /// </summary>
        /// <param name="type">The type of automatic constant to set.</param>
        /// <param name="index">
        ///    The location in the constant list to place this updated constant every time
        ///    it is changed. Note that because of the nature of the types, we know how big the 
        ///    parameter details will be so you don't need to set that like you do for manual constants.
        /// </param>
        /// <param name="extraInfo">If the constant type needs more information (like a light index) put it here.</param>
        public void SetAutoConstant(int index, AutoConstants type, int extraInfo) {
            AutoConstantEntry entry = new AutoConstantEntry(type, index, extraInfo);
            System.Diagnostics.Debug.Assert(type != AutoConstants.SinTime_0_X);
            autoConstantList.Add(entry);
        }

        /// <summary>
        ///    Overloaded method.
        /// </summary>
        /// <param name="type">The type of automatic constant to set.</param>
        /// <param name="index">
        ///    The location in the constant list to place this updated constant every time
        ///    it is changed. Note that because of the nature of the types, we know how big the 
        ///    parameter details will be so you don't need to set that like you do for manual constants.
        /// </param>
        /// <param name="extraInfo">If the constant type needs more information (like a light index) put it here.</param>
        public void SetAutoConstant(AutoConstantEntry entry) {
            autoConstantList.Add(entry);
        }

        /// <summary>
        ///    Overloaded method.
        /// </summary>
        /// <param name="type">The type of automatic constant to set.</param>
        /// <param name="index">
        ///    The location in the constant list to place this updated constant every time
        ///    it is changed. Note that because of the nature of the types, we know how big the 
        ///    parameter details will be so you don't need to set that like you do for manual constants.
        /// </param>
        /// <param name="extraInfo">If the constant type needs more information (like a light index) put it here.</param>
        public void SetAutoConstant(int index, AutoConstants type, float extraInfo) {
            AutoConstantEntry entry = new AutoConstantEntry(type, index, extraInfo);
            autoConstantList.Add(entry);
        }

		/// <summary>
		///    Sends 4 packed floating-point values to the program.
		/// </summary>
		/// <param name="index">Index of the contant register.</param>
		/// <param name="val">Structure containing 4 packed float values.</param>
		public void SetConstant(int index, Vector4 val) {
			SetConstant(index, val.x, val.y, val.z, val.w);
		}

		/// <summary>
		///    Sends 3 packed floating-point values to the program.
		/// </summary>
		/// <param name="index">Index of the contant register.</param>
		/// <param name="val">Structure containing 3 packed float values.</param>
		public void SetConstant(int index, Vector3 val) {
			SetConstant(index, val.x, val.y, val.z, 1.0f);
		}

		/// <summary>
		///    Sends 4 packed floating-point RGBA color values to the program.
		/// </summary>
		/// <param name="index">Index of the contant register.</param>
		/// <param name="color">Structure containing 4 packed RGBA color values.</param>
		public void SetConstant(int index, ColorEx color) {
            if (color != null)
                // verify order of color components
                SetConstant(index, color.r, color.g, color.b, color.a);
		}

		/// <summary>
		///    Sends a multiple value constant floating-point parameter to the program.
		/// </summary>
		/// <remarks>
		///     This method is made virtual to allow GpuProgramManagers, or even individual
		///     GpuProgram implementations to supply their own implementation if need be.
		///     An example would be where a Matrix needs to be transposed to row-major format
		///     before passing to the hardware.
		/// </remarks>
		/// <param name="index">Index of the contant register.</param>
		/// <param name="val">Structure containing 3 packed float values.</param>
		public void SetConstant(int index, Matrix4 val) {
			Matrix4 mat;

			// transpose the matrix if need be
			if(transposeMatrices) {
				mat = val.Transpose();
			}
			else {
				mat = val;
			}

			// resize if necessary
            if (index + 4 >= float4VecConstantsCount)
                ResizeFloatConstants(index + 4);
            floatIsSet[index + 0] = true;
            floatIsSet[index + 1] = true;
            floatIsSet[index + 2] = true;
            floatIsSet[index + 3] = true;
            if (index + 4 >= maxSetCount)
                maxSetCount = index + 4;
            int eltIndex = index * 4;

            floatConstantsArray[eltIndex + 0] = mat.m00;
            floatConstantsArray[eltIndex + 1] = mat.m01;
            floatConstantsArray[eltIndex + 2] = mat.m02;
            floatConstantsArray[eltIndex + 3] = mat.m03;

            floatConstantsArray[eltIndex + 4] = mat.m10;
            floatConstantsArray[eltIndex + 5] = mat.m11;
            floatConstantsArray[eltIndex + 6] = mat.m12;
            floatConstantsArray[eltIndex + 7] = mat.m13;

            floatConstantsArray[eltIndex + 8] = mat.m20;
            floatConstantsArray[eltIndex + 9] = mat.m21;
            floatConstantsArray[eltIndex + 10] = mat.m22;
            floatConstantsArray[eltIndex + 11] = mat.m23;

            floatConstantsArray[eltIndex + 12] = mat.m30;
            floatConstantsArray[eltIndex + 13] = mat.m31;
            floatConstantsArray[eltIndex + 14] = mat.m32;
            floatConstantsArray[eltIndex + 15] = mat.m33;
		}

		/// <summary>
		///    Sends a multiple matrix values to the program.
		/// </summary>
		/// <param name="index">Index of the contant register.</param>
		/// <param name="val">Structure containing 3 packed float values.</param>
		public void SetConstant(int index, Matrix4[] matrices, int count) {
			for(int i = 0; i < count; i++) {
				SetConstant(index, matrices[i]);
                index += 4;
			}
		}

		/// <summary>
		///    Sets an array of int values starting at the specified index.
		/// </summary>
		/// <param name="index">Index of the contant register to start at.</param>
		/// <param name="ints">Array of ints.</param>
		public void SetConstant(int index, int[] ints) {
			int count = ints.Length / 4;
			int srcIndex = 0;

			// resize if necessary
			intConstants.Resize(index + count);

			// copy in chunks of 4
			while(count-- > 0) {
				IntConstantEntry entry = intConstants[index++];
				entry.isSet = true;
				Array.Copy(ints, srcIndex, entry.val, 0, 4);
				srcIndex += 4;
			}
		}

		/// <summary>
		///    Provides a way to pass in the technique pass number
		/// </summary>
		/// <param name="index">Index of the contant register to start at.</param>
		/// <param name="ints">The int value.</param>
		public void SetIntConstant(int index, int value) {
			SetConstant(index, (float)value, 0f, 0f, 0f);
		}

		/// <summary>
		///    Provides a way to pass in the technique pass number
		/// </summary>
		/// <param name="index">Index of the contant register to start at.</param>
		/// <param name="ints">The float value.</param>
		public void SetFloatConstant(int index, float value) {
			SetConstant(index, value, 0f, 0f, 0f);
		}

        /// <summary>
        ///    Provides a way to pass in a single float
        /// </summary>
        /// <param name="index">Index of the contant register to start at.</param>
        /// <param name="value"></param>
        public void SetConstant(int index, float value) {
            SetConstant(index, value, 0f, 0f, 0f);
        }

        /// <summary>
		///    Optimize the most common case of setting constant
		///    consisting of four floats
		/// </summary>
		/// <param name="index">Index of the contant register to start at.</param>
		/// <param name="f0,f1,f2,f3">The floats.</param>
		public void SetConstant(int index, float f0, float f1, float f2, float f3) {
			// resize if necessary
            if (index >= float4VecConstantsCount)
                ResizeFloatConstants(index);
            floatIsSet[index] = true;
            if (index >= maxSetCount)
                maxSetCount = index + 1;
            int eltIndex = index * 4;
            floatConstantsArray[eltIndex] = f0;
            floatConstantsArray[eltIndex + 1] = f1;
            floatConstantsArray[eltIndex + 2] = f2;
            floatConstantsArray[eltIndex + 3] = f3;
        }

        /// <summary>
		///    The maxIndex is the max number of 4-float groups
		/// </summary>
		/// <param name="maxIndex">The results float4VecConstantsCount.</param>
		/// <param name="f0,f1,f2,f3">The floats.</param>
        public void ResizeFloatConstants(int maxIndex) {
            if (maxIndex >= float4VecConstantsCount) {
                float[] tmpFloats = new float[maxIndex * 4 + 4];
                Array.Copy(floatConstantsArray, tmpFloats, float4VecConstantsCount * 4);
                floatConstantsArray = tmpFloats;
                bool[] tmpBools = new bool[maxIndex + 1];
                Array.Copy(floatIsSet, tmpBools, float4VecConstantsCount);
                floatIsSet = tmpBools;
                float4VecConstantsCount = maxIndex + 1;
            }
        }

        /// <summary>
        /// Mark the given float constant as not set
        /// </summary>
        /// <param name="index">index of constant to clear</param>
        public void ClearFloatConstant(int index)
        {
            if (index < float4VecConstantsCount)
                floatIsSet[index] = false;
        }

        /// <summary>
        /// Mark the given int constant as not set
        /// </summary>
        /// <param name="index">index of constant to clear</param>
        public void ClearIntConstant(int index)
        {
            if (index < intConstants.Count)
            {
                intConstants[index].isSet = false;
            }
        }

		/// <summary>
		///    Sets an array of int values starting at the specified index.
		/// </summary>
		/// <param name="index">Index of the contant register to start at.</param>
		/// <param name="ints">Array of floats.</param>
		public void SetConstant(int index, float[] floats) {
			int eltCount = floats.Length;
            int count = (eltCount + 3) / 4;
 
			// resize if necessary
			if (index + count >= float4VecConstantsCount)
                ResizeFloatConstants(index + count);
            if (index + count >= maxSetCount)
                maxSetCount = index + count + 1;
            Array.Copy(floats, 0, floatConstantsArray, index * 4, eltCount);
            while(count-- > 0)
                floatIsSet[index++] = true;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="index"></param>
		/// <param name="factor"></param>
		public void SetConstantFromTime(int index, float factor) {
			ControllerManager.Instance.CreateGpuProgramTimerParam(this, index, factor);
		}

		#region Named parameters

		/// <summary>
		///    Sets up a constant which will automatically be updated by the engine.
		/// </summary>
		/// <remarks>
		///    Vertex and fragment programs often need parameters which are to do with the
		///    current render state, or particular values which may very well change over time,
		///    and often between objects which are being rendered. This feature allows you 
		///    to set up a certain number of predefined parameter mappings that are kept up to 
		///    date for you.
		/// </remarks>
		/// <param name="name">
		///    Name of the param.
		/// </param>
		/// <param name="type">
		///    The type of automatic constant to set.
		/// </param>
		/// <param name="extraInfo">
		///    Any extra infor needed by the auto constant (i.e. light index, etc).
		/// </param>
		public void SetNamedAutoConstant(string name, AutoConstants type, int extraInfo) {
			SetAutoConstant(GetParamIndex(name), type, extraInfo);
		}

        public void SetNamedConstant(string name, float val) {
            SetConstant(GetParamIndex(name), val, 0f, 0f, 0f);
        }

        public void SetNamedConstant(string name, float[] val) {
            SetConstant(GetParamIndex(name), val);
        }

        /// <summary>
        ///    Sends 4 packed floating-point values to the program.
        /// </summary>
        /// <param name="index">Index of the contant register.</param>
        /// <param name="val">Structure containing 4 packed float values.</param>
        public void SetNamedConstant(string name, Vector4 val) {
			SetConstant(GetParamIndex(name), val.x, val.y, val.z, val.w);
		}

		/// <summary>
		///    Sends 3 packed floating-point values to the program.
		/// </summary>
		/// <param name="name">Name of the param.</param>
		/// <param name="val">Structure containing 3 packed float values.</param>
		public void SetNamedConstant(string name, Vector3 val) {
			SetConstant(GetParamIndex(name), val.x, val.y, val.z, 1f);
		}

		/// <summary>
		///    Sends 4 packed floating-point RGBA color values to the program.
		/// </summary>
		/// <param name="name">Name of the param.</param>
		/// <param name="color">Structure containing 4 packed RGBA color values.</param>
		public void SetNamedConstant(string name, ColorEx color) {
			SetConstant(GetParamIndex(name), color.r, color.g, color.b, color.a);
		}

		/// <summary>
		///    Sends a multiple value constant floating-point parameter to the program.
		/// </summary>
		/// <param name="name">Name of the param.</param>
		/// <param name="val">Structure containing 3 packed float values.</param>
		public void SetNamedConstant(string name, Matrix4 val) {
			SetConstant(GetParamIndex(name), val);
		}

		/// <summary>
		///    Sends multiple matrices into a program.
		/// </summary>
		/// <param name="name">Name of the param.</param>
		/// <param name="matrices">Array of matrices.</param>
		public void SetNamedConstant(string name, Matrix4[] matrices, int count) {
			SetConstant(GetParamIndex(name), matrices, count);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="factor"></param>
		public void SetNamedConstantFromTime(string name, float factor) {
			SetConstantFromTime(GetParamIndex(name), factor);
		}

        #endregion

		/// <summary>
		///    Updates the automatic parameters (except lights) based on the details provided.
		/// </summary>
		/// <param name="source">
		///    A source containing all the updated data to be made available for auto updating
		///    the GPU program constants.
		/// </param>
		public void UpdateAutoParamsNoLights(AutoParamDataSource source) {
			// return if no constants
			if(!this.HasAutoConstants) {
				return;
			}

			// loop through and update all constants based on their type
			for(int i = 0; i < autoConstantList.Count; i++) {
				AutoConstantEntry entry = (AutoConstantEntry)autoConstantList[i];

				Matrix4[] matrices = null;
				int numMatrices = 0;
				int index = 0;
                float t;

				switch(entry.type) {
					case AutoConstants.WorldMatrix:
						SetConstant(entry.index, source.WorldMatrix);
						break;

					case AutoConstants.InverseWorldMatrix:
						SetConstant(entry.index, source.InverseWorldMatrix);
						break;

					case AutoConstants.TransposeWorldMatrix:
						SetConstant(entry.index, source.WorldMatrix.Transpose());
						break;

					case AutoConstants.InverseTransposeWorldMatrix:
						SetConstant(entry.index, source.InverseWorldMatrix.Transpose());
						break;

					case AutoConstants.WorldMatrixArray3x4:
						matrices = source.WorldMatrixArray;
						numMatrices = source.WorldMatrixCount;
						index = entry.index;

						for(int j = 0; j < numMatrices; j++) {
							Matrix4 m = matrices[j];
                            SetConstant(index++, m.m00, m.m01, m.m02, m.m03);
							SetConstant(index++, m.m10, m.m11, m.m12, m.m13);
							SetConstant(index++, m.m20, m.m21, m.m22, m.m23);
						}

						break;

					case AutoConstants.WorldMatrixArray:
						SetConstant(entry.index, source.WorldMatrixArray, source.WorldMatrixCount);
						break;

					case AutoConstants.ViewMatrix:
						SetConstant(entry.index, source.ViewMatrix);
						break;

                    case AutoConstants.InverseViewMatrix:
                        SetConstant(entry.index, source.InverseViewMatrix);
                        break;

                    case AutoConstants.TransposeViewMatrix:
                        SetConstant(entry.index, source.ViewMatrix.Transpose());
                        break;

                    case AutoConstants.InverseTransposeViewMatrix:
                        SetConstant(entry.index, source.InverseViewMatrix.Transpose());
                        break;

					case AutoConstants.ProjectionMatrix:
						SetConstant(entry.index, source.ProjectionMatrix);
						break;

					case AutoConstants.InverseProjectionMatrix:
						SetConstant(entry.index, source.InverseProjectionMatrix);
						break;

					case AutoConstants.InverseTransposeProjectionMatrix:
						SetConstant(entry.index, source.InverseProjectionMatrix.Transpose());
						break;

					case AutoConstants.ViewProjMatrix:
						SetConstant(entry.index, source.ViewProjectionMatrix);
						break;

					case AutoConstants.InverseViewProjMatrix:
						SetConstant(entry.index, source.InverseViewProjMatrix);
						break;

					case AutoConstants.TransposeViewProjMatrix:
						SetConstant(entry.index, source.ViewProjectionMatrix.Transpose());
						break;

					case AutoConstants.InverseTransposeViewProjMatrix:
						SetConstant(entry.index, source.InverseViewProjMatrix.Transpose());
						break;

					case AutoConstants.WorldViewMatrix:
						SetConstant(entry.index, source.WorldViewMatrix);
						break;

					case AutoConstants.InverseWorldViewMatrix:
						SetConstant(entry.index, source.InverseWorldViewMatrix);
						break;

					case AutoConstants.TransposeWorldViewMatrix:
						SetConstant(entry.index, source.WorldViewMatrix.Transpose());
						break;

					case AutoConstants.InverseTransposeWorldViewMatrix:
						SetConstant(entry.index, source.InverseWorldViewMatrix.Transpose());
						break;

                    case AutoConstants.RenderTargetFlipping:
                        SetIntConstant(entry.index, source.RenderTarget.RequiresTextureFlipping ? -1 : 1);
                        break;

					case AutoConstants.AmbientLightColor:
						SetConstant(entry.index, source.AmbientLight);
						break;

					case AutoConstants.DerivedAmbientLightColor:
						SetConstant(entry.index, source.DerivedAmbientLight);
						break;

					case AutoConstants.DerivedSceneColor:
                        ColorEx result = source.DerivedAmbientLight + source.CurrentPass.Emissive;
                        result.a = source.CurrentPass.Diffuse.a;
						SetConstant(entry.index, result);
						break;                        

                    case AutoConstants.FogColor:
                        SetConstant(entry.index, source.FogColor);
                        break;

                    case AutoConstants.FogParams:
                        SetConstant(entry.index, source.FogParams);
                        break;

                    case AutoConstants.SurfaceAmbientColor:
                        SetConstant(entry.index, source.CurrentPass.Ambient);
                        break;

                    case AutoConstants.SurfaceDiffuseColor:
                        SetConstant(entry.index, source.CurrentPass.Diffuse);
                        break;

                    case AutoConstants.SurfaceSpecularColor:
                        SetConstant(entry.index, source.CurrentPass.Specular);
                        break;

                    case AutoConstants.SurfaceEmissiveColor:
                        SetConstant(entry.index, source.CurrentPass.Emissive);
                        break;
                    
                    case AutoConstants.SurfaceShininess:
                        SetConstant(entry.index, source.CurrentPass.Shininess);
                        break;
                    
                    case AutoConstants.CameraPosition:
                        SetConstant(entry.index, source.CameraPosition);
                        break;
                    
                    case AutoConstants.CameraPositionObjectSpace:
                        SetConstant(entry.index, source.CameraPositionObjectSpace);
                        break;

                    case AutoConstants.Time:
                        SetFloatConstant(entry.index, source.Time * entry.fdata);
                        break;

                    case AutoConstants.Time_0_X:
                        SetFloatConstant(entry.index, source.GetTime_0_X(entry.fdata));
                        break;
                    
                    case AutoConstants.CosTime_0_X:
                        SetFloatConstant(entry.index, (float)Math.Cos(source.GetTime_0_X(entry.fdata)));
                        break;
                    
                    case AutoConstants.SinTime_0_X:
                        SetFloatConstant(entry.index, (float)Math.Sin(source.GetTime_0_X(entry.fdata)));
                        break;

                    case AutoConstants.TanTime_0_X:
                        SetFloatConstant(entry.index, (float)Math.Tan(source.GetTime_0_X(entry.fdata)));
                        break;

                    case AutoConstants.Time_0_X_Packed:
                        t = source.Time;
                        SetConstant(entry.index, t, (float)Math.Sin(t), (float)Math.Cos(t), (float)Math.Tan(t));
                        break;

                    case AutoConstants.Time_0_1:
                        SetFloatConstant(entry.index, source.GetTime_0_1(entry.fdata));
                        break;
                    
                    case AutoConstants.CosTime_0_1:
                        SetFloatConstant(entry.index, (float)Math.Cos(source.GetTime_0_1(entry.fdata)));
                        break;
                    
                    case AutoConstants.SinTime_0_1:
                        SetFloatConstant(entry.index, (float)Math.Sin(source.GetTime_0_1(entry.fdata)));
                        break;

                    case AutoConstants.TanTime_0_1:
                        SetFloatConstant(entry.index, (float)Math.Tan(source.GetTime_0_1(entry.fdata)));
                        break;

                    case AutoConstants.Time_0_1_Packed:
                        t = source.GetTime_0_1(entry.fdata);
                        SetConstant(entry.index, t, (float)Math.Sin(t), (float)Math.Cos(t), (float)Math.Tan(t));
                        break;

                    case AutoConstants.Time_0_2PI:
                        SetFloatConstant(entry.index, source.GetTime_0_2PI(entry.fdata));
                        break;
                    
                    case AutoConstants.CosTime_0_2PI:
                        SetFloatConstant(entry.index, (float)Math.Cos(source.GetTime_0_2PI(entry.fdata)));
                        break;
                    
                    case AutoConstants.SinTime_0_2PI:
                        SetFloatConstant(entry.index, (float)Math.Sin(source.GetTime_0_2PI(entry.fdata)));
                        break;

                    case AutoConstants.TanTime_0_2PI:
                        SetFloatConstant(entry.index, (float)Math.Tan(source.GetTime_0_2PI(entry.fdata)));
                        break;

                    case AutoConstants.Time_0_2PI_Packed:
                        t = source.GetTime_0_2PI(entry.fdata);
                        SetConstant(entry.index, t, (float)Math.Sin(t), (float)Math.Cos(t), (float)Math.Tan(t));
                        break;

                    case AutoConstants.FrameTime:
                        SetConstant(entry.index, (1.0f / Root.Instance.AverageFPS));
                        break;

                    case AutoConstants.FPS:
                        SetConstant(entry.index, Root.Instance.AverageFPS);
                        break;

                    case AutoConstants.ViewportWidth:
                        SetConstant(entry.index, source.Viewport.ActualWidth);
                        break;

                    case AutoConstants.ViewportHeight:
                        SetConstant(entry.index, source.Viewport.ActualHeight);
                        break;

                    case AutoConstants.ViewportSize:
                        SetConstant(entry.index, 
                                    new Vector4(source.Viewport.ActualWidth,
                                                source.Viewport.ActualHeight,    
                                                1.0f / source.Viewport.ActualWidth,
                                                1.0f / source.Viewport.ActualHeight));
                        break;

                    case AutoConstants.TexelOffsets:
                        RenderSystem rsys = Root.Instance.RenderSystem;
                        SetConstant(entry.index, 
                            new Vector4(rsys.HorizontalTexelOffset,
                                        rsys.VerticalTexelOffset,
                                        rsys.HorizontalTexelOffset / source.Viewport.ActualWidth,
                                        rsys.VerticalTexelOffset / source.Viewport.ActualHeight));
                        break;

                    case AutoConstants.TextureSize:
                        SetConstant(entry.index, source.GetTextureSize(entry.data));
                        break;

                    case AutoConstants.InverseTextureSize:
                        SetConstant(entry.index, 1.0f / source.GetTextureSize(entry.data));
                        break;

                    case AutoConstants.SceneDepthRange:
                        SetConstant(entry.index, source.SceneDepthRange);
                        break;

                    case AutoConstants.ViewDirection:
                        SetConstant(entry.index, source.ViewDirection);
                        break;

                    case AutoConstants.ViewSideVector:
                        SetConstant(entry.index, source.ViewSideVector);
                        break;

                    case AutoConstants.WorldViewProjMatrix:
						SetConstant(entry.index, source.WorldViewProjMatrix);
						break;

                    case AutoConstants.ViewUpVector:
                        SetConstant(entry.index, source.ViewUpVector);
                        break;

                    case AutoConstants.FOV:
                        SetConstant(entry.index, MathUtil.DegreesToRadians(source.Camera.FOVy));
                        break;

                    case AutoConstants.NearClipDistance:
                        SetConstant(entry.index, source.NearClipDistance);
                        break;

                    case AutoConstants.FarClipDistance:
                        SetConstant(entry.index, source.FarClipDistance);
                        break;

					case AutoConstants.PassNumber:
                        SetIntConstant(entry.index, source.PassNumber);
                        break;

                    case AutoConstants.PassIterationNumber:
                        // TODO: This isn't right, and doesn't match
                        // what Ogre does.  I can't figure out what
                        // Ogre does.
                        SetIntConstant(entry.index, source.PassIterationNumber);
                        break;

                    case AutoConstants.TextureViewProjMatrix:
						SetConstant(entry.index, source.GetTextureViewProjectionMatrix(entry.data));
						break;

                    case AutoConstants.Custom:
                    case AutoConstants.AnimationParametric:
						source.Renderable.UpdateCustomGpuParameter(entry, this);
						break;

                    case AutoConstants.MVShadowTechnique:
                        SetConstant(entry.index, source.MVShadowTechnique); 
                        break;

                    case AutoConstants.ShadowFadeParams:
                        SetConstant(entry.index, source.ShadowFadeParams);
                        break;
				}
			}
		}

		/// <summary>
		///    Updates the automatic light parameters based on the details provided.
		/// </summary>
		/// <param name="source">
		///    A source containing all the updated data to be made available for auto updating
		///    the GPU program constants.
		/// </param>
		public void UpdateAutoParamsLightsOnly(AutoParamDataSource source) {
			// return if no constants
			if(!this.HasAutoConstants) {
				return;
			}

			// loop through and update all constants based on their type
			for(int constantIndex = 0; constantIndex < autoConstantList.Count; constantIndex++) {
				AutoConstantEntry entry = (AutoConstantEntry)autoConstantList[constantIndex];

				Vector3 vec3;
                Light light;
                int i;

				switch(entry.type) {
					case AutoConstants.LightDiffuseColor:
						SetConstant(entry.index, source.GetLight(entry.data).Diffuse);
						break;

					case AutoConstants.LightSpecularColor:
						SetConstant(entry.index, source.GetLight(entry.data).Specular);
						break;

					case AutoConstants.LightPosition:
						SetConstant(entry.index, source.GetLight(entry.data).DerivedPosition);
						break;

					case AutoConstants.LightDirection:
						vec3 = source.GetLight(entry.data).DerivedDirection;
						SetConstant(entry.index, vec3.x, vec3.y, vec3.z, 1.0f);
						break;

					case AutoConstants.LightPositionObjectSpace:
						SetConstant(entry.index, source.InverseWorldMatrix * source.GetLight(entry.data).GetAs4DVector());
						break;

					case AutoConstants.LightDirectionObjectSpace:
						vec3 = source.InverseWorldMatrix * source.GetLight(entry.data).DerivedDirection;
						vec3.Normalize();
						SetConstant(entry.index, vec3.x, vec3.y, vec3.z, 1.0f);
						break;

					case AutoConstants.LightPositionViewSpace:
						SetConstant(entry.index, source.ViewMatrix.TransformAffine(source.GetLight(entry.data).GetAs4DVector()));
						break;

					case AutoConstants.LightDirectionViewSpace:
						vec3 = source.InverseViewMatrix.Transpose() * source.GetLight(entry.data).DerivedDirection;
						vec3.Normalize();
						SetConstant(entry.index, vec3.x, vec3.y, vec3.z, 1.0f);
						break;

					case AutoConstants.LightDistanceObjectSpace:
						vec3 = source.InverseWorldMatrix * source.GetLight(entry.data).DerivedPosition;
						SetConstant(entry.index, vec3.Length);
						break;

					case AutoConstants.ShadowExtrusionDistance:
						SetConstant(entry.index, source.ShadowExtrusionDistance);
						break;

					case AutoConstants.ShadowSceneDepthRange:
						SetConstant(entry.index, source.GetShadowSceneDepthRange(entry.data));
						break;

					case AutoConstants.LightPower:
						SetConstant(entry.index, source.GetLight(entry.data).PowerScale);
						break;

					case AutoConstants.LightAttenuation:
						light = source.GetLight(entry.data);
                        SetConstant(entry.index, light.AttenuationRange, light.AttenuationConstant, light.AttenuationLinear, light.AttenuationQuadratic);
						break;

					case AutoConstants.SpotlightParams:
						light = source.GetLight(entry.data);
                        if (light.Type == LightType.Spotlight)
                            SetConstant(entry.index,
                                (float)Math.Cos(MathUtil.DegreesToRadians(light.SpotlightInnerAngle) * 0.5),
                                (float)Math.Cos(MathUtil.DegreesToRadians(light.SpotlightOuterAngle) * 0.5),
                                light.SpotlightFalloff,
                                1.0f);
                        else
                            SetConstant(entry.index, 1f, 0f, 0f, 1f);
                        break;
                        
					case AutoConstants.LightDiffuseColorArray:
						for (i=0; i<entry.data; i++)
                            SetConstant(entry.index + i, source.GetLight(i).Diffuse);
						break;

					case AutoConstants.LightSpecularColorArray:
						for (i=0; i<entry.data; i++)
                            SetConstant(entry.index + i, source.GetLight(i).Specular);
						break;

					case AutoConstants.LightPositionArray:
						for (i=0; i<entry.data; i++)
                            SetConstant(entry.index + i, source.GetLight(i).DerivedPosition);
						break;

					case AutoConstants.LightDirectionArray:
						for (i=0; i<entry.data; i++) {
                            vec3 = source.GetLight(i).DerivedDirection;
                            SetConstant(entry.index + i, vec3.x, vec3.y, vec3.z, 1.0f);
						}
                        break;

					case AutoConstants.LightPositionObjectSpaceArray:
						for (i=0; i<entry.data; i++)
                            SetConstant(entry.index + i, source.ViewMatrix.TransformAffine(source.GetLight(i).GetAs4DVector()));
						break;

					case AutoConstants.LightDirectionObjectSpaceArray:
						for (i=0; i<entry.data; i++) {
                            vec3 = source.InverseWorldMatrix.TransformAffine(source.GetLight(i).DerivedDirection);
                            SetConstant(entry.index + i, vec3.x, vec3.y, vec3.z, 1.0f);
						}
                        break;

					case AutoConstants.LightPositionViewSpaceArray:
						for (i=0; i<entry.data; i++)
 						    SetConstant(entry.index + i, source.ViewMatrix.TransformAffine(source.GetLight(i).GetAs4DVector()));
						break;

					case AutoConstants.LightDirectionViewSpaceArray:
						for (i=0; i<entry.data; i++) {
                            vec3 = source.InverseViewMatrix.Transpose() * source.GetLight(i).DerivedDirection;
                            vec3.Normalize();
                            SetConstant(entry.index + i, vec3.x, vec3.y, vec3.z, 1.0f);
						}
                        break;

					case AutoConstants.LightDistanceObjectSpaceArray:
						for (i=0; i<entry.data; i++) {
                            vec3 = source.InverseWorldMatrix * source.GetLight(i).DerivedPosition;
                            SetConstant(entry.index + i, vec3.Length);
                        }
						break;

					case AutoConstants.LightPowerArray:
						for (i=0; i<entry.data; i++)
                            SetConstant(entry.index + i, source.GetLight(i).PowerScale);
						break;

					case AutoConstants.LightAttenuationArray:
						for (i=0; i<entry.data; i++) {
                            light = source.GetLight(i);
                            SetConstant(entry.index + i, light.AttenuationRange, light.AttenuationConstant, light.AttenuationLinear, light.AttenuationQuadratic);
						}
                        break;

					case AutoConstants.SpotlightParamsArray:
						for (i=0; i<entry.data; i++) {
                            light = source.GetLight(i);
                            if (light.Type == LightType.Spotlight)
                                SetConstant(entry.index + 1,
                                    (float)Math.Cos(MathUtil.DegreesToRadians(light.SpotlightInnerAngle) * 0.5),
                                    (float)Math.Cos(MathUtil.DegreesToRadians(light.SpotlightOuterAngle) * 0.5),
                                    light.SpotlightFalloff,
                                    1.0f);
                            else
                                SetConstant(entry.index + i, 1f, 0f, 0f, 1f);
                        }
                        break;
                        
					case AutoConstants.DerivedLightDiffuseColor:
						SetConstant(entry.index, source.GetLight(entry.data).Diffuse * source.CurrentPass.Diffuse);
						break;

					case AutoConstants.DerivedLightSpecularColor:
						SetConstant(entry.index, source.GetLight(entry.data).Diffuse * source.CurrentPass.Specular);
						break;

					case AutoConstants.DerivedLightDiffuseColorArray:
						for (i=0; i<entry.data; i++) {
                            light = source.GetLight(i);
                            SetConstant(entry.index + i, light.Diffuse * source.CurrentPass.Diffuse);
						}
                        break;

					case AutoConstants.DerivedLightSpecularColorArray:
						for (i=0; i<entry.data; i++) {
                            light = source.GetLight(i);
                            SetConstant(entry.index + i, light.Specular * source.CurrentPass.Diffuse);
						}
                        break;

					case AutoConstants.TextureViewProjMatrix:
                        SetConstant(entry.index, source.GetTextureViewProjectionMatrix(entry.data));
                        break;

                    default:    
                        // do nothing
                        break;
				}
			}
		}

		#endregion
		
		#region Properties
		
		/// <summary>
		///		Gets/Sets the auto add parameter name flag.
		/// </summary>
		/// <remarks>
		///		Not all GPU programs make named parameters available after the high level
		///		source is compiled.  GLSL is one such case.  If parameter names are not loaded
		///		prior to the material serializer reading in parameter names in a script then
		///		an exception is generated.  Set this to true to have names not found
		///		in the map added to the map.
		///		The index of the parameter name will be set to the end of the Float Constant List.
		/// </remarks>
		public bool AutoAddParamName {
			get {
				return autoAddParamName;
			}
			set {
				autoAddParamName = value;
			}
		}

		public ArrayList ParameterInfo {
			get { 
				return this.paramTypeList; 
			}
		}

		/// <summary>
		///    Returns true if this instance contains any automatic constants.
		/// </summary>
		public bool HasAutoConstants {
			get {
				return autoConstantList.Count > 0;
			}
		}

		/// <summary>
		///    Returns true if int constants have been set.
		/// </summary>
		public bool HasIntConstants {
			get {
				return intConstants.Count > 0;
			}
		}

		/// <summary>
		///    Returns true if floating-point constants have been set.
		/// </summary>
		public bool HasFloatConstants {
			get {
				return float4VecConstantsCount > 0;
			}
		}

		/// <summary>
		///    Gets the number of int contants values currently set.
		/// </summary>
		public int IntConstantCount {
			get {
				return intConstants.Count;
			}
		}

		/// <summary>
		///    Gets the number of floating-point contant values currently set.
		/// </summary>
		public int FloatConstantCount {
			get {
				return float4VecConstantsCount;
			}
		}

		/// <summary>
		///		Gets the number of named parameters in this param set.
		/// </summary>
		public int NamedParamCount {
			get { 
				return this.namedParams.Count; 
			}
		}

		/// <summary>
		///     Specifies whether matrices need to be transposed prior to
		///     being sent to the hardware.
		/// </summary>
		public bool TransposeMatrices {
			get {
				return transposeMatrices;
			}
			set {
				transposeMatrices = value;
			}
		}

		#endregion Properties

		#region Inner classes

		/// <summary>
		///    A structure for recording the use of automatic parameters.
		/// </summary>
		public class AutoConstantEntry {
			/// <summary>
			///    The type of the parameter.
			/// </summary>
			public AutoConstants type;
			/// <summary>
			///    The target index.
			/// </summary>
			public int index;
			/// <summary>
			///    Any additional info to go with the parameter.
			/// </summary>
			public int data;
            /// <summary>
            ///    Any additional info to go with the parameter.
            /// </summary>
            public float fdata;

            /// <summary>
            ///    Default constructor.
            /// </summary>
            /// <param name="type">Type of auto param (i.e. WorldViewMatrix, etc)</param>
            /// <param name="index">Index of the param.</param>
            /// <param name="data">Any additional info to go with the parameter.</param>
            public AutoConstantEntry(AutoConstants type, int index, int data) {
                this.type = type;
                this.index = index;
                this.data = data;
                System.Diagnostics.Debug.Assert(type != AutoConstants.SinTime_0_X);
            }

            /// <summary>
            ///    Default constructor.
            /// </summary>
            /// <param name="type">Type of auto param (i.e. WorldViewMatrix, etc)</param>
            /// <param name="index">Index of the param.</param>
            /// <param name="data">Any additional info to go with the parameter.</param>
            public AutoConstantEntry(AutoConstants type, int index, float fdata) {
                this.type = type;
                this.index = index;
                this.fdata = fdata;
            }

            public AutoConstantEntry Clone() {
                AutoConstantEntry rv = new AutoConstantEntry(type, index, fdata);
                rv.data = data;
                return rv;
            }
		}

		/// <summary>
		///		Float parameter entry; contains both a group of 4 values and 
		///		an indicator to say if it's been set or not. This allows us to 
		///		filter out constant entries which have not been set by the renderer
		///		and may actually be being used internally by the program.
		/// </summary>
		public class FloatConstantEntry {
			public float[] val = new float[4];
			public bool	isSet = false;
		}

		/// <summary>
		///		Int parameter entry; contains both a group of 4 values and 
		///		an indicator to say if it's been set or not. This allows us to 
		///		filter out constant entries which have not been set by the renderer
		///		and may actually be being used internally by the program.
		/// </summary>
		public class IntConstantEntry {
			public int[] val = new int[4];
			public bool	isSet = false;
		}

		#endregion
	}
}
