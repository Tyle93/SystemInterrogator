using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Services;
using System.Management;
using System.Diagnostics;
using System.Runtime.InteropServices;


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
                SERVERPROPERTY('servername') as 'Server Name'
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
rd.Add("STORE NAME");
rd.Add(Future.Registry.Util.getHostName());
if(File.Exists(SqlcmdOutputFilePath)){
    rd.AddRange(File.ReadAllText(SqlcmdOutputFilePath,System.Text.Encoding.UTF8).Split(',').ToList());
    rd.Add(Future.Registry.RegistryEntry.FPOSVersion);
    rd.Add(Future.Registry.RegistryEntry.UTGVersion);
}else{
    Console.WriteLine($"Path: {SqlcmdOutputFilePath} does not exitst.");
}

string TotalSystemMemory = "0 GB";

try{
    ObjectQuery wql = new ObjectQuery("SELECT * FROM Win32_OperatingSystem");
    ManagementObjectSearcher searcher =  new ManagementObjectSearcher(wql);
    ManagementObjectCollection results = searcher.Get();
    foreach(var s in results){
        double res = Convert.ToDouble(s["TotalVisibleMemorySize"]);
        res = Math.Round((res / (1024*1024)),2);
                Console.WriteLine("Total usable memory size: "+ TotalSystemMemory +"GB");
        TotalSystemMemory = res.ToString() + "GB";

    }
}catch(Exception e){
    Console.WriteLine(e.StackTrace);
}

rd.Add(TotalSystemMemory);

//string[] scopes = {SheetsService.Scope.Spreadsheets};
string ApplicationName = "System Audit";
//string credPath = "../../APIKeys/";
string credJsonString = @"
{
  ""type"": ""service_account"",
  ""project_id"": ""system-audit"",
  ""private_key_id"": ""49416d064bed44be0716693026916820308c25aa"",
  ""private_key"": ""-----BEGIN PRIVATE KEY-----\nMIIEvQIBADANBgkqhkiG9w0BAQEFAASCBKcwggSjAgEAAoIBAQDxC+zRzihQ7eM9\nDszoNqjsBHH51X1vJoau5wGcvBCj59Lq0b1OQLsFQvZZedLKS5xTYgC+/v35wszr\nXFdw9cr+b7lNbKR+Bb9OALNFORDU+tf+ZAIyLNwbigJGl3EeQiLOmkmqAMMSuItE\nUBrHB9enzJ/o6sf4ZU+zjRnimxchgSEObpHMbnU4rAiHegfYF8UGV4WZGq9mJb7/\nwxLNov9zSGTaULJXPw7PFHYSTR73JWJvInu3uCn+tA2f3RlqqszshRWVWr1rBUBL\nq4eJS8mK2QPZ4B4JQ5A2fidKdsHwGcn+2yhGhWG/n92lAcPeGYPJ5rznh34LFTpf\nZcg0fjNVAgMBAAECggEAHXlNUEwqftbPseAU6xH/7rgObbOOnmAyGMHTXyRZzoji\nr+bMhHyCwb5x2/f679R1/XYezDvmtzr8Do1gfcX4N8tWd6KgrG4sea+5O+4h4kD3\n1znIUPVORqnF5HbRFJeWpPeVqU1ljOz2zngTBiIt2u044mSO5g8RQ02uDBjVp7lj\nHU1+QU4hDrUOS7IcAOATjPod8mm5L9+mF8lDygJ2X/h1171kPWw3OnNW43wX05Mm\nlB1MrQv3FEwOuHbxszobSA78DF05cvIgBKTlbWx09jOknlCHze0zbteUNafzffAf\njxVV1HbgxYPFe1uuU1A2c5yP1NShKTzgcECV9gQxowKBgQD8GcMFATHy9NaDA6y0\n5hEG/XfjOzNJ7LsPVuad4OxHS6oahrEuv8VL7Hi8cmodq7JsDB/A4RZBQzjM3y/+\nMGFR9dWnqTWjK8szMMKYxFj5prFF3p/owa7AgKAIziBbOZxxFonpMWpJlF434ZB/\nYRhUuq5/1/VOCJUZtWsj7zLg5wKBgQD0xmSMRfHrSwCZyuauzpBJ4IEbsnWycf6Y\n/vXeEQMvnor8Idf2orCr+XrDCc35fphyP89Z6zuxjZYmpugKqH3ChIBej6sTDb1q\nJ82JYNExRhk+jMKwKWBHz3TCXJP05HjsQJJjI9eh+MvUl5+JG9LKJbmrgHxaR1fW\ng1LScgu2YwKBgQC1lzzx0u2OyyvlTXVPDNXBCj0cUd1c54fKwdb2bDjmFiueVd01\nBm/wg5Joc6XaX33y9Dy/K6NjOS+l6zJfz6uaZREUZv863OXOptXVQsGwepEA+h6H\ng1QEV+Ju1UNbUsFMeGa4sZ6VB6liaTkzd9YV2i6t6fpJzkbQMOzPApLMiwKBgGzl\nJi5Rn8Kx516EKgRy2TZErquHxVzR4hZmtzNIFkbFHcf1toJ+9mJL5xWF8yCf9Wo+\ngRzbzf67oqUnc+vp8ajsEb//4nfjkPT9KC+J5wcozGpLbQG6Jp9L6AHf9eLpEfLo\n9zcr5u6kJOo7WjpBKpHMHsHxs2DmhGlmmx4MprFJAoGAetGfsKUdq9zsd3s3X4ys\nsoWGoNX5kWerk7P1iddAqsfoKVtK8RLRU9vVLbQu9f5rUlwROpsYyOw8njbkMeLC\noYvavsPfXlnBej48yIDdWs4jXnOMBtQC7rSPNfQ9ycun1Gkwp8eaX8pOoSGYv0XB\nIEzxlUnpFdKVIaNCY5iUNVI=\n-----END PRIVATE KEY-----\n"",
  ""client_email"": ""auditor@system-audit.iam.gserviceaccount.com"",
  ""client_id"": ""118231977528031262619"",
  ""auth_uri"": ""https://accounts.google.com/o/oauth2/auth"",
  ""token_uri"": ""https://oauth2.googleapis.com/token"",
  ""auth_provider_x509_cert_url"": ""https://www.googleapis.com/oauth2/v1/certs"",
  ""client_x509_cert_url"": ""https://www.googleapis.com/robot/v1/metadata/x509/auditor%40system-audit.iam.gserviceaccount.com""
}";
//var cred = GoogleCredential.FromSteam(File.Create(credPath));
//var cred = GoogleCredential.FromStream(new MemoryStream(System.Text.Encoding.ASCII.GetBytes(credJsonString)));
//var json = JsonSerializer.Deserialize<GoogleCredential>(credJsonString);
var cred = GoogleCredential.FromJson(credJsonString);
var service = new SheetsService(new BaseClientService.Initializer(){
                HttpClientInitializer = cred,
                ApplicationName = ApplicationName,
});

RowData row = Google.Util.toRowData(rd);
Request req = Google.Util.InitAppendCellsRequest(row);

string SpreadSheetID = "1_y4Jg-J4X8n3DexV9hDoLkx8w2e0HJPVztPqiavv0YI";

BatchUpdateSpreadsheetRequest batch = Google.Util.CreateBatchUpdateRequest(req);

service.Spreadsheets.BatchUpdate(batch,SpreadSheetID).Execute();
