using System.Buffers;

namespace ByteBuffer;

public partial class ByteBuffer : IList<byte>, IBufferWriter<byte>
{
	private readonly Queue<byte[]> blocks = [];
	private byte[] backBlock = ByteBlock.NewBlock();
	private byte[]? frontBlock = null;
	public int Count { get; private set; } = 0;

	public void AddRange(ReadOnlySpan<byte> bytes)
	{
		ReadOnlySpan<byte> bytesToWrite = bytes[..];

		while (bytesToWrite.Length > 0)
		{
			int remainingCapacity = ByteBlock.GetRemainingCapacity(backBlock);
			if (remainingCapacity > bytesToWrite.Length)
			{
				ByteBlock.Write(backBlock, bytesToWrite);
				bytesToWrite = [];
			}
			else
			{
				ByteBlock.Write(backBlock, bytesToWrite[..remainingCapacity]);
				blocks.Enqueue(backBlock);
				backBlock = ByteBlock.NewBlock();
				bytesToWrite = bytesToWrite[remainingCapacity..];
			}
		}

		Count += bytes.Length;
	}

	public int PeekRange(Span<byte> span, int start = 0)
	{
		int totalRead = 0;
		Span<byte> bytesToRead = span[..];

		IEnumerable<byte[]> allBlocks = frontBlock == null
			? [.. blocks, backBlock]
			: [frontBlock, .. blocks, backBlock];

		foreach (var block in allBlocks)
		{
			int blockLength = ByteBlock.GetDataLength(block);

			if (blockLength >= bytesToRead.Length + start)
			{
				ByteBlock.Read(block, bytesToRead, start);
				totalRead += bytesToRead.Length;
				break;
			}
			else if (blockLength > start)
			{
				ByteBlock.Read(block, bytesToRead[..(blockLength - start)], start);
				bytesToRead = bytesToRead[(blockLength - start)..];
				totalRead += blockLength - start;
				start = 0;
			}
			else
			{
				start -= blockLength;
			}
		}

		return totalRead;
	}

	public int PopRange(Span<byte> bytes)
	{
		int totalRead = 0;
		Span<byte> bytesToRead = bytes[..];

		while (bytesToRead.Length > 0)
		{
			if (frontBlock == null)
			{
				if (blocks.Count == 0)
				{
					totalRead += PopFromBackBlock(bytesToRead);
					break;
				}
				else
				{
					frontBlock = blocks.Dequeue();
				}
			}

			int blockLength = ByteBlock.GetDataLength(frontBlock);

			if (blockLength > bytesToRead.Length)
			{
				ByteBlock.ReadAndPop(frontBlock, bytesToRead);
				totalRead += bytesToRead.Length;
				bytesToRead = [];
			}
			else
			{
				ByteBlock.ReadAndPop(frontBlock, bytesToRead[..blockLength]);
				totalRead += blockLength;
				bytesToRead = bytesToRead[blockLength..];
				frontBlock = null;
			}
		}

		Count -= totalRead;
		return totalRead;
	}

	public byte ByteAt(int index)
	{
		if (index < 0 || index >= Count)
		{
			throw new IndexOutOfRangeException(nameof(ByteAt));
		}

		if (frontBlock != null)
		{
			int frontBlockLength = ByteBlock.GetDataLength(frontBlock);
			if (index < frontBlockLength)
			{
				return ByteBlock.GetByteAtUnchecked(frontBlock, index);
			}

			index -= frontBlockLength;
		}

		foreach (var block in blocks)
		{
			int blockLength = ByteBlock.GetDataLength(block);
			if (index < blockLength)
			{
				return ByteBlock.GetByteAtUnchecked(block, index);
			}
			index -= blockLength;
		}

		return ByteBlock.GetByteAtUnchecked(backBlock, index);
	}

	public int ReplaceRange(ReadOnlySpan<byte> replacement, int start)
	{
		int totalResplaced = 0;
		ReadOnlySpan<byte> bytesToReplace = replacement[..];

		IEnumerable<byte[]> allBlocks = frontBlock == null
			? [.. blocks, backBlock]
			: [frontBlock, .. blocks, backBlock];

		foreach (var block in allBlocks)
		{
			int blockLength = ByteBlock.GetDataLength(block);

			if (blockLength >= bytesToReplace.Length + start)
			{
				ByteBlock.ReplaceUnchecked(block, bytesToReplace, start);
				totalResplaced += bytesToReplace.Length;
				break;
			}
			else if (blockLength > start)
			{
				ByteBlock.ReplaceUnchecked(block, bytesToReplace[..(blockLength - start)], start);
				bytesToReplace = bytesToReplace[(blockLength - start)..];
				totalResplaced += blockLength - start;
				start = 0;
			}
			else
			{
				start -= blockLength;
			}
		}

		return totalResplaced;
	}

	public byte this[int index]
	{
		get => ByteAt(index);
	}

	private int PopFromBackBlock(Span<byte> bytesToRead)
	{
		int backBlockLength = ByteBlock.GetDataLength(backBlock);

		if (backBlockLength > bytesToRead.Length)
		{
			ByteBlock.ReadAndPop(backBlock, bytesToRead);
			return bytesToRead.Length;
		}
		else
		{
			ByteBlock.ReadAndPop(backBlock, bytesToRead[..backBlockLength]);
			ByteBlock.Reset(backBlock);
			return backBlockLength;
		}
	}
}
