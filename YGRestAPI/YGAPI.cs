using System.Dynamic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace YGRestAPI;

public class YGAPI
{
    private string login { get; set; }
    private string password { get; set; }
    public string companyName { get; set; }
    public string companyID { get; set; }
    private RestClient client;
    private string key { get; set; }
    private string MyID { get; set; }
    
    public YGAPI()
    {
        client = new RestClient("https://ru.yougile.com/api-v2/");
    }

    public void Keys()
    {
        var keysResponse = GetReq(false,"auth/keys/get",
            new List<Tuple<string, string>>
            {
                new Tuple<string, string>("login", login), 
                new Tuple<string, string>("password", password),
                new Tuple<string, string>("companyId", companyID)
            });
        var keysJS=(dynamic[])YouGileJSON.DeserializeJSON(keysResponse.Content);
        if(keysJS.Length>0)
            key = YouGileJSON.GetProp(keysJS[0], "key");
        else
        {
            var content = GetReq(false, "auth/keys",
                new List<Tuple<string, string>>
                {
                    new Tuple<string, string>("login", login), new Tuple<string, string>("password", password),
                    new Tuple<string, string>("companyId", companyID)
                }).Content;
            if (content != null)
            {
                dynamic keyJs = YouGileJSON.DeserializeJSON(content);
                key = YouGileJSON.GetProp(keyJs, "key");
            }
        }
    }

    public void MyUsrID()
    {
        MyID=UsrID(login);
    }

    public string UsrID(string login)
    {
        
        var usrsJS =YouGileJSON.DeserializeJSON(GetReq(true, "users", null,
            new List<Tuple<string, string>> { new Tuple<string, string>("email", login) },Method.Get).Content);
        var cont=(dynamic[])YouGileJSON.GetProp(usrsJS, "content");
        return YouGileJSON.GetProp(cont[0], "id");
    }
    public Dictionary<string,string> Projects(ref bool next,int count=-1,int offset=0, bool all=false,string YGid="MyID")
    {
        if (YGid == "MyID")
        {
            YGid = MyID;
        }
        var res = new Dictionary<string, string>();
        bool nxt = true;
        int ofst = 0;
        if (count == -1)
        {
            while (nxt)
            {
                var ProjJS = YouGileJSON.DeserializeJSON(GetReq(true, "projects", null,
                    new List<Tuple<string, string>>
                    {
                        new Tuple<string, string>("limit", 100.ToString()),
                        new Tuple<string, string>("offset", ((100 * ofst)+offset).ToString())
                    }, Method.Get).Content);
                var pag = YouGileJSON.GetProp(ProjJS, "paging");
                nxt = YouGileJSON.GetProp(pag, "next");
                var cont = (dynamic[])YouGileJSON.GetProp(ProjJS, "content");
                foreach (var prj in cont)
                {
                    if (!all&&YGid!=null)
                    {
                        var usrs = YouGileJSON.GetProp(prj, "users");
                        if (!YouGileJSON.HasProp(usrs, YGid))
                            break;
                    }

                    var title = YouGileJSON.GetProp(prj, "title");
                    var id = YouGileJSON.GetProp(prj, "id");
                    res.Add((string)title, (string)id);
                }

                ofst++;
            }
        }

        else
        {

            var ProjJS = YouGileJSON.DeserializeJSON(GetReq(true, "projects", null,
                new List<Tuple<string, string>>
                {
                    new Tuple<string, string>("limit", count.ToString()),
                    new Tuple<string, string>("offset", (offset).ToString())
                }, Method.Get).Content);
            var pag = YouGileJSON.GetProp(ProjJS, "paging");
            next = YouGileJSON.GetProp(pag, "next");
            var cont = (dynamic[])YouGileJSON.GetProp(ProjJS, "content");
            foreach (var prj in cont)
            {
                if (!all&&YGid!=null)
                {
                    var usrs = YouGileJSON.GetProp(prj, "users");
                    if (!YouGileJSON.HasProp(usrs,YGid))
                        break;
                }

                var title = YouGileJSON.GetProp(prj, "title");
                var id = YouGileJSON.GetProp(prj, "id");
                res.Add((string)title, (string)id);
            }
        }

        return res;
    }
    
