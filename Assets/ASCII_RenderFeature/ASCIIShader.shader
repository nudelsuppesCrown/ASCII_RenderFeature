//Based on ASCII shader by Stefan Jovanović : https://github.com/StefanJo3107/ASCII-Rendering-Shader-in-Unity

Shader "Custom/ASCIIShader" {
	Properties{
		_resolutionX("ResolutionX", int) = 1920
		_resolutionY("ResolutionY", int) = 1080
		_MainTex("Base", 2D) = "white" {}
		_CharTex("Character Map", 2D) = "white" {}
		_tilesX("X Characters", int) = 80
		_tilesY("Y Characters", int) = 45
		_monochromatic("Monochromatic", int) = 0
		_charCount("Number of Characters", int) = 8
		_brightness("Darkness", Float) = 0.0
	}

		SubShader{
			Pass{
				CGPROGRAM
				#pragma fragment frag
				#pragma vertex vert_img
				#pragma target 3.0
				#include "UnityCG.cginc"


				struct v2f {
					float4 pos : SV_POSITION;
					float2 uv  : TEXCOORD0;
				};


				sampler2D _MainTex;
				sampler2D _CharTex;
				float _resolutionX;
				float _resolutionY;
				float _tilesX;
				float _tilesY;
				float _tileSize;
				float _brightness;
				int _monochromatic;
				int _charCount;


				float4 frag(v2f i) : COLOR{

					float2 newCoord = float2(saturate(floor(_tilesX * i.uv.x) / (_tilesX)), saturate(floor(_tilesY * i.uv.y) / (_tilesY)));
					float4 col = tex2D(_MainTex, newCoord);
					//for gray we could mult all colors by their respective brightness amount and DONT divide by 3 maybe.
					float gray = saturate((col.r + col.g + col.b)/3.0f);
					int charIndex = round(gray * (_charCount-1));

					//Use current screen resolution to keep it cripsy!
					float2 charCoord = float2(((_resolutionX * i.uv.x) % _tileSize + (_tileSize-1)*charIndex)/ ((_tileSize - 1) * _charCount), saturate(((int)(_resolutionY * i.uv.y) % _tileSize) / (_tileSize-1)));
					float4 charCol = tex2D(_CharTex, charCoord);

					
					if (charCol.r > .8f) {
						if(_monochromatic > 0)
							//this makes lesser value characters darker
							return float4(gray, gray, gray, 1);

							//this makes everything fully white
							//return float4(1, 1, 1, 1);
						else
							return col;
					}
					else {
						return col * _brightness;
					}		
				}
				ENDCG
			}
		}
			FallBack off
}
