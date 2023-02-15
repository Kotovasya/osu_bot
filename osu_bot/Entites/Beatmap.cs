using Newtonsoft.Json.Linq;
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
               
                Attributes.ParseBeatmapAttributes(json, Mods.NM);
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

        public void ParseDifficultyAttributesJson(JToken json)
        {
            if (json != null)
            {
                json = json["attributes"];

                if (json["star_rating"] != null)
                    Stars = json["star_rating"].Value<float>();

                if (json["max_combo"] != null)
                    MaxCombo = json["max_combo"].Value<int>();

                if (json["aim_difficulty"] != null)
                    AimDifficulty = json["aim_difficulty"].Value<float>();

                if (json["speed_difficulty"] != null)
                    SpeedDifficulty = json["speed_difficulty"].Value<float>();

                if (json["speed_note_count"] != null)
                    SpeedNoteCount = json["speed_note_count"].Value<float>();

                if (json["flashlight_difficulty"] != null)
                    FlashlightDifficulty = json["flashlight_difficulty"].Value<float>();

                if (json["slider_factor"] != null)
                    SliderFactor = json["slider_factor"].Value<float>();
            }
        }

        public void ParseBeatmapAttributes(JToken json, Mods mods)
        {
            if (json["difficulty_rating"] != null)
                Stars = json["difficulty_rating"].Value<float>();

            if (json["cs"] != null)
                CS = json["cs"].Value<float>();

            if (json["ar"] != null)
                AR = json["ar"].Value<float>();

            if (json["accuracy"] != null)
                OD = json["accuracy"].Value<float>();

            if (json["drain"] != null)
                HP = json["drain"].Value<float>();

            if (json["bpm"] != null)
                BPM = json["bpm"].Value<float>();

            if (json["total_length"] != null)
                Length = json["total_length"].Value<int>();

            if (json["count_circles"] != null)
                CircleCount = json["count_circles"].Value<int>();

            if (json["count_sliders"] != null)
                SliderCount = json["count_sliders"].Value<int>();

            if (json["count_spinners"] != null)
                SpinnerCount = json["count_spinners"].Value<int>();

            this.CalculateAttributesWithMods(mods);
        }

        public void CalculateAttributesWithMods(Mods mods)
        {
            float ratio;
            if (mods.HasFlag(Mods.HR))
            {
                ratio = 1.4f;
                CS = Math.Min(CS * 1.3f, 10.0f);
                AR = Math.Min(AR * ratio, 10.0f);
                OD = Math.Min(OD * ratio, 10.0f);
                HP = Math.Min(HP * ratio, 10.0f);
            }
            if (mods.HasFlag(Mods.EZ))
            {
                ratio = 0.5f;
                CS *= ratio;
                AR *= ratio;
                OD *= ratio;
                HP *= ratio;
            }
            if (mods.HasFlag(Mods.DT) || mods.HasFlag(Mods.NC))
            {
                AR = Math.Min((AR * 2 + 13) / 3, 11.0f);
                OD = Math.Min((OD * 2 + 13) / 3, 11.0f);
                Length = (int)Math.Round(Length * 0.5f);
                BPM = (int)Math.Round(BPM * 1.5f);
            }
            else if (mods.HasFlag(Mods.HT))
            {
                AR = CalculateAdjustAttribute(AR, 0.75f);
                OD = CalculateAdjustAttribute(OD, 0.75f);
                Length = (int)Math.Round(Length * 1.5f);
                BPM = (int)Math.Round(BPM * 0.75f);
            }
        }

        /// <summary>
        /// Method for calculate AR and OD attributes
        /// </summary>
        /// <param name="attribute">Ar or OD attribute</param>
        /// <param name="coefficient">For HalfTime = 0.75, DoubleTime = 1.5</param>
        /// <param name="mods"></param>
        /// <returns></returns>
        private static float CalculateAdjustAttribute(float attribute, float coefficient)
        {
            float ms;
            if (attribute > 5)
                ms = 1200 + (450 - 1200) * (attribute - 5) / coefficient;
            else if (attribute < 5)
                ms = 1200 - (1200 - 1800) * (5 - attribute) / coefficient;
            else
                ms = 1200;

            if (ms > 1200)
                return (1800 - ms) / 120;
            else
                return (1200 - ms) / 150 + 5;
        }

        public Mods Mods { get; set; }

        public float Stars { get; set; }
        public int MaxCombo { get; set; }
        public float BPM { get; set; }
        public float CS { get; set; }
        public float AR { get; set; }
        public float OD { get; set; }
        public float HP { get; set; }
        public int Length { get; set; }

        public float AimDifficulty { get; set; }
        public float SpeedDifficulty { get; set; }
        public float SpeedNoteCount { get; set; }
        public float FlashlightDifficulty { get; set; }
        public float SliderFactor { get; set; }
        public int CircleCount { get; set; }
        public int SliderCount { get; set; }
        public int SpinnerCount { get; set; }

        public int TotalObjects => CircleCount + SliderCount + SpinnerCount;
    }
}
