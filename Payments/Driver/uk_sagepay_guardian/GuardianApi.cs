using System;
using System.Runtime.InteropServices;
using Integral.Library.GuardianClient;
using Acrelec.Library.Logger;

namespace Acrelec.Mockingbird.Payment
{
    public class GuardianApi : IDisposable
    {

        TransactionInfo transactionInfo;
        VoidTransactionHook voidTransactionHook;
        TillInformation tillInformation;
        NonGuiTransactionHook nonGuiTransactionHook;

        TransactionHook.TRANSACTIONHOOK_TRANSACTIONTYPE transactiontype;
        NonGuiTransactionHook.NONGUITRANSACTION_CONFIRMTYPE confirmationType;


        /// <summary>
        /// construtor
        /// </summary>
        public GuardianApi()
        {

            Log.Info("SagePay Driver Transaction Started.....");

            voidTransactionHook = new VoidTransactionHook();
            transactionInfo = new TransactionInfo();
            tillInformation = new TillInformation();
            nonGuiTransactionHook = new NonGuiTransactionHook();
            //default confirmation type
            confirmationType = NonGuiTransactionHook.NONGUITRANSACTION_CONFIRMTYPE.CONFIRMTYPE_AUTHORISED;

        }

        public void Dispose()
        {

        }

        /// <summary>
        /// Payment 
        /// </summary>
        public DiagnosticErrMsg Pay(int amount, out TransactionInfo result)
        {

            int intAmount;
            DiagnosticErrMsg isSuccessful = DiagnosticErrMsg.OK;

            //initialise the result - fill this with the customer reciept to display
            result = null;

            //we only do sales
            transactiontype = TransactionHook.TRANSACTIONHOOK_TRANSACTIONTYPE.INT_TT_SALE;

            //check amount is valid
            intAmount = Utils.GetNumericAmountValue(amount);

            if (intAmount == 0)
            {
                throw new Exception("Error in Amount value");
            }

            Log.Info($"Valid payment amount: {intAmount}");

            //add the address of the store for the reciept
            AddAddress();

            //Use a non GUI transaction - if transaction is true proceed to the CardEnquiry stage.
            if (nonGuiTransactionHook.StartTransaction(ref tillInformation) == true)
            {
                Log.Info("Transaction Started....");

                //check the card enquiry returns: RETURNCODE_SUCCESS or RETURNCODE_CASHBACKALLOWED
                if (nonGuiTransactionHook.CardEnquiry(transactiontype,
                                                intAmount,
                                                NonGuiTransactionHook.NONGUITRANSACTION_DATAENTRY.DATAENTRY_CHIPORSWIPEORTAP) == NonGuiTransactionHook.NONGUITRANSACTION_RETURNCODE.RETURNCODE_SUCCESS)
                {

                    //authorise the transaction
                    Log.Info("Transaction Authorisation Started....");

                    if (nonGuiTransactionHook.AuthoriseTransaction(TransactionHook.TRANSACTIONHOOK_TRANSACTIONTYPE.INT_TT_SALE, intAmount, 0, "SALE", ref transactionInfo) == NonGuiTransactionHook.NONGUITRANSACTION_RETURNCODE.RETURNCODE_SUCCESS)
                    {

                        if (transactionInfo.DataEntryMethod == TransactionInfo.TRANSINFO_DATAENTRYMETHOD.TRANSINFO_DE_SWIPED)
                        {

                            Log.Info("Swipe transaction - CANCEL the transaction.");
                            confirmationType = NonGuiTransactionHook.NONGUITRANSACTION_CONFIRMTYPE.CONFIRMTYPE_CANCELLED;
                            isSuccessful = DiagnosticErrMsg.NOTOK;
                        }


                        // confirm the transaction
                        if (nonGuiTransactionHook.ConfirmTransaction(confirmationType,
                                                                   nonGuiTransactionHook.TransactionReference,
                                                                   transactionInfo.AuthorisationCode,
                                                                   ref transactionInfo) == true)
                        {
                            result = transactionInfo;
                        }
                    }
                }

            }

            //end the transaction
            nonGuiTransactionHook.EndTransaction();
            Log.Info("SagePay Driver Transaction Finished.....");

            return isSuccessful;

        }

        private void AddAddress()
        {
            // Populate the till information object
            tillInformation.MerchantName = "Acrelec";
            tillInformation.Address1 = "East Wing, Focus 31";
            tillInformation.Address2 = "Mark Road";
            tillInformation.Address3 = "Hemel Hempstead";
            tillInformation.Address4 = "HP2 7BW";
            tillInformation.PhoneNumber = "1234567890";
        }


    }
}
