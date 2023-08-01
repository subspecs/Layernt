using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Formats.Tiff;
using SixLabors.ImageSharp.Formats.Tga;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Formats.Bmp;

namespace Layernt
{
    public static class Layernt
    {
        public static class Bits
        {
            public static int GetBit(byte Byte, int BitIndex)
            {
                return (Byte & (1u << BitIndex)) != 0 ? 1 : 0;
            }
            public static int GetBit(int Int, int BitIndex)
            {
                return (Int & (1 << BitIndex)) != 0 ? 1 : 0;
            }
            public static int GetBit(ushort UShort, int BitIndex)
            {
                return (UShort & (1u << BitIndex)) != 0 ? 1 : 0;
            }
            public static int GetBit(short UShort, int BitIndex)
            {
                return (UShort & (1u << BitIndex)) != 0 ? 1 : 0;
            }

            public static void SetBit(ref byte Byte, int BitIndex, int Value)
            {
                Byte = Value != 0 ? (byte)(Byte | (1u << BitIndex)) : (byte)(Byte & ~(1u << BitIndex));
            }
            public static void SetBit(ref int Int, int BitIndex, int Value)
            {
                Int = Value != 0 ? (int)(Int | (1 << BitIndex)) : (int)(Int & ~(1 << BitIndex));
            }

            public static void SetBit(ref ushort UShort, int BitIndex, int Value)
            {
                UShort = Value != 0 ? (ushort)(UShort | (1u << BitIndex)) : (ushort)(UShort & ~(1u << BitIndex));
            }
            public static void SetBit(ref short UShort, int BitIndex, int Value)
            {
                UShort = Value != 0 ? (short)((byte)UShort | (1 << BitIndex)) : (short)(UShort & ~(1 << BitIndex));
            }
        }

        public struct Pixel24
        {
            public byte B, G, R;
        }
        public struct Pixel32
        {
            public byte B, G, R, A;
        }
        public struct Pixel48
        {
            public ushort B, G, R;
        }
        public struct Pixel64
        {
            public ushort B, G, R, A;
        }


        public static int GetTotalPossibleBufferSize3CH(int SizeX, int SizeY, int SaveBits)
        {
            return (int)((((SizeX * (long)SizeY) - 1) * 3 * SaveBits) / 8); //-1 For the save bit header.
        }
        public static int GetTotalPossibleBufferSize4CH(int SizeX, int SizeY, int SaveBits)
        {
            return (int)((((SizeX * (long)SizeY) - 1) * 4 * SaveBits) / 8); //-1 For the save bit header.
        }

        private static SixLabors.ImageSharp.Image<Rgb24> ConvertToImage24_3CH(string FileName)
        {
            return Image.Load<Rgb24>(FileName);
        }
        public static void EncryptImage24(string InputImage, string OutputImage, byte[] Buffer, int Offset, int Count, int SaveBits, byte[] EncryptionPassword, string DataFileName = null, bool UsePlatformSafeEncryption = false)
        {
            int sBpp = 3 * SaveBits; if (SaveBits > 8 || SaveBits < 1) { throw new System.Exception("Save bits cannot be below 1 or above 8."); }
            if (InputImage == null || OutputImage == null || Buffer == null || EncryptionPassword == null) { throw new System.Exception("InputImage, OutputImage, Buffer and EncryptionPassword cannot be null."); }
            if (!System.IO.File.Exists(InputImage)) { throw new System.Exception("InputImage file doesn't exist."); }

            unsafe
            {
                SixLabors.ImageSharp.Image<Rgb24> Image = ConvertToImage24_3CH(InputImage);
                Memory<Rgb24>[] MemoryBang = Image.GetPixelMemoryGroup().ToArray(); var BufferHandles = new System.Buffers.MemoryHandle[MemoryBang.Length]; Pixel24** RawPixelArray = (Pixel24**)System.Runtime.InteropServices.Marshal.AllocHGlobal(sizeof(void*) * MemoryBang.Length);
                int e = 0, BytesPerBang = MemoryBang[0].Length; while (e < MemoryBang.Length) { BufferHandles[e] = MemoryBang[e].Pin(); RawPixelArray[e] = (Pixel24*)BufferHandles[e].Pointer; e++; }
                static Pixel24* GetPixel(Pixel24** PXArray, int BPB, int Offset) { return &PXArray[Offset / BPB][Offset - ((Offset / BPB) * BPB)]; }

                Bits.SetBit(ref (*GetPixel(RawPixelArray, BytesPerBang, 0)).B, 0, Bits.GetBit(SaveBits - 1, 0)); Bits.SetBit(ref (*GetPixel(RawPixelArray, BytesPerBang, 0)).G, 0, Bits.GetBit(SaveBits - 1, 1));
                Bits.SetBit(ref (*GetPixel(RawPixelArray, BytesPerBang, 0)).R, 0, Bits.GetBit(SaveBits - 1, 2)); Bits.SetBit(ref (*GetPixel(RawPixelArray, BytesPerBang, 0)).R, 1, Bits.GetBit(SaveBits - 1, 3));

                byte[] DataName = DataFileName != null && DataFileName != "" ? System.Text.Encoding.Unicode.GetBytes(System.IO.Path.GetFileName(DataFileName)) : new byte[0];
                byte[] DataToEncrypt = new byte[EncryptionPassword != null ? (4 + DataName.Length + 4 + Count) : 0];

                System.Buffer.BlockCopy(System.BitConverter.GetBytes(DataName.Length), 0, DataToEncrypt, 0, 4); //DataName.Length.
                System.Buffer.BlockCopy(DataName, 0, DataToEncrypt, 4, DataName.Length); //DataName.
                System.Buffer.BlockCopy(System.BitConverter.GetBytes(Count), 0, DataToEncrypt, 4 + DataName.Length, 4); //Count.
                System.Buffer.BlockCopy(Buffer, Offset, DataToEncrypt, 4 + DataName.Length + 4, Count); //Actual Data.

                System.Array.Resize(ref EncryptionPassword, 32); //Make sure password fits.
                System.Security.Cryptography.AesGcm AES = new System.Security.Cryptography.AesGcm(EncryptionPassword);
                byte[] Nonce = System.Security.Cryptography.RandomNumberGenerator.GetBytes(System.Security.Cryptography.AesGcm.NonceByteSizes.MaxSize); //RNG.
                byte[] Tag = new byte[UsePlatformSafeEncryption ? 12 : System.Security.Cryptography.AesGcm.TagByteSizes.MaxSize]; //Gets and Stores Tag.
                byte[] WriteBuffer = new byte[4 + Nonce.Length + 4 + Tag.Length]; //Buffer to store all.

                int BufferByteSize = GetTotalPossibleBufferSize3CH(Image.Width, Image.Height, SaveBits);
                if (WriteBuffer.Length + DataToEncrypt.Length > BufferByteSize) { throw new System.Exception("Not enough space in image to save file with the given noise bits. (Available: " + (BufferByteSize / 1024f).ToString("0.00") + "kb < Needed: " + (WriteBuffer.Length / 1024f).ToString("0.00") + "kb)"); }
                System.Array.Resize(ref DataToEncrypt, BufferByteSize - WriteBuffer.Length); //So we have full frame buffer.
                System.Array.Resize(ref WriteBuffer, BufferByteSize); //So we have full frame buffer.

                byte[] EncryptedData = new byte[DataToEncrypt.Length]; int EncBitOffset = 0; // 1:1.
                System.Buffer.BlockCopy(System.BitConverter.GetBytes(Nonce.Length), 0, WriteBuffer, EncBitOffset, 4); EncBitOffset += 4; //Write Nonce len.
                System.Buffer.BlockCopy(Nonce, 0, WriteBuffer, EncBitOffset, Nonce.Length); EncBitOffset += Nonce.Length; //Write Nonce.
                System.Buffer.BlockCopy(System.BitConverter.GetBytes(Tag.Length), 0, WriteBuffer, EncBitOffset, 4); EncBitOffset += 4; //Write Tag Len.
                AES.Encrypt(Nonce, DataToEncrypt, EncryptedData, Tag); //Encrypt Data.
                System.Buffer.BlockCopy(Tag, 0, WriteBuffer, EncBitOffset, Tag.Length); EncBitOffset += Tag.Length;
                System.Buffer.BlockCopy(EncryptedData, 0, WriteBuffer, EncBitOffset, EncryptedData.Length); EncBitOffset += EncryptedData.Length;

                int TotalPixelCount = Image.Width * Image.Height, z, n = 1; //1 for the pixel we used for bit size/count.
                long BitOffset = 0, TotalBitsToWrite = BufferByteSize * 8;
                while (n < TotalPixelCount)
                {
                    z = 0; while (z < SaveBits) { if (BitOffset == TotalBitsToWrite) { break; }; Bits.SetBit(ref (*GetPixel(RawPixelArray, BytesPerBang, n)).B, z, Bits.GetBit(WriteBuffer[BitOffset / 8], (int)(BitOffset % 8))); BitOffset++; z++; }
                    z = 0; while (z < SaveBits) { if (BitOffset == TotalBitsToWrite) { break; }; Bits.SetBit(ref (*GetPixel(RawPixelArray, BytesPerBang, n)).G, z, Bits.GetBit(WriteBuffer[BitOffset / 8], (int)(BitOffset % 8))); BitOffset++; z++; }
                    z = 0; while (z < SaveBits) { if (BitOffset == TotalBitsToWrite) { break; }; Bits.SetBit(ref (*GetPixel(RawPixelArray, BytesPerBang, n)).R, z, Bits.GetBit(WriteBuffer[BitOffset / 8], (int)(BitOffset % 8))); BitOffset++; z++; }
                    if (BitOffset == TotalBitsToWrite) { break; } //Speed.
                    n++;
                }

                switch (System.IO.Path.GetExtension(OutputImage).ToLower())
                {
                    case ".png": { Image.Save(OutputImage, new PngEncoder() { CompressionLevel = PngCompressionLevel.BestCompression, BitDepth = PngBitDepth.Bit8, TransparentColorMode = PngTransparentColorMode.Clear, ColorType = PngColorType.Rgb }); } break;
                    case ".tiff": { Image.Save(OutputImage, new TiffEncoder() { CompressionLevel = SixLabors.ImageSharp.Compression.Zlib.DeflateCompressionLevel.BestCompression, BitsPerPixel = TiffBitsPerPixel.Bit24 }); } break;
                    case ".tga": { Image.Save(OutputImage, new TgaEncoder() { Compression = TgaCompression.RunLength, BitsPerPixel = TgaBitsPerPixel.Pixel32 }); } break; //Only <= 32bit images.
                    case ".webp": { Image.Save(OutputImage, new WebpEncoder() { FileFormat = WebpFileFormatType.Lossless, Method = WebpEncodingMethod.BestQuality, Quality = 100, TransparentColorMode = WebpTransparentColorMode.Clear, UseAlphaCompression = false }); } break;  //Only <= 32bit images.
                    case ".bmp": { Image.Save(OutputImage, new BmpEncoder() { BitsPerPixel = BmpBitsPerPixel.Pixel32 }); } break; //Only <= 32bit images.
                    default: { throw new System.Exception("Unknown image format."); }
                }

                AES.Dispose();
                n = 0; while (n < BufferHandles.Length) { BufferHandles[n].Dispose(); n++; }; System.Runtime.InteropServices.Marshal.FreeHGlobal((nint)RawPixelArray);
                Image.Dispose();
            }

        }
        public static void DecryptImage24(string InputImage, out byte[] Output, byte[] EncryptionPassword, out string DataFileName)
        {
            if (InputImage == null || EncryptionPassword == null) { throw new System.Exception("InputImage and EncryptionPassword cannot be null."); }
            if (!System.IO.File.Exists(InputImage)) { throw new System.Exception("InputImage file doesn't exist."); }

            unsafe
            {
                SixLabors.ImageSharp.Image<Rgb24> Image = ConvertToImage24_3CH(InputImage);
                Memory<Rgb24>[] MemoryBang = Image.GetPixelMemoryGroup().ToArray(); var BufferHandles = new System.Buffers.MemoryHandle[MemoryBang.Length]; Pixel24** RawPixelArray = (Pixel24**)System.Runtime.InteropServices.Marshal.AllocHGlobal(sizeof(void*) * MemoryBang.Length);
                int e = 0, BytesPerBang = MemoryBang[0].Length; while (e < MemoryBang.Length) { BufferHandles[e] = MemoryBang[e].Pin(); RawPixelArray[e] = (Pixel24*)BufferHandles[e].Pointer; e++; }
                static Pixel24* GetPixel(Pixel24** PXArray, int BPB, int Offset) { return &PXArray[Offset / BPB][Offset - ((Offset / BPB) * BPB)]; }

                int SaveBits = 0, sBpp;
                Bits.SetBit(ref SaveBits, 0, Bits.GetBit((*GetPixel(RawPixelArray, BytesPerBang, 0)).B, 0)); Bits.SetBit(ref SaveBits, 1, Bits.GetBit((*GetPixel(RawPixelArray, BytesPerBang, 0)).G, 0));
                Bits.SetBit(ref SaveBits, 2, Bits.GetBit((*GetPixel(RawPixelArray, BytesPerBang, 0)).R, 0)); Bits.SetBit(ref SaveBits, 3, Bits.GetBit((*GetPixel(RawPixelArray, BytesPerBang, 0)).R, 1));
                SaveBits++; int BufferByteSize = GetTotalPossibleBufferSize3CH(Image.Width, Image.Height, SaveBits); sBpp = 3 * SaveBits;

                static void ReadInPixels(int TotalPixelCount, byte[] Buffer, Pixel24** PXArray, int BPB, int SaveBits)
                {
                    int n = 1, z; long BitOffset = 0, BufferTotalBits = Buffer.Length * 8; while (n < TotalPixelCount && BitOffset != BufferTotalBits) //+1 for the first save bits pixel.
                    {
                        z = 0; while (z < SaveBits && BitOffset != BufferTotalBits) { Bits.SetBit(ref Buffer[BitOffset / 8], (int)(BitOffset % 8), Bits.GetBit((*GetPixel(PXArray, BPB, n)).B, z)); z++; BitOffset++; }
                        z = 0; while (z < SaveBits && BitOffset != BufferTotalBits) { Bits.SetBit(ref Buffer[BitOffset / 8], (int)(BitOffset % 8), Bits.GetBit((*GetPixel(PXArray, BPB, n)).G, z)); z++; BitOffset++; }
                        z = 0; while (z < SaveBits && BitOffset != BufferTotalBits) { Bits.SetBit(ref Buffer[BitOffset / 8], (int)(BitOffset % 8), Bits.GetBit((*GetPixel(PXArray, BPB, n)).R, z)); z++; BitOffset++; }
                        n++;
                    }
                }

                byte[] EncryptedBuffer = new byte[BufferByteSize];
                ReadInPixels(Image.Width * Image.Height, EncryptedBuffer, RawPixelArray, BytesPerBang, SaveBits);

                int ByteOffset = 0;
                int NonceSize = System.BitConverter.ToInt32(EncryptedBuffer, ByteOffset); byte[] Nonce = new byte[NonceSize]; ByteOffset += 4;
                System.Buffer.BlockCopy(EncryptedBuffer, ByteOffset, Nonce, 0, NonceSize); ByteOffset += NonceSize;

                int TagSize = System.BitConverter.ToInt32(EncryptedBuffer, 4 + NonceSize); byte[] Tag = new byte[TagSize]; ByteOffset += 4;
                System.Buffer.BlockCopy(EncryptedBuffer, 4 + NonceSize + 4, Tag, 0, TagSize); ByteOffset += TagSize;

                byte[] EncryptedData = new byte[EncryptedBuffer.Length - ByteOffset];
                System.Buffer.BlockCopy(EncryptedBuffer, ByteOffset, EncryptedData, 0, EncryptedData.Length);

                System.Array.Resize(ref EncryptionPassword, 32); Output = new byte[EncryptedData.Length];
                System.Security.Cryptography.AesGcm AES = new System.Security.Cryptography.AesGcm(EncryptionPassword);

                AES.Decrypt(Nonce, EncryptedData, Tag, Output);

                int DataNameLength = System.BitConverter.ToInt32(Output, 0); DataFileName = DataNameLength > 0 ? System.Text.Encoding.Unicode.GetString(Output, 4, DataNameLength) : null;
                int DataLength = System.BitConverter.ToInt32(Output, 4 + DataNameLength); System.Buffer.BlockCopy(Output, 4 + DataNameLength + 4, Output, 0, DataLength); System.Array.Resize(ref Output, DataLength);

                AES.Dispose();
                int n = 0; while (n < BufferHandles.Length) { BufferHandles[n].Dispose(); n++; }; System.Runtime.InteropServices.Marshal.FreeHGlobal((nint)RawPixelArray);
                Image.Dispose();
            }
        }

