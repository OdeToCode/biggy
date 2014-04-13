using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace Biggy.JSON
{
    public class JsonStore<T> : IBiggyStore<T>
    {
        public JsonStore(string dbPath = "current", bool inMemory = false, string dbName = "")
        {
            if (String.IsNullOrWhiteSpace(dbName))
            {
                var thingyType = this.GetType().GenericTypeArguments[0].Name;
                DbName = Inflector.Inflector.Pluralize(thingyType).ToLower();
            }
            else
            {
                DbName = dbName.ToLower();
            }
            DbFileName = DbName + ".json";
            SetDataDirectory(dbPath);            
        }

        IList<T> IBiggyStore<T>.Load()
        {
            if (File.Exists(DbPath))
            {
                var json = "[" + File.ReadAllText(DbPath).Replace(Environment.NewLine, ",") + "]";
                var result = JsonConvert.DeserializeObject<List<T>>(json);
                return result;
            }
            return new List<T>();
        }

        void IBiggyStore<T>.SaveAll(IList<T> items)
        {
            var json = JsonConvert.SerializeObject(items);            
            var buff = Encoding.Default.GetBytes(json);
            using (var fs = File.OpenWrite(DbPath))
            {
                fs.WriteAsync(buff, 0, buff.Length);
            }   
        }

        void IBiggyStore<T>.Clear()
        {
            ((IBiggyStore<T>)this).SaveAll(new List<T>());
        }

        T IBiggyStore<T>.Add(T item)
        {
            var json = JsonConvert.SerializeObject(item);            
            using (var writer = File.AppendText(DbPath))
            {
                writer.WriteLine(json);
            }
            return item;
        }

        IEnumerable<T> IBiggyStore<T>.Add(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                ((IBiggyStore<T>) this).Add(item);
            }
            return items;
        }

        public string DbFileName { get; set; }
        public string DbName { get; set; }
        public string DbDirectory { get; set; }
        public string DbPath 
        {
            get 
            {
              return Path.Combine(DbDirectory, DbFileName);
            }
        }

        public bool HasDbFile 
        {
            get 
            {
              return File.Exists(DbPath);
            }
        }

        public void SetDataDirectory(string dbPath)
        {
            var dataDir = dbPath;
            if (dbPath == "current")
            {
                var currentDir = Directory.GetCurrentDirectory();
                if (currentDir.EndsWith("Debug") || currentDir.EndsWith("Release"))
                {
                    var projectRoot = Directory.GetParent(@"..\..\").FullName;
                    dataDir = Path.Combine(projectRoot, "Data");
                }
            }
            else
            {
                dataDir = Path.Combine(dbPath, "Data");
            }
            Directory.CreateDirectory(dataDir);
            DbDirectory = dataDir;
        }
    }
}
