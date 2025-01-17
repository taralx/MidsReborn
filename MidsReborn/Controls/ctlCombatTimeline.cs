﻿using Mids_Reborn.Core;
using Mids_Reborn.Core.Base.Master_Classes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Windows.Forms;
using Mids_Reborn.Controls.Extensions;
using Mids_Reborn.Core.Base.Data_Classes;
using Mids_Reborn.Core.Utils;

namespace Mids_Reborn.Controls
{
    public partial class ctlCombatTimeline : UserControl
    {
        #region Custom events

        public delegate void CalcEnhancedProgressEventHandler(object sender, float value);
        public event CalcEnhancedProgressEventHandler? CalcEnhancedProgress;

        public delegate void ItemMouseoverEventHandler(object sender, PowerEffectInfo? powerInfo);
        public event ItemMouseoverEventHandler? ItemMouseover;

        public delegate void SetZoomEventHandler(object sender, Interval? viewInterval);
        public event SetZoomEventHandler? SetZoom;

        #endregion

        #region Enums

        private enum ValueSign
        {
            Zero,
            Negative,
            Positive
        }

        private enum BoostType
        {
            Enhancement,
            Power
        }

        public enum ViewProfileType
        {
            Damage,
            Healing,
            Survival,
            Debuff
        }

        #endregion

        #region Structs
        private struct RechBoost
        {
            public int TimelineIndex;
            public BoostType BoostType;
            public float Duration;
        }

        public struct PowerEffectInfo
        {
            public TimelineItem TimelineItem;
            public GroupedFx GroupedFx;
        }

        private struct PowerEntryStatus
        {
            public bool ProcInclude;
            public bool StatInclude;
            public int VariableValue;
        }

        #endregion

        #region FxIdentifier sub-class

        // Warning: struct/class identifier name used elsewhere
        private class FxIdentifier
        {
            public Enums.eEffectType? EffectType;
            public Enums.eMez? MezType;
            public Enums.eEffectType? ETModifies;
            public Enums.eToWho ToWho;
            public ValueSign? ValueSign;

            public FxIdentifier(Enums.eEffectType? effectType, Enums.eToWho toWho, ValueSign? valueSign)
            {
                EffectType = effectType;
                MezType = null;
                ETModifies = null;
                ToWho = toWho;
                ValueSign = valueSign;
            }

            public FxIdentifier(Enums.eMez? mezType, Enums.eToWho toWho, ValueSign? valueSign)
            {
                EffectType = Enums.eEffectType.Mez;
                MezType = mezType;
                ETModifies = null;
                ToWho = toWho;
                ValueSign = valueSign;
            }

            public FxIdentifier(Enums.eEffectType? effectType, Enums.eEffectType? etModifies, Enums.eToWho toWho, ValueSign? valueSign)
            {
                EffectType = effectType;
                MezType = null;
                ETModifies = etModifies;
                ToWho = toWho;
                ValueSign = valueSign;
            }

            public FxIdentifier(Enums.eEffectType? effectType, Enums.eEffectType? etModifies, Enums.eMez? mezType, Enums.eToWho toWho, ValueSign? valueSign)
            {
                EffectType = effectType;
                MezType = mezType;
                ETModifies = etModifies;
                ToWho = toWho;
                ValueSign = valueSign;
            }

            public override string ToString()
            {
                return $"<ctlCombatTimeline.FxIdentifier> {{EffectType={EffectType}, MezType={MezType}, ETModifies={ETModifies}, ToWho={ToWho}, ValueSign={ValueSign}}}";
            }
        }

        #endregion

        #region ViewProfiles sub-class

        private class ViewProfiles
        {
            public readonly List<FxIdentifier> Damage = new()
            {
                new FxIdentifier(Enums.eEffectType.Damage, Enums.eToWho.Target, null), // ???
                new FxIdentifier(Enums.eEffectType.Mez, Enums.eToWho.Target, ValueSign.Positive),
                new FxIdentifier(Enums.eEffectType.Accuracy, Enums.eToWho.Target, ValueSign.Negative),
                new FxIdentifier(Enums.eEffectType.ToHit, Enums.eToWho.Target, ValueSign.Negative),
                new FxIdentifier(Enums.eEffectType.Regeneration, Enums.eToWho.Target, ValueSign.Negative),
                new FxIdentifier(Enums.eEffectType.Recovery, Enums.eToWho.Target, ValueSign.Negative),
                new FxIdentifier(Enums.eEffectType.PerceptionRadius, Enums.eToWho.Target, ValueSign.Negative),
                new FxIdentifier(Enums.eEffectType.Enhancement, Enums.eToWho.Target, ValueSign.Negative),
                new FxIdentifier(Enums.eEffectType.ToHit, Enums.eToWho.Self, ValueSign.Positive),
                new FxIdentifier(Enums.eEffectType.DamageBuff, Enums.eToWho.Self, ValueSign.Positive),
                new FxIdentifier(Enums.eEffectType.Mez, Enums.eToWho.Self, ValueSign.Negative),
                new FxIdentifier(Enums.eEffectType.Enhancement, Enums.eToWho.Self, ValueSign.Positive)
            };

            public readonly List<FxIdentifier> Healing = new()
            {
                new FxIdentifier(Enums.eEffectType.Heal, Enums.eToWho.Target, ValueSign.Positive),
                new FxIdentifier(Enums.eEffectType.HitPoints, Enums.eToWho.Target, ValueSign.Positive),
                new FxIdentifier(Enums.eEffectType.Regeneration, Enums.eToWho.Target, ValueSign.Positive),
                new FxIdentifier(Enums.eEffectType.Recovery, Enums.eToWho.Target, ValueSign.Positive),
                new FxIdentifier(Enums.eEffectType.Mez, Enums.eToWho.Target, ValueSign.Negative),
                new FxIdentifier(Enums.eEffectType.MezResist, Enums.eToWho.Target, ValueSign.Negative), // ???
                new FxIdentifier(Enums.eEffectType.Defense, Enums.eToWho.Target, ValueSign.Positive),
                new FxIdentifier(Enums.eEffectType.Resistance, Enums.eToWho.Target, ValueSign.Positive),
                new FxIdentifier(Enums.eEffectType.ToHit, Enums.eToWho.Target, ValueSign.Positive),
                new FxIdentifier(Enums.eEffectType.Accuracy, Enums.eToWho.Target, ValueSign.Positive),
                new FxIdentifier(Enums.eEffectType.DamageBuff, Enums.eToWho.Target, ValueSign.Positive),
                new FxIdentifier(Enums.eEffectType.Enhancement, Enums.eToWho.Target, ValueSign.Positive),
                new FxIdentifier(Enums.eEffectType.ResEffect, Enums.eToWho.Target, ValueSign.Negative)
            };