        private static SixLabors.ImageSharp.Image<Rgba32> ConvertToImage32_4CH(string FileName)
        {
            return Image.Load<Rgba32>(FileName);
        }
        public static void EncryptImage32(string InputImage, string OutputImage, byte[] Buffer, int Offset, int Count, int SaveBits, byte[] EncryptionPassword, string DataFileName = null, bool UsePlatformSafeEncryption = false)
        {
            int sBpp = 4 * SaveBits; if (SaveBits > 8 || SaveBits < 1) { throw new System.Exception("Save bits cannot be below 1 or above 8."); }
            if (InputImage == null || OutputImage == null || Buffer == null || EncryptionPassword == null) { throw new System.Exception("InputImage, OutputImage, Buffer and EncryptionPassword cannot be null."); }
            if (!System.IO.File.Exists(InputImage)) { throw new System.Exception("InputImage file doesn't exist."); }

            unsafe
            {
                SixLabors.ImageSharp.Image<Rgba32> Image = ConvertToImage32_4CH(InputImage);
                Memory<Rgba32>[] MemoryBang = Image.GetPixelMemoryGroup().ToArray(); var BufferHandles = new System.Buffers.MemoryHandle[MemoryBang.Length]; Pixel32** RawPixelArray = (Pixel32**)System.Runtime.InteropServices.Marshal.AllocHGlobal(sizeof(void*) * MemoryBang.Length);
                int e = 0, BytesPerBang = MemoryBang[0].Length; while (e < MemoryBang.Length) { BufferHandles[e] = MemoryBang[e].Pin(); RawPixelArray[e] = (Pixel32*)BufferHandles[e].Pointer; e++; }
                static Pixel32* GetPixel(Pixel32** PXArray, int BPB, int Offset) { return &PXArray[Offset / BPB][Offset - ((Offset / BPB) * BPB)]; }

                Bits.SetBit(ref (*GetPixel(RawPixelArray, BytesPerBang, 0)).B, 0, Bits.GetBit(SaveBits - 1, 0)); Bits.SetBit(ref (*GetPixel(RawPixelArray, BytesPerBang, 0)).G, 0, Bits.GetBit(SaveBits - 1, 1));
                Bits.SetBit(ref (*GetPixel(RawPixelArray, BytesPerBang, 0)).R, 0, Bits.GetBit(SaveBits - 1, 2)); Bits.SetBit(ref (*GetPixel(RawPixelArray, BytesPerBang, 0)).R, 1, Bits.GetBit(SaveBits - 1, 3));

                byte[] DataName = DataFileName != null && DataFileName != "" ? System.Text.Encoding.Unicode.GetBytes(System.IO.Path.GetFileName(DataFileName)) : new byte[0];
                byte[] DataToEncrypt = new byte[EncryptionPassword != null ? (4 + DataName.Length + 4 + Count) : 0];

                System.Buffer.BlockCopy(System.BitConverter.GetBytes(DataName.Length), 0, DataToEncrypt, 0, 4); //DataName.Length.
                System.Buffer.BlockCopy(DataName, 0, DataToEncrypt, 4, DataName.Length); //DataName.
                System.Buffer.BlockCopy(System.BitConverter.GetBytes(Count), 0, DataToEncrypt, 4 + DataName.Length, 4); //Count.
                System.Buffer.BlockCopy(Buffer, Offset, DataToEncrypt, 4 + DataName.Length + 4, Count); //Actual Data.

                System.Array.Resize(ref EncryptionPassword, 32); //Make sure password fits.
                System.Security.Cryptography.AesGcm AES = new System.Security.Cryptography.AesGcm(EncryptionPassword);
                byte[] Nonce = System.Security.Cryptography.RandomNumberGenerator.GetBytes(System.Security.Cryptography.AesGcm.NonceByteSizes.MaxSize); //RNG.
                byte[] Tag = new byte[UsePlatformSafeEncryption ? 12 : System.Security.Cryptography.AesGcm.TagByteSizes.MaxSize]; //Gets and Stores Tag.
                byte[] WriteBuffer = new byte[4 + Nonce.Length + 4 + Tag.Length]; //Buffer to store all.

                int BufferByteSize = GetTotalPossibleBufferSize4CH(Image.Width, Image.Height, SaveBits);
                if (WriteBuffer.Length + DataToEncrypt.Length > BufferByteSize) { throw new System.Exception("Not enough space in image to save file with the given noise bits. (Available: " + (BufferByteSize / 1024f).ToString("0.00") + "kb < Needed: " + (WriteBuffer.Length / 1024f).ToString("0.00") + "kb)"); }
                System.Array.Resize(ref DataToEncrypt, BufferByteSize - WriteBuffer.Length); //So we have full frame buffer.
                System.Array.Resize(ref WriteBuffer, BufferByteSize); //So we have full frame buffer.

                byte[] EncryptedData = new byte[DataToEncrypt.Length]; int EncBitOffset = 0; // 1:1.
                System.Buffer.BlockCopy(System.BitConverter.GetBytes(Nonce.Length), 0, WriteBuffer, EncBitOffset, 4); EncBitOffset += 4; //Write Nonce len.
                System.Buffer.BlockCopy(Nonce, 0, WriteBuffer, EncBitOffset, Nonce.Length); EncBitOffset += Nonce.Length; //Write Nonce.
                System.Buffer.BlockCopy(System.BitConverter.GetBytes(Tag.Length), 0, WriteBuffer, EncBitOffset, 4); EncBitOffset += 4; //Write Tag Len.
                AES.Encrypt(Nonce, DataToEncrypt, EncryptedData, Tag); //Encrypt Data.
                System.Buffer.BlockCopy(Tag, 0, WriteBuffer, EncBitOffset, Tag.Length); EncBitOffset += Tag.Length;
                System.Buffer.BlockCopy(EncryptedData, 0, WriteBuffer, EncBitOffset, EncryptedData.Length); EncBitOffset += EncryptedData.Length;

                int TotalPixelCount = Image.Width * Image.Height, z, n = 1; //1 for the pixel we used for bit size/count.
                long BitOffset = 0, TotalBitsToWrite = BufferByteSize * 8;
                while (n < TotalPixelCount)
                {
                    z = 0; while (z < SaveBits) { if (BitOffset == TotalBitsToWrite) { break; }; Bits.SetBit(ref (*GetPixel(RawPixelArray, BytesPerBang, n)).B, z, Bits.GetBit(WriteBuffer[BitOffset / 8], (int)(BitOffset % 8))); BitOffset++; z++; }
                    z = 0; while (z < SaveBits) { if (BitOffset == TotalBitsToWrite) { break; }; Bits.SetBit(ref (*GetPixel(RawPixelArray, BytesPerBang, n)).G, z, Bits.GetBit(WriteBuffer[BitOffset / 8], (int)(BitOffset % 8))); BitOffset++; z++; }
                    z = 0; while (z < SaveBits) { if (BitOffset == TotalBitsToWrite) { break; }; Bits.SetBit(ref (*GetPixel(RawPixelArray, BytesPerBang, n)).R, z, Bits.GetBit(WriteBuffer[BitOffset / 8], (int)(BitOffset % 8))); BitOffset++; z++; }
                    z = 0; while (z < SaveBits) { if (BitOffset == TotalBitsToWrite) { break; }; Bits.SetBit(ref (*GetPixel(RawPixelArray, BytesPerBang, n)).A, z, Bits.GetBit(WriteBuffer[BitOffset / 8], (int)(BitOffset % 8))); BitOffset++; z++; }
                    if (BitOffset == TotalBitsToWrite) { break; } //Speed.
                    n++;
                }

                switch (System.IO.Path.GetExtension(OutputImage).ToLower())
                {
                    case ".png": { Image.Save(OutputImage, new PngEncoder() { CompressionLevel = PngCompressionLevel.BestCompression, BitDepth = PngBitDepth.Bit8, TransparentColorMode = PngTransparentColorMode.Preserve, ColorType = PngColorType.RgbWithAlpha }); } break;
                    case ".tiff": { Image.Save(OutputImage, new TiffEncoder() { CompressionLevel = SixLabors.ImageSharp.Compression.Zlib.DeflateCompressionLevel.BestCompression, BitsPerPixel = TiffBitsPerPixel.Bit32 }); } break;
                    case ".tga": { Image.Save(OutputImage, new TgaEncoder() { Compression = TgaCompression.RunLength, BitsPerPixel = TgaBitsPerPixel.Pixel32 }); } break; //Only <= 32bit images.
                    case ".webp": { Image.Save(OutputImage, new WebpEncoder() { FileFormat = WebpFileFormatType.Lossless, Method = WebpEncodingMethod.BestQuality, Quality = 100, TransparentColorMode = WebpTransparentColorMode.Preserve, UseAlphaCompression = true }); } break;  //Only <= 32bit images.
                    case ".bmp": { Image.Save(OutputImage, new BmpEncoder() { BitsPerPixel = BmpBitsPerPixel.Pixel32 }); } break; //Only <= 32bit images.
                    default: { throw new System.Exception("Unknown image format."); }
                }

                AES.Dispose();
                n = 0; while (n < BufferHandles.Length) { BufferHandles[n].Dispose(); n++; }; System.Runtime.InteropServices.Marshal.FreeHGlobal((nint)RawPixelArray);
                Image.Dispose();
            }

        }
        public static void DecryptImage32(string InputImage, out byte[] Output, byte[] EncryptionPassword, out string DataFileName)
        {
            if (InputImage == null || EncryptionPassword == null) { throw new System.Exception("InputImage and EncryptionPassword cannot be null."); }
            if (!System.IO.File.Exists(InputImage)) { throw new System.Exception("InputImage file doesn't exist."); }

            unsafe
            {
                SixLabors.ImageSharp.Image<Rgba32> Image = ConvertToImage32_4CH(InputImage);
                Memory<Rgba32>[] MemoryBang = Image.GetPixelMemoryGroup().ToArray(); var BufferHandles = new System.Buffers.MemoryHandle[MemoryBang.Length]; Pixel32** RawPixelArray = (Pixel32**)System.Runtime.InteropServices.Marshal.AllocHGlobal(sizeof(void*) * MemoryBang.Length);
                int e = 0, BytesPerBang = MemoryBang[0].Length; while (e < MemoryBang.Length) { BufferHandles[e] = MemoryBang[e].Pin(); RawPixelArray[e] = (Pixel32*)BufferHandles[e].Pointer; e++; }
                static Pixel32* GetPixel(Pixel32** PXArray, int BPB, int Offset) { return &PXArray[Offset / BPB][Offset - ((Offset / BPB) * BPB)]; }

                int SaveBits = 0, sBpp;
                Bits.SetBit(ref SaveBits, 0, Bits.GetBit((*GetPixel(RawPixelArray, BytesPerBang, 0)).B, 0)); Bits.SetBit(ref SaveBits, 1, Bits.GetBit((*GetPixel(RawPixelArray, BytesPerBang, 0)).G, 0));
                Bits.SetBit(ref SaveBits, 2, Bits.GetBit((*GetPixel(RawPixelArray, BytesPerBang, 0)).R, 0)); Bits.SetBit(ref SaveBits, 3, Bits.GetBit((*GetPixel(RawPixelArray, BytesPerBang, 0)).R, 1));
                SaveBits++; int BufferByteSize = GetTotalPossibleBufferSize4CH(Image.Width, Image.Height, SaveBits); sBpp = 4 * SaveBits;

                static void ReadInPixels(int TotalPixelCount, byte[] Buffer, Pixel32** PXArray, int BPB, int SaveBits)
                {
                    int n = 1, z; long BitOffset = 0, BufferTotalBits = Buffer.Length * 8; while (n < TotalPixelCount && BitOffset != BufferTotalBits) //+1 for the first save bits pixel.
                    {
                        z = 0; while (z < SaveBits && BitOffset != BufferTotalBits) { Bits.SetBit(ref Buffer[BitOffset / 8], (int)(BitOffset % 8), Bits.GetBit((*GetPixel(PXArray, BPB, n)).B, z)); z++; BitOffset++; }
                        z = 0; while (z < SaveBits && BitOffset != BufferTotalBits) { Bits.SetBit(ref Buffer[BitOffset / 8], (int)(BitOffset % 8), Bits.GetBit((*GetPixel(PXArray, BPB, n)).G, z)); z++; BitOffset++; }
                        z = 0; while (z < SaveBits && BitOffset != BufferTotalBits) { Bits.SetBit(ref Buffer[BitOffset / 8], (int)(BitOffset % 8), Bits.GetBit((*GetPixel(PXArray, BPB, n)).R, z)); z++; BitOffset++; }
                        z = 0; while (z < SaveBits && BitOffset != BufferTotalBits) { Bits.SetBit(ref Buffer[BitOffset / 8], (int)(BitOffset % 8), Bits.GetBit((*GetPixel(PXArray, BPB, n)).A, z)); z++; BitOffset++; }
                        n++;
                    }
                }

                byte[] EncryptedBuffer = new byte[BufferByteSize];
                ReadInPixels(Image.Width * Image.Height, EncryptedBuffer, RawPixelArray, BytesPerBang, SaveBits);

                int ByteOffset = 0;
                int NonceSize = System.BitConverter.ToInt32(EncryptedBuffer, ByteOffset); byte[] Nonce = new byte[NonceSize]; ByteOffset += 4;
                System.Buffer.BlockCopy(EncryptedBuffer, ByteOffset, Nonce, 0, NonceSize); ByteOffset += NonceSize;

                int TagSize = System.BitConverter.ToInt32(EncryptedBuffer, 4 + NonceSize); byte[] Tag = new byte[TagSize]; ByteOffset += 4;
                System.Buffer.BlockCopy(EncryptedBuffer, 4 + NonceSize + 4, Tag, 0, TagSize); ByteOffset += TagSize;

                byte[] EncryptedData = new byte[EncryptedBuffer.Length - ByteOffset];
                System.Buffer.BlockCopy(EncryptedBuffer, ByteOffset, EncryptedData, 0, EncryptedData.Length);

                System.Array.Resize(ref EncryptionPassword, 32); Output = new byte[EncryptedData.Length];
                System.Security.Cryptography.AesGcm AES = new System.Security.Cryptography.AesGcm(EncryptionPassword);

                AES.Decrypt(Nonce, EncryptedData, Tag, Output);

                int DataNameLength = System.BitConverter.ToInt32(Output, 0); DataFileName = DataNameLength > 0 ? System.Text.Encoding.Unicode.GetString(Output, 4, DataNameLength) : null;
                int DataLength = System.BitConverter.ToInt32(Output, 4 + DataNameLength); System.Buffer.BlockCopy(Output, 4 + DataNameLength + 4, Output, 0, DataLength); System.Array.Resize(ref Output, DataLength);

                AES.Dispose();
                int n = 0; while (n < BufferHandles.Length) { BufferHandles[n].Dispose(); n++; }; System.Runtime.InteropServices.Marshal.FreeHGlobal((nint)RawPixelArray);
                Image.Dispose();
            }
        }

