using Wikisharp.WikiObjects;

namespace Wikisharp
{
	public class WikiaList
	{
		public ReadingList        ReadingList { get; }
		public ReadingListEntry[] Entries     { get; }

		public WikiaList(ReadingList readingList, ReadingListEntry[] entries)
		{
			ReadingList = readingList;
			Entries     = entries;
		}

		public override string ToString()
		{
			return string.Format("Name: {0} | Entries: {1}", ReadingList.Name, Entries.Length);
		}
	}
}