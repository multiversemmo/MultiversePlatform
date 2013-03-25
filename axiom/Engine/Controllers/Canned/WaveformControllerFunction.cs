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
using Axiom.MathLib;

namespace Axiom.Controllers.Canned {
    /// <summary>
    /// A Controller representing a periodic waveform function ranging from Sine to InverseSawtooth
    /// </summary>
    /// <remarks>Function take to form of BaseValue + Amplitude * ( F(time * freq) / 2 + .5 )
    /// such as Base + A * ( Sin(t freq 2 pi) + .5) </remarks>
    public class WaveformControllerFunction : BaseControllerFunction {
        #region Member variables
		
        protected WaveformType type;
        protected float baseVal = 0.0f;
        protected float frequency = 1.0f;
        protected float phase = 0.0f;
        protected float amplitude = 1.0f;

        #endregion
		
        #region Constructors
		
        public WaveformControllerFunction(WaveformType type, float baseVal, float frequency, float phase, float amplitude, bool useDelta) : base(useDelta) {
            this.type = type;
            this.baseVal = baseVal;
            this.frequency = frequency;
            this.phase = phase;
            this.amplitude = amplitude;
        }

        public WaveformControllerFunction(WaveformType type, float baseVal) : base(true) {
            this.type = type;
            this.baseVal = baseVal;
        }

        public WaveformControllerFunction(WaveformType type, float baseVal, float frequency) : base(true) {
            this.type = type;
            this.baseVal = baseVal;
            this.frequency = frequency;
        }

        public WaveformControllerFunction(WaveformType type, float baseVal, float frequency, float phase) : base(true) {
            this.type = type;
            this.baseVal = baseVal;
            this.frequency = frequency;
            this.phase = phase;
        }

        public WaveformControllerFunction(WaveformType type, float baseVal, float frequency, float phase, float amplitude) : base(true) {
            this.type = type;
            this.baseVal = baseVal;
            this.frequency = frequency;
            this.phase = phase;
            this.amplitude = amplitude;
        }

        public WaveformControllerFunction(WaveformType type) : base(true) {
            this.type = type;
        }
		
        #endregion
		
        #region Methods

        public override float Execute(float sourceValue) {
            float input = AdjustInput(sourceValue * frequency) % 1f;
            float output = 0.0f;

            // first, get output in range [-1,1] (typical for waveforms)
            switch(type) {
                case WaveformType.Sine:
                    output = MathUtil.Sin(input * MathUtil.TWO_PI);
                    break;

                case WaveformType.Triangle:
                    if(input < 0.25f)
                        output = input * 4;
                    else if(input >= 0.25f && input < 0.75f)
                        output = 1.0f - ((input - 0.25f) * 4);
                    else
                        output = ((input - 0.75f) * 4) - 1.0f;

                    break;

                case WaveformType.Square:
                    if(input <= 0.5f)
                        output = 1.0f;
                    else
                        output = -1.0f;
                    break;

                case WaveformType.Sawtooth:
                    output = (input * 2) - 1;
                    break;

                case WaveformType.InverseSawtooth:
                    output = -((input * 2) - 1);
                    break;

            } // end switch

            // scale final output to range [0,1], and then by base and amplitude
            return baseVal + ((output + 1.0f) * 0.5f * amplitude);
        }

        protected override float AdjustInput(float input) {
            float adjusted = base.AdjustInput(input);

            // if not using delta accumulation, adjust by phase value
            if(!useDeltaInput)
                adjusted += phase;

            return adjusted;
        }
		
        #endregion
		
        #region Properties
		
        #endregion

    }
}
