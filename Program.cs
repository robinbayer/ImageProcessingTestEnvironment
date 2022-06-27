using Microsoft.Extensions.Configuration;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO.Compression;
using System.Security.Cryptography;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Newtonsoft.Json;

namespace ImageProcessingTestEnvironment
{
    internal class Program
    {

        static async Task Main(string[] args)
        {

            var builder = new ConfigurationBuilder()
                   .SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            IConfigurationRoot configuration = builder.Build();


            Program program = new Program();

            await program.Execute(configuration);

            Console.WriteLine("Press any key to continue....");
            Console.ReadKey();

        }

        private async Task Execute(IConfiguration configuration)
        {


            byte storeByte1 = 0;
            byte storeByte2 = 0;



            //System.IO.StreamWriter outputFileStreamWriter = new StreamWriter(configuration["AppSettings:OutputFileName"]);

            int currentArrayIndex;
            string inputFileName;
            byte[] rgbValuesForRow;
            TequaCreek.WGODataModelLibrary.Models.EncodedImage encodedImage;
            Chilkat.Compression compress = new Chilkat.Compression();


            


            // TEMP CODE
            inputFileName = "/Users/robinbayer/Documents/junk/example.tif";

            // END TEMP CODE

            compress.Algorithm = "deflate";
            compress.EncodingMode = "base64";
            compress.DeflateLevel = 9;

            encodedImage = new TequaCreek.WGODataModelLibrary.Models.EncodedImage();


            using Image<Rgba32> image = Image.Load<Rgba32>(inputFileName);


            image.ProcessPixelRows(accessor =>
            {

                encodedImage.imageHeightPixels = accessor.Height;
                encodedImage.imageWidthPixels = accessor.Width;

                for (int y = 0; y < accessor.Height; y++)
                {
                    currentArrayIndex = 0;

                    Span<Rgba32> pixelRow = accessor.GetRowSpan(y);
                    rgbValuesForRow = new byte[pixelRow.Length * 3];      // storing RGB byte value for each pixel


                    // TEMP CODE
                    if (y == 2000)
                    {
                        storeByte1 = pixelRow[2007].R;
                        storeByte2 = pixelRow[2331].G;

                        // TEMP CODE
                        Console.WriteLine("Stored bytes {0} and {1} for later comparion", storeByte1, storeByte2);
                        // END TEMP CODE


                    }
                    // END TEMP CODE


                    // pixelRow.Length has the same value as accessor.Width,
                    // but using pixelRow.Length allows the JIT to optimize away bounds checks:
                    for (int x = 0; x < pixelRow.Length; x++)
                    {
                        // Get a reference to the pixel at position x
                        ref Rgba32 pixel = ref pixelRow[x];

                        rgbValuesForRow[currentArrayIndex++] = pixel.R;
                        rgbValuesForRow[currentArrayIndex++] = pixel.G;
                        rgbValuesForRow[currentArrayIndex++] = pixel.B;
                    }

                    /*
                    byte[] compressedBytes;
                    compressedBytes = compress.CompressBytes(rgbValuesForRow);


                    if (compress.LastMethodSuccess != true)
                    {
                        Console.WriteLine(compress.LastErrorText);
                        return;
                    }
                    */



                    //string S = Encoding.UTF8.GetString(B)

                    encodedImage.encodedBands.Add(compress.CompressBytesENC(rgbValuesForRow));


                }       // for (int y = 0; y < accessor.Height; y++)

            });


            // TEMP CODE

            // serialize JSON directly to a file
            Console.WriteLine("WRiting output file");

            using (StreamWriter file = File.CreateText("/Users/robinbayer/Documents/junk/encodedImage.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, encodedImage);
            }

            Console.WriteLine("Press any key...");
            Console.ReadKey();
            // END TEMP CODE


            Console.WriteLine("Reversing for comparison");

            TequaCreek.WGODataModelLibrary.Models.EncodedImage encodedImage2;

            // Reverese process to test
            // deserialize JSON directly from a file
            using (StreamReader file = File.OpenText("/Users/robinbayer/Documents/junk/encodedImage.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                encodedImage2 = (TequaCreek.WGODataModelLibrary.Models.EncodedImage)serializer.Deserialize(file, typeof(TequaCreek.WGODataModelLibrary.Models.EncodedImage));
            }

            int counter = 0;
            foreach (string encodedBand in encodedImage2.encodedBands)
            {

                byte[] decoded = compress.DecompressBytesENC(encodedBand);

                if (counter == 2000)
                {

                    Console.WriteLine("Original/New Byte {0}/{1}", storeByte1, decoded[2007 * 3]);
                    Console.WriteLine("Original/New Byte {0}/{1}", storeByte2, decoded[2331 * 3 + 1]);

                }

                counter++;
            }

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
            


        }       // Execute()

        //SaveAsync(Stream, IImageEncoder, CancellationToken)

        private byte[] CompressByteArray(byte[] inputByteArray)
        {

            using (MemoryStream resultStream = new MemoryStream())
            {
                using (DeflateStream compressionStream = new DeflateStream(resultStream, CompressionLevel.SmallestSize))
                {
                    compressionStream.Write(inputByteArray, 0, inputByteArray.Length);
                }
                return resultStream.ToArray();
            }

        }       // CompressByteArray()

    }
}