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
        public static CellData toCellData<T>(T value){
            CellData cd = new CellData();
            cd.UserEnteredValue.StringValue = value.ToString();
            return cd;
        }
        public static List<CellData> toCellData(IEnumerable<string> list){
            List<CellData> cd = new List<CellData>();
            return cd;
        }
    }
}