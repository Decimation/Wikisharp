﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Authenticators;
using Wikisharp.Abstraction;
using Wikisharp.Utilities;
using Wikisharp.WikiObjects;

namespace Wikisharp
{
	public class WikiClient
	{
		private readonly RestClient m_client;

		private readonly RestClient m_wikiClient;

		private const string BASE_URL = "https://www.mediawiki.org/w/api.php";
		private const string BASE2_URL = "https://en.wikipedia.org/w/rest.php/v1/";
		
		public WikiClient(WikiSession ws)
		{
			m_client = new RestClient(BASE_URL)
			{
				CookieContainer = new CookieContainer()
			};

			foreach (var cookie in ws.Cookies) {
				m_client.CookieContainer.Add(cookie);
			}
			
			m_wikiClient = new RestClient(BASE2_URL);
		}

		public static WikiUser GetUserQuick(string name)
		{
			var req = String.Format("{0}?action=query&format=json&list=users&ususers={1}", BASE_URL, name);
			var s   = Common.GetString(req);

			var data = Common.QueryParse<List<WikiUser>>(s, Assets.USERS).FirstOrDefault();

			return data;
		}

		public WikiUser GetUser(string name, [CanBeNull] string[] properties)
		{
			// https://www.mediawiki.org/w/api.php?action=query&list=users&ususers=Example&usprop=groups%7Ceditcount%7Cgender

			var req = Common.Create("query");
			req.AddQueryParameter("list", "users");
			req.AddQueryParameter("ususers", name);

			if (properties != null) {
				req.AddQueryParameter("usprop", String.Join("|", properties));
			}


			var res  = m_client.Execute(req);
			Common.Assert(res);
			var data = Common.QueryParse<List<WikiUser>>(res.Content, Assets.USERS);

			return data.FirstOrDefault();
		}

		public ReadingList GetList(string name)
		{
			var lists = GetAllLists();
			return lists.FirstOrDefault(l => l.List.Name == name);
		}

		public List<ReadingList> GetAllLists([CanBeNull] string dir = null)
		{
			bool export = dir != null;

			DirectoryInfo di = null;

			if (export) {
				di = Directory.CreateDirectory(dir);
			}


			var listsResponse = GetLists();
			var lists         = new List<WikiReadingList>();
			var listsJson     = new List<string>();
			var wlist         = new List<ReadingList>();

			foreach (string str in listsResponse.Select(response => response.Content)) {
				listsJson.Add(str);
				lists.AddRange(Common.QueryParse<List<WikiReadingList>>(str, Assets.READINGLISTS));
			}

			if (export) {
				WriteJson("lists", listsJson);
			}


			foreach (var list in lists) {
				var listResponse = GetList(list.Id);

				var entriesJson = new List<string>();
				var entries     = new List<WikiReadingListEntry>();

				foreach (string str in listResponse.Select(response => response.Content)) {
					entries.AddRange(Common.QueryParse<List<WikiReadingListEntry>>(str, Assets.READINGLISTENTRIES));
					entriesJson.Add(str);
				}

				

				if (export) {
					WriteJson(list.Name, entriesJson);
				}

				wlist.Add(new ReadingList(list, entries.ToArray()));
			}

			void WriteJson(string fname, IEnumerable<string> js)
			{
				
				Debug.Assert(di != null);
				File.WriteAllLines(Path.Combine(di.FullName, fname + ".json"), js);
			}

			return wlist;
		}

		public WikiPage GetPage(string q)
		{
			// https://en.wikipedia.org/w/rest.php/v1/search/page?q=jupiter&limit=1

			const int lim = 1;
			//var qencode = Uri.EscapeDataString(q);
			var qencode=HttpUtility.UrlEncode(q);
			
			var r  = new RestRequest("search/page");
			r.AddQueryParameter("q", qencode);
			r.AddQueryParameter("limit", lim.ToString());

			//Console.WriteLine(m_wikiClient.BuildUri(r));
			var re = m_wikiClient.Execute(r);
			Common.Assert(re);
			//Console.WriteLine(re.Content);

			var jo = JObject.Parse(re.Content);
			var js = jo["pages"].ToObject<WikiPage[]>();

			object? val = null;

			if (js.Length>0) {
				val = js[0];
			}
			

			if (val!=null) {
				return (WikiPage) val;
			}

			return null;
		}

