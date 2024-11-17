using Nindot.Al.EventFlow;
using Nindot.Al.EventFlow.Smo;

namespace Nindot.UnitTest;

public class UnitTestEventDataWrite : IUnitTestGroup
{
    public static void SetupGroup()
    {
    }

    [RunTest]
    public static void ReadGraph()
    {
        Graph flow = Graph.FromFilePath("./src/Nindot.Tests/Resources/Graph-SphinxQuiz.byml", new ProjectSmoEventFlowFactory());
        Test.Should(flow.IsValid());

        Test.Should(flow.WriteFile(Test.TestOutputDirectory + "EventFlowGraphOutput.byml"));

        flow = Graph.FromFilePath(Test.TestOutputDirectory + "EventFlowGraphOutput.byml", new ProjectSmoEventFlowFactory());
        Test.Should(flow.IsValid());
    }

    [RunTest]
    public static void ReadWriteAndCheckGraph()
    {
        Graph flow = Graph.FromFilePath("./src/Nindot.Tests/Resources/Graph-SphinxQuiz.byml", new ProjectSmoEventFlowFactory());
        Test.Should(flow.IsValid());

        Test.Should(flow.WriteFile(Test.TestOutputDirectory + "EventFlowGraphOutput.byml"));

        flow = Graph.FromFilePath(Test.TestOutputDirectory + "EventFlowGraphOutput.byml", new ProjectSmoEventFlowFactory());
        Test.Should(flow.IsValid());
    }

    public static void CleanupGroup()
    {
    }
}