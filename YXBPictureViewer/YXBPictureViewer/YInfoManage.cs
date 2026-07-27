
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.IO;
using Microsoft.Win32;
namespace YXBPictureViewer
{
    public class YInfo
    {
        public Int32 namelength = 0;
        public string name = "";
        public Int32 contentlength = 0;
        public string content = "";
        public byte[] bytearray = null;
        private byte[] namebytes=null;
        private byte[] contentbytes=null;
        public string ToStringPair()
        {
            if (name.Trim().Length <= 0) return "";
            string t = "";
            t = name.Trim() + ":" + content;
            return t;
        }
        public void FromStringPair(string pair)
        {
            name = ""; content = "";
            int pos = pair.IndexOf(':');
            if (pos <= 0)
            {
                name = "";
                content = pair;
            }
            else
            {
                name = pair.Substring(0, pos).Trim();
                content = pair.Substring(pos + 1);
            }
        }
        public int GetLength()
        {

            byte[] src;
            int len = 0;
            int intlen;
            src = BitConverter.GetBytes(namelength);
            intlen = src.Length;
            if (name.Length == 0) { len = intlen; namelength = 0; namebytes = null; }
            else
            {
                namebytes = System.Text.Encoding.BigEndianUnicode.GetBytes(name);
                len = namebytes.Length + intlen;
                namelength = namebytes.Length;
            }
            if (content.Length <= 0) { len = len + intlen; contentlength = 0; contentbytes = null; }
            else
            {
                contentbytes = System.Text.Encoding.BigEndianUnicode.GetBytes(content);
                len = len + contentbytes.Length + intlen;
                contentlength = contentbytes.Length;
            }
            if (bytearray == null || bytearray.Length <= 0) { len = len + intlen;  }
            else
            {
                len = len + intlen;
                len = len + bytearray.Length;
            }
            return len;
        }
        public byte[] ToBytes()
        {
            int len = GetLength();
            byte[] bytes = new byte[len];
            int i, j,pos=0;
            byte[] tempbytes = BitConverter.GetBytes(namelength);  //gen namelength bytes
            j = tempbytes.Length;
            for (i = 0; i < j; i++)  //write to the buffer
            {
                bytes[pos] = tempbytes[i];
                pos++;
            }
            if (namebytes != null)  //write the name string bytes to buffer
            {
                j = namebytes.Length;
                for (i = 0; i < j; i++)
                {
                    bytes[pos] = namebytes[i];
                    pos++;
                }
            }
            tempbytes = BitConverter.GetBytes(contentlength); //gen contentlength bytes
            j = tempbytes.Length;
            for (i = 0; i < j; i++)
            {
                bytes[pos] = tempbytes[i];
                pos++;
            }
            if (contentbytes != null)  //write the content string bytes to the buffer
            {
                j = contentbytes.Length;
                for (i = 0; i < j; i++)
                {
                    bytes[pos] = contentbytes[i];
                    pos++;
                }
            }


            Int32 blen = 0;
            if (bytearray == null) blen = 0;
            else blen = bytearray.Length;

            tempbytes = BitConverter.GetBytes(blen);  //write the length of the bytes 
            j = tempbytes.Length;
            for (i = 0; i < j; i++)
            {
                bytes[pos] = tempbytes[i];
                pos++;
            }
            if (blen > 0)                           //write the bytearray to the buffer
            {
                for (i = 0; i < blen; i++)
                {
                    bytes[pos] = bytearray[i];
                    pos++;
                }
            }
            ClearInnerBytes();
            return bytes;
        }
        private void ClearInnerBytes()
        {
            namebytes = null;
            contentbytes = null;
        }
        public void WriteToBytes(byte[] bytes, ref int pos)
        {
            byte[] b = this.ToBytes();
            int i, len = b.Length;
            for (i = 0; i < len; i++)
            {
                bytes[pos] = b[i];
                pos++;
            }
        }
        public void ReadFromBytes(byte[] bytes, ref int pos)
        {
            int intlen,i;
            byte[] temp = BitConverter.GetBytes(namelength);
            intlen = temp.Length;
            temp=null;
            temp=new byte[intlen];
            for (i = 0; i < intlen; i++) //read the namelength
            {
                temp[i] = bytes[pos];
                pos++;
            }
            namelength = BitConverter.ToInt32(temp, 0);
            if (namelength > 0)
            {
                temp=null;
                temp=new byte[namelength];         //read the name string from buffer
                for (i = 0; i < namelength; i++)
                {
                    temp[i] = bytes[pos];
                    pos++;
                }
                name = System.Text.Encoding.BigEndianUnicode.GetString(temp, 0, temp.Length);
            }
            temp = null;
            temp = new byte[intlen];
            for (i = 0; i < intlen; i++)  //read the content length
            {
                temp[i] = bytes[pos];
                pos++;
            }
            contentlength = BitConverter.ToInt32(temp, 0);
            if (contentlength > 0)
            {
                temp = null;
                temp = new byte[contentlength];
                for (i = 0; i < contentlength; i++)
                {
                    temp[i] = bytes[pos];
                    pos++;
                }
                content = System.Text.Encoding.BigEndianUnicode.GetString(temp, 0, temp.Length);
            }
            temp = null;
            temp = new byte[intlen];
            for (i = 0; i < intlen; i++)  //read the bytearray length
            {
                temp[i] = bytes[pos];
                pos++;
            }
            Int32 blen=BitConverter.ToInt32(temp, 0);
            if (blen > 0)
            {
                bytearray = new byte[blen];
                for (i = 0; i < blen; i++)
                {
                    bytearray[i] = bytes[pos];
                    pos++;
                }
            }
        }
    }
    public class YInfoManage
    {
        public static string m_encryption = "!@#$%^&*";
        public ArrayList infolist = new ArrayList();
        public void Init()
        {
            if (infolist != null) infolist.Clear();
            else infolist = new ArrayList();
        }
        public YInfo Find(string name)
        {
            int len = infolist.Count;
            name = name.Trim().ToUpper();
            for (int i=0; i <len; i++)
            {
                YInfo y = (YInfo)infolist[i];
                if (name == y.name)
                {
                    return y;
                }
            }
            return null;
        }
        #region GetContent
        public string GetContent(string name)
        {
            YInfo y = Find(name);
            if (y == null) return "";
            else return y.content;
        }
        public bool GetContentBool(string name)
        {
            YInfo y = Find(name);
            if (y == null) return false;
            else
            {
                if (y.content.ToUpper().Trim() == "TRUE") return true;
                else return false;
            }
        }
        public int GetContentInt(string name)
        {
            int d = 0;
            YInfo y = Find(name);
            if (y == null) return 0;
            else
            {
                try
                {
                    d = Convert.ToInt32(y.content);
                }
                catch
                {
                    d = 0;
                }
                return d;
            }
        }
        public double GetContentDouble(string name)
        {
            double d = 0;
            YInfo y = Find(name);
            if (y == null) return 0.0;
            else
            {
                try
                {
                    d = Convert.ToDouble(y.content);
                }
                catch
                {
                    d = 0.0;
                }
                return d;
            }
        }
        public DateTime GetContentDateTime(string name)
        {
            DateTime dt=DateTime.Now;
            Int64 ticks = 0;
            YInfo y = Find(name);
            if (y == null) return dt;
            else
            {
                try
                {
                    ticks = Convert.ToInt64(y.content);
                    dt = new DateTime(ticks);
                }
                catch
                {
                   
                }
                return dt;
            }
        }
        public byte[] GetContentBytes(string name)
        {
            YInfo y = Find(name);
            if (y == null) return null;
            else return y.bytearray;
        }
        #endregion
        #region Add Item
        public int Add(string name, string content)
        {
            YInfo y = new YInfo();
            y.name = name.Trim().ToUpper(); y.content = content;
            return infolist.Add(y);
        }
        public int Add(string name, int value)
        {
            YInfo y = new YInfo();
            y.name = name.Trim().ToUpper(); y.content = value.ToString();
            return infolist.Add(y);
        }
        public int Add(string name, bool value)
        {
            YInfo y = new YInfo();
            y.name = name.Trim().ToUpper(); y.content = (value ? "TRUE" : "FALSE");
            return infolist.Add(y);
        }
        public int Add(string name, double value)
        {
            YInfo y = new YInfo();
            y.name = name.Trim().ToUpper(); y.content = value.ToString();
            return infolist.Add(y);
        }
        public int Add(string name, DateTime value)
        {
            YInfo y = new YInfo();
            y.name = name.Trim().ToUpper(); y.content = value.Ticks.ToString();
            return infolist.Add(y);
        }
        public int Add(string name,byte[] buf)
        {
            YInfo y = new YInfo();
            y.name = name.Trim().ToUpper(); y.bytearray = buf;
            return infolist.Add(y);
        }

