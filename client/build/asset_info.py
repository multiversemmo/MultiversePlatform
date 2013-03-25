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

ui_module_files = \
    [ "betaworld.toc",
      "basic.toc"
      ]

material_files = \
    [ "orc_fantasy_rig.material",
      "hero_fantasy_rig.material",
      "hero_nexus_lsleeve_rig.material",
      "hero_nexus_tshirt_rig.material",
      "girl_fantasy_rig.material",
      "girl_nexus_lsleeve_rig.material",
      "girl_nexus_tshirt_rig.material",
      "human_house_open.material",
      "human_house_stilt.material",
      "human_house_stilt_a.material",
      "human_meeting_house.material",
      "human_shack_tall.material",
      "human_town_gate.material",
      "axe.material",
      "dagger.material",
      "pick.material",
      "spear.material",
      "sword.material",
      "bang.material",
      "orc_house.material",
      "portal.material",
      "rocks.material",
      "obelisk.material",
      "shack.material",
      "tower.material",
      "brax.material",
      "crocodile.material",
      "human_male.material",
      "human_female.material",      
      "wolf.material",
      "Multiverse.material",
      "Grass.material",
      "Trees.material",
      "MVSMTerrain.material",
      "Water.material",
      "Ocean.material",
      "Terrain.material",
      "directional_marker.material"
      ]

palma_mesh_files = \
    [[ "Orc",
       "orc_fantasy_rig.mesh",
       "orc.skeleton"
       ],
     [ "Human male - leather",
       "hero_fantasy_rig.mesh",
       "hero_fantasy.skeleton"
       ],
     [ "Human male - long sleeve",
       "hero_nexus_lsleeve_rig.mesh",
       "hero.skeleton",
       ],
     [ "Human male - tshirt",
       "hero_nexus_tshirt_rig.mesh",
       "hero.skeleton",
       ],
     [ "Human female - leather",
       "girl_fantasy_rig.mesh",
       "girl.skeleton",
       ],
     [ "Human female - long sleeve",
       "girl_nexus_lsleeve_rig.mesh",
       "girl.skeleton",
       ],
     [ "Human female - tshirt",
       "girl_nexus_tshirt_rig.mesh",
       "girl.skeleton",
       ]
     ]

mob_mesh_files = \
    [[ "Brax",
       "brax.mesh",
       "brax.skeleton",
       ],
     [ "Crocodile",
       "crocodile.mesh",
       "crocodile.skeleton",
       ],
     [ "Human male",
       "human_male.mesh",
       "human_male.skeleton",
       ],
     [ "Human female",
       "human_female.mesh",
       "human_female.skeleton",
       ],
     [ "Wolf",
       "wolf.mesh",
       "wolf.skeleton",
       ],
     ]

cogswell_mesh_files = \
    [[ "Human house - Open",
       "human_house_open.mesh",
       None,
       ],
     [ "Human house - Stilts",
       "human_house_stilt.mesh",
       None,
       ],
     [ "Human house - Stilts 2",
       "human_house_stilt_a.mesh",
       None,
       ],
     [ "Human meeting house",
       "human_meeting_house.mesh",
       None,
       ],
     [ "Human shack - tall",
       "human_shack_tall.mesh",
       None,
       ],
     [ "Human town gate",
       "human_town_gate.mesh",
       None,
       ]
     ]

equipment_mesh_files = \
    [[ "Axe",
       "axe.mesh",
       None,
       ],
     [ "Dagger",
       "dagger.mesh",
       None,
       ],
     [ "Pick",
       "pick.mesh",
       None,
       ],
     [ "Spear",
       "spear.mesh",
       None,
       ],
     [ "Sword",
       "sword.mesh",
       None,
       ],
     [ "Orc Sword",
       "orcsword.mesh",
       None,
       ],
     [ "Quest Marker",
       "bang.mesh",
       None,
       ]
     ]

misc_mesh_files = \
    [[ "Orc house",
       "orc_house.mesh",
       None,
       ],
     [ "Portal",
       "portal.mesh",
       None,
       ],
     [ "Rocks",
       "rocks.mesh",
       None,
       ],
     [ "Obelisk",
       "obelisk.mesh",
       None,
       ],
     [ "Obelisk",
       "obelisk.mesh",
       None,
       ],
     [ "Human shack",
       "shack.mesh",
       None,
       ],
     [ "Tower",
       "tower.mesh",
       None,
       ]
     ]

tool_mesh_files = \
    [[ "Boundary Marker",
       "directional_marker.mesh",
       None,
       ]
     ]

