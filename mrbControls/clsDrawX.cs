using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Windows.Forms;
using mrbBase;
using mrbBase.Base.Display;
using mrbBase.Base.Master_Classes;

namespace mrbControls
{
    public class clsDrawX
    {
        private const string GfxPath = "images\\";

        private const string GfxPowerFn = "pSlot";

        private const string GfxFileExt = ".png";

        private const string NewSlotName = "Newslot.png";

        // Horizontal space between power slots
        private const int PaddingX = 15;

        // Vertical space between power slots
        private const int PaddingY = 25;

        // Vertical offset for enhancement slots
        public const int OffsetY = 23;

        // Horizontal offset for enhancement slots
        private const int OffsetX = 30;

        private const int OffsetInherent = 10;

        // Same size as target drawing area
        private Size szBuffer;

        // Size of a power slot
        public Size SzPower { get; }

        // Size of an enhancement slot
        public Size szSlot;

        // Surface to draw on before combining to display
        public ExtendedBitmap bxBuffer;

        // List of disabled, empty, filled, waiting
        public readonly List<ExtendedBitmap> bxPower;

        // The unplaced enhancement slot image
        private readonly ExtendedBitmap bxNewSlot;

        // Graphics object of target drawing surface (The panel)
        private Graphics gTarget;


        // Column variables
        private const int vcPowers = 24;
        private int vcCols;
        private int vcRowsPowers;

        // Recoloring variables
        private ColorMatrix pColorMatrix;
        public ImageAttributes pImageAttributes;

        // Scaling variables
        private float ScaleValue;
        public bool Scaling { get; private set; }

        public static readonly float[][] heroMatrix =
        {
            new[]
            {
                1f, 0f, 0f, 0f, 0f
            },
            new[]
            {
                0f, 1f, 0f, 0f, 0f
            },
            new[]
            {
                0f, 0f, 1f, 0f, 0f
            },
            new[]
            {
                0f, 0f, 0f, 1f, 0f
            },
            new[]
            {
                0f, 0f, 0f, 0f, 1f
            }
        };
        private static readonly float[][] villainMatrix =
        {
            new[]
            {
                0.45f, 0, 0, 0, 0
            },
            new[]
            {
                0, 0.35f, 0, 0, 0
            },
            new[]
            {
                0.75f, 0, 0, 0.175f, 0
            },
            new[]
            {
                0, 0, 0, 1f, 0
            },
            new[]
            {
                0, 0, 0, 0, 1f
            }
        };
        private const int icoOffset = 32;
        private Color BackColor;
        private Control cTarget;
        private Font _defaultFont;
        public int Highlight;
        public Enums.eInterfaceMode InterfaceMode;
        
        //bool VillainColor;
        
        public clsDrawX(Control iTarget)
        {
            InterfaceMode = 0;
            //VillainColor = false;
            ScaleValue = 2f;
            Scaling = true;
            vcCols = 6;
            vcRowsPowers = 24;
            bxPower = new List<ExtendedBitmap>();
            checked
            {
                var filePath = $"{FileIO.AddSlash(Application.StartupPath)}{GfxPath}";
                DirectoryInfo directoryInfo = new DirectoryInfo(filePath);
                foreach (var file in directoryInfo.GetFiles($"{GfxPowerFn}*{GfxFileExt}"))
                {
                    bxPower.Add(new ExtendedBitmap($"{filePath}{file}"));
                }

                ColorSwitch();
                InitColumns = MidsContext.Config.Columns; 
                SzPower = bxPower[0].Size;
                foreach (var pg in bxPower)
                {
                    pg.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    pg.Graphics.InterpolationMode = InterpolationMode.Bicubic;
                    pg.Graphics.CompositingMode = CompositingMode.SourceOver;
                    pg.Graphics.CompositingQuality = CompositingQuality.HighQuality;
                    pg.Graphics.SmoothingMode = SmoothingMode.HighQuality;
                    pg.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                    pg.Graphics.PageUnit = GraphicsUnit.Pixel;
                }
                szSlot = new Size(30, 30);
                szBuffer = GetMaxDrawingArea();
                var size = new Size(szBuffer.Width, szBuffer.Height);
                bxBuffer = new ExtendedBitmap(size);
                bxBuffer.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                bxBuffer.Graphics.CompositingQuality = CompositingQuality.HighQuality;
                bxBuffer.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                bxBuffer.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                bxBuffer.Graphics.SmoothingMode = SmoothingMode.HighQuality;
                bxBuffer.Graphics.CompositingMode = CompositingMode.SourceOver;
                bxBuffer.Graphics.PageUnit = GraphicsUnit.Pixel;
                szBuffer = GetRequiredDrawingArea();
                bxNewSlot = new ExtendedBitmap(FileIO.AddSlash(Application.StartupPath) + GfxPath + NewSlotName);
                gTarget = iTarget.CreateGraphics();
                gTarget.PixelOffsetMode = PixelOffsetMode.HighQuality;
                gTarget.CompositingQuality = CompositingQuality.HighQuality;
                gTarget.InterpolationMode = InterpolationMode.HighQualityBicubic;
                gTarget.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                gTarget.SmoothingMode = SmoothingMode.HighQuality;
                gTarget.CompositingMode = CompositingMode.SourceOver;
                gTarget.PageUnit = GraphicsUnit.Pixel;
                cTarget = iTarget;
                //DefaultFont = new Font(iTarget.Font.FontFamily, iTarget.Font.Size, FontStyle.Bold, iTarget.Font.Unit);
                _defaultFont = new Font("Arial", 12.5f, FontStyle.Bold, GraphicsUnit.Pixel, 0);
                BackColor = iTarget.BackColor;
                if (szBuffer.Height < cTarget.Height)
                {
                    gTarget.FillRectangle(new SolidBrush(BackColor), 0, szBuffer.Height, cTarget.Width, cTarget.Height - szBuffer.Height);
                }
            }
        }

        public bool EpicColumns => MidsContext.Character != null && MidsContext.Character.Archetype != null && MidsContext.Character.Archetype.ClassType == Enums.eClassType.HeroEpic;

       public int Columns
        {
            set
            {
                MiniSetCol(value);
                Blank();
                szBuffer = GetRequiredDrawingArea();
                SetScaling(cTarget.Size);
            }
        }

        private int InitColumns
        {
            set
            {
                if (value == vcCols)
                    return;
                if ((value < 2) | (value > 6))
                    return;
                vcCols = value;
                vcRowsPowers = vcPowers / vcCols;
            }
        }

        public void ReInit(Control iTarget)
        {
            gTarget = iTarget.CreateGraphics();
            gTarget.PixelOffsetMode = PixelOffsetMode.HighQuality;
            gTarget.CompositingQuality = CompositingQuality.HighQuality;
            gTarget.InterpolationMode = InterpolationMode.HighQualityBicubic;
            gTarget.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            gTarget.SmoothingMode = SmoothingMode.HighQuality;
            gTarget.CompositingMode = CompositingMode.SourceOver;
            gTarget.PageUnit = GraphicsUnit.Pixel;
            cTarget = iTarget;
            //DefaultFont = new Font(iTarget.Font.FontFamily, iTarget.Font.Size, FontStyle.Bold, iTarget.Font.Unit);
            _defaultFont = new Font("Arial", 12.5f, FontStyle.Bold, GraphicsUnit.Pixel, 0);
            BackColor = iTarget.BackColor;
        }

