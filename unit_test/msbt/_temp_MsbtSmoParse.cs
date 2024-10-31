#if DO_NOT_COMPILE_ME_THANK_YOU_VERY_MUCH_HI_HOW_ARE_YOU_DOING_TODAY_IM_DOING_PRETTY_GOOD_THANKS_FOR_ASKING
using Godot;
using System;

using Nindot.LMS.Msbt;
using Nindot.LMS.Msbt.TagLib;
using Nindot.LMS.Msbt.TagLib.Smo;
using System.Collections.Generic;
using System.Linq;

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

        return ScanElements(msbt);
    }

    public static UnitTestResult ScanElements(MsbtResource res, bool isCheckIntendedFailureKeys = true)
    {
        foreach (EntryContent key in res.Content.Values)
        {
            bool isSupposedToError = key.Key.EndsWith("Failure");

            if (isSupposedToError && !isCheckIntendedFailureKeys)
                continue;

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
                if (tag.GetType() == typeof(MsbtTagElementUnknown) && tag.GetGroupName() != 0xC9) {
                    string warn = string.Format("{0} is tag group {1} ({2}) and subtype {3} ({4}), leading to init of {5}",
                        key.Key, tag.GetGroupName(), Enum.GetName(typeof(TagGroup), tag.GetGroupName()),
                        tag.GetTagName(), tag.GetTagNameStr(), tag.GetType()
                    );

                    GD.PushWarning(warn);
                    
                    return UnitTestResult.FAILURE;
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