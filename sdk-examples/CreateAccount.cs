using System;
using System.Collections.Generic;
using Algorand;
using Account = Algorand.Account;
using Algorand.Algod.Api;
using Algorand.Algod.Model;
using Algorand.Client;
using Transaction = Algorand.Transaction;
using System.Text;
using Org.BouncyCastle.Crypto.Parameters;

namespace sdk_examples
{
    public class CreateAccount
    {
        public static void Main(string[] args)
        {
            Account myAccount = new Account();
            var myMnemonic = myAccount.ToMnemonic();
            Console.WriteLine("Account 1 Address = " + myAccount.Address.ToString());
            Console.WriteLine("Account 1 Mnemonic = " + myMnemonic.ToString());
            Console.WriteLine("You have successefully arrived the end of this test, please press and key to exist.");
            Console.ReadKey();
        }
    }
}