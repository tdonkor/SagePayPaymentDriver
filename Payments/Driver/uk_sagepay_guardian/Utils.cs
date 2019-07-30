using Integral.Library.GuardianClient;
using System;
using Acrelec.Library.Logger;

namespace Acrelec.Mockingbird.Payment
{
    public enum DiagnosticErrMsg : short
    {
        OK = 0,
        StartTransactionError = 1,
        UnableToStartError = 2,
        NotAuthorisedError = 3,
        CardEnquiryError = 4,
        SwipeCardUsedError = 5,
        TransactionNotConfirmedError = 6
    }

    public class Utils
    {

        

        /// <summary>
        /// Check the numeric value of the amount
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        public static int GetNumericAmountValue(int amount)
        {

            if (amount <= 0)
            {
                Log.Info("Invalid pay amount");
                amount = 0;
            }

            return amount;
        }
    }
}

