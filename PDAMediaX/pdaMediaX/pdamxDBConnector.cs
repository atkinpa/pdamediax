using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using MySql.Data.MySqlClient;      // MySQL
using Oracle.DataAccess.Client;    // Oracle
using System.Data.SqlClient;       // SQLServer

// MYSQL reference: http://windows-programming.suite101.com/article.cfm/how_to_access_mysql_with_c
// "" http://dev.mysql.com/doc/refman/5.0/fr/odbc-net-op-c-sharp-cp.html
// "" http://www.codeproject.com/KB/database/MySQLCsharp.aspx
// "" http://www.dreamincode.net/code/snippet1677.htm
// "" http://publib.boulder.ibm.com/infocenter/iseries/v5r4/index.jsp?topic=/rzaha/basicjdbc.htm
// "" http://www.codeproject.com/KB/database/sql_in_csharp.aspx
// "" http://msdn.microsoft.com/en-us/library/7a2f3ay4(VS.80).aspx == Thread Processing
// Search Key: sample c# code to query mysql


namespace pdaMediaX.Net.Sql
{
    public class pdamxDBConnector
    {
        public const int DB_ORACLE = 0x00100;
        public const int DB_SQLSERVER = 0x00200;
        public const int DB_MYSQL = 0x00300;

        public const int CT_ODBC = 0x001000;
        public const int CT_OLE = 0x002000;
        public const int CT_CONNSTRING = 0x003000;
        public const int CT_DIRECT = 0x004000;

        DateTimeFormatInfo __dtFormat = new CultureInfo("en-US", false).DateTimeFormat;
        Hashtable __hConnectionList;
        List<Object> __lSqlParameters;

        int __nDataBaseProvider = DB_MYSQL;
        int __nConnectionType = CT_DIRECT;

        static String __sDefaultConnectionName = "_Default@" + Convert.ToString(DateTime.Now.ToFileTime());
        String __sDatabaseException = "";
        String __sSelectedConnectionName = __sDefaultConnectionName;
        String __sServer;
        String __sDatabase;
        String __sUID;
        String __sPasswd;
        String __sPort;
        String __sConnectionString;
        String __sSqlCommand;
        String __sDataSource;
        String __sDriver;
        System.Data.CommandType __ctCommandType;

        Exception __eErrorException;

