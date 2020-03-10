using System.Text;
using JetBrains.Annotations;

namespace Wikisharp.WikiObjects
{
	[UsedImplicitly]
	public sealed class ReadingListEntry
	{
		public int    Id      { get; set; }
		public int    ListId  { get; set; }
		public string Project { get; set; }
		public string Title   { get; set; }
		public string Created { get; set; }
		public string Updated { get; set; }

		public override string ToString()
		{
			return string.Format("{0} ({1})", Title, Id);
		}
	}
}