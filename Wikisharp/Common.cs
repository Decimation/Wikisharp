using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Newtonsoft.Json;
using RestSharp;

[assembly: InternalsVisibleTo("Test")]

namespace Wikisharp
{
	internal static class Common
	{
		public static string JsonPrettify(string json)
		{
			using var stringReader = new StringReader(json);
			using var stringWriter = new StringWriter();

			var jsonReader = new JsonTextReader(stringReader);
			var jsonWriter = new JsonTextWriter(stringWriter) {Formatting = Formatting.Indented};
			jsonWriter.WriteToken(jsonReader);

			return stringWriter.ToString();
		}

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
	}
}