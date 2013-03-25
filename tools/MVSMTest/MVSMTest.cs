/********************************************************************

The Multiverse Platform is made available under the MIT License.

Copyright (c) 2012 The Multiverse Foundation

Permission is hereby granted, free of charge, to any person 
obtaining a copy of this software and associated documentation 
files (the "Software"), to deal in the Software without restriction, 
including without limitation the rights to use, copy, modify, 
merge, publish, distribute, sublicense, and/or sell copies 
of the Software, and to permit persons to whom the Software 
is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be 
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE 
OR OTHER DEALINGS IN THE SOFTWARE.

*********************************************************************/

using System;
using System.IO;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using Axiom.Core;
using Axiom.Graphics;
using Axiom.MathLib;
using Axiom.Utility;
using Axiom.Input;
using Multiverse;
using Multiverse.Generator;
using System.Diagnostics;
using Axiom.SceneManagers.Multiverse;

namespace Multiverse.Tools.MVSMTest {
	/// <summary>
	/// Summary description for Terrain.
	/// </summary>
	public class MVSMTest : TechDemo {

		private int lastHeightPointsGenerated = 0;

		private Multiverse.Generator.Generator gen;

		private LODSpec lodSpec;
        private LODSpecPrev lodSpecPrev;

		private bool followTerrain = false;
		private bool humanSpeed = false;
		private int lastTick;
		private float time = 0.0f;
		private readonly float oneMeter = 1000f;
        private bool preview = false;
        private bool captureFailed = false;

        private Mesh waterMesh;

        private SceneNode treeSceneNode;
        private Forest forest;
        private Boundary boundary1;
        private Boundary boundary2;
        private Boundary boundary3;

        private ColorEx fogColor = new ColorEx(0.5f, 0.58f, 0.77f);
        private float fogNear = 500 * 1000;

        private int frameCount = 0;
        Axiom.SceneManagers.Multiverse.SceneManager mvScene;

        private Road road1 = null;
        private Road road2 = null;

        protected override void ChooseSceneManager() {
            scene = Root.Instance.SceneManagers.GetSceneManager(SceneType.ExteriorClose);
            mvScene = scene as Axiom.SceneManagers.Multiverse.SceneManager;
        }

        protected override void CreateCamera() {
            camera = scene.CreateCamera("PlayerCam");

//            camera.Position = new Vector3(128, 25, 128);
//            camera.LookAt(new Vector3(0, 0, -300));
//            camera.Near = 1;
//            camera.Far = 384;

			//camera.Position = new Vector3(128 * oneMeter, 800 * oneMeter, 128 * oneMeter);
			//camera.Position = new Vector3(0 * oneMeter, 100 * oneMeter, 3900 * oneMeter);
            //camera.Position = new Vector3(90 * oneMeter, 65 * oneMeter, 4150 * oneMeter);
            //camera.Position = new Vector3(240 * oneMeter, 100 * oneMeter, 4593 * oneMeter);
            //camera.Position = new Vector3(326 * oneMeter, 78 * oneMeter, 4270 * oneMeter);
            camera.Position = new Vector3(339022, 63142, 4282656);
            //camera.LookAt(new Vector3(1, 100 * oneMeter, -300 * oneMeter));
            //camera.LookAt(new Vector3(300, 1 * oneMeter, 3500 * oneMeter));
            camera.LookAt(new Vector3(341022, 63142, 4282656));
			camera.Near = 1 * oneMeter;
			camera.Far = 2000 * oneMeter;

            Console.WriteLine("Right: {0}, Up: {1}", camera.DerivedRight, camera.DerivedUp);
        }

        protected void SetupScene()
        {
            if (preview)
            {
                mvScene.SetWorldParams(gen, lodSpecPrev, 2000 * oneMeter, 2000 * oneMeter, 1024, 4);
                scene.LoadWorldGeometry("");
                WorldManager.Instance.SeaLevel = oneMeter * 10;
                WorldManager.Instance.OceanWaveHeight = oneMeter * 1;
                camera.Far = 10000 * oneMeter;
            }
            else
            {
                mvScene.SetWorldParams(gen, lodSpec, 2000 * oneMeter, 2000 * oneMeter, 256, 4);
                scene.LoadWorldGeometry("");
                camera.Far = 2000 * oneMeter;
                WorldManager.Instance.SeaLevel = oneMeter * 10;
                WorldManager.Instance.OceanWaveHeight = oneMeter * 1;
            }

            camera.Position = camera.Position + new Vector3(0, 1, 0);

            scene.AmbientLight = new ColorEx(1.0f, 0.5f, 0.5f, 0.5f);

            Light light = scene.CreateLight("MainLight");
            light.Type = LightType.Directional;
            Vector3 lightDir = new Vector3(-80 * oneMeter, -70 * oneMeter, -80 * oneMeter);
            //lightDir = Vector3.UnitX;
            lightDir.Normalize();
            light.Direction = lightDir;
            light.Position = -lightDir;
            light.Diffuse = ColorEx.White;
            light.SetAttenuation(1000 * oneMeter, 1, 0, 0);

            scene.SetFog(FogMode.Linear, fogColor, 0.5f, fogNear, fogNear * 2);
            //scene.SetFog(FogMode.None, fogColor, 0.5f, 500 * oneMeter, 1000 * oneMeter);

            //WorldManager.Instance.ShowOcean = false;
            //WorldManager.Instance.DrawStitches = false;
            //WorldManager.Instance.DrawTiles = false;


            // pampas
            WorldManager.Instance.AddDetailVegPlantType(1000, 0.375244f, 0.375244f + 0.124512f, 800, 1200, 800, 1200);

            // sunflower
            WorldManager.Instance.AddDetailVegPlantType(10, 0.625244f, 0.625244f + 0.124512f, 1600, 2400, 1600, 2400);

            // rose
            WorldManager.Instance.AddDetailVegPlantType(10, 0.500244f, 0.500244f + 0.124512f, 1300, 1700, 1300, 1700);

            // cattail
            WorldManager.Instance.AddDetailVegPlantType(10, 0.125244f, 0.125244f + 0.124512f, 1300, 1700, 1300, 1700);

            // marigold
            WorldManager.Instance.AddDetailVegPlantType(10, 0.250244f, 0.250244f + 0.124512f, 1300, 1700, 1300, 1700);

            // bird of paradise
            WorldManager.Instance.AddDetailVegPlantType(10, 0.000244f, 0.000244f + 0.124512f, 1500, 1500, 1500, 1500);

            // grass
            WorldManager.Instance.AddDetailVegPlantType(1000, 0.750244f, 0.750244f + 0.124512f, 1600, 2400, 800, 1200);


            WorldManager.Instance.DetailVegMinHeight = 40000;
            WorldManager.Instance.DetailVegMaxHeight = 150000;
            WorldManager.Instance.ShowDetailVeg = true;

            bool doForest1 = true;
            bool doForest2 = false;
            bool doLakes = false;
            bool doOcean = false;
            bool doBuildings = false;
            bool doRoads = false;
            AddContent(doForest1, doForest2, doLakes, doOcean, doBuildings, doRoads);

            if (!preview)
            {
                scene.SetSkyBox(true, "Multiverse/SceneSkyBox", 1000 * oneMeter);
            }
        }

