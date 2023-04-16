// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using osu_bot.Entites;
using osu_bot.Entites.Enums;
using osu_bot.Entites.Mods;
using SkiaSharp;

namespace osu_bot.Modules.OsuFiles
{
    public class ReplayReader
    {
        private static OsuBinaryReader s_reader;

        public static OsuReplay FromStream(Stream stream, bool minimalLoad = true)
        {
            s_reader = new OsuBinaryReader(stream);
            OsuReplay replay = new();

            replay.PlayMode = (PlayMode)s_reader.ReadByte();
            replay.Version = s_reader.ReadInt32();
            replay.MapHash = s_reader.ReadString();
            replay.PlayerName = s_reader.ReadString();
            replay.ReplayHash = s_reader.ReadString();
            replay.Count300 = s_reader.ReadInt16();
            replay.Count100 = s_reader.ReadInt16();
            replay.Count50 = s_reader.ReadInt16();
            replay.Geki = s_reader.ReadInt16();
            replay.Katu = s_reader.ReadInt16();
            replay.CountMisses = s_reader.ReadInt16();
            replay.TotalScore = s_reader.ReadInt32();
            replay.MaxCombo = s_reader.ReadInt16();
            replay.IsFullCombo = s_reader.ReadBoolean();
            replay.Mods = s_reader.ReadInt32();
            replay.LifeBarData = s_reader.ReadString();
            replay.DateTicks = s_reader.ReadInt64();
            replay.Date = GetDate(replay.DateTicks);
            replay.CompressedReplayLength = s_reader.ReadInt32();

            if (replay.CompressedReplayLength > 0)
            {
                if (minimalLoad)
                    s_reader.ReadBytes(replay.CompressedReplayLength);
                else
                    replay.CompressedReplay = s_reader.ReadBytes(replay.CompressedReplayLength);
            }

            if (replay.Version >= 20140721)
                replay.OnlineScoreId = s_reader.ReadInt64();
            else if (replay.Version >= 20121008)
                replay.OnlineScoreId = s_reader.ReadInt32();

            return replay;
        }

        public static async Task<OsuReplay> FromStringAsync(string data, bool minimalLoad = true)
        {
            using MemoryStream stream = new();
            using StreamWriter writer = new(stream);
            await writer.WriteAsync(data);
            writer.Flush();
            stream.Position = 0;

            return FromStream(stream, minimalLoad);
        }

        private static DateTime GetDate(long ticks)
        {
            if (ticks < 0L)
            {
                return new DateTime();
            }

            try
            {
                return new DateTime(ticks, DateTimeKind.Utc);
            }
            catch (Exception)
            {
                return new DateTime();
            }
        }
    }
}
