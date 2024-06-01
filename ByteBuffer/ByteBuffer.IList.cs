using System.Collections;

namespace ByteBuffer;

public partial class ByteBuffer
{
	public int IndexOf(byte item)
	{
		for (int i = 0; i < Count; i++)
		{
			if (this[i] == item)
			{
				return i;
			}
		}
		return -1;
	}

	public void Insert(int index, byte item)
	{
		throw new NotSupportedException(nameof(Insert));
	}

	public void RemoveAt(int index)
	{
		throw new NotSupportedException(nameof(RemoveAt));
	}

	public void Add(byte item)
	{
		byte[] bytes = [item];
		AddRange(bytes);
	}

	public void Clear()
	{
		frontBlock = null;
		blocks.Clear();
		ByteBlock.Reset(backBlock);
		Count = 0;
	}

	public bool Contains(byte item)
	{
		return IndexOf(item) != -1;
	}

	public void CopyTo(byte[] array, int arrayIndex)
	{
		if (array.Length - arrayIndex < Count)
		{
			throw new ArgumentException($"{nameof(CopyTo)} {nameof(array)} is too small to hold contents of {nameof(ByteBuffer)}");
		}

		PeekRange(array.AsMemory()[arrayIndex..]);
	}

	public bool Remove(byte item)
	{
		throw new NotSupportedException(nameof(Remove));
	}

	public IEnumerator<byte> GetEnumerator()
	{
		return new Enumerator(this);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return new Enumerator(this);
	}
}