        private void AddContent(bool drawForest1, bool drawForest2, bool lakes, bool ocean, bool buildings, bool roads)
        {
            if (drawForest1 || drawForest2)
            {
                treeSceneNode = scene.RootSceneNode.CreateChildSceneNode("Trees");
            }


            bool betaWorldForest = false;

            if (betaWorldForest)
            {
                boundary1 = new Boundary("boundary1");
                boundary1.AddPoint(new Vector3(441 * oneMeter, 0, 4269 * oneMeter));
                boundary1.AddPoint(new Vector3(105 * oneMeter, 0, 4278 * oneMeter));
                boundary1.AddPoint(new Vector3(66 * oneMeter, 0, 4162 * oneMeter));
                boundary1.AddPoint(new Vector3(-132 * oneMeter, 0, 4102 * oneMeter));
                boundary1.AddPoint(new Vector3(-540 * oneMeter, 0, 3658 * oneMeter));
                boundary1.AddPoint(new Vector3(-639 * oneMeter, 0, 3570 * oneMeter));
                boundary1.AddPoint(new Vector3(182 * oneMeter, 0, 3510 * oneMeter));
                boundary1.AddPoint(new Vector3(236 * oneMeter, 0, 3845 * oneMeter));
                boundary1.AddPoint(new Vector3(382 * oneMeter, 0, 3966 * oneMeter));
                boundary1.Close();

                //boundary1.Hilight = true;

                mvScene.AddBoundary(boundary1);

                forest = new Forest(1234, "Forest1", treeSceneNode);
                boundary1.AddSemantic(forest);

                forest.WindFilename = "demoWind.ini";

                forest.WindDirection = Vector3.UnitX;
                forest.WindStrength = 0.0f;

                forest.AddTreeType("CedarOfLebanon_RT.spt", 55 * 300, 0, 4);
                forest.AddTreeType("WeepingWillow_RT.spt", 50 * 300, 0, 5);
                forest.AddTreeType("DatePalm_RT.spt", 40 * 300, 0, 16);

                Boundary boundary4 = new Boundary("boundary4");
                boundary4.AddPoint(new Vector3(441 * oneMeter, 0, 4269 * oneMeter));
                boundary4.AddPoint(new Vector3(105 * oneMeter, 0, 4278 * oneMeter));
                boundary4.AddPoint(new Vector3(66 * oneMeter, 0, 4162 * oneMeter));
                boundary4.AddPoint(new Vector3(-132 * oneMeter, 0, 4102 * oneMeter));
                boundary4.AddPoint(new Vector3(-540 * oneMeter, 0, 3658 * oneMeter));
                boundary4.AddPoint(new Vector3(-639 * oneMeter, 0, 3570 * oneMeter));
                boundary4.AddPoint(new Vector3(182 * oneMeter, 0, 3510 * oneMeter));
                boundary4.AddPoint(new Vector3(236 * oneMeter, 0, 3845 * oneMeter));
                boundary4.AddPoint(new Vector3(382 * oneMeter, 0, 3966 * oneMeter));
                boundary4.Close();

                //boundary1.Hilight = true;

                mvScene.AddBoundary(boundary4);

                Forest forest4 = new Forest(1234, "Forest4", treeSceneNode);
                boundary4.AddSemantic(forest);

                forest4.WindFilename = "demoWind.ini";

                forest4.WindDirection = Vector3.UnitX;
                forest4.WindStrength = 1.0f;

                forest4.AddTreeType("DatePalm_RT.spt", 40 * 300, 0, 14);
                forest4.AddTreeType("CoconutPalm_RT.spt", 55 * 300, 0, 23);
                forest4.AddTreeType("CinnamonFern_RT.spt", 70 * 300, 0, 14);

                boundary2 = new Boundary("boundary2");
                boundary2.AddPoint(new Vector3(285 * oneMeter, 0, 3462 * oneMeter));
                boundary2.AddPoint(new Vector3(-679 * oneMeter, 0, 3560 * oneMeter));
                boundary2.AddPoint(new Vector3(-647 * oneMeter, 0, 3381 * oneMeter));
                boundary2.AddPoint(new Vector3(-512 * oneMeter, 0, 3230 * oneMeter));
                boundary2.AddPoint(new Vector3(402 * oneMeter, 0, 3116 * oneMeter));
                boundary2.AddPoint(new Vector3(402 * oneMeter, 0, 3339 * oneMeter));
                boundary2.AddPoint(new Vector3(305 * oneMeter, 0, 3363 * oneMeter));
                boundary2.Close();

                mvScene.AddBoundary(boundary2);

                Forest forest2 = new Forest(1234, "Forest2", treeSceneNode);

                boundary2.AddSemantic(forest2);

                forest2.WindFilename = "demoWind.ini";

                forest2.WindDirection = Vector3.UnitX;
                forest2.WindStrength = 1.0f;

                forest2.AddTreeType("SpiderTree_RT_Dead.spt", 80 * 300, 0, 23);
                forest2.AddTreeType("CinnamonFern_RT.spt", 70 * 300, 0, 12);
                forest2.AddTreeType("CoconutPalm_RT.spt", 55 * 300, 0, 12);

                Boundary boundary3 = new Boundary("boundary3");
                boundary3.AddPoint(new Vector3(285 * oneMeter, 0, 3462 * oneMeter));
                boundary3.AddPoint(new Vector3(-679 * oneMeter, 0, 3560 * oneMeter));
                boundary3.AddPoint(new Vector3(-647 * oneMeter, 0, 3381 * oneMeter));
                boundary3.AddPoint(new Vector3(-512 * oneMeter, 0, 3230 * oneMeter));
                boundary3.AddPoint(new Vector3(402 * oneMeter, 0, 3116 * oneMeter));
                boundary3.AddPoint(new Vector3(402 * oneMeter, 0, 3339 * oneMeter));
                boundary3.AddPoint(new Vector3(305 * oneMeter, 0, 3363 * oneMeter));
                boundary3.Close();

                mvScene.AddBoundary(boundary3);

                Forest forest3 = new Forest(1234, "Forest3", treeSceneNode);

                boundary3.AddSemantic(forest3);

                forest3.WindFilename = "demoWind.ini";

                forest3.WindDirection = Vector3.UnitX;
                forest3.WindStrength = 1.0f;

                forest3.AddTreeType("DatePalm_RT.spt", 40 * 300, 0, 14);
                forest3.AddTreeType("AmericanHolly_RT.spt", 40 * 300, 0, 24);
                forest3.AddTreeType("CoconutPalm_RT.spt", 55 * 300, 0, 23);
                forest3.AddTreeType("SpiderTree_RT_Dead.spt", 80 * 300, 0, 9);
            }

            if (drawForest1)
            {
                boundary1 = new Boundary("boundary1");
                boundary1.AddPoint(new Vector3(441 * oneMeter, 0, 4269 * oneMeter));
                boundary1.AddPoint(new Vector3(105 * oneMeter, 0, 4278 * oneMeter));
                boundary1.AddPoint(new Vector3(66 * oneMeter, 0, 4162 * oneMeter));
                boundary1.AddPoint(new Vector3(-132 * oneMeter, 0, 4102 * oneMeter));
                boundary1.AddPoint(new Vector3(-540 * oneMeter, 0, 3658 * oneMeter));
                boundary1.AddPoint(new Vector3(-639 * oneMeter, 0, 3570 * oneMeter));
                boundary1.AddPoint(new Vector3(182 * oneMeter, 0, 3510 * oneMeter));
                boundary1.AddPoint(new Vector3(236 * oneMeter, 0, 3845 * oneMeter));
                boundary1.AddPoint(new Vector3(382 * oneMeter, 0, 3966 * oneMeter));
                boundary1.Close();

                //boundary1.Hilight = true;

                mvScene.AddBoundary(boundary1);

                forest = new Forest(1234, "Forest1", treeSceneNode);
                boundary1.AddSemantic(forest);

                forest.WindFilename = "demoWind.ini";

                forest.WindDirection = Vector3.UnitX;
                forest.WindStrength = 1.0f;

                //forest.AddTreeType("EnglishOak_RT.spt", 55 * 300, 0, 100);
                //forest.AddTreeType("AmericanHolly_RT.spt", 40 * 300, 0, 100);
                //forest.AddTreeType("ChristmasScotchPine_RT.spt", 70 * 300, 0, 100);

                //forest.AddTreeType("CedarOfLebanon_RT.spt", 55 * 300, 0, 4);
                //forest.AddTreeType("WeepingWillow_RT.spt", 50 * 300, 0, 5);
                //forest.AddTreeType("DatePalm_RT.spt", 40 * 300, 0, 16);
                //forest.AddTreeType("DatePalm_RT.spt", 40 * 300, 0, 14);
                //forest.AddTreeType("CoconutPalm_RT.spt", 55 * 300, 0, 23);
                //forest.AddTreeType("CinnamonFern_RT.spt", 70 * 300, 0, 14);
                //forest.AddTreeType("SpiderTree_RT_Dead.spt", 80 * 300, 0, 23);
                //forest.AddTreeType("CinnamonFern_RT.spt", 70 * 300, 0, 12);
                //forest.AddTreeType("CoconutPalm_RT.spt", 55 * 300, 0, 12);
                //forest.AddTreeType("DatePalm_RT.spt", 40 * 300, 0, 14);
                //forest.AddTreeType("AmericanHolly_RT.spt", 40 * 300, 0, 24);
                //forest.AddTreeType("CoconutPalm_RT.spt", 55 * 300, 0, 23);
                //forest.AddTreeType("SpiderTree_RT_Dead.spt", 80 * 300, 0, 9);

                uint numinstances = 50;

                //forest.AddTreeType("Azalea_RT.spt", 55 * 300, 0, numinstances);
                //forest.AddTreeType("Azalea_RT_Pink.spt", 55 * 300, 0, numinstances);
                //forest.AddTreeType("AzaleaPatch_RT.spt", 55 * 300, 0, numinstances);
                //forest.AddTreeType("AzaleaPatch_RT_Pink.spt", 55 * 300, 0, numinstances);

                //forest.AddTreeType("CurlyPalm_RT.spt", 55 * 300, 0, numinstances);
                //forest.AddTreeType("CurlyPalmCluster_RT.spt", 55 * 300, 0, numinstances);

                //forest.AddTreeType("FraserFir_RT.spt", 55 * 300, 0, numinstances);
                //forest.AddTreeType("FraserFir_RT_Snow.spt", 55 * 300, 0, numinstances);
                //forest.AddTreeType("FraserFirCluster_RT.spt", 55 * 300, 0, numinstances);
                //forest.AddTreeType("FraserFirCluster_RT_Snow.spt", 55 * 300, 0, numinstances);

                forest.AddTreeType("RDApple_RT.spt", 55 * 300, 0, numinstances);
                //forest.AddTreeType("RDApple_RT_Apples.spt", 55 * 300, 0, numinstances);
                //forest.AddTreeType("RDApple_RT_Spring.spt", 55 * 300, 0, numinstances);
                //forest.AddTreeType("RDApple_RT_Winter.spt", 55 * 300, 0, numinstances);

                forest.AddTreeType("UmbrellaThorn_RT.spt", 55 * 300, 0, numinstances);
                //forest.AddTreeType("UmbrellaThorn_RT_Dead.spt", 55 * 300, 0, numinstances);
                //forest.AddTreeType("UmbrellaThorn_RT_Flowers.spt", 55 * 300, 0, numinstances);

                //forest.AddTreeType("WeepingWillow_RT.spt", 55 * 300, 0, numinstances);
                //forest.AddTreeType("WeepingWillow_RT_Fall.spt", 55 * 300, 0, numinstances);
                //forest.AddTreeType("WeepingWillow_RT_Winter.spt", 55 * 300, 0, numinstances);

                //forest.AddTreeType("AmericanBoxwood_RT.spt", 55 * 300, 0, numinstances);
                //forest.AddTreeType("AmericanBoxwoodCluster_RT.spt", 55 * 300, 0, numinstances);

                //forest.AddTreeType("Beech_RT.spt", 55 * 300, 0, numinstances);
                //forest.AddTreeType("Beech_RT_Fall.spt", 55 * 300, 0, numinstances);
                //forest.AddTreeType("Beech_RT_Winter.spt", 55 * 300, 0, numinstances);

                forest.AddTreeType("SugarPine_RT.spt", 55 * 300, 0, numinstances);
                //forest.AddTreeType("SugarPine_RT_Winter.spt", 55 * 300, 0, numinstances);

                //forest.AddTreeType("VenusTree_RT.spt", 55 * 300, 0, numinstances);

                //forest.AddTreeType("CherryTree_RT.spt", 55 * 300, 0, numinstances);
                //forest.AddTreeType("CherryTree_RT_Spring.spt", 55 * 300, 0, numinstances);
                //forest.AddTreeType("CherryTree_RT_Fall.spt", 55 * 300, 0, numinstances);
                //forest.AddTreeType("CherryTree_RT_Winter.spt", 55 * 300, 0, numinstances);

                //forest.AddTreeType("SpiderTree_RT.spt", 55 * 300, 0, numinstances);
                //forest.AddTreeType("SpiderTree_RT_Dead.spt", 55 * 300, 0, numinstances);

                //forest.AddTreeType("JungleBrush_RT.spt", 55 * 300, 0, numinstances);

                forest.AddTreeType("QueenPalm_RT.spt", 55 * 300, 0, numinstances);
                //forest.AddTreeType("QueenPalm_RT_Flowers.spt", 55 * 300, 0, numinstances);
                //forest.AddTreeType("QueenPalmCluster_RT.spt", 55 * 300, 0, numinstances);


                if (false)
                {
                    //uint numinstances = 30;
                    forest.AddTreeType("CurlyPalm_RT.spt", 55 * 300, 0, numinstances);
                    forest.AddTreeType("DatePalm_RT.spt", 40 * 300, 0, numinstances);
                    forest.AddTreeType("JungleBrush_RT.spt", 70 * 300, 0, numinstances);
                    forest.AddTreeType("Cercropia_RT.spt", 55 * 300, 0, numinstances);
                    forest.AddTreeType("CommonOlive_RT_Summer.spt", 40 * 300, 0, numinstances);
                    forest.AddTreeType("ColvilleaRacemosa_RT_Flower.spt", 70 * 300, 0, numinstances);
                    forest.AddTreeType("JapaneseAngelica_RT_Summer.spt", 55 * 300, 0, numinstances);
                    forest.AddTreeType("NorthIslandRata_RT_Spring.spt", 40 * 300, 0, numinstances);
                    forest.AddTreeType("SpiderTree_RT.spt", 70 * 300, 0, numinstances);
                    forest.AddTreeType("Stump_RT.spt", 150 * 300, 0, numinstances);
                    forest.AddTreeType("UmbrellaThorn_RT_Flowers.spt", 70 * 300, 0, numinstances);

                    forest.AddTreeType("AmurCork_RT_LateSummer.spt", 55 * 300, 0, numinstances);
                    forest.AddTreeType("ArizonaBush_RT_Flowers.spt", 40 * 300, 0, numinstances);
                    forest.AddTreeType("BananaTree_RT.spt", 70 * 300, 0, numinstances);
                    forest.AddTreeType("Baobab_RT.spt", 55 * 300, 0, numinstances);
                    forest.AddTreeType("CaliforniaBuckeye_RT_Nuts.spt", 120 * 300, 0, numinstances);

                    forest.AddTreeType("CedarOfLebanon_RT.spt", 55 * 300, 0, numinstances);
                    forest.AddTreeType("CherryTree_RT_Spring.spt", 40 * 300, 0, numinstances);
                    forest.AddTreeType("CinnamonFern_RT.spt", 70 * 300, 0, numinstances);
                    forest.AddTreeType("CoconutPalm_RT.spt", 55 * 300, 0, numinstances);
                    forest.AddTreeType("Crepe Myrtle_RT_Flowers.spt", 120 * 300, 0, numinstances);
                }
                //forest.AddTreeType("CedarOfLebanon_RT.spt", 100 * 300, 0, 30);
                //forest.AddTreeType("CherryTree_RT_Spring.spt", 40 * 300, 0, 30);
                //forest.AddTreeType("CinnamonFern_RT.spt", 70 * 300, 0, 30);
                //forest.AddTreeType("CoconutPalm_RT.spt", 55 * 300, 0, 30);
                //forest.AddTreeType("Crepe Myrtle_RT_Flowers.spt", 120 * 300, 0, 30);

                if (false)
                {
                    forest.AddTreeType("Crepe Myrtle_RT_Winter.spt", 100 * 300, 0, 30);
                    forest.AddTreeType("FanPalm_RT.spt", 40 * 300, 0, 30);
                    forest.AddTreeType("ItalianCypress_RT.spt", 70 * 300, 0, 30);
                    forest.AddTreeType("JapaneseMaple_RT_Summer.spt", 55 * 300, 0, 30);
                    forest.AddTreeType("JoshuaTree_RT.spt", 120 * 300, 0, 30);

                    forest.AddTreeType("KoreanStewartia_RT.spt", 100 * 300, 0, 30);
                    forest.AddTreeType("ManchurianAngelicaTree_RT_Small.spt", 100 * 300, 0, 30);
                    forest.AddTreeType("MimosaTree_RT.spt", 100 * 300, 0, 30);
                    forest.AddTreeType("MimosaTree_RT_Flower.spt", 100 * 300, 0, 30);
                    forest.AddTreeType("Mulga_RT_Flowers.spt", 50 * 300, 0, 30);

                    forest.AddTreeType("OmenTree_RT.spt", 80 * 300, 0, 30);
                    forest.AddTreeType("OrientalSpruce_RT.spt", 50 * 300, 0, 30);
                    forest.AddTreeType("PonytailPalm_RT.spt", 140 * 300, 0, 30);
                    forest.AddTreeType("QueenPalm_RT.spt", 55 * 300, 0, 30);
                    forest.AddTreeType("ColvilleaRacemosa_RT.spt", 50 * 300, 0, 30);

                    forest.AddTreeType("SpiderTree_RT_Dead.spt", 80 * 300, 0, 30);
                    forest.AddTreeType("Tamarind_RT_Spring.spt", 50 * 300, 0, 30);
                    forest.AddTreeType("WeepingWillow_RT.spt", 50 * 300, 0, 30);
                }
            }

            if ( drawForest2 ) {
                boundary2 = new Boundary("boundary2");
                boundary2.AddPoint(new Vector3(285 * oneMeter, 0, 3462 * oneMeter));
                boundary2.AddPoint(new Vector3(-679 * oneMeter, 0, 3560 * oneMeter));
                boundary2.AddPoint(new Vector3(-647 * oneMeter, 0, 3381 * oneMeter));
                boundary2.AddPoint(new Vector3(-512 * oneMeter, 0, 3230 * oneMeter));
                boundary2.AddPoint(new Vector3(402 * oneMeter, 0, 3116 * oneMeter));
                boundary2.AddPoint(new Vector3(402 * oneMeter, 0, 3339 * oneMeter));
                boundary2.AddPoint(new Vector3(305 * oneMeter, 0, 3363 * oneMeter));
                boundary2.Close();

                mvScene.AddBoundary(boundary2);

                Forest forest2 = new Forest(1234, "Forest2", treeSceneNode);

                boundary2.AddSemantic(forest2);

                forest2.WindFilename = "demoWind.ini";

                forest2.AddTreeType("EnglishOak_RT.spt", 55 * 300, 0, 150);
                forest2.AddTreeType("AmericanHolly_RT.spt", 40 * 300, 0, 150);
                forest2.AddTreeType("ChristmasScotchPine_RT.spt", 70 * 300, 0, 150);

                forest2.WindDirection = Vector3.UnitX;
                forest2.WindStrength = 0f;
            }

            if (lakes)
            {
                boundary3 = new Boundary("boundary3");
                boundary3.AddPoint(new Vector3(-540 * oneMeter, 0, 3151 * oneMeter));
                boundary3.AddPoint(new Vector3(-656 * oneMeter, 0, 3058 * oneMeter));
                boundary3.AddPoint(new Vector3(-631 * oneMeter, 0, 2878 * oneMeter));
                boundary3.AddPoint(new Vector3(-335 * oneMeter, 0, 2882 * oneMeter));
                boundary3.AddPoint(new Vector3(-336 * oneMeter, 0, 3098 * oneMeter));
                boundary3.AddPoint(new Vector3(-478 * oneMeter, 0, 3166 * oneMeter));
                boundary3.Close();

                //boundary3.Hilight = true;

                mvScene.AddBoundary(boundary3);

                WaterPlane waterSemantic = new WaterPlane(42 * WorldManager.oneMeter, "lake1", treeSceneNode);

                boundary3.AddSemantic(waterSemantic);

            }

            if (buildings)
            {
                Entity entity = scene.CreateEntity("tree", "demotree4.mesh");

                SceneNode node = scene.RootSceneNode.CreateChildSceneNode();
                node.AttachObject(entity);
                node.Position = new Vector3(332383, 71536, 4247994);

                entity = scene.CreateEntity("house", "human_house_stilt.mesh");

                node = scene.RootSceneNode.CreateChildSceneNode();
                node.AttachObject(entity);
                node.Position = new Vector3(0, 130.0f * oneMeter, 3900 * oneMeter);

            }

            if (ocean)
            {
                Entity waterEntity = scene.CreateEntity("Water", "WaterPlane");

                Debug.Assert(waterEntity != null);
                waterEntity.MaterialName = "MVSMOcean";

                SceneNode waterNode = scene.RootSceneNode.CreateChildSceneNode("WaterNode");
                Debug.Assert(waterNode != null);
                waterNode.AttachObject(waterEntity);
                waterNode.Translate(new Vector3(0, 0, 0));
            }

            if (roads)
            {
                road1 = mvScene.CreateRoad("Via Appia");
                road1.HalfWidth = 2;

                List<Vector3> roadPoints = new List<Vector3>();
                roadPoints.Add(new Vector3(97000, 0, 4156000));
                roadPoints.Add(new Vector3(205000, 0, 4031000));
                roadPoints.Add(new Vector3(254000, 0, 3954000));
                roadPoints.Add(new Vector3(234000, 0, 3500000));
                roadPoints.Add(new Vector3(256000, 0, 3337000));
                roadPoints.Add(new Vector3(98000, 0, 3242000));

                road1.AddPoints(roadPoints);


            }
        }

