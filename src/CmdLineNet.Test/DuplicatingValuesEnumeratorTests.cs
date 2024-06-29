namespace CmdLineNet.Test
{
	using System.Collections;
	using System.Collections.Generic;
	using Xunit;

	public static class DuplicatingValuesEnumeratorTests
	{
		private static void AssertNext(DuplicatingValuesEnumerator<int> iter, int id)
		{
			Assert.True(iter.MoveNext());
			Assert.Equal(iter.Current, id);
			Assert.Equal(((IEnumerator)iter).Current, id);
		}
		[Fact]
		public static void Enumerating()
		{
			List<IdCount<int>> list = [new IdCount<int>(1, 3), new IdCount<int>(2, 2), new IdCount<int>(3, 1)];
			using DuplicatingValuesEnumerator<int> iter = new(list.GetEnumerator());

			for (int i = 0; i < 3; i++)
			{
				AssertNext(iter, 1);
				AssertNext(iter, 1);
				AssertNext(iter, 1);
				AssertNext(iter, 2);
				AssertNext(iter, 2);
				AssertNext(iter, 3);
				iter.Reset();
			}
		}
	}
}
