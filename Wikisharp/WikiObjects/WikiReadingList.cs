using System.Text;
using JetBrains.Annotations;

namespace Wikisharp.WikiObjects
{
	[UsedImplicitly]
	public sealed class WikiReadingList
	{
		public int    Id          { get; set; }
		public string Name        { get; set; }
		public string Description { get; set; }
		public string Created     { get; set; }
		public string Updated     { get; set; }

		public override string ToString()
		{
			return string.Format("{0} ({1})", Name, Id);
		}
	}
}