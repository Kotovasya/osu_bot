using Newtonsoft.Json.Linq;
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
               
                Attributes.ParseBeatmapAttributes(json);
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

        public void ParseDifficultyAttributesJson(JToken json, Mods mods)
        {
            if (json != null)
            {
                Mods = mods;

                if (json["star_rating"] != null)
                    Stars = json["star_rating"].Value<long>();

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

        public void ParseBeatmapAttributes(JToken json)
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
    }
}
