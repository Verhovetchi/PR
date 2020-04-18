using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSharp.Framework.Data
{
    partial class DataAccessor
    {
        static Dictionary<string, IDataAccessor> Accessors = new Dictionary<string, IDataAccessor>();

        static DataAccessor()
        {
            Accessors["System.Data.SqlClient"] = new SqlDataAccessor();
        }

        public static void RegisterDataAccessor(string dataProviderType, IDataAccessor accessor)
        {
            Accessors[dataProviderType] = accessor;
        }

        public static IDataAccessor GetDataAccessor(string dataProviderType)
        {
            if (Accessors.TryGetValue(dataProviderType, out var result))
                return result;

            throw new Exception("No DataAccessor is registered for provider type: " + dataProviderType);
        }
    }
}
