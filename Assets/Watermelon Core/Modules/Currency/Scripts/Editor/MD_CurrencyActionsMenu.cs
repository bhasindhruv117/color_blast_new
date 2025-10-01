using UnityEngine;
using UnityEditor;

namespace Watermelon
{
    public static class CurrencyActionsMenu
    {
        [MenuItem("Actions/Currency/Get 200K Coins", priority = 21)]
        private static void LotsOfMoney()
        {
            CurrencyController.Set(CurrencyType.Coins, 200000);
        }

        [MenuItem("Actions/Currency/Get 200K Coins", true)]
        private static bool LotsOfMoneyValidation()
        {
            return Application.isPlaying;
        }

        [MenuItem("Actions/Currency/No Money", priority = 21)]
        private static void NoMoney()
        {
            CurrencyController.Set(CurrencyType.Coins, 0);
        }

        [MenuItem("Actions/Currency/No Money", true)]
        private static bool NoMoneyValidation()
        {
            return Application.isPlaying;
        }
    }
}