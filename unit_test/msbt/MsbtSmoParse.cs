#if UNIT_TEST
using Godot;
using System;

using Nindot.MsbtContent;
using Nindot.MsbtTagLibrary;
using Nindot.MsbtTagLibrary.Smo;

namespace Nindot.UnitTest;

public class UnitTestMsbtSmoParse : UnitTestBase
{
    private MsbtResource msbt = null;

    public override void SetupTest()
    {
    }

    public override UnitTestResult Test()
    {
        // Load in an msbt resource and check it's validity
        msbt = MsbtResource.FromFilePath("res://unit_test/msbt/SmoUnitTesting.msbt", Core.Type.SUPER_MARIO_ODYSSEY);
        if (msbt == null || !msbt.IsValid()) {
            GD.PrintErr("Failed to initalize MsbtResource for unit test!");
            return UnitTestResult.FAILURE;
        }

        foreach (EntryContent key in msbt.Content.Values)
        {
            bool isSupposedToError = key.Key.EndsWith("Failure");

            foreach (MsbtBaseElement element in key.ElementList)
            {
                // If the current element is a text element, check validity and move on
                if (element.GetType() == typeof(MsbtTextElement)) {
                    if (!element.IsValid()) {
                        GD.PrintErr(string.Format("Text element in {0} is invalid", key));
                        return UnitTestResult.FAILURE; 
                    }

                    continue;
                }

                // At this point we know it is a tag so we can cast to the tag base class and run some more checks
                MsbtTagElement tag = (MsbtTagElement)element;

                // Check the IsValid function of the tag. This is overridable by the specific tag, but the default
                // behavior will ensure the DataSize matches the FixedDataSize (if that tag has a FixedDataSize)
                // as well as the DataSize being an even number
                if (IsElementFaulty(tag.IsValid(), isSupposedToError, key.Key, element)) {
                    GD.PushError("Failed Test");
                    return UnitTestResult.FAILURE;
                }
                
                // Ensure that the GetBytes function returns an array of equal length to the header and data
                bool isByteLengthValid = tag.GetBytes().Length == tag.GetDataSize() + 0x8 && tag.IsValid();
                if (IsElementFaulty(isByteLengthValid, isSupposedToError, key.Key, element)) {
                    GD.PushError("Failed Test");
                    return UnitTestResult.FAILURE;
                }
                
                // Only throw a warning for this, but note down if tag group is of an unknown type, since this should
                // never happen under normal circumstances, but doesn't inheritely mean something is wrong with the
                // parser or file
                if (!Enum.IsDefined(typeof(TagGroup), tag.GetGroupName()) || tag.GetType() == typeof(MsbtTagElementUnknown)) {
                    string warn = string.Format("Tag in {0} is tag group {1} ({2})\nThis led to tag being initilized as {3}",
                        key.Key, tag.GetGroupName(), Enum.GetName(typeof(TagGroup), tag.GetGroupName()), tag.GetType()
                    );

                    GD.PushWarning(warn);
                }
            }
        }

        return UnitTestResult.OK;
    }

    public static bool IsElementFaulty(bool value, bool isSupposedToError, string key, MsbtBaseElement element)
    {
        if (value && isSupposedToError) {
            GD.PrintErr(string.Format("Element {0} in {1} passed as successful when it should have errored", element.GetType(), key));
            return true;
        }

        if (!value && !isSupposedToError) {
            GD.PrintErr(string.Format("Element {0} in {1} errored when it should have succeeded", element.GetType(), key));
            return true;
        }

        return false;
    }

    public override void CleanupTest()
    {
        msbt = null;
    }

    public override void Failure()
    {
    }
}

#endif