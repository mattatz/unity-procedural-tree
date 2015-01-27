Shader "Mattatz/Planet" {

	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	
	SubShader {
		Tags { 
			"RenderType"="Opaque"
			"Queue"="Transparent"
		}
		
		LOD 200
		
		CGPROGRAM
		
		#pragma surface surf Lambert

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			half4 c = tex2D (_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb;
			o.Emission = c.rgb;
			o.Alpha = c.a;
		}
		
		ENDCG
	} 
	FallBack "Diffuse"
}
