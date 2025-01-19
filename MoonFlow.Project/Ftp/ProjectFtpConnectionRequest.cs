namespace MoonFlow.Project.FTP;

public struct ProjectFtpConnectionRequest(string hostname, int port, string user = "", string pass = "")
{
    public string Hostname = hostname;
    public int Port = port;
    public string Username = user;
    public string Password = pass;
}