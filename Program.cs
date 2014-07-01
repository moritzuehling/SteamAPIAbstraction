using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamAPIAbstraction
{
	class Program
	{
		static void Main(string[] args)
		{
			if (Debugger.IsAttached)
			{
				Console.SetIn(new StreamReader("test.txt"));
				Console.SetOut(new StreamWriter("out.json.txt"));
			}
			
			List<Profile> persons = null;

			string type = Console.ReadLine();
			if (type == "console")
			{
				string input = "";
				while (true)
				{
					string line = Console.ReadLine();

					if (line.Trim().StartsWith("#end"))
					{
						break;
					}

					input += line + Environment.NewLine;
				}

				persons = API.ParseInput(input);
			}
			else if (type == "lines")
			{
				persons = new List<Profile>();
				while (true)
				{
					string p = Console.ReadLine();

					if (p == "end")
						break;

					persons.Add(new Profile(p));
				}
			}
			else
			{
				Console.Error.WriteLine("Wrong usage. [" + type +"]");
				return;
			}

			if (persons.Count > 32) //Nopenopenopenope
			{
				Console.Error.WriteLine("Too big status!");
				return;
			}


			Console.Error.WriteLine("Read profiles. Parsing Persons.");

			List<Task<bool>> results = new List<Task<bool>>();

			foreach (Profile p in persons)
			{
				results.Add(p.Download(true));
			}

			Task.WaitAll(results.ToArray());

			List<Lobby> lobbies = new List<Lobby>();

			Console.Error.WriteLine("Waiting for download to finsih....");
			lobbies.AddRange(persons.Select(a => new Lobby(a)));

			Console.Error.WriteLine("Sorting....");
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

			Console.Error.WriteLine("Serializing....");
			JsonSerializer serializer = new JsonSerializer();

			StringBuilder sb = new StringBuilder();
			StringWriter sw = new StringWriter(sb);

			Dictionary<string, object> dummyDic = new Dictionary<string, object>();

			dummyDic.Add("Lobbies", lobbies);

			JsonTextWriter writer = new JsonTextWriter(sw);
			serializer.Serialize(writer, dummyDic);

			Console.WriteLine(sb.ToString());


		}


	}
}
