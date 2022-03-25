using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Services;
using Microsoft.Win32;
using System.Management;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices;
using NickStrupat;

if(!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)){
    Console.Error.WriteLine("Platform not supported.");
    Environment.Exit(1);
}

string QueryString = @"
                ""
                SET NOCOUNT ON
                SELECT SERVERPROPERTY('productversion') as 'Product Version',
                SERVERPROPERTY('productlevel') as 'Service Pack',
                SERVERPROPERTY('edition') as 'Edition', 
                SERVERPROPERTY('instancename') as 'Instance', 
                SERVERPROPERTY('servername') as 'Server Name'git pu
                ""
";

string CurrentUser = Environment.UserName;
string SqlcmdOutputFilePath = "./Sqloutput.csv";
string PSCommand = $" -S {Future.Registry.Util.getServerName}-Q {QueryString} -o {SqlcmdOutputFilePath} -W -h -1 -s \",\"";
File.Create($".\\{SqlcmdOutputFilePath}").Close();

try{
    Process.Start("sqlcmd.exe",PSCommand).WaitForExit();
}catch(Exception e){
    Console.WriteLine(e.Message);
}

List<string> rd = new List<string>();
rd.Add(Environment.MachineName);
if(File.Exists(SqlcmdOutputFilePath)){
    rd.AddRange(File.ReadAllText(SqlcmdOutputFilePath,System.Text.Encoding.UTF8).Split(',').ToList());
    rd.Add(Future.Registry.RegistryEntry.FPOSVersion);
    rd.Add(Future.Registry.RegistryEntry.UTGVersion);
}else{
    Console.WriteLine($"Path: {SqlcmdOutputFilePath} does not exitst.");
    rd = new List<string>();
}

//Pray this works.
string TotalSystemMemory = "N/a";
ComputerInfo info = new ComputerInfo();
TotalSystemMemory = Math.Round((double)(info.TotalPhysicalMemory / (1024*1024)),2).ToString() + "GB";


//TODO: FIX THIS!
//try{
//    ObjectQuery wql = new ObjectQuery("SELECT * FROM Win32_OperatingSystem");
//    ManagementObjectSearcher searcher =  new ManagementObjectSearcher(wql);
//    ManagementObjectCollection results = searcher.Get();
//    foreach(var s in results){
//        double res = Convert.ToDouble(s["TotalVisibleMemorySize"]);
//        res = Math.Round((res / (1024*1024)),2);
//                Console.WriteLine("Total usable memory size: "+ TotalSystemMemory +"GB");
//        TotalSystemMemory = res.ToString();
//
//    }
//}catch(Exception e){
//    Console.WriteLine(e.StackTrace);
//}

rd.Add(TotalSystemMemory);

string[] scopes = {SheetsService.Scope.Spreadsheets};
string ApplicationName = "System Audit";
//string credPath = "../../APIKeys/";
string credJsonString = @"{

}";
//var cred = GoogleCredential.FromSteam(File.Create(credPath));
var cred = GoogleCredential.FromStream(new MemoryStream(System.Text.Encoding.ASCII.GetBytes(credJsonString)));

var service = new SheetsService(new BaseClientService.Initializer(){
                HttpClientInitializer = cred,
                ApplicationName = ApplicationName,
});

RowData row = Google.Util.toRowData(rd);
Request req = Google.Util.InitAppendCellsRequest(row);

string SpreadSheetID = "1_y4Jg-J4X8n3DexV9hDoLkx8w2e0HJPVztPqiavv0YI";

BatchUpdateSpreadsheetRequest batch = Google.Util.CreateBatchUpdateRequest(req);

service.Spreadsheets.BatchUpdate(batch,SpreadSheetID).Execute();
