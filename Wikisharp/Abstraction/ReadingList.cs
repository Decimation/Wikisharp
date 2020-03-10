using Wikisharp.WikiObjects;

namespace Wikisharp.Abstraction
{
	/// <summary>
	/// Wraps a <see cref="WikiReadingList"/> and an array of <see cref="WikiReadingListEntry"/>
	/// </summary>
	public sealed class ReadingList
	{
		// https://www.mediawiki.org/wiki/Extension:ReadingLists#API
		
		public WikiReadingList List { get; }

		public WikiReadingListEntry[] Entries { get; }

		public ReadingList(WikiReadingList list, WikiReadingListEntry[] entries)
		{
			List = list;
			Entries     = entries;
		}

		public override string ToString()
		{
			return string.Format("Name: {0} | Entries: {1}", List.Name, Entries.Length);
		}
	}
}