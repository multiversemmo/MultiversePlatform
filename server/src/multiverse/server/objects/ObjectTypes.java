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

package multiverse.server.objects;

/** Multiverse object type definitions.
*/
public class ObjectTypes
{
    /** The default/fallback object type. */
    public static final ObjectType unknown =
		ObjectType.intern((short)-1,"Unknown");

    /** Object type for structures and static objects.  Has base type
        {@link ObjectType#BASE_STRUCTURE}. */
    public static final ObjectType structure =
            ObjectType.intern((short)0,"STRUCTURE",ObjectType.BASE_STRUCTURE);

    /** Object type for mobs (mobile objects).  Has base type
        {@link ObjectType#BASE_MOB}. */
    public static final ObjectType mob =
            ObjectType.intern((short)1,"MOB",ObjectType.BASE_MOB);
    /* 2 not used */

    /** Object type for players and users.  Has base types
        {@link ObjectType#BASE_MOB} and {@link ObjectType#BASE_PLAYER}. */
    public static final ObjectType player =
            ObjectType.intern((short)3,"PLAYER",
                ObjectType.BASE_PLAYER | ObjectType.BASE_MOB);

    /** Object type for lights. */
    public static final ObjectType light =
		ObjectType.intern((short)4,"LIGHT");

    /** Object type for terrain decals. Has base type
        {@link ObjectType#BASE_STRUCTURE}. */
    public static final ObjectType terrainDecal =
            ObjectType.intern((short)5,"TDECAL",ObjectType.BASE_STRUCTURE);

    /** Object type for point sounds. Has base type
        {@link ObjectType#BASE_STRUCTURE}. */
    public static final ObjectType pointSound =
            ObjectType.intern((short)6,"PTSOUND",ObjectType.BASE_STRUCTURE);

    /** Object type for items. */
    public static final ObjectType item =
            ObjectType.intern((short)7,"ITEM");

    /** Object type for roads. */
    public static final ObjectType road =
		ObjectType.intern((short)8,"mvRoad");

    /** Object type for bags. */
    public static final ObjectType bag =
		ObjectType.intern((short)9,"Bag");

    /** Object type for combat info. */
    public static final ObjectType combatInfo =
		ObjectType.intern((short)10,"CombatInfo");

    public static final ObjectType instance =
                ObjectType.intern((short)11,"Instance");

}

