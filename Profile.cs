using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SteamAPIAbstraction
{
	class Profile
	{
		string steamID;
		public string SteamID
		{
			get { return steamID; }
		}

		string name;
		public string Name
		{
			get { return name; }
		}

		bool isPrivate;
		public bool IsPrivate
		{
			get { return isPrivate; }
		}

		private List<Profile> friends = new List<Profile>();

		[JsonIgnore]
		public List<Profile> Friends
		{
			get { return friends; }
		}

		bool downloadFinished = false;

		[JsonIgnore]
		public bool DownloadFinished
		{
			get { return downloadFinished; }
		}

		string avatarIcon;
		public string AvatarIcon
		{
			get { return avatarIcon; }
		}

		public Profile(string steamID, string name)
		{
			this.steamID = steamID;
			this.name = name;
		}

		string hoursPlayed;
		public string HoursPlayed
		{
			get { return hoursPlayed; }
			set { hoursPlayed = value; }
		}

		string hoursOnRecord;
		public string HoursOnRecord
		{
			get { return hoursOnRecord; }
			set { hoursOnRecord = value; }
		}


		private string vacBanned;
		public string VacBanned
		{
			get { return vacBanned; }
			set { vacBanned = value; }
		}

		string location;
		public string Location
		{
			get { return location; }
			set { location = value; }
		}


		public Profile(string steamID)
		{
			this.steamID = steamID;
		}

		
		public override bool Equals(object obj)
		{
			return obj is Profile && ((Profile)obj).steamID == this.steamID;
		}

		
		public async Task<bool> Download(bool withFriends)
		{
			try
			{
				var client = new HttpClient();

				var profileData = client.GetStringAsync("http://steamcommunity.com/profiles/" + SteamID + "?xml=1");
				XmlDocument doc = new XmlDocument();

				doc.LoadXml(await profileData);

				var profile = doc.DocumentElement;

				isPrivate = profile["privacyState"].InnerText != "public";

				avatarIcon = profile["avatarIcon"].InnerText;

				name = profile["steamID"].InnerText;

				vacBanned = profile["vacBanned"].InnerText;

				if (!isPrivate)
				{
					location = profile["location"].InnerText;

					var games = profile["mostPlayedGames"].ChildNodes;
					foreach (XmlNode game in games)
					{
						if (game["gameName"].InnerText.Contains("Counter-Strike: Global Offensive"))
						{
							this.hoursPlayed = game["hoursPlayed"].InnerText.Replace(",", "");
							this.hoursOnRecord = game["hoursOnRecord"].InnerText.Replace(",", "");
						}
					}
				}

				downloadFinished = true;

				if (withFriends && !isPrivate)
				{
					return await DownloadFriends();
				}

				return true;
			}
			catch
			{
				return false;
			}
		}

		private async Task<bool> DownloadFriends()
		{
			try
			{
				var client = new HttpClient();

				var profileData = client.GetStringAsync("http://steamcommunity.com/profiles/" + SteamID + "/friends/?xml=1");
				XmlDocument doc = new XmlDocument();

				doc.LoadXml(await profileData);

				var friendsList = doc.DocumentElement["friends"];

				foreach(XmlNode friend in friendsList.ChildNodes)
				{
					friends.Add(new Profile(friend.InnerText));
				}

				return true;
			}
			catch
			{
				return false;
			}
		}
	}
}
