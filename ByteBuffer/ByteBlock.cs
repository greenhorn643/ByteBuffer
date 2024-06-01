namespace ByteBuffer;

internal static class ByteBlock
{
	const int BLOCK_SIZE = 4096;

	public static byte[] NewBlock()
	{
		var block = new byte[BLOCK_SIZE];
		Reset(block);
		return block;
	}

	public static int GetBlockStart(byte[] block)
	{
		return (block[0] | (block[1] << 8));
	}

	public static int GetBlockEnd(byte[] block)
	{
		return (block[2] | (block[3] << 8));
	}

	public static int GetRemainingCapacity(byte[] block)
	{
		return BLOCK_SIZE - GetBlockEnd(block);
	}

	public static int GetDataLength(byte[] block)
	{
		return GetBlockEnd(block) - GetBlockStart(block);
	}

	public static byte GetByteAtUnchecked(byte[] block, int index)
	{
		int offsetIndex = index + GetBlockStart(block);
		return block[offsetIndex];
	}

	public static byte GetByteAt(byte[] block, int index)
	{
		if (index < 0 || index > GetDataLength(block))
		{
			throw new IndexOutOfRangeException(nameof(GetByteAt));
		}

		return GetByteAtUnchecked(block, index);
	}

	public static void SetBlockStart(byte[] block, int blockStart)
	{
		block[0] = (byte)blockStart;
		block[1] = (byte)(blockStart >> 8);
	}

	public static void SetBlockEnd(byte[] block, int blockEnd)
	{
		block[2] = (byte)blockEnd;
		block[3] = (byte)(blockEnd >> 8);
	}

	public static void Write(byte[] block, ReadOnlySpan<byte> bytes)
	{
		int blockEnd = GetBlockEnd(block);
		bytes.CopyTo(block.AsSpan()[blockEnd..]);
		SetBlockEnd(block, blockEnd + bytes.Length);
	}

	public static void ReadAndPop(byte[] block, Span<byte> bytes)
	{
		int blockStart = GetBlockStart(block);
		block.AsSpan().Slice(blockStart, bytes.Length).CopyTo(bytes);
		SetBlockStart(block, blockStart + bytes.Length);
	}

	public static void Read(byte[] block, Span<byte> bytes)
	{
		int blockStart = GetBlockStart(block);
		block.AsSpan().Slice(blockStart, bytes.Length).CopyTo(bytes);
	}

	public static void Reset(byte[] block)
	{
		SetBlockStart(block, 4);
		SetBlockEnd(block, 4);
	}
}