        public int Add(YInfo y)
        {
            return infolist.Add(y);
        }
        #endregion
        #region Set Item Value
        public void SetValue(string name, string content)
        {
            YInfo y = Find(name);
            if (y == null)
            {
                y = new YInfo();
                y.name = name.Trim().ToUpper(); y.content = content;
                infolist.Add(y);
            }
            else
            {
                y.name = name.Trim().ToUpper(); y.content = content;
            }
        }
        public void SetValue(string name, int value)
        {
            YInfo y = Find(name);
            if (y == null)
            {
                y = new YInfo();
                y.name = name.Trim().ToUpper(); y.content = value.ToString();
                infolist.Add(y);
            }
            else
            {
                y.name = name.Trim().ToUpper(); y.content = value.ToString();
            }
        }
        public void SetValue(string name, bool value)
        {
            YInfo y = Find(name);
            if (y == null)
            {
                y = new YInfo();
                y.name = name.Trim().ToUpper(); y.content = (value ? "TRUE" : "FALSE");
                infolist.Add(y);
            }
            else
            {
                y.name = name.Trim().ToUpper(); y.content = (value ? "TRUE" : "FALSE");
            }
        }
        public void SetValue(string name, double value)
        {
            YInfo y = Find(name);
            if (y == null)
            {
                y = new YInfo();
                y.name = name.Trim().ToUpper(); y.content = value.ToString();
                infolist.Add(y);
            }
            else
            {
                y.name = name.Trim().ToUpper(); y.content = value.ToString();
            }
        }
        public void SetValue(string name, DateTime value)
        {
            YInfo y = Find(name);
            if (y == null)
            {
                y = new YInfo();
                y.name = name.Trim().ToUpper(); y.content = value.Ticks.ToString();
                infolist.Add(y);
            }
            else
            {
                y.name = name.Trim().ToUpper(); y.content = value.Ticks.ToString();
            }
        }
        public void SetValue(string name, byte[] buf)
        {
            YInfo y = Find(name);
            if (y == null)
            {
                y = new YInfo();
                y.name = name.Trim().ToUpper(); y.bytearray = buf;
                infolist.Add(y);
            }
            else
            {
                y.name = name.Trim().ToUpper(); y.bytearray = buf;
            }
        }
        #endregion
        public void Delete(int i)
        {
            infolist.RemoveAt(i);
        }

