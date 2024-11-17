using Nindot.Al.EventFlow;
using Nindot.Al.EventFlow.Smo;

namespace Nindot.UnitTest;

public class UnitTestEventDataWrite : IUnitTest
{
    public static void SetupTest()
    {
    }

    public static void RunTest()
    {
        Graph flow = Graph.FromFilePath("./src/Nindot.Tests/Tests/EventData/UnitTest-SphinxQuiz.byml", new ProjectSmoEventFlowFactory());
        Test.Should(flow.IsValid());

        Test.Should(flow.WriteFile(Test.TestOutputDirectory + "EventFlowGraphOutput.byml"));

        flow = Graph.FromFilePath(Test.TestOutputDirectory + "EventFlowGraphOutput.byml", new ProjectSmoEventFlowFactory());
        Test.Should(flow.IsValid());
    }

    public static void CleanupTest()
    {
    }
}