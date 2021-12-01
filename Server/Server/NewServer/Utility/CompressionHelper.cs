using ICSharpCode.SharpZipLib.GZip;
using System;
using System.IO;

/// <summary>
/// 压缩解压缩辅助器。
/// </summary>
public static class CompressionHelper
{
    private const int CachedBytesLength = 0x1000;
    private static readonly byte[] m_CachedBytes = new byte[CachedBytesLength];

    /// <summary>
    /// 压缩数据。
    /// </summary>
    /// <param name="bytes">要压缩的数据的二进制流。</param>
    /// <returns>压缩后的数据的二进制流。</returns>
    public static byte[] Compress(byte[] bytes)
    {
        return Compress(bytes, 0, bytes.Length);
    }

    /// <summary>
    /// 压缩数据。
    /// </summary>
    /// <param name="bytes">要压缩的数据的二进制流。</param>
    /// <param name="compressedStream">压缩后的数据的二进制流。</param>
    /// <returns>是否压缩数据成功。</returns>
    public static bool Compress(byte[] bytes, Stream compressedStream)
    {
        return Compress(bytes, 0, bytes.Length, compressedStream);
    }

    /// <summary>
    /// 压缩数据。
    /// </summary>
    /// <param name="bytes">要压缩的数据的二进制流。</param>
    /// <param name="offset">要压缩的数据的二进制流的偏移。</param>
    /// <param name="length">要压缩的数据的二进制流的长度。</param>
    /// <returns>压缩后的数据的二进制流。</returns>
    public static byte[] Compress(byte[] bytes, int offset, int length)
    {
        using (MemoryStream compressedStream = new MemoryStream())
        {
            if (Compress(bytes, offset, length, compressedStream))
            {
                return compressedStream.ToArray();
            }
            else
            {
                return null;
            }
        }
    }

    /// <summary>
    /// 压缩数据。
    /// </summary>
    /// <param name="stream">要压缩的数据的二进制流。</param>
    /// <returns>压缩后的数据的二进制流。</returns>
    public static byte[] Compress(Stream stream)
    {
        using (MemoryStream compressedStream = new MemoryStream())
        {
            if (Compress(stream, compressedStream))
            {
                return compressedStream.ToArray();
            }
            else
            {
                return null;
            }
        }
    }


    /// <summary>
    /// 解压缩数据。
    /// </summary>
    /// <param name="bytes">要解压缩的数据的二进制流。</param>
    /// <returns>解压缩后的数据的二进制流。</returns>
    public static byte[] Decompress(byte[] bytes)
    {
        if (bytes == null)
        {
            throw new Exception("Bytes is invalid.");
        }

        return Decompress(bytes, 0, bytes.Length);
    }

    /// <summary>
    /// 解压缩数据。
    /// </summary>
    /// <param name="bytes">要解压缩的数据的二进制流。</param>
    /// <param name="decompressedStream">解压缩后的数据的二进制流。</param>
    /// <returns>是否解压缩数据成功。</returns>
    public static bool Decompress(byte[] bytes, Stream decompressedStream)
    {
        if (bytes == null)
        {
            throw new Exception("Bytes is invalid.");
        }

        return Decompress(bytes, 0, bytes.Length, decompressedStream);
    }

    /// <summary>
    /// 解压缩数据。
    /// </summary>
    /// <param name="bytes">要解压缩的数据的二进制流。</param>
    /// <param name="offset">要解压缩的数据的二进制流的偏移。</param>
    /// <param name="length">要解压缩的数据的二进制流的长度。</param>
    /// <returns>解压缩后的数据的二进制流。</returns>
    public static byte[] Decompress(byte[] bytes, int offset, int length)
    {
        using (MemoryStream decompressedStream = new MemoryStream())
        {
            if (Decompress(bytes, offset, length, decompressedStream))
            {
                return decompressedStream.ToArray();
            }
            else
            {
                return null;
            }
        }
    }

