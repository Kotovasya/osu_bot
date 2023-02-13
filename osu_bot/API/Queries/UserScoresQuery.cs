﻿using Newtonsoft.Json.Linq;
using osu_bot.API.Parameters;
using osu_bot.Bot;
using osu_bot.Entites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace osu_bot.API.Queries
{
    public class UserScoresQuery : Query<List<ScoreInfo>>
    {
        public UserScoreQueryParameters Parameters { get; private set; }
 
        public override string UrlParameter => Parameters.GetQueryString();

        private readonly BeatmapAttributesQuery beatmapAttributesQuery = new();

        public UserScoresQuery(UserScoreQueryParameters parameters)
        {
            Parameters = parameters;
        }

        public override async Task<List<ScoreInfo>> ExecuteAsync(OsuAPI api)
        {
            var userInfo = await api.GetUserInfoByUsernameAsync(Parameters.Username);
            Parameters.UserId = userInfo.Id;

            var jsonScores = await api.GetJsonArrayAsync(UrlParameter);
            List<ScoreInfo> scores = new();
            foreach(var jsonScore in jsonScores)
            {
                ScoreInfo score = new();
                score.ParseScoreJson(jsonScore);
                scores.Add(score);
            }            
            var resultScores = scores
                .Where(s => s.Mods == Parameters.Mods || Parameters.Mods == Mods.ALL)
                .Take(Parameters.Limit)             
                .ToList();
            foreach (var score in resultScores)
            {
                beatmapAttributesQuery.Parameters.Mods = score.Mods;
                beatmapAttributesQuery.Parameters.BeatmapId = score.Beatmap.Id;
                score.User = userInfo;
                score.Beatmap.Attributes.ParseDifficultyAttributesJson(await beatmapAttributesQuery.GetJson(api), score.Mods);
            }
            return resultScores;
        }
        /*
         * {
        "accuracy": 0.9835390946502057,
        "best_id": 4339052104,
        "created_at": "2022-12-18T12:36:27Z",
        "id": 4339052104,
        "max_combo": 251,
        "mode": "osu",
        "mode_int": 0,
        "mods": [
            "DT",
            "HD"
        ],
        "passed": true,
        "perfect": true,
        "pp": 269.671,
        "rank": "S",
        "replay": false,
        "score": 1188308,
        "statistics": {
            "count_100": 4,
            "count_300": 158,
            "count_50": 0,
            "count_geki": 34,
            "count_katu": 3,
            "count_miss": 0
        },
        "type": "score_best_osu",
        "user_id": 15833700,
        "current_user_attributes": {
            "pin": null
        },
        "beatmap": {
            "beatmapset_id": 968171,
            "difficulty_rating": 4.59,
            "id": 2025941,
            "mode": "osu",
            "status": "ranked",
            "total_length": 54,
            "user_id": 4378277,
            "version": "Insane",
            "accuracy": 8.2,
            "ar": 9,
            "bpm": 186,
            "convert": false,
            "count_circles": 83,
            "count_sliders": 78,
            "count_spinners": 1,
            "cs": 3.6,
            "deleted_at": null,
            "drain": 4.8,
            "hit_length": 53,
            "is_scoreable": true,
            "last_updated": "2019-05-08T19:29:07Z",
            "mode_int": 0,
            "passcount": 2124648,
            "playcount": 7020198,
            "ranked": 1,
            "url": "https://osu.ppy.sh/beatmaps/2025941",
            "checksum": "9b385e6de94ccdc36a4866ec4238aa1f"
        },
        "beatmapset": {
            "artist": "MIMI feat. Hatsune Miku",
            "artist_unicode": "MIMI feat. 初音ミク",
            "covers": {
                "cover": "https://assets.ppy.sh/beatmaps/968171/covers/cover.jpg?1645788271",
                "cover@2x": "https://assets.ppy.sh/beatmaps/968171/covers/cover@2x.jpg?1645788271",
                "card": "https://assets.ppy.sh/beatmaps/968171/covers/card.jpg?1645788271",
                "card@2x": "https://assets.ppy.sh/beatmaps/968171/covers/card@2x.jpg?1645788271",
                "list": "https://assets.ppy.sh/beatmaps/968171/covers/list.jpg?1645788271",
                "list@2x": "https://assets.ppy.sh/beatmaps/968171/covers/list@2x.jpg?1645788271",
                "slimcover": "https://assets.ppy.sh/beatmaps/968171/covers/slimcover.jpg?1645788271",
                "slimcover@2x": "https://assets.ppy.sh/beatmaps/968171/covers/slimcover@2x.jpg?1645788271"
            },
            "creator": "Log Off Now",
            "favourite_count": 7458,
            "hype": null,
            "id": 968171,
            "nsfw": false,
            "offset": 0,
            "play_count": 34264674,
            "preview_url": "//b.ppy.sh/preview/968171.mp3",
            "source": "",
            "spotlight": false,
            "status": "ranked",
            "title": "Mizuoto to Curtain",
            "title_unicode": "水音とカーテン",
            "track_id": 2081,
            "user_id": 4378277,
            "video": false
        },
        "user": {
            "avatar_url": "https://a.ppy.sh/15833700?1673169745.jpeg",
            "country_code": "BY",
            "default_group": "default",
            "id": 15833700,
            "is_active": true,
            "is_bot": false,
            "is_deleted": false,
            "is_online": false,
            "is_supporter": false,
            "last_visit": "2023-01-19T17:22:23+00:00",
            "pm_friends_only": false,
            "profile_colour": null,
            "username": "Kotovasya"
        },
        "weight": {
            "percentage": 34.05616262881148,
            "pp": 91.8395943227422
        }
    },
         */
    }
}