        private static SixLabors.ImageSharp.Image<Rgb48> ConvertToImage48_3CH(string FileName)
        {
            return Image.Load<Rgb48>(FileName);
        }
        public static void EncryptImage48(string InputImage, string OutputImage, byte[] Buffer, int Offset, int Count, int SaveBits, byte[] EncryptionPassword, string DataFileName = null, bool UsePlatformSafeEncryption = false)
        {
            int sBpp = 3 * SaveBits; if (SaveBits > 16 || SaveBits < 1) { throw new System.Exception("Save bits cannot be below 1 or above 16."); }
            if (InputImage == null || OutputImage == null || Buffer == null || EncryptionPassword == null) { throw new System.Exception("InputImage, OutputImage, Buffer and EncryptionPassword cannot be null."); }
            if (!System.IO.File.Exists(InputImage)) { throw new System.Exception("InputImage file doesn't exist."); }

            unsafe
            {
                SixLabors.ImageSharp.Image<Rgb48> Image = ConvertToImage48_3CH(InputImage);
                Memory<Rgb48>[] MemoryBang = Image.GetPixelMemoryGroup().ToArray(); var BufferHandles = new System.Buffers.MemoryHandle[MemoryBang.Length]; Pixel48** RawPixelArray = (Pixel48**)System.Runtime.InteropServices.Marshal.AllocHGlobal(sizeof(void*) * MemoryBang.Length);
                int e = 0, BytesPerBang = MemoryBang[0].Length; while (e < MemoryBang.Length) { BufferHandles[e] = MemoryBang[e].Pin(); RawPixelArray[e] = (Pixel48*)BufferHandles[e].Pointer; e++; }
                static Pixel48* GetPixel(Pixel48** PXArray, int BPB, int Offset) { return &PXArray[Offset / BPB][Offset - ((Offset / BPB) * BPB)]; }

                Bits.SetBit(ref (*GetPixel(RawPixelArray, BytesPerBang, 0)).B, 0, Bits.GetBit(SaveBits - 1, 0)); Bits.SetBit(ref (*GetPixel(RawPixelArray, BytesPerBang, 0)).G, 0, Bits.GetBit(SaveBits - 1, 1));
                Bits.SetBit(ref (*GetPixel(RawPixelArray, BytesPerBang, 0)).R, 0, Bits.GetBit(SaveBits - 1, 2)); Bits.SetBit(ref (*GetPixel(RawPixelArray, BytesPerBang, 0)).R, 1, Bits.GetBit(SaveBits - 1, 3));

                byte[] DataName = DataFileName != null && DataFileName != "" ? System.Text.Encoding.Unicode.GetBytes(System.IO.Path.GetFileName(DataFileName)) : new byte[0];
                byte[] DataToEncrypt = new byte[EncryptionPassword != null ? (4 + DataName.Length + 4 + Count) : 0];

                System.Buffer.BlockCopy(System.BitConverter.GetBytes(DataName.Length), 0, DataToEncrypt, 0, 4); //DataName.Length.
                System.Buffer.BlockCopy(DataName, 0, DataToEncrypt, 4, DataName.Length); //DataName.
                System.Buffer.BlockCopy(System.BitConverter.GetBytes(Count), 0, DataToEncrypt, 4 + DataName.Length, 4); //Count.
                System.Buffer.BlockCopy(Buffer, Offset, DataToEncrypt, 4 + DataName.Length + 4, Count); //Actual Data.

                System.Array.Resize(ref EncryptionPassword, 32); //Make sure password fits.
                System.Security.Cryptography.AesGcm AES = new System.Security.Cryptography.AesGcm(EncryptionPassword);
                byte[] Nonce = System.Security.Cryptography.RandomNumberGenerator.GetBytes(System.Security.Cryptography.AesGcm.NonceByteSizes.MaxSize); //RNG.
                byte[] Tag = new byte[UsePlatformSafeEncryption ? 12 : System.Security.Cryptography.AesGcm.TagByteSizes.MaxSize]; //Gets and Stores Tag.
                byte[] WriteBuffer = new byte[4 + Nonce.Length + 4 + Tag.Length]; //Buffer to store all.

                int BufferByteSize = GetTotalPossibleBufferSize3CH(Image.Width, Image.Height, SaveBits);
                if (WriteBuffer.Length + DataToEncrypt.Length > BufferByteSize) { throw new System.Exception("Not enough space in image to save file with the given noise bits. (Available: " + (BufferByteSize / 1024f).ToString("0.00") + "kb < Needed: " + (WriteBuffer.Length / 1024f).ToString("0.00") + "kb)"); }
                System.Array.Resize(ref DataToEncrypt, BufferByteSize - WriteBuffer.Length); //So we have full frame buffer.
                System.Array.Resize(ref WriteBuffer, BufferByteSize); //So we have full frame buffer.

                byte[] EncryptedData = new byte[DataToEncrypt.Length]; int EncBitOffset = 0; // 1:1.
                System.Buffer.BlockCopy(System.BitConverter.GetBytes(Nonce.Length), 0, WriteBuffer, EncBitOffset, 4); EncBitOffset += 4; //Write Nonce len.
                System.Buffer.BlockCopy(Nonce, 0, WriteBuffer, EncBitOffset, Nonce.Length); EncBitOffset += Nonce.Length; //Write Nonce.
                System.Buffer.BlockCopy(System.BitConverter.GetBytes(Tag.Length), 0, WriteBuffer, EncBitOffset, 4); EncBitOffset += 4; //Write Tag Len.
                AES.Encrypt(Nonce, DataToEncrypt, EncryptedData, Tag); //Encrypt Data.
                System.Buffer.BlockCopy(Tag, 0, WriteBuffer, EncBitOffset, Tag.Length); EncBitOffset += Tag.Length;
                System.Buffer.BlockCopy(EncryptedData, 0, WriteBuffer, EncBitOffset, EncryptedData.Length); EncBitOffset += EncryptedData.Length;

                int TotalPixelCount = Image.Width * Image.Height, z, n = 1; //1 for the pixel we used for bit size/count.
                long BitOffset = 0, TotalBitsToWrite = BufferByteSize * 8;
                while (n < TotalPixelCount)
                {
                    z = 0; while (z < SaveBits) { if (BitOffset == TotalBitsToWrite) { break; }; Bits.SetBit(ref (*GetPixel(RawPixelArray, BytesPerBang, n)).B, z, Bits.GetBit(WriteBuffer[BitOffset / 8], (int)(BitOffset % 8))); BitOffset++; z++; }
                    z = 0; while (z < SaveBits) { if (BitOffset == TotalBitsToWrite) { break; }; Bits.SetBit(ref (*GetPixel(RawPixelArray, BytesPerBang, n)).G, z, Bits.GetBit(WriteBuffer[BitOffset / 8], (int)(BitOffset % 8))); BitOffset++; z++; }
                    z = 0; while (z < SaveBits) { if (BitOffset == TotalBitsToWrite) { break; }; Bits.SetBit(ref (*GetPixel(RawPixelArray, BytesPerBang, n)).R, z, Bits.GetBit(WriteBuffer[BitOffset / 8], (int)(BitOffset % 8))); BitOffset++; z++; }
                    if (BitOffset == TotalBitsToWrite) { break; } //Speed.
                    n++;
                }

                switch (System.IO.Path.GetExtension(OutputImage).ToLower())
                {
                    case ".png": { Image.Save(OutputImage, new PngEncoder() { CompressionLevel = PngCompressionLevel.BestCompression, BitDepth = PngBitDepth.Bit16 }); } break;
                    //case ".tiff": { Image.Save(OutputImage, new TiffEncoder() { CompressionLevel = SixLabors.ImageSharp.Compression.Zlib.DeflateCompressionLevel.BestCompression, BitsPerPixel = TiffBitsPerPixel.Bit24 }); } break;
                    //case ".tga": { Image.Save(OutputImage, new TgaEncoder() { Compression = TgaCompression.RunLength, BitsPerPixel = TgaBitsPerPixel.Pixel32 }); } break; //Only <= 32bit images.
                    //case ".webp": { Image.Save(OutputImage, new WebpEncoder() { FileFormat = WebpFileFormatType.Lossless, Method = WebpEncodingMethod.BestQuality, Quality = 100, TransparentColorMode  = WebpTransparentColorMode.Preserve, UseAlphaCompression  = true }); } break;  //Only <= 32bit images.
                    //case ".bmp": { Image.Save(OutputImage, new BmpEncoder () {  BitsPerPixel = BmpBitsPerPixel.Pixel32 }); } break; //Only <= 32bit images.
                    default: { throw new System.Exception("Unknown image format."); }
                }

                AES.Dispose();
                n = 0; while (n < BufferHandles.Length) { BufferHandles[n].Dispose(); n++; }; System.Runtime.InteropServices.Marshal.FreeHGlobal((nint)RawPixelArray);
                Image.Dispose();
            }

        }
        public static void DecryptImage48(string InputImage, out byte[] Output, byte[] EncryptionPassword, out string DataFileName)
        {
            if (InputImage == null || EncryptionPassword == null) { throw new System.Exception("InputImage and EncryptionPassword cannot be null."); }
            if (!System.IO.File.Exists(InputImage)) { throw new System.Exception("InputImage file doesn't exist."); }

            unsafe
            {
                SixLabors.ImageSharp.Image<Rgb48> Image = ConvertToImage48_3CH(InputImage);
                Memory<Rgb48>[] MemoryBang = Image.GetPixelMemoryGroup().ToArray(); var BufferHandles = new System.Buffers.MemoryHandle[MemoryBang.Length]; Pixel48** RawPixelArray = (Pixel48**)System.Runtime.InteropServices.Marshal.AllocHGlobal(sizeof(void*) * MemoryBang.Length);
                int e = 0, BytesPerBang = MemoryBang[0].Length; while (e < MemoryBang.Length) { BufferHandles[e] = MemoryBang[e].Pin(); RawPixelArray[e] = (Pixel48*)BufferHandles[e].Pointer; e++; }
                static Pixel48* GetPixel(Pixel48** PXArray, int BPB, int Offset) { return &PXArray[Offset / BPB][Offset - ((Offset / BPB) * BPB)]; }

                int SaveBits = 0, sBpp;
                Bits.SetBit(ref SaveBits, 0, Bits.GetBit((*GetPixel(RawPixelArray, BytesPerBang, 0)).B, 0)); Bits.SetBit(ref SaveBits, 1, Bits.GetBit((*GetPixel(RawPixelArray, BytesPerBang, 0)).G, 0));
                Bits.SetBit(ref SaveBits, 2, Bits.GetBit((*GetPixel(RawPixelArray, BytesPerBang, 0)).R, 0)); Bits.SetBit(ref SaveBits, 3, Bits.GetBit((*GetPixel(RawPixelArray, BytesPerBang, 0)).R, 1));
                SaveBits++; int BufferByteSize = GetTotalPossibleBufferSize3CH(Image.Width, Image.Height, SaveBits); sBpp = 3 * SaveBits;

                static void ReadInPixels(int TotalPixelCount, byte[] Buffer, Pixel48** PXArray, int BPB, int SaveBits)
                {
                    int n = 1, z; long BitOffset = 0, BufferTotalBits = Buffer.Length * 8; while (n < TotalPixelCount && BitOffset != BufferTotalBits) //+1 for the first save bits pixel.
                    {
                        z = 0; while (z < SaveBits && BitOffset != BufferTotalBits) { Bits.SetBit(ref Buffer[BitOffset / 8], (int)(BitOffset % 8), Bits.GetBit((*GetPixel(PXArray, BPB, n)).B, z)); z++; BitOffset++; }
                        z = 0; while (z < SaveBits && BitOffset != BufferTotalBits) { Bits.SetBit(ref Buffer[BitOffset / 8], (int)(BitOffset % 8), Bits.GetBit((*GetPixel(PXArray, BPB, n)).G, z)); z++; BitOffset++; }
                        z = 0; while (z < SaveBits && BitOffset != BufferTotalBits) { Bits.SetBit(ref Buffer[BitOffset / 8], (int)(BitOffset % 8), Bits.GetBit((*GetPixel(PXArray, BPB, n)).R, z)); z++; BitOffset++; }
                        n++;
                    }
                }

                byte[] EncryptedBuffer = new byte[BufferByteSize];
                ReadInPixels(Image.Width * Image.Height, EncryptedBuffer, RawPixelArray, BytesPerBang, SaveBits);

                int ByteOffset = 0;
                int NonceSize = System.BitConverter.ToInt32(EncryptedBuffer, ByteOffset); byte[] Nonce = new byte[NonceSize]; ByteOffset += 4;
                System.Buffer.BlockCopy(EncryptedBuffer, ByteOffset, Nonce, 0, NonceSize); ByteOffset += NonceSize;

                int TagSize = System.BitConverter.ToInt32(EncryptedBuffer, 4 + NonceSize); byte[] Tag = new byte[TagSize]; ByteOffset += 4;
                System.Buffer.BlockCopy(EncryptedBuffer, 4 + NonceSize + 4, Tag, 0, TagSize); ByteOffset += TagSize;

                byte[] EncryptedData = new byte[EncryptedBuffer.Length - ByteOffset];
                System.Buffer.BlockCopy(EncryptedBuffer, ByteOffset, EncryptedData, 0, EncryptedData.Length);

                System.Array.Resize(ref EncryptionPassword, 32); Output = new byte[EncryptedData.Length];
                System.Security.Cryptography.AesGcm AES = new System.Security.Cryptography.AesGcm(EncryptionPassword);

                AES.Decrypt(Nonce, EncryptedData, Tag, Output);

                int DataNameLength = System.BitConverter.ToInt32(Output, 0); DataFileName = DataNameLength > 0 ? System.Text.Encoding.Unicode.GetString(Output, 4, DataNameLength) : null;
                int DataLength = System.BitConverter.ToInt32(Output, 4 + DataNameLength); System.Buffer.BlockCopy(Output, 4 + DataNameLength + 4, Output, 0, DataLength); System.Array.Resize(ref Output, DataLength);

                AES.Dispose();
                int n = 0; while (n < BufferHandles.Length) { BufferHandles[n].Dispose(); n++; }; System.Runtime.InteropServices.Marshal.FreeHGlobal((nint)RawPixelArray);
                Image.Dispose();
            }
        }

