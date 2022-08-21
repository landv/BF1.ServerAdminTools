namespace BF1.ServerAdminTools.Features.Config;

public class AuthConfig
{
    public bool IsUseMode1;
    public List<AuthInfo> AuthInfos;
    public class AuthInfo
    {
        public string AuthName;
        public string Remid;
        public string Sid;
        public string SessionId;
    }
}
