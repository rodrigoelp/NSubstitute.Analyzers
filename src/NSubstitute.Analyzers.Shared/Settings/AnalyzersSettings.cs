﻿using System.Collections.Generic;

namespace NSubstitute.Analyzers.Shared.Settings
{
    internal class AnalyzersSettings
    {
        public static string AnalyzerFileName { get; } = "nsubstitute.json";

        public static AnalyzersSettings Default => new AnalyzersSettings();

        public List<Suppression> Suppressions { get; set; }

        public AnalyzersSettings()
        {
            Suppressions = new List<Suppression>();
        }

        public AnalyzersSettings(List<Suppression> suppressions)
        {
            Suppressions = suppressions;
        }

        public static AnalyzersSettings CreateWithSuppressions(string target, string ruleId)
        {
            return new AnalyzersSettings(new List<Suppression>
            {
                new Suppression
                {
                    Target = target,
                    Rules = new List<string>
                    {
                        ruleId
                    }
                }
            });
        }
    }
}