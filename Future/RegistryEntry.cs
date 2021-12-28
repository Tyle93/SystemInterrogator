using Microsoft.Win32;
using System.Diagnostics;
namespace Future{

    namespace Registry{
        public static class RegistryEntry{
            static readonly string FPOSRegPath = "HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\Future P.O.S.\\DIRECTORIES\\";
            static readonly string UTGRegPath = "HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\Shift4 Corporation\\";
            static readonly string UTGRegValueName = "Installation Path";
            static readonly string FPOSRegValueName = "FPOS Directory";
            static readonly string? UTGInstallPath;
            static readonly string? FPOSInstallPath;
            private static readonly FileVersionInfo? FPOSVersionInfo;
            static readonly string FPOSVersion;
            static readonly string UTGVersion;
            static readonly int FPOSVersionMajor;
            static RegistryEntry(){
                UTGInstallPath  = (string?)Microsoft.Win32.Registry.GetValue(UTGRegPath,UTGRegValueName, null);
                FPOSInstallPath = (string?)Microsoft.Win32.Registry.GetValue(FPOSRegPath,FPOSRegValueName, null);
                try{
                    FPOSVersionInfo = FileVersionInfo.GetVersionInfo(FPOSInstallPath ?? "");
                }catch(FileNotFoundException ex){
                    Console.Error.WriteLine(ex.Message);
                    FPOSVersionInfo = null;
                }
                try{
                    UTGVersion = FileVersionInfo.GetVersionInfo(UTGInstallPath ?? "").FileVersion ?? "N/a";
                }catch(FileNotFoundException ex){
                    Console.Error.WriteLine(ex.Message);
                    UTGVersion = "N/a";
                }
                FPOSVersion = FPOSVersionInfo?.FileVersion ?? "N/a";
                FPOSVersionMajor = FPOSVersionInfo?.FileMajorPart ?? 6;
            }
        }
        public static class Util{
            public static string? getFPOSPath(){
                return null;
            }
            public static string? getFPOSVersion(){
                return null;
            }
            static Util(){

            }
            public static string getServerName(){
                return "";
            }
        }   
    }
}