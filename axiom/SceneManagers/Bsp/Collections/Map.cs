using System;
using System.Collections;
using Axiom.MathLib.Collections;

namespace Axiom.SceneManagers.Bsp.Collections {
    /// <summary>
    ///		Map class, a little different from the Axiom.Collections.Map class
    /// </summary>
    /// <remarks>
    ///     A map allows multiple values per key, unlike the Hashtable which only allows
    ///     unique keys and only a single value per key.  Multiple values assigned to the same
    ///     key are placed in a "bucket", which in this case is an ArrayList.
    ///     <p/>
    ///     An example of values in a map would look like this:
    ///     Key     Value
    ///     "a"     "Alan"
    ///     "a"     "Adam"
    ///     "b"     "Brien"
    ///     "c"     "Chris"
    ///     "c"     "Carl"
    ///     etc
    ///     <p/>
    ///     Currently, enumeration is the only way to iterate through the values, which is
    ///     more pratical in terms of how the Map works internally anyway.  Intial testing showed
    ///     that inserting and iterating through 100,000 items, the Inserts took ~260ms and a full
    ///     enumeration of them all (with unboxing of the value type stored in the map) took between 16-30ms.
    /// </remarks>
    public class Map {
        #region Fields

        /// <summary>
        ///     Number of total items currently in this map.
        /// </summary>
        protected int count;

        /// <summary>
        ///     A list of buckets.
        /// </summary>
        public Hashtable buckets;

        #endregion Fields

        #region Constructor

        /// <summary>
        ///     Default constructor.
        /// </summary>
        public Map()
		{
			buckets = new Hashtable();
        }

        #endregion Constructor

        /// <summary>
        ///     Clears this map of all contained objects.
        /// </summary>
        public void Clear() {
            buckets.Clear();
			count = 0;
        }

		/// <summary>
		///     Clears the bucket with given key.
		/// </summary>
		public void Clear(object key) 
		{
			ArrayList bucket = (ArrayList)buckets[key];
			if (bucket != null)
			{
				count -= bucket.Count;
				buckets.Remove(key);
			}
		}

        /// <summary>
        ///     Given a key, Find will return an IEnumerator that allows
        ///     you to iterate over all items in the bucket associated
        ///     with the key.
        /// </summary>
        /// <param name="key">Key for look for.</param>
        /// <returns>IEnumerator to go through the items assigned to the key.</returns>
        public IEnumerator Find(object key) 
		{
            if(buckets[key] == null) {
                return null;
            }
            else {
                return ((ArrayList)buckets[key]).GetEnumerator();
            }
        }

		public IList FindBucket(object key) {
			if(buckets[key] == null) {
				return null;
			}
			else {
				return (ArrayList)buckets[key];
			}
		}

		/// <summary>
		///     Given a key, FindFirst will return the first item in the bucket
		///     associated with the key.
		/// </summary>
		/// <param name="key">Key to look for.</param>
		public object FindFirst(object key) 
		{
			if(buckets[key] == null) 
			{
				return null;
			}
			else 
			{
				return ((ArrayList)buckets[key])[0];
			}
		}

        /// <summary>
        ///     Gets the count of objects mapped to the specified key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public int Count(object key) 
		{
            if(buckets[key] != null) {
                return ((ArrayList)buckets[key]).Count;
            }

            return 0;
        }

        /// <summary>
        ///     Inserts a value into a bucket that is specified by the
        ///     key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val"></param>
        public void Insert(object key, object val) {
            ArrayList container = null;

            if(buckets[key] == null) {
                container = new ArrayList();
                buckets.Add(key, container);
            }
            else {
                container = (ArrayList)buckets[key];
            }

            // TODO: Doing the contains check is extremely slow, so for now duplicate items are allowed
            //if(!container.Contains(val)) {
            container.Add(val);
            count++;
            //}
        }

        /// <summary>
        ///     Gets the total count of all items contained within the map.
        /// </summary>
        public int TotalCount {
            get {
                return count;
            }
        }

		/// <summary>
		///     Gets an appropriate enumerator for the map, customized to go
		///     through each key in the map and return a Pair of the key and
		///     an ArrayList of the values associated with it.
		/// </summary>
		/// <returns></returns>
		public IEnumerator GetBucketEnumerator() 
		{
			return buckets.Keys.GetEnumerator();
		}
    }
}
