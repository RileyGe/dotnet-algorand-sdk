using Algorand;
using Algorand.Client;
using Algorand.V2;
using System;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Algorand.V2.Model;
using Account = Algorand.Account;

namespace sdk_examples.V2.contract
{
    class DryrunStatefulExample
    {
        public static void Main(params string[] args)
        {
            string ALGOD_API_ADDR = args[0];
            if (ALGOD_API_ADDR.IndexOf("//") == -1)
            {
                ALGOD_API_ADDR = "http://" + ALGOD_API_ADDR;
            }
            string ALGOD_API_TOKEN = args[1];
            var client = new AlgodApi(ALGOD_API_ADDR, ALGOD_API_TOKEN);
            //// TODO: REMOVE:
            string creatorMnemonic = "benefit once mutual legal marble hurdle dress toe fuel country prepare canvas barrel divide major square name captain calm flock crumble receive economy abandon power";
            //string userMnemonic = "pledge become mouse fantasy matrix bunker ask tissue prepare vocal unit patient cliff index train network intact company across stage faculty master mom abstract above";

            ulong localInts = 1;
            ulong localBytes = 0;
            ulong globalInts = 1;
            ulong globalBytes = 0;

            byte[] data = File.ReadAllBytes("V2/contract/hello_world.teal");
            var approval_program_compiled = client.TealCompile(data);
            data = File.ReadAllBytes("V2/contract/hello_world_clear.teal");
            var clear_program_compiled = client.TealCompile(data);
            data = File.ReadAllBytes("V2/contract/hello_world_updated.teal");
            var approval_program_refactored_compiled = client.TealCompile(data);

            var admin = new Account(creatorMnemonic);
            var creator = new Account();
            var user = new Account();
            try
            {
                // transfer to creator and user
                var transParams = client.TransactionParams();
                var amount = Utils.AlgosToMicroalgos(1);
                var tx = Utils.GetPaymentTransaction(admin.Address, creator.Address, amount, "", transParams);
                var signedTx = admin.SignTransaction(tx);
                var id = Utils.SubmitTransaction(client, signedTx);
                var resp = Utils.WaitTransactionToComplete(client, id.TxId);
                Console.WriteLine("Transfer to creator at round: " + resp.ConfirmedRound);

                tx = Utils.GetPaymentTransaction(admin.Address, user.Address, amount, "", transParams);
                signedTx = admin.SignTransaction(tx);
                id = Utils.SubmitTransaction(client, signedTx);
                resp = Utils.WaitTransactionToComplete(client, id.TxId);
                Console.WriteLine("Transfer to user at round: " + resp.ConfirmedRound);

                var appid = CreateApp(client, creator, new TEALProgram(approval_program_compiled.Result),
                    new TEALProgram(clear_program_compiled.Result), globalInts, globalBytes, localInts, localBytes);

                // opt-in to application
                OptIn(client, user, appid);
                OptIn(client, creator, appid);

                //call app from user account updates global storage

                CallApp(client, creator, user, appid, null,
                    new TEALProgram(approval_program_compiled.Result), "hello_world.teal");

                // read global state of application
                ReadGlobalState(client, creator, appid);

                // update application
                UpdateApp(client, creator, appid,
                    new TEALProgram(approval_program_refactored_compiled.Result),
                    new TEALProgram(clear_program_compiled.Result));

                //call application with updated app which updates local storage counter
                CallApp(client, creator, user, appid, null,
                    new TEALProgram(approval_program_refactored_compiled.Result), "hello_world_updated.teal");

                //read local state of application from user account
                ReadLocalState(client, user, appid);

                //close-out from application - removes application from balance record
                CloseOutApp(client, user, (ulong?)appid);

                //opt-in again to application
                OptIn(client, user, appid);

                //call application with arguments
                CallApp(client, creator, user, appid, null,
                    new TEALProgram(approval_program_refactored_compiled.Result), "hello_world_updated.teal");

                // delete application
                // clears global storage only
                // user must clear local
                DeleteApp(client, creator, appid);

                // clear application from user account
                // clears local storage
                ClearApp(client, user, appid);
            }
            catch (ApiException e)
            {
                throw new Exception("Could not get params", e);
            }

            Console.WriteLine("You have successefully arrived the end of this test, please press and key to exist.");
        }

        public static void CloseOutApp(AlgodApi client, Account sender, ulong? appId)
        {
            try
            {
                var transParams = client.TransactionParams();
                var tx = Utils.GetApplicationCloseTransaction(sender.Address, appId, transParams);
                var signedTx = sender.SignTransaction(tx);
                var id = Utils.SubmitTransaction(client, signedTx);
                Console.WriteLine("Successfully sent tx with id: " + id.TxId);
                var resp = Utils.WaitTransactionToComplete(client, id.TxId);
                Console.WriteLine("Close out Application ID is: " + appId);
            }
            catch (ApiException e)
            {
                Console.WriteLine("Exception when calling create application: " + e.Message);
            }
        }

