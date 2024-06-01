using System.Collections;

namespace ByteBuffer;

public partial class ByteBuffer
{
	private enum EnumeratorState
	{
		AtFrontBlock,
		InBlocks,
		AtBackBlock,
		Finished
	}

	private class Enumerator(ByteBuffer target) : IEnumerator<byte>
	{
		private EnumeratorState state = EnumeratorState.AtFrontBlock;
		private int index = -1;
		private IEnumerator<byte[]>? blocksEnumerator = null;

		public byte Current => GetCurret();

		object IEnumerator.Current => GetCurret();

		private byte GetCurret()
		{
			return state switch
			{
				EnumeratorState.AtFrontBlock => ByteBlock.GetByteAtUnchecked(target.frontBlock!, index),
				EnumeratorState.InBlocks => ByteBlock.GetByteAtUnchecked(blocksEnumerator!.Current, index),
				EnumeratorState.AtBackBlock => ByteBlock.GetByteAtUnchecked(target.backBlock, index),
				EnumeratorState.Finished => throw new InvalidOperationException(),
				_ => throw new InvalidOperationException(),
			};
		}

		public void Dispose()
		{
		}

		public bool MoveNext()
		{
			return state switch
			{
				EnumeratorState.AtFrontBlock => MoveNextFrontBlock(),
				EnumeratorState.InBlocks => MoveNextBlocks(),
				EnumeratorState.AtBackBlock => MoveNextBackBlock(),
				EnumeratorState.Finished => false,
				_ => false,
			};
		}

		private bool MoveNextFrontBlock()
		{
			if (target.frontBlock == null)
			{
				state = EnumeratorState.InBlocks;
				index = -1;
				return MoveNext();
			}

			++index;

			if (index == ByteBlock.GetDataLength(target.frontBlock))
			{
				state = EnumeratorState.InBlocks;
				index = -1;
				return MoveNext();
			}

			return true;
		}

		private bool MoveNextBlocks()
		{
			if (blocksEnumerator == null)
			{
				if (target.blocks.Count == 0)
				{
					state = EnumeratorState.AtBackBlock;
					index = -1;
					return MoveNext();
				}
				blocksEnumerator = target.blocks.GetEnumerator();
				blocksEnumerator.MoveNext();
			}

			++index;

			if (index == ByteBlock.GetDataLength(blocksEnumerator.Current))
			{
				index = -1;

				if (blocksEnumerator.MoveNext())
				{
					return MoveNext();
				}

				blocksEnumerator.Dispose();
				blocksEnumerator = null;
				state = EnumeratorState.AtBackBlock;
				return MoveNext();
			}

			return true;
		}

		private bool MoveNextBackBlock()
		{
			++index;

			if (index == ByteBlock.GetDataLength(target.backBlock))
			{
				state = EnumeratorState.Finished;
				index = -1;
				return MoveNext();
			}

			return true;
		}

		public void Reset()
		{
			blocksEnumerator?.Dispose();
			blocksEnumerator = null;
			state = EnumeratorState.AtFrontBlock;
			index = -1;
		}
	}
}
