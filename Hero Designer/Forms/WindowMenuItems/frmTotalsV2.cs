﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Base.Data_Classes;
using Base.Display;
using Base.Master_Classes;
using midsControls;
using Syncfusion.Windows.Forms.Tools;

namespace Hero_Designer.Forms.WindowMenuItems
{
    public partial class frmTotalsV2 : Form
    {
        private readonly frmMain _myParent;
        private bool KeepOnTop { get; set; }

        public Control StatControl(string tab, int panel, string type, int control)
        {
            Regex regEx = new Regex(@"^\d+");
            TabPageAdv page = tabControlAdv2.Controls.OfType<TabPageAdv>().First(t => t.Text.Contains(tab));
            List<GradientPanel> gradientList = page.Controls.OfType<GradientPanel>().ToList();
            List<GradientPanel> gradientPanels = gradientList.OrderBy(x => x.Name).ToList();
            List<TableLayoutPanel> tablePanels = gradientPanels[panel - 1].Controls.OfType<TableLayoutPanel>().ToList();

            switch (type)
            {
                case "Bar":
                    List<Control> controls = new List<Control>();
                    for (int rowIndex = 0; rowIndex < tablePanels[0].RowCount; rowIndex++)
                    {
                        Control tControl = tablePanels[0].GetControlFromPosition(2, rowIndex);
                        controls.Add(tControl);
                    }

                    List<ctlLayeredBar> barList = controls.OfType<ctlLayeredBar>().ToList();
                    List<ctlLayeredBar> bars = barList.OrderBy(x => regEx.Match(x.Name).Value).ToList();

                    return bars[control - 1];
                case "Label":
                    controls = new List<Control>();
                    for (int rowIndex = 0; rowIndex < tablePanels[0].RowCount; rowIndex++)
                    {
                        Control tControl = tablePanels[0].GetControlFromPosition(1, rowIndex);
                        controls.Add(tControl);
                    }

                    List<Label> labelList = controls.OfType<Label>().ToList();
                    List<Label> labels = labelList.OrderBy(x => regEx.Match(x.Name).Value).ToList();

                    return labels[control - 1];
            }

            return null;
        }

        public frmTotalsV2(ref frmMain iParent)
        {
            FormClosed += frmTotalsV2_FormClosed;
            Load += OnLoad;
            KeepOnTop = true;
            InitializeComponent();
            _myParent = iParent;

            // Tab Foreground Colors don't stick in the designer.
            // Note: default colors will be used anyway.
            // This may only cause issues if a custom
            // Windows theme is in use.
            tabControlAdv2.ActiveTabForeColor = Color.White;
            tabControlAdv2.InActiveTabForeColor = Color.Black;
        }

        private void Radio_CheckedChanged(object sender, EventArgs e)
        {
            var sendingControl = (RadioButton) sender;
            var radioControls = Controls.OfType<RadioButton>();
            if (sendingControl.Checked)
            {
                foreach (var radio in radioControls)
                {
                    if (radio.Name != sendingControl.Name)
                    {
                        radio.Checked = false;
                    }
                }
            }
        }

        public override void Refresh()
        {
            if (MidsContext.Character.IsHero())
            {
                tabControlAdv2.InactiveTabColor = Color.FromArgb(61, 111, 161);
                tabControlAdv2.TabPanelBackColor = Color.FromArgb(61, 111, 161);
                tabControlAdv2.FixedSingleBorderColor = Color.Goldenrod;
                tabControlAdv2.ActiveTabColor = Color.Goldenrod;
            }
            else
            {
                /*tabControlAdv2.InactiveTabColor = Color.FromArgb(193, 23, 23);
                tabControlAdv2.TabPanelBackColor = Color.FromArgb(193, 23, 23);
                tabControlAdv2.FixedSingleBorderColor = Color.FromArgb(198, 128, 29);
                tabControlAdv2.ActiveTabColor = Color.FromArgb(198, 128, 29);*/
                tabControlAdv2.InactiveTabColor = Color.FromArgb(138, 62, 57);
                tabControlAdv2.TabPanelBackColor = Color.FromArgb(138, 62, 57);
                tabControlAdv2.FixedSingleBorderColor = Color.Goldenrod;
                tabControlAdv2.ActiveTabColor = Color.Goldenrod;
            }

            base.Refresh();
        }

        private void OnLoad(object sender, EventArgs e)
        {
            CenterToParent();
            Refresh();
        }

        private void frmTotalsV2_FormClosed(object sender, FormClosedEventArgs e)
        {
            _myParent.FloatTotals(false);
        }

        private void frmTotalsV2_Move(object sender, EventArgs e)
        {
            StoreLocation();
        }

        private void PbCloseClick(object sender, EventArgs e)
        {
            Close();
        }

        private void PbClosePaint(object sender, PaintEventArgs e)
        {
            if (_myParent?.Drawing == null) return;

            var iStr = "Close";
            var rectangle = new Rectangle();
            ref var local = ref rectangle;
            var size = MidsContext.Character.IsHero()
                ? _myParent.Drawing.bxPower[2].Size
                : _myParent.Drawing.bxPower[4].Size;
            var width = size.Width;
            size = MidsContext.Character.IsHero()
                ? _myParent.Drawing.bxPower[2].Size
                : _myParent.Drawing.bxPower[4].Size;
            var height1 = size.Height;
            local = new Rectangle(0, 0, width, height1);
            var destRect = new Rectangle(0, 0, 105, 22);
            using var stringFormat = new StringFormat();
            using var bFont = new Font(Font.FontFamily, Font.Size, FontStyle.Bold, GraphicsUnit.Point);
            stringFormat.Alignment = StringAlignment.Center;
            stringFormat.LineAlignment = StringAlignment.Center;
            using var extendedBitmap = new ExtendedBitmap(destRect.Width, destRect.Height);
            extendedBitmap.Graphics.Clear(BackColor);
            extendedBitmap.Graphics.DrawImage(
                MidsContext.Character.IsHero()
                    ? _myParent.Drawing.bxPower[2].Bitmap
                    : _myParent.Drawing.bxPower[4].Bitmap, destRect, 0, 0, rectangle.Width, rectangle.Height,
                GraphicsUnit.Pixel, _myParent.Drawing.pImageAttributes);
            var height2 = bFont.GetHeight(e.Graphics) + 2;
            var Bounds = new RectangleF(0, (22 - height2) / 2, 105, height2);
            var graphics = extendedBitmap.Graphics;
            clsDrawX.DrawOutlineText(iStr, Bounds, Color.WhiteSmoke, Color.FromArgb(192, 0, 0, 0), bFont, 1, graphics);
            e.Graphics.DrawImage(extendedBitmap.Bitmap, 0, 0);
        }

        private void PbTopMostClick(object sender, EventArgs e)
        {
            KeepOnTop = !KeepOnTop;
            TopMost = KeepOnTop;
            pbTopMost.Refresh();
        }

