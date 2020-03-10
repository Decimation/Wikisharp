using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Authenticators;

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
			File.WriteAllText(Path.Combine(di.FullName, "lists.json"), listsResponse.Content);

			var lists = listsResponse.Data.ReadingLists;

			foreach (var list in lists) {
				var listResponse         = AggregateGetList(list.Id);
				var listContentAggregate = listResponse.Select(l => JObject.Parse(l.Content)).ToArray();
				var listEntries = listContentAggregate.Select(s => s["query"]["readinglistentries"].ToObject<List<ReadingListEntry>>()).ToArray();
				var listAggregate = new List<ReadingListEntry>();
				foreach (List<ReadingListEntry> listEntry in listEntries) {
					listAggregate.AddRange(listEntry);
				}
				

				Console.WriteLine("> {0}", list.Name);

				File.WriteAllLines(Path.Combine(di.FullName, list.Name + ".json"), listContentAggregate.tos);
			}
		}


		public List<IRestResponse<ReadingListEntriesQuery>> AggregateGetListOld(int id)
		{
			var list = new List<IRestResponse<ReadingListEntriesQuery>>();

			var    listResponse = GetListOld(id);
			string rleContinue  = listResponse.Data.Continue.RleContinue;

			list.Add(listResponse);

			while (rleContinue != null) {
				listResponse = GetListOld(id, rleContinue);
				var data = listResponse.Data;

				rleContinue = data.Continue.RleContinue;
				list.Add(listResponse);
			}

			return list;
		}

		public List<IRestResponse> AggregateGetList(int id)
		{
			var list = new List<IRestResponse>();

			var    listResponse = GetList(id);

			var listResponseObj = JObject.Parse(listResponse.Content);

			if (!listResponseObj.ContainsKey("continue")) {
				return list;
			}
			
			string rleContinue = listResponseObj["continue"]["rlecontinue"].ToString();
			Console.WriteLine("rlecontinue: {0}", rleContinue);

			list.Add(listResponse);

			while (rleContinue != null) {
				listResponse = GetList(id, rleContinue);
				list.Add(listResponse);
				listResponseObj = JObject.Parse(listResponse.Content);

				if (!listResponseObj.ContainsKey("continue")) {
					break;
				}
				rleContinue = listResponseObj["continue"]["rlecontinue"].ToString();
				
				Console.WriteLine("> rlecontinue: {0}", rleContinue);
				
			}
			
			

			return list;
		}

		public IRestResponse GetList(int id, string rleContinue = null)
		{
			// https://www.mediawiki.org/w/api.php?action=query&list=readinglistentries&rlelists=id

			var req = Common.Create("query");
			req.AddQueryParameter("list", "readinglistentries");
			req.AddQueryParameter("rlelists", id.ToString());

			if (rleContinue != null) {
				req.AddQueryParameter("rlecontinue", rleContinue);
			}

			req.AddQueryParameter("format", "json");
			//req.RootElement = "query";


			var res = m_client.Execute(req, Method.GET);

			return res;
		}

		public IRestResponse<ReadingListEntriesQuery> GetListOld(int id, string rleContinue = null)
		{
			// https://www.mediawiki.org/w/api.php?action=query&list=readinglistentries&rlelists=id

			var req = Common.Create("query");
			req.AddQueryParameter("list", "readinglistentries");
			req.AddQueryParameter("rlelists", id.ToString());

			if (rleContinue != null) {
				req.AddQueryParameter("rlecontinue", rleContinue);
			}

			req.AddQueryParameter("format", "json");
			//req.RootElement = "query";


			var res = m_client.Execute<dynamic>(req, Method.GET);

			var rleq = new ReadingListEntriesQuery();
			rleq.Continue           = (ListContinue) res.Data.Continue;
			rleq.ReadingListEntries = (List<ReadingListEntry>) res.Data.ReadingListEntries;

			return null;
		}


		public IRestResponse<ReadingListsQuery> GetLists()
		{
			// https://www.mediawiki.org/w/api.php?action=query&meta=readinglists

			var req = Common.Create("query");
			req.AddQueryParameter("meta", "readinglists");
			req.AddQueryParameter("format", "json");
			req.RootElement = "query";

			var res = m_client.Execute<ReadingListsQuery>(req, Method.GET);

			return res;
		}
	}
}