        private static SixLabors.ImageSharp.Image<Rgba64> ConvertToImage64_4CH(string FileName)
        {
            return Image.Load<Rgba64>(FileName);
        }
        public static void EncryptImage64(string InputImage, string OutputImage, byte[] Buffer, int Offset, int Count, int SaveBits, byte[] EncryptionPassword, string DataFileName = null, bool UsePlatformSafeEncryption = false)
        {
            int sBpp = 4 * SaveBits; if (SaveBits > 16 || SaveBits < 1) { throw new System.Exception("Save bits cannot be below 1 or above 16."); }
            if (InputImage == null || OutputImage == null || Buffer == null || EncryptionPassword == null) { throw new System.Exception("InputImage, OutputImage, Buffer and EncryptionPassword cannot be null."); }
            if (!System.IO.File.Exists(InputImage)) { throw new System.Exception("InputImage file doesn't exist."); }

            unsafe
            {
                SixLabors.ImageSharp.Image<Rgba64> Image = ConvertToImage64_4CH(InputImage);
                Memory<Rgba64>[] MemoryBang = Image.GetPixelMemoryGroup().ToArray(); var BufferHandles = new System.Buffers.MemoryHandle[MemoryBang.Length]; Pixel64** RawPixelArray = (Pixel64**)System.Runtime.InteropServices.Marshal.AllocHGlobal(sizeof(void*) * MemoryBang.Length);
                int e = 0, BytesPerBang = MemoryBang[0].Length; while (e < MemoryBang.Length) { BufferHandles[e] = MemoryBang[e].Pin(); RawPixelArray[e] = (Pixel64*)BufferHandles[e].Pointer; e++; }
                static Pixel64* GetPixel(Pixel64** PXArray, int BPB, int Offset) { return &PXArray[Offset / BPB][Offset - ((Offset / BPB) * BPB)]; }

                Bits.SetBit(ref (*GetPixel(RawPixelArray, BytesPerBang, 0)).B, 0, Bits.GetBit(SaveBits - 1, 0)); Bits.SetBit(ref (*GetPixel(RawPixelArray, BytesPerBang, 0)).G, 0, Bits.GetBit(SaveBits - 1, 1));
                Bits.SetBit(ref (*GetPixel(RawPixelArray, BytesPerBang, 0)).R, 0, Bits.GetBit(SaveBits - 1, 2)); Bits.SetBit(ref (*GetPixel(RawPixelArray, BytesPerBang, 0)).R, 1, Bits.GetBit(SaveBits - 1, 3));

                byte[] DataName = DataFileName != null && DataFileName != "" ? System.Text.Encoding.Unicode.GetBytes(System.IO.Path.GetFileName(DataFileName)) : new byte[0];
                byte[] DataToEncrypt = new byte[EncryptionPassword != null ? (4 + DataName.Length + 4 + Count) : 0];

                System.Buffer.BlockCopy(System.BitConverter.GetBytes(DataName.Length), 0, DataToEncrypt, 0, 4); //DataName.Length.
                System.Buffer.BlockCopy(DataName, 0, DataToEncrypt, 4, DataName.Length); //DataName.
                System.Buffer.BlockCopy(System.BitConverter.GetBytes(Count), 0, DataToEncrypt, 4 + DataName.Length, 4); //Count.
                System.Buffer.BlockCopy(Buffer, Offset, DataToEncrypt, 4 + DataName.Length + 4, Count); //Actual Data.

                System.Array.Resize(ref EncryptionPassword, 32); //Make sure password fits.
                System.Security.Cryptography.AesGcm AES = new System.Security.Cryptography.AesGcm(EncryptionPassword);
                byte[] Nonce = System.Security.Cryptography.RandomNumberGenerator.GetBytes(System.Security.Cryptography.AesGcm.NonceByteSizes.MaxSize); //RNG.
                byte[] Tag = new byte[UsePlatformSafeEncryption ? 12 : System.Security.Cryptography.AesGcm.TagByteSizes.MaxSize]; //Gets and Stores Tag.
                byte[] WriteBuffer = new byte[4 + Nonce.Length + 4 + Tag.Length]; //Buffer to store all.

                int BufferByteSize = GetTotalPossibleBufferSize4CH(Image.Width, Image.Height, SaveBits);
                if (WriteBuffer.Length + DataToEncrypt.Length > BufferByteSize) { throw new System.Exception("Not enough space in image to save file with the given noise bits. (Available: " + (BufferByteSize / 1024f).ToString("0.00") + "kb < Needed: " + (WriteBuffer.Length / 1024f).ToString("0.00") + "kb)"); }
                System.Array.Resize(ref DataToEncrypt, BufferByteSize - WriteBuffer.Length); //So we have full frame buffer.
                System.Array.Resize(ref WriteBuffer, BufferByteSize); //So we have full frame buffer.

                byte[] EncryptedData = new byte[DataToEncrypt.Length]; int EncBitOffset = 0; // 1:1.
                System.Buffer.BlockCopy(System.BitConverter.GetBytes(Nonce.Length), 0, WriteBuffer, EncBitOffset, 4); EncBitOffset += 4; //Write Nonce len.
                System.Buffer.BlockCopy(Nonce, 0, WriteBuffer, EncBitOffset, Nonce.Length); EncBitOffset += Nonce.Length; //Write Nonce.
                System.Buffer.BlockCopy(System.BitConverter.GetBytes(Tag.Length), 0, WriteBuffer, EncBitOffset, 4); EncBitOffset += 4; //Write Tag Len.
                AES.Encrypt(Nonce, DataToEncrypt, EncryptedData, Tag); //Encrypt Data.
                System.Buffer.BlockCopy(Tag, 0, WriteBuffer, EncBitOffset, Tag.Length); EncBitOffset += Tag.Length;
                System.Buffer.BlockCopy(EncryptedData, 0, WriteBuffer, EncBitOffset, EncryptedData.Length); EncBitOffset += EncryptedData.Length;

                int TotalPixelCount = Image.Width * Image.Height, z, n = 1; //1 for the pixel we used for bit size/count.
                long BitOffset = 0, TotalBitsToWrite = BufferByteSize * 8;
                while (n < TotalPixelCount)
                {
                    z = 0; while (z < SaveBits) { if (BitOffset == TotalBitsToWrite) { break; }; Bits.SetBit(ref (*GetPixel(RawPixelArray, BytesPerBang, n)).B, z, Bits.GetBit(WriteBuffer[BitOffset / 8], (int)(BitOffset % 8))); BitOffset++; z++; }
                    z = 0; while (z < SaveBits) { if (BitOffset == TotalBitsToWrite) { break; }; Bits.SetBit(ref (*GetPixel(RawPixelArray, BytesPerBang, n)).G, z, Bits.GetBit(WriteBuffer[BitOffset / 8], (int)(BitOffset % 8))); BitOffset++; z++; }
                    z = 0; while (z < SaveBits) { if (BitOffset == TotalBitsToWrite) { break; }; Bits.SetBit(ref (*GetPixel(RawPixelArray, BytesPerBang, n)).R, z, Bits.GetBit(WriteBuffer[BitOffset / 8], (int)(BitOffset % 8))); BitOffset++; z++; }
                    z = 0; while (z < SaveBits) { if (BitOffset == TotalBitsToWrite) { break; }; Bits.SetBit(ref (*GetPixel(RawPixelArray, BytesPerBang, n)).A, z, Bits.GetBit(WriteBuffer[BitOffset / 8], (int)(BitOffset % 8))); BitOffset++; z++; }
                    if (BitOffset == TotalBitsToWrite) { break; } //Speed.
                    n++;
                }

                switch (System.IO.Path.GetExtension(OutputImage).ToLower())
                {
                    case ".png": { Image.Save(OutputImage, new PngEncoder() { CompressionLevel = PngCompressionLevel.BestCompression, BitDepth = PngBitDepth.Bit16, TransparentColorMode = PngTransparentColorMode.Preserve, ColorType = PngColorType.RgbWithAlpha }); } break;
                    //case ".tiff": { Image.Save(OutputImage, new TiffEncoder() { CompressionLevel = SixLabors.ImageSharp.Compression.Zlib.DeflateCompressionLevel.BestCompression, BitsPerPixel = TiffBitsPerPixel.Bit24 }); } break;
                    //case ".tga": { Image.Save(OutputImage, new TgaEncoder() { Compression = TgaCompression.RunLength, BitsPerPixel = TgaBitsPerPixel.Pixel32 }); } break; //Only <= 32bit images.
                    //case ".webp": { Image.Save(OutputImage, new WebpEncoder() { FileFormat = WebpFileFormatType.Lossless, Method = WebpEncodingMethod.BestQuality, Quality = 100, TransparentColorMode = WebpTransparentColorMode.Preserve, UseAlphaCompression = true }); } break;  //Only <= 32bit images.
                    //case ".bmp": { Image.Save(OutputImage, new BmpEncoder() { BitsPerPixel = BmpBitsPerPixel.Pixel32 }); } break; //Only <= 32bit images.
                    default: { throw new System.Exception("Unknown image format."); }
                }

                AES.Dispose();
                n = 0; while (n < BufferHandles.Length) { BufferHandles[n].Dispose(); n++; }; System.Runtime.InteropServices.Marshal.FreeHGlobal((nint)RawPixelArray);
                Image.Dispose();
            }

        }
        public static void DecryptImage64(string InputImage, out byte[] Output, byte[] EncryptionPassword, out string DataFileName)
        {
            if (InputImage == null || EncryptionPassword == null) { throw new System.Exception("InputImage and EncryptionPassword cannot be null."); }
            if (!System.IO.File.Exists(InputImage)) { throw new System.Exception("InputImage file doesn't exist."); }

            unsafe
            {
                SixLabors.ImageSharp.Image<Rgba64> Image = ConvertToImage64_4CH(InputImage);
                Memory<Rgba64>[] MemoryBang = Image.GetPixelMemoryGroup().ToArray(); var BufferHandles = new System.Buffers.MemoryHandle[MemoryBang.Length]; Pixel64** RawPixelArray = (Pixel64**)System.Runtime.InteropServices.Marshal.AllocHGlobal(sizeof(void*) * MemoryBang.Length);
                int e = 0, BytesPerBang = MemoryBang[0].Length; while (e < MemoryBang.Length) { BufferHandles[e] = MemoryBang[e].Pin(); RawPixelArray[e] = (Pixel64*)BufferHandles[e].Pointer; e++; }
                static Pixel64* GetPixel(Pixel64** PXArray, int BPB, int Offset) { return &PXArray[Offset / BPB][Offset - ((Offset / BPB) * BPB)]; }

                int SaveBits = 0, sBpp;
                Bits.SetBit(ref SaveBits, 0, Bits.GetBit((*GetPixel(RawPixelArray, BytesPerBang, 0)).B, 0)); Bits.SetBit(ref SaveBits, 1, Bits.GetBit((*GetPixel(RawPixelArray, BytesPerBang, 0)).G, 0));
                Bits.SetBit(ref SaveBits, 2, Bits.GetBit((*GetPixel(RawPixelArray, BytesPerBang, 0)).R, 0)); Bits.SetBit(ref SaveBits, 3, Bits.GetBit((*GetPixel(RawPixelArray, BytesPerBang, 0)).R, 1));
                SaveBits++; int BufferByteSize = GetTotalPossibleBufferSize4CH(Image.Width, Image.Height, SaveBits); sBpp = 4 * SaveBits;

                static void ReadInPixels(int TotalPixelCount, byte[] Buffer, Pixel64** PXArray, int BPB, int SaveBits)
                {
                    int n = 1, z; long BitOffset = 0, BufferTotalBits = Buffer.Length * 8; while (n < TotalPixelCount && BitOffset != BufferTotalBits) //+1 for the first save bits pixel.
                    {
                        z = 0; while (z < SaveBits && BitOffset != BufferTotalBits) { Bits.SetBit(ref Buffer[BitOffset / 8], (int)(BitOffset % 8), Bits.GetBit((*GetPixel(PXArray, BPB, n)).B, z)); z++; BitOffset++; }
                        z = 0; while (z < SaveBits && BitOffset != BufferTotalBits) { Bits.SetBit(ref Buffer[BitOffset / 8], (int)(BitOffset % 8), Bits.GetBit((*GetPixel(PXArray, BPB, n)).G, z)); z++; BitOffset++; }
                        z = 0; while (z < SaveBits && BitOffset != BufferTotalBits) { Bits.SetBit(ref Buffer[BitOffset / 8], (int)(BitOffset % 8), Bits.GetBit((*GetPixel(PXArray, BPB, n)).R, z)); z++; BitOffset++; }
                        z = 0; while (z < SaveBits && BitOffset != BufferTotalBits) { Bits.SetBit(ref Buffer[BitOffset / 8], (int)(BitOffset % 8), Bits.GetBit((*GetPixel(PXArray, BPB, n)).A, z)); z++; BitOffset++; }
                        n++;
                    }
                }

                byte[] EncryptedBuffer = new byte[BufferByteSize];
                ReadInPixels(Image.Width * Image.Height, EncryptedBuffer, RawPixelArray, BytesPerBang, SaveBits);

                int ByteOffset = 0;
                int NonceSize = System.BitConverter.ToInt32(EncryptedBuffer, ByteOffset); byte[] Nonce = new byte[NonceSize]; ByteOffset += 4;
                System.Buffer.BlockCopy(EncryptedBuffer, ByteOffset, Nonce, 0, NonceSize); ByteOffset += NonceSize;

                int TagSize = System.BitConverter.ToInt32(EncryptedBuffer, 4 + NonceSize); byte[] Tag = new byte[TagSize]; ByteOffset += 4;
                System.Buffer.BlockCopy(EncryptedBuffer, 4 + NonceSize + 4, Tag, 0, TagSize); ByteOffset += TagSize;

                byte[] EncryptedData = new byte[EncryptedBuffer.Length - ByteOffset];
                System.Buffer.BlockCopy(EncryptedBuffer, ByteOffset, EncryptedData, 0, EncryptedData.Length);

                System.Array.Resize(ref EncryptionPassword, 32); Output = new byte[EncryptedData.Length];
                System.Security.Cryptography.AesGcm AES = new System.Security.Cryptography.AesGcm(EncryptionPassword);

                AES.Decrypt(Nonce, EncryptedData, Tag, Output);

                int DataNameLength = System.BitConverter.ToInt32(Output, 0); DataFileName = DataNameLength > 0 ? System.Text.Encoding.Unicode.GetString(Output, 4, DataNameLength) : null;
                int DataLength = System.BitConverter.ToInt32(Output, 4 + DataNameLength); System.Buffer.BlockCopy(Output, 4 + DataNameLength + 4, Output, 0, DataLength); System.Array.Resize(ref Output, DataLength);

                AES.Dispose();
                int n = 0; while (n < BufferHandles.Length) { BufferHandles[n].Dispose(); n++; }; System.Runtime.InteropServices.Marshal.FreeHGlobal((nint)RawPixelArray);
                Image.Dispose();
            }
        }

