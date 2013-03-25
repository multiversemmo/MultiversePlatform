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
using System.Collections;
using System.Text;

namespace TUVienna.CS_Lex
{
    /// <summary>
    /// Summary description for SparseBitSet.
    /// </summary>
    /// 

    // BINARY OPERATION MACHINERY
       

    public interface BinOp 
    {
         long op(long a, long b);
    }

    class   mAND:BinOp    
    {
        public  long op(long a, long b) { return a & b; }
    };
    class    mOR :BinOp
    {
        public long op(long a, long b) { return a | b; }
    };
    class mXOR :BinOp
    {
        public   long op(long a, long b) { return a ^ b; }
    };
    
   public  sealed class SparseBitSet  
    {
        /** Sorted array of bit-block offsets. */
        int[]  offs;
        /** Array of bit-blocks; each holding BITS bits. */
        long[] bits;
        /** Number of blocks currently in use. */
        int _size;
        /** log base 2 of BITS, for the identity: x/BITS == x >> LG_BITS */
        private const int LG_BITS = 6;
        /** Number of bits in a block. */
        private const int BITS = 1<<LG_BITS;
        /** BITS-1, using the identity: x % BITS == x & (BITS-1) */
        private const  int BITS_M1 = BITS-1;
        /**
             * Creates an empty Set.
             */
        public SparseBitSet() 
        {
            bits = new long[4];
            offs = new int [4];
            _size = 0;
        }

        /**
             * Creates an empty Set with the specified size.
             * @param nbits the size of the Set
             */
        public SparseBitSet(int nbits) :this()
        {
        }

        /**
             * Creates an empty Set with the same size as the given Set.
             */
        public SparseBitSet(SparseBitSet _set) 
        {
            bits = new long[_set._size];
            offs = new int [_set._size];
            _size = 0;
        }

        private void new_block(int bnum) 
        {
            new_block(bsearch(bnum), bnum);
        }
        private void new_block(int idx, int bnum) 
        {
            if (_size==bits.Length) 
            { // resize
                long[] nbits = new long[_size*3];
                int [] noffs = new int [_size*3];
               // System.arraycopy(bits, 0, nbits, 0, size);
                Array.Copy(bits,nbits,_size);
              //  System.arraycopy(offs, 0, noffs, 0, size);
                Array.Copy(offs,noffs,_size);
                bits = nbits;
                offs = noffs;
            }
            CUtility.ASSERT(_size<bits.Length);
            insert_block(idx, bnum);
        }
        private void insert_block(int idx, int bnum) 
        {
            CUtility.ASSERT(idx<=_size);
            CUtility.ASSERT(idx==_size || offs[idx]!=bnum);
           // System.arraycopy(bits, idx, bits, idx+1, size-idx);
            Array.Copy(bits,idx,bits,idx+1,_size-idx);
            //System.arraycopy(offs, idx, offs, idx+1, size-idx);
            Array.Copy(offs,idx,offs,idx+1,_size-idx);
            offs[idx]=bnum;
            bits[idx]=0; //clear them bits.
            _size++;
        }
        private int bsearch(int bnum) 
        {
            int l=0, r=_size; // search interval is [l, r)
            while (l<r) 
            {
                int p = (l+r)/2;
                if (bnum<offs[p]) r=p;
                else if (bnum>offs[p]) l=p+1;
                else return p;
            }
            CUtility.ASSERT(l==r);
            return l; // index at which the bnum *should* be, if it's not.
        }
	    
        /**
             * Sets a bit.
             * @param bit the bit to be Set
             */
        public void Set(int bit) 
        {
            int bnum = bit >> LG_BITS;
            int idx  = bsearch(bnum);
            if (idx >= _size || offs[idx]!=bnum)
                new_block(idx, bnum);
            bits[idx] |= (1L << (bit & BITS_M1) );
        }

        /**
             * Clears a bit.
             * @param bit the bit to be cleared
             */
        public void clear(int bit) 
        {
            int bnum = bit >> LG_BITS;
            int idx  = bsearch(bnum);
            if (idx >= _size || offs[idx]!=bnum)
                new_block(idx, bnum);
            bits[idx] &= ~(1L << (bit & BITS_M1) );
        }

