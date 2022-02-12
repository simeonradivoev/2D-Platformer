using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;

namespace DefaultNamespace
{
	public static class EnumerableExtensions
	{
		public static void Fill<T>(this T[] buffer, T val)
		{
			for (var i = 0; i < buffer.Length; i++)
			{
				buffer[i] = val;
			}
		}

		public static int FindMinIndex<T>(this IReadOnlyList<T> list, Func<T, float> valueGetter)
		{
			var closestValue = float.MaxValue;
			var closestIndex = -1;

			for (var i = 0; i < list.Count; i++)
			{
				var val = valueGetter.Invoke(list[i]);
				if (val < closestValue || closestIndex < 0)
				{
					closestValue = val;
					closestIndex = i;
				}
			}

			return closestIndex;
		}

		public static int FindMinIndex<T>(this NativeList<T> list, Func<T, float> valueGetter) where T : struct
		{
			var closestValue = float.MaxValue;
			var closestIndex = -1;

			for (var i = 0; i < list.Length; i++)
			{
				var val = valueGetter.Invoke(list[i]);
				if (val < closestValue || closestIndex < 0)
				{
					closestValue = val;
					closestIndex = i;
				}
			}

			return closestIndex;
		}

		public static T MinValue<T>(this IEnumerable<T> list, Func<T, float> valueGetter)
		{
			var closestDist = float.PositiveInfinity;
			T obj = default;
			var isEmpty = true;
			foreach (var val in list)
			{
				var dist = valueGetter(val);
				if (dist <= closestDist)
				{
					closestDist = dist;
					obj = val;
				}

				isEmpty = false;
			}

			if (isEmpty)
			{
				throw new ArgumentException();
			}
			return obj;
		}

		public static int FindIndex<T>(this NativeList<T> val, Predicate<T> predicate) where T : struct
		{
			return FindIndex((NativeArray<T>)val, predicate);
		}

		public static int FindIndex<T>(this NativeArray<T> val, Predicate<T> predicate) where T : struct
		{
			for (var i = 0; i < val.Length; i++)
			{
				if (predicate.Invoke(val[i]))
				{
					return i;
				}
			}

			return -1;
		}

		public static void Reverse<T>(this NativeList<T> val) where T : struct
		{
			Reverse((NativeArray<T>)val);
		}

		public static void Reverse<T>(this NativeArray<T> val) where T : struct
		{
			var tmp = new NativeArray<T>(val.Length, Allocator.Temp);
			for (var i = 0; i < val.Length; i++)
			{
				tmp[val.Length - i - 1] = val[i];
			}
			val.CopyFrom(tmp);
			tmp.Dispose();
		}

		public static NativeArray<T> Fill<T>(this NativeArray<T> val, T v) where T : struct
		{
			for (var i = 0; i < val.Length; i++)
			{
				val[i] = v;
			}

			return val;
		}

		public static IEnumerator<T> Enumerate<T>(this NativeList<T> val) where T : struct
		{
			for (var i = 0; i < val.Length; i++)
			{
				yield return val[i];
			}
		}

		public static IEnumerator<T> Enumerate<T>(this NativeArray<T> val) where T : struct
		{
			for (var i = 0; i < val.Length; i++)
			{
				yield return val[i];
			}
		}

		public static bool CheckIterate<T>(this IEnumerable<T> list) where T : struct
		{
			return list.Any();
		}

		public static void Iterate<T>(this IEnumerable<T> list) where T : struct
		{
			foreach (var unused in list)
			{
			}
		}

		public static IEnumerable<(NativeArray<T>, int)> NativeForEach<T>(this NativeArray<T> list, Func<T, T> func) where T : struct
		{
			for (var i = 0; i < list.Length; i++)
			{
				list[i] = func.Invoke(list[i]);
				yield return (list, i);
			}
		}

		public static IEnumerable<(NativeSlice<T>, int)> NativeForEach<T>(this NativeSlice<T> list, Func<T, T> func) where T : struct
		{
			for (var i = 0; i < list.Length; i++)
			{
				list[i] = func.Invoke(list[i]);
				yield return (list, i);
			}
		}

