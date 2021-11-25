using GameFramework;

/// <summary>
/// xor 异或
/// </summary>
public static class SecurityUtil
{
    /// <summary>
    /// 异或因子
    /// </summary>
    private static readonly byte[] s_XorScale = new byte[] { 45, 66, 38, 55, 23, 254, 9, 165, 90, 19, 41, 45, 201, 58, 55, 37, 254, 185, 165, 169, 19, 171 };//.data文件的xor加解密因子

    /// <summary>
    /// 对数组进行异或
    /// </summary>
    /// <param name="buffer"></param>
    /// <returns></returns>
    public static byte[] Xor(byte[] buffer)
    {
        //------------------
        //第3步：xor解密
        //------------------
        int iScaleLen = s_XorScale.Length;
        for (int i = 0; i < buffer.Length; i++)
        {
            buffer[i] = (byte)(buffer[i] ^ s_XorScale[i % iScaleLen]);
        }
        return buffer;
    }
}