        /**
             * Clears all bits.
             */
        public void clearAll() 
        {
            _size = 0;
        }

        /**
             * Gets a bit.
             * @param bit the bit to be gotten
             */
        public bool Get(int bit) 
        {
            int bnum = bit >> LG_BITS;
            int idx  = bsearch(bnum);
            if (idx >= _size || offs[idx]!=bnum)
                return false;
            return 0 != ( bits[idx] & (1L << (bit & BITS_M1) ) );
        }

        private static readonly BinOp AND = new mAND();
        private static readonly BinOp OR = new mOR();
        private static readonly BinOp XOR = new mXOR();
 
        /**
             * Logically ANDs this bit Set with the specified Set of bits.
             * @param Set the bit Set to be ANDed with
             */
        public void and(SparseBitSet Set) 
        {
            binop(this, Set, AND);
        }

        /**
             * Logically ORs this bit Set with the specified Set of bits.
             * @param Set the bit Set to be ORed with
             */
        public void or(SparseBitSet Set) 
        {
            binop(this, Set, OR);
        }

        /**
             * Logically XORs this bit Set with the specified Set of bits.
             * @param Set the bit Set to be XORed with
             */
        public void xor(SparseBitSet Set) 
        {
            binop(this, Set, XOR);
        }

      
        private  static void binop(SparseBitSet a, SparseBitSet b, BinOp op) 
        {
            int  nsize = a._size + b._size;
            long[] nbits; 
            int [] noffs;
            int a_zero, a_size;
            // be very clever and avoid allocating more memory if we can.
            if (a.bits.Length < nsize) 
            { // oh well, have to make working space.
                nbits = new long[nsize];
                noffs = new int [nsize];
                a_zero  = 0; a_size = a._size;
            } 
            else 
            { // reduce, reuse, recycle!
                nbits = a.bits;
                noffs = a.offs;
                a_zero = a.bits.Length - a._size; a_size = a.bits.Length;
              //  System.arraycopy(a.bits, 0, a.bits, a_zero, a.size);
                Array.Copy(a.bits,0,a.bits,a_zero,a._size);
                //System.arraycopy(a.offs, 0, a.offs, a_zero, a.size);
                Array.Copy(a.offs,0,a.offs,a_zero,a._size);
            }
            // ok, crunch through and binop those sets!
            nsize = 0;
            for (int i=a_zero, j=0; i<a_size || j<b._size; ) 
            {
                long nb; int no;
                if (i<a_size && (j>=b._size || a.offs[i] < b.offs[j])) 
                {
                    nb = op.op(a.bits[i], 0);
                    no = a.offs[i];
                    i++;
                } 
                else if (j<b._size && (i>=a_size || a.offs[i] > b.offs[j])) 
                {
                    nb = op.op(0, b.bits[j]);
                    no = b.offs[j];
                    j++;
                } 
                else 
                { // equal keys; merge.
                    nb = op.op(a.bits[i], b.bits[j]);
                    no = a.offs[i];
                    i++; j++;
                }
                if (nb!=0) 
                {
                    nbits[nsize] = nb;
                    noffs[nsize] = no;
                    nsize++;
                }
            }
            a.bits = nbits;
            a.offs = noffs;
            a._size = nsize;
        }

        /**
         * Gets the hashcode.
         */
        public override int GetHashCode() 
        {
            long h = 1234;
            for (int i=0; i<_size; i++)
                h ^= bits[i] * offs[i];
           return (int)((h >> 32) ^ h);
        }

        /**
         * Calculates and returns the Set's size
         */
        public int size() 
        {
            return (_size==0)?0:((1+offs[_size-1]) << LG_BITS);
        }

        /**
         * Compares this object against the specified object.
         * @param obj the object to commpare with
         * @return true if the objects are the same; false otherwise.
         */
        public override bool Equals(object obj) 
        {
            if ((obj != null) && (obj.GetType()==typeof(SparseBitSet)))
                return equals(this, (SparseBitSet)obj); 
            return false;
        }
        /**
         * Compares two SparseBitSets for equality.
         * @return true if the objects are the same; false otherwise.
         */
        public static bool equals(SparseBitSet a, SparseBitSet b) 
        {
            for (int i=0, j=0; i<a._size || j<b._size; ) 
            {
                if (i<a._size && (j>=b._size || a.offs[i] < b.offs[j])) 
                {
                    if (a.bits[i++]!=0) return false;
                } 
                else if (j<b._size && (i>=a._size || a.offs[i] > b.offs[j])) 
                {
                    if (b.bits[j++]!=0) return false;
                } 
                else 
                { // equal keys
                    if (a.bits[i++]!=b.bits[j++]) return false;
                }
            }
            return true;
        }