        public pdamxDBConnector()
        {
            __hConnectionList = new Hashtable();
            ResetConnectionInfo();
        }
        public void AddSqlParameter(Object _sqlParameter)
        {
            if (_sqlParameter == null)
            {
                return;
            }
            if (__lSqlParameters == null)
            {
                __lSqlParameters = new List<Object>();
            }
            __lSqlParameters.Add(_sqlParameter);
        }
        // Url about this section : http://www.triconsole.com/dotnet/sqlcommand_class.php#commandtype
        public bool BeginTransaction()
        {
            return (BeginTransaction(SelectedConnection));
        }
        public bool BeginTransaction(String _sConnectionName)
        {
            Hashtable hConnectionInfo;
            int nDBProvider;

            if (_sConnectionName == null)
                return (false);

            if (_sConnectionName.Trim().Length == 0)
                return (false);

            if (!ConnectionNameExist(_sConnectionName))
                return (false);

            if (!IsConnectionOpen(_sConnectionName))
                OpenConnection(_sConnectionName);

            hConnectionInfo = GetConnectionInfo(_sConnectionName);
            // Don't start another transaction if one has already started....
            if (((Object)hConnectionInfo["TransactionObject"]) != null)
                return (false);

            nDBProvider = Convert.ToInt32((String)hConnectionInfo["DBProvider"]);
            try
            {
                DataBaseException = "";
                if (nDBProvider == DB_MYSQL)
                {
                    MySqlConnection mysqlConnection = (MySqlConnection)hConnectionInfo["ConnectionObject"];
                    MySqlTransaction mysqlTransaction = mysqlConnection.BeginTransaction(IsolationLevel.ReadCommitted);
                    SetHashProperty(hConnectionInfo, "TransactionObject", mysqlTransaction);
                }
                if (nDBProvider == DB_ORACLE)
                {
                    OracleConnection oracleConnection = (OracleConnection)hConnectionInfo["ConnectionObject"];
                    OracleTransaction oracleTransaction = oracleConnection.BeginTransaction(IsolationLevel.ReadCommitted);
                    SetHashProperty(hConnectionInfo, "TransactionObject", oracleTransaction);
                }
                if (nDBProvider == DB_SQLSERVER)
                {
                    SqlConnection sqlserverConnection = (SqlConnection)hConnectionInfo["ConnectionObject"];
                    SqlTransaction sqlserverTransaction = sqlserverConnection.BeginTransaction(IsolationLevel.ReadCommitted);
                    SetHashProperty(hConnectionInfo, "TransactionObject", sqlserverTransaction);
                }
            }
            catch (Exception eException)
            {
                ErrorException = eException;
                DataBaseException = ErrorException.Message;
                return (false);
            }
            return (true);
        }
        public void CloseAllConnections()
        {
            IDictionaryEnumerator ideKeys = GetConnectionList().GetEnumerator();
            while(ideKeys.MoveNext())
            {
                CloseConnection((String) ideKeys.Key);
            }
        }
        public bool CloseConnection()
        {
            return (CloseConnection(SelectedConnection));
        }
        public bool CloseConnection(String _sConnectionName)
        {
            Hashtable hConnectionInfo;
            int nDBProvider;

            if (_sConnectionName == null)
                return (false);

            if (_sConnectionName.Trim().Length == 0)
                return (false);

            if (!ConnectionNameExist(_sConnectionName))
                return (false);

            hConnectionInfo = GetConnectionInfo(_sConnectionName);
            nDBProvider = Convert.ToInt32((String)hConnectionInfo["DBProvider"]);
            try
            {
                DataBaseException = "";
                EndTransaction(_sConnectionName);  //Attempt to end any transactions that may have started...
                if (nDBProvider == DB_MYSQL)
                {
                    MySqlConnection mysqlConnection = (MySqlConnection)hConnectionInfo["ConnectionObject"];
                    MySqlDataReader mysqlDataReader = (MySqlDataReader)hConnectionInfo["DataReaderObject"];
                    if (mysqlDataReader != null)
                        mysqlDataReader.Close();
                    mysqlConnection.Close();
                }
                if (nDBProvider == DB_ORACLE)
                {
                    OracleConnection oraclelConnection = (OracleConnection)hConnectionInfo["ConnectionObject"];
                    OracleDataReader oracleDataReader = (OracleDataReader)hConnectionInfo["DataReaderObject"];
                    if (oracleDataReader != null)
                        oracleDataReader.Close();
                    oraclelConnection.Close();
                }
                if (nDBProvider == DB_SQLSERVER)
                {
                    SqlConnection sqlserverConnection = (SqlConnection)hConnectionInfo["ConnectionObject"];
                    SqlDataReader sqlDataReader = (SqlDataReader)hConnectionInfo["DataReaderObject"];
                    if (sqlDataReader != null)
                        sqlDataReader.Close();
                    sqlserverConnection.Close();
                }
                SetHashProperty(hConnectionInfo, "ConnectionStatus", "Close");
            }
            catch (Exception eException)
            {
                ErrorException = eException;
                DataBaseException = ErrorException.Message;
                return (false);
            }
            return (true);
        }
        public bool CommitTransaction()
        {
            return (CommitTransaction(SelectedConnection));
        }
        public bool CommitTransaction(String _sConnectionName)
        {
            Hashtable hConnectionInfo;
            int nDBProvider;

            if (_sConnectionName == null)
                return(false);

            if (_sConnectionName.Trim().Length ==0)
                return(false);

            if (!ConnectionNameExist(_sConnectionName))
                return (false);

            if (IsConnectionOpen(_sConnectionName))
                return(false);

            hConnectionInfo = GetConnectionInfo(_sConnectionName);
            if (((Object)hConnectionInfo["TransactionObject"]) == null)
                return (false);

            nDBProvider = Convert.ToInt32((String)hConnectionInfo["DBProvider"]);
            try
            {
                DataBaseException = "";
                if (nDBProvider == DB_MYSQL)
                {
                    MySqlTransaction mysqlTransaction = (MySqlTransaction)hConnectionInfo["TransactionObject"];
                    mysqlTransaction.Commit();
                }
                if (nDBProvider == DB_ORACLE)
                {
                    OracleTransaction oracleTransaction = (OracleTransaction)hConnectionInfo["TransactionObject"];
                    oracleTransaction.Commit();
                }
                if (nDBProvider == DB_SQLSERVER)
                {
                    SqlTransaction sqlserverTransaction = (SqlTransaction)hConnectionInfo["TransactionObject"];
                    sqlserverTransaction.Commit();
                }
            }
            catch (Exception eException)
            {
                ErrorException = eException;
                DataBaseException = ErrorException.Message;
                return (false);
            }
            return (true);
        }
        public bool ConnectionNameExist(String _sConnectionName)
        {
            if (_sConnectionName == null)
                return (false);

            if (_sConnectionName.Trim().Length == 0)
                return (false);

            return (GetConnectionList().Contains(_sConnectionName));
        }
        public bool CreateConnection()
        {
            return (CreateConnection(SelectedConnection));
        }
        public bool CreateConnection(String _sConnectionName)
        {
            Hashtable hConnectionInfo;

            if (_sConnectionName == null)
                return (false);

            if (_sConnectionName.Trim().Length == 0)
                return (false);

            if (ConnectionNameExist(_sConnectionName))
                return (false);
            if (_sConnectionName !=__sDefaultConnectionName)
            {
                ResetConnectionInfo();
            }
            hConnectionInfo = new Hashtable();
            hConnectionInfo.Add("CreationState", "Init");
            hConnectionInfo.Add("ConnectionID", _sConnectionName + "@" + Convert.ToString(hConnectionInfo.GetHashCode()) + ":" + Convert.ToString(DateTime.Now.ToFileTime()));
            hConnectionInfo.Add("ConnectionStatus", "Close");
            hConnectionInfo.Add("ConnectionType", Convert.ToString(ConnectionType));
            hConnectionInfo.Add("DBProvider", Convert.ToString(DataBaseProvider));
            hConnectionInfo.Add("ConnectionCreated", DateTime.Now.ToString("MM/dd/yyyy [hh:mm:ss tt]", __dtFormat));
            hConnectionInfo.Add("ConnectionCreationTimeStamp", Convert.ToString(DateTime.Now.ToFileTime()));
            GetConnectionList().Add(_sConnectionName, hConnectionInfo);
            SelectedConnection = _sConnectionName;
            return (true);
        }
        public void EndTransaction()
        {
            EndTransaction(SelectedConnection);
        }
        public void EndTransaction(String _sConnectionName)
        {
            Hashtable hConnectionInfo;

            if (_sConnectionName == null)
                return;

            if (_sConnectionName.Trim().Length == 0)
                return;

            if (!ConnectionNameExist(_sConnectionName))
                return;

            if (!IsConnectionOpen(_sConnectionName))
                return;

            if (((Object)GetConnectionInfo(_sConnectionName)["TransactionObject"]) != null)
            {
                CommitTransaction(_sConnectionName);
                hConnectionInfo = GetConnectionInfo(_sConnectionName);
                hConnectionInfo.Remove("CommandObject");
                hConnectionInfo.Remove("TransactionObject");
            }
        }
        public Object ExecuteQuery()
        {
            return (ExecuteQuery(SelectedConnection));
        }
        public Object ExecuteQuery(String _sConnectionName)
        {
            Hashtable hConnectionInfo;
            int nDBProvider;

            if (_sConnectionName == null)
                return (null);

            if (_sConnectionName.Trim().Length == 0)
                return (null);

            if (!ConnectionNameExist(_sConnectionName))
                return (null);

            if (SqlCommand == null)
                return (null);

            if (SqlCommand.Trim().Length == 0)
                return (null);

            if (!IsConnectionOpen(_sConnectionName))
                OpenConnection(_sConnectionName);
            hConnectionInfo = GetConnectionInfo(_sConnectionName);
            nDBProvider = Convert.ToInt32((String)hConnectionInfo["DBProvider"]);
            try
            {
                DataBaseException = "";
                if (nDBProvider == DB_MYSQL)
                {
                    MySqlConnection mysqlConnection = (MySqlConnection)hConnectionInfo["ConnectionObject"];
                    MySqlCommand mysqlCommand = new MySqlCommand(SqlCommand, mysqlConnection);
                    SetHashProperty(hConnectionInfo, "CommandObject", mysqlCommand);
                    mysqlCommand.CommandType = CommandType;
                    if (GetSQLParameters()  != null)
                    {
                        foreach(Object mysqlParameter in GetSQLParameters())
                        {
                            mysqlCommand.Parameters.Add((MySqlParameter) mysqlParameter);
                        }
                    }
                    MySqlDataReader mySqlDataReader = mysqlCommand.ExecuteReader();
                    SetHashProperty(hConnectionInfo, "DataReaderObject", mySqlDataReader);
                    return (mySqlDataReader);
                }
                if (nDBProvider == DB_ORACLE)
                {
                    OracleConnection oraclelConnection = (OracleConnection)hConnectionInfo["ConnectionObject"];
                    OracleCommand oracleCommand = new OracleCommand(SqlCommand, oraclelConnection);
                    SetHashProperty(hConnectionInfo, "CommandObject", oracleCommand);
                    oracleCommand.CommandType = CommandType;
                    if (GetSQLParameters() != null)
                    {
                        foreach (Object oracleParameter in GetSQLParameters())
                        {
                            oracleCommand.Parameters.Add((OracleParameter) oracleParameter);
                        }
                    }
                    OracleDataReader oracleDataReader = oracleCommand.ExecuteReader();
                    SetHashProperty(hConnectionInfo, "DataReaderObject", oracleDataReader);
                    return (oracleDataReader);
                }
                if (nDBProvider == DB_SQLSERVER)
                {
                    SqlConnection sqlserverConnection = (SqlConnection)hConnectionInfo["ConnectionObject"];
                    SqlCommand sqlserverCommand = new SqlCommand(SqlCommand, sqlserverConnection);
                    SetHashProperty(hConnectionInfo, "CommandObject", sqlserverCommand);
                    sqlserverCommand.CommandType = CommandType;
                    if (GetSQLParameters() != null)
                    {
                        foreach (Object sqlserverParameter in GetSQLParameters())
                        {
                            sqlserverCommand.Parameters.Add((SqlParameter) sqlserverParameter);
                        }
                    }
                    SqlDataReader sqlserverDataReader = sqlserverCommand.ExecuteReader();
                    SetHashProperty(hConnectionInfo, "DataReaderObject", sqlserverDataReader);
                    return (sqlserverDataReader);
                }
            }
            catch (Exception eException)
            {
                ErrorException = eException;
                DataBaseException = ErrorException.Message;
            }
            return (null);
        }
        public bool ExecuteNonQuery()
        {
            return (ExecuteNonQuery(SelectedConnection));
        }
        public bool ExecuteNonQuery(String _sConnectionName)
        {
            Hashtable hConnectionInfo;
            int nDBProvider;

            if (_sConnectionName == null)
                return (false);

            if (_sConnectionName.Trim().Length == 0)
                return (false);

            if (SqlCommand == null)
                return (false);

            if (SqlCommand.Trim().Length == 0)
                return (false);

            if (!IsConnectionOpen(_sConnectionName))
                OpenConnection(_sConnectionName);

            hConnectionInfo = GetConnectionInfo(_sConnectionName);
            nDBProvider = Convert.ToInt32((String)hConnectionInfo["DBProvider"]);
            try
            {
                DataBaseException = "";
                if (nDBProvider == DB_MYSQL)
                {
                    MySqlCommand mysqlCommand;
                    MySqlConnection mysqlConnection = (MySqlConnection)hConnectionInfo["ConnectionObject"];
                    if (((Object)hConnectionInfo["TransactionObject"]) != null)
                    {
                        if (((Object)hConnectionInfo["CommandObject"]) == null)
                        {
                            mysqlCommand = mysqlConnection.CreateCommand();
                            mysqlCommand.Transaction = (MySqlTransaction)hConnectionInfo["TransactionObject"];
                            SetHashProperty(hConnectionInfo, "CommandObject", mysqlCommand);
                        }
                        else
                        {
                            mysqlCommand = (MySqlCommand)hConnectionInfo["CommandObject"];
                        }
                    }
                    else
                    {
                        mysqlCommand = mysqlConnection.CreateCommand();
                    }
                    mysqlCommand.CommandText = SqlCommand;
                    mysqlCommand.CommandType = CommandType;
                }
                if (nDBProvider == DB_ORACLE)
                {
                    OracleCommand oracleCommand;
                    OracleConnection oraclelConnection = (OracleConnection)hConnectionInfo["ConnectionObject"];
                    if (((Object)hConnectionInfo["TransactionObject"]) != null)
                    {
                        if (((Object)hConnectionInfo["CommandObject"]) == null)
                        {
                            oracleCommand = oraclelConnection.CreateCommand();
                            SetHashProperty(hConnectionInfo, "CommandObject", oracleCommand);
                        }
                        else
                        {
                            oracleCommand = (OracleCommand)hConnectionInfo["CommandObject"];
                        }
                    }
                    else
                    {
                        oracleCommand = oraclelConnection.CreateCommand();
                    }
                    oracleCommand.CommandText = SqlCommand;
                    oracleCommand.CommandType = CommandType;
                }
                if (nDBProvider == DB_SQLSERVER)
                {
                    SqlCommand sqlserverCommand;
                    SqlConnection sqlserverConnection = (SqlConnection)hConnectionInfo["ConnectionObject"];
                    if (((Object)hConnectionInfo["TransactionObject"]) != null)
                    {
                        if (((Object)hConnectionInfo["CommandObject"]) == null)
                        {
                            sqlserverCommand = sqlserverConnection.CreateCommand();
                            sqlserverCommand.Transaction = (SqlTransaction)hConnectionInfo["TransactionObject"];
                            SetHashProperty(hConnectionInfo, "CommandObject", sqlserverCommand);
                        }
                        else
                        {
                            sqlserverCommand = (SqlCommand)hConnectionInfo["CommandObject"];
                        }
                    }
                    else
                    {
                        sqlserverCommand = sqlserverConnection.CreateCommand();
                    }
                    sqlserverCommand.CommandText = SqlCommand;
                    sqlserverCommand.CommandType = CommandType;
                }
            }
            catch (Exception eException)
            {
                ErrorException = eException;
                DataBaseException = ErrorException.Message;
                if (((Object)hConnectionInfo["TransactionObject"]) != null)
                {
                    try
                    {
                        if (nDBProvider == DB_MYSQL)
                            ((MySqlTransaction)hConnectionInfo["TransactionObject"]).Rollback();
                        if (nDBProvider == DB_ORACLE)
                            ((OracleTransaction)hConnectionInfo["TransactionObject"]).Rollback();
                        if (nDBProvider == DB_SQLSERVER)
                            ((SqlTransaction)hConnectionInfo["TransactionObject"]).Rollback();
                        EndTransaction(_sConnectionName);
                    }
                    catch (Exception) { }
                }
                return (false);
            }
            return (true);
        }
        private Hashtable GetConnectionInfo(String _sConnectionName)
        {
            return ((Hashtable)GetConnectionList()[_sConnectionName]);
        }
        private Hashtable GetConnectionList()
        {
            return (__hConnectionList);
        }
        public IEnumerable<String> GetConnectionNames()
        {
            IDictionaryEnumerator ideKeys = GetConnectionList().GetEnumerator();
            while (ideKeys.MoveNext())
            {
                yield return ideKeys.Key.ToString();
            }
        }
        public Object GetCommandObject()
        {
            return (GetCommandObject(SelectedConnection));
        }
        public Object GetCommandObject(String _sConnectionName)
        {
            Hashtable hConnectionInfo;

            if (_sConnectionName == null)
                return (null);

            if (_sConnectionName.Trim().Length == 0)
                return (null);


            if (!ConnectionNameExist(_sConnectionName))
                return (null);

            hConnectionInfo = GetConnectionInfo(_sConnectionName);
            return((Object)hConnectionInfo["CommandObject"]);
        }
        public Object GetConnectionObject()
        {
            return (GetConnectionObject(SelectedConnection));
        }
        public Object GetConnectionObject(String _sConnectionName)
        {
            Hashtable hConnectionInfo;

            if (_sConnectionName == null)
                return (null);

            if (_sConnectionName.Trim().Length == 0)
                return (null);


            if (!ConnectionNameExist(_sConnectionName))
                return (null);

            hConnectionInfo = GetConnectionInfo(_sConnectionName);
            return ((Object)hConnectionInfo["ConnectionObject"]);
        }
        public Object GetDataReaderObject()
        {
            return (GetDataReaderObject(SelectedConnection));
        }
        public Object GetDataReaderObject(String _sConnectionName)
        {
            Hashtable hConnectionInfo;

            if (_sConnectionName == null)
                return (null);

            if (_sConnectionName.Trim().Length == 0)
                return (null);


            if (!ConnectionNameExist(_sConnectionName))
                return (null);

            hConnectionInfo = GetConnectionInfo(_sConnectionName);
            return ((Object)hConnectionInfo["DataReaderObject"]);
        }

