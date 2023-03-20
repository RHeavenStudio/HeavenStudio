// Amplify Color - Advanced Color Grading for Unity
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using UnityEngine;
using UnityEditor;

namespace AmplifyColor
{
	public class PhotoshopProtocol
	{
		private readonly PhotoshopConnection _connection;
		/** values for transaction type */
		internal const int IllegalType = 0;
		internal const int ErrorstringType = 1;
		internal const int JavascriptType = 2;
		internal const int ImageType = 3;
		internal const int ProfileType = 4;
		internal const int DataType = 5;

		/** current protocol version */
		internal const int ProtocolVersion = 1;

		/** length of the header not including the actual length byte or the communication status */
		internal const int ProtocolLength = 4 + 4 + 4;
		internal const int CommLength = 4;
		internal const int NoCommError = 0;

		/** messages are encrypted, helper class to encrypt and decrypt messages */
		private EncryptDecrypt _encryptDecrypt;
		private readonly UTF8Encoding _utf8Encoding;
		internal int _transactionId;

		public PhotoshopProtocol( PhotoshopConnection connection )
		{
			_connection = connection;
			_utf8Encoding = new UTF8Encoding();
		}

		public string SendJSCommand( string command )
		{
			if ( !_connection.Connect() )
			{
				return "Error connecting to Photoshop.";
			}

			var stream = _connection.Stream;

			string password = EditorPrefs.GetString( "AmplifyColor.NetworkPassword", "password" );
			_encryptDecrypt = new EncryptDecrypt( password );

			try
			{
				/* WRITE */
				byte[] strBytes = _utf8Encoding.GetBytes( command + "\n" );
				byte[] allBytes;

				using ( var memoryStream = new MemoryStream() )
				{
					BitConverter.GetBytes( ProtocolVersion );
					memoryStream.Write( BitConverter.GetBytes( ProtocolVersion ).Reverse().ToArray(), 0, 4 );
					_transactionId++;
					memoryStream.Write( BitConverter.GetBytes( _transactionId ).Reverse().ToArray(), 0, 4 );
					memoryStream.Write( BitConverter.GetBytes( JavascriptType ).Reverse().ToArray(), 0, 4 );
					memoryStream.Write( strBytes, 0, strBytes.Length );

					allBytes = memoryStream.GetBuffer().Take( ProtocolLength + strBytes.Length ).ToArray();
				}

				var encryptedBytes = _encryptDecrypt.encrypt( allBytes );

				int messageLength = CommLength + encryptedBytes.Length;

				stream.Write( BitConverter.GetBytes( messageLength ).Reverse().ToArray(), 0, 4 );
				stream.Write( BitConverter.GetBytes( NoCommError ).Reverse().ToArray(), 0, 4 );
				stream.Write( encryptedBytes, 0, encryptedBytes.Length );

				/* READ */

				var buffer = new byte[ 4 ];

				stream.Read( buffer, 0, 4 );
				buffer = buffer.Reverse().ToArray();
				int inLength = BitConverter.ToInt32( buffer, 0 );


				stream.Read( buffer, 0, 4 );
				buffer = buffer.Reverse().ToArray();
				int inComStatus = BitConverter.ToInt32( buffer, 0 );

				if ( inComStatus != 0 )
				{
					stream.Read( buffer, 0, 4 );
					buffer = buffer.Reverse().ToArray();
					//int inVersion = BitConverter.ToInt32(buffer, 0);

					stream.Read( buffer, 0, 4 );
					buffer = buffer.Reverse().ToArray();
					//int inTransaction = BitConverter.ToInt32(buffer, 0);

					stream.Read( buffer, 0, 4 );
					buffer = buffer.Reverse().ToArray();
					int inType = BitConverter.ToInt32( buffer, 0 );

					if ( inType == JavascriptType || inType == ErrorstringType )
					{
						inLength -= ProtocolLength;
						var bytemessage = new byte[ inLength + 1 ];
						int rr = stream.Read( bytemessage, 0, inLength );

						if ( rr > 0 )
						{
							var encoder = new UTF8Encoding();
							var message = encoder.GetString( bytemessage );
							Debug.LogWarning( "[AmplifyColor] Uncoded Message: " + message + ". Please check your password." );
							return message.Trim( new[] { '\0' } );
						}
					}
					else
					{
						return "Message types: " + inType;
					}
				}
				else
				{
					inLength = inLength - 4;

					var messageBytes = new byte[ inLength ];
					stream.Read( messageBytes, 0, inLength );

					IEnumerable<byte> decryptedBytes = _encryptDecrypt.decrypt( messageBytes );

					byte[] tempbytes;
					//tempbytes = decryptedBytes.Take(4).Reverse().ToArray();
					decryptedBytes = decryptedBytes.Skip( 4 );
					//int messageVersion = BitConverter.ToInt32(tempbytes, 0);

					//tempbytes = decryptedBytes.Take(4).Reverse().ToArray();
					decryptedBytes = decryptedBytes.Skip( 4 );
					//int messageId = BitConverter.ToInt32(tempbytes, 0);

					tempbytes = decryptedBytes.Take( 4 ).Reverse().ToArray();
					decryptedBytes = decryptedBytes.Skip( 4 );
					int messageType = BitConverter.ToInt32( tempbytes, 0 );

					if ( messageType == JavascriptType || messageType == ErrorstringType )
					{
						var encoder = new UTF8Encoding();
						var message = encoder.GetString( decryptedBytes.ToArray() );
						return message.Trim( new[] { '\0' } );
					}

					return "Message types: " + messageType;
				}
			}
			catch ( Exception e )
			{
				return "Exception: " + e.Message;
			}

			return string.Empty;
		}

