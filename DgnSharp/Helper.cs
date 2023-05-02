using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace DgnSharp
{
    public class Helper
    {
        public static int GetWordFromByteIndex(byte[] array, int bytePosition)
        {
            if ((bytePosition+1) >= array.Length)
            {
                return int.MinValue;
            }
            return BitConverter.ToInt16(array, bytePosition);
        }

        //Dados são little-endian (B0 B1)
        public static int GetWord(byte[] array, int wordIndex)
        {
            int lsbIndex = (wordIndex - 1) * 2;
            return GetWordFromByteIndex(array, lsbIndex);
        }

        //Dados são little-endian (B0 B1)
        public static void SetWord(ref byte[] array, int wordNumber, int value)
        {
            int lsbIndex = (wordNumber - 1) * 2;
            
            if (array.Length < (lsbIndex + 2))
            {
                Array.Resize(ref array, lsbIndex + 2);                
            }
            
            var conversion = BitConverter.GetBytes((short)value);
            conversion.CopyTo(array, lsbIndex);            
        }

        public static void SetWordFromByteIndex(ref byte[] array, int byteIndex, int value)
        {
            if (array.Length < (byteIndex + 2))
            {
                Array.Resize(ref array, byteIndex + 2);
            }
            var conversion = BitConverter.GetBytes((short)value);
            conversion.CopyTo(array, byteIndex);
        }

        //Segundo a documentação,
        //Dados são armazenados com a ordem dos bytes middle-endian. 
        //(B2 B3 B0 B1), sendo B0 o byte menos significativo 
        //Mas parece que alguns são armazenados como B0 B1 B2 B3
        public static int GetLongFromByteIndex(byte[] array, int firstByteIndex)
        {
            if (array.Length < firstByteIndex + 4)
            {
                return 0;
            }
            return BitConverter.ToInt32(array, firstByteIndex);                        
        }

        public static int GetLongFromByteIndexMid(byte[] array, int firstByteIndex)
        {
            if (array.Length < firstByteIndex + 4)
            {
                return 0;
            }
            byte[] cArr = new byte[]
            {
                array[firstByteIndex+2],
                array[firstByteIndex+3],
                array[firstByteIndex+0],
                array[firstByteIndex+1],
            };
            return BitConverter.ToInt32(cArr, 0);
        }

        public static void SetLongFromByteIndexMid(ref byte[] array, int firstByteIndex, int value)
        {
            var tempArr = BitConverter.GetBytes(value);
            if (array.Length < firstByteIndex + 4)
            {
                Array.Resize(ref array, firstByteIndex + 4);
            }
            array[firstByteIndex + 2] = tempArr[0];
            array[firstByteIndex + 3] = tempArr[1];
            array[firstByteIndex + 0] = tempArr[2];
            array[firstByteIndex + 1] = tempArr[3];
            
        }
        public static void SetLongMid(ref byte[] array, int firstWordNumber, int value)
        {
            int firstByteIndex = (firstWordNumber - 1) * 2;
            SetLongFromByteIndexMid(ref array, firstByteIndex, value);
        }

        public static int GetLongMid(byte[] array, int firstWordIndex)
        {
            return GetLongFromByteIndexMid(array, (firstWordIndex - 1) * 2);
        }

        //Dados são armazenados com a ordem dos bytes middle-endian. 
        //(B2 B3 B0 B1), sendo B0 o byte menos significativo 
        public static int GetLong(byte[] array, int firstWordIndex)
        {
            return GetLongFromByteIndex(array, (firstWordIndex - 1) * 2);
        }

        //Dados são armazenados com a ordem dos bytes middle-endian. 
        //(B2 B3 B0 B1), sendo B0 o byte menos significativo
        public static void SetLong(ref byte[] array, int firstWordNumber, int value)
        {            
            SetWord(ref array, firstWordNumber + 1, (int)(value >> 16));
            SetWord(ref array, firstWordNumber, (int)(value & 0xFFFF));
        }


        public static void SetLongFromByteIndex(ref byte[] array, int firstByteIndex, int value)
        {
            SetWordFromByteIndex(ref array, firstByteIndex + 2, (int)(value >> 16));
            SetWordFromByteIndex(ref array, firstByteIndex, (int)(value & 0xFFFF));
        }
        public static double GetDouble(byte[] array, int firstWordIndex)
        {
            return GetDoubleFromByteIndex(array, (firstWordIndex - 1) * 2);
        }

        public static double GetDoubleFromByteIndex(byte[] array, int firstByteIndex)
        {
            Span<byte> sp = new Span<byte>(array, firstByteIndex, 8);
            byte[] b = new byte[8];
            b[0] = sp[6];
            b[1] = sp[7];
            b[2] = sp[4];
            b[3] = sp[5];
            b[4] = sp[2];
            b[5] = sp[3];
            b[6] = sp[0];
            b[7] = sp[1];

            int sign = b[7] >> 7;
            int exponent = ((b[7] << 1) & 0xFF) + (b[6]>>7);
            ulong fraction = (ulong)(b[6] | 0x80) << (6 * 8);
            for (int i = 0; i <= 5; i++)
            {
                fraction += ((ulong)b[i] << (i * 8));
            }
            double f = 0;
            for (int i = 1; i <= 56; i++)
            {
                f += ((fraction >> (56 - i)) & 0x1) * Math.Pow(2, -i);
            }
            
            double d = Math.Pow(-1, sign) * Math.Pow(2, exponent - 128) * f;
            
            return d;
        }

        public static void SetDoubleFromByteIndex(ref byte[] array, int firstByteIndex, double value)
        {
            long input = BitConverter.ToInt64(BitConverter.GetBytes(value));
            long sign = input >> 63;
            long exponent = (input >> 52) & 0x7FF;
            long fraction = input & 0xFFFFFFFFFFFFF;

            exponent = exponent - 1023 + 129;
            fraction = fraction << 3;

            long output = sign << 63;
            output |= (exponent & 0xFF) << (63 - 8);
            output |= fraction;

            byte[] ret = new byte[8];
            ret[0] = (byte)((output >> (6*8)) & 0xFF);
            ret[1] = (byte)((output >> (7*8)) & 0xFF);
            ret[2] = (byte)((output >> (4*8)) & 0xFF);
            ret[3] = (byte)((output >> (5*8)) & 0xFF);
            ret[4] = (byte)((output >> (2*8)) & 0xFF);
            ret[5] = (byte)((output >> (3*8)) & 0xFF);
            ret[6] = (byte)(output & 0xFF);
            ret[7] = (byte)((output >> 8) & 0xFF);

            ret.CopyTo(array, firstByteIndex);

        }

        public static void SetDouble(ref byte[] array, int firstWordIndex, double value)
        {
            int byteIndex = (firstWordIndex - 1) * 2;
            SetDoubleFromByteIndex(ref array, byteIndex, value);
        }

    }
}
