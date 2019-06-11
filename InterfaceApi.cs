using System;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace InterfaceAPI
{
    public static class InterfaceApi
    {
        /// <summary>
        /// Utilizado pra carregar o certificado
        /// </summary>
        private static X509Certificate2 Certificado { get; set; }

        /// <summary>
        /// Opções do request
        /// </summary>
        public static X509Store X509Store { get; set; }
        public static List<string[]> Headers = new List<string[]>();
        public static string ContenType { get; set; }
        public static string BaseUrl { get; set; }
        public static string Url { get; set; }

        /// <summary>
        /// Prepara pra fazer uma requisição
        /// </summary>
        /// <returns></returns>
        public static WebRequest Init(string serialNumber = "")
        {
            if (X509Store != null && !String.IsNullOrEmpty(serialNumber)) Certificado = LoadCertificatePFX.LoadCertificate(X509Store, serialNumber);
            return (HttpWebRequest)WebRequest.Create(Url);
        }

        /// <summary>
        /// Pega o request
        /// </summary>
        /// <param name="webRequest"></param>
        /// <returns></returns>
        public static WebRequest GetRequest(this WebRequest webRequest)
        {
            ServicePointManager.ServerCertificateValidationCallback = (a, b, c, d) => true;
            webRequest = (HttpWebRequest)WebRequest.Create(Url);
            HttpWebRequest request = webRequest as HttpWebRequest;
            //HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            request.Referer = BaseUrl;
            request.Method = "GET";
            request.AllowAutoRedirect = true;
            if (!String.IsNullOrEmpty(ContenType))
            {
                request.ContentType = ContenType;
                request.Accept = ContenType;
            }
            const byte key = 0; const byte value = 1;
            foreach (String[] item in Headers) request.Headers.Add(Convert.ToString(item[key]), Convert.ToString(item[value]));

            request.MediaType = "application/json";
            request.UseDefaultCredentials = true;
            request.ClientCertificates.Add(Certificado);

            return request;
        }

        /// <summary>
        /// Pega o response do request
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static HttpWebResponse GetResponse(this WebRequest request)
        {
            return (HttpWebResponse)request.GetResponse();
        }

        /// <summary>
        /// Retorna como string
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public static string GetResponseTXT(this WebResponse response)
        {
            string text;
            using (StreamReader reader = new StreamReader(response.GetResponseStream())) text = reader.ReadToEnd();
            return text;
        }

        /// <summary>
        /// Caso a resposta seja um stream
        /// </summary>
        /// <param name="response"></param>
        /// <returns>XmlDocument</returns>
        public static XmlDocument GetResponseXML(this WebResponse response)
        {
            XmlDocument xml = new XmlDocument();
            using (StreamReader stream = new StreamReader(response.GetResponseStream()))
            {
                using (XmlReader reader = XmlReader.Create(stream)) xml.Load(reader);
            }
            return xml;
        }

        /// <summary>
        /// Caso a resposta seja um JSON
        /// </summary>
        /// <param name="response"></param>
        /// <param name="root"></param>
        /// <returns>XmlDocument</returns>
        public static XmlDocument GetResponseXMLFromJSON(this WebResponse response, string root)
        {
            string text = GetResponseTXT(response);
            return JsonConvert.DeserializeXmlNode(text, root);
        }

        /// <summary>
        /// Caso seja um stream
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="response"></param>
        /// <returns></returns>
        public static T GetResponseJSON<T>(this WebResponse response)
        {
            JsonSerializer serializer = new JsonSerializer();
            T jsonResponse;
            using (StreamReader reader = new StreamReader(response.GetResponseStream())) jsonResponse = (T)serializer.Deserialize(reader, typeof(T));
            return jsonResponse;
        }

        /// <summary>
        /// Caso a data esteja mal formatada
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="response"></param>
        /// <returns></returns>
        public static T GetResposeJSONDataFormating<T>(this WebResponse response)
        {
            string text = GetResponseTXT(response);
            var format = "dd/MM/yyyy HH:mm:ss"; // your datetime format
            var dateTimeConverter = new IsoDateTimeConverter { DateTimeFormat = format };
            return JsonConvert.DeserializeObject<T>(text, dateTimeConverter);
        }
    }
}