		public static IEnumerable<(NativeArray<T>, int)> NativeForEach
			<T>(this IEnumerable<(NativeArray<T> array, int index)> list, Func<T, T> func) where T : struct
		{
			foreach (var tuple in list)
			{
				var array = tuple.array;
				array[tuple.index] = func.Invoke(tuple.array[tuple.index]);
				yield return tuple;
			}
		}

		public static IEnumerable<(NativeSlice<T>, int)> NativeForEach
			<T>(this IEnumerable<(NativeSlice<T> array, int index)> list, Func<T, T> func) where T : struct
		{
			foreach (var tuple in list)
			{
				var array = tuple.array;
				array[tuple.index] = func.Invoke(tuple.array[tuple.index]);
				yield return tuple;
			}
		}

		public static IEnumerable<(NativeArray<T>, int)> NativeWhere<T>(this NativeArray<T> list, Predicate<T> predicate) where T : struct
		{
			for (var i = 0; i < list.Length; i++)
			{
				var eVal = list[i];
				if (predicate.Invoke(eVal))
				{
					yield return (list, i);
				}
			}
		}

		public static IEnumerable<(NativeSlice<T>, int)> NativeWhere<T>(this NativeSlice<T> list, Predicate<T> predicate) where T : struct
		{
			for (var i = 0; i < list.Length; i++)
			{
				var eVal = list[i];
				if (predicate.Invoke(eVal))
				{
					yield return (list, i);
				}
			}
		}

		public static (DynamicBuffer<T>, int)? NativeFirstOrOptional<T>(this DynamicBuffer<T> buffer, Predicate<T> predicate) where T : struct
		{
			for (var i = 0; i < buffer.Length; i++)
			{
				var eVal = buffer[i];
				if (predicate.Invoke(eVal))
				{
					return (buffer, i);
				}
			}

			return null;
		}

		public static (NativeSlice<T>, int)? NativeFirstOrOptional<T>(this NativeSlice<T> list, Predicate<T> predicate) where T : struct
		{
			for (var i = 0; i < list.Length; i++)
			{
				var eVal = list[i];
				if (predicate.Invoke(eVal))
				{
					return (list, i);
				}
			}

			return null;
		}

		public static (NativeArray<T>, int)? NativeFirstOrOptional<T>(this NativeArray<T> list, Predicate<T> predicate) where T : struct
		{
			for (var i = 0; i < list.Length; i++)
			{
				var eVal = list[i];
				if (predicate.Invoke(eVal))
				{
					return (list, i);
				}
			}

			return null;
		}

		public static bool NativeModify<T>(this (DynamicBuffer<T>, int)? val, Func<T, T> func) where T : struct
		{
			if (val.HasValue)
			{
				var list = val.Value.Item1;
				var index = val.Value.Item2;
				list[index] = func.Invoke(list[index]);
				return true;
			}

			return false;
		}

		public static bool NativeModify<T>(this (NativeSlice<T>, int)? val, Func<T, T> func) where T : struct
		{
			if (val.HasValue)
			{
				var list = val.Value.Item1;
				var index = val.Value.Item2;
				list[index] = func.Invoke(list[index]);
				return true;
			}

			return false;
		}

		public static bool NativeModify<T>(this (NativeArray<T>, int)? val, Func<T, T> func) where T : struct
		{
			if (val.HasValue)
			{
				var list = val.Value.Item1;
				var index = val.Value.Item2;
				list[index] = func.Invoke(list[index]);
				return true;
			}

			return false;
		}

		private static bool EmptyPredicate<T>(T val)
		{
			return true;
		}

		public static DynamicBufferEnumerator<T> Begin<T>(this DynamicBuffer<T> val) where T : struct, IBufferElementData
		{
			return new DynamicBufferEnumerator<T> { Predicate = EmptyPredicate, Buffer = val, Index = 0 };
		}

