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
        private OsuBinaryReader _reader;

        public async Task<OsuReplay> FromString(string data, bool minimalLoad = true)
        {
            using MemoryStream stream = new();
            using StreamWriter writer = new(stream);
            await writer.WriteAsync(data);
            writer.Flush();
            stream.Position = 0;

            _reader = new OsuBinaryReader(stream);
            OsuReplay replay = new();

            replay.PlayMode = (PlayMode)_reader.ReadByte();
            replay.Version = _reader.ReadInt32();
            replay.MapHash = _reader.ReadString();
            replay.PlayerName = _reader.ReadString();
            replay.ReplayHash = _reader.ReadString();
            replay.Count300 = _reader.ReadInt16();
            replay.Count100 = _reader.ReadInt16();
            replay.Count50 = _reader.ReadInt16();
            replay.Geki = _reader.ReadInt16();
            replay.Katu = _reader.ReadInt16();
            replay.Miss = _reader.ReadInt16();
            replay.TotalScore = _reader.ReadInt32();
            replay.MaxCombo = _reader.ReadInt16();
            replay.IsFullCombo = _reader.ReadBoolean();
            replay.Mods = _reader.ReadInt32();
            replay.LifeBarData = _reader.ReadString();
            replay.DateTicks = _reader.ReadInt64();
            replay.Date = GetDate(replay.DateTicks);
            replay.CompressedReplayLength = _reader.ReadInt32();

            if (replay.CompressedReplayLength > 0)
            {
                if (minimalLoad)
                    _reader.ReadBytes(replay.CompressedReplayLength);
                else
                    replay.CompressedReplay = _reader.ReadBytes(replay.CompressedReplayLength);
            }

            if (replay.Version >= 20140721)
                replay.OnlineScoreId = _reader.ReadInt64();
            else if (replay.Version >= 20121008)
                replay.OnlineScoreId = _reader.ReadInt32();

            return replay;
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
            catch (Exception e)
            {
                return new DateTime();
            }
        }
    }
}
