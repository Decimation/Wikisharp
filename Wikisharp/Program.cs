using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using Wikisharp.Utilities;

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

			var user  = args[0];
			var token = args[1];


			var raindropToken = args.Length >= 2 ? args[2] : null;
			var useRaindrop   = raindropToken != null;

			Console.WriteLine("User: {0} | Token: {1} | Raindrop import: {2}", user, token, useRaindrop);

			if (useRaindrop) {
				Console.WriteLine("Raindrop token: {0}", raindropToken);
			}
			
			
			var ws = new WikiSession(user, token);
			var wc = new WikiClient(ws);

			const string DEST_FOLDER = "wiki";

			var cd   = Environment.CurrentDirectory;
			var dest = Path.Combine(cd, DEST_FOLDER);

			var lists = wc.GetAllLists(dest);

			// todo: messy and poorly organized

			var rc            = useRaindrop ? new RaindropClient(raindropToken) : null;
			var rdCollections = useRaindrop ? rc.GetCollections() : null;

			for (int i = 0; i < lists.Count; i++) {
				var list = lists[i];
				var wrl  = list.List;
				var wrle = list.Entries;

				Console.WriteLine("\t{0} (id {1}) (updated {2}): {3} entries",
				                  wrl.Name, wrl.Id, wrl.Updated, wrle.Length);


				if (useRaindrop) {
					var raindropItems = new List<RaindropClient.RaindropItem>();
					var raindropCol =
						rdCollections.FirstOrDefault(c => c.title == wrl.Name);

					if (raindropCol == default) {
						Console.WriteLine("No collection with name {0} found; continuing", wrl.Name);
						continue;
					}

					foreach (var entry in wrle) {
						var ri = new RaindropClient.RaindropItem
						{
							collectionId = raindropCol._id,
							tags         = new[] {"wiki_import"},
							title        = entry.Title
						};


						var pl = wc.GetPageLink(entry.Title, out _);

						if (pl == null) {
							continue;
						}

						ri.link = pl;

						raindropItems.Add(ri);
					}

					rc.AddLinks(raindropItems.ToArray());

					Console.WriteLine("Added {0} items from {1} to collection {2} ({3})",
					                  raindropItems.Count, wrl.Name, raindropCol.title, raindropCol._id);
				}
			}

			Console.WriteLine("Exported {0} lists to {1}", lists.Count, dest);
		}
	}
}