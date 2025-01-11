using Godot;

namespace MoonFlow.Addons;

#pragma warning disable IDE0051

public static class GitInfo
{
    private static GodotObject Instance;

    public static string GitVersionName() { return Instance.Get("version_name").AsString(); }

    public static string GitBranch() { return Instance.Get("branch").AsString(); }
    public static string GitAuthor() { return Instance.Get("author").AsString(); }

    public static string GitCommitSubject() { return Instance.Get("commit_subject").AsString(); }
    public static string GitCommitAuthor() { return Instance.Get("commit_author").AsString(); }
    public static string GitCommitHash() { return Instance.Get("commit_hash").AsString(); }
    public static string GitCommitHashShort() { return Instance.Get("commit_hash_short").AsString(); }

    public static string GitCommitCount() { return Instance.Get("commit_count").AsString(); }
    public static string GitCommitCountStable() { return Instance.Get("commit_count_stable").AsString(); }
    public static string GitCommitAhead() { return Instance.Get("commit_ahead").AsString(); }
    public static string GitCommitUnixTime() { return Instance.Get("commit_time_unix").AsString(); }

    #region Initilization

    [StartupTask]
    private static void SetupSingletonInstance() { Instance = Engine.GetSingleton("GitInfoInst"); }

    #endregion
}

#pragma warning restore IDE0051