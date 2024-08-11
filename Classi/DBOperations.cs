using System.Data.SqlClient;
using System.Data;
using System.Reflection;

namespace ComandeRestAPI.Classi
{
    [Serializable()]
    public class DBOperations<T>
    {
        /// <summary>
        /// Operazine di update su <typeparamref name="T"/>
        /// </summary>
        public virtual void update()
        {
            Type tipo = typeof(T);

            string sql = "update " + tipo.Name + " set ";
            PropertyInfo[] properties = tipo.GetProperties();
            String Index = "";
            foreach (PropertyInfo p in properties)
            {
                Attribute attr = p.GetCustomAttribute(typeof(DB_Index), true);
                if (attr != null) { Index = p.Name; continue; }

                Attribute attr2 = p.GetCustomAttribute(typeof(DB_Exclude), true);
                if (attr2 != null) { continue; }
                if (p.GetValue(this) == null)
                {
                    sql += p.Name + " = null, ";
                    continue;
                }

                if (p.PropertyType == typeof(byte[]))
                {
                    if (((byte[])p.GetValue(this)).Length == 0) sql += p.Name + "= null, "; else sql += p.Name + "= '" + Convert.ToBase64String((byte[])p.GetValue(this)) + "',";
                }
                if (p.PropertyType == typeof(DateTime))
                {
                    // p.GetCustomAttribute(typeof(DB_Index), true);
                    string dt = ((DateTime)p.GetValue(this)).ToShortDateString();
                    if (dt.Contains("01/01/0001")) { sql += p.Name + " = null, "; continue; }
                    if (p.GetCustomAttribute(typeof(DB_LongTime), true) != null || (tipo.Name == "agenda")) dt = ((DateTime)p.GetValue(this)).ToShortDateString() + " " + ((DateTime)p.GetValue(this)).ToLongTimeString();

                    sql += p.Name + " = convert(datetime, '" + dt + "', 103), ";
                }
                if (p.PropertyType == typeof(DateTime?))
                {
                    if ((DateTime?)p.GetValue(this) == null) { sql += p.Name + " = null, "; }
                    else
                    {
                        string dtNullable = ((DateTime)p.GetValue(this)).ToShortDateString();
                        if (p.GetCustomAttribute(typeof(DB_LongTime), true) != null) dtNullable += " " + ((DateTime)p.GetValue(this)).ToLongTimeString();
                        sql += p.Name + " = convert(datetime, '" + dtNullable + "', 103), ";
                    }
                }
                if (p.PropertyType == typeof(int?) || p.PropertyType == typeof(int))
                {
                    if (p.GetValue(this) == null)
                    {
                        sql += p.Name + " = null, ";
                    }
                    else
                    {
                        sql += p.Name + " = " + p.GetValue(this) + ", ";
                    }
                }
                if (p.PropertyType == typeof(string))
                {
                    if ((string)p.GetValue(this) == "")
                    {
                        sql += p.Name + " = null, ";
                    }
                    else
                    {
                        sql += p.Name + " = '" + ((string)p.GetValue(this)).Replace("'", "''") + "', ";
                    }
                }
                if (p.PropertyType == typeof(decimal) || p.PropertyType == typeof(double) || p.PropertyType == typeof(float) ||
                     p.PropertyType == typeof(decimal?) || p.PropertyType == typeof(double?) || p.PropertyType == typeof(float?))
                {
                    if (p.GetValue(this) == null) sql += p.Name + " = null,";
                    else
                        sql += p.Name + " = " + p.GetValue(this).ToString().Replace(",", ".") + ", ";
                }
                if (p.PropertyType == typeof(bool))
                {
                    if ((bool)p.GetValue(this)) sql += p.Name + "=1, "; else sql += p.Name + "=0, ";
                }
            }

            sql = sql.Substring(0, sql.Length - 2);
            sql += " where " + Index + " = " + tipo.GetProperty(Index).GetValue(this);

            db db = new db();
            db.exe(sql);
            db.Dispose();
        }

        public virtual void delete()
        {
            Type tipo = typeof(T);

            string sql = "delete from  " + tipo.Name;
            PropertyInfo[] properties = tipo.GetProperties();
            String Index = "";
            foreach (PropertyInfo p in properties)
            {
                Attribute attr = p.GetCustomAttribute(typeof(DB_Index), true);
                if (attr != null) { Index = p.Name; continue; }

            }

            //sql = sql.Substring(0, sql.Length - 2);
            sql += " where " + Index + " = " + tipo.GetProperty(Index).GetValue(this);

            db db = new db();
            db.exe(sql);
            db.Dispose();

        }


