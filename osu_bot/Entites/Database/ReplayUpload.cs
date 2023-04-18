// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;

namespace osu_bot.Entites.Database
{
    public class ReplayUpload
    {
        [BsonId]
        public string Hash { get; set; }

        public long? ScoreId { get; set; }

        public string? Url { get; set; }

        public ReplaySkin Skin { get; set; }

        public DateTime? UploadDate { get; set; }

        public ReplayUpload()
        { }

        public ReplayUpload(string hash)
        {
            Hash = hash;
        }

        public ReplayUpload(string hash, long? scoreId)
        {
            Hash = hash;
            ScoreId = scoreId;
        }
    }
}
