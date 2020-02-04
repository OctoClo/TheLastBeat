Shader "Unlit/TransparentSio"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Color("Color", Color) = (1, 1, 1, 1)
		_EdgeColor("Edge Color", Color) = (1, 1, 1, 1)
		_Bias("Bias", Range(-1,1)) = 1.0
	}

		SubShader
		{
			Tags
			{
				"Queue" = "Transparent"
				"RenderType" = "Transparent"
			}

			Pass
			{
				Cull Off
				ZWrite Off
				Blend SrcAlpha OneMinusSrcAlpha // standard alpha blending

				CGPROGRAM

				#pragma vertex vert
				#pragma fragment frag

			// Properties
			sampler2D		_MainTex;
			uniform float4	_Color;
			uniform float4	_EdgeColor;
			uniform float   _Bias;

			struct vertexInput
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float3 texCoord : TEXCOORD0;
			};

			struct vertexOutput
			{
				float4 pos : SV_POSITION;
				float3 normal : NORMAL;
				float3 texCoord : TEXCOORD0;
				float3 viewDir : TEXCOORD1;
			};

			vertexOutput vert(vertexInput input)
			{
				vertexOutput output;

				// convert input to world space
				output.pos = UnityObjectToClipPos(input.vertex);
				float4 normal4 = float4(input.normal, 0.0);
				output.normal = normalize(mul(normal4, unity_WorldToObject).xyz);
				output.viewDir = normalize(_WorldSpaceCameraPos - mul(unity_ObjectToWorld, input.vertex).xyz);

				output.texCoord = input.texCoord;

				return output;
			}

			float4 frag(vertexOutput input) : COLOR
			{
				// sample texture for color
				float4 texColor = tex2D(_MainTex, input.texCoord.xy);
				float edgeFactor = abs(dot(input.viewDir, input.normal)) + _Bias;
				edgeFactor = clamp(edgeFactor, 0, 1);
				float4 output = lerp(_Color, _EdgeColor, edgeFactor);
				return output;
			}

			ENDCG
		}
		}
}
