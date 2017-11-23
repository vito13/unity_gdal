#ifndef GRASS_LIGHTING
#define GRASS_LIGHTING

//Not really pbr lighting any more. I tried to get a similar effect as the trailers of the Wii U Zelda game.
//I used the Unity PBR as a basis and modified it to fit the rest of the shader.
inline half4 FakeGrassLighting(half3 diffColor, half3 specColor, half oneMinusRoughness,
	half3 normal, half3 viewDir,
	UnityLight light, UnityIndirect gi)
{
	half roughness = 1 - oneMinusRoughness;

	half3 halfDir = normalize(light.dir + viewDir);
	half diffuseLH = DotClamped(light.dir, halfDir);
	half diffuseNL = light.ndotl;
	half diffuseNV = DotClamped(normal, viewDir);

	//Mirror light dir and normal to get specular lighting on the same side as the sun/light. 
	//Just because it's not realistic, doesn't mean it doesn't look nice!
	half3 lightDir = half3(-light.dir.x, light.dir.y, -light.dir.z);
	normal = half3(-normal.x, normal.y, -normal.z);

	halfDir = normalize(lightDir + viewDir);
	half nl = DotClamped(normal, lightDir);
	half lh = DotClamped(lightDir, halfDir);
	half nv = DotClamped(normal, viewDir);
	half nh = BlinnTerm(normal, halfDir);


#if UNITY_BRDF_GGX
	half V = SmithGGXVisibilityTerm(diffuseNL, nv, roughness);
	half D = GGXTerm(nh, roughness);
#else
	half V = SmithBeckmannVisibilityTerm(diffuseNL, nv, roughness);
	half D = NDFBlinnPhongNormalizedTerm(nh, RoughnessToSpecPower(roughness));
#endif

	half diffuseNLPow5 = Pow5(1 - diffuseNL);
	half diffuseNVPow5 = Pow5(1 - diffuseNV);
	half Fd90 = 0.5 + 2 * diffuseLH * diffuseLH * roughness;
	half disneyDiffuse = (1 + (Fd90 - 1) * diffuseNLPow5) * (1 + (Fd90 - 1) * diffuseNVPow5);

	// HACK: theoretically we should divide by Pi diffuseTerm and not multiply specularTerm!
	// BUT 1) that will make shader look significantly darker than Legacy ones
	// and 2) on engine side "Non-important" lights have to be divided by Pi to in cases when they are injected into ambient SH
	// NOTE: multiplication by Pi is part of single constant together with 1/4 now

	half specularTerm = (V * D) * (UNITY_PI / 4); // Torrance-Sparrow model, Fresnel is applied later (for optimization reasons)

#if UNITY_VERSION >= 530
	if (IsGammaSpace())
	{
		specularTerm = sqrt(max(1e-4h, specularTerm));
	}
#endif

	specularTerm = max(0, specularTerm * nl);

	half diffuseTerm = disneyDiffuse * diffuseNL;

	//I removed the specular global illumination term. It might be a nice effect, but it looked weird on the grass.
	//half grazingTerm = saturate(oneMinusRoughness + (1 - oneMinusReflectivity));
	half3 color = diffColor * (gi.diffuse + light.color * diffuseTerm)
		+ specularTerm * light.color * FresnelTerm(specColor, lh)
		;// +gi.specular * FresnelLerp(specColor, grazingTerm, nv);

	return half4(color, 1);
}

#endif //GRASS_LIGHTING