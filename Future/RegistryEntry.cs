
using System.Diagnostics;
namespace Future.Registry{
    public static class RegistryEntry{
        public static string FPOSRegPath{get; private set;}= "HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\Future P.O.S.\\DIRECTORIES\\";
        public static string UTGRegPath{get; private set;} = "HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\Shift4 Corporation\\";
        public static string UTGRegValueName{get; private set;} = "Installation Path";
        public static string FPOSRegValueName{get; private set;}= "FPOS Directory";
        public static string? UTGInstallPath{get; private set;}
        public static string? FPOSInstallPath{get; private set;}
        public static FileVersionInfo? FPOSVersionInfo{get; private set;}
        public static string FPOSVersion{get; private set;}
        public static string UTGVersion{get; private set;}
        public static int? FPOSVersionMajor{get; private set;}
        public static string InstanceName{get; private set;}
        static RegistryEntry(){
            InitUTG();
            InitFPOS();
            FPOSVersion = FPOSVersionInfo?.FileVersion ?? "N/a";
            FPOSVersionMajor = FPOSVersionInfo?.FileMajorPart ?? null;
            InstanceName = FPOSVersionMajor switch {
                5 => "FPOSSQL",
                6 => "CESSQL",
                _ => " ",
            };
        }
            
        private static void InitUTG(){
            UTGInstallPath  = (string?)Microsoft.Win32.Registry.GetValue(UTGRegPath,UTGRegValueName, null);
            try{
                UTGVersion = FileVersionInfo.GetVersionInfo(UTGInstallPath ?? "").FileVersion ?? "N/a";
            }catch(FileNotFoundException ex){
                Console.Error.WriteLine(ex.Message);
                UTGVersion = "N/a";
            }
        }
        private static void InitFPOS(){
            FPOSInstallPath = (string?)Microsoft.Win32.Registry.GetValue(FPOSRegPath,FPOSRegValueName, null);
            try{
                FPOSVersionInfo = FileVersionInfo.GetVersionInfo(FPOSInstallPath ?? "");
            }catch(FileNotFoundException ex){
                Console.Error.WriteLine(ex.Message);
                FPOSVersionInfo = null;
            }
        }
    }
    public static class Util{
        public static string? getServerName(){
            return $"{Environment.MachineName}\\{RegistryEntry.InstanceName}";
        }

        public static string getHostName(){
            return $"{Environment.MachineName}";
        }
    }
}