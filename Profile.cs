using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace SteamAPIAbstraction
{
	public class Profile
	{

		long steamID;
		public long SteamID
		{
			get { return steamID; }
			set { steamID = value; }
		}

		string name;
		public string Name
		{
			get { return name; }
			set { name = value; }
		}

		bool isPrivate;
		public bool IsPrivate
		{
			get { return isPrivate; }
			set { isPrivate = value; }
		}

		private List<Profile> friends = new List<Profile>();

		//[JsonIgnore]
		public List<Profile> Friends
		{
			get { return friends; }
			set { friends = value; }
		}

		bool downloadFinished = false;
		[BsonIgnore]
		public bool DownloadFinished {
			get { return downloadFinished;}
			set { downloadFinished = value;}
		}

		string avatarIcon;
		public string AvatarIcon
		{
			get { return avatarIcon; }
			set {avatarIcon = value; }
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

		DateTime lastUpdate;
		public DateTime LastUpdate {
			get {
				return lastUpdate;
			}
			set {
				lastUpdate = value;
			}
		}		

		[BsonConstructor]
		public Profile(long steamID)
		{
			this.steamID = steamID;
		}

		public Profile(long steamID, string name)
		{
			this.steamID = steamID;
			this.name = name;
		}

		public static Profile GetFromDatabase(long steamID)
		{
			var res = Database.Load<Profile> (new BsonInt64(steamID));
			if (res == null)
				return new Profile (steamID);

			return res;
		}
		
		public override bool Equals(object obj)
		{
			return obj is Profile && ((Profile)obj).steamID == this.steamID;
		}

		public override int GetHashCode ()
		{
			return steamID.GetHashCode ();
		}
		
		public async Task<bool> Download(bool withFriends)
		{
			try
			{
				downloadFinished = false;

				var client = new HttpClient();

				var profileData = client.GetStringAsync("http://steamcommunity.com/profiles/" + SteamID + "?xml=1");
				XmlDocument doc = new XmlDocument();

				string res = await profileData;
				doc.LoadXml(res);

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


				if (withFriends && !isPrivate)
				{
					return await DownloadFriends();
				}
				
				lastUpdate = DateTime.Now;
				downloadFinished = true;
				return true;
			}
			catch
			{
				downloadFinished = true;
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
					friends.Add(new Profile(long.Parse(friend.InnerText)));
				}
				
				lastUpdate = DateTime.Now;	
				downloadFinished = true;
				return true;
			}
			catch
			{
				downloadFinished = true;
				return false;
			}
		}
	}
}
