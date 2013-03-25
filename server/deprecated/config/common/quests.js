importPackage(Packages.multiverse.mars);
importPackage(Packages.multiverse.mars.objects);
importPackage(Packages.multiverse.mars.util);
importPackage(Packages.multiverse.server.math);
importPackage(Packages.multiverse.server.npcmodule);
importPackage(Packages.multiverse.server.events);
importPackage(Packages.multiverse.server.objects);
importPackage(Packages.multiverse.server.engine);

//
// register quest generators
//
// jsSwordGenerator = {
//   generateReward: function() {
//     Log.debug("create_quests.js: generateReward method");
//     var orcSword = Mars.TemplateManager.get("Bronze Longsword").generate();
//     return orcSword;
//   },
// };
// swordGenerator = new JavaAdapter(QuestRewardGenerator, jsSwordGenerator);
// quest1 = MarsQuest.getQuest("Prove Your Worth");
// quest2 = MarsQuest.getQuest("Orc Captain");
// ScriptRewardGenerator.registerRewardScript(quest1.getOid(), swordGenerator);
// ScriptRewardGenerator.registerRewardScript(quest2.getOid(), swordGenerator);
// Log.debug("quests.js: registered quest reward generators");
