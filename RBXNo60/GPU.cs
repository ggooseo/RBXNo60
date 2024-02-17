using System;
using Silk.NET.Vulkan;
using System.Management;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using Microsoft.Win32;

namespace RBXNo60
{
    public class GPU
    {
        public static bool SupportsVulkan()
        {
            return Vk.GetApi() != null;
        }

        public static string GetDirectXVersion()
        {
            FeatureLevel maxSupportedFeatureLevel = GetMaxSupportedFeatureLevel();

            switch (maxSupportedFeatureLevel)
            {
                case FeatureLevel.Level_10_0:
                    return "DirectX 10.0";
                case FeatureLevel.Level_10_1:
                    return "DirectX 10.0";
                case FeatureLevel.Level_11_0:
                    return "DirectX 11.0";
                case FeatureLevel.Level_11_1:
                    return "DirectX 11.1";
                default:
                    return "Unknown";
            }
        }

        static FeatureLevel GetMaxSupportedFeatureLevel()
        {
            using (var device = new SharpDX.Direct3D11.Device(DriverType.Hardware, DeviceCreationFlags.None))
            {
                FeatureLevel featureLevel = device.FeatureLevel;
                return featureLevel;
            }
        }

        public static void AddAppToGraphicsPreferences(string valueName, string valueData)
        {
            try
            {
                string keyPath = @"HKEY_CURRENT_USER\Software\Microsoft\DirectX\UserGpuPreferences";

#pragma warning disable CA1416 
                Registry.SetValue(keyPath, valueName, valueData, RegistryValueKind.String);
#pragma warning restore CA1416

                Console.WriteLine("App added to graphics performance preferences successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
