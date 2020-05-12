using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Dx2TierListReader
{
    class Program
    {
        static void Main(string[] args)
        {
            HtmlWeb web = new HtmlWeb();
            var doc = web.Load("https://dx2wiki.com/index.php/Tier_List");

            var fiveStarDemons = LoopThroughTable(doc.DocumentNode.SelectNodes("//table[1]/tbody/tr"));
            var fourStarDemons = LoopThroughTable(doc.DocumentNode.SelectNodes("//table[2]/tbody/tr"));

            var allDemons = new List<DemonInfo>();
            allDemons.AddRange(fiveStarDemons);
            allDemons.AddRange(fourStarDemons);

            CreateCSV(allDemons);
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
                
                demonInfo.Name = SurroundWithQuotes(demonCount[i].ChildNodes[1].InnerText.Replace("\n", "").Trim());
                demonInfo.BestArchetypePvE = CreateArchetypeFrom(demonCount[i].ChildNodes[3].InnerHtml);
                demonInfo.BestArchetypePvP = CreateArchetypeFrom(demonCount[i].ChildNodes[5].InnerHtml);
                demonInfo.PvEScore = Convert.ToDouble(demonCount[i].ChildNodes[7].InnerText);
                demonInfo.PvPOffenseScore = Convert.ToDouble(demonCount[i].ChildNodes[9].InnerText);
                demonInfo.PvPDefScore = Convert.ToDouble(demonCount[i].ChildNodes[11].InnerText);
                demonInfo.Pros = SurroundWithQuotes(demonCount[i + 1].InnerText.Replace("Pros", "").Trim());
                demonInfo.Cons = SurroundWithQuotes(demonCount[i + 2].InnerText.Replace("Cons", "").Trim());

                //Add Demon
                if (demonInfo.Name != "")
                    demonList.Add(demonInfo);
            }

            return demonList;
        }

        static string SurroundWithQuotes(string text)
        {
            return "\"" + text + "\"";
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

            return myArcheType;
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

