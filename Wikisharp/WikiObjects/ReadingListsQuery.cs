using System;
using System.Collections.Generic;
using System.Text;

namespace Wikisharp
{
	public class ReadingListsQuery
	{
		public List<ReadingList> ReadingLists { get; set; }
		public string SyncTimestamp { get; set; }

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.AppendFormat("N Reading lists: {0}\n", ReadingLists?.Count);
			sb.AppendFormat("Sync timestamp: {0}\n", SyncTimestamp);
			return sb.ToString();
		}
	}
}