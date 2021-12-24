using Microsoft.Win32;
using System.Diagnostics;
namespace FutureRegistry{
    public static class Entries{
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
        static Entries(){
            UTGInstallPath  = (string?)Registry.GetValue(UTGRegPath,UTGRegValueName, null);
            FPOSInstallPath = (string?)Registry.GetValue(FPOSRegPath,FPOSRegValueName, null);
            try{
                try{
                    FPOSVersionInfo = FileVersionInfo.GetVersionInfo(FPOSInstallPath ?? "");
                }catch(FileNotFoundException ex){
                    Console.Error.WriteLine(ex.Message);
                    FPOSVersionInfo = null;
                }
                FPOSVersion = FPOSVersionInfo?.FileVersion ?? "N/a";
                FPOSVersionMajor = FPOSVersionInfo.FileMajorPart;
                UTGVersion = FileVersionInfo.GetVersionInfo(UTGInstallPath ?? "").FileVersion ?? "N/a";
            }catch(FileNotFoundException ex){
                Console.WriteLine(ex.Message);
            }catch(Exception e){
                Console.Error.WriteLine(e.Message);
            }
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
    }
}