            public readonly List<FxIdentifier> Survival = new()
            {
                new FxIdentifier(Enums.eEffectType.Heal, Enums.eToWho.Self, ValueSign.Positive),
                new FxIdentifier(Enums.eEffectType.Regeneration, Enums.eToWho.Self, ValueSign.Positive),
                new FxIdentifier(Enums.eEffectType.HitPoints, Enums.eToWho.Self, ValueSign.Positive),
                new FxIdentifier(Enums.eEffectType.Absorb, Enums.eToWho.Self, ValueSign.Positive),
                new FxIdentifier(Enums.eEffectType.Mez, Enums.eToWho.Self, ValueSign.Negative),
                new FxIdentifier(Enums.eEffectType.MezResist, Enums.eToWho.Self, ValueSign.Positive),
                new FxIdentifier(Enums.eEffectType.Defense, Enums.eToWho.Self, ValueSign.Positive),
                new FxIdentifier(Enums.eEffectType.Resistance, Enums.eToWho.Self, ValueSign.Positive)
            };

            public readonly List<FxIdentifier> Debuff = new()
            {
                new FxIdentifier(Enums.eEffectType.Defense, Enums.eToWho.Target, ValueSign.Negative),
                new FxIdentifier(Enums.eEffectType.Resistance, Enums.eToWho.Target, ValueSign.Negative),
                new FxIdentifier(Enums.eEffectType.ToHit, Enums.eToWho.Target, ValueSign.Negative),
                new FxIdentifier(Enums.eEffectType.Accuracy, Enums.eToWho.Target, ValueSign.Negative),
                new FxIdentifier(Enums.eEffectType.Regeneration, Enums.eToWho.Target, ValueSign.Negative),
                new FxIdentifier(Enums.eEffectType.HitPoints, Enums.eToWho.Target, ValueSign.Negative),
                new FxIdentifier(Enums.eEffectType.Endurance, Enums.eToWho.Target, ValueSign.Negative),
                new FxIdentifier(Enums.eEffectType.Recovery, Enums.eToWho.Target, ValueSign.Negative),
                new FxIdentifier(Enums.eEffectType.EnduranceDiscount, Enums.eToWho.Target, ValueSign.Negative),
                new FxIdentifier(Enums.eEffectType.DamageBuff, Enums.eToWho.Target, ValueSign.Negative),
                new FxIdentifier(Enums.eEffectType.Enhancement, Enums.eToWho.Target, ValueSign.Negative),
                new FxIdentifier(Enums.eEffectType.ResEffect, Enums.eToWho.Target, ValueSign.Negative),
                new FxIdentifier(Enums.eEffectType.PerceptionRadius, Enums.eToWho.Target, ValueSign.Negative),
                new FxIdentifier(Enums.eEffectType.StealthRadius, Enums.eToWho.Target, ValueSign.Negative),
                new FxIdentifier(Enums.eEffectType.StealthRadiusPlayer, Enums.eToWho.Target, ValueSign.Negative),
                new FxIdentifier(Enums.eEffectType.SpeedFlying, Enums.eToWho.Target, ValueSign.Negative),
                new FxIdentifier(Enums.eEffectType.SpeedJumping, Enums.eToWho.Target, ValueSign.Negative),
                new FxIdentifier(Enums.eEffectType.SpeedRunning, Enums.eToWho.Target, ValueSign.Negative),
                new FxIdentifier(Enums.eEffectType.MaxFlySpeed, Enums.eToWho.Target, ValueSign.Negative),
                new FxIdentifier(Enums.eEffectType.MaxJumpSpeed, Enums.eToWho.Target, ValueSign.Negative),
                new FxIdentifier(Enums.eEffectType.MaxRunSpeed, Enums.eToWho.Target, ValueSign.Negative),
                new FxIdentifier(Enums.eEffectType.JumpHeight, Enums.eToWho.Target, ValueSign.Negative),
                new FxIdentifier(Enums.eEffectType.Fly, Enums.eToWho.Target, ValueSign.Negative)
            };
        }

        #endregion

        #region BuildPowerSlot sub-class

        public class BuildPowerSlot
        {
            public IPower? BasePower;
            public IPower? EnhancedPower;
            public int HistoryIdx;

            public BuildPowerSlot(IPower? basePower = null, int historyIdx = -1)
            {
                BasePower = basePower;
                HistoryIdx = historyIdx;
                EnhancedPower = null;
            }

            /// <summary>
            /// Set enhanced power after it is calculated.
            /// </summary>
            /// <param name="enhancedPower">Enhanced power</param>
            public void SetEnhancedPower(IPower? enhancedPower = null)
            {
                EnhancedPower = enhancedPower;
            }

            public override string ToString()
            {
                return $"<BuildPowerSlot> {{BasePower={(BasePower == null ? "<null>" : BasePower.FullName)}, EnhancedPower={(EnhancedPower == null ? "<null>" : EnhancedPower.FullName)}, HistoryIdx={HistoryIdx}}}";
            }
        }

        #endregion

        #region TimelineItem sub-class

        public class TimelineItem
        {
            public BuildPowerSlot PowerSlot;
            public float Time;

            public TimelineItem(BuildPowerSlot powerSlot, float time)
            {
                PowerSlot = powerSlot;
                Time = time;
            }

            public override string ToString()
            {
                return $"<TimelineItem> {{PowerSlot={PowerSlot}, Time={Time}}}";
            }
        }

        #endregion

        #region ColorTheme sub-class

        public class ColorTheme
        {
            public struct EffectTypeShort
            {
                public Enums.eEffectType EffectType;
                public Enums.eEffectType? ETModifies;
            }

            public readonly Color BackgroundColor = Color.FromArgb(11, 22, 29);
            public readonly Color TextColor = Color.WhiteSmoke;
            public readonly Color ShadowColor = Color.Black;