        /// <summary>
        /// Operazione di insert su <typeparamref name="T"/>
        /// </summary>
        public virtual void insert()
        {
            Type tipo = typeof(T);

            string sql = "insert into " + tipo.Name + "(";
            PropertyInfo[] properties = tipo.GetProperties();
            String Index = "";
            foreach (PropertyInfo p in properties)
            {

                Attribute attr = p.GetCustomAttribute(typeof(DB_Index), true);
                if (attr != null) { Index = p.Name; continue; }

                Attribute attr2 = p.GetCustomAttribute(typeof(DB_Exclude), true);
                if (attr2 != null) { continue; }


                sql += p.Name + ", ";
            }
            sql = sql.Substring(0, sql.Length - 2);
            sql += ") values (";
            foreach (PropertyInfo p in properties)
            {
                Attribute attr = p.GetCustomAttribute(typeof(DB_Index), true);
                if (attr != null) { Index = p.Name; continue; }

                Attribute attr2 = p.GetCustomAttribute(typeof(DB_Exclude), true);
                if (attr2 != null) { continue; }


                if (p.GetValue(this) == null)
                { sql += "null, "; continue; }

                if (p.PropertyType == typeof(byte[]))
                {
                    if (((byte[])p.GetValue(this)).Length == 0) sql += "null, "; else sql += "'" + Convert.ToBase64String((byte[])p.GetValue(this)) + "',";
                }

                if (p.PropertyType == typeof(DateTime))
                {
                    string dt = ((DateTime)p.GetValue(this)).ToShortDateString();
                    if (dt.Contains("01/01/0001")) { sql += " null, "; continue; }
                    if (p.GetCustomAttribute(typeof(DB_LongTime), true) != null || (tipo.Name == "agenda")) dt = ((DateTime)p.GetValue(this)).ToShortDateString() + " " + ((DateTime)p.GetValue(this)).ToLongTimeString();
                    sql += "convert(datetime, '" + dt + "', 103), ";
                }
                if (p.PropertyType == typeof(DateTime?))
                {
                    if ((DateTime?)p.GetValue(this) == null) { sql += " null, "; }
                    else
                    {
                        string dtNullable = ((DateTime)p.GetValue(this)).ToShortDateString();
                        if (p.GetCustomAttribute(typeof(DB_LongTime), true) != null) dtNullable += " " + ((DateTime)p.GetValue(this)).ToLongTimeString();
                        sql += "convert(datetime, '" + dtNullable + "', 103), ";
                    }
                }
                if (p.PropertyType == typeof(int?) || p.PropertyType == typeof(int))
                {
                    if (p.GetValue(this) == null)
                    {
                        sql += "null, ";
                    }
                    else
                    {
                        sql += p.GetValue(this) + ", ";
                    }
                }
                if (p.PropertyType == typeof(string))

                    sql += "'" + ((string)p.GetValue(this)).Replace("'", "''") + "', ";
                if (p.PropertyType == typeof(decimal) || p.PropertyType == typeof(double) || p.PropertyType == typeof(float)
                    || p.PropertyType == typeof(decimal?) || p.PropertyType == typeof(double?) || p.PropertyType == typeof(float?))
                {
                    if (p.GetValue(this) == null) sql += "null,";
                    else
                        sql += p.GetValue(this).ToString().Replace(",", ".") + ", ";
                }
                if (p.PropertyType == typeof(bool))
                {
                    if ((bool)p.GetValue(this)) sql += "1, "; else sql += "0, ";
                }

            }

            sql = sql.Substring(0, sql.Length - 2);
            sql += " ) SELECT SCOPE_IDENTITY(); ";

            db db = new db();
            SqlDataReader r = db.getReader(sql);
            r.Read();
            tipo.GetProperty(Index).SetValue(this, int.Parse(r[0].ToString()));
            db.Dispose();

        }

        /// <summary>
        /// Ritorna tutte le occorrenze filtrate con il filtro passato (where già è incluso)
        /// </summary>
        /// <param name="filtro">testo SQL</param>
        /// <returns></returns>
        public static List<T> getAll(string filtro)
        {
            List<T> lst = new List<T>();
            Type tipo = typeof(T);

            // string sql = "select  " + tipo.Name + "(";
            PropertyInfo[] properties = tipo.GetProperties();
            String Index = ""; Type IndexType = null;
            foreach (PropertyInfo p in properties)
            {

                Attribute attr = p.GetCustomAttribute(typeof(DB_Index), true);
                if (attr != null) { Index = p.Name; IndexType = p.PropertyType; continue; }

            }
            filtro = filtro.ToLower();
            filtro = filtro.Trim();

            if (filtro.StartsWith("order")) filtro = "1=1 " + filtro;

            string sql = "select " + Index + " from " + tipo.Name + " where " + filtro;
            db db = new db();
            SqlDataReader r = db.getReader(sql);
            while (r.Read())
            {
                if (IndexType.Name.ToLower() == "int32")
                {
                    object cl = Activator.CreateInstance(typeof(T), new object[] { (int)r[0] });
                    lst.Add((T)cl);
                }
                if (IndexType.Name.ToLower() == "string")
                {
                    object cl = Activator.CreateInstance(typeof(T), new object[] { "'" + (string)r[0] + "'" });
                    lst.Add((T)cl);
                }

            }
            db.Dispose();
            return lst;

        }


