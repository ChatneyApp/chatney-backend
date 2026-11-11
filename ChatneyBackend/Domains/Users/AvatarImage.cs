using System.Buffers.Binary;

namespace ChatneyBackend.Domains.Users;

public record AvatarImage(string Extension, string ContentType, int Width, int Height)
{
    public static readonly string[] Extensions = ["gif", "jpg", "png"];

    private static readonly byte[] PngSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    // Format and dimensions come from the file header, so a renamed file or a spoofed content type can't pass.
    public static AvatarImage? Parse(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length >= 24 && bytes.StartsWith(PngSignature))
        {
            return new AvatarImage(
                "png",
                "image/png",
                BinaryPrimitives.ReadInt32BigEndian(bytes[16..]),
                BinaryPrimitives.ReadInt32BigEndian(bytes[20..]));
        }

        if (bytes.Length >= 10 && (bytes.StartsWith("GIF87a"u8) || bytes.StartsWith("GIF89a"u8)))
        {
            return new AvatarImage(
                "gif",
                "image/gif",
                BinaryPrimitives.ReadUInt16LittleEndian(bytes[6..]),
                BinaryPrimitives.ReadUInt16LittleEndian(bytes[8..]));
        }

        if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xD8)
        {
            return ParseJpeg(bytes);
        }

        return null;
    }

    private static AvatarImage? ParseJpeg(ReadOnlySpan<byte> bytes)
    {
        var position = 2;

        // Walk the segments up to the frame header (SOF), which holds the dimensions.
        while (position + 9 <= bytes.Length && bytes[position] == 0xFF)
        {
            var marker = bytes[position + 1];

            if (marker == 0xFF)
            {
                position++;
                continue;
            }

            if (marker is >= 0xC0 and <= 0xCF and not (0xC4 or 0xC8 or 0xCC))
            {
                return new AvatarImage(
                    "jpg",
                    "image/jpeg",
                    BinaryPrimitives.ReadUInt16BigEndian(bytes[(position + 7)..]),
                    BinaryPrimitives.ReadUInt16BigEndian(bytes[(position + 5)..]));
            }

            position += 2 + BinaryPrimitives.ReadUInt16BigEndian(bytes[(position + 2)..]);
        }

        return null;
    }
}
