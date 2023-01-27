// Amplify Color - Advanced Color Grading for Unity
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEditor;

namespace AmplifyColor
{
	public class PhotoshopConnection
	{
		private const int ServerPort = 49494;
		private string _lastHost;
		private string _lastPassword;
		private TcpClient _socket;
		private NetworkStream _stream;

		public NetworkStream Stream { get; private set; }

		static int SwapEndian( int x )
		{
			uint ux = ( uint ) x;
			ux = ( ( ux & 0x000000ff ) << 24 ) + ( ( ux & 0x0000ff00 ) << 8 ) +
				 ( ( ux & 0x00ff0000 ) >> 8 ) + ( ( ux & 0xff000000 ) >> 24 );
			return ( int ) ux;
		}

		string FlushRead()
		{
			_stream.Flush();

			string result = "";
			while ( _stream.DataAvailable )
			{
				var buf = new byte[ 4 ];
				_stream.Read( buf, 0, 4 );
				int inLength = BitConverter.ToInt32( buf.Reverse().ToArray(), 0 );

				_stream.Read( buf, 0, 4 );
				//int inComStatus = BitConverter.ToInt32( buf.Reverse().ToArray(), 0 );

				//Debug.Log("Reading length: " + inLength);
				//Debug.Log("Reading com status: " + inComStatus);

				var skip = new byte[ inLength ];
				_stream.Read( skip, 0, inLength );

				result += Encoding.UTF8.GetString( skip );
			}
			return result;
		}

		string SendTestImage( PhotoshopProtocol pp )
		{
			string result = "";

			result += FlushRead();

			byte[] image = {
			0x02,
			0x00, 0x00, 0x00, 0x01,
			0x00, 0x00, 0x00, 0x01,
			0x00, 0x00, 0x00, 0x01,
			0x01,
			0x03,
			0x08,
			0xff, 0x00, 0x00 };

			MemoryStream temp_stream = new MemoryStream();
			BinaryWriter bw = new BinaryWriter( temp_stream );

			bw.Write( SwapEndian( PhotoshopProtocol.ProtocolVersion ) );	// Protocol version
			bw.Write( SwapEndian( pp._transactionId++ ) );					// Transaction ID
			bw.Write( SwapEndian( 3 ) );									// Content Type
			bw.Write( image );												// Image data
			bw.Flush();

			long len = temp_stream.Length;
			temp_stream.Seek( 0, SeekOrigin.Begin );

			BinaryReader br = new BinaryReader( temp_stream );

			var message = new byte[ len ];
			br.Read( message, 0, ( int ) len );

			string password = EditorPrefs.GetString( "AmplifyColor.NetworkPassword", "password" );
			EncryptDecrypt encryptDecrypt = new EncryptDecrypt( password );
			var encryptedMessage = encryptDecrypt.encrypt( message );

			//string str = "";
			//for (int i = 0; i < message.Length; i++)
			//{
			//	str += ( ( message[i] & 0xff ) + 0x100 ).ToString( "x" ).Substring( 1 ) + " ";
			//	if (i > 0 && (((i+1) % 8) == 0))
			//		str += "\n";
			//}
			//Debug.Log( str );
			//str = "";
			//for (int i = 0; i < encryptedMessage.Length; i++)
			//{
			//	str += ( ( encryptedMessage[i] & 0xff ) + 0x100 ).ToString( "x" ).Substring( 1 ) + " ";
			//	if (i > 0 && (((i+1) % 8) == 0))
			//		str += "\n";
			//}
			//Debug.Log( str );

			int messageLength = PhotoshopProtocol.CommLength + encryptedMessage.Length;
			_stream.Write( BitConverter.GetBytes( messageLength ).Reverse().ToArray(), 0, 4 );
			_stream.Write( BitConverter.GetBytes( PhotoshopProtocol.NoCommError ).Reverse().ToArray(), 0, 4 );
			_stream.Write( encryptedMessage, 0, encryptedMessage.Length );
			_stream.Flush();

			System.Threading.Thread.Sleep( 1000 );

			result += FlushRead();

			return result;
		}

		public void TestConnection()
		{
			if ( string.IsNullOrEmpty( ToolSettings.Instance.Host ) )
			{
				ToolSettings.Instance.Message = "Host can't be empty";
				return;
			}

			try
			{
				if ( !Connect() )
				{
					ToolSettings.Instance.Message =
						"There was an error. Check if the host is available to this machine.";
					return;
				}

				var pp = new PhotoshopProtocol( this );

				//string result = pp.SendJSCommand("scriptingVersion");
				string result = SendTestImage( pp );

				//while ( _stream.DataAvailable )
				//{
				//	var buf = new byte[ 4 ];
				//	_stream.Read( buf, 0, 4);
				//	int inLength = BitConverter.ToInt32( buf.Reverse().ToArray(), 0 );
				//	_stream.Read( buf, 0, 4);
				//	int inComStatus = BitConverter.ToInt32( buf.Reverse().ToArray(), 0 );
				//
				//	Debug.Log("Reading length: " + inLength);
				//	Debug.Log("Reading com status: " + inComStatus);
				//
				//	var skip = new byte[ inLength ];
				//	_stream.Read( skip, 0, inLength );
				//
				//	result += Encoding.UTF8.GetString( skip );
				//}

				if ( result.ToLowerInvariant().Contains( "error" ) )
				{
					ToolSettings.Instance.Message = "There was an error. Try checking the password.";
					CleanConnection();
				}
				else
				{
					ToolSettings.Instance.Message = "Test successfull";
				}
			}
			catch ( Exception )
			{
				ToolSettings.Instance.Message = "Connection could not be established";
			}
		}

		public bool Connect()
		{
			if ( string.IsNullOrEmpty( ToolSettings.Instance.Host ) )
			{
				ToolSettings.Instance.Message = "Host can't be empty";
				return false;
			}

			if ( string.IsNullOrEmpty( ToolSettings.Instance.Password ) )
			{
				ToolSettings.Instance.Message = "Password can't be empty";
				return false;
			}

			if ( _socket != null )
			{
				if ( _socket.Connected )
				{
					if ( string.Equals( _lastHost, ToolSettings.Instance.Host ) )
					{
						if ( string.Equals( _lastPassword, ToolSettings.Instance.Password ) )
						{
							return true;
						}
					}
				}
			}

			try
			{
				_socket = new TcpClient( ToolSettings.Instance.Host, ServerPort );

				_stream = _socket.GetStream();

				Stream = _stream;

				_lastHost = ToolSettings.Instance.Host;
				_lastPassword = ToolSettings.Instance.Password;
			}
			catch ( Exception )
			{
				CleanConnection();

				return false;
			}

			return true;
		}

		public void CleanConnection()
		{
			if ( _stream != null )
			{
				_stream.Flush();
				_stream.Close();
				_stream = null;
			}

			if ( _socket != null )
			{
				_socket.Client.Close();
				_socket = null;
			}
		}
	}
}