        /// <summary>
        /// Ritorna tutte le occorrenze filtrate con il filtro passato (where già è incluso)
        /// </summary>
        /// <param name="filtro">testo SQL</param>
        /// <returns></returns>
        public static async Task<List<T>> getAllAsync(string filtro)
        {
            List<T> lst = new List<T>();
            Type tipo = typeof(T);

            // string sql = "select  " + tipo.Name + "(";
            PropertyInfo[] properties = tipo.GetProperties();
            String Index = ""; Type IndexType = null;
            foreach (PropertyInfo p in properties)
            {

                Attribute attr = p.GetCustomAttribute(typeof(DB_Index), true);
                if (attr != null) { Index = p.Name; IndexType = p.PropertyType; continue; }

            }
            filtro = filtro.ToLower();
            filtro = filtro.Trim();

            if (filtro.StartsWith("order")) filtro = "1=1 " + filtro;

            string sql = "select " + Index + " from " + tipo.Name + " where " + filtro;
            db db = new db();
            SqlDataReader r = await db.getReaderAsync(sql);
            while (r.Read())
            {
                if (IndexType.Name.ToLower() == "int32")
                {
                    object cl = Activator.CreateInstance(typeof(T), new object[] { (int)r[0] });
                    lst.Add((T)cl);
                }
                if (IndexType.Name.ToLower() == "string")
                {
                    object cl = Activator.CreateInstance(typeof(T), new object[] { "'" + (string)r[0] + "'" });
                    lst.Add((T)cl);
                }

            }
            db.Dispose();
            return lst;

        }

        /// <summary>
        /// Ritorna tutte le occorrenze in DataTable filtrate con il filtro passato (where già è incluso)
        /// </summary>
        /// <param name="filtro">testo SQL</param>
        /// <returns></returns>
        public static DataTable getAllDT(string filtro)
        {
            List<T> lst = new List<T>();
            Type tipo = typeof(T);

            filtro = filtro.ToLower();
            filtro = filtro.Trim();

            if (filtro.StartsWith("order") || string.IsNullOrEmpty(filtro)) filtro = "1=1 " + filtro;

            string sql = "select * from " + tipo.Name + " where " + filtro;
            db db = new db();
            DataTable dt = db.getDataTable(sql);

            db.Dispose();
            return dt;

        }


        /// <summary>
        /// Ritorna tutte le occorrenze non filtrate di <typeparamref name="T"/>
        /// </summary>
        /// <returns>List of <typeparamref name="T"/></returns>
        public static List<T> getAll()
        {
            List<T> lst = new List<T>();
            Type tipo = typeof(T);

            // string sql = "select  " + tipo.Name + "(";
            PropertyInfo[] properties = tipo.GetProperties();
            String Index = "";
            foreach (PropertyInfo p in properties)
            {

                Attribute attr = p.GetCustomAttribute(typeof(DB_Index), true);
                if (attr != null) { Index = p.Name; continue; }

            }

            string sql = "select " + Index + " from " + tipo.Name;
            db db = new db();
            SqlDataReader r = db.getReader(sql);
            while (r.Read())
            {
                object cl = Activator.CreateInstance(typeof(T), new object[] { (int)r[0] });
                lst.Add((T)cl);
            }
            db.Dispose();
            return lst;

        }

        public DBOperations(object id)
        {
            Type tipo = typeof(T);

            // string sql = "select  " + tipo.Name + "(";
            PropertyInfo[] properties = tipo.GetProperties();
            String Index = "";
            foreach (PropertyInfo p in properties)
            {

                Attribute attr = p.GetCustomAttribute(typeof(DB_Index), true);
                if (attr != null) { Index = p.Name; continue; }

            }

            string sql = "select * from " + tipo.Name + " where " + Index + "=" + id.ToString();
            db db = new db();
            SqlDataReader r = db.getReader(sql);
            r.Read();
            if (!r.HasRows) return;

            //  object cl = Activator.CreateInstance(typeof(T));
            foreach (PropertyInfo p in properties)
            {
                Attribute exclude = p.GetCustomAttribute(typeof(DB_Exclude), true);
                if (exclude != null) continue;

                if (p.PropertyType == typeof(DateTime) || p.PropertyType == typeof(DateTime?))
                    try { p.SetValue(this, (DateTime)r[p.Name]); } catch { continue; }
                if (p.PropertyType == typeof(int) || p.PropertyType == typeof(int?))
                    try { p.SetValue(this, (int)r[p.Name]); } catch { continue; }
                if (p.PropertyType == typeof(string))
                    p.SetValue(this, r[p.Name].ToString());
                if (p.PropertyType == typeof(decimal) || p.PropertyType == typeof(decimal?))
                    try { p.SetValue(this, decimal.Parse(r[p.Name].ToString())); } catch { continue; }
                if (p.PropertyType == typeof(double) || p.PropertyType == typeof(double?))
                    try { p.SetValue(this, double.Parse(r[p.Name].ToString())); } catch { continue; }
                if (p.PropertyType == typeof(float) || p.PropertyType == typeof(float?))
                    try { p.SetValue(this, float.Parse(r[p.Name].ToString())); } catch { continue; }
                if (p.PropertyType == typeof(bool) || p.PropertyType == typeof(bool?))
                    try { p.SetValue(this, bool.Parse(r[p.Name].ToString())); } catch { continue; }

            }

            db.Dispose();
        }

        public DBOperations() { }



    }
}