        private void DrawSplit()
        {
            var pen = new Pen(Color.Goldenrod, 2f);
            checked
            {
                int iValue;
                switch (vcCols)
                {
                    case 2:
                        iValue = OffsetInherent + vcRowsPowers * (SzPower.Height + 27);
                        bxBuffer.Graphics.DrawLine(pen, 2, ScaleDown(iValue), ScaleDown(GetDrawingArea().Width), ScaleDown(iValue));
                        bxBuffer.Graphics.DrawString("Inherent Powers",
                            new Font("Arial", 13f, FontStyle.Regular, GraphicsUnit.Pixel),
                            MidsContext.Character.IsHero()
                                ? new SolidBrush(Color.DodgerBlue)
                                : new SolidBrush(Color.Red), ScaleDown(GetDrawingArea().Width / 2 - 50),
                            ScaleDown(iValue));
                        break;
                    case 3:
                        iValue = OffsetInherent + vcRowsPowers * (SzPower.Height + 27);
                        bxBuffer.Graphics.DrawLine(pen, 2, ScaleDown(iValue), ScaleDown(GetDrawingArea().Width), ScaleDown(iValue));
                        bxBuffer.Graphics.DrawString("Inherent Powers",
                            new Font("Arial", 13f, FontStyle.Regular, GraphicsUnit.Pixel),
                            MidsContext.Character.IsHero()
                                ? new SolidBrush(Color.DodgerBlue)
                                : new SolidBrush(Color.Red), ScaleDown(GetDrawingArea().Width / 2 - 50),
                            ScaleDown(iValue));
                        break;
                    case 4:
                        iValue = OffsetInherent + vcRowsPowers * (SzPower.Height + 27);
                        bxBuffer.Graphics.DrawLine(pen, 2, ScaleDown(iValue), ScaleDown(GetDrawingArea().Width), ScaleDown(iValue));
                        bxBuffer.Graphics.DrawString("Inherent Powers",
                            new Font("Arial", 13f, FontStyle.Regular, GraphicsUnit.Pixel),
                            MidsContext.Character.IsHero()
                                ? new SolidBrush(Color.DodgerBlue)
                                : new SolidBrush(Color.Red), ScaleDown(GetDrawingArea().Width / 2 - 50),
                            ScaleDown(iValue));
                        break;
                    case 5:
                        iValue = OffsetInherent + vcRowsPowers * (SzPower.Height + 48);
                        bxBuffer.Graphics.DrawLine(pen, 2, ScaleDown(iValue), ScaleDown(GetDrawingArea().Width), ScaleDown(iValue));
                        bxBuffer.Graphics.DrawString("Inherent Powers",
                            new Font("Arial", 13f, FontStyle.Regular, GraphicsUnit.Pixel),
                            MidsContext.Character.IsHero()
                                ? new SolidBrush(Color.DodgerBlue)
                                : new SolidBrush(Color.Red), ScaleDown(GetDrawingArea().Width / 2 - 50),
                            ScaleDown(iValue));
                        break;
                    case 6:
                        iValue = OffsetInherent + vcRowsPowers * (SzPower.Height + 27);
                        bxBuffer.Graphics.DrawLine(pen, 2, ScaleDown(iValue), ScaleDown(GetDrawingArea().Width), ScaleDown(iValue));
                        bxBuffer.Graphics.DrawString("Inherent Powers",
                            new Font("Arial", 13f, FontStyle.Regular, GraphicsUnit.Pixel),
                            MidsContext.Character.IsHero()
                                ? new SolidBrush(Color.DodgerBlue)
                                : new SolidBrush(Color.Red), ScaleDown(GetDrawingArea().Width / 2 - 50),
                            ScaleDown(iValue));
                        break;
                }
            }
        }

        private void DrawPowers()
        {
            checked
            {
                for (var i = 0; i < MidsContext.Character.CurrentBuild.Powers.Count; i++)
                    if (MidsContext.Character.CanPlaceSlot & (Highlight == i))
                    {
                        var currentBuild = MidsContext.Character.CurrentBuild;
                        var powers = currentBuild.Powers;
                        var index = i;
                        var value = powers[index];
                        DrawPowerSlot(ref value, true);
                        currentBuild.Powers[index] = value;
                    }
                    else if (MidsContext.Character.CurrentBuild.Powers[i].Chosen)
                    {
                        var currentBuild = MidsContext.Character.CurrentBuild;
                        var powers2 = currentBuild.Powers;
                        var index = i;
                        var value = powers2[index];
                        DrawPowerSlot(ref value);
                        currentBuild.Powers[index] = value;
                    }
                    else if (MidsContext.Character.CurrentBuild.Powers[i].Power != null && (string.CompareOrdinal(MidsContext.Character.CurrentBuild.Powers[i].Power.GroupName, "Incarnate") == 0) | MidsContext.Character.CurrentBuild.Powers[i].Power.IncludeFlag)
                    {
                        var currentBuild = MidsContext.Character.CurrentBuild;
                        var powers3 = currentBuild.Powers;
                        var index = i;
                        var value = powers3[index];
                        DrawPowerSlot(ref value);
                        currentBuild.Powers[index] = value;
                    }

                Application.DoEvents();
                DrawSplit();
            }
        }

        private float FontScale(float iSZ)
        {
            return Math.Min(ScaleDown(iSZ) * 1.1f, iSZ);
        }

        private static int IndexFromLevel()
        {
            return MidsContext.Character.CurrentBuild.Powers.FindIndex(pow =>
                pow?.Level == MidsContext.Character.RequestedLevel);
        }

        private void DrawEnhancementLevel(SlotEntry slot, Font font, Graphics g, ref RectangleF rect)
        {
            unchecked
            {
                var enhType = DatabaseAPI.Database.Enhancements[slot.Enhancement.Enh].TypeID;
                if (enhType == Enums.eType.SetO || enhType == Enums.eType.InventO)
                {
                    var iValue2 = rect;
                    iValue2.Y -= 5f;
                    iValue2.Height = _defaultFont.GetHeight(bxBuffer.Graphics);
                    var relativeLevelNumeric = "";
                    var enhInternalName = DatabaseAPI.Database.Enhancements[slot.Enhancement.Enh].UID;
                    var catalystSet = DatabaseAPI.EnhHasCatalyst(enhInternalName) || DatabaseAPI.EnhIsNaturallyAttuned(slot.Enhancement.Enh);
                    // Catalysed enhancements take character level no matter what.
                    // Game does not allow boosters over enhancement catalysts.
                    if (!catalystSet & slot.Enhancement.RelativeLevel > Enums.eEnhRelative.Even & MidsContext.Config.ShowEnhRel)
                    {
                        relativeLevelNumeric = Enums.GetRelativeString(slot.Enhancement.RelativeLevel, false);
                    }

                    // If enhancement has boosters, need to stretch the level drawing zone a little,
                    // or relative level doesn't fit in.
                    if (!string.IsNullOrEmpty(relativeLevelNumeric))
                    {
                        iValue2.Width += 10f;
                        iValue2.X -= 5f;
                    }

                    var iStr = (MidsContext.Config.I9.HideIOLevels ? string.Empty : Convert.ToString(checked(slot.Enhancement.IOLevel + 1))) + relativeLevelNumeric;
                    if (!catalystSet)
                    {
                        var bounds = ScaleDown(iValue2);
                        var cyan = Color.Cyan;
                        var outline = Color.FromArgb(128, 0, 0, 0);
                        var outlineSpace = 1f;
                        g = bxBuffer.Graphics;

                        DrawOutlineText(iStr, bounds, cyan, outline, font, outlineSpace, g);
                    }
                }
                else if (enhType == Enums.eType.Normal || enhType == Enums.eType.SpecialO)
                {
                    var iValue2 = rect;
                    iValue2.Y -= 5f;
                    iValue2.Height = _defaultFont.GetHeight(bxBuffer.Graphics);
                    Color color;

                    if (slot.Enhancement.RelativeLevel == 0)
                    {
                        color = Color.Red;
                    }
                    else if (slot.Enhancement.RelativeLevel < Enums.eEnhRelative.Even)
                    {
                        color = Color.Yellow;
                    }
                    else if (slot.Enhancement.RelativeLevel > Enums.eEnhRelative.Even)
                    {
                        color = Color.FromArgb(0, 255, 0);
                    }
                    else
                    {
                        color = Color.White;
                    }

                    string relativeString;
                    // Always display relative level if present
                    //if (MidsContext.Config.ShowEnhRel)
                    relativeString = Enums.GetRelativeString(slot.Enhancement.RelativeLevel, MidsContext.Config.ShowRelSymbols);

                    // +3 SO do not exist ingame, at least not through combinations.
                    // Display flat level instead.
                    if (slot.Enhancement.RelativeLevel == Enums.eEnhRelative.PlusThree)
                    {
                        // I dunno. MidsContext.Character.Level == 48 ?
                        // Zed 07/07 - Having issues using MidsContext.Character.Level
                        //relativeString = Convert.ToString(Math.Max(53, MidsContext.Character.Level + 5), null);
                        relativeString = "53";
                    }
                    else if (MidsContext.Config.ShowSOLevels)
                    {
                        // It isn't slot.Enhancement.IOLevel... always set to 0
                        // Improvisation it is...
                        //relativeString = Convert.ToString(Math.Max(50, MidsContext.Character.Level + 2), null) + relativeString;
                        relativeString = "50" + relativeString;
                    }

                    if (
                        //MidsContext.Config.ShowEnhRel &&
                        MidsContext.Config.ShowSOLevels &&
                        slot.Enhancement.RelativeLevel != Enums.eEnhRelative.None &&
                        slot.Enhancement.RelativeLevel != Enums.eEnhRelative.Even &&
                        slot.Enhancement.RelativeLevel != Enums.eEnhRelative.PlusThree
                    )
                    {
                        iValue2.Width += 10f;
                        iValue2.X -= 5f;
                    }

                    if (!string.IsNullOrEmpty(relativeString))
                    {
                        var bounds2 = ScaleDown(iValue2);
                        var outline2 = Color.FromArgb(128, 0, 0, 0);
                        var outlineSpace2 = 1f;

                        DrawOutlineText(relativeString, bounds2, color, outline2, font, outlineSpace2, g);
                    }
                }
            }
        }

