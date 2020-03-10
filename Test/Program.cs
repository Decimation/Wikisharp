﻿using System;
using System.Collections.Generic;
using System.Linq;
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
			
			var ws = new WikiSession("cadd84b3ec598ae5b46c3be4e4a5aa03", 
			                         "a47da1c7a546ab335a656f9f4f2a231a",
			                         "001Decimation", 
			                         "7eogj13jr0t54gbndkvfnlls3dbcp06o", 
			                         "4511278");

			var wc = new WikiClient(ws);

			var user = wc.GetUser("001Decimation",new[]{"editcount"});
			Console.WriteLine(user.Name);
			Console.WriteLine(user.UserId);


			var wlists = wc.Export(@"C:\Users\Deci\Desktop\wiki");
			foreach (var list in wlists) {
				Console.WriteLine(list);
			}

			var jq = wlists.First(l => l.List.Name == "JQ");
			foreach (var q in jq.Entries) {
				Console.WriteLine(q);
			}
		}
	}
}