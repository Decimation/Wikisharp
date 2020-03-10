using JetBrains.Annotations;

namespace Wikisharp.WikiObjects
{
	[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
	public sealed class WikiUser
	{
		public int UserId { get; set; }

		public string Name { get; set; }

		public int EditCount { get; set; }

		public string[] Groups { get; set; }

		public override string ToString()
		{
			return string.Format("{0} ({1})", Name, UserId);
		}
	}
}