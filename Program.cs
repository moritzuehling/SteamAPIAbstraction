using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Newtonsoft.Json;

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

				persons = API.ExtractProfilesFromStatus(input);
			}
			else if (type == "lines")
			{
				persons = new List<Profile>();
				while (true)
				{
					string p = Console.ReadLine();

					if (p == "end")
						break;

					long steamid = 0;

					if(long.TryParse(p, out steamid ))
						persons.Add(new Profile(steamid));
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


			/*
			Console.Error.WriteLine("Serializing....");
			JsonSerializer serializer = new JsonSerializer();

			StringBuilder sb = new StringBuilder();
			StringWriter sw = new StringWriter(sb);

			Dictionary<string, object> dummyDic = new Dictionary<string, object>();

			dummyDic.Add("Lobbies", lobbies);

			JsonTextWriter writer = new JsonTextWriter(sw);
			serializer.Serialize(writer, dummyDic);

			Console.WriteLine(sb.ToString());
			
			*/
		}

		static void MongoMain()
		{
			var noah = Profile.GetFromDatabase (76561198134960355L);



			noah.Download (true);




			while (!noah.DownloadFinished) {
				System.Threading.Thread.Sleep (100);
			};

			
			Console.WriteLine (noah.Friends.Count);

			Database.Save (noah);
		}
	}
}