            private readonly Color Red = Color.FromArgb(255, 58, 131);
            private readonly Color LightPink = Color.FromArgb(235, 147, 154);
            private readonly Color Orange = Color.FromArgb(255, 170, 0);
            private readonly Color Yellow = Color.FromArgb(246, 240, 128);
            private readonly Color Yellow2 = Color.FromArgb(237, 223, 28);
            private readonly Color Green = Color.FromArgb(85, 228, 57);
            private readonly Color Green2 = Color.FromArgb(72, 193, 48);
            private readonly Color Green3 = Color.FromArgb(57, 153, 38);
            private readonly Color Green4 = Color.FromArgb(139, 226, 122);
            private readonly Color Green5 = Color.FromArgb(186, 226, 169);
            private readonly Color Blue = Color.FromArgb(30, 154, 224);
            private readonly Color Blue2 = Color.FromArgb(23, 115, 165);
            private readonly Color Blue3 = Color.FromArgb(17, 89, 128);
            private readonly Color Indigo = Color.FromArgb(63, 72, 204);
            private readonly Color Indigo2 = Color.FromArgb(71, 81, 229);
            private readonly Color Indigo3 = Color.FromArgb(79, 90, 255);
            private readonly Color Indigo4 = Color.FromArgb(104, 114, 255);
            private readonly Color Indigo5 = Color.FromArgb(55, 63, 178);
            private readonly Color Purple = Color.FromArgb(115, 43, 245);
            private readonly Color Purple2 = Color.FromArgb(86, 33, 186);
            private readonly Color LightPurple = Color.FromArgb(171, 102, 255);
            private readonly Color LightPurple2 = Color.FromArgb(203, 160, 255);
            private readonly Color LightPurple3 = Color.FromArgb(231, 205, 255);
            private readonly Color White = Color.FromArgb(248, 248, 248);
            private readonly Color Gray = Color.FromArgb(168, 186, 194);
            private readonly Color Gray2 = Color.FromArgb(146, 161, 168);
            private readonly Color Gray3 = Color.FromArgb(124, 137, 142);
            private readonly Color Gray4 = Color.FromArgb(102, 112, 117);
            private readonly Color Gray5 = Color.FromArgb(190, 210, 219);
            private readonly Color Gray6 = Color.FromArgb(212, 235, 244);
            private readonly Color Gray7 = Color.FromArgb(232, 248, 255);
            private readonly Color Gray8 = Color.FromArgb(244, 251, 255);

            public Dictionary<EffectTypeShort, List<Color>> BuildColorDictionary()
            {
                var effectsList = new List<EffectTypeShort>
                {
                    new() {EffectType = Enums.eEffectType.Accuracy},
                    new() {EffectType = Enums.eEffectType.Damage},
                    new() {EffectType = Enums.eEffectType.DamageBuff},
                    new() {EffectType = Enums.eEffectType.Defense},
                    new() {EffectType = Enums.eEffectType.Endurance},
                    new() {EffectType = Enums.eEffectType.SpeedFlying},
                    new() {EffectType = Enums.eEffectType.Heal},
                    new() {EffectType = Enums.eEffectType.HitPoints},
                    new() {EffectType = Enums.eEffectType.JumpHeight},
                    new() {EffectType = Enums.eEffectType.SpeedJumping},
                    new() {EffectType = Enums.eEffectType.Mez},
                    new() {EffectType = Enums.eEffectType.MezResist},
                    new() {EffectType = Enums.eEffectType.PerceptionRadius},
                    new() {EffectType = Enums.eEffectType.Recovery},
                    new() {EffectType = Enums.eEffectType.Regeneration},
                    new() {EffectType = Enums.eEffectType.ResEffect},
                    new() {EffectType = Enums.eEffectType.Resistance},
                    new() {EffectType = Enums.eEffectType.SpeedRunning},
                    new() {EffectType = Enums.eEffectType.ToHit},
                    new() {EffectType = Enums.eEffectType.StealthRadius},
                    new() {EffectType = Enums.eEffectType.StealthRadiusPlayer},
                    new() {EffectType = Enums.eEffectType.MaxFlySpeed},
                    new() {EffectType = Enums.eEffectType.MaxJumpSpeed},
                    new() {EffectType = Enums.eEffectType.MaxRunSpeed},
                    new() {EffectType = Enums.eEffectType.Fly},
                    new() {EffectType = Enums.eEffectType.Absorb},
                    new() {EffectType = Enums.eEffectType.Enhancement, ETModifies = Enums.eEffectType.Heal},
                    new() {EffectType = Enums.eEffectType.Enhancement, ETModifies = Enums.eEffectType.Mez},
                    new() {EffectType = Enums.eEffectType.Enhancement, ETModifies = Enums.eEffectType.RechargeTime},
                    new() {EffectType = Enums.eEffectType.Enhancement, ETModifies = Enums.eEffectType.SpeedRunning},
                    new() {EffectType = Enums.eEffectType.Enhancement, ETModifies = Enums.eEffectType.SpeedJumping},
                    new() {EffectType = Enums.eEffectType.Enhancement, ETModifies = Enums.eEffectType.SpeedFlying}
                };

                var ret = effectsList
                    .ToDictionary(fx => fx, fx => new List<Color> { AssignColor(fx.EffectType, false, fx.ETModifies ?? Enums.eEffectType.None) });

                ret.Add(new EffectTypeShort { EffectType = Enums.eEffectType.Enhancement, ETModifies = Enums.eEffectType.None }, new List<Color> { Gray4, Gray3, Gray2, Gray, Gray5, Gray6, Gray7, Gray8 });

                return ret;
            }

            public Color AssignColor(Enums.eEffectType effectType, bool isProc = false,
                Enums.eEffectType etModifies = Enums.eEffectType.None, int index = 0, int maxIndex = 0)
            {
                return isProc
                    ? White
                    : effectType switch
                    {
                        Enums.eEffectType.Absorb => Green5,
                        Enums.eEffectType.Accuracy => Yellow2,
                        Enums.eEffectType.Damage => Red,
                        Enums.eEffectType.DamageBuff => LightPink,
                        Enums.eEffectType.Defense => LightPurple,
                        Enums.eEffectType.Endurance => Blue2,
                        Enums.eEffectType.Fly => Blue3,
                        Enums.eEffectType.Heal => Green,
                        Enums.eEffectType.HitPoints => Green2,
                        Enums.eEffectType.JumpHeight => Indigo4,
                        Enums.eEffectType.MaxFlySpeed or Enums.eEffectType.MaxJumpSpeed or Enums.eEffectType.MaxRunSpeed => LightPurple2,
                        Enums.eEffectType.Mez => Purple,
                        Enums.eEffectType.MezResist => Purple2,
                        Enums.eEffectType.PerceptionRadius => Gray5,
                        Enums.eEffectType.Recovery => Blue,
                        Enums.eEffectType.Regeneration => Green3,
                        Enums.eEffectType.ResEffect => Indigo5,
                        Enums.eEffectType.Resistance => LightPurple2,
                        Enums.eEffectType.SpeedJumping => Indigo2,
                        Enums.eEffectType.SpeedFlying => Indigo3,
                        Enums.eEffectType.SpeedRunning => Indigo,
                        Enums.eEffectType.StealthRadius or Enums.eEffectType.StealthRadiusPlayer => LightPurple3,
                        Enums.eEffectType.ToHit => Yellow,
                        Enums.eEffectType.Enhancement => etModifies switch
                        {
                            Enums.eEffectType.Heal => Green4,
                            Enums.eEffectType.Mez => Purple2,
                            Enums.eEffectType.RechargeTime => Orange,
                            Enums.eEffectType.SpeedRunning or Enums.eEffectType.SpeedJumping
                                or Enums.eEffectType.SpeedFlying or Enums.eEffectType.JumpHeight => Blue3,
                            _ => maxIndex < 4
                                ? index switch
                                {
                                    0 => Gray,
                                    1 => Gray2,
                                    2 => Gray3,
                                    _ => Gray4
                                }
                                : index switch
                                {
                                    0 => Gray4,
                                    1 => Gray3,
                                    2 => Gray2,
                                    3 => Gray,
                                    4 => Gray5,
                                    5 => Gray6,
                                    6 => Gray7,
                                    _ => Gray8
                                }
                        },

                        _ => White
                    };
            }
        }

