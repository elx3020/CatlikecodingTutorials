

struct MeshProperties {
	float3 Position;
	float3 Normal;
	};
#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
	

	StructuredBuffer<MeshProperties> _Properties;
#endif


void ConfigureProcedural () {
	#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
		float3 position = _Properties[unity_InstanceID].Position;
		float3 normal = _Properties[unity_InstanceID].Normal;
		unity_ObjectToWorld = 0.0;
		unity_ObjectToWorld._m03_m13_m23_m33 = float4(position, 1.0);
		unity_ObjectToWorld._m11 = 0.7;
		unity_ObjectToWorld._m00_m22 = 0.7;



		// unity_WorldToObject = 0.0;
		// unity_WorldToObject._m03_m13_m23_m33 = float4(normal, 1.0);
		// unity_WorldToObject._m00_m11_m22 = 1.0;

	#endif
}



void ShaderGraphFunction_float (float3 In, out float3 Out) {
	Out = In;
}



void ShaderGraphFunction_half (half3 In, out half3 Out) {
	Out = In;
}


void GetCustomNormals_float (float3 In, out float3 normal)
{
	normal = float3(0,0,1);
	#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
		normal = _Properties[unity_InstanceID].Normal;
	#endif
}


