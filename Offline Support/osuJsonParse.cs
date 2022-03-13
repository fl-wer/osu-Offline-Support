using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Offline_Support
{
    public class BeatmapScore
    {
        // all of the variables of beatmap scores json
        public string score_id, score, username, maxcombo, count50, count100, count300, countmiss,
        countkatu, countgeki, accuracy, perfect, enabled_mods, user_id, date, rank, pp, replay_available;
    }

    public class osuJsonParse
    {
        // function that returns array of BeatmapScore type objects (above)
        public BeatmapScore[] parseBeatmapScores(string rawJson)
        {
            // amount of scores in provided json string, for creating array
            // each score's first value is score_id
            int scoreCount = Regex.Matches(rawJson, "score_id").Count;

            // creating array with size based on found scores from raw json
            BeatmapScore[] parsedScores = new BeatmapScore[scoreCount];

            // cleaning json by removing square brackets
            rawJson = rawJson.Replace("[", "").Replace("]", "");

            // initialize array
            for (int i = 0; i < parsedScores.Length; i++)
                parsedScores[i] = new BeatmapScore();

            // parsing through json file like usain bolt
            for (int i = 0; i < parsedScores.Length; i++)
            {
                parsedScores[i].score_id = rawJson.Remove(0, rawJson.IndexOf("score_id\":\"") + 11);
                parsedScores[i].score_id = parsedScores[i].score_id.Remove(parsedScores[i].score_id.IndexOf("\""));

                parsedScores[i].score = rawJson.Remove(0, rawJson.IndexOf("score\":\"") + 8);
                parsedScores[i].score = parsedScores[i].score.Remove(parsedScores[i].score.IndexOf("\""));

                int tempScore; int.TryParse(parsedScores[i].score, out tempScore);
                parsedScores[i].score = "Score: " + string.Format("{0:n0}", tempScore);

                parsedScores[i].username = rawJson.Remove(0, rawJson.IndexOf("username\":\"") + 11);
                parsedScores[i].username = parsedScores[i].username.Remove(parsedScores[i].username.IndexOf("\""));

                parsedScores[i].maxcombo = rawJson.Remove(0, rawJson.IndexOf("maxcombo\":\"") + 11);
                parsedScores[i].maxcombo = parsedScores[i].maxcombo.Remove(parsedScores[i].maxcombo.IndexOf("\""));

                int tempMaxcombo; int.TryParse(parsedScores[i].maxcombo, out tempMaxcombo);
                parsedScores[i].maxcombo = string.Format("{0:n0}", tempMaxcombo);

                parsedScores[i].score = parsedScores[i].score + " " + "(" + parsedScores[i].maxcombo + "x)";

                parsedScores[i].count50 = rawJson.Remove(0, rawJson.IndexOf("count50\":\"") + 10);
                parsedScores[i].count50 = parsedScores[i].count50.Remove(parsedScores[i].count50.IndexOf("\""));

                parsedScores[i].count100 = rawJson.Remove(0, rawJson.IndexOf("count100\":\"") + 11);
                parsedScores[i].count100 = parsedScores[i].count100.Remove(parsedScores[i].count100.IndexOf("\""));

                parsedScores[i].count300 = rawJson.Remove(0, rawJson.IndexOf("count300\":\"") + 11);
                parsedScores[i].count300 = parsedScores[i].count300.Remove(parsedScores[i].count300.IndexOf("\""));

                parsedScores[i].countmiss = rawJson.Remove(0, rawJson.IndexOf("countmiss\":\"") + 12);
                parsedScores[i].countmiss = parsedScores[i].countmiss.Remove(parsedScores[i].countmiss.IndexOf("\""));

                parsedScores[i].countkatu = rawJson.Remove(0, rawJson.IndexOf("countkatu\":\"") + 12);
                parsedScores[i].countkatu = parsedScores[i].countkatu.Remove(parsedScores[i].countkatu.IndexOf("\""));

                parsedScores[i].countgeki = rawJson.Remove(0, rawJson.IndexOf("countgeki\":\"") + 12);
                parsedScores[i].countgeki = parsedScores[i].countgeki.Remove(parsedScores[i].countgeki.IndexOf("\""));

                parsedScores[i].accuracy = calculateAccuracy(Convert.ToInt32(parsedScores[i].count50),
                Convert.ToInt32(parsedScores[i].count100), Convert.ToInt32(parsedScores[i].count300),
                Convert.ToInt32(parsedScores[i].countmiss)).ToString();

                if (parsedScores[i].accuracy.Length == 4) parsedScores[i].accuracy += "0%";
                else if (parsedScores[i].accuracy.Length == 3) parsedScores[i].accuracy += ".00%";
                else if (parsedScores[i].accuracy.Length == 2) parsedScores[i].accuracy += ".00%";
                else parsedScores[i].accuracy += "%";

                parsedScores[i].perfect = rawJson.Remove(0, rawJson.IndexOf("perfect\":\"") + 10);
                parsedScores[i].perfect = parsedScores[i].perfect.Remove(parsedScores[i].perfect.IndexOf("\""));

                parsedScores[i].enabled_mods = rawJson.Remove(0, rawJson.IndexOf("enabled_mods\":\"") + 15);
                parsedScores[i].enabled_mods = parsedScores[i].enabled_mods.Remove(parsedScores[i].enabled_mods.IndexOf("\""));

                string convertedMods = "";
                if (parsedScores[i].enabled_mods == "0") convertedMods = "";
                else
                {
                    int modsNum = 0; int.TryParse(parsedScores[i].enabled_mods, out modsNum);

                    Mods selectedMods = (Mods)modsNum;
                    var individualMods = Enum.GetValues(typeof(Mods)).Cast<Mods>().Where
                    (mod => selectedMods.HasFlag(mod) && mod != Mods.None).ToList();

                    foreach (var singleMod in individualMods)
                        convertedMods += singleMod.ToString() + ",";

                    // removing comma at the end
                    convertedMods = convertedMods.Remove(convertedMods.Length - 1, 1);
                }

                convertedMods = convertedMods.Replace("DT,NC", "NC");
                parsedScores[i].enabled_mods = convertedMods;

                parsedScores[i].user_id = rawJson.Remove(0, rawJson.IndexOf("user_id\":\"") + 10);
                parsedScores[i].user_id = parsedScores[i].user_id.Remove(parsedScores[i].user_id.IndexOf("\""));

                parsedScores[i].date = rawJson.Remove(0, rawJson.IndexOf("date\":\"") + 7);
                parsedScores[i].date = parsedScores[i].date.Remove(parsedScores[i].date.IndexOf("\""));

                parsedScores[i].rank = rawJson.Remove(0, rawJson.IndexOf("rank\":\"") + 7);
                parsedScores[i].rank = parsedScores[i].rank.Remove(parsedScores[i].rank.IndexOf("\""));

                parsedScores[i].pp = rawJson.Remove(0, rawJson.IndexOf("pp\":") + 5);
                if (parsedScores[i].pp.Remove(3) == "ull") parsedScores[i].pp = "0";
                else parsedScores[i].pp = parsedScores[i].pp.Remove(parsedScores[i].pp.IndexOf("\""));

                if (parsedScores[i].pp.Contains(".")) parsedScores[i].pp = parsedScores[i].pp.Remove(parsedScores[i].pp.IndexOf("."));
                parsedScores[i].pp += "pp";

                parsedScores[i].replay_available = rawJson.Remove(0, rawJson.IndexOf("replay_available\":\"") + 19);
                parsedScores[i].replay_available = parsedScores[i].replay_available.Remove(parsedScores[i].replay_available.IndexOf("\""));

                // remove score from raw file so it could go to the next one after parsing all values from previous score
                rawJson = rawJson.Remove(0, rawJson.IndexOf("\"}") + 2);
            }

            // return array of scores with all variables inside
            return parsedScores;
        }

        private double calculateAccuracy(int c50, int c100, int c300, int misses)
        {
            // score player gained
            int gainedScore = (c50 * 50) + (c100 * 100) + (c300 * 300);

            // maximum possible score
            int maxScore = (c50 * 300) + (c100 * 300) + (c300 * 300) + (misses * 300);

            // then we calculate % based of two above
            double accuracy = ((float)gainedScore / (float)maxScore) * 100f;

            // this is to shrink digits after dot/comma to 2
            return Math.Round(accuracy * 100) / 100.0;
        }

        enum Mods
        {
            None = 0, NF = 1, EZ = 2, TD = 4, HD = 8, HR = 16, SD = 32, DT = 64,
            RL = 128, HT = 256, NC = 512, FL = 1024, AT = 2048, SO = 4096, AP = 8192,
            PF = 16384, Key4 = 32768, Key5 = 65536, Key6 = 131072, Key7 = 262144,
            Key8 = 524288, FI = 1048576, RD = 2097152, CM = 4194304, TP = 8388608,
            Key9 = 16777216, KeyCoop = 33554432, Key1 = 67108864, Key3 = 134217728,
            Key2 = 268435456, SV2 = 536870912, MR = 1073741824,
            KeyMod = Key1 | Key2 | Key3 | Key4 | Key5 | Key6 | Key7 | Key8 | Key9 | KeyCoop,
            FreeModAllowed = NF | EZ | HD | HR | SD | FL | FI | RL | AP | SO | KeyMod,
            ScoreIncreaseMods = HD | HR | DT | FL | FI
        }
    }
}
