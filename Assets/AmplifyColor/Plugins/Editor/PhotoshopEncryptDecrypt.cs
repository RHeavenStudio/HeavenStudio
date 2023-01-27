// Amplify Color - Advanced Color Grading for Unity
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using System;
using System.Security.Cryptography;
using System.Text;

namespace AmplifyColor
{
	public class PhotoshopEncryptDecrypt
	{
		private readonly ICryptoTransform _eCipher;
		private readonly ICryptoTransform _dCipher;

		// these must match the values used in Photoshop DO NOT CHANGE
		// get the password from the dialog settings in Photoshop
		private readonly char[] _salt = { 'A', 'd', 'o', 'b', 'e', ' ', 'P', 'h', 'o', 't', 'o', 's', 'h', 'o', 'p' };
		private const int IterationCount = 1000;
		private const int KeyLength = 24;

		public PhotoshopEncryptDecrypt( String passPhrase )
		{
			var encoder = new ASCIIEncoding();
			//byte[] bytepass = encoder.GetBytes(passPhrase);
			byte[] bytesalt = encoder.GetBytes( _salt );

			var tripleDes = new TripleDESCryptoServiceProvider
								{
									Mode = CipherMode.CBC,
									Padding = PaddingMode.PKCS7,
								};



			var rfc2898DeriveBytes = new Rfc2898DeriveBytes( passPhrase, bytesalt, IterationCount );
			var key = rfc2898DeriveBytes.GetBytes( KeyLength );

			var empty = new byte[ 8 ];
			_eCipher = tripleDes.CreateEncryptor( key, empty );
			_dCipher = tripleDes.CreateDecryptor( key, empty );

		}

		public byte[] Encrypt( byte[] data )
		{
			byte[] encryptedBytes = _eCipher.TransformFinalBlock( data, 0, data.Length );
			return encryptedBytes;
		}


		public byte[] Decrypt( byte[] data )
		{
			byte[] decryptedBytes = _dCipher.TransformFinalBlock( data, 0, data.Length );
			return decryptedBytes;
		}
	}
}
