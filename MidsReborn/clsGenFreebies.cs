﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Mids_Reborn.Core;
using Mids_Reborn.Core.Base.Master_Classes;
using Newtonsoft.Json;

namespace Mids_Reborn
{
    public static class clsGenFreebies
    {
        private const int EnhancementsTrayCapacity = 70;
        private const string BoostCmd = "boost";
        private const string CmdSeparator = "$$";
        public const string MenuExt = "mnu";
        public const string DefaultMenuName = "MRBTest";
        public static string MenuName { get; set; } = "MRBTest";

        public static class MenuExport
        {
            private static List<List<string>> GenerateBoostChunks()
            {
                var k = 0;
                var l = 0;
                var commandChunks = new List<List<string>>();

                if (MidsContext.Character.CurrentBuild == null)
                {
                    return commandChunks;
                }

                foreach (var p in MidsContext.Character.CurrentBuild.Powers?.Where(p => p != null && p.State != Enums.ePowerState.Empty))
                {
                    for (var j = 0; j < p.Slots.Length; j++)
                    {
                        if (p.Slots[j].Enhancement.Enh < 0) continue; // Empty slot
                        if (k % EnhancementsTrayCapacity == 0)
                        {
                            commandChunks.Add(new List<string>());
                            if (k > 0) l++;
                        }

                        var enhData = DatabaseAPI.Database.Enhancements[p.Slots[j].Enhancement.Enh];
                        var enhUid = enhData.UID.Replace("Shrapnel_", "Artillery_");
                        var enhBoostLevel = p.Slots[j].Enhancement.Grade switch
                        {
                            Enums.eEnhGrade.None => p.Slots[j].Enhancement.IOLevel + 1,
                            _ => 50 + p.Slots[j].Enhancement.RelativeLevel switch
                            {
                                Enums.eEnhRelative.MinusThree => -3,
                                Enums.eEnhRelative.MinusTwo => -2,
                                Enums.eEnhRelative.MinusOne => -1,
                                Enums.eEnhRelative.PlusOne => 1,
                                Enums.eEnhRelative.PlusTwo => 2,
                                Enums.eEnhRelative.PlusThree => 3,
                                _ => 0
                            }
                        };

                        commandChunks[l].Add($"{BoostCmd} {enhUid} {enhUid} {enhBoostLevel}");

                        k++;
                    }
                }

                return commandChunks;
            }

            public static string GenerateMenu()
            {
                if (MainModule.MidsController.Toon == null)
                {
                    return string.Empty;
                }

                var commandChunks = GenerateBoostChunks();
                var dateTag = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss", null);
                var mnuStr = $"// Generated by {Application.ProductName} v{Application.ProductVersion} - {dateTag}\r\n";
                mnuStr += $"// Open the menu ingame: /popmenu {MenuName}\r\n\r\n";
                mnuStr += $"Menu \"{MenuName}\"\r\n";
                mnuStr += "{\r\n";
                mnuStr += $"\tTitle \"{(string.IsNullOrWhiteSpace(MainModule.MidsController.Toon.Name) ? "Test build" : $"{MainModule.MidsController.Toon.Name.Trim()} test build")}\"\r\n";
                mnuStr += "\tDIVIDER\r\n";

                for (var i = 0; i < commandChunks.Count; i++)
                {
                    mnuStr += $"\tOption \"Give enhancements (part {i + 1})\" \"{string.Join(CmdSeparator, commandChunks[i].ToArray())}\"\r\n";
                }

                mnuStr += "\tDIVIDER\r\n";
                mnuStr += "\tLockedOption\r\n";
                mnuStr += "\t{\r\n";
                mnuStr += $"\t\tDisplayName \"{Application.ProductName} v{Application.ProductVersion}\"\r\n";
                mnuStr += "\t\tBadge \"X\"\r\n";
                mnuStr += "\t}\r\n";
                mnuStr += "\tLockedOption\r\n";
                mnuStr += "\t{\r\n";
                mnuStr += $"\t\tDisplayName \"Generated: {dateTag}\"\r\n";
                mnuStr += "\t\tBadge \"X\"\r\n";
                mnuStr += "\t}\r\n";
                mnuStr += "}";

                return mnuStr;
            }

            public static bool SaveTo(string file)
            {
                var mnuStr = GenerateMenu();
                if (string.IsNullOrEmpty(mnuStr)) return false;
                try
                {
                    using var sw = new StreamWriter(file);
                    sw.Write(mnuStr);
                    sw.Close();

                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }
    }
}