        public Point DrawPowerSlot(ref PowerEntry iSlot, bool singleDraw = false)
        {
            Brush pBrush = new SolidBrush(BackColor);
            Pen pPen = new Pen(Color.FromArgb(128, 0, 0, 0), 1f);
            string pString = string.Empty;
            string sString = string.Empty;
            RectangleF pBounds = new RectangleF(0f, 0f, 0f, 0f);
            StringFormat pStringFormat = new StringFormat(StringFormatFlags.NoWrap | StringFormatFlags.NoClip)
            {
                Trimming = StringTrimming.None
            };

            Pen indPen = new Pen(Color.Black);
            Rectangle indRect = new Rectangle();
            Font font;
            var style = !MidsContext.Config.RtFont.PowersBold ? FontStyle.Regular : FontStyle.Bold;
            font = MidsContext.Config.RtFont.PowersBase > 0 ? new Font(_defaultFont.FontFamily, FontScale(MidsContext.Config.RtFont.PowersBase), style, GraphicsUnit.Point, 0) : new Font(_defaultFont.FontFamily, FontScale(8), style, GraphicsUnit.Point, 0);

            int newSlotLevel = MidsContext.Character.SlotCheck(iSlot);
            bool toggling = InterfaceMode == Enums.eInterfaceMode.PowerToggle;
            RectangleF i9Bounds;
            var displayState = iSlot.State;

            bool canPlaceSlot = MidsContext.Character.CanPlaceSlot;
            var drawNewSlot = iSlot.Power != null && (iSlot.State != Enums.ePowerState.Empty && canPlaceSlot) && iSlot.Slots.Length < 6 && singleDraw && iSlot.Power.Slottable & InterfaceMode != Enums.eInterfaceMode.PowerToggle;
            Point location = PowerPosition(iSlot);
            Point slotLocation = default;
            checked
            {
                slotLocation.X = (int) Math.Round(location.X - OffsetX + checked(SzPower.Width - szSlot.Width * 6) / 2.0);
                slotLocation.Y = location.Y + OffsetY;
                Graphics graphics = bxBuffer.Graphics;
                Rectangle clipRect = new Rectangle(slotLocation.X, slotLocation.Y, SzPower.Width, SzPower.Height);
                graphics.FillRectangle(pBrush, ScaleDown(clipRect));

                if (!toggling)
                {
                    if (iSlot.Power != null)
                    {
                        if (singleDraw && newSlotLevel > -1 && canPlaceSlot &&
                            InterfaceMode != Enums.eInterfaceMode.PowerToggle && iSlot.PowerSet != null &&
                            iSlot.Slots.Length < 6 && iSlot.Power.Slottable)
                        {
                            displayState = Enums.ePowerState.Open;
                        }
                        else if (iSlot.Chosen & !canPlaceSlot & InterfaceMode != Enums.eInterfaceMode.PowerToggle &
                                 Highlight == MidsContext.Character.CurrentBuild.Powers.IndexOf(iSlot))
                        {
                            displayState = Enums.ePowerState.Open;
                        }
                    }
                    else if (MidsContext.Character.CurrentBuild.Powers.IndexOf(iSlot) == IndexFromLevel())
                    {
                        displayState = Enums.ePowerState.Open;
                    }
                }

                pBounds.Height = szSlot.Height;
                pBounds.Width = szSlot.Width;
                pStringFormat.Alignment = StringAlignment.Center;
                pStringFormat.LineAlignment = StringAlignment.Center;
                bool isGrey = false;
                ImageAttributes imageAttr;
                if (toggling)
                {
                    if (displayState == Enums.ePowerState.Open)
                    {
                        displayState = Enums.ePowerState.Empty;
                    }

                    if (iSlot.StatInclude & displayState == Enums.ePowerState.Used)
                    {
                        displayState = Enums.ePowerState.Open;
                        isGrey = iSlot.Level >= MidsContext.Config.ForceLevel;
                        imageAttr = GreySlot(isGrey, true);
                    }
                    else if (iSlot.CanIncludeForStats())
                    {
                        isGrey = (iSlot.Level >= MidsContext.Config.ForceLevel);
                        imageAttr = GreySlot(isGrey);
                    }
                    else
                    {
                        imageAttr = GreySlot(true);
                        isGrey = true;
                    }
                }
                else
                {
                    isGrey = (iSlot.Level >= MidsContext.Config.ForceLevel);
                    imageAttr = GreySlot(isGrey);
                }

                Rectangle rBounds = new Rectangle(location.X, location.Y, bxPower[(int) displayState].Size.Width,
                    bxPower[(int) displayState].Size.Height);
                Image bitmap = !MidsContext.Character.IsHero() ? bxPower[4].Bitmap : bxPower[2].Bitmap;
                if (displayState == Enums.ePowerState.Used || toggling)
                {
                    if (!MidsContext.Config.DisableDesaturateInherent & !iSlot.Chosen)
                    {
                        imageAttr = Desaturate(isGrey, displayState == Enums.ePowerState.Open);
                    }

                    graphics.DrawImage(bitmap, ScaleDown(rBounds), 0, 0, bxPower[(int) displayState].ClipRect.Width,
                        bxPower[(int) displayState].ClipRect.Height, GraphicsUnit.Pixel, imageAttr);
                }
                else
                {
                    graphics.DrawImage(bitmap, ScaleDown(rBounds), 0, 0, bxPower[(int) displayState].ClipRect.Width,
                        bxPower[(int) displayState].ClipRect.Height, GraphicsUnit.Pixel);
                }

                if (iSlot.CanIncludeForStats())
                {
                    indRect.Height = 15;
                    indRect.Width = indRect.Height;
                    indRect.Y = (int) Math.Round(rBounds.Top + checked(rBounds.Height - indRect.Height) / 2.0);
                    indRect.X = (int) Math.Round(rBounds.Right -
                                                 (indRect.Width + checked(rBounds.Height - indRect.Height) / 2.0));
                    indRect = ScaleDown(indRect);
                    PathGradientBrush brush2;
                    if (iSlot.StatInclude)
                    {
                        Rectangle iRect = indRect;
                        PointF iCenter = new PointF(-0.25f, -0.33f);
                        brush2 = MakePathBrush(iRect, iCenter, Color.FromArgb(96, 255, 96), Color.FromArgb(0, 32, 0));
                    }
                    else
                    {
                        Rectangle iRect2 = indRect;
                        PointF iCenter = new PointF(-0.25f, -0.33f);
                        brush2 = MakePathBrush(iRect2, iCenter, Color.FromArgb(96, 96, 96), Color.FromArgb(0, 0, 0));
                    }

                    graphics.FillEllipse(brush2, indRect);
                    graphics.DrawEllipse(indPen, indRect);
                }

                //
                SolidBrush solidBrush;
                //if (!System.Diagnostics.Debugger.IsAttached || !this.IsInDesignMode() || !System.Diagnostics.Process.GetCurrentProcess().ProcessName.ToLowerInvariant().Contains("devenv"))
                var inDesigner = Process.GetCurrentProcess().ProcessName.ToLowerInvariant().Contains("devenv");
                for (var i = 0; i <= iSlot.Slots.Length - 1; i++)
                {
                    var slot = iSlot.Slots[i];
                    // Enhancment spacing and position?
                    pBounds.X = slotLocation.X + (szSlot.Width + 2) * i;
                    pBounds.Y = slotLocation.Y;
                    //
                    if (slot.Enhancement.Enh < 0)
                    {
                        Rectangle clipRect2 = new Rectangle((int) Math.Round(pBounds.X), slotLocation.Y, szSlot.Width,
                            szSlot.Height);
                        // New slot rectangle
                        graphics.DrawImage(I9Gfx.EnhTypes.Bitmap, ScaleDown(clipRect2), 0, 0, szSlot.Width,
                            szSlot.Height, GraphicsUnit.Pixel, pImageAttributes);
                        if (MidsContext.Config.CalcEnhLevel == 0 | slot.Level >= MidsContext.Config.ForceLevel |
                            (InterfaceMode == Enums.eInterfaceMode.PowerToggle & !iSlot.StatInclude) |
                            (!iSlot.AllowFrontLoading & slot.Level < iSlot.Level))
                        {
                            solidBrush = new SolidBrush(Color.FromArgb(160, 0, 0, 0));
                            graphics.FillEllipse(solidBrush, ScaleDown(pBounds));
                            graphics.DrawEllipse(pPen, ScaleDown(pBounds));
                        }
                    }
                    else
                    {
                        if (inDesigner) continue;
                        IEnhancement enhancement = DatabaseAPI.Database.Enhancements[slot.Enhancement.Enh];
                        Rectangle clipRect2 = new Rectangle((int) Math.Round(pBounds.X), slotLocation.Y, szSlot.Width,
                            szSlot.Height);
                        I9Gfx.DrawEnhancementAt(ref graphics, ScaleDown(clipRect2), enhancement.ImageIdx,
                            I9Gfx.ToGfxGrade(enhancement.TypeID, slot.Enhancement.Grade));
                        if (slot.Enhancement.RelativeLevel == 0 | slot.Level >= MidsContext.Config.ForceLevel |
                            (InterfaceMode == Enums.eInterfaceMode.PowerToggle & !iSlot.StatInclude) |
                            (!iSlot.AllowFrontLoading & slot.Level < iSlot.Level) |
                            (MidsContext.EnhCheckMode & !slot.Enhancement.Obtained))
                        {
                            solidBrush = new SolidBrush(Color.FromArgb(160, 0, 0, 0));
                            i9Bounds = pBounds;
                            i9Bounds.Inflate(1f, 1f);
                            graphics.FillEllipse(solidBrush, ScaleDown(i9Bounds));
                        }

                        if (slot.Enhancement.Enh > -1)
                            DrawEnhancementLevel(slot, font, graphics, ref pBounds);
                    }

                    if (!MidsContext.Config.ShowSlotLevels) continue;

                    i9Bounds = pBounds;
                    unchecked
                    {
                        //Positioning of slot level text
                        i9Bounds.Y += i9Bounds.Height + 12;
                        i9Bounds.Height = _defaultFont.GetHeight(bxBuffer.Graphics);
                        i9Bounds.Y -= i9Bounds.Height;
                        i9Bounds.X += i9Bounds.Width;
                        i9Bounds.X -= i9Bounds.Width - 1;
                    }

                    DrawOutlineText(
                        iStr: Convert.ToString(slot.Level + 1),
                        bounds: ScaleDown(i9Bounds),
                        textColor: Color.FromArgb(0, 255, 0),
                        outlineColor: Color.FromArgb(192, 0, 0, 0),
                        bFont: font,
                        outlineSpace: 1f,
                        g: graphics);
                }

                //Draws the new slot hover
                if (newSlotLevel > -1 && (displayState != Enums.ePowerState.Empty && drawNewSlot))
                {
                    i9Bounds = new Rectangle(slotLocation.X + (szSlot.Width + 2) * (iSlot.Slots.Length), slotLocation.Y,
                        szSlot.Width, szSlot.Height);
                    graphics.DrawImage(bxNewSlot.Bitmap, ScaleDown(i9Bounds));
                    i9Bounds.Height = _defaultFont.GetHeight(bxBuffer.Graphics);
                    i9Bounds.Y += (szSlot.Height - i9Bounds.Height) / 2f;
                    DrawOutlineText(Convert.ToString(newSlotLevel + 1), ScaleDown(i9Bounds),
                        Color.FromArgb(0, 255, 255), Color.FromArgb(192, 0, 0, 0), font, 1f, graphics);
                }

                //Power name text positioning
                solidBrush = new SolidBrush(Color.White);
                pStringFormat = new StringFormat();
                pBounds.X = location.X + 10;
                pBounds.Y = location.Y + 4;
                pBounds.Width = SzPower.Width;
                pBounds.Height = _defaultFont.GetHeight() * 2f;
                Enums.ePowerState ePowerState2 = iSlot.State;
                if (ePowerState2 == Enums.ePowerState.Empty & displayState == Enums.ePowerState.Open)
                {
                    ePowerState2 = displayState;
                }

                switch (ePowerState2)
                {
                    case 0:
                        solidBrush = new SolidBrush(Color.Transparent);
                        pString = "";
                        break;
                    case Enums.ePowerState.Empty:
                        solidBrush = new SolidBrush(Color.WhiteSmoke);
                        pString = "(" + Convert.ToString(iSlot.Level + 1) + ")";
                        break;
                    case Enums.ePowerState.Used:
                        switch (iSlot.PowerSet.SetType)
                        {
                            case Enums.ePowerSetType.Primary:
                                sString = "";
                                break;
                            case Enums.ePowerSetType.Secondary:
                                sString = "";
                                break;
                            case Enums.ePowerSetType.Ancillary:
                                sString = "";
                                break;
                            case Enums.ePowerSetType.Inherent:
                                sString = "";
                                break;
                            case Enums.ePowerSetType.Pool:
                                sString = "";
                                break;
                        }

                        solidBrush = !MidsContext.Character.IsHero()
                            ? new SolidBrush(Color.White)
                            : new SolidBrush(Color.Black);

                        pString = iSlot.Virtual
                            ? iSlot.Name
                            : string.Concat("(", Convert.ToString(iSlot.Level + 1), ") ", iSlot.Name, " ", sString);
                        break;
                    case Enums.ePowerState.Open:
                        solidBrush = new SolidBrush(Color.WhiteSmoke);
                        pString = "(" + Convert.ToString(iSlot.Level + 1) + ")";
                        break;
                }

                if (displayState == Enums.ePowerState.Empty & iSlot.State == Enums.ePowerState.Used)
                {
                    solidBrush = new SolidBrush(Color.WhiteSmoke);
                }

                if (InterfaceMode == Enums.eInterfaceMode.PowerToggle && solidBrush.Color == Color.Black &&
                    !iSlot.CanIncludeForStats())
                {
                    solidBrush = new SolidBrush(Color.FromArgb(128, 0, 0, 0));
                }

                pStringFormat.FormatFlags |= StringFormatFlags.NoWrap;
                if (MidsContext.Config.EnhanceVisibility)
                {
                    i9Bounds = ScaleDown(pBounds);
                    Color outlineColor = Color.Black;
                    Color textColor = Color.White;
                    float outlineSpace5 = 1f;
                    graphics.CompositingQuality = CompositingQuality.HighQuality;
                    graphics.SmoothingMode = SmoothingMode.HighQuality;
                    graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                    graphics.PageUnit = GraphicsUnit.Pixel;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    DrawOutlineText(pString, i9Bounds, textColor, outlineColor, font, outlineSpace5, graphics, false,
                        true);
                }
                else
                {
                    graphics.DrawString(pString, font, solidBrush, ScaleDown(pBounds), pStringFormat);
                }

                return location;
            }
        }