        private void PbTopMostPaint(object sender, PaintEventArgs e)
        {
            if (_myParent?.Drawing == null) return;

            var index = 2;
            if (KeepOnTop) index = 3;
            var iStr = "Keep On top";
            var rectangle = new Rectangle(0, 0, _myParent.Drawing.bxPower[index].Size.Width,
                _myParent.Drawing.bxPower[index].Size.Height);
            var destRect = new Rectangle(0, 0, 105, 22);
            using var stringFormat = new StringFormat();
            using var bFont = new Font(Font.FontFamily, Font.Size, FontStyle.Bold, GraphicsUnit.Point);
            stringFormat.Alignment = StringAlignment.Center;
            stringFormat.LineAlignment = StringAlignment.Center;
            using var extendedBitmap = new ExtendedBitmap(destRect.Width, destRect.Height);
            extendedBitmap.Graphics.Clear(BackColor);
            if (index == 3)
                extendedBitmap.Graphics.DrawImage(_myParent.Drawing.bxPower[index].Bitmap, destRect, 0, 0,
                    rectangle.Width, rectangle.Height, GraphicsUnit.Pixel);
            else
                extendedBitmap.Graphics.DrawImage(_myParent.Drawing.bxPower[index].Bitmap, destRect, 0, 0,
                    rectangle.Width, rectangle.Height, GraphicsUnit.Pixel, _myParent.Drawing.pImageAttributes);
            var height = bFont.GetHeight(e.Graphics) + 2;
            var Bounds = new RectangleF(0, (22 - height) / 2, 105, height);
            var graphics = extendedBitmap.Graphics;
            clsDrawX.DrawOutlineText(iStr, Bounds, Color.WhiteSmoke, Color.FromArgb(192, 0, 0, 0), bFont, 1, graphics);
            e.Graphics.DrawImage(extendedBitmap.Bitmap, 0, 0);
        }

        #region FormatValue overloads

        private string FormatValue(int formatType, float value)
        {
            return formatType switch
            {
                0 => $"{value:##0.##}%", // Percentage
                1 => $"{value:##0.##}", // Numeric, 2 decimals
                2 => (value > 0 ? "+" : "") + $"{value:##0.##}", // Numeric, 2 decimals, with sign
                3 => $"{Math.Abs(value):##0.##}", // Numeric, 2 decimals (for mez protection)
                4 => $"{value:##0.##}/s", // Numeric, 2 decimals, per second
                _ => $"{value:##0.##}"
            };
        }

        private string FormatValue(int formatType, float value, float uncappedValue, float capValue, string statType,
            string dmgType, string atName)
        {
            string vs;
            string uvs;
            string cs;

            switch (formatType)
            {
                case 0:
                    vs = $"{value:##0.##}%";
                    uvs = $"{uncappedValue:##0.##}%";
                    cs = $"{capValue:##0.##}%";
                    break;

                case 1:
                    vs = $"{value:##0.##}";
                    uvs = $"{uncappedValue:##0.##}";
                    cs = $"{capValue:##0.##}";
                    break;

                case 2:
                    vs = (value > 0 ? "+" : "") + $"{value:##0.##}";
                    uvs = (uncappedValue > 0 ? "+" : "") + $"{uncappedValue:##0.##}";
                    cs = (capValue > 0 ? "+" : "") + $"{capValue:##0.##}";
                    break;

                case 3:
                    vs = $"{Math.Abs(value):##0.##}";
                    uvs = $"{Math.Abs(uncappedValue):##0.##}";
                    cs = $"{Math.Abs(capValue):##0.##}";
                    break;

                case 4:
                    vs = $"{value:##0.##}/s";
                    uvs = $"{uncappedValue:##0.##}/s";
                    cs = $"{capValue:##0.##}/s";
                    break;

                default:
                    vs = $"{value:##0.##}";
                    uvs = $"{uncappedValue:##0.##}";
                    cs = $"{capValue:##0.##}";
                    break;
            }

            return value <= uncappedValue
                ? $"{vs}{(string.IsNullOrEmpty(dmgType) ? "" : " " + dmgType)} {statType}" + (capValue > 0 ? $" ({atName} {statType.ToLower()} cap: {cs})" : "")
                : $"{uvs}{(string.IsNullOrEmpty(dmgType) ? "" : " " + dmgType)} {statType}, capped at {cs}";
        }

        private string FormatValue(int formatType, float value, float uncappedValue, float baseValue, float capValue, string statType, string dmgType, string atName)
        {
            string vs;
            string uvs;
            string bs;
            string cs;

            switch (formatType)
            {
                case 0:
                    vs = $"{value:##0.##}%";
                    uvs = $"{uncappedValue:##0.##}%";
                    cs = $"{capValue:##0.##}%";
                    bs = $"{baseValue:##0.##}%";
                    break;

                case 1:
                    vs = $"{value:##0.##}";
                    uvs = $"{uncappedValue:##0.##}";
                    cs = $"{capValue:##0.##}";
                    bs = $"{baseValue:##0.##}";
                    break;

                case 2:
                    vs = (value > 0 ? "+" : "") + $"{value:##0.##}";
                    uvs = (uncappedValue > 0 ? "+" : "") + $"{uncappedValue:##0.##}";
                    cs = (capValue > 0 ? "+" : "") + $"{capValue:##0.##}";
                    bs = (baseValue > 0 ? "+" : "") + $"{baseValue:##0.##}";
                    break;

                case 3:
                    vs = $"{Math.Abs(value):##0.##}";
                    uvs = $"{Math.Abs(uncappedValue):##0.##}";
                    cs = $"{Math.Abs(capValue):##0.##}";
                    bs = $"{Math.Abs(baseValue):##0.##}";
                    break;

                case 4:
                    vs = $"{value:##0.##}/s";
                    uvs = $"{uncappedValue:##0.##}/s";
                    cs = $"{capValue:##0.##}/s";
                    bs = $"{baseValue:##0.##}/s";
                    break;

                default:
                    vs = $"{value:##0.##}";
                    uvs = $"{uncappedValue:##0.##}";
                    cs = $"{capValue:##0.##}";
                    bs = $"{capValue:##0.##}";
                    break;
            }

            return (value <= uncappedValue
                       ? $"{vs}{(string.IsNullOrEmpty(dmgType) ? "" : " " + dmgType)} {statType}" + (capValue > 0 ? $" ({atName} {statType} cap: {cs})" :"")
                       : $"{uvs}{(string.IsNullOrEmpty(dmgType) ? "" : " " + dmgType)} {statType}, capped at {cs}") +
                             (baseValue != 0 ? $"\r\nBase: {bs}" : "");
        }