        /**
         * Clones the SparseBitSet.
         */
        public object Clone() 
        {
            //SI:was  clone
                SparseBitSet Set = (SparseBitSet)base.MemberwiseClone();
                Set.bits = (long[]) bits.Clone();
                Set.offs = (int []) offs.Clone();
                return Set;
        }

        /**
         * Return an <code>Enumeration</code> of <code>Integer</code>s
         * which represent Set bit indices in this SparseBitSet.
         */
        public IEnumerator elements() 
        {
            return new mEnum(this);
        }
	
        /**
         * Converts the SparseBitSet to a string.
         */
        public override string ToString() 
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('{');
            IEnumerator e=elements();
            while (e.MoveNext() ) 
            {
                if (sb.Length > 1) sb.Append(", ");
                sb.Append(e.Current);
            }
            sb.Append('}');
            return sb.ToString();
        }

        /** Check validity. */
        private bool isValid() 
        {
            if (bits.Length!=offs.Length) return false;
            if (_size>bits.Length) return false;
            if (_size!=0 && 0<=offs[0]) return false;
            for (int i=1; i<_size; i++)
                if (offs[i] < offs[i-1])
                    return false;
            return true;
        }
        /** Self-test. */
        public static void _Main(string[] args) 
        {
            const int ITER = 500;
            const int RANGE= 65536;
            SparseBitSet a = new SparseBitSet();
            CUtility.ASSERT(!a.Get(0) && !a.Get(1));
            CUtility.ASSERT(!a.Get(123329));
            a.Set(0); CUtility.ASSERT(a.Get(0) && !a.Get(1));
            a.Set(1); CUtility.ASSERT(a.Get(0) && a.Get(1));
            a.clearAll();
            CUtility.ASSERT(!a.Get(0) && !a.Get(1));
            Random r = new Random();
            Vector v = new Vector();
            for (int n=0; n<ITER; n++) 
            {
                int rr = ((r.Next()>>1) % RANGE) << 1;
                a.Set(rr); v.addElement(rr);
                // check that all the numbers are there.
                CUtility.ASSERT(a.Get(rr) && !a.Get(rr+1) && !a.Get(rr-1));
                for (int i=0; i<v.size(); i++)
                    CUtility.ASSERT(a.Get((int)v.elementAt(i)));
            }
            SparseBitSet b = (SparseBitSet) a.Clone();
            CUtility.ASSERT(a.Equals(b) && b.Equals(a));
            for (int n=0; n<ITER/2; n++) 
            {
                int rr = (r.Next()>>1) % v.size();
                int m = (int)v.elementAt(rr);
                b.clear(m); v.removeElementAt(rr);
                // check that numbers are removed properly.
                CUtility.ASSERT(!b.Get(m));
            }
            CUtility.ASSERT(!a.Equals(b));
            SparseBitSet c = (SparseBitSet) a.Clone();
            SparseBitSet d = (SparseBitSet) a.Clone();
            c.and(a);
            CUtility.ASSERT(c.Equals(a) && a.Equals(c));
            c.xor(a);
            CUtility.ASSERT(!c.Equals(a) && c.size()==0);
            d.or(b);
            CUtility.ASSERT(d.Equals(a) && !b.Equals(d));
            d.and(b);
            CUtility.ASSERT(!d.Equals(a) && b.Equals(d));
            d.xor(a);
            CUtility.ASSERT(!d.Equals(a) && !b.Equals(d));
            c.or(d); c.or(b);
            CUtility.ASSERT(c.Equals(a) && a.Equals(c));
            c = (SparseBitSet) d.Clone();
            c.and(b);
            CUtility.ASSERT(c.size()==0);
            System.Console.WriteLine("Success.");
        }
    }
}