        private PathGradientBrush MakePathBrush(Rectangle iRect, PointF iCenter, Color iColor1, Color icolor2)
        {
            var num = (float) (iRect.Left + iRect.Width * 0.5);
            var num2 = (float) (iRect.Top + iRect.Height * 0.5);
            var graphicsPath = new GraphicsPath();
            graphicsPath.AddEllipse(iRect);
            PathGradientBrush pathGradientBrush;
            PathGradientBrush pathGradientBrush2;
            checked
            {
                var array = new Color[graphicsPath.PathPoints.GetUpperBound(0) + 1];
                var lowerBound = graphicsPath.PathPoints.GetLowerBound(0);
                var upperBound = graphicsPath.PathPoints.GetUpperBound(0);
                for (var i = lowerBound; i <= upperBound; i++) array[i] = icolor2;

                pathGradientBrush = new PathGradientBrush(graphicsPath)
                {
                    CenterColor = iColor1,
                    SurroundColors = array
                };
                pathGradientBrush2 = pathGradientBrush;
            }

            var centerPoint = new PointF((float) (num + (iCenter.X + iCenter.X * (iRect.Width * 0.5))),
                (float) (num2 + (iCenter.Y + iCenter.Y * (iRect.Height * 0.5))));
            pathGradientBrush2.CenterPoint = centerPoint;
            return pathGradientBrush;
        }

        public void FullRedraw()
        {
            ColorSwitch();
            BackColor = cTarget.BackColor;
            bxBuffer.Graphics.Clear(BackColor);
            DrawPowers();
            var location = new Point(0, 0);
            OutputUnscaled(ref bxBuffer, location);
            GC.Collect();
        }

        public void Refresh(Rectangle Clip)
        {
            OutputRefresh(Clip, Clip, GraphicsUnit.Pixel);
        }