    /// <summary>
    /// 解压缩数据。
    /// </summary>
    /// <param name="stream">要解压缩的数据的二进制流。</param>
    /// <returns>是否解压缩数据成功。</returns>
    public static byte[] Decompress(Stream stream)
    {
        using (MemoryStream decompressedStream = new MemoryStream())
        {
            if (Decompress(stream, decompressedStream))
            {
                return decompressedStream.ToArray();
            }
            else
            {
                return null;
            }
        }
    }

    #region  内部的

    private static bool Compress(byte[] bytes, int offset, int length, Stream compressedStream)
    {
        if (bytes == null)
        {
            return false;
        }

        if (offset < 0 || length < 0 || offset + length > bytes.Length)
        {
            return false;
        }

        if (compressedStream == null)
        {
            return false;
        }

        try
        {
            GZipOutputStream gZipOutputStream = new GZipOutputStream(compressedStream);
            gZipOutputStream.Write(bytes, offset, length);
            gZipOutputStream.Finish();
            ProcessHeader(compressedStream);
            return true;
        }
        catch
        {
            return false;
        }
    }


    private static bool Compress(Stream stream, Stream compressedStream)
    {
        if (stream == null)
        {
            return false;
        }

        if (compressedStream == null)
        {
            return false;
        }

        try
        {
            GZipOutputStream gZipOutputStream = new GZipOutputStream(compressedStream);
            int bytesRead = 0;
            while ((bytesRead = stream.Read(m_CachedBytes, 0, CachedBytesLength)) > 0)
            {
                gZipOutputStream.Write(m_CachedBytes, 0, bytesRead);
            }

            gZipOutputStream.Finish();
            ProcessHeader(compressedStream);
            return true;
        }
        catch
        {
            return false;
        }
        finally
        {
            Array.Clear(m_CachedBytes, 0, CachedBytesLength);
        }
    }

    
    private static bool Decompress(byte[] bytes, int offset, int length, Stream decompressedStream)
    {
        if (bytes == null)
        {
            return false;
        }

        if (offset < 0 || length < 0 || offset + length > bytes.Length)
        {
            return false;
        }

        if (decompressedStream == null)
        {
            return false;
        }

        MemoryStream memoryStream = null;
        try
        {
            memoryStream = new MemoryStream(bytes, offset, length, false);
            using (GZipInputStream gZipInputStream = new GZipInputStream(memoryStream))
            {
                int bytesRead = 0;
                while ((bytesRead = gZipInputStream.Read(m_CachedBytes, 0, CachedBytesLength)) > 0)
                {
                    decompressedStream.Write(m_CachedBytes, 0, bytesRead);
                }
            }

            return true;
        }
        catch
        {
            return false;
        }
        finally
        {
            if (memoryStream != null)
            {
                memoryStream.Dispose();
                memoryStream = null;
            }

            Array.Clear(m_CachedBytes, 0, CachedBytesLength);
        }
    }

    private static bool Decompress(Stream stream, Stream decompressedStream)
    {
        if (stream == null)
        {
            return false;
        }

        if (decompressedStream == null)
        {
            return false;
        }

        try
        {
            GZipInputStream gZipInputStream = new GZipInputStream(stream);
            int bytesRead = 0;
            while ((bytesRead = gZipInputStream.Read(m_CachedBytes, 0, CachedBytesLength)) > 0)
            {
                decompressedStream.Write(m_CachedBytes, 0, bytesRead);
            }

            return true;
        }
        catch
        {
            return false;
        }
        finally
        {
            Array.Clear(m_CachedBytes, 0, CachedBytesLength);
        }
    }


    private static void ProcessHeader(Stream compressedStream)
    {
        if (compressedStream.Length >= 8L)
        {
            long current = compressedStream.Position;
            compressedStream.Position = 4L;
            compressedStream.WriteByte(25);
            compressedStream.WriteByte(134);
            compressedStream.WriteByte(2);
            compressedStream.WriteByte(32);
            compressedStream.Position = current;
        }
    }
    #endregion
}
