using System.Buffers.Binary;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using Amazon.S3;
using Amazon.S3.Model;

namespace ChatneyBackend.Domains.InstallWizard;

public static class SeedAvatars
{
    private const int Side = Users.DomainSettings.AvatarSideSize;
    private const int Cells = 5;
    private const int CellSize = 24;
    private const int Margin = (Side - Cells * CellSize) / 2;
    private const int RowSize = 1 + Side * 3;
    private const byte Background = 0xF0;

    /// <summary>Generates the user's identicon, uploads it and points the user's avatar url at it.</summary>
    public static async Task Upload(IAmazonS3 s3Client, string bucket, Users.User user)
    {
        var s3Key = $"{Users.DomainSettings.AvatarS3Folder}/{user.Id}.png";
        using var content = new MemoryStream(Generate(user.Nickname));

        await s3Client.PutObjectAsync(new PutObjectRequest
        {
            BucketName = bucket,
            Key = s3Key,
            InputStream = content,
            ContentType = "image/png",
        });

        user.AvatarUrl = $"/{s3Key}";
    }

    /// <summary>Draws an identicon PNG: a mirrored 5x5 pattern whose shape and colour come from the nickname.</summary>
    public static byte[] Generate(string nickname)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(nickname));
        var color = HueToRgb(hash[0] * 12 / 256.0);
        var pattern = BinaryPrimitives.ReadUInt16BigEndian(hash.AsSpan(1));

        // Scanlines of RGB pixels, each prefixed with a filter byte (0 = none).
        var pixels = new byte[Side * RowSize];
        Array.Fill(pixels, Background);

        for (var y = 0; y < Side; y++)
        {
            pixels[y * RowSize] = 0;
        }

        for (var row = 0; row < Cells; row++)
        {
            // Only the left half and the middle column are decided, the right half mirrors them.
            for (var column = 0; column <= Cells / 2; column++)
            {
                if ((pattern >> (row * 3 + column) & 1) == 1)
                {
                    FillCell(pixels, row, column, color);
                    FillCell(pixels, row, Cells - 1 - column, color);
                }
            }
        }

        using var compressed = new MemoryStream();

        using (var zlib = new ZLibStream(compressed, CompressionLevel.Optimal))
        {
            zlib.Write(pixels);
        }

        var header = new byte[13];
        BinaryPrimitives.WriteInt32BigEndian(header, Side);
        BinaryPrimitives.WriteInt32BigEndian(header.AsSpan(4), Side);
        header[8] = 8; // bits per channel
        header[9] = 2; // colour type: RGB

        using var png = new MemoryStream();
        png.Write([0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]);
        WriteChunk(png, "IHDR", header);
        WriteChunk(png, "IDAT", compressed.ToArray());
        WriteChunk(png, "IEND", []);

        return png.ToArray();
    }

    private static void FillCell(byte[] pixels, int row, int column, byte[] color)
    {
        for (var y = Margin + row * CellSize; y < Margin + (row + 1) * CellSize; y++)
        {
            for (var x = Margin + column * CellSize; x < Margin + (column + 1) * CellSize; x++)
            {
                color.CopyTo(pixels, y * RowSize + 1 + x * 3);
            }
        }
    }

    /// <param name="hue">Position on the colour wheel, 0..12.</param>
    private static byte[] HueToRgb(double hue)
    {
        // HSL to RGB with a fixed saturation and lightness, so every hue gives a calm mid-tone.
        const double lightness = 0.55;
        const double amplitude = 0.25;

        byte Channel(int offset)
        {
            var k = (offset + hue) % 12;

            return (byte)(255 * (lightness - amplitude * Math.Max(-1, Math.Min(Math.Min(k - 3, 9 - k), 1))));
        }

        return [Channel(0), Channel(8), Channel(4)];
    }

    private static void WriteChunk(Stream png, string type, byte[] data)
    {
        byte[] body = [..Encoding.ASCII.GetBytes(type), ..data];
        Span<byte> number = stackalloc byte[4];

        BinaryPrimitives.WriteInt32BigEndian(number, data.Length);
        png.Write(number);
        png.Write(body);
        BinaryPrimitives.WriteUInt32BigEndian(number, Crc32(body));
        png.Write(number);
    }

    private static uint Crc32(byte[] bytes)
    {
        var crc = uint.MaxValue;

        foreach (var value in bytes)
        {
            crc ^= value;

            for (var bit = 0; bit < 8; bit++)
            {
                crc = (crc & 1) == 1 ? (crc >> 1) ^ 0xEDB88320 : crc >> 1;
            }
        }

        return ~crc;
    }
}
