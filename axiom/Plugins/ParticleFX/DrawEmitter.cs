using System;
using Axiom.Core;
using Axiom.MathLib;
using Axiom.ParticleSystems;
using Axiom.Scripting;

namespace Axiom.ParticleFX {
	/// <summary>
	/// 	Summary description for DrawEmitter.
	/// </summary>
	public class DrawEmitter : ParticleEmitter {
        protected float distance;

        #region Constructors
		
        public DrawEmitter() {
            this.Type = "Draw";
        }
		
        #endregion
		
        #region Methods
		
        public override ushort GetEmissionCount(float timeElapsed) {
            // use basic constant emission
            return GenerateConstantEmissionCount(timeElapsed);
        }

        public override void InitParticle(Particle particle) {
            base.InitParticle(particle);

            Vector3 pos = new Vector3();

            pos.x = MathUtil.SymmetricRandom() * distance;
            pos.y = MathUtil.SymmetricRandom() * distance;
            pos.z = MathUtil.SymmetricRandom() * distance;

            // point emitter emits starting from its own position
            particle.Position = pos + particle.ParentSet.WorldPosition;

            GenerateEmissionColor(particle.Color);
            particle.Direction = particle.ParentSet.WorldPosition - particle.Position;
            GenerateEmissionVelocity(ref particle.Direction);

            // generate time to live
            particle.timeToLive = GenerateEmissionTTL();
        }

        #endregion

        #region Properties

        public float Distance {
            get {
                return distance;
            }
            set {
                distance = value;
            }
        }

        #endregion Properties

        #region Command definition classes

        [Command("distance", "Distance from the center of the emitter where the particles spawn.", typeof(ParticleEmitter))]
        class DistanceCommand : ICommand {
            #region ICommand Members

            public string Get(object target) {
                DrawEmitter emitter = target as DrawEmitter;
                return StringConverter.ToString(emitter.Distance);
            }
            public void Set(object target, string val) {
                DrawEmitter emitter = target as DrawEmitter;
                emitter.Distance = StringConverter.ParseFloat(val);
            }

            #endregion
        }

        #endregion Command definition classes
	}
}
