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

import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;
import multiverse.server.util.*;
import java.beans.*;

/**
 * for encoding the MVObject into xml
 */
public class MVObjectPersistenceDelegate extends DefaultPersistenceDelegate {
    protected void initialize(Class type, 
                              Object oldInstance,
                              Object newInstance,
                              Encoder out) {
        System.out.println("MVObjectPersistenceDelegate: start persisting obj: " + oldInstance);
        super.initialize(type, oldInstance, newInstance, out);

        
//        MVObject obj = (MVObject) oldInstance;
        System.out.println("MVObjectPersistenceDelegate: super.initialize returned, obj: " + oldInstance);
//        out.writeStatement(new Statement(oldInstance, "setOid",
//                                         new Object[]{ obj.getOid() }));
//
//        out.writeStatement(new Statement(oldInstance, "setName",
//                                         new Object[]{ obj.getName() }));
    }
    
    protected Expression instantiate(Object oldInstance, Encoder out)
    {
        ObjectType objectType = (ObjectType) oldInstance;
        System.out.println("instantiate: "+objectType);
        System.out.println("instantiate: "+objectType.getTypeId());
        return (new Expression(ObjectType.class, "getObjectType",
                new Object[]{objectType.getTypeId()}));
    }

    protected boolean mutatesTo(Object oldInstance, Object newInstance) {
        return oldInstance == newInstance;
    }

    public static class One {
        public One() {
        }
        public ObjectType getType() { return type; }
        public void setType(ObjectType t) { type = t; }
        ObjectType type = ObjectTypes.unknown;
    }

    public static class Two extends One {
        public Two() {
        }
        public ObjectType getO1() { return o1; }
        public ObjectType getO2() { return o2; }
        public void setO1(ObjectType o) { o1 = o; }
        public void setO2(ObjectType o) { o2 = o; }

        ObjectType o1;
        ObjectType o2;
    }

    public static void main(String args[]) {
//        try {
            //             FileOutputStream os = new FileOutputStream("out.xml");

            //Log.init();
            //Namespace.WORLD_MANAGER = Namespace.intern("NS.wmgr");

            /* ObjectType objectType;
            objectType = */ ObjectType.intern((short)33,"thirty");

            Two two = new Two();
            two.setO1(ObjectType.intern((short)2,"two"));
            two.setO2(ObjectType.intern((short)2,"two"));
            two.setType(ObjectTypes.player);

            Object object = two;


/*
            MVObject item = new MVObject();
            item.setName("test item");
            DisplayContext dc = new DisplayContext(item.getOid(), "test_item.mesh");
            dc.setAttachInfo(DisplayState.IN_COMBAT,
                             MarsEquipSlot.PRIMARYWEAPON,
                             MarsAttachSocket.PRIMARYWEAPON);
            item.displayContext(dc);
*/

            ByteArrayOutputStream xml = new ByteArrayOutputStream(1000);
            XMLEncoder encoder = new XMLEncoder(xml);
            encoder.setExceptionListener(new ExceptionListener() {
                    public void exceptionThrown(Exception exception) {
                        Log.exception("MVObjectPersistenceDelegate.main caught exception setting encoder exception listener", exception);
                    }
                });
/*
            multiverse.mars.objects.MarsObject obj = new 
                multiverse.mars.objects.MarsObject();
            obj.setName("test object name");

            
            obj.displayContext(new DisplayContext(obj.getOid(), "orc_fantasy_rig.mesh"));
            obj.isUser(true);
            obj.setPersistenceFlag(true);
//             obj.setState("sleeping", 1);
//             obj.setState("diseased", 1);
//             obj.setState("dead", 0);
            obj.setProperty("STR", 18);
            obj.setProperty("INT", 20);
            obj.setProperty("DEX", 13);
            obj.setProperty("CON", 15);
            obj.setProperty("CHA", 19);
            obj.multiverseID(10);
            obj.scale(3.2F);
//             obj.setSound("sound_idle", true); 

            // world node - it needs to know its loc -- need delegate
            InterpolatedWorldNode node = new InterpolatedWorldNode();
            node.setDir(new MVVector(8,8,8));
            node.setOrientation(new Quaternion(5,5,5,5));
            node.setLoc(new Point(100,200,300));
            obj.worldNode(node);

            // permission callback
//             obj.setPermissionCallback();
            
            // container stuff
//             obj.containerAdd(item);

            // setcontained in - dont need it since containeradd handles that
            
            // behavior - dont set this

            // marsobject stuff
            obj.setStun(100);
            obj.setCurrentStun(90);
//             obj.setState(MarsStates.Movement.toString(),
// 			 multiverse.mars.util.MarsMovementState.State.IDLE);
            obj.setSound("foo", "bar");

            obj.setOwnerOID(10);
*/

            encoder.setPersistenceDelegate(ObjectType.class, 
                                           new MVObjectPersistenceDelegate());
//            encoder.setPersistenceDelegate(Two.class, 
//                                           new MVObjectPersistenceDelegate());
            encoder.writeObject(object);
            encoder.close();
            System.out.println(xml.toString());

            XMLDecoder d = new XMLDecoder(new ByteArrayInputStream(xml.toByteArray()));

            Two x2 = (Two)d.readObject();
System.out.println("decode1: "+x2.getO1());
System.out.println("decode2: "+x2.getO2());

//             FileInputStream os = new FileInputStream("C:/cust.xml");
//             XMLDecoder decoder = new XMLDecoder(os);
//             Person p = (Person)decoder.readObject();
//             decoder.close();
//        }
//        catch(Exception e) {
//            Log.exception("MVObjectPersistenceDelegate.main caught exception", e);
//        }
    }
}
