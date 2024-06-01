namespace ByteBuffer.Test
{
	[TestClass]
	public class ByteBlockTests
	{
		[TestMethod]
		public void NewBlockHasLengthZero()
		{
			var block = ByteBlock.NewBlock();

			Assert.IsNotNull(block);
			Assert.AreEqual(0, ByteBlock.GetDataLength(block));
		}

		[TestMethod]
		public void NewBlockHasPositiveCapacityLessThanBlockSize()
		{
			var block = ByteBlock.NewBlock();

			Assert.IsNotNull(block);
			Assert.IsTrue(ByteBlock.GetRemainingCapacity(block) > 0);
			Assert.IsTrue(ByteBlock.GetRemainingCapacity(block) < block.Length);
		}

		[TestMethod]
		public void WritingDataToBlockAdjustsBlockEndAndSizeAndCapacity()
		{
			var block = ByteBlock.NewBlock();

			Assert.IsNotNull(block);

			int initBlockEnd = ByteBlock.GetBlockEnd(block);
			int initBlockSize = ByteBlock.GetDataLength(block);
			int initBlockCapacity = ByteBlock.GetRemainingCapacity(block);

			ByteBlock.Write(block, []);

			int blockEndAfterWritingEmptyChunk = ByteBlock.GetBlockEnd(block);
			int blockSizeAfterWritingEmptyChunk = ByteBlock.GetDataLength(block);
			int blockCapabityAfterWritingEmptyChunk = ByteBlock.GetRemainingCapacity(block);

			ByteBlock.Write(block, [1, 2, 3]);

			int blockEndAfterWritingChunk = ByteBlock.GetBlockEnd(block);
			int blockSizeAfterWritingChunk = ByteBlock.GetDataLength(block);
			int blockCapacityAfterWritingChunk = ByteBlock.GetRemainingCapacity(block);


			Assert.AreEqual(initBlockSize, 0);

			Assert.AreEqual(initBlockEnd, blockEndAfterWritingEmptyChunk);
			Assert.AreEqual(initBlockSize, blockSizeAfterWritingEmptyChunk);
			Assert.AreEqual(initBlockCapacity, blockCapabityAfterWritingEmptyChunk);

			Assert.AreEqual(initBlockEnd + 3, blockEndAfterWritingChunk);
			Assert.AreEqual(initBlockSize + 3, blockSizeAfterWritingChunk);
			Assert.AreEqual(initBlockCapacity - 3, blockCapacityAfterWritingChunk);
		}

		[TestMethod]
		public void CanWriteAndReadBackData()
		{
			var block = ByteBlock.NewBlock();

			var data = new byte[ByteBlock.GetRemainingCapacity(block)];
			var rng = new Random();

			rng.NextBytes(data);

			int offset = 0;

			while (offset < data.Length)
			{
				int nToWrite = rng.Next(0, Math.Min(50, data.Length - offset + 1));
				ByteBlock.Write(block, data.AsSpan(offset, nToWrite));
				offset += nToWrite;
			}

			Assert.AreEqual(data.Length, ByteBlock.GetDataLength(block));
			Assert.AreEqual(0, ByteBlock.GetRemainingCapacity(block));

			var dataToRead = new byte[data.Length];
			ByteBlock.Read(block, dataToRead);

			foreach (var (l, r) in data.Zip(dataToRead))
			{
				Assert.AreEqual(l, r);
			}
		}

		[TestMethod]
		public void CanInterweaveWritingAndPoppingData()
		{
			var block = ByteBlock.NewBlock();
			var data = new byte[ByteBlock.GetRemainingCapacity(block)];
			var dataToRead = new byte[data.Length];
			var rng = new Random();

			rng.NextBytes(data);

			int writeOffset = 0;
			int readOffset = 0;

			while (readOffset < data.Length)
			{
				int nToRead = rng.Next(0, Math.Min(20, ByteBlock.GetDataLength(block) + 1));
				int nToWrite = rng.Next(0, Math.Min(30, ByteBlock.GetRemainingCapacity(block) + 1));

				ByteBlock.ReadAndPop(block, dataToRead.AsSpan(readOffset, nToRead));
				readOffset += nToRead;

				ByteBlock.Write(block, data.AsSpan(writeOffset, nToWrite));
				writeOffset += nToWrite;
			}

			Assert.AreEqual(0, ByteBlock.GetDataLength(block));
			Assert.AreEqual(0, ByteBlock.GetRemainingCapacity(block));

			foreach (var (l, r) in data.Zip(dataToRead))
			{
				Assert.AreEqual(l, r);
			}
		}

		[TestMethod]
		public void CanGetDataByIndexAfterWriting()
		{
			var block = ByteBlock.NewBlock();
			var data = new byte[ByteBlock.GetRemainingCapacity(block)];
			var rng = new Random();

			int writeOffset = 0;

			while (writeOffset < data.Length)
			{
				int nToWrite = rng.Next(0, Math.Min(30, ByteBlock.GetRemainingCapacity(block) + 1));
				ByteBlock.Write(block, data.AsSpan(writeOffset, nToWrite));
				writeOffset += nToWrite;
			}

			Assert.AreEqual(data.Length, ByteBlock.GetDataLength(block));
			Assert.AreEqual(0, ByteBlock.GetRemainingCapacity(block));

			for (int i = 0; i < data.Length; i++)
			{
				Assert.AreEqual(data[i], ByteBlock.GetByteAt(block, i));
			}
		}

		[TestMethod]
		public void CanGetDataByIndexAfterWritingAndPopping()
		{
			var block = ByteBlock.NewBlock();
			var data = new byte[ByteBlock.GetRemainingCapacity(block)];
			var rng = new Random();

			int writeOffset = 0;

			while (writeOffset < data.Length)
			{
				int nToWrite = rng.Next(0, Math.Min(30, ByteBlock.GetRemainingCapacity(block) + 1));
				ByteBlock.Write(block, data.AsSpan(writeOffset, nToWrite));
				writeOffset += nToWrite;
			}

			Assert.AreEqual(data.Length, ByteBlock.GetDataLength(block));
			Assert.AreEqual(0, ByteBlock.GetRemainingCapacity(block));

			int nPopped = 0;

			while (ByteBlock.GetDataLength(block) > 0)
			{
				int nToPop = rng.Next(0, Math.Min(60, ByteBlock.GetDataLength(block) + 1));
				var dataToPop = new byte[nToPop];
				ByteBlock.ReadAndPop(block, dataToPop);
				nPopped += nToPop;

				for (int i = 0; i < ByteBlock.GetDataLength(block); i++)
				{
					Assert.AreEqual(data[i + nPopped], ByteBlock.GetByteAt(block, i));
				}
			}

			Assert.AreEqual(data.Length, nPopped);
		}
	}
}
