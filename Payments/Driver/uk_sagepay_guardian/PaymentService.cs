using Acrelec.Library.Logger;
using Acrelec.Mockingbird.Payment.Configuration;
using Acrelec.Mockingbird.Payment.Contracts;
using System;
using System.Text;
using System.IO;
using System.Reflection;
using System.ServiceModel;
using Integral.Library.GuardianClient;

namespace Acrelec.Mockingbird.Payment
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class PaymentService : IPaymentService
    {
        private static readonly string ticketPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "ticket");

        /// <summary>
        /// Get the configuratiion data
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public Result Init(RuntimeConfiguration configuration)
        {
            Log.Info("Init method started...");

            //initalise confguration file instance
            var configFile = AppConfiguration.Instance;

            try
            {
                if (configuration == null)
                {
                    Log.Info("Can not set configuration to null.");
                    return ResultCode.GenericError;
                }

                if (configuration.PosNumber <= 0)
                {
                    Log.Info($"Invalid PosNumber {configuration.PosNumber}.");
                    return ResultCode.GenericError;
                }

                if (configuration.Port == string.Empty)
                {
                    Log.Info($"Invalid Port {configuration.Port}.");
                    return ResultCode.GenericError;
                }


                using (var api = new GuardianApi())
                {
                    RuntimeConfiguration.Instance = configuration;
                    Heartbeat.Instance.Start();
                    Log.Info("Init success!");

                    return ResultCode.Success;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return ResultCode.GenericError;
            }
            finally
            {
                Log.Info("Init method finished.");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Result Test()
        {
            var alive = Heartbeat.Instance?.Alive == true;
            Log.Debug($"Test status: {alive}");
            return alive ? ResultCode.Success : ResultCode.GenericError;
        }

        /// <summary>
        /// Payment method
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        public Result<PaymentData> Pay(int amount)
        {
            Log.Info("Pay method started...");
            Log.Info($"Amount = {amount/100.0}.");
            Result<PaymentData> transactionResult = null;

            try
            {
                if (File.Exists(ticketPath))
                {
                    File.Delete(ticketPath);
                }

                if (amount <= 0)
                {
                    Log.Info("Invalid pay amount.");
                    return ResultCode.GenericError;
                }

                var config = RuntimeConfiguration.Instance;
                var data = new PaymentData();

                Log.Info("Calling payment driver...");

                using (var api = new GuardianApi())
                {

                    var payResult = api.Pay(amount, out TransactionInfo payResponse);
                    Log.Info($"Pay Result: {payResult}");

                    if (payResult != DiagnosticErrMsg.OK)
                    {
                        Log.Error($"Pay Result Not OK: {payResult} Payment Failed.");
                        //return new Result<PaymentData>((ResultCode)DiagnosticErrMsg.NOTOK);
                        data.Result = PaymentResult.Failed;
                   
                        //print the payment ticket for an error
                        //
                        CreateCustomerTicket("-----\n\nPayment failure with\nyour card or issuer\nNO payment has been taken.\n\nPlease try again with another card,\nor at a manned till.\n\n-----");
                        data.HasClientReceipt = true;

                        return new Result<PaymentData>((ResultCode)payResult, data: data);
                    }
                    else
                    {
                        data.Result = PaymentResult.Successful;
                       
                        data.PaidAmount = amount;

                        Log.Info($"paid Amount: {data.PaidAmount}");
                        transactionResult = new Result<PaymentData>(ResultCode.Success, data: data);
                        Log.Info($"Payment succeeded transaction result: {transactionResult}");

                     //   CreateCustomerTicket(payResponse);
                        data.HasClientReceipt = true;

                    }


                    //persist the transaction
                  //  PersistTransaction(payResponse);
                }


                return transactionResult;
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return ResultCode.GenericError;
            }
            finally
            {
                Log.Info("Pay method finished.");
            }
        }

        /// <summary>
        /// Shutdown
        /// </summary>
        public void Shutdown()
        {
            Log.Info("Shutting down...");
            Program.ManualResetEvent.Set();
        }

        /// <summary>
        /// Persist the transaction as Text file
        /// with Customer and Merchant receiept
        /// </summary>
        /// <param name="result"></param>
        private void PersistTransaction(TransactionInfo result)
        {
            try
            {
                var config = AppConfiguration.Instance;
                var outputDirectory = Path.GetFullPath(config.OutPath);
                var outputPath = Path.Combine(outputDirectory, $"{DateTime.Now:yyyyMMddHHmmss}_ticket.txt");

                if (!Directory.Exists(outputDirectory))
                {
                    Directory.CreateDirectory(outputDirectory);
                }          

                Log.Info("Persisting Customerticket to {0}", outputPath);

                //Write the new ticket
                File.WriteAllText(outputPath, result.CustomerReceipt);
            }
            catch (Exception ex)
            {
                Log.Error("Persist Transaction exception.");
                Log.Error(ex);
            }
        }


        //overload the customer ticket to check to return a string
        // output on error
        /// <summary>
        ///  Create Customer Ticket to output the reciept error string
        /// </summary>
        /// <param name="ticket"></param>
        private static void CreateCustomerTicket(string ticket)
        {
            try
            {
                Log.Info($"Persisting Customer ticket to {ticketPath}");

                //Write the new ticket
                File.WriteAllText(ticketPath, ticket);

            }
            catch (Exception ex)
            {
                Log.Error("Error persisting ticket.");
                Log.Error(ex);
            }
        }

        /// <summary>
        /// Create Customer Ticket to output the reciept
        /// </summary>
        /// <param name="ticket"></param>
        private static void CreateCustomerTicket(TransactionInfo ticket)
        {
  
            try
            {
                Log.Info($"Persisting Customer ticket to {ticketPath}");
                //Write the new ticket
                File.WriteAllText(ticketPath, ticket.CustomerReceipt);
            }
            catch (Exception ex)
            {
                Log.Error("Error persisting ticket.");
                Log.Error(ex);
            }
        }
    }
}
