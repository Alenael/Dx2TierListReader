using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Web;

namespace Dx2TierListReader
{
    class Program
    {
        static void Main(string[] args)
        {
            if (File.Exists("TierData.csv"))
                File.Delete("TierData.csv");

            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = null;
            bool repeat = true;
            int count = 0;

            while (repeat)
            {
                try
                {
                    Console.WriteLine("Attempting to download site's HTML..");
                    doc = web.Load("https://dx2wiki.com/index.php/Tier_List");
                    repeat = false;
                }
                catch (Exception e)
                {
                    count++;
                    if (count >= 5)
                    {
                        repeat = false;
                        Console.WriteLine("I give up. Can't download site's HTML..");
                    }
                    else
                    {
                        Console.WriteLine("Could not download site. Sleeping for 2 minutes..");
                        System.Threading.Thread.Sleep(600000);
                    }
                }
            }

            if (doc != null)
            {
                var fiveStarDemons = LoopThroughTable(doc.DocumentNode.SelectNodes("//table[1]/tbody/tr"));
                var fourStarDemons = LoopThroughTable(doc.DocumentNode.SelectNodes("//table[2]/tbody/tr"));

                var allDemons = new List<DemonInfo>();
                allDemons.AddRange(fiveStarDemons);
                allDemons.AddRange(fourStarDemons);

                CreateCSV(allDemons);

                if (File.Exists("TierData.csv"))
                {
                    File.Copy(@"TierData.csv", @"C:\Users\darks\Documents\GitHub\Dx2DB\csv\TierData.csv", true);
                    Console.WriteLine("File was copied.");
                    CommitTierInfo();
                }
            }
        }

        static void CreateCSV(List<DemonInfo> demons)
        {
            using (StreamWriter outputFile = new StreamWriter("TierData.csv"))
            {
                outputFile.WriteLine("Name,BestArchetypePvE,BestArchetypePvP,PvEScore,PvPOffenseScore,PvPDefScore,Pros,Cons");

                foreach (var demon in demons)
                {
                    outputFile.WriteLine(
                        demon.Name + "," +
                        demon.BestArchetypePvE + "," +
                        demon.BestArchetypePvP + "," +
                        demon.PvEScore + "," +
                        demon.PvPOffenseScore + "," +
                        demon.PvPDefScore + "," +
                        demon.Pros + "," +
                        demon.Cons);
                }
            }
        }

        static List<DemonInfo> LoopThroughTable(HtmlNodeCollection demonCount)
        {
            var demonList = new List<DemonInfo>();

            for (var i = 2; i < demonCount.Count; i += 4)
            {
                //Craete Demon
                var demonInfo = new DemonInfo();
                
                demonInfo.Name = SurroundWithQuotes(demonCount[i].ChildNodes[1].InnerText.Replace("\n", "").Trim().Replace("Error creating thumbnail: Unable to save thumbnail to destination", "").Replace("50px", "").Trim().Replace("_", " "));
                demonInfo.BestArchetypePvE = CreateArchetypeFrom(demonCount[i].ChildNodes[3].InnerHtml);
                demonInfo.BestArchetypePvP = CreateArchetypeFrom(demonCount[i].ChildNodes[5].InnerHtml);
                demonInfo.PvEScore = Convert.ToDouble(demonCount[i].ChildNodes[7].InnerText);
                demonInfo.PvPOffenseScore = Convert.ToDouble(demonCount[i].ChildNodes[9].InnerText);
                demonInfo.PvPDefScore = Convert.ToDouble(demonCount[i].ChildNodes[11].InnerText);
                demonInfo.Pros = SurroundWithQuotes(HttpUtility.HtmlDecode(demonCount[i + 1].InnerText.Replace("Pros", "").Trim()));
                demonInfo.Cons = SurroundWithQuotes(HttpUtility.HtmlDecode(demonCount[i + 2].InnerText.Replace("Cons", "").Trim()));

                //Add Demon
                if (demonInfo.Name != "")
                    demonList.Add(demonInfo);
            }

            return demonList;
        }

        static string SurroundWithQuotes(string text)
        {
            return "\"" + text.Replace("\"", "") + "\"";
        }

        static string CreateArchetypeFrom(string innerText)
        {
            var myArcheType = "";

            if (innerText.Contains("ArchClear"))
                myArcheType += "C";

            if (innerText.Contains("ArchTeal"))
                myArcheType += "T";

            if (innerText.Contains("ArchYellow"))
                myArcheType += "Y";

            if (innerText.Contains("ArchRed"))
                myArcheType += "R";

            if (innerText.Contains("ArchPurple"))
                myArcheType += "P";

            if (innerText.Contains("Any"))
                myArcheType = "Any";

            return myArcheType;
        }

        static public void CommitTierInfo()
        {
            Process.Start("UploadTierInfo.bat");
        }
    }

    public class DemonInfo
    {
        public string Name = "";
        public string BestArchetypePvE = "";
        public string BestArchetypePvP = "";
        public double PvEScore = 0;
        public double PvPOffenseScore = 0;
        public double PvPDefScore = 0;
        public string Pros = "";
        public string Cons = "";
    }
}

