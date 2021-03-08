using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OPD.Helper
{
    public static class DatabaseHelper
    {
        public static String getDbConnectionString(String name)
        {
            return System.Configuration.ConfigurationManager.ConnectionStrings[name].ConnectionString;
        }

        public static String getDbConnectionString()
        {
            return getDbConnectionString("OPD");
        }
    }
}