importPackage(Packages.multiverse.mars);
importPackage(Packages.multiverse.mars.core);
importPackage(Packages.multiverse.mars.objects);
importPackage(Packages.multiverse.mars.util);
importPackage(Packages.multiverse.server.math);
importPackage(Packages.multiverse.server.npcmodule);
importPackage(Packages.multiverse.server.events);
importPackage(Packages.multiverse.server.objects);
importPackage(Packages.multiverse.server.engine);
importPackage(Packages.multiverse.server.util);

leatherGlovesTmpl = Mars.ItemTemplateManager.get("Leather Gloves");
leatherBootsTmpl = Mars.ItemTemplateManager.get("Leather Boots");
leatherHelmetTmpl = Mars.ItemTemplateManager.get("Leather Helmet");
leatherTunicTmpl = Mars.ItemTemplateManager.get("Leather Tunic");
leatherPantsTmpl = Mars.ItemTemplateManager.get("Leather Pants");
brzPlateGlovesTmpl = Mars.ItemTemplateManager.get("Bronze Plate Gloves");
brzPlateBootsTmpl = Mars.ItemTemplateManager.get("Bronze Plate Boots");
brzPlateHelmetTmpl = Mars.ItemTemplateManager.get("Bronze Plate Helmet");
brzPlateChestTmpl = Mars.ItemTemplateManager.get("Bronze Plate Chest");
brzPlatePantsTmpl = Mars.ItemTemplateManager.get("Bronze Plate Pants");
brokenDaggerTmpl = Mars.ItemTemplateManager.get("Broken Dagger");
suppliesTmpl = Mars.ItemTemplateManager.get("Supplies");
coyoteSkinTmpl = Mars.ItemTemplateManager.get("Coyote Skin");
wolfSkinTmpl = Mars.ItemTemplateManager.get("Wolf Skin");
crocSkinTmpl = Mars.ItemTemplateManager.get("Croc Skin");
brzAxeTmpl = Mars.ItemTemplateManager.get("Bronze Axe");
brzSwdTmpl = Mars.ItemTemplateManager.get("Bronze Longsword");
brzDaggerTmpl = Mars.ItemTemplateManager.get("Bronze Dagger");
brzSpearTmpl = Mars.ItemTemplateManager.get("Bronze Spear");
ironSwdTmpl = Mars.ItemTemplateManager.get("Iron Longsword");
axeSkillTmpl = Mars.ItemTemplateManager.get("Axe Scroll");
daggerSkillTmpl = Mars.ItemTemplateManager.get("Dagger Scroll");
swordSkillTmpl = Mars.ItemTemplateManager.get("Sword Scroll");
brawlingSkillTmpl = Mars.ItemTemplateManager.get("Brawling Scroll");
choppingAxeTmpl = Mars.ItemTemplateManager.get("Chopping Axe");

welcomeAshore = new MarsCollectionQuest();
welcomeAshore.setName("Welcome Ashore");
welcomeAshore.setDesc("Welcome ashore!  Take this broken dagger to the blacksmith, up the road behind me. Off you go. Next!");
welcomeAshore.setObjective("Talk to Blacksmith");
welcomeAshore.setCashReward(1000);
welcomeAshore.addDeliveryItem(brokenDaggerTmpl);
welcomeAshore.addReward(brzDaggerTmpl);  // should be prospecting scroll
welcomeAshoreGoal = new MarsCollectionQuest.CollectionGoal(brokenDaggerTmpl, 1);
welcomeAshore.addCollectionGoal(welcomeAshoreGoal);
welcomeAshore.markPersistent();
welcomeAshore.spawn();

everybodyWorks = new MarsCollectionQuest();
everybodyWorks.setName("Everybody Works");
everybodyWorks.setDesc("Here, these supplies are for the captain of the guard further up the road.")
everybodyWorks.setObjective("Deliver supplies to the captain of the guard.");
everybodyWorks.setCashReward(1000);
everybodyWorks.addDeliveryItem(suppliesTmpl);
everybodyWorks.addReward(daggerSkillTmpl);
everybodyWorksGoal = new MarsCollectionQuest.CollectionGoal(suppliesTmpl, 1);
everybodyWorks.addCollectionGoal(everybodyWorksGoal);
everybodyWorks.markPersistent();
everybodyWorks.spawn();

