float3 RGBtoXYZ(in float3 i)
{
	if(i.r > 0.04045)
	{
		i.r = pow(((i.r + 0.055) / 1.055) , 2.4);
	}
	else 
	{
		i.r = i.r / 12.92;
	}

	if (i.g > 0.04045)
	{
		i.g = pow(((i.g + 0.055) / 1.055), 2.4);
	}
	else 
	{
		i.g = i.g / 12.92;
	}

	if (i.b > 0.04045)
	{
		i.b = pow(((i.b + 0.055) / 1.055), 2.4);
	}
	else 
	{
		i.b = i.b / 12.92;
	}

	i.r *= 100;
	i.g *= 100;
	i.b *= 100;

	float3 output;

	output.x = (i.r * 0.4124 + i.g * 0.3576 + i.b * 0.1805);
	output.y = (i.r * 0.2126 + i.g * 0.7152 + i.b * 0.0722);
	output.z = (i.r * 0.0193 + i.g * 0.1192 + i.b * 0.9505);

	return output;
}

float3 XYZtoLAB(in float3 i)
{
	float3 reference = float3(94.811, 100.000, 107.304); //Daylight, sRGB, Adobe-RGB according to http://www.easyrgb.com/en/math.php#text2

	float ka = (175.0 / 198.04) * (reference.y + reference.x);
	float kb = (70.0 / 218.11) * (reference.y + reference.z);

	float3 lab;
	lab.r = 100.0 * sqrt(i.y / reference.y);
	lab.g = ka * (((i.x / reference.x) - (i.y / reference.y)) / sqrt(i.y / reference.y));
	lab.b = kb * (((i.y / reference.y) - (i.z / reference.z)) / sqrt(i.y / reference.y));

	return lab;

}

float3 RGBtoLAB(in float3 i)
{
	return XYZtoLAB(RGBtoXYZ(i));
}

float Distance(in float3 c1, in float3 c2)
{
	c1 = RGBtoLAB(c1);
	c2 = RGBtoLAB(c2);
	float distR = c1[0] - c2[0];
	float distG = c1[1] - c2[1];
	float distB = c1[2] - c2[2];
	return distR * distR + distG * distG + distB * distB;
}

float DistanceNoLuminance(in float3 c1, in float3 c2)
{
	c1 = RGBtoLAB(c1);
	c2 = RGBtoLAB(c2);
	//float distR = c1[0] - c2[0];
	float distG = c1[1] - c2[1];
	float distB = c1[2] - c2[2];
	return distG * distG + distB * distB;
}

float DistanceAlreadyLAB(in float3 labC1, in float3 labC2)
{
	float distR = labC1[0] - labC2[0];
	float distG = labC1[1] - labC2[1];
	float distB = labC1[2] - labC2[2];
	return distR * distR + distG * distG + distB * distB;
}

float DistanceNoLuminanceAlreadyLAB(in float3 labC1, in float3 labC2)
{
	//float distR = labC1[0] - labC2[0];
	float distG = labC1[1] - labC2[1];
	float distB = labC1[2] - labC2[2];
	return distG * distG + distB * distB;
}