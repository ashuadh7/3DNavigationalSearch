// This shader is used to clip a side of a cube mesh
Shader "MTLib/CubicSideClip" {
	Properties {
		[Toggle] _Enable("Enable", Float) = 1
		_MainColor("Main Color", Color) = (1,1,1,1)
		_MainTex("Main Texture", 2D) = "white" {}

		// SideVector is used to specify which side will be clippied.
		// Use normalized forward, back, left, right, up, down vectors only
		_SideVector("Side Vector", Vector) = (0,0,1,0)
		// Local scale of given cube
		_LocalScale("Local Scale", Vector) = (1,1,1,1)
	}

	SubShader {
		Tags { "RenderType"="Transparent" "Queue"="Transparent" "IgnoreProjector"="True"}
		Cull Off

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows addshadow vertex:vert// alpha addshadow

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		struct Input {
			float4 localPos;
			float2 uv_MainTex;
		};

		fixed _Enable;
		float4 _SideVector;
		float4 _LocalScale;
		fixed4 _MainColor;
		sampler2D _MainTex;

		// This function is used to get localPos of the current pixel
		void vert(inout appdata_full v, out Input o) {
        	o.localPos = v.vertex;
        	o.uv_MainTex = float2(0, 0);
       	}

		void surf (Input IN, inout SurfaceOutputStandard o) {
			if (_Enable != 0) {
				if (abs(dot(IN.localPos.xyz * 2, _SideVector.xyz) - abs(dot(_LocalScale.xyz, _SideVector.xyz))) < 0.000001) {
					clip(-1);
				} else {
					clip(1);
				}
			}
			o.Albedo = _MainColor.rgb * tex2D (_MainTex, IN.uv_MainTex);
			o.Alpha = _MainColor.a;
		}

		ENDCG
	}
	Fallback "Cutout/Diffuse"
}
