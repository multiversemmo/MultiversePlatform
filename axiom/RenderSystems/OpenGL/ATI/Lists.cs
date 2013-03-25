using System;
using System.Collections;

namespace Axiom.RenderSystems.OpenGL.ATI {

    public class TokenInstructionList : ArrayList { 
        public void Resize(int size) {
            TokenInstruction[] data = (TokenInstruction[])this.ToArray(typeof(TokenInstruction));
            TokenInstruction[] newData = new TokenInstruction[size];
            Array.Copy(data, 0, newData, 0, size);
            Clear();
            AddRange(newData);
        }
    }
}
