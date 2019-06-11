using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace InterfaceAPI
{
    public static class LoadCertificatePFX
    {
        /// <summary>
        /// Carrega um certificado específico
        /// </summary>
        /// <param name="store"></param>
        /// <returns></returns>
        public static X509Certificate2 LoadCertificate(X509Store store, string serialNumber)
        {
            X509Certificate2 certificado = new X509Certificate2();
            store.Open(OpenFlags.ReadOnly);
            X509Certificate2Collection collection = new X509Certificate2Collection();
            collection = store.Certificates.Find(X509FindType.FindBySerialNumber, serialNumber, true);

            foreach (X509Certificate2 item in store.Certificates)
            {
                if (collection != null && collection.Count > 0 && String.Compare(item.SerialNumber.ToUpper(), serialNumber.ToUpper()) == 0)
                {
                    certificado = item;
                }
            }

            return certificado;
        }

        public static X509Certificate2 LoadCertificate(X509Store store, Func<X509Certificate2, bool> wheres)
        {
            store.Open(OpenFlags.ReadOnly);
            return store.Certificates.OfType<X509Certificate2>().Where(wheres).FirstOrDefault();
        }

        public static X509Certificate2Collection LoadCertificate(X509Store store)
        {
            store.Open(OpenFlags.ReadOnly);
            return store.Certificates;

        }
    }
}
