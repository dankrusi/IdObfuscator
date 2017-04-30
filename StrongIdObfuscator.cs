using System;

namespace IdObfuscator
{
    
    public class StrongIdObfuscator
    {
        // Private variables. Default secret numbers are provided so that the class 
        // works out-of-the-box for those that aren't interested in security but more in reformatting
        private static int _secretPrime = ; // Big prime number (10 digits)
        private static int _secretPrimeInverse = ; // Big integer number so that (_secretPrime * _secretPrimeInverse) & _maxId == 1
        private static int _secretRandomXOR = ; // Random big integer number, not bigger than _maxId
        private static int _maxId = int.MaxValue; // The maximum system integer value
        private static char _base64PlusChar = '+';
        private static char _base64SlashChar = '.';
        private static bool _stripBase64Padding = true;


        /// <summary>
        /// Sets the secret prime number. This should be a really big prime number to improve robustnuss (10 digits).
        /// This number should be kept secret.
        /// </summary>
        public static int SecretPrime {
            set {
                _secretPrime = value;
            }
        }

        /// <summary>
        /// Sets the secret prime inverse number. You can use the CalculatePrimeInverse helper method to calculate this number.
        /// This number should be kept secret.
        /// </summary>
        public static int SecretPrimeInverse {
            set {
                _secretPrimeInverse = value;
            }
        }

        /// <summary>
        /// Sets the secret random number. This improves robustness. It should not be larger than _maxId (int.MaxValue).
        /// This number should be kept secret.
        /// </summary>
        public static int SecretRandomXOR {
            set {
                _secretRandomXOR = value;
            }
        }

        /// <summary>
        /// Gets or sets the plus sign character.
        /// Default is '+'.
        /// </summary>
        public static char Base64PlusChar {
            get {
                return _base64PlusChar;
            }
            set {
                _base64PlusChar = value;
            }
        }

        /// <summary>
        /// Gets or sets the slash character.
        /// Default is '/'.
        /// </summary>
        static public char Base64SlashChar {
            get {
                return _base64SlashChar;
            }
            set {
                _base64SlashChar = value;
            }
        }







        public static int EncodeIdToInt(int id) {
            return ((id * _secretPrime) & _maxId) ^ _secretRandomXOR;
        }

        public static int DecodeIdFromInt(int obfuscatedId) {
            return ((obfuscatedId ^ _secretRandomXOR) * _secretPrimeInverse) & _maxId;
        }

        public static string EncodeIdToBase64String(int id) {
            return ConvertToBase64(EncodeIdToInt(id));
        }

        public static int DecodeIdFromBase64String(string obfuscatedId) {
            return DecodeIdFromInt(ConvertFromBase64ToInt(obfuscatedId));
        }

        public static int GetIdFromObfuscatedBase64IdOrRegularId(string id) {
            int number;
            if (Int32.TryParse(id, out number)) return number;
            else return DecodeIdFromBase64String(id);
        }



        public static string CreateRollingShortID() {
            // Note: this is obviously not collision free
            return ConvertToBase64((int)DateTime.Now.Ticks).ToUpper().Replace(_base64PlusChar, 'X').Replace(_base64SlashChar, 'X');
        }



        public static string ConvertToBase64(int data) {
            byte[] toEncodeAsBytes = BitConverter.GetBytes(data);
            if (_stripBase64Padding) return ConvertToBase64(toEncodeAsBytes).Substring(0, 6); // Trim out the padding from base64 conversion
            else return ConvertToBase64(toEncodeAsBytes);
        }

