using Microsoft.Win32;
using System.Diagnostics;
namespace Future.Registry{
    public static class RegistryEntry{
        public static readonly string FPOSRegPath = "HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\Future P.O.S.\\DIRECTORIES\\";
        public static readonly string UTGRegPath = "HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\Shift4 Corporation\\";
        public static readonly string UTGRegValueName = "Installation Path";
        public static readonly string FPOSRegValueName = "FPOS Directory";
        public static readonly string? UTGInstallPath;
        public static readonly string? FPOSInstallPath;
        public static readonly FileVersionInfo? FPOSVersionInfo;
        public static readonly string FPOSVersion;
        public static readonly string UTGVersion;
        public static readonly int? FPOSVersionMajor;
        public static readonly string InstanceName;
        static RegistryEntry(){

            UTGInstallPath  = (string?)Microsoft.Win32.Registry.GetValue(UTGRegPath,UTGRegValueName, null);
            try{
                UTGVersion = FileVersionInfo.GetVersionInfo(UTGInstallPath ?? "").FileVersion ?? "N/a";
            }catch(FileNotFoundException ex){
                Console.Error.WriteLine(ex.Message);
                UTGVersion = "N/a";
            }
            FPOSInstallPath = (string?)Microsoft.Win32.Registry.GetValue(FPOSRegPath,FPOSRegValueName, null);
            try{
                FPOSVersionInfo = FileVersionInfo.GetVersionInfo(FPOSInstallPath ?? "");
            }catch(FileNotFoundException ex){
                Console.Error.WriteLine(ex.Message);
                FPOSVersionInfo = null;
            }
            FPOSVersion = FPOSVersionInfo?.FileVersion ?? "N/a";
            FPOSVersionMajor = FPOSVersionInfo?.FileMajorPart ?? null;
            InstanceName = FPOSVersionMajor switch {
                5 => "FPOSSQL",
                6 => "CESSQL",
                _ => " ",
            };
        }
    }
    public static class Util{
        public static string? getServerName(){
            return $"{Environment.MachineName}\\{RegistryEntry.InstanceName}";
        }
    }
}