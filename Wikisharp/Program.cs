using System;
using System.IO;

namespace Wikisharp
{
	/**
	 * Single file executable build dir
	 * 
	 * C:\Users\Deci\RiderProjects\Wikisharp\Wikisharp\bin\Release\netcoreapp3.0\win10-x64
	 * C:\Users\Deci\RiderProjects\Wikisharp\Wikisharp\bin\Release\netcoreapp3.0\win10-x64\publish
	 * C:\Users\Deci\RiderProjects\Wikisharp\Wikisharp\bin\Debug\netcoreapp3.0\win10-x64
	 *
	 * Single file publish command
	 *
	 * dotnet publish -c Release -r win10-x64
	 *
	 *
	 * Copy build
	 *
	 * copy Wikisharp.exe C:\Library /Y
	 * copy Wikisharp.exe C:\Users\Deci\Desktop /Y
	 *
	 * Bundle extract dir
	 * 
	 * C:\Users\Deci\AppData\Local\Temp\.net\Wikisharp
	 * DOTNET_BUNDLE_EXTRACT_BASE_DIR 
	 */
	public static class Program
	{
		private static void Main(string[] args)
		{
			if (args == null || args.Length < 2) {
				return;
			}

			var token = args[0];
			var user  = args[1];

			var ws = new WikiSession(user, token);
			var wc = new WikiClient(ws);

			var cd   = Environment.CurrentDirectory;
			var dest = Path.Combine(cd, "wiki");

			var lists=wc.GetAllLists(dest);

			Console.WriteLine("Exported {0} lists to {1}", lists.Count,dest);
		}
	}
}