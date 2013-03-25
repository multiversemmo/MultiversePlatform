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

package multiverse.server.plugins;

import multiverse.msgsys.*;
import multiverse.server.engine.Engine;
import multiverse.server.objects.SpawnData;


/** Mob server API.
*/
public class MobManagerClient
{
    private MobManagerClient()
    {
    }

    /** Create a spawn generator.
        @param spawnData Spawn generator definition.
        @return True on success, false on failure
    */
    public static boolean createSpawnGenerator(SpawnData spawnData)
    {
        CreateSpawnGeneratorMessage message =
                new CreateSpawnGeneratorMessage(spawnData);

        return Engine.getAgent().sendRPCReturnBoolean(message);
    }

    public static class CreateSpawnGeneratorMessage extends Message
    {
        public CreateSpawnGeneratorMessage()
        {
        }

        public CreateSpawnGeneratorMessage(SpawnData spawnData)
        {
            super(MSG_TYPE_CREATE_SPAWN_GEN);
            setSpawnData(spawnData);
        }

        public SpawnData getSpawnData()
        {
            return spawnData;
        }
        public void setSpawnData(SpawnData spawnData)
        {
            this.spawnData = spawnData;
        }

        private SpawnData spawnData;

        private static final long serialVersionUID = 1L;
    }

    public static MessageType MSG_TYPE_CREATE_SPAWN_GEN =
        MessageType.intern("mv.CREATE_SPAWN_GEN");

}
