namespace ByteBuffer.Test
{
	[TestClass]
	public class ByteBuffer_IList
	{
		[TestMethod]
		public void IsConstructibleWithEmptyListPattern()
		{
			ByteBuffer b = [];

			Assert.IsNotNull(b);
			Assert.AreEqual(0, b.Count);
		}

		[TestMethod]
		public void IsConstructibleWithElements()
		{
			ByteBuffer b = [1, 2, 3];

			Assert.IsNotNull(b);
			Assert.AreEqual(3, b.Count);

			foreach (var (l, r) in b.Zip([1, 2, 3]))
			{
				Assert.AreEqual(l, r);
			}
		}


		[TestMethod]
		public void CanGetElementsByIndex()
		{
			ByteBuffer b = [1, 2, 3];

			Assert.IsNotNull(b);

			Assert.AreEqual(3, b.Count);
			Assert.AreEqual(1, b[0]);
			Assert.AreEqual(2, b[1]);
			Assert.AreEqual(3, b[2]);
		}

		[TestMethod]
		public void CanAddGetAndCountIdenticallyToList()
		{
			int rngSeed = new Random().Next();

			var expected = AddGetAndCountTest(new Random(rngSeed), new List<byte>());
			var actual = AddGetAndCountTest(new Random(rngSeed), new ByteBuffer());

			Assert.AreEqual(expected.Count, actual.Count);

			foreach (var (l, r) in expected.Zip(actual))
			{
				Assert.AreEqual(l, r);
			}

			List<int> AddGetAndCountTest(Random rng, IList<byte> list)
			{
				List<int> outputs = [];

				for (int i = 0; i < 1000; i++)
				{
					int op = rng.Next(0, 3);

					if (op == 0)
					{
						int nToWrite = rng.Next(0, 100);
						var bytes = new byte[nToWrite];
						rng.NextBytes(bytes);
						foreach (var b in bytes)
						{
							list.Add(b);
						}
					}
					else if (op == 1)
					{
						if (list.Count == 0)
						{
							outputs.Add(list.Count);
						}
						else
						{
							int nToRead = rng.Next(0, 100);

							for (int j = 0; j < nToRead; j++)
							{
								int idxToRead = rng.Next(0, list.Count);
								outputs.Add(list[idxToRead]);
							}
						}
					}
					else
					{
						outputs.Add(list.Count);
					}
				}

				return outputs;
			}
		}
	}
}