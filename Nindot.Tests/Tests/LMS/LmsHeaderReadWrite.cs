// using System.IO;

// namespace Nindot.UnitTest;

// public class UnitTestLmsHeaderReadWrite : IUnitTestGroup
// {
//     static private byte[] FileData = [];

//     public static void SetupGroup()
//     {
//         FileData = File.ReadAllBytes("./src/Nindot.Tests/Resources/SmoUnitTesting.msbt");
//     }

//     [RunTest]
//     public static void LmsHeaderReadWrite()
//     {
//         LMS.FileHeader head = new(FileData);
//         Test.Should(head.IsValid());

//         MemoryStream stream = new();
//         Test.Should(head.WriteHeader(stream));
//     }

//     public static void CleanupGroup()
//     {
//         FileData = null;
//     }
// }