proveYourMettle = new MarsKillQuest();
proveYourMettle.setName("Prove Your Mettle");
proveYourMettle.setDesc("First, prove your courage. Some coyotes on the hill behind town have been spooking the merchants. Go kill three of them--coyotes, not merchants!");
proveYourMettle.setObjective("Kill 3 coyotes.");
proveYourMettle.setCashReward(1000);
proveYourMettle.addReward(brzSwdTmpl);
proveYourMettle.addReward(swordSkillTmpl);
proveYourMettle.setKillGoal("Coyote", 3);
proveYourMettle.markPersistent();
proveYourMettle.spawn();

killerCrocs = new MarsKillQuest();
killerCrocs.setName("Killer Crocs");
killerCrocs.setDesc("Killing coyotes is one thing, but let's see how you do against creatures that fight back. Along the coast is the beach where we originally made landfall.  Go kill me 3 of those beasts.");
killerCrocs.setObjective("Kill 3 crocodiles.");
killerCrocs.setCashReward(1000);
killerCrocs.addReward(ironSwdTmpl);  // should be special sword scroll
killerCrocs.setKillGoal("Crocodile", 3);
killerCrocs.markPersistent();
killerCrocs.spawn();

bootstrapping = new MarsCollectionQuest();
bootstrapping.setName("Bootstrapping");
bootstrapping.setDesc("Hiya, junior. Lemme guess--looking for armor? Go fetch me 3 coyote pelts, and I'll trade you some nice leather boots.");
bootstrapping.setObjective("Collect 3 coyote pelts.");
bootstrapping.setCashReward(1000);
bootstrapping.addReward(leatherBootsTmpl);
bootstrappingGoal = new MarsCollectionQuest.CollectionGoal(coyoteSkinTmpl, 3);
bootstrapping.addCollectionGoal(bootstrappingGoal);
bootstrapping.markPersistent();
bootstrapping.spawn();

wolfHunter = new MarsCollectionQuest();
wolfHunter.setName("Wolf Hunter");
wolfHunter.setDesc("What I really need are the pelts from wolves. Bring me 3 of them, and I'll trade you a fine leather-armor tunic. You'll find the wolves farther down the road past the coyotes. ");
wolfHunter.setObjective("Collect 3 wolf pelts.");
wolfHunter.setCashReward(1000);
wolfHunter.addReward(leatherTunicTmpl);
wolfHunterGoal = new MarsCollectionQuest.CollectionGoal(wolfSkinTmpl, 3);
wolfHunter.addCollectionGoal(wolfHunterGoal);
wolfHunter.markPersistent();
wolfHunter.spawn();

crocodileHunter = new MarsCollectionQuest();
crocodileHunter.setName("Crocodile Hunter");
crocodileHunter.setDesc("Tell you what I'm going to do: you bring me the skins of 3 sea-crocodiles, and I'll give you a leather helmet and gloves.");
crocodileHunter.setObjective("Collect 3 crocodile skins.");
crocodileHunter.setCashReward(1000);
crocodileHunter.addReward(leatherGlovesTmpl);
crocodileHunter.addReward(leatherHelmetTmpl);
crocodileHunterGoal = new MarsCollectionQuest.CollectionGoal(crocSkinTmpl, 3);
crocodileHunter.addCollectionGoal(crocodileHunterGoal);
crocodileHunter.markPersistent();
crocodileHunter.spawn();

troubleParadise = new MarsKillQuest();
troubleParadise.setName("Trouble in Paradise");
troubleParadise.setDesc("One of the town runners was killed by a monster at the mining shack up the road toward the mountain.  Get up there and kill one of these monsters.")
troubleParadise.setObjective("Kill the monster at the mining shack.");
troubleParadise.setCashReward(1000);
troubleParadise.addReward(brzAxeTmpl);  // should be some other reward
troubleParadise.setKillGoal("Orc Warrior", 1);
troubleParadise.markPersistent();
troubleParadise.spawn();

rightStuff = new MarsKillQuest();
rightStuff.setName("The Right Stuff");
rightStuff.setDesc("Show me you've got the right stuff: kill an orc captain, and you're in the sword school.");
rightStuff.setObjective("Kill an orc captain.");
rightStuff.setCashReward(1000);
rightStuff.addReward(axeSkillTmpl);  // should be admission to school
rightStuff.setKillGoal("Orc Captain", 1);
rightStuff.markPersistent();
rightStuff.spawn();
