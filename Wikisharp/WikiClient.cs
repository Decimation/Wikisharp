﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
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

		private const string BASE_URL = "https://www.mediawiki.org/w/api.php";

		public WikiClient(WikiSession ws)
		{
			m_client = new RestClient(BASE_URL)
			{
				CookieContainer = new CookieContainer()
			};

			foreach (var cookie in ws.Cookies) {
				m_client.CookieContainer.Add(cookie);
			}
		}

		public static WikiUser GetUserQuick(string name)
		{
			var req = string.Format("{0}?action=query&format=json&list=users&ususers={1}", BASE_URL, name);
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
				req.AddQueryParameter("usprop", string.Join("|", properties));
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

		private IRestResponse GetListSegment(int id, string rleContinue = null)
		{
			// https://www.mediawiki.org/w/api.php?action=query&list=readinglistentries&rlelists=id

			var req = Common.Create(Assets.QUERY);
			req.AddQueryParameter("list", Assets.READINGLISTENTRIES);
			req.AddQueryParameter("rlelists", id.ToString());

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
	}
}