        public static void SaveImage24(string InputImage, string OutputImage, byte[] Buffer, int Offset, int Count, int SaveBits, string DataFileName = null)
        {
            int sBpp = 3 * SaveBits; if (SaveBits > 8 || SaveBits < 1) { throw new System.Exception("Save bits cannot be below 1 or above 8."); }
            if (InputImage == null || OutputImage == null || Buffer == null) { throw new System.Exception("InputImage, OutputImage and Buffer cannot be null."); }
            if (!System.IO.File.Exists(InputImage)) { throw new System.Exception("InputImage file doesn't exist."); }

            unsafe
            {
                SixLabors.ImageSharp.Image<Rgb24> Image = ConvertToImage24_3CH(InputImage);
                Memory<Rgb24>[] MemoryBang = Image.GetPixelMemoryGroup().ToArray(); var BufferHandles = new System.Buffers.MemoryHandle[MemoryBang.Length]; Pixel24** RawPixelArray = (Pixel24**)System.Runtime.InteropServices.Marshal.AllocHGlobal(sizeof(void*) * MemoryBang.Length);
                int e = 0, BytesPerBang = MemoryBang[0].Length; while (e < MemoryBang.Length) { BufferHandles[e] = MemoryBang[e].Pin(); RawPixelArray[e] = (Pixel24*)BufferHandles[e].Pointer; e++; }
                static Pixel24* GetPixel(Pixel24** PXArray, int BPB, int Offset) { return &PXArray[Offset / BPB][Offset - ((Offset / BPB) * BPB)]; }

                Bits.SetBit(ref (*GetPixel(RawPixelArray, BytesPerBang, 0)).B, 0, Bits.GetBit(SaveBits - 1, 0)); Bits.SetBit(ref (*GetPixel(RawPixelArray, BytesPerBang, 0)).G, 0, Bits.GetBit(SaveBits - 1, 1));
                Bits.SetBit(ref (*GetPixel(RawPixelArray, BytesPerBang, 0)).R, 0, Bits.GetBit(SaveBits - 1, 2)); Bits.SetBit(ref (*GetPixel(RawPixelArray, BytesPerBang, 0)).R, 1, Bits.GetBit(SaveBits - 1, 3));

                int BufferByteSize = GetTotalPossibleBufferSize3CH(Image.Width, Image.Height, SaveBits);
                byte[] DataName = DataFileName != null && DataFileName != "" ? System.Text.Encoding.Unicode.GetBytes(System.IO.Path.GetFileName(DataFileName)) : new byte[0];
                if ((4 + DataName.Length + Count + 4) > BufferByteSize) { throw new System.Exception("Not enough space in image to save file with the given noise bits. (Available: " + (BufferByteSize / 1024f).ToString("0.00") + "kb < Needed: " + ((4 + DataName.Length + Count + 4) / 1024f).ToString("0.00") + "kb)"); }

                byte[] DataToSave = new byte[4 + DataName.Length + 4 + Count];
                System.Buffer.BlockCopy(System.BitConverter.GetBytes(DataName.Length), 0, DataToSave, 0, 4); //DataName.Length.
                System.Buffer.BlockCopy(DataName, 0, DataToSave, 4, DataName.Length); //DataName.
                System.Buffer.BlockCopy(System.BitConverter.GetBytes(Count), 0, DataToSave, 4 + DataName.Length, 4); //Count.
                System.Buffer.BlockCopy(Buffer, Offset, DataToSave, 4 + DataName.Length + 4, Count); //Actual Data.

                int DataEndOffset = DataToSave.Length;
                System.Array.Resize(ref DataToSave, BufferByteSize);

                int Cycles = (DataToSave.Length - DataEndOffset) / (Count / 3), Extra = (DataToSave.Length - DataEndOffset) - (Cycles * (Count / 3)), n = 0;
                while (n < Cycles) { System.Buffer.BlockCopy(Buffer, Offset + (Count / 3), DataToSave, DataEndOffset + ((Count / 3) * n), Count / 3); n++; } //Fill up the whole frame buffer, otherwise clearly can see where data ends.
                if (Extra != 0) { System.Buffer.BlockCopy(Buffer, Offset + (Count / 3), DataToSave, DataEndOffset + ((Count / 3) * n), Extra); }

                int TotalPixelCount = Image.Width * Image.Height, z; n = 1; //1 for the pixel we used for bit size/count.
                long BitOffset = 0, TotalBitsToWrite = BufferByteSize * 8;
                while (n < TotalPixelCount)
                {
                    z = 0; while (z < SaveBits) { if (BitOffset == TotalBitsToWrite) { break; }; Bits.SetBit(ref (*GetPixel(RawPixelArray, BytesPerBang, n)).B, z, Bits.GetBit(DataToSave[BitOffset / 8], (int)(BitOffset % 8))); BitOffset++; z++; }
                    z = 0; while (z < SaveBits) { if (BitOffset == TotalBitsToWrite) { break; }; Bits.SetBit(ref (*GetPixel(RawPixelArray, BytesPerBang, n)).G, z, Bits.GetBit(DataToSave[BitOffset / 8], (int)(BitOffset % 8))); BitOffset++; z++; }
                    z = 0; while (z < SaveBits) { if (BitOffset == TotalBitsToWrite) { break; }; Bits.SetBit(ref (*GetPixel(RawPixelArray, BytesPerBang, n)).R, z, Bits.GetBit(DataToSave[BitOffset / 8], (int)(BitOffset % 8))); BitOffset++; z++; }
                    if (BitOffset == TotalBitsToWrite) { break; } //Speed.
                    n++;
                }

                switch (System.IO.Path.GetExtension(OutputImage).ToLower())
                {
                    case ".png": { Image.Save(OutputImage, new PngEncoder() { CompressionLevel = PngCompressionLevel.BestCompression, BitDepth = PngBitDepth.Bit8, TransparentColorMode = PngTransparentColorMode.Clear, ColorType = PngColorType.Rgb }); } break;
                    case ".tiff": { Image.Save(OutputImage, new TiffEncoder() { CompressionLevel = SixLabors.ImageSharp.Compression.Zlib.DeflateCompressionLevel.BestCompression, BitsPerPixel = TiffBitsPerPixel.Bit24 }); } break;
                    case ".tga": { Image.Save(OutputImage, new TgaEncoder() { Compression = TgaCompression.RunLength, BitsPerPixel = TgaBitsPerPixel.Pixel32 }); } break; //Only <= 32bit images.
                    case ".webp": { Image.Save(OutputImage, new WebpEncoder() { FileFormat = WebpFileFormatType.Lossless, Method = WebpEncodingMethod.BestQuality, Quality = 100, TransparentColorMode = WebpTransparentColorMode.Clear, UseAlphaCompression = false }); } break;  //Only <= 32bit images.
                    case ".bmp": { Image.Save(OutputImage, new BmpEncoder() { BitsPerPixel = BmpBitsPerPixel.Pixel32 }); } break; //Only <= 32bit images.
                    default: { throw new System.Exception("Unknown image format."); }
                }

                n = 0; while (n < BufferHandles.Length) { BufferHandles[n].Dispose(); n++; }; System.Runtime.InteropServices.Marshal.FreeHGlobal((nint)RawPixelArray);
                Image.Dispose();
            }

        }
        public static void ReadImage24(string InputImage, out byte[] Output, out string DataFileName)
        {
            if (InputImage == null) { throw new System.Exception("InputImage cannot be null."); }
            if (!System.IO.File.Exists(InputImage)) { throw new System.Exception("InputImage file doesn't exist."); }

            unsafe
            {
                SixLabors.ImageSharp.Image<Rgb24> Image = ConvertToImage24_3CH(InputImage);
                Memory<Rgb24>[] MemoryBang = Image.GetPixelMemoryGroup().ToArray(); var BufferHandles = new System.Buffers.MemoryHandle[MemoryBang.Length]; Pixel24** RawPixelArray = (Pixel24**)System.Runtime.InteropServices.Marshal.AllocHGlobal(sizeof(void*) * MemoryBang.Length);
                int e = 0, BytesPerBang = MemoryBang[0].Length; while (e < MemoryBang.Length) { BufferHandles[e] = MemoryBang[e].Pin(); RawPixelArray[e] = (Pixel24*)BufferHandles[e].Pointer; e++; }
                static Pixel24* GetPixel(Pixel24** PXArray, int BPB, int Offset) { return &PXArray[Offset / BPB][Offset - ((Offset / BPB) * BPB)]; }

                int SaveBits = 0, sBpp;
                Bits.SetBit(ref SaveBits, 0, Bits.GetBit((*GetPixel(RawPixelArray, BytesPerBang, 0)).B, 0)); Bits.SetBit(ref SaveBits, 1, Bits.GetBit((*GetPixel(RawPixelArray, BytesPerBang, 0)).G, 0));
                Bits.SetBit(ref SaveBits, 2, Bits.GetBit((*GetPixel(RawPixelArray, BytesPerBang, 0)).R, 0)); Bits.SetBit(ref SaveBits, 3, Bits.GetBit((*GetPixel(RawPixelArray, BytesPerBang, 0)).R, 1));
                SaveBits++; int BufferByteSize = GetTotalPossibleBufferSize3CH(Image.Width, Image.Height, SaveBits); sBpp = 3 * SaveBits;

                static void ReadInPixels(int TotalPixelCount, byte[] Buffer, Pixel24** PXArray, int BPB, int SaveBits)
                {
                    int n = 1, z; long BitOffset = 0, BufferTotalBits = Buffer.Length * 8; while (n < TotalPixelCount && BitOffset != BufferTotalBits) //+1 for the first save bits pixel.
                    {
                        z = 0; while (z < SaveBits && BitOffset != BufferTotalBits) { Bits.SetBit(ref Buffer[BitOffset / 8], (int)(BitOffset % 8), Bits.GetBit((*GetPixel(PXArray, BPB, n)).B, z)); z++; BitOffset++; }
                        z = 0; while (z < SaveBits && BitOffset != BufferTotalBits) { Bits.SetBit(ref Buffer[BitOffset / 8], (int)(BitOffset % 8), Bits.GetBit((*GetPixel(PXArray, BPB, n)).G, z)); z++; BitOffset++; }
                        z = 0; while (z < SaveBits && BitOffset != BufferTotalBits) { Bits.SetBit(ref Buffer[BitOffset / 8], (int)(BitOffset % 8), Bits.GetBit((*GetPixel(PXArray, BPB, n)).R, z)); z++; BitOffset++; }
                        n++;
                    }
                }

                byte[] ReadBuffer = new byte[BufferByteSize];
                ReadInPixels(Image.Width * Image.Height, ReadBuffer, RawPixelArray, BytesPerBang, SaveBits);

                int DataNameLength = System.BitConverter.ToInt32(ReadBuffer, 0); DataFileName = DataNameLength > 0 ? System.Text.Encoding.Unicode.GetString(ReadBuffer, 4, DataNameLength) : null;
                int DataLength = System.BitConverter.ToInt32(ReadBuffer, 4 + DataNameLength); Output = new byte[DataLength]; System.Buffer.BlockCopy(ReadBuffer, 4 + DataNameLength + 4, Output, 0, DataLength); ;

                int n = 0; while (n < BufferHandles.Length) { BufferHandles[n].Dispose(); n++; }; System.Runtime.InteropServices.Marshal.FreeHGlobal((nint)RawPixelArray);
                Image.Dispose();
            }
        }

