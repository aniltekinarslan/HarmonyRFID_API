using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web.Http;

namespace HarmonyRFID_API.Database
{
    public class DBHelper
    {
        #region veri elamanları 

        private PerformanceCounter pc = null;
        private string _connectionString = String.Empty;
        private SqlConnection _con;
        private SqlDataAdapter _adp;
        private SqlTransaction _transaction;

        public SqlTransaction transaction
        {
            get { return _transaction; }
        }

        #endregion

        #region Propertyler

        public SqlConnection Connection
        {
            get { return _con; }
        }

        public string ConnectionString
        {
            get { return _connectionString; }
            set { _connectionString = value; }
        }

        public SqlDataAdapter Adapter
        {
            get { return _adp; }
        }

        #endregion

        #region Constructors-Destructors

        public DBHelper()
        {
            pc = new PerformanceCounter();
            pc.MachineName = Environment.MachineName;
            pc.CategoryName = "Seper";
            pc.CounterName = "DBBağlantiAdedi";
            pc.ReadOnly = true;
        }



        public DBHelper(string connectionString) : this()
        {
            _connectionString = connectionString;
            _con = new SqlConnection(_connectionString);
            // _con.StateChange += new StateChangeEventHandler(durumbilgisi);
        }

        //private void durumbilgisi(object sender , StateChangeEventArgs e) {
        //    //_logger.Info("Baglanti " + e.OriginalState.ToString() + " konumundan " + e.CurrentState.ToString() + " konumuna geçti");
        //}

        ~DBHelper()
        {
            pc.Dispose();
        }


        #endregion

        #region private Methods
        private void Connect(bool isTransactionOpened)
        {
            try
            {
                if (_con != null)
                {

                    if (_con.State == ConnectionState.Closed)
                    {
                        _con.Open();
                        //_logger.Info(" ConnectionState.Closed dı open edildi");
                        if (isTransactionOpened)
                        {
                            _transaction = _con.BeginTransaction();
                        }
                    }
                    else if (_con.State == ConnectionState.Broken)
                    {
                        Wait(300);
                        _con.Open();
                        //_logger.Info("ConnectionState.Broken dı open edildi");
                    }
                    else if (_con.State == ConnectionState.Connecting)
                    {
                        Wait(300);
                        //_logger.Info("ConnectionState.Connecting dı  Wait(300)");
                    }

                }
                else
                {
                    _con = new SqlConnection(_connectionString);
                }
            }
            catch (Exception ex)
            {
                //_logger.Error("Connect1" + ex.Message, ex);

            }

        }

        [Conditional("test")]
        private void Wait(int msn)
        {
            Thread.Sleep(msn);
        }

        private void Disconnect()
        {
            try
            {
                if (_con.State == ConnectionState.Open)
                {
                    Wait(500);
                    _con.Close();
                }
            }
            catch (Exception ex)
            {
                //_logger.Error("Disconnect" + ex.Message, ex);

            }
        }
        public void DisconnectWithTransactions()
        {
            Disconnect();

        }

        private SqlCommand CreateCommand(string commmandtext)
        {
            try
            {
                SqlCommand cmd = _con.CreateCommand();
                cmd.CommandText = commmandtext;
                return cmd;
            }
            catch (Exception ex)
            {
                //_logger.Error(ex.Message, ex);
                return null;
            }
        }
        private SqlCommand CreateCommand(string commmandtext, bool isTransactionOpened)
        {
            try
            {
                SqlCommand cmd = _con.CreateCommand();
                cmd.CommandText = commmandtext;
                if (isTransactionOpened)
                {
                    cmd.Transaction = _transaction;
                }
                return cmd;
            }
            catch (Exception ex)
            {
                //_logger.Error(ex.Message, ex);
                return null;
            }
        }

