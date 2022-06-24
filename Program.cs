﻿using Microsoft.Extensions.Configuration;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO.Compression;
using System.Security.Cryptography;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

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

            int totalUncompressedBytes = 0;
            int totalCompressedBytes = 0;


            //System.IO.StreamWriter outputFileStreamWriter = new StreamWriter(configuration["AppSettings:OutputFileName"]);

            int rowsPerEncodededBand;
            int currentRowForEncodedBand;
            int currentArrayIndex;
            string inputFileName;
            byte[] rgbValuesForRow = null;

            // TEMP CODE
            inputFileName = "/Users/robinbayer/Documents/junk/radar.tif";
            rowsPerEncodededBand = 3;

            // END TEMP CODE


            using Image<Rgba32> image = Image.Load<Rgba32>(inputFileName);
            image.ProcessPixelRows(accessor =>
            {
                currentRowForEncodedBand = 1;
                for (int y = 0; y < accessor.Height; y++)
                {
                    Span<Rgba32> pixelRow = accessor.GetRowSpan(y);
                    rgbValuesForRow = new byte[pixelRow.Length * 3 * rowsPerEncodededBand];      // storing RGB byte value for each pixel
                    totalUncompressedBytes += rgbValuesForRow!.Length * 3;

                    currentArrayIndex = 0;
                    currentRowForEncodedBand = 1;

                    while (currentRowForEncodedBand <= rowsPerEncodededBand)
                    {

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

                        currentRowForEncodedBand++;
                    }       // while (currentRowForEncodedBand <= rowsPerEncodededBand)

                    byte[] compressedBase64ByteArray = CompressByteArray(rgbValuesForRow!);

                    // TEMP CODE
                    //Console.WriteLine(System.Text.Encoding.Default.GetString(compressedBase64ByteArray));

                    //Console.WriteLine("Compressed from {0} bytes to {1} bytes", rgbValuesForRow.Length, compressedBase64ByteArray.Length);

                    totalCompressedBytes += compressedBase64ByteArray.Length;

                    // END TEMP CODE


                }       // for (int y = 0; y < accessor.Height; y++)

            });

            Console.WriteLine("Compressed from {0} bytes to {1} bytes - {2}% compression", totalUncompressedBytes, totalCompressedBytes,
                              (1 - (float)totalCompressedBytes / (float)totalUncompressedBytes) * 100);
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