        private static void UpdateApp(AlgodApi client, Account creator, long? appid, TEALProgram approvalProgram, TEALProgram clearProgram)
        {
            try
            {
                var transParams = client.TransactionParams();
                var tx = Utils.GetApplicationUpdateTransaction(creator.Address, (ulong?)appid, approvalProgram, clearProgram, transParams);
                var signedTx = creator.SignTransaction(tx);
                var id = Utils.SubmitTransaction(client, signedTx);
                Console.WriteLine("Successfully sent tx with id: " + id.TxId);
                var resp = Utils.WaitTransactionToComplete(client, id.TxId);
                Console.WriteLine("Updated the application ID is: " + appid);
            }
            catch (ApiException e)
            {
                Console.WriteLine("Exception when calling create application: " + e.Message);
            }
        }

        static long? CreateApp(AlgodApi client, Account creator, TEALProgram approvalProgram,
            TEALProgram clearProgram, ulong? globalInts, ulong? globalBytes, ulong? localInts, ulong? localBytes)
        {
            try
            {
                var transParams = client.TransactionParams();
                var tx = Utils.GetApplicationCreateTransaction(creator.Address, approvalProgram, clearProgram,
                new StateSchema(globalInts, globalBytes), new StateSchema(localInts, localBytes), transParams);
                var signedTx = creator.SignTransaction(tx);
                var id = Utils.SubmitTransaction(client, signedTx);
                Console.WriteLine("Successfully sent tx with id: " + id.TxId);
                var resp = Utils.WaitTransactionToComplete(client, id.TxId);
                Console.WriteLine("Application ID is: " + JObject.Parse(resp.Txn.ToString())["apid"]);
                return resp.ApplicationIndex;
            }
            catch (ApiException e)
            {
                Console.WriteLine("Exception when calling create application: " + e.Message);
                return null;
            }
        }

        static void OptIn(AlgodApi client, Account sender, long? applicationId)
        {
            try
            {
                var transParams = client.TransactionParams();
                var tx = Utils.GetApplicationOptinTransaction(sender.Address, (ulong?)applicationId, transParams);
                var signedTx = sender.SignTransaction(tx);
                var id = Utils.SubmitTransaction(client, signedTx);
                Console.WriteLine("Successfully sent tx with id: " + id.TxId);
                var resp = Utils.WaitTransactionToComplete(client, id.TxId);
                Console.WriteLine("Optin to Application ID: " + resp.ApplicationIndex);
            }
            catch (ApiException e)
            {
                Console.WriteLine("Exception when calling create application: " + e.Message);
            }
        }

        static void DeleteApp(AlgodApi client, Account sender, long? applicationId)
        {
            try
            {
                var transParams = client.TransactionParams();
                var tx = Utils.GetApplicationDeleteTransaction(sender.Address, (ulong?)applicationId, transParams);
                var signedTx = sender.SignTransaction(tx);
                Console.WriteLine("Signed transaction with txid: " + signedTx.transactionID);

                var id = Utils.SubmitTransaction(client, signedTx);
                Console.WriteLine("Successfully sent tx with id: " + id.TxId);
                var resp = Utils.WaitTransactionToComplete(client, id.TxId);
                Console.WriteLine("Optin to Application ID: " + resp.ApplicationIndex);
            }
            catch (ApiException e)
            {
                Console.WriteLine("Exception when calling create application: " + e.Message);
            }
        }

        static void ClearApp(AlgodApi client, Account sender, long? applicationId)
        {
            try
            {
                var transParams = client.TransactionParams();
                var tx = Utils.GetApplicationClearTransaction(sender.Address, (ulong?)applicationId, transParams);
                var signedTx = sender.SignTransaction(tx);
                var id = Utils.SubmitTransaction(client, signedTx);
                Console.WriteLine("Successfully sent tx with id: " + id.TxId);
                var resp = Utils.WaitTransactionToComplete(client, id.TxId);
                Console.WriteLine("Clear Application ID: " + resp.ApplicationIndex);
            }
            catch (ApiException e)
            {
                Console.WriteLine("Exception when calling create application: " + e.Message);
            }
        }

