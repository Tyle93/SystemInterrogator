using System.Xml;
using System.Data.SqlClient;
namespace Future{
    public static class DBConnection{
        private static string DBName;
        private static string DBInfoPath = "C:\\FPOS\\Data\\dbinfo.xml";
        private static string ConnectionString;
        private static SqlConnection conn;
        private static string getDBName(){
            
            return "";
        }
        static DBConnection(){
            FileStream xmlstream = File.Create(DBInfoPath);
            XmlDocument xml = new XmlDocument();
            xml.Load(xmlstream);
            XmlNode? node = xml.SelectSingleNode("/db/ServerDatabase");
            DBName = node?.InnerText ?? "";    
            ConnectionString = "";
            conn = new SqlConnection(ConnectionString);
        }
    }
}