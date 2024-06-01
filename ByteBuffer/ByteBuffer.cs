﻿namespace ByteBuffer;

public class ByteBuffer
{
	private readonly Queue<byte[]> blocks = [];
	private byte[] backBlock = ByteBlock.NewBlock();
	private byte[]? frontBlock = null;
	public int Count { get; private set; } = 0;

	public void AddRange(ReadOnlyMemory<byte> bytes)
	{
		ReadOnlySpan<byte> bytesToWrite = bytes.Span;

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

	public int PopRange(Memory<byte> memory)
	{
		int totalRead = 0;
		Span<byte> bytesToRead = memory.Span;

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
		if (index < 0 || index >= blocks.Count)
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