        public void Delete(string name)
        {
            int len=infolist.Count;
            name = name.Trim().ToUpper();
            for (int i = len - 1; i >= 0; i--)
            {
                YInfo y = (YInfo)infolist[i];
                if (name == y.name)
                {
                    infolist.RemoveAt(i);
                }
            }
        }
        public int GetLength()
        {
            if (infolist.Count <= 0) return 0;
            int len=0,l,i, c = infolist.Count;
            for (i = 0; i < c; i++)
            {
                YInfo y = (YInfo)infolist[i];
                l = y.GetLength();
                len = len + l;
            }
            return len;
        }
        public byte[] ToBytes()
        {
            if (infolist.Count <= 0) return null;
            int len = GetLength();
            if (len <= 0) return null;
            byte[] bytes = new byte[len];
            int pos = 0;
            int i, c = infolist.Count;
            for (i = 0; i < c; i++)
            {
                YInfo y = (YInfo)infolist[i];
                y.WriteToBytes(bytes, ref pos);
            }
            return bytes;
        }
        public void ReadFromBytes(byte[] bytes)
        {
            infolist.Clear();
            if (bytes == null) return;
            int pos = 0;
            int len=bytes.Length;
            
            try
            {
                while (pos < len)
                {
                    YInfo y = new YInfo();
                    y.ReadFromBytes(bytes, ref pos);
                    Add(y);
                }

            }
            catch(Exception e)
            {
                throw new Exception("Error while reading data from bytes in YInfomanage."+e.Message);
            }
        }
        public bool Save(string name)
        {
            return WriteToRegistry(name);
        }
        public bool Load(string name)
        {
            return LoadFromRegistry(name);
        }
        public string ToBase64String()
        {
            byte[] bytes = this.ToBytes();
            if (bytes == null) return "";
            bytes = Encode(bytes);
            string value = Convert.ToBase64String(bytes);
            return value;
        }
        public bool FromBase64String(string base64string)
        {
            bool re = false;
            try
            {
                string value = base64string;
                byte[] bytes = Convert.FromBase64String(value);
                bytes = Decode(bytes);
                ReadFromBytes(bytes);
                re = true;
            }
            catch
            {
            }
            return re;
        }
        public string ToStringPairs()
        {
            if(infolist==null || infolist.Count<=0) return "";
            string t="";
            foreach (object o in infolist)
            {
                YInfo yi = (YInfo)o;
                t = t + "[" + yi.ToStringPair() + "]";
            }
            return t;
        }
        public void FromStringPairs(string infopairs)
        {
            if (infolist == null) infolist = new ArrayList();
            infolist.Clear();
            infopairs = infopairs.Trim();
            string[] pairs = infopairs.Split(new char[] { ']' });
            string tt;
            foreach (string t in pairs)
            {
                if (t.Length > 2)
                {
                    tt = t.Substring(1); //get rid of the first '['
                    YInfo yi = new YInfo();
                    yi.FromStringPair(tt);
                    Add(yi);
                }
            }
        }
        public bool WriteToRegistry(string name)
        {
            bool re = false;
            try
            {
                byte[] bytes = this.ToBytes();
                if (bytes == null) return false;
                bytes = Encode(bytes);
                string value = Convert.ToBase64String(bytes);
                RegistryKey key = Registry.CurrentUser.CreateSubKey("YPictureViwer");
                key.SetValue(name, value);
                re = true;
            }
            catch
            {
            }
            return re;

        }
        public bool LoadFromRegistry(string name)
        {
            bool re = false;
            try
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey("YPictureViwer");
                if (key == null)
                {
                    System.Diagnostics.Debug.WriteLine("Registry key 'YPictureViwer' not found. This may be first run or after migration.");
                    return false; // Key doesn't exist - not an error, just no settings saved yet
                }

                object valueObj = key.GetValue(name);
                if (valueObj == null)
                {
                    System.Diagnostics.Debug.WriteLine($"Registry value '{name}' not found in 'YPictureViwer'");
                    return false; // Value doesn't exist
                }

                string value = valueObj.ToString();
                if (string.IsNullOrEmpty(value))
                {
                    System.Diagnostics.Debug.WriteLine($"Registry value '{name}' is empty");
                    return false;
                }

                byte[] bytes = Convert.FromBase64String(value);
                bytes = Decode(bytes);
                ReadFromBytes(bytes);
                re = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading from registry: {ex.GetType().Name}: {ex.Message}");
                // Don't rethrow - return false on any error
            }
            return re;
        }

