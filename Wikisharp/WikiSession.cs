using System.Net;

namespace Wikisharp
{
	public sealed class WikiSession
	{
		private const string PATH = "/";

		public WikiSession(string caSession, string caToken, string user, string mwSession, string mwUserId)
		{
			var cookies = new Cookie[6]
			{
				CreateCentralAuthCookie("centralauth_Session", caSession),
				CreateCentralAuthCookie("centralauth_Token", caToken),
				CreateCentralAuthCookie("centralauth_User", user),
				
				CreateMediaWikiCookie("mediawikiwikiSession", mwSession),
				CreateMediaWikiCookie("mediawikiwikiUserID", mwUserId),
				CreateMediaWikiCookie("mediawikiwikiUserName", user),
			};

			Cookies = cookies;
		}
		
		internal Cookie[] Cookies { get; }

		private Cookie CreateMediaWikiCookie(string name, string value)
		{
			const string DOMAIN = "www.mediawiki.org";
			return new Cookie(name, value, PATH, DOMAIN);
		}
		
		private Cookie CreateCentralAuthCookie(string name, string value)
		{
			const string DOMAIN = ".mediawiki.org";
			return new Cookie(name, value, PATH, DOMAIN);
		}
	}
}