        public static void SaveImage32(string InputImage, string OutputImage, byte[] Buffer, int Offset, int Count, int SaveBits, string DataFileName = null)
        {
            int sBpp = 4 * SaveBits; if (SaveBits > 8 || SaveBits < 1) { throw new System.Exception("Save bits cannot be below 1 or above 8."); }
            if (InputImage == null || OutputImage == null || Buffer == null) { throw new System.Exception("InputImage, OutputImage and Buffer cannot be null."); }
            if (!System.IO.File.Exists(InputImage)) { throw new System.Exception("InputImage file doesn't exist."); }

            unsafe
            {
                SixLabors.ImageSharp.Image<Rgba32> Image = ConvertToImage32_4CH(InputImage);
                Memory<Rgba32>[] MemoryBang = Image.GetPixelMemoryGroup().ToArray(); var BufferHandles = new System.Buffers.MemoryHandle[MemoryBang.Length]; Pixel32** RawPixelArray = (Pixel32**)System.Runtime.InteropServices.Marshal.AllocHGlobal(sizeof(void*) * MemoryBang.Length);
                int e = 0, BytesPerBang = MemoryBang[0].Length; while (e < MemoryBang.Length) { BufferHandles[e] = MemoryBang[e].Pin(); RawPixelArray[e] = (Pixel32*)BufferHandles[e].Pointer; e++; }
                static Pixel32* GetPixel(Pixel32** PXArray, int BPB, int Offset) { return &PXArray[Offset / BPB][Offset - ((Offset / BPB) * BPB)]; }

                Bits.SetBit(ref (*GetPixel(RawPixelArray, BytesPerBang, 0)).B, 0, Bits.GetBit(SaveBits - 1, 0)); Bits.SetBit(ref (*GetPixel(RawPixelArray, BytesPerBang, 0)).G, 0, Bits.GetBit(SaveBits - 1, 1));
                Bits.SetBit(ref (*GetPixel(RawPixelArray, BytesPerBang, 0)).R, 0, Bits.GetBit(SaveBits - 1, 2)); Bits.SetBit(ref (*GetPixel(RawPixelArray, BytesPerBang, 0)).R, 1, Bits.GetBit(SaveBits - 1, 3));

                int BufferByteSize = GetTotalPossibleBufferSize4CH(Image.Width, Image.Height, SaveBits);
                byte[] DataName = DataFileName != null && DataFileName != "" ? System.Text.Encoding.Unicode.GetBytes(System.IO.Path.GetFileName(DataFileName)) : new byte[0];
                if ((4 + DataName.Length + Count + 4) > BufferByteSize) { throw new System.Exception("Not enough space in image to save file with the given noise bits. (Available: " + (BufferByteSize / 1024f).ToString("0.00") + "kb < Needed: " + ((4 + DataName.Length + Count + 4) / 1024f).ToString("0.00") + "kb)"); }

                byte[] DataToSave = new byte[4 + DataName.Length + 4 + Count];
                System.Buffer.BlockCopy(System.BitConverter.GetBytes(DataName.Length), 0, DataToSave, 0, 4); //DataName.Length.
                System.Buffer.BlockCopy(DataName, 0, DataToSave, 4, DataName.Length); //DataName.
                System.Buffer.BlockCopy(System.BitConverter.GetBytes(Count), 0, DataToSave, 4 + DataName.Length, 4); //Count.
                System.Buffer.BlockCopy(Buffer, Offset, DataToSave, 4 + DataName.Length + 4, Count); //Actual Data.

                int DataEndOffset = DataToSave.Length;
                System.Array.Resize(ref DataToSave, BufferByteSize);

                int Cycles = (DataToSave.Length - DataEndOffset) / (Count / 3), Extra = (DataToSave.Length - DataEndOffset) - (Cycles * (Count / 3)), n = 0;
                while (n < Cycles) { System.Buffer.BlockCopy(Buffer, Offset + (Count / 3), DataToSave, DataEndOffset + ((Count / 3) * n), Count / 3); n++; } //Fill up the whole frame buffer, otherwise clearly can see where data ends.
                if (Extra != 0) { System.Buffer.BlockCopy(Buffer, Offset + (Count / 3), DataToSave, DataEndOffset + ((Count / 3) * n), Extra); }

                int TotalPixelCount = Image.Width * Image.Height, z; n = 1; //1 for the pixel we used for bit size/count.
                long BitOffset = 0, TotalBitsToWrite = BufferByteSize * 8;
                while (n < TotalPixelCount)
                {
                    z = 0; while (z < SaveBits) { if (BitOffset == TotalBitsToWrite) { break; }; Bits.SetBit(ref (*GetPixel(RawPixelArray, BytesPerBang, n)).B, z, Bits.GetBit(DataToSave[BitOffset / 8], (int)(BitOffset % 8))); BitOffset++; z++; }
                    z = 0; while (z < SaveBits) { if (BitOffset == TotalBitsToWrite) { break; }; Bits.SetBit(ref (*GetPixel(RawPixelArray, BytesPerBang, n)).G, z, Bits.GetBit(DataToSave[BitOffset / 8], (int)(BitOffset % 8))); BitOffset++; z++; }
                    z = 0; while (z < SaveBits) { if (BitOffset == TotalBitsToWrite) { break; }; Bits.SetBit(ref (*GetPixel(RawPixelArray, BytesPerBang, n)).R, z, Bits.GetBit(DataToSave[BitOffset / 8], (int)(BitOffset % 8))); BitOffset++; z++; }
                    z = 0; while (z < SaveBits) { if (BitOffset == TotalBitsToWrite) { break; }; Bits.SetBit(ref (*GetPixel(RawPixelArray, BytesPerBang, n)).A, z, Bits.GetBit(DataToSave[BitOffset / 8], (int)(BitOffset % 8))); BitOffset++; z++; }
                    if (BitOffset == TotalBitsToWrite) { break; } //Speed.
                    n++;
                }

                switch (System.IO.Path.GetExtension(OutputImage).ToLower())
                {
                    case ".png": { Image.Save(OutputImage, new PngEncoder() { CompressionLevel = PngCompressionLevel.BestCompression, BitDepth = PngBitDepth.Bit8, TransparentColorMode = PngTransparentColorMode.Preserve, ColorType = PngColorType.RgbWithAlpha }); } break;
                    case ".tiff": { Image.Save(OutputImage, new TiffEncoder() { CompressionLevel = SixLabors.ImageSharp.Compression.Zlib.DeflateCompressionLevel.BestCompression, BitsPerPixel = TiffBitsPerPixel.Bit32 }); } break;
                    case ".tga": { Image.Save(OutputImage, new TgaEncoder() { Compression = TgaCompression.RunLength, BitsPerPixel = TgaBitsPerPixel.Pixel32 }); } break; //Only <= 32bit images.
                    case ".webp": { Image.Save(OutputImage, new WebpEncoder() { FileFormat = WebpFileFormatType.Lossless, Method = WebpEncodingMethod.BestQuality, Quality = 100, TransparentColorMode = WebpTransparentColorMode.Clear, UseAlphaCompression = false }); } break;  //Only <= 32bit images.
                    case ".bmp": { Image.Save(OutputImage, new BmpEncoder() { BitsPerPixel = BmpBitsPerPixel.Pixel32 }); } break; //Only <= 32bit images.
                    default: { throw new System.Exception("Unknown image format."); }
                }

                n = 0; while (n < BufferHandles.Length) { BufferHandles[n].Dispose(); n++; }; System.Runtime.InteropServices.Marshal.FreeHGlobal((nint)RawPixelArray);
                Image.Dispose();
            }

        }
        public static void ReadImage32(string InputImage, out byte[] Output, out string DataFileName)
        {
            if (InputImage == null) { throw new System.Exception("InputImage cannot be null."); }
            if (!System.IO.File.Exists(InputImage)) { throw new System.Exception("InputImage file doesn't exist."); }

            unsafe
            {
                SixLabors.ImageSharp.Image<Rgba32> Image = ConvertToImage32_4CH(InputImage);
                Memory<Rgba32>[] MemoryBang = Image.GetPixelMemoryGroup().ToArray(); var BufferHandles = new System.Buffers.MemoryHandle[MemoryBang.Length]; Pixel32** RawPixelArray = (Pixel32**)System.Runtime.InteropServices.Marshal.AllocHGlobal(sizeof(void*) * MemoryBang.Length);
                int e = 0, BytesPerBang = MemoryBang[0].Length; while (e < MemoryBang.Length) { BufferHandles[e] = MemoryBang[e].Pin(); RawPixelArray[e] = (Pixel32*)BufferHandles[e].Pointer; e++; }
                static Pixel32* GetPixel(Pixel32** PXArray, int BPB, int Offset) { return &PXArray[Offset / BPB][Offset - ((Offset / BPB) * BPB)]; }

                int SaveBits = 0, sBpp;
                Bits.SetBit(ref SaveBits, 0, Bits.GetBit((*GetPixel(RawPixelArray, BytesPerBang, 0)).B, 0)); Bits.SetBit(ref SaveBits, 1, Bits.GetBit((*GetPixel(RawPixelArray, BytesPerBang, 0)).G, 0));
                Bits.SetBit(ref SaveBits, 2, Bits.GetBit((*GetPixel(RawPixelArray, BytesPerBang, 0)).R, 0)); Bits.SetBit(ref SaveBits, 3, Bits.GetBit((*GetPixel(RawPixelArray, BytesPerBang, 0)).R, 1));
                SaveBits++; int BufferByteSize = GetTotalPossibleBufferSize4CH(Image.Width, Image.Height, SaveBits); sBpp = 4 * SaveBits;

                static void ReadInPixels(int TotalPixelCount, byte[] Buffer, Pixel32** PXArray, int BPB, int SaveBits)
                {
                    int n = 1, z; long BitOffset = 0, BufferTotalBits = Buffer.Length * 8; while (n < TotalPixelCount && BitOffset != BufferTotalBits) //+1 for the first save bits pixel.
                    {
                        z = 0; while (z < SaveBits && BitOffset != BufferTotalBits) { Bits.SetBit(ref Buffer[BitOffset / 8], (int)(BitOffset % 8), Bits.GetBit((*GetPixel(PXArray, BPB, n)).B, z)); z++; BitOffset++; }
                        z = 0; while (z < SaveBits && BitOffset != BufferTotalBits) { Bits.SetBit(ref Buffer[BitOffset / 8], (int)(BitOffset % 8), Bits.GetBit((*GetPixel(PXArray, BPB, n)).G, z)); z++; BitOffset++; }
                        z = 0; while (z < SaveBits && BitOffset != BufferTotalBits) { Bits.SetBit(ref Buffer[BitOffset / 8], (int)(BitOffset % 8), Bits.GetBit((*GetPixel(PXArray, BPB, n)).R, z)); z++; BitOffset++; }
                        z = 0; while (z < SaveBits && BitOffset != BufferTotalBits) { Bits.SetBit(ref Buffer[BitOffset / 8], (int)(BitOffset % 8), Bits.GetBit((*GetPixel(PXArray, BPB, n)).A, z)); z++; BitOffset++; }
                        n++;
                    }
                }

                byte[] ReadBuffer = new byte[BufferByteSize];
                ReadInPixels(Image.Width * Image.Height, ReadBuffer, RawPixelArray, BytesPerBang, SaveBits);

                int DataNameLength = System.BitConverter.ToInt32(ReadBuffer, 0); DataFileName = DataNameLength > 0 ? System.Text.Encoding.Unicode.GetString(ReadBuffer, 4, DataNameLength) : null;
                int DataLength = System.BitConverter.ToInt32(ReadBuffer, 4 + DataNameLength); Output = new byte[DataLength]; System.Buffer.BlockCopy(ReadBuffer, 4 + DataNameLength + 4, Output, 0, DataLength); ;

                int n = 0; while (n < BufferHandles.Length) { BufferHandles[n].Dispose(); n++; }; System.Runtime.InteropServices.Marshal.FreeHGlobal((nint)RawPixelArray);
                Image.Dispose();
            }
        }

