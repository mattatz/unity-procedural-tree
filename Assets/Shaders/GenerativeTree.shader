Shader "mattatz/GenerativeTree" {

	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	SubShader {
		Tags { 
			"RenderType"="Opaque"
		}
		LOD 200
		ZWrite On
		Cull Back
		
		CGPROGRAM
		#pragma surface surf Lambert

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			float2 uv = IN.uv_MainTex;
			uv.y = fmod(uv.y * 10, 1.0);
			half4 c = tex2D (_MainTex, uv);
			o.Albedo = c.rgb;
			o.Emission = c.rgb * 0.2;
			o.Alpha = c.a;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