        public static string ConvertToBase64(byte[] data) {
            int length = data == null ? 0 : data.Length;
            if (length == 0)
                return String.Empty;

            int padding = length % 3;
            if (padding > 0)
                padding = 3 - padding;

            int blocks = (length - 1) / 3 + 1;

            char[] s = new char[blocks * 4];

            for (int i = 0; i < blocks; i++) {
                bool finalBlock = i == blocks - 1;
                bool pad2 = false;
                bool pad1 = false;
                if (finalBlock) {
                    pad2 = padding == 2;
                    pad1 = padding > 0;
                }

                int index = i * 3;
                byte b1 = data[index];
                byte b2 = pad2 ? (byte)0 : data[index + 1];
                byte b3 = pad1 ? (byte)0 : data[index + 2];

                byte temp1 = (byte)((b1 & 0xFC) >> 2);

                byte temp = (byte)((b1 & 0x03) << 4);
                byte temp2 = (byte)((b2 & 0xF0) >> 4);
                temp2 += temp;

                temp = (byte)((b2 & 0x0F) << 2);
                byte temp3 = (byte)((b3 & 0xC0) >> 6);
                temp3 += temp;

                byte temp4 = (byte)(b3 & 0x3F);

                index = i * 4;
                s[index] = _sixBitToChar(temp1);
                s[index + 1] = _sixBitToChar(temp2);
                s[index + 2] = pad2 ? '=' : _sixBitToChar(temp3);
                s[index + 3] = pad1 ? '=' : _sixBitToChar(temp4);
            }

            return new string(s);
        }

        private static char _sixBitToChar(byte b) {
            char c;
            if (b < 26) {
                c = (char)((int)b + (int)'A');
            } else if (b < 52) {
                c = (char)((int)b - 26 + (int)'a');
            } else if (b < 62) {
                c = (char)((int)b - 52 + (int)'0');
            } else if (b == 62) {
                c = _base64PlusChar;
            } else {
                c = _base64SlashChar;
            }
            return c;
        }


        static public int ConvertFromBase64ToInt(string s) {
            byte[] encodedDataAsBytes = _stripBase64Padding ? ConvertFromBase64ToByteArray(s + "==") : ConvertFromBase64ToByteArray(s); // Append the padding for base64 conversion, if we are using stripped mode
            return BitConverter.ToInt32(encodedDataAsBytes, 0);
        }

        public static byte[] ConvertFromBase64ToByteArray(string s) {
            int length = s == null ? 0 : s.Length;
            if (length == 0)
                return new byte[0];

            int padding = 0;
            if (length > 2 && s[length - 2] == '=')
                padding = 2;
            else if (length > 1 && s[length - 1] == '=')
                padding = 1;

            int blocks = (length - 1) / 4 + 1;
            int bytes = blocks * 3;

            byte[] data = new byte[bytes - padding];

            for (int i = 0; i < blocks; i++) {
                bool finalBlock = i == blocks - 1;
                bool pad2 = false;
                bool pad1 = false;
                if (finalBlock) {
                    pad2 = padding == 2;
                    pad1 = padding > 0;
                }

                int index = i * 4;
                byte temp1 = _charToSixBit(s[index]);
                byte temp2 = _charToSixBit(s[index + 1]);
                byte temp3 = _charToSixBit(s[index + 2]);
                byte temp4 = _charToSixBit(s[index + 3]);

                byte b = (byte)(temp1 << 2);
                byte b1 = (byte)((temp2 & 0x30) >> 4);
                b1 += b;

                b = (byte)((temp2 & 0x0F) << 4);
                byte b2 = (byte)((temp3 & 0x3C) >> 2);
                b2 += b;

                b = (byte)((temp3 & 0x03) << 6);
                byte b3 = temp4;
                b3 += b;

                index = i * 3;
                data[index] = b1;
                if (!pad2)
                    data[index + 1] = b2;
                if (!pad1)
                    data[index + 2] = b3;
            }

            return data;
        }

        private static byte _charToSixBit(char c) {
            byte b;
            if (c >= 'A' && c <= 'Z') {
                b = (byte)((int)c - (int)'A');
            } else if (c >= 'a' && c <= 'z') {
                b = (byte)((int)c - (int)'a' + 26);
            } else if (c >= '0' && c <= '9') {
                b = (byte)((int)c - (int)'0' + 52);
            } else if (c == _base64PlusChar) {
                b = (byte)62;
            } else {
                b = (byte)63;
            }
            return b;
        }





        public static int CalculatePrimeInverse(int prime) {
            return (int)_bruteForceModInverse((uint)prime, (uint)_maxId + 1);
        }

        private static uint _bruteForceModInverse(uint a, uint m) {
            a %= m;
            for (uint x = 1; x < m; x++) {
                if ((a * x) % m == 1) return x;
            }
            return 0;
        }

    }
}

