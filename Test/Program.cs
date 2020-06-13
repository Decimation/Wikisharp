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
		private static void Main(string[] args)
		{
			const string token = "a47da1c7a546ab335a656f9f4f2a231a";

			var ws = new WikiSession("001Decimation", token);

			var wc = new WikiClient(ws);


			//var list = wc.GetList("Politics");

			var list = wc.fromjson(File.ReadAllText(@"C:\Users\Deci\Desktop\wiki2\Concepts.json"));
			
			Console.WriteLine(list);
			
			WikiClient.WriteHtmlList(list, wc, @"C:\Users\Deci\Desktop\politics3.html","concepts");
		}
	}
}