        public bool WriteToDisk(string fname)
        {
            bool re = false;
            if (File.Exists(fname))
            {
                try
                {
                    File.Delete(fname);
                }
                catch
                {
                    return false;
                }
            }
            try
            {
                byte[] bytes = this.ToBytes();
                if (bytes == null) return false;
                bytes = Encode(bytes);
                FileStream fs = new FileStream(fname, FileMode.CreateNew);
                BinaryWriter w = new BinaryWriter(fs);
                w.Write(bytes);
                w.Close();
                fs.Close();
                re = true;
            }
            catch
            {
            }
            return re;
        }
        public bool LoadFromDisk(string fname)
        {
            bool re = false;
            try
            {
                FileStream fs = new FileStream(fname, FileMode.Open, FileAccess.Read);
                BinaryReader r = new BinaryReader(fs);
                int len =(int) fs.Length;
                byte[] bytes = r.ReadBytes(len);
                bytes = Decode(bytes);
                ReadFromBytes(bytes);
                r.Close();
                fs.Close();
                re = true;
            }
            catch
            {
            }
            return re;
        }
        public static byte[] Decode(byte[] bytes)
        {
            return YEncrypt.DecryptBufferRecordedLength(bytes, "!@#$%^&*");

        }
        public static byte[] Encode(byte[] bytes)
        {
            return YEncrypt.EncryptBufferRecordLength(bytes, "!@#$%^&*");
        }
    }
}