        private SqlParameter ConvertSqlParameter(DBParameter p)
        {
            try
            {
                SqlParameter prm = new SqlParameter();
                prm.ParameterName = p.Name;

                if (p.Direction == ParameterDirection.Input)
                {
                    prm.SqlValue = p.Value;
                }
                else if (p.Direction == ParameterDirection.Output)
                {
                    prm.Value = prm.SqlValue;
                }
                prm.Direction = p.Direction;
                switch (p.Type)
                {
                    case DBTypes.Char:
                        prm.SqlDbType = SqlDbType.Char;
                        break;
                    case DBTypes.NChar:
                        prm.SqlDbType = SqlDbType.NChar;
                        break;
                    case DBTypes.NVarChar:
                        prm.SqlDbType = SqlDbType.NVarChar;
                        break;
                    case DBTypes.VarChar:
                        prm.SqlDbType = SqlDbType.VarChar;
                        break;
                    case DBTypes.Text:
                        prm.SqlDbType = SqlDbType.Text;
                        break;
                    case DBTypes.NText:
                        prm.SqlDbType = SqlDbType.NText;
                        break;
                    case DBTypes.Date:
                        prm.SqlDbType = SqlDbType.Date;
                        break;
                    case DBTypes.Time:
                        prm.SqlDbType = SqlDbType.Time;
                        break;
                    case DBTypes.DateTime:
                        prm.SqlDbType = SqlDbType.DateTime;
                        break;
                    case DBTypes.SmallDateTime:
                        prm.SqlDbType = SqlDbType.SmallDateTime;
                        break;
                    case DBTypes.Int:
                        prm.SqlDbType = SqlDbType.Int;
                        break;
                    case DBTypes.Double:
                        prm.SqlDbType = SqlDbType.Float;
                        break;
                    case DBTypes.Bit:
                        prm.SqlDbType = SqlDbType.Bit;
                        break;
                    case DBTypes.BigInt:
                        prm.SqlDbType = SqlDbType.BigInt;
                        break;
                    case DBTypes.TinyInt:
                        prm.SqlDbType = SqlDbType.TinyInt;
                        break;
                }

                return prm;
            }
            catch (Exception ex)
            {
                //_logger.Error(ex.Message, ex);
                return null;
            }
        }
        #endregion
        #region public methods


