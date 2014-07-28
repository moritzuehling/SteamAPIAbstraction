using System;
using MongoDB.Driver.Builders;
using MongoDB.Driver;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson;

namespace SteamAPIAbstraction
{
	public static class Database
	{
		private static MongoDatabase database = null;
		private static MongoDatabase GetDatabase()
		{
			if (database == null) {
				MongoClient client = new MongoClient ();
				database = client.GetServer ().GetDatabase ("SteamAPIAbstraction");
			}

			return database;
		}
		
		private static MongoCollection GetCollection<T>()
		{
			return GetCollection (typeof(T));
		}

		private static MongoCollection GetCollection(Type type)
		{
			return GetCollection ("FLAPI_" + type.Name);
		}

		private static MongoCollection GetCollection(string name)
		{
			return GetDatabase().GetCollection (name);
		}

		public static T Load<T>(BsonValue id)
		{
			Map<T> ();
			return GetCollection<T> ().FindOneByIdAs<T> (id);
		}

		public static void Save(Profile profile)
		{
			Save<Profile> (profile);
		}
		
		private static void Save<T>(T t)
		{
			Map<T> ();
			var collection = GetCollection<T> ();
			collection.Save (t);
		}

		private static void Map<T>()	
		{
			if (typeof(Profile) == typeof(T)) {
				MapProfile ();
				return;
			}

			throw new ArgumentException ("Could not Map T (T is " + typeof(T).FullName + ")");
		}

		private static void MapProfile()
		{
			if(!BsonClassMap.IsClassMapRegistered(typeof(Profile)))
			{
				var pack = new ConventionPack();
				//pack.Add(new IgnoreIfNullConvention(true));
				pack.Add(new IgnoreIfDefaultConvention(true));

				ConventionRegistry.Register(
					"SmallConventions",
					pack,
					t => t == typeof(Profile));

				BsonClassMap.RegisterClassMap<Profile> (
					cm => 
					{
						cm.AutoMap();
						cm.MapIdProperty(p => p.SteamID);
						cm.MapCreator(p => new Profile(p.SteamID));
					}
				);
			}
		}
	}
}