		public static DynamicBufferEnumerator<T> Where<T>(this DynamicBufferEnumerator<T> val, Predicate<T> func) where T : struct, IBufferElementData
		{
			return new DynamicBufferEnumerator<T>
			{
				Buffer = val.Buffer, Index = 0, Predicate = v => (val.Predicate == null || val.Predicate.Invoke(v)) && func.Invoke(v)
			};
		}

		public static float Sum<T>(this DynamicBufferEnumerator<T> val, Func<T, float> sum) where T : struct, IBufferElementData
		{
			float final = 0;

			for (var i = val.Index; i < val.Buffer.Length; i++)
			{
				if (val.Predicate(val.Buffer[i]))
				{
					final += sum.Invoke(val.Buffer[i]);
				}
			}

			return final;
		}

		public static int Sum<T>(this DynamicBufferEnumerator<T> val, Func<T, int> sum) where T : struct, IBufferElementData
		{
			var final = 0;

			for (var i = val.Index; i < val.Buffer.Length; i++)
			{
				if (val.Predicate(val.Buffer[i]))
				{
					final += sum.Invoke(val.Buffer[i]);
				}
			}

			return final;
		}

		public static bool Any<T>(this DynamicBufferEnumerator<T> val, Predicate<T> predicate) where T : struct, IBufferElementData
		{
			for (var i = val.Index; i < val.Buffer.Length; i++)
			{
				if (val.Predicate(val.Buffer[i]) && predicate.Invoke(val.Buffer[i]))
				{
					return true;
				}
			}

			return false;
		}

		public static T FirstOrDefault<T>(this DynamicBufferEnumerator<T> val, Predicate<T> predicate) where T : struct, IBufferElementData
		{
			for (var i = val.Index; i < val.Buffer.Length; i++)
			{
				if (val.Predicate(val.Buffer[i]) && predicate.Invoke(val.Buffer[i]))
				{
					return val.Buffer[i];
				}
			}

			return default;
		}

		public static int IndexOf<T>(this DynamicBufferEnumerator<T> val, Predicate<T> predicate) where T : struct, IBufferElementData
		{
			for (var i = val.Index; i < val.Buffer.Length; i++)
			{
				if (val.Predicate(val.Buffer[i]) && predicate.Invoke(val.Buffer[i]))
				{
					return i;
				}
			}
			return -1;
		}

		public static void Fill<T>(this DynamicBuffer<T> buffer, int amount) where T : struct, IBufferElementData
		{
			for (var i = 0; i < amount; i++)
			{
				buffer.Add(default);
			}
		}

		public static NativeHashMapEnum<T, TK> GetEnumerator<T, TK>(this NativeMultiHashMap<T, TK> map, T key)
			where T : struct, IEquatable<T> where TK : struct
		{
			return new NativeHashMapEnum<T, TK> { map = map, key = key };
		}

		public class EnumeratorEndException : Exception
		{
		}

		public struct DynamicBufferEnumerator<T> where T : struct, IBufferElementData
		{
			public int Index;
			public DynamicBuffer<T> Buffer;
			public Predicate<T> Predicate;
		}

		public struct NativeHashMapEnum<T, TK> : IEnumerable<TK> where T : struct, IEquatable<T> where TK : struct
		{
			public NativeMultiHashMap<T, TK> map;
			public T key;

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}

			public IEnumerator<TK> GetEnumerator()
			{
				return new NativeHashMapIt<T, TK> { map = map, key = key };
			}
		}

		public struct NativeHashMapIt<T, TK> : IEnumerator<TK> where T : struct, IEquatable<T> where TK : struct
		{
			public T key;
			public TK current;
			public NativeMultiHashMapIterator<T> it;
			public NativeMultiHashMap<T, TK> map;
			public bool isNotFirst;
			public bool hasMore;

			public bool MoveNext()
			{
				if (!isNotFirst)
				{
					hasMore = map.TryGetFirstValue(key, out current, out it);
					isNotFirst = true;
				}
				else
				{
					hasMore = map.TryGetNextValue(out current, ref it);
				}

				return hasMore;
			}

			public void Reset()
			{
				isNotFirst = false;
			}

			object IEnumerator.Current => Current;

			public void Dispose()
			{
			}

			public TK Current => current;
		}
	}
}