using Integral.Library.GuardianClient;
using System;
using Acrelec.Library.Logger;

namespace Acrelec.Mockingbird.Payment
{
    public enum DiagnosticErrMsg : short
    {
        OK = 0,
        NOTOK = 1
    }
    public class Utils
    {

        /// <summary>
        /// Create Customer Ticket to output the reciept
        /// </summary>
        /// <param name="ticket"></param>
        public static void CreateCustomerTicket(TransactionInfo ticket)
        {
            string ticketStr = string.Empty;

            //display transactionInfo
            Log.Info("\nTransaction Information");
            Log.Info("-----------------------\n");
            Log.Info($" AuthCode: {ticket.AuthorisationCode}");
            Log.Info($" CardHolder Name: {ticket.CardHolderName}");
            Log.Info($" Currency Code: {ticket.CurrencyCode}");
            Log.Info($" Data Entry Method: {ticket.DataEntryMethod}");
            Log.Info($" Merchant Number: {ticket.MerchantNo}");
            Log.Info($" Response Code: {ticket.ResponseCode}");
            Log.Info($" Scheme Number.: {ticket.SchemeName}");
            Log.Info($" Transaction Amount: {ticket.TransactionAmount}");
            Log.Info($" Transaction Ref Number: {ticket.TransactionRefNo.ToString()}");
            Log.Info($" TerminalId: {ticket.TerminalId}");

            ticketStr = ticket.CustomerReceipt;


            //customer receipt
            Log.Info("\n\nCustomer Receipt");
            Log.Info("===================\n");

            Log.Info($" Customer Reciept: {ticketStr}");
        }

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

