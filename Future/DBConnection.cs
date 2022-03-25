using System.Xml;
using System.Data.SqlClient;
using System.Collections.ObjectModel;
using System.Data.Common;

namespace Future{
    public static class DBConnection{
        private static string? DBName;
        private static string DBInfoPath = "C:\\FPOS\\Data\\dbinfo.xml";
        private static string ConnectionString;
        private static SqlConnection conn;
        private static string? getDBName(string dbInfoFilePath){
            XmlNode? node = null;
            try{
                FileStream xmlstream = File.Create(dbInfoFilePath);
                XmlDocument xml = new XmlDocument();
                xml.Load(xmlstream);
                node = xml.SelectSingleNode("/db/ServerDatabase");
            }catch(Exception e){
                Console.Error.WriteLine(e.Message);
            }   
            return node?.InnerText;    
        }
        static DBConnection(){
            DBName = getDBName(DBInfoPath);
            ConnectionString = $"Server={Future.Registry.Util.getServerName()}";
            conn = new SqlConnection(ConnectionString);
        }
        static public List<DBResult> executeCommand(string command){
            List<DBResult> rows = new ();
            using(conn){
                SqlCommand sqlCommand = new SqlCommand(command,conn);
                sqlCommand.Connection.Open();
                using (SqlDataReader reader = sqlCommand.ExecuteReader()){
                    var schema = reader.GetColumnSchema();
                    while(reader.Read()){
                        Object[] row = new string[reader.FieldCount];
                        reader.GetValues(row);
                        rows.Add(new DBResult(schema,row.ToList()));
                    }
                }
            }
            return rows;
        }
    }
    public class DBResult{
        private Dictionary<string,string> result;
        private List<string> columnNames = new List<string>();
        public DBResult(in ReadOnlyCollection<DbColumn> cols,in List<Object> row ){
            result = new Dictionary<string, string>();
            for(int i = 0; i < cols.Count; i++){
                result[cols[i].ColumnName] = (string)row[i]; 
                columnNames.Add(cols[i].ColumnName);
            }
        }
        public IList<string> getColumnNames(){
            return columnNames.AsReadOnly();
        }
        public bool columnExists(string colName){
            return result.ContainsKey(colName);          
        }
        public bool getValue(string colName,out string? value){
            string? val;
            if(result.TryGetValue(colName,out val)){
                value = val;
                return true;
            }else{
                value = null;
                return false;
            }
        }
    }
}
