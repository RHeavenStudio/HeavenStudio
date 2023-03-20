// Amplify Color - Advanced Color Grading for Unity
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using System;
using System.Linq;
using UnityEngine;

namespace AmplifyColor
{
	public class ImageResult
	{
		public ImageResult( Color[ , ] colorData )
		{
			ColorData = colorData;
			Width = colorData.GetLength( 0 );
			Height = colorData.GetLength( 1 );
		}

		public Color[ , ] ColorData { get; private set; }

		public int Width { get; private set; }

		public int Height { get; private set; }

		public Texture2D GenerateTexture2D()
		{
			var data = new Color[ Width * Height ];

			for ( int h = Height - 1, i = 0; h >= 0; h-- )
			{
				for ( int w = 0; w < Width; w++ )
				{
					data[ i++ ] = ColorData[ w, h ];
				}
			}

			var texture = new Texture2D( Width, Height );
			texture.SetPixels( data );

			return texture;
		}

		public Color[] GenerateColorArray()
		{
			var data = new Color[ Width * Height ];

			for ( int h = 0, i = 0; h < Height; h++ )
			{
				for ( int w = 0; w < Width; w++ )
				{
					data[ i++ ] = ColorData[ w, h ];
				}
			}

			return data;
		}

		public byte[] GenerateRGBData()
		{
			var data = new byte[ Width * Height * 3 ];

			for ( int h = 0, i = 0; h < Height; h++ )
			{
				for ( int w = 0; w < Width; w++ )
				{
					var color = ColorData[ w, h ];
					data[ i++ ] = ( byte ) ( color.r * 255 );
					data[ i++ ] = ( byte ) ( color.g * 255 );
					data[ i++ ] = ( byte ) ( color.b * 255 );
				}
			}

			return data;
		}

		public bool AddLUT( LUTSettings settings )
		{
			int size = settings.Size;
			int columns = settings.Columns;
			int rows = settings.Rows;

			if ( Width < columns * size )
			{
				return false;
			}

			if ( Height < rows * size )
			{
				return false;
			}

			var data = ColorData;

			for ( int r = 0; r < rows; r++ )
			{
				for ( int c = 0; c < columns; c++ )
				{
					for ( int h = 0; h < size; h++ )
					{
						for ( int w = 0; w < size; w++ )
						{
							int row = r * size + h;
							int col = c * size + w;
							float rf = ( float ) w / ( size - 1 );
							float gf = ( float ) h / ( size - 1 );
							float bf = ( float ) ( c + r * columns ) / ( size - 1 );
							data[ col, row ] = new Color( rf, gf, bf );
						}
					}
				}
			}

			return true;
		}

		public static ImageResult FromPhotoshopResult( byte[] decryptedBytes )
		{
			int imageWidth = GetBigEndianInt( decryptedBytes, 1 );
			int imageHeight = GetBigEndianInt( decryptedBytes, 5 );

			var bytesize = imageWidth * imageHeight * 3;

			const int headerLength = 16;

			if ( bytesize + headerLength > decryptedBytes.Length )
			{
				return null;
			}

			var imageData = new Color[ imageWidth, imageHeight ];

			for ( int i = 0, k = headerLength; i < imageHeight; i++ )
			{
				for ( int j = 0; j < imageWidth; j++ )
				{
					imageData[ j, i ] = new Color( decryptedBytes[ k++ ] / 255f, decryptedBytes[ k++ ] / 255f, decryptedBytes[ k++ ] / 255f, 1f );
				}
			}

			return new ImageResult( imageData );
		}

		private static readonly byte[] Temp = new byte[ 4 ];
		private static int GetBigEndianInt( byte[] bytes, int offset )
		{
			if ( bytes.Length <= offset + 4 )
			{
				return -1;
			}

			for ( int j = 0, i = 3; j < 4; j++, i-- )
			{
				Temp[ j ] = bytes[ offset + i ];
			}

			return BitConverter.ToInt32( Temp, 0 );
		}

		public static ImageResult FromTexture( Texture2D texture )
		{
			if ( texture == null )
			{
				return null;
			}

			var width = texture.width;
			var height = texture.height;

			if ( width == 0 || height == 0 )
			{
				return null;
			}

			var data = texture.GetPixels();

			var colorData = new Color[ width, height ];

			for ( int h = 0; h < height; h++ )
			{
				for ( int w = 0; w < width; w++ )
				{
					int index = w + ( height - h - 1 ) * width;
					colorData[ w, h ] = data[ index ];
				}
			}

			return new ImageResult( colorData );
		}

		public LUTResult GetLUT( LUTSettings lutSettings )
		{
			int size = lutSettings.Size;
			int columns = lutSettings.Columns;
			int rows = lutSettings.Rows;

			if ( Width < columns * size )
			{
				return null;
			}

			if ( Height < rows * size )
			{
				return null;
			}

			Color[ , , ] data = new Color[ size, size, size ];
			var colorData = ColorData;

			for ( int r = 0; r < rows; r++ )
			{
				for ( int c = 0; c < columns; c++ )
				{
					for ( int h = 0; h < size; h++ )
					{
						for ( int w = 0; w < size; w++ )
						{
							int row = r * size + h;
							int col = c * size + w;
							int stack = ( c + r * columns );
							data[ w, h, stack ] = colorData[ col, row ];
						}
					}
				}
			}

			return new LUTResult( data );
		}

		public bool AddLUTFromTexture( LUTSettings lutSettings, Texture2D lutTexture )
		{
			if ( lutSettings == null || lutTexture == null )
			{
				return false;
			}

			var size = lutTexture.height;

			if ( size != lutSettings.Size )
			{
				return false;
			}

			var irt = FromTexture( lutTexture );

			var lut = irt.GetLUT( new LUTSettings( size, 1 ) );

			if ( lut == null )
			{
				return false;
			}

			int columns = lutSettings.Columns;
			int rows = lutSettings.Rows;

			if ( Width < columns * size )
			{
				return false;
			}

			if ( Height < rows * size )
			{
				return false;
			}

			var lutdata = lut.Data;
			var data = ColorData;

			for ( int r = 0; r < rows; r++ )
			{
				for ( int c = 0; c < columns; c++ )
				{
					for ( int h = 0; h < size; h++ )
					{
						for ( int w = 0; w < size; w++ )
						{
							int row = r * size + h;
							int col = c * size + w;
							//data[col, row] = new Color((float)w / size, (float)h / size, (float)(c + r * columns) / size);
							data[ col, row ] = lutdata[ w, size - h - 1, ( c + r * columns ) ];
						}
					}
				}
			}

			lut.Release();

			return true;
		}
	}
}