        private void NewRoad()
        {
            road2 = mvScene.CreateRoad("Via Sacra");

            List<Vector3> road2Points = new List<Vector3>();
            road2Points.Add(new Vector3(71000, 0, 4014000));
            road2Points.Add(new Vector3(265000, 0, 4177000));

            road2.AddPoints(road2Points);
        }

        protected override void CreateScene()
        {
            viewport.BackgroundColor = ColorEx.White;
			viewport.OverlaysEnabled = false;

			gen = new Multiverse.Generator.Generator();
            gen.Algorithm = GeneratorAlgorithm.HybridMultifractalWithSeedMap;
            gen.LoadSeedMap("map.csv");
            gen.OutsideMapSeedHeight = 0;
            gen.SeedMapOrigin = new Vector3(-3200, 0, -5120);
            gen.SeedMapMetersPerSample = 128;
			gen.XOff = -0.4f;
			gen.YOff = -0.3f;
            gen.HeightFloor = 0;
            gen.FractalOffset = 0.1f;
            gen.HeightOffset = -0.15f;
            gen.HeightScale = 300;
            gen.MetersPerPerlinUnit = 800;

            lodSpec = new LODSpec();
            lodSpecPrev = new LODSpecPrev();

            // water plane setup
            Plane waterPlane = new Plane(Vector3.UnitY, 10f * oneMeter);

            waterMesh = MeshManager.Instance.CreatePlane(
                "WaterPlane",
                waterPlane,
                60 * 128 * oneMeter, 90 * 128 * oneMeter,
                20, 20,
                true, 1,
                10, 10,
                Vector3.UnitZ);

            Debug.Assert(waterMesh != null);

            SetupScene();
        }

