using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using Traficante.TSQL.Lib.Attributes;
using Traficante.TSQL.Lib.Helpers;
using Traficante.TSQL.Schema;

namespace Traficante.TSQL.Lib
{
    [BindableClass]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "UnusedParameter.Global")]
    public partial class Library
    {
        public Library()
        {

        }

        [BindableMethod]
        public int RowNumber()
        {
            return default(int);
        }

        [BindableMethod]
        public string Sha512(string content)
        {
            if (content == null)
                return null;

            return HashHelper.ComputeHash<SHA512Managed>(content);
        }

        [BindableMethod]
        public string Sha256(string content)
        {
            if (content == null)
                return null;

            return HashHelper.ComputeHash<SHA256Managed>(content);
        }

        [BindableMethod]
        public string Md5(string content)
        {
            if (content == null)
                return null;

            return HashHelper.ComputeHash<MD5CryptoServiceProvider>(content);
        }

    }
}