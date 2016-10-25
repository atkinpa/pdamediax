using System;
using System.Text;
using System.IO;

namespace pdaMediaX.Util
{
    public class pdamxCrypter
    {
        public pdamxCrypter()
        {
        }
        private String DecryptData(byte[] _bDataIn)
        {
            String sDataOut = "";
            ASCIIEncoding asciiAncoding = new ASCIIEncoding();

            byte[] bWorkArea = new byte[_bDataIn.Length];
            int nTotalBits = _bDataIn.Length * 8;
            int[] nBitsTable = { 128, 64, 32, 16, 8, 4, 2, 1 };

            for (int nDataCnt = 0, nWorkAreaCnt = 0; nDataCnt < nTotalBits; nDataCnt++)
            {
                // Calculate what byte and bit the bit is in...
                int nByteInPos = Math.Abs((nTotalBits - nDataCnt) / 8)
                              + ((Math.Abs((nTotalBits - nDataCnt) / 8) * 8) < (nTotalBits - nDataCnt) ? 1 : 0);
                int nBitAt = Math.Abs((nTotalBits - nDataCnt) - (nByteInPos * 8));
                bWorkArea[nWorkAreaCnt] = (byte)(bWorkArea[nWorkAreaCnt] + ((_bDataIn[nByteInPos - 1] & nBitsTable[nBitAt]) == 0 ? nBitsTable[7 - nBitAt] : 0));
                if (nBitAt == 7)
                    nWorkAreaCnt++;
            }
            for (int dataCnt = 0; dataCnt < bWorkArea.Length; dataCnt++)
                sDataOut = sDataOut + asciiAncoding.GetString(bWorkArea, dataCnt, 1);

            return (sDataOut);
        }
        public String DecryptFile(String _sFileName)
        {
            String sReturnData;

            FileStream fsStream = new FileStream(_sFileName, FileMode.Open);
            BinaryReader brReader = new BinaryReader(fsStream);

            sReturnData = DecryptData(brReader.ReadBytes((int) brReader.BaseStream.Length));
            brReader.Close();
            fsStream.Close();
            return (sReturnData);
        }
        public String DecryptText(byte[] _bDataIn)
        {
            return (DecryptData(_bDataIn));
        }
        private byte[] EncryptData(byte[] _bDataIn)
        {
            byte[] bDataOut = new byte[_bDataIn.Length];
            int nTotalBits = _bDataIn.Length * 8;
            int[] nBitsTable = { 128, 64, 32, 16, 8, 4, 2, 1 };

            for (int nDataCnt = 0, nDataOutCnt = 0; nDataCnt < nTotalBits; nDataCnt++)
            {
                // Calculate what byte and bit the bit is in...
                int nByteInPos = Math.Abs((nTotalBits - nDataCnt) / 8)
                              + ((Math.Abs((nTotalBits - nDataCnt) / 8) * 8) < (nTotalBits - nDataCnt) ? 1 : 0);
                int nBitAt = Math.Abs((nTotalBits - nDataCnt) - (nByteInPos * 8));
                bDataOut[nDataOutCnt] = (byte)(bDataOut[nDataOutCnt] + ((_bDataIn[nByteInPos - 1] & nBitsTable[nBitAt]) == 0 ? nBitsTable[7 - nBitAt] : 0));
                if (nBitAt == 7)
                    nDataOutCnt++;
            }
            return (bDataOut);
        }
        public void EncryptFile(String _sTextData, String _sFileName)
        {
            ASCIIEncoding asciiAncoding = new ASCIIEncoding();
            try
            {
                FileStream fsStream = new FileStream(_sFileName, FileMode.Create);
                BinaryWriter bwWriter = new BinaryWriter(fsStream);

                bwWriter.Write(EncryptData(asciiAncoding.GetBytes(_sTextData)));
                bwWriter.Close();
                fsStream.Close();
            }
            catch (Exception) {}
        }
        public byte[] EncryptText(String _sTextData)
        {
            ASCIIEncoding asciiAncoding = new ASCIIEncoding();

            return (EncryptData(asciiAncoding.GetBytes(_sTextData)));
        }
    }
}