		public string ReceiveImage( string documentName, out ImageResult imageData )
		{
			imageData = null;

			if ( !_connection.Connect() )
			{
				return "Error connecting to Photoshop.";
			}

			var stream = _connection.Stream;

			string password = EditorPrefs.GetString( "AmplifyColor.NetworkPassword", "password" );
			_encryptDecrypt = new EncryptDecrypt( password );

			var commandBuilder = new StringBuilder();

			if ( !string.IsNullOrEmpty( documentName ) )
			{
				commandBuilder.Append( "app.activeDocument = app.documents.getByName('" + documentName + "');\n" );
			}

			commandBuilder.Append( "var idNS = stringIDToTypeID('sendDocumentThumbnailToNetworkClient');\n" );
			commandBuilder.Append( "var desc1 = new ActionDescriptor();\n" );
			commandBuilder.Append( "desc1.putInteger( stringIDToTypeID('width'), app.activeDocument.width );\n" );
			commandBuilder.Append( "desc1.putInteger( stringIDToTypeID('height'), app.activeDocument.height );\n" );
			commandBuilder.Append( "desc1.putInteger( stringIDToTypeID('format'), 2 );\n" );
			commandBuilder.Append( "executeAction( idNS, desc1, DialogModes.NO );" );
			string command = commandBuilder.ToString();

			try
			{
				/* WRITE */
				byte[] strBytes = _utf8Encoding.GetBytes( command + "\n" );
				byte[] allBytes;

				using ( var memoryStream = new MemoryStream() )
				{
					BitConverter.GetBytes( ProtocolVersion );
					memoryStream.Write( BitConverter.GetBytes( ProtocolVersion ).Reverse().ToArray(), 0, 4 );
					_transactionId++;
					memoryStream.Write( BitConverter.GetBytes( _transactionId ).Reverse().ToArray(), 0, 4 );
					memoryStream.Write( BitConverter.GetBytes( JavascriptType ).Reverse().ToArray(), 0, 4 );
					memoryStream.Write( strBytes, 0, strBytes.Length );

					allBytes = memoryStream.GetBuffer().Take( ProtocolLength + strBytes.Length ).ToArray();
				}

				var encryptedBytes = _encryptDecrypt.encrypt( allBytes );

				int messageLength = CommLength + encryptedBytes.Length;

				stream.Write( BitConverter.GetBytes( messageLength ).Reverse().ToArray(), 0, 4 );
				stream.Write( BitConverter.GetBytes( NoCommError ).Reverse().ToArray(), 0, 4 );
				stream.Write( encryptedBytes, 0, encryptedBytes.Length );

				/* READ */
				DateTime start = DateTime.Now;
				int messageType = 0;
				while ( messageType != ImageType && ( DateTime.Now - start ).TotalMilliseconds < 1000 )
				{
					if ( !stream.DataAvailable )
						continue;

					var buffer = new byte[ 4 ];

					stream.Read( buffer, 0, 4 );
					buffer = buffer.Reverse().ToArray();
					int inLength = BitConverter.ToInt32( buffer, 0 );


					stream.Read( buffer, 0, 4 );
					buffer = buffer.Reverse().ToArray();
					int inComStatus = BitConverter.ToInt32( buffer, 0 );

					if ( inComStatus != 0 )
					{
						stream.Read( buffer, 0, 4 );
						buffer = buffer.Reverse().ToArray();
						//int inVersion = BitConverter.ToInt32(buffer, 0);

						stream.Read( buffer, 0, 4 );
						buffer = buffer.Reverse().ToArray();
						//int inTransaction = BitConverter.ToInt32(buffer, 0);

						stream.Read( buffer, 0, 4 );
						buffer = buffer.Reverse().ToArray();
						messageType = BitConverter.ToInt32( buffer, 0 );

						if ( messageType == JavascriptType || messageType == ErrorstringType )
						{
							inLength -= ProtocolLength;
							var bytemessage = new byte[ inLength + 1 ];
							int rr = stream.Read( bytemessage, 0, inLength );

							if ( rr > 0 )
							{
								var encoder = new UTF8Encoding();
								var message = encoder.GetString( bytemessage );
								return message.Trim( new[] { '\0' } );
							}
						}
						else
						{
							return "Message types: " + messageType;
						}
					}
					else
					{
						inLength = inLength - 4;

						var messageBytes = new List<byte>();
						var messagebuffer = new byte[ 1000 ];
						int totalread = 0;

						while ( totalread < inLength || stream.DataAvailable )
						{
							int bytesread = stream.Read( messagebuffer, 0, 1000 );
							totalread += bytesread;
							messageBytes.AddRange( messagebuffer.Take( bytesread ) );
						}

						IEnumerable<byte> decryptedBytes = _encryptDecrypt.decrypt( messageBytes.ToArray() );

						decryptedBytes = decryptedBytes.Skip( 4 );
						decryptedBytes = decryptedBytes.Skip( 4 );

						byte[] tempbytes = decryptedBytes.Take( 4 ).Reverse().ToArray();
						decryptedBytes = decryptedBytes.Skip( 4 );

						messageType = BitConverter.ToInt32( tempbytes, 0 );

						if ( messageType == ImageType )
							imageData = ImageResult.FromPhotoshopResult( decryptedBytes.ToArray() );
						else if ( messageType == ErrorstringType )
							return Encoding.UTF8.GetString( decryptedBytes.ToArray() );
					}
				}

				//return "Message types: " + messageType;
				return "Image Received";
			}
			catch ( Exception e )
			{
				return e.Message;
			}
		}

