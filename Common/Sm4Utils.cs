/***************************************************************************************************
* copyright : 芯迈微半导体（珠海）有限公司
* version   : 1.00
* file      : Sm4Utils.cs
* date      : 2023/03/16
* author    : haozhe.ni
* brief     : 
* section Modification History
* - 1.0 : Initial version - haozhe.ni
***************************************************************************************************/


using System;
using System.Text;

namespace Common
{
    public class Sm4Utils
    {
        public bool HexString = false;
        public string SecretKey = "1234567890123456";

        /// <summary>
        /// 16进制数组转换为16进制字符串
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>       
        public static string ByteToHexStr(byte[] bytes)
        {
            string returnStr = string.Empty;

            if (bytes == null)
            {
                return returnStr;
            }

            for (int i = 0; i < bytes.Length; i++)
            {
                returnStr += bytes[i].ToString("X2");
            }

            return returnStr;
        }

        /// <summary>
        /// 国密SM4 ECB加密方式
        /// </summary>
        /// <param name="plainText">需要加密字符串</param>
        /// <returns></returns>
        public String EncryptECB(String plainText)
        {
            Sm4Context ctx = new Sm4Context
            {
                IsPadding = true,
                Mode = Sm4.Sm4Encrypt
            };

            byte[] keyBytes = HexString ? Hex.Decode(SecretKey) : Encoding.Default.GetBytes(SecretKey);

            Sm4 sm4 = new Sm4();

            sm4.sm4_setkey_enc(ctx, keyBytes);

            byte[] encrypted = sm4.sm4_crypt_ecb(ctx, Encoding.Default.GetBytes(plainText));

            return Encoding.Default.GetString(Hex.Encode(encrypted));
        }


        /// <summary>
        /// 国密SM4 ECB解密方式
        /// </summary>
        /// <param name="cipherText">需要解密字符串</param>
        /// <returns></returns>
        public String DecryptECB(String cipherText)
        {
            Sm4Context ctx = new Sm4Context
            {
                IsPadding = true,
                Mode = Sm4.Sm4Decrypt
            };

            Sm4 sm4 = new Sm4();
            byte[] keyBytes = HexString ? Hex.Decode(SecretKey) : Encoding.Default.GetBytes(SecretKey);

            sm4.sm4_setkey_dec(ctx, keyBytes);

            byte[] decrypted = sm4.sm4_crypt_ecb(ctx, Hex.Decode(cipherText));

            return Encoding.Default.GetString(decrypted);
        }

    }
}
