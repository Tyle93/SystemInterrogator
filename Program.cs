using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Microsoft.Win32;
using System.Management;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Diagnostics;

string FPOSRegPath = "HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\Future P.O.S.\\DIRECTORIES\\";
string UTGRegPath = "HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\Shift4 Corporation\\";
string UTGRegValueName = "Installation Path";
string FPOSRegVauleName = "FPOS Directory";
string? UTGInstallPath;
string? FPOSInstallPath;
string InstanceName = "";
string ServerName = Environment.MachineName;
string FPOSVersion = "";
string UTGVersion = "";
int FPOSMajorVersion;

try{
    UTGInstallPath = (string?)Registry.GetValue(UTGRegPath,UTGRegValueName, null);
    if(UTGInstallPath == null){
        UTGInstallPath = "Not Installed";
        Console.WriteLine("UTG NOT INSTALLED.");
        UTGVersion = "N/a";
    }else{
        UTGInstallPath += "\\UTG2\\UTG2.exe";
        try{
            UTGVersion = FileVersionInfo.GetVersionInfo(UTGInstallPath).FileVersion ?? "N/a";
            Console.WriteLine($"UTG VERSION: {UTGVersion}");
        }catch(FileNotFoundException e){
            Console.Error.WriteLine(e.Message);
            Console.WriteLine("UTG Executable Not Found.");
        }
        
    }
}catch(ArgumentException e){
    Console.Error.WriteLine(e.Message);
    Console.WriteLine("Invalid UTG Registry Path.");
}

try{
    FPOSInstallPath = (string?)Registry.GetValue(FPOSRegPath,FPOSRegVauleName, "Value not found.");
    if(FPOSInstallPath == null){
        FPOSInstallPath = "Not Installed";
        Console.WriteLine("FPOS NOT INSTALLED.");
        FPOSVersion = "N/a";
    }else{
        FPOSInstallPath += "\\bin\\FPOS.exe";
        try{
            FileVersionInfo FPOSVer = FileVersionInfo.GetVersionInfo(FPOSInstallPath);
            FPOSMajorVersion =  FPOSVer.FileMajorPart;
            FPOSVersion = FPOSVer.FileVersion ?? "N/a";
            Console.WriteLine($"FPOS VERSION: {FPOSVersion}");
            switch(FPOSMajorVersion){
                case 5:
                    InstanceName = "CESSQL";
                    break;
                case 6:
                    InstanceName = "FPOSSQL";
                    break;
                default:
                    InstanceName = "FPOSSQL";
                    break;
            }
        }catch(FileNotFoundException e){
            Console.Error.WriteLine(e.Message);
            Console.WriteLine("FPOS Executable Not Found");
        }
    }
}catch(ArgumentException){
    Console.WriteLine("Invalid FPOS Registry Path.");
}

string QueryString = @"
                ""
                SET NOCOUNT ON
                SELECT SERVERPROPERTY('productversion') as 'Product Version',
                SERVERPROPERTY('productlevel') as 'Service Pack',
                SERVERPROPERTY('edition') as 'Edition', 
                SERVERPROPERTY('instancename') as 'Instance', 
                SERVERPROPERTY('servername') as 'Server Name'
                ""
";

string CurrentUser = Environment.UserName;
string SqlcmdOutputFilePath = "./Sqloutput.csv";
string PSCommand = $" -S {ServerName}\\{InstanceName} -Q {QueryString} -o {SqlcmdOutputFilePath} -W -h -1 -s \",\"";
File.Create($".\\{SqlcmdOutputFilePath}").Close();

try{
    Process.Start("sqlcmd.exe",PSCommand).WaitForExit();
}catch(Exception e){
    Console.WriteLine(e.Message);
}

List<string> rd = new List<string>();
rd.Add(Environment.MachineName);
if(File.Exists(SqlcmdOutputFilePath)){
    Console.WriteLine($"Path: {SqlcmdOutputFilePath} exists.");
    rd.AddRange(File.ReadAllText(SqlcmdOutputFilePath,System.Text.Encoding.UTF8).Split(',').ToList());
    rd.Add(FPOSVersion);
    rd.Add(UTGVersion);
}else{
    Console.WriteLine($"Path: {SqlcmdOutputFilePath} does not exitst.");
    rd = new List<string>();
}

string TotalSystemMemory = "N/a";

try{
    ObjectQuery wql = new ObjectQuery("SELECT * FROM Win32_OperatingSystem");
    ManagementObjectSearcher searcher =  new ManagementObjectSearcher(wql);
    ManagementObjectCollection results = searcher.Get();
    foreach(var s in results){
        string? bytes = s["TotalPhysicalMemory"].ToString();
        TotalSystemMemory = Math.Round((Double.Parse(bytes ?? "0") / (1024*1024)),2).ToString() + "GB";
    }
}catch(Exception e){
    Console.WriteLine(e.Message);
}

rd.Add(TotalSystemMemory);

string[] scopes = {SheetsService.Scope.Spreadsheets};
string ApplicationName = "System Audit";
string credPath = "../../APIKeys";
string credJsonString = @"{}";

var cred = GoogleCredential.FromStream(new MemoryStream(System.Text.Encoding.ASCII.GetBytes(credJsonString)));

var service = new SheetsService(new BaseClientService.Initializer(){
                HttpClientInitializer = cred,
                ApplicationName = ApplicationName,
});

AppendCellsRequest acp = new AppendCellsRequest();
RowData row = new RowData();
row.Values = new List<CellData>();
string SpreadSheetID = "1_y4Jg-J4X8n3DexV9hDoLkx8w2e0HJPVztPqiavv0YI";

foreach(string cd in rd){   
    CellData cell = new CellData();
    cell.UserEnteredValue = new ();
    cell.UserEnteredValue.StringValue = cd;
    row.Values.Add(cell);
}

acp.Rows = new List<RowData>();
acp.Rows.Add(row);
acp.Fields = "*";
acp.SheetId = 0;

BatchUpdateSpreadsheetRequest batch = new BatchUpdateSpreadsheetRequest();
Request req = new Request();

req.AppendCells = acp;
batch.Requests = new List<Request>();
batch.Requests.Add(req);

service.Spreadsheets.BatchUpdate(batch,SpreadSheetID).Execute();
