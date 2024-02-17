using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System;
using RBXNo60.ClientPresets;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace RBXNo60
{
    public class Program
    {
        private static Process roblox;

        public static void Main(string[] args)
        {
            Console.Title = $"gooseo's rbxno60 ({(Environment.Is64BitProcess ? "64" : "32")}-bit)";

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("""

            ―――――――――――――――――――――――――――――――――――――
                █▀█ █▄▄ ▀▄▀ █▄░█ █▀█ █▄▄ █▀█
                █▀▄ █▄█ █░█ █░▀█ █▄█ █▄█ █▄█
            ―――――――――――――――――――――――――――――――――――――

            """);

            Console.ResetColor();

            roblox = Process.GetProcessesByName("RobloxPlayerBeta").FirstOrDefault();

            if (RobloxNotRunning())
                return;

            Console.Write("Found Roblox process ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"({GetRobloxLocation().Parent.Name})");
            Console.ResetColor();

            ManageSettings();

            RestartRoblox();

            Console.ReadLine();
        }

        private static DirectoryInfo GetRobloxLocation() => roblox != null ? new DirectoryInfo(roblox.MainModule.FileName) : null;

        private static bool RobloxNotRunning()
        {
            if (roblox == null)
            {
                Console.WriteLine("Roblox is not running...");
                Task.Delay(2500).Wait();
                Environment.Exit(3);
                return true;
            }
            return false;
        }

        private static void ManageSettings()
        {
            string robloxRootFolder = GetRobloxLocation()?.Parent?.FullName;

            if (robloxRootFolder == null)
                return;

            DirectoryInfo settingsDir = CreateSettingsDirectory(robloxRootFolder);
            string filePath = Path.Combine(settingsDir.FullName, "ClientAppSettings.json");

            ApplySavedSettings(filePath);

            int maxFps = GetIntegerInput("Set Maximum FPS (number):");

            bool highPerformance = false;

            ClientAppSettingsBase clientAppSettingsBase = new ClientAppSettingsBase() { 
                ENABLED = true, 
            };

            ClientAppSettingsMain clientAppSettingsMain = new ClientAppSettingsMain()
            {
                ENABLED = true,
                DFIntTaskSchedulerTargetFps = maxFps
            };

            ClientAppSettingsPotato clientAppSettingsPotato = new ClientAppSettingsPotato();
            ClientAppSettingsTelemetry clientAppSettingsTelemetry = new ClientAppSettingsTelemetry();
            ClientAppSettingsDX11 clientAppSettingsDX11 = new ClientAppSettingsDX11();
            ClientAppSettingsDX10 clientAppSettingsDX10 = new ClientAppSettingsDX10();
            ClientAppSettingsVulkan clientAppSettingsVulkan = new ClientAppSettingsVulkan();

            string DXVersion = GPU.GetDirectXVersion();

            if (GetBoolInput("Go through other optimizations (true/false):"))
            {
                clientAppSettingsMain.FFlagFixGraphicsQuality = GetBoolInput("Increase graphics bar amount (true/false):");
                clientAppSettingsPotato.ENABLED = GetBoolInput("Potato Mode (true/false):");

                if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 16299))
                    highPerformance = GetBoolInput("Use high performance for roblox (true/false):");

                if (OperatingSystem.IsWindows())
                    clientAppSettingsTelemetry.ENABLED = GetBoolInput("Disable Telemetry for roblox (true/false):");

                if (GPU.SupportsVulkan())
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Your GPU Supports: Vulkan");
                    Console.ResetColor();

                    clientAppSettingsVulkan.ENABLED = GetBoolInput("Force Vulkan [possibilty of crashes] (true/false):");
                }

                if (DXVersion != "Unknown" && !clientAppSettingsVulkan.ENABLED)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Your GPU Supports: {DXVersion}");
                    Console.ResetColor();

                    if (DXVersion.Contains("11.0") || DXVersion.Contains("11.1"))
                        clientAppSettingsDX11.ENABLED = GetBoolInput("Force DX11 [better performance on newer pcs] (true/false):");

                    if ((DXVersion.Contains("10.0") || DXVersion.Contains("10.1")) && !clientAppSettingsDX11.ENABLED)
                        clientAppSettingsDX10.ENABLED = GetBoolInput("Force DX10 [better performance on older PCs] (true/false):");
                }
            }

            bool applyAutomatically = GetBoolInput("Apply these settings on launch (true/false):");

            if (highPerformance)
            {
                GPU.AddAppToGraphicsPreferences(roblox.MainModule.FileName, "GpuPreference=2;");
            }

            JObject baseObjectSettings = JObject.FromObject(clientAppSettingsBase);

            List<ClientAppSettingsBase> additionalSettings = new List<ClientAppSettingsBase> {
                clientAppSettingsMain,
                clientAppSettingsPotato,
                clientAppSettingsTelemetry,
                clientAppSettingsDX11,
                clientAppSettingsDX10,
                clientAppSettingsVulkan
            };

            foreach (ClientAppSettingsBase additionalSetting in additionalSettings)
            {
                if (!additionalSetting.ENABLED)
                    continue;

                MergeSettingWithBase(baseObjectSettings, JObject.FromObject(additionalSetting));
            }

            string mergedJson = baseObjectSettings.ToString();
            File.WriteAllText(filePath, mergedJson);

            if (applyAutomatically)
                File.WriteAllText("settings.json", mergedJson);
            else if (!applyAutomatically && File.Exists("settings.json"))
            {
                File.Delete("settings.json");
                Console.WriteLine("Removed existing settings.json file.");
            }
        }

        private static DirectoryInfo CreateSettingsDirectory(string robloxRootFolder)
        {
            DirectoryInfo settingsDir = Directory.CreateDirectory(Path.Combine(robloxRootFolder, "ClientSettings"));
            return settingsDir;
        }

        private static void ApplySavedSettings(string filePath)
        {
            if (File.Exists("settings.json"))
            {
                Task.Delay(500).Wait();

                File.Delete(filePath);
                File.Copy("settings.json", filePath);

                Console.WriteLine("Applied saved settings (You can exit)");

                if (!GetBoolInput("Edit settings (true/false):"))
                    Environment.Exit(0);
            }
        }

        private static void RestartRoblox()
        {
            Task.Delay(500).Wait();

            Console.Clear();
            try
            {
                string robloxPath = roblox.MainModule.FileName;
                roblox.Kill();
                roblox.Dispose();
                Task.Delay(500).Wait();

                roblox = Process.Start(robloxPath);
                Console.WriteLine("Started Roblox.");

                Task.Delay(1000).Wait();
                Console.WriteLine("By the way, you can add your own flags if you enabled RBXNo60 to apply settings automatically. Go to settings.json and add your own flag!");
                Task.Delay(1000).Wait();
                Console.WriteLine("You can safely exit this window.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error restarting Roblox: {ex.Message}");
            }
        }

        private static void MergeSettingWithBase(JObject baseSettings, JObject additionalSetting)
        {
            baseSettings.Merge(JObject.FromObject(additionalSetting), new JsonMergeSettings
            {
                MergeArrayHandling = MergeArrayHandling.Union
            });
        }

        private static int GetIntegerInput(string prompt)
        {
            int value;
            do
            {
                DisplayPrompt(prompt);
            } while (!int.TryParse(Console.ReadLine(), out value));

            return value;
        }

        private static bool GetBoolInput(string prompt)
        {
            bool value;
            do
            {
                DisplayPrompt(prompt);
            } while (!bool.TryParse(Console.ReadLine(), out value));

            return value;
        }

        private static void DisplayPrompt(string prompt)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(prompt);
            Console.ResetColor();
        }
    }
}