        public DBParameter[] BuildDbParamList(params DBParameter[] prms)
        {
            try
            {
                DBParameter[] arr = new DBParameter[prms.Length];
                for (int i = 0; i <= prms.Length - 1; i++)
                    arr[i] = prms[i];
                return arr;
            }
            catch (Exception ex)
            {
                //_logger.Error(ex.Message, ex);
                return null;
            }
        }
        //<summary>
        //Tablodaki PK olan kolondaki verileri getirir
        //</summary>
        //<param name ="table">Tablo adı</param>
        //<param name ="columnName">Pk kolon adı</param>
        //<returns>PK değeri</returns>
        public ArrayList GetKeyVlues(string table, string colmumnName)
        {
            try
            {
                Connect(false);
                SqlCommand cmd = CreateCommand("select" + colmumnName + "from" + table);
                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.KeyInfo);
                ArrayList list = null;
                if (dr.HasRows)
                {
                    list = new ArrayList();
                    while (dr.Read())
                    {
                        list.Add(dr[colmumnName]);
                    }
                    Disconnect();
                    list.TrimToSize();
                    return list;
                }
                else
                {
                    Disconnect();
                    throw new Exception("Record not found");
                }
            }
            catch (Exception ex)
            {
                //_logger.Error(ex.Message, ex);
                Disconnect();
                return null;
            }
        }
        //<summary>
        //Tablodaki belli bir kolondaki verileri getirir
        //</summary>
        //<param name ="table">Tablo adı</param>
        //<param name ="columnName"> kolon adı</param>
        //<param name ="isDistinct"> Özetleme yapılacakmı</param> 
        //<returns>kolon verileri</returns>
        public ArrayList GetColumnValues(string table, string columnName, bool isDistinct)
        {
            try
            {
                Connect(false);
                string query = "";
                if (isDistinct)
                    query = "select Distinct" + columnName + "from" + table;
                else
                    query = "select " + columnName + "from" + table;
                SqlCommand cmd = CreateCommand(query);
                SqlDataReader dr = cmd.ExecuteReader();
                ArrayList list = null;
                if (dr.HasRows)
                {
                    list = new ArrayList();
                    while (dr.Read())
                    {
                        list.Add(dr[columnName]);
                    }
                    Disconnect();
                    list.TrimToSize();
                    return list;
                }
                else
                {
                    Disconnect();
                    throw new Exception("Record not found");
                }
            }
            catch (Exception ex)
            {
                //_logger.Error(ex.Message, ex);
                Disconnect();
                return null;
            }
        }
        public Object GetSingleValue(string query, DBParameter[] prms, bool isProcedure, bool isTransctionOpened)
        {
            try
            {
                Connect(isTransctionOpened);
                Object o;
                using (SqlCommand cmd = CreateCommand(query, isTransctionOpened))
                {
                    for (int i = 0; i < prms.Length; i++)
                        cmd.Parameters.Add(ConvertSqlParameter(prms[i]));
                    cmd.CommandType = isProcedure ? CommandType.StoredProcedure : CommandType.Text;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr != null)
                        {
                            o = dr.GetValue(0);
                            return o;
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //_logger.Error(ex.Message, ex);
                return null;
            }
        }


        public Object GetSingleValue(string query, DBParameter[] prms, bool isProcedure)
        {
            try
            {
                Connect(false);
                Object o;
                using (SqlCommand cmd = CreateCommand(query))
                {
                    for (int i = 0; i < prms.Length; i++)
                        cmd.Parameters.Add(ConvertSqlParameter(prms[i]));
                    cmd.CommandType = isProcedure ? CommandType.StoredProcedure : CommandType.Text;
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr != null)
                        {
                            o = dr.GetValue(0);
                            return o;
                        }
                        else
                        {
                            return null;
                        }
                    }
                    Disconnect();
                }
            }
            catch (Exception ex)
            {
                //_logger.Error(ex.Message, ex);
                Disconnect();
                return null;
            }
        }
        public bool Execute(string commandText, bool isProcedcure)
        {
            try
            {
                Connect(false);
                using (SqlCommand cmd = CreateCommand(commandText))
                {
                    cmd.CommandType = isProcedcure ? CommandType.StoredProcedure : CommandType.Text;
                    int affectedRows = cmd.ExecuteNonQuery();
                    Disconnect();
                    if (affectedRows > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                //_logger.Error(ex.Message, ex);
                Disconnect();
                return false;
            }
        }
        public bool Execute(string commandText, out int affectedRows, bool isProcedure)
        {
            try
            {
                Connect(false);
                using (SqlCommand cmd = CreateCommand(commandText))
                {
                    cmd.CommandType = isProcedure ? CommandType.StoredProcedure : CommandType.Text;
                    affectedRows = cmd.ExecuteNonQuery();
                    Disconnect();
                    if (affectedRows > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                //_logger.Error(ex.Message, ex);
                affectedRows = 0;
                Disconnect();
                return false;
            }
        }


        public bool Execute(string commandText, DBParameter[] prms, bool isProcedure)
        {
            try
            {
                Connect(false);
                using (SqlCommand cmd = CreateCommand(commandText))
                {
                    for (int i = 0; i < prms.Length; i++)
                        cmd.Parameters.Add(ConvertSqlParameter(prms[i]));
                    cmd.CommandType = isProcedure ? CommandType.StoredProcedure : CommandType.Text;
                    int affectedRows = cmd.ExecuteNonQuery();
                    Disconnect();

                    if (affectedRows > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                //_logger.Error(ex.Message, ex);
                return false;
            }
        }
        public bool Execute(string commandText, DBParameter[] prms, out int affectedRows, bool isProcedure)
        {
            try
            {
                Connect(false);
                using (SqlCommand cmd = CreateCommand(commandText))
                {
                    for (int i = 0; i < prms.Length; i++)
                        cmd.Parameters.Add(ConvertSqlParameter(prms[i]));
                    cmd.CommandType = isProcedure ? CommandType.StoredProcedure : CommandType.Text;
                    affectedRows = cmd.ExecuteNonQuery();
                    Disconnect();
                    if (affectedRows > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                //_logger.Error(ex.Message, ex);
                affectedRows = 0;
                return false;
            }

        }
        public bool Execute(string commandText, DBParameter[] prms, out int affectedRows, bool isProcedure, bool isTransctionOpened)
        {
            try
            {
                Connect(isTransctionOpened);
                using (SqlCommand cmd = CreateCommand(commandText, isTransctionOpened))
                {
                    for (int i = 0; i < prms.Length; i++)
                        cmd.Parameters.Add(ConvertSqlParameter(prms[i]));
                    cmd.CommandType = isProcedure ? CommandType.StoredProcedure : CommandType.Text;
                    affectedRows = cmd.ExecuteNonQuery();
                    if (affectedRows > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                //_logger.Error(ex.Message, ex);
                affectedRows = 0;
                return false;
            }
        }

        public bool Execute(string commandText, DBParameter[] prms, bool isProcedure, bool isTransctionOpened)
        {
            try
            {
                Connect(isTransctionOpened);
                SqlCommand cmd = CreateCommand(commandText, isTransctionOpened);

                for (int i = 0; i < prms.Length; i++)
                    cmd.Parameters.Add(ConvertSqlParameter(prms[i]));

                cmd.CommandType = isProcedure ? CommandType.StoredProcedure : CommandType.Text;
                int affectedRows = cmd.ExecuteNonQuery();

                if (commandText.ToLower().Contains("delete") || affectedRows > 0)
                    return true;

                return false;

            }
            catch (Exception ex)
            {
                //_logger.Error(ex.Message, ex);
                return false;
            }
        }


        public DataTable GetTable(string query, bool isProcedure)
        {
            try
            {
                Connect(false);
                using (SqlCommand cmd = CreateCommand(query))
                {
                    cmd.CommandType = isProcedure ? CommandType.StoredProcedure : CommandType.Text;
                    DataTable dataTable = null;
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader != null)
                        {
                            if (!reader.IsClosed)
                            {
                                dataTable = new DataTable();
                                dataTable.Load(reader);
                            }
                        }
                    }
                    Disconnect();
                    return dataTable;
                }
            }
            catch (Exception ex)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new StringContent("HATA GETTABLE1 =" + query.Substring(0, Math.Min(query.Length, 40)) + " EXMESSAGE--------- " + ex.Message),
                    ReasonPhrase = "Error"
                };
                throw new HttpResponseException(resp);
            }
        }

        public DataTable GetTable(string query, DBParameter[] prms, bool isProcedure)
        {
            try
            {
                Connect(false);
                using (SqlCommand cmd = CreateCommand(query))
                {
                    for (int i = 0; i < prms.Length; i++)
                        cmd.Parameters.Add(ConvertSqlParameter(prms[i]));
                    cmd.CommandType = isProcedure ? CommandType.StoredProcedure : CommandType.Text;
                    DataTable dataTable = null;

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader != null)
                        {
                            if (!reader.IsClosed)
                            {
                                dataTable = new DataTable();
                                dataTable.Load(reader);
                            }
                        }
                    }
                    Disconnect();
                    return dataTable;
                }
            }
            catch (Exception ex)
            {
                //_logger.Error("HATA GETTABLE2 =" + query.Substring(0, Math.Min(query.Length, 40)) + " EXMESSAGE---------" + ex.Message, ex);
                Disconnect();
                return null;
            }



        }
        public DataTable GetTable(string query, DBParameter[] prms, bool isProcedure, bool isTansaction)
        {
            try
            {
                Connect(isTansaction);
                using (SqlCommand cmd = CreateCommand(query, isTansaction))
                {
                    for (int i = 0; i < prms.Length; i++)
                        cmd.Parameters.Add(ConvertSqlParameter(prms[i]));
                    cmd.CommandType = isProcedure ? CommandType.StoredProcedure : CommandType.Text;
                    DataTable dataTable = null;
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader != null)
                        {
                            if (!reader.IsClosed)
                            {
                                dataTable = new DataTable();
                                dataTable.Load(reader);
                            }
                        }
                    }
                    return dataTable;
                }
            }
            catch (Exception ex)
            {
                //_logger.Error("HATA GETTABLE3 =" + query.Substring(0, Math.Min(query.Length, 40)) + " EXMESSAGE---------" + ex.Message, ex);
                return null;
            }
        }
        public DataRow GetFirstRowTable(string query, bool isProcedure)
        {
            try
            {
                Connect(false);
                using (SqlCommand cmd = CreateCommand(query))
                {
                    cmd.CommandType = isProcedure ? CommandType.StoredProcedure : CommandType.Text;
                    DataTable dataTable = new DataTable();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        dataTable.Load(reader);
                    }
                    DataRow dataRow = null;
                    if (dataTable != null && dataTable.Rows.Count > 0)
                    {
                        dataRow = dataTable.Rows[0];
                    }
                    Disconnect();
                    return dataRow;
                }
            }
            catch (Exception ex)
            {
                //_logger.Error("HATA GetFirstRowTable1 =" + query.Substring(0, Math.Min(query.Length, 40)) + " EXMESSAGE---------" + ex.Message, ex);
                Disconnect();
                return null;
            }
        }

        public DataRow GetFirstRowTable(string query, DBParameter[] prms, bool isProcedure)
        {
            try
            {
                Connect(false);
                using (SqlCommand cmd = CreateCommand(query))
                {
                    for (int i = 0; i < prms.Length; i++)
                        cmd.Parameters.Add(ConvertSqlParameter(prms[i]));
                    cmd.CommandType = isProcedure ? CommandType.StoredProcedure : CommandType.Text;
                    DataTable dataTable = new DataTable();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader != null)
                        {
                            if (!reader.IsClosed)
                            {
                                dataTable.Load(reader);
                            }
                        }
                    }
                    DataRow dataRow = null;
                    if (dataTable != null && dataTable.Rows.Count > 0)
                    {
                        dataRow = dataTable.Rows[0];
                    }
                    Disconnect();
                    return dataRow;
                }
            }
            catch (Exception ex)
            {
                //_logger.Error("HATA GetFirstRowTable2 =" + query.Substring(0, Math.Min(query.Length, 40)) + " EXMESSAGE---------" + ex.Message, ex);
                Disconnect();
                return null;
            }



        }
        public DataRow GetFirstRowTable(string query, DBParameter[] prms, bool isProcedure, bool isTansaction)
        {
            try
            {
                Connect(isTansaction);
                using (SqlCommand cmd = CreateCommand(query, isTansaction))
                {
                    for (int i = 0; i < prms.Length; i++)
                        cmd.Parameters.Add(ConvertSqlParameter(prms[i]));
                    cmd.CommandType = isProcedure ? CommandType.StoredProcedure : CommandType.Text;
                    DataTable dataTable = new DataTable();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader != null)
                        {
                            if (!reader.IsClosed)
                            {
                                dataTable.Load(reader);
                            }
                        }
                    }
                    DataRow dataRow = null;
                    if (dataTable != null && dataTable.Rows.Count > 0)
                    {
                        dataRow = dataTable.Rows[0];
                    }
                    return dataRow;
                }
            }
            catch (Exception ex)
            {
                //_logger.Error("HATA GetFirstRowTable3 =" + query.Substring(0, Math.Min(query.Length, 40)) + " EXMESSAGE---------" + ex.Message, ex);
                return null;
            }



        }


        public DataSet GetDataset(string query, bool isProcedure)
        {
            try
            {
                Connect(false);
                using (SqlCommand cmd = CreateCommand(query))
                {
                    cmd.CommandType = isProcedure ? CommandType.StoredProcedure : CommandType.Text;
                    _adp = new SqlDataAdapter();
                    _adp.SelectCommand = cmd;
                    DataSet ds = new DataSet();
                    _adp.Fill(ds);
                    ds.Dispose();
                    Disconnect();
                    return ds;
                }
            }
            catch (Exception ex)
            {
                //_logger.Error(ex.Message, ex);
                Disconnect();
                return null;
            }
        }

        public DataSet GetDataSet(string query, DBParameter[] prms, bool isProcedure)
        {
            try
            {
                Connect(false);

                using (SqlCommand cmd = CreateCommand(query))
                {
                    for (int i = 0; i < prms.Length; i++)
                        cmd.Parameters.Add(ConvertSqlParameter(prms[i]));
                    cmd.CommandType = isProcedure ? CommandType.StoredProcedure : CommandType.Text;
                    _adp = new SqlDataAdapter();
                    _adp.SelectCommand = cmd;
                    DataSet ds = new DataSet();
                    _adp.Fill(ds);
                    ds.Dispose();
                    Disconnect();
                    return ds;
                }
            }
            catch (Exception ex)
            {
                //_logger.Error(ex.Message, ex);
                Disconnect();
                return null;
            }
        }

        public void UpdateDataset(DataSet ds, SqlCommand selectCommand)
        {
            try
            {
                Connect(false);
                _adp = new SqlDataAdapter(selectCommand);
                SqlCommandBuilder builder = new SqlCommandBuilder(_adp);
                _adp.Update(ds);
                Disconnect();
            }
            catch (Exception ex)
            {
                //_logger.Error(ex.Message, ex);
                Disconnect();
            }
        }

        public object GetScalar(string query, bool isProcedure)
        {
            try
            {
                Connect(false);
                SqlCommand cmd = CreateCommand(query);
                if (isProcedure)
                    cmd.CommandType = CommandType.StoredProcedure;
                else
                    cmd.CommandType = CommandType.Text;
                object ret = cmd.ExecuteScalar();
                Disconnect();
                return ret;
            }
            catch (Exception ex)
            {
                //_logger.Error(ex.Message, ex);
                Disconnect();
                return null;
            }
        }

        public object GetScalar(string query, DBParameter[] prms, bool isProcedure)
        {
            try
            {
                Connect(false);
                using (SqlCommand cmd = CreateCommand(query))
                {
                    for (int i = 0; i < prms.Length; i++)
                        cmd.Parameters.Add(ConvertSqlParameter(prms[i]));
                    cmd.CommandType = isProcedure ? CommandType.StoredProcedure : CommandType.Text;
                    object ret = cmd.ExecuteScalar();
                    Disconnect();
                    return ret;
                }
            }
            catch (Exception ex)
            {
                //_logger.Error(ex.Message, ex);
                Disconnect();
                return null;
            }
        }
        public object GetScalar(string query, DBParameter[] prms, bool isProcedure, bool isTransctionOpened)
        {
            try
            {
                Connect(isTransctionOpened);
                using (SqlCommand cmd = CreateCommand(query, isTransctionOpened))
                {
                    for (int i = 0; i < prms.Length; i++)
                        cmd.Parameters.Add(ConvertSqlParameter(prms[i]));
                    cmd.CommandType = isProcedure ? CommandType.StoredProcedure : CommandType.Text;
                    object ret = cmd.ExecuteScalar();
                    return ret;
                }
            }
            catch (Exception ex)
            {
                //_logger.Error(ex.Message, ex);
                return null;
            }
        }
        #endregion
    }
}
