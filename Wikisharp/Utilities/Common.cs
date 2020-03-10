using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

[assembly: InternalsVisibleTo("Test")]

namespace Wikisharp.Utilities
{
	internal static class Common
	{
		


		public static string GetString(string url)
		{
			using var wc       = new System.Net.WebClient();
			string    contents = wc.DownloadString(url);

			return contents;
		}

		public static T QueryParse<T>(string str, string key1)
		{
			// var buf = JObject.Parse(str)[Assets.QUERY][Assets.READINGLISTS].ToObject<List<ReadingList>>();
			// data[Assets.QUERY][Assets.READINGLISTENTRIES].ToObject<List<ReadingListEntry>>()


			return JObject.Parse(str)[Assets.QUERY][key1].ToObject<T>();
		}

		public static string JsonPrettify(string json)
		{
			using var stringReader = new StringReader(json);
			using var stringWriter = new StringWriter();

			var jsonReader = new JsonTextReader(stringReader);
			var jsonWriter = new JsonTextWriter(stringWriter) {Formatting = Formatting.Indented};
			jsonWriter.WriteToken(jsonReader);

			return stringWriter.ToString();
		}

		public static string JsonPrettifyAlt(string json) => JToken.Parse(json).ToString();

		internal static void Debug(IRestResponse res, bool includeContent = false, bool includeCookies = false)
		{
			var sb = new StringBuilder();
			sb.AppendFormat("Status code: {0}\n", res.StatusCode);
			sb.AppendFormat("Success: {0}\n", res.IsSuccessful);
			sb.AppendFormat("Response status: {0}\n", res.ResponseStatus);
			sb.AppendFormat("URI: {0}\n", res.ResponseUri);

			var cookies = res.Cookies;
			if (cookies != null) {
				sb.AppendFormat("N cookies returned: {0}\n", cookies.Count);

				if (includeCookies) {
					foreach (var cookie in cookies) {
						sb.AppendFormat("\tCookie: {0}: {1}\n", cookie.Name, cookie.Value);
					}
				}
			}


			var errmsg = res.ErrorMessage;
			if (errmsg != null) {
				sb.AppendFormat("Error message: {0}\n", errmsg);
			}

			var content = res.Content;
			if (includeContent) {
				sb.AppendFormat("Content: {0}\n", content);
			}


			Console.WriteLine(sb);
		}

		public static IRestRequest Create(string action)
		{
			var req = new RestRequest();
			req.AddQueryParameter("action", action);
			req.AddQueryParameter("format", "json");
			return req;
		}

		internal static bool TryGetContinueToken(JObject obj, out JToken next, string key1, string key2)
		{
			if (!obj.ContainsKey(key1)) {
				next = null;
				return false;
			}

			next = obj[key1][key2];
			return true;
		}
	}
}