        private int GetVisualIDX(int PowerIndex)
        {
            var nidpowerset = MidsContext.Character.CurrentBuild.Powers[PowerIndex].NIDPowerset;
            var idxpower = MidsContext.Character.CurrentBuild.Powers[PowerIndex].IDXPower;
            checked
            {
                if (nidpowerset > -1)
                {
                    int vIdx;
                    if (DatabaseAPI.Database.Powersets[nidpowerset].SetType == Enums.ePowerSetType.Inherent)
                    {
                        vIdx = DatabaseAPI.Database.Powersets[nidpowerset].Powers[idxpower].LocationIndex;
                    }
                    else
                    {
                        vIdx = -1;
                        for (var i = 0; i <= PowerIndex; i++)
                            if (MidsContext.Character.CurrentBuild.Powers[i].NIDPowerset > -1)
                            {
                                if (DatabaseAPI.Database
                                        .Powersets[MidsContext.Character.CurrentBuild.Powers[i].NIDPowerset].SetType !=
                                    Enums.ePowerSetType.Inherent)
                                    vIdx++;
                            }
                            else
                            {
                                vIdx++;
                            }
                    }

                    return vIdx;
                }
                else
                {
                    var vIdx = -1;
                    for (var i = 0; i <= PowerIndex; i++)
                        if (MidsContext.Character.CurrentBuild.Powers[i].NIDPowerset > -1)
                        {
                            if (DatabaseAPI.Database.Powersets[MidsContext.Character.CurrentBuild.Powers[i].NIDPowerset]
                                    .SetType !=
                                Enums.ePowerSetType.Inherent)
                                vIdx++;
                        }
                        else
                        {
                            vIdx++;
                        }

                    return vIdx;
                }
            }
        }

        public static void DrawOutlineText(string iStr, RectangleF bounds, Color textColor, Color outlineColor,
            Font bFont, float outlineSpace,
            Graphics g, bool smallMode = false, bool leftAlign = false)
        {
            var stringFormat = new StringFormat(StringFormatFlags.NoWrap)
            {
                LineAlignment = StringAlignment.Near,
                Alignment = leftAlign ? StringAlignment.Near : StringAlignment.Center
            };

            var brush = new SolidBrush(outlineColor);
            var layoutRectangle = bounds;
            var layoutRectangle2 = new RectangleF(layoutRectangle.X, layoutRectangle.Y, layoutRectangle.Width,
                bFont.GetHeight(g));
            layoutRectangle2.X -= outlineSpace;
            if (!smallMode)
                g.DrawString(iStr, bFont, brush, layoutRectangle2, stringFormat);
            layoutRectangle2.Y -= outlineSpace;
            g.DrawString(iStr, bFont, brush, layoutRectangle2, stringFormat);
            layoutRectangle2.X += outlineSpace;
            if (!smallMode)
                g.DrawString(iStr, bFont, brush, layoutRectangle2, stringFormat);
            layoutRectangle2.X += outlineSpace;
            g.DrawString(iStr, bFont, brush, layoutRectangle2, stringFormat);
            layoutRectangle2.Y += outlineSpace;
            if (!smallMode)
                g.DrawString(iStr, bFont, brush, layoutRectangle2, stringFormat);
            layoutRectangle2.Y += outlineSpace;
            g.DrawString(iStr, bFont, brush, layoutRectangle2, stringFormat);
            layoutRectangle2.X -= outlineSpace;
            if (!smallMode)
                g.DrawString(iStr, bFont, brush, layoutRectangle2, stringFormat);
            layoutRectangle2.X -= outlineSpace;
            g.DrawString(iStr, bFont, brush, layoutRectangle2, stringFormat);
            layoutRectangle2.Y -= outlineSpace;
            if (!smallMode)
                g.DrawString(iStr, bFont, brush, layoutRectangle2, stringFormat);
            g.DrawString(iStr, bFont, new SolidBrush(textColor), layoutRectangle, stringFormat);
        }

        public int WhichSlot(int iX, int iY)
        {
            checked
            {
                for (var i = 0; i <= MidsContext.Character.CurrentBuild.Powers.Count - 1; i++)
                {
                    Point point;
                    if (MidsContext.Character.CurrentBuild.Powers[i].Power == null ||
                        MidsContext.Character.CurrentBuild.Powers[i].Chosen)
                        point = PowerPosition(GetVisualIDX(i));
                    else
                        point = PowerPosition(i);
                    if (iX >= point.X && iY >= point.Y && iX < SzPower.Width + point.X && iY < point.Y + SzPower.Height + (PaddingY/2))
                        return i;
                }

                return -1;
            }
        }

        public int WhichEnh(int iX, int iY)
        {
            var oPower = -1;
            checked
            {
                Point point = default;
                for (var i = 0; i <= MidsContext.Character.CurrentBuild.Powers.Count - 1; i++)
                {
                    if (MidsContext.Character.CurrentBuild.Powers[i].Power == null ||
                        MidsContext.Character.CurrentBuild.Powers[i].Chosen)
                        point = PowerPosition(GetVisualIDX(i));
                    else
                        point = PowerPosition(i);

                    if (iX < point.X || iY < point.Y || iX >= SzPower.Width + point.X || iY >= point.Y + SzPower.Height + (PaddingY/2))
                        continue;
                    oPower = i;
                    break;
                }

                if (oPower <= -1)
                    return -1;
                {
                    var isValid = false;
                    if (iY >= point.Y + OffsetY) // Y boundary of enhancments
                    {
                        if (MidsContext.Character.CurrentBuild.Powers[oPower].NIDPowerset > -1)
                        {
                            if (DatabaseAPI.Database.Powersets[MidsContext.Character.CurrentBuild.Powers[oPower].NIDPowerset].Powers[MidsContext.Character.CurrentBuild.Powers[oPower].IDXPower].Slottable)
                                isValid = true;
                        }
                        else
                        {
                            isValid = true;
                        }
                    }

                    if (!isValid)
                        return -1;
                    iX -= point.X - checked(SzPower.Width - icoOffset * 8); //X boundary of enhancments
                    for (var i = 0; i <= MidsContext.Character.CurrentBuild.Powers[oPower].Slots.Length - 1; i++)
                    {
                        var iZ = (i + 1) * icoOffset;
                        if (iX <= iZ)
                            return i;
                    }

                    return -1;
                }
            }
        }

        public bool HighlightSlot(int idx, bool Force = false)
        {
            checked
            {
                if (MidsContext.Character.CurrentBuild.Powers.Count >= 1)
                {
                    if (Highlight == idx && !Force) return false;
                    if (idx != -1)
                    {
                        Build currentBuild;
                        PowerEntry value;
                        Point point2;
                        Rectangle iValue;
                        Rectangle rectangle;
                        if (Highlight != -1 && Highlight < MidsContext.Character.CurrentBuild.Powers.Count)
                        {
                            currentBuild = MidsContext.Character.CurrentBuild;
                            var powers = currentBuild.Powers;
                            var highlight = Highlight;
                            value = powers[highlight];
                            var point = DrawPowerSlot(ref value);
                            currentBuild.Powers[highlight] = value;
                            point2 = point;
                            iValue = new Rectangle(point2.X, point2.Y, SzPower.Width, SzPower.Height + PaddingY);
                            rectangle = ScaleDown(iValue);
                            DrawSplit();
                            Output(ref bxBuffer, rectangle, rectangle, GraphicsUnit.Pixel);
                        }

                        Highlight = idx;
                        currentBuild = MidsContext.Character.CurrentBuild;
                        value = currentBuild.Powers[idx];
                        var point3 = DrawPowerSlot(ref value, true);
                        currentBuild.Powers[idx] = value;
                        point2 = point3;
                        iValue = new Rectangle(point2.X, point2.Y, SzPower.Width, SzPower.Height + PaddingY);
                        rectangle = ScaleDown(iValue);
                        DrawSplit();
                        Output(ref bxBuffer, rectangle, rectangle, GraphicsUnit.Pixel);
                    }
                    else if (Highlight != -1)
                    {
                        var currentBuild = MidsContext.Character.CurrentBuild;
                        var powers2 = currentBuild.Powers;
                        var highlight = Highlight;
                        var value = powers2[highlight];
                        var point4 = DrawPowerSlot(ref value);
                        currentBuild.Powers[highlight] = value;
                        var point2 = point4;
                        var iValue = new Rectangle(point2.X, point2.Y, SzPower.Width, SzPower.Height + PaddingY);
                        var rectangle = ScaleDown(iValue);
                        DrawSplit();
                        Output(ref bxBuffer, rectangle, rectangle, GraphicsUnit.Pixel);
                        Highlight = idx;
                    }
                }
                else
                {
                    return false;
                }

                return false;
            }
        }

        private void Blank()
        {
            bxBuffer?.Graphics.Clear(BackColor);
        }