        public List<Object> GetSQLParameters()
        {
            return (__lSqlParameters);
        }
        public bool IsConnectionOpen()
        {
            return(IsConnectionOpen(SelectedConnection));
        }
        public bool IsConnectionOpen(String _sConnectionName)
        {
            Hashtable hConnectionInfo = null;

            if (_sConnectionName == null)
                return (false);

            if (_sConnectionName.Trim().Length == 0)
                return (false);

            if (!ConnectionNameExist(_sConnectionName))
                return (false);
            hConnectionInfo = GetConnectionInfo(_sConnectionName);
            return ((((String)hConnectionInfo["ConnectionStatus"]).Equals("Open")));
        }
        private bool IsSupportedConnectionType(int _nConnectionType)
        {
            if ((_nConnectionType == CT_CONNSTRING)
                || (_nConnectionType == CT_DIRECT)
                || (_nConnectionType == CT_ODBC)
                || (_nConnectionType == CT_OLE))
                return (true);
            return (false);
        }
        private bool IsSupportedDBProviders(int _nProvider)
        {
            if ((_nProvider == DB_MYSQL)
                || (_nProvider == DB_ORACLE)
                || (_nProvider == DB_SQLSERVER))
                return (true);
            return (false);
        }
        public bool OpenConnection()
        {
            return (OpenConnection(SelectedConnection));
        }
        public bool OpenConnection(String _sConnectionName)
        {
            Hashtable hConnectionInfo;
            String sParseConnString = "";
            bool bConnnectionOpened = false;

            if (_sConnectionName == null)
                return (bConnnectionOpened);

            if (_sConnectionName.Trim().Length == 0)
                return (bConnnectionOpened);

            if (!ConnectionNameExist(_sConnectionName))
                return (bConnnectionOpened);

            // Close DB connection for connection name...
            if (IsConnectionOpen(_sConnectionName))
                CloseConnection(_sConnectionName);

            SelectConnection(_sConnectionName);
            hConnectionInfo = GetConnectionInfo(_sConnectionName);

            // If provided, use provided connection...
            if (ConnectionString != null)
                if (ConnectionString.Trim().Length > 0)
                    sParseConnString = ConnectionString;

            // If not provided, build connection string from connection info...
            if (sParseConnString.Equals(""))
            {
                sParseConnString = "server=" + Server + ";"
                    + "port=" + Port + ";"
                    + "database=" + Database + ";"
                    + "user id=" + User + ";"
                    + "password=" + Password + ";"
                    + "connection timeout=30;";
/*
                if (nDataBaseProvider == DB_MYSQL)
                {
                    sParseConnString = "server=" + sServer + ";"
                        + "port=" + sPort + ";"
                        + "uid=" + sUID + ";"
                        + "password=" + sPasswd + ";"
                    +"database=" + sDatabase + ";";
                }
                if (nDataBaseProvider == DB_ORACLE)
                {
                }
                if (nDataBaseProvider == DB_SQLSERVER)
                {
                    sParseConnString = "user id=" + sUID + ";"
                        + "password=" + sPasswd + ";"
                        + "serve=" + sServer + ";"
                        + "Trusted_Connection=yes;"
                        + "database=" + sDatabase + ";"
                        + "connection timeout=30";
                }
 */
            }
            try
            {
                DataBaseException = "";
                if (DataBaseProvider == DB_MYSQL)
                {
                    MySqlConnection mysqlConnection = new MySqlConnection();
                    mysqlConnection.ConnectionString = sParseConnString;
                    mysqlConnection.Open();
                    SetHashProperty(hConnectionInfo, "ConnectionObject", mysqlConnection);
                }
                if (DataBaseProvider == DB_ORACLE)
                {
                    OracleConnection oraclelConnection = new OracleConnection();
                    oraclelConnection.ConnectionString = sParseConnString;
                    oraclelConnection.Open();
                    SetHashProperty(hConnectionInfo, "ConnectionObject", oraclelConnection);
                }
                if (DataBaseProvider == DB_SQLSERVER)
                {
                    SqlConnection sqlserverConnection = new SqlConnection();
                    sqlserverConnection.ConnectionString = sParseConnString;
                    sqlserverConnection.Open();
                    SetHashProperty(hConnectionInfo, "ConnectionObject", sqlserverConnection);
                }
                SetHashProperty(hConnectionInfo, "CreationState", "Complete");
                SetHashProperty(hConnectionInfo, "ConnectionStatus", "Open");
                SetHashProperty(hConnectionInfo, "Server", Server);
                SetHashProperty(hConnectionInfo, "Port", Port);
                SetHashProperty(hConnectionInfo, "Database", Database);
                SetHashProperty(hConnectionInfo, "DataSource", DataSource);
                SetHashProperty(hConnectionInfo, "Driver", __sDriver);
                SetHashProperty(hConnectionInfo, "Uid", User);
                SetHashProperty(hConnectionInfo, "Passwd", Password);
                SetHashProperty(hConnectionInfo, "DBProvider", Convert.ToString(DataBaseProvider));
                SetHashProperty(hConnectionInfo, "ConnectionString", ConnectionString);
                SetHashProperty(hConnectionInfo, "ParsedConnectionString", sParseConnString);
                SetHashProperty(hConnectionInfo, "ConnectionOpen", "True");
                SetHashProperty(hConnectionInfo, "ConnectionType", ConnectionType);
                SetHashProperty(hConnectionInfo, "CommandType", CommandType);
                SetHashProperty(hConnectionInfo, "DBConnectTime", DateTime.Now.ToString("MM/dd/yyyy [hh:mm:ss tt]", __dtFormat));
                SetHashProperty(hConnectionInfo, "DBConnectTimeStamp", Convert.ToString(DateTime.Now.ToFileTime()));
                SetHashProperty(hConnectionInfo, "ParsedConnectionString", sParseConnString);
                SetHashProperty(hConnectionInfo, "SQLParameters", GetSQLParameters());
                bConnnectionOpened = true;
            }
            catch (Exception eException)
            {
                ErrorException = eException;
                DataBaseException = ErrorException.Message;
            }
            return (bConnnectionOpened);
        }
        public bool RemoveConnection()
        {
            return (RemoveConnection(SelectedConnection));
        }
        public bool RemoveConnection(String _sConnectionName)
        {
            if (_sConnectionName == null)
                return(false);

            if (_sConnectionName.Trim().Length == 0)
                return(false);

            if (!ConnectionNameExist(_sConnectionName))
                return(false);

            if (IsConnectionOpen(_sConnectionName))
                CloseConnection(_sConnectionName);

            if (SelectedConnection.Equals(_sConnectionName))
                ResetConnectionInfo();

            GetConnectionList().Remove(_sConnectionName);
            return(true);
        }
        public void ResetConnectionInfo()
        {
            DataBaseProvider = DB_MYSQL;
            ConnectionType = CT_DIRECT;

            if (!ConnectionNameExist(__sDefaultConnectionName))
            {
                CreateConnection(__sDefaultConnectionName);
            }
            __sServer = null;
            __sDatabase = null;
            __sUID = null;
            __sPasswd = null;
            __sPort = null;
            __sConnectionString = null;
            __sSqlCommand = null;
            __sDataSource = null;
            __sDriver = null;
            __lSqlParameters = null;
        }
        public bool RollbackTransaction()
        {
            return (RollbackTransaction(SelectedConnection));
        }
        public bool RollbackTransaction(String _sConnectionName)
        {
            Hashtable hConnectionInfo;
            int nDBProvider;

            if (_sConnectionName == null)
                return(false);

            if (_sConnectionName.Trim().Length == 0)
                return(false);

            if (!ConnectionNameExist(_sConnectionName))
                return (false);

            hConnectionInfo = GetConnectionInfo(_sConnectionName);
            if (((Object)hConnectionInfo["TransactionObject"]) == null)
                return (false);

            nDBProvider = Convert.ToInt32((String)hConnectionInfo["DBProvider"]);
            try
            {
                DataBaseException = "";
                if (nDBProvider == DB_MYSQL)
                {
                    MySqlTransaction mysqlTransaction = (MySqlTransaction)hConnectionInfo["TransactionObject"];
                    mysqlTransaction.Rollback();
                }
                if (nDBProvider == DB_ORACLE)
                {
                    OracleTransaction oracleTransaction = (OracleTransaction)hConnectionInfo["TransactionObject"];
                    oracleTransaction.Rollback();
                }
                if (nDBProvider == DB_SQLSERVER)
                {
                    SqlTransaction sqlserverTransaction = (SqlTransaction)hConnectionInfo["TransactionObject"];
                    sqlserverTransaction.Rollback();
                }
            }
            catch (Exception eException)
            {
                ErrorException = eException;
                DataBaseException = ErrorException.Message;
                return (false);
            }
            return (true);
        }
        public bool SelectConnection(String _sConnectionName)
        {
            Hashtable hConnectionInfo;

            if (_sConnectionName == null)
                return (false);

            if (_sConnectionName.Trim().Length == 0)
                return (false);

            if (!ConnectionNameExist(_sConnectionName))
                return (false);

            __sSelectedConnectionName = _sConnectionName;
            hConnectionInfo = GetConnectionInfo(SelectedConnection);

            if (((String)hConnectionInfo["CreationState"]).Equals("Complete"))
            {
                __sServer = hConnectionInfo["Server"].ToString();
                __sPort = hConnectionInfo["Port"].ToString();
                __sDatabase = hConnectionInfo["Database"].ToString();
                __sDataSource = hConnectionInfo["DataSource"]?.ToString();
                __sDriver = hConnectionInfo["Driver"]?.ToString();
                __sUID = hConnectionInfo["Uid"].ToString();
                __sPasswd = hConnectionInfo["Passwd"].ToString();
                __sConnectionString = hConnectionInfo["ConnString"]?.ToString();
                 __nConnectionType = Convert.ToInt32(hConnectionInfo["ConnectionType"]);
                 __ctCommandType = (System.Data.CommandType) hConnectionInfo["CommandType"];
                __lSqlParameters = (List<Object>)hConnectionInfo["SQLParameter"];
            }
            DataBaseProvider = Convert.ToInt32((String)hConnectionInfo["DBProvider"]);
            return (true);
        }
        private bool SetConnectionList(Hashtable _hHashtable)
        {
            if (_hHashtable == null)
                return (false);

            __hConnectionList = _hHashtable;
            return (true);
        }
        private Hashtable SetHashProperty(Hashtable _hHashtable, String _sKey, Object _oObject)
        {
            if (_hHashtable == null)
                return (_hHashtable);

            if (_sKey == null)
                return (_hHashtable);

            if (_oObject == null)
                return (_hHashtable);

            _hHashtable.Remove(_sKey);
            _hHashtable.Add(_sKey, _oObject);
            return (_hHashtable);
        }
        public System.Data.CommandType CommandType
        {
            get
            {
                return (__ctCommandType);
            }
            set
            {
                __ctCommandType = value;
            }
        }
        public String ConnectionString
        {
            get
            {
                return (__sConnectionString);
            }
            set
            {
                if (value != null)
                    __sConnectionString = value.Trim();
                else
                    __sConnectionString = value;
            }
        }
        public int ConnectionType
        {
            get
            {
                return (__nConnectionType);
            }
            set
            {
                if (IsSupportedConnectionType(value))
                    __nConnectionType = value;
            }
        }
        public String Database
        {
            get
            {
                return (__sDatabase);
            }
            set
            {
                if (value != null)
                    if (value.Trim().Length > 0)
                        __sDatabase = value.Trim();
            }
        }
        public String DataBaseException
        {
            get
            {
                return (__sDatabaseException);
            }
            private set
            {
                __sDatabaseException = value;
            }
        }
        public int DataBaseProvider
        {
            get
            {
                return (__nDataBaseProvider);
            }
            set
            {
                if (IsSupportedDBProviders(value))
                    __nDataBaseProvider = value;
            }
        }
        public String DataSource
        {
            get
            {
                return (__sDataSource);
            }
            set
            {
                if (__sDataSource != null)
                    __sDataSource = value.Trim();
                else
                    __sDataSource = value;
            }
        }
        public Exception ErrorException
        {
            get
            {
                return (__eErrorException);
            }
            private set { __eErrorException = value; }
        }
        public String Password
        {
            private get { return (__sPasswd); }
            set
            {
                if (value != null)
                    __sPasswd = value.Trim();
                else
                    __sPasswd = value;
            }
        }
        public String Port
        {
            get
            {
                return (__sPort);
            }
            set
            {
                if (value != null)
                    if (value.Trim().Length > 0)
                        if (pdaMediaX.Common.pdamxUtility.IsNumeric(value.Trim()))
                            __sPort = value.Trim();
            }
        }
        public String SelectedConnection
        {
            get
            {
                return (__sSelectedConnectionName);
            }
            set
            {
                if (ConnectionNameExist(value))
                    SelectConnection(value);
            }
        }
        public String Server
        {
            get
            {
                return (__sServer);
            }
            set
            {
                if (value != null)
                    if (value.Trim().Length > 0)
                        __sServer = value.Trim();
            }
        }
        public String SqlCommand
        {
            get
            {
                return (__sSqlCommand);
            }
            set
            {
                if (value != null)
                    if (value.Trim().Length > 0)
                        __sSqlCommand = value.Trim();
            }
        }
        public String User
        {
            get
            {
                return (__sUID);
            }
            set
            {
                if (value != null)
                    if (value.Trim().Length > 0)
                        __sUID = value.Trim();
            }
        }
    }
}
