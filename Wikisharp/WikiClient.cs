﻿using System;
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
			var listsData = new List<ReadingList>();
			var listSb = new StringBuilder();
			
			foreach (var response in listsResponse) {
				var str = response.Content;
				listSb.AppendLine(str);

				var buf = JObject.Parse(str)["query"]["readinglists"].ToObject<List<ReadingList>>();
				foreach (var list in buf) {
					Console.WriteLine(">> {0}",list.Name);
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

					entries.AddRange(data["query"]["readinglistentries"].ToObject<List<ReadingListEntry>>());

					sb.AppendLine(str);
				}

				Console.WriteLine("{0}: Entries: {1}", list.Name, entries.Count);
				File.WriteAllText(Path.Combine(di.FullName, list.Name+".json"), sb.ToString());
			}
		}

		public List<IRestResponse> GetList(int id)
		{
			var list = new List<IRestResponse>();

			var listResponse = GetListSegment(id);

			var listResponseObj = JObject.Parse(listResponse.Content);

			if (!listResponseObj.ContainsKey("continue")) {
				list.Add(listResponse);
				return list;
			}

			string rleContinue = listResponseObj["continue"]["rlecontinue"].ToString();
			//Console.WriteLine("rlecontinue: {0}", rleContinue);

			list.Add(listResponse);

			while (rleContinue != null) {
				listResponse = GetListSegment(id, rleContinue);
				list.Add(listResponse);
				listResponseObj = JObject.Parse(listResponse.Content);

				if (!listResponseObj.ContainsKey("continue")) {
					break;
				}

				rleContinue = listResponseObj["continue"]["rlecontinue"].ToString();

				//Console.WriteLine("> rlecontinue: {0}", rleContinue);
			}


			return list;
		}

		private IRestResponse GetListSegment(int id, string rleContinue = null)
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

		public List<IRestResponse> GetLists()
		{
			var list = new List<IRestResponse>();
			var listResponse = GetListsSegment();
			var listResponseObj = JObject.Parse(listResponse.Content);

			if (!listResponseObj.ContainsKey("continue")) {
				list.Add(listResponse);
				return list;
			}

			string rlContinue = listResponseObj["continue"]["rlcontinue"].ToString();
			//Console.WriteLine("rlecontinue: {0}", rleContinue);

			list.Add(listResponse);

			while (rlContinue != null) {
				listResponse = GetListsSegment(rlContinue);
				list.Add(listResponse);
				
				listResponseObj = JObject.Parse(listResponse.Content);

				if (!listResponseObj.ContainsKey("continue")) {
					break;
				}

				rlContinue = listResponseObj["continue"]["rlcontinue"].ToString();

				//Console.WriteLine("> rlecontinue: {0}", rleContinue);
			}


			return list;
		}

		private IRestResponse GetListsSegment(string rlContinue = null)
		{
			// https://www.mediawiki.org/w/api.php?action=query&meta=readinglists

			var req = Common.Create("query");
			req.AddQueryParameter("meta", "readinglists");

			if (rlContinue != null) {
				req.AddQueryParameter("rlcontinue", rlContinue);
			}

			req.AddQueryParameter("format", "json");
			req.RootElement = "query";

			var res = m_client.Execute(req, Method.GET);

			return res;
		}
	}
}