        #endregion

        public List<BuildPowerSlot> Powers { get; set; }
        public List<IPower> UserBoosts { get; set; }
        public ViewProfileType Profile { get; set; }
        public bool UseArcanaTime { get; set; }
        public List<string> BuffsLookup { get; }
        public List<TimelineItem> Timeline { get; private set; }

        public float MaxTime => Timeline.Count <= 0
            ? 1
            : Timeline
                .Select(p => p.Time + p.PowerSlot.EnhancedPower?.Effects.Max(f => f.Duration))
                .Max() ?? 1;

        private List<string> Boosts;
        private ViewProfiles Profiles;
        private ColorTheme Theme;
        private Dictionary<Rectangle, PowerEffectInfo> ActiveZones;
        private Point? PrevMousePos;
        private Interval? ViewInterval;

        public ctlCombatTimeline()
        {
            Powers = new List<BuildPowerSlot>();
            BuffsLookup = new List<string>();
            Timeline = new List<TimelineItem>();
            UseArcanaTime = true;
            Boosts = new List<string>
            {
                "Boosts.Crafted_Force_Feedback_F.Crafted_Force_Feedback_F", // Force Feedback: Chance for +Recharge
                "Boosts.Crafted_Decimation_F.Crafted_Decimation_F", // Decimation: Chance for Build Up
                "Boosts.Crafted_Gaussians_Synchronized_Firecontrol_F" // Gaussian's Synchronized Fire-Control: Chance for Build Up
            };

            UserBoosts = new List<IPower>();
            Profiles = new ViewProfiles();
            Theme = new ColorTheme();
            ActiveZones = new Dictionary<Rectangle, PowerEffectInfo>();

            InitializeComponent();
            ItemMouseover += ctlCombatTimeline_ItemMouseover;
        }

        #region Helper methods

        public void SetView(Interval? viewInterval = null)
        {
            ViewInterval = viewInterval;
            Invalidate();
        }

        /// <summary>
        /// Calculate power cast time using ArcanaTime formula
        /// </summary>
        /// <param name="castTime">Base cast time</param>
        /// <returns></returns>
        private float CalcArcanaCastTime(float castTime)
        {
            return (float)(Math.Ceiling(castTime / 0.132f) + 1) * 0.132f;
        }

        /// <summary>
        /// Place all powers on the timeline, calculate all enhanced powers
        /// </summary>
        /// <param name="redraw">Triggers a redraw after calculations.</param>
        public void PlacePowers(bool redraw = true)
        {
            if (Powers.Count <= 0)
            {
                return;
            }

            ListPowersToTimeline();

            if (!redraw)
            {
                return;
            }

            Invalidate();
        }

        /// <summary>
        /// Build temporal line from a list of powers
        /// </summary>
        private void ListPowersToTimeline()
        {
            var time = 0f;
            Timeline = new List<TimelineItem>();

            CalcEnhancedProgress?.Invoke(this, 0);

            var k = 0;
            foreach (var power in Powers)
            {
                var previousPowerOccurrences = Timeline
                    .Where(e => e.PowerSlot.BasePower != null && e.PowerSlot.BasePower?.FullName == power.BasePower?.FullName)
                    .ToList();

                var previousOccurrence = k == 0 | previousPowerOccurrences.Count <= 0
                    ? null
                    : previousPowerOccurrences
                        .Select((e, i) => new KeyValuePair<int, TimelineItem>(i, e))
                        .MaxBy(e => e.Key)
                        .Value;

                var ffBoostDuration = DatabaseAPI.GetPowerByFullName("Set_Bonus.Set_Bonus.Force_Feedback")?.Effects
                    .First(e => e.EffectType == Enums.eEffectType.Enhancement & e.ETModifies == Enums.eEffectType.RechargeTime)
                    .Duration;

                Timeline.Add(new TimelineItem(power, time));

                var rechargeBoosts = new List<RechBoost>();
                // Recharge boosts will self-affect current power
                for (var i = 0; i < Math.Min(k + 1, Powers.Count); i++)
                {
                    var enhRechBoost = HasBoost(Powers[i]).Any(e => e != null && e.Contains("Force_Feedback_F"));
                    var powerRechBoost = Powers[i].BasePower?.ClickBuff &
                                         Powers[i].BasePower?.Effects
                                             .Any(e => e.EffectType == Enums.eEffectType.Enhancement &
                                                       e.ETModifies == Enums.eEffectType.RechargeTime &
                                                       e.ToWho == Enums.eToWho.Self);

                    if (enhRechBoost)
                    {
                        rechargeBoosts.Add(new RechBoost
                        {
                            TimelineIndex = i,
                            BoostType = BoostType.Enhancement,
                            Duration = ffBoostDuration ?? 0
                        });

                        continue;
                    }

                    if (powerRechBoost != true)
                    {
                        continue;
                    }

                    var powerBoostDuration = Powers[i].EnhancedPower?.Effects
                        .Where(e => e.EffectType == Enums.eEffectType.Enhancement &
                                    e.ETModifies == Enums.eEffectType.RechargeTime &
                                    e.ToWho == Enums.eToWho.Self & e.BuffedMag > 0)
                        .Select(e => e.Duration)
                        .Max();
                    rechargeBoosts.Add(new RechBoost
                    {
                        TimelineIndex = i,
                        BoostType = BoostType.Power,
                        Duration = powerBoostDuration ?? 0
                    });
                }

                var rechargeTime = previousOccurrence?.PowerSlot.EnhancedPower?.RechargeTime ?? 0;
                var startTimePrev = previousOccurrence?.Time ?? 0;
                var timeWait = k == 0 ? 0 : Math.Max(0, startTimePrev + rechargeTime - time);
                Timeline[k].Time = time + timeWait;
                var tpw = Timeline[k];
                CalcEnhancedPower(ref tpw, rechargeBoosts);

                time += (UseArcanaTime ? power.BasePower?.ArcanaCastTime ?? 0 : power.BasePower?.CastTimeBase ?? 0) + timeWait;
                k++;

                CalcEnhancedProgress?.Invoke(this, (int)Math.Round(k / (float)Powers.Count * 100));
            }

            CalcEnhancedProgress?.Invoke(this, 100);
        }

        /// <summary>
        /// Get occurrences of a single power in the timeline
        /// </summary>
        /// <param name="power">Power to look for (from power object)</param>
        /// <returns>List of matching powers in the timeline</returns>
        private List<TimelineItem> GetPowerOccurrences(IPower? power)
        {
            return Timeline
                .Where(e => e.PowerSlot.BasePower?.FullName == power?.FullName)
                .ToList();
        }

