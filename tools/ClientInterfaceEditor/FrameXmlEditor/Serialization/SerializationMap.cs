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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.MultiverseInterfaceStudio.FrameXml.Serialization
{
	internal class SerializationMap<TKey, TValue> : IDictionary<TKey, TValue>
	{
		internal SerializationMap()
		{
			this.Reset();
		}

		private bool keysSet = true;

		private bool valuesSet = true;

		private IList<KeyValuePair<TKey, TValue>> keyValuePairs;

		private void Reset()
		{
			this.keysSet = true;
			this.valuesSet = true;
			this.keyValuePairs = new List<KeyValuePair<TKey, TValue>>();
		}

		private void NewFromValues(TValue[] values)
		{
			this.keyValuePairs = new List<KeyValuePair<TKey, TValue>>(values.Length);
			foreach (TValue value in values)
			{
				this.keyValuePairs.Add(new KeyValuePair<TKey, TValue>(default(TKey), value));
			}

			this.keysSet = false;
			this.valuesSet = true;
		}

		private void UpdateWithValues(TValue[] values)
		{
			int count = this.keyValuePairs.Count;
			for (int index = 0; index < count; index++)
			{
				this.keyValuePairs[index] =
					new KeyValuePair<TKey, TValue>(this.keyValuePairs[index].Key, values[index]);
			}

			this.keysSet = true;
			this.valuesSet = true;

		}

		private void NewFromKeys(TKey[] keys)
		{
			this.keyValuePairs = new List<KeyValuePair<TKey, TValue>>(keys.Length);
			foreach (TKey key in keys)
			{
				this.keyValuePairs.Add(new KeyValuePair<TKey, TValue>(key, default(TValue)));
			}

			this.keysSet = true;
			this.valuesSet = false;

		}

		private void UpdateWithKeys(TKey[] keys)
		{
			int count = this.keyValuePairs.Count;
			for (int index = 0; index < count; index++)
			{
				this.keyValuePairs[index] =
					new KeyValuePair<TKey, TValue>(keys[index], this.keyValuePairs[index].Value);
			}

			this.keysSet = true;
			this.valuesSet = true;
		}

		private TKey[] GetKeys()
		{
			if ((!this.keysSet) || (this.keyValuePairs == null))
			{
				return null;
			}

			int count = this.keyValuePairs.Count;
			TKey[] keys = new TKey[count];
			for (int index = 0; index < count; index++)
			{
				keys[index] = this.keyValuePairs[index].Key;
			}

			return keys;
		}

		private TValue[] GetValues()
		{
			if ((!this.valuesSet) || (this.keyValuePairs == null))
			{
				return null;
			}

			int count = this.keyValuePairs.Count;
			TValue[] values = new TValue[count];
			for (int index = 0; index < count; index++)
			{
				values[index] = this.keyValuePairs[index].Value;
			}

			return values;
		}

		private KeyValuePair<TKey, TValue>? FindByKey(TKey key)
		{
			if ((!this.keysSet) || (this.keyValuePairs == null))
			{
				return null;
			}

			foreach (KeyValuePair<TKey, TValue> keyValuePair in this.keyValuePairs)
			{
				if (keyValuePair.Key.Equals(key))
				{
					return keyValuePair;
				}
			}

			return null;
		}

		public TKey[] KeysArray
		{
			get
			{
				return this.GetKeys();
			}
			set
			{
				if (value == null)
				{
					this.Reset();
				}
				else if ((!this.keysSet) && this.valuesSet)
				{
					this.UpdateWithKeys(value);
				}
				else
				{
					this.NewFromKeys(value);
				}
			}
		}

		public TValue[] ValuesArray
		{
			get
			{
				return this.GetValues();
			}
			set
			{
				if (value == null)
				{
					this.Reset();
				}
				else if ((!this.valuesSet) && this.keysSet)
				{
					this.UpdateWithValues(value);
				}
				else
				{
					this.NewFromValues(value);
				}
			}
		}

		#region IDictionary<TKey,TValue> Members

		void IDictionary<TKey,TValue>.Add(TKey key, TValue value)
		{
			this.keyValuePairs.Add(new KeyValuePair<TKey,TValue>(key, value));
		}

		bool IDictionary<TKey,TValue>.ContainsKey(TKey key)
		{
			KeyValuePair<TKey, TValue>? keyValuePair = this.FindByKey(key);

			return keyValuePair.HasValue;
		}

		ICollection<TKey> IDictionary<TKey, TValue>.Keys
		{
			get
			{
				IList<TKey> keys = new List<TKey>();

				if ((!this.keysSet) || (this.keyValuePairs == null))
				{
					return keys;
				}

				foreach (KeyValuePair<TKey, TValue> keyValuePair in this.keyValuePairs)
				{
					keys.Add(keyValuePair.Key);
				}

				return keys;
			}
		}

		bool IDictionary<TKey, TValue>.Remove(TKey key)
		{
			KeyValuePair<TKey, TValue>? keyValuePair = this.FindByKey(key);
			bool found = keyValuePair.HasValue;

			if (found)
			{
				this.keyValuePairs.Remove(keyValuePair.Value);
			}

			return found;
		}

		bool IDictionary<TKey, TValue>.TryGetValue(TKey key, out TValue value)
		{
			try
			{
				KeyValuePair<TKey, TValue>? keyValuePair = this.FindByKey(key);
				value = keyValuePair.Value.Value;
				return true;
			}
			catch
			{
				value = default(TValue);
				return false;
			}
		}

		ICollection<TValue> IDictionary<TKey, TValue>.Values
		{
			get
			{
				TValue[] values = this.ValuesArray;
				return (values == null) ? new List<TValue>() : new List<TValue>(values);
			}
		}

		TValue IDictionary<TKey, TValue>.this[TKey key]
		{
			get
			{
				KeyValuePair<TKey, TValue>? keyValuePair = this.FindByKey(key);
				if (!keyValuePair.HasValue)
				{
					throw new KeyNotFoundException("The given key was not present.");
				}

				return keyValuePair.Value.Value;

			}
			set
			{
				KeyValuePair<TKey, TValue>? keyValuePair = this.FindByKey(key);

				if (keyValuePair.HasValue)
				{
					if (!keyValuePair.Value.Value.Equals(value))
					{
						this.keyValuePairs.Remove(keyValuePair.Value);
						this.keyValuePairs.Add(new KeyValuePair<TKey, TValue>(key, value));
					}
				}
				else
				{
					this.keyValuePairs.Add(new KeyValuePair<TKey, TValue>(key, value));
				}

			}
		}

		#endregion

		#region ICollection<KeyValuePair<TKey,TValue>> Members

		void ICollection<KeyValuePair<TKey,TValue>>.Add(KeyValuePair<TKey, TValue> item)
		{
			this.keyValuePairs.Add(item);
		}

		void ICollection<KeyValuePair<TKey, TValue>>.Clear()
		{
			this.keyValuePairs.Clear();
		}

		bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
		{
			KeyValuePair<TKey, TValue>? keyValuePair = this.FindByKey(item.Key);
			return ((keyValuePair.HasValue) && (keyValuePair.Value.Value.Equals(item.Value)));
		}

		void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			this.keyValuePairs.CopyTo(array, arrayIndex);
		}

		int ICollection<KeyValuePair<TKey, TValue>>.Count
		{
			get { return this.keyValuePairs.Count; }
		}

		bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
		{
			get { return false; }
		}

		bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
		{
			KeyValuePair<TKey, TValue>? keyValuePair = this.FindByKey(item.Key);
			bool found = ((keyValuePair.HasValue) && (keyValuePair.Value.Value.Equals(item.Value)));
			if (found)
			{
				this.keyValuePairs.Remove(keyValuePair.Value);
			}

			return found;
		}

		#endregion

		#region IEnumerable<KeyValuePair<TKey,TValue>> Members

		IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey,TValue>>.GetEnumerator()
		{
			return this.keyValuePairs.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.keyValuePairs.GetEnumerator();
		}

		#endregion
	}
}
