using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using RestSharp;
using Wikisharp;
using Wikisharp.Abstraction;
using Wikisharp.Utilities;
using Wikisharp.WikiObjects;

namespace Test
{
	public static class Program
	{
		private static List<ReadingList> Lists;

		private static void Main(string[] args)
		{
			string caSession = "374525a03bc943a193739143125be0df";
			string caToken   = "a47da1c7a546ab335a656f9f4f2a231a";

			var ws = new WikiSession("001Decimation", caToken);

			var wc = new WikiClient(ws);


			var v = wc.GetPage("hello world");
			Console.WriteLine(v);


			Lists = wc.GetAllLists(@"C:\Users\Deci\Desktop\wiki");

			foreach (var list in Lists) {
				Console.WriteLine(list);
			}

			var logic = Lists.First(f => f.List.Name == "Politics");

			WikiClient.WriteHtmlList(logic, wc, @"C:\Users\Deci\Desktop\politics.html");
		}

		private static void CheckCount(string name, int cnt)
		{
			var list = Lists.First(w => w.List.Name == name);
			Debug.Assert(list.Entries.Length == cnt);
		}
	}
}