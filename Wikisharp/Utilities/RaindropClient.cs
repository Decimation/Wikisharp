using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Authenticators;

#nullable enable
namespace Wikisharp.Utilities
{
	public sealed class RaindropClient
	{
		private const string RAINDROP_API_ENDPOINT = "https://api.raindrop.io/rest/v1/";


		private const int COL_ID_UNSORTED   = -1;
		private const int COL_ID_TRASH      = -99;
		private const int COL_ID_SELECT_ALL = 0;

		private const int LINKS_ADD_LIM = 100;


		private readonly RestClient m_client;

		public RaindropClient(string token)
		{
			m_client = new RestClient(RAINDROP_API_ENDPOINT);
			//rc.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(tk);
			m_client.AddDefaultHeader("Authorization", string.Format("Bearer {0}", token));
		}

		public RaindropCollection[] GetCollections()
		{
			var req = new RestRequest("collections", Method.GET)
			{
				RequestFormat = DataFormat.Json
			};


			var res = m_client.Execute(req);
			Common.Assert(res);
			var rgx = JObject.Parse(res.Content);
			var rg  = rgx["items"]!.ToObject<RaindropCollection[]>();


			return rg;
		}

		/// <remarks>https://developer.raindrop.io/v1/raindrops/multiple#create-many-raindrops</remarks>
		public void AddLinks(RaindropItem[] rgLinks)
		{
			if (rgLinks.Length > LINKS_ADD_LIM) {
				Console.WriteLine("Link capacity exceeded ({0}); splitting", rgLinks.Length);
				
				var split = Common.SplitList(rgLinks.ToList(), LINKS_ADD_LIM);

				foreach (List<RaindropItem> splitList in split) {
					AddLinks(splitList.ToArray());
				}

				return;
			}

			var req = new RestRequest("raindrops", Method.POST) {RequestFormat = DataFormat.Json};


			var jarray = JObject.FromObject(new {items = rgLinks});


			var serialized = jarray.ToString();

			//Console.WriteLine(serialized);
			req.AddJsonBody(serialized);

			var res = m_client.Execute(req);

			Common.Assert(res);
		}

		/// <summary>
		/// <remarks>https://developer.raindrop.io/v1/raindrops/single</remarks>
		/// </summary>
		[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
		public sealed class RaindropItem
		{
			public long?     collectionId;
			public string    link;
			public string[]? tags;
			public string?   title;
		}

		[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
		public sealed class RaindropCollection
		{
			public int     _id;
			public string? title;
		}
	}
}