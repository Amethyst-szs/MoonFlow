using Godot;

namespace MoonFlow.Addons;

public static class GitInfo
{
    private static readonly GDScript Data = GD.Load<GDScript>("res://addons/git_database/git.gd");

    public static string GitVersionName() { return Data.Get("version_name").AsString(); }

    public static string GitBranch() { return Data.Get("branch").AsString(); }
    public static string GitAuthor() { return Data.Get("author").AsString(); }

    public static string GitCommitSubject() { return Data.Get("commit_subject").AsString(); }
    public static string GitCommitAuthor() { return Data.Get("commit_author").AsString(); }
    public static string GitCommitHash() { return Data.Get("commit_hash").AsString(); }
    public static string GitCommitHashShort() { return Data.Get("commit_hash_short").AsString(); }

    public static int GitCommitCount() { return Data.Get("commit_count").AsInt32(); }
    public static int GitCommitCountMainBranch() { return Data.Get("commit_count_main_branch").AsInt32(); }
    public static int GitCommitAhead() { return Data.Get("commit_ahead").AsInt32(); }
    public static long GitCommitUnixTime() { return Data.Get("commit_time_unix").AsInt64(); }
}