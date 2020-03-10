using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using RestSharp;
using Wikisharp;
using Wikisharp.WikiObjects;

namespace Test
{
	public static class Program
	{
		private static void Main(string[] args)
		{
			var ws = new WikiSession("cadd84b3ec598ae5b46c3be4e4a5aa03", "a47da1c7a546ab335a656f9f4f2a231a",
			                         "001Decimation", "7eogj13jr0t54gbndkvfnlls3dbcp06o", "4511278");
			
			var wc  = new WikiClient(ws);
			var lists = wc.GetLists();
			
			foreach (var list in lists) {
				var listObj = JObject.Parse(list.Content)["query"]["readinglists"].ToObject<List<ReadingList>>();

				foreach (var readingList in listObj) {
					Console.WriteLine(readingList.Name);
				}

				//var entries = wc.GetList(list.Id).Data.ReadingListEntries;

				//foreach (var entry in entries) {
				//	Console.WriteLine("\t>{0}", entry.Title);
				//}
			}
			wc.Export(@"C:\Users\Deci\Desktop\wiki");

			string j =
				@"{""batchcomplete"":"""",""continue"":{""rlcontinue"":""Language|1677383"",""continue"":""-||""},""query"":{""readinglists"":[{""id"":1960560,""name"":""Biology"",""description"":"""",""created"":""2020-01-23T02:25:00Z"",""updated"":""2020-01-23T02:25:00Z""},{""id"":1249918,""name"":""Computer Science"",""description"":"""",""created"":""2019-06-01T07:34:51Z"",""updated"":""2019-11-03T07:38:35Z""},{""id"":1856893,""name"":""Cultural Marxism"",""description"":"""",""created"":""2019-12-31T11:53:49Z"",""updated"":""2019-12-31T11:53:49Z""},{""id"":1249916,""name"":""Dislike"",""description"":"""",""created"":""2019-06-01T07:34:51Z"",""updated"":""2019-10-28T05:33:22Z""},{""id"":1677417,""name"":""Drugs"",""description"":"""",""created"":""2019-11-03T07:46:45Z"",""updated"":""2019-11-03T07:46:45Z""},{""id"":1947944,""name"":""Esotericism"",""description"":""Paranormal, esotericism, /x/"",""created"":""2020-01-20T07:43:06Z"",""updated"":""2020-01-20T07:57:36Z""},{""id"":1249921,""name"":""Games"",""description"":"""",""created"":""2019-06-01T07:34:52Z"",""updated"":""2019-11-03T07:43:19Z""},{""id"":2035698,""name"":""Genocide"",""description"":"""",""created"":""2020-02-11T23:17:54Z"",""updated"":""2020-02-11T23:17:54Z""},{""id"":1555965,""name"":""JQ"",""description"":"""",""created"":""2019-09-23T20:00:41Z"",""updated"":""2020-01-20T07:45:42Z""},{""id"":1249917,""name"":""Japan"",""description"":"""",""created"":""2019-06-01T07:34:51Z"",""updated"":""2019-11-03T07:29:57Z""}],""readinglists-synctimestamp"":""2020-03-10T01:16:11Z""}}";

			var o = JObject.Parse(j);

			var s = Wikia.TryGetToken(o, out var jt, "continue", "rlcontinue");
			Console.WriteLine(s);
			Console.WriteLine(jt);
		}
	}
}