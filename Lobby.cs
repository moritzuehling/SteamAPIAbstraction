using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamAPIAbstraction
{
	class Lobby
	{
		public List<Profile> Members = new List<Profile>();

		private List<Profile> friends = new List<Profile>();

		public Lobby(Profile a)
		{
			Members.Add(a);

			CreateFriendsList();
		}

		public Lobby(List<Profile> a, List<Profile> b)
		{
			Members.AddRange(a);
			Members.AddRange(b);

			CreateFriendsList();
		}

		private void CreateFriendsList()
		{
			Members.ForEach(a => friends.AddRange(a.Friends));
		}

		public bool canBeJoined(Lobby other)
		{
			return other.Members.Any(a => friends.Contains(a));
		}
	}
}
