using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using DBDiff.Schema;
using DBDiff.Schema.SQLServer.Generates.Generates;
using DBDiff.Schema.SQLServer.Generates.Model;
using DBDiff.Schema.SQLServer.Generates.Options;

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
        public static void GenerateScript(string sourceServer, string sourceDb, string destServer, string destDb, string scriptOutputDirectory)
        {
            GenerateScript(new SqlConnectionStringBuilder("server=" + sourceServer + ";database=" + sourceDb + ";trusted_connection=true"), new SqlConnectionStringBuilder("server=" + destServer + ";database=" + destDb + ";trusted_connection=true"), scriptOutputDirectory);
        }

        public static void GenerateScript(SqlConnectionStringBuilder source, SqlConnectionStringBuilder destination, string scriptOutputDirectory, SqlOption option = null)
	    {
	        Console.WriteLine("Starting " + typeof(Program).Assembly.GetName().Name);
			var sw = new Stopwatch();
			sw.Start();
			var oldSmoServer = new Microsoft.SqlServer.Management.Smo.Server(source.DataSource);
			var oldSmoDb = oldSmoServer.Databases[source.InitialCatalog];
			if (oldSmoDb == null)
			{
                oldSmoDb = new Microsoft.SqlServer.Management.Smo.Database(oldSmoServer, source.InitialCatalog);
				oldSmoDb.Create();
			}
			

            var sql = new Generate
            {
                ConnectionString = source.ToString(),
                Options = option ?? new SqlOption()
            };
            Database sourceDatabase = sql.Process();

            sql.ConnectionString = destination.ToString();
			Database destinationDatabase = sql.Process();

			Database diff = Generate.Compare(destinationDatabase, sourceDatabase);
			var script = new StringBuilder();
			bool issues = false;

			foreach (Table droppedTable in diff.Tables
                .Where(t => t.Status == Enums.ObjectStatusType.DropStatus))
			{
				WriteError("Table drop: " + droppedTable.Name);
				script.AppendLine(droppedTable.ToSqlDrop());
				issues = true;
			}
            //foreach (Table table in diff.Tables)
            //{
            //    foreach (Column droppedColumn in table.Columns
            //        .Where(c => c.Status == Enums.ObjectStatusType.DropStatus))
            //    {
            //        Column renamedColumn = table.Columns.SingleOrDefault(c => c.Id == droppedColumn.Id && c.Name != droppedColumn.Name);
            //        if (renamedColumn != null)
            //        {
            //            WriteError("Column rename: " + droppedColumn.Parent.Name + "." + droppedColumn.Name);
            //            Func<string, string> addSquareBracketsIfContainsADot = s => s.Contains(".") ? ("[" + s + "]") : s;
            //            script.AppendFormat("\r\nsp_rename '{0}.{1}', '{2}', 'COLUMN';\r\nGO",
            //                                addSquareBracketsIfContainsADot(droppedColumn.Parent.Name),
            //                                addSquareBracketsIfContainsADot(droppedColumn.Name),
            //                                renamedColumn.Name);
            //        }
            //        else
            //        {
            //            WriteError("Column drop: " + droppedColumn.Parent.Name + "." + droppedColumn.Name);
            //            script.AppendLine(droppedColumn.ToSqlDrop());
            //        }
            //        issues = true;
            //    }
            //}


            //if (issues == false)
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
            WriteMessage(typeof(Program).Assembly.GetName().Name, "Subcategory", Category.error, "Code",
                         String.Format(text, args ?? new object[]{}));
        }

        static void WriteWarning(string text)
        {
            WriteWarning(text, null);
        }

        static void WriteError(string text)
        {
            WriteError(text, null);
        }

        static void WriteWarning(string text, params object[] args)
        {
            WriteMessage(typeof(Program).Assembly.GetName().Name, "Subcategory", Category.warning, "Code",
                         String.Format(text, args ?? new object[] { }));
        }

		static void WriteMessage(string origin, string subcategory, Category category, string code, string text)
		{
			Console.WriteLine("{0} : {1} {2} {3} : {4}", origin, subcategory, category, code, text);
		}

        public static Regex WildcardToRegex(string pattern)
        {
            return new Regex("^" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") + "$", RegexOptions.IgnoreCase);
        }
	}
}