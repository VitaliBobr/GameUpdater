using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;
using System.Net;

namespace MergeUpdate
{
    class Program
    {
        public static void print(string text) => Console.WriteLine(text);
        static void Main(string[] args)
        {
            #region initializing
            string path = CutExtension(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\";
            string gamePath = path + "\\game\\";
            string clientConfigPath = gamePath + "config.txt";
            string serverConfigPath = path + "config.txt";
            string hosts = "192.168.1.7:21";
            CreateConfig(@"C:\Users\User\Desktop\AutoUpdatedV1\");
            DownloadFileFTPServer("ftp://" + hosts + "//PrgmV2/config.txt", serverConfigPath, "tester", "q6948699");
            Update(@"C:\Users\User\Desktop\AutoUpdatedV1\config.txt", serverConfigPath,hosts);
            Console.ReadKey();
        }
        private static string CutExtension(string path)
        {
            List<string> new_path = path.Split(new char[] { '\\' }).ToList();
            new_path.RemoveAt(new_path.Count - 1);
            path = "";
            foreach (var item in new_path)
            {
                path += item + "\\";
            }

            return path;
        }
        private static void Update (string pathConfigClient, string pathConfigServer,string hosts)
        {
            print("Client Config \n\r");
            DateTime dt = DateTime.Now;
            dt.ToString();
            List<String> one = File.ReadAllLines(pathConfigClient).ToList();
            one.ForEach((x) => { print(x); });
            print("Server Config \n\r");
            List<String> two = File.ReadAllLines(pathConfigServer).ToList();
            two.ForEach((x) => { print(x); });
            print("Diff \n\r");
            List<String> for_replace_equals = new List<string>();
            FindDiffConfig(one, two, for_replace_equals);
            for_replace_equals.ForEach((x) => { print(x); });
            int i = 0;
            foreach (var item in for_replace_equals)
            {
                string name = item.Split(new String[] { " +-+ " },StringSplitOptions.None)[0];
                print(name);
                DownloadFileFTPServer("ftp://" + hosts + "//PrgmV2/" + name, new FileInfo(pathConfigClient).Directory.FullName + "//" + name, "tester", "q6948699");
                i++;
            }
            DownloadFileFTPServer("ftp://" + hosts + "//PrgmV2/config.txt", pathConfigClient, "tester", "q6948699");
            print(for_replace_equals.Count.ToString() + i.ToString());
        }
        private static void UpdateFromUrl(string pathConfigClient, string url)
        {
            List<String> clientConfig = File.ReadAllLines(pathConfigClient).ToList();
            print("Starting_Download");
            List<String> serverConfig = DownloadString(url).Split(new char[] { '\n' }).ToList();
            serverConfig.ForEach(x => print(x));
            print("Ending_Download");
            List<String> for_replace_equals = new List<string>();
            FindDiffConfig(clientConfig, serverConfig, for_replace_equals);
            for_replace_equals.ForEach((x) => { print(x); });

        }
        private static void FindDiffConfig//Find diff between one and two and pull in for_replace
        (List<string> one, List<string> two, List<string> for_replace_equals)
        {
            foreach (var new_item in two)
            {
                bool isFounded = false;
                foreach (var old_item in one)
                {
                    if (new_item == old_item)
                    {
                        isFounded = true;
                        break;
                    }
                }
                if (isFounded == false)
                {
                    for_replace_equals.Add(new_item);
                }
            }
        }

        private static void CreateConfig(string path_to_directory)
        {
            using (StreamWriter sw = File.CreateText(path_to_directory + "config.txt"))
            {
                var files_fnames = Directory.GetFiles(path_to_directory, "*.*", SearchOption.AllDirectories).ToList();
                files_fnames.Remove(path_to_directory + "config.txt");
                foreach (var item in files_fnames)
                {
                    DirectoryInfo di = new DirectoryInfo(item);
                    var date = File.GetCreationTimeUtc(item);
                    
                    sw.WriteLine( di.FullName.Replace(path_to_directory,"") + " +-+ " + GetHash(item));
                }
                sw.Close();
            }
        }
        public static string DownloadString(string url) {
            var client = new WebClient();
            return client.DownloadString(url);
        }
        public static string DownloadFileFTPServer(string target,string pathToSave,string user, string password) {
            WebClient client = new WebClient();
            client.Credentials = new NetworkCredential(user, password);
            try
            {
                client.DownloadFile(target, pathToSave);
            }
            catch (System.Net.WebException ex)
            {
                if(ex.InnerException.GetType().ToString() == "System.IO.DirectoryNotFoundException") {
                    new FileInfo(pathToSave).Directory.Create();
                    print("Created Directory" + pathToSave);
                    DownloadFileFTPServer(target, pathToSave, user, password);
                    print(ex.InnerException.GetType().ToString());
                }
                else {
                    print(ex.InnerException.GetType().ToString());
                    throw ex.InnerException;
                }
            }
            catch (Exception ex) {
                print("Uncommon exception " + ex.Message.ToString());
                throw;
            }
            return pathToSave;
        }
        public static string GetHash(string file_name) {
            using (var md5 = MD5.Create()) {
                using (var stream = File.OpenRead(file_name)) {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }
    }
}
#endregion