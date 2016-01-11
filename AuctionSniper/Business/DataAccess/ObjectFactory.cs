namespace AuctionSniper.Business.DataAccess
{
    using MySql.Data.MySqlClient;
    using System;
    using System.Reflection;

    public static class ObjectFactory
    {

        public static T Insert<T>(this MySqlConnection conn, object p) where T : new()
        {
            string query = "INSERT INTO " + typeof(T).Name;
            string fields = "(";
            string values = "(";

            T obj = default(T);
            conn.Open();
            using (MySqlCommand command = conn.CreateCommand())
            {
                obj = (T)p;
                foreach (PropertyInfo info in p.GetType().GetProperties())
                {
                    if (info.CanRead)
                    {
                        fields += info.Name + ",";
                        values += info.GetValue(p, null) + ",";
                        //object o = info.GetValue(obj, null);
                    }
                }

                //foreach (var item in propertyInfos)
                //{

                //    if (item.val.Equals(name, StringComparison.InvariantCultureIgnoreCase) && item.CanWrite)
                //    {
                //        item.SetValue(obj, reader[i], null);
                //    }
                //}

                command.CommandText = query;
                command.ExecuteNonQuery();
            }
            conn.Close();

            return obj;

        }

        public static T Query<T>(this MySqlConnection conn, string query) where T : new()
        {
            T obj = default(T);
            conn.Open();
            using (MySqlCommand command = new MySqlCommand(query, conn))
            {
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        obj = new T();

                        PropertyInfo[] propertyInfos;
                        propertyInfos = typeof(T).GetProperties();

                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            var name = reader.GetName(i);

                            foreach (var item in propertyInfos)
                            {
                                try
                                {
                                    if (item.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase) && item.CanWrite)
                                    {
                                        item.SetValue(obj, reader[i], null);
                                    }
                                }
                                catch (Exception) { }

                            }

                        }
                    }
                }
            }
            conn.Dispose();

            return obj;
        }

    }
}
