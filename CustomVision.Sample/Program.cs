using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Cognitive.CustomVision;

namespace CustomVision.Sample
{
    class Program
    {
        private static List<MemoryStream> images;

        static void Main(string[] args)
        {
            images = Directory.GetFiles(@"\Imagenes").Select(f => new MemoryStream(File.ReadAllBytes(f))).ToList();
            int listSize = images.Count;

            Console.WriteLine("\n\nComenzando evaluación de imágenes...");

            int acum = 1;
            images.ForEach(async image =>
            {
                Console.WriteLine("\nResultado de la foto n." + acum + "\n");
                MakePredictionRequest(image).Wait();
                Console.WriteLine("\n-------------------------------------");
                acum++;
                Thread.Sleep(1000);
            });

            Console.WriteLine("\n\nEvaulación finalizada.");
            Console.ReadLine();
        }

        public static async Task MakePredictionRequest(MemoryStream image)
        {
            HttpClient client = new HttpClient();

            // Request headers - replace this example key with your valid Prediction-Key.
            client.DefaultRequestHeaders.Add("Prediction-Key", "4fd45c69a1ea4614af0d965f765462ad");

            // Prediction URL - replace this example URL with your valid Prediction URL.
            string url = "https://ines.cognitiveservices.azure.com/customvision/v3.0/Prediction/4ac479aa-b643-4f90-83bd-633fcb8bc431/classify/iterations/Identificar%20INES/image";

            HttpResponseMessage response;

            // Request body. Try this sample with a locally stored image.
            byte[] byteData = GetImageAsByteArray(image);

            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                response = await client.PostAsync(url, content);
                ShowString(await response.Content.ReadAsStringAsync());
            }

        }

        private static void ShowString(string str)
        {
            int i = str.IndexOf("\"predictions\":[{");
            if (i >= 0) str = str.Substring(i+16);

            string[] words = str.Split('{');
            foreach (string tmp in words)
            {
                string[] expl = tmp.Split(',');
                string res = expl[0];
                int k =res.IndexOf("\"probability\":");
                if (k >= 0) res = res.Substring(k + 14);
                double prob = 0.0;
                if (!res.Contains('E'))
                {
                    prob = Convert.ToDouble(res);
                    prob = prob * 100;
                }

                string tag = expl[2];
                int z = tag.IndexOf("\"tagName\":");
                if (z >= 0) tag = tag.Substring(z + 11);
                
                tag = new string((from c in tag
                                    where char.IsWhiteSpace(c) || char.IsLetterOrDigit(c)
                                    select c
                    ).ToArray());
                    
                Console.WriteLine(tag+": "+prob+"%");
            }

        }

        private static byte[] GetImageAsByteArray(MemoryStream fileStream)
        {
            BinaryReader binaryReader = new BinaryReader(fileStream);
            return binaryReader.ReadBytes((int)fileStream.Length);
        }

    }
}