        /// <summary>
        /// Get occurrences of a single power in the timeline
        /// </summary>
        /// <param name="powerName">Power to look for (from power full name)</param>
        /// <returns>List of matching powers in the timeline</returns>
        private List<TimelineItem> GetPowerOccurrences(string powerName)
        {
            return Timeline
                .Where(e => e.PowerSlot.BasePower?.FullName == powerName)
                .ToList();
        }

        /// <summary>
        /// Get unique powers present on a timeline
        /// </summary>
        /// <returns>List of powers full name</returns>
        private List<string> GetDistinctPowers()
        {
            return Timeline
                .Select(e => new KeyValuePair<string?, string?>(e.PowerSlot.BasePower?.FullName, e.PowerSlot.BasePower?.DisplayName))
                .OrderBy(e => e.Value)
                .Select(e => e.Key)
                .Cast<string>()
                .Distinct()
                .ToList();
        }

        /// <summary>
        /// Get total damage from powers on the timeline.
        /// </summary>
        /// <returns>Total powers damage sum</returns>
        public float GetTotalDamage()
        {
            return Timeline.Count <= 0
                ? 0
                : Timeline
                    .Select(e => e.PowerSlot.EnhancedPower?.FXGetDamageValue() ?? 0)
                    .Sum();
        }

        /// <summary>
        /// Check if a power is affected by any boost
        /// </summary>
        /// <param name="powerSlot">Target power</param>
        /// <returns>List of active boosts as power full names</returns>
        private List<string?> HasBoost(BuildPowerSlot powerSlot)
        {
            //var buildPowerEntry = MidsContext.Character.CurrentBuild.Powers[powerSlot.HistoryIdx];
            var buildPowerEntry = GetMatchingPowerEntry(powerSlot.BasePower);
            var slotsUid = buildPowerEntry.Slots
                .Select(e => e.Enhancement.Enh < 0 ? "" : DatabaseAPI.Database.Enhancements[e.Enhancement.Enh].UID)
                .ToList();

            return slotsUid
                .Where(e => !string.IsNullOrEmpty(e))
                .Select(e =>
                    DatabaseAPI.Database.Power
                        .DefaultIfEmpty(new Power { StaticIndex = -1 })
                        .FirstOrDefault(f => f?.FullName.EndsWith(e) == true))
                .Select(e => e == null || e.StaticIndex < 0 || !Boosts.Contains(e.FullName) ? null : e.FullName)
                .ToList();
        }

        /// <summary>
        /// Get active boost sources for a power in the timeline
        /// </summary>
        /// <param name="timelineItem">Target power</param>
        /// <returns>List of boosting powers in the timeline, if any</returns>
        private List<TimelineItem>? IsAffectedByBoosts(TimelineItem timelineItem)
        {
            var boostsMaxDuration = Boosts
                .Select(DatabaseAPI.GetPowerByFullName)
                .Select(e => e == null ? 0 : e.Effects.Max(f => f.Duration))
                .ToList();

            var absoluteMaxDuration = boostsMaxDuration.Max();
            var timelineIntervalPowers = Timeline
                .Where(e => (e.Time >= timelineItem.Time - absoluteMaxDuration) & e.Time < timelineItem.Time)
                .ToList();

            // Assume target boosts have 100% proc chance
            var activeBoostsSources = timelineIntervalPowers
                .Select(e => new KeyValuePair<TimelineItem, List<string?>>(e, HasBoost(e.PowerSlot)))
                .Where(e => e.Value.Any(f => f != null))
                .Select(e => e.Key)
                .ToList();

            var userBoostsNames = UserBoosts
                .Select(e => e.FullName)
                .ToList();
            var userBoostSources = UserBoosts.Count <= 0
                ? new List<TimelineItem>()
                : Timeline
                    .Where(e => e.PowerSlot.BasePower != null && userBoostsNames.Contains(e.PowerSlot.BasePower.FullName))
                    .Where(e => e.Time < timelineItem.Time & e.Time + (e.PowerSlot.EnhancedPower == null ? 0 : e.PowerSlot.EnhancedPower.Effects.Max(f => f.Duration)) >= e.Time)
                    .ToList();
            var boostSources = activeBoostsSources
                .Union(userBoostSources)
                .Distinct()
                .ToList();

            return boostSources.Count <= 0 ? null : boostSources;
        }

        /// <summary>
        /// Calculate enhanced power from a base one with only boosting powers active
        /// </summary>
        /// <param name="timelinePower">Target power</param>
        /// <param name="recalcStats">Recalculate totals with original activation state when done</param>
        /// <remarks>Warning: possibly very slow</remarks>
        private void CalcEnhancedPower(ref TimelineItem timelinePower, bool recalcStats = false)
        {
            var origProcIncludes = MidsContext.Character.CurrentBuild.Powers
                .Select(e => e?.ProcInclude)
                .ToList();

            var origStatIncludes = MidsContext.Character.CurrentBuild.Powers
                .Select(e => e?.StatInclude)
                .ToList();

            foreach (var pe in MidsContext.Character.CurrentBuild.Powers)
            {
                if (pe == null)
                {
                    continue;
                }

                pe.ProcInclude = false;
            }

            var userBoostNames = UserBoosts
                .Select(e => e.FullName)
                .ToList();

            foreach (var pe in MidsContext.Character.CurrentBuild.Powers)
            {
                if (pe == null)
                {
                    continue;
                }

                if (!userBoostNames.Contains(pe.Power?.FullName))
                {
                    continue;
                }

                pe.StatInclude = false;
            }

            var boostingPowers = IsAffectedByBoosts(timelinePower);
            if (boostingPowers != null)
            {
                foreach (var p in boostingPowers)
                {
                    var pe = GetMatchingPowerEntry(p.PowerSlot.BasePower);

                    if (pe == null)
                    {
                        continue;
                    }

                    pe.ProcInclude = true;
                    pe.StatInclude = true;
                }
            }

            RecalcTotals();

            // Select enhanced power by historyIdx doesn't work.
            var pName = timelinePower.PowerSlot.BasePower == null ? "" : timelinePower.PowerSlot.BasePower.FullName;
            timelinePower.PowerSlot.SetEnhancedPower(MainModule.MidsController.Toon.GetEnhancedPower(timelinePower.PowerSlot.BasePower));

            if (!recalcStats)
            {
                // frmMain.DoRedraw() ?
                return;
            }

            for (var i = 0; i < MidsContext.Character.CurrentBuild.Powers.Count; i++)
            {
                if (MidsContext.Character.CurrentBuild.Powers[i] == null)
                {
                    continue;
                }

                MidsContext.Character.CurrentBuild.Powers[i].ProcInclude = origProcIncludes[i] == true;
                MidsContext.Character.CurrentBuild.Powers[i].StatInclude = origStatIncludes[i] == true;
            }

            RecalcTotals();
        }

        

