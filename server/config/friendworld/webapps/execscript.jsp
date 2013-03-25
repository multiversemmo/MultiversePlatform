<%@ page import="javax.management.remote.*, javax.management.*, java.io.*" %>

<%!
public static class ServerScript {
    public static String execScript(String script)
    {
        try {
            JMXConnector jmxc;
            MBeanServerConnection server;

            JMXServiceURL jmxUrl = new JMXServiceURL("service:jmx:rmi:///jndi/rmi://localhost:9004/jmxrmi");
            jmxc = JMXConnectorFactory.connect(jmxUrl);
            server = jmxc.getMBeanServerConnection();
            Object[] parameters = {script};
            String[] signature = {"java.lang.String"};
            Object result;
            result = server.invoke(new ObjectName("net.multiverse:type=Engine"),
                "runPythonScript", parameters, signature);
            jmxc.close();
            return result.toString();
        }
        catch (IOException e) {
            System.err.println("Unable to attach to server" +
                ": " + e.getMessage());
            e.printStackTrace(System.err);
            return null;
        }
        catch (javax.management.MalformedObjectNameException e) {
            System.err.println("Internal error: " + e.getMessage());
            return null;
        }
        catch (javax.management.InstanceNotFoundException e) {
            System.err.println("Process is not a Multiverse engine");
            return null;
        }
        catch (javax.management.MBeanException e) {
            System.err.println("Error: "+e);
            return null;
        }
        catch (javax.management.ReflectionException e) {
            System.err.println("Error: "+e);
            return null;
        }
    }

    public static String validateArg(String arg) {
        if (arg != null) {
            arg = arg.replace("\n", "");
            arg = arg.replace("\'", "\\\'");
            arg = arg.replace("\"", "\\\"");
        }
        return arg;
    }

    public static String listChars =
        "token = %d\n" +
        "db = Engine.getDatabase()\n" +
        "if LoginPlugin.SecureToken == 1:\n" +
        "    token = ~token\n" +
        "charIds = db.getGameIDs('friendworld', token)\n" +
        "for oid in charIds:\n" +
        "    entity = db.loadEntity(oid, Namespace.WORLD_MANAGER)\n" +
        "    print entity.getName(), entity.getOid()\n";

    public static String createChar =
        "token = %d\n" +
        "name = '%s'\n" +
        "sex = '%s'\n" +
        "db = Engine.getDatabase()\n" +
        "if LoginPlugin.SecureToken == 1:\n" +
        "    token = ~token\n" +
        "if sex == 'male':\n" +
        "    model = 'male_01'\n" +
        "elif sex == 'female':\n" +
        "    model = 'female_01'\n" +
        "else:\n" +
        "    print 'ERROR: invalid sex'\n" +
        "props = { 'characterName' : name,\n" +
        "          'model' : model,\n" +
        "          'sex' : sex\n" +
        "          }\n" +
        "factory = LoginPlugin.getCharacterGenerator().getCharacterFactory()\n" +
        "oid = factory.createCharacter('friendworld', token, props)\n" +
        "print oid\n" +
        "ObjectManagerClient.saveObject(oid)\n" +
        "db.mapMultiverseID('friendworld', token, oid)\n";

    public static String delChar =
        "token = %d\n" +
        "oid = long(%s)\n" +
        "db = Engine.getDatabase()\n" +
        "if LoginPlugin.SecureToken == 1:\n" +
        "    token = ~token\n" +
        "factory = LoginPlugin.getCharacterGenerator().getCharacterFactory()\n" +
        "error = factory.deleteCharacter('friendworld', token, oid, None)\n" +
        "db.deletePlayerCharacter(oid)\n";
}
%>
