using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Microsoft.Win32;
using System.Management;
using System.Diagnostics;

string UTGRegPath = "HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\Future P.O.S.\\DIRECTORIES\\";
string FPOSRegPath = "HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\Shift4 Corporation\\";
string UTGRegValueName = "Installation Path";
string FPOSRegVauleName = "FPOS Directory";
string? UTGInstallPath;
string? FPOSInstallPath;
string InstanceName = "";
string ServerName = "";
string FPOSVersion = "";
string UTGVersion = "";
int FPOSMajorVersion;

try{
    UTGInstallPath = (string?)Registry.GetValue(UTGRegPath,UTGRegValueName, "Value not found");
    if(UTGInstallPath == null){
        UTGInstallPath = "Not Installed";
        Console.WriteLine("UTG NOT INSTALLED.");
        UTGVersion = "N/a";
    }else{
        UTGInstallPath += "\\UTG2\\UTG2.exe";
        try{
            UTGVersion = FileVersionInfo.GetVersionInfo(UTGInstallPath).ToString();
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
            FPOSVersion = FPOSVer.ToString();
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
string SqlcmdOutputFilePath = "Sqloutput.csv";
string PSCommand = $" -S {ServerName}\\{InstanceName} -Q {QueryString} -o {SqlcmdOutputFilePath} ";
File.Create($".\\{SqlcmdOutputFilePath}").Close();

try{
    Process.Start("sqlcmd.exe",PSCommand);
}catch(Exception e){
    Console.WriteLine(e.Message);
}

List<string> rd = new List<string>(); //File.ReadAllText(SqlcmdOutputFilePath).Split(',').ToList();
rd.Add(FPOSVersion);
rd.Add(UTGVersion);

string TotalSystemMemory = "0";

try{
    ObjectQuery wql = new ObjectQuery("SELECT * FROM Win32_OperatingSystem");
    ManagementObjectSearcher searcher = new ManagementObjectSearcher(wql);
    ManagementObjectCollection results = searcher.Get();
    foreach(var s in results){
        string? bytes = s["TotalPhysicalMemory"].ToString();
        TotalSystemMemory = Math.Round((Double.Parse(bytes ?? "0") / (1024*1024)),2).ToString() + "GB";
    }
}catch(Exception e){
    Console.WriteLine(e.Message);
}

rd.Add(TotalSystemMemory);

string[] scopes = {SheetsService.Scope.Spreadsheets, SheetsService.Scope.Drive};
string ApplicationName = "System Audit";

UserCredential credential;
ClientSecrets cs = new ();

if(args.Length >= 2){
    cs.ClientId = args[0];
    cs.ClientSecret = args[1];
}else{
    cs.ClientId = "";
    cs.ClientSecret = "";
}

string credPath = ".\\token.json";
credential = GoogleWebAuthorizationBroker.AuthorizeAsync(cs,scopes,"user",CancellationToken.None,new FileDataStore(credPath, true)).Result;
Console.WriteLine("Credential file saved to: " + credPath);

var service = new SheetsService(new BaseClientService.Initializer(){
                HttpClientInitializer = credential,
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