    public Dictionary<string,string> Tasks(bool all=false, string YGid="MyID")
    {
        if (YGid == "MyID")
        {
            YGid = MyID;
        }
        bool next = true;
        bool add = true;
        var res = new Dictionary<string, string>();
        int offset = 0;
        while (next)
        {
            var TaskJS = YouGileJSON.DeserializeJSON(GetReq(true, "tasks", null,
                new List<Tuple<string, string>>
                {
                    new Tuple<string, string>("limit", 100.ToString()),
                    new Tuple<string, string>("offset", (100 * offset).ToString())
                }, Method.Get).Content);
            var pag = YouGileJSON.GetProp(TaskJS, "paging");
            next = YouGileJSON.GetProp(pag, "next");
            var cont = (dynamic[])YouGileJSON.GetProp(TaskJS, "content");
            foreach (var prj in cont)
            {
                if (!all&&YGid!=null)
                {
                    var usrs = YouGileJSON.GetProp(prj, "assigned");
                    if (usrs == null)
                        add = false;
                    else if (usrs.GetType() == typeof(JToken[]))
                    {
                        if (!((dynamic[])usrs).Contains(YGid))
                        {
                            add = false;
                        }
                    }
                    else if (usrs != YGid)
                    {
                        add = false;
                    }
                }

                if (add)
                {
                    var title = YouGileJSON.GetProp(prj, "title");
                    var id = YouGileJSON.GetProp(prj, "id");
                    try
                    {
                        res.Add((string)title, (string)id);
                    }
                    catch
                    {
                        int i = 1;
                        bool rep = true;
                        while (rep)
                        {
                            try
                            {
                                res.Add((string)title + "(" + i + ")", (string)id);
                                rep = false;
                            }
                            catch (Exception e)
                            {
                                i++;
                            }
                        }
                    }
                }

                add = true;
            }

            offset++;
            Task.Delay(2000).Wait();
        }

        return res;
    }

    public string Init(string key)
    {
        this.login = "";
        this.password = "";
        this.companyName = "";
        this.key = key;
        return "OK";
    }


    public string Init(string login, string password, string companyName, Func<string[], bool>? lstFunc=null)
    {
        this.login = login;
        this.password = password;
        this.companyName = companyName;
        var lst = new List<string>();
        var res = GetReq(false,"auth/companies",
            new List<Tuple<string, string>>
            {
                new Tuple<string, string>("login", login),
                new Tuple<string, string>("password", password),
                new Tuple<string, string>("name", companyName)
            });
        if (!res.IsSuccessful)
        {
            var o = YouGileJSON.DeserializeJSON(res.Content);
            try
            {
                return ((dynamic[])YouGileJSON.GetProp(o, "message"))[0];
            }
            catch
            {
                return YouGileJSON.GetProp(o, "message");
            }
        }

        var obj = YouGileJSON.DeserializeJSON(res.Content);
        var pag = YouGileJSON.GetProp(obj, "paging");
        int count = YouGileJSON.GetProp(pag, "count");
        var cont = (dynamic[])YouGileJSON.GetProp(obj, "content");
        if (count > 1)
        {
            for (int i = 0; i < count; i++)
            {
                var name = YouGileJSON.GetProp(cont[i], "name");
                lst.Add(name.ToString());
            }
            if(lstFunc!=null)
                lstFunc(lst.ToArray());
            return "Wait";
        }
        else if (count == 1)
        {
            companyName = YouGileJSON.GetProp(cont[0], "name");
            companyID = YouGileJSON.GetProp(cont[0], "id");
            return "OK";
        }

        return "Нет такой компании у данного пользователя";

    }


    public RestResponse GetReq(bool authkey, string Resour, List<Tuple<string, string>> ParamsBody = null,
        List<Tuple<string, string>> ParamsQuery = null, Method method = Method.Post)

    {
        RestRequest req = new RestRequest();
        req.Method = method;
        req.Resource = Resour;
        req.AddHeader("Content-Type", "application/json");
        string appjs = "{";

        if (ParamsBody == null)
            ParamsBody = new List<Tuple<string, string>>();
        foreach (var par in ParamsBody)
        {
            appjs += "\n" + "\"" + par.Item1 + "\": " + "\"" + par.Item2 + "\",";
        }

        if (ParamsQuery == null)
            ParamsQuery = new List<Tuple<string, string>>();
        foreach (var par in ParamsQuery)
        {
            req.AddQueryParameter(par.Item1, par.Item2);
        }

        appjs = appjs.Remove(appjs.Length - 1);
        appjs += "}";
        if (appjs != "}")
            req.AddParameter("application/json", appjs, ParameterType.RequestBody);
        if (authkey)
            req.AddHeader("Authorization", "Bearer " + key);
        var response = client.Execute(req, method);
        return response;
    }
}