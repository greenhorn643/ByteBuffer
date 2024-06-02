namespace ByteBuffer;

public partial class ByteBuffer
{
	public void Advance(int count)
	{
		ByteBlock.SetBlockEnd(backBlock,
			ByteBlock.GetBlockEnd(backBlock) + count);
		Count += count;
	}

	public Memory<byte> GetMemory(int sizeHint = 0)
	{
		EnsureBackBlockHasMinimumCapacity(sizeHint);
		return ByteBlock.AsMemory(backBlock);
	}

	public Span<byte> GetSpan(int sizeHint = 0)
	{
		EnsureBackBlockHasMinimumCapacity(sizeHint);
		return ByteBlock.AsSpan(backBlock);
	}

	private void EnsureBackBlockHasMinimumCapacity(int sizeHint)
	{
		if (sizeHint > 0 && ByteBlock.GetRemainingCapacity(backBlock) < sizeHint)
		{
			if (ByteBlock.GetDataLength(backBlock) != 0)
			{
				blocks.Enqueue(backBlock);
			}

			backBlock = ByteBlock.NewBlockWithMinimumCapacity(sizeHint);
		}
	}
}
