using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MongoDB.Bson.Serialization.Attributes;

namespace SteamAPIAbstraction
{
	public class Game
	{
		[BsonId]
		private long ID;

		static List<Lobby> lobbies;
		public static List<Lobby> Lobbies {
			get {
				return lobbies;
			}
			set {
				lobbies = value;
			}
		}

		public DateTime timeOfRecord;
		public DateTime TimeOfRecord {
			get {
				return timeOfRecord;
			}
			set {
				timeOfRecord = value;
			}
		}

		[BsonConstructor]
		private Game ()
		{
		}

		public Game (List<Profile> profiles)
		{
			profiles.Select (a => new Lobby (a));

			this.ID = GenerateID ();
		}

		private long GenerateID()
		{
			long currID = 0;

			var ids = GetProfiles ().Select (a => a.SteamID).OrderBy (a => a);

			foreach(var id in ids)
			{
				currID = currID << 5;
				currID += id & 31; //31 = 0b11111
			}

			return currID;
		}

		private IEnumerable<Profile> GetProfiles()
		{
			List<Profile> res = new List<Profile> ();
			lobbies.ForEach (lobby => res.AddRange (lobby.Members));
			return res;
		}

		public void SimplifyLobbies()
		{
			while (true)
			{
				bool changeOccured = false;

				for (int i = 0; i < lobbies.Count; i++)
				{
					Lobby l1 = lobbies[i];
					for (int j = 0; j < lobbies.Count; j++)
					{
						Lobby l2 = lobbies[j];

						if (l1 != l2 && l1.canBeJoined(l2))
						{
							Lobby l3 = new Lobby(l1.Members, l2.Members);
							lobbies.Remove(l1);
							lobbies.Remove(l2);
							lobbies.Add(l3);
							changeOccured = true;
							break;
						}
					}
					if (changeOccured)
						break;
				}

				if (!changeOccured)
					break;

				changeOccured = false;
			}
		}
	}
}