        /// <summary>
        /// Get active stacks for a power boost, at the time of a reference power.
        /// </summary>
        /// <param name="timelinePower">Reference power in timeline</param>
        /// <param name="boostPower">Boost power to check for</param>
        /// <returns>Active stacks of boost power</returns>
        private int GetActiveBoostsCount(TimelineItem timelinePower, TimelineItem? boostPower)
        {
            if (boostPower == null)
            {
                return 0;
            }

            return Timeline.Count <= 0
                ? 0
                : Timeline
                    .Count(e => e.Time <= timelinePower.Time &
                                e.Time + (e.PowerSlot.EnhancedPower == null ? 0 : e.PowerSlot.EnhancedPower.Effects.Select(f => f.Duration).Max()) >= timelinePower.Time);
        }

        private void CalcEnhancedPower(ref TimelineItem timelinePower, List<RechBoost> rechargeBoosts, bool recalcStats = false)
        {
            // Take a snapshot of build powers' ProcInclude, StatInclude, VariableValue to restore later
            var originalPeStatus = MidsContext.Character.CurrentBuild.Powers
                .Select(e => new PowerEntryStatus
                {
                    ProcInclude = e?.ProcInclude ?? false,
                    StatInclude = e?.StatInclude ?? false,
                    VariableValue = e?.VariableValue ?? 0,
                })
                .ToList();

            var userBoostNames = UserBoosts
                .Select(e => e.FullName)
                .ToList();

            foreach (var pe in MidsContext.Character.CurrentBuild.Powers)
            {
                if (pe != null)
                {
                    pe.ProcInclude = false;
                }

                if (userBoostNames.Contains(pe.Power?.FullName))
                {
                    pe.StatInclude = false;
                }
            }

            var boostingPowers = IsAffectedByBoosts(timelinePower);
            if (boostingPowers != null)
            {
                foreach (var p in boostingPowers)
                {
                    var pe = GetMatchingPowerEntry(p.PowerSlot.BasePower);

                    if (pe == null)
                    {
                        continue;
                    }

                    pe.ProcInclude = true;
                    pe.StatInclude = true;

                    // Only set stacks if stacking is enabled.
                    if (pe.Power is not {VariableEnabled: true})
                    {
                        continue;
                    }

                    var activeCount = GetActiveBoostsCount(timelinePower, p);
                    pe.VariableValue = activeCount;
                    pe.Power.Stacks = activeCount;
                }
            }

            foreach (var b in rechargeBoosts)
            {
                if (b.TimelineIndex >= Timeline.Count)
                {
                    continue;
                }

                if (Timeline[b.TimelineIndex].Time + b.Duration < timelinePower.Time)
                {
                    continue;
                }

                var pe = GetMatchingPowerEntry(Timeline[b.TimelineIndex].PowerSlot.BasePower);

                if (pe == null)
                {
                    continue;
                }

                switch (b.BoostType)
                {
                    case BoostType.Enhancement:
                        pe.ProcInclude = true;
                        break;

                    case BoostType.Power:
                        pe.StatInclude = true;
                        break;
                }
            }

            RecalcTotals();

            // Select enhanced power by historyIdx doesn't work.
            var pName = timelinePower.PowerSlot.BasePower == null ? "" : timelinePower.PowerSlot.BasePower.FullName;
            timelinePower.PowerSlot.SetEnhancedPower(MainModule.MidsController.Toon.GetEnhancedPower(timelinePower.PowerSlot.BasePower));

            if (!recalcStats)
            {
                // frmMain.DoRedraw() ?
                return;
            }

            for (var i = 0; i < MidsContext.Character.CurrentBuild.Powers.Count; i++)
            {
                if (MidsContext.Character.CurrentBuild.Powers[i] == null)
                {
                    continue;
                }

                MidsContext.Character.CurrentBuild.Powers[i].ProcInclude = originalPeStatus[i].ProcInclude;
                MidsContext.Character.CurrentBuild.Powers[i].StatInclude = originalPeStatus[i].StatInclude;
                MidsContext.Character.CurrentBuild.Powers[i].VariableValue = originalPeStatus[i].VariableValue;
                MidsContext.Character.CurrentBuild.Powers[i].Power.Stacks = originalPeStatus[i].VariableValue;
            }

            RecalcTotals();
        }

        /// <summary>
        /// Get matching PowerEntry in build containing a specified power
        /// </summary>
        /// <remarks>Will return null if input power is null.</remarks>
        /// <param name="power">Target power</param>
        /// <returns>Matching PowerEntry in build, null if not found</returns>
        private PowerEntry? GetMatchingPowerEntry(IPower? power)
        {
            if (power == null)
            {
                return null;
            }

            return MidsContext.Character.CurrentBuild.Powers
                .Where(e => e is { Power: not null })
                .DefaultIfEmpty(null)
                .FirstOrDefault(e => e.Power.FullName == power.FullName);
        }

        /// <summary>
        /// Trigger a full character stats re-calculation
        /// </summary>
        private void RecalcTotals()
        {
            if (MainModule.MidsController.Toon == null | !MainModule.MidsController.IsAppInitialized)
            {
                return;
            }

            MainModule.MidsController.Toon?.GenerateBuffedPowerArray();
        }

        private List<FxIdentifier> SelectViewProfile(ViewProfileType profile)
        {
            return profile switch
            {
                ViewProfileType.Healing => Profiles.Healing,
                ViewProfileType.Survival => Profiles.Survival,
                ViewProfileType.Debuff => Profiles.Debuff,
                _ => Profiles.Damage
            };
        }

        /// <summary>
        /// Filter effects according to profile
        /// </summary>
        /// <param name="gfx">Grouped effects</param>
        /// <param name="power">Target power</param>
        /// <param name="profile">View profile</param>
        /// <returns>Filtered effects list</returns>
        private List<GroupedFx> ApplyViewProfile(IEnumerable<GroupedFx> gfx, IPower power, ViewProfileType profile)
        {
            var profileFilter = SelectViewProfile(profile);

            var gfxList = gfx.ToList();
            return gfxList
                .Select(e => new KeyValuePair<FxIdentifier, GroupedFx>(new FxIdentifier(
                    e.EffectType,
                    e.ETModifies == Enums.eEffectType.None ? null : e.ETModifies,
                    e.MezType == Enums.eMez.None ? null : e.MezType,
                    e.ToWho,
                    e.GetMagSum(power) switch
                    {
                        > 0 => ValueSign.Positive,
                        < 0 => ValueSign.Negative,
                        _ => ValueSign.Zero
                    }), e))
                .Where(e => FilterGfx(e.Key, profileFilter))
                .Select(e => e.Value)
                .ToList();
        }