        protected override void OnFrameStarted(object source, FrameEventArgs e) {

            frameCount++;

			int tick = Environment.TickCount;

			time += e.TimeSinceLastFrame;

            float moveTime = e.TimeSinceLastFrame;
            if (moveTime > 1)
            {
                moveTime = 1;
            }

			Axiom.SceneManagers.Multiverse.WorldManager.Instance.Time = time;

			if ( ( tick - lastTick ) > 100 ) 
			{
				Console.WriteLine("long frame: {0}", tick-lastTick);
                LogManager.Instance.Write("long frame: {0}", tick - lastTick);
			}

			lastTick = tick;

//			int hpg = gen.HeightPointsGenerated;
//			if ( lastHeightPointsGenerated != hpg ) 
//			{
//				Console.WriteLine("HeightPointsGenerated: {0}", hpg - lastHeightPointsGenerated);
//				lastHeightPointsGenerated = hpg;
//			}

			float scaleMove = 20 * oneMeter * moveTime;

			// reset acceleration zero
			camAccel = Vector3.Zero;

			// set the scaling of camera motion
			cameraScale = 100 * moveTime;

            bool retry = true;

            //System.Threading.Thread.Sleep(3000);

            //while (retry)
            //{
            //    // TODO: Move this into an event queueing mechanism that is processed every frame
            //    try
            //    {
            //        input.Capture();
            //        retry = false;
            //    }
            //    catch (Exception ex)
            //    {
            //        System.Threading.Thread.Sleep(1000);
            //    }
            //}

            // TODO: Move this into an event queueing mechanism that is processed every frame

            bool realIsActive = window.IsActive;
            try
            {

                // force a reset of axiom's internal input state so that it will call Acquire() on the
                // input device again.
                if (captureFailed)
                {
                    window.IsActive = false;
                    input.Capture();
                    window.IsActive = realIsActive;
                }
                input.Capture();
                captureFailed = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                captureFailed = true;
                System.Threading.Thread.Sleep(1000);
            }
            finally
            {
                window.IsActive = realIsActive;
            }

			if(input.IsKeyPressed(KeyCodes.Escape)) 
			{
				Root.Instance.QueueEndRendering();

				return;
			}

			if(input.IsKeyPressed(KeyCodes.A)) 
			{
				camAccel.x = -0.5f;
			}

			if(input.IsKeyPressed(KeyCodes.D)) 
			{
				camAccel.x = 0.5f;
			}

			if(input.IsKeyPressed(KeyCodes.W)) 
			{
				camAccel.z = -1.0f;
			}

			if(input.IsKeyPressed(KeyCodes.S)) 
			{
				camAccel.z = 1.0f;
			}

            if (!captureFailed)
            {
                camAccel.y += (float)(input.RelativeMouseZ * 0.1f);
            }

			if(input.IsKeyPressed(KeyCodes.Left)) 
			{
				camera.Yaw(cameraScale);
			}

			if(input.IsKeyPressed(KeyCodes.Right)) 
			{
				camera.Yaw(-cameraScale);
			}

			if(input.IsKeyPressed(KeyCodes.Up)) 
			{
				camera.Pitch(cameraScale);
			}

			if(input.IsKeyPressed(KeyCodes.Down)) 
			{
				camera.Pitch(-cameraScale);
			}

			// subtract the time since last frame to delay specific key presses
			toggleDelay -= e.TimeSinceLastFrame;

			// toggle rendering mode
			if(input.IsKeyPressed(KeyCodes.R) && toggleDelay < 0) 
			{
				if(camera.SceneDetail == SceneDetailLevel.Points) 
				{
					camera.SceneDetail = SceneDetailLevel.Solid;
				}
				else if(camera.SceneDetail == SceneDetailLevel.Solid) 
				{
					camera.SceneDetail = SceneDetailLevel.Wireframe;
				}
				else 
				{
					camera.SceneDetail = SceneDetailLevel.Points;
				}

				Console.WriteLine("Rendering mode changed to '{0}'.", camera.SceneDetail);

				toggleDelay = 1;
			}

			if ( input.IsKeyPressed(KeyCodes.F) && toggleDelay < 0 ) 
			{
				followTerrain = !followTerrain;
				toggleDelay = 1;
			}

			if ( input.IsKeyPressed(KeyCodes.H) && toggleDelay < 0 ) 
			{
				humanSpeed = !humanSpeed;
				toggleDelay = 1;
			}

			if(input.IsKeyPressed(KeyCodes.T) && toggleDelay < 0) 
			{
				// toggle the texture settings
				switch(filtering) 
				{
					case TextureFiltering.Bilinear:
						filtering = TextureFiltering.Trilinear;
						aniso = 1;
						break;
					case TextureFiltering.Trilinear:
						filtering = TextureFiltering.Anisotropic;
						aniso = 8;
						break;
					case TextureFiltering.Anisotropic:
						filtering = TextureFiltering.Bilinear;
						aniso = 1;
						break;
				}

				Console.WriteLine("Texture Filtering changed to '{0}'.", filtering);

				// set the new default
				MaterialManager.Instance.SetDefaultTextureFiltering(filtering);
				MaterialManager.Instance.DefaultAnisotropy = aniso;
                
				toggleDelay = 1;
			}

			if(input.IsKeyPressed(KeyCodes.P)) 
			{
				string[] temp = Directory.GetFiles(Environment.CurrentDirectory, "screenshot*.jpg");
				string fileName = string.Format("screenshot{0}.jpg", temp.Length + 1);
                
				// show briefly on the screen
				window.DebugText = string.Format("Wrote screenshot '{0}'.", fileName);

				TakeScreenshot(fileName);

				// show for 2 seconds
				debugTextDelay = 2.0f;
			}

			if(input.IsKeyPressed(KeyCodes.B)) 
			{
				scene.ShowBoundingBoxes = !scene.ShowBoundingBoxes;
			}

			if(input.IsKeyPressed(KeyCodes.O)) 
			{
				// hide all overlays, includes ones besides the debug overlay
				viewport.OverlaysEnabled = !viewport.OverlaysEnabled;
			}

			if ( input.IsKeyPressed(KeyCodes.F1) && toggleDelay < 0 ) 
			{
				Axiom.SceneManagers.Multiverse.WorldManager.Instance.DrawStitches =
					! Axiom.SceneManagers.Multiverse.WorldManager.Instance.DrawStitches;
				toggleDelay = 1;
			}

			if ( input.IsKeyPressed(KeyCodes.F2) && toggleDelay < 0 ) 
			{
				Axiom.SceneManagers.Multiverse.WorldManager.Instance.DrawTiles =
					! Axiom.SceneManagers.Multiverse.WorldManager.Instance.DrawTiles;
				toggleDelay = 1;
			}

            if (input.IsKeyPressed(KeyCodes.F3) && toggleDelay < 0)
            {
                Console.WriteLine("Camera Location: {0}, {1}, {2}", camera.Position.x.ToString("F3"),
                    camera.Position.y.ToString("F3"), camera.Position.z.ToString("F3"));
                toggleDelay = 1;
            }

            if (input.IsKeyPressed(KeyCodes.F4) && toggleDelay < 0)
            {
                preview = !preview;

                if (preview)
                {
                    // when in preview mode, move the camera up high and look at the ground
                    camera.MoveRelative(new Vector3(0, 1000 * oneMeter, 0));
                    camera.LookAt(new Vector3(0, 0, 0));

                    followTerrain = false;
                }
                else
                {
                    // when not in preview mode, hug the ground
                    followTerrain = true;
                }
                SetupScene();

                toggleDelay = 1;
            }

            if (input.IsKeyPressed(KeyCodes.F5) && toggleDelay < 0)
            {
                if (WorldManager.Instance.OceanWaveHeight >= 500)
                {
                    WorldManager.Instance.OceanWaveHeight -= 500;
                }
                toggleDelay = 1;
            }

            if (input.IsKeyPressed(KeyCodes.F6) && toggleDelay < 0)
            {
                WorldManager.Instance.OceanWaveHeight += 500;
                toggleDelay = 1;
            }

            if (input.IsKeyPressed(KeyCodes.F7) && toggleDelay < 0)
            {

                NewRoad();
                toggleDelay = 1;
            }


            if (input.IsKeyPressed(KeyCodes.F8) && toggleDelay < 0)
            {
                WorldManager.Instance.SeaLevel += 500;
                toggleDelay = 1;
            }

            if (input.IsKeyPressed(KeyCodes.F9) && toggleDelay < 0)
            {
                StreamWriter s = new StreamWriter("boundaries.xml");
                XmlTextWriter w = new XmlTextWriter(s);
                //w.Formatting = Formatting.Indented;
                //w.Indentation = 2;
                //w.IndentChar = ' ';
                mvScene.ExportBoundaries(w);
                w.Close();
                s.Close();
                toggleDelay = 1;
            }

            if (input.IsKeyPressed(KeyCodes.F10) && toggleDelay < 0)
            {
                StreamReader s = new StreamReader("boundaries.xml");
                XmlTextReader r = new XmlTextReader(s);
                mvScene.ImportBoundaries(r);
                r.Close();
                s.Close();
                toggleDelay = 1;
            }

            if (input.IsKeyPressed(KeyCodes.F11) && toggleDelay < 0)
            {
                StreamWriter s = new StreamWriter("terrain.xml");
                gen.ToXML(s);
                s.Close();
                toggleDelay = 1;
            }

            if (input.IsKeyPressed(KeyCodes.F12) && toggleDelay < 0)
            {
                StreamReader s = new StreamReader("terrain.xml");
                gen.FromXML(s);
                s.Close();

                SetupScene();

                toggleDelay = 1;
            }

            if (input.IsKeyPressed(KeyCodes.I))
            {
                //point camera north
                camera.Direction = Vector3.NegativeUnitZ;
            }
            if (input.IsKeyPressed(KeyCodes.K))
            {
                //point camera south
                camera.Direction = Vector3.UnitZ;
            }
            if (input.IsKeyPressed(KeyCodes.J))
            {
                //point camera west
                camera.Direction = Vector3.NegativeUnitX;
            }
            if (input.IsKeyPressed(KeyCodes.L))
            {
                //point camera east
                camera.Direction = Vector3.UnitX;
            }

            if (!captureFailed)
            {
                if (!input.IsMousePressed(MouseButtons.Left))
                {
                    float cameraYaw = -input.RelativeMouseX * .13f;
                    float cameraPitch = -input.RelativeMouseY * .13f;

                    camera.Yaw(cameraYaw);
                    camera.Pitch(cameraPitch);
                }
                else
                {
                    cameraVector.x += input.RelativeMouseX * 0.13f;
                }
            }

			if ( humanSpeed ) 
			{ // in game running speed is 7m/sec
				//camVelocity = camAccel * 7 * oneMeter;
                camVelocity = camAccel * 32 * oneMeter;
			} 
			else 
			{
				camVelocity += (camAccel * scaleMove * camSpeed);
			}

   //         Console.WriteLine("ScameMove: {0}", scaleMove.ToString("F3"));
   //         Console.WriteLine("Camera Accel: {0}, {1}, {2}", camAccel.x.ToString("F3"),
   //camAccel.y.ToString("F3"), camAccel.z.ToString("F3"));
   //         Console.WriteLine("Camera Velocity: {0}, {1}, {2}", camVelocity.x.ToString("F3"),
   //camVelocity.y.ToString("F3"), camVelocity.z.ToString("F3"));

			// move the camera based on the accumulated movement vector
			camera.MoveRelative(camVelocity * moveTime);

			// Now dampen the Velocity - only if user is not accelerating
			if (camAccel == Vector3.Zero) 
			{
                float slowDown = 6 * moveTime;
                if (slowDown > 1)
                {
                    slowDown = 1;
                }
				camVelocity *= (1 - slowDown); 
			}

			if ( followTerrain ) 
			{	
				// adjust new camera position to be a fixed distance above the ground
				Axiom.Core.RaySceneQuery raySceneQuery = scene.CreateRayQuery( new Ray(camera.Position, Vector3.NegativeUnitY));

				raySceneQuery.QueryMask = (ulong)Axiom.SceneManagers.Multiverse.RaySceneQueryType.Height;
				ArrayList results = raySceneQuery.Execute();

				RaySceneQueryResultEntry result = (RaySceneQueryResultEntry)results[0];

				camera.Position = new Vector3(camera.Position.x, result.worldFragment.SingleIntersection.y + ( 2f * oneMeter ), camera.Position.z);
			}

			// update performance stats once per second
			if(statDelay < 0.0f && showDebugOverlay) 
			{
				UpdateStats();
				statDelay = 1.0f;
			}
			else 
			{
				statDelay -= e.TimeSinceLastFrame;
			}

			// turn off debug text when delay ends
			if(debugTextDelay < 0.0f) 
			{
				debugTextDelay = 0.0f;
				window.DebugText = "";
			}
			else if(debugTextDelay > 0.0f) 
			{
				debugTextDelay -= e.TimeSinceLastFrame;
			}

        }

		static void Main() 
		{
			MVSMTest app = new MVSMTest();

			app.Start();
		}
	}
}
