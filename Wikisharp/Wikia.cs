using Newtonsoft.Json.Linq;

namespace Wikisharp
{
	public class Wikia
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
		
		internal static bool TryGetToken(JObject obj, out JToken tk, params string[] keys)
		{
			if (!obj.ContainsKey(keys[0])) {
				tk = null;
				return false;
			}

			tk = null;

			for (int i = 0; i < keys.Length; i++) {
				tk = obj[keys[i]];
			}

			if (tk!=null) {
				
				return true;
			}
			tk = null;
			return false;
		}
	}
}