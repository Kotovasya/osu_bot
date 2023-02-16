using Newtonsoft.Json.Linq;
using osu_bot.Entites.Mods;
using osu_bot.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace osu_bot.Entites
{
    public class Beatmap
    {
        public Beatmap()
        {
            Attributes = new BeatmapAttributes();
        }


        public void ParseBeatmapJson(JToken json)
        {
            if (json != null)
            {
                if (json["id"] != null)
                    Id = json["id"].Value<long>();

                if (json["version"] != null)
                    DifficultyName = json["version"].Value<string>();

                if (json["url"] != null)
                    Url = json["url"].Value<string>();
               
                Attributes.ParseBeatmapAttributesJson(json);
            }
        }

        public void ParseBeatmapsetJson(JToken json)
        {
            if (json != null)
            {
                if (json["title"] != null)
                    Title = json["title"].Value<string>();

                if (json["covers"] != null)
                    CoverUrl = json["covers"]["cover@2x"].Value<string>();

                if (json["status"] != null)
                    Status = json["status"].Value<string>();

                if (json["artist"] != null)
                    Artist = json["artist"].Value<string>();

                if (json["creator"] != null)
                    MapperName = json["creator"].Value<string>();
            }
        }

        public long Id { get; set; }
        public string Title { get; set; }
        public string DifficultyName { get; set; }
        public string CoverUrl { get; set; }
        public string Status { get; set; }
        public string Artist { get; set; }
        public string Url { get; set; }
        public string MapperName { get; set; }
        public BeatmapAttributes Attributes { get; set; }
    }

    public class BeatmapAttributes
    {
        public BeatmapAttributes()
        {

        }

        public double Stars { get; set; }
        public int MaxCombo { get; set; }
        public double BPM { get; set; }

        public double CS { get; set; }
        public double AR { get; set; }
        public double OD { get; set; }
        public double HP { get; set; }

        public int Length { get; set; }

        public double AimDifficulty { get; set; }
        public double SpeedDifficulty { get; set; }
        public double SpeedNoteCount { get; set; }
        public double FlashlightDifficulty { get; set; }
        public double SliderFactor { get; set; }
        public int CircleCount { get; set; }
        public int SliderCount { get; set; }
        public int SpinnerCount { get; set; }

        public int TotalObjects => CircleCount + SliderCount + SpinnerCount;

        public void ParseDifficultyAttributesJson(JToken json)
        {
            if (json != null)
            {
                json = json["attributes"];

                if (json["star_rating"] != null)
                    Stars = json["star_rating"].Value<double>();

                if (json["max_combo"] != null)
                    MaxCombo = json["max_combo"].Value<int>();

                if (json["aim_difficulty"] != null)
                    AimDifficulty = json["aim_difficulty"].Value<double>();

                if (json["speed_difficulty"] != null)
                    SpeedDifficulty = json["speed_difficulty"].Value<double>();

                if (json["speed_note_count"] != null)
                    SpeedNoteCount = json["speed_note_count"].Value<double>();

                if (json["flashlight_difficulty"] != null)
                    FlashlightDifficulty = json["flashlight_difficulty"].Value<double>();

                if (json["slider_factor"] != null)
                    SliderFactor = json["slider_factor"].Value<double>();
            }
        }

        public void ParseBeatmapAttributesJson(JToken json, IEnumerable<Mod>? mods = null)
        {
            if (json["difficulty_rating"] != null)
                Stars = json["difficulty_rating"].Value<double>();

            if (json["cs"] != null)
                CS = json["cs"].Value<double>();

            if (json["ar"] != null)
                AR = json["ar"].Value<double>();

            if (json["accuracy"] != null)
                OD = json["accuracy"].Value<double>();

            if (json["drain"] != null)
                HP = json["drain"].Value<double>();

            if (json["bpm"] != null)
                BPM = json["bpm"].Value<double>();

            if (json["total_length"] != null)
                Length = json["total_length"].Value<int>();

            if (json["count_circles"] != null)
                CircleCount = json["count_circles"].Value<int>();

            if (json["count_sliders"] != null)
                SliderCount = json["count_sliders"].Value<int>();

            if (json["count_spinners"] != null)
                SpinnerCount = json["count_spinners"].Value<int>();

            if (mods != null)
                CalculateAttributesWithMods(mods);
        }

        public void CalculateAttributesWithMods(IEnumerable<Mod> mods)
        {
            if (!mods.Any())
                return;

            var applicableMods = mods.Where(m => m is IApplicableMod).Select(m => m as IApplicableMod);
            var firstApplicableMods = applicableMods.Where(m => m is ModHardRock || m is ModsEasy);
            
            foreach (var mod in firstApplicableMods)
                mod.ApplyToAttributes(this);

            applicableMods = applicableMods.Except(firstApplicableMods);
            foreach (var mod in applicableMods)
                mod.ApplyToAttributes(this);
        }

        public IEnumerable<Mod> Mods { get; set; }
    }
}
