using System.Collections.Generic;
using RestSharp.Deserializers;

namespace Wikisharp
{
	public class ReadingListEntriesQuery
	{
		public ListContinue Continue { get; set;}
		
		
		public List<ReadingListEntry> ReadingListEntries { get; set;}
	}
}