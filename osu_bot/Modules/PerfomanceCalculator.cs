using osu_bot.Entites;
using osu_bot.Entites.Mods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace osu_bot.Modules
{
    public static class PerfomanceCalculator
    {
        public const double PERFORMANCE_BASE_MULTIPLIER = 1.14;

        private static double accuracy;
        private static int scoreMaxCombo;
        private static int countGreat;
        private static int countOk;
        private static int countMeh;
        private static int countMiss;

        private static double effectiveMissCount;

        private static int totalHits => countGreat + countOk + countMeh + countMiss;

        public static int Calculate(ScoreInfo score, bool isFullCombo = false, bool isPerfect = false)
        {
            if (isFullCombo)
            {
                if (isPerfect)
                {
                    accuracy = 1;
                    scoreMaxCombo = score.Beatmap.Attributes.MaxCombo;
                    countGreat = score.Beatmap.Attributes.TotalObjects;
                    countOk = 0;
                    countMeh = 0;
                    countMiss = 0;
                }
                else
                {
                    scoreMaxCombo = score.Beatmap.Attributes.MaxCombo;
                    if (score.Beatmap.Attributes.TotalObjects == score.Count300 + score.Count100 + score.Count50 + score.CountMisses)
                    {
                        countGreat = score.Count300 + score.CountMisses;
                        countOk = score.Count100;
                        countMeh = score.Count50;
                        accuracy = Extensions.CalculateAccuracyFromHits(countGreat, countOk, countMeh, countMiss);
                    }
                    else
                    {
                        (countGreat, countOk) = Extensions.CalculateHitsFromAccuracy(score.Accuracy * 0.01, score.Beatmap.Attributes.TotalObjects);
                        accuracy = score.Accuracy * 0.01;
                        countMeh = 0;                       
                    }
                    countMiss = 0;
                }
            }
            else
            {
                accuracy = score.Accuracy * 0.01;
                scoreMaxCombo = score.MaxCombo;
                countGreat = score.Count300;
                countOk = score.Count100;
                countMeh = score.Count50;
                countMiss = score.CountMisses;
            }

            effectiveMissCount = CalculateEffectiveMissCount(score.Beatmap.Attributes);

            var attributes = score.Beatmap.Attributes;

            double multiplier = PERFORMANCE_BASE_MULTIPLIER;

            if (score.Mods.Any(m => m.Name == "NF"))
                multiplier *= Math.Max(0.90, 1.0 - 0.02 * effectiveMissCount);

            if (score.Mods.Any(m => m.Name == "SO") && totalHits > 0)
                multiplier *= 1.0 - Math.Pow((double)attributes.SpinnerCount / totalHits, 0.85);

            if (score.Mods.Any(m => m.Name == "RL"))
            {
                double okMultiplier = Math.Max(0.0, attributes.OD > 0.0 ? 1 - Math.Pow(attributes.OD / 13.33, 1.8) : 1.0);
                double mehMultiplier = Math.Max(0.0, attributes.OD > 0.0 ? 1 - Math.Pow(attributes.OD / 13.33, 5) : 1.0);
             
                effectiveMissCount = Math.Min(effectiveMissCount + countOk * okMultiplier + countMeh * mehMultiplier, totalHits);
            }

            double aimValue = computeAimValue(score, attributes);
            double speedValue = computeSpeedValue(score, attributes);
            double accuracyValue = computeAccuracyValue(score, attributes);
            double flashlightValue = computeFlashlightValue(score, attributes);
            double totalValue =
                Math.Pow(
                    Math.Pow(aimValue, 1.1) +
                    Math.Pow(speedValue, 1.1) +
                    Math.Pow(accuracyValue, 1.1) +
                    Math.Pow(flashlightValue, 1.1), 1.0 / 1.1
                ) * multiplier;
            return (int)Math.Round(totalValue, 2, MidpointRounding.ToEven);
        }

        private static double getComboScalingFactor(BeatmapAttributes attributes) => attributes.MaxCombo <= 0 ? 1.0 : Math.Min(Math.Pow(scoreMaxCombo, 0.8) / Math.Pow(attributes.MaxCombo, 0.8), 1.0);

        private static double computeAimValue(ScoreInfo score, BeatmapAttributes attributes)
        {
            double aimValue = Math.Pow(5.0 * Math.Max(1.0, attributes.AimDifficulty / 0.0675) - 4.0, 3.0) / 100000.0;

            double lengthBonus = 0.95 + 0.4 * Math.Min(1.0, totalHits / 2000.0) +
                                 (totalHits > 2000 ? Math.Log10(totalHits / 2000.0) * 0.5 : 0.0);
            aimValue *= lengthBonus;

            if (effectiveMissCount > 0)
                aimValue *= 0.97 * Math.Pow(1 - Math.Pow(effectiveMissCount / totalHits, 0.775), effectiveMissCount);

            aimValue *= getComboScalingFactor(attributes);

            double approachRateFactor = 0.0;
            if (attributes.AR > 10.33)
                approachRateFactor = 0.3 * (attributes.AR - 10.33);
            else if (attributes.AR < 8.0)
                approachRateFactor = 0.05 * (8.0 - attributes.AR);

            if (score.Mods.Any(m => m.Name == "RL"))
                approachRateFactor = 0.0;

            aimValue *= 1.0 + approachRateFactor * lengthBonus;

            if (score.Mods.Any(m => m.Name == "HD"))
            {
                aimValue *= 1.0 + 0.04 * (12.0 - attributes.AR);
            }

            double estimateDifficultSliders = attributes.SliderCount * 0.15;

            if (attributes.SliderCount > 0)
            {
                double estimateSliderEndsDropped = Math.Clamp(Math.Min(countOk + countMeh + countMiss, attributes.MaxCombo - scoreMaxCombo), 0, estimateDifficultSliders);
                double sliderNerfFactor = (1 - attributes.SliderFactor) * Math.Pow(1 - estimateSliderEndsDropped / estimateDifficultSliders, 3) + attributes.SliderFactor;
                aimValue *= sliderNerfFactor;
            }

            aimValue *= accuracy;
            aimValue *= 0.98 + Math.Pow(attributes.OD, 2) / 2500;

            return aimValue;
        }

        private static double computeSpeedValue(ScoreInfo score, BeatmapAttributes attributes)
        {
            if (score.Mods.Any(m => m.Name == "RL"))
                return 0.0;

            double speedValue = Math.Pow(5.0 * Math.Max(1.0, attributes.SpeedDifficulty / 0.0675) - 4.0, 3.0) / 100000.0;

            double lengthBonus = 0.95 + 0.4 * Math.Min(1.0, totalHits / 2000.0) +
                                 (totalHits > 2000 ? Math.Log10(totalHits / 2000.0) * 0.5 : 0.0);
            speedValue *= lengthBonus;

            if (effectiveMissCount > 0)
                speedValue *= 0.97 * Math.Pow(1 - Math.Pow(effectiveMissCount / totalHits, 0.775), Math.Pow(effectiveMissCount, .875));

            speedValue *= getComboScalingFactor(attributes);

            double approachRateFactor = 0.0;
            if (attributes.AR > 10.33)
                approachRateFactor = 0.3 * (attributes.AR - 10.33);

            speedValue *= 1.0 + approachRateFactor * lengthBonus;

            if (score.Mods.Any(m => m.Name == "HD"))
            {
                speedValue *= 1.0 + 0.04 * (12.0 - attributes.AR);
            }

            double relevantTotalDiff = totalHits - attributes.SpeedNoteCount;
            double relevantCountGreat = Math.Max(0, countGreat - relevantTotalDiff);
            double relevantCountOk = Math.Max(0, countOk - Math.Max(0, relevantTotalDiff - countGreat));
            double relevantCountMeh = Math.Max(0, countMeh - Math.Max(0, relevantTotalDiff - countGreat - countOk));
            double relevantAccuracy = attributes.SpeedNoteCount == 0 ? 0 : (relevantCountGreat * 6.0 + relevantCountOk * 2.0 + relevantCountMeh) / (attributes.SpeedNoteCount * 6.0);

            speedValue *= (0.95 + Math.Pow(attributes.OD, 2) / 750) * Math.Pow((accuracy + relevantAccuracy) / 2.0, (14.5 - Math.Max(attributes.OD, 8)) / 2);

            speedValue *= Math.Pow(0.99, countMeh < totalHits / 500.0 ? 0 : countMeh - totalHits / 500.0);

            return speedValue;
        }

        private static double computeAccuracyValue(ScoreInfo score, BeatmapAttributes attributes)
        {
            if (score.Mods.Any(m => m.Name == "RL"))
                return 0.0;

            double betterAccuracyPercentage;
            int amountHitObjectsWithAccuracy = attributes.CircleCount;

            if (amountHitObjectsWithAccuracy > 0)
                betterAccuracyPercentage = ((countGreat - (totalHits - amountHitObjectsWithAccuracy)) * 6 + countOk * 2 + countMeh) / (double)(amountHitObjectsWithAccuracy * 6);
            else
                betterAccuracyPercentage = 0;

            if (betterAccuracyPercentage < 0)
                betterAccuracyPercentage = 0;

            double accuracyValue = Math.Pow(1.52163, attributes.OD) * Math.Pow(betterAccuracyPercentage, 24) * 2.83;

            accuracyValue *= Math.Min(1.15, Math.Pow(amountHitObjectsWithAccuracy / 1000.0, 0.3));

            if (score.Mods.Any(m => m.Name == "HD"))
                accuracyValue *= 1.08;

            if (score.Mods.Any(m => m.Name == "FL"))
                accuracyValue *= 1.02;

            return accuracyValue;
        }

        private static double computeFlashlightValue(ScoreInfo score, BeatmapAttributes attributes)
        {
            if (!score.Mods.Any(m => m.Name == "FL"))
                return 0.0;

            double flashlightValue = Math.Pow(attributes.FlashlightDifficulty, 2.0) * 25.0;

            if (effectiveMissCount > 0)
                flashlightValue *= 0.97 * Math.Pow(1 - Math.Pow(effectiveMissCount / totalHits, 0.775), Math.Pow(effectiveMissCount, .875));

            flashlightValue *= getComboScalingFactor(attributes);

            flashlightValue *= 0.7 + 0.1 * Math.Min(1.0, totalHits / 200.0) +
                               (totalHits > 200 ? 0.2 * Math.Min(1.0, (totalHits - 200) / 200.0) : 0.0);

            flashlightValue *= 0.5 + accuracy / 2.0;
            flashlightValue *= 0.98 + Math.Pow(attributes.OD, 2) / 2500;

            return flashlightValue;
        }

        private static double CalculateEffectiveMissCount(BeatmapAttributes attributes)
        {
            double comboBasedMissCount = 0.0;

            if (attributes.SliderCount > 0)
            {
                double fullComboThreshold = attributes.MaxCombo - 0.1 * attributes.SliderCount;
                if (scoreMaxCombo < fullComboThreshold)
                    comboBasedMissCount = fullComboThreshold / Math.Max(1.0, scoreMaxCombo);
            }

            comboBasedMissCount = Math.Min(comboBasedMissCount, countOk + countMeh + countMiss);

            return Math.Max(countMiss, comboBasedMissCount);
        }
    }
}
