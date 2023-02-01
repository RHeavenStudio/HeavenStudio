// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Amplify Color - Advanced Color Grading for Unity
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

#ifndef AMPLIFY_COLOR_COMMON_INCLUDED
#define AMPLIFY_COLOR_COMMON_INCLUDED

#include "UnityCG.cginc"

// Enabling Stereo adjustment in versions prior to 4.5
#ifndef UnityStereoScreenSpaceUVAdjust
	#ifdef UNITY_SINGLE_PASS_STEREO
		inline float2 UnityStereoScreenSpaceUVAdjustInternal ( float2 uv, float4 scaleAndOffset )
		{
			return saturate ( uv.xy ) * scaleAndOffset.xy + scaleAndOffset.zw;
		}

		inline float4 UnityStereoScreenSpaceUVAdjustInternal ( float4 uv, float4 scaleAndOffset )
		{
			return float4( UnityStereoScreenSpaceUVAdjustInternal ( uv.xy, scaleAndOffset ), UnityStereoScreenSpaceUVAdjustInternal ( uv.zw, scaleAndOffset ) );
		}
		#define UnityStereoScreenSpaceUVAdjust(x, y) UnityStereoScreenSpaceUVAdjustInternal(x, y)
	#else
		#define UnityStereoScreenSpaceUVAdjust(x, y) x
	#endif
#endif

#ifndef UNITY_DECLARE_SCREENSPACE_TEXTURE
	#define UNITY_DECLARE_SCREENSPACE_TEXTURE( tex ) sampler2D tex;
#endif
#ifndef UNITY_SAMPLE_SCREENSPACE_TEXTURE
	#define UNITY_SAMPLE_SCREENSPACE_TEXTURE tex2D
#endif
#ifndef UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX
	#define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( x )
#endif

UNITY_DECLARE_SCREENSPACE_TEXTURE( _MainTex );
uniform float4 _MainTex_TexelSize;
uniform float4 _MainTex_ST;
uniform float4 _StereoScale;
uniform sampler2D _RgbTex;
uniform sampler2D _LerpRgbTex;
uniform sampler2D _RgbBlendCacheTex;
uniform sampler2D _MaskTex;
uniform float4 _MaskTex_TexelSize;
#if UNITY_VERSION >= 560
UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
#else
sampler2D_float _CameraDepthTexture;
#endif
uniform sampler2D _DepthCurveLut;
uniform float _LerpAmount;
uniform float _Exposure;
uniform float _ShoulderStrength;
uniform float _LinearStrength;
uniform float _LinearAngle;
uniform float _ToeStrength;
uniform float _ToeNumerator;
uniform float _ToeDenominator;
uniform float _LinearWhite;

inline float4 CustomObjectToClipPos( in float3 pos )
{
#if UNITY_VERSION >= 540
	return UnityObjectToClipPos( pos );
#else
	return mul( UNITY_MATRIX_VP, mul( unity_ObjectToWorld, float4( pos, 1.0 ) ) );
#endif
}

// Enabling Stereo adjustment in versions prior to 4.5
#ifndef UnityStereoScreenSpaceUVAdjust
	#ifdef UNITY_SINGLE_PASS_STEREO
		inline float2 UnityStereoScreenSpaceUVAdjustInternal ( float2 uv, float4 scaleAndOffset )
		{
			return saturate ( uv.xy ) * scaleAndOffset.xy + scaleAndOffset.zw;
		}

		inline float4 UnityStereoScreenSpaceUVAdjustInternal ( float4 uv, float4 scaleAndOffset )
		{
			return float4( UnityStereoScreenSpaceUVAdjustInternal ( uv.xy, scaleAndOffset ), UnityStereoScreenSpaceUVAdjustInternal ( uv.zw, scaleAndOffset ) );
		}
		#define UnityStereoScreenSpaceUVAdjust(x, y) UnityStereoScreenSpaceUVAdjustInternal(x, y)
	#else
		#define UnityStereoScreenSpaceUVAdjust(x, y) x
	#endif
#endif

