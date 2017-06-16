using System;
using System.Text;
using System.Security.Cryptography;
using System.Web.Script.Serialization;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace ffs2play
{
	struct sVersion
	{
		public string Version;
		public int Major;
		public int Minor;
		public int Build;

	}
	class Outils
	{
        static private DateTime Last_UTC_Now=DateTime.UtcNow;
        static private long LastTicks= Last_UTC_Now.Ticks;
        static public DateTime Now
        {
            get
            {
                if (DateTime.UtcNow.Ticks != Last_UTC_Now.Ticks)
                {
                    Last_UTC_Now = DateTime.UtcNow;
                }
                return Last_UTC_Now;
            }
        }
		static public sVersion GetVersion()
		{
			sVersion resultat;
			Assembly assembly = Assembly.GetExecutingAssembly();
			FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
			resultat.Version = fvi.FileVersion;
			resultat.Major = fvi.FileMajorPart;
			resultat.Minor = fvi.FileMinorPart;
			resultat.Build = fvi.FileBuildPart;
			return resultat;
		}

        static public string Encrypt(string Password)
		{
			var data = Encoding.UTF8.GetBytes(Password);
			byte[] encrypted = ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser);
			return Convert.ToBase64String(encrypted);
		}
		static public string Decrypt(string Password)
		{
			byte[] encrypted = Convert.FromBase64String(Password);

			return Encoding.UTF8.GetString(ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser));
		}

		/// 
		/// Chiffre une chaîne de caractère
		/// 
		/// Texte clair à chiffrer
		/// Clé de chiffrement
		/// Vecteur d'initialisation
		/// Retourne le texte chiffré
		static public string EncryptMessage(string clearText, string strKey)
		{
			RijndaelManaged aes = new RijndaelManaged();
			aes.KeySize = 256;
			aes.BlockSize = 256;
			aes.Padding = PaddingMode.Zeros;
			aes.Mode = CipherMode.CBC;

			aes.Key = Convert.FromBase64String(strKey);
            aes.GenerateIV();

			string IV = ("-[--IV-[-" + Encoding.Default.GetString(aes.IV));

			ICryptoTransform AESEncrypt = aes.CreateEncryptor(aes.Key, aes.IV);
			byte[] buffer = Encoding.UTF8.GetBytes(clearText);
            string inter = Encoding.Default.GetString(AESEncrypt.TransformFinalBlock(buffer, 0, buffer.Length)) + IV;
			return Convert.ToBase64String(Encoding.Default.GetBytes(inter));
		}

		public static string DecryptMessage(string text, string key)
		{
			RijndaelManaged aes = new RijndaelManaged();
			aes.KeySize = 256;
			aes.BlockSize = 256;
			aes.Padding = PaddingMode.Zeros;
			aes.Mode = CipherMode.CBC;

			aes.Key =Convert.FromBase64String(key);

			text = Encoding.Default.GetString(Convert.FromBase64String(text));
			string IV = text;
			IV = IV.Substring(IV.IndexOf("-[--IV-[-") + 9);
			text = text.Replace("-[--IV-[-" + IV, "");

			text = Convert.ToBase64String(Encoding.Default.GetBytes(text));
			aes.IV = Encoding.Default.GetBytes(IV);

			ICryptoTransform AESDecrypt = aes.CreateDecryptor(aes.Key, aes.IV);
			byte[] buffer = Convert.FromBase64String(text);
			return Encoding.UTF8.GetString(AESDecrypt.TransformFinalBlock(buffer, 0, buffer.Length)).TrimEnd('\0');
		}

		public static double distance(double lat1, double lon1, double lat2, double lon2, char unit)
		{
			double theta = lon1 - lon2;
			double dist = Math.Sin(deg2rad(lat1)) * Math.Sin(deg2rad(lat2)) + Math.Cos(deg2rad(lat1)) * Math.Cos(deg2rad(lat2)) * Math.Cos(deg2rad(theta));
			dist = Math.Acos(dist);
			dist = rad2deg(dist);
			dist = dist * 60 * 1.1515;
			if (unit == 'K')
			{
				dist = dist * 1.609344;
			}
			else if (unit == 'N')
			{
				dist = dist * 0.8684;
			}
			if (double.IsNaN(dist)) dist = 0;
			return (dist);
		}
		//:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
		//::  This function converts decimal degrees to radians             :::
		//:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
		public static double deg2rad(double deg)
		{
			return (deg * Math.PI / 180.0);
		}
	 
		//:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
		//::  This function converts radians to decimal degrees             :::
		//:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
		public static double rad2deg(double rad)
		{
			return (rad / Math.PI * 180.0);
		}

        private static readonly DateTime UnixEpoch =
            new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long UnixTimestampFromDateTime(DateTime date)
        {
            return (long)(date - UnixEpoch).TotalMilliseconds;
        }

        public static DateTime TimeFromUnixTimestamp(long millis)
        {
            return UnixEpoch.AddMilliseconds(millis);
        }

		static public string PhpSerialize(object contenu)
		{
			var serializer = new JavaScriptSerializer();
			return serializer.Serialize(contenu);
		}

		static public List<string> PhpDeSerialize(string contenu)
		{
			var deserializer = new JavaScriptSerializer();
			return deserializer.Deserialize<List<string>>(contenu);
		}

		static public string HexToString(byte[] Tampon, int Len=0)
		{
			int pos = 0;
			int len = Tampon.Length;
			if (len > 1000) len = 1000;
			string OutPut = string.Format("Longueur = {0:0}", (Len >0) ? Len : Tampon.Length );
			while (pos < len)
			{
				OutPut += string.Format(":{0:X}",Tampon[pos]);
				pos++;
				if ((Len > 0) && (pos >= Len)) break;
			}
			return OutPut;
		}

		static public double Average(ref List<Double> Array)
		{
			double total = 0;
			foreach (double Value in Array) total += Value;
			return total / (double)Array.Count;
		}

		public static int ConvertToBinaryCodedDecimal(int Code)
		{
			byte[] bytes = BitConverter.GetBytes(Code);
			int Length = bytes.Length;
			if (Length > 2) Length = 2;
			StringBuilder bcd = new StringBuilder(Length * 2);
			for (int i = Length - 1; i >= 0; i--)
			{
				byte bcdByte = bytes[i];
				int idHigh = (bcdByte >> 4) & 0X07;
				int idLow = bcdByte & 0x07;
				bcd.Append(string.Format("{0:X}{1:X}", idHigh, idLow));
			}
			return Convert.ToInt32(bcd.ToString());
		}
	}
}
