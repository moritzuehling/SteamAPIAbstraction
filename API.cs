using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace SteamAPIAbstraction
{
	class API
	{
		public static List<Profile> ExtractProfilesFromStatus(string input)
		{
			List<Profile> res = new List<Profile>();

			string pattern = @"""(.+)"" (STEAM_\d:\d:\d+)";
			Regex regex = new Regex(pattern);

			foreach (Match match in regex.Matches(input))
			{
				string name = match.Groups[1].Value;

				long id = long.Parse(SteamId2IdConverter(match.Groups[2].Value).ToString());

				res.Add(new Profile(id, name));
			}

			return res;
		}

		private static string GetFriendID(string authid)
		{
			string[] split = authid.Split(':');
			return "765611979" + ((Convert.ToInt64(split[2]) * 2) + 60265728 + Convert.ToInt64(split[1])).ToString();
		}

		public static long SteamId2IdConverter(string param) //41832667 TO STEAM_0:0: 76561198043931062
		{
			var splitInfo = param.Split(':'); //Do da splitz.
			var identifierMultiply = Int64.Parse(splitInfo[2]) * 2; // Multiply the unique number by 2 STEAM_0:0: [ 41832667 ] < DAT BIT
			var identifierTotal = identifierMultiply + 76561197960265728;
			identifierTotal += Int64.Parse(splitInfo[1]);  // Add the odd / even identifier :1: / :0:
			return identifierTotal; // Hoorah.
		}

	}
}
