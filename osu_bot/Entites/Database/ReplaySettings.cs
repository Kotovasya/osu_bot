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
        public int GlobalVolume { get; set; }

        public int MusicVolume { get; set; }

        public int HitsoundVolume { get; set; }

        public bool ShowHitErrorMeter { get; set; }

        public bool ShowUnstableRate { get; set; }

        public bool ShowScore { get; set; }

        public bool ShowHPBar { get; set; }

        public bool ShowComboCounter { get; set; }

        public bool ShowPPCounter { get; set; }

        public bool ShowScoreboard { get; set; }

        public bool ShowBorders { get; set; }

        public bool ShowMods { get; set; }

        public bool ShowResultScreen { get; set; }

        public ReplaySkin Skin { get; set; }

        public bool UseSkinColors { get; set; }

        public bool UseSkinHitsounds { get; set; }

        public bool UseBeatmapColors { get; set; }

        public bool CursorScaleToCS { get; set; }

        public bool CursorRainbow { get; set; }

        public bool CursorTrailGlow { get; set; }

        public bool DrawFollowPoints { get; set; }

        public bool ScaleToTheBeat { get; set; }

        public bool SliderMerge { get; set; }

        public bool ObjectsRainbow { get; set; }

        public bool ObjectsFlashToTheBeat { get; set; }

        public bool UseHitCircleColor { get; set; }

        public bool SeizureWarning { get; set; }

        public bool LoadStoryBoard { get; set; }

        public bool LoadVideo { get;  set; }

        public int IntroBGDim { get; set; }

        public int InGameBGDim { get; set; }

        public int BreakBGDim { get; set; }

        public bool BGParallax { get; set; }

        public bool ShowDancerLogo { get; set; }

        public bool Skip { get; set; }

        public bool CursorRipples { get; set; }

        public int CursorSize { get; set; }

        public bool CursorTrail { get; set; }

        public bool DrawComboNumbers { get; set; }

        public bool SliderShakingIn { get; set; }

        public bool SliderShakingOut { get; set; }

        public bool ShowHitCounter { get; set; }

        public bool ShowKeyOverlay { get; set; }

        public bool ShowAvatarsOnScoreboard { get; set; }

        public bool ShowAimErrorMeter { get; set; }

        public bool PlayNightcoreSamples { get; set; }
    }
}