        public static void SaveImage48(string InputImage, string OutputImage, byte[] Buffer, int Offset, int Count, int SaveBits, string DataFileName = null)
        {
            int sBpp = 3 * SaveBits; if (SaveBits > 16 || SaveBits < 1) { throw new System.Exception("Save bits cannot be below 1 or above 16."); }
            if (InputImage == null || OutputImage == null || Buffer == null) { throw new System.Exception("InputImage, OutputImage and Buffer cannot be null."); }
            if (!System.IO.File.Exists(InputImage)) { throw new System.Exception("InputImage file doesn't exist."); }

            unsafe
            {
                SixLabors.ImageSharp.Image<Rgb48> Image = ConvertToImage48_3CH(InputImage);
                Memory<Rgb48>[] MemoryBang = Image.GetPixelMemoryGroup().ToArray(); var BufferHandles = new System.Buffers.MemoryHandle[MemoryBang.Length]; Pixel48** RawPixelArray = (Pixel48**)System.Runtime.InteropServices.Marshal.AllocHGlobal(sizeof(void*) * MemoryBang.Length);
                int e = 0, BytesPerBang = MemoryBang[0].Length; while (e < MemoryBang.Length) { BufferHandles[e] = MemoryBang[e].Pin(); RawPixelArray[e] = (Pixel48*)BufferHandles[e].Pointer; e++; }
                static Pixel48* GetPixel(Pixel48** PXArray, int BPB, int Offset) { return &PXArray[Offset / BPB][Offset - ((Offset / BPB) * BPB)]; }

                Bits.SetBit(ref (*GetPixel(RawPixelArray, BytesPerBang, 0)).B, 0, Bits.GetBit(SaveBits - 1, 0)); Bits.SetBit(ref (*GetPixel(RawPixelArray, BytesPerBang, 0)).G, 0, Bits.GetBit(SaveBits - 1, 1));
                Bits.SetBit(ref (*GetPixel(RawPixelArray, BytesPerBang, 0)).R, 0, Bits.GetBit(SaveBits - 1, 2)); Bits.SetBit(ref (*GetPixel(RawPixelArray, BytesPerBang, 0)).R, 1, Bits.GetBit(SaveBits - 1, 3));

                int BufferByteSize = GetTotalPossibleBufferSize3CH(Image.Width, Image.Height, SaveBits);
                byte[] DataName = DataFileName != null && DataFileName != "" ? System.Text.Encoding.Unicode.GetBytes(System.IO.Path.GetFileName(DataFileName)) : new byte[0];
                if ((4 + DataName.Length + Count + 4) > BufferByteSize) { throw new System.Exception("Not enough space in image to save file with the given noise bits. (Available: " + (BufferByteSize / 1024f).ToString("0.00") + "kb < Needed: " + ((4 + DataName.Length + Count + 4) / 1024f).ToString("0.00") + "kb)"); }

                byte[] DataToSave = new byte[4 + DataName.Length + 4 + Count];
                System.Buffer.BlockCopy(System.BitConverter.GetBytes(DataName.Length), 0, DataToSave, 0, 4); //DataName.Length.
                System.Buffer.BlockCopy(DataName, 0, DataToSave, 4, DataName.Length); //DataName.
                System.Buffer.BlockCopy(System.BitConverter.GetBytes(Count), 0, DataToSave, 4 + DataName.Length, 4); //Count.
                System.Buffer.BlockCopy(Buffer, Offset, DataToSave, 4 + DataName.Length + 4, Count); //Actual Data.

                int DataEndOffset = DataToSave.Length;
                System.Array.Resize(ref DataToSave, BufferByteSize);

                int Cycles = (DataToSave.Length - DataEndOffset) / (Count / 3), Extra = (DataToSave.Length - DataEndOffset) - (Cycles * (Count / 3)), n = 0;
                while (n < Cycles) { System.Buffer.BlockCopy(Buffer, Offset + (Count / 3), DataToSave, DataEndOffset + ((Count / 3) * n), Count / 3); n++; } //Fill up the whole frame buffer, otherwise clearly can see where data ends.
                if (Extra != 0) { System.Buffer.BlockCopy(Buffer, Offset + (Count / 3), DataToSave, DataEndOffset + ((Count / 3) * n), Extra); }

                int TotalPixelCount = Image.Width * Image.Height, z; n = 1; //1 for the pixel we used for bit size/count.
                long BitOffset = 0, TotalBitsToWrite = BufferByteSize * 8;
                while (n < TotalPixelCount)
                {
                    z = 0; while (z < SaveBits) { if (BitOffset == TotalBitsToWrite) { break; }; Bits.SetBit(ref (*GetPixel(RawPixelArray, BytesPerBang, n)).B, z, Bits.GetBit(DataToSave[BitOffset / 8], (int)(BitOffset % 8))); BitOffset++; z++; }
                    z = 0; while (z < SaveBits) { if (BitOffset == TotalBitsToWrite) { break; }; Bits.SetBit(ref (*GetPixel(RawPixelArray, BytesPerBang, n)).G, z, Bits.GetBit(DataToSave[BitOffset / 8], (int)(BitOffset % 8))); BitOffset++; z++; }
                    z = 0; while (z < SaveBits) { if (BitOffset == TotalBitsToWrite) { break; }; Bits.SetBit(ref (*GetPixel(RawPixelArray, BytesPerBang, n)).R, z, Bits.GetBit(DataToSave[BitOffset / 8], (int)(BitOffset % 8))); BitOffset++; z++; }
                    if (BitOffset == TotalBitsToWrite) { break; } //Speed.
                    n++;
                }

                switch (System.IO.Path.GetExtension(OutputImage).ToLower())
                {
                    case ".png": { Image.Save(OutputImage, new PngEncoder() { CompressionLevel = PngCompressionLevel.BestCompression, BitDepth = PngBitDepth.Bit16, TransparentColorMode = PngTransparentColorMode.Clear, ColorType = PngColorType.Rgb }); } break;
                    //case ".tiff": { Image.Save(OutputImage, new TiffEncoder() { CompressionLevel = SixLabors.ImageSharp.Compression.Zlib.DeflateCompressionLevel.BestCompression, BitsPerPixel = TiffBitsPerPixel.Bit24 }); } break;
                    //case ".tga": { Image.Save(OutputImage, new TgaEncoder() { Compression = TgaCompression.RunLength, BitsPerPixel = TgaBitsPerPixel.Pixel32 }); } break; //Only <= 32bit images.
                    //case ".webp": { Image.Save(OutputImage, new WebpEncoder() { FileFormat = WebpFileFormatType.Lossless, Method = WebpEncodingMethod.BestQuality, Quality = 100, TransparentColorMode = WebpTransparentColorMode.Clear, UseAlphaCompression = false }); } break;  //Only <= 32bit images.
                    //case ".bmp": { Image.Save(OutputImage, new BmpEncoder() { BitsPerPixel = BmpBitsPerPixel.Pixel32 }); } break; //Only <= 32bit images.
                    default: { throw new System.Exception("Unknown image format."); }
                }

                n = 0; while (n < BufferHandles.Length) { BufferHandles[n].Dispose(); n++; }; System.Runtime.InteropServices.Marshal.FreeHGlobal((nint)RawPixelArray);
                Image.Dispose();
            }

        }
        public static void ReadImage48(string InputImage, out byte[] Output, out string DataFileName)
        {
            if (InputImage == null) { throw new System.Exception("InputImage cannot be null."); }
            if (!System.IO.File.Exists(InputImage)) { throw new System.Exception("InputImage file doesn't exist."); }

            unsafe
            {
                SixLabors.ImageSharp.Image<Rgb48> Image = ConvertToImage48_3CH(InputImage);
                Memory<Rgb48>[] MemoryBang = Image.GetPixelMemoryGroup().ToArray(); var BufferHandles = new System.Buffers.MemoryHandle[MemoryBang.Length]; Pixel48** RawPixelArray = (Pixel48**)System.Runtime.InteropServices.Marshal.AllocHGlobal(sizeof(void*) * MemoryBang.Length);
                int e = 0, BytesPerBang = MemoryBang[0].Length; while (e < MemoryBang.Length) { BufferHandles[e] = MemoryBang[e].Pin(); RawPixelArray[e] = (Pixel48*)BufferHandles[e].Pointer; e++; }
                static Pixel48* GetPixel(Pixel48** PXArray, int BPB, int Offset) { return &PXArray[Offset / BPB][Offset - ((Offset / BPB) * BPB)]; }

                int SaveBits = 0, sBpp;
                Bits.SetBit(ref SaveBits, 0, Bits.GetBit((*GetPixel(RawPixelArray, BytesPerBang, 0)).B, 0)); Bits.SetBit(ref SaveBits, 1, Bits.GetBit((*GetPixel(RawPixelArray, BytesPerBang, 0)).G, 0));
                Bits.SetBit(ref SaveBits, 2, Bits.GetBit((*GetPixel(RawPixelArray, BytesPerBang, 0)).R, 0)); Bits.SetBit(ref SaveBits, 3, Bits.GetBit((*GetPixel(RawPixelArray, BytesPerBang, 0)).R, 1));
                SaveBits++; int BufferByteSize = GetTotalPossibleBufferSize3CH(Image.Width, Image.Height, SaveBits); sBpp = 4 * SaveBits;

                static void ReadInPixels(int TotalPixelCount, byte[] Buffer, Pixel48** PXArray, int BPB, int SaveBits)
                {
                    int n = 1, z; long BitOffset = 0, BufferTotalBits = Buffer.Length * 8; while (n < TotalPixelCount && BitOffset != BufferTotalBits) //+1 for the first save bits pixel.
                    {
                        z = 0; while (z < SaveBits && BitOffset != BufferTotalBits) { Bits.SetBit(ref Buffer[BitOffset / 8], (int)(BitOffset % 8), Bits.GetBit((*GetPixel(PXArray, BPB, n)).B, z)); z++; BitOffset++; }
                        z = 0; while (z < SaveBits && BitOffset != BufferTotalBits) { Bits.SetBit(ref Buffer[BitOffset / 8], (int)(BitOffset % 8), Bits.GetBit((*GetPixel(PXArray, BPB, n)).G, z)); z++; BitOffset++; }
                        z = 0; while (z < SaveBits && BitOffset != BufferTotalBits) { Bits.SetBit(ref Buffer[BitOffset / 8], (int)(BitOffset % 8), Bits.GetBit((*GetPixel(PXArray, BPB, n)).R, z)); z++; BitOffset++; }
                        n++;
                    }
                }

                byte[] ReadBuffer = new byte[BufferByteSize];
                ReadInPixels(Image.Width * Image.Height, ReadBuffer, RawPixelArray, BytesPerBang, SaveBits);

                int DataNameLength = System.BitConverter.ToInt32(ReadBuffer, 0); DataFileName = DataNameLength > 0 ? System.Text.Encoding.Unicode.GetString(ReadBuffer, 4, DataNameLength) : null;
                int DataLength = System.BitConverter.ToInt32(ReadBuffer, 4 + DataNameLength); Output = new byte[DataLength]; System.Buffer.BlockCopy(ReadBuffer, 4 + DataNameLength + 4, Output, 0, DataLength); ;

                int n = 0; while (n < BufferHandles.Length) { BufferHandles[n].Dispose(); n++; }; System.Runtime.InteropServices.Marshal.FreeHGlobal((nint)RawPixelArray);
                Image.Dispose();
            }
        }

