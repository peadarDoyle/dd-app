using Microsoft.VisualStudio.TestTools.UnitTesting;
using DirectDebits.Common.Utility;

namespace DirectDebits.Tests.Services
{
    [TestClass]
    public class StringCipherTest
    {
        [TestMethod]
        public void Encrpyt_IsSuccess()
        {
            string plainText = "plain text is not secure - rather, one should encrypt";
            string passPhrase = "super secret passphrase 45649846489496495635697";

            string encryptedText = StringCipher.Encrypt(plainText, passPhrase);

            Assert.AreNotEqual(plainText, encryptedText);
        }

        [TestMethod]
        public void Decrpyt_IsSuccess()
        {
            string plainText = "plain text is not secure - rather, one should encrypt";
            string passPhrase = "super secret passphrase 45649846489496495635697";

            string encryptedText = StringCipher.Encrypt(plainText, passPhrase);
            string decryptedText = StringCipher.Decrypt(encryptedText, passPhrase);

            Assert.AreEqual(plainText, decryptedText);
        }
    }
}