        private string FormatValue(int formatType, float value, float uncappedValue, float baseValue, float capValue,
            float overlay1Value, string statType, string overlay1Stat, string dmgType, string atName)
        {
            string vs;
            string uvs;
            string bs;
            string cs;
            string o1s;

            switch (formatType)
            {
                case 0:
                    vs = $"{value:##0.##}%";
                    uvs = $"{uncappedValue:##0.##}%";
                    cs = $"{capValue:##0.##}%";
                    bs = $"{baseValue:##0.##}%";
                    o1s = $"{overlay1Value:##0.##}%";
                    break;

                case 1:
                    vs = $"{value:##0.##}";
                    uvs = $"{uncappedValue:##0.##}";
                    cs = $"{capValue:##0.##}";
                    bs = $"{baseValue:##0.##}";
                    o1s = $"{overlay1Value:##0.##}";
                    break;

                case 2:
                    vs = (value > 0 ? "+" : "") + $"{value:##0.##}";
                    uvs = (uncappedValue > 0 ? "+" : "") + $"{uncappedValue:##0.##}";
                    cs = (capValue > 0 ? "+" : "") + $"{capValue:##0.##}";
                    bs = (baseValue > 0 ? "+" : "") + $"{baseValue:##0.##}";
                    o1s = (overlay1Value > 0 ? "+" : "") + $"{overlay1Value:##0.##}";
                    break;

                case 3:
                    vs = $"{Math.Abs(value):##0.##}";
                    uvs = $"{Math.Abs(uncappedValue):##0.##}";
                    cs = $"{Math.Abs(capValue):##0.##}";
                    bs = $"{Math.Abs(baseValue):##0.##}";
                    o1s = $"{Math.Abs(overlay1Value):##0.##}";
                    break;

                case 4:
                    vs = $"{value:##0.##}/s";
                    uvs = $"{uncappedValue:##0.##}/s";
                    cs = $"{capValue:##0.##}/s";
                    bs = $"{baseValue:##0.##}/s";
                    o1s = $"{overlay1Value:##0.##}/s";
                    break;

                default:
                    vs = $"{value:##0.##}";
                    uvs = $"{uncappedValue:##0.##}";
                    cs = $"{capValue:##0.##}";
                    bs = $"{capValue:##0.##}";
                    o1s = $"{overlay1Value:##0.##}";
                    break;
            }

            if (statType == "HP")
            {
                return (value <= uncappedValue
                           ? $"{vs}{(string.IsNullOrEmpty(dmgType) ? "" : " " + dmgType)} {statType}" +
                             (capValue > 0 ? $"({atName} {statType} cap: {cs})" : "")
                           : $"{uvs}{(string.IsNullOrEmpty(dmgType) ? "" : " " + dmgType)} {statType}, capped at {cs}"
                       ) +
                       (baseValue != 0 ? $"\r\nBase: {bs}" : "") +
                       (overlay1Value != 0 ? $"\r\n{overlay1Stat}: {o1s} ({(overlay1Value / baseValue * 100):##0.##}% of base HP)" : "");
            }

            return (value <= uncappedValue
                       ? $"{vs}{(string.IsNullOrEmpty(dmgType) ? "" : " " + dmgType)} {statType}" +
                         (capValue > 0 ? $"({atName} {statType} cap: {cs})" : "")
                       : $"{uvs}{(string.IsNullOrEmpty(dmgType) ? "" : " " + dmgType)} {statType}, capped at {cs}"
                   ) +
                   (baseValue != 0 ? $"\r\nBase: {bs}" : "") +
                   (overlay1Value != 0 ? $"\r\n{overlay1Stat}: {o1s}" : "");
        }

        #endregion

        #region Bars/Labels indexes

        private ctlLayeredBar FetchBar(int n)
        {
            return n switch
            {
                0 => bar1,
                1 => bar2,
                2 => bar3,
                3 => bar4,
                4 => bar5,
                5 => bar6,
                6 => bar7,
                7 => bar8,
                8 => bar9,
                9 => bar10,
                10 => bar11,
                11 => bar12,
                12 => bar13,
                13 => bar14,
                14 => bar15,
                15 => bar16,
                16 => bar17,
                17 => bar18,
                18 => bar19,
                19 => bar20,
                20 => bar21,
                21 => bar22,
                22 => bar23,
                23 => bar24,
                24 => bar25,
                25 => bar26,
                26 => bar27,
                27 => bar28,
                28 => bar29,
                29 => bar30,
                30 => bar31,
                31 => bar32,
                32 => bar33,
                33 => bar34,
                34 => bar35,
                35 => bar36,
                36 => bar37,
                37 => bar38,
                38 => bar39,
                39 => bar40,
                40 => bar41,
                41 => bar42,
                42 => bar43,
                43 => bar44,
                44 => bar45,
                45 => bar46,
                46 => bar47,
                47 => bar48,
                48 => bar49,
                49 => bar50,
                50 => bar51,
                51 => bar52,
                52 => bar53,
                53 => bar54,
                54 => bar55,
                55 => bar56,
                56 => bar57,
                57 => bar58,
                58 => bar59,
                59 => bar60,
                60 => bar61,
                61 => bar62,
                62 => bar63,
                63 => bar64,
                64 => bar65,
                65 => bar66,
                66 => bar67,
                _ => bar1
            };
        }

        private ctlLayeredBar FetchBar(Enums.eBarType barType)
        {
            return FetchBar((int) barType);
        }

        private Label FetchLv(int n)
        {
            return n switch
            {
                0 => lv1,
                1 => lv2,
                2 => lv3,
                3 => lv4,
                4 => lv5,
                5 => lv6,
                6 => lv7,
                7 => lv8,
                8 => lv9,
                9 => lv10,
                10 => lv11,
                11 => lv12,
                12 => lv13,
                13 => lv14,
                14 => lv15,
                15 => lv16,
                16 => lv17,
                17 => lv18,
                18 => lv19,
                19 => lv20,
                20 => lv21,
                21 => lv22,
                22 => lv23,
                23 => lv24,
                24 => lv25,
                25 => lv26,
                26 => lv27,
                27 => lv28,
                28 => lv29,
                29 => lv30,
                30 => lv31,
                31 => lv32,
                32 => lv33,
                33 => lv34,
                34 => lv35,
                35 => lv36,
                36 => lv37,
                37 => lv38,
                38 => lv39,
                39 => lv40,
                40 => lv41,
                41 => lv42,
                42 => lv43,
                43 => lv44,
                44 => lv45,
                45 => lv46,
                46 => lv47,
                47 => lv48,
                48 => lv49,
                49 => lv50,
                50 => lv51,
                51 => lv52,
                52 => lv53,
                53 => lv54,
                54 => lv55,
                55 => lv56,
                56 => lv57,
                57 => lv58,
                58 => lv59,
                59 => lv60,
                60 => lv61,
                61 => lv62,
                62 => lv63,
                63 => lv64,
                64 => lv65,
                65 => lv66,
                66 => lv67,
                _ => lv1
            };
        }

        private Label FetchLv(Enums.eBarType barType)
        {
            return FetchLv((int) barType);
        }
        #endregion

        #region frmTotals import

        /*
        private void RbSpeedCheckedChanged(object sender, EventArgs e)
        {
            if (!MainModule.MidsController.IsAppInitialized)
                return;
            if (rbMPH.Checked)
                MidsContext.Config.SpeedFormat = Enums.eSpeedMeasure.MilesPerHour;
            else if (rbKPH.Checked)
                MidsContext.Config.SpeedFormat = Enums.eSpeedMeasure.KilometersPerHour;
            else if (rbFPS.Checked)
                MidsContext.Config.SpeedFormat = Enums.eSpeedMeasure.FeetPerSecond;
            else if (rbMSec.Checked)
                MidsContext.Config.SpeedFormat = Enums.eSpeedMeasure.MetersPerSecond;
            UpdateData();
        }
        */

        private void StoreLocation()
        {
            if (!MainModule.MidsController.IsAppInitialized) return;

            MainModule.MidsController.SzFrmTotals.X = Left;
            MainModule.MidsController.SzFrmTotals.Y = Top;
            MainModule.MidsController.SzFrmTotals.Width = Width;
            MainModule.MidsController.SzFrmTotals.Height = Height;
        }

        public void SetLocation()
        {
            Top = MainModule.MidsController.SzFrmTotals.X;
            Left = MainModule.MidsController.SzFrmTotals.Y;
        }

        #endregion

