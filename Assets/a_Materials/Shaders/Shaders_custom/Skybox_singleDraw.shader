Shader "Custom/Skybox_singleDraw" {
Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}
}

SubShader {

	Tags { "Queue"="Background" "RenderType"="Background" }
	Cull Off ZWrite Off Fog { Mode Off }

	LOD 100
	
	Pass {
		Lighting Off
		SetTexture [_MainTex] { combine texture } 
	}

    }
}