speedtree_files = \
    [[ "American Boxwood",
       "AmericanBoxwood_RT.spt",
       [ "blank.dds",
         "AmericanBoxwood_Composite.dds"
         ]
       ],
     [ "American Holly",
       "AmericanHolly_RT.spt",
       [ "AmericanHollyBark.dds",
         "AmericanHolly_Composite.dds"
         ]
       ],
     [ "Amur Cork - Late Summer",
       "AmurCork_RT_LateSummer.spt",
       [ "AmurCorkBark.dds",
         "AmurCork_LateSummer_Composite.dds"
         ]
       ],
     [ "Arizona Bush - Flowers",
       "ArizonaBush_RT_Flowers.spt",
       [ "blank.dds",
         "ArizonaBush_Flowers_Composite.dds"
         ]
       ],
     [ "Banana Tree",
       "BananaTree_RT.spt",
       [ "BananaTreeBark.dds",
         "BananaTree_Composite.dds"
         ]
       ],
     [ "Baobab",
       "Baobab_RT.spt",
       [ "BaobabBark.dds",
         "Baobab_Composite.dds"
         ]
       ],
     [ "California Buckeye - Nuts",
       "CaliforniaBuckeye_RT_Nuts.spt",
       [ "CaliforniaBuckeyeBark.dds",
         "CaliforniaBuckeye_Nuts_Composite.dds"
         ]
       ],
     [ "Cedar Of Lebanon",
       "CedarOfLebanon_RT.spt",
       [ "CedarOfLebanonBark.dds",
         "CedarofLebanon_Composite.dds"
         ]
       ],
     [ "Cercropia",
       "Cercropia_RT.spt",
       [ "CercropiaBark.dds",
         "Cercropia_Composite.dds"
         ]
       ],
     [ "Cherry Tree - Spring",
       "CherryTree_RT_Spring.spt",
       [ "CherryTreeBark.dds",
         "CherryTree_Spring_Composite.dds"
         ]
       ],
     [ "Christmas Scotch Pine",
       "ChristmasScotchPine_RT.spt",
       [ "ChristmasScotchPineBark.dds",
         "ChristmasScotchPine_Composite.dds"
         ]
       ],
     [ "Cinnamon Fern",
       "CinnamonFern_RT.spt",
       [ "CinnamonFernBark.dds",
         "CinnamonFern_Composite.dds"
         ]
       ],
     [ "Coconut Palm",
       "CoconutPalm_RT.spt",
       [ "CoconutPalmBark.dds",
         "CoconutPalm_Composite.dds"
         ]
       ],
     [ "Colvillea Racemosa",
       "ColvilleaRacemosa_RT.spt",
       [ "ColvilleaRacemosaBark.dds",
         "ColvilleaRacemosa_Composite.dds"
         ]
       ],
     [ "Colvillea Racemosa - Flower",
       "ColvilleaRacemosa_RT_Flower.spt",
       [ "ColvilleaRacemosaBark.dds",
         "ColvilleaRacemosa_Flower.dds"
         ]
       ],
     [ "Common Olive - Summer",
       "CommonOlive_RT_Summer.spt",
       [ "CommonOliveBark.dds",
         "CommonOlive_Summer.dds"
         ]
       ],
     [ "Crepe Myrtle - Flowers",
       "Crepe Myrtle_RT_Flowers.spt",
       [ "CrepeMyrtleBark.dds",
         "CrepeMyrtle_Flowers_Composite.dds"
         ]
       ],
     [ "Crepe Myrtle - Flowers",
       "Crepe Myrtle_RT_Winter.spt",
       [ "CrepeMyrtleBark.dds",
         "CrepeMyrtle_Winter_Composite.dds"
         ]
       ],
     [ "Curly Palm",
       "CurlyPalm_RT.spt",
       [ "CurlyPalmBark.dds",
         "CurlyPalm_Composite.dds"
         ]
       ],
     [ "Date Palm",
       "DatePalm_RT.spt",
       [ "DatePalmBark.dds",
         "DatePalm_Composite.dds"
         ]
       ],
     [ "English Oak",
       "EnglishOak_RT.spt",
       [ "EnglishOakBark.dds",
         "EnglishOak_Composite.dds"
         ]
       ],
     [ "Fan Palm",
       "FanPalm_RT.spt",
       [ "FanPalmBark.dds",
         "FanPalm_Composite.dds"
         ]
       ],
     [ "Italian Cypress",
       "ItalianCypress_RT.spt",
       [ "ItalianCypressBark.dds",
         "ItalianCypress_Composite.dds"
         ]
       ],
     [ "Japanese Angelica - Summer",
       "JapaneseAngelica_RT_Summer.spt",
       [ "JapaneseAngelicaBark.dds",
         "JapaneseAngelica_Composite.dds"
         ]
       ],
     [ "Japanese Maple - Summer",
       "JapaneseMaple_RT_Summer.spt",
       [ "JapaneseMapleBark.dds",
         "JapaneseMaple_Summer_Composite.dds"
         ]
       ],
     [ "Joshua Tree",
       "JoshuaTree_RT.spt",
       [ "JoshuaTreeBark.dds",
         "JoshuaTree_Composite.dds"
         ]
       ],
     [ "Jungle Brush",
       "JungleBrush_RT.spt",
       [ "JungleBrushBase.dds",
         "JungleBrush_Composite.dds"
         ]
       ],
     [ "Korean Stewartia",
       "KoreanStewartia_RT.spt",
       [ "KoreanStewartiaBark.dds",
         "KoreanStewartia_Composite.dds"
         ]
       ],
     [ "Manchurian Angelica Tree - Small",
       "ManchurianAngelicaTree_RT_Small.spt",
       [ "AraliaManchurianBark.dds",
         "ManchurianAngelica_Small_Composite.dds"
         ]
       ],
     [ "Mimosa Tree",
       "MimosaTree_RT.spt",
       [ "MimosaBark.dds",
         "Mimosa_Composite.dds"
         ]
       ],
     [ "Mimosa Tree - Flower",
       "MimosaTree_RT_Flower.spt",
       [ "MimosaBark.dds",
         "Mimosa_Flower_Composite.dds"
         ]
       ],
     [ "Mulga - Flowers",
       "Mulga_RT_Flowers.spt",
       [ "MulgaBark.dds",
         "Mulga_Flower_Composite.dds"
         ]
       ],
     [ "North Island Rata - Spring",
       "NorthIslandRata_RT_Spring.spt",
       [ "RataBark.dds",
         "NorthIslandRata_Spring_Composite.dds"
         ]
       ],
     [ "Omen Tree",
       "OmenTree_RT.spt",
       [ "OmenTreeBark.dds",
         "OmenTree_Composite.dds"
         ]
       ],
     [ "Oriental Spruce",
       "OrientalSpruce_RT.spt",
       [ "OrientalSpruceBark.dds",
         "OrientalSpruce_Composite.dds"
         ]
       ],
     [ "Ponytail Palm",
       "PonytailPalm_RT.spt",
       [ "PonytailPalmBark.dds",
         "PonytailPalm_Composite.dds"
         ]
       ],
     [ "Queen Palm",
       "QueenPalm_RT.spt",
       [ "QueenPalmBark.dds",
         "QueenPalm_Composite.dds"
         ]
       ],
     [ "Spider Tree",
       "SpiderTree_RT.spt",
       [ "SpiderTreeBark.dds",
         "SpiderTree_Composite.dds"
         ]
       ],
     [ "Spider Tree - Dead",
       "SpiderTree_RT_Dead.spt",
       [ "SpiderTreeBark.dds",
         "SpiderTree_Dead_Composite.dds"
         ]
       ],
     [ "Stump",
       "Stump_RT.spt",
       [ "Vines.dds",
         "Stump_Composite.dds"
         ]
       ],
     [ "Tamarind - Spring",
       "Tamarind_RT_Spring.spt",
       [ "TamarindBark.dds",
         "Tamarind_Spring_Composite.dds"
         ]
       ],
     [ "Umbrella Thorn - Flowers",
       "UmbrellaThorn_RT_Flowers.spt",
       [ "UmbrellaThornBark.dds",
         "UmbrellaThorn_Flowers_Composite.dds"
         ]
       ],
     [ "Weeping Willow",
       "WeepingWillow_RT.spt",
       [ "WeepingWillowBark.dds",
         "WeepingWillow_Composite.dds"
         ]
       ]
     ]

