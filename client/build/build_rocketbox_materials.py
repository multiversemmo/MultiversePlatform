#
#
#  The Multiverse Platform is made available under the MIT License.
#
#  Copyright (c) 2012 The Multiverse Foundation
#
#  Permission is hereby granted, free of charge, to any person 
#  obtaining a copy of this software and associated documentation 
#  files (the "Software"), to deal in the Software without restriction, 
#  including without limitation the rights to use, copy, modify, 
#  merge, publish, distribute, sublicense, and/or sell copies 
#  of the Software, and to permit persons to whom the Software 
#  is furnished to do so, subject to the following conditions:
#
#  The above copyright notice and this permission notice shall be 
#  included in all copies or substantial portions of the Software.
#
#  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
#  EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
#  OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
#  NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
#  HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
#  WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
#  FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE 
#  OR OTHER DEALINGS IN THE SOFTWARE.
#
#  

#!/usr/bin/python

model_info = \
	[[ "business01_m_highpoly",
	   [ "body" ],
	   [ "business01_m_60" ]
 	 ],
	 [ "business02_m_highpoly",
	   [ "body" ],
	   [ "business02_m_35" ]
 	 ],
	 [ "business03_m_highpoly",
	   [ "body" ],
	   [ "business03_m_35" ]
 	 ],
	 [ "business04_m_highpoly",
	   [ "body" ],
	   [ "business04_m_35" ]
 	 ],
	 [ "business05_m_highpoly",
	   [ "body" ],
	   [ "business05_m_25" ]
 	 ],
	 [ "business06_m_highpoly",
	   [ "body" ],
	   [ "business06_m_25" ]
 	 ],
	 [ "business07_m_highpoly",
	   [ "body" ],
	   [ "business07_m_25" ]
 	 ],
	 [ "child01_m_highpoly",
	   [ "body" ],
	   [ "child01_m" ]
 	 ],
	 [ "child02_m_highpoly",
	   [ "body" ],
	   [ "child02_m" ]
 	 ],
	 [ "nude01_m_highpoly",
	   [ "body", "hair_transparent" ],
	   [ "nude01_m_25", "nude01_m_25_hair" ]
 	 ],
	 [ "nude02_m_highpoly",
	   [ "body" ],
	   [ "nude_02_m_25" ]
 	 ],
	 [ "nude03_m_highpoly",
	   [ "body" ],
	   [ "nude_03_m_25" ]
 	 ],
	 [ "nude04_m_highpoly",
	   [ "body" ],
	   [ "nude04_m_25" ]
 	 ],
	 [ "nude08_m_highpoly",
	   [ "body" ],
	   [ "nude08_m_25" ]
 	 ],
	 [ "nude09_m_highpoly",
	   [ "body" ],
	   [ "nude09_m_25" ]
 	 ],
	 [ "nude10_m_highpoly",
	   [ "body" ],
	   [ "nude_10_m_25" ]
 	 ],
	 [ "soccerplayer01_m_highpoly",
	   [ "body" ],
	   [ "soccerplayer01_m" ]
 	 ],
	 [ "sportive01_m_highpoly",
	   [ "body" ],
	   [ "sportive01_m_20" ]
 	 ],
	 [ "sportive03_m_highpoly",
	   [ "body" ],
	   [ "sportive03_m_25" ]
 	 ],
	 [ "sportive04_m_highpoly",
	   [ "body" ],
	   [ "sportive04_m_25" ]
 	 ],
	 [ "sportive05_m_highpoly",
	   [ "body" ],
	   [ "sportive05_m_30" ]
 	 ],
	 [ "sportive07_m_highpoly",
	   [ "body" ],
	   [ "sportive07_m_25" ]
 	 ],
	 [ "sportive08_m_highpoly",
	   [ "body" ],
	   [ "sportive08_m_25" ]
 	 ],
	 [ "sportive09_m_highpoly",
	   [ "body" ],
	   [ "sportive09_m_25" ]
 	 ],
	 [ "sportive10_m_highpoly",
	   [ "body" ],
	   [ "sportive10_m_20" ]
 	 ],
	 [ "sportive11_m_highpoly",
	   [ "body" ],
	   [ "sportive11_m_30" ]
 	 ],
	 [ "business01_f_highpoly",
	   [ "body", "hair_transparent" ],
	   [ "business01_f_30", "business01_f_30_hair" ]
 	 ],
	 [ "business02_f_highpoly",
	   [ "body" ],
	   [ "business01_f_50" ]
 	 ],
	 [ "business03_f_highpoly",
	   [ "body" ],
	   [ "business03_f_25" ]
 	 ],
	 [ "nude01_f_highpoly",
	   [ "body" ],
	   [ "nude01_f_20" ]
 	 ]
	]

