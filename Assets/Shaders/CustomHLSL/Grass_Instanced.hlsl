

struct MeshProperties {
	float4x4 mat;
	float3 Normal;
	};
#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
	
	StructuredBuffer<MeshProperties> _PropertiesM;
#endif


void vertInstancingMatrices (inout float4x4 objectToWorld, out float4x4 worldToObject) {
	#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
		MeshProperties data = _PropertiesM[unity_InstanceID];

		objectToWorld = mul(objectToWorld, data.mat);

		// Transform matrix (override current)
		// I prefer keeping positions relative to the bounds passed into DrawMeshInstancedIndirect so use the above instead
		//objectToWorld._11_21_31_41 = float4(data.m._11_21_31, 0.0f);
		//objectToWorld._12_22_32_42 = float4(data.m._12_22_32, 0.0f);
		//objectToWorld._13_23_33_43 = float4(data.m._13_23_33, 0.0f);
		//objectToWorld._14_24_34_44 = float4(data.m._14_24_34, 1.0f);

		// Inverse transform matrix
		float3x3 w2oRotation;
		w2oRotation[0] = objectToWorld[1].yzx * objectToWorld[2].zxy - objectToWorld[1].zxy * objectToWorld[2].yzx;
		w2oRotation[1] = objectToWorld[0].zxy * objectToWorld[2].yzx - objectToWorld[0].yzx * objectToWorld[2].zxy;
		w2oRotation[2] = objectToWorld[0].yzx * objectToWorld[1].zxy - objectToWorld[0].zxy * objectToWorld[1].yzx;

		float det = dot(objectToWorld[0].xyz, w2oRotation[0]);
		w2oRotation = transpose(w2oRotation);
		w2oRotation *= rcp(det);
		float3 w2oPosition = mul(w2oRotation, -objectToWorld._14_24_34);

		worldToObject._11_21_31_41 = float4(w2oRotation._11_21_31, 0.0f);
		worldToObject._12_22_32_42 = float4(w2oRotation._12_22_32, 0.0f);
		worldToObject._13_23_33_43 = float4(w2oRotation._13_23_33, 0.0f);
		worldToObject._14_24_34_44 = float4(w2oPosition, 1.0f);

	#endif
}

void ConfigureProcedual() {
	#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
		vertInstancingMatrices(unity_ObjectToWorld, unity_WorldToObject);
	#endif
	}



void ShaderGraphFunctionM_float (float3 In, out float3 Out) {
	Out = In;
}



void ShaderGraphFunctionM_half (half3 In, out half3 Out) {
	Out = In;
}


void GetCustomNormalsM_float (float3 In, out float3 normal)
{
	normal = float3(0,0,1);
	#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
		normal = _PropertiesM[unity_InstanceID].Normal;
	#endif
}