        public void SetScaling(Size iSize)
        {
            var origScaling = Scaling;
            var origScaleValue = ScaleValue;
            if ((iSize.Width < 10) | (iSize.Height < 10))
                return;
            var drawingArea = GetDrawingArea();
            if ((drawingArea.Width > iSize.Width) | (drawingArea.Height > iSize.Height))
            {
                // Enable scaling
                Scaling = true;
                if ((double) drawingArea.Width / iSize.Width > drawingArea.Height / (double) iSize.Height)
                {
                    // Shrink to fit width
                    ScaleValue = (float) (drawingArea.Width / (double) iSize.Width);
                }
                else
                {
                    // Fit height
                    ScaleValue = (float) (drawingArea.Height / (double) iSize.Height);
                }

                ResetTarget();
                bxBuffer.Graphics.CompositingQuality = CompositingQuality.HighQuality;
                bxBuffer.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                bxBuffer.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                bxBuffer.Graphics.SmoothingMode = SmoothingMode.HighQuality;
                bxBuffer.Graphics.CompositingMode = CompositingMode.SourceOver;

                if (ScaleValue != origScaleValue)
                {
                    FullRedraw();
                    bxBuffer.Graphics.CompositingQuality = CompositingQuality.HighQuality;
                    bxBuffer.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    bxBuffer.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    bxBuffer.Graphics.SmoothingMode = SmoothingMode.HighQuality;
                    bxBuffer.Graphics.CompositingMode = CompositingMode.SourceOver;
                }
            }
            else
            {
                bxBuffer.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                ScaleValue = 1f;
                ResetTarget();
                Scaling = false;
                if (origScaling != Scaling | origScaleValue != ScaleValue)
                {
                    FullRedraw();
                }
            }
        }

        private void ResetTarget()
        {
            bxBuffer.Graphics.TextRenderingHint = ScaleValue > 1.125 ? TextRenderingHint.SystemDefault : TextRenderingHint.ClearTypeGridFit;
            gTarget.Dispose();
            gTarget = cTarget.CreateGraphics();
            gTarget.InterpolationMode = InterpolationMode.HighQualityBicubic;
            gTarget.CompositingQuality = CompositingQuality.HighQuality;
            gTarget.CompositingMode = CompositingMode.SourceOver;
            gTarget.PixelOffsetMode = PixelOffsetMode.HighQuality;
            gTarget.SmoothingMode = SmoothingMode.HighQuality;
        }

        public int ScaleDown(int iValue)
        {
            // Take a full size value and convert it to a scaled value
            // for co-ordinates, etc.
            int result;
            if (!Scaling)
            {
                result = iValue;
            }
            else
            {
                iValue = checked((int) Math.Round(iValue / ScaleValue));
                result = iValue;
            }

            return result;
        }

        public int ScaleUp(int iValue)
        {
            // Take a full size value and convert it to a scaled value
            // for co-ordinates, etc.
            int result;
            if (!Scaling)
            {
                result = iValue;
            }
            else
            {
                iValue = checked((int) Math.Round(iValue * ScaleValue));
                result = iValue;
            }

            return result;
        }

        private float ScaleDown(float iValue)
        {
            // Take a full size value and convert it to a scaled value
            // for co-ordinates, etc.
            float result;
            if (!Scaling)
            {
                result = iValue;
            }
            else
            {
                iValue /= ScaleValue;
                result = iValue;
            }

            return result;
        }

        public Rectangle ScaleDown(Rectangle iValue)
        {
            // Take a full size value and convert it to a scaled value
            // for co-ordinates, etc.
            checked
            {
                Rectangle result;
                if (!Scaling)
                {
                    result = iValue;
                }
                else
                {
                    iValue.X = (int) Math.Round(iValue.X / ScaleValue);
                    iValue.Y = (int) Math.Round(iValue.Y / ScaleValue);
                    iValue.Width = (int) Math.Round(iValue.Width / ScaleValue);
                    iValue.Height = (int) Math.Round(iValue.Height / ScaleValue);
                    result = iValue;
                }

                return result;
            }
        }

        private RectangleF ScaleDown(RectangleF iValue)
        {
            // Take a full size value and convert it to a scaled value
            // for co-ordinates, etc.
            checked
            {
                if (!Scaling)
                    return iValue;
                iValue.X = (int) Math.Round(iValue.X / ScaleValue);
                iValue.Y = (int) Math.Round(iValue.Y / ScaleValue);
                iValue.Width = (int) Math.Round(iValue.Width / ScaleValue);
                iValue.Height = (int) Math.Round(iValue.Height / ScaleValue);
                return iValue;
            }
        }

        private void Output(ref ExtendedBitmap Buffer, Rectangle DestRect, Rectangle SrcRect, GraphicsUnit iUnit)
        {
            gTarget.DrawImage(Buffer.Bitmap, DestRect, SrcRect, iUnit);
        }

        private void OutputRefresh(Rectangle DestRect, Rectangle SrcRect, GraphicsUnit iUnit)
        {
            gTarget.DrawImage(bxBuffer.Bitmap, DestRect, SrcRect, iUnit);
        }

        private void OutputUnscaled(ref ExtendedBitmap Buffer, Point Location)
        {
            gTarget.DrawImageUnscaled(Buffer.Bitmap, Location);
        }

        public void ColorSwitch()
        {
            /*bool useHeroColors = true;
            if (MidsContext.Character != null)
                useHeroColors = MidsContext.Character.IsHero();
            if (MidsContext.Config.DisableVillainColors)
                useHeroColors = true;
            VillainColor = !useHeroColors;*/
            pColorMatrix = new ColorMatrix(heroMatrix);
            pImageAttributes ??= new ImageAttributes();
            pImageAttributes.SetColorMatrix(pColorMatrix);
        }

        public static ImageAttributes GetRecolorIa(bool hero)
        {
            var colorMatrix = new ColorMatrix(heroMatrix);
            var imageAttributes = new ImageAttributes();
            imageAttributes.SetColorMatrix(colorMatrix);
            return imageAttributes;
        }

        private ImageAttributes GreySlot(bool grey, bool bypassIa = false)
        {
            if (!grey) return bypassIa ? new ImageAttributes() : pImageAttributes;

            checked
            {
                var colorMatrix = new ColorMatrix(heroMatrix);
                var r = 0;
                do
                {
                    var c = 0;
                    do
                    {
                        if (!bypassIa) colorMatrix[r, c] = pColorMatrix[r, c];

                        colorMatrix[r, c] = (float) (colorMatrix[r, c] / 1.5);

                        c++;
                    } while (c <= 2);

                    r++;
                } while (r <= 2);

                var imageAttributes = new ImageAttributes();
                imageAttributes.SetColorMatrix(colorMatrix);
                return imageAttributes;
            }
        }

        private ImageAttributes Desaturate(bool Grey, bool BypassIA = false)
        {
            var tMM = new ColorMatrix(new[]
            {
                new[]
                {
                    0.299f, 0.299f, 0.299f, 0f, 0f
                },
                new[]
                {
                    0.587f, 0.587f, 0.587f, 0f, 0f
                },
                new[]
                {
                    0.114f, 0.114f, 0.114f, 0f, 0f
                },
                new[]
                {
                    0, 0, 0, 1f, 0
                },
                new[]
                {
                    0, 0, 0, 0, 1f
                }
            });
            var tCM = new ColorMatrix(heroMatrix);
            var r = 0;
            checked
            {
                do
                {
                    var c = 0;
                    do
                    {
                        //controls shading of inherents
                        if (!BypassIA) tCM[r, c] = (pColorMatrix[r, c] + tMM[r, c]) / 2f;

                        if (Grey) tCM[r, c] = (float) (tCM[r, c] / 1.5);

                        c++;
                    } while (c <= 2);

                    r++;
                } while (r <= 2);

                var imageAttributes = new ImageAttributes();
                imageAttributes.SetColorMatrix(tCM);
                return imageAttributes;
            }
        }

        public Rectangle PowerBoundsUnScaled(int hIdx)
        {
            // Returns unscaled bounds
            var rectangle = new Rectangle(0, 0, 1, 1);
            checked
            {
                Rectangle result;
                if ((hIdx < 0) | (hIdx > MidsContext.Character.CurrentBuild.Powers.Count - 1))
                {
                    result = rectangle;
                }
                else
                {
                    if (!MidsContext.Character.CurrentBuild.Powers[hIdx].Chosen &&
                        MidsContext.Character.CurrentBuild.Powers[hIdx].Power != null)
                        rectangle.Location = PowerPosition(hIdx);
                    else
                        rectangle.Location = PowerPosition(GetVisualIDX(hIdx));

                    rectangle.Width = SzPower.Width;
                    var num = rectangle.Y + OffsetY;
                    num += szSlot.Height;
                    rectangle.Height = num - rectangle.Y;
                    result = rectangle;
                }

                return result;
            }
        }

        public bool WithinPowerBar(Rectangle pBounds, Point e)
        {
            pBounds.Height = SzPower.Height;
            return e.X >= pBounds.Left && e.X < pBounds.Right && e.Y >= pBounds.Top && e.Y < pBounds.Bottom;
        }

        private Point PowerPosition(int powerEntryIdx)
        {
            return PowerPosition(MidsContext.Character.CurrentBuild.Powers[powerEntryIdx]);
        }

