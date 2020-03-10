using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Wikisharp;

namespace Test
{
	public static class Program
	{
		private static void Main(string[] args)
		{
			var ws = new WikiSession("cadd84b3ec598ae5b46c3be4e4a5aa03", "a47da1c7a546ab335a656f9f4f2a231a",
			                         "001Decimation", "7eogj13jr0t54gbndkvfnlls3dbcp06o", "4511278");
			
			var wc  = new WikiClient(ws);
			var lists = wc.GetListsAggregate();
			
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
		}
	}
}