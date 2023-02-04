﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace osu_bot.Entites
{
    public class Beatmap
    {
        public Beatmap()
        {

        }

        public void ParseBeatmapJson(JsonObject json)
        {
            if (json != null)
            {
                if (json.TryGetPropertyValue("id", out JsonNode? node))
                    Id = node.GetValue<long>();

                if (json.TryGetPropertyValue("version", out node))
                    DifficultyName = node.GetValue<string>();

                if (json.TryGetPropertyValue("url", out node))
                    Url = node.GetValue<string>();

                Attributes ??= new();

                if (json.TryGetPropertyValue("difficulty_rating", out node))
                    Attributes.Stars = node.GetValue<double>();

                if (json.TryGetPropertyValue("cs", out node))
                    Attributes.CS = node.GetValue<double>();

                if (json.TryGetPropertyValue("ar", out node))
                    Attributes.AR = node.GetValue<double>();

                if (json.TryGetPropertyValue("accuracy", out node))
                    Attributes.OD = node.GetValue<double>();

                if (json.TryGetPropertyValue("drain", out node))
                    Attributes.HP = node.GetValue<double>();

                if (json.TryGetPropertyValue("bpm", out node))
                    Attributes.BPM = node.GetValue<int>();

                if (json.TryGetPropertyValue("total_length", out node))
                    Attributes.Length = node.GetValue<int>();

                if (json.TryGetPropertyValue("count_circles", out node))
                    Attributes.CircleCount = node.GetValue<int>();

                if (json.TryGetPropertyValue("count_sliders", out node))
                    Attributes.SliderCount = node.GetValue<int>();

                if (json.TryGetPropertyValue("count_spinners", out node))
                    Attributes.SpinnerCount = node.GetValue<int>();
            }
        }

        public void ParseBeatmapsetJson(JsonObject json)
        {
            if (json != null)
            {
                if (json.TryGetPropertyValue("title", out JsonNode? node))
                    Title = node.GetValue<string>();

                if (json.TryGetPropertyValue("covers", out node))
                    CoverUrl = node["cover@2x"].GetValue<string>();

                if (json.TryGetPropertyValue("status", out node))
                    Status = node.GetValue<string>();

                if (json.TryGetPropertyValue("artist", out node))
                    Artist = node.GetValue<string>();

                Mapper ??= new();

                if (json.TryGetPropertyValue("creator", out node))
                    Mapper.Name = node.GetValue<string>();
            }
        }

        public long Id { get; set; }
        public string Title { get; set; }
        public string DifficultyName { get; set; }
        public string CoverUrl { get; set; }
        public string Status { get; set; }
        public string Artist { get; set; }
        public string Url { get; set; }
        public User Mapper { get; set; }
        public BeatmapAttributes Attributes { get; set; }
    }

    public class BeatmapAttributes
    {
        public BeatmapAttributes()
        {

        }

        public void ParseAttributesJson(JsonObject json)
        {
            if (json != null)
            {
                if (json.TryGetPropertyValue("star_rating", out JsonNode? node))
                    Stars = node.GetValue<long>();

                if (json.TryGetPropertyValue("max_combo", out node))
                    MaxCombo = node.GetValue<int>();

                if (json.TryGetPropertyValue("aim_difficulty", out node))
                    AimDifficulty = node.GetValue<double>();

                if (json.TryGetPropertyValue("speed_difficulty", out node))
                    SpeedDifficulty = node.GetValue<double>();

                if (json.TryGetPropertyValue("speed_note_count", out node))
                    SpeedNoteCount = node.GetValue<double>();

                if (json.TryGetPropertyValue("flashlight_difficulty", out node))
                    FlashlightDifficulty = node.GetValue<double>();

                if (json.TryGetPropertyValue("slider_factor", out node))
                    SliderFactor = node.GetValue<double>();
            }
        }

        /*
 * {
    "attributes": {
        "star_rating": 4.956999778747559,
        "max_combo": 447,
        "aim_difficulty": 2.5800299644470215,
        "speed_difficulty": 2.1105499267578125,
        "speed_note_count": 120.0469970703125,
        "flashlight_difficulty": 0,
        "slider_factor": 0.9993990063667297,
        "approach_rate": 9,
        "overall_difficulty": 8.5
    }
}
 */

        public double Stars { get; set; }
        public int MaxCombo { get; set; }
        public int BPM { get; set; }
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
    }
}
