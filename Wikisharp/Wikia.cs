using Newtonsoft.Json.Linq;

namespace Wikisharp
{
	internal static class Wikia
	{
		internal static bool TryGetContinueToken(JObject obj, out JToken next,string s1, string s2)
		{
			/*if (!listResponseObj.ContainsKey("continue")) {
				break;
			}
			rlContinue = listResponseObj["continue"]["rlcontinue"].ToString();*/


			if (!obj.ContainsKey(s1)) {
				next = null;
				return false;
			}

			next = obj[s1][s2];
			return true;
		}
	}
}