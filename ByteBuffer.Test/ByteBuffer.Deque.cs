namespace ByteBuffer.Test;

[TestClass]
public class ByteBuffer_Deque
{
	[TestMethod]
	[DataRow(100000, 10, 20)]
	[DataRow(10000000, 1000000, 2000000)]
	public void FunctionsAsDeque_AddRange_PopRange(int nItems, int maxWrite, int maxRead)
	{
		var rng = new Random();
		var bytesToFeed = new byte[nItems];
		rng.NextBytes(bytesToFeed);

		ByteBuffer buffer = [];

		int writeIdx = 0;
		int readIdx = 0;

		var bytesRead = new byte[nItems];

		while (readIdx < nItems)
		{
			int nToWrite = rng.Next(0, Math.Min(maxWrite, nItems - writeIdx) + 1);
			buffer.AddRange(bytesToFeed.AsSpan(writeIdx, nToWrite));
			writeIdx += nToWrite;

			int nToRead = rng.Next(0, Math.Min(maxRead, nItems - readIdx) + 1);

			int bufferCountBeforeRead = buffer.Count;

			int nPopped = buffer.PopRange(bytesRead.AsSpan(readIdx, nToRead));

			Assert.AreEqual(nPopped, Math.Min(nToRead, bufferCountBeforeRead));

			readIdx += nPopped;
		}

		Assert.AreEqual(writeIdx, nItems);
		Assert.AreEqual(readIdx, nItems);

		Assert.AreEqual(buffer.Count, 0);
		byte[] tmp = new byte[100];

		Assert.AreEqual(0, buffer.PeekRange(tmp));
		Assert.AreEqual(0, buffer.PeekRange(tmp, 100));
		Assert.AreEqual(0, buffer.PopRange(tmp));
		Assert.AreEqual(buffer.Count, 0);

		for (int i = 0; i < nItems; i++)
		{
			Assert.AreEqual(bytesToFeed[i], bytesRead[i]);
		}
	}

	[TestMethod]
	[DataRow(100000, 10, 20)]
	[DataRow(10000000, 1000000, 2000000)]
	public void FunctionsAsDeque_AddRange_PeekRange(int nItems, int maxWrite, int maxRead)
	{
		var rng = new Random();
		var bytesToFeed = new byte[nItems];
		rng.NextBytes(bytesToFeed);

		ByteBuffer buffer = [];

		int writeIdx = 0;
		int readIdx = 0;

		var bytesRead = new byte[nItems];

		while (readIdx < nItems)
		{
			int nToWrite = rng.Next(0, Math.Min(maxWrite, nItems - writeIdx) + 1);
			buffer.AddRange(bytesToFeed.AsSpan(writeIdx, nToWrite));
			writeIdx += nToWrite;

			int nToRead = rng.Next(0, Math.Min(maxRead, nItems - readIdx) + 1);

			int bufferCountBeforeRead = buffer.Count;

			int nPeeked = buffer.PeekRange(bytesRead.AsSpan(readIdx, nToRead), readIdx);

			Assert.AreEqual(nPeeked, Math.Min(nToRead, bufferCountBeforeRead - readIdx));
			Assert.AreEqual(bufferCountBeforeRead, buffer.Count);

			readIdx += nPeeked;
		}

		Assert.AreEqual(writeIdx, nItems);
		Assert.AreEqual(readIdx, nItems);

		Assert.AreEqual(buffer.Count, nItems);
		byte[] tmp = new byte[100];

		Assert.AreEqual(Math.Min(nItems, 100), buffer.PeekRange(tmp));
		Assert.AreEqual(Math.Min(nItems, 100), buffer.PeekRange(tmp, 100));
		Assert.AreEqual(Math.Min(nItems, 100), buffer.PopRange(tmp));
		Assert.AreEqual(buffer.Count, Math.Max(nItems - 100, 0));

		for (int i = 0; i < nItems; i++)
		{
			Assert.AreEqual(bytesToFeed[i], bytesRead[i]);
		}
	}
}
