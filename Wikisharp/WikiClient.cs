using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Authenticators;
using Wikisharp.WikiObjects;

namespace Wikisharp
{
	public class WikiClient
	{
		private readonly RestClient m_client;

		public WikiClient(WikiSession ws)
		{
			m_client = new RestClient("https://www.mediawiki.org/w/api.php")
			{
				CookieContainer = new CookieContainer()
			};

			foreach (var cookie in ws.Cookies) {
				m_client.CookieContainer.Add(cookie);
			}
		}


		public void Export(string dir)
		{
			var di = Directory.CreateDirectory(dir);

			var listsResponse = GetLists();
			var listsData     = new List<ReadingList>();
			var listSb        = new StringBuilder();

			foreach (var response in listsResponse) {
				var str = response.Content;
				listSb.AppendLine(str);

				var buf = JObject.Parse(str)[Assets.QUERY][Assets.READINGLISTS].ToObject<List<ReadingList>>();
				foreach (var list in buf) {
					Console.WriteLine(">> {0}", list.Name);
				}

				listsData.AddRange(buf);
			}

			File.WriteAllText(Path.Combine(di.FullName, "lists.json"), listSb.ToString());

			var lists = listsData;

			foreach (var list in lists) {
				var listResponse = GetList(list.Id);

				var sb      = new StringBuilder();
				var entries = new List<ReadingListEntry>();

				foreach (var response in listResponse) {
					var str  = response.Content;
					var data = JObject.Parse(str);

					entries.AddRange(data[Assets.QUERY]["readinglistentries"].ToObject<List<ReadingListEntry>>());

					sb.AppendLine(str);
				}

				Console.WriteLine("{0}: Entries: {1}", list.Name, entries.Count);
				File.WriteAllText(Path.Combine(di.FullName, list.Name + ".json"), sb.ToString());
			}
		}

		public List<IRestResponse> GetList(int id)
		{
			var list            = new List<IRestResponse>();
			var listResponse    = GetListSegment(id);
			var listResponseObj = JObject.Parse(listResponse.Content);

			if (!Wikia.TryGetContinueToken(listResponseObj, out var rleContinueTk, Assets.CONTINUE,
			                               Assets.RLECONTINUE)) {
				list.Add(listResponse);
				return list;
			}

			string rleContinue = rleContinueTk.ToString();

			list.Add(listResponse);

			while (rleContinue != null) {
				listResponse = GetListSegment(id, rleContinue);
				list.Add(listResponse);
				listResponseObj = JObject.Parse(listResponse.Content);

				if (!Wikia.TryGetContinueToken(listResponseObj, out rleContinueTk, Assets.CONTINUE,
				                               Assets.RLECONTINUE)) {
					break;
				}

				rleContinue = rleContinueTk.ToString();
			}


			return list;
		}

		private IRestResponse GetListSegment(int id, string rleContinue = null)
		{
			// https://www.mediawiki.org/w/api.php?action=query&list=readinglistentries&rlelists=id

			var req = Common.Create(Assets.QUERY);
			req.AddQueryParameter("list", "readinglistentries");
			req.AddQueryParameter("rlelists", id.ToString());

			if (rleContinue != null) {
				req.AddQueryParameter(Assets.RLECONTINUE, rleContinue);
			}

			req.AddQueryParameter("format", "json");
			//req.RootElement = WikiaAssets.QUERY;


			var res = m_client.Execute(req, Method.GET);

			return res;
		}

		public List<IRestResponse> GetLists()
		{
			var list            = new List<IRestResponse>();
			var listResponse    = GetListsSegment();
			var listResponseObj = JObject.Parse(listResponse.Content);

			if (!Wikia.TryGetContinueToken(listResponseObj, out var rlContinueTk, Assets.CONTINUE, "rlcontinue")) {
				list.Add(listResponse);
				return list;
			}

			string rlContinue = rlContinueTk.ToString();

			list.Add(listResponse);

			while (rlContinue != null) {
				listResponse = GetListsSegment(rlContinue);
				list.Add(listResponse);
				listResponseObj = JObject.Parse(listResponse.Content);


				if (!Wikia.TryGetContinueToken(listResponseObj, out rlContinueTk, Assets.CONTINUE,
				                               Assets.RLCONTINUE)) {
					break;
				}

				rlContinue = rlContinueTk.ToString();
			}

			return list;
		}

		private IRestResponse GetListsSegment(string rlContinue = null)
		{
			// https://www.mediawiki.org/w/api.php?action=query&meta=readinglists

			var req = Common.Create(Assets.QUERY);
			req.AddQueryParameter("meta", Assets.READINGLISTS);

			if (rlContinue != null) {
				req.AddQueryParameter(Assets.RLCONTINUE, rlContinue);
			}

			req.AddQueryParameter("format", "json");
			req.RootElement = Assets.QUERY;

			var res = m_client.Execute(req, Method.GET);

			return res;
		}
	}
}