        /// <summary>
        /// Filter GroupedFx key according to profile
        /// </summary>
        /// <remarks>Null values in profile are considered pass-through and will always validate key.</remarks>
        /// <param name="fxIdentifier">GroupedFx identifier</param>
        /// <param name="profile">View profile</param>
        /// <returns>true if this effect type is displayed, false if hidden</returns>
        private bool FilterGfx(FxIdentifier fxIdentifier, IEnumerable<FxIdentifier> profile)
        {
            return (from e in profile
                    let effectTypeCheck = e.EffectType == null | e.EffectType == fxIdentifier.EffectType
                    let mezTypeCheck = e.MezType == null | e.MezType == fxIdentifier.MezType
                    let etModifies = e.ETModifies == null | e.ETModifies == fxIdentifier.ETModifies
                    let toWhoCheck = e.ToWho is Enums.eToWho.All or Enums.eToWho.Unspecified | e.ToWho == fxIdentifier.ToWho
                    let valueSignCheck = e.ValueSign == null | e.ValueSign == fxIdentifier.ValueSign
                    where effectTypeCheck & mezTypeCheck & etModifies & toWhoCheck & valueSignCheck
                    select effectTypeCheck).Any();
        }

        /// <summary>
        /// Checks if the view interval can be zoomed out.
        /// </summary>
        /// <returns>True if ViewInterval.Length is lower than MaxTime</returns>
        public bool MaxZoomOut()
        {
            return ViewInterval == null || ViewInterval.Length >= MaxTime;
        }

        /// <summary>
        /// Checks if the view interval can be zoomed in.
        /// </summary>
        /// <returns>True if ViewInterval.Length > 5</returns>
        public bool MaxZoomIn()
        {
            return ViewInterval is {Length: <= 5};
        }

        /// <summary>
        /// Zooms in, by a defined factor.
        /// </summary>
        /// <remarks>Factor must be in ]0; 1[ and will shrink the view interval by this value.</remarks>
        /// <param name="redraw">Performs a redraw when done</param>
        /// <param name="factor">Scale factor</param>
        public void ZoomIn(bool redraw = false, float factor = 0.8f)
        {
            if (Math.Abs(factor) <= 0.01)
            {
                return;
            }

            if (factor is < 0 or >= 1)
            {
                return;
            }

            if (MaxZoomIn())
            {
                return;
            }

            ViewInterval ??= new Interval(MaxTime);
            ViewInterval = ViewInterval.ScaleCenter(factor);

            SetZoom?.Invoke(this, ViewInterval);

            if (!redraw)
            {
                return;
            }

            Invalidate();
        }

        /// <summary>
        /// Zooms out, by a defined factor.
        /// </summary>
        /// <remarks>Factor must be in ]0; 1[ and will grow the view interval by the inverse of this value.</remarks>
        /// <param name="redraw">Performs a redraw when done</param>
        /// <param name="factor">Scale factor</param>
        public void ZoomOut(bool redraw = false, float factor = 0.8f)
        {
            if (Math.Abs(factor) <= 0.01)
            {
                return;
            }

            if (factor is < 0 or >= 1)
            {
                return;
            }

            if (MaxZoomOut())
            {
                return;
            }

            var timelineInterval = new Interval(MaxTime);
            ViewInterval ??= timelineInterval;
            ViewInterval = ViewInterval.ScaleCenter(1 / factor).MinMax(timelineInterval);
            if (Math.Abs(ViewInterval.Length - timelineInterval.Length) <= 0.01)
            {
                ViewInterval = null;
            }

            SetZoom?.Invoke(this, ViewInterval);

            if (!redraw)
            {
                return;
            }

            Invalidate();
        }

        #endregion

        #region Drawing process

        protected struct PowerText
        {
            public string PowerName;
            public Font Font;
            public Rectangle Bounds;
            public Color ShadowColor;
            public Color TextColor;
            public TextFormatFlags FormatFlags;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            var bgColor = Theme.BackgroundColor;
            var textColor = Theme.TextColor;
            var shadowColor = Theme.ShadowColor;

            var distinctPowers = GetDistinctPowers();
            if (distinctPowers.Count <= 0)
            {
                g.Clear(bgColor);

                return;
            }

            const int padding = 8;
            const int minLineThickness = 3;
            const int interlineHeight = 16;
            const float normalTextSize = 11; // px
            const int textGapLeft = 90; // px

            var powerHeights = distinctPowers
                .Select(p => Timeline
                    .Where(f => f.PowerSlot.EnhancedPower?.FullName == p)
                    .Select(f => ApplyViewProfile(GroupedFx.AssembleGroupedEffects(f.PowerSlot.EnhancedPower),
                        f.PowerSlot.EnhancedPower, Profile).Count))
                .Select(p => p.Max())
                .ToList();
            var powersRows = distinctPowers
                .Select((p, i) => new KeyValuePair<string, int>(p, i))
                .ToDictionary(p => p.Key, p => p.Value);

            var totalItems = powerHeights.Sum();
            var lineThickness = Math.Max(minLineThickness, (int) Math.Round((Height - Math.Max(0, totalItems - 1) * interlineHeight) / (double) totalItems));
            var hScale = (Width - 2 * padding - textGapLeft) / (ViewInterval?.Length ?? MaxTime); // time -> pixels
            var tOffset = ViewInterval?.Start * hScale ?? 0;
            //var totalHeight = powerHeights.Sum() + Math.Max(0, powerHeights.Count - 1) * interlineHeight;

            var font = new Font(new FontFamily("Microsoft Sans Serif"), normalTextSize, FontStyle.Regular, GraphicsUnit.Pixel);

            var orderedTimeline = Timeline
                .OrderBy(f => f.PowerSlot.EnhancedPower == null ? 9999 : powersRows[f.PowerSlot.EnhancedPower.FullName])
                .ThenBy(f => f.Time)
                .ToList();

            ActiveZones = new Dictionary<Rectangle, PowerEffectInfo>();
            var powerTexts = new List<PowerText>();

            g.Clear(bgColor);

