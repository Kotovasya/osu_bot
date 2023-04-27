// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using osu_bot.Modules;

namespace osu_bot.Entites.Database
{
    public class ReplaySettings
    {
        public const string PAGE_URL = "https://kotovasya.github.io/osu_bot/ReplaySettings.html";

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("globalVolume")]
        public int GlobalVolume { get; set; } = 50;

        [JsonProperty("musicVolume")]
        public int MusicVolume { get; set; } = 50;

        [JsonProperty("hitsoundVolume")]
        public int HitsoundVolume { get; set; } = 50;

        [JsonProperty("showHitErrorMeter")]
        public bool ShowHitErrorMeter { get; set; } = true;

        [JsonProperty("showUnstableRate")]
        public bool ShowUnstableRate { get; set; } = true;

        [JsonProperty("showScore")]
        public bool ShowScore { get; set; } = true;

        [JsonProperty("showHPBar")]
        public bool ShowHPBar { get; set; } = true;

        [JsonProperty("showComboCounter")]
        public bool ShowComboCounter { get; set; } = true;

        [JsonProperty("showPPCounter")]
        public bool ShowPPCounter { get; set; } = true;

        [JsonProperty("showScoreboard")]
        public bool ShowScoreboard { get; set; } = false;

        [JsonProperty("showBorders")]
        public bool ShowBorders { get; set; } = false;

        [JsonProperty("showMods")]
        public bool ShowMods { get; set; } = true;

        [JsonProperty("showResultScreen")]
        public bool ShowResultScreen { get; set; } = true;

        [JsonProperty("useSkinCursor")]
        public bool UseSkinCursor { get; set; } = true;

        [JsonProperty("useSkinColors")]
        public bool UseSkinColors { get; set; } = false;

        [JsonProperty("useSkinHitSounds")]
        public bool UseSkinHitSounds { get; set; } = true;

        [JsonProperty("useBeatmapColors")]
        public bool UseBeatmapColors { get; set; } = true;

        [JsonProperty("cursorScaleToCS")]
        public bool CursorScaleToCS { get; set; } = false;

        [JsonProperty("cursorRainbow")]
        public bool CursorRainbow { get; set; } = false;

        [JsonProperty("cursorTrailGlow")]
        public bool CursorTrailGlow { get; set; } = false;

        [JsonProperty("drawFollowPoints")]
        public bool DrawFollowPoints { get; set; } = true;

        [JsonProperty("scaleToTheBeat")]
        public bool ScaleToTheBeat { get; set; } = false;

        [JsonProperty("sliderMerge")]
        public bool SliderMerge { get; set; } = false;

        [JsonProperty("objectsRainbow")]
        public bool ObjectsRainbow { get; set; } = false;

        [JsonProperty("objectsFlashToTheBeat")]
        public bool ObjectsFlashToTheBeat { get; set; } = false;

        [JsonProperty("useHitCircleColor")]
        public bool UseHitCircleColor { get; set; } = true;

        [JsonProperty("seizureWarning")]
        public bool SeizureWarning { get; set; } = false;

        [JsonProperty("loadStoryBoard")]
        public bool LoadStoryBoard { get; set; } = true;

        [JsonProperty("introBGDim")]
        public int IntroBGDim { get; set; } = 0;

        [JsonProperty("inGameBGDim")]
        public int InGameBGDim { get; set; } = 75;

        [JsonProperty("breakBGDim")]
        public int BreakBGDim { get; set; } = 30;

        [JsonProperty("bgParallax")]
        public bool BgParallax { get; set; } = false;

        [JsonProperty("showDancerLogo")]
        public bool ShowDancerLogo { get; set; } = true;

        [JsonProperty("skip")]
        public bool Skip { get; set; } = true;

        [JsonProperty("cursorRipples")]
        public bool CursorRipples { get; set; } = false;

        [JsonProperty("cursorSize")]
        public int CursorSize { get; set; } = 1;

        [JsonProperty("cursorTrail")]
        public bool CursorTrail { get; set; } = true;

        [JsonProperty("drawComboNumbers")]
        public bool DrawComboNumbers { get; set; } = true;

        [JsonProperty("sliderShakingIn")]
        public bool SliderShakingIn { get; set; } = true;

        [JsonProperty("sliderShakingOut")]
        public bool SliderShakingOut { get; set; } = true;

        [JsonProperty("showHitCounter")]
        public bool ShowHitCounter { get; set; } = false;

        [JsonProperty("showKeyOverlay")]
        public bool ShowKeyOverlay { get; set; } = true;

        [JsonProperty("showAvatarsOnScoreboard")]
        public bool ShowAvatarsOnScoreboard { get; set; } = false;

        [JsonProperty("showAimErrorMeter")]
        public bool ShowAimErrorMeter { get; set; } = true;

        [JsonProperty("playNightcoreSamples")]
        public bool PlayNightcoreSamples { get; set; } = true;

        [JsonIgnore]
        public ReplaySkin Skin { get; set; }

        [JsonIgnore]
        public TelegramUser Owner { get; set; }

        public string GetWebPageString()
        {
            int skinId = Skin is null ? 0 : Skin.Id;
            StringBuilder stringBuilder = new($"{PAGE_URL}?");

            Type type = GetType();
            IEnumerable<PropertyInfo> properties =
                type.GetProperties().Where(p => p.GetCustomAttribute<JsonIgnoreAttribute>() is null);
            string requestName;
            string? requestValue;
            foreach (PropertyInfo property in properties)
            {
                requestName = property.Name.FirstCharToLower();
                requestValue = property.GetValue(this)?.ToString();
                if (requestValue is not null)
                    stringBuilder.Append($"{requestName}={requestValue}&");            
            }
            stringBuilder.Append($"skin={skinId}");
            return stringBuilder.ToString();
        }
    }
}
