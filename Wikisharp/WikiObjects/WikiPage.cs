using System.Text;
using JetBrains.Annotations;

namespace Wikisharp.WikiObjects
{
	[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
	public sealed class WikiPage
	{
		public int    Id          { get; set; }
		public string Key         { get; set; }
		public string Title       { get; set; }
		public string Excerpt     { get; set; }
		public string Description { get; set; }

		public WikiThumbnail Thumbnail { get; set; }

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.AppendFormat("Id {0}\n", Id);
			sb.AppendFormat("Title {0}\n", Title);
			sb.AppendFormat("Desc {0}\n", Description);
			return sb.ToString();
		}
	}

	[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
	public sealed class WikiThumbnail
	{
		public string MimeType { get; set; }
		public int?   Size     { get; set; }
		public int    Width    { get; set; }
		public int    Height   { get; set; }

		[CanBeNull]
		public string Duration { get; set; }

		public string Url { get; set; }
	}
}