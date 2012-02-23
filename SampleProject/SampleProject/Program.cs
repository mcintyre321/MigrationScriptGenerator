 
using System;
using System.Linq;
using FluentNHibernate.AutoMap;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using Microsoft.SqlServer.Management.Smo;
using NHibernate.Bytecode.Lightweight;
using NHibernate.Tool.hbm2ddl;
using SampleProject.Entities;
using Configuration=NHibernate.Cfg.Configuration;
using Environment=NHibernate.Cfg.Environment;

namespace SampleProject
{
	class Program
	{
		static void Main(string[] args)
		{
			if (args[0] == "build")
			{
				string serverName = args[1];
				string databaseName = args[2];
				var server = new Server(serverName);
				var database = server.Databases[databaseName];
				if (database != null)
				{
					database.Drop();
				}
				database = new Database(server, databaseName);
				database.Create();
				Environment.BytecodeProvider = new BytecodeProvider();
				Configuration configuration = null;
				var sf = Fluently.Configure()
					.Database(MsSqlConfiguration.MsSql2005
								.ConnectionString(
								c => c.Is(String.Format("server={0};database={1};trusted_connection=true", serverName, databaseName)))
								.UseReflectionOptimizer()
					)
					.Mappings(a => a.AutoMappings.Add(() =>
													  AutoPersistenceModel
														.MapEntitiesFromAssemblyOf<Customer>().Where(
														t => t.GetProperties().Any(p => p.Name == "Id"))

									))
					.ExposeConfiguration(c => configuration = c).BuildSessionFactory();

				new SchemaExport(configuration).Create(false, true);
			}
		}
	}
}
