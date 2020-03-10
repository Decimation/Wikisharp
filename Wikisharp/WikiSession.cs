using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Wikisharp.Utilities;
using Wikisharp.WikiObjects;

namespace Wikisharp
{
	public sealed class WikiSession
	{
		private const string PATH = "/";

		public WikiSession(string user, string caSession, string caToken, string mwSession)
		{
			var data = WikiClient.GetUserQuick(user);
			int mwUserId = data.UserId;
			
			Cookies = new[]
			{
				CreateCentralAuthCookie("centralauth_Session", caSession),
				CreateCentralAuthCookie("centralauth_Token", caToken),
				CreateCentralAuthCookie("centralauth_User", user),

				CreateMediaWikiCookie("mediawikiwikiSession", mwSession),
				CreateMediaWikiCookie("mediawikiwikiUserID", mwUserId.ToString()),
				CreateMediaWikiCookie("mediawikiwikiUserName", user),
			};
		}

		
		public Cookie[] Cookies { get; }

		private static Cookie CreateMediaWikiCookie(string name, string value)
		{
			const string DOMAIN = "www.mediawiki.org";
			return new Cookie(name, value, PATH, DOMAIN);
		}

		private static Cookie CreateCentralAuthCookie(string name, string value)
		{
			const string DOMAIN = ".mediawiki.org";
			return new Cookie(name, value, PATH, DOMAIN);
		}
	}
}