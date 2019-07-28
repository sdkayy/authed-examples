using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using System.Timers;

class Authed {
    public internal string AppSession;
    public internal string UserSession;
    public internal string AppID;
    public internal string UserKey;
    public internal string Access;
    private internal string baseUri = "https://api.authed.io/app";

    public Authed(string id, string access, string userKey) {
        this.AppID = id;
        this.UserKey = userKey;
        this.Access = access;
    }

    private dynamic CallApi(string url, Dictionary<string, string> body, Dictionary<string, string> headers) {
        WebClient wc = new WebClient();
        wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
        string bodyString = "";
        foreach(KeyValuePair<string, string> entry in body)
        {
            bodyString.Length > 0 ? bodyString += $"{entry.Key}={entry.Value}" : bodyString = $"{entry.Key}={entry.Value}";
        }
        foreach(KeyValuePair<string, string> entry in headers) {
            wc.Headers.Add(entry.Key, entry.Value);
        }
        return new JsonConvert.DeserializeObject(wc.UploadString(url, bodyString));
    }

    private void DoAppSessionCheck() {
        if(this.AppSession == null) throw new System.Exception("Please verify before logging in!");
    }
    
    private void DoUserSessionCheck() {
        if(this.UserSession == null) throw new System.Exception("Please login before accessing this function!");
    }

    private void StartUserSesionTimer() {
        System.Timers.Timer sessTimer = new System.Timers.Timer(25000);
        sessTimer.Elapsed += OnTimedEvent;
        sessTimer.Enabled = true;
        sessTimer.Start();
    }

    public boolean VerifyUserSession() {
        var headers = new Dictionary<string, string>()
        {
            { "session", this.AppSession }
        };

        var body = new Dictionary<string, string>()
        {
            { "userSession", this.UserSession }
        };

        dynamic json = this.CallApi($"{this.baseUri}/session", body, headers);
        if(json.userSession != null) {
            // Use Jose.JWT to verify this if you want. Its more "secure"
            this.UserSession = json.userSession;
            // CHANGE
        } else {
            // Bad exit, invalid user
            Environment.Exit(99);
            // Do something here
            // CHANGE
        }
    }

    private static void OnTimedEvent(Object source, ElapsedEventArgs e)
    {
        this.VerifyUserSession();
    }

    public boolean Verify() {
        var body = new Dictionary<string, string>() 
        {
            { "access",  this.Access }
        };

        dynamic json = this.CallApi($"{this.baseUri}/verify/{this.AppID}", body, null);
        if(json.session != null) {
            this.AppSession = json.session;
            return true;
        } else {
            throw new System.Exception(json.message);
        }
    }
    
    public boolean Login(string email, string password) {
        this.DoAppSessionCheck();

        var headers = new Dictionary<string, string>()
        {
            { "session", this.AppSession },
        };

        var body = new Dictionary<string, string>()
        {
            { "email", email },
            { "password", password}
        };

        dynamic json = this.CallApi($"{this.baseUri}/login", body, headers);

        if(json.userSession != null) {
            this.UserSession = json.userSession;
            this.StartUserSesionTimer();
            return true;
        } else {
            throw new System.Exception(json.message);
        }
    }

    public boolean Register(string email, string password, string license) {
        this.DoAppSessionCheck();

        var headers = new Dictionary<string, string>()
        {
            { "session", this.AppSession },
        };

        var body = new Dictionary<string, string> ()
        {
            { "email", email },
            { "password", password },
            { "licenseCode", license }
        };

        dynamic json = this.CallApi($"{this.baseUri}", body, headers);

        if(json.status == 200) return true;
        else throw new System.Exception(json.message);
        
    }

    public boolean Renew(string email, string license) {
        this.DoAppSessionCheck();
        var headers = new Dictionary<string, string>()
        {
            { "session", this.AppSession }
        };

        var body = new Dictionary<string, string>()
        {
            { "email", email },
            { "licenseCode", license }
        };

        dynamic json = this.CallApi($"{this.baseUri}/renew", body, headers);

        if(json.status == 200) return true;
        else throw new System.Exception(json.message);
    }

    public boolean UpdateUser(string field, string value) {
        this.DoAppSessionCheck();
        this.DoUserSessionCheck();

        var headers = new Dictionary<string, string>()
        {
            { "session", this.AppSession }
        };

        var body = new Dictionary<string, string>()
        {
            { "userSession", this.UserSession },
            { "field", field },
            { "value", valie },
        };

        dynamic json = this.CallApi($"{this.baseUri}/users/set", body, headers);

        if(json.status == 200) return true;
        else throw new System.Exception(json.message);
    }

    public boolean UpdateApp(string secret, string field, string value) {
        this.DoAppSessionCheck();

        var headers = new Dictionary<string, string>()
        {
            { "session", this.AppSession }
        };

        var body = new Dictionary<string, string>()
        {
            { "secret", secret },
            { "value", value }
        };

        dynamic json = this.CallApi($"{this.baseUri}/set/${field}", body, headers);

        if(json.status == 200) return true;
        else throw new System.Exception(json.message);
    }

    public GenerateLicense(string secret, string prefix, int level, int time, int amount) {
        this.DoAppSessionCheck();

        var headers = new Dictionary<string, string>()
        {
            { "session", this.AppSession }
        };

        var body = new Dictionary<string, string>()
        {
            { "secret", secret },
            { "prefix", prefix },
            { "level", level },
            { "time", time },
            { "amount", amount }
        };

        dynamic json = this.CallApi($"{this.baseUri}/generate/license", body, headers);

        if(json.status == 200) return json.message;
        else throw new System.Exception(json.message);
    }
}