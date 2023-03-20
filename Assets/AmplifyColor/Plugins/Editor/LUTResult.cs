// Amplify Color - Advanced Color Grading for Unity
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using System;
using UnityEngine;

namespace AmplifyColor
{
	public class LUTResult
	{
		private readonly Color[ , , ] _data;
		private Texture2D _texture;

		public LUTResult( Color[ , , ] data )
		{
			_data = data;

			GenerateTexture();
		}

		public void Release()
		{
			if ( _texture != null )
			{
				Texture2D.DestroyImmediate( _texture );
				_texture = null;
			}
		}

		private void GenerateTexture()
		{
			if ( _data == null )
			{
				throw new ArgumentNullException();
			}

			int width = _data.GetLength( 0 );
			int length = _data.GetLength( 1 );
			int height = _data.GetLength( 2 );

			if ( width != length || width != height || length != height )
			{
				throw new ArgumentOutOfRangeException();
			}

			int size = width;

			if ( _texture != null )
				Texture2D.DestroyImmediate( _texture );

			_texture = new Texture2D( size * size, size, TextureFormat.ARGB32, false );

			var textureData = new Color[ size * size * size ];

			for ( int w = 0; w < size; w++ )
			{
				for ( int l = 0; l < size; l++ )
				{
					for ( int h = 0; h < size; h++ )
					{
						int index = w + h * size + l * size * size;
						textureData[ index ] = _data[ w, l, h ];
					}
				}
			}

			_texture.SetPixels( textureData );
		}

		public Texture2D Texture
		{
			get { return _texture; }
		}

		public Color[ , , ] Data
		{
			get { return _data; }
		}
	}
}
