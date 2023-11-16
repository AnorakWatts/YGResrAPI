using System.Dynamic;
using System.Text.Json;
using System.Dynamic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace YGRestAPI;

public  static class YouGileJSON
{
    public static bool HasProp(dynamic o,string propname)
    {
        if (o.GetType() == typeof(JObject))
        {
            var reso = (JObject)o;
            return reso.ContainsKey(propname);
        }
        try
        {
            var dic = ((IDictionary<string, object>)o);
            return dic.ContainsKey(propname);
        }
        catch
        {
            return false;
        }
    }

    public static object GetProp(dynamic o, string propname)
    {
        if (HasProp(o, propname))
        {
            if (o.GetType() == typeof(JObject))
            {
                var reso = (JObject)o;
                var res= reso.GetValue(propname).Value<object>();
                if (res.GetType() == typeof(JArray))
                {
                    return ((JArray)res).ToArray();
                }

                return res;
            }
            try
            {
                return ((IDictionary<string, object>)o)[propname];
            }
            catch
            {
                return null;
            }
        }
        else
        {
            return null;
        }
    }

    public static dynamic DeserializeJSON(string js)
    {
        dynamic json = JsonConvert.DeserializeObject(js);
        if (json.GetType() == typeof(JArray))
        {
            return ((JArray)json).ToArray();
        }
        return json;
    }

    public static string SerializeJSON(object o)
    {
        return JsonConvert.SerializeObject(o);
    }
}