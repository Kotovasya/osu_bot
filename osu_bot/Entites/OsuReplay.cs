// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu_bot.API;
using osu_bot.Entites.Enums;

namespace osu_bot.Entites
{
    public class OsuReplay
    {
        private readonly OsuService _service = OsuService.Instance;

        public PlayMode PlayMode { get; set; }
        public int Version { get; set; }
        public string MapHash { get; set; }
        public string PlayerName { get; set; }
        public string ReplayHash { get; set; }
        public short Count300 { get; set; }
        public short Count100 { get; set; }
        public short Count50 { get; set; }
        public short Geki { get; set; }
        public short Katu { get; set; }
        public short CountMisses { get; set; }
        public int TotalScore { get; set; }
        public short MaxCombo { get; set; }
        public bool IsFullCombo { get; set; }
        public int Mods { get; set; }
        public double AdditionalMods { get; set; }
        public string LifeBarData { get; set; }
        public DateTime Date { get; set; }
        public long DateTicks { get; set; }
        public int CompressedReplayLength { get; set; }
        public byte[] CompressedReplay { get; set; }
        public long OnlineScoreId { get; set; } = -1;

        public async Task<OsuScore?> ToScore()
        {
            OsuScore? score = null;
            if (OnlineScoreId != -1)
                score = await _service.GetScoreAsync(OnlineScoreId);

            if (score is not null)
                return score;

            score = new OsuScore();
            score.Id = OnlineScoreId;
            score.Mods = Mods;
            score.Score = TotalScore;
            score.Count300 = Count300;
            score.Count100 = Count100;
            score.Count50 = Count50;
            score.CountMisses = Count

            OsuBeatmap? beatmap = await _service.GetBeatmapAsync(MapHash);
            if (beatmap is not null)
            {
                score.Beatmap = beatmap;
                score.Beatmapset = beatmap.Beatmapset;
                OsuBeatmapAttributes? attributes = await _service.GetBeatmapAttributesAsync(beatmap, Mods);
                if (attributes is not null)
                    score.BeatmapAttributes = attributes;
            }
            
            if (LifeBarData != null)
            {
                int indexLastHPInfo = LifeBarData.LastIndexOf("/");
                float hp = float.Parse(LifeBarData[indexLastHPInfo..]);
                if (hp > 0.0)
                    score.IsPassed = true;
                else
                    score.IsPassed = false;
            }
            else
                score.IsPassed = false;

            return score;
        }
    }
}