struct appdata
{
    float4 vertex : POSITION;
    half2 texcoord : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2f
{
	float4 pos : SV_POSITION;
	float4 screenPos : TEXCOORD0;
	float4 uv01 : TEXCOORD1;
	float4 uv01Stereo : TEXCOORD2;
	UNITY_VERTEX_OUTPUT_STEREO
};

v2f vert( appdata v )
{
	v2f o;
	UNITY_SETUP_INSTANCE_ID( v );
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
	o.pos = CustomObjectToClipPos( v.vertex );
	o.screenPos = ComputeScreenPos( o.pos );
	o.uv01.xy = v.texcoord.xy;
	o.uv01.zw = v.texcoord.xy;

#if defined( UNITY_UV_STARTS_AT_TOP )
	if ( _MainTex_TexelSize.y < 0 )
		o.uv01.w = 1 - o.uv01.w;
#endif

#if defined( UNITY_HALF_TEXEL_OFFSET )
	o.uv01.zw += _MaskTex_TexelSize.xy * float2( -0.5, 0.5 );
#endif
	o.uv01Stereo = UnityStereoScreenSpaceUVAdjust( o.uv01, _MainTex_ST );
	return o;
}

inline float4 to_srgb( float4 c )
{
	c.rgb = max( 1.055 * pow( c.rgb, 0.416666667 ) - 0.055, 0 );
	return c;
}

inline float4 to_linear( float4 c )
{
	c.rgb = c.rgb * ( c.rgb * ( c.rgb * 0.305306011 + 0.682171111 ) + 0.012522878 );
	return c;
}

#define TONEMAPPING_DISABLED ( 0 )
#define TONEMAPPING_PHOTO ( 1 )
#define TONEMAPPING_HABLE ( 2 )
#define TONEMAPPING_ACES ( 3 )

inline float3 tonemap_photo( float3 color )
{
	color *= _Exposure;
    return 1.0 - exp2( -color );
}

inline float3 tonemap_hable( float3 color )
{
	// Uncharted 2 tone mapping operator, by John Hable
	// http://www.gdcvault.com/play/1012459/Uncharted_2__HDR_Lighting
	// http://filmicgames.com/archives/75

	const float A = _ShoulderStrength;
	const float B = _LinearStrength;
	const float C = _LinearAngle;
	const float D = _ToeStrength;
	const float E = _ToeNumerator;
	const float F = _ToeDenominator;
	const float W = _LinearWhite;

	float4 x = float4( color.rgb * _Exposure * 2.0, _LinearWhite );
	x = ( ( x * ( A * x + C * B ) + D *  E ) / ( x * ( A * x + B ) + D *  F ) ) - E / F;
	return x.rgb / x.w;
}

inline float3 tonemap_aces( float3 color )
{
	// ACES Filmic Tone Mapping Curve, by Krzysztof Narkowicz
	// https://knarkowicz.wordpress.com/2016/01/06/aces-filmic-tone-mapping-curve/

	color *= _Exposure;

	const float a = 2.51;
    const float b = 0.03;
    const float c = 2.43;
    const float d = 0.59;
    const float e = 0.14;

    return saturate( ( color * ( a * color + b ) ) / ( color * ( c * color + d ) + e ) );
}

inline float3 tonemap( const int tonemapper, float3 color )
{
	if ( tonemapper == TONEMAPPING_PHOTO )
	{
		color.rgb = tonemap_photo( color.rgb );
	}
	else if ( tonemapper == TONEMAPPING_HABLE )
	{
		color.rgb = tonemap_hable( color.rgb );
	}
	else if ( tonemapper == TONEMAPPING_ACES )
	{
		color.rgb = tonemap_aces( color.rgb );
	}
	else
	{
		color.rgb *= _Exposure;
	}
	return color;
}

inline float4 safe_tex2D( sampler2D tex, float2 uv )
{
#if ( UNITY_VERSION >= 540 ) && !defined( SHADER_API_D3D11_9X )
	return tex2Dlod( tex, float4( uv, 0, 0 ) );
#else
	return tex2D( tex, uv );
#endif
}

inline float3 screen_space_dither( float4 screenPos )
{
	// Iestyn's RGB dither (7 asm instructions) from Portal 2 X360, slightly modified for VR
	// http://alex.vlachos.com/graphics/Alex_Vlachos_Advanced_VR_Rendering_GDC2015.pdf

	float3 d = dot( float2( 171.0, 231.0 ), UNITY_PROJ_COORD( screenPos ).xy * _MainTex_TexelSize.zw ).xxx;
	d.rgb = frac( d.rgb / float3( 103.0, 71.0, 97.0 ) ) - float3( 0.5, 0.5, 0.5 );
	return d.rgb / 255.0;
}

inline void init_frag( inout v2f i )
{
	UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( i );
}

inline float4 fetch_process_ldr_gamma( v2f i, const bool mobile )
{
	// fetch
	float4 color = UNITY_SAMPLE_SCREENSPACE_TEXTURE( _MainTex, i.uv01Stereo.xy );

	// clamp
	if ( mobile )
	{
		color.rgb = min( ( 0.999 ).xxx, color.rgb ); // dev/hw compatibility
	}
	return color;
}

inline float4 fetch_process_ldr_linear( v2f i, const bool mobile )
{
	// fetch
	float4 color = UNITY_SAMPLE_SCREENSPACE_TEXTURE( _MainTex, i.uv01Stereo.xy );

	// convert to gamma
	color = to_srgb( color );

	// clamp
	if ( mobile )
	{
		color.rgb = min( ( 0.999 ).xxx, color.rgb ); // dev/hw compatibility
	}
	return color;
}

inline float4 fetch_process_hdr_gamma( v2f i, const bool mobile, const bool dithering, const int tonemapper )
{
	// fetch
	float4 color = UNITY_SAMPLE_SCREENSPACE_TEXTURE( _MainTex, i.uv01Stereo.xy );

	// tonemap
	color.rgb = tonemap( tonemapper, color.rgb );

	// dither
	if ( dithering )
	{
		color.rgb += screen_space_dither( i.screenPos );
	}

	// clamp
	if ( mobile )
	{
		color.rgb = clamp( ( 0.0 ).xxx, ( 0.999 ).xxx, color.rgb ); // dev/hw compatibility
	}
	else
	{
		color.rgb = saturate( color.rgb );
	}
	return color;
}

inline float4 fetch_process_hdr_linear( v2f i, const bool mobile, const bool dithering, const int tonemapper )
{
	// fetch
	float4 color = UNITY_SAMPLE_SCREENSPACE_TEXTURE( _MainTex, i.uv01Stereo.xy );

	// tonemap
	color.rgb = tonemap( tonemapper, color.rgb );

	// convert to gamma
	color = to_srgb( color );

	// dither
	if ( dithering )
	{
		color.rgb += screen_space_dither( i.screenPos );
	}

	// clamp
	if ( mobile )
	{
		color.rgb = clamp( ( 0.0 ).xxx, ( 0.999 ).xxx, color.rgb ); // dev/hw compatibility
	}
	else
	{
		color.rgb = saturate( color.rgb );
	}
	return color;
}

inline float4 output_ldr_gamma( float4 color )
{
	return color;
}

inline float4 output_hdr_gamma( float4 color )
{
	return color;
}

inline float4 output_ldr_linear( float4 color )
{
	return to_linear( color );
}

inline float4 output_hdr_linear( float4 color )
{
	return to_linear( color );
}

inline float3 apply_lut( float3 color, sampler2D lut, const bool mobile )
{
	const float4 coord_scale = float4( 0.0302734375, 0.96875, 31.0, 0.0 );
	const float4 coord_offset = float4( 0.00048828125, 0.015625, 0.0, 0.0 );
	const float2 texel_height_X0 = float2( 0.03125, 0.0 );

	float3 coord = color * coord_scale + coord_offset;

	if ( mobile )
	{
		float3 coord_floor = floor( coord + 0.5 );
		float2 coord_bot = coord.xy + coord_floor.zz * texel_height_X0;

		color.rgb = safe_tex2D( lut, coord_bot ).rgb;
	}
	else
	{
		float3 coord_frac = frac( coord );
		float3 coord_floor = coord - coord_frac;

		float2 coord_bot = coord.xy + coord_floor.zz * texel_height_X0;
		float2 coord_top = coord_bot + texel_height_X0;

		float3 lutcol_bot = safe_tex2D( lut, coord_bot ).rgb;
		float3 lutcol_top = safe_tex2D( lut, coord_top ).rgb;

		color.rgb = lerp( lutcol_bot, lutcol_top, coord_frac.z );
	}

	return color;
}

inline float4 apply( float4 color, const bool mobile )
{
	color.rgb = apply_lut( color.rgb, _RgbTex, mobile );
	return color;
}

inline float4 apply_blend( float4 color, const bool mobile )
{
	color.rgb = apply_lut( color.rgb, _RgbBlendCacheTex, mobile );
	return color;
}

#endif // AMPLIFY_COLOR_COMMON_INCLUDED