        static void CallApp(AlgodApi client, Account creator, Account user, long? applicationId, List<byte[]> args,
            TEALProgram program, string tealFileName)
        {
            Console.WriteLine("Creator Account:" + creator.Address.ToString());
            Console.WriteLine("User Account:" + user.Address.ToString());
            try
            {
                var transParams = client.TransactionParams();
                var tx = Utils.GetApplicationCallTransaction(user.Address, (ulong?)applicationId, transParams, args);
                var signedTx = user.SignTransaction(tx);
                //Console.WriteLine("Signed transaction with txid: " + signedTx.transactionID);

                var cr = client.AccountInformation(creator.Address.ToString());
                var usr = client.AccountInformation(user.Address.ToString());
                var mydrr = DryrunDrr(signedTx, program, cr, usr);
                var drrFile = "mydrr.dr";
                WriteDrr(drrFile, mydrr);
                Console.WriteLine("drr file created ... debugger starting - goto chrome://inspect");

                // START debugging session
                // either use from terminal in this folder
                // `tealdbg debug program.teal --dryrun-req mydrr.dr`
                //
                // or use this line to invoke debugger
                // and switch to chrome://inspect to inspect and debug
                // (program execution will continue aafter debuigging session completes)

                Excute(string.Format("tealdbg debug {0} --dryrun-req {1}", tealFileName, drrFile));

                var id = Utils.SubmitTransaction(client, signedTx);
                Console.WriteLine("Successfully sent tx with id: " + id.TxId);
                var resp = Utils.WaitTransactionToComplete(client, id.TxId);
                Console.WriteLine("Confirmed at round: " + resp.ConfirmedRound);
                //System.out.println("Called app-id: " + pTrx.txn.tx.applicationId);
                if (resp.GlobalStateDelta != null)
                {
                    Console.WriteLine("    Global state: " + resp.GlobalStateDelta.ToString());
                }
                if (resp.LocalStateDelta != null)
                {
                    Console.WriteLine("    Local state: " + resp.LocalStateDelta.ToString());
                }
            }
            catch (ApiException e)
            {
                Console.WriteLine("Exception when calling create application: " + e.Message);
            }
        }

        private static void Excute(string line)
        {

            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.UseShellExecute = false;    //是否使用操作系统shell启动
            p.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
            p.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息
            p.StartInfo.RedirectStandardError = true;//重定向标准错误输出
            p.StartInfo.CreateNoWindow = true;//不显示程序窗口
            p.Start();//启动程序

            //向cmd窗口发送输入信息
            p.StandardInput.WriteLine(line);
            p.StandardInput.AutoFlush = true;
            //p.StandardInput.WriteLine("exit");
            //向标准输入写入要执行的命令。这里使用&是批处理命令的符号，表示前面一个命令不管是否执行成功都执行后面(exit)命令，如果不执行exit命令，后面调用ReadToEnd()方法会假死
            //同类的符号还有&&和||前者表示必须前一个命令执行成功才会执行后面的命令，后者表示必须前一个命令执行失败才会执行后面的命令

            //获取cmd窗口的输出信息
            string output = p.StandardOutput.ReadToEnd();
            Console.WriteLine(output);

            //StreamReader reader = p.StandardOutput;
            //string line=reader.ReadLine();
            //while (!reader.EndOfStream)
            //{
            //    str += line + "  ";
            //    line = reader.ReadLine();
            //}

            p.WaitForExit();//等待程序执行完退出进程
            p.Close();
            //return output;
        }

        private static void WriteDrr(string filePath, DryrunRequest content)
        {
            var data = Encoder.EncodeToMsgPack(content);
            File.WriteAllBytes(filePath, data);
        }

        static DryrunRequest DryrunDrr(SignedTransaction signTx, TEALProgram program, Algorand.V2.Model.Account cr, Algorand.V2.Model.Account usr)
        {
            var sources = new List<DryrunSource>();

            if (program != null)
            {
                sources.Add(new DryrunSource("approv", Convert.ToBase64String(program.Bytes), 0));
            }
            var drr = new DryrunRequest(new List<SignedTransaction>() { signTx },
                new List<Algorand.V2.Model.Account>() { cr, usr }, sources: sources);
            return drr;
        }

        static public void ReadLocalState(AlgodApi client, Account account, long? appId)
        {
            var acctResponse = client.AccountInformation(account.Address.ToString());
            var applicationLocalState = acctResponse.AppsLocalState;
            for (int i = 0; i < applicationLocalState.Count; i++)
            {
                if (applicationLocalState[i].Id == appId)
                {
                    Console.WriteLine("User's application local state: " + applicationLocalState[i].KeyValue.ToString());
                }
            }
        }

        static public void ReadGlobalState(AlgodApi client, Account account, long? appId)
        {
            var acctResponse = client.AccountInformation(account.Address.ToString());
            var createdApplications = acctResponse.CreatedApps;
            for (int i = 0; i < createdApplications.Count; i++)
            {
                if (createdApplications[i].Id == appId)
                {
                    Console.WriteLine("Application global state: " + createdApplications[i].Params.GlobalState.ToString());
                }
            }
        }
    }
}
