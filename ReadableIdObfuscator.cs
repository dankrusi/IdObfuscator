using System;
using System.Text;

namespace IdObfuscator {

    public class ReadableIdObfuscator {

        public static string GetStringFromInt(int id, bool insertDashes) {
            // Hex chars:   1 2 3 4 5 6 7 8 9 0 A B C D E F
            // replacemtns: H       Y   N     V
            string strongId = StrongIdObfuscator.EncodeIdToBase64String(id); // Now we have somthing like 'td_BBw'
            string hexId = HexEncode(strongId); // Now we have '74645F424277'
            string readableId = hexId
                .Replace('1', 'H')
                .Replace('7', 'N')
                .Replace('0', 'V')
                .Replace('5', 'Y');
            if(insertDashes) readableId = readableId.Insert(8, "-").Insert(4, "-");
            return readableId;
        }

        public static int GetIntFromString(string id) {
            string hexId = id
                .Replace('H', '1')
                .Replace('N', '7')
                .Replace('V', '0')
                .Replace('Y', '5')
                .Replace("-", "");
            string strongId = HexDecode(hexId);
            int intId = StrongIdObfuscator.GetIdFromObfuscatedBase64IdOrRegularId(strongId);
            return intId;
        }
        
        public static string HexEncode(string asciiString)
        {
            string hex = "";
            foreach (char c in asciiString)
            {
                int tmp = c;
                hex += String.Format("{0:X2}", (uint)System.Convert.ToUInt32(tmp.ToString()));
            }
            return hex;
        }
     
        public static string HexDecode(string HexValue)
        {
            string StrValue = "";
            while (HexValue.Length > 0)
            {
                StrValue += System.Convert.ToChar(System.Convert.ToUInt32(HexValue.Substring(0, 2), 16)).ToString();
                HexValue = HexValue.Substring(2, HexValue.Length - 2);
            }
            return StrValue;
        }

    }
}