        private int[][] GetInherentGrid()
        {
            switch (vcCols)
            {
                case 2:
                    if (MidsContext.Character.Archetype.ClassType == Enums.eClassType.HeroEpic)
                        return new[]
                        {
                            new[]
                            {
                                0, 1
                            },
                            new[]
                            {
                                2, 3
                            },
                            new[]
                            {
                                4, 5
                            },
                            new[]
                            {
                                6, 7
                            },
                            new[]
                            {
                                8, 9
                            },
                            new[]
                            {
                                10, 11
                            },
                            new[]
                            {
                                12, 13
                            },
                            new[]
                            {
                                14, 15
                            },
                            new[]
                            {
                                16, 17
                            },
                            new[]
                            {
                                18, 19
                            },
                            new[]
                            {
                                20, 21
                            },
                            new[]
                            {
                                22, 23
                            },
                            new[]
                            {
                                24, 25
                            },
                            new[]
                            {
                                26, 27
                            },
                            new[]
                            {
                                28, 29
                            },
                            new[]
                            {
                                30, 31
                            },
                            new[]
                            {
                                32, 33
                            },
                            new[]
                            {
                                34, 35
                            },
                            new[]
                            {
                                36, 37
                            },
                            new[]
                            {
                                38, 39
                            },
                            new[]
                            {
                                40, 41
                            },
                            new[]
                            {
                                42, 43
                            },
                            new[]
                            {
                                44, 45
                            },
                            new[]
                            {
                                46, 47
                            },
                            new[]
                            {
                                48, 49
                            },
                            new[]
                            {
                                50, 51
                            },
                            new[]
                            {
                                52, 53
                            },
                            new[]
                            {
                                54, 55
                            },
                            new[]
                            {
                                56, 57
                            },
                            new[]
                            {
                                58, 59
                            }
                        };

                    return new[]
                    {
                        new[]
                        {
                            0, 1
                        },
                        new[]
                        {
                            2, 3
                        },
                        new[]
                        {
                            4, 5
                        },
                        new[]
                        {
                            6, 7
                        },
                        new[]
                        {
                            8, 9
                        },
                        new[]
                        {
                            10, 11
                        },
                        new[]
                        {
                            12, 13
                        },
                        new[]
                        {
                            14, 15
                        },
                        new[]
                        {
                            16, 17
                        },
                        new[]
                        {
                            18, 19
                        },
                        new[]
                        {
                            20, 21
                        },
                        new[]
                        {
                            22, 23
                        },
                        new[]
                        {
                            24, 25
                        },
                        new[]
                        {
                            26, 27
                        },
                        new[]
                        {
                            28, 29
                        },
                        new[]
                        {
                            30, 31
                        },
                        new[]
                        {
                            32, 33
                        },
                        new[]
                        {
                            34, 35
                        },
                        new[]
                        {
                            36, 37
                        },
                        new[]
                        {
                            38, 39
                        },
                        new[]
                        {
                            40, 41
                        },
                        new[]
                        {
                            42, 43
                        },
                        new[]
                        {
                            44, 45
                        },
                        new[]
                        {
                            46, 47
                        },
                        new[]
                        {
                            48, 49
                        },
                        new[]
                        {
                            50, 51
                        },
                        new[]
                        {
                            52, 53
                        },
                        new[]
                        {
                            54, 55
                        },
                        new[]
                        {
                            56, 57
                        },
                        new[]
                        {
                            58, 59
                        }
                    };
                case 4:
                    if (MidsContext.Character.Archetype.ClassType == Enums.eClassType.HeroEpic)
                        return new[]
                        {
                            new[]
                            {
                                0, 1, 2, 3
                            },
                            new[]
                            {
                                4, 5, 6, 7
                            },
                            new[]
                            {
                                8, 9, 10, 11
                            },
                            new[]
                            {
                                12, 13, 14, 15
                            },
                            new[]
                            {
                                16, 17, 18, 19
                            },
                            new[]
                            {
                                20, 21, 22, 23
                            },
                            new[]
                            {
                                24, 25, 26, 27
                            },
                            new[]
                            {
                                28, 29, 30, 31
                            },
                            new[]
                            {
                                32, 33, 34, 35
                            },
                            new[]
                            {
                                36, 37, 38, 39
                            },
                            new[]
                            {
                                40, 41, 42, 43
                            },
                            new[]
                            {
                                44, 45, 46, 47
                            },
                            new[]
                            {
                                48, 49, 50, 51
                            },
                            new[]
                            {
                                52, 53, 54, 55
                            },
                            new[]
                            {
                                56, 57, 58, 59
                            }
                        };

                    return new[]
                    {
                        new[]
                        {
                            0, 1, 2, 3
                        },
                        new[]
                        {
                            4, 5, 6, 7
                        },
                        new[]
                        {
                            8, 9, 10, 11
                        },
                        new[]
                        {
                            12, 13, 14, 15
                        },
                        new[]
                        {
                            16, 17, 18, 19
                        },
                        new[]
                        {
                            20, 21, 22, 23
                        },
                        new[]
                        {
                            24, 25, 26, 27
                        },
                        new[]
                        {
                            28, 29, 30, 31
                        },
                        new[]
                        {
                            32, 33, 34, 35
                        },
                        new[]
                        {
                            36, 37, 38, 39
                        },
                        new[]
                        {
                            40, 41, 42, 43
                        },
                        new[]
                        {
                            44, 45, 46, 47
                        },
                        new[]
                        {
                            48, 49, 50, 51
                        },
                        new[]
                        {
                            52, 53, 54, 55
                        },
                        new[]
                        {
                            56, 57, 58, 59
                        }
                    };
                case 5:
                    if (MidsContext.Character.Archetype.ClassType == Enums.eClassType.HeroEpic)
                        return new[]
                        {
                            new[]
                            {
                                0, 1, 2, 3, 4
                            },
                            new[]
                            {
                                5, 6, 7, 8, 9
                            },
                            new[]
                            {
                                10, 11, 12, 13, 14
                            },
                            new[]
                            {
                                15, 16, 17, 18, 19
                            },
                            new[]
                            {
                                20, 21, 22, 23, 24
                            },
                            new[]
                            {
                                25, 26, 27, 28, 29
                            },
                            new[]
                            {
                                30, 31, 32, 33, 34
                            },
                            new[]
                            {
                                35, 36, 37, 38, 39
                            },
                            new[]
                            {
                                40, 41, 42, 43, 44
                            },
                            new[]
                            {
                                45, 46, 47, 48, 49
                            },
                            new[]
                            {
                                50, 51, 52, 53, 54
                            },
                            new[]
                            {
                                55, 56, 57, 58, 59
                            }
                        };

                    return new[]
                    {
                        new[]
                        {
                            0, 1, 2, 3, 4
                        },
                        new[]
                        {
                            5, 6, 7, 8, 9
                        },
                        new[]
                        {
                            10, 11, 12, 13, 14
                        },
                        new[]
                        {
                            15, 16, 17, 18, 19
                        },
                        new[]
                        {
                            20, 21, 22, 23, 24
                        },
                        new[]
                        {
                            25, 26, 27, 28, 29
                        },
                        new[]
                        {
                            30, 31, 32, 33, 34
                        },
                        new[]
                        {
                            35, 36, 37, 38, 39
                        },
                        new[]
                        {
                            40, 41, 42, 43, 44
                        },
                        new[]
                        {
                            45, 46, 47, 48, 49
                        },
                        new[]
                        {
                            50, 51, 52, 53, 54
                        },
                        new[]
                        {
                            55, 56, 57, 58, 59
                        }
                    };
                case 6:
                    if (MidsContext.Character.Archetype.ClassType == Enums.eClassType.HeroEpic)
                        return new[]
                        {
                            new[]
                            {
                                0, 1, 2, 3, 4, 5
                            },
                            new[]
                            {
                                6, 7, 8, 9, 10, 11
                            },
                            new[]
                            {
                                12, 13, 14, 15, 16, 17
                            },
                            new[]
                            {
                                18, 19, 20, 21, 22, 23
                            },
                            new[]
                            {
                                24, 25, 26, 27, 28, 29
                            },
                            new[]
                            {
                                30, 31, 32, 33, 34, 35
                            },
                            new[]
                            {
                                36, 37, 38, 39, 40, 41
                            },
                            new[]
                            {
                                42, 43, 44, 45, 46, 47
                            },
                            new[]
                            {
                                48, 49, 50, 51, 52, 53
                            },
                            new[]
                            {
                                54, 55, 56, 57, 58, 59
                            }
                        };

                    return new[]
                    {
                        new[]
                        {
                            0, 1, 2, 3, 4, 5
                        },
                        new[]
                        {
                            6, 7, 8, 9, 10, 11
                        },
                        new[]
                        {
                            12, 13, 14, 15, 16, 17
                        },
                        new[]
                        {
                            18, 19, 20, 21, 22, 23
                        },
                        new[]
                        {
                            24, 25, 26, 27, 28, 29
                        },
                        new[]
                        {
                            30, 31, 32, 33, 34, 35
                        },
                        new[]
                        {
                            36, 37, 38, 39, 40, 41
                        },
                        new[]
                        {
                            42, 43, 44, 45, 46, 47
                        },
                        new[]
                        {
                            48, 49, 50, 51, 52, 53
                        },
                        new[]
                        {
                            54, 55, 56, 57, 58, 59
                        }
                    };
            }

            if (MidsContext.Character.Archetype.ClassType == Enums.eClassType.HeroEpic)
                return new[]
                {
                    new[]
                    {
                        0, 1, 2
                    },
                    new[]
                    {
                        3, 4, 5
                    },
                    new[]
                    {
                        6, 7, 8
                    },
                    new[]
                    {
                        9, 10, 11
                    },
                    new[]
                    {
                        12, 13, 14
                    },
                    new[]
                    {
                        15, 16, 17
                    },
                    new[]
                    {
                        18, 19, 20
                    },
                    new[]
                    {
                        21, 22, 23
                    },
                    new[]
                    {
                        24, 25, 26
                    },
                    new[]
                    {
                        27, 28, 29
                    },
                    new[]
                    {
                        30, 31, 32
                    },
                    new[]
                    {
                        33, 34, 35
                    },
                    new[]
                    {
                        36, 37, 38
                    },
                    new[]
                    {
                        39, 40, 41
                    },
                    new[]
                    {
                        42, 43, 44
                    },
                    new[]
                    {
                        45, 46, 47
                    },
                    new[]
                    {
                        48, 49, 50
                    },
                    new[]
                    {
                        51, 52, 53
                    },
                    new[]
                    {
                        54, 55, 56
                    },
                    new[]
                    {
                        57, 58, 59
                    }
                };

            return new[]
            {
                new[]
                {
                    0, 1, 2
                },
                new[]
                {
                    3, 4, 5
                },
                new[]
                {
                    6, 7, 8
                },
                new[]
                {
                    9, 10, 11
                },
                new[]
                {
                    12, 13, 14
                },
                new[]
                {
                    15, 16, 17
                },
                new[]
                {
                    18, 19, 20
                },
                new[]
                {
                    21, 22, 23
                },
                new[]
                {
                    24, 25, 26
                },
                new[]
                {
                    27, 28, 29
                },
                new[]
                {
                    30, 31, 32
                },
                new[]
                {
                    33, 34, 35
                },
                new[]
                {
                    36, 37, 38
                },
                new[]
                {
                    39, 40, 41
                },
                new[]
                {
                    42, 43, 44
                },
                new[]
                {
                    45, 46, 47
                },
                new[]
                {
                    48, 49, 50
                },
                new[]
                {
                    51, 52, 53
                },
                new[]
                {
                    54, 55, 56
                },
                new[]
                {
                    57, 58, 59
                }
            };
        }