environment_sound_files = \
    [[ "New Fountain",
       "newfountain.wav"
       ],
     [ "Ambient 1 (Ambient)",
       "ambient.wav"
       ],
     [ "Combat 1",
       "combat.wav"
       ],
     [ "Ambient 2 (Fog)",
       "fog.wav"
       ],
     [ "Ambient 3 (Market Time)",
       "market_time.wav"
       ],
     [ "Waterflow",
       "waterflow.wav"
       ]
     ]

mob_sound_files = \
    [[ "Run (grass)",
       "grassrun.wav"
       ],
     [ "Walk (grass)",
       "grasswalk.wav"
       ],
     [ "Walk (gravel)",
       "gravelwalk.wav"
       ],
     [ "Death (man)",
       "mandie.wav"
       ],
     [ "Attack 1 (swordhit)",
       "swordhit.wav"
       ],
     [ "Attack 2 (swing)",
       "swing.wav"
       ],
     [ "Recoil (man)",
       "ugh.wav"
       ]
     ]

font_files = \
    [ "ARIALN.TTF",
      "FRIZQT__.TTF",
      "MORPHEUS.TTF",
      "SKURRI.TTF"
     ]

skyboxes = \
    [[ "Blue Sky/Clouds",
       "Multiverse.material",
       "Multiverse/SceneSkyBox"
       ],
     [ "Sunset",
       "Multiverse.material",
       "Multiverse/SceneSkyBoxSunset"
       ],
     ]

# deprecated_mesh_files = \
#     rock1.mesh \
#     rock2.mesh \
#     rock3.mesh \
#     largetree1.mesh \
#     demotree1.mesh \
#     demotree2.mesh \
#     demotree4.mesh \
#     tree2.mesh \
#     tree3.mesh \
#     well.mesh \
#     tavern.mesh \
#     chapel.mesh \
#     smith.mesh \
#     shroom1.mesh \
#     mill.mesh \
#     plaza-ground.mesh \
#     fountain.mesh \
#     Tabla01.mesh \
#     marker.mesh

