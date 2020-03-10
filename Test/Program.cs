﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
			var ws = new WikiSession("001Decimation",
			                         "cadd84b3ec598ae5b46c3be4e4a5aa03",
			                         "a47da1c7a546ab335a656f9f4f2a231a",
			                         "7eogj13jr0t54gbndkvfnlls3dbcp06o");

			var wc = new WikiClient(ws);


			Lists = wc.GetAllLists(@"C:\Users\Deci\Desktop\wiki");
			
			CheckCount("default",6);
			CheckCount("Biology", 14);
			CheckCount("Computer Science", 47);
			CheckCount("Games", 2);
			CheckCount("Language", 13);
		}

		private static void CheckCount(string name, int cnt)
		{
			var list=Lists.First(w => w.List.Name == name);
			Debug.Assert(list.Entries.Length == cnt);
		}
	}
}