        public Point PowerPosition(PowerEntry powerEntry, int displayLocation = -1)
        {
            var powerIdx = MidsContext.Character.CurrentBuild.Powers.IndexOf(powerEntry);
            checked
            {
                // Assume that this is a copy and not the actual powerEntry item
                if (powerIdx == -1)
                {
                    const int num2 = 0;
                    var num3 = MidsContext.Character.CurrentBuild.Powers.Count - 1;
                    for (var i = num2; i <= num3; i++)
                    {
                        if (MidsContext.Character.CurrentBuild.Powers[i].Power.PowerIndex != powerEntry.Power.PowerIndex || MidsContext.Character.CurrentBuild.Powers[i].Level != powerEntry.Level)
                            continue;
                        powerIdx = i;
                        break;
                    }
                }

                //Inherent Grid
                var inherentGrid = GetInherentGrid();
                var flag = false;
                var iRow = 0;
                var iCol = 0;

                if (!powerEntry.Chosen)
                {
                    if (displayLocation == -1 && powerEntry.Power != null)
                    {
                        displayLocation = powerEntry.Power.DisplayLocation;
                    }

                    if (displayLocation <= -1)
                    {
                        return CRtoXY(iCol, iRow);
                    }

                    iRow = vcRowsPowers;
                    for (var i = 0; i <= inherentGrid.Length - 1; i++)
                    {
                        for (var k = 0; k <= inherentGrid[i].Length - 1; k++)
                        {
                            if (displayLocation != inherentGrid[i][k]) continue;
                            if (vcCols != 5)
                            {
                                iRow += i + 1;
                                iCol = k;
                            }
                            else
                            {
                                iRow += i + 2;
                                iCol = k;
                            }

                            flag = true;
                            break;
                        }

                        if (flag) break;
                    }
                }
                // Main Powers
                else if (powerIdx > -1)
                {
                    for (var i = 1; i <= vcCols; i++)
                    {
                        if (vcCols == 5)
                        {
                            iCol = (int)Math.Floor((double)powerIdx / vcCols);
                            iRow = powerIdx % vcCols;
                        }
                        else
                        {
                            if (powerIdx >= vcRowsPowers * i)
                                continue;
                            iCol = i - 1;
                            iRow = powerIdx - vcRowsPowers * iCol;
                        }
                        break;
                    }
                }

                return CRtoXY(iCol, iRow);
            }
        }


        private Point CRtoXY(int iCol, int iRow)
        {
            // Convert a column/row location to the top left XY co-ord of a power entry
            // 3 Columns, 15 Rows
            var result = new Point(0, 0);
            checked
            {
                if (iRow >= vcRowsPowers)
                {
                    result.X = iCol * (SzPower.Width + PaddingX);
                    result.Y = OffsetInherent + iRow * (SzPower.Height + PaddingY);
                }
                else
                {
                    result.X = iCol * (SzPower.Width + PaddingX);
                    result.Y = iRow * (SzPower.Height + PaddingY);
                }

                return result;
            }
        }

        public Size GetDrawingArea()
        {
            var result = (Size) PowerPosition(vcPowers - 1);
            checked
            {
                result.Width += SzPower.Width;
                result.Height = result.Height + SzPower.Height + PaddingY;
                for (var i = 0; i <= MidsContext.Character.CurrentBuild.Powers.Count - 1; i++)
                {
                    if (MidsContext.Character.CurrentBuild.Powers[i].Power == null || MidsContext.Character.CurrentBuild.Powers[i].Chosen && i > MidsContext.Character.CurrentBuild.LastPower)
                        continue;
                    var size = new Size(result.Width, PowerPosition(i).Y + SzPower.Height + PaddingY);
                    if (size.Height > result.Height) result.Height = size.Height;

                    if (size.Width > result.Width) result.Width = size.Width;
                }

                return result;
            }
        }

        private Size GetMaxDrawingArea()
        {
            var cols = vcCols;
            MiniSetCol(6);
            var result = (Size) PowerPosition(vcPowers - 1);
            MiniSetCol(2);
            var inherentGrid = GetInherentGrid();
            checked
            {
                var size = (Size) CRtoXY(inherentGrid[inherentGrid.Length - 1].Length - 1, inherentGrid.Length - 1);
                if (size.Height > result.Height) result.Height = size.Height;

                if (size.Width > result.Width) result.Width = size.Width;

                MiniSetCol(cols);
                result.Width += SzPower.Width;
                result.Height = result.Height + SzPower.Height + PaddingY;
                return result;
            }
        }

        private void MiniSetCol(int cols)
        {
            if (cols == vcCols)
                return;
            if ((cols < 2) | (cols > 6))
                return;
            vcCols = cols;
            vcRowsPowers = checked(vcPowers / vcCols);
        }

        private Size GetRequiredDrawingArea()
        {
            var maxY = -1;
            var maxX = -1;
            checked
            {
                var num4 = MidsContext.Character.CurrentBuild.Powers.Count - 1;
                for (var i = 0; i <= num4; i++)
                {
                    if (!((MidsContext.Character.CurrentBuild.Powers[i].IDXPower > -1) |
                          MidsContext.Character.CurrentBuild.Powers[i].Chosen))
                        continue;
                    var point = PowerPosition(i);
                    if (point.X > maxX) maxX = point.X;

                    if (point.Y > maxY) maxY = point.Y;
                }

                Size result;
                if ((maxX > -1) & (maxY > -1))
                {
                    var size = new Size(maxX + SzPower.Width, maxY + SzPower.Height + PaddingY);
                    result = size;
                }
                else
                {
                    if (vcCols != 5)
                    {
                        var point2 = PowerPosition(MidsContext.Character.CurrentBuild.LastPower);
                        var size = new Size(point2.X + SzPower.Width, point2.Y + SzPower.Height + PaddingY + OffsetInherent);
                        result = size;
                    }
                    else
                    {
                        var point2 = PowerPosition(MidsContext.Character.CurrentBuild.LastPower);
                        var size = new Size(point2.X + SzPower.Width, point2.Y + SzPower.Height + PaddingY + OffsetInherent);
                        result = size;
                    }
                }

                return result;
            }
        }
    }
}