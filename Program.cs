using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Microsoft.Win32;
using System.Management;
using System.Diagnostics;

string UTGRegPath = "HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\\"Future P.O.S.\"\\Directories";
string FPOSRegPath = "HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\\"Shift4 Corporation\"";
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
    }else{
        UTGInstallPath += "\\UTG2\\UTG2.exe";  
        UTGVersion = FileVersionInfo.GetVersionInfo(UTGInstallPath).ToString();
    }
}catch(ArgumentException e){
    Console.Error.WriteLine(e.Message);
    Console.WriteLine("Invalid UTG Registry Path.");
}

try{
    FPOSInstallPath = (string?)Registry.GetValue(FPOSRegPath,FPOSRegVauleName, "Value not found.");
    if(FPOSInstallPath == null){
        FPOSInstallPath = "Not Installed";
    }else{
        FPOSInstallPath += "\\bin\\FPOS.exe";
        FileVersionInfo FPOSVer = FileVersionInfo.GetVersionInfo(FPOSInstallPath);
        FPOSMajorVersion =  FPOSVer.FileMajorPart;
        FPOSVersion = FPOSVer.ToString();
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
string SqlcmdOutputFilePath = $"\"C:\\users\\{CurrentUser}\\Desktop\\Sqloutput.csv\"";
string PSCommand = $" -E -S {ServerName}\\{InstanceName} -Q {QueryString} -o {SqlcmdOutputFilePath} ";

//PowerShell ps = PowerShell.Create();
//ps.AddScript(PSCommand);
//try{
//    ps.Invoke();
//}catch(Exception e){
//    Console.WriteLine(e.Message);
//}
Process.Start("sqlcmd.exe",$"\"{PSCommand}\"");

List<string> RowData = File.ReadAllText(SqlcmdOutputFilePath).Split(',').ToList();
RowData.Add(FPOSVersion);
RowData.Add(UTGVersion);

string TotalSystemMemory = "0";

ObjectQuery wql = new ObjectQuery("SELECT * FROM Win32_OperatingSystem");
ManagementObjectSearcher searcher = new ManagementObjectSearcher(wql);
ManagementObjectCollection results = searcher.Get();
foreach(var s in results){
    string? bytes = s["TotalPhysicalMemory"].ToString();
    TotalSystemMemory = Math.Round((Double.Parse(bytes ?? "0") / (1024*1024)),2).ToString() + "GB";
}

RowData.Add(TotalSystemMemory);

string[] scopes = {SheetsService.Scope.Spreadsheets, SheetsService.Scope.Drive};
string ApplicationName = "System Interrogator";

UserCredential credential;
 using (var stream = new FileStream("..\\APIKeys\\credentials.json", FileMode.Open, FileAccess.Read)){
    // The file token.json stores the user's access and refresh tokens, and is created
    // automatically when the authorization flow completes for the first time.
    string credPath = "token.json";
    credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
        GoogleClientSecrets.FromStream(stream).Secrets,
        scopes,
        "user",
        CancellationToken.None,
        new FileDataStore(credPath, true)).Result;
    Console.WriteLine("Credential file saved to: " + credPath);
}

var service = new SheetsService(new BaseClientService.Initializer(){
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
});

AppendCellsRequest acp = new ();
RowData row = new RowData();
string SpreadSheetID = "1_y4Jg-J4X8n3DexV9hDoLkx8w2e0HJPVztPqiavv0YI";

foreach(var cd in RowData){
    CellData cell = new ();
    cell.UserEnteredValue.StringValue = cd; 
    row.Values.Add(cell);
}

acp.Rows.Add(row);
acp.Fields = "*";
acp.SheetId = 0;

BatchUpdateSpreadsheetRequest batch = new BatchUpdateSpreadsheetRequest();
Request req = new Request();
req.AppendCells = acp;
batch.Requests.Add(req);

service.Spreadsheets.BatchUpdate(batch,SpreadSheetID);