            foreach (var p in orderedTimeline)
            {
                var pIndex = powersRows[p.PowerSlot.EnhancedPower?.FullName];
                var vOffset = padding + (pIndex == 0 ? 0 : powerHeights.Take(pIndex - 1).Sum()) * lineThickness + interlineHeight * pIndex;

                if (p.PowerSlot.EnhancedPower == null)
                {
                    continue;
                }

                var vp = 0f;

                // Profile-filtered GroupedFx
                var gfx = ApplyViewProfile(GroupedFx.AssembleGroupedEffects(p.PowerSlot.EnhancedPower, true), p.PowerSlot.EnhancedPower, Profile);

                // Move damage/heal effects to last elements so they are drawn on top of the others.
                gfx = gfx
                    .OrderBy(f => $"{f.EffectType}")
                    .ThenBy(f => f.EffectType is Enums.eEffectType.Damage or Enums.eEffectType.Heal ? 1 : 0)
                    .ToList();

                // Generic enhancement effects for power
                var genericEnhancements = gfx
                    .Select((f, i) => new KeyValuePair<int, GroupedFx>(i, f))
                    .Where(f => f.Value.EffectType == Enums.eEffectType.Enhancement &
                                f.Value.ETModifies is not (Enums.eEffectType.RechargeTime or Enums.eEffectType.Heal
                                    or Enums.eEffectType.SpeedRunning or Enums.eEffectType.SpeedJumping
                                    or Enums.eEffectType.SpeedFlying or Enums.eEffectType.JumpHeight
                                    or Enums.eEffectType.Mez))
                    .ToDictionary(f => f.Key, f => f.Value);

                for (var i = 0; i < gfx.Count; i++)
                {
                    if (i == 0)
                    {
                        // Power name text
                        powerTexts.Add(new PowerText
                        {
                            PowerName = p.PowerSlot.EnhancedPower.DisplayName,
                            Bounds = new Rectangle(0, (int)Math.Round(vOffset + vp - normalTextSize / 2f), textGapLeft, (int)Math.Round(Height - padding - vOffset - vp + normalTextSize / 2f)),
                            Font = font,
                            ShadowColor = shadowColor,
                            TextColor = textColor,
                            FormatFlags = TextFormatFlags.Right | TextFormatFlags.Top
                        });
                    }

                    // Index among generic enhancements, 0 if none.
                    var index = genericEnhancements.ContainsKey(i)
                        ? genericEnhancements.Keys.TryFindIndex(f => f == i)
                        : 0;

                    // Count of generic enhancements, 0 if none
                    var maxIndex = gfx[i].EffectType == Enums.eEffectType.Enhancement
                        ? genericEnhancements.Count
                        : 0;

                    var barColor = Theme.AssignColor(gfx[i].EffectType, gfx[i].EnhancementEffect, gfx[i].ETModifies, index, maxIndex);
                    var barBrush = new SolidBrush(barColor);
                    var linePen = new Pen(barBrush, lineThickness);

                    if (gfx[i].GetEffectAt(p.PowerSlot.EnhancedPower).Duration < float.Epsilon)
                    {
                        // Zero-duration effects: draw hollow ring
                        if (padding + textGapLeft + p.Time * hScale - tOffset >= textGapLeft)
                        {
                            var ringRect = new RectangleF(
                                Math.Max(textGapLeft,
                                    padding + textGapLeft + p.Time * hScale - 2 * lineThickness - tOffset),
                                vOffset + vp - 1.5f * lineThickness, 4 * lineThickness, 4 * lineThickness);
                            g.DrawEllipse(linePen, ringRect);
                            ActiveZones.Add(
                                key: new Rectangle((int) Math.Floor(ringRect.X),
                                    (int) Math.Floor(ringRect.Y),
                                    (int) Math.Ceiling(ringRect.Width),
                                    (int) Math.Ceiling(ringRect.Height)),
                                value: new PowerEffectInfo {TimelineItem = p, GroupedFx = gfx[i]});
                        }
                    }
                    else
                    {
                        // DoTs, HoTs + effects with duration > 0: draw line
                        var x1 = padding + textGapLeft + p.Time * hScale - tOffset;
                        var x2 = padding + textGapLeft + (p.Time + gfx[i].GetEffectAt(p.PowerSlot.EnhancedPower).Duration) * hScale - tOffset;
                        var barPoint1 = new PointF(Math.Max(textGapLeft, x1), vOffset + vp);
                        var barPoint2 = new PointF(Math.Max(textGapLeft, x2), vOffset + vp);
                        if (x1 >= textGapLeft & x2 >= textGapLeft)
                        {
                            //g.DrawLine(linePen, barPoint1.X, barPoint1.Y, barPoint2.X, barPoint2.Y);
                            g.FillRectangle(barBrush, barPoint1.X, barPoint1.Y, Math.Abs(barPoint2.X - barPoint1.X),
                                lineThickness);
                            ActiveZones.Add(
                                key: new Rectangle((int) Math.Floor(barPoint1.X),
                                    (int) Math.Floor(barPoint1.Y),
                                    (int) Math.Ceiling(Math.Abs(barPoint2.X - barPoint1.X)),
                                    lineThickness),
                                value: new PowerEffectInfo {TimelineItem = p, GroupedFx = gfx[i]});
                        }
                    }

                    vp += lineThickness;
                }

                TextRendererExt.DrawOutlineText(g,
                    $"Time interval: [{(ViewInterval == null ? $"0 s. - {MaxTime:####0.##} s." : $"{ViewInterval.Start:####0.##} s. - {ViewInterval.End:####0.##} s.")}]",
                    font, new Rectangle(3, 3, Width - 6, 24), shadowColor, textColor,
                    TextFormatFlags.Right | TextFormatFlags.Top);
            }

            // Draw power texts
            foreach (var pt in powerTexts)
            {
                TextRendererExt.DrawOutlineText(g, pt.PowerName, pt.Font, pt.Bounds, pt.ShadowColor, pt.TextColor, pt.FormatFlags);
            }
        }

        #endregion

        #region Event handlers

        private void ctlCombatTimeline_MouseMove(object sender, MouseEventArgs e)
        {
            foreach (var z in ActiveZones)
            {
                if (!z.Key.Contains(e.Location))
                {
                    continue;
                }

                if (PrevMousePos != null && PrevMousePos?.X == e.X & PrevMousePos?.Y == e.Y)
                {
                    return;
                }

                ItemMouseover?.Invoke(this, z.Value);
                PrevMousePos = new Point(e.X, e.Y);

                return;
            }

            /*if (PrevMousePos != null && PrevMousePos?.X == e.X & PrevMousePos?.Y == e.Y)
            {
                return;
            }*/

            ItemMouseover?.Invoke(this, null);
            PrevMousePos = new Point(e.X, e.Y);
        }

        private void ctlCombatTimeline_MouseLeave(object sender, EventArgs e)
        {
            ItemMouseover?.Invoke(this, null);
            PrevMousePos = null;
        }

        private void ctlCombatTimeline_ItemMouseover(object sender, PowerEffectInfo? powerInfo)
        {
            var tip = powerInfo == null
                ? ""
                : $"Power: {powerInfo.Value.TimelineItem.PowerSlot.EnhancedPower?.DisplayName}, at time: {powerInfo.Value.TimelineItem.Time:#####0.##} s.\r\nCast time (ArcanaTime): {powerInfo.Value.TimelineItem.PowerSlot.EnhancedPower?.ArcanaCastTime:####0.###} s.\r\nRecharge time: {powerInfo.Value.TimelineItem.PowerSlot.EnhancedPower?.RechargeTime:####0.###} s.\r\n---------------\r\n{powerInfo.Value.GroupedFx.GetTooltip(powerInfo.Value.TimelineItem.PowerSlot.EnhancedPower)}";

            toolTip1.SetToolTip(this, tip);
        }

        #endregion

    }
}
