﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using DBDiff.Schema;
using DBDiff.Schema.SQLServer;
using DBDiff.Schema.SQLServer.Options;
using Microsoft.SqlServer.Management.Smo;
using Column=DBDiff.Schema.SQLServer.Model.Column;
using Table=DBDiff.Schema.SQLServer.Model.Table;

namespace MigrationScriptGenerator
{
    public class Program
	{
		#region Category enum

		public enum Category
		{
			error,
			warning
		}

		#endregion
        //. $(SolutionName)Db_$(ConfigurationName) . $(SolutionName)Db_$(ConfigurationName)_Next $(ProjectDir)\Sql
		static void Main(string[] args)
		{
		    GenerateScript(args[0], args[1], args[2], args[3], args[4]);
		}
        public static void GenerateScript(string oldServerName, string oldDbName, string newServerName, string newDbName, string scriptOutputDirectory)
        {
	    Console.WriteLine("Starting " + typeof(Program).Assembly.GetName().Name);
			var sw = new Stopwatch();
			sw.Start();
			var oldSmoServer = new Server(oldServerName);
			Database oldSmoDb = oldSmoServer.Databases[oldDbName];
			if (oldSmoDb == null)
			{
				oldSmoDb = new Database(oldSmoServer, oldDbName);
				oldSmoDb.Create();
			}
			

			var options = new SqlOption();
			var sql = new Generate();
			sql.ConnectionString = "server=" + oldServerName + ";database=" + oldDbName + ";trusted_connection=true";
			DBDiff.Schema.SQLServer.Model.Database oldDiffDb = sql.Process(options);

			sql.ConnectionString = "server=" + newServerName + ";database=" + newDbName + ";trusted_connection=true";
			DBDiff.Schema.SQLServer.Model.Database newDiffDb = sql.Process(options);

			DBDiff.Schema.SQLServer.Model.Database diff = Generate.Compare(oldDiffDb, newDiffDb);
			var script = new StringBuilder();
			bool issues = false;

			foreach (
				Table droppedTable in Enumerable.Where<Table>(diff.Tables, t => t.Status == Enums.ObjectStatusType.DropStatus))
			{
				WriteError("Table drop: " + droppedTable.Name);
				script.AppendLine(droppedTable.ToSqlDrop());
				issues = true;
			}
			foreach (Table table in diff.Tables)
			{
				foreach (Column droppedColumn in table.Columns
					.Where(c => c.Status == Enums.ObjectStatusType.DropStatus))
				{
					Column renamedColumn = table.Columns.SingleOrDefault(c => c.Id == droppedColumn.Id && c.Name != droppedColumn.Name);
					if (renamedColumn != null)
					{
						WriteError("Column rename: " + droppedColumn.Parent.Name + "." + droppedColumn.Name);
						Func<string, string> addSquareBracketsIfContainsADot = s => s.Contains(".") ? ("[" + s + "]") : s;
						script.AppendFormat("\r\nsp_rename '{0}.{1}', '{2}', 'COLUMN';\r\nGO",
						                    addSquareBracketsIfContainsADot(droppedColumn.Parent.Name),
						                    addSquareBracketsIfContainsADot(droppedColumn.Name),
						                    renamedColumn.Name);
					}
					else
					{
						WriteError("Column drop: " + droppedColumn.Parent.Name + "." + droppedColumn.Name);
						script.AppendLine(droppedColumn.ToSqlDrop());
					}
					issues = true;
				}
			}


			if (issues == false)
			{
				string updateScript = diff.ToSqlDiff().ToSQL();
				updateScript = updateScript.Substring(updateScript.IndexOf("GO") + 2).Trim();
				script.AppendLine(updateScript);
			}
			string text = script.ToString().Trim();
			if (text.Length > 0)
			{
				WriteScriptToDisk(text, scriptOutputDirectory);
			}
			sw.Stop();
			Console.WriteLine("Finished " + typeof(Program).Assembly.GetName().Name + " " + sw.ElapsedMilliseconds + "ms");
		}

		static void WriteScriptToDisk(string text, string scriptOutputDirectory)
		{
			var dir = new DirectoryInfo(scriptOutputDirectory);
			IEnumerable<FileInfo> scripts = from fi in dir.GetFiles("*.sql") select fi;
			int versionNumber = 0;
			var scriptVersions = from fi in scripts
			                     where fi.Name.IndexOf(" ") > -1
			                           && int.TryParse(fi.Name.Split(" ".ToCharArray(), 2)[0], out versionNumber)
			                     select new {File = fi, Version = versionNumber};
			var highest = scriptVersions.OrderByDescending(s => s.Version).FirstOrDefault();
			string scriptNameEnding = " - generated update.sql.suggested";
			int version = highest != null ? highest.Version + 1 : 1;
			string scriptFileName = version.ToString().PadLeft(5, '0') + scriptNameEnding;
			WriteWarning("Created script: " + scriptFileName);
			File.WriteAllText(Path.Combine(dir.FullName, scriptFileName),
			                  text);
		}


		static void WriteError(string text, params object[] args)
		{
			WriteMessage(Assembly.GetEntryAssembly().GetName().Name, "Subcategory", Category.error, "Code",
			             String.Format(text, args));
		}

		static void WriteWarning(string text, params object[] args)
		{
			WriteMessage(Assembly.GetEntryAssembly().GetName().Name, "Subcategory", Category.warning, "Code",
			             String.Format(text, args));
		}

		static void WriteMessage(string origin, string subcategory, Category category, string code, string text)
		{
			Console.WriteLine("{0} : {1} {2} {3} : {4}", origin, subcategory, category, code, text);
		}
	}
}