using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
			File.WriteAllText(Path.Combine(di.FullName,"lists.json"),listsResponse.Content);

			var lists = listsResponse.Data.ReadingLists;

			foreach (var list in lists) {
				var listResponse = AggregateGetList(list.Id);
				var listCount = listResponse.Sum(c => c.Data.ReadingListEntries.Count);
				var listContentAggregate = listResponse.Select(l => l.Content);
				
				Console.WriteLine("> {0} | N: {1}", list.Name, listCount);
				
				File.WriteAllLines(Path.Combine(di.FullName,list.Name + ".json"),listContentAggregate);
			}
		}


		private static IRestRequest Create(string action)
		{
			var req = new RestRequest();
			req.AddQueryParameter("action", action);
			return req;
		}

		class WikiList
		{
			
		}

		public List<IRestResponse<ReadingListEntriesQuery>> AggregateGetList(int id)
		{
			var list = new List<IRestResponse<ReadingListEntriesQuery>>();
			
			var listResponse = GetList(id);
			string rleContinue = listResponse.Data.Continue.RleContinue;
			
			list.Add(listResponse);

			while (rleContinue!=null) {
				listResponse = GetList(id, rleContinue);
				var data = listResponse.Data;
				
				rleContinue = data.Continue.RleContinue;
				list.Add(listResponse);
			}

			return list;
		}

		public IRestResponse<ReadingListEntriesQuery> GetList(int id, string rleContinue = null)
		{
			// https://www.mediawiki.org/w/api.php?action=query&list=readinglistentries&rlelists=id
			
			var req = Create("query");
			req.AddQueryParameter("list", "readinglistentries");
			req.AddQueryParameter("rlelists", id.ToString());

			if (rleContinue!=null) {
				req.AddQueryParameter("rlecontinue", rleContinue);
			}
			
			req.AddQueryParameter("format", "json");
			//req.RootElement = "query";
			
			
			
			var res = m_client.Execute<dynamic>(req, Method.GET);
			
			var rleq = new ReadingListEntriesQuery();
			rleq.Continue = (ListContinue) res.Data.Continue;
			rleq.ReadingListEntries = (List<ReadingListEntry>) res.Data.ReadingListEntries;
			
			return rleq;
		}


		public IRestResponse<ReadingListsQuery> GetLists()
		{
			// https://www.mediawiki.org/w/api.php?action=query&meta=readinglists
			
			var req = Create("query");
			req.AddQueryParameter("meta", "readinglists");
			req.AddQueryParameter("format", "json");
			req.RootElement = "query";
			
			var res = m_client.Execute<ReadingListsQuery>(req, Method.GET);

			return res;
		}
	}
}