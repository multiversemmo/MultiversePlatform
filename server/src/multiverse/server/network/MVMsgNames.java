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

package multiverse.server.network;
import java.util.*;

public class MVMsgNames {
    
    public static String msgName(int msgType) {
        String s = msgNames.get(msgType);
        if (s == null)
            return "Unknown Message";
        else
            return s;
    }

    public static Map<Integer, String> initializeMsgNames() {
        Map<Integer, String> names = new HashMap<Integer, String>();
        names.put(1, "Login");
        names.put(2, "Direction");
        names.put(3, "Comm");
        names.put(4, "LoginResponse");
        names.put(5, "Logout");
        names.put(6, "OldTerrainConfig");
        names.put(7, "SkyboxMaterial");
        names.put(8, "NewObject");
        names.put(9, "Orientation");
        names.put(10, "FreeObject");
        names.put(11, "Acquire");
        names.put(12, "AcquireResponse");
        names.put(13, "Command");
        names.put(14, "Equip");
        names.put(15, "EquipResponse");
        names.put(16, "Unequip");
        names.put(17, "UnequipResponse");
        names.put(18, "Attach");
        names.put(19, "Detach");
        names.put(20, "Combat");
        names.put(21, "AutoAttack");
        names.put(22, "StatUpdate");
        names.put(23, "Damage");
        names.put(24, "Drop");
        names.put(25, "DropResponse");
        names.put(26, "Animation");
        names.put(27, "Sound");
        names.put(28, "AmbientSound");
        names.put(29, "FollowTerrain");
        names.put(30, "Portal");
        names.put(31, "AmbientLight");
        names.put(32, "NewLight");
        names.put(33, "TradeRequest");
        names.put(34, "TradeEstablished");
        names.put(35, "TradeAccepted");
        names.put(36, "TradeEnded");
        names.put(37, "TradeItem");
        names.put(38, "StateMessage");
        names.put(39, "QuestInfoRequest");
        names.put(40, "QuestInfoResponse");
        names.put(41, "QuestResponse");
        names.put(42, "RegionConfig");
        names.put(43, "InventoryUpdate");
        names.put(44, "QuestLogInfo");
        names.put(45, "QuestStateInfo");
        names.put(46, "RemoveQuestRequest");
        names.put(47, "RemoveQuestResponse");
        names.put(48, "GroupInfo");
        names.put(49, "QuestConcludeRequest");
        names.put(50, "UiTheme");
        names.put(51, "LootAll");
        names.put(52, "OldModelInfo");
        names.put(53, "FragmentMessage");
        names.put(54, "RoadInfo");
        names.put(55, "Fog");
        names.put(56, "AbilityUpdate");
        names.put(57, "AbilityInfo");
        names.put(61, "OldObjectProperty");
        names.put(62, "ObjectProperty");
        names.put(63, "AddParticleEffect");
        names.put(64, "RemoveParticleEffect");
        names.put(65, "ClientParameter");
        names.put(66, "TerrainConfig");
        names.put(67, "TrackObjectInterpolation");
        names.put(68, "TrackLocationInterpolation");
        names.put(69, "FreeRoad");
        names.put(70, "Extension");
        names.put(71, "InvokeEffect");
        names.put(72, "ActivateItem");
        names.put(73, "MobPath");
        names.put(74, "AggregatedRDP");
        names.put(75, "NewDecal");
        names.put(76, "FreeDecal");
        names.put(77, "ModelInfo");
        names.put(78, "SoundControl");
        names.put(79, "DirLocOrient");
        names.put(80, "AuthorizedLogin");
        names.put(81, "AuthorizedLoginResponse");
        return names;
    }

    protected static Map<Integer, String> msgNames = initializeMsgNames();

}
    
