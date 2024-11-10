using System.Collections.Generic;

namespace Nindot.Al.EventFlow;

public static class ItemTranslationTable
{
    // Switch-case table located at FUN_7100555590
    public static readonly Dictionary<string, string> Table = new(){
        {"コイン[自動取得]", "Coins (Automatic)"},
        {"コイン[放出]", "Coins"},
        {"コイン[放出・極小]", "Coins (Small Amount)"},
        {"コイン[飛出し出現]", "Coins (Flying)"},
        {"コイン[飛出し出現・出現音無し]", "Coins (Silent)"},
        {"コインx3[自動取得]", "3 Coins (Automatic)"},
        {"コインx5[自動取得]", "5 Coins (Automatic)"},
        {"コインx10[自動取得]", "10 Coins (Automatic)"},
        {"コイン[自動取得5枚]", "5 Coins (Automatic)"},
        {"ライフアップアイテム[飛出し出現]", "Life-Up Heart (Flying)"},
        {"ライフアップアイテム[真上出現]", "Life-Up Heart (Above)"},
        {"ライフアップアイテム[逆向き飛出し出現]", "Life-Up Heart (Reverse Spawn)"},
        {"最大ライフアップアイテム[飛出し出現]", "Max Life-Up Heart (Flying)"},
        {"最大ライフアップアイテム[真上出現]", "Max Life-Up Heart (Above)"},
        {"空気泡", "Air Bubble"},
        {"ドットキャラクター(レア)", "2D Character (Rare)"},
        {"跳ねる積みコイン]", "Bouncing Coin Stack"},
    };
}