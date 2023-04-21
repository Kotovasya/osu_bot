// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_bot.Entites.Database
{
    public class ReplaySettings
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int GlobalVolume { get; set; } = 50;

        public int MusicVolume { get; set; } = 50;

        public int HitsoundVolume { get; set; } = 50;

        public bool ShowHitErrorMeter { get; set; } = true;

        public bool ShowUnstableRate { get; set; } = true;

        public bool ShowScore { get; set; } = true;

        public bool ShowHPBar { get; set; } = true;

        public bool ShowComboCounter { get; set; } = true;

        public bool ShowPPCounter { get; set; } = true;

        public bool ShowScoreboard { get; set; } = false;

        public bool ShowBorders { get; set; } = false;

        public bool ShowMods { get; set; } = true;

        public bool ShowResultScreen { get; set; } = true;

        public ReplaySkin Skin { get; set; }

        public bool UseSkinCursor { get; set; } = true;

        public bool UseSkinColors { get; set; } = false;

        public bool UseSkinHitsounds { get; set; } = true;

        public bool UseBeatmapColors { get; set; } = true;

        public bool CursorScaleToCS { get; set; } = false;

        public bool CursorRainbow { get; set; } = false;

        public bool CursorTrailGlow { get; set; } = false;

        public bool DrawFollowPoints { get; set; } = true;

        public bool ScaleToTheBeat { get; set; } = false;

        public bool SliderMerge { get; set; } = false;

        public bool ObjectsRainbow { get; set; } = false;

        public bool ObjectsFlashToTheBeat { get; set; } = false;

        public bool UseHitCircleColor { get; set; } = true;

        public bool SeizureWarning { get; set; } = false;

        public bool LoadStoryBoard { get; set; } = true;

        public int IntroBGDim { get; set; } = 0;

        public int InGameBGDim { get; set; } = 75;

        public int BreakBGDim { get; set; } = 30;

        public bool BGParallax { get; set; } = false;

        public bool ShowDancerLogo { get; set; } = true;

        public bool Skip { get; set; } = true;

        public bool CursorRipples { get; set; } = false;

        public int CursorSize { get; set; } = 1;

        public bool CursorTrail { get; set; } = true;

        public bool DrawComboNumbers { get; set; } = true;

        public bool SliderShakingIn { get; set; } = true;

        public bool SliderShakingOut { get; set; } = true;

        public bool ShowHitCounter { get; set; } = false;

        public bool ShowKeyOverlay { get; set; } = true;

        public bool ShowAvatarsOnScoreboard { get; set; } = false;

        public bool ShowAimErrorMeter { get; set; } = true;

        public bool PlayNightcoreSamples { get; set; } = true;

        public TelegramUser Owner { get; set; }
    }
}
