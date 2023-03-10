// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using osu_bot.Entites;

namespace osu_bot.Modules
{
    public static class PerfomanceCalculator
    {
        public const double PERFORMANCE_BASE_MULTIPLIER = 1.14;

        private static double s_accuracy;
        private static int s_scoreMaxCombo;
        private static int s_countGreat;
        private static int s_countOk;
        private static int s_countMeh;
        private static int s_countMiss;

        private static double s_effectiveMissCount;

        private static int totalHits => s_countGreat + s_countOk + s_countMeh + s_countMiss;

        public static int Calculate(OsuScoreInfo score, bool isFullCombo = false, bool isPerfect = false)
        {
            if (isFullCombo)
            {
                if (isPerfect)
                {
                    s_accuracy = 1;
                    s_scoreMaxCombo = score.Beatmap.Attributes.MaxCombo;
                    s_countGreat = score.Beatmap.Attributes.TotalObjects;
                    s_countOk = 0;
                    s_countMeh = 0;
                    s_countMiss = 0;
                }
                else
                {
                    s_scoreMaxCombo = score.Beatmap.Attributes.MaxCombo;
                    if (score.Beatmap.Attributes.TotalObjects == score.Count300 + score.Count100 + score.Count50 + score.CountMisses)
                    {
                        s_countGreat = score.Count300 + score.CountMisses;
                        s_countOk = score.Count100;
                        s_countMeh = score.Count50;
                        s_accuracy = CalculateAccuracyFromHits(s_countGreat, s_countOk, s_countMeh, s_countMiss);
                    }
                    else
                    {
                        (s_countGreat, s_countOk) = CalculateHitsFromAccuracy(score.Accuracy * 0.01, score.Beatmap.Attributes.TotalObjects);
                        s_accuracy = score.Accuracy * 0.01;
                        s_countMeh = 0;
                    }
                    s_countMiss = 0;
                }
            }
            else
            {
                s_accuracy = score.Accuracy * 0.01;
                s_scoreMaxCombo = score.MaxCombo;
                s_countGreat = score.Count300;
                s_countOk = score.Count100;
                s_countMeh = score.Count50;
                s_countMiss = score.CountMisses;
            }

            s_effectiveMissCount = CalculateEffectiveMissCount(score.Beatmap.Attributes);

            OsuBeatmapAttributes attributes = score.Beatmap.Attributes;

            double multiplier = PERFORMANCE_BASE_MULTIPLIER;

            if (score.Mods.Any(m => m.Name == "NF"))
            {
                multiplier *= Math.Max(0.90, 1.0 - (0.02 * s_effectiveMissCount));
            }

            if (score.Mods.Any(m => m.Name == "SO") && totalHits > 0)
            {
                multiplier *= 1.0 - Math.Pow((double)attributes.SpinnerCount / totalHits, 0.85);
            }

            if (score.Mods.Any(m => m.Name == "RL"))
            {
                double okMultiplier = Math.Max(0.0, attributes.OD > 0.0 ? 1 - Math.Pow(attributes.OD / 13.33, 1.8) : 1.0);
                double mehMultiplier = Math.Max(0.0, attributes.OD > 0.0 ? 1 - Math.Pow(attributes.OD / 13.33, 5) : 1.0);

                s_effectiveMissCount = Math.Min(s_effectiveMissCount + (s_countOk * okMultiplier) + (s_countMeh * mehMultiplier), totalHits);
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

        private static double getComboScalingFactor(OsuBeatmapAttributes attributes) => attributes.MaxCombo <= 0 ? 1.0 : Math.Min(Math.Pow(s_scoreMaxCombo, 0.8) / Math.Pow(attributes.MaxCombo, 0.8), 1.0);

        private static double computeAimValue(OsuScoreInfo score, OsuBeatmapAttributes attributes)
        {
            double aimValue = Math.Pow((5.0 * Math.Max(1.0, attributes.AimDifficulty / 0.0675)) - 4.0, 3.0) / 100000.0;

            double lengthBonus = 0.95 + (0.4 * Math.Min(1.0, totalHits / 2000.0)) +
                                 (totalHits > 2000 ? Math.Log10(totalHits / 2000.0) * 0.5 : 0.0);
            aimValue *= lengthBonus;

            if (s_effectiveMissCount > 0)
            {
                aimValue *= 0.97 * Math.Pow(1 - Math.Pow(s_effectiveMissCount / totalHits, 0.775), s_effectiveMissCount);
            }

            aimValue *= getComboScalingFactor(attributes);

            double approachRateFactor = 0.0;
            if (attributes.AR > 10.33)
            {
                approachRateFactor = 0.3 * (attributes.AR - 10.33);
            }
            else if (attributes.AR < 8.0)
            {
                approachRateFactor = 0.05 * (8.0 - attributes.AR);
            }

            if (score.Mods.Any(m => m.Name == "RL"))
            {
                approachRateFactor = 0.0;
            }

            aimValue *= 1.0 + (approachRateFactor * lengthBonus);

            if (score.Mods.Any(m => m.Name == "HD"))
            {
                aimValue *= 1.0 + (0.04 * (12.0 - attributes.AR));
            }

            double estimateDifficultSliders = attributes.SliderCount * 0.15;

            if (attributes.SliderCount > 0)
            {
                double estimateSliderEndsDropped = Math.Clamp(Math.Min(s_countOk + s_countMeh + s_countMiss, attributes.MaxCombo - s_scoreMaxCombo), 0, estimateDifficultSliders);
                double sliderNerfFactor = ((1 - attributes.SliderFactor) * Math.Pow(1 - (estimateSliderEndsDropped / estimateDifficultSliders), 3)) + attributes.SliderFactor;
                aimValue *= sliderNerfFactor;
            }

            aimValue *= s_accuracy;
            aimValue *= 0.98 + (Math.Pow(attributes.OD, 2) / 2500);

            return aimValue;
        }

        private static double computeSpeedValue(OsuScoreInfo score, OsuBeatmapAttributes attributes)
        {
            if (score.Mods.Any(m => m.Name == "RL"))
            {
                return 0.0;
            }

            double speedValue = Math.Pow((5.0 * Math.Max(1.0, attributes.SpeedDifficulty / 0.0675)) - 4.0, 3.0) / 100000.0;

            double lengthBonus = 0.95 + (0.4 * Math.Min(1.0, totalHits / 2000.0)) +
                                 (totalHits > 2000 ? Math.Log10(totalHits / 2000.0) * 0.5 : 0.0);
            speedValue *= lengthBonus;

            if (s_effectiveMissCount > 0)
            {
                speedValue *= 0.97 * Math.Pow(1 - Math.Pow(s_effectiveMissCount / totalHits, 0.775), Math.Pow(s_effectiveMissCount, .875));
            }

            speedValue *= getComboScalingFactor(attributes);

            double approachRateFactor = 0.0;
            if (attributes.AR > 10.33)
            {
                approachRateFactor = 0.3 * (attributes.AR - 10.33);
            }

            speedValue *= 1.0 + (approachRateFactor * lengthBonus);

            if (score.Mods.Any(m => m.Name == "HD"))
            {
                speedValue *= 1.0 + (0.04 * (12.0 - attributes.AR));
            }

            double relevantTotalDiff = totalHits - attributes.SpeedNoteCount;
            double relevantCountGreat = Math.Max(0, s_countGreat - relevantTotalDiff);
            double relevantCountOk = Math.Max(0, s_countOk - Math.Max(0, relevantTotalDiff - s_countGreat));
            double relevantCountMeh = Math.Max(0, s_countMeh - Math.Max(0, relevantTotalDiff - s_countGreat - s_countOk));
            double relevantAccuracy = attributes.SpeedNoteCount == 0 ? 0 : ((relevantCountGreat * 6.0) + (relevantCountOk * 2.0) + relevantCountMeh) / (attributes.SpeedNoteCount * 6.0);

            speedValue *= (0.95 + (Math.Pow(attributes.OD, 2) / 750)) * Math.Pow((s_accuracy + relevantAccuracy) / 2.0, (14.5 - Math.Max(attributes.OD, 8)) / 2);

            speedValue *= Math.Pow(0.99, s_countMeh < totalHits / 500.0 ? 0 : s_countMeh - (totalHits / 500.0));

            return speedValue;
        }

        private static double computeAccuracyValue(OsuScoreInfo score, OsuBeatmapAttributes attributes)
        {
            if (score.Mods.Any(m => m.Name == "RL"))
            {
                return 0.0;
            }

            double betterAccuracyPercentage;
            int amountHitObjectsWithAccuracy = attributes.CircleCount;

            betterAccuracyPercentage = amountHitObjectsWithAccuracy > 0
                ? (((s_countGreat - (totalHits - amountHitObjectsWithAccuracy)) * 6) + (s_countOk * 2) + s_countMeh) / (double)(amountHitObjectsWithAccuracy * 6)
                : 0;

            if (betterAccuracyPercentage < 0)
            {
                betterAccuracyPercentage = 0;
            }

            double accuracyValue = Math.Pow(1.52163, attributes.OD) * Math.Pow(betterAccuracyPercentage, 24) * 2.83;

            accuracyValue *= Math.Min(1.15, Math.Pow(amountHitObjectsWithAccuracy / 1000.0, 0.3));

            if (score.Mods.Any(m => m.Name == "HD"))
            {
                accuracyValue *= 1.08;
            }

            if (score.Mods.Any(m => m.Name == "FL"))
            {
                accuracyValue *= 1.02;
            }

            return accuracyValue;
        }

        private static double computeFlashlightValue(OsuScoreInfo score, OsuBeatmapAttributes attributes)
        {
            if (!score.Mods.Any(m => m.Name == "FL"))
            {
                return 0.0;
            }

            double flashlightValue = Math.Pow(attributes.FlashlightDifficulty, 2.0) * 25.0;

            if (s_effectiveMissCount > 0)
            {
                flashlightValue *= 0.97 * Math.Pow(1 - Math.Pow(s_effectiveMissCount / totalHits, 0.775), Math.Pow(s_effectiveMissCount, .875));
            }

            flashlightValue *= getComboScalingFactor(attributes);

            flashlightValue *= 0.7 + (0.1 * Math.Min(1.0, totalHits / 200.0)) +
                               (totalHits > 200 ? 0.2 * Math.Min(1.0, (totalHits - 200) / 200.0) : 0.0);

            flashlightValue *= 0.5 + (s_accuracy / 2.0);
            flashlightValue *= 0.98 + (Math.Pow(attributes.OD, 2) / 2500);

            return flashlightValue;
        }

        private static double CalculateEffectiveMissCount(OsuBeatmapAttributes attributes)
        {
            double comboBasedMissCount = 0.0;

            if (attributes.SliderCount > 0)
            {
                double fullComboThreshold = attributes.MaxCombo - (0.1 * attributes.SliderCount);
                if (s_scoreMaxCombo < fullComboThreshold)
                {
                    comboBasedMissCount = fullComboThreshold / Math.Max(1.0, s_scoreMaxCombo);
                }
            }

            comboBasedMissCount = Math.Min(comboBasedMissCount, s_countOk + s_countMeh + s_countMiss);

            return Math.Max(s_countMiss, comboBasedMissCount);
        }

        private static double CalculateAccuracyFromHits(int count300, int count100, int count50, int countMiss) => ((300.0 * count300) + (100.0 * count100) + (50.0 * count50)) / (300.0 * (count300 + count100 + count50 + countMiss));

        private static (int, int) CalculateHitsFromAccuracy(double accuracy, int totalObjects)
        {
            int count300 = totalObjects;
            int count100 = 0;
            double lastAccuracy = 1;
            double nowAccuracy = 1;
            while (!(Math.Round(nowAccuracy, 4) <= Math.Round(accuracy, 4) && Math.Round(accuracy, 4) >= Math.Round(lastAccuracy, 4)))
            {
                count300--;
                count100++;
                lastAccuracy = nowAccuracy;
                nowAccuracy = CalculateAccuracyFromHits(count300 - 1, count100 + 1, 0, 0);
            }
            return (count300, count100);
        }
    }
}
