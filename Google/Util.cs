using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace Google{
    public static class Util{
        public static RowData toRowData(IEnumerable<string> list){
            RowData rd = new RowData();
            rd.Values = toCellData(list);
            return rd;
        }
        public static CellData toCellData<T>(T? value){
            CellData cd = new CellData();
            cd.UserEnteredValue = new ExtendedValue();
            cd.UserEnteredValue.StringValue = value?.ToString() ?? "Null";
            return cd;
        }
        public static List<CellData> toCellData(IEnumerable<string> list){
            List<CellData> cd = new List<CellData>();
            foreach(string s in list){
                cd.Add(toCellData(s));
            }
            return cd;
        }
        public static Request InitAppendCellsRequest(){
            AppendCellsRequest acp = new ();
            Request req = new Request();
            acp.Rows = new List<RowData>();
            acp.Fields = "*";
            acp.SheetId = 0;
            req.AppendCells = acp;
            return req;
        }
        public static Request InitAppendCellsRequest(RowData rd, string fields = "*", int sheetid = 0){
            AppendCellsRequest acp = new ();
            Request req = new Request();
            acp.Rows = new List<RowData>();
            acp.Rows.Add(rd);
            acp.Fields = fields;
            acp.SheetId = sheetid;
            req.AppendCells = acp;
            return req;
        }
        public static Request InitAppendCellsRequest(IList<RowData> rd, string fields = "*", int sheetid = 0){
            AppendCellsRequest acp = new ();
            Request req = new Request();
            acp.Rows = rd;
            acp.Fields = fields;
            acp.SheetId = sheetid;
            req.AppendCells = acp;
            return req;
        }
        public static BatchUpdateSpreadsheetRequest CreateBatchUpdateRequest(){
            BatchUpdateSpreadsheetRequest batch = new ();
            batch.Requests = new List<Request>();
            return batch;
        }
        public static BatchUpdateSpreadsheetRequest CreateBatchUpdateRequest(List<Request> reqs){
            BatchUpdateSpreadsheetRequest batch = new ();
            batch.Requests = reqs;
            return batch;
        }
        public static BatchUpdateSpreadsheetRequest CreateBatchUpdateRequest(Request req){
            BatchUpdateSpreadsheetRequest batch = new ();
            batch.Requests = new List<Request>();
            batch.Requests.Add(req);
            return batch;
        }
    }
}