        public static void SaveImage64(string InputImage, string OutputImage, byte[] Buffer, int Offset, int Count, int SaveBits, string DataFileName = null)
        {
            int sBpp = 4 * SaveBits; if (SaveBits > 16 || SaveBits < 1) { throw new System.Exception("Save bits cannot be below 1 or above 16."); }
            if (InputImage == null || OutputImage == null || Buffer == null) { throw new System.Exception("InputImage, OutputImage and Buffer cannot be null."); }
            if (!System.IO.File.Exists(InputImage)) { throw new System.Exception("InputImage file doesn't exist."); }

            unsafe
            {
                SixLabors.ImageSharp.Image<Rgba64> Image = ConvertToImage64_4CH(InputImage);
                Memory<Rgba64>[] MemoryBang = Image.GetPixelMemoryGroup().ToArray(); var BufferHandles = new System.Buffers.MemoryHandle[MemoryBang.Length]; Pixel64** RawPixelArray = (Pixel64**)System.Runtime.InteropServices.Marshal.AllocHGlobal(sizeof(void*) * MemoryBang.Length);
                int e = 0, BytesPerBang = MemoryBang[0].Length; while (e < MemoryBang.Length) { BufferHandles[e] = MemoryBang[e].Pin(); RawPixelArray[e] = (Pixel64*)BufferHandles[e].Pointer; e++; }
                static Pixel64* GetPixel(Pixel64** PXArray, int BPB, int Offset) { return &PXArray[Offset / BPB][Offset - ((Offset / BPB) * BPB)]; }

                Bits.SetBit(ref (*GetPixel(RawPixelArray, BytesPerBang, 0)).B, 0, Bits.GetBit(SaveBits - 1, 0)); Bits.SetBit(ref (*GetPixel(RawPixelArray, BytesPerBang, 0)).G, 0, Bits.GetBit(SaveBits - 1, 1));
                Bits.SetBit(ref (*GetPixel(RawPixelArray, BytesPerBang, 0)).R, 0, Bits.GetBit(SaveBits - 1, 2)); Bits.SetBit(ref (*GetPixel(RawPixelArray, BytesPerBang, 0)).R, 1, Bits.GetBit(SaveBits - 1, 3));

                int BufferByteSize = GetTotalPossibleBufferSize4CH(Image.Width, Image.Height, SaveBits);
                byte[] DataName = DataFileName != null && DataFileName != "" ? System.Text.Encoding.Unicode.GetBytes(System.IO.Path.GetFileName(DataFileName)) : new byte[0];
                if ((4 + DataName.Length + Count + 4) > BufferByteSize) { throw new System.Exception("Not enough space in image to save file with the given noise bits. (Available: " + (BufferByteSize / 1024f).ToString("0.00") + "kb < Needed: " + ((4 + DataName.Length + Count + 4) / 1024f).ToString("0.00") + "kb)"); }

                byte[] DataToSave = new byte[4 + DataName.Length + 4 + Count];
                System.Buffer.BlockCopy(System.BitConverter.GetBytes(DataName.Length), 0, DataToSave, 0, 4); //DataName.Length.
                System.Buffer.BlockCopy(DataName, 0, DataToSave, 4, DataName.Length); //DataName.
                System.Buffer.BlockCopy(System.BitConverter.GetBytes(Count), 0, DataToSave, 4 + DataName.Length, 4); //Count.
                System.Buffer.BlockCopy(Buffer, Offset, DataToSave, 4 + DataName.Length + 4, Count); //Actual Data.

                int DataEndOffset = DataToSave.Length;
                System.Array.Resize(ref DataToSave, BufferByteSize);

                int Cycles = (DataToSave.Length - DataEndOffset) / (Count / 3), Extra = (DataToSave.Length - DataEndOffset) - (Cycles * (Count / 3)), n = 0;
                while (n < Cycles) { System.Buffer.BlockCopy(Buffer, Offset + (Count / 3), DataToSave, DataEndOffset + ((Count / 3) * n), Count / 3); n++; } //Fill up the whole frame buffer, otherwise clearly can see where data ends.
                if (Extra != 0) { System.Buffer.BlockCopy(Buffer, Offset + (Count / 3), DataToSave, DataEndOffset + ((Count / 3) * n), Extra); }

                int TotalPixelCount = Image.Width * Image.Height, z; n = 1; //1 for the pixel we used for bit size/count.
                long BitOffset = 0, TotalBitsToWrite = BufferByteSize * 8;
                while (n < TotalPixelCount)
                {
                    z = 0; while (z < SaveBits) { if (BitOffset == TotalBitsToWrite) { break; }; Bits.SetBit(ref (*GetPixel(RawPixelArray, BytesPerBang, n)).B, z, Bits.GetBit(DataToSave[BitOffset / 8], (int)(BitOffset % 8))); BitOffset++; z++; }
                    z = 0; while (z < SaveBits) { if (BitOffset == TotalBitsToWrite) { break; }; Bits.SetBit(ref (*GetPixel(RawPixelArray, BytesPerBang, n)).G, z, Bits.GetBit(DataToSave[BitOffset / 8], (int)(BitOffset % 8))); BitOffset++; z++; }
                    z = 0; while (z < SaveBits) { if (BitOffset == TotalBitsToWrite) { break; }; Bits.SetBit(ref (*GetPixel(RawPixelArray, BytesPerBang, n)).R, z, Bits.GetBit(DataToSave[BitOffset / 8], (int)(BitOffset % 8))); BitOffset++; z++; }
                    z = 0; while (z < SaveBits) { if (BitOffset == TotalBitsToWrite) { break; }; Bits.SetBit(ref (*GetPixel(RawPixelArray, BytesPerBang, n)).A, z, Bits.GetBit(DataToSave[BitOffset / 8], (int)(BitOffset % 8))); BitOffset++; z++; }
                    if (BitOffset == TotalBitsToWrite) { break; } //Speed.
                    n++;
                }

                switch (System.IO.Path.GetExtension(OutputImage).ToLower())
                {
                    case ".png": { Image.Save(OutputImage, new PngEncoder() { CompressionLevel = PngCompressionLevel.BestCompression, BitDepth = PngBitDepth.Bit16, TransparentColorMode = PngTransparentColorMode.Preserve, ColorType = PngColorType.RgbWithAlpha }); } break;
                    //case ".tiff": { Image.Save(OutputImage, new TiffEncoder() { CompressionLevel = SixLabors.ImageSharp.Compression.Zlib.DeflateCompressionLevel.BestCompression, BitsPerPixel = TiffBitsPerPixel.Bit32 }); } break;
                    //case ".tga": { Image.Save(OutputImage, new TgaEncoder() { Compression = TgaCompression.RunLength, BitsPerPixel = TgaBitsPerPixel.Pixel32 }); } break; //Only <= 32bit images.
                    //case ".webp": { Image.Save(OutputImage, new WebpEncoder() { FileFormat = WebpFileFormatType.Lossless, Method = WebpEncodingMethod.BestQuality, Quality = 100, TransparentColorMode = WebpTransparentColorMode.Clear, UseAlphaCompression = false }); } break;  //Only <= 32bit images.
                    //case ".bmp": { Image.Save(OutputImage, new BmpEncoder() { BitsPerPixel = BmpBitsPerPixel.Pixel32 }); } break; //Only <= 32bit images.
                    default: { throw new System.Exception("Unknown image format."); }
                }

                n = 0; while (n < BufferHandles.Length) { BufferHandles[n].Dispose(); n++; }; System.Runtime.InteropServices.Marshal.FreeHGlobal((nint)RawPixelArray);
                Image.Dispose();
            }

        }
        public static void ReadImage64(string InputImage, out byte[] Output, out string DataFileName)
        {
            if (InputImage == null) { throw new System.Exception("InputImage cannot be null."); }
            if (!System.IO.File.Exists(InputImage)) { throw new System.Exception("InputImage file doesn't exist."); }

            unsafe
            {
                SixLabors.ImageSharp.Image<Rgba64> Image = ConvertToImage64_4CH(InputImage);
                Memory<Rgba64>[] MemoryBang = Image.GetPixelMemoryGroup().ToArray(); var BufferHandles = new System.Buffers.MemoryHandle[MemoryBang.Length]; Pixel64** RawPixelArray = (Pixel64**)System.Runtime.InteropServices.Marshal.AllocHGlobal(sizeof(void*) * MemoryBang.Length);
                int e = 0, BytesPerBang = MemoryBang[0].Length; while (e < MemoryBang.Length) { BufferHandles[e] = MemoryBang[e].Pin(); RawPixelArray[e] = (Pixel64*)BufferHandles[e].Pointer; e++; }
                static Pixel64* GetPixel(Pixel64** PXArray, int BPB, int Offset) { return &PXArray[Offset / BPB][Offset - ((Offset / BPB) * BPB)]; }

                int SaveBits = 0, sBpp;
                Bits.SetBit(ref SaveBits, 0, Bits.GetBit((*GetPixel(RawPixelArray, BytesPerBang, 0)).B, 0)); Bits.SetBit(ref SaveBits, 1, Bits.GetBit((*GetPixel(RawPixelArray, BytesPerBang, 0)).G, 0));
                Bits.SetBit(ref SaveBits, 2, Bits.GetBit((*GetPixel(RawPixelArray, BytesPerBang, 0)).R, 0)); Bits.SetBit(ref SaveBits, 3, Bits.GetBit((*GetPixel(RawPixelArray, BytesPerBang, 0)).R, 1));
                SaveBits++; int BufferByteSize = GetTotalPossibleBufferSize4CH(Image.Width, Image.Height, SaveBits); sBpp = 4 * SaveBits;

                static void ReadInPixels(int TotalPixelCount, byte[] Buffer, Pixel64** PXArray, int BPB, int SaveBits)
                {
                    int n = 1, z; long BitOffset = 0, BufferTotalBits = Buffer.Length * 8; while (n < TotalPixelCount && BitOffset != BufferTotalBits) //+1 for the first save bits pixel.
                    {
                        z = 0; while (z < SaveBits && BitOffset != BufferTotalBits) { Bits.SetBit(ref Buffer[BitOffset / 8], (int)(BitOffset % 8), Bits.GetBit((*GetPixel(PXArray, BPB, n)).B, z)); z++; BitOffset++; }
                        z = 0; while (z < SaveBits && BitOffset != BufferTotalBits) { Bits.SetBit(ref Buffer[BitOffset / 8], (int)(BitOffset % 8), Bits.GetBit((*GetPixel(PXArray, BPB, n)).G, z)); z++; BitOffset++; }
                        z = 0; while (z < SaveBits && BitOffset != BufferTotalBits) { Bits.SetBit(ref Buffer[BitOffset / 8], (int)(BitOffset % 8), Bits.GetBit((*GetPixel(PXArray, BPB, n)).R, z)); z++; BitOffset++; }
                        z = 0; while (z < SaveBits && BitOffset != BufferTotalBits) { Bits.SetBit(ref Buffer[BitOffset / 8], (int)(BitOffset % 8), Bits.GetBit((*GetPixel(PXArray, BPB, n)).A, z)); z++; BitOffset++; }
                        n++;
                    }
                }

                byte[] ReadBuffer = new byte[BufferByteSize];
                ReadInPixels(Image.Width * Image.Height, ReadBuffer, RawPixelArray, BytesPerBang, SaveBits);

                int DataNameLength = System.BitConverter.ToInt32(ReadBuffer, 0); DataFileName = DataNameLength > 0 ? System.Text.Encoding.Unicode.GetString(ReadBuffer, 4, DataNameLength) : null;
                int DataLength = System.BitConverter.ToInt32(ReadBuffer, 4 + DataNameLength); Output = new byte[DataLength]; System.Buffer.BlockCopy(ReadBuffer, 4 + DataNameLength + 4, Output, 0, DataLength); ;

                int n = 0; while (n < BufferHandles.Length) { BufferHandles[n].Dispose(); n++; }; System.Runtime.InteropServices.Marshal.FreeHGlobal((nint)RawPixelArray);
                Image.Dispose();
            }
        }

        public static long GetAvailableSpace(string InputImage, bool NoAlpha, int SaveBits, bool Encrypted, string DataName = null)
        {
            int BufferByteSize = 0;
            Image Input = Image.Load(InputImage);
            if (NoAlpha) { BufferByteSize = GetTotalPossibleBufferSize3CH(Input.Width, Input.Height, SaveBits); } else { BufferByteSize = GetTotalPossibleBufferSize4CH(Input.Width, Input.Height, SaveBits); }
            Input.Dispose();
            if (!Encrypted)
            {
                return BufferByteSize - (4 + System.Text.Encoding.Unicode.GetBytes(DataName ?? "").Length + 4);
            }
            else
            {
                return BufferByteSize - (4 + System.Text.Encoding.Unicode.GetBytes(DataName ?? "").Length + 4) - (4 + (System.Security.Cryptography.AesGcm.NonceByteSizes.MaxSize) + 4 + (System.Security.Cryptography.AesGcm.TagByteSizes.MaxSize));
            }
        }

        public static int GetPerfectSaveBits(string InputImage, bool NoAlpha, bool Encrypted, int DataSize, string DataName = null, int MaxSaveBits = 8)
        {
            int n = 0; long Size = 0;
            while (n < MaxSaveBits) { Size = GetAvailableSpace(InputImage, NoAlpha, n + 1, Encrypted, DataName); if (Size >= DataSize) { return n + 1; }; n++;  }
            return -1;
        }

        public static int GetFilePixelBits(string InputImage)
        {
            Image Input = Image.Load(InputImage);
            int Bits = Input.PixelType.BitsPerPixel;
            Input.Dispose();
            return Bits;
        }



    }


}