		static int SwapEndian( int x )
		{
			uint ux = ( uint ) x;
			ux = ( ( ux & 0x000000ff ) << 24 ) + ( ( ux & 0x0000ff00 ) << 8 ) +
				 ( ( ux & 0x00ff0000 ) >> 8 ) + ( ( ux & 0xff000000 ) >> 24 );
			return ( int ) ux;
		}

		public string SendImage( ImageResult imageData )
		{
			if ( !_connection.Connect() )
			{
				return "Error connecting to Photoshop.";
			}

			var stream = _connection.Stream;

			int width = imageData.Width;
			int height = imageData.Height;

			string password = EditorPrefs.GetString( "AmplifyColor.NetworkPassword", "password" );
			_encryptDecrypt = new EncryptDecrypt( password );

			try
			{
				/* WRITE */
				var temp_stream = new MemoryStream();
				var w = new BinaryWriter( temp_stream );

				w.Write( SwapEndian( ProtocolVersion ) );
				w.Write( SwapEndian( _transactionId++ ) );
				w.Write( SwapEndian( ImageType ) );

				w.Write( ( byte ) 2 ); 				//imagetype (2 for PixMap);
				w.Write( SwapEndian( width ) );
				w.Write( SwapEndian( height ) );
				w.Write( SwapEndian( width * 3 ) );	//bytesPerScanLine
				w.Write( ( byte ) 1 ); 				//color mode RGB
				w.Write( ( byte ) 3 ); 				//channel count
				w.Write( ( byte ) 8 ); 				//bits per channel

				var data = imageData.GenerateRGBData();
				w.Write( data, 0, data.Length );
				w.Flush();

				long len = temp_stream.Length;
				temp_stream.Seek( 0, SeekOrigin.Begin );
				BinaryReader r = new BinaryReader( temp_stream );
				var message = new byte[ len ];
				r.Read( message, 0, ( int ) len );

				var encryptedMessage = _encryptDecrypt.encrypt( message );
				int messageLength = CommLength + encryptedMessage.Length;

				stream.Write( BitConverter.GetBytes( messageLength ).Reverse().ToArray(), 0, 4 );
				stream.Write( BitConverter.GetBytes( NoCommError ).Reverse().ToArray(), 0, 4 );
				stream.Write( encryptedMessage, 0, encryptedMessage.Length );

				// Photoshop CS6 will issue a response, make sure we skip it (small timeout)
				DateTime start = DateTime.Now;
				while ( ( DateTime.Now - start ).TotalMilliseconds < 100 )
				{
					if ( !stream.DataAvailable )
						continue;

					var buf = new byte[ 4 ];
					stream.Read( buf, 0, 4 );
					int length = BitConverter.ToInt32( buf.Reverse().ToArray(), 0 );

					buf = new byte[ length ];
					stream.Read( buf, 0, length );
				}

				return "Sent";
			}
			catch ( Exception e )
			{
				//Debug.Log( e.Message );
				return e.Message;
			}
		}
	}
}
