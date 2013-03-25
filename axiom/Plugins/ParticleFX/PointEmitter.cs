using System;
using Axiom.Core;
using Axiom.ParticleSystems;
using Axiom.Scripting;

namespace Axiom.ParticleFX {
    /// <summary>
    /// 	Summary description for PointEmitter.
    /// </summary>
    public class PointEmitter : ParticleEmitter {
        #region Constructors
		
        public PointEmitter() {
            this.Type = "Point";
        }
		
        #endregion
		
        #region Methods
		
        public override ushort GetEmissionCount(float timeElapsed) {
            // use basic constant emission
            return GenerateConstantEmissionCount(timeElapsed);
        }

        public override void InitParticle(Particle particle) {
            base.InitParticle(particle);

            // point emitter emits starting from its own position
            particle.Position = this.position;

            GenerateEmissionColor(particle.Color);
            GenerateEmissionDirection(ref particle.Direction);
            GenerateEmissionVelocity(ref particle.Direction);

            // generate time to live
            particle.timeToLive = particle.totalTimeToLive = GenerateEmissionTTL();
        }

        #endregion
    }
}