material_script = """
material %s.%s
{
    // The concept here is that we make one ambient pass which just
    // provides a bit of lightening, then we run the multiple light
    // passes (additive), then we run the texture pass (multiplicative)
    technique
    {
        // Base ambient pass
        pass
	{
            fog_override true

            // Really basic vertex program
            // NB we don't use fixed function here because GL does not like
            // mixing fixed function and vertex programs, depth fighting can
            // be an issue
            vertex_program_ref Examples/AmbientOneVS
	    {
		// Models with a skeleton will have already had their vertices
		// transformed into world space by the software skinning.
		// Pass in the viewproj_matrix instead of worldviewproj_matrix
                param_named_auto WorldViewProj viewproj_matrix
                param_named_auto ambient ambient_light_colour
            }
	}
        // Now do the lighting pass
        // NB we don't do decal texture here because this is repeated per light
        pass
        {
            fog_override true

            // do this for each light
            iteration once_per_light
    
            scene_blend add
    
            // Vertex program reference
            vertex_program_ref Examples/DiffuseBumpVS
            {
                param_named_auto WorldViewProj viewproj_matrix
                param_named_auto WorldMatrix world_matrix
                param_named_auto LightPosition light_position_object_space 0
                param_named_auto EyePosition camera_position_object_space
            }

            fragment_program_ref Examples/DiffuseBumpPS_20
            {
                param_named_auto LMd light_diffuse_colour 0
                param_named_auto LMs light_specular_colour 0
                param_named shininess float 10
                param_named NormalMap int 0
                param_named GlossMap int 1
	    }

	    // Normal map
	    texture_unit
	    {
		texture %s_normal.DDS
	 	colour_op replace
	    }
	    // Gloss map
	    texture_unit
	    {
		tex_coord_set 1
	        texture %s_spec.DDS
	    }
        }
        // Decal pass
        pass
        {
            fog_override true
            lighting off

            // Really basic vertex program
            // NB we don't use fixed function here because GL does not like
            // mixing fixed function and vertex programs, depth fighting can
            // be an issue
            vertex_program_ref Examples/AmbientOneVS
            {
                param_named_auto WorldViewProj viewproj_matrix
                param_named ambient float4 1 1 1 1
            }
            scene_blend dest_colour zero
            texture_unit
            {
                texture %s.DDS
            }
        }
    }

    // fallback method for machines without the ps_2_0 support
    technique
    {
        // Base ambient pass
        pass
        {
            fog_override true
            // base colours, not needed for rendering, but as information
            // to lighting pass categorisation routine
            ambient 1 1 1
            diffuse 0 0 0
            specular 0 0 0 0

            texture_unit
            {
                texture %s.DDS
            }
        }
    }
}
"""

def write_material(f, model_name, material_suffix, texture_prefix):
    script = material_script % (model_name,
                                material_suffix,
                                texture_prefix,
                                texture_prefix,
                                texture_prefix,
                                texture_prefix)
    f.write(script)

def write_material_script(model_name, texture_prefix_map):
    f = open(model_name + ".material", 'w')
    for material_suffix in texture_prefix_map.keys():
        write_material(f, model_name, material_suffix, texture_prefix_map[material_suffix])
    f.close()

for model_entry in model_info:
    model_name = model_entry[0]
    model_materials = model_entry[1]
    model_textures = model_entry[2]
    texture_prefix_map = {}
    for i in range(len(model_materials)):
        texture_prefix_map[model_materials[i]] = model_textures[i]

    write_material_script(model_name, texture_prefix_map)