        public void UpdateData()
        {
            //pbClose.Refresh();
            //pbTopMost.Refresh();
            Statistics displayStats = MidsContext.Character.DisplayStats;
            Character.TotalStatistics uncappedStats = MidsContext.Character.Totals;
            Character.TotalStatistics cappedStats = MidsContext.Character.TotalsCapped;
            Stopwatch watch = Stopwatch.StartNew();
            tabControlAdv2.SuspendLayout();
            #region Bars setup
            FetchBar(Enums.eBarType.DefenseSmashing).ValueMainBar = displayStats.Defense((int)Enums.eDamage.Smashing);
            FetchBar(Enums.eBarType.DefenseLethal).ValueMainBar = displayStats.Defense((int)Enums.eDamage.Lethal);
            FetchBar(Enums.eBarType.DefenseFire).ValueMainBar = displayStats.Defense((int)Enums.eDamage.Fire);
            FetchBar(Enums.eBarType.DefenseCold).ValueMainBar = displayStats.Defense((int)Enums.eDamage.Cold);
            FetchBar(Enums.eBarType.DefenseEnergy).ValueMainBar = displayStats.Defense((int)Enums.eDamage.Energy);
            FetchBar(Enums.eBarType.DefenseNegative).ValueMainBar = displayStats.Defense((int)Enums.eDamage.Negative);
            FetchBar(Enums.eBarType.DefensePsionic).ValueMainBar = displayStats.Defense((int)Enums.eDamage.Psionic);
            FetchBar(Enums.eBarType.DefenseMelee).ValueMainBar = displayStats.Defense((int)Enums.eDamage.Melee);
            FetchBar(Enums.eBarType.DefenseRanged).ValueMainBar = displayStats.Defense((int)Enums.eDamage.Ranged);
            FetchBar(Enums.eBarType.DefenseAoE).ValueMainBar = displayStats.Defense((int)Enums.eDamage.AoE);

            FetchBar(Enums.eBarType.ResistanceSmashing).SuspendUpdate();
            FetchBar(Enums.eBarType.ResistanceSmashing).ValueMainBar = displayStats.DamageResistance((int)Enums.eDamage.Smashing, false);
            FetchBar(Enums.eBarType.ResistanceSmashing).ValueOverCap = displayStats.DamageResistance((int)Enums.eDamage.Smashing, true);
            FetchBar(Enums.eBarType.ResistanceSmashing).ResumeUpdate();
            FetchBar(Enums.eBarType.ResistanceLethal).SuspendUpdate();
            FetchBar(Enums.eBarType.ResistanceLethal).ValueMainBar = displayStats.DamageResistance((int)Enums.eDamage.Lethal, false);
            FetchBar(Enums.eBarType.ResistanceLethal).ValueOverCap = displayStats.DamageResistance((int)Enums.eDamage.Lethal, true);
            FetchBar(Enums.eBarType.ResistanceLethal).ResumeUpdate();
            FetchBar(Enums.eBarType.ResistanceFire).SuspendUpdate();
            FetchBar(Enums.eBarType.ResistanceFire).ValueMainBar = displayStats.DamageResistance((int)Enums.eDamage.Fire, false);
            FetchBar(Enums.eBarType.ResistanceFire).ValueOverCap = displayStats.DamageResistance((int)Enums.eDamage.Fire, true);
            FetchBar(Enums.eBarType.ResistanceFire).ResumeUpdate();
            FetchBar(Enums.eBarType.ResistanceCold).SuspendUpdate();
            FetchBar(Enums.eBarType.ResistanceCold).ValueMainBar = displayStats.DamageResistance((int)Enums.eDamage.Cold, false);
            FetchBar(Enums.eBarType.ResistanceCold).ValueOverCap = displayStats.DamageResistance((int)Enums.eDamage.Cold, true);
            FetchBar(Enums.eBarType.ResistanceCold).ResumeUpdate();
            FetchBar(Enums.eBarType.ResistanceEnergy).SuspendUpdate();
            FetchBar(Enums.eBarType.ResistanceEnergy).ValueMainBar = displayStats.DamageResistance((int)Enums.eDamage.Energy, false);
            FetchBar(Enums.eBarType.ResistanceEnergy).ValueOverCap = displayStats.DamageResistance((int)Enums.eDamage.Energy, true);
            FetchBar(Enums.eBarType.ResistanceEnergy).ResumeUpdate();
            FetchBar(Enums.eBarType.ResistanceNegative).SuspendUpdate();
            FetchBar(Enums.eBarType.ResistanceNegative).ValueMainBar = displayStats.DamageResistance((int)Enums.eDamage.Negative, false);
            FetchBar(Enums.eBarType.ResistanceNegative).ValueOverCap = displayStats.DamageResistance((int)Enums.eDamage.Negative, true);
            FetchBar(Enums.eBarType.ResistanceNegative).ResumeUpdate();
            FetchBar(Enums.eBarType.ResistanceToxic).SuspendUpdate();
            FetchBar(Enums.eBarType.ResistanceToxic).ValueMainBar = displayStats.DamageResistance((int)Enums.eDamage.Toxic, false);
            FetchBar(Enums.eBarType.ResistanceToxic).ValueOverCap = displayStats.DamageResistance((int)Enums.eDamage.Toxic, true);
            FetchBar(Enums.eBarType.ResistanceToxic).ResumeUpdate();
            FetchBar(Enums.eBarType.ResistancePsionic).SuspendUpdate();
            FetchBar(Enums.eBarType.ResistancePsionic).ValueMainBar = displayStats.DamageResistance((int)Enums.eDamage.Psionic, false);
            FetchBar(Enums.eBarType.ResistancePsionic).ValueOverCap = displayStats.DamageResistance((int)Enums.eDamage.Psionic, true);
            FetchBar(Enums.eBarType.ResistancePsionic).ResumeUpdate();

            FetchBar(Enums.eBarType.Regeneration).SuspendUpdate();
            FetchBar(Enums.eBarType.Regeneration).ValueBase = MidsContext.Character.Archetype.BaseRegen * 100;
            FetchBar(Enums.eBarType.Regeneration).ValueMainBar = displayStats.HealthRegenPercent(false);
            FetchBar(Enums.eBarType.Regeneration).ValueOverCap = displayStats.HealthRegenPercent(true);
            FetchBar(Enums.eBarType.Regeneration).ResumeUpdate();

            //Debug.WriteLine($"Regen: {MidsContext.Character.Archetype.BaseRegen} / {displayStats.HealthRegenPercent(false)} / {displayStats.HealthRegenPercent(true)}");

            FetchBar(Enums.eBarType.MaxHPAbsorb).SuspendUpdate();
            FetchBar(Enums.eBarType.MaxHPAbsorb).ValueBase = MidsContext.Character.Archetype.Hitpoints;
            FetchBar(Enums.eBarType.MaxHPAbsorb).ValueMainBar = displayStats.HealthHitpointsNumeric(false);
            FetchBar(Enums.eBarType.MaxHPAbsorb).ValueOverCap = displayStats.HealthHitpointsNumeric(true);
            FetchBar(Enums.eBarType.MaxHPAbsorb).ValueOverlay1 = Math.Min(displayStats.Absorb, MidsContext.Character.Archetype.Hitpoints);
            FetchBar(Enums.eBarType.MaxHPAbsorb).ResumeUpdate();

            FetchBar(Enums.eBarType.EndRec).SuspendUpdate();
            FetchBar(Enums.eBarType.EndRec).ValueBase = MidsContext.Character.Archetype.BaseRecovery;
            FetchBar(Enums.eBarType.EndRec).ValueMainBar = displayStats.EnduranceRecoveryNumeric;
            FetchBar(Enums.eBarType.EndRec).ValueOverCap = displayStats.EnduranceRecoveryNumericUncapped; //displayStats.EnduranceRecoveryNumeric;
            FetchBar(Enums.eBarType.EndRec).ResumeUpdate();
            
            FetchBar(Enums.eBarType.EndUse).ValueMainBar = displayStats.EnduranceUsage;

            FetchBar(Enums.eBarType.MaxEnd).SuspendUpdate();
            FetchBar(Enums.eBarType.MaxEnd).ValueBase = 100;
            FetchBar(Enums.eBarType.MaxEnd).ValueMainBar = displayStats.EnduranceMaxEnd;
            FetchBar(Enums.eBarType.MaxEnd).ResumeUpdate();

            ///////////////////////////////

            FetchBar(Enums.eBarType.RunSpeed).SuspendUpdate();
            FetchBar(Enums.eBarType.RunSpeed).ValueBase = Statistics.BaseRunSpeed; // ft/s
            FetchBar(Enums.eBarType.RunSpeed).ValueMainBar = displayStats.MovementRunSpeed(Enums.eSpeedMeasure.FeetPerSecond, false);
            FetchBar(Enums.eBarType.RunSpeed).ValueOverCap = displayStats.MovementRunSpeed(Enums.eSpeedMeasure.FeetPerSecond, true);
            FetchBar(Enums.eBarType.RunSpeed).ResumeUpdate();
            //Debug.WriteLine($"Run speed:\r\nBase: {Statistics.BaseRunSpeed}, Main: {displayStats.MovementRunSpeed(Enums.eSpeedMeasure.FeetPerSecond, false)}, Overcap: {displayStats.MovementRunSpeed(Enums.eSpeedMeasure.FeetPerSecond, true)}");

            FetchBar(Enums.eBarType.JumpSpeed).SuspendUpdate();
            FetchBar(Enums.eBarType.JumpSpeed).ValueBase = Statistics.BaseJumpSpeed;
            FetchBar(Enums.eBarType.JumpSpeed).ValueMainBar = displayStats.MovementJumpSpeed(Enums.eSpeedMeasure.FeetPerSecond, false);
            FetchBar(Enums.eBarType.JumpSpeed).ValueOverCap = displayStats.MovementJumpSpeed(Enums.eSpeedMeasure.FeetPerSecond, true);
            FetchBar(Enums.eBarType.JumpSpeed).ResumeUpdate();
            //Debug.WriteLine($"Jump speed:\r\nBase: {Statistics.BaseJumpSpeed}, Main: {displayStats.MovementJumpSpeed(Enums.eSpeedMeasure.FeetPerSecond, false)}, Overcap: {displayStats.MovementJumpSpeed(Enums.eSpeedMeasure.FeetPerSecond, true)}");

            FetchBar(Enums.eBarType.JumpHeight).SuspendUpdate();
            float jumpHeight = displayStats.MovementJumpHeight(Enums.eSpeedMeasure.FeetPerSecond);
            FetchBar(Enums.eBarType.JumpHeight).ValueBase = Statistics.BaseJumpHeight;
            FetchBar(Enums.eBarType.JumpHeight).ValueMainBar = jumpHeight;
            FetchBar(Enums.eBarType.JumpHeight).ValueOverCap = jumpHeight;
            FetchBar(Enums.eBarType.JumpHeight).ResumeUpdate();
            //Debug.WriteLine($"Jump height:\r\nBase: {Statistics.BaseJumpHeight}, Main: {displayStats.MovementJumpHeight(Enums.eSpeedMeasure.FeetPerSecond)}");

            FetchBar(Enums.eBarType.FlySpeed).SuspendUpdate();
            float flySpeed = displayStats.MovementFlySpeed(Enums.eSpeedMeasure.MilesPerHour, false);
            FetchBar(Enums.eBarType.FlySpeed).ValueBase = (flySpeed == 0) ? 0 : Statistics.BaseFlySpeed;
            FetchBar(Enums.eBarType.FlySpeed).ValueMainBar = flySpeed;
            FetchBar(Enums.eBarType.FlySpeed).ValueOverCap = displayStats.MovementFlySpeed(Enums.eSpeedMeasure.MilesPerHour, true);
            FetchBar(Enums.eBarType.FlySpeed).ResumeUpdate();

            ///////////////////////////////

            FetchBar(Enums.eBarType.StealthPvE).SuspendUpdate();
            FetchBar(Enums.eBarType.StealthPvE).ValueBase = 0;
            FetchBar(Enums.eBarType.StealthPvE).ValueMainBar = MidsContext.Character.Totals.StealthPvE;
            FetchBar(Enums.eBarType.StealthPvE).ValueOverCap = MidsContext.Character.Totals.StealthPvE;
            FetchBar(Enums.eBarType.StealthPvE).ResumeUpdate();
            FetchBar(Enums.eBarType.StealthPvP).SuspendUpdate();
            FetchBar(Enums.eBarType.StealthPvP).ValueBase = 0;
            FetchBar(Enums.eBarType.StealthPvP).ValueMainBar = MidsContext.Character.Totals.StealthPvP;
            FetchBar(Enums.eBarType.StealthPvP).ValueOverCap = MidsContext.Character.Totals.StealthPvP;
            FetchBar(Enums.eBarType.StealthPvP).ResumeUpdate();
            FetchBar(Enums.eBarType.Perception).SuspendUpdate();
            FetchBar(Enums.eBarType.Perception).ValueBase = Statistics.BasePerception;
            FetchBar(Enums.eBarType.Perception).ValueMainBar = displayStats.Perception(false);
            FetchBar(Enums.eBarType.Perception).ValueOverCap = displayStats.Perception(true);
            FetchBar(Enums.eBarType.Perception).ResumeUpdate();

            ///////////////////////////////

            FetchBar(Enums.eBarType.Haste).SuspendUpdate();
            FetchBar(Enums.eBarType.Haste).ValueBase = 100;
            FetchBar(Enums.eBarType.Haste).ValueMainBar = displayStats.BuffHaste(false);
            FetchBar(Enums.eBarType.Haste).ValueOverCap = displayStats.BuffHaste(true);
            FetchBar(Enums.eBarType.Haste).ResumeUpdate();
            FetchBar(Enums.eBarType.ToHit).ValueMainBar = displayStats.BuffToHit;
            FetchBar(Enums.eBarType.Accuracy).ValueMainBar = displayStats.BuffAccuracy;
            FetchBar(Enums.eBarType.Damage).SuspendUpdate();
            FetchBar(Enums.eBarType.Damage).ValueBase = 100;
            FetchBar(Enums.eBarType.Damage).ValueMainBar = displayStats.BuffDamage(false); // Need to add +100 here ?
            FetchBar(Enums.eBarType.Damage).ValueOverCap = displayStats.BuffDamage(true); // Need to add +100 here ?
            FetchBar(Enums.eBarType.Damage).ResumeUpdate();
            FetchBar(Enums.eBarType.EndRdx).ValueMainBar = displayStats.BuffEndRdx;
            FetchBar(Enums.eBarType.ThreatLevel).SuspendUpdate();
            FetchBar(Enums.eBarType.ThreatLevel).ValueBase = MidsContext.Character.Archetype.BaseThreat;
            FetchBar(Enums.eBarType.ThreatLevel).ValueMainBar = displayStats.ThreatLevel;
            FetchBar(Enums.eBarType.ThreatLevel).ResumeUpdate();
            FetchBar(Enums.eBarType.Elusivity).ValueMainBar = MidsContext.Character.Totals.Elusivity;

            ///////////////////////////////

            FetchBar(Enums.eBarType.MezProtectionHold).ValueMainBar = Math.Abs(MidsContext.Character.Totals.Mez[(int) Enums.eMez.Held]);
            FetchBar(Enums.eBarType.MezProtectionStunned).ValueMainBar = Math.Abs(MidsContext.Character.Totals.Mez[(int) Enums.eMez.Stunned]);
            FetchBar(Enums.eBarType.MezProtectionSleep).ValueMainBar = Math.Abs(MidsContext.Character.Totals.Mez[(int) Enums.eMez.Sleep]);
            FetchBar(Enums.eBarType.MezProtectionImmob).ValueMainBar = Math.Abs(MidsContext.Character.Totals.Mez[(int) Enums.eMez.Immobilized]);
            FetchBar(Enums.eBarType.MezProtectionKnockback).ValueMainBar = Math.Abs(MidsContext.Character.Totals.Mez[(int) Enums.eMez.Knockback]);
            FetchBar(Enums.eBarType.MezProtectionRepel).ValueMainBar = Math.Abs(MidsContext.Character.Totals.Mez[(int) Enums.eMez.Repel]);
            FetchBar(Enums.eBarType.MezProtectionConfuse).ValueMainBar = Math.Abs(MidsContext.Character.Totals.Mez[(int) Enums.eMez.Confused]);
            FetchBar(Enums.eBarType.MezProtectionFear).ValueMainBar = Math.Abs(MidsContext.Character.Totals.Mez[(int) Enums.eMez.Terrorized]);
            FetchBar(Enums.eBarType.MezProtectionTaunt).ValueMainBar = Math.Abs(MidsContext.Character.Totals.Mez[(int) Enums.eMez.Taunt]);
            FetchBar(Enums.eBarType.MezProtectionPlacate).ValueMainBar = Math.Abs(MidsContext.Character.Totals.Mez[(int) Enums.eMez.Placate]);
            FetchBar(Enums.eBarType.MezProtectionTeleport).ValueMainBar = Math.Abs(MidsContext.Character.Totals.Mez[(int) Enums.eMez.Teleport]);

            FetchBar(Enums.eBarType.MezResistanceHold).ValueMainBar = MidsContext.Character.Totals.MezRes[(int) Enums.eMez.Held];
            FetchBar(Enums.eBarType.MezResistanceStunned).ValueMainBar = MidsContext.Character.Totals.MezRes[(int) Enums.eMez.Stunned];
            FetchBar(Enums.eBarType.MezResistanceSleep).ValueMainBar = MidsContext.Character.Totals.MezRes[(int) Enums.eMez.Sleep];
            FetchBar(Enums.eBarType.MezResistanceImmob).ValueMainBar = MidsContext.Character.Totals.MezRes[(int) Enums.eMez.Immobilized];
            FetchBar(Enums.eBarType.MezResistanceKnockback).ValueMainBar = MidsContext.Character.Totals.MezRes[(int) Enums.eMez.Knockback];
            FetchBar(Enums.eBarType.MezResistanceRepel).ValueMainBar = MidsContext.Character.Totals.MezRes[(int) Enums.eMez.Repel];
            FetchBar(Enums.eBarType.MezResistanceConfuse).ValueMainBar = MidsContext.Character.Totals.MezRes[(int) Enums.eMez.Confused];
            FetchBar(Enums.eBarType.MezResistanceFear).ValueMainBar = MidsContext.Character.Totals.MezRes[(int) Enums.eMez.Terrorized];
            FetchBar(Enums.eBarType.MezResistanceTaunt).ValueMainBar = MidsContext.Character.Totals.MezRes[(int) Enums.eMez.Taunt];
            FetchBar(Enums.eBarType.MezResistancePlacate).ValueMainBar = MidsContext.Character.Totals.MezRes[(int) Enums.eMez.Placate];
            FetchBar(Enums.eBarType.MezResistanceTeleport).ValueMainBar = MidsContext.Character.Totals.MezRes[(int) Enums.eMez.Teleport];

            ///////////////////////////////

            FetchBar(Enums.eBarType.DebuffResistanceDefense).ValueMainBar = MidsContext.Character.Totals.DebuffRes[(int) Enums.eEffectType.Defense];
            FetchBar(Enums.eBarType.DebuffResistanceEndurance).ValueMainBar = MidsContext.Character.Totals.DebuffRes[(int) Enums.eEffectType.Endurance];
            FetchBar(Enums.eBarType.DebuffResistanceRecovery).ValueMainBar = MidsContext.Character.Totals.DebuffRes[(int) Enums.eEffectType.Recovery];
            FetchBar(Enums.eBarType.DebuffResistancePerception).ValueMainBar = MidsContext.Character.Totals.DebuffRes[(int) Enums.eEffectType.PerceptionRadius];
            FetchBar(Enums.eBarType.DebuffResistanceToHit).ValueMainBar = MidsContext.Character.Totals.DebuffRes[(int) Enums.eEffectType.ToHit];
            FetchBar(Enums.eBarType.DebuffResistanceRechargeTime).ValueMainBar = MidsContext.Character.Totals.DebuffRes[(int) Enums.eEffectType.RechargeTime];
            FetchBar(Enums.eBarType.DebuffResistanceSpeedRunning).ValueMainBar = MidsContext.Character.Totals.DebuffRes[(int) Enums.eEffectType.SpeedRunning];
            FetchBar(Enums.eBarType.DebuffResistanceRegen).ValueMainBar = MidsContext.Character.Totals.DebuffRes[(int) Enums.eEffectType.Regeneration];

            #endregion

            #region Labels setup

            FetchLv(Enums.eBarType.DefenseSmashing).Text = FormatValue(0, displayStats.Defense(1));
            FetchLv(Enums.eBarType.DefenseLethal).Text = FormatValue(0, displayStats.Defense(2));
            FetchLv(Enums.eBarType.DefenseFire).Text = FormatValue(0, displayStats.Defense(3));
            FetchLv(Enums.eBarType.DefenseCold).Text = FormatValue(0, displayStats.Defense(4));
            FetchLv(Enums.eBarType.DefenseEnergy).Text = FormatValue(0, displayStats.Defense(5));
            FetchLv(Enums.eBarType.DefenseNegative).Text = FormatValue(0, displayStats.Defense(6));
            FetchLv(Enums.eBarType.DefensePsionic).Text = FormatValue(0, displayStats.Defense(8));
            FetchLv(Enums.eBarType.DefenseMelee).Text = FormatValue(0, displayStats.Defense(10));
            FetchLv(Enums.eBarType.DefenseRanged).Text = FormatValue(0, displayStats.Defense(11));
            FetchLv(Enums.eBarType.DefenseAoE).Text = FormatValue(0, displayStats.Defense(12));

            
            FetchLv(Enums.eBarType.ResistanceSmashing).Text = FormatValue(0, displayStats.DamageResistance(1, false));
            FetchLv(Enums.eBarType.ResistanceLethal).Text = FormatValue(0, displayStats.DamageResistance(2, false));
            FetchLv(Enums.eBarType.ResistanceFire).Text = FormatValue(0, displayStats.DamageResistance(3, false));
            FetchLv(Enums.eBarType.ResistanceCold).Text = FormatValue(0, displayStats.DamageResistance(4, false));
            FetchLv(Enums.eBarType.ResistanceEnergy).Text = FormatValue(0, displayStats.DamageResistance(5, false));
            FetchLv(Enums.eBarType.ResistanceNegative).Text = FormatValue(0, displayStats.DamageResistance(6, false));
            FetchLv(Enums.eBarType.ResistanceToxic).Text = FormatValue(0, displayStats.DamageResistance(7, false));
            FetchLv(Enums.eBarType.ResistancePsionic).Text = FormatValue(0, displayStats.DamageResistance(8, false));

            FetchLv(Enums.eBarType.Regeneration).Text = FormatValue(0, displayStats.HealthRegenPercent(false));
            FetchLv(Enums.eBarType.MaxHPAbsorb).Text = FormatValue(1, displayStats.HealthHitpointsNumeric(false));

            FetchLv(Enums.eBarType.EndRec).Text = FormatValue(1, displayStats.EnduranceRecoveryNumeric) + "/s";
            FetchLv(Enums.eBarType.EndUse).Text = FormatValue(1, displayStats.EnduranceUsage) + "/s";
            FetchLv(Enums.eBarType.MaxEnd).Text = FormatValue(0, displayStats.EnduranceMaxEnd);

            ///////////////////////////////

            FetchLv(Enums.eBarType.RunSpeed).Text = FormatValue(1, displayStats.MovementRunSpeed(Enums.eSpeedMeasure.MilesPerHour, false)) + "mph";
            FetchLv(Enums.eBarType.JumpSpeed).Text = FormatValue(1, displayStats.MovementJumpSpeed(Enums.eSpeedMeasure.MilesPerHour, false)) + "mph";
            FetchLv(Enums.eBarType.JumpHeight).Text = FormatValue(1, displayStats.MovementJumpHeight(Enums.eSpeedMeasure.FeetPerSecond)) + "ft";
            FetchLv(Enums.eBarType.FlySpeed).Text = FormatValue(1, displayStats.MovementFlySpeed(Enums.eSpeedMeasure.MilesPerHour, false)) + "mph";

            ///////////////////////////////

            FetchLv(Enums.eBarType.StealthPvE).Text = FormatValue(1, MidsContext.Character.Totals.StealthPvE) + "ft"; // ???
            FetchLv(Enums.eBarType.StealthPvP).Text = FormatValue(1, MidsContext.Character.Totals.StealthPvP) + "ft"; // ???
            FetchLv(Enums.eBarType.Perception).Text = FormatValue(1, displayStats.Perception(false)) + "ft";

            ///////////////////////////////

            FetchLv(Enums.eBarType.Haste).Text = FormatValue(0, displayStats.BuffHaste(false));
            FetchLv(Enums.eBarType.ToHit).Text = FormatValue(0, displayStats.BuffToHit);
            FetchLv(Enums.eBarType.Accuracy).Text = FormatValue(0, displayStats.BuffAccuracy);
            FetchLv(Enums.eBarType.Damage).Text = FormatValue(0, displayStats.BuffDamage(false)); // Need to add +100 here ?
            FetchLv(Enums.eBarType.EndRdx).Text = FormatValue(0, displayStats.BuffEndRdx);
            FetchLv(Enums.eBarType.ThreatLevel).Text = Convert.ToString(displayStats.ThreatLevel, CultureInfo.InvariantCulture); // ???
            FetchLv(Enums.eBarType.Elusivity).Text = FormatValue(0, MidsContext.Character.Totals.Elusivity);

            ///////////////////////////////

            FetchLv(Enums.eBarType.MezProtectionHold).Text = FormatValue(3, MidsContext.Character.Totals.Mez[(int) Enums.eMez.Held]);
            FetchLv(Enums.eBarType.MezProtectionStunned).Text = FormatValue(3, MidsContext.Character.Totals.Mez[(int) Enums.eMez.Stunned]);
            FetchLv(Enums.eBarType.MezProtectionSleep).Text = FormatValue(3, MidsContext.Character.Totals.Mez[(int) Enums.eMez.Sleep]);
            FetchLv(Enums.eBarType.MezProtectionImmob).Text = FormatValue(3, MidsContext.Character.Totals.Mez[(int) Enums.eMez.Immobilized]);
            FetchLv(Enums.eBarType.MezProtectionKnockback).Text = FormatValue(3, MidsContext.Character.Totals.Mez[(int) Enums.eMez.Knockback]);
            FetchLv(Enums.eBarType.MezProtectionRepel).Text = FormatValue(3, MidsContext.Character.Totals.Mez[(int) Enums.eMez.Repel]);
            FetchLv(Enums.eBarType.MezProtectionConfuse).Text = FormatValue(3, MidsContext.Character.Totals.Mez[(int) Enums.eMez.Confused]);
            FetchLv(Enums.eBarType.MezProtectionFear).Text = FormatValue(3, MidsContext.Character.Totals.Mez[(int) Enums.eMez.Terrorized]);
            FetchLv(Enums.eBarType.MezProtectionTaunt).Text = FormatValue(3, MidsContext.Character.Totals.Mez[(int) Enums.eMez.Taunt]);
            FetchLv(Enums.eBarType.MezProtectionPlacate).Text = FormatValue(3, MidsContext.Character.Totals.Mez[(int) Enums.eMez.Placate]);
            FetchLv(Enums.eBarType.MezProtectionTeleport).Text = FormatValue(3, MidsContext.Character.Totals.Mez[(int) Enums.eMez.Teleport]);

            FetchLv(Enums.eBarType.MezResistanceHold).Text = FormatValue(0, MidsContext.Character.Totals.MezRes[(int) Enums.eMez.Held]);
            FetchLv(Enums.eBarType.MezResistanceStunned).Text = FormatValue(0, MidsContext.Character.Totals.MezRes[(int) Enums.eMez.Stunned]);
            FetchLv(Enums.eBarType.MezResistanceSleep).Text = FormatValue(0, MidsContext.Character.Totals.MezRes[(int) Enums.eMez.Sleep]);
            FetchLv(Enums.eBarType.MezResistanceImmob).Text = FormatValue(0, MidsContext.Character.Totals.MezRes[(int) Enums.eMez.Immobilized]);
            FetchLv(Enums.eBarType.MezResistanceKnockback).Text = FormatValue(0, MidsContext.Character.Totals.MezRes[(int) Enums.eMez.Knockback]);
            FetchLv(Enums.eBarType.MezResistanceRepel).Text = FormatValue(0, MidsContext.Character.Totals.MezRes[(int) Enums.eMez.Repel]);
            FetchLv(Enums.eBarType.MezResistanceConfuse).Text = FormatValue(0, MidsContext.Character.Totals.MezRes[(int) Enums.eMez.Confused]);
            FetchLv(Enums.eBarType.MezResistanceFear).Text = FormatValue(0, MidsContext.Character.Totals.MezRes[(int) Enums.eMez.Terrorized]);
            FetchLv(Enums.eBarType.MezResistanceTaunt).Text = FormatValue(0, MidsContext.Character.Totals.MezRes[(int) Enums.eMez.Taunt]);
            FetchLv(Enums.eBarType.MezResistancePlacate).Text = FormatValue(0, MidsContext.Character.Totals.MezRes[(int) Enums.eMez.Placate]);
            FetchLv(Enums.eBarType.MezResistanceTeleport).Text = FormatValue(0, MidsContext.Character.Totals.MezRes[(int) Enums.eMez.Teleport]);

            ///////////////////////////////

            FetchLv(Enums.eBarType.DebuffResistanceDefense).Text = FormatValue(0, MidsContext.Character.Totals.DebuffRes[(int)Enums.eEffectType.Defense]);
            FetchLv(Enums.eBarType.DebuffResistanceEndurance).Text = FormatValue(0, MidsContext.Character.Totals.DebuffRes[(int)Enums.eEffectType.Endurance]);
            FetchLv(Enums.eBarType.DebuffResistanceRecovery).Text = FormatValue(0, MidsContext.Character.Totals.DebuffRes[(int)Enums.eEffectType.Recovery]);
            FetchLv(Enums.eBarType.DebuffResistancePerception).Text = FormatValue(0, MidsContext.Character.Totals.DebuffRes[(int)Enums.eEffectType.PerceptionRadius]);
            FetchLv(Enums.eBarType.DebuffResistanceToHit).Text = FormatValue(0, MidsContext.Character.Totals.DebuffRes[(int)Enums.eEffectType.ToHit]);
            FetchLv(Enums.eBarType.DebuffResistanceRechargeTime).Text = FormatValue(0, MidsContext.Character.Totals.DebuffRes[(int)Enums.eEffectType.RechargeTime]);
            FetchLv(Enums.eBarType.DebuffResistanceSpeedRunning).Text = FormatValue(0, MidsContext.Character.Totals.DebuffRes[(int)Enums.eEffectType.SpeedRunning]);
            FetchLv(Enums.eBarType.DebuffResistanceRegen).Text = FormatValue(0, MidsContext.Character.Totals.DebuffRes[(int)Enums.eEffectType.Regeneration]);

            #endregion

            #region Tooltips setup
            FetchBar(Enums.eBarType.DefenseSmashing).Tip = FormatValue(0, displayStats.Defense(1)) + " Smashing Defense";
            FetchBar(Enums.eBarType.DefenseLethal).Tip = FormatValue(0, displayStats.Defense(2)) + " Lethal Defense";
            FetchBar(Enums.eBarType.DefenseFire).Tip = FormatValue(0, displayStats.Defense(3)) + " Fire Defense";
            FetchBar(Enums.eBarType.DefenseCold).Tip = FormatValue(0, displayStats.Defense(4)) + " Cold Defense";
            FetchBar(Enums.eBarType.DefenseEnergy).Tip = FormatValue(0, displayStats.Defense(5)) + " Energy Defense";
            FetchBar(Enums.eBarType.DefenseNegative).Tip = FormatValue(0, displayStats.Defense(6)) + " Negative Defense";
            FetchBar(Enums.eBarType.DefensePsionic).Tip = FormatValue(0, displayStats.Defense(8)) + " Psionic Defense";
            FetchBar(Enums.eBarType.DefenseMelee).Tip = FormatValue(0, displayStats.Defense(10)) + " Melee Defense";
            FetchBar(Enums.eBarType.DefenseRanged).Tip = FormatValue(0, displayStats.Defense(11)) + " Ranged Defense";
            FetchBar(Enums.eBarType.DefenseAoE).Tip = FormatValue(0, displayStats.Defense(12)) + " AoE Defense";

            ///////////////////////////////

            FetchBar(Enums.eBarType.ResistanceSmashing).Tip = FormatValue(
                0,
                displayStats.DamageResistance(1, true),
                displayStats.DamageResistance(1, false),
                MidsContext.Character.Archetype.ResCap * 100,
                "Resistance",
                "Smashing",
                MidsContext.Character.Archetype.DisplayName);

            FetchBar(Enums.eBarType.ResistanceLethal).Tip = FormatValue(
                0,
                displayStats.DamageResistance(2, true),
                displayStats.DamageResistance(2, false),
                MidsContext.Character.Archetype.ResCap * 100,
                "Resistance",
                "Lethal",
                MidsContext.Character.Archetype.DisplayName);

            FetchBar(Enums.eBarType.ResistanceFire).Tip = FormatValue(
                0,
                displayStats.DamageResistance(3, true),
                displayStats.DamageResistance(3, false),
                MidsContext.Character.Archetype.ResCap * 100,
                "Resistance",
                "Fire",
                MidsContext.Character.Archetype.DisplayName);

            FetchBar(Enums.eBarType.ResistanceCold).Tip = FormatValue(
                0,
                displayStats.DamageResistance(4, true),
                displayStats.DamageResistance(4, false),
                MidsContext.Character.Archetype.ResCap * 100,
                "Resistance",
                "Cold",
                MidsContext.Character.Archetype.DisplayName);

            FetchBar(Enums.eBarType.ResistanceEnergy).Tip = FormatValue(
                0,
                displayStats.DamageResistance(5, true),
                displayStats.DamageResistance(5, false),
                MidsContext.Character.Archetype.ResCap * 100,
                "Resistance",
                "Energy",
                MidsContext.Character.Archetype.DisplayName);

            FetchBar(Enums.eBarType.ResistanceNegative).Tip = FormatValue(
                0,
                displayStats.DamageResistance(6, true),
                displayStats.DamageResistance(6, false),
                MidsContext.Character.Archetype.ResCap * 100,
                "Resistance",
                "Negative",
                MidsContext.Character.Archetype.DisplayName);

            FetchBar(Enums.eBarType.ResistanceToxic).Tip = FormatValue(
                0,
                displayStats.DamageResistance(7, true),
                displayStats.DamageResistance(7, false),
                MidsContext.Character.Archetype.ResCap * 100,
                "Resistance",
                "Toxic",
                MidsContext.Character.Archetype.DisplayName);

            FetchBar(Enums.eBarType.ResistancePsionic).Tip = FormatValue(
                0,
                displayStats.DamageResistance(8, true),
                displayStats.DamageResistance(8, false),
                MidsContext.Character.Archetype.ResCap * 100,
                "Resistance",
                "Psionic",
                MidsContext.Character.Archetype.DisplayName);

            ///////////////////////////////

            FetchBar(Enums.eBarType.Regeneration).Tip = FormatValue(
                0,
                displayStats.HealthRegenPercent(false),
                displayStats.HealthRegenPercent(true),
                MidsContext.Character.Archetype.BaseRegen * 100,
                MidsContext.Character.Archetype.RegenCap * 100,
                "Regeneration",
                "",
                MidsContext.Character.Archetype.DisplayName);

            FetchBar(Enums.eBarType.MaxHPAbsorb).Tip = FormatValue(
                1,
                displayStats.HealthHitpointsNumeric(false),
                displayStats.HealthHitpointsNumeric(true),
                MidsContext.Character.Archetype.Hitpoints,
                MidsContext.Character.Archetype.HPCap,
                Math.Min(displayStats.Absorb, MidsContext.Character.Archetype.Hitpoints),
                "HP",
                "Absorb",
                "",
                MidsContext.Character.Archetype.DisplayName);

            FetchBar(Enums.eBarType.EndRec).Tip = FormatValue(
                4,
                displayStats.EnduranceRecoveryNumeric,
                displayStats.EnduranceRecoveryNumericUncapped,
                MidsContext.Character.Archetype.BaseRecovery,
                MidsContext.Character.Archetype.RecoveryCap,
                "End. Recovery",
                "",
                MidsContext.Character.Archetype.DisplayName);

            FetchBar(Enums.eBarType.EndUse).Tip = "End. use: " + FormatValue(1, displayStats.EnduranceUsage) + "/s";

            FetchBar(Enums.eBarType.MaxEnd).Tip = FormatValue(
                1,
                displayStats.EnduranceMaxEnd,
                displayStats.EnduranceMaxEnd,
                100,
                0,
                "Max End",
                "",
                MidsContext.Character.Archetype.DisplayName);
            #endregion

            tabControlAdv2.ResumeLayout();

            watch.Stop();
            Debug.WriteLine($"frmTotalsV2.UpdateData(): {watch.ElapsedMilliseconds}ms");
        }
    }
}