		public List<IRestResponse> GetList(int id)
		{
			var list            = new List<IRestResponse>();
			var listResponse    = GetListSegment(id);
			var listResponseObj = JObject.Parse(listResponse.Content);

			if (!Common.TryGetContinueToken(listResponseObj, out var rleContinueTk,
			                                Assets.CONTINUE, Assets.RLECONTINUE)) {
				list.Add(listResponse);
				return list;
			}

			string rleContinue = rleContinueTk.ToString();

			list.Add(listResponse);

			while (rleContinue != null) {
				listResponse = GetListSegment(id, rleContinue);
				list.Add(listResponse);
				listResponseObj = JObject.Parse(listResponse.Content);

				if (!Common.TryGetContinueToken(listResponseObj, out rleContinueTk,
				                                Assets.CONTINUE, Assets.RLECONTINUE)) {
					break;
				}

				rleContinue = rleContinueTk.ToString();
			}


			return list;
		}

		private const int RLELIMIT = 100;
		
		private IRestResponse GetListSegment(int id, string rleContinue = null)
		{
			// https://www.mediawiki.org/w/api.php?action=query&list=readinglistentries&rlelists=id&rlelimit=int

			var req = Common.Create(Assets.QUERY);
			req.AddQueryParameter("list", Assets.READINGLISTENTRIES);
			req.AddQueryParameter("rlelists", id.ToString());
			req.AddQueryParameter("rlelimit", RLELIMIT.ToString());
			
			
			
			if (rleContinue != null) {
				req.AddQueryParameter(Assets.RLECONTINUE, rleContinue);
			}


			//req.RootElement = Assets.QUERY;


			var res = m_client.Execute(req, Method.GET);
			
			Common.Assert(res);

			return res;
		}

		public List<IRestResponse> GetLists()
		{
			var list            = new List<IRestResponse>();
			var listResponse    = GetListsSegment();
			var listResponseObj = JObject.Parse(listResponse.Content);

			if (!Common.TryGetContinueToken(listResponseObj, out var rlContinueTk,
			                                Assets.CONTINUE, Assets.RLCONTINUE)) {
				list.Add(listResponse);
				return list;
			}

			string rlContinue = rlContinueTk.ToString();

			list.Add(listResponse);

			while (rlContinue != null) {
				listResponse = GetListsSegment(rlContinue);
				list.Add(listResponse);
				listResponseObj = JObject.Parse(listResponse.Content);


				if (!Common.TryGetContinueToken(listResponseObj, out rlContinueTk,
				                                Assets.CONTINUE, Assets.RLCONTINUE)) {
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

			req.RootElement = Assets.QUERY;

			var res = m_client.Execute(req, Method.GET);
			Common.Assert(res);

			return res;
		}

		public static void WriteHtmlList(ReadingList list, WikiClient wc, string d)
		{
			var sb = new List<string>
			{
				list.List.Name, 
				"<ul>"
			};
			
			//sb.Add(Environment.NewLine);

			foreach (var entry in list.Entries) {
				const string s = "https://en.wikipedia.org/wiki/";


				var page = wc.GetPage(entry.Title);
				if (page != null) {
					var nt   = String.Format("{0} - Wikipedia", page.Title);
					var link = s + page.Key;
					var sz   = String.Format("<li> <a href=\"{0}\">{1}</a> </li>", link, nt);
					sb.Add(sz);
					//sb.Add(Environment.NewLine);
				}
				else {
					Console.WriteLine("0 results for {0}", entry.Title);
				}
			}

			sb.Add("</ul>");

			File.WriteAllLines(d, sb);
		}
	}
}