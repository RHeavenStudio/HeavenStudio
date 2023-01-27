// Amplify Color - Advanced Color Grading for Unity
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using System;

namespace AmplifyColor
{
	public class LUTSettings
	{
		private int _columns;
		private int _rows;
		private int _size;

		public LUTSettings()
		{
			_size = 32;
			_columns = 8;
			_rows = 4;
		}

		public LUTSettings( int size )
		{
			_size = size;

			RecalcLutSize();
		}

		public LUTSettings( int columns, int rows )
		{
			_columns = columns;
			_rows = rows;
			_size = columns * rows;
		}

		public int Size
		{
			get { return _size; }
		}

		public int Columns
		{
			get { return _columns; }
		}

		public int Rows
		{
			get { return _rows; }
		}

		public int Height
		{
			get { return _rows * _size; }
		}

		public int Width
		{
			get { return _columns * _size; }
		}

		private void RecalcLutSize()
		{
			_size = Math.Min( _size, 256 );

			if ( _size == 0 )
			{
				_columns = 1;
				_rows = 1;
				_size = 1;

				return;
			}

			double colsize;
			double rowsize;

			double root = Math.Sqrt( _size );
			rowsize = Math.Floor( root );

			do
			{
				colsize = _size / rowsize;
				rowsize -= 1.0;
			} while ( colsize != Math.Floor( colsize ) );

			rowsize += 1.0;

			_columns = ( int ) colsize;
			_rows = ( int ) rowsize;
		}

		public void Update( int size, int columns, int rows )
		{
			if ( _size != size )
			{
				_size = Math.Min( size, 64 );
				RecalcLutSize();
			}
			else
			{
				_size = columns * rows;
				_columns = columns;
				_rows = rows;

				if ( size > 64 )
				{
					_size = 64;
					RecalcLutSize();
				}
			}
		}
	}
}
