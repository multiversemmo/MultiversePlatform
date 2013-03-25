using System;
using System.Collections;

namespace Axiom.Core {
    /// <summary>
    ///     Generics: List<MeshLodUsage>
    /// </summary>
    public class MeshLodUsageList : ArrayList {}

    /// <summary>
    ///     Generics: List<int>
    /// </summary>
    public class IntList : ArrayList { 
        public void Resize(int size) {
            int[] data = (int[])this.ToArray(typeof(int));
            int[] newData = new int[size];
            Array.Copy(data, 0, newData, 0, size);
            Clear();
            AddRange(newData);
        }
    }

    /// <summary>
    ///     Generics: List<float>
    /// </summary>
    public class FloatList : ArrayList { 
        public void Resize(int size) {
            float[] data = (float[])this.ToArray(typeof(float));
            float[] newData = new float[size];
            Array.Copy(data, 0, newData, 0, size);
            Clear();
            AddRange(newData);
        }
    }
}
